// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Query;
using Microsoft.WindowsAzure.MobileServices.Threading;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    /// <summary>
    /// Base class for table specific sync actions that push all the pending changes on that table before executing i.e. Purge and Pull
    /// </summary>
    internal abstract class TableAction: SyncAction
    {
        private Task pendingPush;
        private MobileServiceSyncContext context;

        protected string QueryKey { get; private set; }
        protected MobileServiceTableQueryDescription Query { get; private set; }
        protected MobileServiceTable Table { get; private set; }
        protected MobileServiceSyncSettingsManager Settings { get; private set; }

        public TableAction(MobileServiceTable table,
                           string queryKey,
                           MobileServiceTableQueryDescription query,
                           MobileServiceSyncContext context, 
                           OperationQueue operationQueue,
                           MobileServiceSyncSettingsManager settings,
                           IMobileServiceLocalStore store,
                           CancellationToken cancellationToken): base(operationQueue, store, cancellationToken)
        {
            this.Table = table;
            this.QueryKey = queryKey;
            this.Query = query;
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
                        // there are pending operations on the same table so defer the action
                        this.pendingPush = this.context.DeferTableActionAsync(this);
                        // we need to return in order to give PushAsync a chance to execute so we don't await the pending push
                        return;
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
