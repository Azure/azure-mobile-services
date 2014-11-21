// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Sync;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Provides extension methods on <see cref="IMobileServiceSyncTable"/>
    /// </summary>
    public static class MobileServiceSyncTableExtensions
    {
        /// <summary>
        /// Pulls all items that match the given query from the associated remote table. Supports incremental sync.
        /// </summary>
        /// <param name="table">The instance of table to execute pull on.</param>
        /// <param name="queryId">
        /// A string that uniquely identifies this query and is used to keep track of its sync state. Supplying this parameter enables incremental sync whenever the same key is used again. Must be 25 characters or less and contain only alphanumeric characters, dash, and underscore.
        /// </param>
        /// <param name="query">
        /// An OData query that determines which items to 
        /// pull from the remote table.
        /// </param>
        /// <returns>
        /// A task that completes when pull operation has finished.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="queryId"/> does not match the regular expression <value>[a-zA-Z][a-zA-Z0-9_-]{0,24}</value>.
        /// </exception>
        public static Task PullAsync(this IMobileServiceSyncTable table, string queryId, string query)
        {
            return table.PullAsync(queryId, query, null, CancellationToken.None);
        }


        /// <summary>
        /// Pulls all items that match the given query from the associated remote table. Supports incremental sync.
        /// </summary>
        /// <param name="table">The instance of table to execute pull on.</param>
        /// <param name="queryId">
        /// A string that uniquely identifies this query and is used to keep track of its sync state. Supplying this parameter enables incremental sync whenever the same key is used again. Must be 25 characters or less and contain only alphanumeric characters, dash, and underscore.
        /// </param>
        /// <param name="query">
        /// An OData query that determines which items to 
        /// pull from the remote table.
        /// </param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to include in 
        /// the request URI query string.
        /// </param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> token to observe
        /// </param>
        /// <returns>
        /// A task that completes when pull operation has finished.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="queryId"/> does not match the regular expression <value>[a-zA-Z][a-zA-Z0-9_-]{0,24}</value>.
        /// </exception>
        public static Task PullAsync(this IMobileServiceSyncTable table, string queryId, string query, IDictionary<string, string> parameters, CancellationToken cancellationToken)
        {
            return table.PullAsync(queryId, query, parameters, true, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Pulls all items that match the given query from the associated remote table.
        /// </summary>
        /// <param name="table">The instance of table to execute pull on.</param>
        /// <param name="queryId">
        /// A string that uniquely identifies this query and is used to keep track of its sync state. Supplying this parameter enables incremental sync whenever the same key is used again. Must be 25 characters or less and contain only alphanumeric characters, dash, and underscore.
        /// </param>
        /// <param name="query">
        /// An OData query that determines which items to 
        /// pull from the remote table.
        /// </param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> token to observe
        /// </param>
        /// <returns>
        /// A task that completes when pull operation has finished.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="queryId"/> does not match the regular expression <value>[a-zA-Z][a-zA-Z0-9_-]{0,24}</value>.
        /// </exception>
        public static Task PullAsync<T, U>(this IMobileServiceSyncTable<T> table, string queryId, IMobileServiceTableQuery<U> query, CancellationToken cancellationToken)
        {
            return table.PullAsync(queryId, query, pushOtherTables: true, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Pulls all items that match the given query
        /// from the associated remote table.
        /// </summary>
        /// <param name="table">The instance of table to execute pull on.</param>
        /// <param name="queryId">
        /// A string that uniquely identifies this query and is used to keep track of its sync state. Supplying this parameter enables incremental sync whenever the same key is used again. Must be 25 characters or less and contain only alphanumeric characters, dash, and underscore.
        /// </param>
        /// <param name="query">
        /// An OData query that determines which items to 
        /// pull from the remote table.
        /// </param>
        /// <returns>
        /// A task that completes when pull operation has finished.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="queryId"/> does not match the regular expression <value>[a-zA-Z][a-zA-Z0-9_-]{0,24}</value>.
        /// </exception>
        public static Task PullAsync<T, U>(this IMobileServiceSyncTable<T> table, string queryId, IMobileServiceTableQuery<U> query)
        {
            return table.PullAsync(queryId, query, cancellationToken: CancellationToken.None);
        }

        /// <summary>
        /// Deletes all the items in local table
        /// </summary>
        /// <param name="table">The instance of table to execute purge on.</param>
        /// <returns>A task that completes when purge operation has finished.</returns>
        public static Task PurgeAsync(this IMobileServiceSyncTable table)
        {
            return table.PurgeAsync(null, null, false, CancellationToken.None);
        }

        /// <summary>
        /// Deletes all the items in local table
        /// </summary>
        /// <param name="table">The instance of table to execute purge on.</param>
        /// <param name="force">Force the purge by discarding the pending operations.</param>
        /// <returns>A task that completes when purge operation has finished.</returns>
        public static Task PurgeAsync(this IMobileServiceSyncTable table, bool force)
        {
            return table.PurgeAsync(null, null, force, CancellationToken.None);
        }

        /// <summary>
        /// Deletes all the items in local table that match the query.
        /// </summary>
        /// <param name="table">The instance of table to execute purge on.</param>
        /// <param name="query">An OData query that determines which items to delete.</param>
        /// <returns>A task that completes when purge operation has finished.</returns>
        public static Task PurgeAsync(this IMobileServiceSyncTable table, string query)
        {
            return table.PurgeAsync(null, query, false, CancellationToken.None);
        }


        /// <summary>
        /// Deletes all the items in local table that match the query.
        /// </summary>
        /// <param name="table">The instance of table to execute purge on.</param>
        /// <param name="query">An OData query that determines which items to delete.</param>
        /// <returns>A task that completes when purge operation has finished.</returns>
        public static Task PurgeAsync<T, U>(this IMobileServiceSyncTable<T> table, IMobileServiceTableQuery<U> query)
        {
            return table.PurgeAsync(null, query, CancellationToken.None);
        }
    }
}
