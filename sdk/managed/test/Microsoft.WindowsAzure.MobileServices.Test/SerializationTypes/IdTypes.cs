// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Runtime.Serialization;
using Newtonsoft.Json;
namespace Microsoft.WindowsAzure.MobileServices.Test
{
    public class IDType
    {
        public int ID { get; set; }
    }

    public class idType
    {
        public int id { get; set; }
    }

    public class iDType
    {
        public int iD { get; set; }
    }

    public class MulitpleIdType
    {
        public int Id { get; set; }

        public int id { get; set; }
    }

    public class MissingIdType
    {
        public int NotAnId { get; set; }
    }

    [DataContract]
    public class DataContractMissingIdType
    {
        public int id { get; set; }
    }

    public class IgnoreDataMemberMissingIdType
    {
        [IgnoreDataMember]
        public int id { get; set; }
    }

    public class JsonIgnoreMissingIdType
    {
        [JsonIgnore]
        public int id { get; set; }
    }
}
