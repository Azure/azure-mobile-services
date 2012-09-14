// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Represents an HTTP request that can be manipulated by IServiceFilters
    /// and is backed by an HttpWebRequest.
    /// </summary>
    internal sealed class ServiceFilterRequest : IServiceFilterRequest
    {
        /// <summary>
        /// Initializes a new instance of the ServiceFilterRequest class.
        /// </summary>
        public ServiceFilterRequest()
        {
            this.Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
        
        /// <summary>
        /// Gets or sets the HTTP method for the request.
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Gets or sets the URI for the request.
        /// </summary>
        public Uri Uri { get; set; }

        /// <summary>
        /// Gets or sets a collection of headers for the request.
        /// </summary>
        /// <remarks>
        /// Because we're using HttpWebRequest behind the scenes, there are a
        /// few headers like Accept and ContentType which must be set using the
        /// properties defined on the interface rather than via Headers.
        /// </remarks>
        public IDictionary<string, string> Headers { get; private set; }

        /// <summary>
        /// Gets or sets the type of responses accepted.
        /// </summary>
        public string Accept { get; set; }

        /// <summary>
        /// Gets or sets the body of the request.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the type of the body's content for the request.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Create an HttpWebRequest that represents the request.
        /// </summary>
        /// <returns>An HttpWebRequest.</returns>
        private async Task<HttpWebRequest> CreateHttpWebRequestAsync()
        {
            // Create the request
            HttpWebRequest request = HttpWebRequest.CreateHttp(this.Uri);
            request.Method = this.Method;
            request.Accept = this.Accept;
            
            // Copy over any headers
            foreach (KeyValuePair<string, string> header in this.Headers)
            {
                // This could possibly throw if the user sets one of the
                // known headers on HttpWebRequest.  There's no way we can
                // recover so we'll just let the exception bubble up to the
                // user.  There will be no way they can work around this
                // problem unless we add that particular header to the
                // IServiceFilterRequest interface.
                request.Headers[header.Key] = header.Value;
            }

            // Copy the request body
            if (!string.IsNullOrEmpty(this.Content))
            {
                request.ContentType = this.ContentType;
                using (Stream stream = await request.GetRequestStreamAsync())
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    await writer.WriteAsync(this.Content);
                }
            }
            
            return request;
        }

        /// <summary>
        /// Get the HTTP response for this request.
        /// </summary>
        /// <returns>The HTTP response.</returns>
        public IAsyncOperation<IServiceFilterResponse> GetResponse()
        {
            return this.GetResponseAsync().AsAsyncOperation();
        }

        /// <summary>
        /// Get the HTTP response for this request.
        /// </summary>
        /// <returns>The HTTP response.</returns>
        private async Task<IServiceFilterResponse> GetResponseAsync()
        {
            HttpWebResponse response = null;
            ServiceFilterResponseStatus errorStatus = ServiceFilterResponseStatus.Success;
            try
            {
                // Create the request and send for the response
                HttpWebRequest request = await this.CreateHttpWebRequestAsync();
                response = await request.GetResponseAsync() as HttpWebResponse;
            }
            catch (WebException ex)
            {
                // Don't raise WebExceptions for >= 400 responses because it's
                // actually up to the various filters whether or not we'll
                // return the failure to the user or whether we'll make another
                // request.  The MobileServiceClient.RequestAsync method will
                // check the final response it receives for errors and throw an
                // exception if appropriate.
                response = ex.Response as HttpWebResponse;
                errorStatus = (ServiceFilterResponseStatus)ex.Status;
            }

            return new ServiceFilterResponse(response, errorStatus);
        }
    }
}
