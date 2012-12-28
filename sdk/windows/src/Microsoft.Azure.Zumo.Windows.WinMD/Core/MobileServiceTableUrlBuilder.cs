// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// A static helper class for building URLs for Mobile Service tables.
    /// </summary>
    internal static class MobileServiceTableUrlBuilder
    {
        /// <summary>
        /// Name of the reserved Mobile Services ID member.
        /// </summary>
        /// <remarks>
        /// Note: This value is used by other areas like serialiation to find
        /// the name of the reserved ID member.
        /// </remarks>
        internal const string IdPropertyName = "id";

        /// <summary>
        /// The route separator used to denote the table in a uri like
        /// .../{app}/tables/{coll}.
        /// </summary>
        internal const string TableRouteSeparatorName = "tables";

        /// <summary>
        /// Get a uri fragment representing the resource corresponding to the
        /// table.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <returns>A URI fragment representing the resource.</returns>
        public static string GetUriFragment(string tableName)
        {
            Debug.Assert(!string.IsNullOrEmpty(tableName),
                "tableName should not be null or empty!");
            return Path.Combine(TableRouteSeparatorName, tableName);
        }

        /// <summary>
        /// Get a uri fragment representing the resource corresponding to the
        /// given instance in the table.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="instance">The instance.</param>
        /// <returns>A URI fragment representing the resource.</returns>
        public static string GetUriFragment(string tableName, JsonObject instance)
        {
            Debug.Assert(!string.IsNullOrEmpty(tableName),
                "tableName should not be null or empty!");

            // Get the value of the object (as a primitive JSON type)
            object id = null;
            if (!instance.Get(IdPropertyName).TryConvert(out id) || id == null)
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources.IdNotFoundExceptionMessage,
                        IdPropertyName),
                    "instance");
            }

            return GetUriFragment(tableName, id);
        }

        /// <summary>
        /// Get a uri fragment representing the resource corresponding to the
        /// given id in the table.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="id">The id of the instance.</param>
        /// <returns>A URI fragment representing the resource.</returns>
        public static string GetUriFragment(string tableName, object id)
        {
            Debug.Assert(!string.IsNullOrEmpty(tableName),
                "tableName should not be null or empty!");
            Debug.Assert(id != null, "id should not be null!");

            string uriFragment = GetUriFragment(tableName);
            return Path.Combine(uriFragment, TypeExtensions.ToUriConstant(id));
        }

        /// <summary>
        /// Converts a dictionary of string key-value pairs into a URI query string
        /// </summary>
        /// <param name="parameters">The parameters from which to create a query string.</param>
        /// <returns>A URI query string.</returns>
        public static string GetQueryString(IDictionary<string, string> parameters)
        {
            string parametersString = null;

            if (parameters != null)
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
                                Resources.InvalidParameterBeginsWithDollarSign,
                                parameter.Key),
                            "parameters");
                    }
                    string escapedKey = Uri.EscapeDataString(parameter.Key);
                    string escapedValue = Uri.EscapeDataString(parameter.Value);
                    parametersString += string.Format(CultureInfo.InvariantCulture, formatString, escapedKey, escapedValue);
                    formatString = "&{0}={1}";
                }
            }

            return parametersString;
        }

        /// <summary>
        /// Concatenates the URI query string to the URI path.
        /// </summary>
        /// <param name="path">The URI path</param>
        /// <param name="queryString">The query string.</param>
        /// <returns>The concatenated URI path and query string.</returns>
        public static string CombinePathAndQuery(string path, string queryString)
        {
            Debug.Assert(!string.IsNullOrEmpty(path), "path should not be null or empty!");

            if (!string.IsNullOrEmpty(queryString))
            {
                path += '?' + queryString.TrimStart('?');
            }

            return path;
        }
    }
}