// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using Windows.Foundation;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Represents the remaining operations to compute in the chain of filters
    /// comprising the MobileServices HTTP pipeline.
    /// </summary>
    /// <remarks>
    /// You don't need to implement IServiceFilterContinuation yourself.  The
    /// HTTP pipeline will convert your filters into continuations behind the
    /// scenes while composing filters.
    /// </remarks>
    public interface IServiceFilterContinuation
    {
        /// <summary>
        /// Allow the rest of the filters to handle the HTTP request and return
        /// a response.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <returns>The corresponding HTTP response.</returns>
        IAsyncOperation<IServiceFilterResponse> Handle(IServiceFilterRequest request);
    }
}
