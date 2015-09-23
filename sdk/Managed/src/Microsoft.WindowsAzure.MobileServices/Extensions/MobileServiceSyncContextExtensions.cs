// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Sync;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Provides extension methods on <see cref="IMobileServiceSyncContext"/>
    /// </summary>
    public static class MobileServiceSyncContextExtensions
    {
        /// <summary>
        /// Replays all pending local operations against the remote tables.
        /// </summary>
        public static Task PushAsync(this IMobileServiceSyncContext context)
        {
            return context.PushAsync(CancellationToken.None);
        }

        /// <summary>
        /// Initializes the sync context.
        /// </summary>
        /// <param name="context">An instance of <see cref="IMobileServiceSyncContext"/>.</param>
        /// <param name="store">An instance of <see cref="IMobileServiceLocalStore"/>.</param>
        public static Task InitializeAsync(this IMobileServiceSyncContext context, IMobileServiceLocalStore store)
        {
            return context.InitializeAsync(store, new MobileServiceSyncHandler());
        }

        /// <summary>
        /// Initializes the sync context.
        /// </summary>
        /// <param name="context">An instance of <see cref="IMobileServiceSyncContext"/>.</param>
        /// <param name="store">An instance of <see cref="IMobileServiceLocalStore"/>.</param>
        /// <param name="trackingOptions">The traking options that should be enabled on this instance of <see cref="IMobileServiceSyncContext"/>.</param>
        /// <returns>A task that completes when the initialization when initialization has finished.</returns>
        public static Task InitializeAsync(this IMobileServiceSyncContext context, IMobileServiceLocalStore store, StoreTrackingOptions trackingOptions)
        {
            return context.InitializeAsync(store, new MobileServiceSyncHandler(), trackingOptions);
        }
    }
}
