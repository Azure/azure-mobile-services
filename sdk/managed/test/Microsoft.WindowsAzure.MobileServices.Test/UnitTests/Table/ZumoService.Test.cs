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
    [Tag("service")]
    [Tag("unit")]
    public class ZumoServiceTests : TestBase
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

        [AsyncTestMethod]
        public async Task ReadAsync()
        {
            var userDefinedParameters = new Dictionary<string, string>() { { "tags", "#pizza #beer" } };

            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("[]");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            IMobileServiceTable table = service.GetTable("tests");

            await table.ReadAsync("$filter=id eq 12", userDefinedParameters);

            Assert.Contains(hijack.Request.RequestUri.ToString(), "tests");
            Assert.Contains(hijack.Request.RequestUri.AbsoluteUri, "tags=%23pizza%20%23beer");
            Assert.Contains(hijack.Request.RequestUri.ToString(), "$filter=id eq 12");

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
        public async Task ReadAsyncThrowsWithInvalidUserParameters()
        {
            var invalidUserDefinedParameters = new Dictionary<string, string>() { { "$this is invalid", "since it starts with a '$'" } };
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            IMobileServiceTable table = service.GetTable("test");
            ArgumentException expected = null;

            try
            {
                await table.ReadAsync("$filter=id eq 12", invalidUserDefinedParameters);
            }
            catch (ArgumentException e)
            {
                expected = e;
            }

            Assert.IsNotNull(expected);

        }

        [AsyncTestMethod]
        public async Task ReadAsyncGeneric()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            IMobileServiceTable<StringType> table = service.GetTable<StringType>();

            hijack.SetResponseContent("[{\"id\":12,\"String\":\"Hey\"}]");

            List<StringType> people = await table.Where(p => p.Id == 12).ToListAsync();
            Assert.AreEqual(1, people.Count);
            Assert.AreEqual(12, people[0].Id);
            Assert.AreEqual("Hey", people[0].String);
        }

        [AsyncTestMethod]
        public async Task LookupAsync()
        {
            var userDefinedParameters = new Dictionary<string, string>() { { "state", "CA" } };

            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            IMobileServiceTable<StringType> table = service.GetTable<StringType>();

            hijack.SetResponseContent("{\"id\":12,\"String\":\"Hello\"}");

            StringType expected = await table.LookupAsync(12, userDefinedParameters);
            Assert.Contains(hijack.Request.RequestUri.Query, "state=CA");
            Assert.AreEqual(12, expected.Id);
            Assert.AreEqual("Hello", expected.String);
        }

        [AsyncTestMethod]
        public async Task LookupAsyncThrowsWhenNotFound()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            IMobileServiceTable<StringType> table = service.GetTable<StringType>();

            hijack.Response.StatusCode = HttpStatusCode.BadRequest;
            InvalidOperationException expected = null;
            
            try
            {
                await table.LookupAsync(12);
            }
            catch (InvalidOperationException e)
            {
                expected = e;
            }

            Assert.IsNotNull(expected);
        }

        [AsyncTestMethod]
        public async Task InsertAsync()
        {
            var userDefinedParameters = new Dictionary<string, string>() { { "state", "AL" } };

            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            IMobileServiceTable table = service.GetTable("tests");

            JObject obj = JToken.Parse("{\"value\":\"new\"}") as JObject;
            hijack.SetResponseContent("{\"id\":12,\"value\":\"new\"}");
            JToken newObj = await table.InsertAsync(obj, userDefinedParameters);

            Assert.AreEqual(12, (int)newObj["id"]);
            Assert.AreNotEqual(newObj, obj);
            Assert.Contains(hijack.Request.RequestUri.ToString(), "tests");
            Assert.Contains(hijack.Request.RequestUri.Query, "state=AL");
        }

        [AsyncTestMethod]
        public async Task InsertAsyncAsyncThrowsOnNull()
        {
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            IMobileServiceTable table = service.GetTable("tests");
            ArgumentNullException expected = null;

            try
            {
                await table.InsertAsync(null);
            }
            catch (ArgumentNullException e)
            {
                expected = e;
            }

            Assert.IsNotNull(expected);
        }

        [AsyncTestMethod]
        public async Task InsertAsyncThrowsIfIdExists()
        {
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            IMobileServiceTable table = service.GetTable("tests");
            ArgumentException expected = null;

            JObject obj = JToken.Parse("{\"id\":12}") as JObject;
            try
            {
                await table.InsertAsync(obj);
            }
            catch (ArgumentException e)
            {
                expected = e;
            }

            Assert.IsNotNull(expected);
        }

        [AsyncTestMethod]
        public async Task InsertAsyncGenericThrowsIfIdExists()
        {
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            IMobileServiceTable<BoolType> table = service.GetTable<BoolType>();
            ArgumentException expected = null;

            try
            {
                await table.InsertAsync(new BoolType() { Id = 5 });
            }
            catch (ArgumentException e)
            {
                expected = e;
            }

            Assert.IsNotNull(expected);
        }

        [AsyncTestMethod]
        public async Task UpdateAsync()
        {
            var userDefinedParameters = new Dictionary<string, string>() { { "state", "FL" } };

            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            IMobileServiceTable table = service.GetTable("tests");

            JObject obj = JToken.Parse("{\"id\":12,\"value\":\"new\"}") as JObject;
            hijack.SetResponseContent("{\"id\":12,\"value\":\"new\",\"other\":\"123\"}");
            JToken newObj = await table.UpdateAsync(obj, userDefinedParameters);

            Assert.AreEqual("123", (string)newObj["other"]);
            Assert.AreNotEqual(newObj, obj);
            Assert.Contains(hijack.Request.RequestUri.ToString(), "tests");
            Assert.Contains(hijack.Request.RequestUri.Query, "state=FL");
        }

        [AsyncTestMethod]
        public async Task UpdateAsyncThrowsOnNull()
        {
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            IMobileServiceTable table = service.GetTable("tests");
            ArgumentNullException expected = null;

            try
            {
                await table.UpdateAsync(null);
            }
            catch (ArgumentNullException e)
            {
                expected = e;
            }

            Assert.IsNotNull(expected);
        }

        [AsyncTestMethod]
        public async Task UpdateAsyncThrowsWhenIdIsMissing()
        {
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            IMobileServiceTable table = service.GetTable("tests");
            ArgumentException expected = null;

            try
            {
                await table.UpdateAsync(new JObject());
            }
            catch (ArgumentException e)
            {
                expected = e;
            }

            Assert.IsNotNull(expected);
        }

        [AsyncTestMethod]
        public async Task UpdateAsyncThrowsWhenObjectHasDefaultId()
        {
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            IMobileServiceTable<BoolType> table = service.GetTable<BoolType>();
            ArgumentException expected = null;

            try
            {
                await table.UpdateAsync(new BoolType());
            }
            catch (ArgumentException e)
            {
                expected = e;
            }

            Assert.IsNotNull(expected);
        }

        [AsyncTestMethod]
        public async Task DeleteAsync()
        {
            var userDefinedParameters = new Dictionary<string, string>() { { "state", "WY" } };

            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            IMobileServiceTable table = service.GetTable("tests");

            JObject obj = JToken.Parse("{\"id\":12,\"value\":\"new\"}") as JObject;
            await table.DeleteAsync(obj, userDefinedParameters);

            Assert.Contains(hijack.Request.RequestUri.ToString(), "tests");
            Assert.IsNull(hijack.Request.Content);
            Assert.Contains(hijack.Request.RequestUri.Query, "state=WY");
        }

        [AsyncTestMethod]
        public async Task DeleteAsyncThrowsOnNull()
        {
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            IMobileServiceTable table = service.GetTable("tests");
            ArgumentNullException expected = null;
            
            try
            {
                await table.DeleteAsync(null);
            }
            catch (ArgumentNullException e)
            {
               expected = e;
            }

            Assert.IsNotNull(expected);
        }

        [AsyncTestMethod]
        public async Task DeleteAsyncThrowsWhenIdIsMissing()
        {
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            IMobileServiceTable table = service.GetTable("tests");
            ArgumentException expected = null;
            
            try
            {
                await table.DeleteAsync(new JObject());
            }
            catch (ArgumentException e)
            {
                expected = e;
            }

            Assert.IsNotNull(expected);
        }

        [AsyncTestMethod]
        public async Task DeleteAsyncThrowsWhenObjectHasDefaultId()
        {
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            IMobileServiceTable<BoolType> table = service.GetTable<BoolType>();
            ArgumentException expected = null;

            try
            {
                await table.DeleteAsync(new BoolType());
            }
            catch (ArgumentException e)
            {
                expected = e;
            }

            Assert.IsNotNull(expected);
        }
    }
}
