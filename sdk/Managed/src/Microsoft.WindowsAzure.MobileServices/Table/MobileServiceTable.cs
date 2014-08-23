// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Query;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Provides operations on tables for a Microsoft Azure Mobile Service.
    /// </summary>
    internal class MobileServiceTable : IMobileServiceTable
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
        /// The name of the _system query string parameter
        /// </summary>
        private const string SystemPropertiesQueryParameterName = "__systemproperties";

        /// <summary>
        /// Gets a reference to the <see cref="MobileServiceClient"/> associated 
        /// with this table.
        /// </summary>
        public MobileServiceClient MobileServiceClient { get; private set; }

        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        public string TableName { get; private set; }

        /// <summary>
        /// The Mobile Service system properties to be included with items.
        /// </summary>
        public MobileServiceSystemProperties SystemProperties { get; set; }

        /// <summary>
        /// Initializes a new instance of the MobileServiceTable class.
        /// </summary>
        /// <param name="tableName">
        /// The name of the table.
        /// </param>
        /// <param name="client">
        /// The <see cref="MobileServiceClient"/> associated with this table.
        /// </param>
        public MobileServiceTable(string tableName, MobileServiceClient client)
        {
            Debug.Assert(tableName != null);
            Debug.Assert(client != null);

            this.TableName = tableName;
            this.MobileServiceClient = client;
            this.SystemProperties = MobileServiceSystemProperties.None;
        }

        /// <summary>
        /// Executes a query against the table.
        /// </summary>
        /// <param name="query">
        /// A query to execute.
        /// </param>
        /// <returns>
        /// A task that will return with results when the query finishes.
        /// </returns>
        public Task<JToken> ReadAsync(string query)
        {
            return this.ReadAsync(query, null, wrapResult: false);
        }

        /// <summary>
        /// Executes a query against the table.
        /// </summary>
        /// <param name="query">
        /// A query to execute.
        /// </param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to include in 
        /// the request URI query string.
        /// </param>
        /// <param name="wrapResult">
        /// Specifies whether response should be formatted as JObject including extra response details e.g. link header
        /// </param>
        /// <returns>
        /// A task that will return with results when the query finishes.
        /// </returns>
        public virtual async Task<JToken> ReadAsync(string query, IDictionary<string, string> parameters, bool wrapResult)
        {
            QueryResult result = await this.ReadAsync(query, parameters, MobileServiceFeatures.UntypedTable);
            if (wrapResult)
            {
                return result.ToJObject();
            }
            else
            {
                return result.Response;
            }
        }

        /// <summary>
        /// Executes a query against the table.
        /// </summary>
        /// <param name="query">
        /// A query to execute.
        /// </param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to include in 
        /// the request URI query string.
        /// </param>
        /// <param name="features">
        /// Value indicating which features of the SDK are being used in this call. Useful for telemetry.
        /// </param>
        /// <returns>
        /// A task that will return with results when the query finishes.
        /// </returns>
        internal virtual async Task<QueryResult> ReadAsync(string query, IDictionary<string, string> parameters, MobileServiceFeatures features)
        {
            parameters = AddSystemProperties(this.SystemProperties, parameters);

            string uriPath;
            Uri uri;
            if (Uri.TryCreate(query, UriKind.Absolute, out uri))
            {
                features |= MobileServiceFeatures.ReadWithLinkHeader;
                uriPath = uri.GetComponents(UriComponents.Scheme | UriComponents.UserInfo | UriComponents.Host | UriComponents.Port | UriComponents.Path, UriFormat.UriEscaped);
                query = uri.Query;
            }
            else
            {
                uriPath = MobileServiceUrlBuilder.CombinePaths(TableRouteSeparatorName, this.TableName);
            }

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
            features = AddQueryParametersFeature(features, parameters);

            return await ReadAsync(uriString, features);
        }

        private async Task<QueryResult> ReadAsync(string uriString, MobileServiceFeatures features)
        {
            MobileServiceHttpResponse response = await this.MobileServiceClient.HttpClient.RequestAsync(HttpMethod.Get, uriString, this.MobileServiceClient.CurrentUser, null, true, features: features);

            return QueryResult.Parse(response, this.MobileServiceClient.SerializerSettings, validate: false);
        }


        /// <summary>
        /// Inserts an <paramref name="instance"/> into the table.
        /// </summary>
        /// <param name="instance">
        /// The instance to insert into the table.
        /// </param>
        /// <returns>
        /// A task that will complete when the insert finishes.
        /// </returns>
        public Task<JToken> InsertAsync(JObject instance)
        {
            return this.InsertAsync(instance, null);
        }

        /// <summary>
        /// Inserts an <paramref name="instance"/> into the table.
        /// </summary>
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
        public Task<JToken> InsertAsync(JObject instance, IDictionary<string, string> parameters)
        {
            return this.InsertAsync(instance, parameters, MobileServiceFeatures.UntypedTable);
        }

        /// <summary>
        /// Inserts an <paramref name="instance"/> into the table.
        /// </summary>
        /// <param name="instance">
        /// The instance to insert into the table.
        /// </param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to include in 
        /// the request URI query string.
        /// </param>
        /// <param name="features">
        /// Value indicating which features of the SDK are being used in this call. Useful for telemetry.
        /// </param>
        /// <returns>
        /// A task that will complete when the insert finishes.
        /// </returns>
        internal async Task<JToken> InsertAsync(JObject instance, IDictionary<string, string> parameters, MobileServiceFeatures features)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            // Make sure the instance doesn't have an int id set for an insertion
            object id = MobileServiceSerializer.GetId(instance, ignoreCase: false, allowDefault: true);
            bool isStringIdOrDefaultIntId = id is string || MobileServiceSerializer.IsDefaultId(id);
            if (!isStringIdOrDefaultIntId)
            {
                throw new ArgumentException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            Resources.MobileServiceTable_InsertWithExistingId,
                            MobileServiceSerializer.IdPropertyName),
                            "instance");
            }

            features = AddQueryParametersFeature(features, parameters);
            parameters = AddSystemProperties(this.SystemProperties, parameters);
            string uriString = GetUri(this.TableName, null, parameters);

            return await this.TransformHttpException(async () =>
            {
                MobileServiceHttpResponse response = await this.MobileServiceClient.HttpClient.RequestAsync(HttpMethod.Post, uriString, this.MobileServiceClient.CurrentUser, instance.ToString(Formatting.None), true, features: features);
                var result = GetJTokenFromResponse(response);
                return RemoveUnrequestedSystemProperties(result, parameters, response.Etag);
            });
        }

        /// <summary>
        /// Removes system properties from the passed in item
        /// </summary>
        /// <param name="item">
        /// The item returned from the server
        /// </param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to include in 
        /// the request URI query string.
        /// </param>
        /// <param name="version">
        /// Set to the value of the version system property before it is removed.
        /// </param>
        /// <returns>
        /// An item that only contains normal fields and the requested system properties
        /// </returns>
        private JToken RemoveUnrequestedSystemProperties(JToken item, IDictionary<string, string> parameters, string version)
        {
            object id = null;

            if (item == null)
            {
                return null;
            }

            MobileServiceSerializer.TryGetId(item as JObject, true, out id);
            if (id == null || MobileServiceSerializer.IsIntegerId(id))
            {
                return item;
            }

            var actualSystemProperties = GetRequestedSystemProperties(parameters);
            if (actualSystemProperties == MobileServiceSystemProperties.All)
            {
                return item;
            }

            string ignoredVersion = null;
            var cleanedItem = MobileServiceSerializer.RemoveSystemProperties(item as JObject, out ignoredVersion, actualSystemProperties);

            if (version != null)
            {
                cleanedItem[MobileServiceSerializer.VersionSystemPropertyString] = GetValueFromEtag(version);
            }

            return cleanedItem;
        }

        /// <summary>
        /// Calculates the requested system properties from the server
        /// </summary>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to include in 
        /// the request URI query string.
        /// </param>
        /// <returns>
        /// The calculated system properties value
        /// </returns>
        private MobileServiceSystemProperties GetRequestedSystemProperties(IDictionary<string, string> parameters)
        {
            var requestedSystemProperties = MobileServiceSystemProperties.None;

            // Parse dictionary for system properties override
            if (parameters != null && parameters.ContainsKey(SystemPropertiesQueryParameterName))
            {
                var sysProps = parameters[SystemPropertiesQueryParameterName];
                if (sysProps.IndexOf("*") >= 0)
                {
                    requestedSystemProperties = MobileServiceSystemProperties.All;
                }
                else
                {
                    if (sysProps.IndexOf(MobileServiceSystemProperties.Version.ToString(), StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        requestedSystemProperties |= MobileServiceSystemProperties.Version;
                    }
                    if (sysProps.IndexOf(MobileServiceSystemProperties.UpdatedAt.ToString(), StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        requestedSystemProperties |= MobileServiceSystemProperties.UpdatedAt;
                    }
                    if (sysProps.IndexOf(MobileServiceSystemProperties.CreatedAt.ToString(), StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        requestedSystemProperties |= MobileServiceSystemProperties.CreatedAt;
                    }
                }
            }
            else
            {
                requestedSystemProperties = this.SystemProperties;
            }

            return requestedSystemProperties;
        }

        /// <summary>
        /// Updates an <paramref name="instance"/> in the table.
        /// </summary>
        /// <param name="instance">
        /// The instance to update in the table.
        /// </param>
        /// <returns>
        /// A task that will complete when the update finishes.
        /// </returns>
        public Task<JToken> UpdateAsync(JObject instance)
        {
            return this.UpdateAsync(instance, null);
        }

        /// <summary>
        /// Updates an <paramref name="instance"/> in the table.
        /// </summary>
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
        public Task<JToken> UpdateAsync(JObject instance, IDictionary<string, string> parameters)
        {
            return this.UpdateAsync(instance, parameters, MobileServiceFeatures.UntypedTable);
        }

        /// <summary>
        /// Updates an <paramref name="instance"/> in the table.
        /// </summary>
        /// <param name="instance">
        /// The instance to update in the table.
        /// </param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to include in 
        /// the request URI query string.
        /// </param>
        /// <param name="features">
        /// Value indicating which features of the SDK are being used in this call. Useful for telemetry.
        /// </param>
        /// <returns>
        /// A task that will complete when the update finishes.
        /// </returns>
        internal async Task<JToken> UpdateAsync(JObject instance, IDictionary<string, string> parameters, MobileServiceFeatures features)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            features = AddQueryParametersFeature(features, parameters);
            object id = MobileServiceSerializer.GetId(instance);
            Dictionary<string, string> headers = StripSystemPropertiesAndAddVersionHeader(ref instance, ref parameters, id);
            string content = instance.ToString(Formatting.None);
            string uriString = GetUri(this.TableName, id, parameters);

            return await this.TransformHttpException(async () =>
            {
                MobileServiceHttpResponse response = await this.MobileServiceClient.HttpClient.RequestAsync(patchHttpMethod, uriString, this.MobileServiceClient.CurrentUser, content, true, headers, features);
                var result = GetJTokenFromResponse(response);
                return RemoveUnrequestedSystemProperties(result, parameters, response.Etag);
            });
        }

        /// <summary>
        /// Deletes an <paramref name="instance"/> from the table.
        /// </summary>
        /// <param name="instance">
        /// The instance to delete from the table.
        /// </param>
        /// <returns>
        /// A task that will complete when the delete finishes.
        /// </returns>
        public Task<JToken> DeleteAsync(JObject instance)
        {
            return this.DeleteAsync(instance, null);
        }

        /// <summary>
        /// Deletes an <paramref name="instance"/> from the table.
        /// </summary>
        /// <param name="instance">
        /// The instance to delete from the table.
        /// </param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to include in 
        /// the request URI query string.
        /// </param>
        /// <returns>
        /// A task that will complete when the delete finishes.
        /// </returns>
        public Task<JToken> DeleteAsync(JObject instance, IDictionary<string, string> parameters)
        {
            return this.DeleteAsync(instance, parameters, MobileServiceFeatures.UntypedTable);
        }

        /// <summary>
        /// Deletes an <paramref name="instance"/> from the table.
        /// </summary>
        /// <param name="instance">
        /// The instance to delete from the table.
        /// </param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to include in 
        /// the request URI query string.
        /// </param>
        /// <param name="features">
        /// Value indicating which features of the SDK are being used in this call. Useful for telemetry.
        /// </param>
        /// <returns>
        /// A task that will complete when the delete finishes.
        /// </returns>
        internal async Task<JToken> DeleteAsync(JObject instance, IDictionary<string, string> parameters, MobileServiceFeatures features)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            object id = MobileServiceSerializer.GetId(instance);
            features = AddQueryParametersFeature(features, parameters);
            Dictionary<string, string> headers = StripSystemPropertiesAndAddVersionHeader(ref instance, ref parameters, id);
            string uriString = GetUri(this.TableName, id, parameters);

            return await TransformHttpException(async () =>
            {
                MobileServiceHttpResponse response = await this.MobileServiceClient.HttpClient.RequestAsync(HttpMethod.Delete, uriString, this.MobileServiceClient.CurrentUser, null, false, headers, features);
                var result = GetJTokenFromResponse(response);
                return RemoveUnrequestedSystemProperties(result, parameters, response.Etag);
            });
        }

        /// <summary>
        /// Executes a lookup against a table.
        /// </summary>
        /// <param name="id">
        /// The id of the instance to lookup.
        /// </param>
        /// <returns>
        /// A task that will return with a result when the lookup finishes.
        /// </returns>
        public Task<JToken> LookupAsync(object id)
        {
            return this.LookupAsync(id, null);
        }

        /// <summary>
        /// Executes a lookup against a table.
        /// </summary>
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
        public Task<JToken> LookupAsync(object id, IDictionary<string, string> parameters)
        {
            return this.LookupAsync(id, parameters, MobileServiceFeatures.UntypedTable);
        }

        /// <summary>
        /// Executes a lookup against a table.
        /// </summary>
        /// <param name="id">
        /// The id of the instance to lookup.
        /// </param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to include in 
        /// the request URI query string.
        /// </param>
        /// <param name="features">
        /// Value indicating which features of the SDK are being used in this call. Useful for telemetry.
        /// </param>
        /// <returns>
        /// A task that will return with a result when the lookup finishes.
        /// </returns>
        internal async Task<JToken> LookupAsync(object id, IDictionary<string, string> parameters, MobileServiceFeatures features)
        {
            MobileServiceSerializer.EnsureValidId(id);

            features = AddQueryParametersFeature(features, parameters);
            parameters = AddSystemProperties(this.SystemProperties, parameters);

            string uriString = GetUri(this.TableName, id, parameters);
            MobileServiceHttpResponse response = await this.MobileServiceClient.HttpClient.RequestAsync(HttpMethod.Get, uriString, this.MobileServiceClient.CurrentUser, null, true, features: features);
            return GetJTokenFromResponse(response);
        }

        /// <summary>
        /// Adds the tables requested system properties to the parameters collection.
        /// </summary>
        /// <param name="systemProperties">The sytem properties to add.</param>
        /// <param name="parameters">The parameters collection.</param>
        /// <returns>
        /// The parameters collection with any requested system properties included.
        /// </returns>
        internal static IDictionary<string, string> AddSystemProperties(MobileServiceSystemProperties systemProperties, IDictionary<string, string> parameters)
        {
            // Make sure we have a case-insensitive parameters dictionary
            if (parameters != null)
            {
                parameters = new Dictionary<string, string>(parameters, StringComparer.OrdinalIgnoreCase);
            }

            // If there is already a user parameter for the system properties, just use it
            if (parameters == null || !parameters.ContainsKey(SystemPropertiesQueryParameterName))
            {
                string systemPropertiesString = GetSystemPropertiesString(systemProperties);
                if (systemPropertiesString != null)
                {
                    parameters = parameters ?? new Dictionary<string, string>();
                    parameters.Add(SystemPropertiesQueryParameterName, systemPropertiesString);
                }
            }

            return parameters;
        }

        /// <summary>
        /// Gets the system properties header value from the <see cref="MobileServiceSystemProperties"/>.
        /// </summary>
        /// <param name="properties">The system properties to set in the system properties header.</param>
        /// <returns>
        /// The system properties header value. Returns null if the systemProperty value is None.
        /// </returns>
        private static string GetSystemPropertiesString(MobileServiceSystemProperties properties)
        {
            if (properties == MobileServiceSystemProperties.None)
            {
                return null;
            }
            if (properties == MobileServiceSystemProperties.All)
            {
                return "*";
            }

            string[] systemProperties = properties.ToString().Split(',');

            for (int i = 0; i < systemProperties.Length; i++)
            {
                string property = systemProperties[i].Trim();
                char firstLetterAsLower = char.ToLowerInvariant(property[0]);
                systemProperties[i] = MobileServiceSerializer.SystemPropertyPrefix + firstLetterAsLower + property.Substring(1);
            }

            string systemPropertiesString = string.Join(",", systemProperties);
            return systemPropertiesString;
        }

        /// <summary>
        /// Parses body of the <paramref name="response"/> as JToken
        /// </summary>
        /// <param name="response">The http response message.</param>
        /// <returns>A pair of raw response and parsed JToken</returns>
        internal async Task<Tuple<string, JToken>> ParseContent(HttpResponseMessage response)
        {
            return await ParseContent(response, this.MobileServiceClient.SerializerSettings);
        }

        internal static async Task<Tuple<string, JToken>> ParseContent(HttpResponseMessage response, MobileServiceJsonSerializerSettings serializerSettings)
        {
            string content = null;
            JToken value = null;
            try
            {
                if (response.Content != null)
                {
                    content = await response.Content.ReadAsStringAsync();
                    value = content.ParseToJToken(serializerSettings);
                }
            }
            catch { }
            return Tuple.Create(content, value);
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
        private JToken GetJTokenFromResponse(MobileServiceHttpResponse response)
        {
            JToken jtoken = response.Content.ParseToJToken(this.MobileServiceClient.SerializerSettings);
            if (response.Etag != null)
            {
                jtoken[MobileServiceSerializer.VersionSystemPropertyString] = GetValueFromEtag(response.Etag);
            }

            return jtoken;
        }

        /// <summary>
        /// Adds, if applicable, the <see cref="MobileServiceFeatures.AdditionalQueryParameters"/> value to the
        /// existing list of features used in the current operation.
        /// </summary>
        /// <param name="existingFeatures">The features from the SDK being used for the current operation.</param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to include in
        /// the request URI query string.
        /// </param>
        /// <returns>The features used in the current operation.</returns>
        private static MobileServiceFeatures AddQueryParametersFeature(MobileServiceFeatures existingFeatures, IDictionary<string, string> parameters)
        {
            if (parameters != null && parameters.Count > 0)
            {
                existingFeatures |= MobileServiceFeatures.AdditionalQueryParameters;
            }

            return existingFeatures;
        }

        /// <summary>
        /// Executes a request and transforms a 412 and 409 response to respective exception type.
        /// </summary>
        private async Task<JToken> TransformHttpException(Func<Task<JToken>> action)
        {
            MobileServiceInvalidOperationException error = null;

            try
            {
                return await action();
            }
            catch (MobileServiceInvalidOperationException ex)
            {
                if (ex.Response != null &&
                    ex.Response.StatusCode != HttpStatusCode.PreconditionFailed &&
                    ex.Response.StatusCode != HttpStatusCode.Conflict)
                {
                    throw;
                }

                error = ex;
            }

            Tuple<string, JToken> responseContent = await this.ParseContent(error.Response);
            JObject value = responseContent.Item2 as JObject;
            if (error.Response.StatusCode == HttpStatusCode.Conflict)
            {
                error = new MobileServiceConflictException(error, value);
            }
            else if (error.Response.StatusCode == HttpStatusCode.PreconditionFailed)
            {
                error = new MobileServicePreconditionFailedException(error, value);
            }
            throw error;
        }

        /// <summary>
        /// if id is of type string then it strips the system properties and adds version header.
        /// </summary>
        /// <returns>The header collection with if-match header.</returns>
        private Dictionary<string, string> StripSystemPropertiesAndAddVersionHeader(ref JObject instance, ref IDictionary<string, string> parameters, object id)
        {
            string version = null;
            if (!MobileServiceSerializer.IsIntegerId(id))
            {
                instance = MobileServiceSerializer.RemoveSystemProperties(instance, out version);
            }
            parameters = AddSystemProperties(this.SystemProperties, parameters);
            Dictionary<string, string> headers = AddIfMatchHeader(version, null);
            return headers;
        }

        /// <summary>
        /// Adds If-Match header to request if version is non-null.
        /// </summary>
        private static Dictionary<string, string> AddIfMatchHeader(string version, Dictionary<string, string> headers)
        {
            if (!String.IsNullOrEmpty(version))
            {
                if (headers == null)
                {
                    headers = new Dictionary<string, string>();
                }

                headers.Add("If-Match", GetEtagFromValue(version));
            }

            return headers;
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
        /// <param name="etag">The etag to get the value from.</param>
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
