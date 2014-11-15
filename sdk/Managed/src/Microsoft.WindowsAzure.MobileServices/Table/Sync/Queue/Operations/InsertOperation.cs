// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    internal class InsertOperation : MobileServiceTableOperation
    {
        public override MobileServiceTableOperationKind Kind
        {
            get { return MobileServiceTableOperationKind.Insert; }
        }

        public InsertOperation(string tableName, MobileServiceTableKind tableKind, string itemId)
            : base(tableName, tableKind, itemId)
        {
        }

        protected override Task<JToken> OnExecuteAsync()
        {
            string version;
            // for insert operations version should not be sent so strip it out
            JObject item = MobileServiceSerializer.RemoveSystemProperties(this.Item, out version);
            return this.Table.InsertAsync(item);
        }

        public override void Validate(MobileServiceTableOperation newOperation)
        {
            Debug.Assert(newOperation.ItemId == this.ItemId);

            if (newOperation is InsertOperation)
            {
                throw new InvalidOperationException(Resources.SyncContext_DuplicateInsert);
            }

            if (newOperation is DeleteOperation && this.State != MobileServiceTableOperationState.Pending)
            {
                // if insert was attempted then we can't be sure if it went through or not hence we can't collapse delete
                throw new InvalidOperationException(Resources.SyncContext_InsertAttempted);
            }
        }

        public override void Collapse(MobileServiceTableOperation newOperation)
        {
            Debug.Assert(newOperation.ItemId == this.ItemId);

            if (newOperation is DeleteOperation)
            {
                this.Cancel();
                newOperation.Cancel();
            }
            else if (newOperation is UpdateOperation)
            {
                this.Update();
                newOperation.Cancel();
            }
        }

        public override async Task ExecuteLocalAsync(IMobileServiceLocalStore store, JObject item)
        {
            if (await store.LookupAsync(this.TableName, this.ItemId) != null)
            {
                throw new MobileServiceLocalStoreException(Resources.SyncContext_DuplicateInsert, null);
            }

            await store.UpsertAsync(this.TableName, item, fromServer: false);
        }
    }
}
