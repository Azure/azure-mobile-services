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

        string GetExpectedListUri()
        {
            var channelUri = Uri.EscapeUriString(DefaultChannelUri);
            return string.Format("{0}{1}?deviceId={2}&platform={3}", DefaultServiceUri, RegistrationsPath, channelUri, Uri.EscapeUriString(this.platform));
        }

        [AsyncTestMethod]
        public async Task DeleteInstallationAsync()
        {
            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri);
            var expectedUri = string.Format("{0}{1}/{2}", DefaultServiceUri, InstallationsPath, mobileClient.InstallationId);
            var hijack = TestHttpDelegatingHandler.CreateTestHttpHandler(expectedUri, HttpMethod.Delete, null, HttpStatusCode.NoContent);

            mobileClient = new MobileServiceClient(DefaultServiceUri, hijack);
            var pushHttpClient = new PushHttpClient(mobileClient);

            await pushHttpClient.DeleteInstallationAsync();
        }

        [AsyncTestMethod]
        public async Task DeleteInstallationAsync_Error()
        {
            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri);
            var expectedUri = string.Format("{0}{1}/{2}", DefaultServiceUri, InstallationsPath, mobileClient.InstallationId);
            var hijack = TestHttpDelegatingHandler.CreateTestHttpHandler(expectedUri, HttpMethod.Delete, null, HttpStatusCode.BadRequest);
            mobileClient = new MobileServiceClient(DefaultServiceUri, hijack);
            var pushHttpClient = new PushHttpClient(mobileClient);
            var exception = await AssertEx.Throws<MobileServiceInvalidOperationException>(
          () => pushHttpClient.DeleteInstallationAsync());
            Assert.AreEqual(exception.Response.StatusCode, HttpStatusCode.BadRequest);
        }
    }
}