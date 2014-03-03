using System.Collections.Generic;
using ZumoE2ETestApp.Framework;

namespace ZumoE2ETestApp.Tests
{
    internal static class ZumoSetupTests
    {
        public static ZumoTestGroup CreateTests()
        {
            ZumoTestGroup result = new ZumoTestGroup("Tests Setup");
            result.AddTest(CreateSetupTest());

            return result;
        }

        private static ZumoTest CreateSetupTest()
        {
            return new ZumoTest("Identify enabled runtime features", async delegate(ZumoTest test)
                {
                    // Start clean.
                    Dictionary<string, bool> runtimeFeatures = await ZumoTestGlobals.Instance.GetRuntimeFeatures(test);
                    if (runtimeFeatures.Count > 0)
                    {
                        test.AddLog("Runtime: {0}", ZumoTestGlobals.Instance.RuntimeType);
                        test.AddLog("Version: {0}", ZumoTestGlobals.Instance.RuntimeVersion);
                        return true;
                    }
                    else
                    {
                        test.AddLog("Could not load the runtime information");
                        return false;
                    }
                });
        }
    }
}
