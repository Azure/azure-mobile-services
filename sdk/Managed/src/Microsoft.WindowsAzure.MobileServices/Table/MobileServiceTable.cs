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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Provides operations on tables for a Windows Azure Mobile Service.
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

            TableName = tableName;
            MobileServiceClient = client;
            SystemProperties = MobileServiceSystemProperties.None;
        }

        /// <summary>
        /// Excutes a query against the table.
        /// </summary>
        /// <param name="query">
        /// A query to execute.
        /// </param>
        /// <returns>
        /// A task that will return with results when the query finishes.
        /// </returns>
        public Task<JToken> ReadAsync(string query)
        {
            return ReadAsync(query, null);
        }

        /// <summary>
        /// Excutes a query against the table.
        /// </summary>
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
        public async Task<JToken> ReadAsync(string query, IDictionary<string, string> parameters)
        {
            parameters = AddSystemProperties(SystemProperties, parameters);

            var uriPath = MobileServiceUrlBuilder.CombinePaths(TableRouteSeparatorName, TableName);
            var parametersString = MobileServiceUrlBuilder.GetQueryString(parameters);

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

            var uriString = MobileServiceUrlBuilder.CombinePathAndQuery(uriPath, query);

            var response = await MobileServiceClient.HttpClient.RequestAsync(HttpMethod.Get, uriString, MobileServiceClient.CurrentUser);
            return response.Content.ParseToJToken(MobileServiceClient.SerializerSettings);
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
            return InsertAsync(instance, null);
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
        public async Task<JToken> InsertAsync(JObject instance, IDictionary<string, string> parameters)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            CheckForProperIdCasing(instance);

            JToken id;
            // RWM: We're using this because instance.Property("id").Type always returns JTokenType.Property
            instance.TryGetValue("id", StringComparison.Ordinal, out id);
            if (id != null)
            {
                // RWM: Resist the urge to refactor this into a reusable function. Each table operation has unique validation logic.
                switch (id.Type)
                {
                    case JTokenType.String:
                        var stringResult = id.Value<string>();
                        // RWM: Deal with the null case explicitly, because the default MobileServiceClient behavior does not 
                        //      consider empties as null, which could short-circuit the null check in string.IsNullOrWhiteSpace() otherwise.
                        if (stringResult == null || (string.IsNullOrWhiteSpace(stringResult) && MobileServiceClient.TreatEmptyIdsAsNullOnInsert))
                        {
                            instance.Remove("id");
                            break;
                        }
                        // RWM: If we haven't removed the ID, validate it.
                        if (stringResult.Length > MobileServiceSerializer.MaxStringIdLength)
                        {
                            throw new InvalidOperationException(
                                string.Format(Resources.MobileServiceSerializer_StringIdTooLong,
                                id, 
                                MobileServiceSerializer.MaxStringIdLength));
                        }
                        if (MobileServiceSerializer.StringIdValidationRegex.IsMatch(stringResult))
                        {
                            throw new InvalidOperationException(
                                string.Format(Resources.MobileServiceSerializer_InvalidStringId,
                                id));
                        }
                        break;
                    case JTokenType.Float:
                    case JTokenType.Integer:
                        // RWM: May be better to cast the value to a data type that encompasses more numbers than long.
                        var numberResult = id.Value<long>();
                        if (numberResult > 0)
                        {
                            // RWM: Shouldn't be setting Integer IDs for Inserts. Warn the user.
                            throw GetArgumentException(Resources.MobileServiceTable_InsertWithExistingId,
                                    MobileServiceSerializer.IdPropertyName);
                        }
                        if (numberResult == 0)
                        {
                            instance.Remove("id");
                        }
                        if (numberResult < 0)
                        {
                            // RWM: Can't use negative IDs. Warn the user.
                            throw GetArgumentException(Resources.MobileServiceSerializer_InvalidIntegerId,
                                    MobileServiceSerializer.IdPropertyName);
                        }
                        break;
                    case JTokenType.Null:
                        // RWM: We have an ID token but no value. Remove the token.
                        instance.Remove("id");
                        break;
                }
            }

            parameters = AddSystemProperties(SystemProperties, parameters);

            var uriString = GetUri(TableName, null, parameters);
            var payload = instance.ToString(Formatting.None);
            var response = await MobileServiceClient.HttpClient.RequestAsync(HttpMethod.Post, uriString, MobileServiceClient.CurrentUser, payload);
            return GetJTokenFromResponse(response);
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
            return UpdateAsync(instance, null);
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
        public async Task<JToken> UpdateAsync(JObject instance, IDictionary<string, string> parameters)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
           
            MobileServiceInvalidOperationException error;
            string version = null;

            object id = MobileServiceSerializer.GetId(instance);
            if (!MobileServiceSerializer.IsIntegerId(id))
            {
                instance = RemoveSystemProperties(instance, out version);
            }

            parameters = AddSystemProperties(SystemProperties, parameters);

            try
            {
                Dictionary<string, string> headers = null;

                string content = instance.ToString(Formatting.None);
                string uriString = GetUri(TableName, id, parameters);

                if (version != null)
                {
                    headers = new Dictionary<string, string> {{"If-Match", GetEtagFromValue(version)}};
                }

                var response = await MobileServiceClient.HttpClient.RequestAsync(patchHttpMethod, uriString, MobileServiceClient.CurrentUser, content, true, headers);
                return GetJTokenFromResponse(response);
            }
            catch (MobileServiceInvalidOperationException ex)
            {
                if (ex.Response != null && 
                    ex.Response.StatusCode != HttpStatusCode.PreconditionFailed)
                {
                    throw;
                }

                error = ex;                    
            }

            var value = await ParseContent(error.Response);
            throw new MobileServicePreconditionFailedException(error, value);
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
            return DeleteAsync(instance, null);
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
        public async Task<JToken> DeleteAsync(JObject instance, IDictionary<string, string> parameters)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            CheckForProperIdCasing(instance);

            JToken id;
            // RWM: We're using this because instance.Property("id").Type always returns JTokenType.Property
            instance.TryGetValue("id", StringComparison.Ordinal, out id);
            if (id != null)
            {
                switch (id.Type)
                {
                    case JTokenType.String:
                        var stringResult = id.Value<string>();
                        if (string.IsNullOrWhiteSpace(stringResult))
                        {
                            throw new InvalidOperationException(Resources.MobileServiceSerializer_NullOrEmptyStringId);
                        }
                        if (stringResult.Length > MobileServiceSerializer.MaxStringIdLength)
                        {
                            throw new InvalidOperationException(
                                string.Format(Resources.MobileServiceSerializer_StringIdTooLong,
                                id, 
                                MobileServiceSerializer.MaxStringIdLength));
                        }
                        if (MobileServiceSerializer.StringIdValidationRegex.IsMatch(stringResult))
                        {
                            throw new InvalidOperationException(
                                string.Format(Resources.MobileServiceSerializer_InvalidStringId,
                                id));
                        }
                        break;
                    case JTokenType.Float:
                    case JTokenType.Integer:
                        // RWM: May be better to cast the value to a data type that encompasses more numbers than long.
                        var numberResult = id.Value<long>();
                        if (numberResult <= 0)
                        {
                            // RWM: Can't use negative IDs. Warn the user.
                            throw GetArgumentException(Resources.MobileServiceSerializer_InvalidIntegerId,
                                numberResult.ToString());
                        }
                        break;
                    case JTokenType.Null:
                        // RWM: We have an ID token but no value. Warn the user.
                        throw GetArgumentException(Resources.MobileServiceTable_DeleteWithoutId,
                            MobileServiceSerializer.IdPropertyName);
                    default:
                        // RWM: ID type is unsupported. Warn the user. Might not be the right string resource.
                        throw GetArgumentException(Resources.MobileServiceSerializer_IdTypeMismatch,
                            MobileServiceSerializer.IdPropertyName);
                }
            }
            else
            {
                throw GetArgumentException(Resources.MobileServiceSerializer_IdNotFound,
                            MobileServiceSerializer.IdPropertyName);
            }

            parameters = AddSystemProperties(SystemProperties, parameters);

            var uriString = GetUri(TableName, id, parameters);
            var response = await MobileServiceClient.HttpClient.RequestAsync(HttpMethod.Delete, uriString, MobileServiceClient.CurrentUser, null, false);
            return GetJTokenFromResponse(response);
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
            return LookupAsync(id, null);
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
        public async Task<JToken> LookupAsync(object id, IDictionary<string, string> parameters)
        {
            MobileServiceSerializer.EnsureValidId(id);

            parameters = AddSystemProperties(SystemProperties, parameters);

            var uriString = GetUri(TableName, id, parameters);
            var response = await MobileServiceClient.HttpClient.RequestAsync(HttpMethod.Get, uriString, MobileServiceClient.CurrentUser);
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
        /// Checks an instance of <see cref="JObject"/> to make sure the ID value has is in the proper casing.
        /// </summary>
        /// <param name="instance">The JObject instance to check.</param>
        /// <exception cref="ArgumentException">Thrown if the ID property is "ID" or "Id".</exception>
        private static void CheckForProperIdCasing(JObject instance)
        {
            // RWM: No need for complex lookups here: there are only 4 permutations of "ID", and only one works.
            if (instance.Property("ID") != null || instance.Property("Id") != null || instance.Property("iD") != null)
            {
                throw GetArgumentException(Resources.MobileServiceSerializer_IdCasingIncorrect, MobileServiceSerializer.IdPropertyName);
            }
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
        /// Parses the response content into a JToken and adds the version system property
        /// if the ETag was returned from the server.
        /// </summary>
        /// <param name="response">The response to parse.</param>
        /// <returns>The parsed JToken.</returns>
        private JToken GetJTokenFromResponse(MobileServiceHttpResponse response)
        {
            var jtoken = response.Content.ParseToJToken(MobileServiceClient.SerializerSettings);
            if (response.Etag != null)
            {
                jtoken[MobileServiceSerializer.VersionSystemPropertyString] = GetValueFromEtag(response.Etag);
            }

            return jtoken;
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

        private async Task<JToken> ParseContent(HttpResponseMessage response)
        {
            JToken value = null;
            try
            {
                if (response.Content != null)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    value = content.ParseToJToken(this.MobileServiceClient.SerializerSettings);
                }
            }
            catch { }
            return value;
        }

        /// <summary>
        /// Removes all system properties (name start with '__') from the instance
        /// if the instance is determined to have a string id and therefore be for table that
        /// supports system properties.
        /// </summary>
        /// <param name="instance">The instance to remove the system properties from.</param>
        /// <param name="version">Set to the value of the version system property before it is removed.</param>
        /// <returns>
        /// The instance with the system properties removed.
        /// </returns>
        protected static JObject RemoveSystemProperties(JObject instance, out string version)
        {
            version = null;

            bool haveCloned = false;
            foreach (JProperty property in instance.Properties())
            {
                if (property.Name.StartsWith(MobileServiceSerializer.SystemPropertyPrefix))
                {
                    // We don't want to alter the original jtoken passed in by the caller
                    // so if we find a system property to remove, we have to clone first
                    if (!haveCloned)
                    {
                        instance = instance.DeepClone() as JObject;
                        haveCloned = true;
                    }

                    if (String.Equals(property.Name, MobileServiceSerializer.VersionSystemPropertyString, StringComparison.OrdinalIgnoreCase))
                    {
                        version = (string)instance[property.Name];
                    }

                    instance.Remove(property.Name);
                }
            }

            return instance;
        }

        /// <summary>
        /// Creates a new ArgumentException for a given resource string and argument name.
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="argumentName"></param>
        /// <returns></returns>
        private static ArgumentException GetArgumentException(string resource, string argumentName)
        {
            var message = string.Format(CultureInfo.InvariantCulture, resource, argumentName);
            return new ArgumentException(message, "instance");
        }

    }
}
