using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Newtonsoft.Json.Linq;
using OfflinePerfCore.Common;
using OfflinePerfCore.Types;

namespace OfflinePerfCore.Setup
{
    public static class TestSetup
    {
        public static async Task<MobileServiceClient> CreateClient(string dbFileName)
        {
            var result = new MobileServiceClient("https://not.used.com", "notused", new SimpleTypeTableMockHandler());
            var store = new MobileServiceSQLiteStore(dbFileName);
            store.DefineTable<SimpleType>();
            await result.SyncContext.InitializeAsync(store, new BypassNetworkSyncHandler());
            return result;
        }
    }
}
