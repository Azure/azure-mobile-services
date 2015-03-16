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

#if __UNIFIED__
    using Foundation;
#else
    using MonoTouch.Foundation;
#endif

    /// <summary>
    /// Define a class help to create/update/delete notification registrations
    /// </summary>
    public sealed class Push
    {
        internal readonly PushHttpClient PushHttpClient;
        private MobileServiceClient Client { get; set; }

        internal Push(MobileServiceClient client)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }

            this.PushHttpClient = new PushHttpClient(client);
            this.Client = client;
        }

        /// <summary>
        /// Installation Id used to register the device with Notification Hubs
        /// </summary>
        public string InstallationId
        {
            get
            {
                return this.Client.applicationInstallationId;
            }
        }

        /// <summary>
        /// Register an Installation with particular deviceToken
        /// </summary>
        /// <param name="deviceToken">The deviceToken to register</param>
        /// <returns>Task that completes when registration is complete</returns>
        public Task RegisterAsync(NSData deviceToken)
        {
            return this.RegisterAsync(deviceToken, null);
        }

        /// <summary>
        /// Register an Installation with particular deviceToken and templates
        /// </summary>
        /// <param name="deviceToken">The deviceToken to register</param>
        /// <param name="templates">JSON with one more templates to register</param>
        /// <returns>Task that completes when registration is complete</returns>
        public Task RegisterAsync(NSData deviceToken, JObject templates)
        {
            string channelUri = ParseDeviceToken(deviceToken);
            JObject installation = new JObject();
            installation[PushInstallationProperties.PUSHCHANNEL] = channelUri;
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

        internal static string ParseDeviceToken(NSData deviceToken)
        {
            if (deviceToken == null)
            {
                throw new ArgumentNullException("deviceToken");
            }

            return deviceToken.Description.Trim('<', '>').Replace(" ", string.Empty).ToUpperInvariant();
        }
    }
}