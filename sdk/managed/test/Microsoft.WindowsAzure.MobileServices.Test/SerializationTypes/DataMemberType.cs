// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    public class DataMemberType
    {
        public long Id { get; set; }

        [DataMember]
        public string PublicProperty { get; set; }
    }
}
