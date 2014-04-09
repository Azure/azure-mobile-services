// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    /// <summary>
    /// Handles table operation errors and push completion results.
    /// </summary>
    public interface IMobileServiceSyncHandler
    {
        /// <summary>
        /// A method that is called when push operation has completed.
        /// </summary>
        /// <param name="result">An instance of <see cref="MobileServicePushCompletionResult"/></param>
        /// <returns>Task that completes when result has been handled.</returns>
        Task OnPushCompleteAsync(MobileServicePushCompletionResult result);

        /// <summary>
        /// A method that is called to execute a single table operation against remote table.
        /// </summary>
        /// <param name="operation">Instance of <see cref="IMobileServiceTableOperation"/> that represents a remote table operation.</param>
        /// <returns>Task that returns the server version of the item.</returns>
        Task<JObject> ExecuteTableOperationAsync(IMobileServiceTableOperation operation);
    }
}
