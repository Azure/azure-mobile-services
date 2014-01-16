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

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    internal class PurgeAction: TableAction
    {
        public PurgeAction(MobileServiceTable table,
                           MobileServiceTableQueryDescription query,
                           MobileServiceSyncContext context, 
                           OperationQueue operationQueue, 
                           IMobileServiceLocalStore store,
                           CancellationToken cancellationToken)
            : base(table, query, context, operationQueue, store, cancellationToken)
        {
        }

        protected override Task ProcessTableAsync()
        {
            return this.Store.DeleteAsync(this.Query);
        }
    }
}
