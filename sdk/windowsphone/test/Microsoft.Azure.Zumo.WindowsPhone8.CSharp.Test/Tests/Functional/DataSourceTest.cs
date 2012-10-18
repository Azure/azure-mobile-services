// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.Azure.Zumo.WindowsPhone8.Test;
using Microsoft.WindowsAzure.MobileServices;

namespace Microsoft.Azure.Zumo.WindowsPhone8.CSharp.Test
{
    public class DataSourceTest : FunctionalTestBase
    {
        [AsyncTestMethod]
        public async Task SimpleDataSource()
        {
            // Get the Books table
            IMobileServiceTable<Book> table = GetClient().GetTable<Book>();

            // Create a new CollectionView
            Log("Creating DataSource");
            MobileServiceCollectionView<Book> dataSource =
                table.OrderByDescending(b => b.Price).ToCollectionView();

            // Spin until the data finishes loading on another thread
            Log("Waiting for population");
            while (dataSource.Count <= 0)
            {
                await Task.Delay(500);
            }

            Log("Verifying loaded");
            Assert.AreEqual(18, dataSource.Count);
            Assert.AreEqual(22.95, dataSource[0].Price);
        }
    }
}
