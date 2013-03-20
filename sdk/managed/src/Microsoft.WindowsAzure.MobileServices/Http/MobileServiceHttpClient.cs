// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices
{
    internal class MobileServiceHttpClient : IDisposable
    {
        /// <summary>
        /// Name of the Installation ID header included on each request.
        /// </summary>
        private const string RequestInstallationIdHeader = "X-ZUMO-INSTALLATION-ID";

        /// <summary>
        /// Name of the application key header included when there's a key.
        /// </summary>
        private const string RequestApplicationKeyHeader = "X-ZUMO-APPLICATION";

        /// <summary>
        /// Name of the authentication header included when the user's logged
        /// in.
        /// </summary>
        private const string RequestAuthenticationHeader = "X-ZUMO-AUTH";

        /// <summary>
        /// Name of the user-agent header.
        /// </summary>
        private const string UserAgentHeader = "User-Agent";

        /// <summary>
        /// Content type for request bodies and accepted responses.
        /// </summary>
        private const string RequestJsonContentType = "application/json";

        /// <summary>
        /// Gets a reference to the <see cref="MobileServiceClient"/> associated 
        /// with this table.
        /// </summary>
        private MobileServiceClient client;

        /// <summary>
        /// The user-agent header value to use with all requests.
        /// </summary>
        private string userAgentHeaderValue;

        /// <summary>
        /// Represents a handler used to process HTTP requests and responses
        /// associated with the Mobile Service.  
        /// </summary>
        public HttpMessageHandler httpHandler;

        /// <summary>
        /// The client which will be used to send regular (non-login) HTTP
        /// requests by this mobile service.
        /// </summary>
        /// <remarks>It's defined as an instance member (instead of being
        /// created based on the handler) so that the underlying connection
        /// can be reused across multiple requests.</remarks>
        private HttpClient httpClient;

        /// <summary>
        /// The client which will be used to send login HTTP requests
        /// by this client.
        /// </summary>
        /// <remarks>Login operations should not apply any delegating handlers set
        /// by the users, since they're "system" operations, so we use a separate
        /// client for them.</remarks>
        private HttpClient authenticationHttpClient;

        /// <summary>
        /// Instantiates a new <see cref="MobileServiceHttpClient"/>, 
        /// which does all the request to a mobile service.
        /// </summary>
        /// <param name="client">
        /// The client associated with this <see cref="MobileServiceHttpClient"/>.
        /// </param>
        /// <param name="handler">
        /// An http handler.
        /// </param>
        public MobileServiceHttpClient(MobileServiceClient client, HttpMessageHandler handler)
        {
            this.client = client;

            this.httpHandler = handler ?? new HttpClientHandler();
            this.httpClient = new HttpClient(httpHandler);
            this.authenticationHttpClient = new HttpClient();

            this.userAgentHeaderValue = GetUserAgentHeader();

            this.httpClient.DefaultRequestHeaders.Add(UserAgentHeader, userAgentHeaderValue);
            this.authenticationHttpClient.DefaultRequestHeaders.Add(UserAgentHeader, userAgentHeaderValue);
        }

        /// <summary>
        /// Performs a web request and include the standard Mobile Services
        /// headers. It will use an HttpClient without any http handlers.
        /// </summary>
        /// <param name="method">
        /// The HTTP method used to request the resource.
        /// </param>
        /// <param name="uriFragment">
        /// The URI of the resource to request (relative to the Mobile Services
        /// runtime).
        /// </param>
        /// <param name="content">
        /// Optional content to send to the resource.
        /// </param>
        /// <param name="ensureResponseContent">
        /// Optional parameter to indicate if the response should include content.
        /// </param>
        /// <returns>The JSON value of the response.</returns>
        public Task<string> RequestWithoutHandlersAsync(string method, string uriFragment, string content = null , bool ensureResponseContent = true)
        {
            return this.RequestAsync(method, uriFragment, authenticationHttpClient, content, ensureResponseContent);
        }

        /// <summary>
        /// Performs a web request and include the standard Mobile Services
        /// headers. It will use an HttpClient with user defined http handlers.
        /// </summary>
        /// <param name="method">
        /// The HTTP method used to request the resource.
        /// </param>
        /// <param name="uriFragment">
        /// The URI of the resource to request (relative to the Mobile Services
        /// runtime).
        /// </param>
        /// <param name="content">
        /// Optional content to send to the resource.
        /// </param>
        /// <param name="ensureResponseContent">
        /// Optional parameter to indicate if the response should include content.
        /// </param>
        /// <returns>The JSON value of the response.</returns>
        public Task<string> RequestAsync(string method, string uriFragment, string content = null, bool ensureResponseContent = true)
        {
            return this.RequestAsync(method, uriFragment, httpClient, content, ensureResponseContent);
        }

        /// <summary>
        /// Performs a web request using the specified http client
        /// and include the standard Mobile Services headers.
        /// </summary>
        /// <param name="method">
        /// The HTTP method used to request the resource.
        /// </param>
        /// <param name="uriFragment">
        /// The URI of the resource to request (relative to the Mobile Services
        /// runtime).
        /// </param>
        /// <param name="content">
        /// The content to send to the resource.
        /// </param>
        /// <param name="httpClient">
        /// The client to use for the request.
        /// </param>
        /// <param name="ensureResponseContent">
        /// Optional parameter to indicate if the response should include content.
        /// </param>
        /// <returns>
        /// The JSON value of the response.
        /// </returns>
        private async Task<string> RequestAsync(string method, string uriFragment, HttpClient httpClient, string content, bool ensureResponseContent)
        {
            Debug.Assert(!string.IsNullOrEmpty(method), "method cannot be null or empty!");
            Debug.Assert(!string.IsNullOrEmpty(uriFragment), "uriFragment cannot be null or empty!");

            // Create the web request
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(client.ApplicationUri, uriFragment);
            request.Method = new HttpMethod(method.ToUpper());
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(RequestJsonContentType));

            // Set Mobile Services authentication, application, and telemetry
            // headers
            request.Headers.Add(RequestInstallationIdHeader, MobileServiceClient.applicationInstallationId);
            if (!string.IsNullOrEmpty(client.ApplicationKey))
            {
                request.Headers.Add(RequestApplicationKeyHeader, client.ApplicationKey);
            }
            if (client.CurrentUser != null && !string.IsNullOrEmpty(client.CurrentUser.MobileServiceAuthenticationToken))
            {
                request.Headers.Add(RequestAuthenticationHeader, client.CurrentUser.MobileServiceAuthenticationToken);
            }

            // Add any request as JSON
            if (content != null)
            {
                request.Content = new StringContent(content, Encoding.UTF8, RequestJsonContentType);
            }

            // Send the request and get the response back as string
            HttpResponseMessage response = await httpClient.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();

            // Throw errors for any failing responses
            if (!response.IsSuccessStatusCode)
            {
                ThrowInvalidResponse(request, response, responseContent);
            }

            // If there was supposed to be response content and there was not, throw
            if (ensureResponseContent && responseContent == null)
            {
                throw new MobileServiceInvalidOperationException(Resources.MobileServiceClient_NoResponseContent, request, response);
            }

            request.Dispose();
            response.Dispose();

            return responseContent;
        }

        /// <summary>
        /// Throws an exception for an invalid response to a web request.
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <param name="response">
        /// The response.
        /// </param>
        /// <param name="responseContent">
        /// The content of the response as JSON.
        /// </param>
        private static void ThrowInvalidResponse(HttpRequestMessage request, HttpResponseMessage response, string responseContent)
        {
            Debug.Assert(request != null, "request cannot be null!");
            Debug.Assert(response != null, "response cannot be null!");
            Debug.Assert(!response.IsSuccessStatusCode,
                "response should be failing!");

            // Create either an invalid response or connection failed message
            // (check the status code first because some status codes will
            // set a protocol ErrorStatus).
            string message = null;
            if (!response.IsSuccessStatusCode)
            {
                if (responseContent != null)
                {
                    JToken body = null;
                    try
                    {
                        body = JToken.Parse(responseContent);
                    }
                    catch
                    {
                    }

                    if (body != null)
                    {
                        if (body.Type == JTokenType.String)
                        {
                            // User scripts might return errors with just a plain string message as the
                            // body content, so use it as the exception message
                            message = body.ToString();
                        }
                        else if (body.Type == JTokenType.Object)
                        {
                            // Get the error message, but default to the status description
                            // below if there's no error message present.
                            JToken error = body["error"];
                            if (error != null && error.Type == JTokenType.String)
                            {
                                message = (string)error;
                            }
                            else
                            {
                                JToken description = body["description"];
                                if (description != null && description.Type == JTokenType.String)
                                {
                                    message = (string)description;
                                }
                            }
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(message))
                {
                    message = string.Format(
                        CultureInfo.InvariantCulture,
                        Resources.MobileServiceClient_ErrorMessage,
                        response.ReasonPhrase);
                }
            }
            else
            {
                message = string.Format(
                    CultureInfo.InvariantCulture,
                    Resources.MobileServiceClient_ErrorMessage,
                    response.ReasonPhrase);
            }

            // Combine the pieces and throw the exception
            throw new MobileServiceInvalidOperationException(message, request, response);
        }



        /// <summary>
        /// Gets the user-agent header to use with all requests.
        /// </summary>
        /// <returns>
        /// An HTTP user-agent header.
        /// </returns>
        private string GetUserAgentHeader()
        {
            IPlatformInformation platformInformation = Platform.Instance.PlatformInformation;
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}/{1} (lang={2}; os={3}; os_version={4}; arch={5})",
                "ZUMO",
                "1.0",  // TODO: Determine a way to get the SDK version
                "Managed",
                platformInformation.OperatingSystemName,
                platformInformation.OperatingSystemVersion,
                platformInformation.OperatingSystemArchitecture);
        }

        /// <summary>
        /// Implemenation of <see cref="IDisposable"/>
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Implemenation of <see cref="IDisposable"/> for
        /// derived classes to use.
        /// </summary>
        /// <param name="disposing">
        /// Indicates if being called from the Dispose() method
        /// or the finalizer.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                if (this.httpHandler != null)
                {
                    this.httpHandler.Dispose();
                    this.httpHandler = null;
                }

                if (this.httpClient != null)
                {
                    this.httpClient.Dispose();
                    this.httpClient = null;
                }

                if (this.authenticationHttpClient != null)
                {
                    this.authenticationHttpClient.Dispose();
                    this.authenticationHttpClient = null;
                }
            }
        }
    }
}
