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
        /// Executes th eoperation against remote table.
        /// </summary>
        /// <returns>The json object returned from the server.</returns>
        Task<JToken> ExecuteAsync();
    }
}
