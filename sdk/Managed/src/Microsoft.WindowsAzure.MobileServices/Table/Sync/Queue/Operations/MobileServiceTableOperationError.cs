// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    /// <summary>
    /// Gives details of failed table operation.
    /// </summary>
    public class MobileServiceTableOperationError
    {
        /// <summary>
        /// A unique identifier for the error.
        /// </summary>
        internal string Id { get; private set; }

        /// <summary>
        /// Indicates whether error is handled.
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// The version of the operation.
        /// </summary>
        internal long OperationVersion { get; private set; }

        /// <summary>
        /// The kind of table operation.
        /// </summary>
        public MobileServiceTableOperationKind OperationKind { get; private set; }

        /// <summary>
        /// The kind of table 
        /// </summary>
        internal MobileServiceTableKind TableKind { get; set; }

        /// <summary>
        /// The name of the remote table.
        /// </summary>
        public string TableName { get; private set; }

        /// <summary>
        /// The item associated with the operation.
        /// </summary>
        public JObject Item { get; private set; }

        /// <summary>
        /// Response of the table operation.
        /// </summary>
        public JObject Result { get; private set; }

        /// <summary>
        /// Raw response of the table operation.
        /// </summary>
        public string RawResult { get; private set; }

        /// <summary>
        /// The HTTP status code returned by server.
        /// </summary>        
        public HttpStatusCode? Status { get; private set; } // this is nullable because this error can also occur if the handler throws an exception

        internal MobileServiceSyncContext Context { get; set; }

        /// <summary>
        /// Initializes an instance of <see cref="MobileServiceTableOperationError"/>
        /// </summary>
        /// <param name="id">The id of error that is same as operation id.</param>
        /// <param name="operationVersion">The version of the operation.</param>
        /// <param name="operationKind">The kind of table operation.</param>
        /// <param name="status">The HTTP status code returned by server.</param>
        /// <param name="tableName">The name of the remote table.</param>
        /// <param name="item">The item associated with the operation.</param>
        /// <param name="rawResult">Raw response of the table operation.</param>
        /// <param name="result">Response of the table operation.</param>
        public MobileServiceTableOperationError(string id,
                                                long operationVersion,
                                                MobileServiceTableOperationKind operationKind,
                                                HttpStatusCode? status,
                                                string tableName,
                                                JObject item,
                                                string rawResult,
                                                JObject result)
        {
            this.Id = id;
            this.OperationVersion = operationVersion;
            this.Status = status;
            this.OperationKind = operationKind;
            this.TableName = tableName;
            this.Item = item;
            this.RawResult = rawResult;
            this.Result = result;
        }

        /// <summary>
        /// Cancels the table operation and updates the local instance of the item with the given item.
        /// </summary>
        /// <param name="item">The item to update in local store.</param>
        /// <returns>Task that completes when operation is cancelled.</returns>
        public async Task CancelAndUpdateItemAsync(JObject item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            await this.Context.CancelAndUpdateItemAsync(this, item);
            this.Handled = true;
        }

        /// <summary>
        /// Cancels the table operation and discards the local instance of the item.
        /// </summary>
        /// <returns>Task that completes when operation is cancelled.</returns>
        public async Task CancelAndDiscardItemAsync()
        {
            await this.Context.CancelAndDiscardItemAsync(this);
            this.Handled = true;
        }

        /// <summary>
        /// Defines the the table for storing errors
        /// </summary>
        /// <param name="store">An instance of <see cref="IMobileServiceLocalStore"/></param>
        internal static void DefineTable(MobileServiceLocalStore store)
        {
            store.DefineTable(MobileServiceLocalSystemTables.SyncErrors, new JObject()
            {
                { MobileServiceSystemColumns.Id, String.Empty },
                { "httpStatus", 0 },
                { "operationVersion", 0 },
                { "operationKind", 0 },
                { "tableName", String.Empty },
                { "tableKind", 0 },
                { "item", String.Empty },
                { "rawResult", String.Empty }
            });
        }

        internal JObject Serialize()
        {
            return new JObject()
            {
                { MobileServiceSystemColumns.Id, this.Id },
                { "httpStatus", this.Status.HasValue ? (int?)this.Status.Value: null },
                { "operationVersion", this.OperationVersion },
                { "operationKind", (int)this.OperationKind },
                { "tableName", this.TableName },
                { "tableKind", (int)this.TableKind },
                { "item", this.Item.ToString(Formatting.None) },
                { "rawResult", this.RawResult }
            };
        }

        internal static MobileServiceTableOperationError Deserialize(JObject obj, MobileServiceJsonSerializerSettings settings)
        {
            HttpStatusCode? status = null;
            if (obj["httpStatus"] != null)
            {
                status = (HttpStatusCode?)obj.Value<int?>("httpStatus");
            }
            string id = obj.Value<string>(MobileServiceSystemColumns.Id);
            long operationVersion = obj.Value<long?>("operationVersion").GetValueOrDefault();
            MobileServiceTableOperationKind operationKind = (MobileServiceTableOperationKind)obj.Value<int>("operationKind");
            var tableName = obj.Value<string>("tableName");
            var tableKind = (MobileServiceTableKind)obj.Value<int?>("tableKind").GetValueOrDefault();

            string itemStr = obj.Value<string>("item");
            JObject item = itemStr == null ? null : JObject.Parse(itemStr);
            string rawResult = obj.Value<string>("rawResult");
            var result = rawResult.ParseToJToken(settings) as JObject;

            return new MobileServiceTableOperationError(id,
                                                        operationVersion,
                                                        operationKind,
                                                        status,
                                                        tableName,
                                                        item,
                                                        rawResult,
                                                        result)
            {
                Id = id,
                TableKind = tableKind
            };
        }
    }
}
