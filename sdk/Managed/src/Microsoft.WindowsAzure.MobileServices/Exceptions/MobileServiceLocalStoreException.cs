// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    /// <summary>
    /// Provides additional details of failed sync store operation
    /// </summary>
    public class MobileServiceLocalStoreException: Exception
    {
        /// <summary>
        /// Initializes an instance of <see cref="MobileServiceLocalStoreException"/>
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException"> The exception that is the cause of the current exception, or a null reference</param>
        public MobileServiceLocalStoreException(string message, Exception innerException): base(message, innerException)
        {
        }
    }
}
