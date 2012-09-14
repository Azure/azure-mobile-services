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
        [SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames", MessageId = "1#", Justification = "Query is both a verb and noun.")]
        public Task<IJsonValue> ReadAsync(string query)
        {
            return this.SendReadAsync(query);
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
            return this.SendInsertAsync(instance);
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
            return this.SendUpdateAsync(instance);
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
            return this.SendDeleteAsync(instance);
        }
    }
}
