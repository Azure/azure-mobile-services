// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Security.Authentication.Web;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Provides login functionality for the MobileServiceClient
    /// </summary>
    internal class MobileServiceLogin
    {
        /// <summary>
        /// Name of the  JSON member in the config setting that stores the
        /// authentication token.
        /// </summary>
        private const string LoginAsyncAuthenticationTokenKey = "authenticationToken";

        /// <summary>
        /// Relative URI fragment of the login endpoint.
        /// </summary>
        private const string LoginAsyncUriFragment = "login";

        /// <summary>
        /// Relative URI fragment of the login/done endpoint.
        /// </summary>
        private const string LoginAsyncDoneUriFragment = "login/done";

        /// <summary>
        /// Initializes a new instance of the MobileServiceLogin class.
        /// </summary>
        /// <param name="client">
        /// Reference to the MobileServiceClient associated with this table.
        /// </param>
        /// <param name="ignoreFilters">
        /// Optional parameter to indicate if the client filters should be ignored
        /// and requests should be sent directly. Is <c>true</c> by default. This should
        /// only be set to false for testing purposes when filters are needed to intercept
        /// and validate requests and responses.
        /// </param>
        public MobileServiceLogin(MobileServiceClient client, bool ignoreFilters = true)
        {
            Debug.Assert(client != null, "client should not be null.");
            this.Client = client;
            this.IgnoreFilters = ignoreFilters;
        }

        public MobileServiceClient Client { get; private set; }

        /// <summary>
        /// Indicates whether a login operation is currently in progress.
        /// </summary>
        public bool LoginInProgress { get; private set; }

        /// <summary>
        /// Indicates whether the login should use the client's filters.  Filters are
        /// not used by the login other than for test scenarios.
        /// </summary>
        public bool IgnoreFilters { get; set; }

        /// <summary>
        /// Log a user into a Mobile Services application given an access
        /// token.
        /// </summary>
        /// <param name="authenticationToken">
        /// OAuth access token that authenticates the user.
        /// </param>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public async Task<MobileServiceUser> SendLoginAsync(string authenticationToken)
        {
            if (authenticationToken == null)
            {
                throw new ArgumentNullException("authenticationToken");
            }
            else if (string.IsNullOrEmpty(authenticationToken))
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources.EmptyArgumentExceptionMessage,
                        "authenticationToken"));
            }

            JsonObject request = new JsonObject()
                .Set(LoginAsyncAuthenticationTokenKey, authenticationToken);
            IJsonValue response = await this.Client.RequestAsync("POST", LoginAsyncUriFragment, request, this.IgnoreFilters);

            // Get the Mobile Services auth token and user data
            this.Client.CurrentUser = new MobileServiceUser(response.Get("user").Get("userId").AsString());
            this.Client.CurrentUser.MobileServiceAuthenticationToken = response.Get(LoginAsyncAuthenticationTokenKey).AsString();

            return this.Client.CurrentUser;
        }

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name and optional token object.
        /// </summary>
        /// <param name="provider" type="MobileServiceAuthenticationProvider">
        /// Authentication provider to use.
        /// </param>
        /// <param name="token" type="JsonObject">
        /// Optional, provider specific object with existing OAuth token to log in with.
        /// </param>
        /// <param name="useSingleSignOn">
        /// Indicates that single sign-on should be used. Single sign-on requires that the
        /// application's Package SID be registered with the Windows Azure Mobile Service, but it
        /// provides a better experience as HTTP cookies are supported so that users do not have to
        /// login in everytime the application is launched.
        /// </param>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public async Task<MobileServiceUser> SendLoginAsync(MobileServiceAuthenticationProvider provider, JsonObject token, bool useSingleSignOn)
        {
            if (this.LoginInProgress)
            {
                throw new InvalidOperationException(Resources.Platform_Login_Error_Response);
            }

            if (!Enum.IsDefined(typeof(MobileServiceAuthenticationProvider), provider))
            {
                throw new ArgumentOutOfRangeException("provider");
            }

            string providerName = provider.ToString().ToLower();

            this.LoginInProgress = true;
            try
            {
                IJsonValue response = null;
                if (token != null)
                {
                    // Invoke the POST endpoint to exchange provider-specific token for a Windows Azure Mobile Services token
                    response = await this.Client.RequestAsync("POST", LoginAsyncUriFragment + "/" + providerName, token, this.IgnoreFilters);
                }
                else
                {
                    // Use WebAuthenicationBroker to launch server side OAuth flow using the GET endpoint    
                    WebAuthenticationResult result = null;

                    if (useSingleSignOn)
                    {
                        string endUri = WebAuthenticationBroker.GetCurrentApplicationCallbackUri().AbsoluteUri;
                        Uri startUri = new Uri(this.Client.ApplicationUri, LoginAsyncUriFragment + "/" + providerName + "?sso_end_uri=" + endUri);
                        result = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, startUri);
                    }
                    else
                    {
                        Uri startUri = new Uri(this.Client.ApplicationUri, LoginAsyncUriFragment + "/" + providerName);
                        Uri endUri = new Uri(this.Client.ApplicationUri, LoginAsyncDoneUriFragment);
                        result = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, startUri, endUri);
                    }
                    
                    if (result.ResponseStatus == WebAuthenticationStatus.ErrorHttp)
                    {
                        throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Resources.Authentication_Failed, result.ResponseErrorDetail));
                    }
                    else if (result.ResponseStatus == WebAuthenticationStatus.UserCancel)
                    {
                        throw new InvalidOperationException(Resources.Authentication_Canceled);
                    }

                    int index = result.ResponseData.IndexOf("#token=");
                    if (index > 0)
                    {
                        response = JsonValue.Parse(Uri.UnescapeDataString(result.ResponseData.Substring(index + 7)));
                    }
                    else
                    {
                        index = result.ResponseData.IndexOf("#error=");
                        if (index > 0)
                        {
                            throw new InvalidOperationException(string.Format(
                                CultureInfo.InvariantCulture,
                                Resources.MobileServiceLogin_Login_Error_Response,
                                Uri.UnescapeDataString(result.ResponseData.Substring(index + 7))));
                        }
                        else
                        {
                            throw new InvalidOperationException(Resources.Platform_Login_Invalid_Response_Format);
                        }
                    }
                }

                // Get the Mobile Services auth token and user data
                this.Client.CurrentUser = new MobileServiceUser(response.Get("user").Get("userId").AsString());
                this.Client.CurrentUser.MobileServiceAuthenticationToken = response.Get(LoginAsyncAuthenticationTokenKey).AsString();
            }
            finally
            {
                this.LoginInProgress = false;
            }

            return this.Client.CurrentUser;
        }

    }
}
