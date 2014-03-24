// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    internal abstract class MobileServiceTableOperation: IMobileServiceTableOperation
    {
        // --- Persisted properties -- //
        public string Id { get; private set; }
        public abstract MobileServiceTableOperationKind Kind { get; }
        public string TableName { get; private set; }
        public string ItemId { get; private set; }
        public JObject Item { get; set; }
        public DateTime CreatedAt { get; private set; }
        public long Sequence { get; set; }

        // --- Non persisted properties -- //
        public MobileServiceTable Table { get; set; }
        public JToken Result { get; set; }
        public string RawResult { get; set; }
        public HttpStatusCode? StatusCode { get; private set; }
        public bool IsCancelled { get; private set; }
        public virtual bool WriteResultToStore
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
            this.CreatedAt = DateTime.UtcNow;
            this.TableName = tableName;
            this.ItemId = itemId;
        }

        public async Task ExecuteAsync()
        {
            this.Result = null;
            this.RawResult = null;
            this.StatusCode = null;

            if (this.IsCancelled)
            {
                return;
            }

            if (this.Item == null)
            {
                throw new MobileServiceInvalidOperationException(Resources.MobileServiceTableOperation_ItemNotFound, request: null, response: null);
            }


            ExceptionDispatchInfo edi = null;
            MobileServiceInvalidOperationException iox = null;

            try
            {
                this.Result = await OnExecuteAsync();
            }
            catch (MobileServiceInvalidOperationException ex)
            {
                iox = ex;
                edi = ExceptionDispatchInfo.Capture(ex);
            }               
         
            if (iox != null)
            {
                // if the operation was not successful and we have an error that can give us jtoken result, then take it.
                Tuple<string, JToken> content = await this.Table.ParseContent(iox.Response);
                if (iox.Response != null)
                {
                    this.StatusCode = iox.Response.StatusCode;
                }

                this.RawResult = content.Item1;
                this.Result = content.Item2;

                edi.Throw();
            }
        }
        protected abstract Task<JToken> OnExecuteAsync();

        internal void Cancel()
        {
            this.IsCancelled = true;
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

        public JObject Serialize()
        {
            var obj = new JObject()
            {
                { "id", this.Id },
                { "kind", (int)this.Kind },
                { "tableName", this.TableName },
                { "itemId", this.ItemId },
                { "item", this.Item != null && this.SerializeItemToQueue ? this.Item.ToString(Formatting.None) : null },
                { "__createdAt", this.CreatedAt },
                { "sequence", this.Sequence }
            };

            return obj;
        }

        public static MobileServiceTableOperation Deserialize(JObject obj)
        {
            var kind = (MobileServiceTableOperationKind)obj.Value<int>("kind");
            string id = obj.Value<string>("id");
            string tableName = obj.Value<string>("tableName");
            string itemId = obj.Value<string>("itemId");
            string itemJson = obj.Value<string>("item");
            JObject item = !String.IsNullOrEmpty(itemJson) ? JObject.Parse(itemJson) : null;
            DateTime createdAt = obj.Value<DateTime>("__createdAt");
            long sequence = obj.Value<long>("sequence");

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
                operation.Id = id;
                operation.Sequence = sequence;
                operation.CreatedAt = createdAt;
                operation.Item = item;
            }

            return operation;
        }
    }
}
