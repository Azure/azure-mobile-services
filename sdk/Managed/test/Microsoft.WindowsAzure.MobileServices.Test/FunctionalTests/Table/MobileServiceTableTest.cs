// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.TestFramework;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    [DataTable("stringId_test_table")]
    public class ToDoWithStringId
    {
        public string Id { get; set; }

        public string String { get; set; }
    }

    [DataTable("test_table")]
    public class ToDoWithStringIdAgainstIntIdTable
    {
        public string Id { get; set; }

        public string String { get; set; }
    }

    [DataTable("test_table")]
    public class ToDoWithIntId
    {
        public long Id { get; set; }

        public string String { get; set; }
    }

    [DataTable("stringId_test_table")]
    public class ToDoWithSystemPropertiesType
    {
        public string Id { get; set; }

        public string String { get; set; }

        [CreatedAt]
        public DateTime CreatedAt { get; set; }

        [UpdatedAt]
        public DateTime UpdatedAt { get; set; }

        [Version]
        public String Version { get; set; }
    }

    [Tag("e2e")]
    [Tag("table")]
    public class MobileServiceTableGenericFunctionalTests : FunctionalTestBase
    {
        private async Task EnsureEmptyTableAsync<T>()
        {
            // Make sure the table is empty
            IMobileServiceTable<T> table = GetClient().GetTable<T>();
            IEnumerable<T> results = await table.ReadAsync();
            T[] items = results.ToArray();

            foreach (T item in items)
            {
                await table.DeleteAsync(item);
            }
        }

        [AsyncTestMethod]
        public async Task AsyncTableOperationsWithValidStringIdAgainstStringIdTable()
        {
            await EnsureEmptyTableAsync<ToDoWithStringId>();

            string[] testIdData = IdTestData.ValidStringIds;

            foreach (string testId in testIdData)
            {
                IMobileServiceTable<ToDoWithStringId> table = GetClient().GetTable<ToDoWithStringId>();

                ToDoWithStringId item = new ToDoWithStringId() { Id = testId, String = "Hey" };
                await table.InsertAsync(item);

                // Read
                IEnumerable<ToDoWithStringId> results = await table.ReadAsync();
                ToDoWithStringId[] items = results.ToArray();

                Assert.AreEqual(1, items.Count());
                Assert.AreEqual(testId, items[0].Id);
                Assert.AreEqual("Hey", items[0].String);

                // Filter
                results = await table.Where(i => i.Id == testId).ToEnumerableAsync();
                items = results.ToArray();

                Assert.AreEqual(1, items.Count());
                Assert.AreEqual(testId, items[0].Id);
                Assert.AreEqual("Hey", items[0].String);

                // Projection
                var projectedResults = await table.Select(i => new { XId = i.Id, XString = i.String }).ToEnumerableAsync();
                var projectedItems = projectedResults.ToArray();

                Assert.AreEqual(1, projectedItems.Count());
                Assert.AreEqual(testId, projectedItems[0].XId);
                Assert.AreEqual("Hey", projectedItems[0].XString);

                // Lookup
                item = await table.LookupAsync(testId);
                Assert.AreEqual(testId, item.Id);
                Assert.AreEqual("Hey", item.String);

                // Update
                item.String = "What?";
                await table.UpdateAsync(item);
                Assert.AreEqual(testId, item.Id);
                Assert.AreEqual("What?", item.String);

                // Refresh
                item = new ToDoWithStringId() { Id = testId, String = "Hey" };
                await table.RefreshAsync(item);
                Assert.AreEqual(testId, item.Id);
                Assert.AreEqual("What?", item.String);

                // Read Again
                results = await table.ReadAsync();
                items = results.ToArray();

                Assert.AreEqual(1, items.Count());
                Assert.AreEqual(testId, items[0].Id);
                Assert.AreEqual("What?", items[0].String);

                await table.DeleteAsync(item);
            }
        }

        [AsyncTestMethod]
        public async Task OrderingReadAsyncWithValidStringIdAgainstStringIdTable()
        {
            await EnsureEmptyTableAsync<ToDoWithStringId>();

            string[] testIdData = new string[] { "a", "b", "C", "_A", "_B", "_C", "1", "2", "3" };
            IMobileServiceTable<ToDoWithStringId> table = GetClient().GetTable<ToDoWithStringId>();

            foreach (string testId in testIdData)
            {
                ToDoWithStringId item = new ToDoWithStringId() { Id = testId, String = "Hey" };
                await table.InsertAsync(item);
            }

            IEnumerable<ToDoWithStringId> results = await table.OrderBy(p => p.Id).ToEnumerableAsync();
            ToDoWithStringId[] items = results.ToArray();

            Assert.AreEqual(9, items.Count());
            Assert.AreEqual("_A", items[0].Id);
            Assert.AreEqual("_B", items[1].Id);
            Assert.AreEqual("_C", items[2].Id);
            Assert.AreEqual("1", items[3].Id);
            Assert.AreEqual("2", items[4].Id);
            Assert.AreEqual("3", items[5].Id);
            Assert.AreEqual("a", items[6].Id);
            Assert.AreEqual("b", items[7].Id);
            Assert.AreEqual("C", items[8].Id);

            results = await table.OrderByDescending(p => p.Id).ToEnumerableAsync();
            items = results.ToArray();

            Assert.AreEqual(9, items.Count());
            Assert.AreEqual("_A", items[8].Id);
            Assert.AreEqual("_B", items[7].Id);
            Assert.AreEqual("_C", items[6].Id);
            Assert.AreEqual("1", items[5].Id);
            Assert.AreEqual("2", items[4].Id);
            Assert.AreEqual("3", items[3].Id);
            Assert.AreEqual("a", items[2].Id);
            Assert.AreEqual("b", items[1].Id);
            Assert.AreEqual("C", items[0].Id);

            foreach (string testId in testIdData)
            {
                ToDoWithStringId item = new ToDoWithStringId() { Id = testId };
                await table.DeleteAsync(item);
            }
        }

        [AsyncTestMethod]
        public async Task FilterReadAsyncWithEmptyStringIdAgainstStringIdTable()
        {
            await EnsureEmptyTableAsync<ToDoWithStringId>();

            string[] testIdData = IdTestData.ValidStringIds;
            IMobileServiceTable<ToDoWithStringId> table = GetClient().GetTable<ToDoWithStringId>();

            foreach (string testId in testIdData)
            {
                ToDoWithStringId item = new ToDoWithStringId() { Id = testId, String = "Hey" };
                await table.InsertAsync(item);
            }

            string[] invalidIdData = IdTestData.EmptyStringIds.Concat(
                                    IdTestData.InvalidStringIds).Concat( 
                                    new string[] { null }).ToArray();

            foreach (string invalidId in invalidIdData)
            {
                IEnumerable<ToDoWithStringId> results = await table.Where(p => p.Id == invalidId).ToEnumerableAsync();
                ToDoWithStringId[] items = results.ToArray();

                Assert.AreEqual(0, items.Count());
            }

            foreach (string testId in testIdData)
            {
                ToDoWithStringId item = new ToDoWithStringId() { Id = testId };
                await table.DeleteAsync(item);
            }
        }

        [AsyncTestMethod]
        public async Task LookupAsyncWithNosuchItemAgainstStringIdTable()
        {
            await EnsureEmptyTableAsync<ToDoWithStringId>();

            string[] testIdData = IdTestData.ValidStringIds;
            IMobileServiceTable<ToDoWithStringId> table = GetClient().GetTable<ToDoWithStringId>();

            foreach (string testId in testIdData)
            {
                ToDoWithStringId item = new ToDoWithStringId() { Id = testId, String = "Hey" };
                await table.InsertAsync(item);
            }

            foreach (string testId in testIdData)
            {
                ToDoWithStringId item = await table.LookupAsync(testId);
                await table.DeleteAsync(item);

                MobileServiceInvalidOperationException exception = null; 
                try
                {
                    await table.LookupAsync(testId);
                }
                catch (MobileServiceInvalidOperationException e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.AreEqual(exception.Response.StatusCode, HttpStatusCode.NotFound);
                Assert.IsTrue(exception.Message.Contains(string.Format("Error: An item with id '{0}' does not exist.", testId)));
            }
        }

        [AsyncTestMethod]
        public async Task RefreshAsyncWithNoSuchItemAgainstStringIdTable()
        {
            await EnsureEmptyTableAsync<ToDoWithStringId>();

            string[] testIdData = IdTestData.ValidStringIds;
            IMobileServiceTable<ToDoWithStringId> table = GetClient().GetTable<ToDoWithStringId>();

            foreach (string testId in testIdData)
            {
                ToDoWithStringId item = new ToDoWithStringId() { Id = testId, String = "Hey" };
                await table.InsertAsync(item);
            }

            foreach (string testId in testIdData)
            {
                ToDoWithStringId item = await table.LookupAsync(testId);
                await table.DeleteAsync(item);
                item.Id = testId;

                InvalidOperationException exception = null;
                try
                {
                    await table.RefreshAsync(item);
                }
                catch (InvalidOperationException e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
            }
        }

        [AsyncTestMethod]
        public async Task InsertAsyncWithEmptyStringIdAgainstStringIdTable()
        {
            string[] emptyIdData = IdTestData.EmptyStringIds.Concat(
                                    new string[] { null }).ToArray();
            IMobileServiceTable<ToDoWithStringId> table = GetClient().GetTable<ToDoWithStringId>();
            
            int count = 0;
            List<ToDoWithStringId> itemsToDelete = new List<ToDoWithStringId>();

            foreach (string emptyId in emptyIdData)
            {
                ToDoWithStringId item = new ToDoWithStringId() { Id = emptyId, String = (++count).ToString() };
                await table.InsertAsync(item);

                Assert.IsNotNull(item.Id);
                Assert.AreEqual(count.ToString(), item.String);
                itemsToDelete.Add(item);
            }

            foreach (var item in itemsToDelete)
            {
                await table.DeleteAsync(item);
            }
        }

        [AsyncTestMethod]
        public async Task InsertAsyncWithExistingItemAgainstStringIdTable()
        {
            await EnsureEmptyTableAsync<ToDoWithStringId>();

            string[] testIdData = IdTestData.ValidStringIds;
            IMobileServiceTable<ToDoWithStringId> table = GetClient().GetTable<ToDoWithStringId>();

            foreach (string testId in testIdData)
            {
                ToDoWithStringId item = new ToDoWithStringId() { Id = testId, String = "Hey" };
                await table.InsertAsync(item);
            }

            foreach (string testId in testIdData)
            {
                ToDoWithStringId item = await table.LookupAsync(testId);
                item.String = "No we're talking!";

                MobileServiceInvalidOperationException exception = null;
                try
                {
                    await table.InsertAsync(item);
                }
                catch (MobileServiceInvalidOperationException e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.AreEqual(exception.Response.StatusCode, HttpStatusCode.BadRequest);
                Assert.IsTrue(exception.Message.Contains("Could not insert the item because an item with that id already exists."));
            }
        }

        [AsyncTestMethod]
        public async Task UpdateAsyncWithNosuchItemAgainstStringIdTable()
        {
            await EnsureEmptyTableAsync<ToDoWithStringId>();

            string[] testIdData = IdTestData.ValidStringIds;
            IMobileServiceTable<ToDoWithStringId> table = GetClient().GetTable<ToDoWithStringId>();

            foreach (string testId in testIdData)
            {
                ToDoWithStringId item = new ToDoWithStringId() { Id = testId, String = "Hey" };
                await table.InsertAsync(item);
            }

            foreach (string testId in testIdData)
            {
                ToDoWithStringId item = await table.LookupAsync(testId);
                await table.DeleteAsync(item);
                item.Id = testId;
                item.String = "Alright!";

                MobileServiceInvalidOperationException exception = null;
                try
                {
                    await table.UpdateAsync(item);
                }
                catch (MobileServiceInvalidOperationException e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.AreEqual(exception.Response.StatusCode, HttpStatusCode.NotFound);
                Assert.IsTrue(exception.Message.Contains(string.Format("Error: An item with id '{0}' does not exist.", testId)));
            }
        }

        [AsyncTestMethod]
        public async Task DeleteAsyncWithNosuchItemAgainstStringIdTable()
        {
            await EnsureEmptyTableAsync<ToDoWithStringId>();

            string[] testIdData = IdTestData.ValidStringIds;
            IMobileServiceTable<ToDoWithStringId> table = GetClient().GetTable<ToDoWithStringId>();

            foreach (string testId in testIdData)
            {
                ToDoWithStringId item = new ToDoWithStringId() { Id = testId, String = "Hey" };
                await table.InsertAsync(item);
            }

            foreach (string testId in testIdData)
            {
                ToDoWithStringId item = await table.LookupAsync(testId);
                await table.DeleteAsync(item);
                item.Id = testId;

                MobileServiceInvalidOperationException exception = null;
                try
                {
                    await table.DeleteAsync(item);
                }
                catch (MobileServiceInvalidOperationException e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.AreEqual(exception.Response.StatusCode, HttpStatusCode.NotFound);
                Assert.IsTrue(exception.Message.Contains(string.Format("Error: An item with id '{0}' does not exist.", testId)));
            }
        }

        [AsyncTestMethod]
        public async Task AsyncTableOperationsWithIntegerAsStringIdAgainstIntIdTable()
        {
            await EnsureEmptyTableAsync<ToDoWithStringIdAgainstIntIdTable>();

            IMobileServiceTable<ToDoWithStringIdAgainstIntIdTable> stringIdTable = GetClient().GetTable<ToDoWithStringIdAgainstIntIdTable>();
            ToDoWithStringIdAgainstIntIdTable item = new ToDoWithStringIdAgainstIntIdTable() { String = "Hey" };

            // Insert
            await stringIdTable.InsertAsync(item);
            string testId = item.Id.ToString();

            // Read
            IEnumerable<ToDoWithStringIdAgainstIntIdTable> results = await stringIdTable.ReadAsync();
            ToDoWithStringIdAgainstIntIdTable[] items = results.ToArray();

            Assert.AreEqual(1, items.Count());
            Assert.AreEqual(testId, items[0].Id);
            Assert.AreEqual("Hey", items[0].String);

            // Filter
            results = await stringIdTable.Where(i => i.Id == testId).ToEnumerableAsync();
            items = results.ToArray();

            Assert.AreEqual(1, items.Count());
            Assert.AreEqual(testId, items[0].Id);
            Assert.AreEqual("Hey", items[0].String);

            // Projection
            var projectedResults = await stringIdTable.Select(i => new { XId = i.Id, XString = i.String }).ToEnumerableAsync();
            var projectedItems = projectedResults.ToArray();

            Assert.AreEqual(1, projectedItems.Count());
            Assert.AreEqual(testId, projectedItems[0].XId);
            Assert.AreEqual("Hey", projectedItems[0].XString);

            // Lookup
            ToDoWithStringIdAgainstIntIdTable stringIdItem = await stringIdTable.LookupAsync(testId);
            Assert.AreEqual(testId, stringIdItem.Id);
            Assert.AreEqual("Hey", stringIdItem.String);

            // Update
            stringIdItem.String = "What?";
            await stringIdTable.UpdateAsync(stringIdItem);
            Assert.AreEqual(testId, stringIdItem.Id);
            Assert.AreEqual("What?", stringIdItem.String);

            // Refresh
            stringIdItem = new ToDoWithStringIdAgainstIntIdTable() { Id = testId, String = "Hey" };
            await stringIdTable.RefreshAsync(stringIdItem);
            Assert.AreEqual(testId, stringIdItem.Id);
            Assert.AreEqual("What?", stringIdItem.String);

            // Read Again
            results = await stringIdTable.ReadAsync();
            items = results.ToArray();

            Assert.AreEqual(1, items.Count());
            Assert.AreEqual(testId, items[0].Id);
            Assert.AreEqual("What?", items[0].String);

            // Delete
            await stringIdTable.DeleteAsync(item);
        }

        [AsyncTestMethod]
        public async Task AsyncTableOperationsWithStringIdAgainstIntegerIdTable()
        {
            await EnsureEmptyTableAsync<ToDoWithIntId>();

            IMobileServiceTable<ToDoWithIntId> table = GetClient().GetTable<ToDoWithIntId>();
            List<ToDoWithIntId> integerIdItems = new List<ToDoWithIntId>();
            for (var i = 0; i < 10; i++)
            {
                ToDoWithIntId item = new ToDoWithIntId() { String = i.ToString() };
                await table.InsertAsync(item);
                integerIdItems.Add(item);
            }

            string[] testIdData = IdTestData.ValidStringIds.ToArray();

            IMobileServiceTable<ToDoWithStringIdAgainstIntIdTable> stringIdTable = GetClient().GetTable<ToDoWithStringIdAgainstIntIdTable>();

            foreach (string testId in testIdData)
            {
                // Filter
                Exception exception = null;
                try
                {
                    IEnumerable<ToDoWithStringIdAgainstIntIdTable> results = await stringIdTable.Where(p => p.Id == testId).ToEnumerableAsync();
                    ToDoWithStringIdAgainstIntIdTable[] items = results.ToArray();
                }
                catch (Exception e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.IsTrue(exception.Message.Contains("Bad request"));

                // Refresh
                exception = null;
                try
                {
                    ToDoWithStringIdAgainstIntIdTable item = new ToDoWithStringIdAgainstIntIdTable() { Id = testId, String = "Hey!" };
                    await stringIdTable.RefreshAsync(item);
                }
                catch (Exception e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.IsTrue(exception.Message.Contains("Bad request"));

                // Insert
                exception = null;
                try
                {
                    ToDoWithStringIdAgainstIntIdTable item = new ToDoWithStringIdAgainstIntIdTable() { Id = testId, String = "Hey!" };
                    await stringIdTable.InsertAsync(item);
                }
                catch (Exception e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.IsTrue(exception.Message.Contains("Error: A value cannot be specified for property 'id'"));

                // Lookup
                exception = null;
                try
                {
                    await stringIdTable.LookupAsync(testId);
                }
                catch (Exception e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.IsTrue(exception.Message.Contains("Error: The value specified for 'id' must be a number."));

                // Update
                exception = null;
                try
                {
                    ToDoWithStringIdAgainstIntIdTable item = new ToDoWithStringIdAgainstIntIdTable() { Id = testId, String = "Hey!" };
                    await stringIdTable.UpdateAsync(item);
                }
                catch (Exception e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.IsTrue(exception.Message.Contains("Error: The value specified for 'id' must be a number."));

                // Delete
                exception = null;
                try
                {
                    ToDoWithStringIdAgainstIntIdTable item = new ToDoWithStringIdAgainstIntIdTable() { Id = testId, String = "Hey!" };
                    await stringIdTable.DeleteAsync(item);
                }
                catch (Exception e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.IsTrue(exception.Message.Contains("Error: The value specified for 'id' must be a number."));
            }

            foreach (ToDoWithIntId integerIdItem in integerIdItems)
            {
                await table.DeleteAsync(integerIdItem);
            }
        }

        [AsyncTestMethod]
        public async Task OrderingReadAsyncWithStringIdAgainstIntegerIdTable()
        {
            await EnsureEmptyTableAsync<ToDoWithIntId>();

            IMobileServiceTable<ToDoWithIntId> table = GetClient().GetTable<ToDoWithIntId>();
            List<ToDoWithIntId> integerIdItems = new List<ToDoWithIntId>();
            for (var i = 0; i < 10; i++ )
            {
                ToDoWithIntId item = new ToDoWithIntId() { String = i.ToString() };
                await table.InsertAsync(item);
                integerIdItems.Add(item);
            }

            IMobileServiceTable<ToDoWithStringIdAgainstIntIdTable> stringIdTable = GetClient().GetTable<ToDoWithStringIdAgainstIntIdTable>();

            IEnumerable<ToDoWithStringIdAgainstIntIdTable> results = await stringIdTable.OrderBy(p => p.Id).ToEnumerableAsync();
            ToDoWithStringIdAgainstIntIdTable[] items = results.ToArray();

            Assert.AreEqual(10, items.Count());
            for (var i = 0; i < 8; i++)
            {
                Assert.AreEqual((int.Parse(items[i].Id) + 1).ToString(), items[i+1].Id);
            }

            results = await stringIdTable.OrderByDescending(p => p.Id).ToEnumerableAsync();
            items = results.ToArray();

            Assert.AreEqual(10, items.Count());
            for (var i = 8; i >= 0; i--)
            {
                Assert.AreEqual((int.Parse(items[i].Id) - 1).ToString(), items[i + 1].Id);
            }

            foreach (ToDoWithIntId integerIdItem in integerIdItems)
            {
                await table.DeleteAsync(integerIdItem);
            }
        }

        [AsyncTestMethod]
        public async Task FilterReadAsyncWithIntegerAsStringIdAgainstIntegerIdTable()
        {
            await EnsureEmptyTableAsync<ToDoWithIntId>();

            IMobileServiceTable<ToDoWithIntId> table = GetClient().GetTable<ToDoWithIntId>();
            List<ToDoWithIntId> integerIdItems = new List<ToDoWithIntId>();
            for (var i = 0; i < 10; i++)
            {
                ToDoWithIntId item = new ToDoWithIntId() { String = i.ToString() };
                await table.InsertAsync(item);
                integerIdItems.Add(item);
            }

            IMobileServiceTable<ToDoWithStringIdAgainstIntIdTable> stringIdTable = GetClient().GetTable<ToDoWithStringIdAgainstIntIdTable>();

            IEnumerable<ToDoWithStringIdAgainstIntIdTable> results = await stringIdTable.Where(p => p.Id == integerIdItems[0].Id.ToString()).ToEnumerableAsync();
            ToDoWithStringIdAgainstIntIdTable[] items = results.ToArray();
            Assert.AreEqual(1, items.Count());
            Assert.AreEqual(integerIdItems[0].Id.ToString(), items[0].Id);
            Assert.AreEqual("0", items[0].String);
        
            foreach (ToDoWithIntId integerIdItem in integerIdItems)
            {
                await table.DeleteAsync(integerIdItem);
            }
        }

        [AsyncTestMethod]
        public async Task FilterReadAsyncWithEmptyStringIdAgainstIntegerIdTable()
        {
            await EnsureEmptyTableAsync<ToDoWithIntId>();

            IMobileServiceTable<ToDoWithIntId> table = GetClient().GetTable<ToDoWithIntId>();
            List<ToDoWithIntId> integerIdItems = new List<ToDoWithIntId>();
            for (var i = 0; i < 10; i++)
            {
                ToDoWithIntId item = new ToDoWithIntId() { String = i.ToString() };
                await table.InsertAsync(item);
                integerIdItems.Add(item);
            }

            string[] testIdData = new string[] { "", " ", null };

            IMobileServiceTable<ToDoWithStringIdAgainstIntIdTable> stringIdTable = GetClient().GetTable<ToDoWithStringIdAgainstIntIdTable>();

            foreach (string testId in testIdData)
            {
                IEnumerable<ToDoWithStringIdAgainstIntIdTable> results = await stringIdTable.Where(p => p.Id == testId).ToEnumerableAsync();
                ToDoWithStringIdAgainstIntIdTable[] items = results.ToArray();

                Assert.AreEqual(0, items.Length);
            }

            foreach (ToDoWithIntId integerIdItem in integerIdItems)
            {
                await table.DeleteAsync(integerIdItem);
            }
        }

        [AsyncTestMethod]
        public async Task ReadAsyncWithValidIntIdAgainstIntIdTable()
        {
            await EnsureEmptyTableAsync<ToDoWithIntId>();

            IMobileServiceTable<ToDoWithIntId> table = GetClient().GetTable<ToDoWithIntId>();

            ToDoWithIntId item = new ToDoWithIntId() { String = "Hey" };
            await table.InsertAsync(item);

            IEnumerable<ToDoWithIntId> results = await table.ReadAsync();
            ToDoWithIntId[] items = results.ToArray();

            Assert.AreEqual(1, items.Count());
            Assert.IsTrue(items[0].Id > 0);
            Assert.AreEqual("Hey", items[0].String);

            await table.DeleteAsync(item);
        }

        [AsyncTestMethod]
        public async Task AsyncTableOperationsWithAllSystemProperties()
        {
            await EnsureEmptyTableAsync<ToDoWithSystemPropertiesType>();

            string id = "an id";
            IMobileServiceTable<ToDoWithSystemPropertiesType> table = GetClient().GetTable<ToDoWithSystemPropertiesType>();

            ToDoWithSystemPropertiesType item = new ToDoWithSystemPropertiesType() { Id = id, String = "a value" };
            await table.InsertAsync(item);

            Assert.IsNotNull(item.CreatedAt);
            Assert.IsNotNull(item.UpdatedAt);
            Assert.IsNotNull(item.Version);

            // Read
            IEnumerable<ToDoWithSystemPropertiesType> results = await table.ReadAsync();
            ToDoWithSystemPropertiesType[] items = results.ToArray();

            Assert.AreEqual(1, items.Count());
            Assert.IsNotNull(items[0].CreatedAt);
            Assert.IsNotNull(items[0].UpdatedAt);
            Assert.IsNotNull(items[0].Version);

            // Filter against version
            results = await table.Where(i => i.Version == items[0].Version).ToEnumerableAsync();
            ToDoWithSystemPropertiesType[] filterItems = results.ToArray();

            Assert.AreEqual(1, items.Count());
            Assert.AreEqual(filterItems[0].CreatedAt, items[0].CreatedAt);
            Assert.AreEqual(filterItems[0].UpdatedAt, items[0].UpdatedAt);
            Assert.AreEqual(filterItems[0].Version, items[0].Version);

            // Filter against createdAt
            results = await table.Where(i => i.CreatedAt == items[0].CreatedAt).ToEnumerableAsync();
            filterItems = results.ToArray();

            Assert.AreEqual(1, items.Count());
            Assert.AreEqual(filterItems[0].CreatedAt, items[0].CreatedAt);
            Assert.AreEqual(filterItems[0].UpdatedAt, items[0].UpdatedAt);
            Assert.AreEqual(filterItems[0].Version, items[0].Version);

            // Filter against updatedAt
            results = await table.Where(i => i.UpdatedAt == items[0].UpdatedAt).ToEnumerableAsync();
            filterItems = results.ToArray();

            Assert.AreEqual(1, items.Count());
            Assert.AreEqual(filterItems[0].CreatedAt, items[0].CreatedAt);
            Assert.AreEqual(filterItems[0].UpdatedAt, items[0].UpdatedAt);
            Assert.AreEqual(filterItems[0].Version, items[0].Version);

            // Projection
            var projectedResults = await table.Select(i => new { XId = i.Id, XCreatedAt = i.CreatedAt, XUpdatedAt = i.UpdatedAt, XVersion = i.Version }).ToEnumerableAsync();
            var projectedItems = projectedResults.ToArray();

            Assert.AreEqual(1, projectedResults.Count());
            Assert.AreEqual(projectedItems[0].XId, items[0].Id);
            Assert.AreEqual(projectedItems[0].XCreatedAt, items[0].CreatedAt);
            Assert.AreEqual(projectedItems[0].XUpdatedAt, items[0].UpdatedAt);
            Assert.AreEqual(projectedItems[0].XVersion, items[0].Version);

            // Lookup
            item = await table.LookupAsync(id);
            Assert.AreEqual(id, item.Id);
            Assert.AreEqual(item.Id, items[0].Id);
            Assert.AreEqual(item.CreatedAt, items[0].CreatedAt);
            Assert.AreEqual(item.UpdatedAt, items[0].UpdatedAt);
            Assert.AreEqual(item.Version, items[0].Version);

            // Refresh
            item = new ToDoWithSystemPropertiesType() { Id = id };
            await table.RefreshAsync(item);
            Assert.AreEqual(id, item.Id);
            Assert.AreEqual(item.Id, items[0].Id);
            Assert.AreEqual(item.CreatedAt, items[0].CreatedAt);
            Assert.AreEqual(item.UpdatedAt, items[0].UpdatedAt);
            Assert.AreEqual(item.Version, items[0].Version);

            // Update
            item.String = "Hello!";
            await table.UpdateAsync(item);
            Assert.AreEqual(item.Id, items[0].Id);
            Assert.AreEqual(item.CreatedAt, items[0].CreatedAt);
            Assert.IsTrue(item.UpdatedAt >= items[0].UpdatedAt);
            Assert.IsNotNull(item.Version);
            Assert.AreNotEqual(item.Version, items[0].Version);

            // Read Again
            results = await table.ReadAsync();
            items = results.ToArray();
            Assert.AreEqual(id, item.Id);
            Assert.AreEqual(item.Id, items[0].Id);
            Assert.AreEqual(item.CreatedAt, items[0].CreatedAt);
            Assert.AreEqual(item.UpdatedAt, items[0].UpdatedAt);
            Assert.AreEqual(item.Version, items[0].Version);

            await table.DeleteAsync(item);
        }

        [AsyncTestMethod]
        public async Task AsyncTableOperationsWithSystemPropertiesSetExplicitly()
        {
            await EnsureEmptyTableAsync<ToDoWithSystemPropertiesType>();

            IMobileServiceTable<ToDoWithSystemPropertiesType> allSystemPropertiesTable = GetClient().GetTable<ToDoWithSystemPropertiesType>();

            // Regular insert
            ToDoWithSystemPropertiesType item = new ToDoWithSystemPropertiesType() { String = "a value" };
            await allSystemPropertiesTable.InsertAsync(item);

            Assert.IsNotNull(item.CreatedAt);
            Assert.IsNotNull(item.UpdatedAt);
            Assert.IsNotNull(item.Version);

            // Explicit System Properties insert
            ToDoWithSystemPropertiesType item2 = new ToDoWithSystemPropertiesType();
            allSystemPropertiesTable.SystemProperties = MobileServiceSystemProperties.Version | MobileServiceSystemProperties.CreatedAt;
            await allSystemPropertiesTable.InsertAsync(item2);

            Assert.IsNotNull(item2.CreatedAt);
            Assert.AreEqual(new DateTime(), item2.UpdatedAt);
            Assert.IsNotNull(item2.Version);

            // Explicit System Properties Read
            allSystemPropertiesTable.SystemProperties = MobileServiceSystemProperties.Version | MobileServiceSystemProperties.UpdatedAt;
            IEnumerable<ToDoWithSystemPropertiesType> results = await allSystemPropertiesTable.Where(p => p.Id == item2.Id).ToEnumerableAsync();
            ToDoWithSystemPropertiesType[] items = results.ToArray();

            Assert.AreEqual(1, items.Count());
            Assert.AreEqual(new DateTime(), items[0].CreatedAt);
            Assert.IsNotNull(items[0].UpdatedAt);
            Assert.IsNotNull(items[0].Version);

            // Lookup
            allSystemPropertiesTable.SystemProperties = MobileServiceSystemProperties.None;
            var item3 = await allSystemPropertiesTable.LookupAsync(item2.Id);
            Assert.AreEqual(new DateTime(), item3.CreatedAt);
            Assert.AreEqual(new DateTime(), item3.UpdatedAt);
            Assert.IsNull(item3.Version);

            await allSystemPropertiesTable.DeleteAsync(item);
            await allSystemPropertiesTable.DeleteAsync(item2);
        }

        [AsyncTestMethod]
        public async Task AsyncTableOperationsWithAllSystemPropertiesUsingCustomSystemParameters()
        {
            await EnsureEmptyTableAsync<ToDoWithSystemPropertiesType>();

            foreach (string systemProperties in SystemPropertiesTestData.ValidSystemPropertyQueryStrings)
            {
                string[] systemPropertiesKeyValue = systemProperties.Split('=');
                string key = systemPropertiesKeyValue[0];
                string value = systemPropertiesKeyValue[1];
                Dictionary<string, string> userParameters = new Dictionary<string, string>() { { key, value  } };

                bool shouldHaveCreatedAt = value.ToLower().Contains("created");
                bool shouldHaveUpdatedAt = value.ToLower().Contains("updated");
                bool shouldHaveVersion = value.ToLower().Contains("version");

                if (value.Trim() == "*")
                {
                    shouldHaveVersion = shouldHaveUpdatedAt = shouldHaveCreatedAt = true;
                }

                string id = "an id";
                IMobileServiceTable<ToDoWithSystemPropertiesType> table = GetClient().GetTable<ToDoWithSystemPropertiesType>();

                ToDoWithSystemPropertiesType item = new ToDoWithSystemPropertiesType() { Id = id, String = "a value" };
                await table.InsertAsync(item, userParameters);

                if (shouldHaveCreatedAt)
                {
                    Assert.IsNotNull(item.CreatedAt);
                }
                else
                {
                    Assert.AreEqual(new DateTime(), item.CreatedAt);
                }
                if (shouldHaveUpdatedAt)
                {
                    Assert.IsNotNull(item.UpdatedAt);
                }
                else
                {
                    Assert.AreEqual(new DateTime(), item.UpdatedAt);
                }
                if (shouldHaveVersion)
                {
                    Assert.IsNotNull(item.Version);
                }
                else
                {
                    Assert.IsNull(item.Version);
                }

                // Read
                IEnumerable<ToDoWithSystemPropertiesType> results = await table.WithParameters(userParameters).ToEnumerableAsync();
                ToDoWithSystemPropertiesType[] items = results.ToArray();

                Assert.AreEqual(1, items.Count());
                Assert.AreEqual(shouldHaveCreatedAt ? item.CreatedAt : new DateTime(), items[0].CreatedAt);
                Assert.AreEqual(shouldHaveUpdatedAt ? item.UpdatedAt : new DateTime(), items[0].UpdatedAt);
                Assert.AreEqual(shouldHaveVersion ? item.Version : null, items[0].Version);
               
                // Filter against version
                results = await table.Where(i => i.Version == item.Version).WithParameters(userParameters).ToEnumerableAsync();
                ToDoWithSystemPropertiesType[] filterItems = results.ToArray();

                if (shouldHaveVersion)
                {
                    Assert.AreEqual(1, filterItems.Count());
                    Assert.AreEqual(shouldHaveCreatedAt ? item.CreatedAt : new DateTime(), filterItems[0].CreatedAt);
                    Assert.AreEqual(shouldHaveUpdatedAt ? item.UpdatedAt : new DateTime(), filterItems[0].UpdatedAt);
                    Assert.AreEqual(shouldHaveVersion ? item.Version : null, filterItems[0].Version);
                }
                else
                {
                    Assert.AreEqual(0, filterItems.Count());
                }

                // Filter against createdAt
                results = await table.Where(i => i.CreatedAt == item.CreatedAt).WithParameters(userParameters).ToEnumerableAsync();
                filterItems = results.ToArray();

                if (shouldHaveCreatedAt)
                {
                    Assert.AreEqual(1, filterItems.Count());
                    Assert.AreEqual(shouldHaveCreatedAt ? item.CreatedAt : new DateTime(), filterItems[0].CreatedAt);
                    Assert.AreEqual(shouldHaveUpdatedAt ? item.UpdatedAt : new DateTime(), filterItems[0].UpdatedAt);
                    Assert.AreEqual(shouldHaveVersion ? item.Version : null, filterItems[0].Version);
                }
                else
                {
                    Assert.AreEqual(0, filterItems.Count());
                }

                // Filter against updatedAt
                results = await table.Where(i => i.UpdatedAt == item.UpdatedAt).WithParameters(userParameters).ToEnumerableAsync();
                filterItems = results.ToArray();

                if (shouldHaveUpdatedAt)
                {
                    Assert.AreEqual(1, filterItems.Count());
                    Assert.AreEqual(shouldHaveCreatedAt ? item.CreatedAt : new DateTime(), filterItems[0].CreatedAt);
                    Assert.AreEqual(shouldHaveUpdatedAt ? item.UpdatedAt : new DateTime(), filterItems[0].UpdatedAt);
                    Assert.AreEqual(shouldHaveVersion ? item.Version : null, filterItems[0].Version);
                }
                else
                {
                    Assert.AreEqual(0, filterItems.Count());
                }

                // Projection
                var projectedResults = await table.Select(i => new { XId = i.Id, XCreatedAt = i.CreatedAt, XUpdatedAt = i.UpdatedAt, XVersion = i.Version })
                                                  .WithParameters(userParameters)
                                                  .ToEnumerableAsync();
                var projectedItems = projectedResults.ToArray();

                Assert.AreEqual(1, projectedItems.Count());
                Assert.AreEqual(projectedItems[0].XId, item.Id);
                Assert.AreNotEqual(new DateTime(), projectedItems[0].XCreatedAt);
                Assert.AreNotEqual(new DateTime(), projectedItems[0].XUpdatedAt);
                Assert.IsNotNull(projectedItems[0].XVersion);

                // Lookup
                var lookupItem = await table.LookupAsync(id, userParameters);
                Assert.AreEqual(id, lookupItem.Id);
                Assert.AreEqual(shouldHaveCreatedAt ? item.CreatedAt : new DateTime(), lookupItem.CreatedAt);
                Assert.AreEqual(shouldHaveUpdatedAt ? item.UpdatedAt : new DateTime(), lookupItem.UpdatedAt);
                Assert.AreEqual(shouldHaveVersion ? item.Version : null, lookupItem.Version);

                // Refresh
                var refreshItem = new ToDoWithSystemPropertiesType() { Id = id };
                await table.RefreshAsync(refreshItem, userParameters);
                Assert.AreEqual(id, refreshItem.Id);
                Assert.AreEqual(shouldHaveCreatedAt ? item.CreatedAt : new DateTime(), refreshItem.CreatedAt);
                Assert.AreEqual(shouldHaveUpdatedAt ? item.UpdatedAt : new DateTime(), refreshItem.UpdatedAt);
                Assert.AreEqual(shouldHaveVersion ? item.Version : null, refreshItem.Version);

                // Update
                item.String = "Hello!";
                await table.UpdateAsync(item, userParameters);
                Assert.AreEqual(item.Id, items[0].Id);
                Assert.AreEqual(1, items.Count());
                if (shouldHaveCreatedAt)
                {
                    Assert.AreEqual(item.CreatedAt, items[0].CreatedAt);
                }
                else
                {
                    Assert.AreEqual(new DateTime(), item.CreatedAt);
                }
                if (shouldHaveUpdatedAt)
                {
                    Assert.IsTrue(item.UpdatedAt >= items[0].UpdatedAt);
                }
                if (shouldHaveVersion)
                {
                    Assert.IsNotNull(item.Version);
                    Assert.AreNotEqual(item.Version, items[0].Version);
                }
                else
                {
                    Assert.IsNull(item.Version);
                }

                // Read Again
                results = await table.WithParameters(userParameters).ToEnumerableAsync();
                items = results.ToArray();
                Assert.AreEqual(id, item.Id);
                Assert.AreEqual(item.Id, items[0].Id);
                if (shouldHaveCreatedAt)
                {
                    Assert.AreEqual(item.CreatedAt, items[0].CreatedAt);
                }
                if (shouldHaveUpdatedAt)
                {
                    Assert.AreEqual(item.UpdatedAt, items[0].UpdatedAt);
                }
                if (shouldHaveVersion)
                {
                    Assert.AreEqual(item.Version, items[0].Version);
                }
                else
                {
                    Assert.IsNull(item.Version);
                }

                await table.DeleteAsync(item);
            }
        }

        [AsyncTestMethod]
        public async Task AsyncTableOperationsWithInvalidSystemPropertiesQuerystring()
        {
            await EnsureEmptyTableAsync<ToDoWithSystemPropertiesType>();

            string id = "an id";
            IMobileServiceTable<ToDoWithSystemPropertiesType> table = GetClient().GetTable<ToDoWithSystemPropertiesType>();
            table.SystemProperties = MobileServiceSystemProperties.None;
            ToDoWithSystemPropertiesType item = new ToDoWithSystemPropertiesType() { Id = id, String = "a value" };

            // Insert without failing
            await table.InsertAsync(item);

            foreach (string systemProperties in SystemPropertiesTestData.InvalidSystemPropertyQueryStrings)
            {
                string[] systemPropertiesKeyValue = systemProperties.Split('=');
                string key = systemPropertiesKeyValue[0];
                string value = systemPropertiesKeyValue[1];
                Dictionary<string, string> userParameters = new Dictionary<string, string>() { { key, value } };

                // Insert
                MobileServiceInvalidOperationException exception = null;
                try
                {
                    await table.InsertAsync(item, userParameters);
                }
                catch (MobileServiceInvalidOperationException e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.AreEqual(exception.Response.StatusCode, HttpStatusCode.BadRequest);
                Assert.IsTrue(exception.Message.Contains("is not a supported system property."));
              
                // Read
                exception = null;
                try
                {
                    IEnumerable<ToDoWithSystemPropertiesType> results = await table.WithParameters(userParameters).ToEnumerableAsync();
                    ToDoWithSystemPropertiesType[] items = results.ToArray();
                }
                catch (MobileServiceInvalidOperationException e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.AreEqual(exception.Response.StatusCode, HttpStatusCode.BadRequest);
                Assert.IsTrue(exception.Message.Contains("is not a supported system property."));

                // Filter against version
                exception = null;
                try
                {
                    IEnumerable<ToDoWithSystemPropertiesType> results = await table.Where(i => i.Version == item.Version).WithParameters(userParameters).ToEnumerableAsync();
                    ToDoWithSystemPropertiesType[] filterItems = results.ToArray();
                }
                catch (MobileServiceInvalidOperationException e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.AreEqual(exception.Response.StatusCode, HttpStatusCode.BadRequest);
                Assert.IsTrue(exception.Message.Contains("is not a supported system property."));

                // Filter against createdAt
                exception = null;
                try
                {
                    IEnumerable<ToDoWithSystemPropertiesType> results = await table.Where(i => i.CreatedAt == item.CreatedAt).WithParameters(userParameters).ToEnumerableAsync();
                    ToDoWithSystemPropertiesType[] filterItems = results.ToArray();
                }
                catch (MobileServiceInvalidOperationException e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.AreEqual(exception.Response.StatusCode, HttpStatusCode.BadRequest);
                Assert.IsTrue(exception.Message.Contains("is not a supported system property."));

                // Filter against updatedAt
                exception = null;
                try
                {
                    IEnumerable<ToDoWithSystemPropertiesType> results = await table.Where(i => i.UpdatedAt == item.UpdatedAt).WithParameters(userParameters).ToEnumerableAsync();
                    ToDoWithSystemPropertiesType[] filterItems = results.ToArray();
                }
                catch (MobileServiceInvalidOperationException e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.AreEqual(exception.Response.StatusCode, HttpStatusCode.BadRequest);
                Assert.IsTrue(exception.Message.Contains("is not a supported system property."));

                // Projection
                exception = null;
                try
                {
                    var projectedResults = await table.Select(i => new { XId = i.Id, XCreatedAt = i.CreatedAt, XUpdatedAt = i.UpdatedAt, XVersion = i.Version })
                                  .WithParameters(userParameters)
                                  .ToEnumerableAsync();
                    var projectedItems = projectedResults.ToArray();
                }
                catch (MobileServiceInvalidOperationException e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.AreEqual(exception.Response.StatusCode, HttpStatusCode.BadRequest);
                Assert.IsTrue(exception.Message.Contains("is not a supported system property."));

                // Lookup
                exception = null;
                try
                {
                    var lookupItem = await table.LookupAsync(id, userParameters);
                }
                catch (MobileServiceInvalidOperationException e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.AreEqual(exception.Response.StatusCode, HttpStatusCode.BadRequest);
                Assert.IsTrue(exception.Message.Contains("is not a supported system property."));

                // Refresh
                exception = null;
                try
                {
                    var refreshItem = new ToDoWithSystemPropertiesType() { Id = id };
                    await table.RefreshAsync(refreshItem, userParameters);
                }
                catch (MobileServiceInvalidOperationException e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.AreEqual(exception.Response.StatusCode, HttpStatusCode.BadRequest);
                Assert.IsTrue(exception.Message.Contains("is not a supported system property."));

                // Update
                exception = null;
                try
                {
                    item.String = "Hello!";
                    await table.UpdateAsync(item, userParameters);
                }
                catch (MobileServiceInvalidOperationException e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.AreEqual(exception.Response.StatusCode, HttpStatusCode.BadRequest);
                Assert.IsTrue(exception.Message.Contains("is not a supported system property."));
            }

            await table.DeleteAsync(item);
        }

        [AsyncTestMethod]
        public async Task AsyncTableOperationsWithInvalidSystemParameterQueryString()
        {
            await EnsureEmptyTableAsync<ToDoWithSystemPropertiesType>();

            string id = "an id";
            IMobileServiceTable<ToDoWithSystemPropertiesType> table = GetClient().GetTable<ToDoWithSystemPropertiesType>();
            table.SystemProperties = MobileServiceSystemProperties.None;
            ToDoWithSystemPropertiesType item = new ToDoWithSystemPropertiesType() { Id = id, String = "a value"};

            // Insert without failing
            await table.InsertAsync(item);

            string[] systemPropertiesKeyValue = SystemPropertiesTestData.InvalidSystemParameterQueryString.Split('=');
            string key = systemPropertiesKeyValue[0];
            string value = systemPropertiesKeyValue[1];
            Dictionary<string, string> userParameters = new Dictionary<string, string>() { { key, value } };

            // Insert
            MobileServiceInvalidOperationException exception = null;
            try
            {
                await table.InsertAsync(item, userParameters);
            }
            catch (MobileServiceInvalidOperationException e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.AreEqual(exception.Response.StatusCode, HttpStatusCode.BadRequest);
            Assert.IsTrue(exception.Message.Contains("Custom query parameter names must start with a letter."));

            // Read
            exception = null;
            try
            {
                IEnumerable<ToDoWithSystemPropertiesType> results = await table.WithParameters(userParameters).ToEnumerableAsync();
                ToDoWithSystemPropertiesType[] items = results.ToArray();
            }
            catch (MobileServiceInvalidOperationException e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.AreEqual(exception.Response.StatusCode, HttpStatusCode.BadRequest);
            Assert.IsTrue(exception.Message.Contains("Custom query parameter names must start with a letter."));

            // Filter against version
            exception = null;
            try
            {
                IEnumerable<ToDoWithSystemPropertiesType> results = await table.Where(i => i.Version == item.Version).WithParameters(userParameters).ToEnumerableAsync();
                ToDoWithSystemPropertiesType[] filterItems = results.ToArray();
            }
            catch (MobileServiceInvalidOperationException e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.AreEqual(exception.Response.StatusCode, HttpStatusCode.BadRequest);
            Assert.IsTrue(exception.Message.Contains("Custom query parameter names must start with a letter."));

            // Filter against createdAt
            exception = null;
            try
            {
                IEnumerable<ToDoWithSystemPropertiesType> results = await table.Where(i => i.CreatedAt == item.CreatedAt).WithParameters(userParameters).ToEnumerableAsync();
                ToDoWithSystemPropertiesType[] filterItems = results.ToArray();
            }
            catch (MobileServiceInvalidOperationException e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.AreEqual(exception.Response.StatusCode, HttpStatusCode.BadRequest);
            Assert.IsTrue(exception.Message.Contains("Custom query parameter names must start with a letter."));

            // Filter against updatedAt
            exception = null;
            try
            {
                IEnumerable<ToDoWithSystemPropertiesType> results = await table.Where(i => i.UpdatedAt == item.UpdatedAt).WithParameters(userParameters).ToEnumerableAsync();
                ToDoWithSystemPropertiesType[] filterItems = results.ToArray();
            }
            catch (MobileServiceInvalidOperationException e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.AreEqual(exception.Response.StatusCode, HttpStatusCode.BadRequest);
            Assert.IsTrue(exception.Message.Contains("Custom query parameter names must start with a letter."));

            // Projection
            exception = null;
            try
            {
                var projectedResults = await table.Select(i => new { XId = i.Id, XCreatedAt = i.CreatedAt, XUpdatedAt = i.UpdatedAt, XVersion = i.Version })
                              .WithParameters(userParameters)
                              .ToEnumerableAsync();
                var projectedItems = projectedResults.ToArray();
            }
            catch (MobileServiceInvalidOperationException e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.AreEqual(exception.Response.StatusCode, HttpStatusCode.BadRequest);
            Assert.IsTrue(exception.Message.Contains("Custom query parameter names must start with a letter."));

            // Lookup
            exception = null;
            try
            {
                var lookupItem = await table.LookupAsync(id, userParameters);
            }
            catch (MobileServiceInvalidOperationException e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.AreEqual(exception.Response.StatusCode, HttpStatusCode.BadRequest);
            Assert.IsTrue(exception.Message.Contains("Custom query parameter names must start with a letter."));

            // Refresh
            exception = null;
            try
            {
                var refreshItem = new ToDoWithSystemPropertiesType() { Id = id };
                await table.RefreshAsync(refreshItem, userParameters);
            }
            catch (MobileServiceInvalidOperationException e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.AreEqual(exception.Response.StatusCode, HttpStatusCode.BadRequest);
            Assert.IsTrue(exception.Message.Contains("Custom query parameter names must start with a letter."));

            // Update
            exception = null;
            try
            {
                item.String = "Hello!";
                await table.UpdateAsync(item, userParameters);
            }
            catch (MobileServiceInvalidOperationException e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.AreEqual(exception.Response.StatusCode, HttpStatusCode.BadRequest);
            Assert.IsTrue(exception.Message.Contains("Custom query parameter names must start with a letter."));

            await table.DeleteAsync(item);
        }

        [AsyncTestMethod]
        public async Task AsyncFilterSelectOrderingOperationsNotImpactedBySystemProperties()
        {
            await EnsureEmptyTableAsync<ToDoWithSystemPropertiesType>();

            IMobileServiceTable<ToDoWithSystemPropertiesType> table = GetClient().GetTable<ToDoWithSystemPropertiesType>();
            List<ToDoWithSystemPropertiesType> items = new List<ToDoWithSystemPropertiesType>();

            // Insert some items
            for (int id = 0; id < 5; id++)
            {
                ToDoWithSystemPropertiesType item = new ToDoWithSystemPropertiesType() { Id = id.ToString(), String = "a value" };

                await table.InsertAsync(item);

                Assert.IsNotNull(item.CreatedAt);
                Assert.IsNotNull(item.UpdatedAt);
                Assert.IsNotNull(item.Version);
                items.Add(item);
            }


            foreach (MobileServiceSystemProperties systemProperties in SystemPropertiesTestData.SystemProperties)
            {
                table.SystemProperties = systemProperties;

                // Ordering
                var results  = await table.OrderBy(t => t.CreatedAt).ToEnumerableAsync();
                ToDoWithSystemPropertiesType[] orderItems = results.ToArray();

                for (int i = 0; i < orderItems.Length - 1; i++)
                {
                    Assert.IsTrue(int.Parse(orderItems[i].Id) < int.Parse(orderItems[i + 1].Id));
                }

                results = await table.OrderBy(t => t.UpdatedAt).ToEnumerableAsync();
                orderItems = results.ToArray();

                for (int i = 0; i < orderItems.Length - 1; i++)
                {
                    Assert.IsTrue(int.Parse(orderItems[i].Id) < int.Parse(orderItems[i + 1].Id));
                }

                results = await table.OrderBy(t => t.Version).ToEnumerableAsync();
                orderItems = results.ToArray();

                for (int i = 0; i < orderItems.Length - 1; i++)
                {
                    Assert.IsTrue(int.Parse(orderItems[i].Id) < int.Parse(orderItems[i + 1].Id));
                }

                // Filtering
                results = await table.Where( t => t.CreatedAt >= items[4].CreatedAt).ToEnumerableAsync();
                ToDoWithSystemPropertiesType[] filteredItems = results.ToArray();

                for (int i = 0; i < filteredItems.Length - 1; i++)
                {
                    Assert.IsTrue(filteredItems[i].CreatedAt >= items[4].CreatedAt);
                }

                results = await table.Where(t => t.UpdatedAt >= items[4].UpdatedAt).ToEnumerableAsync();
                filteredItems = results.ToArray();

                for (int i = 0; i < filteredItems.Length - 1; i++)
                {
                    Assert.IsTrue(filteredItems[i].UpdatedAt >= items[4].UpdatedAt);
                }

                results = await table.Where(t => t.Version == items[4].Version).ToEnumerableAsync();
                filteredItems = results.ToArray();

                for (int i = 0; i < filteredItems.Length - 1; i++)
                {
                    Assert.IsTrue(filteredItems[i].Version == items[4].Version);
                }

                // Selection
                var selectionResults = await table.Select(t => new { Id = t.Id, CreatedAt = t.CreatedAt  }).ToEnumerableAsync();
                var selectedItems = selectionResults.ToArray();

                for (int i = 0; i < selectedItems.Length; i++)
                {
                    var item = items.Where(t => t.Id == selectedItems[i].Id).FirstOrDefault();
                    Assert.IsTrue(item.CreatedAt == selectedItems[i].CreatedAt);
                }

                var selectionResults2 = await table.Select(t => new { Id = t.Id, UpdatedAt = t.UpdatedAt }).ToEnumerableAsync();
                var selectedItems2 = selectionResults2.ToArray();

                for (int i = 0; i < selectedItems2.Length; i++)
                {
                    var item = items.Where(t => t.Id == selectedItems2[i].Id).FirstOrDefault();
                    Assert.IsTrue(item.UpdatedAt == selectedItems2[i].UpdatedAt);
                }

                var selectionResults3 = await table.Select(t => new { Id = t.Id, Version = t.Version }).ToEnumerableAsync();
                var selectedItems3 = selectionResults3.ToArray();

                for (int i = 0; i < selectedItems3.Length; i++)
                {
                    var item = items.Where(t => t.Id == selectedItems3[i].Id).FirstOrDefault();
                    Assert.IsTrue(item.Version == selectedItems3[i].Version);
                }

            }

            // Delete
            foreach (var item in items)
            {
                await table.DeleteAsync(item);
            }
        }

        [AsyncTestMethod]
        public async Task UpdateAsyncWithMergeConflict()
        {
            await EnsureEmptyTableAsync<ToDoWithSystemPropertiesType>();
            string id = "an id";
            IMobileServiceTable table = GetClient().GetTable("stringId_test_table");
            table.SystemProperties = MobileServiceSystemProperties.Version;

            var item = new JObject() { { "id", id }, {"String", "a value" }};
            var inserted = await table.InsertAsync(item);
            item["__version"] = "random";

            MobileServicePreconditionFailedException expectedException = null;
            try
            {
                await table.UpdateAsync(item);
            }
            catch (MobileServicePreconditionFailedException ex)
            {
                expectedException = ex;
            }
            Assert.IsNotNull(expectedException);
            Assert.AreEqual(expectedException.Value["__version"], inserted["__version"]);
            Assert.AreEqual(expectedException.Value["String"], inserted["String"]);
        }

        [AsyncTestMethod]
        public async Task UpdateAsyncWitMergeConflict_Generic()
        {
            await EnsureEmptyTableAsync<ToDoWithSystemPropertiesType>();

            string id = "an id";
            IMobileServiceTable<ToDoWithSystemPropertiesType> table = GetClient().GetTable<ToDoWithSystemPropertiesType>();

            ToDoWithSystemPropertiesType item = new ToDoWithSystemPropertiesType() { Id = id, String = "a value" };
            await table.InsertAsync(item);

            Assert.IsNotNull(item.CreatedAt);
            Assert.IsNotNull(item.UpdatedAt);
            Assert.IsNotNull(item.Version);

            string version = item.Version;

            // Update
            item.String = "Hello!";
            await table.UpdateAsync(item);
            Assert.IsNotNull(item.Version);
            Assert.AreNotEqual(item.Version, version);

            string newVersion = item.Version;

            // Update again but with the original version
            item.Version = version;
            item.String = "But wait!";
            MobileServicePreconditionFailedException<ToDoWithSystemPropertiesType> expectedException = null;
            try
            {
                 await table.UpdateAsync(item);
            }
            catch (MobileServicePreconditionFailedException<ToDoWithSystemPropertiesType> exception)
            {
                expectedException = exception;
            }

            Assert.IsNotNull(expectedException);
            Assert.AreEqual(expectedException.Response.StatusCode, HttpStatusCode.PreconditionFailed);

            string responseContent = await expectedException.Response.Content.ReadAsStringAsync();
            JToken jtoken = responseContent.ParseToJToken();
            string serverVersion = (string)jtoken["__version"];
            string stringValue = (string)jtoken["String"];

            Assert.AreEqual(newVersion, serverVersion);
            Assert.AreEqual(stringValue, "Hello!");

            Assert.IsNotNull(expectedException.Item);
            Assert.AreEqual(newVersion, expectedException.Item.Version);
            Assert.AreEqual(stringValue, expectedException.Item.String);

            // Update one last time with the version from the server
            item.Version = serverVersion;
            await table.UpdateAsync(item);
            Assert.IsNotNull(item.Version);
            Assert.AreEqual(item.String, "But wait!");
            Assert.AreNotEqual(item.Version, serverVersion);


            await table.DeleteAsync(item);
        }
    }
}
