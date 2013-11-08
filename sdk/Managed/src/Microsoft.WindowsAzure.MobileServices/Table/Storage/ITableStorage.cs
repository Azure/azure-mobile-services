// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices
{
    internal interface ITableStorage
    {
        /// <summary>
        /// Excutes a query against the storage context.
        /// </summary>
        /// <param name="tableName">
        /// The name of the table to query.
        /// </param>
        /// <param name="query">
        /// A query to execute.
        /// </param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to provide to the storage
        /// context.
        /// </param>
        /// <returns>
        /// A task that will return with results when the query finishes.
        /// </returns>
        Task<JToken> ReadAsync(string tableName, string query, IDictionary<string, string> parameters);

        /// <summary>
        /// Inserts an <paramref name="instance"/> into the storage context.
        /// </summary>
        /// <param name="tableName">
        /// The name of the insert the instance into.
        /// </param>
        /// <param name="instance">
        /// The instance to insert into the table.
        /// </param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to provide to the storage
        /// context.
        /// </param>
        /// <returns>
        /// A task that will complete when the insert finishes.
        /// </returns>
        Task<JToken> InsertAsync(string tableName, JToken instance, IDictionary<string, string> parameters);

        /// <summary>
        /// Updates an <paramref name="instance"/> in the storage context.
        /// </summary>
        /// <param name="tableName">
        /// The name of the table to update the instance in.
        /// </param>
        /// <param name="id">
        /// The id of the <paramref name="instance"/> to update in the table.
        /// </param>
        /// <param name="instance">
        /// The instance to update in the table.
        /// </param>
        /// <param name="version">
        /// The version of the object.
        /// </param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to provide to the storage
        /// context.
        /// </param>
        /// <returns>
        /// A task that will complete when the update finishes.
        /// </returns>
        Task<JToken> UpdateAsync(string tableName, object id, JToken instance, string version, IDictionary<string, string> parameters);

        /// <summary>
        /// Executes a lookup against the storage context.
        /// </summary>
        /// <param name="tableName">
        /// The name of the table to lookup the instance in.
        /// </param>
        /// <param name="id">
        /// The id of the instance to lookup.
        /// </param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to provide to the storage
        /// context.
        /// </param>
        /// <returns>
        /// A task that will return with a result when the lookup finishes.
        /// </returns>
        Task<JToken> LookupAsync(string tableName, object id, IDictionary<string, string> parameters);

        /// <summary>
        /// Deletes an instance with the given <paramref name="id"/> from the storage context.
        /// </summary>
        /// <param name="tableName">
        /// The name of the table to delete the instance from.
        /// </param>
        /// <param name="id">
        /// The id of the instance to delete from the table.
        /// </param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to provide to the storage
        /// context.
        /// </param>
        /// <returns>
        /// A task that will complete when the delete finishes.
        /// </returns>
        Task<JToken> DeleteAsync(string tableName, object id, IDictionary<string, string> parameters);
    }
}
