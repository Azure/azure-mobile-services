﻿// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Microsoft.WindowsAzure.MobileServices.Test.UnitTests;
using Microsoft.WindowsAzure.MobileServices.Test.UnitTests.Common;
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
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);

            //string settings = ApplicationData.Current.LocalSettings.Values["MobileServices.Installation.config"] as string;
            //string id = (string)JToken.Parse(settings)["applicationInstallationId"];
            //Assert.IsNotNull(id);
        }

        [TestMethod]
        public void Construction()
        {
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            Assert.AreEqual(MobileAppUriValidator.DummyMobileApp, service.MobileAppUri.ToString());

            // No Mobile Application URI null or invalid
            Throws<ArgumentNullException>(() => new MobileServiceClient(mobileAppUri: (string)null));
            Throws<ArgumentNullException>(() => new MobileServiceClient(mobileAppUri: (Uri)null));
            Throws<FormatException>(() => new MobileServiceClient("not a valid uri!!!@#!@#"));

            // Mobile Application URI without trailing Slash
            service = new MobileServiceClient(MobileAppUriValidator.DummyMobileAppWithoutTralingSlash);

            Assert.IsNotNull(service.MobileAppUri);
            Assert.IsTrue(service.MobileAppUri.IsAbsoluteUri);
            Assert.IsTrue(service.MobileAppUri.AbsoluteUri.EndsWith(UriUtilities.Slash.ToString()));
            Assert.IsNotNull(service.HttpClient);
        }

        [AsyncTestMethod]
        public async Task SingleHttpHandlerConstructor()
        {
            TestHttpHandler hijack = new TestHttpHandler();

            IMobileServiceClient service =
                new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, handlers: hijack);
            MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);

            // Ensure properties are copied over
            Assert.AreEqual(MobileAppUriValidator.DummyMobileApp, service.MobileAppUri.ToString());

            // Set the handler to return an empty array
            hijack.SetResponseContent("[]");
            JToken response = await service.GetTable("foo").ReadAsync("bar");

            // Verify the handler was in the loop
            Assert.StartsWith(hijack.Request.RequestUri.ToString(), mobileAppUriValidator.TableBaseUri);
        }

        [TestMethod]
        public void DoesNotRewireSingleWiredDelegatingHandler()
        {
            string appUrl = MobileAppUriValidator.DummyMobileApp;
            string appKey = "secret...";

            TestHttpHandler innerHandler = new TestHttpHandler();
            DelegatingHandler wiredHandler = new TestHttpHandler();
            wiredHandler.InnerHandler = innerHandler;

            IMobileServiceClient service = new MobileServiceClient(appUrl, handlers: wiredHandler);

            Assert.AreEqual(wiredHandler.InnerHandler, innerHandler, "The prewired handler passed in should not have been rewired");
        }

        [AsyncTestMethod]
        public async Task MultipleHttpHandlerConstructor()
        {
            string appUrl = MobileAppUriValidator.DummyMobileApp;
            string appKey = "secret...";
            TestHttpHandler hijack = new TestHttpHandler();

            string firstBeforeMessage = "Message before 1";
            string firstAfterMessage = "Message after 1";
            string secondBeforeMessage = "Message before 2";
            string secondAfterMessage = "Message after 2";

            ComplexDelegatingHandler firstHandler = new ComplexDelegatingHandler(firstBeforeMessage, firstAfterMessage);
            ComplexDelegatingHandler secondHandler = new ComplexDelegatingHandler(secondBeforeMessage, secondAfterMessage);

            IMobileServiceClient service =
                new MobileServiceClient(appUrl, handlers: new HttpMessageHandler[] { firstHandler, secondHandler, hijack });

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

        [AsyncTestMethod]
        public async Task Logout()
        {
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            service.CurrentUser = new MobileServiceUser("123456");
            Assert.IsNotNull(service.CurrentUser);

            await service.LogoutAsync();
            Assert.IsNull(service.CurrentUser);
        }

        [AsyncTestMethod]
        public async Task StandardRequestFormat()
        {
            string appUrl = MobileAppUriValidator.DummyMobileApp;
            string collection = "tests";
            string query = "$filter=id eq 12";

            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient(appUrl, hijack);
            service.CurrentUser = new MobileServiceUser("someUser");
            service.CurrentUser.MobileServiceAuthenticationToken = "Not rhubarb";

            hijack.SetResponseContent("[{\"id\":12,\"value\":\"test\"}]");
            JToken response = await service.GetTable(collection).ReadAsync(query);

            Assert.IsNotNull(hijack.Request.Headers.GetValues("X-ZUMO-INSTALLATION-ID").First());
            Assert.AreEqual("application/json", hijack.Request.Headers.Accept.First().MediaType);
            Assert.AreEqual("Not rhubarb", hijack.Request.Headers.GetValues("X-ZUMO-AUTH").First());
            Assert.IsNotNull(hijack.Request.Headers.GetValues("ZUMO-API-VERSION").First());

            // Workaround mono bug https://bugzilla.xamarin.com/show_bug.cgi?id=15128
            // use commented line below once the bug fix has hit stable channel for xamarin.iOS
            // string userAgent = hijack.Request.Headers.UserAgent.ToString();

            string userAgent = string.Join(" ", hijack.Request.Headers.GetValues("user-agent"));
            Assert.IsTrue(userAgent.Contains("ZUMO/2."));
            Assert.IsTrue(userAgent.Contains("version=2."));
        }

        [AsyncTestMethod]
        public async Task ErrorMessageConstruction()
        {
            string appUrl = MobileAppUriValidator.DummyMobileApp;
            string appKey = "secret...";
            string collection = "tests";
            string query = "$filter=id eq 12";

            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient(appUrl, hijack);
            MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);

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
                Assert.StartsWith(ex.Request.RequestUri.ToString(), mobileAppUriValidator.TableBaseUri);
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
        public async Task DeleteAsync_Throws_WhenSyncContextIsNotInitialized()
        {
            await this.TestSyncContextNotInitialized(table => table.DeleteAsync(new ToDoWithSystemPropertiesType("abc")));
        }

        [TestMethod]
        public async Task InsertAsync_Throws_WhenSyncContextIsNotInitialized()
        {
            await this.TestSyncContextNotInitialized(table => table.InsertAsync(new ToDoWithSystemPropertiesType("abc")));
        }

        [TestMethod]
        public async Task UpdateAsync_Throws_WhenSyncContextIsNotInitialized()
        {
            await this.TestSyncContextNotInitialized(table => table.UpdateAsync(new ToDoWithSystemPropertiesType("abc")));
        }

        [TestMethod]
        public async Task LookupAsync_Throws_WhenSyncContextIsNotInitialized()
        {
            await this.TestSyncContextNotInitialized(table => table.LookupAsync("abc"));
        }

        [TestMethod]
        public async Task ReadAsync_Throws_WhenSyncContextIsNotInitialized()
        {
            await this.TestSyncContextNotInitialized(table => table.Where(t => t.String == "abc").ToListAsync());
        }

        [TestMethod]
        public async Task PurgeAsync_Throws_WhenSyncContextIsNotInitialized()
        {
            await this.TestSyncContextNotInitialized(table => table.PurgeAsync(table.Where(t => t.String == "abc")));
        }

        [TestMethod]
        public async Task PullAsync_Throws_WhenSyncContextIsNotInitialized()
        {
            await this.TestSyncContextNotInitialized(table => table.PullAsync(null, table.Where(t => t.String == "abc")));
        }

        private async Task TestSyncContextNotInitialized(Func<IMobileServiceSyncTable<ToDoWithSystemPropertiesType>, Task> action)
        {
            var service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            IMobileServiceSyncTable<ToDoWithSystemPropertiesType> table = service.GetSyncTable<ToDoWithSystemPropertiesType>();
            var ex = await AssertEx.Throws<InvalidOperationException>(() => action(table));
            Assert.AreEqual(ex.Message, "SyncContext is not yet initialized.");
        }

        [TestMethod]
        public void GetTableThrowsWithNullTable()
        {
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
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
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
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
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            ArgumentNullException expected = null;

            try
            {
                await service.InvokeApiAsync("", null);
            }
            catch (ArgumentNullException e)
            {
                expected = e;
            }

            Assert.IsNotNull(expected);
        }

        [AsyncTestMethod]
        public async Task InvokeCustomAPISimple()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);

            hijack.SetResponseContent("{\"id\":3}");

            IntType expected = await service.InvokeApiAsync<IntType>("calculator/add?a=1&b=2");

            Assert.AreEqual(hijack.Request.RequestUri.LocalPath, mobileAppUriValidator.GetApiUriPath("calculator/add"));
            Assert.Contains(hijack.Request.RequestUri.Query, "a=1&b=2");
            Assert.AreEqual(3, expected.Id);
        }

        [AsyncTestMethod]
        public async Task InvokeCustomAPISimpleJToken()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":3}");
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);

            JToken expected = await service.InvokeApiAsync("calculator/add?a=1&b=2");

            Assert.AreEqual(hijack.Request.RequestUri.LocalPath, mobileAppUriValidator.GetApiUriPath("calculator/add"));
            Assert.Contains(hijack.Request.RequestUri.Query, "?a=1&b=2");
            Assert.AreEqual(3, (int)expected["id"]);
        }

        [AsyncTestMethod]
        public async Task InvokeCustomAPIPost()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":3}");
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            var body = "{\"test\" : \"one\"}";
            IntType expected = await service.InvokeApiAsync<string, IntType>("calculator/add", body);
            Assert.IsNotNull(hijack.Request.Content);
        }

        [AsyncTestMethod]
        public async Task InvokeCustomAPIPostJToken()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":3}");
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            JObject body = JToken.Parse("{\"test\":\"one\"}") as JObject;
            JToken expected = await service.InvokeApiAsync("calculator/add", body);
            Assert.IsNotNull(hijack.Request.Content);
        }

        [AsyncTestMethod]
        public async Task InvokeCustomAPIPostJTokenBooleanPrimative()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":3}");
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
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
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            hijack.OnSendingRequest = async request =>
            {
                string content = await request.Content.ReadAsStringAsync();
                Assert.AreEqual(content, "null");

                return request;
            };

            JToken expected = await service.InvokeApiAsync("calculator/add", new JValue((object)null));
        }


        [AsyncTestMethod]
        public async Task InvokeCustomAPIGet()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);

            hijack.SetResponseContent("{\"id\":3}");

            IntType expected = await service.InvokeApiAsync<IntType>("calculator/add?a=1&b=2", HttpMethod.Get, null);

            Assert.AreEqual(hijack.Request.RequestUri.LocalPath, mobileAppUriValidator.GetApiUriPath("calculator/add"));
            Assert.Contains(hijack.Request.RequestUri.Query, "a=1&b=2");
            Assert.AreEqual(3, expected.Id);
        }

        [AsyncTestMethod]
        public async Task InvokeApiAsync_DoesNotAppendApiPath_IfApiStartsWithSlash()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            var service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            hijack.SetResponseContent("{\"id\":3}");

            await service.InvokeApiAsync<IntType>("/calculator/add?a=1&b=2", HttpMethod.Get, null);

            Assert.AreEqual(hijack.Request.RequestUri.LocalPath, "/calculator/add");
            Assert.Contains(hijack.Request.RequestUri.Query, "a=1&b=2");
        }

        [AsyncTestMethod]
        public async Task InvokeCustomAPIGetJToken()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":3}");
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);

            JToken expected = await service.InvokeApiAsync("calculator/add?a=1&b=2", HttpMethod.Get, null);

            Assert.AreEqual(hijack.Request.RequestUri.LocalPath, mobileAppUriValidator.GetApiUriPath("calculator/add"));
            Assert.Contains(hijack.Request.RequestUri.Query, "?a=1&b=2");
            Assert.AreEqual(3, (int)expected["id"]);
        }

        [AsyncTestMethod]
        public async Task InvokeCustomAPIGetWithParams()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":3}");
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            var myParams = new Dictionary<string, string>() { { "a", "1" }, { "b", "2" } };
            IntType expected = await service.InvokeApiAsync<IntType>("calculator/add", HttpMethod.Get, myParams);

            Assert.Contains(hijack.Request.RequestUri.Query, "?a=1&b=2");
        }

        [AsyncTestMethod]
        public async Task InvokeCustomAPIGetWithParamsJToken()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":3}");
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            var myParams = new Dictionary<string, string>() { { "a", "1" }, { "b", "2" } };
            JToken expected = await service.InvokeApiAsync("calculator/add", HttpMethod.Get, myParams);

            Assert.Contains(hijack.Request.RequestUri.Query, "?a=1&b=2");
        }

        [AsyncTestMethod]
        public async Task InvokeCustomAPIGetWithODataParams()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":3}");
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            var myParams = new Dictionary<string, string>() { { "$select", "one,two" }, { "$take", "1" } };
            IntType expected = await service.InvokeApiAsync<IntType>("calculator/add", HttpMethod.Get, myParams);

            Assert.Contains(hijack.Request.RequestUri.Query, "?%24select=one%2Ctwo&%24take=1");
        }

        [AsyncTestMethod]
        public async Task InvokeCustomAPIGetWithODataParamsJToken()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":3}");
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            var myParams = new Dictionary<string, string>() { { "$select", "one,two" } };
            JToken expected = await service.InvokeApiAsync("calculator/add", HttpMethod.Get, myParams);

            Assert.Contains(hijack.Request.RequestUri.Query, "?%24select=one%2Ctwo");
        }

        [AsyncTestMethod]
        public async Task InvokeCustomAPIPostWithBody()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":3}");
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

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
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

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

            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);
            HttpResponseMessage response = await service.InvokeApiAsync("calculator/add?a=1&b=2", null, HttpMethod.Post, null, null);

            Assert.AreEqual(hijack.Request.RequestUri.LocalPath, mobileAppUriValidator.GetApiUriPath("calculator/add"));
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

            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);

            HttpResponseMessage response = await service.InvokeApiAsync("calculator/add", null, HttpMethod.Post, null, myParams);

            Assert.AreEqual(hijack.Request.RequestUri.LocalPath, mobileAppUriValidator.GetApiUriPath("calculator/add"));
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
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);

            var myHeaders = new Dictionary<string, string>() { { "x-zumo-test", "test" } };

            HttpResponseMessage response = await service.InvokeApiAsync("calculator/add", content, HttpMethod.Post, myHeaders, myParams);

            Assert.AreEqual(myHeaders.Count, 1); // my headers should not be modified
            Assert.AreEqual(myHeaders["x-zumo-test"], "test");

            Assert.AreEqual(hijack.Request.RequestUri.LocalPath, mobileAppUriValidator.GetApiUriPath("calculator/add"));
            Assert.AreEqual(hijack.Request.Headers.GetValues("x-zumo-test").First(), "test");
            Assert.IsNotNull(hijack.Request.Content);
            Assert.Contains(hijack.Request.RequestUri.Query, "?a=1&b=2");
            Assert.Contains(response.Content.ReadAsStringAsync().Result, "{\"id\":\"2\"}");
        }

        [AsyncTestMethod]
        public async Task InvokeCustomAPIWithEmptyStringResponse_Success()
        {
            TestHttpHandler hijack = new TestHttpHandler();

            hijack.Response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            hijack.Response.Content = new StringContent("", Encoding.UTF8, "application/json");

            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);

            JToken expected = await service.InvokeApiAsync("testapi");
            Assert.AreEqual(hijack.Request.RequestUri.LocalPath, mobileAppUriValidator.GetApiUriPath("testapi"));
            Assert.AreEqual(expected, null);
        }

        [AsyncTestMethod]
        public async Task InvokeGenericCustomAPIWithNullResponse_Success()
        {
            TestHttpHandler hijack = new TestHttpHandler();

            hijack.Response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            hijack.Response.Content = null;

            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);

            IntType expected = await service.InvokeApiAsync<IntType>("testapi");
            Assert.AreEqual(hijack.Request.RequestUri.LocalPath, mobileAppUriValidator.GetApiUriPath("testapi"));
            Assert.AreEqual(expected, null);
        }

        [AsyncTestMethod]
        public async Task InvokeCustomAPI_ErrorWithJsonObject()
        {
            TestHttpHandler hijack = new TestHttpHandler();

            hijack.Response = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
            hijack.Response.Content = new StringContent("{ error: \"message\"}", Encoding.UTF8, "application/json");

            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            try
            {
                await service.InvokeApiAsync("testapi");
                Assert.Fail("Invoke API should have thrown");
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.Message, "message");
            }
        }

        [AsyncTestMethod]
        public async Task InvokeCustomAPI_ErrorWithJsonString()
        {
            TestHttpHandler hijack = new TestHttpHandler();

            hijack.Response = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
            hijack.Response.Content = new StringContent("\"message\"", Encoding.UTF8, "application/json");

            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            try
            {
                await service.InvokeApiAsync("testapi");
                Assert.Fail("Invoke API should have thrown");
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.Message, "message");
            }
        }

        [AsyncTestMethod]
        public async Task InvokeCustomAPI_ErrorWithString()
        {
            TestHttpHandler hijack = new TestHttpHandler();

            hijack.Response = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
            hijack.Response.Content = new StringContent("message", Encoding.UTF8, "text/html");

            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            try
            {
                await service.InvokeApiAsync("testapi");
                Assert.Fail("Invoke API should have thrown");
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.Message, "message");
            }
        }

        [AsyncTestMethod]
        public async Task InvokeCustomAPI_ErrorStringAndNoContentType()
        {
            TestHttpHandler hijack = new TestHttpHandler();

            hijack.Response = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
            hijack.Response.Content = new StringContent("message", Encoding.UTF8, null);
            hijack.Response.Content.Headers.ContentType = null;

            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            try
            {
                await service.InvokeApiAsync("testapi");
                Assert.Fail("Invoke API should have thrown");
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.Message, "The request could not be completed.  (Bad Request)");
            }
        }

        [AsyncTestMethod]
        public async Task InvokeApiJsonOverloads_HasCorrectFeaturesHeader()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.OnSendingRequest = (request) =>
            {
                Assert.AreEqual("AJ", request.Headers.GetValues("X-ZUMO-FEATURES").First());
                return Task.FromResult(request);
            };

            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            hijack.SetResponseContent("{\"hello\":\"world\"}");
            await service.InvokeApiAsync("apiName");

            hijack.SetResponseContent("{\"hello\":\"world\"}");
            await service.InvokeApiAsync("apiName", JObject.Parse("{\"a\":1}"));

            hijack.OnSendingRequest = (request) =>
            {
                Assert.AreEqual("AJ,QS", request.Headers.GetValues("X-ZUMO-FEATURES").First());
                return Task.FromResult(request);
            };

            var dic = new Dictionary<string, string> { { "a", "b" } };
            hijack.SetResponseContent("{\"hello\":\"world\"}");
            await service.InvokeApiAsync("apiName", HttpMethod.Get, dic);

            hijack.SetResponseContent("{\"hello\":\"world\"}");
            await service.InvokeApiAsync("apiName", null, HttpMethod.Delete, dic);
        }

        [AsyncTestMethod]
        public async Task InvokeApiTypedOverloads_HasCorrectFeaturesHeader()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.OnSendingRequest = (request) =>
            {
                Assert.AreEqual("AT", request.Headers.GetValues("X-ZUMO-FEATURES").First());
                return Task.FromResult(request);
            };

            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            hijack.SetResponseContent("{\"id\":3}");
            await service.InvokeApiAsync<IntType>("apiName");

            hijack.SetResponseContent("{\"id\":3}");
            await service.InvokeApiAsync<IntType, IntType>("apiName", new IntType { Id = 1 });

            hijack.OnSendingRequest = (request) =>
            {
                Assert.AreEqual("AT,QS", request.Headers.GetValues("X-ZUMO-FEATURES").First());
                return Task.FromResult(request);
            };

            var dic = new Dictionary<string, string> { { "a", "b" } };
            hijack.SetResponseContent("{\"id\":3}");
            await service.InvokeApiAsync<IntType>("apiName", HttpMethod.Get, dic);

            hijack.SetResponseContent("{\"hello\":\"world\"}");
            await service.InvokeApiAsync<IntType, IntType>("apiName", new IntType { Id = 1 }, HttpMethod.Put, dic);
        }

        [AsyncTestMethod]
        public Task FeatureHeaderValidation_InvokeApi_String()
        {
            return this.ValidateFeaturesHeader("AJ", c => c.InvokeApiAsync("apiName"));
        }

        [AsyncTestMethod]
        public Task FeatureHeaderValidation_InvokeApi_String_JToken()
        {
            return this.ValidateFeaturesHeader("AJ", c => c.InvokeApiAsync("apiName", JObject.Parse("{\"id\":1}")));
        }

        [AsyncTestMethod]
        public Task FeatureHeaderValidation_InvokeApi_String_HttpMethod_Dict()
        {
            return this.ValidateFeaturesHeader("AJ,QS", c => c.InvokeApiAsync("apiName", null, HttpMethod.Get, new Dictionary<string, string> { { "a", "b" } }));
        }

        [AsyncTestMethod]
        public Task FeatureHeaderValidation_InvokeApi_String_JToken_HttpMethod_Dict()
        {
            return this.ValidateFeaturesHeader("AJ,QS", c => c.InvokeApiAsync("apiName", JObject.Parse("{\"id\":1}"), HttpMethod.Put, new Dictionary<string, string> { { "a", "b" } }));
        }

        [AsyncTestMethod]
        public Task FeatureHeaderValidation_InvokeApi_String_HttpContent_NoQueryParams()
        {
            var content = new StringContent("hello world", Encoding.UTF8, "text/plain");
            return this.ValidateFeaturesHeader("AG", c => c.InvokeApiAsync("apiName", content, HttpMethod.Post, null, null));
        }

        [AsyncTestMethod]
        public Task FeatureHeaderValidation_InvokeApi_String_HttpContent_WithQueryParams()
        {
            var content = new StringContent("hello world", Encoding.UTF8, "text/plain");
            return this.ValidateFeaturesHeader("AG", c => c.InvokeApiAsync("apiName", content, HttpMethod.Post, null, new Dictionary<string, string> { { "a", "b" } }));
        }

        [AsyncTestMethod]
        public Task FeatureHeaderValidation_TypedInvokeApi_String()
        {
            return this.ValidateFeaturesHeader("AT", c => c.InvokeApiAsync<IntType>("apiName"));
        }

        [AsyncTestMethod]
        public Task FeatureHeaderValidation_TypedInvokeApi_String_HttpMethod_Dict()
        {
            return this.ValidateFeaturesHeader("AT,QS", c => c.InvokeApiAsync<IntType>("apiName", HttpMethod.Get, new Dictionary<string, string> { { "a", "b" } }));
        }

        [AsyncTestMethod]
        public Task FeatureHeaderValidation_TypedInvokeApi_String_T()
        {
            return this.ValidateFeaturesHeader("AT", c => c.InvokeApiAsync<IntType, IntType>("apiName", new IntType { Id = 1 }));
        }

        [AsyncTestMethod]
        public Task FeatureHeaderValidation_TypedInvokeApi_String_T_HttpMethod_Dict()
        {
            return this.ValidateFeaturesHeader("AT,QS", c => c.InvokeApiAsync<IntType, IntType>("apiName", new IntType { Id = 1 }, HttpMethod.Get, new Dictionary<string, string> { { "a", "b" } }));
        }

        private async Task ValidateFeaturesHeader(string expectedFeaturesHeader, Func<IMobileServiceClient, Task> operation)
        {
            TestHttpHandler hijack = new TestHttpHandler();
            bool validationDone = false;
            hijack.OnSendingRequest = (request) =>
            {
                Assert.AreEqual(expectedFeaturesHeader, request.Headers.GetValues("X-ZUMO-FEATURES").First());
                validationDone = true;
                return Task.FromResult(request);
            };

            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            hijack.SetResponseContent("{\"id\":3}");
            await operation(service);
            Assert.IsTrue(validationDone);
        }
    }
}
