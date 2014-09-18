using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoTouch.UIKit;

namespace Microsoft.WindowsAzure.MobileServices
{
    internal class PlatformInformation : IPlatformInformation
    {
        /// <summary>
        /// A singleton instance of the <see cref="PlatformInformation"/>.
        /// </summary>
        private static IPlatformInformation instance = new PlatformInformation();

        /// <summary>
        /// A singleton instance of the <see cref="PlatformInformation"/>.
        /// </summary>
        public static IPlatformInformation Instance
        {
            get
            {
                return instance;
            }
        }

        public string OperatingSystemArchitecture
        {
            get { return PlatformID.MacOSX.ToString(); }
        }

        public string OperatingSystemName
        {
            get { return "iOS"; }
        }

        public string OperatingSystemVersion
        {
            get { return UIDevice.CurrentDevice.SystemVersion; }
        }

        public bool IsEmulator
        {
            get
            {
                return (UIDevice.CurrentDevice.Model.ToLower().Contains ("simulator"));
            }
        }
    }
}