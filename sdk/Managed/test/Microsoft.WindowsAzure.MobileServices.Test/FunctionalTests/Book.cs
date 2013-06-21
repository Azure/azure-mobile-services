// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using Newtonsoft.Json;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    [DataTable("books")]
    public class Book
    {
        public int Id { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string BookType { get; set; }

        [JsonProperty(PropertyName = "pub_id")]
        public int PublicationId { get; set; }

        [JsonProperty(PropertyName = "price")]
        public double Price { get; set; }

        [JsonProperty(PropertyName = "advance")]
        public double Advance { get; set; }

        [JsonProperty(PropertyName = "royalty")]
        public double Royalty { get; set; }

        [JsonProperty(PropertyName = "ytd_sales")]
        public double YearToDateSales { get; set; }

        [JsonProperty(PropertyName = "pubdate")]
        public DateTime PublicationDate { get; set; }

        [JsonProperty(PropertyName = "notes")]
        public string Notes { get; set; }
    }
}
