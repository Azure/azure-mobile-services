// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Diagnostics;
using Windows.Foundation;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Represents two IServiceFilters that have been composed into a single
    /// IServiceFilter.  The result of applying the composed filter is the same
    /// as applying the second filter to the results obtained by applying the
    /// first filter.
    /// </summary>
    internal class ComposedServiceFilter : IServiceFilter
    {
        /// <summary>
        /// Initializes a new instance of the ComposedServiceFilter class.
        /// </summary>
        /// <param name="first">The first IServiceFilter to apply.</param>
        /// <param name="second">The second IServiceFilter to apply.</param>
        public ComposedServiceFilter(IServiceFilter first, IServiceFilter second)
        {
            Debug.Assert(first != null, "first cannot be null!");
            Debug.Assert(second != null, "second cannot be null!");

            this.First = first;
            this.Second = second;
        }
        
        /// <summary>
        /// Gets the first IServiceFilter to apply.
        /// </summary>
        public IServiceFilter First { get; private set; }

        /// <summary>
        /// Gets the second IServiceFilter to apply.
        /// </summary>
        public IServiceFilter Second { get; private set; }

        /// <summary>
        /// Handle an HTTP request and its corresponding response by applying
        /// the first filter and then the second filter.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <param name="next">
        /// The next operation in the HTTP pipeline to continue with.
        /// </param>
        /// <returns>The HTTP response.</returns>
        public IAsyncOperation<IServiceFilterResponse> Handle(IServiceFilterRequest request, IServiceFilterContinuation next)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            else if (next == null)
            {
                throw new ArgumentNullException("next");
            }

            // Second(First(next(request))
            return this.Second.Handle(request, new ComposedContinuation(this.First, next));
        }

        /// <summary>
        /// Compose a filter and a continuation into a new continuation that
        /// will be the result of applying the filter to the continuation.
        /// </summary>
        internal class ComposedContinuation : IServiceFilterContinuation
        {
            /// <summary>
            /// Initializes a new instance of the ComposedContinuation class.
            /// </summary>
            /// <param name="filter">The filter to compose.</param>
            /// <param name="next">The continuation to compose.</param>
            public ComposedContinuation(IServiceFilter filter, IServiceFilterContinuation next)
            {
                Debug.Assert(filter != null, "filter cannot be null!");
                Debug.Assert(next != null, "next cannot be null!");
                this.Filter = filter;
                this.Next = next;
            }

            /// <summary>
            /// Gets the Filter to compose.
            /// </summary>
            public IServiceFilter Filter { get; private set; }

            /// <summary>
            /// Gets the continuation to compose.
            /// </summary>
            public IServiceFilterContinuation Next { get; private set; }

            /// <summary>
            /// Handle an HTTP request and its corresponding response.
            /// </summary>
            /// <param name="request">The HTTP request.</param>
            /// <returns>The HTTP response.</returns>
            public IAsyncOperation<IServiceFilterResponse> Handle(IServiceFilterRequest request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }

                // Filter(next(request))
                return this.Filter.Handle(request, this.Next);
            }
        }
    }
}
