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
        /// Return a new, strongly typed native registration instance
        /// </summary>        
        /// <returns>
        /// An object to JSON deserialize into
        /// </returns>
        RegistrationBase GetNewNativeRegistration();

        /// <summary>
        /// Return a new, strongly typed native registration instance
        /// </summary>
        /// <param name="deviceId">The unique device Id for the registration</param>
        /// <param name="tags">The template name</param>
        /// <returns></returns>
        RegistrationBase GetNewNativeRegistration(string deviceId, IEnumerable<string> tags);

        /// <summary>
        /// Return a new, strongly typed template registration instance
        /// </summary>        
        /// <returns>
        /// An object to JSON deserialize into
        /// </returns>
        RegistrationBase GetNewTemplateRegistration();

        /// <summary>
        /// Return a new, strongly typed template registration instance
        /// </summary>
        /// <param name="deviceId">The unique device Id for the registration</param>
        /// <param name="bodyTemplate">The template body in string format</param>
        /// <param name="templateName">The template name</param>
        /// <returns></returns>
        RegistrationBase GetNewTemplateRegistration(string deviceId, string bodyTemplate, string templateName);
        

        /// <summary>
        /// Return the string describing the notification platform
        /// </summary>        
        /// <returns>
        /// String describing notfication platform. Examples: gcm, apns, wns
        /// </returns>
        string GetPlatform();
    }
}
