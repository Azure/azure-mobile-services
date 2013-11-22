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
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Provides operations on tables for a Windows Azure Mobile Service.
    /// </summary>
    internal class MobileServiceTable : IMobileServiceTable
    {
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
        /// The underlying storage for the table.
        /// </summary>
        protected ITableStorage StorageContext { get; private set; }

        /// <summary>
        /// Initializes a new instance of the MobileServiceTable class.
        /// </summary>
        /// <param name="tableName">
        /// The name of the table.
        /// </param>
        /// <param name="client">
        /// The <see cref="MobileServiceClient"/> associated with this table.
        /// </param>
        /// <param name="storageContext">
        /// The <see cref="ITableStorage"/> implementation to use with this table.
        /// </param>
        public MobileServiceTable(string tableName, MobileServiceClient client, ITableStorage storageContext)
        {
            Debug.Assert(tableName != null);
            Debug.Assert(client != null);
            Debug.Assert(storageContext != null);

            this.TableName = tableName;
            this.MobileServiceClient = client;
            this.StorageContext = storageContext;
            this.SystemProperties = MobileServiceSystemProperties.None;
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
            return this.ReadAsync(query, null);
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
        public Task<JToken> ReadAsync(string query, IDictionary<string, string> parameters)
        {
            parameters = AddSystemProperties(this.SystemProperties, parameters);

            return this.StorageContext.ReadAsync(this.TableName, query, parameters);
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
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            // Make sure the instance doesn't have an int id set for an insertion
            object id = MobileServiceSerializer.GetId(instance, true, true);
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

            // If there is an id, it is either a default int or a string id
            if (id != null)
            {
                // Make sure only proper casing is allowed
                if (MobileServiceSerializer.GetId(instance, ignoreCase: false, allowDefault: true) == null)
                {
                    throw new ArgumentException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            Resources.MobileServiceSerializer_IdCasingIncorrect,
                            MobileServiceSerializer.IdPropertyName),
                            "instance");
                }
            }

            parameters = AddSystemProperties(this.SystemProperties, parameters);

            return this.StorageContext.InsertAsync(this.TableName, instance, parameters);
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
        public async Task<JToken> UpdateAsync(JObject instance, IDictionary<string, string> parameters)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }            
            
            MobileServiceInvalidOperationException error = null;

            object id = MobileServiceSerializer.GetId(instance);
            string version = null;
            if (!MobileServiceSerializer.IsIntegerId(id))
            {
                instance = RemoveSystemProperties(instance, out version);
            }
            parameters = AddSystemProperties(this.SystemProperties, parameters);

            try
            {                
                return await this.StorageContext.UpdateAsync(this.TableName, id, instance, version, parameters);
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

            JToken value = await ParseContent(error.Response);
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
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            object id = MobileServiceSerializer.GetId(instance);
            parameters = AddSystemProperties(this.SystemProperties, parameters);

            return this.StorageContext.DeleteAsync(this.TableName, id, parameters);
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
            MobileServiceSerializer.EnsureValidId(id);

            parameters = AddSystemProperties(this.SystemProperties, parameters);

            return this.StorageContext.LookupAsync(this.TableName, id, parameters);
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
        /// Gets the system properties header value from the <see cref="MobileServiceSystemProperty"/>.
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

        private static async Task<JToken> ParseContent(HttpResponseMessage response)
        {
            JToken value = null;
            try
            {
                if (response.Content != null)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    value = content.ParseToJToken();
                }
            }
            catch { }
            return value;
        }
    }
}
