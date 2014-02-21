// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.Mobile.Service;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZumoE2EServerApp.DataObjects
{
    public class W8JSRoundTripTableItem : EntityData
    {
        public string String1 { get; set; }
        public DateTimeOffset? Date1 { get; set; }
        public bool? Bool1 { get; set; }
        public double? Number { get; set; }

        [NotMapped]
        public string[] ComplexType { get; set; }

        [JsonIgnore]
        public string ComplexTypeS
        {
            get
            {
                return (this.ComplexType == null ? null : JsonConvert.SerializeObject(this.ComplexType));
            }
            set
            {
                this.ComplexType = value == null ? null : JsonConvert.DeserializeObject<string[]>(value);
            }
        }
    }
}