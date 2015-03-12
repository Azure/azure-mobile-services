// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Test.UnitTests;
using Microsoft.WindowsAzure.MobileServices.TestFramework;
using Newtonsoft.Json;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    [Tag("unit")]
    [Tag("push")]
    [Tag("notNetFramework")]
    public class PushTest : TestBase
    {
        const string DefaultChannelUri = "http://channelUri.com/a b";
        const string DefaultServiceUri = MobileAppUriValidator.DummyMobileApp;
        const string RegistrationsPath = "push/registrations";
        const string InstallationsPath = "push/installations";
        const string DefaultRegistrationId = "7313155627197174428-6522078074300559092-1";

        readonly IPushUtility pushUtility;
        readonly IPushTestUtility pushTestUtility;
        readonly string platform;

        public PushTest()
        {
            this.pushTestUtility = TestPlatform.Instance.PushTestUtility;

            this.pushUtility = Platform.Instance.PushUtility;
            if (this.pushUtility != null)
            {
                this.platform = this.pushUtility.GetPlatform();
            }
        }

        [AsyncTestMethod]
        public async Task ListRegistrations_Empty()
        {
            // Ensure Uri and method are correct for request and specify body to return for empty registrations list
            var expectedUri = this.GetExpectedListUri();
            var hijack = TestHttpDelegatingHandler.CreateTestHttpHandler(expectedUri, HttpMethod.Get, "[]");

            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri, hijack);
            var pushHttpClient = new PushHttpClient(mobileClient);

            var registrations = await pushHttpClient.ListRegistrationsAsync(DefaultChannelUri);

            Assert.AreEqual(registrations.Count(), 0, "Expected empty list to return an empty list of registrations.");
        }

        [AsyncTestMethod]
        public async Task ListRegistrations_Native()
        {
            // Ensure Uri and method are correct for request and specify body to return for registrations list with only 1 native registration
            var expectedUri = this.GetExpectedListUri();
            var hijack = TestHttpDelegatingHandler.CreateTestHttpHandler(expectedUri, HttpMethod.Get, this.pushTestUtility.GetListNativeRegistrationResponse());

            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri, hijack);
            var pushHttpClient = new PushHttpClient(mobileClient);

            var registrations = await pushHttpClient.ListRegistrationsAsync(DefaultChannelUri);

            Assert.AreEqual(registrations.Count(), 1, "Expected 1 registration.");

            var firstRegistration = registrations.First();
            var nativeReg = this.pushUtility.GetNewNativeRegistration();
            Assert.AreEqual(nativeReg.GetType(), firstRegistration.GetType(), "The type of the registration returned from ListRegistrationsAsync is not of the correct type.");

            Assert.AreEqual(firstRegistration.RegistrationId, DefaultRegistrationId, "The registrationId returned from ListRegistrationsAsync is not correct.");

            var tags = firstRegistration.Tags.ToList();
            Assert.AreEqual(tags[0], "fooWns", "tag[0] on the registration is not correct.");
            Assert.AreEqual(tags[1], "barWns", "tag[1] on the registration is not correct.");
            Assert.AreEqual(tags[2], "4de2605e-fd09-4875-a897-c8c4c0a51682", "tag[2] on the registration is not correct.");

            Assert.AreEqual(firstRegistration.PushHandle, DefaultChannelUri, "The DeviceId on the registration is not correct.");
        }

        [AsyncTestMethod]
        public async Task ListRegistrations_Template()
        {
            var expectedUri = this.GetExpectedListUri();
            var hijack = TestHttpDelegatingHandler.CreateTestHttpHandler(expectedUri, HttpMethod.Get, this.pushTestUtility.GetListTemplateRegistrationResponse());

            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri, hijack);
            var pushHttpClient = new PushHttpClient(mobileClient);

            var registrations = await pushHttpClient.ListRegistrationsAsync(DefaultChannelUri);

            Assert.AreEqual(registrations.Count(), 1, "Expected 1 registration.");

            var firstRegistration = registrations.First();
            var templateReg = this.pushUtility.GetNewTemplateRegistration();
            Assert.AreEqual(templateReg.GetType(), firstRegistration.GetType(), "The type of the registration returned from ListRegistrationsAsync is not of the correct type.");

            Assert.AreEqual(firstRegistration.RegistrationId, DefaultRegistrationId, "The registrationId returned from ListRegistrationsAsync is not correct.");

            var tags = firstRegistration.Tags.ToList();
            Assert.AreEqual(tags[0], "fooWns", "tag[0] on the registration is not correct.");
            Assert.AreEqual(tags[1], "barWns", "tag[1] on the registration is not correct.");
            Assert.AreEqual(tags[2], "4de2605e-fd09-4875-a897-c8c4c0a51682", "tag[2] on the registration is not correct.");

            Assert.AreEqual(firstRegistration.PushHandle, DefaultChannelUri, "The DeviceId on the registration is not correct.");
        }

        string GetExpectedListUri()
        {
            var channelUri = Uri.EscapeUriString(DefaultChannelUri);
            return string.Format("{0}{1}?deviceId={2}&platform={3}", DefaultServiceUri, RegistrationsPath, channelUri, Uri.EscapeUriString(this.platform));
        }

        [AsyncTestMethod]
        public async Task ListRegistrations_NativeAndTemplate()
        {
            var expectedUri = this.GetExpectedListUri();
            var hijack = TestHttpDelegatingHandler.CreateTestHttpHandler(expectedUri, HttpMethod.Get, this.pushTestUtility.GetListMixedRegistrationResponse());

            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri, hijack);
            var pushHttpClient = new PushHttpClient(mobileClient);

            var registrations = await pushHttpClient.ListRegistrationsAsync(DefaultChannelUri);
            var registrationsArray = registrations.ToArray();
            Assert.AreEqual(registrationsArray.Length, 2, "Expected 2 registrations.");

            var firstRegistration = registrationsArray[0];
            var nativeReg = this.pushUtility.GetNewNativeRegistration();
            Assert.AreEqual(nativeReg.GetType(), firstRegistration.GetType(), "The type of the native registration returned from ListRegistrationsAsync is not of the correct type.");

            Assert.AreEqual(firstRegistration.RegistrationId, DefaultRegistrationId, "The native registrationId returned from ListRegistrationsAsync is not correct.");
            var nativeTags = firstRegistration.Tags.ToList();
            Assert.AreEqual(nativeTags[0], "fooWns", "nativeTags[0] on the registration is not correct.");
            Assert.AreEqual(nativeTags[1], "barWns", "nativeTags[1] on the registration is not correct.");
            Assert.AreEqual(nativeTags[2], "4de2605e-fd09-4875-a897-c8c4c0a51682", "nativeTags[2] on the registration is not correct.");

            Assert.AreEqual(firstRegistration.PushHandle, DefaultChannelUri, "The DeviceId on the native registration is not correct.");

            var secondRegistration = registrationsArray[1];
            var templateReg = this.pushUtility.GetNewTemplateRegistration();
            Assert.AreEqual(templateReg.GetType(), secondRegistration.GetType(), "The type of the template registration returned from ListRegistrationsAsync is not of the correct type.");

            Assert.AreEqual(secondRegistration.RegistrationId, DefaultRegistrationId, "The template registrationId returned from ListRegistrationsAsync is not correct.");
            var templateTags = secondRegistration.Tags.ToList();
            Assert.AreEqual(templateTags[0], "fooWns", "templateTags[0] on the registration is not correct.");
            Assert.AreEqual(templateTags[1], "barWns", "templateTags[1] on the registration is not correct.");
            Assert.AreEqual(templateTags[2], "4de2605e-fd09-4875-a897-c8c4c0a51682", "templateTags[2] on the registration is not correct.");

            Assert.AreEqual(secondRegistration.PushHandle, DefaultChannelUri, "The DeviceId on the template registration is not correct.");
        }

        [AsyncTestMethod]
        public async Task ListRegistrations_Error_WithStringBody()
        {
            var expectedUri = this.GetExpectedListUri();
            var hijack = TestHttpDelegatingHandler.CreateTestHttpHandler(expectedUri, HttpMethod.Get, "\"Server threw 500\"", HttpStatusCode.InternalServerError);

            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri, hijack);
            var pushHttpClient = new PushHttpClient(mobileClient);

            var exception = await AssertEx.Throws<MobileServiceInvalidOperationException>(
                () => pushHttpClient.ListRegistrationsAsync(DefaultChannelUri));
            Assert.AreEqual(exception.Message, "Server threw 500");
        }

        [AsyncTestMethod]
        public async Task ListRegistrations_Error_WithError()
        {
            var expectedUri = this.GetExpectedListUri();
            var hijack = TestHttpDelegatingHandler.CreateTestHttpHandler(expectedUri, HttpMethod.Get, "{\"error\":\"Server threw 500\"}", HttpStatusCode.InternalServerError);

            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri, hijack);
            var pushHttpClient = new PushHttpClient(mobileClient);

            var exception = await AssertEx.Throws<MobileServiceInvalidOperationException>
                (() => pushHttpClient.ListRegistrationsAsync(DefaultChannelUri));
            Assert.AreEqual(exception.Message, "Server threw 500");
        }

        const string CreatePath = "push/registrationids";
        static readonly Uri LocationUri = new Uri(string.Format("{0}{1}/{2}", DefaultServiceUri, RegistrationsPath, DefaultRegistrationId));

        [AsyncTestMethod]
        public async Task CreateRegistrationId()
        {
            var expectedUri = string.Format("{0}{1}", DefaultServiceUri, CreatePath);
            var hijack = TestHttpDelegatingHandler.CreateTestHttpHandler(expectedUri, HttpMethod.Post, null, HttpStatusCode.Created, LocationUri);

            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri, hijack);
            var pushHttpClient = new PushHttpClient(mobileClient);

            var registrationId = await pushHttpClient.CreateRegistrationIdAsync();

            Assert.AreEqual(registrationId, DefaultRegistrationId, "Expected CreateRegistrationIdAsync to return correct RegistrationId.");
        }

        [AsyncTestMethod]
        public async Task CreateRegistrationId_Error()
        {
            var expectedUri = string.Format("{0}{1}", DefaultServiceUri, CreatePath);
            var hijack = TestHttpDelegatingHandler.CreateTestHttpHandler(expectedUri, HttpMethod.Post, "\"Server threw 500\"", HttpStatusCode.InternalServerError, LocationUri);

            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri, hijack);
            var pushHttpClient = new PushHttpClient(mobileClient);

            var exception = await AssertEx.Throws<MobileServiceInvalidOperationException>(() => pushHttpClient.CreateRegistrationIdAsync());
            Assert.AreEqual(exception.Message, "Server threw 500");
        }

        [AsyncTestMethod]
        public async Task DeleteRegistration()
        {
            var expectedUri = string.Format("{0}{1}/{2}", DefaultServiceUri, RegistrationsPath, DefaultRegistrationId);
            var hijack = TestHttpDelegatingHandler.CreateTestHttpHandler(expectedUri, HttpMethod.Delete, null, HttpStatusCode.OK);

            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri, hijack);
            var pushHttpClient = new PushHttpClient(mobileClient);

            // Because Unregistrer returns nothing, the only test we can perform is that the Http Method and body are correct
            // and that UnregisterAsync does not throw if Ok is returned
            await pushHttpClient.UnregisterAsync(DefaultRegistrationId);
        }

        [AsyncTestMethod]
        public async Task DeleteRegistration_Error()
        {
            var expectedUri = string.Format("{0}{1}/{2}", DefaultServiceUri, RegistrationsPath, DefaultRegistrationId);
            var hijack = TestHttpDelegatingHandler.CreateTestHttpHandler(expectedUri, HttpMethod.Delete, "\"Server threw 500\"", HttpStatusCode.InternalServerError);

            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri, hijack);
            var pushHttpClient = new PushHttpClient(mobileClient);

            var exception = await AssertEx.Throws<MobileServiceInvalidOperationException>(
                () => pushHttpClient.UnregisterAsync(DefaultRegistrationId));
            Assert.AreEqual(exception.Message, "Server threw 500");
        }

        [AsyncTestMethod]
        public async Task CreateOrUpdateRegistration()
        {
            var expectedUri = string.Format("{0}{1}/{2}", DefaultServiceUri, RegistrationsPath, DefaultRegistrationId);
            var registration = this.pushTestUtility.GetNewNativeRegistration(DefaultChannelUri, new[] { "foo", "bar" });
            registration.RegistrationId = DefaultRegistrationId;
            string jsonRegistration = JsonConvert.SerializeObject(registration);

            // The entire test is performed within the TestHttpHandler.
            // We verify that the request content is correct and that the method is correct.
            var hijack = TestHttpDelegatingHandler.CreateTestHttpHandler(expectedUri, HttpMethod.Put, null, HttpStatusCode.NoContent, expectedRequestContent: jsonRegistration);

            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri, hijack);
            var pushHttpClient = new PushHttpClient(mobileClient);

            await pushHttpClient.CreateOrUpdateRegistrationAsync(registration);
        }

        [AsyncTestMethod]
        public async Task CreateOrUpdateRegistration_Error()
        {
            var expectedUri = string.Format("{0}{1}/{2}", DefaultServiceUri, RegistrationsPath, DefaultRegistrationId);
            var registration = this.pushTestUtility.GetNewNativeRegistration(DefaultChannelUri, new[] { "foo", "bar" });
            registration.RegistrationId = DefaultRegistrationId;

            string jsonRegistration = JsonConvert.SerializeObject(registration);
            var hijack = TestHttpDelegatingHandler.CreateTestHttpHandler(expectedUri, HttpMethod.Put, "\"Server threw 500\"", HttpStatusCode.InternalServerError, expectedRequestContent: jsonRegistration);

            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri, hijack);
            var pushHttpClient = new PushHttpClient(mobileClient);

            var exception = await AssertEx.Throws<MobileServiceInvalidOperationException>(() => pushHttpClient.CreateOrUpdateRegistrationAsync(registration));
            Assert.AreEqual(exception.Message, "Server threw 500");
        }

        [AsyncTestMethod]
        public async Task DeleteInstallationAsync()
        {
            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri);
            var expectedUri = string.Format("{0}{1}/{2}", DefaultServiceUri, InstallationsPath, mobileClient.applicationInstallationId);
            var hijack = TestHttpDelegatingHandler.CreateTestHttpHandler(expectedUri, HttpMethod.Delete, null, HttpStatusCode.NoContent);

            mobileClient = new MobileServiceClient(DefaultServiceUri, hijack);
            var pushHttpClient = new PushHttpClient(mobileClient);

            await pushHttpClient.DeleteInstallationAsync();
        }

        [AsyncTestMethod]
        public async Task DeleteInstallationAsync_Error()
        {
            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri);
            var expectedUri = string.Format("{0}{1}/{2}", DefaultServiceUri, InstallationsPath, mobileClient.applicationInstallationId);
            var hijack = TestHttpDelegatingHandler.CreateTestHttpHandler(expectedUri, HttpMethod.Delete, null, HttpStatusCode.BadRequest);
            mobileClient = new MobileServiceClient(DefaultServiceUri, hijack);
            var pushHttpClient = new PushHttpClient(mobileClient);
            var exception = await AssertEx.Throws<MobileServiceInvalidOperationException>(
          () => pushHttpClient.DeleteInstallationAsync());
            Assert.AreEqual(exception.Response.StatusCode, HttpStatusCode.BadRequest);
        }
    }
}