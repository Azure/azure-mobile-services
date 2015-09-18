// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.TestFramework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    [Tag("unit")]
    [Tag("push")]
    public class PushUnit : TestBase
    {
        const string DefaultServiceUri = "http://www.test.com";
        const string InstallationsPath = "/push/installations";
        readonly IPushTestUtility pushTestUtility;
        readonly string registrationId;

        public PushUnit()
        {
            this.pushTestUtility = TestPlatform.Instance.PushTestUtility;
            this.registrationId = this.pushTestUtility.GetPushHandle();
        }

        [AsyncTestMethod]
        public async Task RegisterAsync_ChannelUri()
        {
            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri);

            var expectedUri = string.Format("{0}{1}/{2}", DefaultServiceUri, InstallationsPath, mobileClient.GetPush().InstallationId);
            string installationRegistration = JsonConvert.SerializeObject(this.pushTestUtility.GetInstallation(mobileClient.GetPush().InstallationId, false));
            var hijack = TestHttpDelegatingHandler.CreateTestHttpHandler(expectedUri, HttpMethod.Put, null, HttpStatusCode.OK, expectedRequestContent: installationRegistration);

            mobileClient = new MobileServiceClient(DefaultServiceUri, hijack);
            await mobileClient.GetPush().RegisterAsync(this.registrationId);
        }

        [AsyncTestMethod]
        public async Task RegisterAsync_ErrorEmptyChannelUri()
        {
            var mobileClient = new MobileServiceClient(DefaultServiceUri);
            string emptyRegistrationId = string.Empty;
            var exception = await AssertEx.Throws<ArgumentNullException>(() => mobileClient.GetPush().RegisterAsync(emptyRegistrationId));
            Assert.AreEqual(exception.Message, "Argument cannot be null.\nParameter name: registrationId");
        }

        [AsyncTestMethod]
        public async Task RegisterAsync_ErrorHttp()
        {
            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri);
            var expectedUri = string.Format("{0}{1}/{2}", DefaultServiceUri, InstallationsPath, mobileClient.GetPush().InstallationId);
            var hijack = TestHttpDelegatingHandler.CreateTestHttpHandler(expectedUri, HttpMethod.Put, null, HttpStatusCode.BadRequest);
            mobileClient = new MobileServiceClient(DefaultServiceUri, hijack);
            var exception = await AssertEx.Throws<MobileServiceInvalidOperationException>(() => mobileClient.GetPush().RegisterAsync(this.registrationId));
            Assert.AreEqual(exception.Response.StatusCode, HttpStatusCode.BadRequest);
        }

        [AsyncTestMethod]
        public async Task RegisterAsync_WithTemplates()
        {
            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri);

            var expectedUri = string.Format("{0}{1}/{2}", DefaultServiceUri, InstallationsPath, mobileClient.GetPush().InstallationId);
            JObject templates = this.pushTestUtility.GetTemplates();
            string installationRegistration = JsonConvert.SerializeObject(this.pushTestUtility.GetInstallation(mobileClient.GetPush().InstallationId, true));
            var hijack = TestHttpDelegatingHandler.CreateTestHttpHandler(expectedUri, HttpMethod.Put, null, HttpStatusCode.OK, expectedRequestContent: installationRegistration);

            mobileClient = new MobileServiceClient(DefaultServiceUri, hijack);
            await mobileClient.GetPush().RegisterAsync(this.registrationId, templates);
        }

        [AsyncTestMethod]
        public async Task RegisterAsync_ErrorWithTemplates()
        {
            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri);
            var expectedUri = string.Format("{0}{1}/{2}", DefaultServiceUri, InstallationsPath, mobileClient.GetPush().InstallationId);
            JObject templates = this.pushTestUtility.GetTemplates();
            var hijack = TestHttpDelegatingHandler.CreateTestHttpHandler(expectedUri, HttpMethod.Put, null, HttpStatusCode.BadRequest);
            mobileClient = new MobileServiceClient(DefaultServiceUri, hijack);
            var exception = await AssertEx.Throws<MobileServiceInvalidOperationException>(() => mobileClient.GetPush().RegisterAsync(this.registrationId, templates));
            Assert.AreEqual(exception.Response.StatusCode, HttpStatusCode.BadRequest);
        }
    }
}