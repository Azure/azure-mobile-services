// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    /// <summary>
    /// Handles table operation errors and push completion results.
    /// </summary>
    public class MobileServiceSyncHandler : IMobileServiceSyncHandler
    {
        /// <summary>
        /// A method that is called when push operation has completed.
        /// </summary>
        /// <param name="result">An instance of <see cref="MobileServicePushCompletionResult"/></param>
        /// <returns>Task that completes when result has been handled.</returns>
        public virtual Task OnPushCompleteAsync(MobileServicePushCompletionResult result)
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// A method that is called to execute a single table operation against remote table.
        /// </summary>
        /// <param name="operation">Instance of <see cref="IMobileServiceTableOperation"/> that represents a remote table operation.</param>
        /// <returns>Task that completes when operation has been executed and errors have been handled.</returns>
        public virtual Task<JObject> ExecuteTableOperationAsync(IMobileServiceTableOperation operation)
        {
            return operation.ExecuteAsync();
        }
    }
}
