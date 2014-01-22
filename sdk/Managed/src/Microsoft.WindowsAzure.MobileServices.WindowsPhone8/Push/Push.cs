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
        private readonly RegistrationManager registrationManager;

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
            var pushHttpClient = new PushHttpClient(client.HttpClient, client.Serializer);
            this.registrationManager = new RegistrationManager(pushHttpClient, storageManager);
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
        /// <param name="tags">The tags to register to receive notifcations from</param>
        /// <returns>Task that completes when registration is complete</returns>
        public Task RegisterNativeAsync(string channelUri, IEnumerable<string> tags)
        {
            if (string.IsNullOrWhiteSpace(channelUri))
            {
                throw new ArgumentNullException("channelUri");
            }

            var registration = new Registration(channelUri, tags);
            return registrationManager.RegisterAsync(registration);
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
        /// <param name="tags">The tags to register to receive notifcations from</param>
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

            var registration = new TemplateRegistration(channelUri, xmlTemplate, templateName, tags, null);
            return this.registrationManager.RegisterAsync(registration);

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
            return this.registrationManager.UnregisterAsync(templateName);
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

            return this.registrationManager.DeleteRegistrationsForChannelAsync(channelUri);
        }
    }
}