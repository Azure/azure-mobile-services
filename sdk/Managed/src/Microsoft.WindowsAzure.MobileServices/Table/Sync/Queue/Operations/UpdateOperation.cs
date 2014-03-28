// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    internal class UpdateOperation: MobileServiceTableOperation
    {
        public override MobileServiceTableOperationKind Kind
        {
            get { return MobileServiceTableOperationKind.Update; }
        }

        public UpdateOperation(string tableName, string itemId) : base(tableName, itemId)
        {
        }

        protected override Task<JToken> OnExecuteAsync()
        {
            return this.Table.UpdateAsync(this.Item);
        }

        public override void Validate(MobileServiceTableOperation newOperation)
        {
            Debug.Assert(newOperation.ItemId == this.ItemId);

            if (newOperation is InsertOperation)
            {
                throw new InvalidOperationException(Resources.SyncContext_UpdatePending);
            }            
        }

        public override void Collapse(MobileServiceTableOperation newOperation)
        {
            Debug.Assert(newOperation.ItemId == this.ItemId);

            if (newOperation is DeleteOperation)
            {
                this.Cancel();
            }
            else if (newOperation is UpdateOperation)
            {
                newOperation.Cancel();
            }            
        }

        public override Task ExecuteLocalAsync(IMobileServiceLocalStore store, JObject item)
        {
            return store.UpsertAsync(this.TableName, item, fromServer: false);
        }
    }
}
