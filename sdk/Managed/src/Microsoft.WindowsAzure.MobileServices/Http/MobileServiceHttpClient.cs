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
        /// Name of the header to indicate the feature(s) initiating the remote server call.
        /// </summary>
        internal const string ZumoFeaturesHeader = "X-ZUMO-FEATURES";

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
        /// The URI for the Microsoft Azure Mobile Service.
        /// </summary>
        private readonly Uri applicationUri;

        /// <summary>
        /// The installation id of the application.
        /// </summary>
        private readonly string installationId;

        /// <summary>
        /// The application key for the Microsoft Azure Mobile Service.
        /// </summary>
        private readonly string applicationKey;

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
        /// Factory method for creating the default http client handler
        /// </summary>
        internal static Func<HttpMessageHandler> DefaultHandlerFactory = GetDefaultHttpClientHandler;

        /// <summary>
        /// Instantiates a new <see cref="MobileServiceHttpClient"/>, 
        /// which does all the request to a mobile service.
        /// </summary>
        /// <param name="handlers">
        /// Chain of <see cref="HttpMessageHandler" /> instances. 
        /// All but the last should be <see cref="DelegatingHandler"/>s. 
        /// </param>
        /// <param name="applicationUri">
        /// The URI for the Microsoft Azure Mobile Service.
        /// </param>
        /// <param name="installationId">
        /// The installation id of the application.
        /// </param>
        /// <param name="applicationKey">
        /// The application key for the Microsoft Azure Mobile Service.
        /// </param>
        public MobileServiceHttpClient(IEnumerable<HttpMessageHandler> handlers, Uri applicationUri, string installationId, string applicationKey)
        {
            Debug.Assert(handlers != null);
            Debug.Assert(applicationUri != null);

            this.applicationUri = applicationUri;
            this.installationId = installationId;
            this.applicationKey = applicationKey;

            this.httpHandler = CreatePipeline(handlers);
            this.httpClient = new HttpClient(httpHandler);
            this.httpClientSansHandlers = new HttpClient(DefaultHandlerFactory());

            this.userAgentHeaderValue = GetUserAgentHeader();

            // Work around user agent header passing mono bug
            // https://bugzilla.xamarin.com/show_bug.cgi?id=15128
            this.httpClient.DefaultRequestHeaders.TryAddWithoutValidation(UserAgentHeader, userAgentHeaderValue);
            this.httpClient.DefaultRequestHeaders.Add(ZumoVersionHeader, userAgentHeaderValue);
            this.httpClientSansHandlers.DefaultRequestHeaders.TryAddWithoutValidation(UserAgentHeader, userAgentHeaderValue);
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
        /// <param name="user">
        /// The object representing the user on behalf of whom the request will be sent.
        /// </param>
        /// <param name="content">
        /// Optional content to send to the resource.
        /// </param>
        /// <returns>
        /// The content of the response as a string.
        /// </returns>
        public async Task<string> RequestWithoutHandlersAsync(HttpMethod method, string uriPathAndQuery, MobileServiceUser user, string content = null)
        {
            MobileServiceHttpResponse response = await this.RequestAsync(false, method, uriPathAndQuery, user, content, false);
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
        /// <param name="user">
        /// The object representing the user on behalf of whom the request will be sent.
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
        /// <param name="features">
        /// Value indicating which features of the SDK are being used in this call. Useful for telemetry.
        /// </param>
        /// <returns>
        /// The response.
        /// </returns>
        public Task<MobileServiceHttpResponse> RequestAsync(HttpMethod method,
                                                             string uriPathAndQuery,
                                                             MobileServiceUser user,
                                                             string content = null,
                                                             bool ensureResponseContent = true,
                                                             IDictionary<string, string> requestHeaders = null,
                                                             MobileServiceFeatures features = MobileServiceFeatures.None)
        {
            requestHeaders = FeaturesHelper.AddFeaturesHeader(requestHeaders, features);
            return this.RequestAsync(true, method, uriPathAndQuery, user, content, ensureResponseContent, requestHeaders);
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
        /// <param name="user">
        /// The object representing the user on behalf of whom the request will be sent.
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
                                                        MobileServiceUser user,
                                                        string content = null,
                                                        bool ensureResponseContent = true,
                                                        IDictionary<string, string> requestHeaders = null)
        {
            Debug.Assert(method != null);
            Debug.Assert(!string.IsNullOrEmpty(uriPathAndQuery));

            // Create the request
            HttpContent httpContent = CreateHttpContent(content);
            HttpRequestMessage request = this.CreateHttpRequestMessage(method, uriPathAndQuery, requestHeaders, httpContent, user);
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

            LinkHeaderValue link = null;
            if (response.Headers.Contains("Link"))
            {
                link = LinkHeaderValue.Parse(response.Headers.GetValues("Link").FirstOrDefault());
            }

            // Dispose of the request and response
            request.Dispose();
            response.Dispose();

            return new MobileServiceHttpResponse(responseContent, etag, link);
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
        /// <param name="user">
        /// The object representing the user on behalf of whom the request will be sent.
        /// </param>
        /// <param name="content">
        /// Content to send to the resource.
        /// </param>
        /// <param name="requestHeaders">
        /// Additional request headers to include with the request.
        /// </param>
        /// <param name="features">
        /// Value indicating which features of the SDK are being used in this call. Useful for telemetry.
        /// </param>
        /// <returns>
        /// An <see cref="HttpResponseMessage"/>.
        /// </returns>
        public async Task<HttpResponseMessage> RequestAsync(HttpMethod method, string uriPathAndQuery, MobileServiceUser user, HttpContent content, IDictionary<string, string> requestHeaders, MobileServiceFeatures features = MobileServiceFeatures.None)
        {
            Debug.Assert(method != null);
            Debug.Assert(!string.IsNullOrEmpty(uriPathAndQuery));

            requestHeaders = FeaturesHelper.AddFeaturesHeader(requestHeaders, features);
            // Create the request
            HttpRequestMessage request = this.CreateHttpRequestMessage(method, uriPathAndQuery, requestHeaders, content, user);

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

            string responseContent = response.Content == null ? null : await response.Content.ReadAsStringAsync();

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
                    else if (response.Content.Headers.ContentType != null &&
                                response.Content.Headers.ContentType.MediaType != null &&
                                response.Content.Headers.ContentType.MediaType.Contains("text"))
                    {
                        message = responseContent;
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
        /// <param name="user">
        /// The object representing the user on behalf of whom the request will be sent.
        /// </param>
        /// <returns>
        /// An <see cref="HttpRequestMessage"/> with all of the 
        /// required Mobile Service headers.
        /// </returns>
        private HttpRequestMessage CreateHttpRequestMessage(HttpMethod method, string uriPathAndQuery, IDictionary<string, string> requestHeaders, HttpContent content, MobileServiceUser user)
        {
            Debug.Assert(method != null);
            Debug.Assert(!string.IsNullOrEmpty(uriPathAndQuery));

            HttpRequestMessage request = new HttpRequestMessage();

            // Set the Uri and Http Method
            request.RequestUri = new Uri(this.applicationUri, uriPathAndQuery);
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
            request.Headers.Add(RequestInstallationIdHeader, this.installationId);
            if (!string.IsNullOrEmpty(this.applicationKey))
            {
                request.Headers.Add(RequestApplicationKeyHeader, this.applicationKey);
            }

            if (user != null && !string.IsNullOrEmpty(user.MobileServiceAuthenticationToken))
            {
                request.Headers.Add(RequestAuthenticationHeader, user.MobileServiceAuthenticationToken);
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
            HttpMessageHandler pipeline = handlers.LastOrDefault() ?? DefaultHandlerFactory();
            DelegatingHandler dHandler = pipeline as DelegatingHandler;
            if (dHandler != null)
            {
                dHandler.InnerHandler = DefaultHandlerFactory();
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
        /// Returns a default HttpMessageHandler that supports automatic decompression.
        /// </summary>
        /// <returns>
        /// A default HttpClientHandler that supports automatic decompression
        /// </returns>
        private static HttpMessageHandler GetDefaultHttpClientHandler()
        {
            var handler = new HttpClientHandler();
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
            AssemblyFileVersionAttribute fileVersionAttribute = typeof(MobileServiceClient).GetTypeInfo().Assembly
                                                                        .GetCustomAttributes(typeof(AssemblyFileVersionAttribute))
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

        /// <summary>
        /// Helper class to create the HTTP headers used for sending feature usage to the service.
        /// </summary>
        static class FeaturesHelper
        {
            /// <summary>
            /// Existing features which can be sent for telemetry purposes to the server.
            /// </summary>
            private static readonly List<Tuple<MobileServiceFeatures, string>> AllTelemetryFeatures;

            static FeaturesHelper()
            {
                AllTelemetryFeatures = new List<Tuple<MobileServiceFeatures, string>>();
                var features = (MobileServiceFeatures[])Enum.GetValues(typeof(MobileServiceFeatures));
                foreach (var feature in features)
                {
                    if (feature != MobileServiceFeatures.None)
                    {
                        AllTelemetryFeatures.Add(new Tuple<MobileServiceFeatures, string>(feature, EnumValueAttribute.GetValue(feature)));
                    }
                }
            }

            /// <summary>
            /// Adds a header for features used in this request. Used for telemetry.
            /// </summary>
            /// <param name="requestHeaders">
            /// Additional request headers to include with the request.
            /// </param>
            /// <param name="features">
            /// Value indicating which features of the SDK are being used in this call.
            /// </param>
            /// <returns>The list of headers to send in this request.</returns>
            public static IDictionary<string, string> AddFeaturesHeader(IDictionary<string, string> requestHeaders, MobileServiceFeatures features)
            {
                if (features != MobileServiceFeatures.None)
                {
                    if (requestHeaders == null || !requestHeaders.ContainsKey(ZumoFeaturesHeader))
                    {
                        requestHeaders = new Dictionary<string, string>(requestHeaders ?? new Dictionary<string, string>());
                        requestHeaders.Add(ZumoFeaturesHeader, FeaturesToString(features));
                    }
                }

                return requestHeaders;
            }

            /// <summary>
            /// Returns the value to be used in the HTTP header corresponding to the given features.
            /// </summary>
            /// <param name="features">The features to be sent as telemetry to the service.</param>
            /// <returns>The value of the HTTP header to be sent to the service.</returns>
            private static string FeaturesToString(MobileServiceFeatures features)
            {
                return string.Join(",",
                    AllTelemetryFeatures
                        .Where(t => (features & t.Item1) == t.Item1)
                        .Select(t => t.Item2));
            }
        }
    }
}