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
            if (newOperation.ItemId != this.ItemId)
            {
                return;
            }

            if (newOperation is InsertOperation)
            {
                throw new InvalidOperationException(Resources.SyncContext_DuplicateInsert);
            }            
        }

        public override void Collapse(MobileServiceTableOperation other)
        {
            if (other.ItemId != this.ItemId)
            {
                return;
            }

            if (other is DeleteOperation)
            {
                this.Cancel();
                other.Cancel();
            }
            if (other is UpdateOperation)
            {
                other.Cancel();
            }
        }

        public override Task ExecuteLocalAsync(IMobileServiceLocalStore store, JObject item)
        {
            return store.UpsertAsync(this.TableName, item);
        }
    }
}
