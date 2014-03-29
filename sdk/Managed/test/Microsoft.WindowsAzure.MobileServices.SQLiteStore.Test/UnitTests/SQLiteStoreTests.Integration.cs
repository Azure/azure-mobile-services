// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore.Test;
using Newtonsoft.Json.Linq;
using Microsoft.WindowsAzure.MobileServices.TestFramework;
using Microsoft.WindowsAzure.MobileServices.Test;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json;

namespace Microsoft.WindowsAzure.MobileServices.SQLiteStore.Test.UnitTests
{
    public class SQLiteStoreIntegrationTests: TestBase
    {
        private const string TestDbName = "test.db";
        private const string TestTable = "stringId_test_table";

        [AsyncTestMethod]
        public async Task ReadAsync_RoundTripsDate()
        {
            string tableName = "itemWithDate";

            ResetDatabase(tableName);

            var store = new MobileServiceSQLiteStore(TestDbName);
            store.DefineTable(tableName, new JObject()
            {
                { "id", String.Empty},
                { "date", DateTime.Now }
            });

            var hijack = new TestHttpHandler();
            IMobileServiceClient service = await CreateClient(hijack, store);
            IMobileServiceSyncTable table = service.GetSyncTable(tableName);

            DateTime theDate = new DateTime(2014, 3, 10, 0, 0, 0, DateTimeKind.Utc);
            JObject inserted = await table.InsertAsync(new JObject() { { "date",  theDate} });
            
            Assert.AreEqual(inserted["date"].Value<DateTime>(), theDate);

            JObject rehydrated = await table.LookupAsync(inserted["id"].Value<string>());

            Assert.AreEqual(rehydrated["date"].Value<DateTime>(), theDate);
        }

        [AsyncTestMethod]
        public async Task ReadAsync_RoundTripsDate_Generic()
        {
            string tableName = "NotSystemPropertyCreatedAtType";

            ResetDatabase(tableName);

            var store = new MobileServiceSQLiteStore(TestDbName);
            store.DefineTable<NotSystemPropertyCreatedAtType>();

            var hijack = new TestHttpHandler();
            IMobileServiceClient service = await CreateClient(hijack, store);
            IMobileServiceSyncTable<NotSystemPropertyCreatedAtType> table = service.GetSyncTable<NotSystemPropertyCreatedAtType>();

            DateTime theDate = new DateTime(2014, 3, 10, 0, 0, 0, DateTimeKind.Utc);
            var inserted = new NotSystemPropertyCreatedAtType() { CreatedAt = theDate };
            await table.InsertAsync(inserted);

            Assert.AreEqual(inserted.CreatedAt.ToUniversalTime(), theDate);

            NotSystemPropertyCreatedAtType rehydrated = await table.LookupAsync(inserted.Id);

            Assert.AreEqual(rehydrated.CreatedAt.ToUniversalTime(), theDate);
        }
        
        [AsyncTestMethod]
        public async Task ReadAsync_WithSystemPropertyType_Generic()
        {
            string tableName = "stringId_test_table";

            ResetDatabase(tableName);

            var store = new MobileServiceSQLiteStore(TestDbName);
            store.DefineTable<ToDoWithSystemPropertiesType>();

            var hijack = new TestHttpHandler();
            hijack.AddResponseContent(@"{""id"": ""123"", ""__version"": ""xyz""}");
            IMobileServiceClient service = await CreateClient(hijack, store);
            var table = service.GetSyncTable<ToDoWithSystemPropertiesType>();

            var inserted = new ToDoWithSystemPropertiesType() { Id ="123", Version = "abc", String = "def" };
            await table.InsertAsync(inserted);

            Assert.AreEqual(inserted.Version, "abc");

            await service.SyncContext.PushAsync();

            ToDoWithSystemPropertiesType rehydrated = await table.LookupAsync(inserted.Id);

            Assert.AreEqual(rehydrated.Version, "xyz");
        }


        [AsyncTestMethod]
        public async Task DefineTable_IgnoresColumn_IfCaseIsDifferentButNameIsSame()
        {
            string tableName = "itemWithDate";

            ResetDatabase(tableName);

            using (var store = new MobileServiceSQLiteStore(TestDbName))
            {
                store.DefineTable(tableName, new JObject()
                {
                    { "id", String.Empty},
                    { "date", DateTime.Now }
                });

                var hijack = new TestHttpHandler();
                await CreateClient(hijack, store);
            }

            using (var store = new MobileServiceSQLiteStore(TestDbName))
            {
                store.DefineTable(tableName, new JObject()
                {
                    { "id", String.Empty},
                    { "DaTE", DateTime.Now } // the casing of date is different here
                });

                var hijack = new TestHttpHandler();
                await CreateClient(hijack, store);
            }
        }

