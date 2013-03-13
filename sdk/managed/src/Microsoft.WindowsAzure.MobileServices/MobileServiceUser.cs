// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// An authenticated Mobile Services user.
    /// </summary>
    public class MobileServiceUser
    {
        /// <summary>
        /// Initializes a new instance of the MobileServiceUser class.
        /// </summary>
        /// <param name="userId">
        /// User id of a successfully authenticated user.
        /// </param>
        public MobileServiceUser(string userId)
        {
            this.UserId = userId;
        }

        /// <summary>
        /// Gets or sets the user id of a successfully authenticated user.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// A Windows Azure Mobile Services authentication token for the user given by
        /// the <see cref="UserId"/>. If non-null, the authentication token will be 
        /// included in all requests made to the Windows Azure Mobile Service, allowing 
        /// the client to perform all actions on the Windows Azure Mobile Service that 
        /// require authenticated-user level permissions.
        /// </summary>
        public string MobileServiceAuthenticationToken { get; set; }
    }
}
