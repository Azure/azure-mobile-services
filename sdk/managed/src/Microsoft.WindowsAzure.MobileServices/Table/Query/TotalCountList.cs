// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// The TotalCountList class provides records returned by a query along
    /// with the total count for all the records that would have been returned
    /// ignoring any take paging/limit clause specified by client or server.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the elements in the list.
    /// </typeparam>
    internal class TotalCountList<T> : List<T>, ITotalCountProvider
    {
        /// <summary>
        /// Initializes a new instance of the TotalCountList class.
        /// </summary>
        /// <param name="sequence">
        /// The sequence whose elements comprise the list.
        /// </param>
        public TotalCountList(IEnumerable<T> sequence)
            : base(sequence)
        {
            this.TotalCount = -1;

            // Forward along the total count from our sequence if it was
            // provided
            ITotalCountProvider provider = sequence as ITotalCountProvider;
            if (provider != null)
            {
                this.TotalCount = provider.TotalCount;
            }
        }

        /// <summary>
        /// Gets the total count for all the records that would have been
        /// returned ignoring any take paging/limit clause specified by client
        /// or server.
        /// </summary>
        public long TotalCount { get; private set; }
    }
}
