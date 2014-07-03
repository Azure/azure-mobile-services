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
    public interface IPushTestUtility
    {
        string GetPushHandle();

        string GetUpdatedPushHandle();        

        Registration GetTemplateRegistrationForToast();        

        void ValidateTemplateRegistration(Registration registration);        

        void ValidateTemplateRegistrationBeforeRegister(Registration registration);        

        void ValidateTemplateRegistrationAfterRegister(Registration registration, string zumoInstallationId);        

        Registration GetNewNativeRegistration(string deviceId, IEnumerable<string> tags);        

        Registration GetNewTemplateRegistration(string deviceId, string bodyTemplate, string templateName);

        string GetListNativeRegistrationResponse();

        string GetListTemplateRegistrationResponse();

        string GetListMixedRegistrationResponse();
    }
}
