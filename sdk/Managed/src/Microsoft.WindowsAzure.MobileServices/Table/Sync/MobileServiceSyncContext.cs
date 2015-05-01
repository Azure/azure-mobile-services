// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Query;
using Microsoft.WindowsAzure.MobileServices.Threading;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    internal class MobileServiceSyncContext : IMobileServiceSyncContext, IDisposable
    {
        private MobileServiceSyncSettingsManager settings;
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
            get
            {
                if (!this.IsInitialized)
                {
                    return 0;
                }
                return this.opQueue.PendingOperations;
            }
        }

        public async Task InitializeAsync(IMobileServiceLocalStore store, IMobileServiceSyncHandler handler)
        {
            if (store == null)
            {
                throw new ArgumentNullException("store");
            }
            handler = handler ?? new MobileServiceSyncHandler();

            this.initializeTask = new TaskCompletionSource<object>();

            using (await this.storeQueueLock.WriterLockAsync())
            {
                this.Handler = handler;
                this.Store = store;

                this.syncQueue = new ActionBlock();
                await this.Store.InitializeAsync();
                this.opQueue = await OperationQueue.LoadAsync(store);
                this.settings = new MobileServiceSyncSettingsManager(store);
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

        public async Task InsertAsync(string tableName, MobileServiceTableKind tableKind, string id, JObject item)
        {
            var operation = new InsertOperation(tableName, tableKind, id)
            {
                Table = await this.GetTable(tableName)
            };

            await this.ExecuteOperationAsync(operation, item);
        }

        public async Task UpdateAsync(string tableName, MobileServiceTableKind tableKind, string id, JObject item)
        {
            var operation = new UpdateOperation(tableName, tableKind, id)
            {
                Table = await this.GetTable(tableName)
            };

            await this.ExecuteOperationAsync(operation, item);
        }

        public async Task DeleteAsync(string tableName, MobileServiceTableKind tableKind, string id, JObject item)
        {
            var operation = new DeleteOperation(tableName, tableKind, id)
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

        /// <summary>
        /// Pulls all items that match the given query from the associated remote table.
        /// </summary>
        /// <param name="tableName">The name of table to pull</param>
        /// <param name="tableKind">The kind of table</param>
        /// <param name="queryId">A string that uniquely identifies this query and is used to keep track of its sync state.</param>
        /// <param name="query">An OData query that determines which items to 
        /// pull from the remote table.</param>
        /// <param name="options">An instance of <see cref="MobileServiceRemoteTableOptions"/></param>
        /// <param name="parameters">A dictionary of user-defined parameters and values to include in 
        /// the request URI query string.</param>
        /// <param name="relatedTables">
        /// List of tables that may have related records that need to be push before this table is pulled down.
        /// When no table is specified, all tables are considered related.
        /// </param>
        /// <param name="reader">An instance of <see cref="MobileServiceObjectReader"/></param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> token to observe
        /// </param>
        /// <returns>
        /// A task that completes when pull operation has finished.
        /// </returns>
        public async Task PullAsync(string tableName, MobileServiceTableKind tableKind, string queryId, string query, MobileServiceRemoteTableOptions options, IDictionary<string, string> parameters, IEnumerable<string> relatedTables, MobileServiceObjectReader reader, CancellationToken cancellationToken)
        {
            await this.EnsureInitializedAsync();

            if (parameters != null)
            {
                if (parameters.Keys.Any(k => k.Equals(MobileServiceTable.IncludeDeletedParameterName, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new ArgumentException("The key '{0}' is reserved and cannot be specified as a query parameter.".FormatInvariant(MobileServiceTable.IncludeDeletedParameterName));
                }

                if (parameters.Keys.Any(k => k.Equals(MobileServiceTable.SystemPropertiesQueryParameterName, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new ArgumentException("The key '{0}' is reserved and cannot be specified as a query parameter.".FormatInvariant(MobileServiceTable.SystemPropertiesQueryParameterName));
                }
            }

            var table = await this.GetTable(tableName);
            var queryDescription = MobileServiceTableQueryDescription.Parse(this.client.ApplicationUri, tableName, query);


            // local schema should be same as remote schema otherwise push can't function
            if (queryDescription.Selection.Any() || queryDescription.Projections.Any())
            {
                throw new ArgumentException("Pull query with select clause is not supported.", "query");
            }

            bool isIncrementalSync = !String.IsNullOrEmpty(queryId);
            if (isIncrementalSync)
            {
                if (queryDescription.Ordering.Any())
                {
                    throw new ArgumentException("Incremental pull query must not have orderby clause.", "query");
                }
                if (queryDescription.Top.HasValue || queryDescription.Skip.HasValue)
                {
                    throw new ArgumentException("Incremental pull query must not have skip or top specified.", "query");
                }
            }

            if (!options.HasFlag(MobileServiceRemoteTableOptions.OrderBy) && queryDescription.Ordering.Any())
            {
                throw new ArgumentException("The supported table options does not include orderby.", "query");
            }

            if (!options.HasFlag(MobileServiceRemoteTableOptions.Skip) && queryDescription.Skip.HasValue)
            {
                throw new ArgumentException("The supported table options does not include skip.", "query");
            }

            if (!options.HasFlag(MobileServiceRemoteTableOptions.Top) && queryDescription.Top.HasValue)
            {
                throw new ArgumentException("The supported table options does not include top.", "query");
            }

            // let us not burden the server to calculate the count when we don't need it for pull
            queryDescription.IncludeTotalCount = false;

            var action = new PullAction(table, tableKind, this, queryId, queryDescription, parameters, relatedTables, this.opQueue, this.settings, this.Store, options, reader, cancellationToken);
            await this.ExecuteSyncAction(action);
        }

        public async Task PurgeAsync(string tableName, MobileServiceTableKind tableKind, string queryId, string query, bool force, CancellationToken cancellationToken)
        {
            await this.EnsureInitializedAsync();

            var table = await this.GetTable(tableName);
            var queryDescription = MobileServiceTableQueryDescription.Parse(tableName, query);
            var action = new PurgeAction(table, tableKind, queryId, queryDescription, force, this, this.opQueue, this.settings, this.Store, cancellationToken);
            await this.ExecuteSyncAction(action);
        }

        public Task PushAsync(CancellationToken cancellationToken)
        {
            return PushAsync(cancellationToken, MobileServiceTableKind.Table, new string[0]);
        }

        public async Task PushAsync(CancellationToken cancellationToken, MobileServiceTableKind tableKind, params string[] tableNames)
        {
            await this.EnsureInitializedAsync();

            // use empty handler if its not a standard table push
            var handler = tableKind == MobileServiceTableKind.Table ? this.Handler : new MobileServiceSyncHandler();

            var action = new PushAction(this.opQueue,
                                      this.Store,
                                      tableKind,
                                      tableNames,
                                      handler,
                                      this.client,
                                      this,
                                      cancellationToken);

            await this.ExecuteSyncAction(action);
        }

        public async Task ExecuteSyncAction(SyncAction action)
        {
            Task discard = this.syncQueue.Post(action.ExecuteAsync, action.CancellationToken);

            await action.CompletionTask;
        }

        public virtual async Task<MobileServiceTable> GetTable(string tableName)
        {
            await this.EnsureInitializedAsync();

            var table = this.client.GetTable(tableName) as MobileServiceTable;
            table.SystemProperties = await settings.GetSystemPropertiesAsync(tableName);
            table.Features = MobileServiceFeatures.Offline;

            return table;
        }

        public Task CancelAndUpdateItemAsync(MobileServiceTableOperationError error, JObject item)
        {
            string itemId = error.Item.Value<string>(MobileServiceSystemColumns.Id);
            return this.ExecuteOperationSafeAsync(itemId, error.TableName, async () =>
            {
                await this.TryCancelOperation(error);
                await this.Store.UpsertAsync(error.TableName, item, fromServer: true);
            });
        }

        public Task CancelAndDiscardItemAsync(MobileServiceTableOperationError error)
        {
            string itemId = error.Item.Value<string>(MobileServiceSystemColumns.Id);
            return this.ExecuteOperationSafeAsync(itemId, error.TableName, async () =>
            {
                await this.TryCancelOperation(error);
                await this.Store.DeleteAsync(error.TableName, itemId);
            });
        }

        public async Task DeferTableActionAsync(TableAction action)
        {
            IEnumerable<string> tableNames;
            if (action.RelatedTables == null) // no related table
            {
                tableNames = new[] { action.Table.TableName };
            }
            else if (action.RelatedTables.Any()) // some related tables
            {
                tableNames = new[] { action.Table.TableName }.Concat(action.RelatedTables);
            }
            else // all tables are related
            {
                tableNames = Enumerable.Empty<string>();
            }

            try
            {
                await this.PushAsync(action.CancellationToken, action.TableKind, tableNames.ToArray());
            }
            finally
            {
                Task discard = this.syncQueue.Post(action.ExecuteAsync, action.CancellationToken);
            }
        }

        private async Task TryCancelOperation(MobileServiceTableOperationError error)
        {
            if (!await this.opQueue.DeleteAsync(error.Id, error.OperationVersion))
            {
                throw new InvalidOperationException("The operation has been updated and cannot be cancelled.");
            }
            // delete errors for cancelled operation
            await this.Store.DeleteAsync(MobileServiceLocalSystemTables.SyncErrors, error.Id);
        }

        private async Task EnsureInitializedAsync()
        {
            if (this.initializeTask == null)
            {
                throw new InvalidOperationException("SyncContext is not yet initialized.");
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
                MobileServiceTableOperation existing = await this.opQueue.GetOperationByItemIdAsync(operation.TableName, operation.ItemId);
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
                    if (ex is MobileServiceLocalStoreException)
                    {
                        throw;
                    }
                    throw new MobileServiceLocalStoreException("Failed to perform operation on local store.", ex);
                }

                if (existing != null)
                {
                    existing.Collapse(operation); // cancel either existing, new or both operation 
                    // delete error for collapsed operation
                    await this.Store.DeleteAsync(MobileServiceLocalSystemTables.SyncErrors, existing.Id);
                    if (existing.IsCancelled) // if cancelled we delete it
                    {
                        await this.opQueue.DeleteAsync(existing.Id, existing.Version);
                    }
                    else if (existing.IsUpdated)
                    {
                        await this.opQueue.UpdateAsync(existing);
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
                this.settings.Dispose();
                this._store.Dispose();
            }
        }
    }
}
