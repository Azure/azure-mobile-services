//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.WindowsAzure.MobileServices
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Define a class help to create/update/delete notification registrations
    /// </summary>
    public sealed class Push
    {
        internal readonly RegistrationManager RegistrationManager;

        internal Push(MobileServiceClient client)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }

            this.Client = client;
            
            var storageManager = new LocalStorageManager(client.ApplicationUri.Host);
            var pushHttpClient = new PushHttpClient(client);
            this.RegistrationManager = new RegistrationManager(pushHttpClient, storageManager);
        }

        private MobileServiceClient Client { get; set; }

        /// <summary>
        /// Register a particular deviceToken
        /// </summary>
        /// <param name="deviceToken">The deviceToken to register</param>
        /// <returns>Task that completes when registration is complete</returns>
        public Task RegisterNativeAsync(string deviceToken)
        {
            return this.RegisterNativeAsync(deviceToken, null);
        }

        /// <summary>
        /// Register a particular deviceToken
        /// </summary>
        /// <param name="deviceToken">The deviceToken to register</param>
        /// <param name="tags">The tags to register to receive notifications from</param>
        /// <returns>Task that completes when registration is complete</returns>
        public Task RegisterNativeAsync(string deviceToken, IEnumerable<string> tags)
        {
            if (string.IsNullOrWhiteSpace(deviceToken))
            {
                throw new ArgumentNullException("deviceToken");
            }

            var registration = new ApnsRegistration(deviceToken, tags);
            return this.RegistrationManager.RegisterAsync(registration);
        } 

        /// <summary>
        /// Register a particular deviceToken with a template
        /// </summary>
        /// <param name="deviceToken">The deviceToken to register</param>
        /// <param name="jsonTemplate">The string defining the template</param>
        /// <param name="expiry">The string defining the expiry template</param>
        /// <param name="templateName">The template name</param>
        /// <returns>Task that completes when registration is complete</returns>
        public Task RegisterTemplateAsync(string deviceToken, string jsonTemplate, string expiry, string templateName)
        {
            return this.RegisterTemplateAsync(deviceToken, jsonTemplate, expiry, templateName, null);
        }

        /// <summary>
        /// Register a particular deviceToken with a template
        /// </summary>
        /// <param name="deviceToken">The deviceToken to register</param>
        /// <param name="jsonTemplate">The string defining the json template</param>
        /// <param name="expiry">The string defining the expiry template</param>
        /// <param name="templateName">The template name</param>
        /// <param name="tags">The tags to register to receive notifications from</param>
        /// <returns>Task that completes when registration is complete</returns>
        public Task RegisterTemplateAsync(string deviceToken, string jsonTemplate, string expiry, string templateName, IEnumerable<string> tags)
        {
            if (string.IsNullOrWhiteSpace(deviceToken))
            {
                throw new ArgumentNullException("deviceToken");
            }

            if (string.IsNullOrWhiteSpace(jsonTemplate))
            {
                throw new ArgumentNullException("jsonTemplate");
            }

            if (string.IsNullOrWhiteSpace(templateName))
            {
                throw new ArgumentNullException("templateName");
            }

            var registration = new ApnsTemplateRegistration(deviceToken, jsonTemplate, expiry, templateName, tags);
            return this.RegistrationManager.RegisterAsync(registration);
        }

        /// <summary>
        /// Unregister any registrations previously registered from this device
        /// </summary>
        /// <returns>Task that completes when unregister is complete</returns>
        public Task UnregisterNativeAsync()
        {
            return this.UnregisterTemplateAsync(Registration.NativeRegistrationName);
        }

        /// <summary>
        /// Unregister any registrations with given templateName registered from this device
        /// </summary>
        /// <param name="templateName">The template name</param>
        /// <returns>Task that completes when unregister is complete</returns>
        public Task UnregisterTemplateAsync(string templateName)
        {
            return this.RegistrationManager.UnregisterAsync(templateName);
        }

        /// <summary>
        /// DEBUG-ONLY: Unregister any registrations with given deviceToken
        /// </summary>
        /// <param name="deviceToken">The device token</param>
        /// <returns>Task that completes when unregister is complete</returns>
        public Task UnregisterAllAsync(string deviceToken)
        {
            if (string.IsNullOrWhiteSpace(deviceToken))
            {
                throw new ArgumentNullException("deviceToken");
            }

            deviceToken = deviceToken.ToUpperInvariant();
            return this.RegistrationManager.DeleteRegistrationsForChannelAsync(deviceToken);
        }

        /// <summary>
        /// Register for notifications
        /// </summary>
        /// <param name="registration">The object defining the registration</param>
        /// <returns>Task that will complete when the registration is completed</returns>
        public Task RegisterAsync(Registration registration)
        {
            if (registration == null)
            {
                throw new ArgumentNullException("registration");
            }

            if (string.IsNullOrWhiteSpace(registration.PushHandle))
            {
                throw new ArgumentNullException("registration.deviceToken");
            }

            registration.PushHandle = registration.PushHandle.ToUpperInvariant();
            return this.RegistrationManager.RegisterAsync(registration);
        }

        /// <summary>
        /// DEBUG-ONLY: List the registrations made with the service for a deviceToken
        /// </summary>
        /// <param name="deviceToken">The deviceToken to check for</param>
        /// <returns>List of registrations</returns>
        public Task<List<Registration>> ListRegistrationsAsync(string deviceToken)
        {
            deviceToken = deviceToken.ToUpperInvariant();
            return this.RegistrationManager.ListRegistrationsAsync(deviceToken);
        }
    }
}