// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// The QueryResultList{T} class provides next link, records returned by a query
    /// and the total count for all the records that would have been returned
    /// ignoring any take paging/limit clause specified by client or server.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the elements in the list.
    /// </typeparam>
#pragma warning disable 618 // for implementing obsolete ITotalCountProvider
    internal class QueryResultList<T> : List<T>, ITotalCountProvider, IQueryResultEnumerable<T>
    {
        /// <summary>
        /// Initializes a new instance of the QueryResultList{T} class.
        /// </summary>
        /// <param name="sequence">
        /// The sequence whose elements comprise the list.
        /// </param>
        public QueryResultList(IEnumerable<T> sequence)
            : base(sequence)
        {
            this.TotalCount = -1;

            // Forward along the total count from our sequence if it was
            // provided
            var provider = sequence as IQueryResultEnumerable<T>;
            if (provider != null)
            {
                this.TotalCount = provider.TotalCount;
                this.NextLink = provider.NextLink;
            }
        }

        /// <summary>
        /// Gets the total count for all the records that would have been
        /// returned ignoring any take paging/limit clause specified by client
        /// or server.
        /// </summary>
        public long TotalCount { get; private set; }

        /// <summary>
        /// Gets the link to next page of result that is returned in response headers.
        /// </summary>
        public string NextLink { get; private set; }
    }
}
