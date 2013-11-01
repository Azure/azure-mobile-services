// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Provides login functionality for the <see cref="MobileServiceClient"/>. 
    /// </summary>
    internal abstract class MobileServiceAuthentication
    {
        /// <summary>
        /// Name of the  JSON member in the config setting that stores the
        /// authentication token.
        /// </summary>
        private const string LoginAsyncAuthenticationTokenKey = "authenticationToken";

        /// <summary>
        /// Relative URI fragment of the login endpoint.
        /// </summary>
        protected const string LoginAsyncUriFragment = "login";

        /// <summary>
        /// Relative URI fragment of the login/done endpoint.
        /// </summary>
        protected const string LoginAsyncDoneUriFragment = "login/done";

        /// <summary>
        /// Initializes a new instance of the <see cref="MobileServiceAuthentication"/> class.
        /// </summary>
        /// <param name="client">
        /// The <see cref="MobileServiceClient"/> associated with this 
        /// MobileServiceLogin instance.
        /// </param>
        /// <param name="providerName">
        /// The <see cref="MobileServiceAuthenticationProvider"/> used to authenticate.
        /// </param>
        public MobileServiceAuthentication(IMobileServiceClient client, string providerName)
        {
            Debug.Assert(client != null, "client should not be null.");
            if (providerName == null)
            {
                throw new ArgumentNullException("providerName");
            }

            this.Client = client;

            this.ProviderName = providerName.ToLower();

            this.StartUri = new Uri(this.Client.ApplicationUri, MobileServiceAuthentication.LoginAsyncUriFragment + "/" + providerName);
            this.EndUri = new Uri(this.Client.ApplicationUri, MobileServiceAuthentication.LoginAsyncDoneUriFragment);
        }

        /// <summary>
        /// The <see cref="MobileServiceClient"/> associated with this 
        /// <see cref="MobileServiceAuthentication"/> instance.
        /// </summary>
        protected IMobileServiceClient Client { get; private set; }

        /// <summary>
        /// The name of the authentication provider used by this
        /// <see cref="MobileServiceAuthentication"/> instance.
        /// </summary>
        protected string ProviderName { get; private set; }

        /// <summary>
        /// The start uri to use for authentication.
        /// The browser-based control should 
        /// first navigate to this Uri in order to start the authenication flow.
        /// </summary>
        protected Uri StartUri { get; private set; }

        /// <summary>
        /// The end Uri to use for authentication.
        /// This Uri indicates that the authentication flow has 
        /// completed. Upon being redirected to any URL that starts with the 
        /// endUrl, the browser-based control must stop navigating and
        /// return the response data.
        /// </summary>
        protected Uri EndUri { get; private set; }

        /// <summary>
        /// Log a user into a Mobile Services application with the provider name and
        /// optional token object from this instance.
        /// </summary>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        internal async Task<MobileServiceUser> LoginAsync()
        {
            string response = await this.LoginAsyncOverride();
            if (!string.IsNullOrEmpty(response))
            {
                JToken authToken = JToken.Parse(response);

                // Get the Mobile Services auth token and user data
                this.Client.CurrentUser = new MobileServiceUser((string)authToken["user"]["userId"]);
                this.Client.CurrentUser.MobileServiceAuthenticationToken = (string)authToken[LoginAsyncAuthenticationTokenKey];
            }

            return this.Client.CurrentUser;
        }
        
        /// <summary>
        /// Provides Login logic.
        /// </summary>
        /// <returns>
        /// Task that will complete with the response string when the user has finished authentication.
        /// </returns>
        protected abstract Task<string> LoginAsyncOverride();
    }
}
