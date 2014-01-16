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
    internal abstract class TableAction: SyncAction
    {
        private Task pendingPush;

        protected MobileServiceTableQueryDescription Query { get; private set; }
        private MobileServiceSyncContext context;
        protected MobileServiceTable Table { get; private set; }

        public TableAction(MobileServiceTable table,
                           MobileServiceTableQueryDescription query,
                           MobileServiceSyncContext context, 
                           OperationQueue operationQueue, 
                           IMobileServiceLocalStore store,
                           CancellationToken cancellationToken): base(operationQueue, store, cancellationToken)
        {
            this.Table = table;
            this.Query = query;
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

                using (await OperationQueue.LockTableAsync(this.Table.TableName, this.CancellationToken))
                {
                    if (OperationQueue.CountPending(this.Table.TableName) > 0)
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
                throw;
            }
            this.TaskSource.SetResult(0);
        }

        protected abstract Task ProcessTableAsync();
    }
}
