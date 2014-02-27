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
        public const string AllTestsUnattendedGroupName = AllTestsGroupName + " (unattended)";

        public static List<ZumoTestGroup> CreateTestGroups()
        {
            List<ZumoTestGroup> result = new List<ZumoTestGroup>
            {
#if !NET45
                ZumoLoginTests.CreateTests(),
#endif
                ZumoCustomApiTests.CreateTests(),
                ZumoRoundTripTests.CreateTests(),
                ZumoQueryTests.CreateTests(),
                ZumoCUDTests.CreateTests(),
                ZumoMiscTests.CreateTests(),
#if WINDOWS_PHONE
                ZumoWP8PushTests.CreateTests(),
#endif
#if NETFX_CORE                
                ZumoPushTests.CreateTests()
#endif
            };

            ZumoTestGroup allTestsUnattendedGroup = CreateGroupWithAllTests(result, true);
            ZumoTestGroup allTestsGroup = CreateGroupWithAllTests(result, false);
            result.Add(allTestsUnattendedGroup);
            result.Add(allTestsGroup);

            return result;
        }

        private static ZumoTestGroup CreateGroupWithAllTests(List<ZumoTestGroup> testGroups, bool unattendedOnly)
        {
            ZumoTestGroup result = new ZumoTestGroup(unattendedOnly ? AllTestsUnattendedGroupName : AllTestsGroupName);
            foreach (var group in testGroups)
            {
                result.AddTest(ZumoTestCommon.CreateSeparator("Start of group: " + group.Name));
                foreach (var test in group.AllTests)
                {
                    if (test.CanRunUnattended || !unattendedOnly)
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
