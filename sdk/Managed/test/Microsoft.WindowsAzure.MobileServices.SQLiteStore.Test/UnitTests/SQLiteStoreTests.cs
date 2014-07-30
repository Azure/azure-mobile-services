// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Query;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.TestFramework;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using SQLitePCL;

namespace Microsoft.WindowsAzure.MobileServices.SQLiteStore.Test.UnitTests
{
    public class SQLiteStoreTests : TestBase
    {
        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private const string TestTable = "todo";
        private static readonly DateTime testDate = DateTime.Parse("2014-02-11 14:52:19").ToUniversalTime();

        public static string TestDbName = "test.db";

        [AsyncTestMethod]
        public async Task InitializeAsync_InitializesTheStore()
        {
            var store = new MobileServiceSQLiteStore(TestDbName);
            store.DefineTable(TestTable, new JObject()
            {
                {"id", String.Empty },
                {"__createdAt", DateTime.UtcNow}
            });
            await store.InitializeAsync();
        }

        [AsyncTestMethod]
        public async Task InitializeAsync_Throws_WhenStoreIsAlreadyInitialized()
        {
            var store = new MobileServiceSQLiteStore(TestDbName);
            await store.InitializeAsync();

            var ex = await ThrowsAsync<InvalidOperationException>(() => store.InitializeAsync());

            Assert.AreEqual(ex.Message, "The store is already initialized.");
        }

        [AsyncTestMethod]
        public async Task DefineTable_Throws_WhenStoreIsInitialized()
        {
            var store = new MobileServiceSQLiteStore(TestDbName);
            await store.InitializeAsync();
            var ex = Throws<InvalidOperationException>(()=> store.DefineTable(TestTable, new JObject()));
            Assert.AreEqual(ex.Message, "Cannot define a table after the store has been initialized.");
        }

        [TestMethod]
        public void LookupAsync_Throws_WhenStoreIsNotInitialized()
        {
            TestStoreThrowOnUninitialized(store => store.LookupAsync("asdf", "asdf"));
        }

        [AsyncTestMethod]
        public async Task LookupAsync_ReadsItem()
        {
            await PrepareTodoTable();

            long date = (long)(testDate - epoch).TotalSeconds;

            // insert a row and make sure it is inserted
            TestUtilities.ExecuteNonQuery(TestDbName, "INSERT INTO todo (id, __createdAt) VALUES ('abc', " + date + ")");
            long count = TestUtilities.CountRows(TestDbName, TestTable);
            Assert.AreEqual(count, 1L);

            using (var store = new MobileServiceSQLiteStore(TestDbName))
            {
                DefineTestTable(store);
                await store.InitializeAsync();

                JObject item = await store.LookupAsync(TestTable, "abc");
                Assert.IsNotNull(item);
                Assert.AreEqual(item.Value<string>("id"), "abc");
                Assert.AreEqual(item.Value<DateTime>("__createdAt"), testDate);
            }
        }

        [TestMethod]
        public void ReadAsync_Throws_WhenStoreIsNotInitialized()
        {
            TestStoreThrowOnUninitialized(store => store.ReadAsync(MobileServiceTableQueryDescription.Parse("abc", "")));
        }

        [AsyncTestMethod]
        public async Task ReadAsync_ReadsItems()
        {
            await PrepareTodoTable();

            // insert rows and make sure they are inserted
            TestUtilities.ExecuteNonQuery(TestDbName, "INSERT INTO todo (id, __createdAt) VALUES ('abc', 1), ('def', 2), ('ghi', 3)");
            long count = TestUtilities.CountRows(TestDbName, TestTable);
            Assert.AreEqual(count, 3L);

            using (var store = new MobileServiceSQLiteStore(TestDbName))
            {
                DefineTestTable(store);
                await store.InitializeAsync();

                var query = MobileServiceTableQueryDescription.Parse(TestTable, "$filter=__createdAt gt 1&$inlinecount=allpages");
                JToken item = await store.ReadAsync(query);
                Assert.IsNotNull(item);
                var results = item["results"].Value<JArray>();
                long resultCount = item["count"].Value<long>();

                Assert.AreEqual(results.Count, 2);
                Assert.AreEqual(resultCount, 2L);
            }
        }

        [TestMethod]
        public void DeleteAsyncByQuery_Throws_WhenStoreIsNotInitialized()
        {
            TestStoreThrowOnUninitialized(store => store.DeleteAsync(MobileServiceTableQueryDescription.Parse("abc", "")));
        }

        [TestMethod]
        public void DeleteAsyncById_Throws_WhenStoreIsNotInitialized()
        {
            TestStoreThrowOnUninitialized(store => store.DeleteAsync("abc", new[]{""}));
        }

