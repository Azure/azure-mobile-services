// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Query;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    /// <summary>
    /// Executes all the table operations that are triggered by a Push
    /// </summary>
    internal class OperationBatch
    {
        private MobileServiceSyncContext context;

        /// <summary>
        /// Errors while interacting with store or calling push complete on handler
        /// </summary>
        public List<Exception> OtherErrors { get; private set; }

        /// <summary>
        /// Status that returns the reson of abort.
        /// </summary>
        public MobileServicePushStatus? AbortReason { get; private set; }        

        /// <summary>
        /// Instance of <see cref="IMobileServiceSyncHandler"/> that is used to execute batch operations.
        /// </summary>
        public IMobileServiceSyncHandler SyncHandler { get; private set; }

        /// <summary>
        /// Local store that operations and date are read from
        /// </summary>
        public IMobileServiceLocalStore Store { get; private set; }        

        public OperationBatch(IMobileServiceSyncHandler syncHandler, IMobileServiceLocalStore store, MobileServiceSyncContext context)
        {
            this.OtherErrors = new List<Exception>();
            this.SyncHandler = syncHandler;
            this.Store = store;
            this.context = context;
        }

        /// <summary>
        /// Changes the status of the operation batch to aborted with the specified reason.
        /// </summary>
        /// <param name="reason">Status value that respresents the reason of abort.</param>
        public void Abort(MobileServicePushStatus reason)
        {
            Debug.Assert(reason != MobileServicePushStatus.Complete);

            this.AbortReason = reason;
        }

        /// <summary>
        /// Adds a sync error to local store for this batch
        /// </summary>
        /// <param name="error">The sync error that occurred.</param>
        public Task AddSyncErrorAsync(MobileServiceTableOperationError error)
        {
            return this.Store.UpsertAsync(MobileServiceLocalSystemTables.SyncErrors, error.Serialize(), fromServer: false);
        }

        /// <summary>
        /// Loads all the sync errors in local store that are recorded for this batch.
        /// </summary>
        /// <param name="serializerSettings">the serializer settings to use for reading the errors.</param>
        /// <returns>List of sync errors.</returns>
        public async Task<IList<MobileServiceTableOperationError>> LoadSyncErrorsAsync(MobileServiceJsonSerializerSettings serializerSettings)
        {
            var errors = new List<MobileServiceTableOperationError>();

            JToken result = await this.Store.ReadAsync(new MobileServiceTableQueryDescription(MobileServiceLocalSystemTables.SyncErrors));
            if (result is JArray)
            {
                foreach (JObject error in result)
                {
                    var obj = MobileServiceTableOperationError.Deserialize(error, serializerSettings);
                    obj.Context = this.context;
                    errors.Add(obj);
                }
            }            

            return errors;
        }

        /// <summary>
        /// Checks if there are any unhandled sync or handler errors recorded for this batch.
        /// </summary>
        /// <param name="syncErrors">List of all handled and unhandled sync errors.</param>
        /// <returns>True if there are handler errors or unhandled sync errors, False otherwise.</returns>
        public bool HasErrors(IEnumerable<MobileServiceTableOperationError> syncErrors)
        {
            // unhandled sync errors or handler errors
            return syncErrors.Any(e => !e.Handled) || this.OtherErrors.Any();    
        }

        /// <summary>
        /// Deletes all the sync errors from local database
        /// </summary>
        public async Task DeleteErrorsAsync()
        {
            MobileServiceLocalStoreException toThrow = null;

            try
            {
                await this.Store.DeleteAsync(new MobileServiceTableQueryDescription(MobileServiceLocalSystemTables.SyncErrors));
            }
            catch (Exception ex)
            {

                toThrow = new MobileServiceLocalStoreException("Failed to delete error from the local store.", ex);
            }

            if (toThrow != null)
            {
                throw toThrow;
            }
        }
    }
}
