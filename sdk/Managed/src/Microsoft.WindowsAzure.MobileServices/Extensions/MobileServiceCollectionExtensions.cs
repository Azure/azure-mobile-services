// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Provides extension methods on <see cref="T:IMobileServiceTableQuery`1{T}"/>
    /// and <see cref="T:IMobileServiceTable`1{T}"/> to wrap them in a collection.
    /// </summary>
    public static class MobileServiceCollectionExtensions
    {
        /// <summary>
        /// Create a new collection based on the query.
        /// </summary>
        /// <param name="query">
        /// The query to evaluate for data.
        /// </param>
        /// <param name="pageSize">
        /// Optional page size.
        /// </param>
        /// <returns>The collection.</returns>
        public async static Task<MobileServiceCollection<TTable, TTable>> ToCollectionAsync<TTable>(this IMobileServiceTableQuery<TTable> query, int pageSize = 0)
        {
            var collection = new MobileServiceCollection<TTable, TTable>(query, pageSize);
            await collection.LoadMoreItemsAsync();
            return collection;
        }

        /// <summary>
        /// Create a new collection based on the table.
        /// </summary>
        /// <param name="table">
        /// The table from which to create the new collection. 
        /// </param>
        /// <param name="pageSize">
        /// Optional page size.
        /// </param>
        /// <returns>The collection.</returns>
        public static Task<MobileServiceCollection<TTable, TTable>> ToCollectionAsync<TTable>(this IMobileServiceTable<TTable> table, int pageSize = 0)
        {
            return table.CreateQuery().ToCollectionAsync(pageSize);
        }
    }
}
