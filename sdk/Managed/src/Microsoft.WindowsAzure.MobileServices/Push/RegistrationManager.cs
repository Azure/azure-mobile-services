// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Help to do registrationManagement and convert the exceptions
    /// </summary>
    internal class RegistrationManager
    {
        internal readonly PushHttpClient PushHttpClient;
        internal readonly ILocalStorageManager LocalStorageManager;

        public RegistrationManager(PushHttpClient pushHttpClient, ILocalStorageManager storageManager)
        {
            this.PushHttpClient = pushHttpClient;

            this.LocalStorageManager = storageManager;
        }

        /// <summary>
        /// If local storage does not have this registrationName, we will create a new one.
        /// If local storage has this name, we will call update.
        /// If update failed with 404(not found), we will create a new one.
        /// </summary>
        /// <param name="registration"></param>
        /// <returns></returns>
        public async Task RegisterAsync(Registration registration)
        {
            // if localStorage is empty or has different storage version, we need retrieve registrations and refresh local storage
            if (this.LocalStorageManager.IsRefreshNeeded)
            {
                string refreshDeviceId = string.IsNullOrEmpty(this.LocalStorageManager.PushHandle) ? registration.PushHandle : this.LocalStorageManager.PushHandle;
                await this.RefreshRegistrationsForChannelAsync(refreshDeviceId);
                this.LocalStorageManager.RefreshFinished(refreshDeviceId);
            }

            var cached = this.LocalStorageManager.GetRegistration(registration.Name);
            if (cached != null)
            {
                registration.RegistrationId = cached.RegistrationId;
            }
            else
            {
                await this.CreateRegistrationIdAsync(registration);
            }

            try
            {
                await this.UpsertRegistration(registration);
                return;
            }
            catch (MobileServiceInvalidOperationException e)
            {
                // if we get an RegistrationGoneException (410) from service, we will recreate registration id and will try to do upsert one more time.
                // The likely cause of this is an expired registration in local storage due to a long unused app.
                if (e.Response.StatusCode != HttpStatusCode.Gone)
                {
                    throw;
                }
            }

            // recreate registration id if we encountered a previously expired registrationId
            await this.CreateRegistrationIdAsync(registration);
            await this.UpsertRegistration(registration);
        }

        public async Task RefreshRegistrationsForChannelAsync(string deviceId)
        {
            var registrations = new List<Registration>(await this.PushHttpClient.ListRegistrationsAsync(deviceId));
            var count = registrations.Count;
            if (count == 0)
            {
                this.LocalStorageManager.ClearRegistrations();
            }

            for (int i = 0; i < count; i++)
            {
                this.LocalStorageManager.UpdateRegistrationByRegistrationId(registrations[i].RegistrationId, registrations[i].Name, registrations[i].PushHandle);
            }
        }

        public async Task UnregisterAsync(string registrationName)
        {
            if (string.IsNullOrWhiteSpace(registrationName))
            {
                throw new ArgumentNullException("registrationName");
            }

            var cached = this.LocalStorageManager.GetRegistration(registrationName);
            if (cached == null)
            {
                return;
            }

            await this.PushHttpClient.UnregisterAsync(cached.RegistrationId);
            this.LocalStorageManager.DeleteRegistrationByName(registrationName);
        }

        public async Task DeleteRegistrationsForChannelAsync(string deviceId)
        {
            var registrations = await this.ListRegistrationsAsync(deviceId);
            foreach (var registration in registrations)
            {
                await this.PushHttpClient.UnregisterAsync(registration.RegistrationId);
                this.LocalStorageManager.DeleteRegistrationByRegistrationId(registration.RegistrationId);
            }

            // clear local storage
            this.LocalStorageManager.ClearRegistrations();
        }

        public async Task<List<Registration>> ListRegistrationsAsync(string deviceId)
        {
            return new List<Registration>(await this.PushHttpClient.ListRegistrationsAsync(deviceId));
        }

        private async Task<Registration> CreateRegistrationIdAsync(Registration registration)
        {
            registration.RegistrationId = await this.PushHttpClient.CreateRegistrationIdAsync();
            this.LocalStorageManager.UpdateRegistrationByName(registration.Name, registration.RegistrationId, registration.PushHandle);
            return registration;
        }

        private async Task UpsertRegistration(Registration registration)
        {
            await this.PushHttpClient.CreateOrUpdateRegistrationAsync(registration);
            this.LocalStorageManager.UpdateRegistrationByName(registration.Name, registration.RegistrationId, registration.PushHandle);
        }
    }
}