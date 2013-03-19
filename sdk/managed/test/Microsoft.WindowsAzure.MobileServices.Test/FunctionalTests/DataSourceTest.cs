// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.TestFramework;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    [Tag("data")]
    [Tag("e2e")]
    public class DataSourceTest : FunctionalTestBase
    {
        [AsyncTestMethod]
        public async Task SimpleDataSource()
        {
            // Get the Books table
            IMobileServiceTable<Book> table = GetClient().GetTable<Book>();

            // Create a new CollectionView
            Log("Creating DataSource");
            MobileServiceCollection<Book,Book> dataSource =
                await table.OrderByDescending(b => b.Price).ToCollectionAsync();

            Log("Verifying loaded");
            Assert.AreEqual(18, dataSource.Count);
            Assert.AreEqual((long)-1, ((ITotalCountProvider)dataSource).TotalCount);
            Assert.AreEqual(22.95, dataSource[0].Price);
        }

        [AsyncTestMethod]
        public async Task SimpleDataSourceWithTotalCount()
        {
            // Get the Books table
            IMobileServiceTable<Book> table = GetClient().GetTable<Book>();

            // Create a new CollectionView
            Log("Creating DataSource");
            MobileServiceCollection<Book,Book> dataSource =
                await table.Take(5).IncludeTotalCount().ToCollectionAsync();

            Log("Verifying loaded");
            Assert.AreEqual(5, dataSource.Count);
            Assert.AreEqual((long)18, ((ITotalCountProvider)dataSource).TotalCount);
        }
    }
}
