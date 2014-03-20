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
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    internal class PullAction: TableAction
    {
        public PullAction(MobileServiceTable table, 
                          MobileServiceSyncContext context,
                          MobileServiceTableQueryDescription query,
                          OperationQueue operationQueue, 
                          IMobileServiceLocalStore store,
                          CancellationToken cancellationToken)
            : base(table, query, context, operationQueue, store, cancellationToken)
        {
        }

        protected async override Task ProcessTableAsync()
        {
            long ignore;

            JToken remoteResults = await this.Table.ReadAsync(this.Query.ToQueryString());
            JArray remoteItems = MobileServiceTableQueryProvider.GetResponseSequence(remoteResults, totalCount: out ignore);

            this.CancellationToken.ThrowIfCancellationRequested();

            await this.UpsertAll(remoteItems);
        }

        private async Task DeleteItems(IEnumerable<string> itemIds)
        {
            foreach (string id in itemIds)
            {
                await this.Store.DeleteAsync(this.Table.TableName, id);
            }
        }

        private async Task UpsertAll(JArray items)
        {
            foreach (JObject item in items)
            {
                await this.Store.UpsertAsync(this.Table.TableName, item);
            }
        }
    }
}
