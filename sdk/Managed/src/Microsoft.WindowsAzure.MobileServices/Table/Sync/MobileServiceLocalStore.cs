// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Query;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    /// <summary>
    /// Base implementation for <see cref="IMobileServiceLocalStore"/>
    /// </summary>
    public abstract class MobileServiceLocalStore: IMobileServiceLocalStore
    {
        /// <summary>
        /// Indicates whether store has been initialized or not.
        /// </summary>
        protected bool Initialized { get; private set; }

        /// <summary>
        /// Initializes the store for use.
        /// </summary>
        /// <returns>A task that completes when store has initialized.</returns>
        public virtual async Task InitializeAsync()
        {
            if (this.Initialized)
            {
                throw new InvalidOperationException("The store is already initialized.");
            }

            MobileServiceLocalSystemTables.DefineAll(this);

            await this.OnInitialize();

            this.Initialized = true;
        }

        /// <summary>
        /// Executed when Initilize is called on the store instance.
        /// </summary>
        /// <returns></returns>
        protected virtual Task OnInitialize()
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// Defines the local table on the store.
        /// </summary>
        /// <param name="tableName">Name of the local table.</param>
        /// <param name="item">An object that represents the structure of the table.</param>
        public virtual void DefineTable(string tableName, JObject item)
        {
        }

        /// <summary>
        /// Reads data from local table by executing the query.
        /// </summary>
        /// <param name="query">Instance of <see cref="MobileServiceTableQueryDescription"/> that defines the query to be executed on local table.</param>
        /// <returns>A task that returns instance of JObject or JArray with items matching the query.</returns>
        public abstract Task<JToken> ReadAsync(MobileServiceTableQueryDescription query);

        /// <summary>
        /// Updates or inserts data in local table.
        /// </summary>
        /// <param name="tableName">Name of the local table.</param>
        /// <param name="items">A list of items to be inserted.</param>
        /// <param name="ignoreMissingColumns"><code>true</code> if the extra properties on item can be ignored; <code>false</code> otherwise.</param>        
        /// <returns>A task that completes when item has been upserted in local table.</returns>
        public abstract Task UpsertAsync(string tableName, IEnumerable<JObject> items, bool ignoreMissingColumns);

        /// <summary>
        /// Deletes all the items from local table that match the query.
        /// </summary>
        /// <param name="query">Instance of <see cref="MobileServiceTableQueryDescription"/></param>
        /// <returns>A task that completes when delete has been executed on local table.</returns>
        public abstract Task DeleteAsync(MobileServiceTableQueryDescription query);

        /// <summary>
        /// Deletes items from local table with the given list of ids
        /// </summary>
        /// <param name="tableName">Name of the local table.</param>
        /// <param name="ids">A list of ids of the items to be deleted</param>
        /// <returns>A task that completes when delete query has executed.</returns>
        public abstract Task DeleteAsync(string tableName, IEnumerable<string> ids);

        /// <summary>
        /// Reads a single item from local table with specified id.
        /// </summary>
        /// <param name="tableName">Name of the local table.</param>
        /// <param name="id">Id for the object to be read.</param>
        /// <returns>A task that returns the item read from local table.</returns>
        public abstract Task<JObject> LookupAsync(string tableName, string id);

        /// <summary>
        /// Throws an exception if store is not initialized.
        /// </summary>
        protected void EnsureInitialized()
        {
            if (!this.Initialized)
            {
                throw new InvalidOperationException("The store must be initialized before it can be used.");
            }
        }

        /// <summary>
        /// Dispose the instance
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Executed to clean up resources
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
