// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// An interface for platform-specific assemblies to provide utility functions
    /// regarding Push capabilities.
    /// </summary>
    public interface IPushUtility
    {
        /// <summary>
        /// Return the string describing the notification platform
        /// </summary>        
        /// <returns>
        /// String describing notfication platform. Examples: gcm, apns, wns
        /// </returns>
        string GetPlatform();
    }
}