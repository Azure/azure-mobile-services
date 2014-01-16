using System;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Provides additional details of failed sync store operation
    /// </summary>
    public class MobileServiceSyncStoreException: Exception
    {
        /// <summary>
        /// Initializes an instance of <see cref="MobileServiceSyncStoreException"/>
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException"> The exception that is the cause of the current exception, or a null reference</param>
        public MobileServiceSyncStoreException(string message, Exception innerException): base(message, innerException)
        {
        }
    }
}
