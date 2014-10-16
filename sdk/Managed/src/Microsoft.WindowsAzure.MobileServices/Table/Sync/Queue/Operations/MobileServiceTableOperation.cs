// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    internal abstract class MobileServiceTableOperation : IMobileServiceTableOperation
    {
        // --- Persisted properties -- //
        public string Id { get; private set; }
        public abstract MobileServiceTableOperationKind Kind { get; }
        public string TableName { get; private set; }
        public string ItemId { get; private set; }
        public JObject Item { get; set; }

        public MobileServiceTableOperationState State { get; internal set; }
        public long Sequence { get; set; }
        public long Version { get; set; }

        // --- Non persisted properties -- //
        IMobileServiceTable IMobileServiceTableOperation.Table
        {
            get { return this.Table; }
        }

        public MobileServiceTable Table { get; set; }

        public bool IsCancelled { get; private set; }
        public bool IsUpdated { get; private set; }

        public virtual bool CanWriteResultToStore
        {
            get { return true; }
        }

        protected virtual bool SerializeItemToQueue
        {
            get { return false; }
        }

        protected MobileServiceTableOperation(string tableName, string itemId)
        {
            this.Id = Guid.NewGuid().ToString();
            this.State = MobileServiceTableOperationState.Pending;
            this.TableName = tableName;
            this.ItemId = itemId;
            this.Version = 1;
        }

        public void AbortPush()
        {
            throw new MobileServicePushAbortException();
        }

        public async Task<JObject> ExecuteAsync()
        {
            if (this.IsCancelled)
            {
                return null;
            }

            if (this.Item == null)
            {
                throw new MobileServiceInvalidOperationException(Resources.MobileServiceTableOperation_ItemNotFound, request: null, response: null);
            }

            JToken response = await OnExecuteAsync();
            var result = response as JObject;
            if (response != null && result == null)
            {
                throw new MobileServiceInvalidOperationException(Resources.MobileServiceTableOperation_ResultNotJObject, request: null, response: null);
            }

            return result;
        }

        protected abstract Task<JToken> OnExecuteAsync();

        internal void Cancel()
        {
            this.IsCancelled = true;
        }

        internal void Update()
        {
            this.Version++;
            this.IsUpdated = true;
        }

        /// <summary>
        /// Execute the operation on sync store
        /// </summary>
        /// <param name="store">Sync store</param>
        /// <param name="item">The item to use for store operation</param>
        public abstract Task ExecuteLocalAsync(IMobileServiceLocalStore store, JObject item);

        /// <summary>
        /// Validates that the operation can collapse with the late operation
        /// </summary>
        /// <exception cref="InvalidOperationException">This method throws when the operation cannot collapse with new operation.</exception>
        public abstract void Validate(MobileServiceTableOperation newOperation);

        /// <summary>
        /// Collapse this operation with the late operation by cancellation of either operation.
        /// </summary>
        public abstract void Collapse(MobileServiceTableOperation newOperation);

        /// <summary>
        /// Defines the the table for storing operations
        /// </summary>
        /// <param name="store">An instance of <see cref="IMobileServiceLocalStore"/></param>
        internal static void DefineTable(MobileServiceLocalStore store)
        {
            store.DefineTable(MobileServiceLocalSystemTables.OperationQueue, new JObject()
            {
                { MobileServiceSystemColumns.Id, String.Empty },
                { "kind", 0 },
                { "state", 0 },
                { "tableName", String.Empty },
                { "itemId", String.Empty },
                { "item", String.Empty },
                { MobileServiceSystemColumns.CreatedAt, DateTime.Now },
                { "sequence", 0 },
                { "version", 0 }
            });
        }

        internal JObject Serialize()
        {
            var obj = new JObject()
            {
                { MobileServiceSystemColumns.Id, this.Id },
                { "kind", (int)this.Kind },
                { "state", (int)this.State },
                { "tableName", this.TableName },
                { "itemId", this.ItemId },
                { "item", this.Item != null && this.SerializeItemToQueue ? this.Item.ToString(Formatting.None) : null },
                { "sequence", this.Sequence },
                { "version", this.Version }
            };

            return obj;
        }

        internal static MobileServiceTableOperation Deserialize(JObject obj)
        {
            if (obj == null)
            {
                return null;
            }

            var kind = (MobileServiceTableOperationKind)obj.Value<int>("kind");
            string tableName = obj.Value<string>("tableName");
            string itemId = obj.Value<string>("itemId");


            MobileServiceTableOperation operation = null;
            switch (kind)
            {
                case MobileServiceTableOperationKind.Insert:
                    operation = new InsertOperation(tableName, itemId); break;
                case MobileServiceTableOperationKind.Update:
                    operation = new UpdateOperation(tableName, itemId); break;
                case MobileServiceTableOperationKind.Delete:
                    operation = new DeleteOperation(tableName, itemId); break;
            }

            if (operation != null)
            {
                operation.Id = obj.Value<string>(MobileServiceSystemColumns.Id);
                operation.Sequence = obj.Value<long?>("sequence").GetValueOrDefault();
                operation.Version = obj.Value<long?>("version").GetValueOrDefault();
                string itemJson = obj.Value<string>("item");
                operation.Item = !String.IsNullOrEmpty(itemJson) ? JObject.Parse(itemJson) : null;
                operation.State = (MobileServiceTableOperationState)obj.Value<int?>("state").GetValueOrDefault();
            }

            return operation;
        }
    }
}
