// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Collection storing any additional MpnsHeaders
    /// </summary>
    [JsonObject]
    public sealed class MpnsHeaderCollection : Dictionary<string, string>
    {
        /// <summary>
        /// Create a MpnsHeaderCollection
        /// </summary>
        public MpnsHeaderCollection()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }
    }
}
