// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using Newtonsoft.Json;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    public class CreatedAtType
    {
        public string Id { get; set; }

        [CreatedAt]
        public DateTime CreatedAt { get; set; }
    }

    public class NotSystemPropertyCreatedAtType
    {
        public string Id { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class IntegerIdNotSystemPropertyCreatedAtType
    {
        public int Id { get; set; }

        public DateTime __createdAt { get; set; }
    }

    public class UpdatedAtType
    {
        public string Id { get; set; }

        [UpdatedAt]
        public DateTime UpdatedAt { get; set; }
    }

    public class NotSystemPropertyUpdatedAtType
    {
        public string Id { get; set; }

        public DateTime _UpdatedAt { get; set; }
    }

    public class VersionType
    {
        public string Id { get; set; }

        [Version]
        public string Version { get; set; }
    }

    public class NotSystemPropertyVersionType
    {
        public string Id { get; set; }

        public string version { get; set; }
    }

    public class AllBaseTypesWithAllSystemPropertiesType : IEquatable<AllBaseTypesWithAllSystemPropertiesType>
    {
        public string Id { get; set; }

        [CreatedAt]
        public DateTime CreatedAt { get; set; }

        [UpdatedAt]
        public DateTime UpdatedAt { get; set; }

        [Version]
        public string Version { get; set; }

        public bool Bool { get; set; }
        public byte Byte { get; set; }
        public sbyte SByte { get; set; }
        public ushort UShort { get; set; }
        public short Short { get; set; }
        public uint UInt { get; set; }
        public int Int { get; set; }
        public ulong ULong { get; set; }
        public long Long { get; set; }
        public float Float { get; set; }
        public double Double { get; set; }
        public decimal Decimal { get; set; }
        public string String { get; set; }
        public char Char { get; set; }
        public DateTime DateTime { get; set; }
        public DateTimeOffset DateTimeOffset { get; set; }
        public double? Nullable { get; set; }
        public DateTime? NullableDateTime { get; set; }
        public TimeSpan TimeSpan { get; set; }
        public Uri Uri { get; set; }
        public Enum1 Enum1 { get; set; }
        public Enum2 Enum2 { get; set; }
        public Enum3 Enum3 { get; set; }
        public Enum4 Enum4 { get; set; }
        public Enum5 Enum5 { get; set; }
        public Enum6 Enum6 { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as AllBaseTypesWithAllSystemPropertiesType;
            if (other != null)
            {
                return Equals(other);
            }
            return false;
        }

        public bool Equals(AllBaseTypesWithAllSystemPropertiesType other)
        {
            return this.Id == other.Id &&
                   this.CreatedAt == other.CreatedAt &&
                   this.UpdatedAt == other.UpdatedAt &&
                   this.Version == other.Version &&
                   this.Bool == other.Bool &&
                   this.Byte == other.Byte &&
                   this.SByte == other.SByte &&
                   this.UShort == other.UShort &&
                   this.Short == other.Short &&
                   this.UInt == other.UInt &&
                   this.Int == other.Int &&
                   this.ULong == other.ULong &&
                   this.Long == other.Long &&
                   this.Float == other.Float &&
                   this.Double == other.Double &&
                   this.Decimal == other.Decimal &&
                   this.String == other.String &&
                   this.Char == other.Char &&
                   this.DateTime == other.DateTime &&
                   this.DateTimeOffset == other.DateTimeOffset &&
                   this.Nullable == other.Nullable &&
                   this.NullableDateTime == other.NullableDateTime &&
                   this.TimeSpan == other.TimeSpan &&
                   this.Uri == other.Uri &&
                   this.Enum1 == other.Enum1 &&
                   this.Enum2 == other.Enum2 &&
                   this.Enum3 == other.Enum3 &&
                   this.Enum4 == other.Enum4 &&
                   this.Enum5 == other.Enum5 &&
                   this.Enum6 == other.Enum6;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class AllSystemPropertiesType
    {
        public string Id { get; set; }

        [CreatedAt]
        public DateTime CreatedAt { get; set; }

        [UpdatedAt]
        public DateTime UpdatedAt { get; set; }

        [Version]
        public string Version { get; set; }
    }

    public class MultipleSystemPropertiesType
    {
        public string Id { get; set; }

        [CreatedAt]
        public DateTime CreatedAt { get; set; }

        [CreatedAt]
        public DateTime CreatedAt2 { get; set; }

        [Version]
        public string Version { get; set; }
    }

    public class NamedSystemPropertiesType
    {
        public string Id { get; set; }

        public DateTime __createdAt { get; set; }
    }

    public class NamedDifferentCasingSystemPropertiesType
    {
        public string Id { get; set; }

        public DateTime __CreatedAt { get; set; }
    }

    public class NamedAndAttributedSystemPropertiesType
    {
        public string Id { get; set; }

        public DateTime __createdAt { get; set; }

        [CreatedAt]
        public DateTime CreatedAt { get; set; }
    }

    public class DoubleNamedSystemPropertiesType
    {
        public string Id { get; set; }

        public DateTime __createdAt { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class DoubleJsonPropertyNamedSystemPropertiesType
    {
        public string Id { get; set; }

        [CreatedAt]
        public DateTime CreatedAt { get; set; }

        [JsonProperty(PropertyName = "__createdAt")]
        public DateTime AlsoCreatedAt { get; set; }
    }

    public class IntegerIdWithSystemPropertiesType
    {
        public int Id { get; set; }

        [CreatedAt]
        public DateTime CreatedAt { get; set; }
    }

    public class LongIdWithSystemPropertiesType
    {
        public long Id { get; set; }

        [CreatedAt]
        public DateTime CreatedAt { get; set; }
    }

    public class IntegerIdWithNamedSystemPropertiesType
    {
        public int Id { get; set; }

        public DateTime __createdAt { get; set; }
    }

    public class LongIdWithNamedSystemPropertiesType
    {
        public long Id { get; set; }

        public DateTime __createdAt { get; set; }
    }

    public class StringCreatedAtType
    {
        public string Id { get; set; }

        [CreatedAt]
        public String CreatedAt { get; set; }
    }

    public class StringUpdatedAtType
    {
        public string Id { get; set; }

        [UpdatedAt]
        public String UpdatedAt { get; set; }
    }
}
