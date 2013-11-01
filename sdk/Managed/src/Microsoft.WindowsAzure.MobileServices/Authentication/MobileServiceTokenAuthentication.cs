// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices
{
    internal class MobileServiceTokenAuthentication : MobileServiceAuthentication
    {
        /// <summary>
        /// The token to send.
        /// </summary>
        private readonly JObject token;

        /// <summary>
        /// The <see cref="MobileServiceClient"/> used by this authentication session.
        /// </summary>
        private readonly MobileServiceClient client;

        /// <summary>
        /// Instantiates a new instance of <see cref="MobileServiceTokenAuthentication"/>.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="provider">
        /// The authentication provider.
        /// </param>
        /// <param name="token">
        /// The token.
        /// </param>
        public MobileServiceTokenAuthentication(MobileServiceClient client, string provider, JObject token)
            :base(client, provider)
        {
            Debug.Assert(client != null, "client should not be null.");
            Debug.Assert(token != null, "token should not be null.");

            this.client = client;
            this.token = token;
        }

        /// <summary>
        /// Provides Login logic for an existing token.
        /// </summary>
        /// <returns>
        /// Task that will complete with the response string when the user has finished authentication.
        /// </returns>
        protected override Task<string> LoginAsyncOverride()
        {
            return client.HttpClient.RequestWithoutHandlersAsync(HttpMethod.Post, MobileServiceAuthentication.LoginAsyncUriFragment + "/" + this.ProviderName, token.ToString());
        }
    }
}
