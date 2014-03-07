// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Provides additional details of an invalid operation specific to a
    /// Mobile Service.
    /// </summary>
    public class MobileServiceInvalidOperationException : InvalidOperationException
    {
        /// <summary>
        /// Initializes a new instance of the
        /// MobileServiceInvalidOperationException class.
        /// </summary>
        /// <param name="message">
        /// The exception message.
        /// </param>
        /// <param name="request">
        /// The originating service request.
        /// </param>
        /// <param name="response">
        /// The returned service response.
        /// </param>
        public MobileServiceInvalidOperationException(string message, HttpRequestMessage request, HttpResponseMessage response)
            : base(message)
        {
            this.Request = request;
            this.Response = response;
        }

        /// <summary>
        /// Gets the originating service request.
        /// </summary>
        public HttpRequestMessage Request { get; private set; }

        /// <summary>
        /// Gets the returned service response.
        /// </summary>
        public HttpResponseMessage Response { get; private set; }
    }
}
