// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// interface for local storage, for test purpose
    /// </summary>
    internal interface IStaticLocalStorage
    {
        IDictionary<string, object> Settings { get; }
        string KeyNameForVersion { get; set; }
        string KeyNameForChannelUri { get; set; }
        string KeyNameForRegistrations { get; set; }
    }

}
