// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    internal class UpdateOperation : MobileServiceTableOperation
    {
        public override MobileServiceTableOperationKind Kind
        {
            get { return MobileServiceTableOperationKind.Update; }
        }

        public UpdateOperation(string tableName, MobileServiceTableKind tableKind, string itemId)
            : base(tableName, tableKind, itemId)
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
                throw new InvalidOperationException("An update operation on the item is already in the queue.");
            }
        }

        public override void Collapse(MobileServiceTableOperation newOperation)
        {
            Debug.Assert(newOperation.ItemId == this.ItemId);

            if (newOperation is DeleteOperation)
            {
                this.Cancel();
                newOperation.Update();
            }
            else if (newOperation is UpdateOperation)
            {
                this.Update();
                newOperation.Cancel();
            }
        }

        public override Task ExecuteLocalAsync(IMobileServiceLocalStore store, JObject item)
        {
            return store.UpsertAsync(this.TableName, item, fromServer: false);
        }
    }
}
