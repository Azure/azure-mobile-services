﻿// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Query;
using Microsoft.WindowsAzure.MobileServices.Threading;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    internal sealed class MobileServiceSyncContext: IMobileServiceSyncContext, IDisposable  
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

        /// <summary>
        /// Lock to ensure that multiple sync and op queue operations don't interleave
        /// </summary>
        private AsyncLock syncOpQueueLock = new AsyncLock();

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

        public int PendingOperations
        {
            get
            {
                return this.opQueue.CountPending();   
            }
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
                this.opQueue = await OperationQueue.LoadAsync(this.Store, this.client);

                this.initializeTask.SetResult(null);
            }
        }

        public async Task<JToken> ReadAsync(MobileServiceTable table, string query)
        {
            await this.EnsureInitializedAsync();

            var queryDescription = MobileServiceTableQueryDescription.Parse(table.TableName, query);
            using (await this.storeQueueLock.ReaderLockAsync())
            {
                return await this.Store.ReadAsync(queryDescription);
            }
        }

        public async Task InsertAsync(MobileServiceTable table, string id, JObject item)
        {
            var operation = new InsertOperation(table.TableName, id)
            {
                Table = table
            };

            await this.ExecuteOperationAsync(operation, item);
        }        

        public async Task UpdateAsync(MobileServiceTable table, string id, JObject item)
        {
            var operation = new UpdateOperation(table.TableName, id)
            {
                Table = table
            };

            await this.ExecuteOperationAsync(operation, item);
        }

        public async Task DeleteAsync(MobileServiceTable table, string id, JObject item)
        {
            var operation = new DeleteOperation(table.TableName, id)
            {
                Table = table,
                Item = item // item will be deleted from store, so we need to put it in the operation queue
            };

            await this.ExecuteOperationAsync(operation, item);
        }
        
        public async Task<JObject> LookupAsync(MobileServiceTable table, string id)
        {
            await this.EnsureInitializedAsync();

            return await this.Store.LookupAsync(table.TableName, id);
        }

        public async Task PullAsync(MobileServiceTable table, string query, CancellationToken cancellationToken)
        {
            await this.EnsureInitializedAsync();

            var queryDescription = MobileServiceTableQueryDescription.Parse(table.TableName, query);
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

        public async Task PurgeAsync(MobileServiceTable table, string query, CancellationToken cancellationToken)
        {
            await this.EnsureInitializedAsync();

            var queryDescription = MobileServiceTableQueryDescription.Parse(table.TableName, query);
            var purge = new PurgeAction(table, queryDescription, this, this.opQueue, this.Store, cancellationToken);
            Task discard = this.syncQueue.Post(purge.ExecuteAsync, cancellationToken);

            await purge.CompletionTask;
        }

        public async Task PushAsync(CancellationToken cancellationToken)
        {
            await this.EnsureInitializedAsync();

            var bookmark = new BookmarkOperation();
            var push = new PushAction(this.opQueue, this.Store, this.Handler, this.client.SerializerSettings, cancellationToken, bookmark);

            using (await this.syncOpQueueLock.Acquire(cancellationToken))
            {                
                await this.opQueue.EnqueueAsync(bookmark);
                Task discard = this.syncQueue.Post(push.ExecuteAsync, cancellationToken);
            }

            await push.CompletionTask;
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

        private async Task ExecuteOperationAsync(MobileServiceTableOperation operation, JObject item)
        {
            await this.EnsureInitializedAsync();

            // take slowest lock first and quickest last in order to avoid blocking quick operations for long time            
            using (await this.opQueue.LockItemAsync(operation.ItemId, CancellationToken.None))  // prevent any inflight operation on the same item
            using (await this.opQueue.LockTableAsync(operation.TableName, CancellationToken.None)) // prevent interferance with any in-progress pull/purge action
            using (await this.storeQueueLock.WriterLockAsync()) // prevent any other operation from interleaving between store and queue insert
            {
                MobileServiceTableOperation existing;
                if (this.opQueue.TryGetOperation(operation.ItemId, out existing))
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
                        await this.opQueue.DeleteAsync(existing);
                    }
                }

                // if validate didn't cancel the operation then queue it
                if (!operation.IsCancelled)
                {
                    await this.opQueue.EnqueueAsync(operation);
                }
            }
        }

        public void Dispose()
        {
            this.syncOpQueueLock.Dispose();
            if (this._store != null)
            {
                this._store.Dispose();
            }
        }
    }
}
