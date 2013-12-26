// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
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
        /// <typeparam name="T"></typeparam>
        /// <param name="registration"></param>
        /// <returns></returns>
        public async Task RegisterAsync<T>(T registration) where T : Registration
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
            }
            catch (Exception)
            // TODO: catch (RegistrationGoneException)
            {
                // if we get an RegistrationGoneException (410) from service, we will recreate registration id and will try to do upsert one more time.
            }

            // recreate registration id.
            await this.CreateRegistrationIdAsync(registration);
            await this.UpsertRegistration(registration);
        }

        public async Task<IEnumerable<Registration>> GetRegistrationsForChannelAsync(string channelUri)
        {
            // TODO: Fix exceptions
            //try
            //{
                List<Registration> registrations = new List<Registration>(await this.pushHttpClient.ListRegistrationsAsync(channelUri));
                for (int i = 0; i < registrations.Count; i++)
                {
                    this.localStorageManager.UpdateRegistration(registrations[i]);
                }

                return registrations;
            //}
            //catch (Exception e)
            //{
            //    throw HttpUtilities.ConvertToRegistrationException(e);
            //}
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

            // TODO: Fix exceptions
            //try
            //{
                await this.pushHttpClient.UnregisterAsync(cached.RegistrationId);
                this.localStorageManager.DeleteRegistration(registrationName);
            //}
            //catch (Exception e)
            //{
            //    throw HttpUtilities.ConvertToRegistrationException(e);
            //}
        }

        public async Task DeleteRegistrationsForChannelAsync(string channelUri)
        {
            // TODO: Fix exceptions
            //try
            //{
                List<Registration> registrations = new List<Registration>(await this.pushHttpClient.ListRegistrationsAsync(channelUri));
                foreach (var registration in registrations)
                {
                    await this.pushHttpClient.UnregisterAsync(registration.RegistrationId);
                }

                // clear local storage
                this.localStorageManager.ClearRegistrations();
            //}
            //catch (Exception e)
            //{
            //    throw HttpUtilities.ConvertToRegistrationException(e);
            //}
        }        

        async Task<Registration> CreateRegistrationIdAsync(Registration registration)
        {
            // TODO: Deal with exceptions
            //try
            //{
                registration.RegistrationId = await this.pushHttpClient.CreateRegistrationIdAsync();
                this.localStorageManager.UpdateRegistration(registration.Name, ref registration);
                return registration;
            //}
            //catch (Exception ex)
            //{
            //    // if return as NotFound, it should be notificationHubNotFound
            //    var exception = ex is AggregateException ? ((AggregateException)ex).Flatten().InnerException : ex;
            //    WindowsAzureException azureException = exception as WindowsAzureException;
            //    if (azureException != null && azureException.ErrorCode == (int)HttpStatusCode.NotFound)
            //    {
            //        throw new NotificationHubNotFoundException(exception.Message, exception);
            //    }

            //    throw HttpUtilities.ConvertToRegistrationException(ex);
            //}
        }

        async Task UpsertRegistration<T>(T registration) where T : Registration
        {
            // TODO: Deal with exceptions
            //try
            //{
                await this.pushHttpClient.CreateOrUpdateRegistrationAsync(registration);
                this.localStorageManager.UpdateRegistration(registration.Name, ref registration);
            //}
            //catch (Exception ex)
            //{
            //    // if return as NotFound, it should be notificationHubNotFound
            //    var exception = ex is AggregateException ? ((AggregateException)ex).Flatten().InnerException : ex;
            //    WindowsAzureException azureException = exception as WindowsAzureException;
            //    if (azureException != null && azureException.ErrorCode == (int)HttpStatusCode.NotFound)
            //    {
            //        throw new NotificationHubNotFoundException(exception.Message, exception);
            //    }

            //    throw HttpUtilities.ConvertToRegistrationException(exception);
            //}
        }
    }
}