// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// A static helper class for building URLs for Mobile Service tables.
    /// </summary>
    internal static class MobileServiceUrlBuilder
    {
        /// <summary>
        /// Converts a dictionary of string key-value pairs into a URI query string.
        /// </summary>
        /// <remarks>
        /// Both the query parameter and value will be percent encoded before being
        /// added to the query string.
        /// </remarks>
        /// <param name="parameters">
        /// The parameters from which to create the query string.
        /// </param>
        /// <returns>
        /// A URI query string.
        /// </returns>
        public static string GetQueryString(IDictionary<string, string> parameters)
        {
            string parametersString = null;

            if (parameters != null && parameters.Count > 0)
            {
                parametersString = "";
                string formatString = "{0}={1}";
                foreach (var parameter in parameters)
                {
                    if (parameter.Key.StartsWith("$"))
                    {
                        throw new ArgumentException(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                Resources.MobileServiceTableUrlBuilder_InvalidParameterBeginsWithDollarSign,
                                parameter.Key),
                            "parameters");
                    }

                    string escapedKey = Uri.EscapeDataString(parameter.Key);
                    string escapedValue = Uri.EscapeDataString(parameter.Value);
                    parametersString += string.Format(CultureInfo.InvariantCulture, 
                                                      formatString, 
                                                      escapedKey, 
                                                      escapedValue);
                    formatString = "&{0}={1}";
                }
            }

            return parametersString;
        }

        /// <summary>
        /// Concatenates the URI query string to the URI path.
        /// </summary>
        /// <param name="path">
        /// The URI path.
        /// </param>
        /// <param name="queryString">
        /// The query string.
        /// </param>
        /// <returns>
        /// The concatenated URI path and query string.
        /// </returns>
        public static string CombinePathAndQuery(string path, string queryString)
        {
            Debug.Assert(!string.IsNullOrEmpty(path));

            if (!string.IsNullOrEmpty(queryString))
            {
                path = string.Format(CultureInfo.InvariantCulture, "{0}?{1}", path,queryString.TrimStart('?'));
            }

            return path;
        }

        /// <summary>
        /// Concatenates two URI path segments into a single path and ensures
        /// that there is not an extra forward-slash.
        /// </summary>
        /// <param name="path1">
        /// The first path.
        /// </param>
        /// <param name="path2">
        /// The second path.
        /// </param>
        /// <returns>
        /// 
        /// </returns>
        public static string CombinePaths(string path1, string path2)
        {
            if (path1.Length == 0)
            {
                return path2;
            }

            if (path2.Length == 0)
            {
                return path1;
            }

            return string.Format(CultureInfo.InvariantCulture,
                                 "{0}/{1}",
                                 path1.TrimEnd('/'),
                                 path2.TrimStart('/'));
        }
    }
}