        [AsyncTestMethod]
        public async Task Upsert_Succeeds_IfCaseIsDifferentButNameIsSame()
        {
            string tableName = "itemWithDate";

            ResetDatabase(tableName);

            using (var store = new MobileServiceSQLiteStore(TestDbName))
            {
                store.DefineTable(tableName, new JObject()
                {
                    { "id", String.Empty},
                    { "date", DateTime.Now }
                });

                await store.InitializeAsync();

                await store.UpsertAsync("ITEMwithDATE", new JObject()
                {
                    { "ID", Guid.NewGuid() },
                    {"dATE", DateTime.UtcNow }
                });
            }
        }

        [AsyncTestMethod]
        public async Task PushAsync_SavesErrorInStore_WhenConflictOccurs()
        {
            ResetDatabase(TestTable);

            var hijack = new TestHttpHandler();
            string conflictResult = "{\"id\":\"b\",\"String\":\"Hey\",\"__version\":\"def\"}";
            hijack.Responses.Add(new HttpResponseMessage(HttpStatusCode.PreconditionFailed) { Content = new StringContent(conflictResult) });

            var store = new MobileServiceSQLiteStore(TestDbName);
            store.DefineTable<ToDoWithSystemPropertiesType>();

            IMobileServiceClient service = await CreateClient(hijack, store);
            IMobileServiceSyncTable<ToDoWithSystemPropertiesType> table = service.GetSyncTable<ToDoWithSystemPropertiesType>();

            // first insert an item
            var updatedItem = new ToDoWithSystemPropertiesType() { Id = "b", String = "Hey", Version = "abc" };
            await table.UpdateAsync(updatedItem);
            
            // then push it to server
            var ex = await ThrowsAsync<MobileServicePushFailedException>(service.SyncContext.PushAsync);

            Assert.IsNotNull(ex.PushResult);
            Assert.AreEqual(ex.PushResult.Status, MobileServicePushStatus.Complete);
            Assert.AreEqual(ex.PushResult.Errors.Count(), 1);
            MobileServiceTableOperationError error = ex.PushResult.Errors.FirstOrDefault();
            Assert.IsNotNull(error);
            Assert.AreEqual(error.Handled, false);
            Assert.AreEqual(error.OperationKind, MobileServiceTableOperationKind.Update);
            Assert.AreEqual(error.RawResult, conflictResult);
            Assert.AreEqual(error.TableName, TestTable);
            Assert.AreEqual(error.Status, HttpStatusCode.PreconditionFailed);

            var errorItem = error.Item.ToObject<ToDoWithSystemPropertiesType>(JsonSerializer.Create(service.SerializerSettings));
            Assert.AreEqual(errorItem.Id, updatedItem.Id);
            Assert.AreEqual(errorItem.String, updatedItem.String);
            Assert.AreEqual(errorItem.Version, updatedItem.Version);
            Assert.AreEqual(errorItem.CreatedAt, updatedItem.CreatedAt);
            Assert.AreEqual(errorItem.UpdatedAt, updatedItem.UpdatedAt);

            Assert.AreEqual(error.Result.ToString(Formatting.None), conflictResult);
        }

