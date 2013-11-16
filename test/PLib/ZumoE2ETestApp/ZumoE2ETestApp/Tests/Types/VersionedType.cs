// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Globalization;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using ZumoE2ETestApp.Framework;

namespace ZumoE2ETestApp.Tests.Types
{
    [DataTable(ZumoTestGlobals.StringIdRoundTripTableName)]
    public class VersionedType
    {
        public string Id { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "number")]
        public double Number { get; set; }
        [Version]
        public string Version { get; set; }
        [CreatedAt]
        public DateTime CreatedAt { get; set; }
        [UpdatedAt]
        public DateTime UpdatedAt { get; set; }

        public VersionedType() { }
        public VersionedType(Random rndGen)
        {
            this.Name = Util.CreateSimpleRandomString(rndGen, 20);
            this.Number = rndGen.Next(10000);
        }
        private VersionedType(VersionedType other)
        {
            this.Id = other.Id;
            this.Name = other.Name;
            this.Number = other.Number;
            this.Version = other.Version;
            this.CreatedAt = other.CreatedAt;
            this.UpdatedAt = other.UpdatedAt;
        }

        public override string ToString()
        {
            return string.Format("Versioned[Id={0},Name={1},Number={2},Version={3},CreatedAt={4},UpdatedAt={5}]",
                Id, Name, Number, Version,
                CreatedAt.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture),
                UpdatedAt.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture));
        }

        public override int GetHashCode()
        {
            int result = 0;
            if (Name != null) result ^= Name.GetHashCode();
            result ^= Number.GetHashCode();
            return result;
        }

        public override bool Equals(object obj)
        {
            var other = obj as VersionedType;
            if (other == null) return false;
            if (this.Name != other.Name) return false;
            if (this.Number != other.Number) return false;
            return true;
        }

        public VersionedType Clone()
        {
            return new VersionedType(this);
        }
    }
}
