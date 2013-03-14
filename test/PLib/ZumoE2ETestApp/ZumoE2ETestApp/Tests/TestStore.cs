using System.Collections.Generic;
using ZumoE2ETestApp.Framework;

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
#if !WINDOWS_PHONE
                ZumoLoginTests.CreateTests(),
#endif
                ZumoMiscTests.CreateTests(),
#if !WINDOWS_PHONE
                ZumoPushTests.CreateTests(),
#endif
            };

            return result;
        }
    }
}
