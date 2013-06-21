// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
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
        /// <returns>A JToken containing the response or null.</returns>
        public static JToken ParseToJToken(this string response)
        {
            return !string.IsNullOrEmpty(response) ? JToken.Parse(response) : null;
        }
    }
}

