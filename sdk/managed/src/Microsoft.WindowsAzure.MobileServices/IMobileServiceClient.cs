// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Net.Http;

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
        /// Logs a user into a Windows Azure Mobile Service with the provider and optional token object.
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
        /// Log a user out.
        /// </summary>
        void Logout();

        /// <summary>
        /// Gets or sets the settings used for serialization.
        /// </summary>
        MobileServiceJsonSerializerSettings SerializerSettings { get; set; }
    }
}
