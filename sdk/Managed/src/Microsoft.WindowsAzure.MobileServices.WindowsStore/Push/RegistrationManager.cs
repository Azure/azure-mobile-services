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
        readonly PushHttpClient pushHttpClient;
        readonly LocalStorageManager localStorageManager;

        public RegistrationManager(PushHttpClient pushHttpClient, LocalStorageManager storageManager)
        {
            this.pushHttpClient = pushHttpClient;

            this.localStorageManager = storageManager;
        }

        /// <summary>
        /// If local storage does not have this registartionName, we will create a new one.
        /// If local storage has this name, we will call update.
        /// If update failed with 404(not found), we will create a new one.
        /// </summary>
        /// <param name="registration"></param>
        /// <returns></returns>
        public async Task RegisterAsync(Registration registration)
        {
            // if localStorage is empty or has different storage version, we need retrieve registrations and refresh local storage
            if (this.localStorageManager.IsRefreshNeeded)
            {
                string refreshChannelUri = string.IsNullOrEmpty(this.localStorageManager.ChannelUri) ? registration.ChannelUri : this.localStorageManager.ChannelUri;
                await this.GetRegistrationsForChannelAsync(refreshChannelUri);
                this.localStorageManager.RefreshFinished(refreshChannelUri);
            }

            var cached = this.localStorageManager.GetRegistration(registration.Name);
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

        public async Task GetRegistrationsForChannelAsync(string channelUri)
        {
            List<Registration> registrations = new List<Registration>(await this.pushHttpClient.ListRegistrationsAsync(channelUri));
            var count = registrations.Count;
            if (count == 0)
            {
                this.localStorageManager.ClearRegistrations();
            }

            for (int i = 0; i < count; i++)
            {
                this.localStorageManager.UpdateRegistrationByRegistrationId(registrations[i].RegistrationId, registrations[i].Name, registrations[i].ChannelUri);
            }
        }

        public async Task UnregisterAsync(string registrationName)
        {
            if (string.IsNullOrWhiteSpace(registrationName))
            {
                throw new ArgumentNullException("registrationName");
            }

            var cached = this.localStorageManager.GetRegistration(registrationName);
            if (cached == null)
            {
                return;
            }

            await this.pushHttpClient.UnregisterAsync(cached.RegistrationId);
            this.localStorageManager.DeleteRegistrationByName(registrationName);
        }

        public async Task DeleteRegistrationsForChannelAsync(string channelUri)
        {
            List<Registration> registrations = new List<Registration>(await this.pushHttpClient.ListRegistrationsAsync(channelUri));
            foreach (var registration in registrations)
            {
                await this.pushHttpClient.UnregisterAsync(registration.RegistrationId);
                this.localStorageManager.DeleteRegistrationByRegistrationId(registration.RegistrationId);
            }

            // clear local storage
            this.localStorageManager.ClearRegistrations();
        }

        async Task<Registration> CreateRegistrationIdAsync(Registration registration)
        {
            registration.RegistrationId = await this.pushHttpClient.CreateRegistrationIdAsync();
            this.localStorageManager.UpdateRegistrationByName(registration.Name, registration.RegistrationId, registration.ChannelUri);
            return registration;
        }

        async Task UpsertRegistration<T>(T registration) where T : Registration
        {
            await this.pushHttpClient.CreateOrUpdateRegistrationAsync(registration);
            this.localStorageManager.UpdateRegistrationByName(registration.Name, registration.RegistrationId, registration.ChannelUri);
        }
    }
}