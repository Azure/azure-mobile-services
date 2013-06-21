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
    /// Provide extension methods on <see cref="T:IMobileServiceTableQuery`1{T}"/>
    /// and <see cref="T:IMobileServiceTable`1{T}"/> to wrap them in an incremental loading collection.
    /// </summary>
    public static class MobileServiceIncrementalLoadingCollectionExtensions
    {
        /// <summary>
        /// Create a new collection view based on the query.
        /// </summary>
        /// <returns>The collection view.</returns>
        public static MobileServiceIncrementalLoadingCollection<TTable, TTable> ToIncrementalLoadingCollection<TTable>(this IMobileServiceTableQuery<TTable> query)
        {
            return new MobileServiceIncrementalLoadingCollection<TTable, TTable>(query);
        }

        /// <summary>
        /// Create a new collection view based on the query.
        /// </summary>
        /// <returns>The collection view.</returns>
        public static MobileServiceIncrementalLoadingCollection<TTable, TTable> ToIncrementalLoadingCollection<TTable>(this IMobileServiceTable<TTable> table)
        {
            return table.CreateQuery().ToIncrementalLoadingCollection();
        }
    }
}
