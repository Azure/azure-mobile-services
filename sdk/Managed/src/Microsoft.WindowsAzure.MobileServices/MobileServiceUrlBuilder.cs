// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// A static helper class for building URLs for Mobile Service tables.
    /// </summary>
    internal static class MobileServiceUrlBuilder
    {
        #region Constants

        /// <summary>
        /// Delimiter following the scheme in a URI.
        /// </summary>
        private const string SchemeDelimiter = "://";
        
        /// <summary>
        /// A constant variable that defines the character '/'.
        /// </summary>
        private const char Slash = '/';

        #endregion

        #region Methods

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
        /// <param name="useTableAPIRules">
        /// A boolean to indicate if query string paramters should be checked that they do not contain system added
        /// querystring. This currently only means to check if they match oData  queries (beginn with a $)
        /// </param>
        /// <returns>
        /// A URI query string.
        /// </returns>
        public static string GetQueryString(IDictionary<string, string> parameters, bool useTableAPIRules = true)
        {
            string parametersString = null;

            if (parameters != null && parameters.Count > 0)
            {
                parametersString = "";
                string formatString = "{0}={1}";
                foreach (var parameter in parameters)
                {
                    if (useTableAPIRules && parameter.Key.StartsWith("$"))
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
                                 "{0}{1}{2}",
                                 path1.TrimEnd(Slash),
                                 Slash,
                                 path2.TrimStart(Slash));
        }

        /// <summary>
        /// Appends a slash ('/') to <paramref name="uri"/> if it is missing a trailing slash.
        /// </summary>
        /// <param name="uri">
        /// URI to add a trailing slash to.
        /// </param>
        /// <returns>
        /// Uri with a slash appended to <paramref name="uri"/> if it is missing one.
        /// Else, <paramref name="uri"/> is returned unchanged.
        /// </returns>
        /// <remarks>
        /// No validation of the uri is performed.
        /// </remarks>
        public static string AddTrailingSlash(string uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            if (!uri.EndsWith(Slash.ToString()))
            {
                uri = uri + Slash;
            }

            return uri;
        }

        #endregion
    }
}