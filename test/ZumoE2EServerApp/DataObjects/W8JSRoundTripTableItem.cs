// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.Mobile.Service;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZumoE2EServerApp.DataObjects
{
    public class W8JSRoundTripTableItemForDB
    {
        public string W8JSRoundTripTableItemForDBId { get; set; }

        public string String1 { get; set; }

        public DateTimeOffset? Date1 { get; set; }

        public bool? Bool1 { get; set; }

        public double? Number { get; set; }

        public string ComplexTypeSerialized { get; set; }

        public byte[] Version { get; set; }

        public DateTimeOffset? CreatedAt { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }

        public bool Deleted { get; set; }
    }

    public class W8JSRoundTripTableItem : EntityData
    {
        public string String1 { get; set; }

        public DateTimeOffset? Date1 { get; set; }

        public bool? Bool1 { get; set; }

        public double? Number { get; set; }

        public string[] ComplexType { get; set; }
    }
}