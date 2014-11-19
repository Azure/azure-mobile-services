// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Query;
using Microsoft.WindowsAzure.MobileServices.Test;
using Microsoft.WindowsAzure.MobileServices.TestFramework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
            TestUtilities.DropTestTable(TestDbName, TestTable);

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
            var ex = Throws<InvalidOperationException>(() => store.DefineTable(TestTable, new JObject()));
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
        public async Task UpsertAsync_ThenReadAsync_AllTypes()
        {
            TestUtilities.DropTestTable(TestDbName, TestTable);

            // first create a table called todo
            using (MobileServiceSQLiteStore store = new MobileServiceSQLiteStore(TestDbName))
            {
                store.DefineTable(TestTable, JObjectTypes.GetObjectWithAllTypes());

                await store.InitializeAsync();

                var upserted = new JObject()
                {
                    { "id", "xyz" },
                    { "Object", new JObject() { {"id", "abc"} }},
                    { "Array", new JArray() { new JObject(){{"id", 3}} } },
                    { "Integer", 123L },
                    { "Float", 12.5m },
                    { "String", "def" },
                    { "Boolean", true },
                    { "Date", new DateTime(2003, 5, 6, 4, 5, 1, DateTimeKind.Utc) },
                    { "Bytes", new byte[] { 1, 2, 3} },
                    { "Guid", new Guid("AB3EB1AB-53CD-4780-928B-A7E1CB7A927C") },
                    { "TimeSpan", new TimeSpan(1234) }
                };
                await store.UpsertAsync(TestTable, new[] { upserted }, false);

                var query = new MobileServiceTableQueryDescription(TestTable);
                var items = await store.ReadAsync(query) as JArray;
                Assert.IsNotNull(items);
                Assert.AreEqual(items.Count, 1);

                var lookedup = items.First as JObject;
                Assert.AreEqual(upserted.ToString(Formatting.None), lookedup.ToString(Formatting.None));
            }
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
            TestStoreThrowOnUninitialized(store => store.DeleteAsync("abc", new[] { "" }));
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
                await store.DeleteAsync(TestTable, new[] { "abc" });
            }

            // rows should be zero now
            count = TestUtilities.CountRows(TestDbName, TestTable);
            Assert.AreEqual(count, 0L);
        }

        [TestMethod]
        public void UpsertAsync_Throws_WhenStoreIsNotInitialized()
        {
            TestStoreThrowOnUninitialized(store => store.UpsertAsync("asdf", new[] { new JObject() }, ignoreMissingColumns: false));
        }

        [AsyncTestMethod]
        public async Task UpsertAsync_Throws_WhenColumnInItemIsNotDefinedAndItIsLocal()
        {
            TestUtilities.DropTestTable(TestDbName, TestTable);

            using (var store = new MobileServiceSQLiteStore(TestDbName))
            {
                store.DefineTable(TestTable, new JObject()
                {
                    { "id", String.Empty },
                    { "dob", DateTime.UtcNow }
                });

                await store.InitializeAsync();

                var ex = await ThrowsAsync<InvalidOperationException>(() => store.UpsertAsync(TestTable, new[] { new JObject() { { "notDefined", "okok" } } }, ignoreMissingColumns: false));

                Assert.AreEqual(ex.Message, "Column with name 'notDefined' is not defined on the local table 'todo'.");
            }
        }

        [AsyncTestMethod]
        public async Task UpsertAsync_DoesNotThrow_WhenColumnInItemIsNotDefinedAndItIsFromServer()
        {
            TestUtilities.DropTestTable(TestDbName, TestTable);

            using (var store = new MobileServiceSQLiteStore(TestDbName))
            {
                store.DefineTable(TestTable, new JObject()
                {
                    { "id", String.Empty },
                    { "dob", DateTime.UtcNow }
                });

                await store.InitializeAsync();

                await store.UpsertAsync(TestTable, new[] { new JObject() 
                { 
                    { "id", "abc" },
                    { "notDefined", "okok" }, 
                    { "dob", DateTime.UtcNow } 
                } }, ignoreMissingColumns: true);
            }
        }

        [AsyncTestMethod]
        public async Task UpsertAsync_DoesNotThrow_WhenItemIsEmpty()
        {
            TestUtilities.DropTestTable(TestDbName, TestTable);

            using (var store = new MobileServiceSQLiteStore(TestDbName))
            {
                store.DefineTable(TestTable, new JObject()
                {
                    { "id", String.Empty },
                    { "dob", DateTime.UtcNow }
                });

                await store.InitializeAsync();

                await store.UpsertAsync(TestTable, new[] { new JObject() }, ignoreMissingColumns: true);
                await store.UpsertAsync(TestTable, new[] { new JObject() }, ignoreMissingColumns: false);
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
                await store.UpsertAsync(TestTable, new[] { inserted }, ignoreMissingColumns: false);

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
                }}, ignoreMissingColumns: false);
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
                    { "text", "xyz" },
                    { "__createdAt", DateTime.Now } 
                }}, ignoreMissingColumns: false);

                await store.UpsertAsync(TestTable, new[]{new JObject() 
                { 
                    { "id", "abc" }, 
                    { "__createdAt", new DateTime(200,1,1) } 
                }}, ignoreMissingColumns: false);

                JObject result = await store.LookupAsync(TestTable, "abc");

                Assert.AreEqual(result.Value<string>("id"), "abc");
                Assert.AreEqual(result.Value<string>("text"), "xyz");
                Assert.AreEqual(result.Value<string>("__createdAt"), "01/01/0200 00:00:00");
            }
            long count = TestUtilities.CountRows(TestDbName, TestTable);
            Assert.AreEqual(count, 1L);
        }

        [AsyncTestMethod]
        public async Task UpsertAsync_Throws_WhenInsertingRecordsWhichAreTooLarge()
        {
            TestUtilities.DropTestTable(TestDbName, TestTable);

            using (var store = new MobileServiceSQLiteStore(TestDbName))
            {
                var template = new JObject
                {
                    { "id", 0 },
                };

                //SQLite limits us to 999 "parameters" per prepared statement
                for (var i = 0; i < 1000; i++)
                {
                    template["column" + i] = "Hello, world";
                }

                store.DefineTable(TestTable, template);

                //create the table
                await store.InitializeAsync();

                //attempt to insert a couple of items
                var item1 = new JObject(template);
                item1["id"] = 1;

                var item2 = new JObject(template);
                item1["id"] = 2;

                InvalidOperationException ex = await AssertEx.Throws<InvalidOperationException>(() => store.UpsertAsync(TestTable, new[] { item1, item2 }, ignoreMissingColumns: false));

                Assert.AreEqual("The number of fields per entity in an upsert operation is limited to 800.", ex.Message);
            }
        }

        [AsyncTestMethod]
        public async Task UpsertAsync_CanProcessManyRecordsAtOnce()
        {
            TestUtilities.DropTestTable(TestDbName, TestTable);

            using (var store = new MobileServiceSQLiteStore(TestDbName))
            {
                var template = new JObject 
                {
                    { "id", 0 },
                    { "value1", "Hello, world" },
                    { "value2", "Hello, world" },
                    { "value3", "Hello, world" },
                    { "value4", "Hello, world" },
                    { "value5", "Hello, world" }
                };

                store.DefineTable(TestTable, template);

                //create the table
                await store.InitializeAsync();

                //add a whole bunch of items. We want {number of items} * {number of fields} to exceed sqlite's parameter limit
                const int insertedItemCount = 500;

                var itemsToInsert = Enumerable.Range(1, insertedItemCount)
                                              .Select(id =>
                                              {
                                                  var o = new JObject(template);
                                                  o["id"] = id;
                                                  return o;
                                              })
                                              .ToArray();

                //Insert the items
                await store.UpsertAsync(TestTable, itemsToInsert, ignoreMissingColumns: false);

                JArray records = (JArray)await store.ReadAsync(MobileServiceTableQueryDescription.Parse(TestTable, "$orderby=id"));

                //Verify that all 500 records were inserted
                Assert.AreEqual(records.Count, insertedItemCount);

                //Verify that all fields are intact
                for (var i = 0; i < insertedItemCount; i++)
                {
                    Assert.IsTrue(JToken.DeepEquals(itemsToInsert[i], records[i]), "Results retrieved from DB do not match input");
                }
            }
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
                await store.UpsertAsync(TestTable, new[] { originalItem }, ignoreMissingColumns: false);

                // read the item back
                JObject itemRead = await store.LookupAsync(TestTable, "abc");

                // make sure everything was persisted the same
                Assert.AreEqual(originalItem.ToString(), itemRead.ToString());

                // change the item
                originalItem["double"] = 111.222d;

                // upsert the item
                await store.UpsertAsync(TestTable, new[] { originalItem }, ignoreMissingColumns: false);

                // read the updated item
                JObject updatedItem = await store.LookupAsync(TestTable, "abc");

                // make sure the float was updated
                Assert.AreEqual(updatedItem.Value<double>("double"), 111.222d);

                // make sure the item is same as updated item
                Assert.AreEqual(originalItem.ToString(), updatedItem.ToString());

                // make sure item is not same as its initial state
                Assert.AreNotEqual(originalItem.ToString(), itemRead.ToString());

                // now delete the item
                await store.DeleteAsync(TestTable, new[] { "abc" });

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
                { "id", String.Empty },
                { "text", String.Empty },
                { "__createdAt", DateTime.Now }
            });
        }
    }
}
