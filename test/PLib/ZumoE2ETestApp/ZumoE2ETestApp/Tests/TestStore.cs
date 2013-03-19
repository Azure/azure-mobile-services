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
                ZumoLoginTests.CreateTests(),
                ZumoMiscTests.CreateTests(),
                ZumoWP8PushTests.CreateTests(),
            };

            return result;
        }
    }
}
