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
    public static class MobileServiceCollectionViewExtensions
    {
        /// <summary>
        /// Create a new collection view based on the query.
        /// </summary>
        /// <returns>The collection view.</returns>
        public static MobileServiceCollectionView<T> ToCollectionView<T>(this IMobileServiceTableQuery<T> query)
        {
            return new MobileServiceCollectionView<T>(query.Table, query);
        }

        /// <summary>
        /// Create a new collection view based on the query.
        /// </summary>
        /// <returns>The collection view.</returns>
        public static MobileServiceCollectionView<T> ToCollectionView<T>(this IMobileServiceTable<T> table)
        {
            return new MobileServiceTableQuery<T>(table).ToCollectionView();
        }
    }
}
