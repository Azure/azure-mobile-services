// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Interface for the <see cref="MobileServiceClient"/>.
    /// </summary>
    public interface IMobileServiceClient
    {
        /// <summary>
        /// Gets the Mobile Services application's name that is provided by the
        /// call to MobileServiceClient(...).
        /// </summary>
        string ApplicationKey { get; }

        /// <summary>
        /// Gets the Uri to the Mobile Services application that is provided by
        /// the call to MobileServiceClient(...).
        /// </summary>
        Uri ApplicationUri { get; }

        /// <summary>
        /// The current authenticated user provided after a successful call to
        /// MobileServiceClient.Login().
        /// </summary>
        MobileServiceUser CurrentUser { get; set; }

        /// <summary>
        /// Returns a <see cref="IMobileServiceTable"/> instance, which provides 
        /// untyped data operations for that table.
        /// </summary>
        /// <param name="tableName">
        /// The name of the table.
        /// </param>
        /// <returns>
        /// The table.
        /// </returns>
        IMobileServiceTable GetTable(string tableName);

        /// <summary>
        /// Returns a <see cref="IMobileServiceTable{T}"/> instance, which provides 
        /// strongly typed data operations for that table.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the instances in the table.
        /// </typeparam>
        /// <returns>
        /// The table.
        /// </returns>
        IMobileServiceTable<T> GetTable<T>();

        /// <summary>
        /// Logs a user into a Windows Azure Mobile Service with the provider and a token object.
        /// </summary>
        /// <param name="provider">
        /// Authentication provider to use.
        /// </param>
        /// <param name="token">
        /// Provider specific object with existing OAuth token to log in with.
        /// </param>
        /// <remarks>
        /// The token object needs to be formatted depending on the specific provider. These are some
        /// examples of formats based on the providers:
        /// <list type="bullet">
        ///   <item>
        ///     <term>MicrosoftAccount</term>
        ///     <description><code>{"authenticationToken":"&lt;the_authentication_token&gt;"}</code></description>
        ///   </item>
        ///   <item>
        ///     <term>Facebook</term>
        ///     <description><code>{"access_token":"&lt;the_access_token&gt;"}</code></description>
        ///   </item>
        ///   <item>
        ///     <term>Google</term>
        ///     <description><code>{"access_token":"&lt;the_access_token&gt;"}</code></description>
        ///   </item>
        /// </list>
        /// </remarks>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        Task<MobileServiceUser> LoginAsync(MobileServiceAuthenticationProvider provider, JObject token);

        /// <summary>
        /// Logs a user into a Windows Azure Mobile Service with the provider and a token object.
        /// </summary>
        /// <param name="provider">
        /// Authentication provider to use.
        /// </param>
        /// <param name="token">
        /// Provider specific object with existing OAuth token to log in with.
        /// </param>
        /// <remarks>
        /// The token object needs to be formatted depending on the specific provider. These are some
        /// examples of formats based on the providers:
        /// <list type="bullet">
        ///   <item>
        ///     <term>MicrosoftAccount</term>
        ///     <description><code>{"authenticationToken":"&lt;the_authentication_token&gt;"}</code></description>
        ///   </item>
        ///   <item>
        ///     <term>Facebook</term>
        ///     <description><code>{"access_token":"&lt;the_access_token&gt;"}</code></description>
        ///   </item>
        ///   <item>
        ///     <term>Google</term>
        ///     <description><code>{"access_token":"&lt;the_access_token&gt;"}</code></description>
        ///   </item>
        /// </list>
        /// </remarks>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        Task<MobileServiceUser> LoginAsync(string provider, JObject token);

        /// <summary>
        /// Log a user out.
        /// </summary>
        void Logout();

        /// <summary>
        /// Invokes a user-defined custom API of a Windows Azure Mobile Service using an HTTP POST.
        /// </summary>
        /// <typeparam name="T">The type of instance returned from the Windows Azure Mobile Service.</typeparam>    
        /// <param name="apiName">The name of the custom API.</param>
        /// <returns>The response content from the custom api invocation.</returns>
        Task<T> InvokeApiAsync<T>(string apiName);

        /// <summary>
        /// Invokes a user-defined custom API of a Windows Azure Mobile Service using an HTTP POST with
        /// support for sending HTTP content.
        /// </summary>
        /// <typeparam name="T">The type of instance sent to the Windows Azure Mobile Service.</typeparam>
        /// <typeparam name="U">The type of instance returned from the Windows Azure Mobile Service.</typeparam>    
        /// <param name="apiName">The name of the custom API.</param>
        /// <param name="body">The value to be sent as the HTTP body.</param>
        /// <returns>The response content from the custom api invocation.</returns>
        Task<U> InvokeApiAsync<T, U>(string apiName, T body);

        /// <summary>
        /// Invokes a user-defined custom API of a Windows Azure Mobile Service using the specified HTTP Method.
        /// Additional data can be passed using the query string.
        /// </summary>
        /// <typeparam name="T">The type of instance sent to the Windows Azure Mobile Service.</typeparam>
        /// <param name="apiName">The name of the custom API.</param>
        /// <param name="method">The HTTP method.</param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to include in the request URI query string.
        /// </param>
        /// <returns>The response content from the custom api invocation.</returns>
        Task<T> InvokeApiAsync<T>(string apiName, HttpMethod method, IDictionary<string, string> parameters);

        /// <summary>
        /// Invokes a user-defined custom API of a Windows Azure Mobile Service using the specified HTTP Method.
        /// Additional data can be sent though the HTTP content or the query string.
        /// </summary>
        /// <typeparam name="T">The type of instance sent to the Windows Azure Mobile Service.</typeparam>
        /// <typeparam name="U">The type of instance returned from the Windows Azure Mobile Service.</typeparam>    
        /// <param name="apiName">The name of the custom API.</param>
        /// <param name="body">The value to be sent as the HTTP body.</param>
        /// <param name="method">The HTTP method.</param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to include in the request URI query string.
        /// </param>
        /// <returns>The response content from the custom api invocation.</returns>
        Task<U> InvokeApiAsync<T, U>(string apiName, T body, HttpMethod method, IDictionary<string, string> parameters);

        /// <summary>
        /// Invokes a user-defined custom API of a Windows Azure Mobile Service using an HTTP POST.
        /// </summary>
        /// <param name="apiName">The name of the custom API.</param>
        /// <returns></returns>
        Task<JToken> InvokeApiAsync(string apiName);

        /// <summary>
        /// Invokes a user-defined custom API of a Windows Azure Mobile Service using an HTTP POST, with
        /// support for sending HTTP content.
        /// </summary>
        /// <param name="apiName">The name of the custom API.</param>
        /// <param name="body">The value to be sent as the HTTP body.</param>
        /// <returns></returns>
        Task<JToken> InvokeApiAsync(string apiName, JToken body);

        /// <summary>
        /// Invokes a user-defined custom API of a Windows Azure Mobile Service using the specified HTTP Method.
        /// Additional data will sent to through the query string.
        /// </summary>
        /// <param name="apiName">The name of the custom API.</param>
        /// <param name="method">The HTTP method.</param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to include in the request URI query string.
        /// </param>
        /// <returns>The response content from the custom api invocation.</returns>
        Task<JToken> InvokeApiAsync(string apiName, HttpMethod method, IDictionary<string, string> parameters);

        /// <summary>
        /// Invokes a user-defined custom API of a Windows Azure Mobile Service using the specified HTTP method.
        /// Additional data can be sent though the HTTP content or the query string.
        /// </summary>
        /// <param name="apiName">The name of the custom API.</param>
        /// <param name="body">The value to be sent as the HTTP body.</param>
        /// <param name="method">The HTTP method.</param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to include in the request URI query string.
        /// </param>
        /// <returns>The response content from the custom api invocation.</returns>
        Task<JToken> InvokeApiAsync(string apiName, JToken body, HttpMethod method, IDictionary<string, string> parameters);
        
        /// <summary>
        /// Invokes a user-defined custom API of a Windows Azure Mobile Service using the specified HttpMethod.
        /// Additional data can be sent though the HTTP content or the query string. 
        /// </summary>
        /// <param name="apiName">The name of the custom API.</param>
        /// <param name="content">The HTTP content.</param>
        /// <param name="method">The HTTP method.</param>
        /// <param name="requestHeaders">
        /// A dictionary of user-defined headers to include in the HttpRequest.
        /// </param>
        /// <param name="parameters">
        /// A dictionary of user-defined parameters and values to include in the request URI query string.
        /// </param>
        /// <returns>The HTTP Response from the custom api invocation.</returns>
        Task<HttpResponseMessage> InvokeApiAsync(string apiName, HttpContent content, HttpMethod method, IDictionary<string, string> requestHeaders, IDictionary<string, string> parameters);

        /// <summary>
        /// Gets or sets the settings used for serialization.
        /// </summary>
        MobileServiceJsonSerializerSettings SerializerSettings { get; set; }
    }
}
