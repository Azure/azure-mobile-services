//// ----------------------------------------------------------------------------
//// Copyright (c) Microsoft Corporation. All rights reserved.
//// ----------------------------------------------------------------------------

//using System;
//using System.Linq;
//using System.Net;
//using System.Net.Http;
//using System.Threading.Tasks;
//using Microsoft.WindowsAzure.MobileServices.TestFramework;

//using Newtonsoft.Json;

//namespace Microsoft.WindowsAzure.MobileServices.Test
//{
//    [Tag("unit")]
//    [Tag("push")]
//    [Tag("notNetFramework")]
//    public class PushHttpClientTests : TestBase
//    {
//        const string DefaultChannelUri = "http://channelUri.com/a b";
//        const string DefaultServiceUri = "http://www.test.com";
//        const string RegistrationsPath = "/push/registrations";
//        const string DefaultRegistrationId = "7313155627197174428-6522078074300559092-1";
        
//        const string NativeRegistrationsResponse = "[{\"registrationId\":\"7313155627197174428-6522078074300559092-1\",\"tags\":[\"fooWns\",\"barWns\",\"4de2605e-fd09-4875-a897-c8c4c0a51682\"],\"deviceId\":\"http://channelUri.com/a%20b\"}]";
//        const string TemplateRegistrationsResponse = "[{\"registrationId\":\"7313155627197174428-6522078074300559092-1\",\"tags\":[\"fooWns\",\"barWns\",\"4de2605e-fd09-4875-a897-c8c4c0a51682\"],\"deviceId\":\"http://channelUri.com/a%20b\",\"templateBody\":\"cool template body\"}]";
//        const string MixedRegistrationsResponse = "[{\"registrationId\":\"7313155627197174428-6522078074300559092-1\",\"tags\":[\"fooWns\",\"barWns\",\"4de2605e-fd09-4875-a897-c8c4c0a51682\"],\"deviceId\":\"http://channelUri.com/a%20b\"}, " +
//            "{\"registrationId\":\"7313155627197174428-6522078074300559092-1\",\"tags\":[\"fooWns\",\"barWns\",\"4de2605e-fd09-4875-a897-c8c4c0a51682\"],\"deviceId\":\"http://channelUri.com/a%20b\",\"templateBody\":\"cool template body\"}]";

//        [AsyncTestMethod]
//        public async Task ListRegistrations_Empty()
//        {
//            string platform = Platform.Instance.PushUtility.GetPlatform();           

//            // Ensure Uri and method are correct for request and specify body to return for empty registrations list
//            var expectedUri = string.Format("{0}{1}?deviceId={2}&platform={3}", DefaultServiceUri, RegistrationsPath, Uri.EscapeUriString(DefaultChannelUri), Uri.EscapeUriString(platform));
//            var hijack = CreateTestHttpHandler(expectedUri, HttpMethod.Get, "[]");
            
//            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri, null, hijack);
//            var pushHttpClient = new PushHttpClient(mobileClient);

//            var registrations = await pushHttpClient.ListRegistrationsAsync(DefaultChannelUri);

//            Assert.AreEqual(registrations.Count(), 0, "Expected empty list to return an empty list of registrations.");
//        }

//        [AsyncTestMethod]
//        public async Task ListRegistrations_Native()
//        {
//            string platform = Platform.Instance.PushUtility.GetPlatform();

//            // Ensure Uri and method are correct for request and specify body to return for registrations list with only 1 native registration
//            var expectedUri = string.Format("{0}{1}?deviceId={2}&platform={3}", DefaultServiceUri, RegistrationsPath, Uri.EscapeUriString(DefaultChannelUri), Uri.EscapeUriString(platform));
//            var hijack = CreateTestHttpHandler(expectedUri, HttpMethod.Get, NativeRegistrationsResponse);

//            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri, null, hijack);
//            var pushHttpClient = new PushHttpClient(mobileClient);

//            var registrations = await pushHttpClient.ListRegistrationsAsync(DefaultChannelUri);

//            Assert.AreEqual(registrations.Count(), 1, "Expected 1 registration.");
            
//            var firstRegistration = registrations.First();
//            var nativeReg = Platform.Instance.PushUtility.GetNewNativeRegistration();
//            Assert.AreEqual(nativeReg.GetType(), firstRegistration.GetType(), "The type of the registration returned from ListRegistrationsAsync is not of the correct type.");

//            Assert.AreEqual(firstRegistration.RegistrationId, DefaultRegistrationId, "The registrationId returned from ListRegistrationsAsync is not correct.");

