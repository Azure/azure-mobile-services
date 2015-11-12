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
        string validAlternateLoginUrl = "https://www.testalternatelogin.com/";
        string validAlternateLoginUrlWithoutTrailingSlash = "https://www.testalternatelogin.com";


        private void TestInitialize(string appUrl = null, string loginPrefix = null, string alternateLoginUri = null)
        {
            if (string.IsNullOrEmpty(appUrl))
            {
                appUrl = MobileAppUriValidator.DummyMobileApp;
            }
            hijack = new TestHttpHandler();
            hijack.SetResponseContent(String.Empty);

            var originalFactory = MobileServiceHttpClient.DefaultHandlerFactory;
            MobileServiceHttpClient.DefaultHandlerFactory = () => hijack;
            client = new MobileServiceClient(appUrl, hijack);
            client.LoginUriPrefix = loginPrefix;
            if (!string.IsNullOrEmpty(alternateLoginUri))
            {
                client.AlternateLoginHost = new Uri(alternateLoginUri);
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
            }, MobileAppUriValidator.DummyMobileApp + legacyLoginAsyncUriFragment + "/microsoftaccount?display=popup&scope=email%2Cbirthday", "login");
        }


        [TestMethod]
        public void StartUri_AlternateLoginUri_IncludesParameters()
        {
            TestStartUriForParameters(new Dictionary<string, string>()
            {
                { "display", "popup" },
                { "scope", "email,birthday" }
            }, validAlternateLoginUrl + loginAsyncUriFragment + "/microsoftaccount?display=popup&scope=email%2Cbirthday", null, validAlternateLoginUrl);
        }

        [TestMethod]
        public void StartUri_Legacy_AlternateLoginUri_IncludesParameters()
        {
            TestStartUriForParameters(new Dictionary<string, string>()
            {
                { "display", "popup" },
                { "scope", "email,birthday" }
            }, validAlternateLoginUrl + legacyLoginAsyncUriFragment + "/microsoftaccount?display=popup&scope=email%2Cbirthday", "/login", validAlternateLoginUrl);
        }

        [TestMethod]
        public void StartUri_MobileAppWithFolder_IncludesParameters()
        {
            TestStartUriForParameters(new Dictionary<string, string>()
            {
                { "display", "popup" },
                { "scope", "email,birthday" }
            }, MobileAppUriValidator.DummyMobileApp + loginAsyncUriFragment + "/microsoftaccount?display=popup&scope=email%2Cbirthday", null, null, MobileAppUriValidator.DummyMobileAppUriWithFolder);
        }

        [TestMethod]
        public void StartUri_Legacy_MobileAppUriWithFolder_IncludesParameters()
        {
            TestStartUriForParameters(new Dictionary<string, string>()
            {
                { "display", "popup" },
                { "scope", "email,birthday" }
            }, MobileAppUriValidator.DummyMobileApp + legacyLoginAsyncUriFragment + "/microsoftaccount?display=popup&scope=email%2Cbirthday", "login");
        }

        [TestMethod]
        public void StartUri_WithNullParameters()
        {
            TestStartUriForParameters(null, MobileAppUriValidator.DummyMobileApp + loginAsyncUriFragment + "/microsoftaccount");
        }

        [TestMethod]
        public void StartUri_Legacy_WithNullParameters()
        {
            TestStartUriForParameters(null, MobileAppUriValidator.DummyMobileApp + legacyLoginAsyncUriFragment + "/microsoftaccount", "/login");
        }

        [TestMethod]
        public void StartUri_MobileAppUriWihtoutTrailingSlash_WithNullParameters()
        {
            TestStartUriForParameters(null, MobileAppUriValidator.DummyMobileAppWithoutTralingSlash + "/" + legacyLoginAsyncUriFragment + "/microsoftaccount", "login", null, MobileAppUriValidator.DummyMobileAppWithoutTralingSlash);
        }

        [TestMethod]
        public void StartUri_AlternateLoginUri_WithNullParameters()
        {
            TestStartUriForParameters(null, validAlternateLoginUrl + loginAsyncUriFragment + "/microsoftaccount", null, validAlternateLoginUrl);
        }

        [TestMethod]
        public void StartUri_Legacy_AlternateLoginUri_WithNullParameters()
        {
            TestStartUriForParameters(null, validAlternateLoginUrl + legacyLoginAsyncUriFragment + "/microsoftaccount", "login", validAlternateLoginUrl);
        }

        [TestMethod]
        public void StartUri_WithEmptyParameters()
        {
            TestStartUriForParameters(new Dictionary<string, string>(), MobileAppUriValidator.DummyMobileApp + loginAsyncUriFragment + "/microsoftaccount");
        }

        [TestMethod]
        public void StartUri_Legacy_WithEmptyParameters()
        {
            TestStartUriForParameters(new Dictionary<string, string>(), MobileAppUriValidator.DummyMobileApp + legacyLoginAsyncUriFragment + "/microsoftaccount", "login");
        }

        [TestMethod]
        public void StartUri_AlternateLoginUrl_WithEmptyParameters()
        {
            TestStartUriForParameters(new Dictionary<string, string>(), validAlternateLoginUrlWithoutTrailingSlash + "/" + loginAsyncUriFragment + "/microsoftaccount", null, validAlternateLoginUrlWithoutTrailingSlash);
        }

        [TestMethod]
        public void StartUri_Legacy_AlternateLoginUrl_WithEmptyParameters()
        {
            TestStartUriForParameters(new Dictionary<string, string>(), validAlternateLoginUrl + legacyLoginAsyncUriFragment + "/microsoftaccount", "/login", validAlternateLoginUrl);
        }

        [TestMethod]
        public void StartUri_Legacy_MobileAppUriWithFolder_WithEmptyParameters()
        {
            TestStartUriForParameters(new Dictionary<string, string>(), MobileAppUriValidator.DummyMobileApp + legacyLoginAsyncUriFragment + "/microsoftaccount", "login", null, MobileAppUriValidator.DummyMobileAppUriWithFolder);
        }

        [TestMethod]
        public void StartUri_ThrowsInvalidAlternateLoginHost()
        {
            var client = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            AssertEx.Throws<ArgumentException>(() => client.AlternateLoginHost = new Uri(MobileAppUriValidator.DummyMobileAppUriWithFolder));
            AssertEx.Throws<ArgumentException>(() => client.AlternateLoginHost = new Uri(MobileAppUriValidator.DummyMobileAppUriWithFolderWithoutTralingSlash));
            AssertEx.Throws<ArgumentException>(() => client.AlternateLoginHost = new Uri("http://www.testalternatelogin.com/"));
        }

        [TestMethod]
        public void AlternateLoginUri_Null()
        {
            var client = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            client.AlternateLoginHost = null;
            Assert.AreEqual(client.AlternateLoginHost, client.MobileAppUri);
        }

        private void TestStartUriForParameters(Dictionary<string, string> parameters, string uri, string loginPrefix = null, string alternateLoginUri = null, string appUrl = null)
        {
            TestInitialize(appUrl, loginPrefix, alternateLoginUri);
            var auth = new MobileServiceTokenAuthentication(client, "MicrosoftAccount", new JObject(), parameters);
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
            }, MobileAppUriValidator.DummyMobileApp + legacyLoginAsyncUriFragment + "/microsoftaccount?display=popup&scope=email%2Cbirthday", "login");
        }

        [AsyncTestMethod]
        public Task LoginAsync_AlternateLoginUri_IncludesTheParameters()
        {
            return TestLoginAsyncForParameters(new Dictionary<string, string>()
            {
                { "display", "popup" },
                { "scope", "email,birthday" }
            }, validAlternateLoginUrl + loginAsyncUriFragment + "/microsoftaccount?display=popup&scope=email%2Cbirthday", null, validAlternateLoginUrl);
        }

        [AsyncTestMethod]
        public Task LoginAsync_Legacy_AlternateLoginUri_IncludesTheParameters()
        {
            return TestLoginAsyncForParameters(new Dictionary<string, string>()
            {
                { "display", "popup" },
                { "scope", "email,birthday" }
            }, validAlternateLoginUrl + legacyLoginAsyncUriFragment + "/microsoftaccount?display=popup&scope=email%2Cbirthday", "/login", validAlternateLoginUrl);
        }

        [AsyncTestMethod]
        public Task LoginAsync_MobileAppUriWithFolder_IncludesTheParameters()
        {
            return TestLoginAsyncForParameters(new Dictionary<string, string>()
            {
                { "display", "popup" },
                { "scope", "email,birthday" }
            }, MobileAppUriValidator.DummyMobileApp + loginAsyncUriFragment + "/microsoftaccount?display=popup&scope=email%2Cbirthday", null, null, MobileAppUriValidator.DummyMobileAppUriWithFolder);
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
            return TestLoginAsyncForParameters(new Dictionary<string, string>(), MobileAppUriValidator.DummyMobileApp + loginAsyncUriFragment + "/microsoftaccount", null, null, MobileAppUriValidator.DummyMobileAppWithoutTralingSlash);
        }

        [AsyncTestMethod]
        public Task LoginAsync_Legacy_WithEmptyParameters()
        {
            return TestLoginAsyncForParameters(new Dictionary<string, string>(),
                MobileAppUriValidator.DummyMobileApp + legacyLoginAsyncUriFragment + "/microsoftaccount", "login");
        }

        [AsyncTestMethod]
        public Task LoginAsync_AlternateLoginUri_WithEmptyParameters()
        {
            return TestLoginAsyncForParameters(new Dictionary<string, string>(), validAlternateLoginUrl + loginAsyncUriFragment + "/microsoftaccount", null, validAlternateLoginUrl);
        }

        [AsyncTestMethod]
        public Task LoginAsync_Legacy_AlternateLoginUri_WithEmptyParameters()
        {
            return TestLoginAsyncForParameters(new Dictionary<string, string>(), validAlternateLoginUrl + legacyLoginAsyncUriFragment + "/microsoftaccount", "login", validAlternateLoginUrl);
        }

        private async Task TestLoginAsyncForParameters(Dictionary<string, string> parameters, string uri, string loginPrefix = null, string alternateLoginUrl = null, string appUrl = null)
        {
            TestInitialize(appUrl, loginPrefix, alternateLoginUrl);
            var auth = new MobileServiceTokenAuthentication(client, "MicrosoftAccount", new JObject(), parameters);
            await auth.LoginAsync();
            Assert.AreEqual(hijack.Request.RequestUri.OriginalString, uri);
        }
    }
}
