// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Provides operations on tables for a Mobile Service.
    /// </summary>
    public partial interface IMobileServiceTable
    {
        /// <summary>
        /// Excute a query against a table.
        /// </summary>
        /// <param name="query">
        /// An OData query to execute.
        /// </param>
        /// <returns>
        /// A task that will return with results when the query finishes.
        /// </returns>
        [SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames", MessageId = "1#", Justification = "Query is both a verb and noun.")]
        Task<JToken> ReadAsync(string query);

        /// <summary>
        /// Insert a new object into a table.
        /// </summary>
        /// <param name="instance">
        /// The instance to insert into the table.
        /// </param>
        /// <returns>
        /// A task that will complete when the insert finishes.
        /// </returns>
        Task<JToken> InsertAsync(JObject instance);

        /// <summary>
        /// Update an object in a given table.
        /// </summary>
        /// <param name="instance">
        /// The instance to update in the table.
        /// </param>
        /// <returns>
        /// A task that will complete when the update finishes.
        /// </returns>
        Task<JToken> UpdateAsync(JObject instance);

        /// <summary>
        /// Delete an object from a given table.
        /// </summary>
        /// <param name="instance">
        /// The instance to delete from the table.
        /// </param>
        /// <returns>
        /// A task that will complete when the delete finishes.
        /// </returns>
        Task DeleteAsync(JObject instance);
    }
}
