using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using ZumoE2ETestApp.Framework;
using ZumoE2ETestApp.Tests.Types;
using System.Net;
using System.Net.Http;

namespace ZumoE2ETestApp.Tests
{
    public static class ZumoSetupTests
    {
        public static ZumoTestGroup CreateTests()
        {
            ZumoTestGroup result = new ZumoTestGroup("Test Setup tests");
            result.AddTest(CreateSetupTest());

            return result;
        }

        private static ZumoTest CreateSetupTest()
        {
            return new ZumoTest("Identify enabled runtime features", async delegate(ZumoTest test)
                {
                    var client = ZumoTestGlobals.Instance.Client;
                    JToken apiResult = null;

                    apiResult = await client.InvokeApiAsync("runtimeInfo", HttpMethod.Get, null);
                    var runtimeInfo = apiResult;

                    test.AddLog("Got runtime features:");
                    test.AddLog(runtimeInfo.ToString());
                    return true;
                });
        }
    }
}
