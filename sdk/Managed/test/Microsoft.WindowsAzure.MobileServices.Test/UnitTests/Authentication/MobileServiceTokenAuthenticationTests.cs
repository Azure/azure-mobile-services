// ----------------------------------------------------------------------------
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

        string loginAsyncUriFragment = ".auth/login";
        string legacyLoginAsyncUriFragment = "login";
        string validAlternateLoginUrl = "http://www.testalternatelogin.com/";
        string validAlternateLoginUrlWithoutTrailingSlash = "http://www.testalternatelogin.com";


        private void TestInitialize(string appUrl = null, bool legacyAuth = false, string alternateLoginUri = null)
        {
            if (string.IsNullOrEmpty(appUrl))
            {
                appUrl = MobileAppUriValidator.DummyMobileApp + "?n=John&n=Susan";
            }
            this.hijack = new TestHttpHandler();
            this.hijack.SetResponseContent(String.Empty);

            var originalFactory = MobileServiceHttpClient.DefaultHandlerFactory;
            MobileServiceHttpClient.DefaultHandlerFactory = () => this.hijack;
            this.client = new MobileServiceClient(appUrl, hijack);
            if (legacyAuth)
            {
                this.client.LoginUriPrefix = legacyLoginAsyncUriFragment;
            }
            if (!string.IsNullOrEmpty(alternateLoginUri))
            {
                this.client.AlternateLoginUri = alternateLoginUri;
            }
            MobileServiceHttpClient.DefaultHandlerFactory = originalFactory;
        }

        [TestMethod]
        public void StartUri_IncludesParameters()
        {
            TestStartUriForParameters(new Dictionary<string, string>()
            {
                { "display", "popup" },
                { "scope", "email,birthday" }
            }, MobileAppUriValidator.DummyMobileApp + loginAsyncUriFragment + "/microsoftaccount?display=popup&scope=email%2Cbirthday");
        }

        [TestMethod]
        public void StartUri_Leagcy_IncludesParameters()
        {
            TestStartUriForParameters(new Dictionary<string, string>()
            {
                { "display", "popup" },
                { "scope", "email,birthday" }
            }, MobileAppUriValidator.DummyMobileApp + legacyLoginAsyncUriFragment + "/microsoftaccount?display=popup&scope=email%2Cbirthday", true);
        }


        [TestMethod]
        public void StartUri_AlternateLoginUri_IncludesParameters()
        {
            TestStartUriForParameters(new Dictionary<string, string>()
            {
                { "display", "popup" },
                { "scope", "email,birthday" }
            }, validAlternateLoginUrl + loginAsyncUriFragment + "/microsoftaccount?display=popup&scope=email%2Cbirthday", false, validAlternateLoginUrl);
        }

        [TestMethod]
        public void StartUri_Legacy_AlternateLoginUri_IncludesParameters()
        {
            TestStartUriForParameters(new Dictionary<string, string>()
            {
                { "display", "popup" },
                { "scope", "email,birthday" }
            }, validAlternateLoginUrl + legacyLoginAsyncUriFragment + "/microsoftaccount?display=popup&scope=email%2Cbirthday", true, validAlternateLoginUrl);
        }

        [TestMethod]
        public void StartUri_MobileAppWithFolder_IncludesParameters()
        {
            TestStartUriForParameters(new Dictionary<string, string>()
            {
                { "display", "popup" },
                { "scope", "email,birthday" }
            }, MobileAppUriValidator.DummyMobileApp + loginAsyncUriFragment + "/microsoftaccount?display=popup&scope=email%2Cbirthday", false, null, MobileAppUriValidator.DummyMobileAppUriWithFolder);
        }

        [TestMethod]
        public void StartUri_Legacy_MobileAppUriWithFolder_IncludesParameters()
        {
            TestStartUriForParameters(new Dictionary<string, string>()
            {
                { "display", "popup" },
                { "scope", "email,birthday" }
            }, MobileAppUriValidator.DummyMobileApp + legacyLoginAsyncUriFragment + "/microsoftaccount?display=popup&scope=email%2Cbirthday", true);
        }

        [TestMethod]
        public void StartUri_WithNullParameters()
        {
            TestStartUriForParameters(null, MobileAppUriValidator.DummyMobileApp + loginAsyncUriFragment + "/microsoftaccount");
        }

        [TestMethod]
        public void StartUri_Legacy_WithNullParameters()
        {
            TestStartUriForParameters(null, MobileAppUriValidator.DummyMobileApp + legacyLoginAsyncUriFragment + "/microsoftaccount", true);
        }

        [TestMethod]
        public void StartUri_MobileAppUriWihtoutTrailingSlash_WithNullParameters()
        {
            TestStartUriForParameters(null, MobileAppUriValidator.DummyMobileAppWithoutTralingSlash + "/" + legacyLoginAsyncUriFragment + "/microsoftaccount", true, null, MobileAppUriValidator.DummyMobileAppWithoutTralingSlash);
        }

        [TestMethod]
        public void StartUri_AlternateLoginUri_WithNullParameters()
        {
            TestStartUriForParameters(null, validAlternateLoginUrl + loginAsyncUriFragment + "/microsoftaccount", false, validAlternateLoginUrl);
        }

        [TestMethod]
        public void StartUri_Legacy_AlternateLoginUri_WithNullParameters()
        {
            TestStartUriForParameters(null, validAlternateLoginUrl + legacyLoginAsyncUriFragment + "/microsoftaccount", true, validAlternateLoginUrl);
        }

        [TestMethod]
        public void StartUri_WithEmptyParameters()
        {
            TestStartUriForParameters(new Dictionary<string, string>(), MobileAppUriValidator.DummyMobileApp + loginAsyncUriFragment + "/microsoftaccount");
        }

        [TestMethod]
        public void StartUri_Legacy_WithEmptyParameters()
        {
            TestStartUriForParameters(new Dictionary<string, string>(), MobileAppUriValidator.DummyMobileApp + legacyLoginAsyncUriFragment + "/microsoftaccount", true);
        }

        [TestMethod]
        public void StartUri_AlternateLoginUrl_WithEmptyParameters()
        {
            TestStartUriForParameters(new Dictionary<string, string>(), validAlternateLoginUrlWithoutTrailingSlash + "/" + loginAsyncUriFragment + "/microsoftaccount", false, validAlternateLoginUrlWithoutTrailingSlash);
        }

        [TestMethod]
        public void StartUri_Legacy_AlternateLoginUrl_WithEmptyParameters()
        {
            TestStartUriForParameters(new Dictionary<string, string>(), validAlternateLoginUrl + legacyLoginAsyncUriFragment + "/microsoftaccount", true, validAlternateLoginUrl);
        }

        [TestMethod]
        public void StartUri_Legacy_MobileAppUriWithFolder_WithEmptyParameters()
        {
            TestStartUriForParameters(new Dictionary<string, string>(), MobileAppUriValidator.DummyMobileApp + legacyLoginAsyncUriFragment + "/microsoftaccount", true, null, MobileAppUriValidator.DummyMobileAppUriWithFolder);
        }


        [TestMethod]
        public void StartUri_InvalidAlternateLoginUri_IncludesParameters()
        {
            var client = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            client.AlternateLoginUri = MobileAppUriValidator.DummyMobileApp + "?n=John&n=Susan";
            AssertEx.Throws<ArgumentException>(() => client.AlternateLoginUri = MobileAppUriValidator.DummyMobileAppUriWithFolder);
            AssertEx.Throws<ArgumentException>(() => client.AlternateLoginUri = MobileAppUriValidator.DummyMobileAppUriWithFolderWithoutTralingSlash);
        }

        private void TestStartUriForParameters(Dictionary<string, string> parameters, string uri, bool legacyAuth = false, string alternateLoginUri = null, string appUrl = null)
        {
            TestInitialize(appUrl, legacyAuth, alternateLoginUri);
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
            }, MobileAppUriValidator.DummyMobileApp + loginAsyncUriFragment + "/microsoftaccount?display=popup&scope=email%2Cbirthday");
        }

        [AsyncTestMethod]
        public Task LoginAsync_Legacy_IncludesTheParameters()
        {
            return TestLoginAsyncForParameters(new Dictionary<string, string>()
            {
                { "display", "popup" },
                { "scope", "email,birthday" }
            }, MobileAppUriValidator.DummyMobileApp + legacyLoginAsyncUriFragment + "/microsoftaccount?display=popup&scope=email%2Cbirthday", true);
        }

        [AsyncTestMethod]
        public Task LoginAsync_AlternateLoginUri_IncludesTheParameters()
        {
            return TestLoginAsyncForParameters(new Dictionary<string, string>()
            {
                { "display", "popup" },
                { "scope", "email,birthday" }
            }, validAlternateLoginUrl + loginAsyncUriFragment + "/microsoftaccount?display=popup&scope=email%2Cbirthday", false, validAlternateLoginUrl);
        }

        [AsyncTestMethod]
        public Task LoginAsync_Legacy_AlternateLoginUri_IncludesTheParameters()
        {
            return TestLoginAsyncForParameters(new Dictionary<string, string>()
            {
                { "display", "popup" },
                { "scope", "email,birthday" }
            }, validAlternateLoginUrl + legacyLoginAsyncUriFragment + "/microsoftaccount?display=popup&scope=email%2Cbirthday", true, validAlternateLoginUrl);
        }

        [AsyncTestMethod]
        public Task LoginAsync_MobileAppUriWithFolder_IncludesTheParameters()
        {
            return TestLoginAsyncForParameters(new Dictionary<string, string>()
            {
                { "display", "popup" },
                { "scope", "email,birthday" }
            }, MobileAppUriValidator.DummyMobileApp + loginAsyncUriFragment + "/microsoftaccount?display=popup&scope=email%2Cbirthday", false, null, MobileAppUriValidator.DummyMobileAppUriWithFolder);
        }

        [AsyncTestMethod]
        public Task LoginAsync_WithNullParameters()
        {
            return TestLoginAsyncForParameters(null, MobileAppUriValidator.DummyMobileApp + loginAsyncUriFragment + "/microsoftaccount");
        }

        [AsyncTestMethod]
        public Task LoginAsync_WithEmptyParameters()
        {
            return TestLoginAsyncForParameters(new Dictionary<string, string>(), MobileAppUriValidator.DummyMobileApp + loginAsyncUriFragment + "/microsoftaccount");
        }

        [AsyncTestMethod]
        public Task LoginAsync_MobileAppUriWithoutTrailingSlash_WithEmptyParameters()
        {
            return TestLoginAsyncForParameters(new Dictionary<string, string>(), MobileAppUriValidator.DummyMobileApp + loginAsyncUriFragment + "/microsoftaccount", false, null, MobileAppUriValidator.DummyMobileAppWithoutTralingSlash);
        }

        [AsyncTestMethod]
        public Task LoginAsync_Legacy_WithEmptyParameters()
        {
            return TestLoginAsyncForParameters(new Dictionary<string, string>(),
                MobileAppUriValidator.DummyMobileApp + legacyLoginAsyncUriFragment + "/microsoftaccount", true);
        }

        [AsyncTestMethod]
        public Task LoginAsync_AlternateLoginUri_WithEmptyParameters()
        {
            return TestLoginAsyncForParameters(new Dictionary<string, string>(), validAlternateLoginUrl + loginAsyncUriFragment + "/microsoftaccount", false, validAlternateLoginUrl);
        }

        [AsyncTestMethod]
        public Task LoginAsync_Legacy_AlternateLoginUri_WithEmptyParameters()
        {
            return TestLoginAsyncForParameters(new Dictionary<string, string>(), validAlternateLoginUrl + legacyLoginAsyncUriFragment + "/microsoftaccount", true, validAlternateLoginUrl);
        }


        private async Task TestLoginAsyncForParameters(Dictionary<string, string> parameters, string uri, bool legacyAuth = false, string alternateLoginUrl = null, string appUrl = null)
        {
            TestInitialize(appUrl, legacyAuth, alternateLoginUrl);
            var auth = new MobileServiceTokenAuthentication(this.client, "MicrosoftAccount", new JObject(), parameters);
            await auth.LoginAsync();
            Assert.AreEqual(this.hijack.Request.RequestUri.OriginalString, uri);
        }
    }
}
