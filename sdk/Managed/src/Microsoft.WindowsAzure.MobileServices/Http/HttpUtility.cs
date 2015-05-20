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
        /// Tries to parse the query as relative or absolute uri
        /// </summary>
        /// <param name="applicationUri">The application uri to use as base</param>
        /// <param name="query">The query string that may be relative path starting with slash or absolute uri</param>
        /// <param name="uri">The uri in case it was relative path or absolute uri</param>
        /// <param name="absolute">Returns true if the uri was absolute uri</param>
        /// <returns>True if it was relative or absolute uri, False otherwise</returns>
        public static bool TryParseQueryUri(Uri applicationUri, string query, out Uri uri, out bool absolute)
        {
            if (query.StartsWith("/") && Uri.TryCreate(applicationUri, query, out uri))
            {
                absolute = false;
                return true;
            }
            else if (Uri.TryCreate(query, UriKind.Absolute, out uri))
            {
                if (uri.Host != applicationUri.Host)
                {
                    throw new ArgumentException("The query uri must be on the same host as the Mobile Service.");
                }

                absolute = true;
                return true;
            }
            else
            {
                absolute = false;
                return false;
            }
        }

        /// Returns the complete uri excluding the query part
        public static string GetUriWithoutQuery(Uri uri)
        {
            string path = uri.GetComponents(UriComponents.Scheme | UriComponents.UserInfo | UriComponents.Host | UriComponents.Port | UriComponents.Path, UriFormat.UriEscaped);
            return path;
        }

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
