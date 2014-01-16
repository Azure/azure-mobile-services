// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Query;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Microsoft.WindowsAzure.MobileServices.TestFramework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    [Tag("unit")]
    [Tag("table")]
    [Tag("offline")]
    public class MobileServiceSyncTableGenericTests :TestBase
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
            var hijack = new TestHttpHandler();
            hijack.AddResponseContent("[{\"id\":\"def\",\"String\":\"World\"}]"); // remote item

            var store = new MobileServiceLocalStoreMock();

            var handler = new MobileServiceSyncHandlerMock();
            handler.TableOperationAction = op => Task.Delay(TimeSpan.MaxValue); // long slow operation

            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(store, handler);

            IMobileServiceSyncTable table = service.GetSyncTable("someTable");

            using (var cts = new CancellationTokenSource())
            {
                // now pull
                Task pullTask = table.PullAsync(null, cancellationToken: cts.Token);
                cts.Cancel();

                await ThrowsAsync<TaskCanceledException>(() => pullTask);

                Assert.AreEqual(pullTask.Status, TaskStatus.Canceled);
            }
        }

        [AsyncTestMethod]
        public async Task PullAsync_DoesNotPurge_WhenItemIsMissing()
        {
            var hijack = new TestHttpHandler();
            hijack.AddResponseContent("{\"id\":\"abc\",\"String\":\"Hey\"}"); // for insert
            hijack.AddResponseContent("[{\"id\":\"def\",\"String\":\"World\"}]"); // remote item

            var store = new MobileServiceLocalStoreMock();

            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable table = service.GetSyncTable("someTable");
            await table.InsertAsync(new JObject() { { "id", "abc" } }); // insert an item
            await service.SyncContext.PushAsync(); // push to clear the queue 

            // now pull
            await table.PullAsync(null, cancellationToken: CancellationToken.None);

            Assert.AreEqual(store.Tables[table.TableName].Count, 2); // 1 from remote and 1 from local
            Assert.AreEqual(hijack.Requests.Count, 2); 
        }

        [AsyncTestMethod]
        public async Task PullAsync_DoesNotTriggerPush_WhenThereIsNoOperationInTable()
        {
            var hijack = new TestHttpHandler();
            hijack.AddResponseContent("{\"id\":\"abc\",\"String\":\"Hey\"}"); // for insert
            hijack.AddResponseContent("[{\"id\":\"abc\",\"String\":\"Hey\"},{\"id\":\"def\",\"String\":\"World\"}]"); // for pull

            var store = new MobileServiceLocalStoreMock();
            store.ReadResponses.Enqueue("[{\"id\":\"abc\",\"String\":\"Hey\"},{\"id\":\"def\",\"String\":\"World\"}]"); // for pull

            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            // insert item in pull table
            IMobileServiceSyncTable table1 = service.GetSyncTable("someTable");
            await table1.InsertAsync(new JObject() { { "id", "abc" } });

            // but push to clear the queue
            await service.SyncContext.PushAsync();
            Assert.AreEqual(store.Tables[table1.TableName].Count, 1); // item is inserted
            Assert.AreEqual(hijack.Requests.Count, 1); // first push

            // then insert item in other table
            IMobileServiceSyncTable<StringIdType> table2 = service.GetSyncTable<StringIdType>();
            var item = new StringIdType() { Id = "an id", String = "what?" };
            await table2.InsertAsync(item);

            await table1.PullAsync(null, cancellationToken: CancellationToken.None);

            Assert.AreEqual(store.Tables[table1.TableName].Count, 2); // table should contain 2 pulled items
            Assert.AreEqual(hijack.Requests.Count, 2); // 1 for push and 1 for pull
            Assert.AreEqual(store.Tables[table2.TableName].Count, 1); // this table should not be touched
        }

        [AsyncTestMethod]
        public async Task PullAsync_TriggersPush_WhenThereIsOperationInTable()
        {
            var hijack = new TestHttpHandler();
            hijack.AddResponseContent("{\"id\":\"abc\",\"String\":\"Hey\"}"); // for insert
            hijack.AddResponseContent("[{\"id\":\"abc\",\"String\":\"Hey\"},{\"id\":\"def\",\"String\":\"World\"}]"); // for pull

            var store = new MobileServiceLocalStoreMock();
            store.ReadResponses.Enqueue("[{\"id\":\"abc\",\"String\":\"Hey\"},{\"id\":\"def\",\"String\":\"World\"}]"); // for pull

            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            // insert an item but don't push
            IMobileServiceSyncTable table1 = service.GetSyncTable("someTable");
            await table1.InsertAsync(new JObject() { { "id", "abc" } });
            Assert.AreEqual(store.Tables[table1.TableName].Count, 1); // item is inserted

            // this should trigger a push
            await table1.PullAsync(null, cancellationToken: CancellationToken.None);

            Assert.AreEqual(hijack.Requests.Count, 2); // 1 for push and 1 for pull
            Assert.AreEqual(store.Tables[table1.TableName].Count, 2); // table is populated
        }

        [AsyncTestMethod]
        public async Task PullAsync_Throws_WhenSelectClauseIsSpecified()
        {
            var store = new MobileServiceLocalStoreMock();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();
            var query = table.Skip(5)
                             .Take(3)
                             .Where(t => t.String == "world")
                             .OrderBy(o => o.Id)
                             .OrderByDescending(o => o.String)
                             .IncludeTotalCount()
                             .Select(x => x.String);

            var exception = await ThrowsAsync<ArgumentException>(() => table.PullAsync(query, cancellationToken: CancellationToken.None));
            Assert.AreEqual(exception.ParamName, "query");
            Assert.StartsWith(exception.Message, "Pull query with select clause is not supported.");
        }

        [AsyncTestMethod]
        public async Task PullAsync_Succeds()
        {
            var hijack = new TestHttpHandler();
            hijack.OnSendingRequest = req =>
            {
                Assert.AreEqual(req.RequestUri.Query, "?$filter=(String%20eq%20'world')&$orderby=String%20desc,id&$skip=5&$top=3&__systemproperties=__version");
                return Task.FromResult(req);
            };
            hijack.AddResponseContent("[{\"id\":\"abc\",\"String\":\"Hey\"},{\"id\":\"def\",\"String\":\"World\"}]"); // for pull

            var store = new MobileServiceLocalStoreMock();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();
            var query = table.Skip(5)
                             .Take(3)
                             .Where(t => t.String == "world")
                             .OrderBy(o => o.Id)
                             .OrderByDescending(o => o.String)
                             .IncludeTotalCount();

            await table.PullAsync(query, cancellationToken: CancellationToken.None);
            Assert.AreEqual(hijack.Requests.Count, 1);
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
            Assert.AreEqual(store.Tables[table1.TableName].Count, 1); // item is inserted
            Assert.AreEqual(hijack.Requests.Count, 1); // first push

            // then insert item in other table
            IMobileServiceSyncTable<StringIdType> table2 = service.GetSyncTable<StringIdType>();
            var item = new StringIdType() { Id = "an id", String = "what?" };
            await table2.InsertAsync(item);

            // try purge on first table now
            await table1.PurgeAsync();

            Assert.AreEqual(store.DeleteQueries.Count, 1);
            Assert.AreEqual(store.Tables[table1.TableName].Count, 0); // table should now be empty
            Assert.AreEqual(hijack.Requests.Count, 1); // still 1 means no other push happened
            Assert.AreEqual(store.Tables[table2.TableName].Count, 1); // this table should not be touched
        }

        [AsyncTestMethod]
        public async Task PurgeAsync_TriggersPush_WhenThereIsOperationInTable()
        {
            var hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":\"abc\",\"String\":\"Hey\"}");
            var store = new MobileServiceLocalStoreMock();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            // insert an item but don't push
            IMobileServiceSyncTable table1 = service.GetSyncTable("someTable");
            await table1.InsertAsync(new JObject() { { "id", "abc" } });            
            Assert.AreEqual(store.Tables[table1.TableName].Count, 1); // item is inserted

            // this should trigger a push
            await table1.PurgeAsync();

            Assert.AreEqual(hijack.Requests.Count, 1); // push triggered
            Assert.AreEqual(store.DeleteQueries.Count, 1);
            Assert.AreEqual(store.Tables[table1.TableName].Count, 0); // table is purged
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

            Assert.AreEqual(store.Tables[table.TableName].Count, 1);

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

            Assert.AreEqual(storeMock.Tables[table.TableName].Count, 1);            

            // for good measure also push it
            await service.SyncContext.PushAsync();

            await table.DeleteAsync(item);

            Assert.AreEqual(storeMock.Tables[table.TableName].Count, 0);

            // now play it on server
            await service.SyncContext.PushAsync();

            // wait we don't want to upsert the result back because its delete operation
            Assert.AreEqual(storeMock.Tables[table.TableName].Count, 0);
            // looks good
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
            Assert.AreEqual(service.SyncContext.PendingOperations, 1);

            await table.DeleteAsync(item);
            await service.SyncContext.PushAsync();

            Assert.AreEqual(service.SyncContext.PendingOperations, 0);
        }

        [AsyncTestMethod]
        public async Task DeleteAsync_CancelsUpdate_WhenUpdateIsInQueue()
        {
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
                                        });            
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
                                                          .Where (t=>t.String == "world")
                                                          .OrderBy(o=>o.Id)
                                                          .OrderByDescending(o=>o.String)
                                                          .IncludeTotalCount()
                                                          .Select(x => x.String);

            IEnumerable<string> result = await table.ReadAsync(query);

            string odata = store.ReadQueries.First().ToQueryString();
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

            string odata2 = store.ReadQueries.First().ToQueryString();
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
                                              Action<HttpRequestMessage, int> assertRequest)
        {
            var hijack = new TestHttpHandler();
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            var item1 = new StringIdType() { Id = "an id", String = "what?" };
            var item2 = new StringIdType() { Id = "two", String = "this" };
            int executed = 0;
            hijack.SetResponseContent("[{\"id\":\"abc\",\"String\":\"Hey\"}]");
            hijack.OnSendingRequest = req =>
            {
                ++executed;
                assertRequest(req, executed);
                return Task.FromResult(req);
            };

            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), new MobileServiceSyncHandler());
            IMobileServiceSyncTable<StringIdType> table = service.GetSyncTable<StringIdType>();

            await firstOperationOnItem1(table, item1);
            Assert.AreEqual(service.SyncContext.PendingOperations, 1);

            await operationOnItem2(table, item2);
            Assert.AreEqual(service.SyncContext.PendingOperations, 2);

            await secondOperationOnItem1(table, item1);
            Assert.AreEqual(service.SyncContext.PendingOperations, 2); 

            await service.SyncContext.PushAsync();

            Assert.AreEqual(service.SyncContext.PendingOperations, 0);
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
            Assert.AreEqual(service.SyncContext.PendingOperations, 1);

            await ThrowsAsync<InvalidOperationException>(() => secondOperation(table, item));

            Assert.AreEqual(service.SyncContext.PendingOperations, 1);
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

            hijack.SetResponseContent("[{\"id\":\"abc\",\"String\":\"Hey\"}]");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), operationHandler);

            IMobileServiceSyncTable<StringIdType> table = service.GetSyncTable<StringIdType>();

            var item = new StringIdType() { Id = "an id", String = "what?" };
            await table.InsertAsync(item);

            Assert.AreEqual(service.SyncContext.PendingOperations, 1);

            MobileServicePushFailedException ex = await ThrowsAsync<MobileServicePushFailedException>(service.SyncContext.PushAsync);

            Assert.AreEqual(ex.PushResult.Status, expectedStatus);
            Assert.AreEqual(ex.PushResult.Errors.Count(), 0);

            Assert.AreEqual(operationHandler.PushCompletionResult.Status, expectedStatus);

            // the insert operation is still in queue
            Assert.AreEqual(service.SyncContext.PendingOperations, 1);

            await service.SyncContext.PushAsync();

            Assert.AreEqual(service.SyncContext.PendingOperations, 0);
            Assert.AreEqual(operationHandler.PushCompletionResult.Status, MobileServicePushStatus.Complete);
        }
    }
}
