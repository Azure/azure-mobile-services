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
   public class SQLiteStoreTests: TestBase
    {
       private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

       private const string TestDbName = "test.db";
       private const string TestTable = "todo";
       private static readonly DateTime testDate = DateTime.Parse("2014-02-11 14:52:19").ToUniversalTime();

        [AsyncTestMethod]
        public async Task InitializeAsync_InitializesTheStore()
        {
            MobileServiceSQLiteStore store = new MobileServiceSQLiteStore(TestDbName);
            store.DefineTable(TestTable, new JObject()
            {
                {"id", String.Empty },
                {"__createdAt", DateTime.UtcNow}
            });
            await store.InitializeAsync();
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

            using (MobileServiceSQLiteStore store = new MobileServiceSQLiteStore(TestDbName))
            {
                DefineTestTable(store);
                await store.InitializeAsync();

                JObject item = await store.LookupAsync(TestTable, "abc");
                Assert.IsNotNull(item);
                Assert.AreEqual(item.Value<string>("id"), "abc");
                Assert.AreEqual(item.Value<DateTime>("__createdAt"), testDate);
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

            using (MobileServiceSQLiteStore store = new MobileServiceSQLiteStore(TestDbName))
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

        [AsyncTestMethod]
        public async Task DeleteAsync_DeletesTheRow_WhenTheyMatchTheQuery()
        {
            await PrepareTodoTable();

            // insert rows and make sure they are inserted
            TestUtilities.ExecuteNonQuery(TestDbName, "INSERT INTO todo (id, __createdAt) VALUES ('abc', 1), ('def', 2), ('ghi', 3)");
            long count = TestUtilities.CountRows(TestDbName, TestTable);
            Assert.AreEqual(count, 3L);

            // delete the row
            using (MobileServiceSQLiteStore store = new MobileServiceSQLiteStore(TestDbName))
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
            using (MobileServiceSQLiteStore store = new MobileServiceSQLiteStore(TestDbName))
            {
                DefineTestTable(store);
                await store.InitializeAsync();
                await store.DeleteAsync(TestTable, "abc");
            }

            // rows should be zero now
            count = TestUtilities.CountRows(TestDbName, TestTable);
            Assert.AreEqual(count, 0L);
        }

        [AsyncTestMethod]
        public async Task UpsertAsync_InsertsTheRow_WhenItemHasNullValues()
        {
            TestUtilities.DropTestTable(TestDbName, TestTable);

            // insert a row and make sure it is inserted
            using (MobileServiceSQLiteStore store = new MobileServiceSQLiteStore(TestDbName))
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
                await store.UpsertAsync(TestTable, inserted);

                JObject read = await store.LookupAsync(TestTable, "abc");

                Assert.AreEqual(inserted.ToString(), read.ToString());
            }
        }

        [AsyncTestMethod]
        public async Task UpsertAsync_InsertsTheRow_WhenItDoesNotExist()
        {
            await PrepareTodoTable();            

            // insert a row and make sure it is inserted
            using (MobileServiceSQLiteStore store = new MobileServiceSQLiteStore(TestDbName))
            {
                DefineTestTable(store);
                await store.UpsertAsync(TestTable, new JObject() 
                { 
                    { "id", "abc" }, 
                    { "__createdAt", DateTime.Now } 
                });
            }
            long count = TestUtilities.CountRows(TestDbName, TestTable);
            Assert.AreEqual(count, 1L);
        }

        [AsyncTestMethod]
        public async Task UpsertAsync_UpdatesTheRow_WhenItExists()
        {
            await PrepareTodoTable();

            // insert a row and make sure it is inserted
            using (MobileServiceSQLiteStore store = new MobileServiceSQLiteStore(TestDbName))
            {
                DefineTestTable(store);

                await store.UpsertAsync(TestTable, new JObject() 
                { 
                    { "id", "abc" }, 
                    { "__createdAt", DateTime.Now } 
                });

                await store.UpsertAsync(TestTable, new JObject() 
                { 
                    { "id", "abc" }, 
                    { "__createdAt", new DateTime(200,1,1) } 
                });
            }
            long count = TestUtilities.CountRows(TestDbName, TestTable);
            Assert.AreEqual(count, 1L);
        }

       [AsyncTestMethod]
       public async Task Upsert_ThenLookup_ThenUpsert_ThenDelete_ThenLookup()
       {
           TestUtilities.DropTestTable(TestDbName, TestTable);

           using (MobileServiceSQLiteStore store = new MobileServiceSQLiteStore(TestDbName))
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
               await store.UpsertAsync(TestTable, originalItem);

               // read the item back
               JObject itemRead = await store.LookupAsync(TestTable, "abc");

               // make sure everything was persisted the same
               Assert.AreEqual(originalItem.ToString(), itemRead.ToString());

               // change the item
               originalItem["double"] = 111.222d;

               // upsert the item
               await store.UpsertAsync(TestTable, originalItem);

               // read the updated item
               JObject updatedItem = await store.LookupAsync(TestTable, "abc");

               // make sure the float was updated
               Assert.AreEqual(updatedItem.Value<double>("double"), 111.222d);

               // make sure the item is same as updated item
               Assert.AreEqual(originalItem.ToString(), updatedItem.ToString());

               // make sure item is not same as its initial state
               Assert.AreNotEqual(originalItem.ToString(), itemRead.ToString());

               // now delete the item
               await store.DeleteAsync(TestTable, "abc");

               // now read it back
               JObject item4 = await store.LookupAsync(TestTable, "abc");

               // it should be null because it doesn't exist
               Assert.IsNull(item4);
           }
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
