// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Microsoft.WindowsAzure.MobileServices.TestFramework;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    [Tag("unit")]
    [Tag("table")]
    [Tag("offline")]
    public class MobileServiceSyncTableTests : TestBase
    {        
        [AsyncTestMethod]
        public async Task InsertAsync_GeneratesId_WhenNull()
        {
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), new MobileServiceSyncHandler());

            var item = new JObject();
            JObject inserted = await service.GetSyncTable("someTable").InsertAsync(item);
            Assert.IsNotNull(inserted.Value<string>("id"), "Expected id member not found.");

            item = new JObject(){{"id", null}};
            inserted = await service.GetSyncTable("someTable").InsertAsync(item);
            Assert.IsNotNull(inserted.Value<string>("id"), "Expected id member not found.");
        }
    }
}
