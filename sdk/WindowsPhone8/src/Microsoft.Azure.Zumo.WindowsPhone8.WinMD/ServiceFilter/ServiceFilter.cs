// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Provides the default implementation for the final continuation in a
    /// chain of IServiceFilters.
    /// </summary>
    internal class ServiceFilter : IServiceFilterContinuation
    {
        /// <summary>
        /// Apply a chain of IServiceFilters to an HTTP request and get the
        /// corresponding HTTP response.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <param name="filter">
        /// The filter (or chain of filters composed together into a single
        /// filter) to apply to the request to obtain the response.
        /// </param>
        /// <returns>The HTTP response.</returns>
        public static async Task<IServiceFilterResponse> ApplyAsync(IServiceFilterRequest request, IServiceFilter filter)
        {
            return (filter != null) ?
                // If we have a filter, create a new instance of this type to
                // provide the final default continuation
                await filter.Handle(request, new ServiceFilter()) :
                // If we don't have a filter, just return the response directly
                await request.GetResponse();
        }

        /// <summary>
        /// Provide an implementation of the default next continuation which
        /// just returns the response corresponding to a request.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <returns>The HTTP response.</returns>
        IAsyncOperation<IServiceFilterResponse> IServiceFilterContinuation.Handle(IServiceFilterRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            return request.GetResponse();
        }
    }
}