//            var tags = firstRegistration.Tags.ToList();
//            Assert.AreEqual(tags[0], "fooWns", "tag[0] on the registration is not correct.");
//            Assert.AreEqual(tags[1], "barWns", "tag[1] on the registration is not correct.");
//            Assert.AreEqual(tags[2], "4de2605e-fd09-4875-a897-c8c4c0a51682", "tag[2] on the registration is not correct.");

//            Assert.AreEqual(firstRegistration.PushHandle, DefaultChannelUri, "The DeviceId on the registration is not correct.");
//        }

//        [AsyncTestMethod]
//        public async Task ListRegistrations_Template()
//        {
//            string platform = Platform.Instance.PushUtility.GetPlatform();
//            var expectedUri = string.Format("{0}{1}?deviceId={2}&platform={3}", DefaultServiceUri, RegistrationsPath, Uri.EscapeUriString(DefaultChannelUri), Uri.EscapeUriString(platform));
//            var hijack = CreateTestHttpHandler(expectedUri, HttpMethod.Get, TemplateRegistrationsResponse);

//            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri, null, hijack);
//            var pushHttpClient = new PushHttpClient(mobileClient);

//            var registrations = await pushHttpClient.ListRegistrationsAsync(DefaultChannelUri);

//            Assert.AreEqual(registrations.Count(), 1, "Expected 1 registration.");

//            var firstRegistration = registrations.First();
//            var templateReg = Platform.Instance.PushUtility.GetNewTemplateRegistration();
//            Assert.AreEqual(templateReg.GetType(), firstRegistration.GetType(), "The type of the registration returned from ListRegistrationsAsync is not of the correct type.");

//            Assert.AreEqual(firstRegistration.RegistrationId, DefaultRegistrationId, "The registrationId returned from ListRegistrationsAsync is not correct.");

//            var tags = firstRegistration.Tags.ToList();
//            Assert.AreEqual(tags[0], "fooWns", "tag[0] on the registration is not correct.");
//            Assert.AreEqual(tags[1], "barWns", "tag[1] on the registration is not correct.");
//            Assert.AreEqual(tags[2], "4de2605e-fd09-4875-a897-c8c4c0a51682", "tag[2] on the registration is not correct.");

//            Assert.AreEqual(firstRegistration.PushHandle, DefaultChannelUri, "The DeviceId on the registration is not correct.");
//        }

//        [AsyncTestMethod]
//        public async Task ListRegistrations_NativeAndTemplate()
//        {
//            string platform = Platform.Instance.PushUtility.GetPlatform();
//            var expectedUri = string.Format("{0}{1}?deviceId={2}&platform={3}", DefaultServiceUri, RegistrationsPath, Uri.EscapeUriString(DefaultChannelUri), Uri.EscapeUriString(platform));
//            var hijack = CreateTestHttpHandler(expectedUri, HttpMethod.Get, MixedRegistrationsResponse);

//            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri, null, hijack);
//            var pushHttpClient = new PushHttpClient(mobileClient);

//            var registrations = await pushHttpClient.ListRegistrationsAsync(DefaultChannelUri);
//            var registrationsArray = registrations.ToArray();
//            Assert.AreEqual(registrationsArray.Length, 2, "Expected 2 registrations.");

//            var firstRegistration = registrationsArray[0];
//            var nativeReg = Platform.Instance.PushUtility.GetNewNativeRegistration();
//            Assert.AreEqual(nativeReg.GetType(), firstRegistration.GetType(), "The type of the native registration returned from ListRegistrationsAsync is not of the correct type.");

//            Assert.AreEqual(firstRegistration.RegistrationId, DefaultRegistrationId, "The native registrationId returned from ListRegistrationsAsync is not correct.");
//            var nativeTags = firstRegistration.Tags.ToList();
//            Assert.AreEqual(nativeTags[0], "fooWns", "nativeTags[0] on the registration is not correct.");
//            Assert.AreEqual(nativeTags[1], "barWns", "nativeTags[1] on the registration is not correct.");
//            Assert.AreEqual(nativeTags[2], "4de2605e-fd09-4875-a897-c8c4c0a51682", "nativeTags[2] on the registration is not correct.");

//            Assert.AreEqual(firstRegistration.PushHandle, DefaultChannelUri, "The DeviceId on the native registration is not correct.");

//            var secondRegistration = registrationsArray[1];
//            var templateReg = Platform.Instance.PushUtility.GetNewTemplateRegistration();
//            Assert.AreEqual(templateReg.GetType(), secondRegistration.GetType(), "The type of the template registration returned from ListRegistrationsAsync is not of the correct type.");

