﻿// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Metadata;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Provides basic access to Mobile Services.
    /// </summary>
    public partial class MobileServiceClient
    {
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
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Login", Justification = "Login is more appropriate than LogOn for our usage.")]
        public IAsyncOperation<MobileServiceUser> LoginAsync(string authenticationToken)
        {
            return this.SendLoginAsync(authenticationToken).AsAsyncOperation();
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
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Login", Justification = "Login is more appropriate than LogOn for our usage.")]
        public IAsyncOperation<MobileServiceUser> LoginAsync(MobileServiceAuthenticationProvider provider, JsonObject token)
        {
            return this.SendLoginAsync(provider, token).AsAsyncOperation();
        }

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="provider" type="MobileServiceAuthenticationProvider">
        /// Authentication provider to use.
        /// </param>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Login", Justification = "Login is more appropriate than LogOn for our usage.")]
        [DefaultOverloadAttribute]
        public IAsyncOperation<MobileServiceUser> LoginAsync(MobileServiceAuthenticationProvider provider)
        {
            return this.SendLoginAsync(provider, null).AsAsyncOperation();
        }

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="provider" type="MobileServiceAuthenticationProvider">
        /// Authentication provider to use.
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
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Login", Justification = "Login is more appropriate than LogOn for our usage.")]
        [DefaultOverloadAttribute]
        public IAsyncOperation<MobileServiceUser> LoginAsync(MobileServiceAuthenticationProvider provider, bool useSingleSignOn)
        {
            return this.SendLoginAsync(provider, null, useSingleSignOn).AsAsyncOperation();
        }

        /// <summary>
        /// Create an exception for an invalid response to a web request.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="request">The request.</param>
        /// <param name="response">The response.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "request", Justification = "Not used in this overload, but used by C# version")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "response", Justification = "Not used in this overload, but used by C# version")]
        private static Exception CreateMobileServiceException(string errorMessage, IServiceFilterRequest request, IServiceFilterResponse response)
        {
            Debug.Assert(!string.IsNullOrEmpty(errorMessage), "errorMessage cannot be null or empty!");
            Debug.Assert(request != null, "request cannot be null!");
            Debug.Assert(response != null, "response cannot be null!");
            return new InvalidOperationException(errorMessage);
        }
    }
}
