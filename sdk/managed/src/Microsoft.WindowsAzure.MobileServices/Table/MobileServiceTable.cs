// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
            string uriFragment = MobileServiceTableUrlBuilder.GetUriFragment(this.TableName);
            string parametersString = MobileServiceTableUrlBuilder.GetQueryString(parameters);

            // Concatenate the query and the user-defined query string paramters
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

            string uriString = MobileServiceTableUrlBuilder.CombinePathAndQuery(uriFragment, query);

            string response = await this.MobileServiceClient.HttpClient.RequestAsync("GET", uriString, null);
            return JToken.Parse(response);
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
            if (this.MobileServiceClient.Serializer.GetId(instance) != null)
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources.MobileServiceTable_InsertWithExistingId,
                        MobileServiceTableUrlBuilder.IdPropertyName),
                        "instance");
            }

            string response = await this.SendInsertAsync(instance.ToString(), parameters);
            return JToken.Parse(response);
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
            return JToken.Parse(response);
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
        public Task DeleteAsync(JObject instance)
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
        public async Task DeleteAsync(JObject instance, IDictionary<string, string> parameters)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            object id = this.GetIdFromInstance(instance);
            await this.SendDeleteAsync(id, parameters);
            this.MobileServiceClient.Serializer.ClearId(instance);
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
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }

            string response = await this.SendLookupAsync(id, parameters);
            return JToken.Parse(response);
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

            string uriFragment = MobileServiceTableUrlBuilder.GetUriFragment(this.TableName);
            string queryString = MobileServiceTableUrlBuilder.GetQueryString(parameters);
            string uriString = MobileServiceTableUrlBuilder.CombinePathAndQuery(uriFragment, queryString);

            return this.MobileServiceClient.HttpClient.RequestAsync("POST", uriString, instance);
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

            string uriFragment = MobileServiceTableUrlBuilder.GetUriFragment(this.TableName, id);
            string queryString = MobileServiceTableUrlBuilder.GetQueryString(parameters);
            string uriString = MobileServiceTableUrlBuilder.CombinePathAndQuery(uriFragment, queryString);

            return this.MobileServiceClient.HttpClient.RequestAsync("PATCH", uriString, instance);
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
        internal Task SendDeleteAsync(object id, IDictionary<string, string> parameters)
        {
            Debug.Assert(id != null);

            string uriFragment = MobileServiceTableUrlBuilder.GetUriFragment(this.TableName, id);
            string queryString = MobileServiceTableUrlBuilder.GetQueryString(parameters);
            string uriString = MobileServiceTableUrlBuilder.CombinePathAndQuery(uriFragment, queryString);

            return this.MobileServiceClient.HttpClient.RequestAsync("DELETE", uriString, ensureResponseContent: false);
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
        public Task<string> SendLookupAsync(object id, IDictionary<string, string> parameters)
        {
            Debug.Assert(id != null);

            string uriFragment = MobileServiceTableUrlBuilder.GetUriFragment(this.TableName, id);
            string queryString = MobileServiceTableUrlBuilder.GetQueryString(parameters);
            string uriString = MobileServiceTableUrlBuilder.CombinePathAndQuery(uriFragment, queryString);

            return this.MobileServiceClient.HttpClient.RequestAsync("GET", uriString, null);
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
            object id = this.MobileServiceClient.Serializer.GetId(instance);
            if (id == null)
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources.MobileServiceTable_IdNotFound,
                        MobileServiceTableUrlBuilder.IdPropertyName),
                    "instance");
            }

            return id;
        }
    }
}
