//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Net.Http;

using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    internal sealed class PushHttpClient
    {
        private readonly MobileServiceHttpClient httpClient;

        private readonly MobileServiceSerializer serializer;

        internal PushHttpClient(MobileServiceHttpClient httpClient, MobileServiceSerializer serializer)
        {
            this.httpClient = httpClient;
            this.serializer = serializer;
        }

        public async Task<IEnumerable<Registration>> ListRegistrationsAsync(string channelUri)
        {
            var response = await httpClient.RequestAsync(HttpMethod.Get, string.Format("/push/registrations?deviceId={0}&platform=wns", Uri.EscapeUriString(channelUri)));
            var jsonRegistrations = JToken.Parse(response.Content) as JArray;
            return serializer.Deserialize<Registration>(jsonRegistrations);
        }

        public Task UnregisterAsync(string registrationId)
        {
            return httpClient.RequestAsync(HttpMethod.Delete, "/push/registrations/" + registrationId, ensureResponseContent: false);
        }

        public async Task<string> CreateRegistrationIdAsync()
        {
            var response = await httpClient.RequestAsync(HttpMethod.Post, "/push/registrationids", null, null);
            var locationPath = response.Headers.Location.AbsolutePath;
            return locationPath.Substring(locationPath.LastIndexOf('/') + 1);
        }

        public Task CreateOrUpdateRegistrationAsync(Registration registration)
        {
            var content = this.serializer.Serialize(registration).ToString();
            return httpClient.RequestAsync(HttpMethod.Put, "/push/registrations/" + registration.RegistrationId, content, ensureResponseContent: false);
        }
    }
}