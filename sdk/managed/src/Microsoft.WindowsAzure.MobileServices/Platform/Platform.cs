// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Provides access to platform-specific framework API's.
    /// </summary>
    internal static class Platform
    {
        /// <summary>
        /// The string value to use for the operating system name, arch, or version if
        /// the value is unknown.
        /// </summary>
        public const string UnknownValueString = "--";

        private static IPlatform current;

        /// <summary>
        /// Name of the assembly containing the Class with the <see cref="Platform.PlatformTypeFullName"/> name.
        /// </summary>
        public static string PlatformAssemblyName = "Microsoft.WindowsAzure.Mobile.Ext";

        /// <summary>
        /// Name of the type implementing <see cref="IPlatform"/>.
        /// </summary>
        public static string PlatformTypeFullName = "Microsoft.WindowsAzure.MobileServices.CurrentPlatform";

        /// <summary>
        /// Gets the current platform. If none is loaded yet, accessing this property triggers platform resolution.
        /// </summary>
        public static IPlatform Instance
        {
            get
            {
                // create if not yet created
                if (current == null)
                {
                    //assume the platform assembly has the same key, same version and same culture
                    // as the assembly where the IPlatformProvider interface lives.
                    var provider = typeof(IPlatform);
                    var asm = new AssemblyName(provider.Assembly.FullName);
                    //change name to the specified name
                    asm.Name = PlatformAssemblyName;
                    var name = PlatformTypeFullName + ", " + asm.FullName;

                    //look for the type information but do not throw if not found
                    var type = Type.GetType(name, false);
                    if (type != null)
                    {
                        // create type
                        // since we are the only one implementing this interface
                        // this cast is safe.
                        current = (IPlatform)Activator.CreateInstance(type);
                    }
                    else
                    {
                        // throw
                        ThrowForMissingPlatformAssembly();
                    }
                }

                return current;
            }

            // keep this public so we can set a Platform for unit testing.
            set
            {
                current = value;
            }
        }

        /// <summary>
        /// Method to throw an exception in case no Platform assembly could be found.
        /// </summary>
        private static void ThrowForMissingPlatformAssembly()
        {
            AssemblyName portable = new AssemblyName(Assembly.GetExecutingAssembly().FullName);

            throw new InvalidOperationException(
                string.Format(CultureInfo.InvariantCulture,
                              Resources.Platform_AssemblyNotFound,
                            portable.Name,
                            PlatformAssemblyName));
        }
    }
}
