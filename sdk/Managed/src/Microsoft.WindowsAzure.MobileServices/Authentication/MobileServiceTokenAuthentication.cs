// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
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
        /// <param name="parameters">
        /// Provider specific extra parameters that are sent as query string parameters to login endpoint.
        /// </param>
        public MobileServiceTokenAuthentication(MobileServiceClient client, string provider, JObject token, IDictionary<string, string> parameters)
            : base(client, provider, parameters)
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
            string path = MobileServiceUrlBuilder.CombinePaths(MobileServiceAuthentication.LoginAsyncUriFragment, this.ProviderName);
            string queryString = MobileServiceUrlBuilder.GetQueryString(this.Parameters);
            string pathAndQuery = MobileServiceUrlBuilder.CombinePathAndQuery(path, queryString);
            return client.HttpClient.RequestWithoutHandlersAsync(HttpMethod.Post, pathAndQuery, client.CurrentUser, token.ToString());
        }
    }
}
