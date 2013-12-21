// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace Microsoft.WindowsAzure.MobileServices
{
    [JsonObject]
    public sealed class WnsHeaderCollection : Dictionary<string, string>
    {
        public WnsHeaderCollection()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }
    }
}