        [AsyncTestMethod]
        public async Task DeleteAsync_DeletesTheRow_WhenTheyMatchTheQuery()
        {
            await PrepareTodoTable();

            // insert rows and make sure they are inserted
            TestUtilities.ExecuteNonQuery(TestDbName, "INSERT INTO todo (id, __createdAt) VALUES ('abc', 1), ('def', 2), ('ghi', 3)");
            long count = TestUtilities.CountRows(TestDbName, TestTable);
            Assert.AreEqual(count, 3L);

            // delete the row
            using (var store = new MobileServiceSQLiteStore(TestDbName))
            {
                DefineTestTable(store);
                await store.InitializeAsync();
                var query = MobileServiceTableQueryDescription.Parse(TestTable, "$filter=__createdAt gt 1");
                await store.DeleteAsync(query);
            }

            // 1 row should be left
            count = TestUtilities.CountRows(TestDbName, TestTable);
            Assert.AreEqual(count, 1L);
        }

        [AsyncTestMethod]
        public async Task DeleteAsync_DeletesTheRow()
        {
            await PrepareTodoTable();

            // insert a row and make sure it is inserted
            TestUtilities.ExecuteNonQuery(TestDbName, "INSERT INTO todo (id, __createdAt) VALUES ('abc', 123)");
            long count = TestUtilities.CountRows(TestDbName, TestTable);
            Assert.AreEqual(count, 1L);

            // delete the row
            using (var store = new MobileServiceSQLiteStore(TestDbName))
            {
                DefineTestTable(store);
                await store.InitializeAsync();
                await store.DeleteAsync(TestTable, new[]{"abc"});
            }

            // rows should be zero now
            count = TestUtilities.CountRows(TestDbName, TestTable);
            Assert.AreEqual(count, 0L);
        }

        [TestMethod]
        public void UpsertAsync_Throws_WhenStoreIsNotInitialized()
        {
            TestStoreThrowOnUninitialized(store => store.UpsertAsync("asdf", new[]{new JObject()}, fromServer: false));
        }

        [AsyncTestMethod]
        public async Task UpsertAsync_Throws_WhenColumnInItemIsNotDefinedAndItIsLocal()
        {
            using (var store = new MobileServiceSQLiteStore(TestDbName))
            {
                store.DefineTable(TestTable, new JObject()
                {
                    { "id", String.Empty },
                    { "dob", DateTime.UtcNow }
                });

                await store.InitializeAsync();

                var ex = await ThrowsAsync<InvalidOperationException>(() => store.UpsertAsync(TestTable, new[]{new JObject() { { "notDefined", "okok" } }}, fromServer: false));

                Assert.AreEqual(ex.Message, "Column with name 'notDefined' is not defined on the local table 'todo'.");
            }
        }

        [AsyncTestMethod]
        public async Task UpsertAsync_DoesNotThrow_WhenColumnInItemIsNotDefinedAndItIsFromServer()
        {
            using (var store = new MobileServiceSQLiteStore(TestDbName))
            {
                store.DefineTable(TestTable, new JObject()
                {
                    { "id", String.Empty },
                    { "dob", DateTime.UtcNow }
                });

                await store.InitializeAsync();

                await store.UpsertAsync(TestTable, new[]{new JObject() { { "notDefined", "okok" }, {"dob", DateTime.UtcNow} }}, fromServer: true);
            }
        }

        [AsyncTestMethod]
        public async Task UpsertAsync_DoesNotThrow_WhenItemIsEmpty()
        {
            using (var store = new MobileServiceSQLiteStore(TestDbName))
            {
                store.DefineTable(TestTable, new JObject()
                {
                    { "id", String.Empty },
                    { "dob", DateTime.UtcNow }
                });

                await store.InitializeAsync();

                await store.UpsertAsync(TestTable, new[]{new JObject()}, fromServer: true);
                await store.UpsertAsync(TestTable, new[]{new JObject()}, fromServer: false);
            }
        }

        [AsyncTestMethod]
        public async Task UpsertAsync_InsertsTheRow_WhenItemHasNullValues()
        {
            TestUtilities.DropTestTable(TestDbName, TestTable);

            // insert a row and make sure it is inserted
            using (var store = new MobileServiceSQLiteStore(TestDbName))
            {
                store.DefineTable(TestTable, new JObject()
                {
                    { "id", String.Empty },
                    { "dob", DateTime.UtcNow },
                    { "age", 0},
                    { "weight", 3.5 },
                    { "code", Guid.NewGuid() },   
                    { "options", new JObject(){} },  
                    { "friends", new JArray(){} },  
                    { "__version", String.Empty }
                });

                await store.InitializeAsync();

                var inserted = new JObject() 
                { 
                    { "id", "abc" }, 
                    { "dob", null },
                    { "age", null },
                    { "weight", null },
                    { "code", null }, 
                    { "options", null },  
                    { "friends", null },  
                    { "__version", null }
                };
                await store.UpsertAsync(TestTable, new[]{inserted}, fromServer: false);

                JObject read = await store.LookupAsync(TestTable, "abc");

                Assert.AreEqual(inserted.ToString(), read.ToString());
            }
        }

