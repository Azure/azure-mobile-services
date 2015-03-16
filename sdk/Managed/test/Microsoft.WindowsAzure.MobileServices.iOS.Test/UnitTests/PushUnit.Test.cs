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
    
    public class PushUnit : TestBase
    {
        readonly string originalPushHandleDescription = "<f6e7cd2 80fc5b5 d488f8394baf216506bc1bba 864d5b483d>";
        readonly NSData originalNSData;
        const string DefaultServiceUri = "http://www.test.com";
        const string InstallationsPath = "/push/installations";
        readonly IPushTestUtility pushTestUtility;

        public PushUnit()
        {
            this.originalNSData = NSDataFromDescription(this.originalPushHandleDescription);
            this.pushTestUtility = TestPlatform.Instance.PushTestUtility;
        }

        [AsyncTestMethod]
        public async Task RegisterAsync_ChannelUri()
        {
            MobileServiceClient mobileClient = new MobileServiceClient(DefaultServiceUri);

            var expectedUri = string.Format("{0}{1}/{2}", DefaultServiceUri, InstallationsPath, mobileClient.GetPush().InstallationId);
            string installationRegistration = JsonConvert.SerializeObject(this.pushTestUtility.GetInstallation(mobileClient.GetPush().InstallationId, false));
            var hijack = TestHttpDelegatingHandler.CreateTestHttpHandler(expectedUri, HttpMethod.Put, null, HttpStatusCode.OK, expectedRequestContent: installationRegistration);

            mobileClient = new MobileServiceClient(DefaultServiceUri, hijack);
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
            mobileClient = new MobileServiceClient(DefaultServiceUri, hijack);
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
            string installationRegistration = JsonConvert.SerializeObject(this.pushTestUtility.GetInstallation(mobileClient.GetPush().InstallationId, true));
            var hijack = TestHttpDelegatingHandler.CreateTestHttpHandler(expectedUri, HttpMethod.Put, null, HttpStatusCode.OK, expectedRequestContent: installationRegistration);

            mobileClient = new MobileServiceClient(DefaultServiceUri, hijack);
            await mobileClient.GetPush().RegisterAsync(this.originalNSData, templates);
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
    }
}