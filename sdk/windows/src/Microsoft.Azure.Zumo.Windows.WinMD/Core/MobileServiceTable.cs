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
        internal const string TableRouteSeperatorName = "tables";

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
        /// Get a uri fragment representing the resource corresponding to the
        /// table.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <returns>A URI fragment representing the resource.</returns>
        private static string GetUriFragment(string tableName)
        {
            Debug.Assert(!string.IsNullOrEmpty(tableName),
                "tableName should not be null or empty!");
            return Path.Combine(TableRouteSeperatorName, tableName);
        }

        /// <summary>
        /// Get a uri fragment representing the resource corresponding to the
        /// given instance in the table.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="instance">The instance.</param>
        /// <returns>A URI fragment representing the resource.</returns>
        private static string GetUriFragment(string tableName, JsonObject instance)
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
        private static string GetUriFragment(string tableName, object id)
        {
            Debug.Assert(!string.IsNullOrEmpty(tableName),
                "tableName should not be null or empty!");
            Debug.Assert(id != null, "id should not be null!");

            string uriFragment = GetUriFragment(tableName);
            return Path.Combine(uriFragment, TypeExtensions.ToUriConstant(id));
        }

        /// <summary>
        /// Execute a query against a table.
        /// </summary>
        /// <param name="query">
        /// An object defining the query to execute.
        /// </param>
        /// <returns>
        /// A task that will return with results when the query finishes.
        /// </returns>
        internal async Task<IJsonValue> SendReadAsync(string query)
        {
            string uriFragment = GetUriFragment(this.TableName);
            if (!string.IsNullOrEmpty(query))
            {
                uriFragment += '?' + query.TrimStart('?');
            }

            return await this.MobileServiceClient.RequestAsync("GET", uriFragment, null);
        }

        /// <summary>
        /// Execute a lookup against a table.
        /// </summary>
        /// <param name="id">
        /// The id of the object to lookup.
        /// </param>
        /// <returns>
        /// A task that will return with results when the query finishes.
        /// </returns>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called via the strongly typed MobileServiceTable in the C# library only.")]
        internal async Task<IJsonValue> SendLookupAsync(object id)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }
            string uriFragment = GetUriFragment(this.TableName, id);
            return await this.MobileServiceClient.RequestAsync("GET", uriFragment, null);
        }

        /// <summary>
        /// Insert a new object into a table.
        /// </summary>
        /// <param name="instance">
        /// The instance to insert into the table.
        /// </param>
        /// <returns>
        /// A task that will complete when the insert finishes.
        /// </returns>
        internal async Task<IJsonValue> SendInsertAsync(JsonObject instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            // Make sure the instance doesn't have its ID set for an insertion
            if (instance.Get(IdPropertyName) != null)
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources.CannotInsertWithExistingIdMessage,
                        IdPropertyName),
                    "instance");
            }

            string url = GetUriFragment(this.TableName);
            IJsonValue response = await this.MobileServiceClient.RequestAsync("POST", url, instance);
            IJsonValue patched = Patch(instance, response);
            return patched;
        }

        /// <summary>
        /// Update an object in a given table.
        /// </summary>
        /// <param name="instance">
        /// The instance to update in the table.
        /// </param>
        /// <returns>
        /// A task that will complete when the update finishes.
        /// </returns>
        internal async Task<IJsonValue> SendUpdateAsync(JsonObject instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            string url = GetUriFragment(this.TableName, instance);
            IJsonValue response = await this.MobileServiceClient.RequestAsync("PATCH", url, instance);
            IJsonValue patched = Patch(instance, response);
            return patched;
        }

        /// <summary>
        /// Delete an object from a given table.
        /// </summary>
        /// <param name="instance">
        /// The instance to delete from the table.
        /// </param>
        /// <returns>
        /// A task that will complete when the delete finishes.
        /// </returns>
        internal async Task SendDeleteAsync(JsonObject instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            string url = GetUriFragment(this.TableName, instance);
            await this.MobileServiceClient.RequestAsync("DELETE", url, instance);
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
