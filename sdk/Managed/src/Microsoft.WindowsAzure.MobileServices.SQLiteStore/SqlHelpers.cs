// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.SQLiteStore
{
    internal class SqlHelpers
    {
        static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static object SerializeValue(JValue value, bool allowNull)
        {
            string columnType = SqlHelpers.GetColumnType(value.Type, allowNull);
            return SerializeValue(value, columnType, value.Type);
        }

        public static object SerializeValue(JToken value, string sqlType, JTokenType columnType)
        {
            if (value == null || value.Type == JTokenType.Null)
            {
                return null;
            }

            if (sqlType == SqlColumnType.Text)
            {
                return SerializeAsText(value, columnType);
            }
            if (sqlType == SqlColumnType.Real)
            {
                return SerializeAsReal(value, columnType);
            }
            if (sqlType == SqlColumnType.Integer)
            {
                return SerializeAsInteger(value, columnType);
            }

            return value.ToString();
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

        private static long SerializeAsInteger(JToken value, JTokenType columnType)
        {
            return value.Value<long>();
        }

        private static double SerializeAsReal(JToken value, JTokenType columnType)
        {
            if (columnType == JTokenType.Date)
            {
                var date = value.ToObject<DateTime>();
                if (date.Kind == DateTimeKind.Unspecified)
                {
                    date = DateTime.SpecifyKind(date, DateTimeKind.Utc);
                }
                double unixTimestamp = Math.Round((date.ToUniversalTime() - epoch).TotalSeconds, 3);
                return unixTimestamp;
            }
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

        public static JToken ParseText(JTokenType type, object value)
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
            if (type == JTokenType.Array || type == JTokenType.Object)
            {
                return JToken.Parse(strValue);
            }

            return strValue;
        }

        public static JToken ParseReal(JTokenType type, object value)
        {
            double dblValue = (value as double?).GetValueOrDefault();
            if (type == JTokenType.Date)
            {
                return epoch.AddSeconds(dblValue);
            }
            return dblValue;
        }

        public static JToken ParseInteger(JTokenType type, object value)
        {
            long longValue = (value as long?).GetValueOrDefault();
            if (type == JTokenType.Boolean)
            {
                bool boolValue = longValue == 1;
                return boolValue;
            }
            return longValue;
        }

        public static string GetColumnType(Type type)
        {
            if (type == typeof(bool) ||
                type == typeof(int))
            {
                return SqlColumnType.Integer;
            }
            else if (type == typeof(DateTime) ||
                     type == typeof(float) ||
                     type == typeof(double))
            {
                return SqlColumnType.Real;
            }
            else if (type == typeof(string) ||
                    type == typeof(Guid) ||
                    type == typeof(byte[]))
            {
                return SqlColumnType.Text;
            }

            throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, WindowsAzure.MobileServices.SQLiteStore.Properties.Resources.SQLiteStore_ValueTypeNotSupported, type.Name));
        }

        public static string GetColumnType(JTokenType type, bool allowNull)
        {
            switch (type)
            {
                case JTokenType.Boolean:
                case JTokenType.Integer:
                    return SqlColumnType.Integer;
                case JTokenType.Date:
                case JTokenType.Float:
                    return SqlColumnType.Real;
                case JTokenType.String:
                case JTokenType.Guid:
                case JTokenType.Array:
                case JTokenType.Object:
                case JTokenType.Bytes:
                    return SqlColumnType.Text;
                case JTokenType.Null:
                    if (allowNull)
                    {
                        return null;
                    }
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, WindowsAzure.MobileServices.SQLiteStore.Properties.Resources.SQLiteStore_JTokenNotSupported, type));
                case JTokenType.Comment:
                case JTokenType.Constructor:
                case JTokenType.None:
                case JTokenType.Property:
                case JTokenType.Raw:
                case JTokenType.TimeSpan:
                case JTokenType.Undefined:
                case JTokenType.Uri:
                default:
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, WindowsAzure.MobileServices.SQLiteStore.Properties.Resources.SQLiteStore_JTokenNotSupported, type));
            }
        }

        private static void ValidateIdentifier(string identifier)
        {
            if (!IsValidIdentifier(identifier))
            {
                throw new ArgumentException(string.Format(Properties.Resources.SQLiteStore_InvalidIdentifier, identifier), "identifier");
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
