// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Newtonsoft.Json;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    public class DerivedDuplicateKeyType : PocoType
    {
        [JsonProperty(PropertyName = "PublicProperty")]
        public string OtherThanPublicProperty { get; set; }
    }
}
