// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices
{
    internal class MobileServiceUIAuthentication : MobileServiceAuthentication
    {
        /// <summary>
        /// Instantiates a new instance of <see cref="MobileServiceUIAuthentication"/>.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="provider">
        /// The authentication provider.
        /// </param>
        public MobileServiceUIAuthentication(IMobileServiceClient client, MobileServiceAuthenticationProvider provider)
            :base(client, provider)
        {
        }

        /// <summary>
        /// Provides Login logic by showing a login UI.
        /// </summary>
        /// <returns>
        /// Task that will complete with the response string when the user has finished authentication.
        /// </returns>
        protected override Task<string> LoginInternalAsync()
        {
            string providerName = this.Provider.ToString().ToLower();

            Uri startUri = new Uri(this.Client.ApplicationUri, MobileServiceAuthentication.LoginAsyncUriFragment + "/" + providerName);
            Uri endUri = new Uri(this.Client.ApplicationUri, MobileServiceAuthentication.LoginAsyncDoneUriFragment);

            AuthenticationBroker broker = new AuthenticationBroker();

            return broker.AuthenticateAsync(startUri, endUri, false);
        }
    }
}
