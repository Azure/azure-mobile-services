// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------'

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Provides extension methods for single sign-on.
    /// </summary>
    public static class SingleSignOnExtensions
    {
        /// <summary>
        /// Log a user into a Mobile Services application given a provider name and optional token object.
        /// </summary>
        /// <param name="provider" type="MobileServiceAuthenticationProvider">
        /// Authentication provider to use.
        /// </param>
        /// <param name="client">
        /// The <see cref="MobileServiceClient"/> to login with.
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
        public static Task<MobileServiceUser> LoginAsync(this IMobileServiceClient client, MobileServiceAuthenticationProvider provider, bool useSingleSignOn)
        {
            return LoginAsync(client, provider.ToString(), useSingleSignOn);
        }

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name and optional token object.
        /// </summary>
        /// <param name="provider">
        /// Authentication provider to use.
        /// </param>
        /// <param name="client">
        /// The <see cref="MobileServiceClient"/> to login with.
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
        public static Task<MobileServiceUser> LoginAsync(this IMobileServiceClient client, string provider, bool useSingleSignOn)
        {
            if(!useSingleSignOn)
            {
                return client.LoginAsync(provider);
            }

            MobileServiceSingleSignOnAuthentication auth = new MobileServiceSingleSignOnAuthentication(client, provider);
            return auth.LoginAsync();
        }
    }
}
