// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.Mobile.Service;
using System;

namespace ZumoE2EServerApp.DataObjects
{
    public class StringIdRoundTripTableItemForDB : EntityData
    {
        public string Name { get; set; }

        public DateTime? Date { get; set; }

        public DateTime? Date1 { get; set; }

        public bool? Bool { get; set; }

        public int Integer { get; set; }

        public double? Number { get; set; }

        public string ComplexSerialized { get; set; }

        public string ComplexTypeSerialized { get; set; }
    }

    public class StringIdRoundTripTableItem : EntityData
    {
        public string Name { get; set; }

        public DateTime? Date { get; set; }

        public DateTime? Date1 { get; set; }

        public bool? Bool { get; set; }

        public int Integer { get; set; }

        public double? Number { get; set; }

        public string[] Complex { get; set; }

        public string[] ComplexType { get; set; }
    }
}
