// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Represents an HTTP response that can be manipulated by filters and is
    /// backed by an HttpWebResponse.
    /// </summary>
    internal class ServiceFilterResponse : IServiceFilterResponse
    {
        /// <summary>
        /// Initializes a new instance of the ServiceFilterResponse class.
        /// </summary>
        /// <param name="response">
        /// The HttpWebResponse backing this request.
        /// </param>
        /// <param name="errorStatus">
        /// The error status for any connection failures.
        /// </param>
        public ServiceFilterResponse(HttpWebResponse response, ServiceFilterResponseStatus errorStatus)
        {
            this.ResponseStatus = errorStatus;
            this.Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // Read all the interesting values from the response if it's
            // provided
            if (response != null)
            {
                // Pull out the headers
                foreach (string key in response.Headers.AllKeys)
                {
                    this.Headers[key] = response.Headers[key];
                }

                // Get the status code
                this.StatusCode = (int)response.StatusCode;

                // Get the status description
                this.StatusDescription = response.StatusDescription;

                // Get the content
                this.ContentType = response.ContentType;
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    this.Content = reader.ReadToEnd();
                }
            }
            else
            {
                // Provide sane defaults that will likely prevent folks from
                // crashing if they're not careful to check ErrorStatus first
                this.StatusCode = 0;
                this.StatusDescription = string.Empty;
                this.ContentType = string.Empty;
                this.Content = string.Empty; 
            }
        }

        /// <summary>
        /// Gets a collection of response headers.
        /// </summary>
        public IDictionary<string, string> Headers { get; private set; }

        /// <summary>
        /// Gets the HTTP status code of the response.
        /// </summary>
        public int StatusCode { get; private set; }
        
        /// <summary>
        /// Gets the HTTP status description of the response.
        /// </summary>
        public string StatusDescription { get; private set; }        

        /// <summary>
        /// Gets the type of the response body's content.
        /// </summary>
        public string ContentType { get; private set; }
        
        /// <summary>
        /// Gets the response body's content.
        /// </summary>
        public string Content { get; private set; }

        /// <summary>
        /// Gets the status that indicates reasons for connection failures in
        /// a service response.
        /// </summary>
        public ServiceFilterResponseStatus ResponseStatus { get; private set; }
    }
}
