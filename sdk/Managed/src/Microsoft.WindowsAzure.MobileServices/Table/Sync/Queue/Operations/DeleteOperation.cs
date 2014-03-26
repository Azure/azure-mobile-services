// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    internal class DeleteOperation: MobileServiceTableOperation
    {
        public override MobileServiceTableOperationKind Kind
        {
            get { return MobileServiceTableOperationKind.Delete; }
        }

        public override bool CanWriteResultToStore
        {
            get { return false; } // delete result should not be written to store, otherwise we're adding back the item that user deleted
        }

        protected override bool SerializeItemToQueue
        {
            get { return true; } // delete should save the item in queue since store copy is deleted right away with delete operation
        }

        public DeleteOperation(string tableName, string itemId) : base(tableName, itemId)
        {
        }

        protected override Task<JToken> OnExecuteAsync()
        {
            return this.Table.DeleteAsync(this.Item);
        }

        public override void Validate(MobileServiceTableOperation newOperation)
        {
            Debug.Assert(newOperation.ItemId == this.ItemId);

            // we don't allow any more operations on object that has already been deleted
            throw new InvalidOperationException(Resources.SyncContext_DeletePending);
        }

        public override void Collapse(MobileServiceTableOperation other)
        {
            // nothing to collapse we don't allow any operation after delete
        }

        public override Task ExecuteLocalAsync(IMobileServiceLocalStore store, JObject item)
        {
            return store.DeleteAsync(this.TableName, this.ItemId);
        }
    }
}
