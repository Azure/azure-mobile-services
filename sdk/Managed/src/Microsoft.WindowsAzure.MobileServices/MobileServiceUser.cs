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
        private string userId;
        private string mobileServiceAuthenticationToken;

        /// <summary>
        /// Initializes a new instance of the MobileServiceUser class.
        /// </summary>
        /// <param name="userId">
        /// User id of a successfully authenticated user.
        /// </param>
        public MobileServiceUser(string userId)
        {
            this.userId = userId;
        }

        /// <summary>
        /// Gets or sets the user id of a successfully authenticated user.
        /// </summary>
        public virtual string UserId 
        {
            get { return this.userId; }
            set { this.userId = value; }
        }

        /// <summary>
        /// A Microsoft Azure Mobile Services authentication token for the user given by
        /// the <see cref="UserId"/>. If non-null, the authentication token will be 
        /// included in all requests made to the Microsoft Azure Mobile Service, allowing 
        /// the client to perform all actions on the Microsoft Azure Mobile Service that 
        /// require authenticated-user level permissions.
        /// </summary>        
        public virtual string MobileServiceAuthenticationToken
        {
            get { return this.mobileServiceAuthenticationToken; }
            set { this.mobileServiceAuthenticationToken = value; }
        }
    }
}
