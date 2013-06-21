// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
        /// The HTTP PATCH method used for update operations.
        /// </summary>
        private static readonly HttpMethod patchHttpMethod = new HttpMethod("PATCH");

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
        public async Task<JToken> ReadAsync(string query, IDictionary<string, string> parameters)
        {
            string uriFragment = MobileServiceUrlBuilder.GetUriFragment(this.TableName);
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

            string uriString = MobileServiceUrlBuilder.CombinePathAndQuery(uriFragment, query);

            string response = await this.MobileServiceClient.HttpClient.RequestAsync(HttpMethod.Get, uriString, null);
            return response.ParseToJToken();
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
        public async Task<JToken> InsertAsync(JObject instance, IDictionary<string, string> parameters)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            // Make sure the instance doesn't have an id set for an insertion
            MobileServiceSerializer serializer = this.MobileServiceClient.Serializer;
            object id = serializer.GetId(instance, true);
            if (!serializer.IsDefaultId(id))
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources.MobileServiceTable_InsertWithExistingId,
                        MobileServiceUrlBuilder.IdPropertyName),
                        "instance");
            }

            string response = await this.SendInsertAsync(instance.ToString(), parameters);
            return response.ParseToJToken();
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

            object id = this.GetIdFromInstance(instance);
            string response = await this.SendUpdateAsync(id, instance.ToString(), parameters);
            return response.ParseToJToken();
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
        public async Task<JToken> DeleteAsync(JObject instance, IDictionary<string, string> parameters)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            object id = this.GetIdFromInstance(instance);
            string response = await this.SendDeleteAsync(id, parameters);
            return response.ParseToJToken();
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
        public async Task<JToken> LookupAsync(object id, IDictionary<string, string> parameters)
        {
            if (this.MobileServiceClient.Serializer.IsDefaultId(id))
            {
                throw new ArgumentOutOfRangeException("id");
            }

            string response = await this.SendLookupAsync(id, parameters);
            return response.ParseToJToken();
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
        internal Task<string> SendInsertAsync(string instance, IDictionary<string, string> parameters)
        {
            Debug.Assert(instance != null);

            string uriFragment = MobileServiceUrlBuilder.GetUriFragment(this.TableName);
            string queryString = MobileServiceUrlBuilder.GetQueryString(parameters);
            string uriString = MobileServiceUrlBuilder.CombinePathAndQuery(uriFragment, queryString);

            return this.MobileServiceClient.HttpClient.RequestAsync(HttpMethod.Post, uriString, instance);
        }

        /// <summary>
        /// Updates an <paramref name="instance"/> in the table.
        /// </summary>
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
        internal Task<string> SendUpdateAsync(object id, string instance, IDictionary<string, string> parameters)
        {
            Debug.Assert(id != null);
            Debug.Assert(instance != null);

            string uriFragment = MobileServiceUrlBuilder.GetUriFragment(this.TableName, id);
            string queryString = MobileServiceUrlBuilder.GetQueryString(parameters);
            string uriString = MobileServiceUrlBuilder.CombinePathAndQuery(uriFragment, queryString);

            return this.MobileServiceClient.HttpClient.RequestAsync(patchHttpMethod, uriString, instance);
        }

        /// <summary>
        /// Deletes an instance with the given <paramref name="id"/> from the table.
        /// </summary>
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
        internal Task<string> SendDeleteAsync(object id, IDictionary<string, string> parameters)
        {
            Debug.Assert(id != null);

            string uriFragment = MobileServiceUrlBuilder.GetUriFragment(this.TableName, id);
            string queryString = MobileServiceUrlBuilder.GetQueryString(parameters);
            string uriString = MobileServiceUrlBuilder.CombinePathAndQuery(uriFragment, queryString);

            return this.MobileServiceClient.HttpClient.RequestAsync(HttpMethod.Delete, uriString, ensureResponseContent: false);
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
        internal Task<string> SendLookupAsync(object id, IDictionary<string, string> parameters)
        {
            Debug.Assert(id != null);

            string uriFragment = MobileServiceUrlBuilder.GetUriFragment(this.TableName, id);
            string queryString = MobileServiceUrlBuilder.GetQueryString(parameters);
            string uriString = MobileServiceUrlBuilder.CombinePathAndQuery(uriFragment, queryString);

            return this.MobileServiceClient.HttpClient.RequestAsync(HttpMethod.Get, uriString, null);
        }

        /// <summary>
        /// Returns the id of the <paramref name="instance"/>.
        /// </summary>
        /// <param name="instance">
        /// The instance that should have an id.
        /// </param>
        /// <returns>
        /// The id of the instance.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if the <paramref name="instance"/> does not have an id.
        /// </exception>
        private object GetIdFromInstance(JObject instance)
        {
            Debug.Assert(instance != null);

            // Get the value of the object (as a primitive JSON type)
            MobileServiceSerializer serializer = this.MobileServiceClient.Serializer;
            object id = serializer.GetId(instance);
            if (serializer.IsDefaultId(id)) 
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources.MobileServiceTable_IdNotFound,
                        MobileServiceUrlBuilder.IdPropertyName),
                    "instance");
            }           

            return id;
        }
    }
}
