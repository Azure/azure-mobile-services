// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.WindowsAzure.MobileServices
{
    internal static class HttpUtility
    {
        /// <summary>
        /// Parses a query string into a <see cref="System.Collections.Generic.IDictionary{TKey, TValue}"/>
        /// </summary>
        /// <param name="query">The query string to parse.</param>
        /// <returns>An <see cref="System.Collections.Generic.IDictionary{TKey, TValue}"/> of query parameters and values.</returns>
        public static IDictionary<string, string> ParseQueryString(string query)
        {
            char[] separator = new[] { '=' };
            var parameters = query.Split('&')
                                  .Select(part => part.Split(separator, 2))
                                  .ToDictionary(x => Uri.UnescapeDataString(x[0]), x => x.Length > 1 ? Uri.UnescapeDataString(x[1]) : String.Empty);

            return parameters;
        }
    }
}