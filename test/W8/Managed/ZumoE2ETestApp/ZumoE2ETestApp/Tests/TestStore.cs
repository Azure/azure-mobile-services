using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            };

            return result;
        }
    }
}
