// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    public class BoolType
    {
        public int Id { get; set; }
        public bool Bool { get; set; }
    }

    public class ByteType
    {
        public int Id { get; set; }
        public byte Byte { get; set; }
    }

    public class SByteType
    {
        public int Id { get; set; }
        public sbyte SByte { get; set; }
    }

    public class UShortType
    {
        public int Id { get; set; }
        public ushort UShort { get; set; }
    }

    public class ShortType
    {
        public int Id { get; set; }
        public short Short { get; set; }
    }

    public class UIntType
    {
        public int Id { get; set; }
        public uint UInt { get; set; }
    }

    public class IntType
    {
        public int Id { get; set; }
    }

    public class ULongType
    {
        public int Id { get; set; }
        public ulong ULong { get; set; }
    }

    public class LongType
    {
        public int Id { get; set; }
        public long Long { get; set; }
    }

    public class FloatType
    {
        public int Id { get; set; }
        public float Float { get; set; }
    }

    public class DoubleType
    {
        public int Id { get; set; }
        public double Double { get; set; }
    }

    public class DecimalType
    {
        public int Id { get; set; }
        public decimal Decimal { get; set; }
    }

    public class StringType
    {
        public int Id { get; set; }
        public string String { get; set; }
    }

    public class CharType
    {
        public int Id { get; set; }
        public char Char { get; set; }
    }

    public class DateTimeType
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
    }

    public class DateTimeOffsetType
    {
        public int Id { get; set; }
        public DateTimeOffset DateTimeOffset { get; set; }
    }

    public class NullableType
    {
        public int Id { get; set; }
        public double? Nullable { get; set; }
    }

    public class UriType
    {
        public int Id { get; set; }
        public Uri Uri { get; set; }
    }

    public class EnumType
    {
        public int Id { get; set; }
        public Enum1 Enum1 { get; set; }
        public Enum2 Enum2 { get; set; }
        public Enum3 Enum3 { get; set; }
        public Enum4 Enum4 { get; set; }
        public Enum5 Enum5 { get; set; }
        public Enum6 Enum6 { get; set; }
    }

    public enum Enum1
    {
        Enum1Value1,
        Enum1Value2,
    }

    public enum Enum2 : short
    {
        Enum2Value1,
        Enum2Value2,
    }

    [Flags]
    public enum Enum3
    {
        Enum3Value1 = 0x01,
        Enum3Value2 = 0x02,
    }

    public enum Enum4 : byte
    {
        Enum4Value1,
        Enum4Value2,
    }

    public enum Enum5 : sbyte
    {
        Enum5Value1,
        Enum5Value2,
    }

    public enum Enum6 : ulong
    {
        Enum6Value1 = 4,
        Enum6Value2 = 5,
    }
}
