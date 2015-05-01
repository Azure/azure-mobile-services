// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Microsoft.WindowsAzure.MobileServices.TestFramework;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    [Tag("unit")]
    [Tag("table")]
    [Tag("offline")]
    public class MobileServiceSyncTableGenericTests : TestBase
    {
        [AsyncTestMethod]
        public async Task RefreshAsync_Succeeds_WhenIdIsNull()
        {
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), new MobileServiceSyncHandler());

            IMobileServiceSyncTable<StringIdType> table = service.GetSyncTable<StringIdType>();

            var item = new StringIdType() { String = "what?" };
            await table.RefreshAsync(item);
        }

        [AsyncTestMethod]
        public async Task RefreshAsync_ThrowsInvalidOperationException_WhenIdItemDoesNotExist()
        {
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), new MobileServiceSyncHandler());

            IMobileServiceSyncTable<StringIdType> table = service.GetSyncTable<StringIdType>();

            var item = new StringIdType() { Id = "abc" };
            InvalidOperationException ex = await ThrowsAsync<InvalidOperationException>(() => table.RefreshAsync(item));
            Assert.AreEqual(ex.Message, "Item not found in local store.");
        }

        [AsyncTestMethod]
        public async Task RefreshAsync_UpdatesItem_WhenItExistsInStore()
        {
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), new MobileServiceSyncHandler());

            IMobileServiceSyncTable<StringIdType> table = service.GetSyncTable<StringIdType>();

            // add item to store
            var item = new StringIdType() { String = "what?" };
            await table.InsertAsync(item);

            Assert.IsNotNull(item.Id, "Id must be generated");

            // update it in store
            item.String = "nothing!";
            await table.UpdateAsync(item);

            // read it back into new object
            var refreshed = new StringIdType() { Id = item.Id };
            await table.RefreshAsync(refreshed);

            Assert.AreEqual(refreshed.String, "nothing!");
        }

        [AsyncTestMethod]
        public async Task PullAsync_Cancels_WhenCancellationTokenIsCancelled()
        {
            var store = new MobileServiceLocalStoreMock();

            var handler = new MobileServiceSyncHandlerMock();
            handler.TableOperationAction = op => Task.Delay(TimeSpan.MaxValue).ContinueWith<JObject>(t => null); // long slow operation

            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            await service.SyncContext.InitializeAsync(store, handler);

            IMobileServiceSyncTable table = service.GetSyncTable("someTable");

            using (var cts = new CancellationTokenSource())
            {
                // now pull
                Task pullTask = table.PullAsync(null, null, null, cancellationToken: cts.Token);
                cts.Cancel();

                var ex = await ThrowsAsync<Exception>(() => pullTask);

                Assert.IsTrue((ex is OperationCanceledException || ex is TaskCanceledException));
                Assert.AreEqual(pullTask.Status, TaskStatus.Canceled);
            }
        }

        [AsyncTestMethod]
        public async Task PullAsync_DoesNotPurge_WhenItemIsMissing()
        {
            var hijack = new TestHttpHandler();
            hijack.AddResponseContent("{\"id\":\"abc\",\"String\":\"Hey\"}"); // for insert
            hijack.AddResponseContent("[{\"id\":\"def\",\"String\":\"World\"}]"); // remote item
            hijack.AddResponseContent("[]"); // last page

            var store = new MobileServiceLocalStoreMock();

            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable table = service.GetSyncTable("someTable");
            await table.InsertAsync(new JObject() { { "id", "abc" } }); // insert an item
            await service.SyncContext.PushAsync(); // push to clear the queue 

            // now pull
            await table.PullAsync(null, null);

            Assert.AreEqual(store.TableMap[table.TableName].Count, 2); // 1 from remote and 1 from local
            Assert.AreEqual(hijack.Requests.Count, 3); // one for push and 2 for pull
        }

        [AsyncTestMethod]
        public async Task PullAsync_DoesNotTriggerPush_WhenThereIsNoOperationInTable()
        {
            var hijack = new TestHttpHandler();
            hijack.AddResponseContent("{\"id\":\"abc\",\"String\":\"Hey\"}"); // for insert
            hijack.AddResponseContent("[{\"id\":\"abc\",\"String\":\"Hey\"},{\"id\":\"def\",\"String\":\"World\"}]"); // for pull
            hijack.AddResponseContent("[]"); // last page

            var store = new MobileServiceLocalStoreMock();
            store.ReadResponses.Enqueue("[{\"id\":\"abc\",\"String\":\"Hey\"},{\"id\":\"def\",\"String\":\"World\"}]"); // for pull

            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            // insert item in pull table
            IMobileServiceSyncTable table1 = service.GetSyncTable("someTable");
            await table1.InsertAsync(new JObject() { { "id", "abc" } });

            // but push to clear the queue
            await service.SyncContext.PushAsync();
            Assert.AreEqual(store.TableMap[table1.TableName].Count, 1); // item is inserted
            Assert.AreEqual(hijack.Requests.Count, 1); // first push

            // then insert item in other table
            IMobileServiceSyncTable<StringIdType> table2 = service.GetSyncTable<StringIdType>();
            var item = new StringIdType() { Id = "an id", String = "what?" };
            await table2.InsertAsync(item);

            await table1.PullAsync(null, null);

            Assert.AreEqual(store.TableMap[table1.TableName].Count, 2); // table should contain 2 pulled items
            Assert.AreEqual(hijack.Requests.Count, 3); // 1 for push and 2 for pull
            Assert.AreEqual(store.TableMap[table2.TableName].Count, 1); // this table should not be touched
        }

        [AsyncTestMethod]
        public async Task PullAsync_TriggersPush_WhenThereIsOperationInTable()
        {
            var hijack = new TestHttpHandler();
            hijack.AddResponseContent("{\"id\":\"abc\",\"String\":\"Hey\"}"); // for insert
            hijack.AddResponseContent("[{\"id\":\"abc\",\"String\":\"Hey\"},{\"id\":\"def\",\"String\":\"World\"}]"); // for pull
            hijack.AddResponseContent("[]"); // last page

            var store = new MobileServiceLocalStoreMock();
            store.ReadResponses.Enqueue("[{\"id\":\"abc\",\"String\":\"Hey\"},{\"id\":\"def\",\"String\":\"World\"}]"); // for pull

            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            // insert an item but don't push
            IMobileServiceSyncTable table1 = service.GetSyncTable("someTable");
            await table1.InsertAsync(new JObject() { { "id", "abc" } });
            Assert.AreEqual(store.TableMap[table1.TableName].Count, 1); // item is inserted

            // this should trigger a push
            await table1.PullAsync(null, null);

            Assert.AreEqual(hijack.Requests.Count, 3); // 1 for push and 2 for pull
            Assert.AreEqual(store.TableMap[table1.TableName].Count, 2); // table is populated
        }

        [AsyncTestMethod]
        public async Task PullAsync_TriggersPush_FeatureHeaderInOperation()
        {
            var hijack = new TestHttpHandler();
            hijack.AddResponseContent("{\"id\":\"abc\",\"String\":\"Hey\"}"); // for insert
            hijack.AddResponseContent("[{\"id\":\"abc\",\"String\":\"Hey\"},{\"id\":\"def\",\"String\":\"World\"}]"); // for pull
            hijack.AddResponseContent("[]"); // last page

            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), new MobileServiceSyncHandler());

            // insert an item but don't push
            IMobileServiceSyncTable table = service.GetSyncTable("someTable");
            await table.InsertAsync(new JObject() { { "id", "abc" } });

            // this should trigger a push
            await table.PullAsync(null, null);

            Assert.AreEqual(hijack.Requests[0].Headers.GetValues("X-ZUMO-FEATURES").First(), "TU,OL");
            Assert.AreEqual(hijack.Requests[1].Headers.GetValues("X-ZUMO-FEATURES").First(), "QS,OL");
            Assert.AreEqual(hijack.Requests[2].Headers.GetValues("X-ZUMO-FEATURES").First(), "QS,OL");
        }

        [AsyncTestMethod]
        public async Task PullAsync_FollowsNextLinks()
        {
            var hijack = new TestHttpHandler();
            hijack.AddResponseContent("[{\"id\":\"abc\",\"String\":\"Hey\"},{\"id\":\"def\",\"String\":\"How\"}]"); // first page
            hijack.Responses[0].Headers.Add("Link", "http://localhost:31475/tables/Green?$top=1&$select=Text%2CDone%2CId&$skip=2; rel=next");
            hijack.AddResponseContent("[{\"id\":\"ghi\",\"String\":\"Are\"},{\"id\":\"jkl\",\"String\":\"You\"}]"); // second page
            hijack.AddResponseContent("[]"); // end of the list

            var store = new MobileServiceLocalStoreMock();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();

            Assert.IsFalse(store.TableMap.ContainsKey("stringId_test_table"));

            await table.PullAsync(null, null);

            Assert.AreEqual(store.TableMap["stringId_test_table"].Count, 4);
            Assert.AreEqual(store.TableMap["stringId_test_table"]["abc"].Value<string>("String"), "Hey");
            Assert.AreEqual(store.TableMap["stringId_test_table"]["def"].Value<string>("String"), "How");
            Assert.AreEqual(store.TableMap["stringId_test_table"]["ghi"].Value<string>("String"), "Are");
            Assert.AreEqual(store.TableMap["stringId_test_table"]["jkl"].Value<string>("String"), "You");

            AssertEx.MatchUris(hijack.Requests, "http://www.test.com/tables/stringId_test_table?$skip=0&$top=50&__includeDeleted=true&__systemproperties=__version%2C__deleted",
                                        "http://localhost:31475/tables/Green?$top=1&$select=Text%2CDone%2CId&$skip=2",
                                        "http://www.test.com/tables/stringId_test_table?$skip=4&$top=50&__includeDeleted=true&__systemproperties=__version%2C__deleted");
        }

        [AsyncTestMethod]
        public async Task PullAsync_UpdatesDeltaToken_AfterEachResult_IfOrderByIsSupported()
        {
            var hijack = new TestHttpHandler();
            hijack.AddResponseContent(@"[{""id"":""abc"",""String"":""Hey"", ""__updatedAt"": ""2001-02-03T00:00:00.0000000+00:00""},
                                         {""id"":""def"",""String"":""How"", ""__updatedAt"": ""2001-02-04T00:00:00.0000000+00:00""}]"); // first page
            hijack.Responses.Add(new HttpResponseMessage(HttpStatusCode.InternalServerError)); // throw on second page

            var store = new MobileServiceLocalStoreMock();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();

            Assert.IsFalse(store.TableMap.ContainsKey("stringId_test_table"));

            await ThrowsAsync<MobileServiceInvalidOperationException>(() => table.PullAsync("items", table.CreateQuery()));

            Assert.AreEqual(store.TableMap["stringId_test_table"].Count, 2);
            Assert.AreEqual(store.TableMap["stringId_test_table"]["abc"].Value<string>("String"), "Hey");
            Assert.AreEqual(store.TableMap["stringId_test_table"]["def"].Value<string>("String"), "How");

            AssertEx.MatchUris(hijack.Requests, "http://www.test.com/tables/stringId_test_table?$filter=(__updatedAt ge datetimeoffset'1970-01-01T00:00:00.0000000%2B00:00')&$orderby=__updatedAt&$skip=0&$top=50&__includeDeleted=true&__systemproperties=__updatedAt%2C__version%2C__deleted",
                                                "http://www.test.com/tables/stringId_test_table?$filter=(__updatedAt ge datetimeoffset'2001-02-04T00:00:00.0000000%2B00:00')&$orderby=__updatedAt&$skip=0&$top=50&__includeDeleted=true&__systemproperties=__updatedAt%2C__version%2C__deleted");

            Assert.Equals(store.TableMap[MobileServiceLocalSystemTables.Config]["deltaToken|stringId_test_table|items"]["value"], "2001-02-04T00:00:00.0000000+00:00");
        }

        [AsyncTestMethod]
        public async Task PullAsync_DoesNotUpdateDeltaToken_AfterEachResult_IfOrderByIsNotSupported()
        {
            var hijack = new TestHttpHandler();
            hijack.AddResponseContent(@"[{""id"":""abc"",""String"":""Hey"", ""__updatedAt"": ""2001-02-03T00:00:00.0000000+00:00""},
                                         {""id"":""def"",""String"":""How"", ""__updatedAt"": ""2001-02-04T00:00:00.0000000+00:00""}]"); // first page
            hijack.Responses.Add(new HttpResponseMessage(HttpStatusCode.InternalServerError)); // throw on second page

            var store = new MobileServiceLocalStoreMock();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();
            table.SupportedOptions &= ~MobileServiceRemoteTableOptions.OrderBy;

            Assert.IsFalse(store.TableMap.ContainsKey("stringId_test_table"));

            await ThrowsAsync<MobileServiceInvalidOperationException>(() => table.PullAsync("items", table.CreateQuery()));

            Assert.AreEqual(store.TableMap["stringId_test_table"].Count, 2);
            Assert.AreEqual(store.TableMap["stringId_test_table"]["abc"].Value<string>("String"), "Hey");
            Assert.AreEqual(store.TableMap["stringId_test_table"]["def"].Value<string>("String"), "How");

            AssertEx.MatchUris(hijack.Requests, "http://www.test.com/tables/stringId_test_table?$filter=(__updatedAt ge datetimeoffset'1970-01-01T00:00:00.0000000%2B00:00')&$skip=0&$top=50&__includeDeleted=true&__systemproperties=__updatedAt%2C__version%2C__deleted",
                                                "http://www.test.com/tables/stringId_test_table?$filter=(__updatedAt ge datetimeoffset'1970-01-01T00:00:00.0000000%2B00:00')&$skip=2&$top=50&__includeDeleted=true&__systemproperties=__updatedAt%2C__version%2C__deleted");

            Assert.IsFalse(store.TableMap[MobileServiceLocalSystemTables.Config].ContainsKey("deltaToken|stringId_test_table|items"));
        }

        [AsyncTestMethod]
        public async Task PullAsync_UsesSkipAndTakeThenFollowsLinkThenUsesSkipAndTake()
        {
            var hijack = new TestHttpHandler();
            // first page
            hijack.AddResponseContent("[{\"id\":\"abc\",\"String\":\"Hey\"},{\"id\":\"def\",\"String\":\"How\"}]");
            // second page with a link
            hijack.AddResponseContent("[{\"id\":\"ghi\",\"String\":\"Are\"},{\"id\":\"jkl\",\"String\":\"You\"}]");
            hijack.Responses[1].Headers.Add("Link", "http://localhost:31475/tables/Green?$top=1&$select=Text%2CDone%2CId&$skip=2; rel=next");
            // forth page without link
            hijack.AddResponseContent("[{\"id\":\"mno\",\"String\":\"Mr\"},{\"id\":\"pqr\",\"String\":\"X\"}]");
            // last page
            hijack.AddResponseContent("[]");

            var store = new MobileServiceLocalStoreMock();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();

            Assert.IsFalse(store.TableMap.ContainsKey("stringId_test_table"));

            await table.PullAsync(null, table.Take(51).Skip(3));

            Assert.AreEqual(store.TableMap["stringId_test_table"].Count, 6);
            Assert.AreEqual(store.TableMap["stringId_test_table"]["abc"].Value<string>("String"), "Hey");
            Assert.AreEqual(store.TableMap["stringId_test_table"]["def"].Value<string>("String"), "How");
            Assert.AreEqual(store.TableMap["stringId_test_table"]["ghi"].Value<string>("String"), "Are");
            Assert.AreEqual(store.TableMap["stringId_test_table"]["jkl"].Value<string>("String"), "You");
            Assert.AreEqual(store.TableMap["stringId_test_table"]["mno"].Value<string>("String"), "Mr");
            Assert.AreEqual(store.TableMap["stringId_test_table"]["pqr"].Value<string>("String"), "X");

            AssertEx.MatchUris(hijack.Requests, "http://www.test.com/tables/stringId_test_table?$skip=3&$top=50&__includeDeleted=true&__systemproperties=__version%2C__deleted",
                                        "http://www.test.com/tables/stringId_test_table?$skip=5&$top=49&__includeDeleted=true&__systemproperties=__version%2C__deleted",
                                        "http://localhost:31475/tables/Green?$top=1&$select=Text%2CDone%2CId&$skip=2",
                                        "http://www.test.com/tables/stringId_test_table?$skip=9&$top=45&__includeDeleted=true&__systemproperties=__version%2C__deleted");
        }

        [AsyncTestMethod]
        public async Task PullAsync_Supports_AbsoluteAndRelativeUri()
        {
            var data = new string[] 
            {
                "http://www.test.com/api/todoitem",
                "/api/todoitem"
            };

            foreach (string uri in data)
            {
                var hijack = new TestHttpHandler();
                hijack.AddResponseContent("[{\"id\":\"abc\",\"String\":\"Hey\"},{\"id\":\"def\",\"String\":\"How\"}]"); // first page
                hijack.AddResponseContent("[]");

                var store = new MobileServiceLocalStoreMock();
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
                await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

                IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();

                Assert.IsFalse(store.TableMap.ContainsKey("stringId_test_table"));

                await table.PullAsync(null, uri);

                Assert.AreEqual(store.TableMap["stringId_test_table"].Count, 2);

                AssertEx.MatchUris(hijack.Requests, "http://www.test.com/api/todoitem?$skip=0&$top=50&__includeDeleted=true&__systemproperties=__version%2C__deleted",
                                                    "http://www.test.com/api/todoitem?$skip=2&$top=50&__includeDeleted=true&__systemproperties=__version%2C__deleted");

                Assert.AreEqual("QS,OL", hijack.Requests[0].Headers.GetValues("X-ZUMO-FEATURES").First());
                Assert.AreEqual("QS,OL", hijack.Requests[1].Headers.GetValues("X-ZUMO-FEATURES").First());
            }
        }

        public async Task PullASync_Throws_IfAbsoluteUriHostNameDoesNotMatch()
        {
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", new TestHttpHandler());
            IMobileServiceSyncTable<ToDo> table = service.GetSyncTable<ToDo>();

            var ex = await AssertEx.Throws<ArgumentException>(async () => await table.PullAsync(null, "http://www.contoso.com/about?$filter=a eq b&$orderby=c"));

            Assert.AreEqual(ex.Message, "The query uri must be on the same host as the Mobile Service.");
        }

        [AsyncTestMethod]
        public async Task PullAsync_DoesNotFollowLink_IfRelationIsNotNext()
        {
            var hijack = new TestHttpHandler();
            hijack.AddResponseContent("[{\"id\":\"abc\",\"String\":\"Hey\"},{\"id\":\"def\",\"String\":\"How\"}]"); // first page
            hijack.Responses[0].Headers.Add("Link", "http://contoso.com:31475/tables/Green?$top=1&$select=Text%2CDone%2CId&$skip=2; rel=prev");
            hijack.AddResponseContent("[{\"id\":\"ghi\",\"String\":\"Are\"},{\"id\":\"jkl\",\"String\":\"You\"}]"); // second page
            hijack.AddResponseContent("[]"); // end of the list

            var store = new MobileServiceLocalStoreMock();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();

            Assert.IsFalse(store.TableMap.ContainsKey("stringId_test_table"));

            await table.PullAsync(null, null);

            Assert.AreEqual(store.TableMap["stringId_test_table"].Count, 4);

            AssertEx.MatchUris(hijack.Requests, "http://www.test.com/tables/stringId_test_table?$skip=0&$top=50&__includeDeleted=true&__systemproperties=__version%2C__deleted",
                                        "http://www.test.com/tables/stringId_test_table?$skip=2&$top=50&__includeDeleted=true&__systemproperties=__version%2C__deleted",
                                        "http://www.test.com/tables/stringId_test_table?$skip=4&$top=50&__includeDeleted=true&__systemproperties=__version%2C__deleted");
        }

        [AsyncTestMethod]
        public async Task PullAsync_DoesNotFollowLink_IfLinkHasNonSupportedOptions()
        {
            var hijack = new TestHttpHandler();
            hijack.AddResponseContent("[{\"id\":\"abc\",\"String\":\"Hey\"},{\"id\":\"def\",\"String\":\"How\"}]"); // first page
            hijack.Responses[0].Headers.Add("Link", "http://contoso.com:31475/tables/Green?$top=1&$select=Text%2CDone%2CId&$skip=2; rel=next");
            hijack.AddResponseContent("[{\"id\":\"ghi\",\"String\":\"Are\"},{\"id\":\"jkl\",\"String\":\"You\"}]"); // second page
            hijack.AddResponseContent("[]"); // end of the list

            var store = new MobileServiceLocalStoreMock();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();
            table.SupportedOptions &= ~MobileServiceRemoteTableOptions.Skip;

            Assert.IsFalse(store.TableMap.ContainsKey("stringId_test_table"));

            await table.PullAsync(null, null);

            Assert.AreEqual(store.TableMap["stringId_test_table"].Count, 2);

            AssertEx.MatchUris(hijack.Requests, "http://www.test.com/tables/stringId_test_table?$top=50&__includeDeleted=true&__systemproperties=__version%2C__deleted");
        }

        [AsyncTestMethod]
        public async Task PullAsync_UsesTopInQuery_IfLessThan50()
        {
            var hijack = new TestHttpHandler();
            hijack.AddResponseContent("[{\"id\":\"abc\",\"String\":\"Hey\"},{\"id\":\"def\",\"String\":\"How\"}]"); // first page
            hijack.AddResponseContent("[]"); // end of the list

            var store = new MobileServiceLocalStoreMock();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();

            Assert.IsFalse(store.TableMap.ContainsKey("stringId_test_table"));

            await table.PullAsync(null, table.Take(49));

            Assert.AreEqual(store.TableMap["stringId_test_table"].Count, 2);

            AssertEx.MatchUris(hijack.Requests, "http://www.test.com/tables/stringId_test_table?$skip=0&$top=49&__includeDeleted=true&__systemproperties=__version%2C__deleted",
                                        "http://www.test.com/tables/stringId_test_table?$skip=2&$top=47&__includeDeleted=true&__systemproperties=__version%2C__deleted");
        }

        [AsyncTestMethod]
        public async Task PullAsync_DefaultsTo50_IfGreaterThan50()
        {
            var hijack = new TestHttpHandler();
            hijack.AddResponseContent("[{\"id\":\"abc\",\"String\":\"Hey\"},{\"id\":\"def\",\"String\":\"How\"}]"); // first page
            hijack.AddResponseContent("[]"); // end of the list

            var store = new MobileServiceLocalStoreMock();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();

            Assert.IsFalse(store.TableMap.ContainsKey("stringId_test_table"));

            await table.PullAsync(null, table.Take(51));

            Assert.AreEqual(store.TableMap["stringId_test_table"].Count, 2);

            AssertEx.MatchUris(hijack.Requests, "http://www.test.com/tables/stringId_test_table?$skip=0&$top=50&__includeDeleted=true&__systemproperties=__version%2C__deleted",
                                        "http://www.test.com/tables/stringId_test_table?$skip=2&$top=49&__includeDeleted=true&__systemproperties=__version%2C__deleted");
        }

        [AsyncTestMethod]
        public async Task PullAsync_DoesNotFollowLink_IfMaxRecordsAreRetrieved()
        {
            var hijack = new TestHttpHandler();
            hijack.AddResponseContent("[{\"id\":\"abc\",\"String\":\"Hey\"},{\"id\":\"def\",\"String\":\"How\"}]"); // first page
            hijack.Responses[0].Headers.Add("Link", "http://localhost:31475/tables/Green?$top=1&$select=Text%2CDone%2CId&$skip=2; rel=next");
            hijack.AddResponseContent("[{\"id\":\"ghi\",\"String\":\"Are\"},{\"id\":\"jkl\",\"String\":\"You\"}]"); // second page

            var store = new MobileServiceLocalStoreMock();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();

            Assert.IsFalse(store.TableMap.ContainsKey("stringId_test_table"));

            await table.PullAsync(null, table.Take(1));

            Assert.AreEqual(store.TableMap["stringId_test_table"].Count, 1);
        }

        [AsyncTestMethod]
        public async Task PullAsync_DoesNotFollowLink_IfResultIsEmpty()
        {
            var hijack = new TestHttpHandler();
            hijack.AddResponseContent("[]"); // first page
            hijack.Responses[0].Headers.Add("Link", "http://localhost:31475/tables/Green?$top=1&$select=Text%2CDone%2CId&$skip=2; rel=next");
            hijack.AddResponseContent("[{\"id\":\"ghi\",\"String\":\"Are\"},{\"id\":\"jkl\",\"String\":\"You\"}]"); // second page

            var store = new MobileServiceLocalStoreMock();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();

            Assert.IsFalse(store.TableMap.ContainsKey("stringId_test_table"));

            await table.PullAsync(null, table.Take(1));

            Assert.IsFalse(store.TableMap.ContainsKey("stringId_test_table"));
        }

        [AsyncTestMethod]
        public async Task PullAsync_Throws_WhenPushThrows()
        {
            var hijack = new TestHttpHandler();
            hijack.Responses.Add(new HttpResponseMessage(HttpStatusCode.NotFound)); // for push

            var store = new MobileServiceLocalStoreMock();

            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            // insert an item but don't push
            IMobileServiceSyncTable table = service.GetSyncTable("someTable");
            await table.InsertAsync(new JObject() { { "id", "abc" } });
            Assert.AreEqual(store.TableMap[table.TableName].Count, 1); // item is inserted

            // this should trigger a push
            var ex = await ThrowsAsync<MobileServicePushFailedException>(() => table.PullAsync(null, null));

            Assert.AreEqual(ex.PushResult.Errors.Count(), 1);
            Assert.AreEqual(hijack.Requests.Count, 1); // 1 for push 
        }

        [AsyncTestMethod]
        public async Task PullAsync_Throws_WhenSelectClauseIsSpecified()
        {
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();
            var query = table.Select(x => x.String);

            var exception = await ThrowsAsync<ArgumentException>(() => table.PullAsync(null, query, cancellationToken: CancellationToken.None));
            Assert.AreEqual(exception.ParamName, "query");
            Assert.StartsWith(exception.Message, "Pull query with select clause is not supported.");
        }

        [AsyncTestMethod]
        public async Task PullAsync_Throws_WhenOrderByClauseIsSpecifiedWithQueryId()
        {
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();
            var query = table.OrderBy(x => x.String);

            var exception = await ThrowsAsync<ArgumentException>(() => table.PullAsync("incQuery", query, cancellationToken: CancellationToken.None));
            Assert.AreEqual(exception.ParamName, "query");
            Assert.StartsWith(exception.Message, "Incremental pull query must not have orderby clause.");
        }

        [AsyncTestMethod]
        public async Task PullAsync_Throws_WhenOrderByClauseIsSpecifiedAndOptionIsNotSet()
        {
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();
            table.SupportedOptions &= ~MobileServiceRemoteTableOptions.OrderBy;
            var query = table.OrderBy(x => x.String);

            var exception = await ThrowsAsync<ArgumentException>(() => table.PullAsync(null, query));
            Assert.AreEqual(exception.ParamName, "query");
            Assert.StartsWith(exception.Message, "The supported table options does not include orderby.");
        }

        [AsyncTestMethod]
        public async Task PullAsync_Throws_WhenTopClauseIsSpecifiedAndOptionIsNotSet()
        {
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();
            table.SupportedOptions &= ~MobileServiceRemoteTableOptions.Top;
            var query = table.Take(30);

            var exception = await ThrowsAsync<ArgumentException>(() => table.PullAsync(null, query));
            Assert.AreEqual(exception.ParamName, "query");
            Assert.StartsWith(exception.Message, "The supported table options does not include top.");
        }

        [AsyncTestMethod]
        public async Task PullAsync_Throws_WhenSkipClauseIsSpecifiedAndOptionIsNotSet()
        {
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();
            table.SupportedOptions &= ~MobileServiceRemoteTableOptions.Skip;
            var query = table.Skip(30);

            var exception = await ThrowsAsync<ArgumentException>(() => table.PullAsync(null, query));
            Assert.AreEqual(exception.ParamName, "query");
            Assert.StartsWith(exception.Message, "The supported table options does not include skip.");
        }

        [AsyncTestMethod]
        public async Task PullAsync_Throws_WhenTopOrSkipIsSpecifiedWithQueryId()
        {
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();
            string expectedError = "Incremental pull query must not have skip or top specified.";

            var query = table.Take(5);
            var exception = await ThrowsAsync<ArgumentException>(() => table.PullAsync("incQuery", query, cancellationToken: CancellationToken.None));
            Assert.AreEqual(exception.ParamName, "query");
            Assert.StartsWith(exception.Message, expectedError);

            query = table.Skip(5);
            exception = await ThrowsAsync<ArgumentException>(() => table.PullAsync("incQuery", query, cancellationToken: CancellationToken.None));
            Assert.AreEqual(exception.ParamName, "query");
            Assert.StartsWith(exception.Message, expectedError);
        }

        [AsyncTestMethod]
        public async Task PullAsync_Succeeds()
        {
            var hijack = new TestHttpHandler();
            hijack.OnSendingRequest = req =>
            {
                return Task.FromResult(req);
            };
            hijack.AddResponseContent("[{\"id\":\"abc\",\"String\":\"Hey\"},{\"id\":\"def\",\"String\":\"World\"}]"); // for pull
            hijack.AddResponseContent("[]"); // last page

            var store = new MobileServiceLocalStoreMock();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();
            var query = table.Skip(5)
                             .Take(3)
                             .Where(t => t.String == "world")
                             .OrderBy(o => o.Id)
                             .WithParameters(new Dictionary<string, string>() { { "param1", "val1" } })
                             .OrderByDescending(o => o.String)
                             .IncludeTotalCount();

            await table.PullAsync(null, query, cancellationToken: CancellationToken.None);
            Assert.AreEqual(hijack.Requests.Count, 2);
            AssertEx.QueryEquals(hijack.Requests[0].RequestUri.Query, "?$filter=(String%20eq%20'world')&$orderby=String%20desc,id&$skip=5&$top=3&param1=val1&__includeDeleted=true&__systemproperties=__version%2C__deleted");
        }

        [AsyncTestMethod]
        public async Task PullAsync_Incremental_AllOptions_MovesByUpdatedAt()
        {
            await TestPullAsyncIncrementalWithOptions(MobileServiceRemoteTableOptions.All,
                    "http://test.com/tables/stringId_test_table?$filter=((String eq 'world') and (__updatedAt ge datetimeoffset'2001-02-01T00:00:00.0000000%2B00:00'))&$orderby=__updatedAt&$skip=0&$top=50&param1=val1&__includeDeleted=true&__systemproperties=__createdAt%2C__updatedAt%2C__deleted",
                    "http://test.com/tables/stringId_test_table?$filter=((String eq 'world') and (__updatedAt ge datetimeoffset'2001-02-03T00:00:00.0000000%2B00:00'))&$orderby=__updatedAt&$skip=0&$top=50&param1=val1&__includeDeleted=true&__systemproperties=__createdAt%2C__updatedAt%2C__deleted");
        }

        [AsyncTestMethod]
        public async Task PullAsync_Incremental_WithNullUpdatedAt_Succeeds()
        {
            var hijack = new TestHttpHandler();
            hijack.OnSendingRequest = req =>
            {
                return Task.FromResult(req);
            };
            hijack.AddResponseContent(@"[{""id"":""abc"",""String"":""Hey"", ""__updatedAt"": null}]");
            hijack.AddResponseContent("[]"); // last page

            var store = new MobileServiceLocalStoreMock();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();

            await table.PullAsync("items", table.CreateQuery());
            AssertEx.MatchUris(hijack.Requests,
                "http://www.test.com/tables/stringId_test_table?$filter=(__updatedAt ge datetimeoffset'1970-01-01T00:00:00.0000000%2B00:00')&$orderby=__updatedAt&$skip=0&$top=50&__includeDeleted=true&__systemproperties=__updatedAt%2C__version%2C__deleted",
                "http://www.test.com/tables/stringId_test_table?$filter=(__updatedAt ge datetimeoffset'1970-01-01T00:00:00.0000000%2B00:00')&$orderby=__updatedAt&$skip=1&$top=50&__includeDeleted=true&__systemproperties=__updatedAt%2C__version%2C__deleted");
        }

        [AsyncTestMethod]
        public async Task PullAsync_Incremental_MovesByUpdatedAt_ThenUsesSkipAndTop_WhenUpdatedAtDoesNotChange()
        {
            var hijack = new TestHttpHandler();
            hijack.OnSendingRequest = req =>
            {
                return Task.FromResult(req);
            };
            hijack.AddResponseContent(@"[{""id"":""abc"",""String"":""Hey"", ""__updatedAt"": ""2001-02-03T00:00:00.0000000+00:00""}]");
            hijack.AddResponseContent(@"[{""id"":""def"",""String"":""World"", ""__updatedAt"": ""2001-02-03T00:00:00.0000000+00:00""}]");
            hijack.AddResponseContent("[]"); // last page

            var store = new MobileServiceLocalStoreMock();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();

            await table.PullAsync("items", table.CreateQuery());
            AssertEx.MatchUris(hijack.Requests,
                "http://www.test.com/tables/stringId_test_table?$filter=(__updatedAt ge datetimeoffset'1970-01-01T00:00:00.0000000%2B00:00')&$orderby=__updatedAt&$skip=0&$top=50&__includeDeleted=true&__systemproperties=__updatedAt%2C__version%2C__deleted",
                "http://www.test.com/tables/stringId_test_table?$filter=(__updatedAt ge datetimeoffset'2001-02-03T00:00:00.0000000%2B00:00')&$orderby=__updatedAt&$skip=0&$top=50&__includeDeleted=true&__systemproperties=__updatedAt%2C__version%2C__deleted",
                "http://www.test.com/tables/stringId_test_table?$filter=(__updatedAt ge datetimeoffset'2001-02-03T00:00:00.0000000%2B00:00')&$orderby=__updatedAt&$skip=1&$top=50&__includeDeleted=true&__systemproperties=__updatedAt%2C__version%2C__deleted");
        }

        [AsyncTestMethod]
        public async Task PullAsync_Incremental_WithoutOrderBy_MovesBySkipAndTop()
        {
            await TestPullAsyncIncrementalWithOptions(MobileServiceRemoteTableOptions.Skip | MobileServiceRemoteTableOptions.Top,
                "http://test.com/tables/stringId_test_table?$filter=((String eq 'world') and (__updatedAt ge datetimeoffset'2001-02-01T00:00:00.0000000%2B00:00'))&$skip=0&$top=50&param1=val1&__includeDeleted=true&__systemproperties=__createdAt%2C__updatedAt%2C__deleted",
                "http://test.com/tables/stringId_test_table?$filter=((String eq 'world') and (__updatedAt ge datetimeoffset'2001-02-01T00:00:00.0000000%2B00:00'))&$skip=2&$top=50&param1=val1&__includeDeleted=true&__systemproperties=__createdAt%2C__updatedAt%2C__deleted");
        }

        [AsyncTestMethod]
        public async Task PullAsync_Incremental_WithoutSkipAndOrderBy_CanNotMoveForward()
        {
            await TestPullAsyncIncrementalWithOptions(MobileServiceRemoteTableOptions.Top,
                "http://test.com/tables/stringId_test_table?$filter=((String eq 'world') and (__updatedAt ge datetimeoffset'2001-02-01T00:00:00.0000000%2B00:00'))&$top=50&param1=val1&__includeDeleted=true&__systemproperties=__createdAt%2C__updatedAt%2C__deleted");
        }

        [AsyncTestMethod]
        public async Task PullAsync_Incremental_WithoutTopAndOrderBy_MovesBySkip()
        {
            await TestPullAsyncIncrementalWithOptions(MobileServiceRemoteTableOptions.Skip,
                "http://test.com/tables/stringId_test_table?$filter=((String eq 'world') and (__updatedAt ge datetimeoffset'2001-02-01T00:00:00.0000000%2B00:00'))&param1=val1&__includeDeleted=true&__systemproperties=__createdAt%2C__updatedAt%2C__deleted",
                "http://test.com/tables/stringId_test_table?$filter=((String eq 'world') and (__updatedAt ge datetimeoffset'2001-02-01T00:00:00.0000000%2B00:00'))&$skip=2&param1=val1&__includeDeleted=true&__systemproperties=__createdAt%2C__updatedAt%2C__deleted");
        }

        [AsyncTestMethod]
        public async Task PullAsync_Incremental_WithoutSkipAndTop_MovesByUpdatedAt()
        {
            await TestPullAsyncIncrementalWithOptions(MobileServiceRemoteTableOptions.OrderBy,
                "http://test.com/tables/stringId_test_table?$filter=((String eq 'world') and (__updatedAt ge datetimeoffset'2001-02-01T00:00:00.0000000%2B00:00'))&$orderby=__updatedAt&param1=val1&__includeDeleted=true&__systemproperties=__createdAt%2C__updatedAt%2C__deleted",
                "http://test.com/tables/stringId_test_table?$filter=((String eq 'world') and (__updatedAt ge datetimeoffset'2001-02-03T00:00:00.0000000%2B00:00'))&$orderby=__updatedAt&param1=val1&__includeDeleted=true&__systemproperties=__createdAt%2C__updatedAt%2C__deleted");
        }

        private static async Task TestPullAsyncIncrementalWithOptions(MobileServiceRemoteTableOptions options, params string[] uris)
        {
            var store = new MobileServiceLocalStoreMock();
            var settings = new MobileServiceSyncSettingsManager(store);
            await settings.SetDeltaTokenAsync("stringId_test_table", "incquery", new DateTime(2001, 02, 01, 0, 0, 0, DateTimeKind.Utc));
            await TestIncrementalPull(store, options, uris);
        }

        [AsyncTestMethod]
        public async Task PullAsync_Incremental_WithoutDeltaTokenInDb()
        {
            var store = new MobileServiceLocalStoreMock();
            store.TableMap[MobileServiceLocalSystemTables.Config] = new Dictionary<string, JObject>();
            await TestIncrementalPull(store, MobileServiceRemoteTableOptions.All,
                "http://test.com/tables/stringId_test_table?$filter=((String eq 'world') and (__updatedAt ge datetimeoffset'1970-01-01T00:00:00.0000000%2B00:00'))&$orderby=__updatedAt&$skip=0&$top=50&param1=val1&__includeDeleted=true&__systemproperties=__createdAt%2C__updatedAt%2C__deleted",
                "http://test.com/tables/stringId_test_table?$filter=((String eq 'world') and (__updatedAt ge datetimeoffset'2001-02-03T00:00:00.0000000%2B00:00'))&$orderby=__updatedAt&$skip=0&$top=50&param1=val1&__includeDeleted=true&__systemproperties=__createdAt%2C__updatedAt%2C__deleted");
        }

        private static async Task TestIncrementalPull(MobileServiceLocalStoreMock store, MobileServiceRemoteTableOptions options, params string[] expectedUris)
        {
            var hijack = new TestHttpHandler();
            hijack.AddResponseContent(@"[{""id"":""abc"",""String"":""Hey"", ""__updatedAt"": ""2001-02-03T00:00:00.0000000+00:00""},
                                        {""id"":""def"",""String"":""World"", ""__updatedAt"": ""2001-02-03T00:03:00.0000000+07:00""}]"); // for pull
            hijack.AddResponseContent(@"[]");


            store.TableMap[MobileServiceLocalSystemTables.Config]["systemProperties|stringId_test_table"] = new JObject
            {
                { MobileServiceSystemColumns.Id, "systemProperties|stringId_test_table" },
                { "value", "1" }
            };

            IMobileServiceClient service = new MobileServiceClient("http://test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();
            table.SupportedOptions = options;
            var query = table.Where(t => t.String == "world")
                             .WithParameters(new Dictionary<string, string>() { { "param1", "val1" } })
                             .IncludeTotalCount();

            await table.PullAsync("incquery", query, cancellationToken: CancellationToken.None);

            AssertEx.MatchUris(hijack.Requests, expectedUris);
        }

        [AsyncTestMethod]
        public async Task PullAsync_Throws_IfSystemPropertiesProvidedInParameters()
        {
            await this.TestPullQueryOverrideThrows(new Dictionary<string, string>() 
                             { 
                                { "__systemProperties", "createdAt" },
                                { "param1", "val1" } 
                             },
                             "The key '__systemproperties' is reserved and cannot be specified as a query parameter.");
        }

        [AsyncTestMethod]
        public async Task PullAsync_Throws_IfIncludeDeletedProvidedInParameters()
        {
            await this.TestPullQueryOverrideThrows(new Dictionary<string, string>() 
                             { 
                                { "__includeDeleted", "false" },
                                { "param1", "val1" } 
                             },
                             "The key '__includeDeleted' is reserved and cannot be specified as a query parameter.");
        }

        [AsyncTestMethod]
        public async Task PullAsync_Throws_WhenQueryIdIsInvalid()
        {
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", new TestHttpHandler());
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();

            await ThrowsAsync<ArgumentException>(() => table.PullAsync("2as|df", table.CreateQuery(), CancellationToken.None));
        }

        private async Task TestPullQueryOverrideThrows(IDictionary<string, string> parameters, string errorMessage)
        {
            var store = new MobileServiceLocalStoreMock();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();
            var query = table.CreateQuery()
                             .WithParameters(parameters);

            var ex = await ThrowsAsync<ArgumentException>(() => table.PullAsync(null, query, cancellationToken: CancellationToken.None));
            Assert.AreEqual(errorMessage, ex.Message);
        }

        [AsyncTestMethod]
        public async Task PurgeAsync_DoesNotTriggerPush_WhenThereIsNoOperationInTable()
        {
            var hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":\"abc\",\"String\":\"Hey\"}");
            var store = new MobileServiceLocalStoreMock();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            // insert item in purge table
            IMobileServiceSyncTable table1 = service.GetSyncTable("someTable");
            await table1.InsertAsync(new JObject() { { "id", "abc" } });

            // but push to clear the queue
            await service.SyncContext.PushAsync();
            Assert.AreEqual(store.TableMap[table1.TableName].Count, 1); // item is inserted
            Assert.AreEqual(hijack.Requests.Count, 1); // first push

            // then insert item in other table
            IMobileServiceSyncTable<StringIdType> table2 = service.GetSyncTable<StringIdType>();
            var item = new StringIdType() { Id = "an id", String = "what?" };
            await table2.InsertAsync(item);

            // try purge on first table now
            await table1.PurgeAsync();

            Assert.AreEqual(store.DeleteQueries[0].TableName, MobileServiceLocalSystemTables.SyncErrors); // push deletes all sync erros
            Assert.AreEqual(store.DeleteQueries[1].TableName, table1.TableName); // purged table
            Assert.AreEqual(hijack.Requests.Count, 1); // still 1 means no other push happened
            Assert.AreEqual(store.TableMap[table2.TableName].Count, 1); // this table should not be touched
        }

        [AsyncTestMethod]
        public async Task PurgeAsync_ResetsDeltaToken_WhenQueryIdIsSpecified()
        {
            var hijack = new TestHttpHandler();
            hijack.AddResponseContent(@"[{""id"":""abc"",""String"":""Hey"", ""__updatedAt"": ""2001-02-03T00:00:00.0000000+00:00""},
                                         {""id"":""def"",""String"":""How"", ""__updatedAt"": ""2001-02-04T00:00:00.0000000+00:00""}]"); // first page
            hijack.AddResponseContent("[]"); // last page of first pull
            hijack.AddResponseContent("[]"); // second pull

            var store = new MobileServiceLocalStoreMock();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();

            // ensure there is no delta token present already
            Assert.IsFalse(store.TableMap.ContainsKey("stringId_test_table"));

            // now pull down data
            await table.PullAsync("items", table.CreateQuery());

            // ensure items were pulled down
            Assert.AreEqual(store.TableMap["stringId_test_table"].Count, 2);
            Assert.AreEqual(store.TableMap["stringId_test_table"]["abc"].Value<string>("String"), "Hey");
            Assert.AreEqual(store.TableMap["stringId_test_table"]["def"].Value<string>("String"), "How");

            // ensure delta token was updated
            Assert.Equals(store.TableMap[MobileServiceLocalSystemTables.Config]["deltaToken|stringId_test_table|items"]["value"], "2001-02-04T00:00:00.0000000+00:00");

            // now purge and forget the delta token
            await table.PurgeAsync("items", null, false, CancellationToken.None);

            // make sure data is purged
            Assert.AreEqual(store.TableMap["stringId_test_table"].Count, 0);
            // make sure delta token is removed
            Assert.IsFalse(store.TableMap[MobileServiceLocalSystemTables.Config].ContainsKey("deltaToken|stringId_test_table|items"));

            // pull again
            await table.PullAsync("items", table.CreateQuery());

            // verify request urls
            AssertEx.MatchUris(hijack.Requests, "http://www.test.com/tables/stringId_test_table?$filter=(__updatedAt ge datetimeoffset'1970-01-01T00:00:00.0000000%2B00:00')&$orderby=__updatedAt&$skip=0&$top=50&__includeDeleted=true&__systemproperties=__updatedAt%2C__version%2C__deleted",
                                                "http://www.test.com/tables/stringId_test_table?$filter=(__updatedAt ge datetimeoffset'2001-02-04T00:00:00.0000000%2B00:00')&$orderby=__updatedAt&$skip=0&$top=50&__includeDeleted=true&__systemproperties=__updatedAt%2C__version%2C__deleted",
                                                "http://www.test.com/tables/stringId_test_table?$filter=(__updatedAt ge datetimeoffset'1970-01-01T00:00:00.0000000%2B00:00')&$orderby=__updatedAt&$skip=0&$top=50&__includeDeleted=true&__systemproperties=__updatedAt%2C__version%2C__deleted");
        }

        [AsyncTestMethod]
        public async Task PurgeAsync_Throws_WhenQueryIdIsInvalid()
        {
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", new TestHttpHandler());
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();

            await ThrowsAsync<ArgumentException>(() => table.PurgeAsync("2as|df", table.CreateQuery(), CancellationToken.None));
        }

        [AsyncTestMethod]
        public async Task PurgeAsync_Throws_WhenThereIsOperationInTable()
        {
            var hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":\"abc\",\"String\":\"Hey\"}");
            var store = new MobileServiceLocalStoreMock();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            // insert an item but don't push
            IMobileServiceSyncTable table1 = service.GetSyncTable("someTable");
            await table1.InsertAsync(new JObject() { { "id", "abc" } });
            Assert.AreEqual(store.TableMap[table1.TableName].Count, 1); // item is inserted

            // this should trigger a push
            var ex = await ThrowsAsync<InvalidOperationException>(table1.PurgeAsync);

            Assert.AreEqual(ex.Message, "The table cannot be purged because it has pending operations.");
            Assert.AreEqual(service.SyncContext.PendingOperations, 1L); // operation still in queue
        }

        [AsyncTestMethod]
        public async Task PurgeAsync_Throws_WhenThereIsOperationInTable_AndForceIsTrue_AndQueryIsSpecified()
        {
            var hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":\"abc\",\"String\":\"Hey\"}");
            var store = new MobileServiceLocalStoreMock();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            // insert an item but don't push
            IMobileServiceSyncTable table = service.GetSyncTable("someTable");
            await table.InsertAsync(new JObject() { { "id", "abc" } });
            Assert.AreEqual(store.TableMap[table.TableName].Count, 1); // item is inserted

            // this should trigger a push
            var ex = await ThrowsAsync<InvalidOperationException>(() => table.PurgeAsync(null, "$filter=a eq b", true, CancellationToken.None));

            Assert.AreEqual(ex.Message, "The table cannot be purged because it has pending operations.");
            Assert.AreEqual(service.SyncContext.PendingOperations, 1L); // operation still in queue
        }

        [AsyncTestMethod]
        public async Task PurgeAsync_DeletesOperations_WhenThereIsOperationInTable_AndForceIsTrue()
        {
            var hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":\"abc\",\"String\":\"Hey\"}");
            var store = new MobileServiceLocalStoreMock();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            // put a dummy delta token
            string deltaKey = "deltaToken|someTable|abc";
            store.TableMap[MobileServiceLocalSystemTables.Config] = new Dictionary<string, JObject>() { { deltaKey, new JObject() } };

            // insert an item but don't push
            IMobileServiceSyncTable table = service.GetSyncTable("someTable");
            await table.InsertAsync(new JObject() { { "id", "abc" } });
            Assert.AreEqual(store.TableMap[table.TableName].Count, 1); // item is inserted
            Assert.AreEqual(service.SyncContext.PendingOperations, 1L);

            await table.PurgeAsync("abc", null, force: true, cancellationToken: CancellationToken.None);

            Assert.AreEqual(store.TableMap[table.TableName].Count, 0); // item is deleted
            Assert.AreEqual(service.SyncContext.PendingOperations, 0L); // operation is also removed

            // deleted delta token
            Assert.IsFalse(store.TableMap[MobileServiceLocalSystemTables.Config].ContainsKey(deltaKey));
        }

        [AsyncTestMethod]
        public async Task PushAsync_ExecutesThePendingOperations_OnAllTables()
        {
            var hijack = new TestHttpHandler();
            var store = new MobileServiceLocalStoreMock();
            hijack.AddResponseContent("{\"id\":\"abc\",\"String\":\"Hey\"}");
            hijack.AddResponseContent("{\"id\":\"def\",\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<StringIdType> table = service.GetSyncTable<StringIdType>();
            var item = new StringIdType() { Id = "abc", String = "what?" };
            await table.InsertAsync(item);

            IMobileServiceSyncTable table1 = service.GetSyncTable("someTable");
            await table1.InsertAsync(new JObject() { { "id", "def" } });

            Assert.AreEqual(store.TableMap[table.TableName].Count, 1);
            Assert.AreEqual(store.TableMap[table1.TableName].Count, 1);

            await service.SyncContext.PushAsync();
        }

        [AsyncTestMethod]
        public async Task PushAsync_ExecutesThePendingOperations()
        {
            var hijack = new TestHttpHandler();
            var store = new MobileServiceLocalStoreMock();
            hijack.SetResponseContent("{\"id\":\"abc\",\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<StringIdType> table = service.GetSyncTable<StringIdType>();

            var item = new StringIdType() { Id = "an id", String = "what?" };
            await table.InsertAsync(item);

            Assert.AreEqual(store.TableMap[table.TableName].Count, 1);

            await service.SyncContext.PushAsync();
        }

        [AsyncTestMethod]
        public async Task PushAsync_IsAborted_OnNetworkError()
        {
            await TestPushAbort(new HttpRequestException(), MobileServicePushStatus.CancelledByNetworkError);
        }

        [AsyncTestMethod]
        public async Task PushAsync_IsAborted_OnAuthenticationError()
        {
            var authError = new MobileServiceInvalidOperationException(String.Empty, new HttpRequestMessage(), new HttpResponseMessage(HttpStatusCode.Unauthorized));
            await TestPushAbort(authError, MobileServicePushStatus.CancelledByAuthenticationError);
        }

        [AsyncTestMethod]
        public async Task DeleteAsync_DoesNotUpsertResultOnStore_WhenOperationIsPushed()
        {
            var storeMock = new MobileServiceLocalStoreMock();

            var hijack = new TestHttpHandler();
            hijack.AddResponseContent("{\"id\":\"abc\",\"String\":\"Hey\"}"); // for insert
            hijack.AddResponseContent("{\"id\":\"abc\",\"String\":\"Hey\"}"); // for delete
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            await service.SyncContext.InitializeAsync(storeMock, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<StringIdType> table = service.GetSyncTable<StringIdType>();

            // first add an item
            var item = new StringIdType() { Id = "abc", String = "what?" };
            await table.InsertAsync(item);

            Assert.AreEqual(storeMock.TableMap[table.TableName].Count, 1);

            // for good measure also push it
            await service.SyncContext.PushAsync();

            await table.DeleteAsync(item);

            Assert.AreEqual(storeMock.TableMap[table.TableName].Count, 0);

            // now play it on server
            await service.SyncContext.PushAsync();

            // wait we don't want to upsert the result back because its delete operation
            Assert.AreEqual(storeMock.TableMap[table.TableName].Count, 0);
            // looks good
        }

        [AsyncTestMethod]
        public async Task DeleteAsync_Throws_WhenInsertWasAttempted()
        {
            var hijack = new TestHttpHandler();
            hijack.Response = new HttpResponseMessage(HttpStatusCode.RequestTimeout); // insert response
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), new MobileServiceSyncHandler());
            IMobileServiceSyncTable<StringIdType> table = service.GetSyncTable<StringIdType>();

            var item = new StringIdType() { Id = "an id", String = "what?" };
            await table.InsertAsync(item);
            // insert is in queue
            Assert.AreEqual(service.SyncContext.PendingOperations, 1L);

            var pushException = await ThrowsAsync<MobileServicePushFailedException>(service.SyncContext.PushAsync);
            Assert.AreEqual(pushException.PushResult.Errors.Count(), 1);

            var delException = await ThrowsAsync<InvalidOperationException>(() => table.DeleteAsync(item));
            Assert.AreEqual(delException.Message, "The item is in inconsistent state in the local store. Please complete the pending sync by calling PushAsync() before deleting the item.");

            // insert still in queue
            Assert.AreEqual(service.SyncContext.PendingOperations, 1L);
        }

        [AsyncTestMethod]
        public Task DeleteAsync_Throws_WhenDeleteIsInQueue()
        {
            return this.TestCollapseThrow(firstOperation: (table, item) => table.DeleteAsync(item),
                                          secondOperation: (table, item) => table.DeleteAsync(item));
        }

        [AsyncTestMethod]
        public async Task DeleteAsync_CancelsAll_WhenInsertIsInQueue()
        {
            var hijack = new TestHttpHandler();
            hijack.SetResponseContent("[{\"id\":\"abc\",\"String\":\"Hey\"}]");
            hijack.OnSendingRequest = req =>
            {
                Assert.Fail("No request should be made.");
                return Task.FromResult(req);
            };
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), new MobileServiceSyncHandler());
            IMobileServiceSyncTable<StringIdType> table = service.GetSyncTable<StringIdType>();

            var item = new StringIdType() { Id = "an id", String = "what?" };

            await table.InsertAsync(item);
            Assert.AreEqual(service.SyncContext.PendingOperations, 1L);

            await table.DeleteAsync(item);
            await service.SyncContext.PushAsync();

            Assert.AreEqual(service.SyncContext.PendingOperations, 0L);
        }

        [AsyncTestMethod]
        public async Task DeleteAsync_CancelsUpdate_WhenUpdateIsInQueue()
        {
            var store = new MobileServiceLocalStoreMock();
            await this.TestCollapseCancel(firstOperationOnItem1: (table, item1) => table.UpdateAsync(item1),
                                          operationOnItem2: (table, item2) => table.InsertAsync(item2),
                                          secondOperationOnItem1: (table, item1) => table.DeleteAsync(item1),
                                          assertRequest: (req, executed) =>
                                          {
                                              if (executed == 1) // order is maintained by doing insert first and delete after that. This means first update was cancelled, not the second one.
                                              {
                                                  Assert.AreEqual(req.Method, HttpMethod.Post);
                                              }
                                              else
                                              {
                                                  Assert.AreEqual(req.Method, HttpMethod.Delete);
                                              }
                                          },
                                          assertQueue: queue =>
                                          {
                                              var op = queue.Values.Single(o => o.Value<string>("itemId") == "item1");
                                              Assert.AreEqual(op.Value<long>("version"), 2L);
                                          });
        }

        [AsyncTestMethod]
        public Task InsertAsync_Throws_WhenDeleteIsInQueue()
        {
            return this.TestCollapseThrow(firstOperation: (table, item) => table.DeleteAsync(item),
                                          secondOperation: (table, item) => table.InsertAsync(item));
        }

        [AsyncTestMethod]
        public Task InsertAsync_Throws_WhenUpdateIsInQueue()
        {
            return this.TestCollapseThrow(firstOperation: (table, item) => table.UpdateAsync(item),
                                          secondOperation: (table, item) => table.InsertAsync(item));
        }

        [AsyncTestMethod]
        public Task InsertAsync_Throws_WhenInsertIsInQueue()
        {
            return this.TestCollapseThrow(firstOperation: (table, item) => table.InsertAsync(item),
                                          secondOperation: (table, item) => table.InsertAsync(item));
        }

        [AsyncTestMethod]
        public Task UpdateAsync_Throws_WhenDeleteIsInQueue()
        {
            return this.TestCollapseThrow(firstOperation: (table, item) => table.DeleteAsync(item),
                                          secondOperation: (table, item) => table.UpdateAsync(item));
        }

        [AsyncTestMethod]
        public async Task UpdateAsync_CancelsSecondUpdate_WhenUpdateIsInQueue()
        {
            await this.TestCollapseCancel(firstOperationOnItem1: (table, item1) => table.UpdateAsync(item1),
                                        operationOnItem2: (table, item2) => table.DeleteAsync(item2),
                                        secondOperationOnItem1: (table, item1) => table.UpdateAsync(item1),
                                        assertRequest: (req, executed) =>
                                        {
                                            if (executed == 1) // order is maintained by doing update first and delete after that. This means second update was cancelled, not the first one.
                                            {
                                                Assert.AreEqual(req.Method, new HttpMethod("Patch"));
                                            }
                                            else
                                            {
                                                Assert.AreEqual(req.Method, HttpMethod.Delete);
                                            }
                                        },
                                        assertQueue: queue =>
                                        {
                                            var op = queue.Values.Single(o => o.Value<string>("itemId") == "item1");
                                            Assert.AreEqual(op.Value<long>("version"), 2L);
                                        });
        }

        [AsyncTestMethod]
        public async Task Collapse_DeletesTheError_OnMutualCancel()
        {
            var store = new MobileServiceLocalStoreMock();
            var hijack = new TestHttpHandler();
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            var item = new StringIdType() { Id = "item1", String = "what?" };

            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());
            IMobileServiceSyncTable<StringIdType> table = service.GetSyncTable<StringIdType>();

            await table.InsertAsync(item);
            Assert.AreEqual(service.SyncContext.PendingOperations, 1L);

            string id = store.TableMap[MobileServiceLocalSystemTables.OperationQueue].Values.First().Value<string>("id");
            // inject an error to test if it is deleted on collapse
            store.TableMap[MobileServiceLocalSystemTables.SyncErrors] = new Dictionary<string, JObject>() { { id, new JObject() } };

            await table.DeleteAsync(item);
            Assert.AreEqual(service.SyncContext.PendingOperations, 0L);

            // error should be deleted
            Assert.AreEqual(store.TableMap[MobileServiceLocalSystemTables.SyncErrors].Count, 0);
        }

        [AsyncTestMethod]
        public async Task Collapse_DeletesTheError_OnReplace()
        {
            var store = new MobileServiceLocalStoreMock();
            var hijack = new TestHttpHandler();
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            var item = new StringIdType() { Id = "item1", String = "what?" };

            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());
            IMobileServiceSyncTable<StringIdType> table = service.GetSyncTable<StringIdType>();

            await table.InsertAsync(item);
            Assert.AreEqual(service.SyncContext.PendingOperations, 1L);

            string id = store.TableMap[MobileServiceLocalSystemTables.OperationQueue].Values.First().Value<string>("id");

            // inject an error to test if it is deleted on collapse
            store.TableMap[MobileServiceLocalSystemTables.SyncErrors] = new Dictionary<string, JObject>() { { id, new JObject() } };

            await table.UpdateAsync(item);
            Assert.AreEqual(service.SyncContext.PendingOperations, 1L);

            // error should be deleted
            Assert.AreEqual(store.TableMap[MobileServiceLocalSystemTables.SyncErrors].Count, 0);
        }


        [AsyncTestMethod]
        public async Task UpdateAsync_CancelsSecondUpdate_WhenInsertIsInQueue()
        {
            await this.TestCollapseCancel(firstOperationOnItem1: (table, item1) => table.InsertAsync(item1),
                                        operationOnItem2: (table, item2) => table.DeleteAsync(item2),
                                        secondOperationOnItem1: (table, item1) => table.UpdateAsync(item1),
                                        assertRequest: (req, executed) =>
                                        {
                                            if (executed == 1) // order is maintained by doing insert first and delete after that. This means second update was cancelled.
                                            {
                                                Assert.AreEqual(req.Method, HttpMethod.Post);
                                            }
                                            else
                                            {
                                                Assert.AreEqual(req.Method, HttpMethod.Delete);
                                            }
                                        },
                                        assertQueue: queue =>
                                        {
                                            var op = queue.Values.Single(o => o.Value<string>("itemId") == "item1");
                                            Assert.AreEqual(op.Value<long>("version"), 2L);
                                        });
        }

        [AsyncTestMethod]
        public async Task ReadAsync_PassesOdataToStore_WhenLinqIsUsed()
        {
            var store = new MobileServiceLocalStoreMock();
            store.ReadResponses.Enqueue("{count: 1, results: [{\"id\":\"abc\",\"String\":\"Hey\"}]}");

            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<StringIdType> table = service.GetSyncTable<StringIdType>();

            IMobileServiceTableQuery<string> query = table.Skip(5)
                                                          .Take(3)
                                                          .Where(t => t.String == "world")
                                                          .OrderBy(o => o.Id)
                                                          .OrderByDescending(o => o.String)
                                                          .IncludeTotalCount()
                                                          .Select(x => x.String);

            IEnumerable<string> result = await table.ReadAsync(query);

            string odata = store.ReadQueries.First().ToODataString();
            Assert.AreEqual(odata, "$filter=(String eq 'world')&" +
                                    "$orderby=String desc,id&" +
                                    "$skip=5&" +
                                    "$top=3&" +
                                    "$select=String&" +
                                    "$inlinecount=allpages");
        }

        [AsyncTestMethod]
        public async Task ToEnumerableAsync_ParsesOData_WhenRawQueryIsProvided()
        {
            var store = new MobileServiceLocalStoreMock();
            store.ReadResponses.Enqueue("{count: 1, results: [{\"id\":\"abc\",\"String\":\"Hey\"}]}");

            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<StringIdType> table = service.GetSyncTable<StringIdType>();

            string odata = "$filter=(String eq 'world')&" +
                            "$orderby=String desc,id&" +
                            "$skip=5&" +
                            "$top=3&" +
                            "$select=String&" +
                            "$inlinecount=allpages";

            await table.ReadAsync(odata);

            string odata2 = store.ReadQueries.First().ToODataString();
            Assert.AreEqual(odata, odata2);
        }

        /// <summary>
        /// Tests that the second operation on the same item will cancel one of the two operations how ever other operations between the two (on other items) are not reordered
        /// </summary>
        /// <param name="firstOperationOnItem1">first operation on item 1</param>
        /// <param name="operationOnItem2">operation on item 2</param>
        /// <param name="secondOperationOnItem1">second operation on item 1</param>
        /// <param name="assertRequest">To check which of the two operations got cancelled</param>
        private async Task TestCollapseCancel(Func<IMobileServiceSyncTable<StringIdType>, StringIdType, Task> firstOperationOnItem1,
                                              Func<IMobileServiceSyncTable<StringIdType>, StringIdType, Task> operationOnItem2,
                                              Func<IMobileServiceSyncTable<StringIdType>, StringIdType, Task> secondOperationOnItem1,
                                              Action<HttpRequestMessage, int> assertRequest,
                                              Action<Dictionary<string, JObject>> assertQueue)
        {
            var store = new MobileServiceLocalStoreMock();
            var hijack = new TestHttpHandler();
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            var item1 = new StringIdType() { Id = "item1", String = "what?" };
            var item2 = new StringIdType() { Id = "item2", String = "this" };
            int executed = 0;
            hijack.SetResponseContent("{\"id\":\"abc\",\"String\":\"Hey\"}");
            hijack.OnSendingRequest = req =>
            {
                ++executed;
                assertRequest(req, executed);
                return Task.FromResult(req);
            };

            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());
            IMobileServiceSyncTable<StringIdType> table = service.GetSyncTable<StringIdType>();

            await firstOperationOnItem1(table, item1);
            Assert.AreEqual(service.SyncContext.PendingOperations, 1L);

            await operationOnItem2(table, item2);
            Assert.AreEqual(service.SyncContext.PendingOperations, 2L);

            await secondOperationOnItem1(table, item1);
            Assert.AreEqual(service.SyncContext.PendingOperations, 2L);

            Dictionary<string, JObject> queue = store.TableMap[MobileServiceLocalSystemTables.OperationQueue];
            assertQueue(queue);

            await service.SyncContext.PushAsync();

            Assert.AreEqual(service.SyncContext.PendingOperations, 0L);
            Assert.AreEqual(executed, 2); // total two operations executed
        }

        /// <summary>
        /// Tests that the second operation on the same item will throw if first operation is in the queue
        /// </summary>
        /// <param name="firstOperation">The operation that was already in queue.</param>
        /// <param name="secondOperation">The operation that came in late but could not be collapsed.</param>
        /// <returns></returns>
        private async Task TestCollapseThrow(Func<IMobileServiceSyncTable<StringIdType>, StringIdType, Task> firstOperation, Func<IMobileServiceSyncTable<StringIdType>, StringIdType, Task> secondOperation)
        {
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), new MobileServiceSyncHandler());
            IMobileServiceSyncTable<StringIdType> table = service.GetSyncTable<StringIdType>();

            var item = new StringIdType() { Id = "an id", String = "what?" };
            await firstOperation(table, item);
            Assert.AreEqual(service.SyncContext.PendingOperations, 1L);

            await ThrowsAsync<InvalidOperationException>(() => secondOperation(table, item));

            Assert.AreEqual(service.SyncContext.PendingOperations, 1L);
        }

        /// <summary>
        /// Tests that throwing an exception of type toThrow from the http handler causes the push sync to be aborted
        /// </summary>
        /// <param name="toThrow">The exception to simulate coming from http layer</param>
        /// <param name="expectedStatus">The expected status of push operation as reported in PushCompletionResult and PushFailedException</param>
        /// <returns></returns>
        private async Task TestPushAbort(Exception toThrow, MobileServicePushStatus expectedStatus)
        {
            bool thrown = false;
            var hijack = new TestHttpHandler();
            hijack.OnSendingRequest = req =>
            {
                if (!thrown)
                {
                    thrown = true;
                    throw toThrow;
                }
                return Task.FromResult(req);
            };

            var operationHandler = new MobileServiceSyncHandlerMock();

            hijack.SetResponseContent("{\"id\":\"abc\",\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), operationHandler);

            IMobileServiceSyncTable<StringIdType> table = service.GetSyncTable<StringIdType>();

            var item = new StringIdType() { Id = "an id", String = "what?" };
            await table.InsertAsync(item);

            Assert.AreEqual(service.SyncContext.PendingOperations, 1L);

            MobileServicePushFailedException ex = await ThrowsAsync<MobileServicePushFailedException>(service.SyncContext.PushAsync);

            Assert.AreEqual(ex.PushResult.Status, expectedStatus);
            Assert.AreEqual(ex.PushResult.Errors.Count(), 0);

            Assert.AreEqual(operationHandler.PushCompletionResult.Status, expectedStatus);

            // the insert operation is still in queue
            Assert.AreEqual(service.SyncContext.PendingOperations, 1L);

            await service.SyncContext.PushAsync();

            Assert.AreEqual(service.SyncContext.PendingOperations, 0L);
            Assert.AreEqual(operationHandler.PushCompletionResult.Status, MobileServicePushStatus.Complete);
        }
    }
}