//            Assert.AreEqual(secondRegistration.RegistrationId, DefaultRegistrationId, "The template registrationId returned from ListRegistrationsAsync is not correct.");
//            var templateTags = secondRegistration.Tags.ToList();
//            Assert.AreEqual(templateTags[0], "fooWns", "templateTags[0] on the registration is not correct.");
//            Assert.AreEqual(templateTags[1], "barWns", "templateTags[1] on the registration is not correct.");
//            Assert.AreEqual(templateTags[2], "4de2605e-fd09-4875-a897-c8c4c0a51682", "templateTags[2] on the registration is not correct.");

//            Assert.AreEqual(secondRegistration.PushHandle, DefaultChannelUri, "The DeviceId on the template registration is not correct.");
//        }
        
//        [AsyncTestMethod]
//        public async Task ListRegistrations_Error_WithStringBody()
//        {
//            string platform = Platform.Instance.PushUtility.GetPlatform();
//            var expectedUri = string.Format("{0}{1}?deviceId={2}&platform={3}", DefaultServiceUri, RegistrationsPath, Uri.EscapeUriString(DefaultChannelUri), Uri.EscapeUriString(platform));
//            var hijack = CreateTestHttpHandler(expectedUri, HttpMethod.Get, "\"Server threw 500\"", HttpStatusCode.InternalServerError);

//            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri, null, hijack);
//            var pushHttpClient = new PushHttpClient(mobileClient);

//            var exception = await AssertEx.Throws<MobileServiceInvalidOperationException>(
//                () => pushHttpClient.ListRegistrationsAsync(DefaultChannelUri));
//            Assert.AreEqual(exception.Message, "Server threw 500");
//        }

//        [AsyncTestMethod]
//        public async Task ListRegistrations_Error_WithError()
//        {
//            string platform = Platform.Instance.PushUtility.GetPlatform();
//            var expectedUri = string.Format("{0}{1}?deviceId={2}&platform={3}", DefaultServiceUri, RegistrationsPath, Uri.EscapeUriString(DefaultChannelUri), Uri.EscapeUriString(platform));
//            var hijack = CreateTestHttpHandler(expectedUri, HttpMethod.Get, "{\"error\":\"Server threw 500\"}", HttpStatusCode.InternalServerError);

//            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri, null, hijack);
//            var pushHttpClient = new PushHttpClient(mobileClient);

//            var exception = await AssertEx.Throws<MobileServiceInvalidOperationException>
//                (() => pushHttpClient.ListRegistrationsAsync(DefaultChannelUri));
//            Assert.AreEqual(exception.Message, "Server threw 500");
//        }
        
//        const string CreatePath = "/push/registrationids";
//        static readonly Uri LocationUri = new Uri(string.Format("{0}{1}/{2}", DefaultServiceUri, RegistrationsPath, DefaultRegistrationId));

//        [AsyncTestMethod]
//        public async Task CreateRegistrationId()
//        {
//            var expectedUri = string.Format("{0}{1}", DefaultServiceUri, CreatePath);
//            var hijack = CreateTestHttpHandler(expectedUri, HttpMethod.Post, null, HttpStatusCode.Created, LocationUri);

//            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri, null, hijack);
//            var pushHttpClient = new PushHttpClient(mobileClient);

//            var registrationId = await pushHttpClient.CreateRegistrationIdAsync();

//            Assert.AreEqual(registrationId, DefaultRegistrationId, "Expected CreateRegistrationIdAsync to return correct RegistrationId.");
//        }
        
//        [AsyncTestMethod]
//        public async Task CreateRegistrationId_Error()
//        {
//            var expectedUri = string.Format("{0}{1}", DefaultServiceUri, CreatePath);
//            var hijack = CreateTestHttpHandler(expectedUri, HttpMethod.Post, "\"Server threw 500\"", HttpStatusCode.InternalServerError, LocationUri);

//            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri, null, hijack);
//            var pushHttpClient = new PushHttpClient(mobileClient);
        
//            var exception = await AssertEx.Throws<MobileServiceInvalidOperationException>(() => pushHttpClient.CreateRegistrationIdAsync());
//            Assert.AreEqual(exception.Message, "Server threw 500");
//        }

//        [AsyncTestMethod]
//        public async Task DeleteRegistration()
//        {
//            var expectedUri = string.Format("{0}{1}/{2}", DefaultServiceUri, RegistrationsPath, DefaultRegistrationId);
//            var hijack = CreateTestHttpHandler(expectedUri, HttpMethod.Delete, null, HttpStatusCode.OK);

