// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Represents an HTTP response that can be manipulated by filters.
    /// </summary>
    public interface IServiceFilterResponse
    {
        /// <summary>
        /// Gets the HTTP status code of the response.
        /// </summary>
        int StatusCode { get; }

        /// <summary>
        /// Gets the HTTP status description of the response.
        /// </summary>
        string StatusDescription { get; }

        /// <summary>
        /// Gets the type of the response body's content.
        /// </summary>
        string ContentType { get; }

        /// <summary>
        /// Gets the body of the response.
        /// </summary>
        string Content { get; }

        /// <summary>
        /// Gets a collection of response headers.
        /// </summary>
        IDictionary<string, string> Headers { get; }

        /// <summary>
        /// Gets the status that indicates reasons for connection failures in
        /// a service response.
        /// </summary>
        ServiceFilterResponseStatus ResponseStatus { get; }
    }
}
