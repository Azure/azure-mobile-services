//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System.Net.Http;

using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices
{
    using System;
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
            var response = await httpClient.RequestAsync(HttpMethod.Put, string.Format("/push/registrations/?channelUri={0}&platform=wns", channelUri));
            var jsonRegistrations = JToken.Parse(response.Content) as JArray;
            return serializer.Deserialize<Registration>(jsonRegistrations);
        }

        public Task UnregisterAsync(string registrationId)
        {
            return httpClient.RequestAsync(HttpMethod.Delete, "/push/registrations/" + registrationId);
        }

        public async Task<string> CreateRegistrationIdAsync()
        {
            var response = await httpClient.RequestAsync(HttpMethod.Post, "/push/registrationids");
            return response.Content;
        }

        public Task CreateOrUpdateRegistrationAsync(Registration registration)
        {
            return httpClient.RequestAsync(HttpMethod.Put, "/push/registrations/" + registration.RegistrationId, this.serializer.Serialize(registration).ToString());
        }
    }
}