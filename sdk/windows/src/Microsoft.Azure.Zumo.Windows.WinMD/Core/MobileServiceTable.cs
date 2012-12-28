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
    /// Provides operations on tables for a Mobile Service.
    /// </summary>
    internal partial class MobileServiceTable : IMobileServiceTable
    {
        /// <summary>
        /// Initializes a new instance of the MobileServiceTables class.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="client">
        /// Reference to the MobileServiceClient associated with this table.
        /// </param>
        public MobileServiceTable(string tableName, MobileServiceClient client)
        {
            Debug.Assert(tableName != null, "tableName cannot be null!");
            Debug.Assert(client != null, "client cannot be null!");
            this.TableName = tableName;
            this.MobileServiceClient = client;            
        }

        /// <summary>
        /// Gets a reference to the MobileServiceClient associated with this
        /// table.
        /// </summary>
        public MobileServiceClient MobileServiceClient { get; private set; }

        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        public string TableName { get; private set; }

        

        /// <summary>
        /// Execute a query against a table.
        /// </summary>
        /// <param name="queryString">
        /// An object defining the query to execute.
        /// </param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to include in the request URI query string.
        /// </param>
        /// <returns>
        /// A task that will return with results when the query finishes.
        /// </returns>
        internal async Task<IJsonValue> SendReadAsync(string queryString, IDictionary<string, string> parameters)
        {
            string uriFragment = MobileServiceTableUrlBuilder.GetUriFragment(this.TableName);
            string parametersString = MobileServiceTableUrlBuilder.GetQueryString(parameters);
            
            // Concatenate the query and the user-defined query string paramters
            if (!string.IsNullOrEmpty(parametersString))
            {
                if (!string.IsNullOrEmpty(queryString))
                {
                    queryString += '&' + parametersString;
                }
                else
                {
                    queryString = parametersString;
                }
            }

            string uriString = MobileServiceTableUrlBuilder.CombinePathAndQuery(uriFragment, queryString);

            return await this.MobileServiceClient.RequestAsync("GET", uriString, null);
        }

        /// <summary>
        /// Execute a lookup against a table.
        /// </summary>
        /// <param name="id">
        /// The id of the object to lookup.
        /// </param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to include in the request URI query string.
        /// </param>
        /// <returns>
        /// A task that will return with a result when the lookup finishes.
        /// </returns>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called via the strongly typed MobileServiceTable in the C# library only.")]
        internal async Task<IJsonValue> SendLookupAsync(object id, IDictionary<string, string> parameters)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }

            string uriFragment = MobileServiceTableUrlBuilder.GetUriFragment(this.TableName, id);
            string queryString = MobileServiceTableUrlBuilder.GetQueryString(parameters);
            string uriString = MobileServiceTableUrlBuilder.CombinePathAndQuery(uriFragment, queryString);

            return await this.MobileServiceClient.RequestAsync("GET", uriString, null);
        }

        /// <summary>
        /// Insert a new object into a table.
        /// </summary>
        /// <param name="instance">
        /// The instance to insert into the table.
        /// </param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to include in the request URI query string.
        /// </param>
        /// <returns>
        /// A task that will complete when the insert finishes.
        /// </returns>
        internal async Task<IJsonValue> SendInsertAsync(JsonObject instance, IDictionary<string, string> parameters)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            // Make sure the instance doesn't have its ID set for an insertion
            if (instance.Get(MobileServiceTableUrlBuilder.IdPropertyName) != null)
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources.CannotInsertWithExistingIdMessage,
                        MobileServiceTableUrlBuilder.IdPropertyName),
                    "instance");
            }

            string uriFragment = MobileServiceTableUrlBuilder.GetUriFragment(this.TableName);
            string queryString = MobileServiceTableUrlBuilder.GetQueryString(parameters);
            string uriString = MobileServiceTableUrlBuilder.CombinePathAndQuery(uriFragment, queryString);

            IJsonValue response = await this.MobileServiceClient.RequestAsync("POST", uriString, instance);
            IJsonValue patched = Patch(instance, response);
            return patched;
        }

        /// <summary>
        /// Update an object in a given table.
        /// </summary>
        /// <param name="instance">
        /// The instance to update in the table.
        /// </param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to include in the request URI query string.
        /// </param>
        /// <returns>
        /// A task that will complete when the update finishes.
        /// </returns>
        internal async Task<IJsonValue> SendUpdateAsync(JsonObject instance, IDictionary<string, string> parameters)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            string uriFragment = MobileServiceTableUrlBuilder.GetUriFragment(this.TableName, instance);
            string queryString = MobileServiceTableUrlBuilder.GetQueryString(parameters);
            string uriString = MobileServiceTableUrlBuilder.CombinePathAndQuery(uriFragment, queryString);

            IJsonValue response = await this.MobileServiceClient.RequestAsync("PATCH", uriString, instance);
            IJsonValue patched = Patch(instance, response);
            return patched;
        }

        /// <summary>
        /// Delete an object from a given table.
        /// </summary>
        /// <param name="instance">
        /// The instance to delete from the table.
        /// </param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to include in the request URI query string.
        /// </param>
        /// <returns>
        /// A task that will complete when the delete finishes.
        /// </returns>
        internal async Task SendDeleteAsync(JsonObject instance, IDictionary<string, string> parameters)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            string uriFragment = MobileServiceTableUrlBuilder.GetUriFragment(this.TableName, instance);
            string queryString = MobileServiceTableUrlBuilder.GetQueryString(parameters);
            string uriString = MobileServiceTableUrlBuilder.CombinePathAndQuery(uriFragment, queryString);

            await this.MobileServiceClient.RequestAsync("DELETE", uriString, null);
        }

        /// <summary>
        /// Patch an object with the values returned by from the server.  Given
        /// that it's possible for the server to change values on an insert or
        /// update, we want to make sure the client object reflects those
        /// changes.
        /// </summary>
        /// <param name="original">The first instance.</param>
        /// <param name="updated">The second instance.</param>
        /// <returns>
        /// The first instance patched with values from the second.
        /// </returns>
        private static IJsonValue Patch(IJsonValue original, IJsonValue updated)
        {
            JsonObject originalObj = original.AsObject();
            JsonObject updatedObj = updated.AsObject();
            if (originalObj != null && updatedObj != null)
            {
                foreach (KeyValuePair<string, JsonValue> property in updatedObj.GetPropertyValues())
                {
                    originalObj.SetNamedValue(property.Key, property.Value);
                }

                // TODO: Should we also delete any fields on the first object
                // that aren't also on the second object?  Is that a scenario
                // for scripts?
            }
            else
            {
                Debug.Assert(false, "Patch expects two JSON objects.");
            }

            return original;
        }
    }
}
