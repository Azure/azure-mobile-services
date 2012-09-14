// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using Windows.Foundation;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// An operation that can be used to manipulate requests and responses in
    /// the HTTP pipeline used by the MobileServiceClient.  IServiceFilters can
    /// be associated with a MobileServiceClient via the WithFilter method.
    /// </summary>
    public interface IServiceFilter
    {
        /// <summary>
        /// Handle an HTTP request and its corresponding response.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <param name="continuation">
        /// The next operation in the HTTP pipeline to continue with.
        /// </param>
        /// <returns>The HTTP response.</returns>
        /// <remarks>
        /// The Mobile Services HTTP pipeline is a chain of filters composed
        /// together by giving each the next operation which it can invoke
        /// (zero, one, or many times as necessary).  The default continuation
        /// of a brand new MobileServiceClient will just get the HTTP response
        /// for the corresponding request.  Here's an example of a Handle
        /// implementation that will automatically retry a request that times
        /// out.
        ///     var response = await next.Handle(request).AsTask();
        ///     if (response.StatusCode == 408) // Request timeout
        ///     {
        ///         response = await next.Handle(request).AsTask();
        ///     }
        ///     return response;
        /// Note that because these operations are asynchronous, this sample
        /// filter could end up actually making two HTTP requests before
        /// returning a response to the developer without the developer writing
        /// any special code to handle the situation.
        /// -
        /// Filters are composed just like standard function composition.  If
        /// we had new MobileServiceClient().WithFilter(F1).WithFilter(F2)
        /// .WithFilter(F3), it's conceptually equivalent to saying:
        ///     var response = await F3(await F2(await F1(await next(request)));
        /// </remarks>
        IAsyncOperation<IServiceFilterResponse> Handle(
            IServiceFilterRequest request,
            IServiceFilterContinuation continuation);
    }
}
