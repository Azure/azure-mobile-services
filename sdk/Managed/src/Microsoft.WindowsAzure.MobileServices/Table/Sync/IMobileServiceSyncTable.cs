// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    /// <summary>
    /// Provides operations on local table.
    /// </summary>
    public interface IMobileServiceSyncTable
    {
        /// <summary>
        /// Gets a reference to the <see cref="MobileServiceClient"/> associated 
        /// with this table.
        /// </summary>
        MobileServiceClient MobileServiceClient { get; }

        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        string TableName { get; }

        /// <summary>
        /// The supported odata options on the remote table
        /// </summary>
        MobileServiceRemoteTableOptions SupportedOptions { get; set; }

        /// <summary>
        /// Executes a query against the table.
        /// </summary>
        /// <param name="query">
        /// A query to execute.
        /// </param>
        /// <returns>
        /// A task that will return with results when the query finishes.
        /// </returns>
        Task<JToken> ReadAsync(string query);

        /// <summary>
        /// Inserts an <paramref name="instance"/> into the table.
        /// </summary>
        /// <param name="instance">
        /// The instance to insert into the table.
        /// </param>
        /// <returns>
        /// A task that will complete when the insert finishes.
        /// </returns>
        Task<JObject> InsertAsync(JObject instance);

        /// <summary>
        /// Updates an <paramref name="instance"/> in the table.
        /// </summary>
        /// <param name="instance">
        /// The instance to update in the table.
        /// </param>
        /// <returns>
        /// A task that will complete when the update finishes.
        /// </returns>
        Task UpdateAsync(JObject instance);

        /// <summary>
        /// Deletes an <paramref name="instance"/> from the table.
        /// </summary>
        /// <param name="instance">
        /// The instance to delete from the table.
        /// </param>
        /// <returns>
        /// A task that will complete when the delete finishes.
        /// </returns>
        Task DeleteAsync(JObject instance);

        /// <summary>
        /// Executes a lookup against a table.
        /// </summary>
        /// <param name="id">
        /// The id of the instance to lookup.
        /// </param>
        /// <returns>
        /// A task that will return with a result when the lookup finishes.
        /// </returns>
        Task<JObject> LookupAsync(string id);

        /// <summary>
        /// Pulls all items that match the given query from the associated remote table. Supports incremental sync.
        /// </summary>
        /// <param name="queryId">
        /// A string that uniquely identifies this query and is used to keep track of its sync state. Supplying this parameter enables incremental sync whenever the same key is used again.
        /// </param>
        /// <param name="query">
        /// An OData query that determines which items to 
        /// pull from the remote table.
        /// </param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to include in 
        /// the request URI query string.
        /// </param>
        /// <param name="pushOtherTables">
        /// Push other tables if this table is dirty.
        /// </param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> token to observe
        /// </param>
        /// <returns>
        /// A task that completes when pull operation has finished.
        /// </returns>
        Task PullAsync(string queryId, string query, IDictionary<string, string> parameters, bool pushOtherTables, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes all the items in local table that match the query.
        /// </summary>
        /// <param name="queryId">
        /// A string that uniquely identifies this query and is used to keep track of its sync state. Supplying this parameter resets the incremental sync state for the query.
        /// </param>
        /// <param name="query">An OData query that determines which items to delete.</param>
        /// <param name="force">Force the purge by discarding the pending operations.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> token to observe
        /// </param>
        /// <returns>A task that completes when purge operation has finished.</returns>
        Task PurgeAsync(string queryId, string query, bool force, CancellationToken cancellationToken);
    }
}
