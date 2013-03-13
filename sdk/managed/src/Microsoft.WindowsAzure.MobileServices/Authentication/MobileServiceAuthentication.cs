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
        public const string LoginAsyncAuthenticationTokenKey = "authenticationToken";

        /// <summary>
        /// Relative URI fragment of the login endpoint.
        /// </summary>
        public const string LoginAsyncUriFragment = "login";

        /// <summary>
        /// Relative URI fragment of the login/done endpoint.
        /// </summary>
        public const string LoginAsyncDoneUriFragment = "login/done";

        /// <summary>
        /// Initializes a new instance of the <see cref="MobileServiceAuthentication"/> class.
        /// </summary>
        /// <param name="client">
        /// The <see cref="MobileServiceClient"/> associated with this 
        /// MobileServiceLogin instance.
        /// </param>
        /// <param name="provider">
        /// The <see cref="MobileServiceAuthenticationProvider"/> used to authenticate.
        /// </param>
        public MobileServiceAuthentication(IMobileServiceClient client, MobileServiceAuthenticationProvider provider)
        {
            Debug.Assert(client != null, "client should not be null.");

            this.Client = client;
            this.Provider = provider;
        }

        /// <summary>
        /// The <see cref="MobileServiceClient"/> associated with this 
        /// <see cref="MobileServiceAuthentication"/> instance.
        /// </summary>
        protected IMobileServiceClient Client { get; private set; }

        /// <summary>
        /// Indicates whether a login operation is currently in progress.
        /// </summary>
        protected MobileServiceAuthenticationProvider Provider { get; private set; }

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name and 
        /// optional token object.
        /// </summary>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public async Task<MobileServiceUser> LoginAsync()
        {
            if (!Enum.IsDefined(typeof(MobileServiceAuthenticationProvider), Provider))
            {
                throw new ArgumentOutOfRangeException("provider");
            }

            string response = await this.LoginInternalAsync();
                    
                JToken authToken = JToken.Parse(response);

                // Get the Mobile Services auth token and user data
                this.Client.CurrentUser = new MobileServiceUser((string)authToken["user"]["userId"]);
                this.Client.CurrentUser.MobileServiceAuthenticationToken = (string)authToken[LoginAsyncAuthenticationTokenKey];

            return this.Client.CurrentUser;
        }
        
        /// <summary>
        /// Provides Login logic.
        /// </summary>
        /// <returns>
        /// Task that will complete with the response string when the user has finished authentication.
        /// </returns>
        protected abstract Task<string> LoginInternalAsync();
    }
}
