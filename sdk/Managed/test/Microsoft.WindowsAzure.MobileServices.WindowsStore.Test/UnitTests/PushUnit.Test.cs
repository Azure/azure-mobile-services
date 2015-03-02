// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Test.UnitTests;
using Microsoft.WindowsAzure.MobileServices.TestFramework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    [Tag("push")]
    public class PushUnit : TestBase
    {
        readonly IPushTestUtility pushTestUtility;
        const string DefaultChannelUri =
            "https://bn1.notify.windows.com/?token=AgYAAADs42685sa5PFCEy82eYpuG8WCPB098AWHnwR8kNRQLwUwf%2f9p%2fy0r82m4hxrLSQ%2bfl5aNlSk99E4jrhEatfsWgyutFzqQxHcLk0Xun3mufO2G%2fb2b%2ftjQjCjVcBESjWvY%3d";
        const string DefaultServiceUri = MobileAppUriValidator.DummyMobileApp;
        const string InstallationsPath = "push/installations";

        public PushUnit()
        {
            this.pushTestUtility = TestPlatform.Instance.PushTestUtility;
        }

        [TestMethod]
        public void InvalidBodyTemplateIfNotXml()
        {
            try
            {
                var registration = new WnsTemplateRegistration("uri", "junkBodyTemplate", "testName");
                Assert.Fail("Expected templateBody that is not XML to throw ArgumentException");
            }
            catch
            {
                // PASSES
            }
        }

        [TestMethod]
        public void InvalidBodyTemplateIfImproperXml()
        {
            try
            {
                var registration = new WnsTemplateRegistration(
                    "uri",
                    "<foo><visual><binding template=\"ToastText01\"><text id=\"1\">$(message)</text></binding></visual></foo>",
                    "testName");
                Assert.Fail("Expected templateBody with unexpected first XML node to throw ArgumentException");
            }
            catch
            {
                // PASSES
            }
        }

        [AsyncTestMethod]
        public async Task RegisterAsync_ChannelUri()
        {
            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri);

            var expectedUri = string.Format("{0}{1}/{2}", DefaultServiceUri, InstallationsPath, mobileClient.GetPush().InstallationId);
            string installationRegistration = JsonConvert.SerializeObject(this.pushTestUtility.GetInstallation(mobileClient.GetPush().InstallationId, false));
            var hijack = TestHttpDelegatingHandler.CreateTestHttpHandler(expectedUri, HttpMethod.Put, null, HttpStatusCode.OK, expectedRequestContent: installationRegistration);

            mobileClient = new MobileServiceClient(DefaultServiceUri, hijack);
            await mobileClient.GetPush().RegisterAsync(DefaultChannelUri);
        }

        [AsyncTestMethod]
        public async Task RegisterAsync_ErrorEmptyChannelUri()
        {
            var mobileClient = new MobileServiceClient(DefaultServiceUri);
            var exception = await AssertEx.Throws<ArgumentNullException>(
           () => mobileClient.GetPush().RegisterAsync(""));
            Assert.AreEqual(exception.Message, "Value cannot be null.\r\nParameter name: channelUri");
        }

        [AsyncTestMethod]
        public async Task RegisterAsync_ErrorHttp()
        {
            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri);
            var expectedUri = string.Format("{0}{1}/{2}", DefaultServiceUri, InstallationsPath, mobileClient.GetPush().InstallationId);
            var hijack = TestHttpDelegatingHandler.CreateTestHttpHandler(expectedUri, HttpMethod.Put, null, HttpStatusCode.BadRequest);
            mobileClient = new MobileServiceClient(DefaultServiceUri, hijack);
            var exception = await AssertEx.Throws<MobileServiceInvalidOperationException>(
          () => mobileClient.GetPush().RegisterAsync(DefaultChannelUri));
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
            await mobileClient.GetPush().RegisterAsync(DefaultChannelUri, templates);
        }

        [AsyncTestMethod]
        public async Task RegisterAsync_ErrorWithTemplates()
        {
            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri);
            var expectedUri = string.Format("{0}{1}/{2}", DefaultServiceUri, InstallationsPath, mobileClient.GetPush().InstallationId);
            JObject templates = this.pushTestUtility.GetTemplates();
            var hijack = TestHttpDelegatingHandler.CreateTestHttpHandler(expectedUri, HttpMethod.Put, null, HttpStatusCode.BadRequest);
            mobileClient = new MobileServiceClient(DefaultServiceUri, hijack);
            var exception = await AssertEx.Throws<MobileServiceInvalidOperationException>(
          () => mobileClient.GetPush().RegisterAsync(DefaultChannelUri, templates));
            Assert.AreEqual(exception.Response.StatusCode, HttpStatusCode.BadRequest);
        }
    }
}