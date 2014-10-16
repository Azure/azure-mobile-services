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
        private Task pendingPush;
        private MobileServiceSyncContext context;

        protected string QueryKey { get; private set; }
        protected MobileServiceTableQueryDescription Query { get; private set; }
        public MobileServiceTable Table { get; private set; }
        public MobileServiceTableKind TableKind { get; private set; }

        protected MobileServiceSyncSettingsManager Settings { get; private set; }

        protected abstract bool CanDeferIfDirty { get; }
        public IEnumerable<string> RelatedTables { get; set; }

        public TableAction(MobileServiceTable table,
                           MobileServiceTableKind tableKind,
                           string queryKey,
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
            this.QueryKey = queryKey;
            this.Query = query;
            this.RelatedTables = relatedTables;
            this.Settings = settings;
            this.context = context;
        }

        public async override Task ExecuteAsync()
        {
            try
            {
                if (this.pendingPush != null)
                {
                    await pendingPush; // this will cause any failed push to fail this dependant table action also
                }

                using (await this.OperationQueue.LockTableAsync(this.Table.TableName, this.CancellationToken))
                {
                    if (await this.OperationQueue.CountPending(this.Table.TableName) > 0)
                    {
                        if (this.CanDeferIfDirty)
                        {
                            // there are pending operations on the same table so defer the action
                            this.pendingPush = this.context.DeferTableActionAsync(this);
                            // we need to return in order to give PushAsync a chance to execute so we don't await the pending push
                            return;
                        }
                        throw new InvalidOperationException(Resources.SyncContext_PurgeOnDirtyTable);
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

        protected abstract Task ProcessTableAsync();
    }
}
