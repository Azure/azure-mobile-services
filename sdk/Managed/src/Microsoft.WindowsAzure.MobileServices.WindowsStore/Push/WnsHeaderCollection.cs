// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.MobileServices
{
    [CollectionDataContract(Name = "WnsHeaders", Namespace = "http://schemas.microsoft.com/netservices/2010/10/servicebus/connect", ItemName = "WnsHeader", KeyName = "Header", ValueName = "Value")]
    public sealed class WnsHeaderCollection : Dictionary<string, string>
    {
        public WnsHeaderCollection()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }
    }
}
