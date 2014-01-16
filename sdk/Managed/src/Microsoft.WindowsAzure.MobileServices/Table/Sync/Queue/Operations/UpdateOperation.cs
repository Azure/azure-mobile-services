// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

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
            if (newOperation.ItemId != this.ItemId)
            {
                return;
            }

            if (newOperation is InsertOperation)
            {
                throw new InvalidOperationException(Resources.SyncContext_UpdatePending);
            }            
        }

        public override void Collapse(MobileServiceTableOperation newOperation)
        {
            if (newOperation.ItemId != this.ItemId)
            {
                return;
            }

            if (newOperation is DeleteOperation)
            {
                this.Cancel();
            }
            if (newOperation is UpdateOperation)
            {
                newOperation.Cancel();
            }            
        }

        public override Task ExecuteLocalAsync(IMobileServiceLocalStore store, JObject item)
        {
            return store.UpsertAsync(this.TableName, item);
        }
    }
}
