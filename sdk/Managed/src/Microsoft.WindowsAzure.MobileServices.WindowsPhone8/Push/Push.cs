//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.WindowsAzure.MobileServices
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Define a class help to create/update/query/delete notification registrations
    /// </summary>
    /// Exceptions: 
    /// ArgumentException: when argument is not valid.
    /// RegistrationNotFoundException: When try to query/delete not existing registration(s).
    /// RegistrationAuthorizationException: When there is authorization error.
    /// RegistrationException: generatal registration operation error.
    public sealed class Push
    {
        private readonly RegistrationManager registrationManager;        

        public Push(MobileServiceClient client)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }

            this.Client = client;

            var storageManager = new LocalStorageManager(client.ApplicationUri.AbsoluteUri);
            var pushHttpClient = new PushHttpClient(client.HttpClient, client.Serializer);
            this.registrationManager = new RegistrationManager(pushHttpClient, storageManager);
        }

        private MobileServiceClient Client { get; set; }

        public Task RegisterNativeAsync(string channelUri)
        {
            return this.RegisterNativeAsync(channelUri, null);
        }

        public Task RegisterNativeAsync(string channelUri, IEnumerable<string> tags)
        {
            if (string.IsNullOrWhiteSpace(channelUri))
            {
                throw new ArgumentNullException("channelUri");
            }

            var registration = new Registration(channelUri, tags);
            return registrationManager.RegisterAsync(registration);
        }

        public Task RegisterTemplateAsync(string channelUri, string xmlTemplate, string templateName)
        {
            return this.RegisterTemplateAsync(channelUri, xmlTemplate, templateName, null);
        }

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

        public Task UnregisterNativeAsync()
        {
            return this.UnregisterTemplateAsync(Registration.NativeRegistrationName);
        }

        public Task UnregisterTemplateAsync(string templateName)
        {
            return this.registrationManager.UnregisterAsync(templateName);
        }

        public Task UnregisterAllAsync(string channelUri)
        {
            if (string.IsNullOrWhiteSpace(channelUri))
            {
                throw new ArgumentNullException("channelUri");
            }

            return this.registrationManager.DeleteRegistrationsForChannelAsync(channelUri);
        }

        public Task RegisterAsync(Registration registration)
        {
            if (registration == null)
            {
                throw new ArgumentNullException("registration");
            }

            if (string.IsNullOrWhiteSpace(registration.ChannelUri))
            {
                throw new ArgumentNullException("registration.ChannelUri");
            }

            return this.registrationManager.RegisterAsync(registration);
        }

        public Task UnregisterAsync(Registration registration)
        {
            if (registration == null)
            {
                throw new ArgumentNullException("registration");
            }

            if (string.IsNullOrWhiteSpace(registration.ChannelUri))
            {
                throw new ArgumentNullException("registration.ChannelUri");
            }

            return this.registrationManager.UnregisterAsync(registration.Name);
        }
    }
}