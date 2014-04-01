// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// The list of zumo features exposed in the http headers of requests for tracking usages
    /// </summary>
    static class MobileServiceFeatures
    {
        /// <summary>
        /// Feature header value for offline initiated requests
        /// </summary>
        internal const string Offline = "OL";
    }
}
