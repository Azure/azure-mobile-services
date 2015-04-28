// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Provides extension methods on <see cref="IMobileServiceTable"/>
    /// </summary>
    public static class MobileServiceTableExtensions
    {
        /// <summary>
        /// Executes a query against the table.
        /// </summary>
        /// <param name="table">
        /// The instance of table to read from.
        /// </param>
        /// <param name="query">
        /// A query to execute.
        /// </param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to include in 
        /// the request URI query string.
        /// </param>
        /// <returns>
        /// A task that will return with results when the query finishes.
        /// </returns>
        public static Task<JToken> ReadAsync(this IMobileServiceTable table, string query, IDictionary<string, string> parameters)
        {
            return table.ReadAsync(query, parameters, wrapResult: false);
        }
    }
}
