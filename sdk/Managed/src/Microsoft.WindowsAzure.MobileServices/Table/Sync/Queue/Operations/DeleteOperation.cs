// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    internal class DeleteOperation : MobileServiceTableOperation
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

        public DeleteOperation(string tableName, MobileServiceTableKind tableKind, string itemId)
            : base(tableName, tableKind, itemId)
        {
        }

        protected override async Task<JToken> OnExecuteAsync()
        {
            try
            {
                return await this.Table.DeleteAsync(this.Item);
            }
            catch (MobileServiceInvalidOperationException ex)
            {
                // if the item is already deleted then local store is in-sync with the server state
                if (ex.Response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                throw;
            }
        }

        public override void Validate(MobileServiceTableOperation newOperation)
        {
            Debug.Assert(newOperation.ItemId == this.ItemId);

            // we don't allow any more operations on object that has already been deleted
            throw new InvalidOperationException("A delete operation on the item is already in the queue.");
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
