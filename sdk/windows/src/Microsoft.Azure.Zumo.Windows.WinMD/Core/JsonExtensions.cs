// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Windows.Data.Json;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Helpful extensions that make it easier to manipulate JSON values.
    /// </summary>
    /// <remarks>
    /// The Windows.Data.Json types are very exception heavy so most JSON usage
    /// ends up taking 12 lines to get a string member from an object.  We
    /// simplify that a little using C# style "as" semantics where we'll return
    /// the value you want or null.  Nulls also propagate through calls so for
    /// example, given a JSON object like { 'typeInfo': { 'bar' : 2 } }, you
    /// could just write obj.Get("typeInfo").Get("bar").AsInteger() to get the
    /// value 2.  Given a JSON object like { 'baz': 2 }, you could write
    /// obj.Get("typeInfo").Get("bar").AsInteger() to get the value null.
    /// </remarks>
    internal static class JsonExtensions
    {
        /// <summary>
        /// Convert a JSON value into an object, returning null if it's unable.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>A JSON object or null.</returns>
        public static JsonObject AsObject(this IJsonValue value)
        {
            return (value != null && value.ValueType == JsonValueType.Object) ?
                value.GetObject() :
                null;
        }

        /// <summary>
        /// Convert a JSON value into a string, returning null if it's unable.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>A string or null.</returns>
        public static string AsString(this IJsonValue value)
        {
            return (value != null && value.ValueType == JsonValueType.String) ?
                value.GetString() :
                null;
        }

        /// <summary>
        /// Convert a JSON value into a double, returning null if it's unable.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>A double or null.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This code is used in one project but not the other")]
        public static double? AsNumber(this IJsonValue value)
        {
            return (value != null && value.ValueType == JsonValueType.Number) ?
                (double?)value.GetNumber() :
                null;
        }

        /// <summary>
        /// Convert a JSON value into an int, returning null if it's unable.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>An int or null.</returns>
        public static int? AsInteger(this IJsonValue value)
        {
            return (value != null && value.ValueType == JsonValueType.Number) ?
                (int?)value.GetNumber() :
                null;
        }

        /// <summary>
        /// Convert a JSON value into a bool, returning null if it's unable.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>A bool or null.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This code is used in one project but not the other")]
        public static bool? AsBool(this IJsonValue value)
        {
            return (value != null && value.ValueType == JsonValueType.Boolean) ?
                (bool?)value.GetBoolean() :
                null;
        }

        /// <summary>
        /// Convert a JSON value into an array, returning null if it's unable.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>A JSON array or null.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This code is used in one project but not the other")]
        public static JsonArray AsArray(this IJsonValue value)
        {
            return (value != null && value.ValueType == JsonValueType.Array) ?
                value.GetArray() :
                null;
        }

        /// <summary>
        /// Determine if the value represents null.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>True if the value is null, false otherwise.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This code is used in one project but not the other")]
        public static bool IsNull(this IJsonValue value)
        {
            return value == null || value.ValueType == JsonValueType.Null;
        }

        /// <summary>
        /// Get a null JSON value (that when serialized will stay null rather
        /// than not being written out).
        /// </summary>
        /// <returns>A NULL JSON value.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This code is used in one project but not the other")]
        public static IJsonValue Null()
        {
            // TODO: Follow up with WinRT folks and make sure there's no other
            // sensible way to do this.
            return JsonValue.Parse("null");
        }

        /// <summary>
        /// Enumerate the member name/value pairs of a JSON object.
        /// </summary>
        /// <param name="value">A JSON object.</param>
        /// <returns>The name/value pairs of the JSON object.</returns>
        public static IEnumerable<KeyValuePair<string, JsonValue>> GetPropertyValues(this JsonObject value)
        {
            if (value != null)
            {
                foreach (string key in value.Keys)
                {
                    JsonValue val = value.GetNamedValue(key);
                    yield return new KeyValuePair<string, JsonValue>(key, val);
                }
            }
        }

        /// <summary>
        /// Gets a named value from an object.
        /// </summary>
        /// <param name="value">
        /// The object to check (though the type is IJsonValue so multiple
        /// calls to Get can be chained together).
        /// </param>
        /// <param name="name">The name of the value to lookup.</param>
        /// <returns>
        /// The value associated with the name, or null if the instance isn't
        /// an object or the name was not found.
        /// </returns>
        public static IJsonValue Get(this IJsonValue value, string name)
        {
            JsonObject obj = value.AsObject();
            IJsonValue val = null;
            if (obj != null)
            {
                obj.TryGetValue(name, out val);
            }
            return val;
        }

        /// <summary>
        /// Try to convert a value to a CLR object.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="propertyValue">The converted value.</param>
        /// <returns>
        /// A value indicating whether the conversion was successful.
        /// </returns>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This code is used in one project but not the other")]
        public static bool TryConvert(this IJsonValue value, out object propertyValue)
        {
            propertyValue = null;
            if (!value.IsNull())
            {
                switch (value.ValueType)
                {
                    case JsonValueType.Boolean:
                        propertyValue = value.AsBool().Value;
                        break;
                    case JsonValueType.Number:
                        propertyValue = value.AsNumber().Value;
                        break;
                    case JsonValueType.String:
                        propertyValue = value.AsString();
                        break;
                    case JsonValueType.Null:
                        break;
                    case JsonValueType.Object:
                    case JsonValueType.Array:
                    default:
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Set a named value on an object and return that object so multiple
        /// calls can be chained.
        /// </summary>
        /// <param name="obj">The object to set the value on.</param>
        /// <param name="name">The name to set.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>The object the value was set on.</returns>
        public static JsonObject Set(this JsonObject obj, string name, IJsonValue value)
        {
            Debug.Assert(obj != null, "obj should probably not be null.");
            Debug.Assert(!string.IsNullOrEmpty(name), "name should not be null or empty.");

            if (obj != null)
            {
                obj.SetNamedValue(name, value);
            }
            return obj;
        }

        /// <summary>
        /// Set a named value on an object and return that object so multiple
        /// calls can be chained.
        /// </summary>
        /// <param name="obj">The object to set the value on.</param>
        /// <param name="name">The name to set.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>The object the value was set on.</returns>
        public static JsonObject Set(this JsonObject obj, string name, string value)
        {
            // Null isn't a valid string value, but it's a valid JSON value so
            // we'll automatically map that for you.
            return obj.Set(name,
                value != null ?
                    JsonValue.CreateStringValue(value) :
                    null);
        }

        /// <summary>
        /// Set a named value on an object and return that object so multiple
        /// calls can be chained.
        /// </summary>
        /// <param name="obj">The object to set the value on.</param>
        /// <param name="name">The name to set.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>The object the value was set on.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This code is used in one project but not the other")]
        public static JsonObject Set(this JsonObject obj, string name, double value)
        {
            return obj.Set(name, JsonValue.CreateNumberValue(value));
        }

        /// <summary>
        /// Set a named value on an object and return that object so multiple
        /// calls can be chained.
        /// </summary>
        /// <param name="obj">The object to set the value on.</param>
        /// <param name="name">The name to set.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>The object the value was set on.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This code is used in one project but not the other")]
        public static JsonObject Set(this JsonObject obj, string name, int value)
        {
            return obj.Set(name, JsonValue.CreateNumberValue(value));
        }

        /// <summary>
        /// Set a named value on an object and return that object so multiple
        /// calls can be chained.
        /// </summary>
        /// <param name="obj">The object to set the value on.</param>
        /// <param name="name">The name to set.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>The object the value was set on.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This code is used in one project but not the other")]
        public static JsonObject Set(this JsonObject obj, string name, bool value)
        {
            return obj.Set(name, JsonValue.CreateBooleanValue(value));
        }

        /// <summary>
        /// Set a named value on an object of any type.  Unlike the other set
        /// overloads, this will attempt to convert arbitrary CLR values into
        /// the correct JSON types.
        /// </summary>
        /// <param name="obj">The object to set the value on.</param>
        /// <param name="name">The name of the value to set.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>Whether we were able to set the value.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This code is used in one project but not the other")]
        public static bool TrySet(this JsonObject obj, string name, object value)
        {
            Debug.Assert(obj != null, "obj should probably not be null!");
            Debug.Assert(!string.IsNullOrEmpty(name), "name cannot be null or empty.");

            if (obj == null)
            {
                return false;
            }
            if (value == null)
            {
                obj.Set(name, JsonExtensions.Null());
            }
            else
            {
                // If the type is nullable, strip off the Nullable<> which will
                // allow the comparisons below to convert correctly (since
                // we've already checked the value isn't null)
                Type memberType = value.GetType();
                memberType = Nullable.GetUnderlyingType(memberType) ?? memberType;
                RuntimeTypeHandle handle = memberType.TypeHandle;

                // Set the value based on the type for known primitives
                if (handle.Equals(typeof(bool).TypeHandle))
                {
                    obj.Set(name, Convert.ToBoolean(value, CultureInfo.InvariantCulture));
                }
                else if (handle.Equals(typeof(int).TypeHandle) ||
                    handle.Equals(typeof(uint).TypeHandle) ||
                    handle.Equals(typeof(sbyte).TypeHandle) ||
                    handle.Equals(typeof(byte).TypeHandle) ||
                    handle.Equals(typeof(short).TypeHandle) ||
                    handle.Equals(typeof(ushort).TypeHandle) ||
                    handle.Equals(typeof(double).TypeHandle) ||
                    handle.Equals(typeof(float).TypeHandle) ||
                    handle.Equals(typeof(Decimal).TypeHandle) ||
                    handle.Equals(typeof(uint).TypeHandle))
                {
                    // Convert all numeric types into doubles
                    obj.Set(name, Convert.ToDouble(value, CultureInfo.InvariantCulture));
                }
                else if (handle.Equals(typeof(long).TypeHandle) ||
                    handle.Equals(typeof(ulong).TypeHandle))
                {
                    if (!NumericCanRoundtrip(value, memberType))
                    {
                        throw new ArgumentOutOfRangeException(
                            name,
                            value,
                            string.Format(
                                CultureInfo.InvariantCulture,
                                Resources.JsonExtensions_TrySetValue_CannotRoundtripNumericValue,
                                value,
                                name));
                    }

                    obj.Set(name, Convert.ToDouble(value, CultureInfo.InvariantCulture));
                }
                else if (handle.Equals(typeof(char).TypeHandle))
                {
                    // Convert characters to strings
                    obj.Set(name, value.ToString());
                }
                else if (handle.Equals(typeof(string).TypeHandle))
                {
                    obj.Set(name, value as string);
                }
                else if (handle.Equals(typeof(DateTime).TypeHandle))
                {
                    // Serialize DateTime as an ISO 8061 date/time
                    obj.Set(name, ((DateTime)value).ToRoundtripDateString());
                }
                else if (handle.Equals(typeof(DateTimeOffset).TypeHandle))
                {
                    // Serialize DateTimeOffset as an ISO 8061 date/time
                    obj.Set(name, ((DateTimeOffset)value).DateTime.ToRoundtripDateString());
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Ensure that a numeric value can safely roundtrip as a double.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="desiredType">The desired type.</param>
        /// <returns>
        /// True if the value can roundtrip, false otherwise.
        /// </returns>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Only used by the Managed project")]
        private static bool NumericCanRoundtrip(object value, Type desiredType)
        {
            // Make sure that the value can roundtrip
            bool canRoundtrip = false;
            object original = Convert.ChangeType(value, desiredType, CultureInfo.InvariantCulture);
            double converted = Convert.ToDouble(value, CultureInfo.InvariantCulture);
            try
            {
                object roundtripped = Convert.ChangeType(converted, desiredType, CultureInfo.InvariantCulture);
                canRoundtrip = original.Equals(roundtripped);
            }
            catch (InvalidCastException)
            {
            }
            catch (OverflowException)
            {
            }
            return canRoundtrip;
        }

        /// <summary>
        /// Add an element to a JSON array.
        /// </summary>
        /// <param name="array">The array to add to.</param>
        /// <param name="value">The element to add.</param>
        /// <returns>The original JSON array.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Only used from one linked project and not the other.")]
        public static JsonArray Append(this JsonArray array, IJsonValue value)
        {
            if (array != null)
            {
                array.Add(value);
            }
            return array;
        }
    }
}
