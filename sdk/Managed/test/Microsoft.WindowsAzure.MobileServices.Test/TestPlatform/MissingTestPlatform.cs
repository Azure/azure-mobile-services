using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    class MissingTestPlatform : ITestPlatform
    {
        public IPushTestUtility PushTestUtility
        {
            get { return new MissingPushTestUtility(); }
        }
    }
}
