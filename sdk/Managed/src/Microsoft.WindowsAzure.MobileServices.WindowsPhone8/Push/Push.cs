//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.WindowsAzure.MobileServices
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Define a class help to create/update/delete notification registrations
    /// </summary>    
    public sealed class Push
    {
        /// <summary>
        /// Creates a Push object for registering for notifications
        /// </summary>
        /// <param name="client">The MobileServiceClient to create with.</param>
        public Push(MobileServiceClient client)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }

            var pushHttpClient = new PushHttpClient(client);
        }
    }
}