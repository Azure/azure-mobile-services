// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Globalization;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.SQLiteStore
{
    internal class SqlHelpers
    {
        static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static object SerializeValue(JValue value, bool allowNull)
        {
            string storeType = SqlHelpers.GetStoreType(value.Type, allowNull);
            return SerializeValue(value, storeType, value.Type);
        }

        public static object SerializeValue(JToken value, string storeType, JTokenType columnType)
        {
            if (value == null || value.Type == JTokenType.Null)
            {
                return null;
            }

            if (IsTextType(storeType))
            {
                return SerializeAsText(value, columnType);
            }
            if (IsRealType(storeType))
            {
                return SerializeAsReal(value, columnType);
            }
            if (IsNumberType(storeType))
            {
                return SerializeAsNumber(value, columnType);
            }

            return value.ToString();
        }

        public static JToken DeserializeValue(object value, string storeType, JTokenType columnType)
        {
            if (value == null)
            {
                return null;
            }

            if (IsTextType(storeType))
            {
                return SqlHelpers.ParseText(columnType, value);
            }
            if (IsRealType(storeType))
            {
                return SqlHelpers.ParseReal(columnType, value);
            }
            if (IsNumberType(storeType))
            {
                return SqlHelpers.ParseNumber(columnType, value);
            }

            return null;
        }

        // https://www.sqlite.org/datatype3.html (2.2 Affinity Name Examples)
        public static string GetStoreCastType(Type type)
        {
            if (type == typeof(bool) ||
                type == typeof(DateTime) ||
                type == typeof(decimal))
            {
                return SqlColumnType.Numeric;
            }
            else if (type == typeof(int) ||
                    type == typeof(uint) ||
                    type == typeof(long) ||
                    type == typeof(ulong) ||
                    type == typeof(short) ||
                    type == typeof(ushort) ||
                    type == typeof(byte) ||
                    type == typeof(sbyte))
            {
                return SqlColumnType.Integer;
            }
            else if (type == typeof(float) ||
                     type == typeof(double))
            {
                return SqlColumnType.Real;
            }
            else if (type == typeof(string) ||
                    type == typeof(Guid) ||
                    type == typeof(byte[]) ||
                    type == typeof(Uri) ||
                    type == typeof(TimeSpan))
            {
                return SqlColumnType.Text;
            }

            throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Value of type '{0}' is not supported.", type.Name));
        }

        public static string GetStoreType(JTokenType type, bool allowNull)
        {
            switch (type)
            {
                case JTokenType.Boolean:
                    return SqlColumnType.Boolean;
                case JTokenType.Integer:
                    return SqlColumnType.Integer;
                case JTokenType.Date:
                    return SqlColumnType.DateTime;
                case JTokenType.Float:
                    return SqlColumnType.Float;
                case JTokenType.String:
                    return SqlColumnType.Text;
                case JTokenType.Guid:
                    return SqlColumnType.Guid;
                case JTokenType.Array:
                case JTokenType.Object:
                    return SqlColumnType.Json;
                case JTokenType.Bytes:
                    return SqlColumnType.Blob;
                case JTokenType.Uri:
                    return SqlColumnType.Uri;
                case JTokenType.TimeSpan:
                    return SqlColumnType.TimeSpan;
                case JTokenType.Null:
                    if (allowNull)
                    {
                        return null;
                    }
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Property of type '{0}' is not supported.", type));
                case JTokenType.Comment:
                case JTokenType.Constructor:
                case JTokenType.None:
                case JTokenType.Property:
                case JTokenType.Raw:
                case JTokenType.Undefined:
                default:
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Property of type '{0}' is not supported.", type));
            }
        }

        public static string FormatTableName(string tableName)
        {
            ValidateIdentifier(tableName);
            return string.Format("[{0}]", tableName);
        }

        public static string FormatMember(string memberName)
        {
            ValidateIdentifier(memberName);
            return string.Format("[{0}]", memberName);
        }

        private static bool IsNumberType(string storeType)
        {
            return storeType == SqlColumnType.Integer ||
                    storeType == SqlColumnType.Numeric ||
                    storeType == SqlColumnType.Boolean ||
                    storeType == SqlColumnType.DateTime;
        }

        private static bool IsRealType(string storeType)
        {
            return storeType == SqlColumnType.Real ||
                    storeType == SqlColumnType.Float;
        }

        private static bool IsTextType(string storeType)
        {
            return storeType == SqlColumnType.Text ||
                    storeType == SqlColumnType.Blob ||
                    storeType == SqlColumnType.Guid ||
                    storeType == SqlColumnType.Json ||
                    storeType == SqlColumnType.Uri ||
                    storeType == SqlColumnType.TimeSpan;
        }

        private static object SerializeAsNumber(JToken value, JTokenType columnType)
        {
            if (columnType == JTokenType.Date)
            {
                return SerializeDateTime(value);
            }
            return value.Value<long>();
        }

        private static double SerializeAsReal(JToken value, JTokenType columnType)
        {
            return value.Value<double>();
        }

        private static string SerializeAsText(JToken value, JTokenType columnType)
        {
            if (columnType == JTokenType.Bytes && value.Type == JTokenType.Bytes)
            {
                return Convert.ToBase64String(value.Value<byte[]>());
            }

            return value.ToString();
        }

        private static JToken ParseText(JTokenType type, object value)
        {
            string strValue = value as string;
            if (value == null)
            {
                return strValue;
            }

            if (type == JTokenType.Guid)
            {
                return Guid.Parse(strValue);
            }
            if (type == JTokenType.Bytes)
            {
                return Convert.FromBase64String(strValue);
            }
            if (type == JTokenType.TimeSpan)
            {
                return TimeSpan.Parse(strValue);
            }
            if (type == JTokenType.Uri)
            {
                return new Uri(strValue, UriKind.RelativeOrAbsolute);
            }
            if (type == JTokenType.Array || type == JTokenType.Object)
            {
                return JToken.Parse(strValue);
            }

            return strValue;
        }

        private static JToken ParseReal(JTokenType type, object value)
        {
            double dblValue = Convert.ToDouble(value);
            if (type == JTokenType.Date) // for compatibility reason i.e. in earlier release datetime was serialized as real type
            {
                return DeserializeDateTime(dblValue);
            }

            return dblValue;
        }

        private static JToken ParseNumber(JTokenType type, object value)
        {
            if (type == JTokenType.Date)
            {
                return DeserializeDateTime(Convert.ToDouble(value));
            }

            long longValue = Convert.ToInt64(value);
            if (type == JTokenType.Boolean)
            {
                bool boolValue = longValue == 1;
                return boolValue;
            }
            return longValue;
        }

        private static JToken DeserializeDateTime(double value)
        {
            return epoch.AddSeconds(value);
        }

        private static double SerializeDateTime(JToken value)
        {
            var date = value.ToObject<DateTime>();
            if (date.Kind == DateTimeKind.Unspecified)
            {
                date = DateTime.SpecifyKind(date, DateTimeKind.Utc);
            }
            double unixTimestamp = Math.Round((date.ToUniversalTime() - epoch).TotalSeconds, 3);
            return unixTimestamp;
        }

        private static void ValidateIdentifier(string identifier)
        {
            if (!IsValidIdentifier(identifier))
            {
                throw new ArgumentException(string.Format("'{0}' is not a valid identifier. Identifiers must be under 128 characters in length, start with a letter or underscore, and can contain only alpha-numeric and underscore characters.", identifier), "identifier");
            }
        }

        private static bool IsValidIdentifier(string identifier)
        {
            if (String.IsNullOrWhiteSpace(identifier) || identifier.Length > 128)
            {
                return false;
            }

            char first = identifier[0];
            if (!(Char.IsLetter(first) || first == '_'))
            {
                return false;
            }

            for (int i = 1; i < identifier.Length; i++)
            {
                char ch = identifier[i];
                if (!(Char.IsLetterOrDigit(ch) || ch == '_'))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
