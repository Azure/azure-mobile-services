// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using Newtonsoft.Json.Linq;

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
        
        JObject GetInstallation(string installationId, bool includeTemplates = false, string defaultChannelUri = null);
        
        JObject GetTemplates();
    }
}
