using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfflinePerfCore.Common;

namespace PhoneOfflinePerf
{
    class PlatformInfo : IPlatformInfo
    {
        public long GetAppMemoryUsage()
        {
            return Microsoft.Phone.Info.DeviceStatus.ApplicationCurrentMemoryUsage;
        }

        public string PlatformName
        {
            get { return "Windows Phone 8"; }
        }
    }
}
