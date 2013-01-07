using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace ZumoE2ETestApp.Framework
{
    /// <summary>
    ///  Utilitary functions used in the tests.
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// Dates are represented in the server with millisecond precision. This method
        /// will trim the sub-millisecond part of a DateTime instance to make sure that, when
        /// round-tripping, the value will remain the same.
        /// </summary>
        /// <param name="dateTime">The date to have its sub-millisecond portion removed.</param>
        /// <returns>A new instance, with millisecond precision.</returns>
        public static DateTime TrimSubMilliseconds(DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day,
                dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond, dateTime.Kind);
        }

        public static string CreateSimpleRandomString(Random rndGen, int size)
        {
            return new string(
                Enumerable.Range(0, size)
                    .Select(_ => (char)rndGen.Next(' ', '~' + 1))
                    .ToArray());
        }

        public static string ArrayToString<T>(T[] array)
        {
            if (array == null)
            {
                return "<<NULL>>";
            }
            else
            {
                return "[" + string.Join(", ", array.Select(i => i == null ? "<NULL>" : i.ToString())) + "]";
            }
        }

        public static int GetArrayHashCode<T>(T[] array)
        {
            int result = 0;
            if (array != null)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    object item = array.GetValue(i);
                    if (item != null)
                    {
                        result ^= item.GetHashCode();
                    }
                }
            }

            return result;
        }

        public static bool CompareArrays<T>(T[] array1, T[] array2)
        {
            return CompareArrays<T>(array1, array2, null);
        }

        public static bool CompareArrays<T>(T[] array1, T[] array2, List<string> errors)
        {
            if (array1 == null)
            {
                if (array2 != null && errors != null)
                {
                    errors.Add("First array is null, second is not");
                }

                return array2 == null;
            }

            if (array2 == null)
            {
                if (errors != null)
                {
                    errors.Add("First array is not null, second is null");
                }

                return false;
            }

            if (array1.Length != array2.Length)
            {
                if (errors != null)
                {
                    errors.Add(string.Format(CultureInfo.InvariantCulture, "Size of first array ({0}) is different than second ({1})", array1.Length, array2.Length));
                }

                return false;
            }

            for (int i = 0; i < array2.Length; i++)
            {
                object item1 = array1.GetValue(i);
                object item2 = array2.GetValue(i);
                if ((item1 == null) != (item2 == null))
                {
                    if (errors != null)
                    {
                        errors.Add(string.Format(CultureInfo.InvariantCulture, "Difference in item {0}: first {1} null, second {2} null",
                            i, item1 == null ? "is" : "is not", item2 == null ? "is" : "is not"));
                    }

                    return false;
                }

                if (item1 != null && !item1.Equals(item2))
                {
                    if (errors != null)
                    {
                        errors.Add(string.Format(CultureInfo.InvariantCulture, "Difference in item {0}: first = {1}; second = {2}", i, item1, item2));
                    }

                    return false;
                }
            }

            return true;
        }

        public static bool CompareJson(IJsonValue expected, IJsonValue actual, List<string> errors)
        {
            if (expected == null)
            {
                return actual == null;
            }

            if (actual == null)
            {
                return false;
            }

            if (expected.ValueType != actual.ValueType)
            {
                errors.Add(string.Format("Expected value type {0} != actual {1}", expected.ValueType, actual.ValueType));
                return false;
            }

            switch (expected.ValueType)
            {
                case JsonValueType.Boolean:
                    return expected.GetBoolean() == actual.GetBoolean();
                case JsonValueType.Null:
                    return true;
                case JsonValueType.String:
                    return expected.GetString() == actual.GetString();
                case JsonValueType.Number:
                    double expectedNumber = expected.GetNumber();
                    double actualNumber = actual.GetNumber();
                    double delta = 1 - expectedNumber / actualNumber;
                    double acceptableEpsilon = 0.000001;
                    if (Math.Abs(delta) > acceptableEpsilon)
                    {
                        errors.Add(string.Format("Numbers differ more than the allowed difference: {0} - {1}",
                            expected.Stringify(), actual.Stringify()));
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                case JsonValueType.Array:
                    JsonArray expectedArray = expected.GetArray();
                    JsonArray actualArray = actual.GetArray();
                    if (expectedArray.Count != actualArray.Count)
                    {
                        errors.Add(string.Format("Size of arrays are different: expected {0} != actual {1}", expectedArray.Count, actualArray.Count));
                        return false;
                    }

                    for (int i = 0; i < expectedArray.Count; i++)
                    {
                        if (!CompareJson(expectedArray[i], actualArray[i], errors))
                        {
                            errors.Add("Difference in array item at index " + i);
                            return false;
                        }
                    }

                    return true;
                case JsonValueType.Object:
                    JsonObject expectedObject = expected.GetObject();
                    JsonObject actualObject = actual.GetObject();
                    foreach (var key in expectedObject.Keys)
                    {
                        if (key == "id") continue; // set by server, ignored at comparison

                        if (!actualObject.ContainsKey(key))
                        {
                            errors.Add(string.Format("Expected object contains a pair with key {0}, actual does not.", key));
                            return false;
                        }

                        if (!CompareJson(expectedObject[key], actualObject[key], errors))
                        {
                            errors.Add("Difference in object member with key " + key);
                            return false;
                        }
                    }

                    return true;
                default:
                    throw new ArgumentException("Don't know how to compare IJsonValue of type " + expected.ValueType);
            }
        }
    }
}
