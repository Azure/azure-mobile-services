﻿// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Query;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    internal class PurgeAction : TableAction
    {
        public PurgeAction(MobileServiceTable table,
                           MobileServiceTableKind tableKind,
                           string queryKey,
                           MobileServiceTableQueryDescription query,
                           MobileServiceSyncContext context,
                           OperationQueue operationQueue,
                           MobileServiceSyncSettingsManager settings,
                           IMobileServiceLocalStore store,
                           CancellationToken cancellationToken)
            : base(table, tableKind, queryKey, query, null, context, operationQueue, settings, store, cancellationToken)
        {
        }

        protected override bool CanDeferIfDirty
        {
            get { return false; }
        }

        protected override async Task ProcessTableAsync()
        {
            if (!String.IsNullOrEmpty(this.QueryKey))
            {
                await this.Settings.ResetDeltaTokenAsync(this.Table.TableName, this.QueryKey);
            }
            await this.Store.DeleteAsync(this.Query);
        }
    }
}
