// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
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
        /// Name of the zumo version header.
        /// </summary>
        private const string ZumoVersionHeader = "X-ZUMO-VERSION";

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
        private HttpClient httpClientSansHandlers;

        /// <summary>
        /// Instantiates a new <see cref="MobileServiceHttpClient"/>, 
        /// which does all the request to a mobile service.
        /// </summary>
        /// <param name="client">
        /// The client associated with this <see cref="MobileServiceHttpClient"/>.
        /// </param>
        /// <param name="handlers">
        /// Chain of <see cref="HttpMessageHandler" /> instances. 
        /// All but the last should be <see cref="DelegatingHandler"/>s. 
        /// </param>
        public MobileServiceHttpClient(MobileServiceClient client, IEnumerable<HttpMessageHandler> handlers)
        {
            Debug.Assert(handlers != null);

            this.client = client;
            this.httpHandler = CreatePipeline(handlers);
            this.httpClient = new HttpClient(httpHandler);
            this.httpClientSansHandlers = new HttpClient(GetDefaultHttpClientHandler());

            this.userAgentHeaderValue = GetUserAgentHeader();

            this.httpClient.DefaultRequestHeaders.Add(UserAgentHeader, userAgentHeaderValue);
            this.httpClient.DefaultRequestHeaders.Add(ZumoVersionHeader, userAgentHeaderValue);
            this.httpClientSansHandlers.DefaultRequestHeaders.Add(UserAgentHeader, userAgentHeaderValue);
            this.httpClientSansHandlers.DefaultRequestHeaders.Add(ZumoVersionHeader, userAgentHeaderValue);
        }

        /// <summary>
        /// Performs a web request and includes the standard Mobile Services
        /// headers. It will use an HttpClient without any http handlers.
        /// </summary>
        /// <param name="method">
        /// The HTTP method used to request the resource.
        /// </param>
        /// <param name="uriPathAndQuery">
        /// The URI of the resource to request (relative to the Mobile Services
        /// runtime).
        /// </param>
        /// <param name="content">
        /// Optional content to send to the resource.
        /// </param>
        /// <returns>
        /// The content of the response as a string.
        /// </returns>
        public async Task<string> RequestWithoutHandlersAsync(HttpMethod method, string uriPathAndQuery, string content = null)
        {
            MobileServiceHttpResponse response = await this.RequestAsync(false, method, uriPathAndQuery, content, false);
            return response.Content;
        }

        /// <summary>
        /// Makes an HTTP request that includes the standard Mobile Services
        /// headers. It will use an HttpClient with user-defined http handlers.
        /// </summary>
        /// <param name="method">
        /// The HTTP method used to request the resource.
        /// </param>
        /// <param name="uriPathAndQuery">
        /// The URI of the resource to request (relative to the Mobile Services
        /// runtime).
        /// </param>
        /// <param name="content">
        /// Optional content to send to the resource.
        /// </param>
        /// <param name="ensureResponseContent">
        /// Optional parameter to indicate if the response should include content.
        /// </param>
        /// <param name="requestHeaders">
        /// Additional request headers to include with the request.
        /// </param>
        /// <returns>
        /// The response.
        /// </returns>
        public Task<MobileServiceHttpResponse> RequestAsync(HttpMethod method,
                                                             string uriPathAndQuery, 
                                                             string content = null,
                                                             bool ensureResponseContent = true,
                                                             IDictionary<string, string> requestHeaders = null)
        {
            return this.RequestAsync(true, method, uriPathAndQuery, content, ensureResponseContent, requestHeaders);
        }

        /// <summary>
        /// Makes an HTTP request that includes the standard Mobile Services
        /// headers. It will use an HttpClient that optionally has user-defined 
        /// http handlers.
        /// </summary>
        /// <param name="UseHandlers">Determines if the HttpClient will use user-defined http handlers</param>
        /// <param name="method">
        /// The HTTP method used to request the resource.
        /// </param>
        /// <param name="uriPathAndQuery">
        /// The URI of the resource to request (relative to the Mobile Services
        /// runtime).
        /// </param>
        /// <param name="content">
        /// Optional content to send to the resource.
        /// </param>
        /// <param name="ensureResponseContent">
        /// Optional parameter to indicate if the response should include content.
        /// </param>
        /// <param name="requestHeaders">
        /// Additional request headers to include with the request.
        /// </param>
        /// <returns>
        /// The content of the response as a string.
        /// </returns>
        private async Task<MobileServiceHttpResponse> RequestAsync(bool UseHandlers, 
                                                        HttpMethod method, 
                                                        string uriPathAndQuery, 
                                                        string content = null, 
                                                        bool ensureResponseContent = true,
                                                        IDictionary<string, string> requestHeaders = null)
        {
            Debug.Assert(method != null);
            Debug.Assert(!string.IsNullOrEmpty(uriPathAndQuery));

            // Create the request
            HttpContent httpContent = CreateHttpContent(content);
            HttpRequestMessage request = this.CreateHttpRequestMessage(method, uriPathAndQuery, requestHeaders, httpContent);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(RequestJsonContentType));

            // Get the response
            HttpClient client;
            if (UseHandlers)
            {
                client = this.httpClient;
            }
            else
            {
                client = this.httpClientSansHandlers;
            }
            HttpResponseMessage response = await this.SendRequestAsync(client, request, ensureResponseContent);
            string responseContent = await GetResponseContent(response);
            string etag = null;
            if (response.Headers.ETag != null)
            {
                etag = response.Headers.ETag.Tag;
            }

            // Dispose of the request and response
            request.Dispose();
            response.Dispose();

            return new MobileServiceHttpResponse(responseContent, etag);
        }

        /// <summary>
        /// Makes an HTTP request that includes the standard Mobile Services
        /// headers. It will use an HttpClient with user-defined http handlers.
        /// </summary>
        /// <param name="method">
        /// The HTTP method used to request the resource.
        /// </param>
        /// <param name="uriPathAndQuery">
        /// The URI of the resource to request (relative to the Mobile Services
        /// runtime).
        /// </param>
        /// <param name="content">
        /// Content to send to the resource.
        /// </param>
        /// <param name="requestHeaders">
        /// Additional request headers to include with the request.
        /// </param>
        /// <returns>
        /// An <see cref="HttpResponseMessage"/>.
        /// </returns>
        public async Task<HttpResponseMessage> RequestAsync(HttpMethod method, string uriPathAndQuery, HttpContent content, IDictionary<string, string> requestHeaders)
        {
            Debug.Assert(method != null);
            Debug.Assert(!string.IsNullOrEmpty(uriPathAndQuery));

            // Create the request
            HttpRequestMessage request = this.CreateHttpRequestMessage(method, uriPathAndQuery, requestHeaders, content);

            // Get the response
            HttpResponseMessage response = await this.SendRequestAsync(httpClient, request, ensureResponseContent: false);

            return response;
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

                if (this.httpClientSansHandlers != null)
                {
                    this.httpClientSansHandlers.Dispose();
                    this.httpClientSansHandlers = null;
                }
            }
        }

        /// <summary>
        /// Creates an <see cref="HttpContent"/> instance from a string.
        /// </summary>
        /// <param name="content">
        /// The string content from which to create the <see cref="HttpContent"/> instance. 
        /// </param>
        /// <returns>
        /// An <see cref="HttpContent"/> instance or null if the <paramref name="content"/>
        /// was null.
        /// </returns>
        private static HttpContent CreateHttpContent(string content)
        {
            HttpContent httpContent = null;
            if (content != null)
            {
                httpContent = new StringContent(content, Encoding.UTF8, RequestJsonContentType);
            }

            return httpContent;
        }

        /// <summary>
        /// Returns the content from the <paramref name="response"/> as a string.
        /// </summary>
        /// <param name="response">
        /// The <see cref="HttpResponseMessage"/> from which to read the content as a string.
        /// </param>
        /// <returns>
        /// The response content as a string.
        /// </returns>
        private static async Task<string> GetResponseContent(HttpResponseMessage response)
        {
            string responseContent = null;
            if (response.Content != null)
            {
                responseContent = await response.Content.ReadAsStringAsync();
            }

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
        private static async Task ThrowInvalidResponse(HttpRequestMessage request, HttpResponseMessage response)
        {
            Debug.Assert(request != null);
            Debug.Assert(response != null);
            Debug.Assert(!response.IsSuccessStatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();

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
        /// Creates an <see cref="HttpRequestMessage"/> with all of the 
        /// required Mobile Service headers.
        /// </summary>
        /// <param name="method">
        /// The HTTP method of the request.
        /// </param>
        /// <param name="uriPathAndQuery">
        /// The URI of the resource to request (relative to the Mobile Services
        /// runtime).
        /// </param>
        /// <param name="requestHeaders">
        /// Additional request headers to include with the request.
        /// </param>
        /// <param name="content">
        /// The content of the request.
        /// </param>
        /// <returns>
        /// An <see cref="HttpRequestMessage"/> with all of the 
        /// required Mobile Service headers.
        /// </returns>
        private HttpRequestMessage CreateHttpRequestMessage(HttpMethod method, string uriPathAndQuery, IDictionary<string, string> requestHeaders, HttpContent content)
        {
            Debug.Assert(method != null);
            Debug.Assert(!string.IsNullOrEmpty(uriPathAndQuery));

            HttpRequestMessage request = new HttpRequestMessage();

            // Set the Uri and Http Method
            request.RequestUri = new Uri(client.ApplicationUri, uriPathAndQuery);
            request.Method = method;

            // Add the user's headers
            if (requestHeaders != null)
            {
                foreach (KeyValuePair<string, string> header in requestHeaders)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            // Set Mobile Services authentication, application, and telemetry headers
            request.Headers.Add(RequestInstallationIdHeader, client.applicationInstallationId);
            if (!string.IsNullOrEmpty(client.ApplicationKey))
            {
                request.Headers.Add(RequestApplicationKeyHeader, client.ApplicationKey);
            }

            if (client.CurrentUser != null && 
                !string.IsNullOrEmpty(client.CurrentUser.MobileServiceAuthenticationToken))
            {
                request.Headers.Add(RequestAuthenticationHeader, client.CurrentUser.MobileServiceAuthenticationToken);
            }

            // Add the content
            if (content != null)
            {
                request.Content = content; 
            }

            return request;
        }

        /// <summary>
        /// Sends the <paramref name="request"/> with the given <paramref name="client"/>.
        /// </summary>
        /// <param name="client">
        /// The <see cref="HttpClient"/> to send the request with.
        /// </param>
        /// <param name="request">
        /// The <see cref="HttpRequestMessage"/> to be sent.
        /// </param>
        /// <param name="ensureResponseContent">
        /// Optional parameter to indicate if the response should include content.
        /// </param>
        /// <returns>
        /// An <see cref="HttpResponseMessage"/>.
        /// </returns>
        private async Task<HttpResponseMessage> SendRequestAsync(HttpClient client, HttpRequestMessage request, bool ensureResponseContent)
        {
            Debug.Assert(client != null);
            Debug.Assert(request != null);

            // Send the request and get the response back as string
            HttpResponseMessage response = await client.SendAsync(request);

            // Throw errors for any failing responses
            if (!response.IsSuccessStatusCode)
            {
                await ThrowInvalidResponse(request, response);
            }

            // If there was supposed to be response content and there was not, throw
            if (ensureResponseContent)
            {
                long? contentLength = null;
                if (response.Content != null)
                {
                    contentLength = response.Content.Headers.ContentLength;
                }

                if (contentLength == null || contentLength <= 0)
                {
                    throw new MobileServiceInvalidOperationException(Resources.MobileServiceClient_NoResponseContent, request, response);
                }
            }

            return response;
        }

        /// <summary>
        /// Transform an IEnumerable of <see cref="HttpMessageHandler"/>s into
        /// a chain of <see cref="HttpMessageHandler"/>s.
        /// </summary>
        /// <param name="handlers">
        /// Chain of <see cref="HttpMessageHandler" /> instances. 
        /// All but the last should be <see cref="DelegatingHandler"/>s. 
        /// </param>
        /// <returns>A chain of <see cref="HttpMessageHandler"/>s</returns>
        private static HttpMessageHandler CreatePipeline(IEnumerable<HttpMessageHandler> handlers)
        {
            HttpMessageHandler pipeline = handlers.LastOrDefault() ?? GetDefaultHttpClientHandler();
            DelegatingHandler dHandler = pipeline as DelegatingHandler;
            if (dHandler != null)
            {
                dHandler.InnerHandler = GetDefaultHttpClientHandler();
                pipeline = dHandler;
            }

            // Wire handlers up in reverse order
            IEnumerable<HttpMessageHandler> reversedHandlers = handlers.Reverse().Skip(1);
            foreach (HttpMessageHandler handler in reversedHandlers)
            {
                dHandler = handler as DelegatingHandler;
                if (dHandler == null)
                {
                    throw new ArgumentException(
                        string.Format(
                        Resources.HttpMessageHandlerExtensions_WrongHandlerType,
                        typeof(DelegatingHandler).Name));
                }

                dHandler.InnerHandler = pipeline;
                pipeline = dHandler;
            }

            return pipeline;
        }

        /// <summary>
        /// Returns a default HttpClientHandler that supports automatic decompression.
        /// </summary>
        /// <returns>
        /// A default HttpClientHandler that supports automatic decompression
        /// </returns>
        private static HttpClientHandler GetDefaultHttpClientHandler()
        {
            HttpClientHandler handler = new HttpClientHandler();
            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip;
            }

            return handler;
        }

        /// <summary>
        /// Gets the user-agent header to use with all requests.
        /// </summary>
        /// <returns>
        /// An HTTP user-agent header.
        /// </returns>
        private string GetUserAgentHeader()
        {
            AssemblyFileVersionAttribute fileVersionAttribute = Assembly.GetExecutingAssembly()
                                                                        .GetCustomAttributes(typeof(AssemblyFileVersionAttribute), true)
                                                                        .Cast<AssemblyFileVersionAttribute>()
                                                                        .FirstOrDefault();
            string fileVersion = fileVersionAttribute.Version;
            string sdkVersion = string.Join(".", fileVersion.Split('.').Take(2)); // Get just the major and minor versions

            IPlatformInformation platformInformation = Platform.Instance.PlatformInformation;
            return string.Format(
                CultureInfo.InvariantCulture,
                "ZUMO/{0} (lang={1}; os={2}; os_version={3}; arch={4}; version={5})",
                sdkVersion,
                "Managed",
                platformInformation.OperatingSystemName,
                platformInformation.OperatingSystemVersion,
                platformInformation.OperatingSystemArchitecture,
                fileVersion);
        }
    }
}
