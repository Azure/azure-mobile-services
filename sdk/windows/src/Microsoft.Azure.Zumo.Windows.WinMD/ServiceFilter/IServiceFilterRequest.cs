// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Windows.Foundation;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Represents an HTTP request that can be manipulated by IServiceFilters.
    /// </summary>
    public interface IServiceFilterRequest
    {
        /// <summary>
        /// Gets or sets the HTTP method for the request.
        /// </summary>
        string Method { get; set; }

        /// <summary>
        /// Gets or sets the URI for the request.
        /// </summary>
        Uri Uri { get; set;  }

        /// <summary>
        /// Gets or sets a collection of headers for the request.
        /// </summary>
        /// <remarks>
        /// Because we're using HttpWebRequest behind the scenes, there are a
        /// few headers like Accept and ContentType which must be set using the
        /// properties defined on the interface rather than via Headers.
        /// </remarks>
        IDictionary<string, string> Headers { get; }

        /// <summary>
        /// Gets or sets the type of responses accepted.
        /// </summary>
        string Accept { get; set; }

        /// <summary>
        /// Gets or sets the body of the request.
        /// </summary>
        /// <remarks>
        /// For simplicity, we've made Content a string even though it could
        /// really be any type of stream.  Keeping a reference to an
        /// InMemoryRandomAccessStream without allowing any callers to
        /// accidentally close it while reading/writing is a bit of a pain, so
        /// we're going with a simple string instead.  You can use the Encoding
        /// class if you really want to send arbitrary byte streams (which is
        /// not currently a scenario for Mobile Services).
        /// </remarks>
        string Content { get; set; }

        /// <summary>
        /// Gets or sets the type of the body's content for the request.
        /// </summary>
        string ContentType { get; set; }

        /// <summary>
        /// Get the HTTP response for this request.
        /// </summary>
        /// <returns>The HTTP response.</returns>
        /// <remarks>
        /// This is the method that will tie a concrete implementation of
        /// IServiceFilterRequest to a concrete implementation of
        /// IServiceFilterResponse.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Given the cost to compute, a method is more appropriate.")]
        IAsyncOperation<IServiceFilterResponse> GetResponse();
    }
}
