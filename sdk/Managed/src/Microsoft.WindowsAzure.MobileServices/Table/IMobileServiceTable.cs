﻿// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Provides operations on a table for a Microsoft Azure Mobile Service.
    /// </summary>
    public interface IMobileServiceTable
    {
        /// <summary>
        /// Gets a reference to the <see cref="MobileServiceClient"/> associated 
        /// with this table.
        /// </summary>
        MobileServiceClient MobileServiceClient { get; }

        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        string TableName { get; }

        /// <summary>
        /// The Mobile Service system properties to be included with items.
        /// </summary>
        MobileServiceSystemProperties SystemProperties { get; set; }

        /// <summary>
        /// Executes a query against the table.
        /// </summary>
        /// <param name="query">
        /// A query to execute.
        /// </param>
        /// <returns>
        /// A task that will return with results when the query finishes.
        /// </returns>
        Task<JToken> ReadAsync(string query);

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
        Task<JToken> ReadAsync(string query, IDictionary<string, string> parameters, bool wrapResult);

        /// <summary>
        /// Inserts an <paramref name="instance"/> into the table.
        /// </summary>
        /// <param name="instance">
        /// The instance to insert into the table.
        /// </param>
        /// <returns>
        /// A task that will complete when the insert finishes.
        /// </returns>
        Task<JToken> InsertAsync(JObject instance);

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
        Task<JToken> InsertAsync(JObject instance, IDictionary<string, string> parameters);

        /// <summary>
        /// Updates an <paramref name="instance"/> in the table.
        /// </summary>
        /// <param name="instance">
        /// The instance to update in the table.
        /// </param>
        /// <returns>
        /// A task that will complete when the update finishes.
        /// </returns>
        Task<JToken> UpdateAsync(JObject instance);

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
        Task<JToken> UpdateAsync(JObject instance, IDictionary<string, string> parameters);

        /// <summary>
        /// Deletes an <paramref name="instance"/> from the table.
        /// </summary>
        /// <param name="instance">
        /// The instance to delete from the table.
        /// </param>
        /// <returns>
        /// A task that will complete when the delete finishes.
        /// </returns>
        Task<JToken> DeleteAsync(JObject instance);

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
        Task<JToken> DeleteAsync(JObject instance, IDictionary<string, string> parameters);

        /// <summary>
        /// Undeletes an <paramref name="instance"/> from the table. This requires the soft delete feature to be enabled on the Mobile Service. Visit <see href="http://go.microsoft.com/fwlink/?LinkId=507647">the link</see> for details.
        /// </summary>
        /// <param name="instance">The instance to undelete from the table.</param>
        /// <returns>A task that will complete when the undelete finishes.</returns>
        Task<JToken> UndeleteAsync(JObject instance);

        /// <summary>
        /// Undeletes an <paramref name="instance"/> from the table. This requires the soft delete feature to be enabled on the Mobile Service. Visit <see href="http://go.microsoft.com/fwlink/?LinkId=507647">the link</see> for details.
        /// </summary>
        /// <param name="instance">The instance to undelete from the table.</param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to include in 
        /// the request URI query string.
        /// </param>
        /// <returns>A task that will complete when the undelete finishes.</returns>
        Task<JToken> UndeleteAsync(JObject instance, IDictionary<string, string> parameters);

        /// <summary>
        /// Executes a lookup against a table.
        /// </summary>
        /// <param name="id">
        /// The id of the instance to lookup.
        /// </param>
        /// <returns>
        /// A task that will return with a result when the lookup finishes.
        /// </returns>
        Task<JToken> LookupAsync(object id);

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
        Task<JToken> LookupAsync(object id, IDictionary<string, string> parameters);
    }
}
