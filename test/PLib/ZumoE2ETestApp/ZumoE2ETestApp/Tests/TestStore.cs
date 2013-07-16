// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using ZumoE2ETestApp.Framework;
#if WINDOWS_PHONE
using ZumoE2ETestAppWP8.Tests;
#endif

namespace ZumoE2ETestApp.Tests
{
    public static class TestStore
    {
        public const string AllTestsGroupName = "All tests";

        public static List<ZumoTestGroup> CreateTestGroups()
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
                ZumoCustomApiTests.CreateTests(),
            };

            result.Add(CreateGroupWithAllTests(result));

            return result;
        }

        private static ZumoTestGroup CreateGroupWithAllTests(List<ZumoTestGroup> testGroups)
        {
            ZumoTestGroup result = new ZumoTestGroup(AllTestsGroupName);
            foreach (var group in testGroups)
            {
                result.AddTest(ZumoTestCommon.CreateSeparator("Start of group: " + group.Name));
                foreach (var test in group.AllTests)
                {
                    if (test.CanRunUnattended)
                    {
                        result.AddTest(test);
                    }
                }

                result.AddTest(ZumoTestCommon.CreateSeparator("------------------"));
            }

            return result;
        }
    }
}
