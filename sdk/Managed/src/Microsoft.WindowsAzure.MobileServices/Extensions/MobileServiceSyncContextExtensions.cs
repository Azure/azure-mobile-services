using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    /// <summary>
    /// Provides extension methods on <see cref="IMobileServiceSyncContext"/>
    /// </summary>
    public static class MobileServiceSyncContextExtensions
    {
        /// <summary>
        /// Pushes all pending operations up to the remote table.
        /// </summary>
        public static Task PushAsync(this IMobileServiceSyncContext context)
        {
            return context.PushAsync(CancellationToken.None);
        }

        /// <summary>
        /// Initializes the sync context.
        /// </summary>
        /// <param name="store">An instance of <see cref="IMobileServiceLocalStore"/>.</param>
        public static Task InitializeAsync(this IMobileServiceSyncContext context, IMobileServiceLocalStore store)
        {
            return context.InitializeAsync(store, new MobileServiceSyncHandler());
        }
    }
}
