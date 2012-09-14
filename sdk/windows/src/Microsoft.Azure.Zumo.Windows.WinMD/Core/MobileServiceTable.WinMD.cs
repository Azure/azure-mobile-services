// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
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
        /// Query a collection.
        /// </summary>
        /// <param name="query">
        /// An object defining the query to execute.
        /// </param>
        /// <returns>
        /// A task that will return with results when the query finishes.
        /// </returns>
        public IAsyncOperation<IJsonValue> ReadAsync(string query)
        {
            return this.SendReadAsync(query).AsAsyncOperation();
        }

        /// <summary>
        /// Insert a new object into a collection.
        /// </summary>
        /// <param name="instance">
        /// The instance to insert into the collection.
        /// </param>
        /// <returns>
        /// A task that will complete when the insert finishes.
        /// </returns>
        public IAsyncOperation<IJsonValue> InsertAsync(JsonObject instance)
        {
            return this.SendInsertAsync(instance).AsAsyncOperation();
        }

        /// <summary>
        /// Update an object in a given collection.
        /// </summary>
        /// <param name="instance">
        /// The instance to update in the collection.
        /// </param>
        /// <returns>
        /// A task that will complete when the update finishes.
        /// </returns>
        public IAsyncOperation<IJsonValue> UpdateAsync(JsonObject instance)
        {
            return this.SendUpdateAsync(instance).AsAsyncOperation();
        }

        /// <summary>
        /// Delete an object from a given collection.
        /// </summary>
        /// <param name="instance">
        /// The instance to delete from the collection.
        /// </param>
        /// <returns>
        /// A task that will complete when the delete finishes.
        /// </returns>
        public IAsyncAction DeleteAsync(JsonObject instance)
        {
            return this.SendDeleteAsync(instance).AsAsyncAction();
        }
    }
}
