// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// An implementation of <see cref="ITableStorage"/> for working with
    /// remote Mobile Services data.
    /// </summary>
    internal class RemoteTableStorage : ITableStorage
    {
        /// <summary>
        /// The route separator used to denote the table in a uri like
        /// .../{app}/tables/{coll}.
        /// </summary>
        internal const string TableRouteSeparatorName = "tables";

        /// <summary>
        /// The HTTP PATCH method used for update operations.
        /// </summary>
        private static readonly HttpMethod patchHttpMethod = new HttpMethod("PATCH");        

        /// <summary>
        /// The <see cref="MobileServiceHttpClient"/> instance to use for the storage context.
        /// </summary>
        private MobileServiceHttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteTableStorage"/> class.
        /// </summary>
        /// <param name="httpClient">
        /// The <see cref="MobileServiceHttpClient"/> instance to use for the storage context.
        /// </param>
        /// <param name="tableName">
        /// The table name to use with the storage context.
        /// </param>
        public RemoteTableStorage(MobileServiceHttpClient httpClient)
        {
            Debug.Assert(httpClient != null);

            this.httpClient = httpClient;
        }

        /// <summary>
        /// Excutes a query against the table.
        /// </summary>
        /// <param name="tableName">
        /// The name of the table to query.
        /// </param>
        /// <param name="query">
        /// A query to execute.
        /// </param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to include in 
        /// the request URI query string.
        /// </param>
        /// <returns>
        /// A task that will return with results when the query finishes.
        /// </returns>
        public async Task<JToken> ReadAsync(string tableName, string query, IDictionary<string, string> parameters)
        {
            string uriPath = MobileServiceUrlBuilder.CombinePaths(TableRouteSeparatorName, tableName);
            string parametersString = MobileServiceUrlBuilder.GetQueryString(parameters);

            // Concatenate the query and the user-defined query string parameters
            if (!string.IsNullOrEmpty(parametersString))
            {
                if (!string.IsNullOrEmpty(query))
                {
                    query += '&' + parametersString;
                }
                else
                {
                    query = parametersString;
                }
            }

            string uriString = MobileServiceUrlBuilder.CombinePathAndQuery(uriPath, query);

            MobileServiceHttpResponse response = await this.httpClient.RequestAsync(HttpMethod.Get, uriString, null, true);
            return response.Content.ParseToJToken();
        }

        /// <summary>
        /// Inserts an <paramref name="instance"/> into the table.
        /// </summary>
        /// <param name="tableName">
        /// The name of the insert the instance into.
        /// </param>
        /// <param name="instance">
        /// The instance to insert into the table.
        /// </param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to include in 
        /// the request URI query string.
        /// </param>
        /// <returns>
        /// A task that will complete when the insert finishes.
        /// </returns>
        public async Task<JToken> InsertAsync(string tableName, JToken instance, IDictionary<string, string> parameters)
        {
            Debug.Assert(instance != null);

            string uriString = GetUri(tableName, null, parameters);
            MobileServiceHttpResponse response = await this.httpClient.RequestAsync(HttpMethod.Post, uriString, instance.ToString(Formatting.None), true);
            return GetJTokenFromResponse(response);
        }

        /// <summary>
        /// Updates an <paramref name="instance"/> in the table.
        /// </summary>
        /// <param name="tableName">
        /// The name of the table to update the instance in.
        /// </param>
        /// <param name="id">
        /// The id of the <paramref name="instance"/> to update in the table.
        /// </param>
        /// <param name="instance">
        /// The instance to update in the table.
        /// </param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to include in 
        /// the request URI query string.
        /// </param>
        /// <returns>
        /// A task that will complete when the update finishes.
        /// </returns>
        public async Task<JToken> UpdateAsync(string tableName, object id, JToken instance, string version, IDictionary<string, string> parameters)
        {
            Debug.Assert(id != null);
            Debug.Assert(instance != null);

            Dictionary<string, string> headers = null;

            string content = instance.ToString(Formatting.None);
            string uriString = GetUri(tableName, id, parameters);

            if (version != null)
            {
                headers = new Dictionary<string, string>();
                headers.Add("If-Match", GetEtagFromValue(version));
            }

            MobileServiceHttpResponse response = await this.httpClient.RequestAsync(patchHttpMethod, uriString, content, true, headers);
            return GetJTokenFromResponse(response);
        }

        /// <summary>
        /// Executes a lookup against a table.
        /// </summary>
        /// <param name="tableName">
        /// The name of the table to lookup the instance in.
        /// </param>
        /// <param name="id">
        /// The id of the instance to lookup.
        /// </param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to include in 
        /// the request URI query string.
        /// </param>
        /// <returns>
        /// A task that will return with a result when the lookup finishes.
        /// </returns>
        public async Task<JToken> LookupAsync(string tableName, object id, IDictionary<string, string> parameters)
        {
            Debug.Assert(id != null);

            string uriString = GetUri(tableName, id, parameters);
            MobileServiceHttpResponse response = await this.httpClient.RequestAsync(HttpMethod.Get, uriString, null, true);
            return GetJTokenFromResponse(response);
        }

        /// <summary>
        /// Deletes an instance with the given <paramref name="id"/> from the table.
        /// </summary>
        /// <param name="tableName">
        /// The name of the table to delete the instance from.
        /// </param>
        /// <param name="id">
        /// The id of the instance to delete from the table.
        /// </param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to include in 
        /// the request URI query string.
        /// </param>
        /// <returns>
        /// A task that will complete when the delete finishes.
        /// </returns>
        public async Task<JToken> DeleteAsync(string tableName, object id, IDictionary<string, string> parameters)
        {
            Debug.Assert(id != null);

            string uriString = GetUri(tableName, id, parameters);
            MobileServiceHttpResponse response = await this.httpClient.RequestAsync(HttpMethod.Delete, uriString, null, false);
            return GetJTokenFromResponse(response);
        }

        /// <summary>
        /// Returns a URI for the table, optional id and parameters.
        /// </summary>
        /// <param name="tableName">
        /// The name of the table.
        /// </param>
        /// <param name="id">
        /// The id of the instance.
        /// </param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to include in 
        /// the request URI query string.
        /// </param>
        /// <returns>
        /// A URI string.
        /// </returns>
        private static string GetUri(string tableName, object id = null, IDictionary<string, string> parameters = null)
        {
            Debug.Assert(!string.IsNullOrEmpty(tableName));

            string uriPath = MobileServiceUrlBuilder.CombinePaths(TableRouteSeparatorName, tableName);
            if (id != null)
            {
                string idString = Uri.EscapeDataString(string.Format(CultureInfo.InvariantCulture, "{0}", id));
                uriPath = MobileServiceUrlBuilder.CombinePaths(uriPath, idString);
            }

            string queryString = MobileServiceUrlBuilder.GetQueryString(parameters);

            return MobileServiceUrlBuilder.CombinePathAndQuery(uriPath, queryString);
        }        

        /// <summary>
        /// Parses the response content into a JToken and adds the version system property
        /// if the ETag was returned from the server.
        /// </summary>
        /// <param name="response">The response to parse.</param>
        /// <returns>The parsed JToken.</returns>
        private static JToken GetJTokenFromResponse(MobileServiceHttpResponse response)
        {
            JToken jtoken = response.Content.ParseToJToken();
            if (response.Etag != null)
            {
                jtoken[MobileServiceSerializer.VersionSystemPropertyString] = GetValueFromEtag(response.Etag);
            }

            return jtoken;
        }

        /// <summary>
        /// Gets a valid etag from a string value. Etags are surrounded
        /// by double quotes and any internal quotes must be escaped with a 
        /// '\'.
        /// </summary>
        /// <param name="value">The value to create the etag from.</param>
        /// <returns>
        /// The etag.
        /// </returns>
        private static string GetEtagFromValue(string value)
        {
            // If the value has double quotes, they will need to be escaped.
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == '"' && (i == 0 || value[i - 1] != '\\'))
                {
                    value = value.Insert(i, "\\");
                }
            }

            // All etags are quoted;
            return string.Format("\"{0}\"", value);
        }

        /// <summary>
        /// Gets a value from an etag. Etags are surrounded
        /// by double quotes and any internal quotes must be escaped with a 
        /// '\'.
        /// </summary>
        /// <param name="value">The etag to get the value from.</param>
        /// <returns>
        /// The value.
        /// </returns>
        private static string GetValueFromEtag(string etag)
        {
            int length = etag.Length;
            if (length > 1 && etag[0] == '\"' && etag[length - 1] == '\"')
            {
                etag = etag.Substring(1, length - 2);
            }

            return etag.Replace("\\\"", "\"");
        }
    }
}
