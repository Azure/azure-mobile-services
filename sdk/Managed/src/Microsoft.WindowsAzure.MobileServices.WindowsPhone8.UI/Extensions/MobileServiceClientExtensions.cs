// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Extension methods for UI-based login.
    /// </summary>
    public static class MobileServiceClientExtensions
    {
        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="provider">
        /// Authentication provider to use.
        /// </param>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public static Task<MobileServiceUser> LoginAsync(this IMobileServiceClient client, MobileServiceAuthenticationProvider provider)
        {
            return LoginAsync(client, provider, parameters: null);
        }

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="provider">
        /// Authentication provider to use.
        /// </param>
        /// <param name="parameters">
        /// Provider specific extra parameters that are sent as query string parameters to login endpoint.
        /// </param>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public static Task<MobileServiceUser> LoginAsync(this IMobileServiceClient client, MobileServiceAuthenticationProvider provider, IDictionary<string, string> parameters)
        {
            return LoginAsync(client, provider.ToString(), parameters);
        }

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="provider">
        /// Authentication provider to use.
        /// </param>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public static Task<MobileServiceUser> LoginAsync(this IMobileServiceClient client, string provider)
        {
            return LoginAsync(client, provider, null);
        }

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="provider">
        /// Authentication provider to use.
        /// </param>
        /// <param name="parameters">
        /// Provider specific extra parameters that are sent as query string parameters to login endpoint.
        /// </param>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public static Task<MobileServiceUser> LoginAsync(this IMobileServiceClient client, string provider, IDictionary<string, string> parameters)
        {
            MobileServiceUIAuthentication auth = new MobileServiceUIAuthentication(client, provider, parameters);
            return auth.LoginAsync();
        }
    }
}

