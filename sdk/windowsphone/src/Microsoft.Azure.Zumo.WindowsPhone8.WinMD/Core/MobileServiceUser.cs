// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;

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
        /// User ID of a successfull authenticated user.
        /// </param>
        internal MobileServiceUser(string userId)
        {
            this.UserId = userId;
        }

        /// <summary>
        /// Gets the user ID of a successfully authenticated user.
        /// </summary>
        public string UserId { get; private set; }
    }
}
