// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Query;
using Microsoft.WindowsAzure.MobileServices.Threading;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    internal class MobileServiceSyncContext: IMobileServiceSyncContext, IDisposable  
    {
        private TaskCompletionSource<object> initializeTask;
        private MobileServiceClient client;

        /// <summary>
        /// Lock to ensure that multiple insert,update,delete operations don't interleave as they are added to queue and storage
        /// </summary>
        private AsyncReaderWriterLock storeQueueLock = new AsyncReaderWriterLock();        

        /// <summary>
        /// Variable for Store property. Not meant to be accessed directly.
        /// </summary>
        private IMobileServiceLocalStore _store; 

        /// <summary>
        /// Queue for executing sync calls (push,pull) one after the other
        /// </summary>
        private ActionBlock syncQueue;

        /// <summary>
        /// Queue for pending operations (insert,delete,update) against remote table 
        /// </summary>
        private OperationQueue opQueue;

        public IMobileServiceSyncHandler Handler { get; private set; }

        public IMobileServiceLocalStore Store
        {
            get { return this._store; }
            private set
            {
                IMobileServiceLocalStore oldStore = this._store;
                this._store = value;
                if (oldStore != null)
                {
                    oldStore.Dispose();
                }
            }
        }        

        public bool IsInitialized
        {
            get { return this.initializeTask != null && this.initializeTask.Task.Status == TaskStatus.RanToCompletion; }
        }

        public MobileServiceSyncContext(MobileServiceClient client)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }

            this.client = client;
        }

        public long PendingOperations
        {
            get { return this.opQueue.PendingOperations; }
        }

        public async Task InitializeAsync(IMobileServiceLocalStore store, IMobileServiceSyncHandler handler)
        {
            if (store == null)
            {
                throw new ArgumentNullException("store");
            }
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            this.initializeTask = new TaskCompletionSource<object>();

            using (await this.storeQueueLock.WriterLockAsync())
            {
                this.Handler = handler;
                this.Store = store;

                this.syncQueue = new ActionBlock();
                await this.Store.InitializeAsync();
                this.opQueue = await OperationQueue.LoadAsync(this.Store);

                this.initializeTask.SetResult(null);
            }
        }

        public async Task<JToken> ReadAsync(string tableName, string query)
        {
            await this.EnsureInitializedAsync();

            var queryDescription = MobileServiceTableQueryDescription.Parse(tableName, query);
            using (await this.storeQueueLock.ReaderLockAsync())
            {
                return await this.Store.ReadAsync(queryDescription);
            }
        }

        public async Task InsertAsync(string tableName, string id, JObject item)
        {
            var operation = new InsertOperation(tableName, id)
            {
                Table = await this.GetTable(tableName)
            };

            await this.ExecuteOperationAsync(operation, item);
        }        

        public async Task UpdateAsync(string tableName, string id, JObject item)
        {
            var operation = new UpdateOperation(tableName, id)
            {
                Table = await this.GetTable(tableName)
            };

            await this.ExecuteOperationAsync(operation, item);
        }

        public async Task DeleteAsync(string tableName, string id, JObject item)
        {
            var operation = new DeleteOperation(tableName, id)
            {
                Table = await this.GetTable(tableName),
                Item = item // item will be deleted from store, so we need to put it in the operation queue
            };

            await this.ExecuteOperationAsync(operation, item);
        }
        
        public async Task<JObject> LookupAsync(string tableName, string id)
        {
            await this.EnsureInitializedAsync();

            return await this.Store.LookupAsync(tableName, id);
        }

        public async Task PullAsync(string tableName, string query, CancellationToken cancellationToken)
        {
            await this.EnsureInitializedAsync();

            var table = await this.GetTable(tableName);
            var queryDescription = MobileServiceTableQueryDescription.Parse(tableName, query);
            // local schema should be same as remote schema otherwise push can't function
            if (queryDescription.Selection.Count > 0 || queryDescription.Projections.Count > 0)
            {
                throw new ArgumentException(Resources.MobileServiceSyncTable_PullWithSelectNotSupported, "query");
            }
            // let us not burden the server to calculate the count when we don't need it for pull
            queryDescription.IncludeTotalCount = false;

            var pull = new PullAction(table, this, queryDescription, this.opQueue, this.Store, cancellationToken);
            Task discard = this.syncQueue.Post(pull.ExecuteAsync, cancellationToken);

            await pull.CompletionTask;
        }

        public async Task PurgeAsync(string tableName, string query, CancellationToken cancellationToken)
        {
            await this.EnsureInitializedAsync();

            var table = await this.GetTable(tableName);
            var queryDescription = MobileServiceTableQueryDescription.Parse(tableName, query);
            var purge = new PurgeAction(table, queryDescription, this, this.opQueue, this.Store, cancellationToken);
            Task discard = this.syncQueue.Post(purge.ExecuteAsync, cancellationToken);

            await purge.CompletionTask;
        }

        public async Task PushAsync(CancellationToken cancellationToken)
        {
            await this.EnsureInitializedAsync();

            var push = new PushAction(this.opQueue, 
                                      this.Store, 
                                      this.Handler, 
                                      this.client, 
                                      this,
                                      cancellationToken);

            Task discard = this.syncQueue.Post(push.ExecuteAsync, cancellationToken);

            await push.CompletionTask;
        }        

        public virtual async Task<MobileServiceTable> GetTable(string tableName)
        {
            await this.EnsureInitializedAsync();

            var table = this.client.GetTable(tableName) as MobileServiceTable;
            JObject value = await this.Store.LookupAsync(MobileServiceLocalSystemTables.Config, tableName + "_systemProperties");
            if (value == null)
            {
                table.SystemProperties = MobileServiceSystemProperties.Version;
            }
            else
            {
                table.SystemProperties = (MobileServiceSystemProperties)value.Value<int>("value");
            }
            table.AddRequestHeader(MobileServiceHttpClient.ZumoFeaturesHeader, MobileServiceFeatures.Offline);

            return table;
        }

        public Task CancelAndUpdateItemAsync(MobileServiceTableOperationError error, JObject item)
        {
            string itemId = error.Item.Value<string>(MobileServiceSystemColumns.Id);
            return this.ExecuteOperationSafeAsync(itemId, error.TableName, async () =>
            {
                await this.Store.UpsertAsync(error.TableName, item, fromServer: true);
                await this.opQueue.DeleteAsync(error.OperationId);
            });
        }

        public Task CancelAndDiscardItemAsync(MobileServiceTableOperationError error)
        {
            string itemId = error.Item.Value<string>(MobileServiceSystemColumns.Id);
            return this.ExecuteOperationSafeAsync(itemId, error.TableName, async () =>
            {
                await this.Store.DeleteAsync(error.TableName, itemId);
                await this.opQueue.DeleteAsync(error.OperationId);
            });
        }

        public async Task DeferTableActionAsync(TableAction action)
        {
            try
            {
                await this.PushAsync(action.CancellationToken);
            }
            finally
            {
                Task discard = this.syncQueue.Post(action.ExecuteAsync, action.CancellationToken);
            }
        }

        private async Task EnsureInitializedAsync()
        {            
            if (this.initializeTask == null)
            {
                throw new InvalidOperationException(Resources.SyncContext_NotInitialized);
            }
            else
            {
                // when the initialization has started we wait for it to complete
                await this.initializeTask.Task;
            }
        }

        private Task ExecuteOperationAsync(MobileServiceTableOperation operation, JObject item)
        {
            return this.ExecuteOperationSafeAsync(operation.ItemId, operation.TableName, async () =>
            {
                MobileServiceTableOperation existing = await this.opQueue.GetOperationAsync(operation.ItemId);
                if (existing != null)
                {
                    existing.Validate(operation); // make sure this operation is legal and collapses after any previous operation on same item already in the queue
                }

                try
                {
                    await operation.ExecuteLocalAsync(this.Store, item); // first execute operation on local store
                }
                catch (Exception ex)
                {
                    throw new MobileServiceLocalStoreException(Resources.SyncStore_OperationFailed, ex);
                }

                if (existing != null)
                {
                    existing.Collapse(operation); // cancel either existing, new or both operation 
                    if (existing.IsCancelled)
                    {
                        await this.opQueue.DeleteAsync(existing.Id);
                    }
                }

                // if validate didn't cancel the operation then queue it
                if (!operation.IsCancelled)
                {
                    await this.opQueue.EnqueueAsync(operation);
                }
            });
        }

        private async Task ExecuteOperationSafeAsync(string itemId, string tableName, Func<Task> action)
        {
            await this.EnsureInitializedAsync();

            // take slowest lock first and quickest last in order to avoid blocking quick operations for long time            
            using (await this.opQueue.LockItemAsync(itemId, CancellationToken.None))  // prevent any inflight operation on the same item
            using (await this.opQueue.LockTableAsync(tableName, CancellationToken.None)) // prevent interferance with any in-progress pull/purge action
            using (await this.storeQueueLock.WriterLockAsync()) // prevent any other operation from interleaving between store and queue insert
            {
                await action();
            }
        }

        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && this._store != null)
            {
                this._store.Dispose();
            }
        }
    }
}
