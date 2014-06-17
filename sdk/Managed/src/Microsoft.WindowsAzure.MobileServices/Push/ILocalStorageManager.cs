// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Provides a platform-specific implementation to allow storage of registrationId to registrationName mapping
    /// </summary>
    internal interface ILocalStorageManager
    {
        /// <summary>
        /// Gets a value indicating whether local storage data needs to be refreshed.
        /// </summary>
        bool IsRefreshNeeded { get; set; }

        /// <summary>
        /// Gets the DeviceId of all registrations in this instance of ILocalStorageManager
        /// </summary>
        string PushHandle { get; }

        /// <summary>
        /// Get the registration from local storage
        /// </summary>
        /// <param name="registrationName">The name of the registration mapping to search for</param>
        /// <returns>The StoredRegistrationEntry if it exists or null if it does not.</returns>
        StoredRegistrationEntry GetRegistration(string registrationName);

        /// <summary>
        /// Delete a registration from local storage by name
        /// </summary>
        /// <param name="registrationName">The name of the registration mapping to delete.</param>
        void DeleteRegistrationByName(string registrationName);

        /// <summary>
        /// Delete a registration from local storage by registrationId
        /// </summary>
        /// <param name="registrationId">The registrationId of the registration mapping to delete.</param>
        void DeleteRegistrationByRegistrationId(string registrationId);

        /// <summary>
        /// Update a registration mapping and the deviceId in local storage by registrationName
        /// </summary>
        /// <param name="registrationName">The name of the registration mapping to update.</param>
        /// <param name="registrationId">The registrationId to update.</param>
        /// <param name="registrationDeviceId">The device Id to update the ILocalStorageManager to.</param>
        void UpdateRegistrationByName(string registrationName, string registrationId, string registrationDeviceId);

        /// <summary>
        /// Update a registration mapping and the deviceId in local storage by registrationId
        /// </summary>
        /// <param name="registrationId">The registrationId of the registration mapping to update.</param>
        /// <param name="registrationName">The registration name to update.</param>
        /// <param name="registrationDeviceId">The device Id to update the ILocalStorageManager to.</param>
        void UpdateRegistrationByRegistrationId(string registrationId, string registrationName, string registrationDeviceId);

        /// <summary>
        /// Clear all registrations from local storage.
        /// </summary>
        void ClearRegistrations();

        /// <summary>
        /// Ensure that the ILocalStorageManager instance has the updated DeviceId
        /// </summary>
        /// <param name="refreshedDeviceId">The deviceId to update</param>
        void RefreshFinished(string refreshedDeviceId);
    }
}