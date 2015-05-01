// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// The QueryResultEnumerable{T} class provides next link, records returned by a query
    /// and the total count for all the records that would have been
    /// returned ignoring any take paging/limit clause specified by client or
    /// server.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the elements in the sequence.
    /// </typeparam>
#pragma warning disable 618 // for implementing obsolete ITotalCountProvider
    internal class QueryResultEnumerable<T> : ITotalCountProvider, IQueryResultEnumerable<T>
    {
        /// <summary>
        /// The actual sequence of elements to enumerate.
        /// </summary>
        private IEnumerable<T> sequence;

        /// <summary>
        /// Initializes a new instance of the QueryResultEnumerable{T} class.
        /// </summary>
        /// <param name="totalCount">
        /// The total count for all of the records in the sequence that would
        /// have been returned ignoring any take paging/limit clause specified
        /// by client or server.
        /// </param>
        /// <param name="link">
        /// The link to next page of result
        /// </param>
        /// <param name="sequence">
        /// The sequence whose elements comprise the sequence.
        /// </param>
        public QueryResultEnumerable(long totalCount, Uri link, IEnumerable<T> sequence)
        {
            this.sequence = sequence ?? new T[0];
            this.TotalCount = totalCount;
            this.NextLink = link == null ? null : link.ToString();
        }

        /// <summary>
        /// Gets the link to next page of result that is returned in response headers.
        /// </summary>
        public string NextLink { get; private set; }

        /// <summary>
        /// Gets the total count for all the records that would have been
        /// returned ignoring any take paging/limit clause specified by client
        /// or server.
        /// </summary>
        public long TotalCount { get; private set; }

        /// <summary>
        /// Get an enumerator for the elements of the sequence.
        /// </summary>
        /// <returns>
        /// An enumerator for the elements of the sequence.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            return this.sequence.GetEnumerator();
        }

        /// <summary>
        /// Get an enumerator for the elements of the sequence.
        /// </summary>
        /// <returns>
        /// An enumerator for the elements of the sequence.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
