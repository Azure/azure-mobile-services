using System;
using System.Linq;
using Android.OS;

namespace Microsoft.WindowsAzure.MobileServices
{
    class PlatformInformation : IPlatformInformation
    {
        private static IPlatformInformation instance = new PlatformInformation();

        public static IPlatformInformation Instance
        {
            get { return instance; }
        }

        public string OperatingSystemArchitecture
        {
            get { return System.Environment.OSVersion.Platform.ToString(); }
        }

        public string OperatingSystemName
        {
            get { return "Android"; }
        }

        public string OperatingSystemVersion
        {
            get { return Build.VERSION.Release; }
        }

        public bool IsEmulator
        {
            get { return Build.Brand.Equals ("generic", StringComparison.OrdinalIgnoreCase); }
        }
    }
}