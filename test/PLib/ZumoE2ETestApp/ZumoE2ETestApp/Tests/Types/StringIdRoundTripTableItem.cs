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
    [DataTable(ZumoTestGlobals.StringIdRoundTripTableName)]
    public class StringIdRoundTripTableItem : ICloneableItem<StringIdRoundTripTableItem>
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "date1")]
        public DateTime? Date { get; set; }

        [JsonProperty(PropertyName = "bool")]
        public bool? Bool { get; set; }

        [JsonProperty(PropertyName = "number")]
        public double Number { get; set; }

        [JsonProperty(PropertyName = "complex")]
        public string[] ComplexType { get; set; }

        public StringIdRoundTripTableItem() { }
        public StringIdRoundTripTableItem(Random rndGen)
        {
            this.Name = Util.CreateSimpleRandomString(rndGen, 5);
            this.Date = new DateTime(rndGen.Next(1980, 2000), rndGen.Next(1, 12), rndGen.Next(1, 25), rndGen.Next(0, 24), rndGen.Next(0, 60), rndGen.Next(0, 60), DateTimeKind.Utc);
            this.Bool = rndGen.Next(2) == 0;
            this.Number = rndGen.Next(10000) * rndGen.NextDouble();
            this.ComplexType = Enumerable.Range(0, rndGen.Next(3, 5)).Select(_ => Util.CreateSimpleRandomString(rndGen, 10)).ToArray();
        }

        object ICloneableItem<StringIdRoundTripTableItem>.Id
        {
            get { return this.Id; }
            set { this.Id = (string)value; }
        }

        public StringIdRoundTripTableItem Clone()
        {
            var result = new StringIdRoundTripTableItem
            {
                Id = this.Id,
                Bool = this.Bool,
                Date = this.Date,
                Number = this.Number,
                Name = this.Name,
            };

            if (this.ComplexType != null)
            {
                result.ComplexType = this.ComplexType.Select(ct => ct).ToArray();
            }

            return result;
        }

        public override bool Equals(object obj)
        {
            const double acceptableDifference = 1e-6;
            var other = obj as StringIdRoundTripTableItem;
            if (other == null) return false;
            if (!this.Bool.Equals(other.Bool)) return false;
            if (!Util.CompareArrays(this.ComplexType, other.ComplexType)) return false;
            if (this.Date.HasValue != other.Date.HasValue) return false;
            if (this.Date.HasValue && !this.Date.Value.ToUniversalTime().Equals(other.Date.Value.ToUniversalTime())) return false;
            if (Math.Abs(this.Number - other.Number) > acceptableDifference) return false;
            if (this.Name != other.Name) return false;
            return true;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "StringIdRoundTripTableItem[Bool={0},ComplexType={1},Date1={2},Number={3},Name={4}]",
                Bool.HasValue ? Bool.Value.ToString() : "<<NULL>>",
                Util.ArrayToString(ComplexType),
                Date.HasValue ? Date.Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fff") : "<<NULL>>",
                Number,
                Name);
        }

        public override int GetHashCode()
        {
            int result = 0;
            result ^= this.Bool.GetHashCode();
            result ^= Util.GetArrayHashCode(this.ComplexType);

            if (this.Date.HasValue)
            {
                result ^= this.Date.Value.ToUniversalTime().GetHashCode();
            }

            result ^= this.Number.GetHashCode();

            if (this.Name != null)
            {
                result ^= this.Name.GetHashCode();
            }

            return result;
        }
    }
}
