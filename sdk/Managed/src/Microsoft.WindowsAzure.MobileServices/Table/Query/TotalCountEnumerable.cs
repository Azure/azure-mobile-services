// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// The TotalCountEnumerable class provides records returned by a query
    /// along with the total count for all the records that would have been
    /// returned ignoring any take paging/limit clause specified by client or
    /// server.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the elements in the sequence.
    /// </typeparam>
    internal class TotalCountEnumerable<T> : IEnumerable<T>, ITotalCountProvider
    {
        /// <summary>
        /// The actual sequence of elements to enumerate.
        /// </summary>
        private IEnumerable<T> sequence;

        /// <summary>
        /// Initializes a new instance of the TotalCountEnumerable class.
        /// </summary>
        /// <param name="totalCount">
        /// The total count for all of the records in the sequence that would
        /// have been returned ignoring any take paging/limit clause specified
        /// by client or server.
        /// </param>
        /// <param name="sequence">
        /// The sequence whose elements comprise the sequence.
        /// </param>
        public TotalCountEnumerable(long totalCount, IEnumerable<T> sequence)            
        {
            this.sequence = sequence ?? new T[0];
            this.TotalCount = totalCount;
        }

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
