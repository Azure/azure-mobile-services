// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    /// <summary>
    /// State in which push completed.
    /// </summary>
    public enum MobileServicePushStatus
    {
        /// <summary>
        /// All table operations in the push action were completed, possibly with errors.
        /// </summary>
        Complete = 0,

        /// <summary>
        /// Push was aborted due to network error.
        /// </summary>
        CancelledByNetworkError = 1,
        
        /// <summary>
        /// Push was aborted due to authentication error.
        /// </summary>
        CancelledByAuthenticationError = 2,
        
        /// <summary>
        /// Push was aborted due to error from sync store.
        /// </summary>
        CancelledBySyncStoreError = 3,
        
        /// <summary>
        /// Push was aborted due to cancellation.
        /// </summary>
        CancelledByToken = 4,

        /// <summary>
        /// Push was aborted by <see cref="IMobileServiceTableOperation"/>.
        /// </summary>
        CancelledByOperation = 5,

        /// <summary>
        /// Push failed due to an internal error.
        /// </summary>
        InternalError = Int32.MaxValue
    }
}
