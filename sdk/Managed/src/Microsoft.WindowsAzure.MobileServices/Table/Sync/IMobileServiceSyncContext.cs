// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    /// <summary>
    /// Provides a way to synchronize local database with remote database.
    /// </summary>
    public interface IMobileServiceSyncContext
    {       
        /// <summary>
        /// An instance of <see cref="IMobileServiceLocalStore"/>
        /// </summary>
        IMobileServiceLocalStore Store { get; }

        /// <summary>
        /// An instance of <see cref="IMobileServiceSyncHandler"/>
        /// </summary>
        IMobileServiceSyncHandler Handler { get; }

        /// <summary>
        /// Indicates whether sync context has been initialized or not.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Initializes the sync context.
        /// </summary>
        /// <param name="store">An instance of <see cref="IMobileServiceLocalStore"/>.</param>
        /// <param name="handler">An instance of <see cref="IMobileServiceSyncHandler"/></param>
        /// <returns>A task that completes when sync context has initialized.</returns>
        Task InitializeAsync(IMobileServiceLocalStore store, IMobileServiceSyncHandler handler);

        /// <summary>
        /// Returns the no. of pending operations that are not yet pushed to remote table.
        /// </summary>
        /// <returns>A task that returns the number of pending operations against the remote table.</returns>
        int PendingOperations { get; }

        /// <summary>
        /// Pushes all pending operations up to the remote table.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> token to observe</param>
        /// <returns></returns>
        Task PushAsync(CancellationToken cancellationToken);
    }
}