        [AsyncTestMethod]
        public async Task UpsertAsync_InsertsTheRow_WhenItDoesNotExist()
        {
            await PrepareTodoTable();

            // insert a row and make sure it is inserted
            using (var store = new MobileServiceSQLiteStore(TestDbName))
            {
                DefineTestTable(store);
                await store.InitializeAsync();
                await store.UpsertAsync(TestTable, new[]{new JObject() 
                { 
                    { "id", "abc" }, 
                    { "__createdAt", DateTime.Now } 
                }}, fromServer: false);
            }
            long count = TestUtilities.CountRows(TestDbName, TestTable);
            Assert.AreEqual(count, 1L);
        }

        [AsyncTestMethod]
        public async Task UpsertAsync_UpdatesTheRow_WhenItExists()
        {
            await PrepareTodoTable();

            // insert a row and make sure it is inserted
            using (var store = new MobileServiceSQLiteStore(TestDbName))
            {
                DefineTestTable(store);
                await store.InitializeAsync();

                await store.UpsertAsync(TestTable, new[]{new JObject() 
                { 
                    { "id", "abc" }, 
                    { "__createdAt", DateTime.Now } 
                }}, fromServer: false);

                await store.UpsertAsync(TestTable, new[]{new JObject() 
                { 
                    { "id", "abc" }, 
                    { "__createdAt", new DateTime(200,1,1) } 
                }}, fromServer: false);
            }
            long count = TestUtilities.CountRows(TestDbName, TestTable);
            Assert.AreEqual(count, 1L);
        }

        [AsyncTestMethod]
        public async Task Upsert_ThenLookup_ThenUpsert_ThenDelete_ThenLookup()
        {
            TestUtilities.DropTestTable(TestDbName, TestTable);

            using (var store = new MobileServiceSQLiteStore(TestDbName))
            {
                // define item with all type of supported fields
                var originalItem = new JObject()
                {
                    { "id", "abc" },
                    { "bool", true },
                    { "int", 45 },
                    { "double", 123.45d },
                    { "guid", Guid.NewGuid() },
                    { "date", testDate },
                    { "options", new JObject(){ {"class", "A"} } },  
                    { "friends", new JArray(){ "Eric", "Jeff" } }
                };
                store.DefineTable(TestTable, originalItem);

                // create the table
                await store.InitializeAsync();

                // first add an item
                await store.UpsertAsync(TestTable, new[]{originalItem}, fromServer: false);

                // read the item back
                JObject itemRead = await store.LookupAsync(TestTable, "abc");

                // make sure everything was persisted the same
                Assert.AreEqual(originalItem.ToString(), itemRead.ToString());

                // change the item
                originalItem["double"] = 111.222d;

                // upsert the item
                await store.UpsertAsync(TestTable, new[]{originalItem}, fromServer: false);

                // read the updated item
                JObject updatedItem = await store.LookupAsync(TestTable, "abc");

                // make sure the float was updated
                Assert.AreEqual(updatedItem.Value<double>("double"), 111.222d);

                // make sure the item is same as updated item
                Assert.AreEqual(originalItem.ToString(), updatedItem.ToString());

                // make sure item is not same as its initial state
                Assert.AreNotEqual(originalItem.ToString(), itemRead.ToString());

                // now delete the item
                await store.DeleteAsync(TestTable, new[]{"abc"});

                // now read it back
                JObject item4 = await store.LookupAsync(TestTable, "abc");

                // it should be null because it doesn't exist
                Assert.IsNull(item4);
            }
        }

        private void TestStoreThrowOnUninitialized(Action<MobileServiceSQLiteStore> storeAction)
        {
            var store = new MobileServiceSQLiteStore(TestDbName);
            var ex = Throws<InvalidOperationException>(() => storeAction(store));
            Assert.AreEqual(ex.Message, "The store must be initialized before it can be used.");
        }

        private static async Task PrepareTodoTable()
        {
            TestUtilities.DropTestTable(TestDbName, TestTable);

            // first create a table called todo
            using (MobileServiceSQLiteStore store = new MobileServiceSQLiteStore(TestDbName))
            {
                DefineTestTable(store);

                await store.InitializeAsync();
            }
        }

        public static void DefineTestTable(MobileServiceSQLiteStore store)
        {
            store.DefineTable(TestTable, new JObject()
            {
                {"id", String.Empty },
                {"__createdAt", DateTime.Now}
            });
        }
    }
}