//            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri, null, hijack);
//            var pushHttpClient = new PushHttpClient(mobileClient);

//            // Because Unregistrer returns nothing, the only test we can perform is that the Http Method and body are correct
//            // and that UnregisterAsync does not throw if Ok is returned
//            await pushHttpClient.UnregisterAsync(DefaultRegistrationId);
//        }

//        [AsyncTestMethod]
//        public async Task DeleteRegistration_Error()
//        {
//            var expectedUri = string.Format("{0}{1}/{2}", DefaultServiceUri, RegistrationsPath, DefaultRegistrationId);
//            var hijack = CreateTestHttpHandler(expectedUri, HttpMethod.Delete, "\"Server threw 500\"", HttpStatusCode.InternalServerError);

//            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri, null, hijack);
//            var pushHttpClient = new PushHttpClient(mobileClient);

//            var exception = await AssertEx.Throws<MobileServiceInvalidOperationException>(
//                () => pushHttpClient.UnregisterAsync(DefaultRegistrationId));
//            Assert.AreEqual(exception.Message, "Server threw 500");
//        }

//        [AsyncTestMethod]
//        public async Task CreateOrUpdateRegistration()
//        {
//            var expectedUri = string.Format("{0}{1}/{2}", DefaultServiceUri, RegistrationsPath, DefaultRegistrationId); 
//            var registration = Platform.Instance.PushUtility.GetNewNativeRegistration(DefaultChannelUri, new[] { "foo", "bar" });
//            registration.RegistrationId = DefaultRegistrationId;
//            string jsonRegistration = JsonConvert.SerializeObject(registration);

//            // The entire test is performed within the TestHttpHandler.
//            // We verify that the request content is correct and that the method is correct.
//            var hijack = CreateTestHttpHandler(expectedUri, HttpMethod.Put, null, HttpStatusCode.NoContent, expectedRequestContent: jsonRegistration);

//            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri, null, hijack);
//            var pushHttpClient = new PushHttpClient(mobileClient);

//            await pushHttpClient.CreateOrUpdateRegistrationAsync(registration);
//        }

//        [AsyncTestMethod]
//        public async Task CreateOrUpdateRegistration_Error()
//        {
//            var expectedUri = string.Format("{0}{1}/{2}", DefaultServiceUri, RegistrationsPath, DefaultRegistrationId);
//            var registration = Platform.Instance.PushUtility.GetNewNativeRegistration(DefaultChannelUri, new[] { "foo", "bar" });
//            registration.RegistrationId = DefaultRegistrationId;

//            string jsonRegistration = JsonConvert.SerializeObject(registration);
//            var hijack = CreateTestHttpHandler(expectedUri, HttpMethod.Put, "\"Server threw 500\"", HttpStatusCode.InternalServerError, expectedRequestContent: jsonRegistration);

//            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri, null, hijack);
//            var pushHttpClient = new PushHttpClient(mobileClient);

//            var exception = await AssertEx.Throws<MobileServiceInvalidOperationException>(() => pushHttpClient.CreateOrUpdateRegistrationAsync(registration));
//            Assert.AreEqual(exception.Message, "Server threw 500");
//        }

//        static DelegatingHandler CreateTestHttpHandler(string expectedUri, HttpMethod expectedMethod, string responseContent, HttpStatusCode? httpStatusCode = null, Uri location = null, string expectedRequestContent = null)
//        {
//            var handler = new TestHttpHandler
//            {
//                OnSendingRequest = message =>
//                {                    
//                    Assert.AreEqual(expectedUri, message.RequestUri.OriginalString, "The Http Uri used to send the request is different than expected.");
//                    Assert.AreEqual(expectedMethod, message.Method, "The Http Method used to send the request is different than expected.");
                    
//                    if (expectedRequestContent != null)
//                    {
//                        Assert.AreEqual(expectedRequestContent, message.Content.ReadAsStringAsync().Result, "The Http request content is different than expected.");
//                    }

//                    return Task.FromResult(message);
//                }
//            };

//            if (responseContent != null)
//            {
//                handler.SetResponseContent(responseContent);
//            }

//            if (location != null)
//            {
//                handler.Response.Headers.Location = location;
//            }

//            if (httpStatusCode.HasValue)
//            {
//                handler.Response.StatusCode = httpStatusCode.Value;
//            }

//            return handler;
//        }
//    }
//}
