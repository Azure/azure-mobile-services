// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices
{
    internal static class StringExtensions
    {
        /// <summary>
        /// Parses the response into a JToken.
        /// If the response is null or empty, null will be returned.
        /// </summary>
        /// <param name="response">The response to parse.</param>
        /// <param name="settings">The serializer settings used for parsing the response.</param>
        /// <returns>A JToken containing the response or null.</returns>
        public static JToken ParseToJToken(this string response, JsonSerializerSettings settings)
        {
            if (String.IsNullOrEmpty(response))
            {
                return null;
            }
            using (var reader = new JsonTextReader(new StringReader(response)))
            {
                reader.DateParseHandling = settings.DateParseHandling;
                reader.DateTimeZoneHandling = settings.DateTimeZoneHandling;
                reader.FloatParseHandling = settings.FloatParseHandling;
                reader.Culture = settings.Culture;

                return JToken.Load(reader);
            }
        }
    }
}

