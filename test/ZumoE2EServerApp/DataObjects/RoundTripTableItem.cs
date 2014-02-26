// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.Mobile.Service.Tables;
using Newtonsoft.Json;
using System;

namespace ZumoE2EServerApp.DataObjects
{
    public class RoundTripTableItem
    {
        public int RoundTripTableItemId { get; set; }

        public string String1 { get; set; }

        public DateTime? Date1 { get; set; }

        public bool? Bool1 { get; set; }

        // Number types
        public double Double1 { get; set; }

        public long Long1 { get; set; }

        public int? Int1 { get; set; }

        // Enum, with converter
        public EnumType EnumType { get; set; }

        public string ComplexType1Serialized { get; set; }

        public string ComplexType2Serialized { get; set; }

        public byte[] Version { get; set; }

        public DateTimeOffset? CreatedAt { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }

        public bool Deleted { get; set; }
    }

    public class RoundTripTableItemFakeStringId : ITableData
    {
        public string Id { get { return this.IntId.ToString(); } set { this.IntId = int.Parse(value); } }

        [JsonIgnore]
        public int IntId { get; set; }

        public string String1 { get; set; }

        public DateTime? Date1 { get; set; }

        public bool? Bool1 { get; set; }

        // Number types
        public double Double1 { get; set; }

        public long Long1 { get; set; }

        public int? Int1 { get; set; }

        // Enum, with converter
        [JsonConverter(typeof(EnumTypeConverter<EnumType>))]
        public EnumType EnumType { get; set; }

        public ComplexType[] ComplexType1 { get; set;}

        public ComplexType2 ComplexType2 { get ; set;}

        public byte[] Version { get; set; }

        public DateTimeOffset? CreatedAt { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }

        public bool Deleted { get; set; }
    }

    public class ComplexType2
    {
        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Age")]
        public int Age { get; set; }

        [JsonProperty(PropertyName = "Friends")]
        public string[] Friends { get; set; }
    }

    public class ComplexType
    {
        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Age")]
        public int Age { get; set; }
    }

    public enum EnumType { First, Second, Third }

    public class EnumTypeConverter<TEnum> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TEnum);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return Enum.Parse(typeof(TEnum), (reader.Value ?? default(TEnum)).ToString(), true);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value == null ? (string)null : value.ToString());
        }
    }
}
