//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices
{
    internal sealed class PushHttpClient
    {
        private readonly MobileServiceClient client;

        internal PushHttpClient(MobileServiceClient client)
        {
            this.client = client;
        }

        public async Task<IEnumerable<Registration>> ListRegistrationsAsync(string deviceId)
        {
            var response = await this.client.MobileAppHttpClient.RequestAsync(HttpMethod.Get, string.Format("push/registrations?deviceId={0}&platform={1}", Uri.EscapeUriString(deviceId), Uri.EscapeUriString(Platform.Instance.PushUtility.GetPlatform())), this.client.CurrentUser);

            return JsonConvert.DeserializeObject<IEnumerable<Registration>>(response.Content, new JsonConverter[] { new RegistrationConverter() });
        }

        public Task UnregisterAsync(string registrationId)
        {
            return this.client.MobileAppHttpClient.RequestAsync(HttpMethod.Delete, string.Format("push/registrations/{0}", Uri.EscapeUriString(registrationId)), this.client.CurrentUser, ensureResponseContent: false);
        }

        public Task DeleteInstallationAsync()
        {
            return this.client.MobileAppHttpClient.RequestAsync(HttpMethod.Delete, string.Format("push/installations/{0}", Uri.EscapeUriString(this.client.applicationInstallationId)), this.client.CurrentUser, ensureResponseContent: false);
        }

        public async Task<string> CreateRegistrationIdAsync()
        {
            var response = await this.client.MobileAppHttpClient.RequestAsync(HttpMethod.Post, "push/registrationids", this.client.CurrentUser, null, null);
            var locationPath = response.Headers.Location.AbsolutePath;
            return locationPath.Substring(locationPath.LastIndexOf('/') + 1);
        }

        public async Task CreateOrUpdateRegistrationAsync(Registration registration)
        {
            var regId = registration.RegistrationId;

            // Ensures RegistrationId is not serialized and sent to service.
            registration.RegistrationId = null;

            var content = JsonConvert.SerializeObject(registration);
            await this.client.MobileAppHttpClient.RequestAsync(HttpMethod.Put, string.Format("push/registrations/{0}", Uri.EscapeUriString(regId)), this.client.CurrentUser, content, ensureResponseContent: false);

            // Ensure local storage is updated properly
            registration.RegistrationId = regId;
        }

        public Task CreateOrUpdateInstallationAsync(JObject installation)
        {
            return this.client.MobileAppHttpClient.RequestAsync(HttpMethod.Put, string.Format("push/installations/{0}", Uri.EscapeUriString(this.client.applicationInstallationId)), this.client.CurrentUser, installation.ToString(), ensureResponseContent: false);
        }
    }
}