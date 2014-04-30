// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.TestFramework;

using Newtonsoft.Json;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    [Tag("unit")]
    [Tag("push")]
    public class PushHttpClientTests : TestBase
    {
        const string DefaultChannelUri = "http://channelUri.com/a b";
        const string DefaultServiceUri = "http://www.test.com";
        const string RegistrationsPath = "/push/registrations";
        const string DefaultRegistrationId = "7313155627197174428-6522078074300559092-1";
        
        const string NativeRegistrationsResponse = "[{\"registrationId\":\"7313155627197174428-6522078074300559092-1\",\"tags\":[\"fooWns\",\"barWns\",\"4de2605e-fd09-4875-a897-c8c4c0a51682\"],\"deviceId\":\"https://bn1.notify.windows.com/?token=AgYAAADs42685sa5PFCEy82eYpuG8WCPB098AWHnwR8kNRQLwUwf%2f9p%2fy0r82m4hxrLSQ%2bfl5aNlSk99E4jrhEatfsWgyutFzqQxHcLk0Xun3mufO2G%2fb2b%2ftjQjCjVcBESjWvY%3d\"}]";
        const string TemplateRegistrationsResponse = "[{\"registrationId\":\"7313155627197174428-6522078074300559092-1\",\"tags\":[\"fooWns\",\"barWns\",\"4de2605e-fd09-4875-a897-c8c4c0a51682\"],\"deviceId\":\"https://bn1.notify.windows.com/?token=AgYAAADs42685sa5PFCEy82eYpuG8WCPB098AWHnwR8kNRQLwUwf%2f9p%2fy0r82m4hxrLSQ%2bfl5aNlSk99E4jrhEatfsWgyutFzqQxHcLk0Xun3mufO2G%2fb2b%2ftjQjCjVcBESjWvY%3d\",\"templateBody\":\"cool template body\"}]";
        const string MixedRegistrationsResponse = "[{\"registrationId\":\"7313155627197174428-6522078074300559092-1\",\"tags\":[\"fooWns\",\"barWns\",\"4de2605e-fd09-4875-a897-c8c4c0a51682\"],\"deviceId\":\"https://bn1.notify.windows.com/?token=AgYAAADs42685sa5PFCEy82eYpuG8WCPB098AWHnwR8kNRQLwUwf%2f9p%2fy0r82m4hxrLSQ%2bfl5aNlSk99E4jrhEatfsWgyutFzqQxHcLk0Xun3mufO2G%2fb2b%2ftjQjCjVcBESjWvY%3d\"}, " +
            "{\"registrationId\":\"7313155627197174428-6522078074300559092-1\",\"tags\":[\"fooWns\",\"barWns\",\"4de2605e-fd09-4875-a897-c8c4c0a51682\"],\"deviceId\":\"https://bn1.notify.windows.com/?token=AgYAAADs42685sa5PFCEy82eYpuG8WCPB098AWHnwR8kNRQLwUwf%2f9p%2fy0r82m4hxrLSQ%2bfl5aNlSk99E4jrhEatfsWgyutFzqQxHcLk0Xun3mufO2G%2fb2b%2ftjQjCjVcBESjWvY%3d\",\"templateBody\":\"cool template body\"}]";

        [AsyncTestMethod]
        public async Task ListRegistrationsEmpty()
        {
            string platform = Platform.Instance.PushUtility.GetPlatform();
            var expectedUri = string.Format("{0}{1}?deviceId={2}&platform={3}", DefaultServiceUri, RegistrationsPath, Uri.EscapeUriString(DefaultChannelUri), Uri.EscapeUriString(platform));
            var hijack = CreateTestHttpHandler(expectedUri, HttpMethod.Get, "[]");
            
            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri, null, hijack);
            var pushHttpClient = new PushHttpClient(mobileClient);

            var registrations = await pushHttpClient.ListRegistrationsAsync(DefaultChannelUri);

            Assert.AreEqual(registrations.Count(), 0, "Expected empty list to return an empty list of registrations.");
        }

        [AsyncTestMethod]
        public async Task ListRegistrationsNative()
        {
            string platform = Platform.Instance.PushUtility.GetPlatform();            
            var expectedUri = string.Format("{0}{1}?deviceId={2}&platform={3}", DefaultServiceUri, RegistrationsPath, Uri.EscapeUriString(DefaultChannelUri), Uri.EscapeUriString(platform));
            var hijack = CreateTestHttpHandler(expectedUri, HttpMethod.Get, NativeRegistrationsResponse);

            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri, null, hijack);
            var pushHttpClient = new PushHttpClient(mobileClient);

            var registrations = await pushHttpClient.ListRegistrationsAsync(DefaultChannelUri);

            Assert.AreEqual(registrations.Count(), 1, "Expected 1 native registration.");
            
            var firstRegistration = registrations.First();
            var nativeReg = Platform.Instance.PushUtility.GetNewNativeRegistration();
            Assert.AreEqual(nativeReg.GetType(), firstRegistration.GetType());

            Assert.AreEqual(firstRegistration.RegistrationId, "7313155627197174428-6522078074300559092-1");
            var tags = firstRegistration.Tags.ToList();
            Assert.AreEqual(tags[0], "fooWns");
            Assert.AreEqual(tags[1], "barWns");
            Assert.AreEqual(tags[2], "4de2605e-fd09-4875-a897-c8c4c0a51682");
            Assert.AreEqual(firstRegistration.RegistrationId, "7313155627197174428-6522078074300559092-1");
            Assert.AreEqual(firstRegistration.DeviceId, "https://bn1.notify.windows.com/?token=AgYAAADs42685sa5PFCEy82eYpuG8WCPB098AWHnwR8kNRQLwUwf%2f9p%2fy0r82m4hxrLSQ%2bfl5aNlSk99E4jrhEatfsWgyutFzqQxHcLk0Xun3mufO2G%2fb2b%2ftjQjCjVcBESjWvY%3d");
        }

        [AsyncTestMethod]
        public async Task ListRegistrationsTemplate()
        {
            string platform = Platform.Instance.PushUtility.GetPlatform();
            var expectedUri = string.Format("{0}{1}?deviceId={2}&platform={3}", DefaultServiceUri, RegistrationsPath, Uri.EscapeUriString(DefaultChannelUri), Uri.EscapeUriString(platform));
            var hijack = CreateTestHttpHandler(expectedUri, HttpMethod.Get, TemplateRegistrationsResponse);

            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri, null, hijack);
            var pushHttpClient = new PushHttpClient(mobileClient);

            var registrations = await pushHttpClient.ListRegistrationsAsync(DefaultChannelUri);

            Assert.AreEqual(registrations.Count(), 1, "Expected 1 template registration.");

            var firstRegistration = registrations.First();
            var templateReg = Platform.Instance.PushUtility.GetNewTemplateRegistration();
            Assert.AreEqual(templateReg.GetType(), firstRegistration.GetType());

            Assert.AreEqual(firstRegistration.RegistrationId, "7313155627197174428-6522078074300559092-1");
            var tags = firstRegistration.Tags.ToList();
            Assert.AreEqual(tags[0], "fooWns");
            Assert.AreEqual(tags[1], "barWns");
            Assert.AreEqual(tags[2], "4de2605e-fd09-4875-a897-c8c4c0a51682");
            Assert.AreEqual(firstRegistration.RegistrationId, "7313155627197174428-6522078074300559092-1");
            Assert.AreEqual(firstRegistration.DeviceId, "https://bn1.notify.windows.com/?token=AgYAAADs42685sa5PFCEy82eYpuG8WCPB098AWHnwR8kNRQLwUwf%2f9p%2fy0r82m4hxrLSQ%2bfl5aNlSk99E4jrhEatfsWgyutFzqQxHcLk0Xun3mufO2G%2fb2b%2ftjQjCjVcBESjWvY%3d");
        }

        [AsyncTestMethod]
        public async Task ListRegistrationsMixed()
        {
            string platform = Platform.Instance.PushUtility.GetPlatform();
            var expectedUri = string.Format("{0}{1}?deviceId={2}&platform={3}", DefaultServiceUri, RegistrationsPath, Uri.EscapeUriString(DefaultChannelUri), Uri.EscapeUriString(platform));
            var hijack = CreateTestHttpHandler(expectedUri, HttpMethod.Get, MixedRegistrationsResponse);

            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri, null, hijack);
            var pushHttpClient = new PushHttpClient(mobileClient);

            var registrations = await pushHttpClient.ListRegistrationsAsync(DefaultChannelUri);
            var registrationsArray = registrations.ToArray();
            Assert.AreEqual(registrationsArray.Length, 2, "Expected 2 registrations, 1 template and 1 native.");

            var firstRegistration = registrationsArray[0];
            var nativeReg = Platform.Instance.PushUtility.GetNewNativeRegistration();
            Assert.AreEqual(nativeReg.GetType(), firstRegistration.GetType());

            Assert.AreEqual(firstRegistration.RegistrationId, "7313155627197174428-6522078074300559092-1");
            var templateTags = firstRegistration.Tags.ToList();
            Assert.AreEqual(templateTags[0], "fooWns");
            Assert.AreEqual(templateTags[1], "barWns");
            Assert.AreEqual(templateTags[2], "4de2605e-fd09-4875-a897-c8c4c0a51682");
            Assert.AreEqual(firstRegistration.RegistrationId, "7313155627197174428-6522078074300559092-1");
            Assert.AreEqual(firstRegistration.DeviceId, "https://bn1.notify.windows.com/?token=AgYAAADs42685sa5PFCEy82eYpuG8WCPB098AWHnwR8kNRQLwUwf%2f9p%2fy0r82m4hxrLSQ%2bfl5aNlSk99E4jrhEatfsWgyutFzqQxHcLk0Xun3mufO2G%2fb2b%2ftjQjCjVcBESjWvY%3d");

            var secondRegistration = registrationsArray[1];
            var templateReg = Platform.Instance.PushUtility.GetNewTemplateRegistration();
            Assert.AreEqual(templateReg.GetType(), secondRegistration.GetType());

            Assert.AreEqual(secondRegistration.RegistrationId, "7313155627197174428-6522078074300559092-1");
            var tags = secondRegistration.Tags.ToList();
            Assert.AreEqual(tags[0], "fooWns");
            Assert.AreEqual(tags[1], "barWns");
            Assert.AreEqual(tags[2], "4de2605e-fd09-4875-a897-c8c4c0a51682");
            Assert.AreEqual(secondRegistration.RegistrationId, "7313155627197174428-6522078074300559092-1");
            Assert.AreEqual(secondRegistration.DeviceId, "https://bn1.notify.windows.com/?token=AgYAAADs42685sa5PFCEy82eYpuG8WCPB098AWHnwR8kNRQLwUwf%2f9p%2fy0r82m4hxrLSQ%2bfl5aNlSk99E4jrhEatfsWgyutFzqQxHcLk0Xun3mufO2G%2fb2b%2ftjQjCjVcBESjWvY%3d");
        }
        
        [AsyncTestMethod]
        public async Task ListRegistrationsErrorWithString()
        {
            string platform = Platform.Instance.PushUtility.GetPlatform();
            var expectedUri = string.Format("{0}{1}?deviceId={2}&platform={3}", DefaultServiceUri, RegistrationsPath, Uri.EscapeUriString(DefaultChannelUri), Uri.EscapeUriString(platform));
            var hijack = CreateTestHttpHandler(expectedUri, HttpMethod.Get, "\"Server threw 500\"", HttpStatusCode.InternalServerError);

            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri, null, hijack);
            var pushHttpClient = new PushHttpClient(mobileClient);

            var exception = await AssertEx.Throws<MobileServiceInvalidOperationException>(() => pushHttpClient.ListRegistrationsAsync(DefaultChannelUri));
            Assert.AreEqual(exception.Message, "Server threw 500");
        }

        [AsyncTestMethod]
        public async Task ListRegistrationsErrorWithError()
        {
            string platform = Platform.Instance.PushUtility.GetPlatform();
            var expectedUri = string.Format("{0}{1}?deviceId={2}&platform={3}", DefaultServiceUri, RegistrationsPath, Uri.EscapeUriString(DefaultChannelUri), Uri.EscapeUriString(platform));
            var hijack = CreateTestHttpHandler(expectedUri, HttpMethod.Get, "{\"error\":\"Server threw 500\"}", HttpStatusCode.InternalServerError);

            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri, null, hijack);
            var pushHttpClient = new PushHttpClient(mobileClient);

            var exception = await AssertEx.Throws<MobileServiceInvalidOperationException>(() => pushHttpClient.ListRegistrationsAsync(DefaultChannelUri));
            Assert.AreEqual(exception.Message, "Server threw 500");
        }
        
        const string CreatePath = "/push/registrationids";
        static readonly Uri LocationUri = new Uri(string.Format("{0}{1}/{2}", DefaultServiceUri, RegistrationsPath, DefaultRegistrationId));

        [AsyncTestMethod]
        public async Task CreateRegistrationId()
        {
            var expectedUri = string.Format("{0}{1}", DefaultServiceUri, CreatePath);
            var hijack = CreateTestHttpHandler(expectedUri, HttpMethod.Post, null, HttpStatusCode.Created, LocationUri);

            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri, null, hijack);
            var pushHttpClient = new PushHttpClient(mobileClient);

            var registrationId = await pushHttpClient.CreateRegistrationIdAsync();

            Assert.AreEqual(registrationId, DefaultRegistrationId);
        }
        
        [AsyncTestMethod]
        public async Task CreateRegistrationIdError()
        {
            var expectedUri = string.Format("{0}{1}", DefaultServiceUri, CreatePath);
            var hijack = CreateTestHttpHandler(expectedUri, HttpMethod.Post, "\"Server threw 500\"", HttpStatusCode.InternalServerError, LocationUri);

            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri, null, hijack);
            var pushHttpClient = new PushHttpClient(mobileClient);
        
            var exception = await AssertEx.Throws<MobileServiceInvalidOperationException>(() => pushHttpClient.CreateRegistrationIdAsync());
            Assert.AreEqual(exception.Message, "Server threw 500");
        }

        [AsyncTestMethod]
        public async Task DeleteRegistration()
        {
            var expectedUri = string.Format("{0}{1}/{2}", DefaultServiceUri, RegistrationsPath, DefaultRegistrationId);
            var hijack = CreateTestHttpHandler(expectedUri, HttpMethod.Delete, null, HttpStatusCode.OK);

            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri, null, hijack);
            var pushHttpClient = new PushHttpClient(mobileClient);

            await pushHttpClient.UnregisterAsync(DefaultRegistrationId);
        }

        [AsyncTestMethod]
        public async Task DeleteRegistrationError()
        {
            var expectedUri = string.Format("{0}{1}/{2}", DefaultServiceUri, RegistrationsPath, DefaultRegistrationId);
            var hijack = CreateTestHttpHandler(expectedUri, HttpMethod.Delete, "\"Server threw 500\"", HttpStatusCode.InternalServerError);

            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri, null, hijack);
            var pushHttpClient = new PushHttpClient(mobileClient);

            var exception = await AssertEx.Throws<MobileServiceInvalidOperationException>(() => pushHttpClient.UnregisterAsync(DefaultRegistrationId));
            Assert.AreEqual(exception.Message, "Server threw 500");
        }

        [AsyncTestMethod]
        public async Task CreateOrUpdateRegistration()
        {
            var expectedUri = string.Format("{0}{1}/{2}", DefaultServiceUri, RegistrationsPath, DefaultRegistrationId); 
            var registration = Platform.Instance.PushUtility.GetNewNativeRegistration(DefaultChannelUri, new[] { "foo", "bar" });
            registration.RegistrationId = DefaultRegistrationId;
            string jsonRegistration = JsonConvert.SerializeObject(registration);
            var hijack = CreateTestHttpHandler(expectedUri, HttpMethod.Put, null, HttpStatusCode.NoContent, expectedRequestContent: jsonRegistration);

            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri, null, hijack);
            var pushHttpClient = new PushHttpClient(mobileClient);

            await pushHttpClient.CreateOrUpdateRegistrationAsync(registration);
        }

        [AsyncTestMethod]
        public async Task CreateOrUpdateRegistrationError()
        {
            var expectedUri = string.Format("{0}{1}/{2}", DefaultServiceUri, RegistrationsPath, DefaultRegistrationId);
            var registration = Platform.Instance.PushUtility.GetNewNativeRegistration(DefaultChannelUri, new[] { "foo", "bar" });
            registration.RegistrationId = DefaultRegistrationId;

            string jsonRegistration = JsonConvert.SerializeObject(registration);
            var hijack = CreateTestHttpHandler(expectedUri, HttpMethod.Put, "\"Server threw 500\"", HttpStatusCode.InternalServerError, expectedRequestContent: jsonRegistration);

            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri, null, hijack);
            var pushHttpClient = new PushHttpClient(mobileClient);

            var exception = await AssertEx.Throws<MobileServiceInvalidOperationException>(() => pushHttpClient.CreateOrUpdateRegistrationAsync(registration));
            Assert.AreEqual(exception.Message, "Server threw 500");
        }

        static DelegatingHandler CreateTestHttpHandler(string expectedUri, HttpMethod expectedMethod, string responseContent, HttpStatusCode? httpStatusCode = null, Uri location = null, string expectedRequestContent = null)
        {
            var handler = new TestHttpHandler
            {
                OnSendingRequest = message =>
                {                    
                    Assert.AreEqual(expectedUri, message.RequestUri.OriginalString);
                    Assert.AreEqual(expectedMethod, message.Method);
                    
                    if (expectedRequestContent != null)
                    {
                        Assert.AreEqual(expectedRequestContent, message.Content.ReadAsStringAsync().Result);
                    }

                    return Task.FromResult(message);
                }
            };

            if (responseContent != null)
            {
                handler.SetResponseContent(responseContent);
            }

            if (location != null)
            {
                handler.Response.Headers.Location = location;
            }

            if (httpStatusCode.HasValue)
            {
                handler.Response.StatusCode = httpStatusCode.Value;
            }

            return handler;
        }
    }
}
