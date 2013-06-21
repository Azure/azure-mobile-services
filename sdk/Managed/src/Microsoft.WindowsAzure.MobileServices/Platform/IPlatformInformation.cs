// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Provides information about the platform it is running on.
    /// </summary>
    internal interface IPlatformInformation
    {
        /// <summary>
        /// The architecture of the platform.
        /// </summary>
        string OperatingSystemArchitecture { get; }

        /// <summary>
        /// The name of the operating system of the platform.
        /// </summary>
        string OperatingSystemName { get; }

        /// <summary>
        /// The version of the operating system of the platform.
        /// </summary>
        string OperatingSystemVersion { get; }

        /// <summary>
        /// Boolean indicating if the platform is running on an emulator.
        /// </summary>
        bool IsEmulator { get; }
    }
}
