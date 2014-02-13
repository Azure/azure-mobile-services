//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Net.Http;
using System.Reflection;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    internal sealed class PushHttpClient
    {
        private readonly MobileServiceClient client;

        internal PushHttpClient(MobileServiceClient client)
        {
            this.client = client;
        }

        public async Task<IEnumerable<Registration>> ListRegistrationsAsync(string channelUri)
        {
            var response = await this.client.HttpClient.RequestAsync(HttpMethod.Get, string.Format("/push/registrations?deviceId={0}&platform={1}", Uri.EscapeUriString(channelUri), Uri.EscapeUriString(Registration.PlatformConstant)), this.client.CurrentUser);

            return JsonConvert.DeserializeObject<IEnumerable<Registration>>(response.Content, new JsonConverter[] { new RegistrationConverter() });
        }

        public Task UnregisterAsync(string registrationId)
        {
            return this.client.HttpClient.RequestAsync(HttpMethod.Delete, string.Format("/push/registrations/{0}", Uri.EscapeUriString(registrationId)), this.client.CurrentUser, ensureResponseContent: false);
        }

        public async Task<string> CreateRegistrationIdAsync()
        {
            var response = await this.client.HttpClient.RequestAsync(HttpMethod.Post, "/push/registrationids", this.client.CurrentUser, null, null);
            var locationPath = response.Headers.Location.AbsolutePath;
            return locationPath.Substring(locationPath.LastIndexOf('/') + 1);
        }

        public async Task CreateOrUpdateRegistrationAsync(Registration registration)
        {
            var regId = registration.RegistrationId;

            // Ensures RegistrationId is not serialized and sent to service.
            registration.RegistrationId = null;

            var content = JsonConvert.SerializeObject(registration);
            await this.client.HttpClient.RequestAsync(HttpMethod.Put, string.Format("/push/registrations/{0}", Uri.EscapeUriString(regId)), this.client.CurrentUser, content, ensureResponseContent: false);
            
            // Ensure local storage is updated properly
            registration.RegistrationId = regId;
        }
    }

    class RegistrationConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);
            object registration = jObject.Property("templateBody") == null ? new Registration() : new TemplateRegistration();
            serializer.Populate(jObject.CreateReader(), registration);
            return registration;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(Registration).GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo());
        }
    }
}