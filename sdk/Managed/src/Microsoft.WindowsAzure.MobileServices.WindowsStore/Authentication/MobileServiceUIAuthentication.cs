// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
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
        /// <param name="parameters">
        /// Provider specific extra parameters that are sent as query string parameters to login endpoint.
        /// </param>
        public MobileServiceUIAuthentication(IMobileServiceClient client, string provider, IDictionary<string, string> parameters)
            : base(client, provider, parameters)
        {
        }

        /// <summary>
        /// Provides Login logic by showing a login UI.
        /// </summary>
        /// <returns>
        /// Task that will complete with the response string when the user has finished authentication.
        /// </returns>
        protected override Task<string> LoginAsyncOverride()
        {
            AuthenticationBroker broker = new AuthenticationBroker();

            return broker.AuthenticateAsync(this.StartUri, this.EndUri, false);
        }
    }
}
