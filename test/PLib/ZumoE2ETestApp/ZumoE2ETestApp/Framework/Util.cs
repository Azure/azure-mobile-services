// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
#if WP75
using System.Threading;
#endif
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

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

        public static async Task<string> UploadLogs(string uploadLogsUrl, string testLogs, string platform, bool allTests)
        {
            using (var client = new HttpClient())
            {
                string url = uploadLogsUrl + "?platform=" + platform;
                if (allTests)
                {
                    url = url + "&allTests=true";
                }

                object clientVersion, runtimeVersion;
                if (ZumoTestGlobals.Instance.GlobalTestParams.TryGetValue(ZumoTestGlobals.ClientVersionKeyName, out clientVersion))
                {
                    url = url + "&clientVersion=" + clientVersion;
                }

                if (ZumoTestGlobals.Instance.GlobalTestParams.TryGetValue(ZumoTestGlobals.RuntimeVersionKeyName, out runtimeVersion))
                {
                    url = url + "&runtimeVersion=" + runtimeVersion;
                }

                using (var request = new HttpRequestMessage(HttpMethod.Post, url))
                {
                    request.Content = new StringContent(testLogs, Encoding.UTF8, "text/plain");
                    using (var response = await client.SendAsync(request))
                    {
                        var body = await response.Content.ReadAsStringAsync();
                        var title = response.IsSuccessStatusCode ? "Upload successful" : "Error uploading logs";

                        if (ZumoTestGlobals.ShowAlerts)
                        {
                            // do not show dialog if test run was started by the run all buttons; used in test automation scenarios
                            await MessageBox(body, title);
                        }
                        return body;
                    }
                }
            }
        }

        #region Methods which are different based on platforms

        /// <summary>
        /// Returns a task which completes after the specified delay.
        /// </summary>
        /// <param name="milliseconds">The number of milliseconds before the task completes.</param>
        /// <returns>A task which completes after the specified delay.</returns>
        public static Task TaskDelay(int milliseconds)
        {
#if !WP75
            return Task.Delay(milliseconds);
#else
            return Task.Factory.StartNew(() => Thread.Sleep(1000));
#endif
        }

        /// <summary>
        /// Retrieves an array of the values of the constants in a specified enumeration.
        /// </summary>
        /// <param name="enumType">An enumeration type.</param>
        /// <returns>An array that contains the values of the constants in <paramref name="enumType"/>.</returns>
        public static Array EnumGetValues(Type enumType)
        {
#if !WP75
            return Enum.GetValues(enumType);
#else
            if (enumType == null) throw new ArgumentNullException("enumType");
            if (!enumType.IsEnum) throw new ArgumentException("Argument must be an enum type");

            var fields = enumType.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            Array result = Array.CreateInstance(enumType, fields.Length);
            for (int i = 0; i < fields.Length; i++)
            {
                result.SetValue(fields[i].GetValue(null), i);
            }

            return result;
#endif
        }

        /// <summary>
        /// Returns a task with a stream to be used to read app settings.
        /// </summary>
        /// <param name="appSettingsFileName">The name of the file used to store the application settings.</param>
        /// <returns>A task with a stream to be used to read app settings.</returns>
        public static Task<Stream> OpenAppSettingsForRead(string appSettingsFileName)
        {
#if NET45
            return Task.FromResult(
                (Stream)File.OpenRead(
                    Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        appSettingsFileName)));
#else
#if !WP75
            return NetfxCoreOpenAppSettings(appSettingsFileName, false);
#else
            var isolatedStorage = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication();
                    return Task.Factory.StartNew<Stream>(
                        () => isolatedStorage.OpenFile(appSettingsFileName, FileMode.OpenOrCreate));
#endif
#endif
        }

#if !NET45 && !WP75
        private static async Task<Stream> NetfxCoreOpenAppSettings(string appSettingsFileName, bool forWrite)
        {
            if (forWrite)
            {
                var file = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync(
                    appSettingsFileName, Windows.Storage.CreationCollisionOption.ReplaceExisting);
                return await file.OpenStreamForWriteAsync();
            }
            else
            {
                var file = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFileAsync(appSettingsFileName);
                return await file.OpenStreamForReadAsync();
            }
        }
#endif

        public static Task<Stream> OpenAppSettingsForWrite(string appSettingsFileName)
        {
#if NET45
            return Task.FromResult(
                (Stream)File.OpenWrite(
                    Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        appSettingsFileName)));
#else
#if !WP75
            return NetfxCoreOpenAppSettings(appSettingsFileName, true);
#else
            var isolatedStorage = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication();
            return Task.Factory.StartNew<Stream>(
                () => isolatedStorage.OpenFile(appSettingsFileName, FileMode.Create));
#endif
#endif
        }

        public static Task MessageBox(string text, string title = null)
        {
#if NETFX_CORE
            return new Windows.UI.Popups.MessageDialog(text, title).ShowAsync().AsTask();
#else
            System.Windows.MessageBox.Show(text, title, System.Windows.MessageBoxButton.OK);
            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();
            tcs.SetResult(1);
            return tcs.Task;
#endif
        }
        #endregion

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

        public static bool CompareJson(JToken expected, JToken actual, List<string> errors)
        {
            if (expected == null)
            {
                return actual == null;
            }

            if (actual == null)
            {
                return false;
            }

            if (expected.Type != actual.Type)
            {
                errors.Add(string.Format("Expected value type {0} != actual {1}", expected.Type, actual.Type));
                return false;
            }

            switch (expected.Type)
            {
                case JTokenType.Boolean:
                    return expected.Value<bool>() == actual.Value<bool>();
                case JTokenType.Null:
                    return true;
                case JTokenType.String:
                case JTokenType.Date:
                    return expected.Value<string>() == actual.Value<string>();
                case JTokenType.Float:
                case JTokenType.Integer:
                    double expectedNumber = expected.Value<double>();
                    double actualNumber = actual.Value<double>();
                    double delta = 1 - expectedNumber / actualNumber;
                    double acceptableEpsilon = 0.000001;
                    if (Math.Abs(delta) > acceptableEpsilon)
                    {
                        errors.Add(string.Format("Numbers differ more than the allowed difference: {0} - {1}",
                            expected, actual));
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                case JTokenType.Array:
                    JArray expectedArray = (JArray)expected;
                    JArray actualArray = (JArray)actual;
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
                case JTokenType.Object:
                    JObject expectedObject = (JObject)expected;
                    JObject actualObject = (JObject)actual;
                    foreach (var child in expectedObject)
                    {
                        var key = child.Key;
                        if (key == "id") continue; // set by server, ignored at comparison

                        if (actualObject[key] == null)
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
                    throw new ArgumentException("Don't know how to compare JToken of type " + expected.Type);
            }
        }
    }
}
