// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// An interface for platform-specific assemblies to provide utility functions
    /// regarding Push capabilities.
    /// </summary>
    interface IPushUtility
    {
        /// <summary>
        /// Return a new, strongly typed native registration instance
        /// </summary>        
        /// <returns>
        /// An object to JSON deserialize into
        /// </returns>
        RegistrationBase GetNewNativeRegistration();

        /// <summary>
        /// Return a new, strongly typed template registration instance
        /// </summary>        
        /// <returns>
        /// An object to JSON deserialize into
        /// </returns>
        RegistrationBase GetNewTemplateRegistration();

        /// <summary>
        /// Return the string describing the notification platform
        /// </summary>        
        /// <returns>
        /// String describing notfication platform. Examples: gcm, apns, wns
        /// </returns>
        string GetPlatform();
    }
}
