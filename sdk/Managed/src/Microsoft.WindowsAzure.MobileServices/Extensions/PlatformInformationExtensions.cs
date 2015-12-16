using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices
{
    public static class PlatformInformationExtensions
    {
        /// <summary>
        /// Returns the version set in the <see cref="AssemblyFileVersionAttribute"/> set in the assembly
        /// containing this <see cref="IPlatformInformation"/> implementation.
        /// </summary>
        /// <param name="platformInformation"></param>
        /// <returns>The version set in the </returns>
        internal static string GetVersionFromAssemblyFileVersion(this IPlatformInformation platformInformation)
        {
            var attribute = platformInformation.GetType().GetTypeInfo().Assembly
                .GetCustomAttributes(typeof(AssemblyFileVersionAttribute)).FirstOrDefault() as AssemblyFileVersionAttribute;

            return attribute != null ? attribute.Version : string.Empty;
        }
    }
}
