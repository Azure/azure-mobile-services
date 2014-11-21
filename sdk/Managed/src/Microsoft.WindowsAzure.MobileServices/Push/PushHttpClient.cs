//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Net.Http;

using Newtonsoft.Json;

namespace Microsoft.WindowsAzure.MobileServices
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.MobileServices.Http;

    internal sealed class PushHttpClient
    {
        private readonly MobileServiceClient client;

        internal PushHttpClient(MobileServiceClient client)
        {
            this.client = client;
        }

        public async Task<IEnumerable<Registration>> ListRegistrationsAsync(string deviceId)
        {
            string route = RouteHelper.GetRoute(this.client, RouteKind.Push, "registrations");
            var response = await this.client.HttpClient.RequestAsync(HttpMethod.Get, string.Format("{0}?deviceId={1}&platform={2}", route, Uri.EscapeUriString(deviceId), Uri.EscapeUriString(Platform.Instance.PushUtility.GetPlatform())), this.client.CurrentUser);

            return JsonConvert.DeserializeObject<IEnumerable<Registration>>(response.Content, new JsonConverter[] { new RegistrationConverter() });
        }

        public Task UnregisterAsync(string registrationId)
        {
            string route = RouteHelper.GetRoute(this.client, RouteKind.Push, "registrations");
            return this.client.HttpClient.RequestAsync(HttpMethod.Delete, string.Format("{0}/{1}", route, Uri.EscapeUriString(registrationId)), this.client.CurrentUser, ensureResponseContent: false);
        }

        public async Task<string> CreateRegistrationIdAsync()
        {
            string route = RouteHelper.GetRoute(this.client, RouteKind.Push, "registrationids");
            var response = await this.client.HttpClient.RequestAsync(HttpMethod.Post, route, this.client.CurrentUser, null, null);
            var locationPath = response.Headers.Location.AbsolutePath;
            return locationPath.Substring(locationPath.LastIndexOf('/') + 1);
        }

        public async Task CreateOrUpdateRegistrationAsync(Registration registration)
        {
            var regId = registration.RegistrationId;

            // Ensures RegistrationId is not serialized and sent to service.
            registration.RegistrationId = null;

            var content = JsonConvert.SerializeObject(registration);
            string route = RouteHelper.GetRoute(this.client, RouteKind.Push, "registrations");
            await this.client.HttpClient.RequestAsync(HttpMethod.Put, string.Format("{0}/{1}", route, Uri.EscapeUriString(regId)), this.client.CurrentUser, content, ensureResponseContent: false);
            
            // Ensure local storage is updated properly
            registration.RegistrationId = regId;
        }
    }    
}