// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.TestFramework;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    [Tag("unit")]
    [Tag("client")]
    public class MobileServiceClientTests : TestBase
    {
        /// <summary>
        /// Verify we have an installation ID created whenever we use a ZUMO
        /// service. 
        /// </summary>
        [TestMethod]
        public void InstallationId()
        {
            MobileServiceClient service = new MobileServiceClient("http://test.com");

            //string settings = ApplicationData.Current.LocalSettings.Values["MobileServices.Installation.config"] as string;
            //string id = (string)JToken.Parse(settings)["applicationInstallationId"];
            //Assert.IsNotNull(id);
        }

        [TestMethod]
        public void Construction()
        {
            string appUrl = "http://www.test.com/";
            string appKey = "secret...";

            MobileServiceClient service = new MobileServiceClient(new Uri(appUrl), appKey);
            Assert.AreEqual(appUrl, service.ApplicationUri.ToString());
            Assert.AreEqual(appKey, service.ApplicationKey);

            service = new MobileServiceClient(appUrl, appKey);
            Assert.AreEqual(appUrl, service.ApplicationUri.ToString());
            Assert.AreEqual(appKey, service.ApplicationKey);

            service = new MobileServiceClient(new Uri(appUrl));
            Assert.AreEqual(appUrl, service.ApplicationUri.ToString());
            Assert.AreEqual(null, service.ApplicationKey);

            service = new MobileServiceClient(appUrl);
            Assert.AreEqual(appUrl, service.ApplicationUri.ToString());
            Assert.AreEqual(null, service.ApplicationKey);

            Uri none = null;
            Throws<ArgumentNullException>(() => new MobileServiceClient(none));
            Throws<FormatException>(() => new MobileServiceClient("not a valid uri!!!@#!@#"));
        }


        [AsyncTestMethod]
        public async Task SingleHttpHandlerConstructor()
        {
            string appUrl = "http://www.test.com/";
            string appKey = "secret...";
            TestHttpHandler hijack = new TestHttpHandler();

            IMobileServiceClient service =
                new MobileServiceClient(new Uri(appUrl), appKey, hijack);

            // Ensure properties are copied over
            Assert.AreEqual(appUrl, service.ApplicationUri.ToString());
            Assert.AreEqual(appKey, service.ApplicationKey);

            // Set the handler to return an empty array
            hijack.SetResponseContent("[]");
            JToken response = await service.GetTable("foo").ReadAsync("bar");

            // Verify the handler was in the loop
            Assert.StartsWith(hijack.Request.RequestUri.ToString(), appUrl);
        }

        [AsyncTestMethod]
        public async Task MultipleHttpHandlerConstructor()
        {
            string appUrl = "http://www.test.com/";
            string appKey = "secret...";
            TestHttpHandler hijack = new TestHttpHandler();

            string firstBeforeMessage = "Message before 1";
            string firstAfterMessage = "Message after 1";
            string secondBeforeMessage = "Message before 2";
            string secondAfterMessage = "Message after 2";

            ComplexDelegatingHandler firstHandler = new ComplexDelegatingHandler(firstBeforeMessage, firstAfterMessage);
            ComplexDelegatingHandler secondHandler = new ComplexDelegatingHandler(secondBeforeMessage, secondAfterMessage);

            IMobileServiceClient service =
                new MobileServiceClient(new Uri(appUrl), appKey, firstHandler, secondHandler, hijack);

            // Validate that handlers are properly chained
            Assert.AreSame(hijack, secondHandler.InnerHandler);
            Assert.AreSame(secondHandler, firstHandler.InnerHandler);

            // Clears the messages on the handler
            ComplexDelegatingHandler.ClearStoredMessages();

            // Set the handler to return an empty array
            hijack.SetResponseContent("[]");
            JToken response = await service.GetTable("foo").ReadAsync("bar");

            var storedMessages = new List<string>(ComplexDelegatingHandler.AllMessages);
            Assert.AreEqual(4, storedMessages.Count);
            Assert.AreEqual(firstBeforeMessage, storedMessages[0]);
            Assert.AreEqual(secondBeforeMessage, storedMessages[1]);
            Assert.AreEqual(secondAfterMessage, storedMessages[2]);
            Assert.AreEqual(firstAfterMessage, storedMessages[3]);
        }

        [TestMethod]
        public void Logout()
        {
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            service.CurrentUser = new MobileServiceUser("123456");
            Assert.IsNotNull(service.CurrentUser);

            service.Logout();
            Assert.IsNull(service.CurrentUser);
        }

        [AsyncTestMethod]
        public async Task StandardRequestFormat()
        {
            string appUrl = "http://www.test.com";
            string appKey = "secret...";
            string collection = "tests";
            string query = "$filter=id eq 12";

            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient(appUrl, appKey, hijack);
            service.CurrentUser = new MobileServiceUser("someUser");
            service.CurrentUser.MobileServiceAuthenticationToken = "Not rhubarb";

            hijack.SetResponseContent("[{\"id\":12,\"value\":\"test\"}]");
            JToken response = await service.GetTable(collection).ReadAsync(query);

            Assert.IsNotNull(hijack.Request.Headers.GetValues("X-ZUMO-INSTALLATION-ID").First());
            Assert.AreEqual("secret...", hijack.Request.Headers.GetValues("X-ZUMO-APPLICATION").First());
            Assert.AreEqual("application/json", hijack.Request.Headers.Accept.First().MediaType);
            Assert.AreEqual("Not rhubarb", hijack.Request.Headers.GetValues("X-ZUMO-AUTH").First());

            string userAgent = hijack.Request.Headers.UserAgent.ToString();
            Assert.IsTrue(userAgent.Contains("ZUMO/1.0"));
            Assert.IsTrue(userAgent.Contains("version=1.0.0.0"));
        }

        [AsyncTestMethod]
        public async Task ErrorMessageConstruction()
        {
            string appUrl = "http://www.test.com";
            string appKey = "secret...";
            string collection = "tests";
            string query = "$filter=id eq 12";

            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient(appUrl, appKey, hijack);

            // Verify the error message is correctly pulled out
            hijack.SetResponseContent("{\"error\":\"error message\",\"other\":\"donkey\"}");
            hijack.Response.StatusCode = HttpStatusCode.Unauthorized;
            hijack.Response.ReasonPhrase = "YOU SHALL NOT PASS.";
            try
            {
                JToken response = await service.GetTable(collection).ReadAsync(query);
            }
            catch (InvalidOperationException ex)
            {
                Assert.AreEqual(ex.Message, "error message");
            }

            // Verify all of the exception parameters
            hijack.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            hijack.Response.Content = new StringContent("{\"error\":\"error message\",\"other\":\"donkey\"}", Encoding.UTF8, "application/json");
            hijack.Response.ReasonPhrase = "YOU SHALL NOT PASS.";
            try
            {
                JToken response = await service.GetTable(collection).ReadAsync(query);
            }
            catch (MobileServiceInvalidOperationException ex)
            {
                Assert.AreEqual(ex.Message, "error message");
                Assert.AreEqual(HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                Assert.Contains(ex.Response.Content.ReadAsStringAsync().Result, "donkey");
                Assert.StartsWith(ex.Request.RequestUri.ToString(), appUrl);
                Assert.AreEqual("YOU SHALL NOT PASS.", ex.Response.ReasonPhrase);
            }

            // If no error message in the response, we'll use the
            // StatusDescription instead
            hijack.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            hijack.Response.Content = new StringContent("{\"error\":\"error message\",\"other\":\"donkey\"}", Encoding.UTF8, "application/json");
            hijack.Response.ReasonPhrase = "YOU SHALL NOT PASS.";
            hijack.SetResponseContent("{\"other\":\"donkey\"}");

            try
            {
                JToken response = await service.GetTable(collection).ReadAsync(query);
            }
            catch (InvalidOperationException ex)
            {
                Assert.AreEqual("The request could not be completed.  (YOU SHALL NOT PASS.)", ex.Message);
            }
        }

        [TestMethod]
        public void GetTableThrowsWithNullTable()
        {
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            ArgumentNullException expected = null;

            try
            {
                service.GetTable(null);
            }
            catch (ArgumentNullException e)
            {
                expected = e;
            }

            Assert.IsNotNull(expected);

        }

        [TestMethod]
        public void GetTableThrowsWithEmptyStringTable()
        {
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            ArgumentException expected = null;

            try
            {
                service.GetTable("");
            }
            catch (ArgumentException e)
            {
                expected = e;
            }

            Assert.IsNotNull(expected);

        }

        [AsyncTestMethod]
        public async Task InvokeCustomApiThrowsForNullApiName()
        {
            MobileServiceClient service = new MobileServiceClient("http://www.test.com");
            ArgumentNullException expected = null;

            try
            {
                await service.InvokeApiAsync("", null);
            }
            catch(ArgumentNullException e)
            {
                expected = e;
            }

            Assert.IsNotNull(expected);
        }

        [AsyncTestMethod]
        public async Task InvokeCustomAPISimple()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            hijack.SetResponseContent("{\"id\":3}");

            IntType expected = await service.InvokeApiAsync<IntType>("calculator/add?a=1&b=2");

            Assert.AreEqual(hijack.Request.RequestUri.LocalPath, "/api/calculator/add");
            Assert.Contains(hijack.Request.RequestUri.Query, "a=1&b=2");
            Assert.AreEqual(3, expected.Id);
        }

        [AsyncTestMethod]
        public async Task InvokeCustomAPISimpleJToken()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":3}");
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            JToken expected = await service.InvokeApiAsync("calculator/add?a=1&b=2");

            Assert.AreEqual(hijack.Request.RequestUri.LocalPath, "/api/calculator/add");
            Assert.Contains(hijack.Request.RequestUri.Query, "?a=1&b=2");
            Assert.AreEqual(3, (int)expected["id"]);
        }

        [AsyncTestMethod]
        public async Task InvokeCustomAPIPost()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":3}");
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            var body = "{\"test\" : \"one\"}";
            IntType expected = await service.InvokeApiAsync<string, IntType>("calculator/add", body);
            Assert.IsNotNull(hijack.Request.Content);
        }

        [AsyncTestMethod]
        public async Task InvokeCustomAPIPostJToken()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":3}");
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            JObject body = JToken.Parse("{\"test\":\"one\"}") as JObject;
            JToken expected = await service.InvokeApiAsync("calculator/add", body);
            Assert.IsNotNull(hijack.Request.Content);
        }

        [AsyncTestMethod]
        public async Task InvokeCustomAPIPostJTokenBooleanPrimative()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":3}");
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            hijack.OnSendingRequest = async request =>
            {
                string content = await request.Content.ReadAsStringAsync();
                Assert.AreEqual(content, "true");

                return request;
            };

            JToken expected = await service.InvokeApiAsync("calculator/add", new JValue(true));
        }

        [AsyncTestMethod]
        public async Task InvokeCustomAPIPostJTokenNullPrimative()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":3}");
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            hijack.OnSendingRequest = async request =>
            {
                string content = await request.Content.ReadAsStringAsync();
                Assert.AreEqual(content, "null");

                return request;
            };
            
            JToken expected = await service.InvokeApiAsync("calculator/add", new JValue((object) null));
        }


        [AsyncTestMethod]
        public async Task InvokeCustomAPIGet()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            hijack.SetResponseContent("{\"id\":3}");
 
            IntType expected = await service.InvokeApiAsync<IntType>("calculator/add?a=1&b=2", HttpMethod.Get, null);

            Assert.AreEqual(hijack.Request.RequestUri.LocalPath, "/api/calculator/add");
            Assert.Contains(hijack.Request.RequestUri.Query, "a=1&b=2");
            Assert.AreEqual(3, expected.Id);  
        }

        [AsyncTestMethod]
        public async Task InvokeCustomAPIGetJToken()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":3}");
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            JToken expected = await service.InvokeApiAsync("calculator/add?a=1&b=2", HttpMethod.Get, null);

            Assert.AreEqual(hijack.Request.RequestUri.LocalPath, "/api/calculator/add");
            Assert.Contains(hijack.Request.RequestUri.Query, "?a=1&b=2");
            Assert.AreEqual(3, (int)expected["id"]);
        }

        [AsyncTestMethod]
        public async Task InvokeCustomAPIGetWithParams()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":3}");
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            var myParams = new Dictionary<string, string>() { { "a", "1" }, {"b", "2"} };
            IntType expected = await service.InvokeApiAsync<IntType>("calculator/add", HttpMethod.Get, myParams);

            Assert.Contains(hijack.Request.RequestUri.Query, "?a=1&b=2");
        }

        [AsyncTestMethod]
        public async Task InvokeCustomAPIGetWithParamsJToken()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":3}");
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            var myParams = new Dictionary<string, string>() { { "a", "1" }, { "b", "2" } };
            JToken expected = await service.InvokeApiAsync("calculator/add", HttpMethod.Get, myParams);

            Assert.Contains(hijack.Request.RequestUri.Query, "?a=1&b=2");
        }

        [AsyncTestMethod]
        public async Task InvokeCustomAPIPostWithBody()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":3}");
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            var myParams = new Dictionary<string, string>() { { "a", "1" }, { "b", "2" } };
            var body = "{\"test\" : \"one\"}";
            IntType expected = await service.InvokeApiAsync<string, IntType>("calculator/add", body, HttpMethod.Post, myParams);

            Assert.Contains(hijack.Request.RequestUri.Query, "?a=1&b=2");
            Assert.IsNotNull(hijack.Request.Content);
        }

        [AsyncTestMethod]
        public async Task InvokeCustomAPIPostWithBodyJToken()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":3}");
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            var myParams = new Dictionary<string, string>() { { "a", "1" }, { "b", "2" } };
            JObject body = JToken.Parse("{\"test\":\"one\"}") as JObject;
            JToken expected = await service.InvokeApiAsync("calculator/add", body, HttpMethod.Post, myParams);

            Assert.Contains(hijack.Request.RequestUri.Query, "?a=1&b=2");
            Assert.IsNotNull(hijack.Request.Content);
        }

        [AsyncTestMethod]
        public async Task InvokeCustomAPIResponse()
        {
            TestHttpHandler hijack = new TestHttpHandler();

            hijack.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
            hijack.Response.Content = new StringContent("{\"id\":\"2\"}", Encoding.UTF8, "application/json");

            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            HttpResponseMessage response = await service.InvokeApiAsync("calculator/add?a=1&b=2", null, HttpMethod.Post, null, null);
            
            Assert.AreEqual(hijack.Request.RequestUri.LocalPath, "/api/calculator/add");
            Assert.Contains(hijack.Request.RequestUri.Query, "?a=1&b=2");
            Assert.Contains(response.Content.ReadAsStringAsync().Result, "{\"id\":\"2\"}");
        }

        [AsyncTestMethod]
        public async Task InvokeCustomAPIResponseWithParams()
        {
            TestHttpHandler hijack = new TestHttpHandler();

            hijack.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
            hijack.Response.Content = new StringContent("{\"id\":\"2\"}", Encoding.UTF8, "application/json");
            var myParams = new Dictionary<string, string>() { { "a", "1" }, { "b", "2" } };

            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            HttpResponseMessage response = await service.InvokeApiAsync("calculator/add", null, HttpMethod.Post, null, myParams);

            Assert.AreEqual(hijack.Request.RequestUri.LocalPath, "/api/calculator/add");
            Assert.Contains(hijack.Request.RequestUri.Query, "?a=1&b=2");
            Assert.IsNull(hijack.Request.Content);
            Assert.Contains(response.Content.ReadAsStringAsync().Result, "{\"id\":\"2\"}");
        }

        [AsyncTestMethod]
        public async Task InvokeCustomAPIResponseWithParamsBodyAndHeader()
        {
            TestHttpHandler hijack = new TestHttpHandler();

            hijack.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
            hijack.Response.Content = new StringContent("{\"id\":\"2\"}", Encoding.UTF8, "application/json");
            var myParams = new Dictionary<string, string>() { { "a", "1" }, { "b", "2" } };

            HttpContent content = new StringContent("{\"test\" : \"one\"}", Encoding.UTF8, "application/json");
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            var myHeaders = new Dictionary<string, string>() { { "x-zumo-test", "test" } };

            HttpResponseMessage response = await service.InvokeApiAsync("calculator/add", content, HttpMethod.Post, myHeaders, myParams);

            Assert.AreEqual(hijack.Request.RequestUri.LocalPath, "/api/calculator/add");
            Assert.AreEqual(hijack.Request.Headers.GetValues("x-zumo-test").First(), "test");
            Assert.IsNotNull(hijack.Request.Content);
            Assert.Contains(hijack.Request.RequestUri.Query, "?a=1&b=2");
            Assert.Contains(response.Content.ReadAsStringAsync().Result, "{\"id\":\"2\"}");
        }
    
    }
}
