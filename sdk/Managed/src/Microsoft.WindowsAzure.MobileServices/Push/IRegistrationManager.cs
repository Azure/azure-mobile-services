// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices
{
    internal interface IRegistrationManager
    {
        ILocalStorageManager LocalStorageManager { get; }

        Task DeleteRegistrationsForChannelAsync(string deviceId);
        Task<List<Registration>> ListRegistrationsAsync(string deviceId);
        Task RegisterAsync(Registration registration);
        Task UnregisterAsync(string registrationName);
    }
}
