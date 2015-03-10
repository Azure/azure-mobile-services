// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Foundation;
using Microsoft.WindowsAzure.MobileServices.TestFramework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    [Tag("unit")]
    [Tag("push")]
    public class PushPlatformTest : TestBase
    {
        readonly string originalPushHandleDescription = "<f6e7cd2 80fc5b5 d488f8394baf216506bc1bba 864d5b483d>";
        readonly NSData originalNSData;
        readonly string originalNSDataTrimmed;
        const string DefaultServiceUri = "http://www.test.com";
        const string InstallationsPath = "/push/installations";
        readonly IPushTestUtility pushTestUtility;

        public PushPlatformTest()
        {
            this.originalNSData = NSDataFromDescription(this.originalPushHandleDescription);
            this.originalNSDataTrimmed = TrimDeviceToken(this.originalPushHandleDescription);
            this.pushTestUtility = TestPlatform.Instance.PushTestUtility;
        }

        [TestMethod]
        public void UnregisterAllAsync()
        {
            var registrationManager = new RegistrationManagerForTest();
            var push = new Push(registrationManager);

            push.UnregisterAllAsync(this.originalNSData).Wait();
            registrationManager.VerifyPushHandle(this.originalNSDataTrimmed);
        }

        [TestMethod]
        public void RegisterNativeAsync()
        {
            var registrationManager = new RegistrationManagerForTest();
            var push = new Push(registrationManager);

            push.RegisterNativeAsync(this.originalNSData).Wait();
            registrationManager.VerifyPushHandle(this.originalNSDataTrimmed);
        }

        [TestMethod]
        public void RegisterNativeAsyncWithTags()
        {
            var registrationManager = new RegistrationManagerForTest();
            var push = new Push(registrationManager);

            push.RegisterNativeAsync(this.originalNSData, new List<string> { "foo" }).Wait();
            registrationManager.VerifyPushHandle(this.originalNSDataTrimmed);
        }

        [TestMethod]
        public void RegisterTemplateAsyncWithTags()
        {
            var registrationManager = new RegistrationManagerForTest();
            var push = new Push(registrationManager);

            push.RegisterTemplateAsync(this.originalNSData, "jsonBody", "expiry", "templateName", new List<string> { "foo" }).Wait();
            registrationManager.VerifyPushHandle(this.originalNSDataTrimmed);
        }

        [TestMethod]
        public void RegisterTemplateAsync()
        {
            var registrationManager = new RegistrationManagerForTest();
            var push = new Push(registrationManager);

            push.RegisterTemplateAsync(this.originalNSData, "jsonBody", "expiry", "templateName").Wait();
            registrationManager.VerifyPushHandle(this.originalNSDataTrimmed);
        }

        [TestMethod]
        public void ListRegistrationsAsync()
        {
            var registrationManager = new RegistrationManagerForTest();
            var push = new Push(registrationManager);

            push.ListRegistrationsAsync(this.originalNSData).Wait();
            registrationManager.VerifyPushHandle(this.originalNSDataTrimmed);
        }

        [AsyncTestMethod]
        public async Task RegisterAsync_ChannelUri()
        {
            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri);

            var expectedUri = string.Format("{0}{1}/{2}", DefaultServiceUri, InstallationsPath, mobileClient.GetPush().InstallationId);
            string installationRegistration = JsonConvert.SerializeObject(this.pushTestUtility.GetInstallation(mobileClient.GetPush().InstallationId, false, ApnsRegistration.ParseDeviceToken(this.originalNSData)));
            var hijack = TestHttpDelegatingHandler.CreateTestHttpHandler(expectedUri, HttpMethod.Put, null, HttpStatusCode.OK, expectedRequestContent: installationRegistration);

            mobileClient = new MobileServiceClient(DefaultServiceUri, null, hijack);
            await mobileClient.GetPush().RegisterAsync(this.originalNSData);
        }

        [AsyncTestMethod]
        public async Task RegisterAsync_ErrorEmptyChannelUri()
        {
            var mobileClient = new MobileServiceClient(DefaultServiceUri);
            NSData deviceToken = null;
            var exception = await AssertEx.Throws<ArgumentNullException>(
           () => mobileClient.GetPush().RegisterAsync(deviceToken));
            Assert.AreEqual(exception.Message, "Argument cannot be null.\nParameter name: deviceToken");
        }

        [AsyncTestMethod]
        public async Task RegisterAsync_ErrorHttp()
        {
            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri);
            var expectedUri = string.Format("{0}{1}/{2}", DefaultServiceUri, InstallationsPath, mobileClient.GetPush().InstallationId);
            var hijack = TestHttpDelegatingHandler.CreateTestHttpHandler(expectedUri, HttpMethod.Put, null, HttpStatusCode.BadRequest);
            mobileClient = new MobileServiceClient(DefaultServiceUri, null, hijack);
            var exception = await AssertEx.Throws<MobileServiceInvalidOperationException>(
          () => mobileClient.GetPush().RegisterAsync(this.originalNSData));
            Assert.AreEqual(exception.Response.StatusCode, HttpStatusCode.BadRequest);
        }

        [AsyncTestMethod]
        public async Task RegisterAsync_WithTemplates()
        {
            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri);

            var expectedUri = string.Format("{0}{1}/{2}", DefaultServiceUri, InstallationsPath, mobileClient.GetPush().InstallationId);
            JObject templates = this.pushTestUtility.GetTemplates();
            string installationRegistration = JsonConvert.SerializeObject(this.pushTestUtility.GetInstallation(mobileClient.GetPush().InstallationId, true, ApnsRegistration.ParseDeviceToken(this.originalNSData)));
            var hijack = TestHttpDelegatingHandler.CreateTestHttpHandler(expectedUri, HttpMethod.Put, null, HttpStatusCode.OK, expectedRequestContent: installationRegistration);

            mobileClient = new MobileServiceClient(DefaultServiceUri, null, hijack);
            await mobileClient.GetPush().RegisterAsync(this.originalNSData, templates);
        }

        [AsyncTestMethod]
        public async Task RegisterAsync_ErrorWithTemplates()
        {
            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri);
            var expectedUri = string.Format("{0}{1}/{2}", DefaultServiceUri, InstallationsPath, mobileClient.GetPush().InstallationId);
            JObject templates = this.pushTestUtility.GetTemplates();
            var hijack = TestHttpDelegatingHandler.CreateTestHttpHandler(expectedUri, HttpMethod.Put, null, HttpStatusCode.BadRequest);
            mobileClient = new MobileServiceClient(DefaultServiceUri, null, hijack);
            var exception = await AssertEx.Throws<MobileServiceInvalidOperationException>(
          () => mobileClient.GetPush().RegisterAsync(this.originalNSData, templates));
            Assert.AreEqual(exception.Response.StatusCode, HttpStatusCode.BadRequest);
        }

        NSData NSDataFromDescription(string hexString)
        {
            hexString = hexString.Trim('<', '>').Replace(" ", string.Empty);
            NSMutableData data = new NSMutableData();
            byte[] hexAsBytes = new byte[hexString.Length / 2];
            for (int index = 0; index < hexAsBytes.Length; index++)
            {
                string byteValue = hexString.Substring(index * 2, 2);
                hexAsBytes[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            data.AppendBytes(hexAsBytes);
            return data;
        }

        static string TrimDeviceToken(string deviceTokenDescription)
        {
            return deviceTokenDescription.Trim('<', '>').Replace(" ", string.Empty).ToUpperInvariant();
        }

        private class RegistrationManagerForTest : IRegistrationManager
        {
            private string lastDeviceId;

            public ILocalStorageManager LocalStorageManager
            {
                get { throw new System.NotImplementedException(); }
            }

            public Task DeleteRegistrationsForChannelAsync(string deviceId)
            {
                this.lastDeviceId = deviceId;
                return Task.FromResult(0);
            }

            public Task<List<Registration>> ListRegistrationsAsync(string deviceId)
            {
                this.lastDeviceId = deviceId;
                return Task.FromResult(new List<Registration> { new Registration(deviceId, null) });
            }

            public Task RegisterAsync(Registration registration)
            {
                this.lastDeviceId = registration.PushHandle;
                return Task.FromResult(0);
            }

            public Task UnregisterAsync(string registrationName)
            {
                return Task.FromResult(0);
            }

            public void VerifyPushHandle(string expectedPushHandle)
            {
                Assert.AreEqual(expectedPushHandle, this.lastDeviceId, "Expected deviceId passed to RegistrationManager is not accurate.");
            }
        }
    }
}