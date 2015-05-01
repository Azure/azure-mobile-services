// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    /// <summary>
    /// An exception thrown when push does not complete successfully.
    /// </summary>
    public class MobileServicePushFailedException : Exception
    {
        /// <summary>
        /// Result of push operation
        /// </summary>
        public MobileServicePushCompletionResult PushResult { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="MobileServicePushFailedException"/>
        /// </summary>
        /// <param name="pushResult">Result of push operation.</param>
        /// <param name="innerException">Inner exception that caused the push to fail.</param>
        public MobileServicePushFailedException(MobileServicePushCompletionResult pushResult, Exception innerException)
            : base("Push operation has failed. See the PushResult for details.", innerException)
        {
            this.PushResult = pushResult;
        }
    }
}
