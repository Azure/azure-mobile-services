// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Linq;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using ZumoE2ETestApp.Framework;

namespace ZumoE2ETestApp.Tests.Types
{
    [DataTable(ZumoTestGlobals.RoundTripTableName)]
    public class RoundTripTableItem : ICloneableItem<RoundTripTableItem>
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "string1")]
        public string String1 { get; set; }

        [JsonProperty(PropertyName = "date1")]
        public DateTime? Date1 { get; set; }

        [JsonProperty(PropertyName = "bool1")]
        public bool? Bool1 { get; set; }

        // Number types
        [JsonProperty(PropertyName = "double1")]
        public double Double1 { get; set; }
        [JsonProperty(PropertyName = "long1")]
        public long Long1 { get; set; }
        [JsonProperty(PropertyName = "int1")]
        public int? Int1 { get; set; }

        // Enum, with converter
        [JsonProperty(PropertyName = "enumType1")]
        [JsonConverter(typeof(EnumTypeConverter<EnumType>))]
        public EnumType EnumType { get; set; }

        [JsonProperty(PropertyName = "complexType1")]
        public ComplexType[] ComplexType1 { get; set; }

        [JsonProperty(PropertyName = "complexType2")]
        public ComplexType2 ComplexType2 { get; set; }

        public RoundTripTableItem() { }
        public RoundTripTableItem(Random rndGen)
        {
            this.String1 = Util.CreateSimpleRandomString(rndGen, 5);
            this.Date1 = new DateTime(rndGen.Next(1980, 2000), rndGen.Next(1, 12), rndGen.Next(1, 25), rndGen.Next(0, 24), rndGen.Next(0, 60), rndGen.Next(0, 60), DateTimeKind.Utc);
            this.Bool1 = rndGen.Next(2) == 0;
            this.Double1 = rndGen.Next(10000) * rndGen.NextDouble();
            this.EnumType = (Types.EnumType)rndGen.Next(3);
            this.Int1 = rndGen.Next();
            this.Long1 = rndGen.Next();
            this.ComplexType1 = new ComplexType[] { new ComplexType(rndGen) };
            this.ComplexType2 = new ComplexType2(rndGen);
        }

        object ICloneableItem<RoundTripTableItem>.Id
        {
            get { return this.Id; }
            set { this.Id = (int)value; }
        }

        public RoundTripTableItem Clone()
        {
            RoundTripTableItem result = new RoundTripTableItem
            {
                Id = this.Id,
                Bool1 = this.Bool1,
                Date1 = this.Date1,
                Double1 = this.Double1,
                EnumType = this.EnumType,
                Int1 = this.Int1,
                Long1 = this.Long1,
                String1 = this.String1,
            };

            if (this.ComplexType1 != null)
            {
                result.ComplexType1 = this.ComplexType1.Select(ct => ct == null ? null : ct.Clone()).ToArray();
            }

            if (this.ComplexType2 != null)
            {
                result.ComplexType2 = this.ComplexType2.Clone();
            }

            return result;
        }

        public override bool Equals(object obj)
        {
            const double acceptableDifference = 1e-6;
            RoundTripTableItem other = obj as RoundTripTableItem;
            if (other == null) return false;
            if (!this.Bool1.Equals(other.Bool1)) return false;
            if (!Util.CompareArrays(this.ComplexType1, other.ComplexType1)) return false;
            if ((this.ComplexType2 == null) != (other.ComplexType2 == null)) return false;
            if (this.Date1.HasValue != other.Date1.HasValue) return false;
            if (this.Date1.HasValue && !this.Date1.Value.ToUniversalTime().Equals(other.Date1.Value.ToUniversalTime())) return false;
            if (Math.Abs(this.Double1 - other.Double1) > acceptableDifference) return false;
            if (!this.EnumType.Equals(other.EnumType)) return false;
            if (!this.Int1.Equals(other.Int1)) return false;
            if (!this.Long1.Equals(other.Long1)) return false;
            if (this.String1 != other.String1) return false;
            return true;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "RoundTripTableItem[Bool1={0},ComplexType1={1},ComplexType2={2},Date1={3},Double1={4},EnumType={5},Int1={6},Long1={7},String1={8}]",
                Bool1.HasValue ? Bool1.Value.ToString() : "<<NULL>>",
                Util.ArrayToString(ComplexType1),
                ComplexType2 == null ? "<<NULL>>" : ComplexType2.ToString(),
                Date1.HasValue ? Date1.Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fff") : "<<NULL>>",
                Double1,
                EnumType,
                Int1.HasValue ? Int1.Value.ToString(CultureInfo.InvariantCulture) : "<<NULL>>",
                Long1,
                String1);
        }

        public override int GetHashCode()
        {
            int result = 0;
            result ^= this.Bool1.GetHashCode();
            result ^= Util.GetArrayHashCode(this.ComplexType1);

            if (this.ComplexType2 != null)
            {
                result ^= this.ComplexType2.GetHashCode();
            }

            if (this.Date1.HasValue)
            {
                result ^= this.Date1.Value.ToUniversalTime().GetHashCode();
            }

            result ^= this.Double1.GetHashCode();
            result ^= this.EnumType.GetHashCode();
            result ^= this.Int1.GetHashCode();
            result ^= this.Long1.GetHashCode();

            if (this.String1 != null)
            {
                result ^= this.String1.GetHashCode();
            }

            return result;
        }
    }

    public class ComplexType2
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string[] Friends { get; set; }

        public ComplexType2() { }
        public ComplexType2(Random rndGen)
        {
            this.Name = Util.CreateSimpleRandomString(rndGen, 10);
            this.Age = rndGen.Next(80);
            this.Friends = Enumerable.Range(1, 5)
                .Select(_ => Util.CreateSimpleRandomString(rndGen, 5))
                .ToArray();
        }

        public override int GetHashCode()
        {
            int result = 0;
            if (this.Name != null) result ^= this.Name.GetHashCode();
            result ^= this.Age.GetHashCode();
            result ^= Util.GetArrayHashCode(this.Friends);
            return result;
        }

        public override bool Equals(object obj)
        {
            ComplexType2 other = obj as ComplexType2;
            if (other == null) return false;
            if (this.Age != other.Age) return false;
            if (this.Name != other.Name) return false;
            if (!Util.CompareArrays(this.Friends, other.Friends)) return false;
            return true;
        }

        public override string ToString()
        {
            return string.Format("ComplexType2[Name={0},Age={1},Friends={2}]",
                Name,
                Age,
                Util.ArrayToString(Friends));
        }

        internal ComplexType2 Clone()
        {
            return new ComplexType2
            {
                Name = this.Name,
                Age = this.Age,
                Friends = this.Friends == null ? null : this.Friends.Select(a => a).ToArray()
            };
        }
    }

    public class ComplexType
    {
        public ComplexType() { }
        public ComplexType(Random rndGen)
        {
            this.Name = Util.CreateSimpleRandomString(rndGen, 10);
            this.Age = rndGen.Next(80);
        }

        public string Name { get; set; }
        public int Age { get; set; }

        public override int GetHashCode()
        {
            int result = this.Age.GetHashCode();
            if (this.Name != null)
            {
                result ^= this.Name.GetHashCode();
            }

            return result;
        }

        public override bool Equals(object obj)
        {
            ComplexType other = obj as ComplexType;
            return other != null && this.Age == other.Age && this.Name == other.Name;
        }

        public override string ToString()
        {
            return string.Format("ComplexType[Name={0},Age={1}]", Name, Age);
        }

        internal ComplexType Clone()
        {
            return new ComplexType { Name = this.Name, Age = this.Age };
        }
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
