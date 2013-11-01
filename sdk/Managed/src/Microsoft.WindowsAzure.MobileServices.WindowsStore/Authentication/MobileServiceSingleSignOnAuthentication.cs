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
    /// <summary>
    /// Performs single sign-on authentication functionality.
    /// </summary>
    internal class MobileServiceSingleSignOnAuthentication : MobileServiceAuthentication
    {
        /// <summary>
        /// Instantiates a new instance of <see cref="MobileServiceSingleSignOnAuthentication"/>.
        /// Single sign-on requires that the
        /// application's Package SID be registered with the Windows Azure Mobile Service, but it
        /// provides a better experience as HTTP cookies are supported so that users do not have to
        /// login in everytime the application is launched.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="provider">
        /// The authentication provider.
        /// </param>
        public MobileServiceSingleSignOnAuthentication(IMobileServiceClient client, string provider)
            : base(client, provider)
        {
        }

        /// <summary>
        /// Provides Login logic by showing a UI and using single sign-on.
        /// </summary>
        /// <returns>
        /// Task that will complete with the response string when the user has finished authentication.
        /// </returns>
        protected override Task<string> LoginAsyncOverride()
        {
            AuthenticationBroker broker = new AuthenticationBroker();

            return broker.AuthenticateAsync(this.StartUri, this.EndUri, true);
        }
    }
}
