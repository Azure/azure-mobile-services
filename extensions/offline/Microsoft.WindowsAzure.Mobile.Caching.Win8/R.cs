using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace Microsoft.WindowsAzure.MobileServices.Caching
{
    internal static class R
    {
        private static ResourceLoader resourceLoader;
        internal static ResourceLoader ResourceLoader
        {
            get
            {
                if (object.ReferenceEquals(resourceLoader, null))
                {
                    resourceLoader = new ResourceLoader("Microsoft.WindowsAzure.Mobile.Caching/Resources");
                }
                return resourceLoader;
            }
        }

        public static string GetString(string key)
        {
            return ResourceLoader.GetString(key);
        }
    }
}
