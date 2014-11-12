// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Query;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    /// <summary>
    /// Allows saving and reading data in the local tables.
    /// </summary>
    public interface IMobileServiceLocalStore : IDisposable
    {
        /// <summary>
        /// Initializes the store for use.
        /// </summary>
        /// <returns>A task that completes when store has initialized.</returns>
        Task InitializeAsync();

        /// <summary>
        /// Reads data from local table by executing the query.
        /// </summary>
        /// <param name="query">Instance of <see cref="MobileServiceTableQueryDescription"/> that defines the query to be executed on local table.</param>
        /// <returns>A task that returns instance of JObject or JArray with items matching the query.</returns>
        Task<JToken> ReadAsync(MobileServiceTableQueryDescription query);

        /// <summary>
        /// Updates or inserts data in local table.
        /// </summary>
        /// <param name="tableName">Name of the local table.</param>
        /// <param name="items">A list of items to be inserted.</param>
        /// <param name="ignoreMissingColumns"><code>true</code> if the extra properties on item can be ignored; <code>false</code> otherwise.</param>
        /// <returns>A task that completes when item has been upserted in local table.</returns>
        Task UpsertAsync(string tableName, IEnumerable<JObject> items, bool ignoreMissingColumns);

        /// <summary>
        /// Deletes all the items from local table that match the query.
        /// </summary>
        /// <param name="query">Instance of <see cref="MobileServiceTableQueryDescription"/></param>
        /// <returns>A task that completes when delete has been executed on local table.</returns>
        Task DeleteAsync(MobileServiceTableQueryDescription query);

        /// <summary>
        /// Deletes items from local table with the given list of ids
        /// </summary>
        /// <param name="tableName">Name of the local table.</param>
        /// <param name="ids">A list of ids of the items to be deleted</param>
        /// <returns>A task that completes when delete query has executed.</returns>
        Task DeleteAsync(string tableName, IEnumerable<string> ids);

        /// <summary>
        /// Reads a single item from local table with specified id.
        /// </summary>
        /// <param name="tableName">Name of the local table.</param>
        /// <param name="id">Id for the object to be read.</param>
        /// <returns>A task that returns the item read from local table.</returns>
        Task<JObject> LookupAsync(string tableName, string id);
    }
}
