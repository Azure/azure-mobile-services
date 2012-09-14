// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Provides additional details of an invalid operation specific to a
    /// Mobile Service.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification = "The exception cannot be instantiated by customers.")]
    public sealed class MobileServiceInvalidOperationException : InvalidOperationException
    {
        /// <summary>
        /// Initializes a new instance of the
        /// MobileServiceInvalidOperationException class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="request">The originating service request.</param>
        /// <param name="response">The returned service response.</param>
        internal MobileServiceInvalidOperationException(string message, IServiceFilterRequest request, IServiceFilterResponse response)
            : base(message)
        {
            this.Request = request;
            this.Response = response;
        }

        /// <summary>
        /// Gets the originating service request.
        /// </summary>
        public IServiceFilterRequest Request { get; private set; }

        /// <summary>
        /// Gets the returned service response.
        /// </summary>
        public IServiceFilterResponse Response { get; private set; }
    }
}