        [AsyncTestMethod]
        public async Task Insert_AllTypes_ThenRead_ThenPush_ThenLookup()
        {
            ResetDatabase("AllBaseTypesWithAllSystemPropertiesType");

            var hijack = new TestHttpHandler();
            var store = new MobileServiceSQLiteStore(TestDbName);
            store.DefineTable<AllBaseTypesWithAllSystemPropertiesType>();            

            IMobileServiceClient service = await CreateClient(hijack, store);
            IMobileServiceSyncTable<AllBaseTypesWithAllSystemPropertiesType> table = service.GetSyncTable<AllBaseTypesWithAllSystemPropertiesType>();

            // first insert an item
            var inserted = new AllBaseTypesWithAllSystemPropertiesType()
            {
                Id = "abc",
                Bool = true,
                Byte  = 11,
                SByte = -11,
                UShort = 22,
                Short = -22,
                UInt = 33,
                Int = -33,
                ULong = 44,
                Long = -44,
                Float = 55.66f,
                Double = 66.77,
                Decimal = 77.88M,
                String = "EightyEight",
                Char = '9',
                DateTime = new DateTime(2010,10,10, 10, 10, 10, DateTimeKind.Utc),
                DateTimeOffset = new DateTimeOffset(2011,11,11,11,11,11, 11, TimeSpan.Zero),
                Nullable = 12.13,
                NullableDateTime = new DateTime(2014,12,14, 14, 14, 14, DateTimeKind.Utc),
                Uri = new Uri("http://example.com"),
                Enum1 = Enum1.Enum1Value2,
                Enum2 = Enum2.Enum2Value2,
                Enum3 = Enum3.Enum3Value2,
                Enum4 = Enum4.Enum4Value2,
                Enum5 = Enum5.Enum5Value2,
                Enum6 = Enum6.Enum6Value2                
            };

            await table.InsertAsync(inserted);

            IList<AllBaseTypesWithAllSystemPropertiesType> records = await table.ToListAsync();
            Assert.AreEqual(records.Count, 1);

            Assert.AreEqual(records.First(), inserted);

            // now push
            hijack.AddResponseContent(@"
{""id"":""abc"",
""bool"":true,
""byte"":11,
""sByte"":-11,
""uShort"":22,
""short"":-22,
""uInt"":33,
""int"":-33,
""uLong"":44,
""long"":-44,
""float"":55.66,
""double"":66.77,
""decimal"":77.88,
""string"":""EightyEight"",
""char"":""9"",
""dateTime"":""2010-10-10T10:10:10.000Z"",
""dateTimeOffset"":""2011-11-11T11:11:11.011Z"",
""nullableDateTime"":""2010-10-10T10:10:10.000Z"",
""nullable"":12.13,
""uri"":""http://example.com/"",
""enum1"":""Enum1Value2"",
""enum2"":""Enum2Value2"",
""enum3"":""Enum3Value2"",
""enum4"":""Enum4Value2"",
""enum5"":""Enum5Value2"",
""enum6"":""Enum6Value2"",
""__version"":""XYZ""}");
            await service.SyncContext.PushAsync();
            AllBaseTypesWithAllSystemPropertiesType lookedUp = await table.LookupAsync("abc");
            inserted.Version = "XYZ";
            Assert.AreEqual(inserted, lookedUp);
        }

        [AsyncTestMethod]
        public async Task Insert_ThenPush_ThenPull_ThenRead_ThenUpdate_ThenRefresh_ThenDelete_ThenLookup_ThenPurge_ThenRead()
        {
            ResetDatabase(TestTable);

            var hijack = new TestHttpHandler();
            hijack.AddResponseContent("{\"id\":\"b\",\"String\":\"Hey\"}"); // insert response
            hijack.AddResponseContent("[{\"id\":\"b\",\"String\":\"Hey\"},{\"id\":\"a\",\"String\":\"World\"}]"); // pull response            

            IMobileServiceClient service = await CreateTodoClient(hijack);
            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();

            // first insert an item
            await table.InsertAsync(new ToDoWithStringId() { Id = "b", String = "Hey" }); 
            
            // then push it to server
            await service.SyncContext.PushAsync(); 

            // then pull changes from server
            await table.PullAsync();

            // order the records by id so we can assert them predictably 
            IList<ToDoWithStringId> items = await table.OrderBy(i => i.Id).ToListAsync();

            // we should have 2 records 
            Assert.AreEqual(items.Count, 2);

            // according to ordering a id comes first
            Assert.AreEqual(items[0].Id, "a");
            Assert.AreEqual(items[0].String, "World");

            // then comes b record
            Assert.AreEqual(items[1].Id, "b");
            Assert.AreEqual(items[1].String, "Hey");

            // we made 2 requests, one for push and one for pull
            Assert.AreEqual(hijack.Requests.Count, 2);

            // recreating the client from state in the store
            service = await CreateTodoClient(hijack);
            table = service.GetSyncTable<ToDoWithStringId>();

            // update the second record
            items[1].String = "Hello";
            await table.UpdateAsync(items[1]);

            // create an empty record with same id as modified record
            var second = new ToDoWithStringId() { Id = items[1].Id };
            // refresh the empty record
            await table.RefreshAsync(second);

            // make sure it is same as modified record now
            Assert.AreEqual(second.String, items[1].String);

            // now delete the record
            await table.DeleteAsync(second);

            // now try to get the deleted record
            ToDoWithStringId deleted = await table.LookupAsync(second.Id);

            // this should be null
            Assert.IsNull(deleted);

            // try to get the non-deleted record
            ToDoWithStringId first = await table.LookupAsync(items[0].Id);

            // this should still be there;
            Assert.IsNotNull(first);

            // make sure it is same as 
            Assert.AreEqual(first.String, items[0].String);

            // recreating the client from state in the store
            service = await CreateTodoClient(hijack);
            table = service.GetSyncTable<ToDoWithStringId>();

            // now purge the remaining records
            await table.PurgeAsync();

            // now read one last time
            IEnumerable<ToDoWithStringId> remaining = await table.ReadAsync();

            // There shouldn't be anything remaining
            Assert.AreEqual(remaining.Count(), 0);
        }

        private static async Task<IMobileServiceClient> CreateTodoClient(TestHttpHandler hijack)
        {
            var store = new MobileServiceSQLiteStore(TestDbName);
            store.DefineTable<ToDoWithStringId>();
            return await CreateClient(hijack, store);
        }

        private static async Task<IMobileServiceClient> CreateClient(TestHttpHandler hijack, MobileServiceSQLiteStore store)
        {
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());
            return service;
        }

        private static void ResetDatabase(string testTableName)
        {
            TestUtilities.DropTestTable(TestDbName, testTableName);
            TestUtilities.DropTestTable(TestDbName, MobileServiceLocalSystemTables.OperationQueue);
            TestUtilities.DropTestTable(TestDbName, MobileServiceLocalSystemTables.SyncErrors);
        }
    }
}
