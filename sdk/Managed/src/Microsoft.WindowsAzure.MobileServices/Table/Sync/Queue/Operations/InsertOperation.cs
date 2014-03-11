// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    internal class InsertOperation: MobileServiceTableOperation
    {
        public override MobileServiceTableOperationKind Kind
        {
            get { return MobileServiceTableOperationKind.Insert; }
        }

        public InsertOperation(string tableName, string itemId):base(tableName, itemId)
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
                newOperation.Cancel();
            }
        }

        public override Task ExecuteLocalAsync(IMobileServiceLocalStore store, JObject item)
        {
            return store.UpsertAsync(this.TableName, item);
        }
    }
}
