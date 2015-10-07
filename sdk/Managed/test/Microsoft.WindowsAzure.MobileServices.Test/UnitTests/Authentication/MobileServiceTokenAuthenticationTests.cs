﻿// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.TestFramework;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Test.UnitTests
{
    [Tag("unit")]
    [Tag("auth")]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public class MobileServiceTokenAuthenticationTests : TestBase
    {
        private TestHttpHandler hijack;
        private MobileServiceClient client;

        private void TestInitialize()
        {
            string appUrl = MobileAppUriValidator.DummyMobileApp;
            this.hijack = new TestHttpHandler();
            this.hijack.SetResponseContent(String.Empty);

            var originalFactory = MobileServiceHttpClient.DefaultHandlerFactory;
            MobileServiceHttpClient.DefaultHandlerFactory = () => this.hijack;

            this.client = new MobileServiceClient(appUrl, hijack);

            MobileServiceHttpClient.DefaultHandlerFactory = originalFactory;
        }

        [TestMethod]
        public void StartUri_IncludesParameters()
        {
            TestStartUriForParameters(new Dictionary<string, string>()
            {
                { "display", "popup" },
                { "scope", "email,birthday" }
            }, MobileAppUriValidator.DummyMobileApp+"login/microsoftaccount?display=popup&scope=email%2Cbirthday");
        }

        [TestMethod]
        public void StartUri_WithNullParameters()
        {
            TestStartUriForParameters(null, MobileAppUriValidator.DummyMobileApp + "login/microsoftaccount");
        }

        [TestMethod]
        public void StartUri_WithEmptyParameters()
        {
            TestStartUriForParameters(new Dictionary<string, string>(), MobileAppUriValidator.DummyMobileApp + "login/microsoftaccount");
        }

        private void TestStartUriForParameters(Dictionary<string, string> parameters, string uri)
        {
            TestInitialize();
            var auth = new MobileServiceTokenAuthentication(this.client, "MicrosoftAccount", new JObject(), parameters);
            Assert.AreEqual(auth.StartUri.OriginalString, uri);
        }

        [AsyncTestMethod]
        public Task LoginAsync_IncludesTheParameters()
        {
            return TestLoginAsyncForParameters(new Dictionary<string, string>()
            {
                { "display", "popup" },
                { "scope", "email,birthday" }
            }, MobileAppUriValidator.DummyMobileApp + "login/microsoftaccount?display=popup&scope=email%2Cbirthday");
        }

        [AsyncTestMethod]
        public Task LoginAsync_WithNullParameterss()
        {
            return TestLoginAsyncForParameters(null, MobileAppUriValidator.DummyMobileApp + "login/microsoftaccount");
        }

        [AsyncTestMethod]
        public Task LoginAsync_WithEmptyParameterss()
        {
            return TestLoginAsyncForParameters(new Dictionary<string, string>(), MobileAppUriValidator.DummyMobileApp + "login/microsoftaccount");
        }

        private async Task TestLoginAsyncForParameters(Dictionary<string, string> parameters, string uri)
        {
            TestInitialize();
            var auth = new MobileServiceTokenAuthentication(this.client, "MicrosoftAccount", new JObject(), parameters);
            await auth.LoginAsync();
            Assert.AreEqual(this.hijack.Request.RequestUri.OriginalString, uri);
        }
    }
}
