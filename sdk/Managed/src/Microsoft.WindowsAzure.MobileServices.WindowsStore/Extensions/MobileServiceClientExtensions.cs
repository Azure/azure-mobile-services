// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

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
            return LoginAsync(client, provider.ToString());
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
            MobileServiceUIAuthentication auth = new MobileServiceUIAuthentication(client, provider);

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

