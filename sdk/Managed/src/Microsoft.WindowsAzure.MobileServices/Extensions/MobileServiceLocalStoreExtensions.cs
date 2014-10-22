// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Query;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    /// <summary>
    ///  Provides extension methods on <see cref="IMobileServiceLocalStore"/>.
    /// </summary>
    internal static class MobileServiceLocalStoreExtensions
    {
        /// <summary>
        /// Updates or inserts data in local table.
        /// </summary>
        /// <param name="store">Instance of <see cref="IMobileServiceLocalStore"/></param>
        /// <param name="tableName">Name of the local table.</param>
        /// <param name="item">Item to be inserted.</param>
        /// <param name="fromServer"><code>true</code> if the call is made based on data coming from the server e.g. in a pull operation; <code>false</code> if the call is made by the client, such as insert or update calls on an <see cref="IMobileServiceSyncTable"/>.</param>
        /// <returns>A task that completes when item has been upserted in local table.</returns>
        public static Task UpsertAsync(this IMobileServiceLocalStore store, string tableName, JObject item, bool fromServer)
        {
            return store.UpsertAsync(tableName, new[] { item }, fromServer);
        }

        /// <summary>
        /// Deletes an item with the specified id in the local table.
        /// </summary>
        /// <param name="store">Instance of <see cref="IMobileServiceLocalStore"/></param>
        /// <param name="tableName">Name of the local table.</param>
        /// <param name="id">Id for the object to be deleted.</param>
        /// <returns>A task that compltes when delete has been executed on local table.</returns>
        public static Task DeleteAsync(this IMobileServiceLocalStore store, string tableName, string id)
        {
            return store.DeleteAsync(tableName, new[] { id });
        }

        /// <summary>
        /// Counts all the items in a local table
        /// </summary>
        /// <param name="store">Instance of <see cref="IMobileServiceLocalStore"/></param>
        /// <param name="tableName">Name of the table</param>
        /// <returns>Task that will complete with count of items.</returns>
        public async static Task<long> CountAsync(this IMobileServiceLocalStore store, string tableName)
        {
            var query = new MobileServiceTableQueryDescription(MobileServiceLocalSystemTables.OperationQueue);
            return await CountAsync(store, query);
        }

        /// <summary>
        /// Counts all the items returned from the query
        /// </summary>
        /// <param name="store">An instance of <see cref="IMobileServiceLocalStore"/></param>
        /// <param name="query">An instance of <see cref="MobileServiceTableQueryDescription"/></param>
        /// <returns>Task that will complete with count of items.</returns>
        public static async Task<long> CountAsync(this IMobileServiceLocalStore store, MobileServiceTableQueryDescription query)
        {
            query.Top = 0;
            query.IncludeTotalCount = true;

            QueryResult result = await store.QueryAsync(query);
            return result.TotalCount;
        }

        /// <summary>
        /// Executes the query on local store and returns the parsed result
        /// </summary>
        /// <param name="store">An instance of <see cref="IMobileServiceLocalStore"/></param>
        /// <param name="query">An instance of <see cref="MobileServiceTableQueryDescription"/></param>
        /// <returns>Task that will complete with the parsed result of the query.</returns>
        public static async Task<QueryResult> QueryAsync(this IMobileServiceLocalStore store, MobileServiceTableQueryDescription query)
        {
            return QueryResult.Parse(await store.ReadAsync(query), null, validate: true);
        }

        /// <summary>
        /// Executes the query on local store and returns the first or default item from parsed result
        /// </summary>
        /// <param name="store">An instance of <see cref="IMobileServiceLocalStore"/></param>
        /// <param name="query">An instance of <see cref="MobileServiceTableQueryDescription"/></param>
        /// <returns>Task that will complete with the first or default item from parsed result of the query.</returns>
        public static async Task<JObject> FirstOrDefault(this IMobileServiceLocalStore store, MobileServiceTableQueryDescription query)
        {
            QueryResult result = await store.QueryAsync(query);
            return result.Values.FirstOrDefault() as JObject;
        }
    }
}
