// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace Microsoft.WindowsAzure.MobileServices
{
    [JsonObject]
    public sealed class MpnsHeaderCollection : Dictionary<string, string>
    {
        public MpnsHeaderCollection()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }
    }
}
