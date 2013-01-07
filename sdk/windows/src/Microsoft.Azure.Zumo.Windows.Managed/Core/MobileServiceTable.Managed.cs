// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Provides operations on tables for a Mobile Service.
    /// </summary>
    internal partial class MobileServiceTable
    {
        /// <summary>
        /// Gets the name of the results key in an inline count response
        /// object.
        /// </summary>
        protected const string InlineCountResultsKey = "results";

        /// <summary>
        /// Gets the name of the count key in an inline count response object.
        /// </summary>
        protected const string InlineCountCountKey = "count";

        /// <summary>
        /// Excute a query against a table.
        /// </summary>
        /// <param name="query">
        /// An object defining the query to execute.
        /// </param>
        /// <returns>
        /// A task that will return with results when the query finishes.
        /// </returns>
        public Task<IJsonValue> ReadAsync(string query)
        {
            return this.SendReadAsync(query, null);
        }

        /// <summary>
        /// Excute a query against a table.
        /// </summary>
        /// <param name="query">
        /// An object defining the query to execute.
        /// </param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to include in the request URI query string.
        /// </param>
        /// <returns>
        /// A task that will return with results when the query finishes.
        /// </returns>
        public Task<IJsonValue> ReadAsync(string query, IDictionary<string, string> parameters)
        {
            return this.SendReadAsync(query, parameters);
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
        public Task<IJsonValue> InsertAsync(JsonObject instance)
        {
            return this.SendInsertAsync(instance, null);
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
        public Task<IJsonValue> InsertAsync(JsonObject instance, IDictionary<string, string> parameters)
        {
            return this.SendInsertAsync(instance, parameters);
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
        public Task<IJsonValue> UpdateAsync(JsonObject instance)
        {
            return this.SendUpdateAsync(instance, null);
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
        public Task<IJsonValue> UpdateAsync(JsonObject instance, IDictionary<string, string> parameters)
        {
            return this.SendUpdateAsync(instance, parameters);
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
        public Task DeleteAsync(JsonObject instance)
        {
            return this.SendDeleteAsync(instance, null);
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
        public Task DeleteAsync(JsonObject instance, IDictionary<string, string> parameters)
        {
            return this.SendDeleteAsync(instance, parameters);
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
        public Task<IJsonValue> LookupAsync(object id)
        {
            return this.SendLookupAsync(id, null);
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
        public Task<IJsonValue> LookupAsync(object id, IDictionary<string, string> parameters)
        {
            return this.SendLookupAsync(id, parameters);
        }
    }
}
