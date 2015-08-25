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
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Define a class help to create/delete notification registrations
    /// </summary>
    public sealed class Push
    {
        internal readonly PushHttpClient PushHttpClient;
        private IMobileServiceClient Client { get; set; }

        internal Push(IMobileServiceClient client)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }

            MobileServiceClient internalClient = (MobileServiceClient)client;
            if (internalClient == null)
            {
                throw new ArgumentException("Client must be a MobileServiceClient object");
            }

            this.PushHttpClient = new PushHttpClient(internalClient);
            this.Client = client;
        }

        /// <summary>
        /// Installation Id used to register the device with Notification Hubs
        /// </summary>
        public string InstallationId
        {
            get
            {
                return this.Client.InstallationId;
            }
        }

        /// <summary>
        /// Register an Installation with particular registrationId
        /// </summary>
        /// <param name="registrationId">The registrationId to register</param>
        /// <returns>Task that completes when registration is complete</returns>
        public Task RegisterAsync(string registrationId)
        {
            return this.RegisterAsync(registrationId, null);
        }

        /// <summary>
        /// Register an Installation with particular registrationId and templates
        /// </summary>
        /// <param name="registrationId">The registrationId to register</param>
        /// <param name="templates">JSON with one more templates to register</param>
        /// <returns>Task that completes when registration is complete</returns>
        public Task RegisterAsync(string registrationId, JObject templates)
        {
            JObject installation = new JObject();
            installation[PushInstallationProperties.PUSHCHANNEL] = registrationId;
            installation[PushInstallationProperties.PLATFORM] = Platform.Instance.PushUtility.GetPlatform();
            if (templates != null)
            {
                installation[PushInstallationProperties.TEMPLATES] = templates;
            }
            return this.PushHttpClient.CreateOrUpdateInstallationAsync(installation);
        }

        /// <summary>
        /// Unregister any installations for a particular app
        /// </summary>
        /// <returns>Task that completes when unregister is complete</returns>
        public Task UnregisterAsync()
        {
            return this.PushHttpClient.DeleteInstallationAsync();
        }
    }
}