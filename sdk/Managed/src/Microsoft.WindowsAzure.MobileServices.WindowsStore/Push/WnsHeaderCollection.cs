// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Collection storing any additional WnsHeaders
    /// </summary>
    [JsonObject]
    public sealed class WnsHeaderCollection : Dictionary<string, string>
    {
        /// <summary>
        /// Create a WnsHeaderCollection
        /// </summary>
        public WnsHeaderCollection()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }
    }
}
