// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using Windows.Data.Json;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// An authenticated Mobile Services user.
    /// </summary>
    public sealed class MobileServiceUser
    {
        /// <summary>
        /// Initializes a new instance of the MobileServiceUser class.
        /// </summary>
        /// <param name="userId">
        /// User ID of a successfull authenticated user.
        /// </param>
        public MobileServiceUser(string userId)
        {
            this.UserId = userId;
        }

        /// <summary>
        /// Gets the user ID of a successfully authenticated user.
        /// </summary>
        public string UserId { get; private set; }

        /// <summary>
        /// A Windows Azure Mobile Services authentication token for the logged in
        /// end user. If non-nil, the authentication token will be included in all
        /// requests made to the Windows Azure Mobile Service, allowing the client to
        /// perform all actions on the Windows Azure Mobile Service that require
        /// authenticated-user level permissions.
        /// </summary>
        public string MobileServiceAuthenticationToken { get; set; }
    }
}
