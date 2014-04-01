// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
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
        public async Task PushAsync_ExecutesThePendingOperations_InOrder()
        {
            var hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            var store = new MobileServiceLocalStoreMock();
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable table = service.GetSyncTable("someTable");
            
            JObject item1 = new JObject(){{"id", "abc"}}, item2 = new JObject(){{"id", "def"}};

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
            Assert.AreEqual(hijack.Requests[0].Headers.GetValues("X-ZUMO-FEATURES").First(), "OL");
            Assert.AreEqual(hijack.RequestContents[1], item2.ToString(Formatting.None));
            Assert.AreEqual(hijack.Requests[1].Headers.GetValues("X-ZUMO-FEATURES").First(), "OL");

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

            Assert.AreEqual(hijack.Requests[0].Headers.GetValues("X-ZUMO-FEATURES").First(), "OL");
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

            Assert.AreEqual(hijack.Requests[0].Headers.GetValues("X-ZUMO-FEATURES").First(), "OL");
        }

        [AsyncTestMethod]
        public async Task PushAsync_ReplaysStoredErrors_IfTheyAreInStore()
        {
            var hijack = new TestHttpHandler();
            hijack.Responses.Add(new HttpResponseMessage(HttpStatusCode.Conflict) { Content = new StringContent("[{\"id\":\"abc\",\"String\":\"Hey\"}]") });

            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), new MobileServiceSyncHandler());

            IMobileServiceSyncTable table = service.GetSyncTable("someTable");

            await table.InsertAsync(new JObject() { { "id", "abc" } });

            Assert.AreEqual(hijack.Requests.Count, 0);

            var ex = await ThrowsAsync<MobileServicePushFailedException>(service.SyncContext.PushAsync);
        }
    }
}
