using System.Collections.Generic;
using ZumoE2ETestApp.Framework;
#if WINDOWS_PHONE
using ZumoE2ETestAppWP8.Tests;
#endif

namespace ZumoE2ETestApp.Tests
{
    public static class TestStore
    {
        public static List<ZumoTestGroup> CreateTests()
        {
            List<ZumoTestGroup> result = new List<ZumoTestGroup>
            {
                ZumoRoundTripTests.CreateTests(),
                ZumoQueryTests.CreateTests(),
                ZumoCUDTests.CreateTests(),
                ZumoMiscTests.CreateTests(),
#if WINDOWS_PHONE
                ZumoWP8PushTests.CreateTests(),
#endif
#if NETFX_CORE                
                ZumoPushTests.CreateTests(),
#endif
#if !NET45
                ZumoLoginTests.CreateTests(),
#endif
            };

            return result;
        }
    }
}
