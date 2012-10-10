// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;

namespace Microsoft.Azure.Zumo.WindowsPhone8.CSharp.Test
{
    [DataTable(Name = "books")]
    public class Book
    {
        public int Id { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "type")]
        public string BookType { get; set; }

        [DataMember(Name = "pub_id")]
        public int PublicationId { get; set; }

        [DataMember(Name = "price")]
        public double Price { get; set; }

        [DataMember(Name = "advance")]
        public double Advance { get; set; }

        [DataMember(Name = "royalty")]
        public double Royalty { get; set; }

        [DataMember(Name = "ytd_sales")]
        public double YearToDateSales { get; set; }

        [DataMember(Name = "pubdate")]
        public DateTime PublicationDate { get; set; }

        [DataMember(Name = "notes")]
        public string Notes { get; set; }
    }
}
