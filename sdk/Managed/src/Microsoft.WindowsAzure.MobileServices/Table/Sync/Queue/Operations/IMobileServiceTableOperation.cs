// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    /// <summary>
    /// An object representing table operation against remote table
    /// </summary>
    public interface IMobileServiceTableOperation
    {
        /// <summary>
        /// The kind of operation
        /// </summary>
        MobileServiceTableOperationKind Kind { get; }

        /// <summary>
        /// Name of the table the operation will be executed against.
        /// </summary>
        string TableName { get; }

        /// <summary>
        /// The item associated with the operation.
        /// </summary>
        JObject Item { get; set; }

        /// <summary>
        /// Result returned by the server.
        /// </summary>
        JToken Result { get; set; }

        /// <summary>
        /// Executes the operation against remote table.
        /// </summary>
        Task ExecuteAsync();

        /// <summary>
        /// Abort the parent push operation.
        /// </summary>
        void AbortPush();
    }
}
