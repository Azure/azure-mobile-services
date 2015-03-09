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

        [AsyncTestMethod]
        public async Task PushAsync_FailsOnPendingUpdates_EvenWithNoActualConflict()
        {
            SemaphoreSlim untilPendingOpsCreated = new SemaphoreSlim(0);
            int version = 0;

            JObject item = new JObject() { { "id", "abc" }, { "value", 0 } };
            item[MobileServiceSystemColumns.Version] = "v" + version++; // we start having v0 on our side

            var hijack = new TestHttpHandler();
            string responseContentFormat = @"{{""id"": ""abc"", ""value"": ""0"", ""__version"": ""{0}"" }}";
            string responseContent = string.Format(responseContentFormat, "v" + version); // make the first response be v1
            hijack.SetResponseContent(responseContent);

            var handler = new MobileServiceSyncHandlerMock();

            handler.TableOperationAction = async op =>
            {
                try
                {
                    await untilPendingOpsCreated.WaitAsync();

                    JObject result = await op.ExecuteAsync();
                    string resultVersion = result.Value<string>(MobileServiceSystemColumns.Version);
                    string opVersion = op.Item.Value<string>(MobileServiceSystemColumns.Version);
                    Assert.AreNotEqual(resultVersion, opVersion);
                    Assert.AreEqual(resultVersion, "v" + version);

                    // This will fail on the second push most likely, depending on the race, because the stale InsertOperation will still
                    // have v0 as its version, since it keeps getting shoved back into the operations table.
                    Assert.AreEqual(opVersion, "v" + (version - 1));

                    // Could also assert here that the operation on the second push is an UpdateOperation, which it usually won't be.

                    return result;
                }
                // When running against the actual server, real scenarios where we have pending operations frequently cause false merge conflicts
                // which will generate the PreconditionFailed. This requires client code to manually do all sorts of fixing up to versions, both
                // on the local/remote items on which we're operating, an directly on the remote table and the local store.
                //catch (MobileServicePreconditionFailedException ex)
                catch (Exception ex)
                {
                    Assert.Fail("Things are bad: " + ex.Message);
                }

                return null;
            };

            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), handler);

            // Add the initial operation and perform a push
            IMobileServiceSyncTable table = service.GetSyncTable("someTable");
            await table.InsertAsync(item);

            Task pushComplete = service.SyncContext.PushAsync();

            // Add some more operations while that push is in flight. Since these operations affect the same item in someTable, the operations
            // will be stuck awaiting the PushAction since it locks on the row.
            item["value"] = 1;
            Task updateOnce = table.UpdateAsync(item);

            item["value"] = 2;
            Task updateTwice = table.UpdateAsync(item);

            // Let the first push finish
            untilPendingOpsCreated.Release();
            await pushComplete;
            await updateOnce;
            await updateTwice;

            // One would expect that since the push completed, the server would give us a new version, which is what happens with the
            // real AMS back-end, so let's fake that
            version += 1;
            responseContent = string.Format(responseContentFormat, "v" + version); // make the next response v2
            hijack.SetResponseContent(responseContent);

            // Push again, but now the operation condensed from the two updates will see the item as having an old version, which causes
            // a supposed merge conflict, but really there is none semantically.
            pushComplete = service.SyncContext.PushAsync();
            untilPendingOpsCreated.Release();

            await pushComplete;
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
