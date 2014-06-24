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
        internal readonly RegistrationManager RegistrationManager;

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

            var storageManager = new LocalStorageManager(client.ApplicationUri.Host);
            var pushHttpClient = new PushHttpClient(client);
            this.RegistrationManager = new RegistrationManager(pushHttpClient, storageManager);
        }

        /// <summary>
        /// Register a particular channelUri
        /// </summary>
        /// <param name="channelUri">The channelUri to register</param>
        /// <returns>Task that completes when registration is complete</returns>
        public Task RegisterNativeAsync(string channelUri)
        {
            return this.RegisterNativeAsync(channelUri, null);
        }

        /// <summary>
        /// Register a particular channelUri
        /// </summary>
        /// <param name="channelUri">The channelUri to register</param>
        /// <param name="tags">The tags to register to receive notifications from</param>
        /// <returns>Task that completes when registration is complete</returns>
        public Task RegisterNativeAsync(string channelUri, IEnumerable<string> tags)
        {
            if (string.IsNullOrWhiteSpace(channelUri))
            {
                throw new ArgumentNullException("channelUri");
            }

            var registration = new MpnsRegistration(channelUri, tags);
            return this.RegistrationManager.RegisterAsync(registration);
        }

        /// <summary>
        /// Register a particular channelUri with a template
        /// </summary>
        /// <param name="channelUri">The channelUri to register</param>
        /// <param name="xmlTemplate">The XmlDocument defining the template</param>
        /// <param name="templateName">The template name</param>
        /// <returns>Task that completes when registration is complete</returns>
        public Task RegisterTemplateAsync(string channelUri, string xmlTemplate, string templateName)
        {
            return this.RegisterTemplateAsync(channelUri, xmlTemplate, templateName, null);
        }

        /// <summary>
        /// Register a particular channelUri with a template
        /// </summary>
        /// <param name="channelUri">The channelUri to register</param>
        /// <param name="xmlTemplate">The XmlDocument defining the template</param>
        /// <param name="templateName">The template name</param>
        /// <param name="tags">The tags to register to receive notifications from</param>
        /// <returns>Task that completes when registration is complete</returns>        
        public Task RegisterTemplateAsync(string channelUri, string xmlTemplate, string templateName, IEnumerable<string> tags)
        {
            if (string.IsNullOrWhiteSpace(channelUri))
            {
                throw new ArgumentNullException("channelUri");
            }

            if (string.IsNullOrWhiteSpace(xmlTemplate))
            {
                throw new ArgumentNullException("xmlTemplate");
            }

            if (string.IsNullOrWhiteSpace(templateName))
            {
                throw new ArgumentNullException("templateName");
            }

            var registration = new MpnsTemplateRegistration(channelUri, xmlTemplate, templateName, tags, null);
            return this.RegistrationManager.RegisterAsync(registration);
        }

        /// <summary>
        /// Unregister any registrations previously registered from this device
        /// </summary>
        /// <returns>Task that completes when unregister is complete</returns>
        public Task UnregisterNativeAsync()
        {
            return this.UnregisterTemplateAsync(MpnsRegistration.NativeRegistrationName);
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
        /// Unregister any registrations with given channelUri
        /// </summary>
        /// <param name="channelUri">The channel Uri</param>
        /// <returns>Task that completes when unregister is complete</returns>
        public Task UnregisterAllAsync(string channelUri)
        {
            if (string.IsNullOrWhiteSpace(channelUri))
            {
                throw new ArgumentNullException("channelUri");
            }

            return this.RegistrationManager.DeleteRegistrationsForChannelAsync(channelUri);
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
                throw new ArgumentNullException("registration.ChannelUri");
            }

            return this.RegistrationManager.RegisterAsync(registration);
        }

        /// <summary>
        /// DEBUG-ONLY: List the registrations made with the service for a channelUri
        /// </summary>
        /// <param name="channelUri">The deviceToken to check for</param>
        /// <returns>List of registrations</returns>
        public Task<List<Registration>> ListRegistrationsAsync(string channelUri)
        {
            return this.RegistrationManager.ListRegistrationsAsync(channelUri);
        }
    }
}