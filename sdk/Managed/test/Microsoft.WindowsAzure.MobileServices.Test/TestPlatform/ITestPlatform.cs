// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.ComponentModel;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    /// <summary>
    /// Provides an interface that platform-specific Mobile Services assemblies 
    /// can implement to provide functionality required by the Mobile Services SDK 
    /// that is platform specific.
    /// </summary>
    public interface ITestPlatform
    {
        /// <summary>
        /// Returns a platform-specific implemention of application storage.
        /// </summary>
        IPushTestUtility PushTestUtility { get; }        
    }
}
