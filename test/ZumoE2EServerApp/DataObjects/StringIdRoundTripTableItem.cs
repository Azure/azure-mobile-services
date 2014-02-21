// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.Mobile.Service;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZumoE2EServerApp.DataObjects
{
    public class StringIdRoundTripTableItem : EntityData
    {
        public string Name { get; set; }

        public DateTime? Date { get; set; }

        public DateTime? Date1 { get; set; }

        public bool? Bool { get; set; }

        public int Integer { get; set; }

        public double? Number { get; set; } 

        [NotMapped]
        public string[] Complex
        {
            get
            {
                return (this.ComplexS == null ? null : JsonConvert.DeserializeObject<string[]>(this.ComplexS));
            }
            set
            {
                this.ComplexS = value == null ? null : JsonConvert.SerializeObject(value);
            }
        }

        [JsonIgnore]
        public string ComplexS { get; set; }

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
