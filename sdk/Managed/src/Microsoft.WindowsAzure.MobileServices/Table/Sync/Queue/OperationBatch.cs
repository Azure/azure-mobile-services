// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Query;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    internal class OperationBatch
    {
        /// <summary>
        /// Errors while processing queue operations
        /// </summary>
        public List<Exception> HandlerErrors { get; private set; }

        public MobileServicePushStatus? Status { get; private set; }        

        public IMobileServiceSyncHandler SyncHandler { get; private set; }

        public IMobileServiceLocalStore Store { get; private set; }

        public bool IsAborted 
        { 
            get { return this.Status.HasValue; } 
        }

        public OperationBatch(IMobileServiceSyncHandler syncHandler, IMobileServiceLocalStore store)
        {
            this.HandlerErrors = new List<Exception>();
            this.SyncHandler = syncHandler;
            this.Store = store;
        }

        public void Abort(MobileServicePushStatus status)
        {
            this.Status = status;
        }

        public Task AddSyncErrorAsync(MobileServiceTableOperationError error)
        {
            return this.Store.UpsertAsync(LocalSystemTables.SyncErrors, error.Serialize());
        }

        public async Task<IList<MobileServiceTableOperationError>> LoadSyncErrorsAsync(MobileServiceJsonSerializerSettings serializerSettings)
        {
            var errors = new List<MobileServiceTableOperationError>();

            JToken result = await this.Store.ReadAsync(new MobileServiceTableQueryDescription(LocalSystemTables.SyncErrors));
            if (result is JArray)
            {
                foreach (JObject error in result)
                {
                    errors.Add(MobileServiceTableOperationError.Deserialize(error, serializerSettings));
                }
            }

            return errors;
        }

        public bool HasErrors(IEnumerable<MobileServiceTableOperationError> syncErrors)
        {
            // unhandled sync errors or handler errors
            return syncErrors.Any(e => !e.Handled) || this.HandlerErrors.Any();    
        }

        public async Task DeleteErrorsAsync(IEnumerable<MobileServiceTableOperationError> syncErrors)
        {
            MobileServiceSyncStoreException toThrow = null;

            foreach (MobileServiceTableOperationError error in syncErrors)
            {
                try
                {
                    await this.Store.DeleteAsync(LocalSystemTables.SyncErrors, error.Id);
                }
                catch (Exception ex)
                {

                    toThrow = new MobileServiceSyncStoreException(Resources.SyncStore_FailedToDeleteError, ex);
                }
            }

            if (toThrow != null)
            {
                throw toThrow;
            }
        }
    }
}
