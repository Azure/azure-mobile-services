// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Windows.Data.Json;
using Windows.Foundation;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Provides operations on tables for a Mobile Service.
    /// </summary>
    internal partial class MobileServiceTable
    {
        /// <summary>
        /// Query a table.
        /// </summary>
        /// <param name="query">
        /// A query to execute.
        /// </param>
        /// <returns>
        /// A task that will return with results when the query finishes.
        /// </returns>
        public IAsyncOperation<IJsonValue> ReadAsync(string query)
        {
            return this.SendReadAsync(query, null).AsAsyncOperation();
        }

        /// <summary>
        /// Query a table.
        /// </summary>
        /// <param name="query">
        /// A query to execute.
        /// </param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to include in the request URI query string.
        /// </param>
        /// <returns>
        /// A task that will return with results when the query finishes.
        /// </returns>
        public IAsyncOperation<IJsonValue> ReadAsync(string query, IDictionary<string, string> parameters)
        {
            return this.SendReadAsync(query, parameters).AsAsyncOperation();
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
        public IAsyncOperation<IJsonValue> InsertAsync(JsonObject instance)
        {
            return this.SendInsertAsync(instance, null).AsAsyncOperation();
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
        public IAsyncOperation<IJsonValue> InsertAsync(JsonObject instance, IDictionary<string, string> parameters)
        {
            return this.SendInsertAsync(instance, parameters).AsAsyncOperation();
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
        public IAsyncOperation<IJsonValue> UpdateAsync(JsonObject instance)
        {
            return this.SendUpdateAsync(instance, null).AsAsyncOperation();
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
        public IAsyncOperation<IJsonValue> UpdateAsync(JsonObject instance, IDictionary<string, string> parameters)
        {
            return this.SendUpdateAsync(instance, parameters).AsAsyncOperation();
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
        public IAsyncAction DeleteAsync(JsonObject instance)
        {
            return this.SendDeleteAsync(instance, null).AsAsyncAction();
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
        public IAsyncAction DeleteAsync(JsonObject instance, IDictionary<string, string> parameters)
        {
            return this.SendDeleteAsync(instance, parameters).AsAsyncAction();
        }

        /// <summary>
        /// Execute a lookup against a table.
        /// </summary>
        /// <param name="id">
        /// The id of the object to lookup.
        /// </param>
        /// <returns>
        /// A task that will return with a result when the lookup finishes.
        /// </returns>
        public IAsyncOperation<IJsonValue> LookupAsync(object id)
        {
            return this.SendLookupAsync(id, null).AsAsyncOperation();
        }
    }
}
