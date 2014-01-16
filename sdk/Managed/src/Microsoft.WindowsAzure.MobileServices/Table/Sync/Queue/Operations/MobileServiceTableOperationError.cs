// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
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
        public string Id { get; private set; }

        /// <summary>
        /// Indicates whether error is handled.
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// The kind of table operation.
        /// </summary>
        public MobileServiceTableOperationKind OperationKind { get; private set; }

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
        public JToken Result { get; private set; }

        /// <summary>
        /// Raw response of the table operation.
        /// </summary>
        public string RawResult { get; private set; }

        /// <summary>
        /// The HTTP status code returned by server.
        /// </summary>        
        public HttpStatusCode? Status { get; private set; } // this is nullable because this error can also occur if the handler throws an exception

        /// <summary>
        /// Initializes an instance of <see cref="MobileServiceTableOperationError"/>
        /// </summary>
        /// <param name="status">The HTTP status code returned by server.</param>
        /// <param name="operationKind">The kind of table operation.</param>
        /// <param name="tableName">The name of the remote table.</param>
        /// <param name="item">The item associated with the operation.</param>
        /// <param name="rawResult">Raw response of the table operation.</param>
        /// <param name="result">Response of the table operation.</param>
        public MobileServiceTableOperationError(HttpStatusCode? status, 
                                                MobileServiceTableOperationKind operationKind, 
                                                string tableName, 
                                                JObject item,
                                                string rawResult,
                                                JToken result)
        {
            this.Id = Guid.NewGuid().ToString();
            this.Status = status;
            this.OperationKind = operationKind;
            this.TableName = tableName;
            this.Item = item;
            this.RawResult = rawResult;
            this.Result = result;
        }

        internal JObject Serialize()
        {
            return new JObject()
            {
                { "id", this.Id },
                { "httpStatus", this.Status.HasValue ? (int?)this.Status.Value: null },
                { "operationKind", (int)this.OperationKind },
                { "tableName", this.TableName },
                { "item", this.Item },
                { "rawResult", this.RawResult }
            };
        }

        internal static MobileServiceTableOperationError Deserialize(JObject obj, MobileServiceJsonSerializerSettings settings)
        {
            HttpStatusCode? status = null;
            if (obj["httpStatus"] != null)
            {
                status = (HttpStatusCode)obj.Value<int>("httpStatus");
            }
            MobileServiceTableOperationKind operation = (MobileServiceTableOperationKind)obj.Value<int>("operationKind");
            var tableName = obj.Value<string>("tableName");
            JToken item = obj.Value<string>("item");
            string rawResult = obj.Value<string>("rawResult");
            JToken result = rawResult.ParseToJToken(settings);
            string id = obj.Value<string>("id");
            return new MobileServiceTableOperationError(status, operation, tableName, item as JObject, rawResult, result) { Id = id };
        }
    }
}
