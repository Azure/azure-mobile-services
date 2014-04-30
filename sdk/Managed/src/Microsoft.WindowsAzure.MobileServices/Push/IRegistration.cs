// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.WindowsAzure.MobileServices
{
    internal interface IRegistration
    {
        /// <summary>
        /// Contains notification platform this registration is for
        /// </summary>
        string Platform { get; }

        /// <summary>
        /// If specified, restricts the notifications that the registration will receive to only those that
        /// are annotated with one of the specified tags. Note that a tag with a comma in it will be split into two tags.
        /// </summary>        
        ISet<string> Tags { get; }

        /// <summary>
        /// The unique identifier for the device
        /// </summary>        
        string DeviceId { get; }

        /// <summary>
        /// The registration id.
        /// </summary>
        string RegistrationId { get; }
    }
}
