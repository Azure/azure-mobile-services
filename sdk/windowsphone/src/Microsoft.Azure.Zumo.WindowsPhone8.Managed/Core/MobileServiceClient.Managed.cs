// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Provides basic access to Mobile Services.
    /// </summary>
    public sealed partial class MobileServiceClient
    {
        /// <summary>
        /// Initializes a new instance of the MobileServiceClient class.
        /// </summary>
        /// <param name="applicationUrl">
        /// The Url to the Mobile Services application.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#", Justification = "Enables easier copy/paste getting started workflow")]
        public MobileServiceClient(string applicationUrl)
            : this(new Uri(applicationUrl))
        {
        }

        /// <summary>
        /// Initializes a new instance of the MobileServiceClient class.
        /// </summary>
        /// <param name="applicationUrl">
        /// The Url to the Mobile Services application.
        /// </param>
        /// <param name="applicationKey">
        /// The application name for the Mobile Services application.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#", Justification = "Enables easier copy/paste getting started workflow")]
        public MobileServiceClient(string applicationUrl, string applicationKey)
            : this(new Uri(applicationUrl), applicationKey)
        {
        }

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
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Login", Justification = "Login is preferred by design")]
        public Task<MobileServiceUser> LoginAsync(string authenticationToken)
        {
            return this.SendLoginAsync(authenticationToken);
        }

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name and optional token object.
        /// </summary>
        /// <param name="provider" type="MobileServiceAuthenticationProvider">
        /// Authentication provider to use.
        /// </param>
        /// <param name="token" type="JObject">
        /// Optional, provider specific object with existing OAuth token to log in with.
        /// </param>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>

        //[SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Login", Justification = "Login is more appropriate than LogOn for our usage.")]
        public Task<MobileServiceUser> LoginAsync(MobileServiceAuthenticationProvider provider, JObject token)
        {
            return this.SendLoginAsync(provider, token);
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
        public Task<MobileServiceUser> LoginAsync(MobileServiceAuthenticationProvider provider)
        {
            return this.SendLoginAsync(provider, null);
        }

        /// <summary>
        /// Gets a reference to a table and its data operations.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the elements in the table.  This implies the name of
        /// the table will either be the type's name or the value of the
        /// DataTableAttribute applied to the type.
        /// </typeparam>
        /// <returns>A reference to the table.</returns>
        public IMobileServiceTable<T> GetTable<T>()
        {
            SerializableType type = SerializableType.Get(typeof(T));
            return new MobileServiceTable<T>(type.TableName, this);
        }

        /// <summary>
        /// Create an exception for an invalid response to a web request.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="request">The request.</param>
        /// <param name="response">The response.</param>
        private static Exception CreateMobileServiceException(string errorMessage, IServiceFilterRequest request, IServiceFilterResponse response)
        {
            Debug.Assert(!string.IsNullOrEmpty(errorMessage), "errorMessage cannot be null or empty!");
            Debug.Assert(request != null, "request cannot be null!");
            Debug.Assert(response != null, "response cannot be null!");

            throw new MobileServiceInvalidOperationException(errorMessage, request, response);
        }
    }
}
