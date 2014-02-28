using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using ZumoE2ETestApp.Framework;

namespace ZumoE2ETestApp.Tests
{
    public static class ZumoSetupTests
    {
        public static ZumoTestGroup CreateTests()
        {
            ZumoTestGroup result = new ZumoTestGroup("Tests Setup");
            result.AddTest(CreateSetupTest());

            return result;
        }

        public static ZumoTest CreateSetupTest()
        {
            return new ZumoTest("Identify enabled runtime features", async delegate(ZumoTest test)
                {
                    var client = ZumoTestGlobals.Instance.Client;
                    JToken apiResult = await client.InvokeApiAsync("runtimeInfo", HttpMethod.Get, null);
                    try
                    {
                        ZumoTestGlobals.RuntimeFeatures = JsonConvert.DeserializeObject<Dictionary<string, bool>>(JsonConvert.SerializeObject(apiResult["features"]));
                        if (apiResult["runtime"].ToString().Contains("node.js"))
                        {
                            ZumoTestGlobals.NetRuntimeEnabled = false;
                            ZumoTestGlobals.RuntimeFeatures.Add(ZumoTestGlobals.RuntimeFeatureNames.AAD_LOGIN, true);
                            ZumoTestGlobals.RuntimeFeatures.Add(ZumoTestGlobals.RuntimeFeatureNames.SSO_LOGIN, true);
                            ZumoTestGlobals.RuntimeFeatures.Add(ZumoTestGlobals.RuntimeFeatureNames.LIVE_LOGIN, true);
                        }
                        else
                        {
                            ZumoTestGlobals.NetRuntimeEnabled = true;
                            ZumoTestGlobals.RuntimeFeatures.Add(ZumoTestGlobals.RuntimeFeatureNames.AAD_LOGIN, false);
                            ZumoTestGlobals.RuntimeFeatures.Add(ZumoTestGlobals.RuntimeFeatureNames.SSO_LOGIN, false);
                            ZumoTestGlobals.RuntimeFeatures.Add(ZumoTestGlobals.RuntimeFeatureNames.LIVE_LOGIN, false);
                        }
                        return true;
                    }
                    catch (Exception ex)
                    {
                        test.AddLog(ex.Message);
                        return false;
                    }
                });
        }
    }
}
