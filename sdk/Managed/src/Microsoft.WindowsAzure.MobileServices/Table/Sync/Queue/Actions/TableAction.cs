// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Query;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    /// <summary>
    /// Base class for table specific sync actions that push all the pending changes on that table before executing i.e. Purge and Pull
    /// </summary>
    internal abstract class TableAction : SyncAction
    {
        protected MobileServiceSyncContext Context { get; private set; }

        protected string QueryId { get; private set; }
        protected MobileServiceTableQueryDescription Query { get; private set; }
        public MobileServiceTable Table { get; private set; }
        public MobileServiceTableKind TableKind { get; private set; }

        protected MobileServiceSyncSettingsManager Settings { get; private set; }

        public IEnumerable<string> RelatedTables { get; set; }

        public TableAction(MobileServiceTable table,
                           MobileServiceTableKind tableKind,
                           string queryId,
                           MobileServiceTableQueryDescription query,
                           IEnumerable<string> relatedTables,
                           MobileServiceSyncContext context,
                           OperationQueue operationQueue,
                           MobileServiceSyncSettingsManager settings,
                           IMobileServiceLocalStore store,
                           CancellationToken cancellationToken)
            : base(operationQueue, store, cancellationToken)
        {
            this.Table = table;
            this.TableKind = tableKind;
            this.QueryId = queryId;
            this.Query = query;
            this.RelatedTables = relatedTables;
            this.Settings = settings;
            this.Context = context;
        }

        public async override Task ExecuteAsync()
        {
            try
            {
                await this.WaitPendingAction();

                using (await this.OperationQueue.LockTableAsync(this.Table.TableName, this.CancellationToken))
                {
                    if (await this.OperationQueue.CountPending(this.Table.TableName) > 0 && !await this.HandleDirtyTable())
                    {
                        return; // table is dirty and we cannot proceed for execution as handle return false
                    }

                    await this.ProcessTableAsync();
                }
            }
            catch (Exception ex)
            {
                this.TaskSource.TrySetException(ex);
                return;
            }
            this.TaskSource.SetResult(0);
        }

        protected virtual Task WaitPendingAction()
        {
            return Task.FromResult(0);
        }

        protected abstract Task<bool> HandleDirtyTable();

        protected abstract Task ProcessTableAsync();
    }
}
