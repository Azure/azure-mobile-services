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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    [Tag("unit")]
    [Tag("offline")]
    [Tag("synccontext")]
    public class MobileServiceSyncContextTests : TestBase
    {

        [AsyncTestMethod]
        public async Task InitializeAsync_Throws_WhenStoreIsNull()
        {
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            await AssertEx.Throws<ArgumentException>(() => service.SyncContext.InitializeAsync(null));
        }

        [AsyncTestMethod]
        public async Task InitializeAsync_DoesNotThrow_WhenSyncHandlerIsNull()
        {
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), null);
        }

        [AsyncTestMethod]
        public async Task PushAsync_ExecutesThePendingOperations_InOrder()
        {
            var hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            var store = new MobileServiceLocalStoreMock();
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable table = service.GetSyncTable("someTable");

            JObject item1 = new JObject() { { "id", "abc" } }, item2 = new JObject() { { "id", "def" } };

            await table.InsertAsync(item1);
            await table.InsertAsync(item2);

            Assert.AreEqual(hijack.Requests.Count, 0);

            // create a new service to test that operations are loaded from store
            hijack = new TestHttpHandler();
            hijack.AddResponseContent("{\"id\":\"abc\",\"String\":\"Hey\"}");
            hijack.AddResponseContent("{\"id\":\"def\",\"String\":\"What\"}");

            service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            Assert.AreEqual(hijack.Requests.Count, 0);
            await service.SyncContext.PushAsync();
            Assert.AreEqual(hijack.Requests.Count, 2);

            Assert.AreEqual(hijack.RequestContents[0], item1.ToString(Formatting.None));
            Assert.AreEqual(hijack.Requests[0].Headers.GetValues("X-ZUMO-FEATURES").First(), "TU,OL");
            Assert.AreEqual(hijack.RequestContents[1], item2.ToString(Formatting.None));
            Assert.AreEqual(hijack.Requests[1].Headers.GetValues("X-ZUMO-FEATURES").First(), "TU,OL");

            // create yet another service to make sure the old items were purged from queue
            hijack = new TestHttpHandler();
            hijack.AddResponseContent("{\"id\":\"abc\",\"String\":\"Hey\"}");
            service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());
            Assert.AreEqual(hijack.Requests.Count, 0);
            await service.SyncContext.PushAsync();
            Assert.AreEqual(hijack.Requests.Count, 0);
        }

        [AsyncTestMethod]
        public async Task PushAsync_FeatureHeaderPresent()
        {
            var hijack = new TestHttpHandler();
            hijack.AddResponseContent("{\"id\":\"abc\",\"String\":\"Hey\"}");

            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            var store = new MobileServiceLocalStoreMock();
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable table = service.GetSyncTable("someTable");
            JObject item1 = new JObject() { { "id", "abc" } };
            await table.InsertAsync(item1);
            await service.SyncContext.PushAsync();

            Assert.AreEqual(hijack.Requests[0].Headers.GetValues("X-ZUMO-FEATURES").First(), "TU,OL");
        }

        [AsyncTestMethod]
        public async Task PushAsync_FeatureHeaderPresentWhenRehydrated()
        {
            var hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            var store = new MobileServiceLocalStoreMock();
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable table = service.GetSyncTable("someTable");
            JObject item1 = new JObject() { { "id", "abc" } };
            await table.InsertAsync(item1);

            // create a new service to test that operations are loaded from store
            hijack = new TestHttpHandler();
            hijack.AddResponseContent("{\"id\":\"abc\",\"String\":\"Hey\"}");

            service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());
            await service.SyncContext.PushAsync();

            Assert.AreEqual(hijack.Requests[0].Headers.GetValues("X-ZUMO-FEATURES").First(), "TU,OL");
        }

        [AsyncTestMethod]
        public async Task PushAsync_ReplaysStoredErrors_IfTheyAreInStore()
        {
            var error = new MobileServiceTableOperationError("abc",
                                                            1,
                                                            MobileServiceTableOperationKind.Update,
                                                            HttpStatusCode.PreconditionFailed,
                                                            "test",
                                                            new JObject(),
                                                            "{}",
                                                            new JObject());
            var store = new MobileServiceLocalStoreMock();
            await store.UpsertAsync(MobileServiceLocalSystemTables.SyncErrors, error.Serialize(), fromServer: false);

            var hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            var ex = await ThrowsAsync<MobileServicePushFailedException>(service.SyncContext.PushAsync);
        }

        [AsyncTestMethod]
        public async Task PushAsync_Succeeds_WhenDeleteReturnsNotFound()
        {
            var hijack = new TestHttpHandler();
            hijack.Responses.Add(new HttpResponseMessage(HttpStatusCode.NotFound));

            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), new MobileServiceSyncHandlerMock());

            IMobileServiceSyncTable table = service.GetSyncTable("someTable");

            await table.DeleteAsync(new JObject() { { "id", "abc" }, { "__version", "Wow" } });

            await service.SyncContext.PushAsync();
        }

        [AsyncTestMethod]
        public async Task PushAsync_DoesNotRunHandler_WhenTableTypeIsNotTable()
        {
            var hijack = new TestHttpHandler();
            hijack.AddResponseContent("{\"id\":\"abc\",\"__version\":\"Hey\"}");

            bool invoked = false;
            var handler = new MobileServiceSyncHandlerMock();
            handler.TableOperationAction = op =>
            {
                invoked = true;
                throw new InvalidOperationException();
            };

            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), handler);

            IMobileServiceSyncTable table = service.GetSyncTable("someTable");

            await table.InsertAsync(new JObject() { { "id", "abc" }, { "__version", "Wow" } });

            await (service.SyncContext as MobileServiceSyncContext).PushAsync(CancellationToken.None, (MobileServiceTableKind)1);

            Assert.IsFalse(invoked);
        }

        [AsyncTestMethod]
        public async Task PushAsync_InvokesHandler_WhenTableTypeIsTable()
        {
            var hijack = new TestHttpHandler();
            hijack.Responses.Add(new HttpResponseMessage(HttpStatusCode.InternalServerError));

            bool invoked = false;
            var handler = new MobileServiceSyncHandlerMock();
            handler.TableOperationAction = op =>
            {
                invoked = true;
                return Task.FromResult(JObject.Parse("{\"id\":\"abc\",\"__version\":\"Hey\"}"));
            };

            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), handler);

            IMobileServiceSyncTable table = service.GetSyncTable("someTable");

            await table.InsertAsync(new JObject() { { "id", "abc" }, { "__version", "Wow" } });

            await (service.SyncContext as MobileServiceSyncContext).PushAsync(CancellationToken.None, MobileServiceTableKind.Table);

            Assert.IsTrue(invoked);
        }

        [AsyncTestMethod]
        public async Task PushAsync_Succeeds_WithClientWinsPolicy()
        {
            var hijack = new TestHttpHandler();
            hijack.Responses.Add(new HttpResponseMessage(HttpStatusCode.PreconditionFailed)
            {
                Content = new StringContent("{\"id\":\"abc\",\"__version\":\"Hey\"}")
            });
            hijack.AddResponseContent(@"{""id"": ""abc""}");

            var handler = new MobileServiceSyncHandlerMock();
            handler.TableOperationAction = async op =>
            {
                for (int i = 0; i < 2; i++)
                {
                    try
                    {
                        return await op.ExecuteAsync();
                    }
                    catch (MobileServicePreconditionFailedException ex)
                    {
                        op.Item[MobileServiceSystemColumns.Version] = ex.Value[MobileServiceSystemColumns.Version];
                    }
                }
                return null;
            };
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), handler);

            IMobileServiceSyncTable table = service.GetSyncTable("someTable");

            await table.UpdateAsync(new JObject() { { "id", "abc" }, { "__version", "Wow" } });

            await service.SyncContext.PushAsync();
        }

        class LocalStoreWithDelay : MobileServiceLocalStoreMock
        {
            private int _lookupDelayInMilliseconds = 0;

            public override async Task<JObject> LookupAsync(string tableName, string id)
            {
                if (0 != _lookupDelayInMilliseconds)
                {
                    // releases this thread and causes this lookup to take longer
                    await Task.Delay(_lookupDelayInMilliseconds);
                }

                return await base.LookupAsync(tableName, id);
            }

            /// <summary>
            /// Tells this mock to yield and to delay for the given number of milliseconds before proceeding with the LookupAsync().
            /// </summary>
            /// <param name="delayInMilliseconds">The delay in milliseconds.</param>
            internal void SetLookupDelay(int delayInMilliseconds)
            {
                _lookupDelayInMilliseconds = delayInMilliseconds;
            }
        }

        [AsyncTestMethod]
        public async Task PushAsync_Succeeds_WithPendingOperations_AndOpQueueIsConsistent()
        {
            // Essentially async ManualResetEvents
            SemaphoreSlim untilPendingOpsCreated = new SemaphoreSlim(0, 1);
            SemaphoreSlim untilAboutToExecuteOp = new SemaphoreSlim(0, 1);

            int pushState = 0;

            var handler = new MobileServiceSyncHandlerMock();

            handler.TableOperationAction = async op =>
            {
                try
                {
                    untilAboutToExecuteOp.Release();
                    await untilPendingOpsCreated.WaitAsync();

                    JObject result = await op.ExecuteAsync();

                    if (0 == pushState)
                    {
                        Assert.AreEqual(MobileServiceTableOperationKind.Insert, op.Kind);
                        Assert.AreEqual(0, op.Item.Value<int>("value"));
                    }
                    else
                    {
                        Assert.AreEqual(MobileServiceTableOperationKind.Update, op.Kind);
                        Assert.AreEqual(2, op.Item.Value<int>("value")); // We shouldn't see the value == 1, since it should have been collapsed
                    }

                    // We don't care what the server actually returned, as long as there was no exception raised in our Push logic
                    return result;
                }
                catch (Exception ex)
                {
                    Assert.Fail("Things are bad: " + ex.Message);
                }

                return null;
            };

            var hijack = new TestHttpHandler();

            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            LocalStoreWithDelay mockLocalStore = new LocalStoreWithDelay();
            await service.SyncContext.InitializeAsync(mockLocalStore, handler);

            JObject item = null;

            // Add the initial operation and perform a push
            IMobileServiceSyncTable table = service.GetSyncTable("someTable");

            string responseContent = @"{ ""id"": ""abc"", ""value"": ""0"", ""__version"": ""v0"" }"; // Whatever is fine, since we won't use it or look at it

            // Do this Insert/Push/Update+Update/Push cycle several times fast to try to hit any race conditions that would cause an error
            for (int id = 0; id < 10; id++)
            {
                hijack.SetResponseContent(responseContent);
                string idStr = "id" + id; // Generate a new Id each time in case the mock objects ever care that we insert an item that already exists

                // The Operations and PushAction don't necessarily clone the JObject, so we need a fresh one for each operation or else we'll change
                // the in-memory representation of the JObject stored in all operations, as well as in the "batch" the PushAction owns. This is problematic.
                item = new JObject() { { "id", idStr }, { "value", 0 } };
                await table.InsertAsync(item);

                Task pushComplete = service.SyncContext.PushAsync();

                // Make sure the PushAction has actually called into our SyncHandler, otherwise the two UpdateOperations could collapse onto it, and
                // there won't necessarily even be a second PushAction
                await untilAboutToExecuteOp.WaitAsync();

                // Add some more operations while that push is in flight. Since these operations affect the same item in someTable, the operations
                // will be stuck awaiting the PushAction since it locks on the row.
                item = new JObject() { { "id", idStr }, { "value", 1 } };
                Task updateOnce = table.UpdateAsync(item);

                item = new JObject() { { "id", idStr }, { "value", 2 } };
                Task updateTwice = table.UpdateAsync(item);

                // Before we let the push finish, let's inject a delay that will cause it to take a long time deleting the operation from the queue.
                // This will give the other operations, if there's an unaddressed race condition, a chance to wreak havoc on the op queue.
                mockLocalStore.SetLookupDelay(500);

                // Let the first push finish
                untilPendingOpsCreated.Release();
                await pushComplete;

                mockLocalStore.SetLookupDelay(0);

                await updateOnce;
                await updateTwice;

                // Push again, but now the operation condensed from the two updates should be executed remotely
                pushState = (pushState + 1) % 2;
                hijack.SetResponseContent(responseContent);
                pushComplete = service.SyncContext.PushAsync();
                await untilAboutToExecuteOp.WaitAsync(); // not strictly necessary other than to keep the semaphore count at 0
                untilPendingOpsCreated.Release();

                await pushComplete;
                pushState = (pushState + 1) % 2;
            }
        }

        [AsyncTestMethod]
        public async Task CancelAndUpdateItemAsync_UpsertsTheItemInLocalStore_AndDeletesTheOperationAndError()
        {
            var client = new MobileServiceClient("http://www.test.com");
            var store = new MobileServiceLocalStoreMock();
            var context = new MobileServiceSyncContext(client);
            await context.InitializeAsync(store);

            string operationId = "abc";
            string itemId = "def";
            string tableName = "test";


            store.TableMap[MobileServiceLocalSystemTables.SyncErrors] = new Dictionary<string, JObject>() { { operationId, new JObject() } };
            store.TableMap[MobileServiceLocalSystemTables.OperationQueue].Add(operationId, new JObject());

            // operation exists before cancel
            Assert.IsNotNull(await store.LookupAsync(MobileServiceLocalSystemTables.OperationQueue, operationId));
            // item doesn't exist before upsert
            Assert.IsNull(await store.LookupAsync(tableName, itemId));

            var error = new MobileServiceTableOperationError(operationId,
                                                             0,
                                                             MobileServiceTableOperationKind.Update,
                                                             HttpStatusCode.Conflict,
                                                             tableName,
                                                             item: new JObject() { { "id", itemId } },
                                                             rawResult: "{}",
                                                             result: new JObject());

            var item = new JObject() { { "id", itemId }, { "name", "unknown" } };
            await context.CancelAndUpdateItemAsync(error, item);

            // operation is deleted
            Assert.IsNull(await store.LookupAsync(MobileServiceLocalSystemTables.OperationQueue, operationId));
            // error is deleted
            Assert.IsNull(await store.LookupAsync(MobileServiceLocalSystemTables.SyncErrors, operationId));

            JObject upserted = await store.LookupAsync(tableName, itemId);
            // item is upserted
            Assert.IsNotNull(upserted);
            Assert.AreEqual(item, upserted);
        }

        [AsyncTestMethod]
        public async Task CancelAndDiscardItemAsync_DeletesTheItemInLocalStore_AndDeletesTheOperationAndError()
        {
            var client = new MobileServiceClient("http://www.test.com");
            var store = new MobileServiceLocalStoreMock();
            var context = new MobileServiceSyncContext(client);
            await context.InitializeAsync(store);

            string operationId = "abc";
            string itemId = "def";
            string tableName = "test";

            store.TableMap[MobileServiceLocalSystemTables.SyncErrors] = new Dictionary<string, JObject>() { { operationId, new JObject() } };
            store.TableMap[MobileServiceLocalSystemTables.OperationQueue].Add(operationId, new JObject());
            store.TableMap.Add(tableName, new Dictionary<string, JObject>() { { itemId, new JObject() } });

            // operation exists before cancel
            Assert.IsNotNull(await store.LookupAsync(MobileServiceLocalSystemTables.OperationQueue, operationId));
            // item exists before upsert
            Assert.IsNotNull(await store.LookupAsync(tableName, itemId));

            var error = new MobileServiceTableOperationError(operationId,
                                                             0,
                                                             MobileServiceTableOperationKind.Update,
                                                             HttpStatusCode.Conflict,
                                                             tableName,
                                                             item: new JObject() { { "id", itemId } },
                                                             rawResult: "{}",
                                                             result: new JObject());

            await context.CancelAndDiscardItemAsync(error);

            // operation is deleted
            Assert.IsNull(await store.LookupAsync(MobileServiceLocalSystemTables.OperationQueue, operationId));
            // error is deleted
            Assert.IsNull(await store.LookupAsync(MobileServiceLocalSystemTables.SyncErrors, operationId));

            // item is upserted
            Assert.IsNull(await store.LookupAsync(tableName, itemId));
        }

        [AsyncTestMethod]
        public async Task CancelAndUpdateItemAsync_Throws_IfOperationDoesNotExist()
        {
            await TestOperationModifiedException(false, (error, context) => context.CancelAndUpdateItemAsync(error, new JObject()));
        }

        [AsyncTestMethod]
        public async Task CancelAndDiscardItemAsync_Throws_IfOperationDoesNotExist()
        {
            await TestOperationModifiedException(false, (error, context) => context.CancelAndDiscardItemAsync(error));
        }


        [AsyncTestMethod]
        public async Task CancelAndUpdateItemAsync_Throws_IfOperationIsModified()
        {
            await TestOperationModifiedException(true, (error, context) => context.CancelAndUpdateItemAsync(error, new JObject()));
        }

        [AsyncTestMethod]
        public async Task CancelAndDiscardItemAsync_Throws_IfOperationIsModified()
        {
            await TestOperationModifiedException(true, (error, context) => context.CancelAndDiscardItemAsync(error));
        }

        private async Task TestOperationModifiedException(bool operationExists, Func<MobileServiceTableOperationError, MobileServiceSyncContext, Task> action)
        {
            var client = new MobileServiceClient("http://www.test.com");
            var store = new MobileServiceLocalStoreMock();
            var context = new MobileServiceSyncContext(client);
            await context.InitializeAsync(store);

            string operationId = "abc";
            string itemId = "def";
            string tableName = "test";

            if (operationExists)
            {
                store.TableMap[MobileServiceLocalSystemTables.OperationQueue].Add(operationId, new JObject() { { "version", 3 } });
            }
            else
            {
                // operation exists before cancel
                Assert.IsNull(await store.LookupAsync(MobileServiceLocalSystemTables.OperationQueue, operationId));
            }

            var error = new MobileServiceTableOperationError(operationId,
                                                             1,
                                                             MobileServiceTableOperationKind.Update,
                                                             HttpStatusCode.Conflict,
                                                             tableName,
                                                             item: new JObject() { { "id", itemId } },
                                                             rawResult: "{}",
                                                             result: new JObject());

            var ex = await ThrowsAsync<InvalidOperationException>(() => action(error, context));

            Assert.AreEqual(ex.Message, "The operation has been updated and cannot be cancelled.");
        }
    }
}
