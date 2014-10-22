// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    /// <summary>
    /// Gives you errors and status of the push completion.
    /// </summary>
    public class MobileServicePushCompletionResult
    {
        /// <summary>
        /// Errors caused by executing operation against remote table.
        /// </summary>
        public ReadOnlyCollection<MobileServiceTableOperationError> Errors { get; private set; }

        /// <summary>
        /// The state in which push completed.
        /// </summary>
        public MobileServicePushStatus Status { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="MobileServicePushCompletionResult"/>
        /// </summary>
        /// <param name="errors">Collection of errors that occured on executing operation on remote table.</param>
        /// <param name="status">The state in which push completed.</param>
        public MobileServicePushCompletionResult(IEnumerable<MobileServiceTableOperationError> errors, MobileServicePushStatus status)
        {
            this.Errors = new ReadOnlyCollection<MobileServiceTableOperationError>(errors as IList<MobileServiceTableOperationError> ?? new List<MobileServiceTableOperationError>(errors));
            this.Status = status;
        }
    }
}
