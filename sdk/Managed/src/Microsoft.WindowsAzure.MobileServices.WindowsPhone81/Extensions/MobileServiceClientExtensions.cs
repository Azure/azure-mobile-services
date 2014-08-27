// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;

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
            return LoginAsync(client, provider.ToString());
        }

        /// <summary>
        /// Finishes the login process initiated by the LoginAsync() call, should be called from the App OnActivated event
        /// when it is triggered due to the Web Authentication Broker
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param> 
        /// <param name="result">
        /// Result from the Web Authentication Broker 
        /// </param>
        /// <returns>
        /// Logged in mobile user
        /// </returns>
        public static void LoginComplete(this IMobileServiceClient client, WebAuthenticationBrokerContinuationEventArgs result)
        {
            AuthenticationBroker authBroker = new AuthenticationBroker();

            authBroker.AuthenticationComplete(result.WebAuthenticationResult);
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
        public static Task<MobileServiceUser> LoginAsync(this IMobileServiceClient client, string provider, IDictionary<string, string> parameters)
        {
            MobileServiceUIAuthentication auth = new MobileServiceUIAuthentication(client, provider, parameters);

            return auth.LoginAsync();
        }

        /// <summary>
        /// Extension method to get a <see cref="Push"/> object made from an existing <see cref="MobileServiceClient"/>.
        /// </summary>
        /// <param name="client">
        /// The <see cref="MobileServiceClient"/> to create with.
        /// </param>
        /// <returns>
        /// The <see cref="Push"/> object used for registering for notifications.
        /// </returns>
        public static Push GetPush(this MobileServiceClient client)
        {
            return new Push(client);
        }
    }
}

