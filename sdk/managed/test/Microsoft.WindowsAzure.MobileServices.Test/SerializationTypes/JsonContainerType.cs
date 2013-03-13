// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Newtonsoft.Json;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    [JsonObject(Title="NamedJsonContainerType")]
    public class JsonContainerType
    {
        [JsonProperty(PropertyName = "AnotherPublicProperty")]
        public int OtherThanPublicProperty { get; set; }

        public int PublicProperty { get; set; }
    }

    [JsonObject]
    public class UnnamedJsonContainerType
    {
        [JsonProperty(PropertyName = "AnotherPublicProperty")]
        public int OtherThanPublicProperty { get; set; }

        public int PublicProperty { get; set; }
    }
}
