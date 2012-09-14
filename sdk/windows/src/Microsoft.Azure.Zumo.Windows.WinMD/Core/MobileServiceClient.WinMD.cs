// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using Windows.Data.Json;
using Windows.Foundation;

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
