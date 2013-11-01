// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    ///  Provides extension methods on <see cref="MobileServiceClient"/>.
    /// </summary>
    public static class MobileServiceClientExtensions
    {
        /// <summary>
        /// Name of the  JSON member in the token object that stores the
        /// authentication token fo Microsoft Account.
        /// </summary>
        private const string MicrosoftAccountLoginAsyncAuthenticationTokenKey = "authenticationToken";

        /// <summary>
        /// Log a user into a Mobile Services application given a Microsoft
        /// Account authentication token.
        /// </summary>
        /// <param name="thisClient">
        /// The client with which to login.
        /// </param>
        /// <param name="authenticationToken">
        /// Live SDK session authentication token.
        /// </param>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Login", Justification = "Login is preferred by design")]
        public static Task<MobileServiceUser> LoginWithMicrosoftAccountAsync(this MobileServiceClient thisClient, string authenticationToken)
        {
            JObject token = new JObject();
            token[MicrosoftAccountLoginAsyncAuthenticationTokenKey] = authenticationToken;
            MobileServiceTokenAuthentication tokenAuth = new MobileServiceTokenAuthentication(thisClient,
                MobileServiceAuthenticationProvider.MicrosoftAccount.ToString(),
                token);

            return tokenAuth.LoginAsync();
        }
    }
}
