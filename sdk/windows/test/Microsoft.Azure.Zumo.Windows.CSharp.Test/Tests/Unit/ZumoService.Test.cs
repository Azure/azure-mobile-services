// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Zumo.Win8.Test;
using Microsoft.WindowsAzure.MobileServices;
using Windows.Data.Json;
using Windows.Storage;

namespace Microsoft.Azure.Zumo.Win8.CSharp.Test
{
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

            string settings = ApplicationData.Current.LocalSettings.Values["MobileServices.Installation.config"] as string;
            string id = JsonValue.Parse(settings).Get("applicationInstallationId").AsString();
            Assert.IsNotNull(id);
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
        public async Task WithFilter()
        {
            string appUrl = "http://www.test.com/";
            string appKey = "secret...";
            TestServiceFilter hijack = new TestServiceFilter();

            MobileServiceClient service =
                new MobileServiceClient(new Uri(appUrl), appKey)
                .WithFilter(hijack);

            // Ensure properties are copied over
            Assert.AreEqual(appUrl, service.ApplicationUri.ToString());
            Assert.AreEqual(appKey, service.ApplicationKey);

            // Set the filter to return an empty array
            hijack.Response.Content = new JsonArray().Stringify();
            IJsonValue response = await service.GetTable("foo").ReadAsync("bar");

            // Verify the filter was in the loop
            Assert.StartsWith(hijack.Request.Uri.ToString(), appUrl);

            Throws<ArgumentNullException>(() => service.WithFilter(null));            
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

            TestServiceFilter hijack = new TestServiceFilter();
            MobileServiceClient service = new MobileServiceClient(appUrl, appKey)
                .WithFilter(hijack);
            service.CurrentUser = new MobileServiceUser("someUser");
            service.CurrentUser.MobileServiceAuthenticationToken = "Not rhubarb";

            hijack.Response.Content =
                new JsonArray()
                    .Append(new JsonObject().Set("id", 12).Set("value", "test"))
                    .Stringify();
            IJsonValue response = await service.GetTable(collection).ReadAsync(query);

            Assert.IsNotNull(hijack.Request.Headers["X-ZUMO-INSTALLATION-ID"]);
            Assert.AreEqual("secret...", hijack.Request.Headers["X-ZUMO-APPLICATION"]);
            Assert.AreEqual("application/json", hijack.Request.Accept);
            Assert.AreEqual("Not rhubarb", hijack.Request.Headers["X-ZUMO-AUTH"]);
        }

        [AsyncTestMethod]
        public async Task ErrorMessageConstruction()
        {
            string appUrl = "http://www.test.com";
            string appKey = "secret...";
            string collection = "tests";
            string query = "$filter=id eq 12";

            TestServiceFilter hijack = new TestServiceFilter();
            MobileServiceClient service = new MobileServiceClient(appUrl, appKey)
                .WithFilter(hijack);

            // Verify the error message is correctly pulled out
            hijack.Response.Content =
                new JsonObject()
                    .Set("error", "error message")
                    .Set("other", "donkey")
                    .Stringify();
            hijack.Response.StatusCode = 401;
            hijack.Response.StatusDescription = "YOU SHALL NOT PASS.";
            try
            {
                IJsonValue response = await service.GetTable(collection).ReadAsync(query);
            }
            catch (InvalidOperationException ex)
            {
                Assert.AreEqual(ex.Message, "error message");
            }

            // Verify all of the exception parameters
            hijack.Response.Content =
                new JsonObject()
                    .Set("error", "error message")
                    .Set("other", "donkey")
                    .Stringify();
            hijack.Response.StatusCode = 401;
            hijack.Response.StatusDescription = "YOU SHALL NOT PASS.";
            try
            {
                IJsonValue response = await service.GetTable(collection).ReadAsync(query);
            }
            catch (MobileServiceInvalidOperationException ex)
            {
                Assert.AreEqual(ex.Message, "error message");
                Assert.AreEqual((int)HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                Assert.Contains(ex.Response.Content, "donkey");
                Assert.StartsWith(ex.Request.Uri.ToString(), appUrl);
                Assert.AreEqual("YOU SHALL NOT PASS.", ex.Response.StatusDescription);
            }

            // If no error message in the response, we'll use the
            // StatusDescription instead
            hijack.Response.Content =
                new JsonObject()
                    .Set("other", "donkey")
                    .Stringify();
            try
            {
                IJsonValue response = await service.GetTable(collection).ReadAsync(query);
            }
            catch (InvalidOperationException ex)
            {
                Assert.AreEqual("The request could not be completed.  (YOU SHALL NOT PASS.)", ex.Message);
            }
        }

        public class Person
        {
            public long Id { get; set; }
            public string Name { get; set; }
        }

        [AsyncTestMethod]
        public async Task ReadAsync()
        {
            string appUrl = "http://www.test.com";
            string appKey = "secret...";
            string collection = "tests";
            string query = "$filter=id eq 12";
            var userDefinedParameters = new Dictionary<string, string>() { { "tags", "#pizza #beer" } };

            TestServiceFilter hijack = new TestServiceFilter();
            MobileServiceClient service = new MobileServiceClient(appUrl, appKey)
                .WithFilter(hijack);

            hijack.Response.Content =
                new JsonArray()
                    .Append(new JsonObject().Set("id", 12).Set("value", "test"))
                    .Stringify();
            IJsonValue response = await service.GetTable(collection).ReadAsync(query, userDefinedParameters);

            Assert.Contains(hijack.Request.Uri.ToString(), collection);
            Assert.Contains(hijack.Request.Uri.AbsoluteUri, "tags=%23pizza%20%23beer");
            Assert.Contains(hijack.Request.Uri.ToString(), query);

            ThrowsAsync<ArgumentNullException>(async () => await service.GetTable(null).ReadAsync(query));
            ThrowsAsync<ArgumentException>(async () => await service.GetTable("").ReadAsync(query));

            var invalidUserDefinedParameters = new Dictionary<string, string>() { { "$this is invalid", "since it starts with a '$'" } };
            ThrowsAsync<ArgumentException>(async () => await service.GetTable(collection).ReadAsync(query, invalidUserDefinedParameters));
        }

        [AsyncTestMethod]
        public async Task ReadAsyncGeneric()
        {
            string appUrl = "http://www.test.com";
            string appKey = "secret...";

            TestServiceFilter hijack = new TestServiceFilter();
            MobileServiceClient service = new MobileServiceClient(appUrl, appKey)
                .WithFilter(hijack);
            hijack.Response.Content =
                new JsonArray()
                    .Append(new JsonObject().Set("id", 12).Set("Name", "Bob"))
                    .Stringify();

            IMobileServiceTable<Person> table = service.GetTable<Person>();
            List<Person> people = await table.Where(p => p.Id == 12).ToListAsync();
            Assert.AreEqual(1, people.Count);
            Assert.AreEqual(12L, people[0].Id);
            Assert.AreEqual("Bob", people[0].Name);
        }

        [AsyncTestMethod]
        public async Task LookupAsync()
        {
            string appUrl = "http://www.test.com";
            string appKey = "secret...";
            var userDefinedParameters = new Dictionary<string, string>() { { "state", "CA"} };

            TestServiceFilter hijack = new TestServiceFilter();
            MobileServiceClient service = new MobileServiceClient(appUrl, appKey)
                .WithFilter(hijack);
            hijack.Response.Content =
                new JsonObject()
                    .Set("id", 12)
                    .Set("Name", "Bob")
                    .Stringify();

            IMobileServiceTable<Person> table = service.GetTable<Person>();
            Person bob = await table.LookupAsync(12, userDefinedParameters);
            Assert.Contains(hijack.Request.Uri.Query, "state=CA");
            Assert.AreEqual(12L, bob.Id);
            Assert.AreEqual("Bob", bob.Name);

            hijack.Response.StatusCode = 404;
            bool thrown = false;
            try
            {
                bob = await table.LookupAsync(12);
            }
            catch (InvalidOperationException)
            {
                thrown = true;
            }
            Assert.IsTrue(thrown, "Exception should be thrown on a 404!");
        }

        [AsyncTestMethod]
        public async Task InsertAsync()
        {
            string appUrl = "http://www.test.com";
            string appKey = "secret...";
            string collection = "tests";
            var userDefinedParameters = new Dictionary<string, string>() {{ "state", "AL" }};

            TestServiceFilter hijack = new TestServiceFilter();
            MobileServiceClient service = new MobileServiceClient(appUrl, appKey)
                .WithFilter(hijack);
                
            JsonObject obj = new JsonObject().Set("value", "new");
            hijack.Response.Content =
                new JsonObject().Set("id", 12).Set("value", "new").Stringify();
            await service.GetTable(collection).InsertAsync(obj, userDefinedParameters);

            Assert.AreEqual(12, obj.Get("id").AsInteger());
            Assert.Contains(hijack.Request.Uri.ToString(), collection);
            Assert.Contains(hijack.Request.Uri.Query, "state=AL");

            ThrowsAsync<ArgumentNullException>(
                async () => await service.GetTable(collection).InsertAsync(null));

            // Verify we throw if ID is set on both JSON and strongly typed
            // instances
            ThrowsAsync<ArgumentException>(
                async () => await service.GetTable(collection).InsertAsync(
                    new JsonObject().Set("id", 15)));
            ThrowsAsync<ArgumentException>(
                async () => await service.GetTable<Person>().InsertAsync(
                    new Person() { Id = 15 }));
        }

        [TestMethod]
        public void InsertAsyncThrowsIfIdExists()
        {
            string appUrl = "http://www.test.com";
            string collection = "tests";
            MobileServiceClient service = new MobileServiceClient(appUrl);

            // Verify we throw if ID is set on both JSON and strongly typed
            // instances
            ThrowsAsync<ArgumentException>(
                async () => await service.GetTable(collection).InsertAsync(
                    new JsonObject().Set("id", 15)));
            ThrowsAsync<ArgumentException>(
                async () => await service.GetTable<Person>().InsertAsync(
                    new Person() { Id = 15 }));
        }

        [AsyncTestMethod]
        public async Task UpdateAsync()
        {
            string appUrl = "http://www.test.com";
            string appKey = "secret...";
            string collection = "tests";
            var userDefinedParameters = new Dictionary<string, string>() { { "state", "FL" } };

            TestServiceFilter hijack = new TestServiceFilter();
            MobileServiceClient service = new MobileServiceClient(appUrl, appKey)
                .WithFilter(hijack);

            JsonObject obj = new JsonObject().Set("id", 12).Set("value", "new");
            hijack.Response.Content =
                new JsonObject()
                    .Set("id", 12)
                    .Set("value", "new")
                    .Set("other", "123")
                    .Stringify();
            IMobileServiceTable table = service.GetTable(collection);
            await table.UpdateAsync(obj, userDefinedParameters);

            Assert.AreEqual("123", obj.Get("other").AsString());
            Assert.Contains(hijack.Request.Uri.ToString(), collection);
            Assert.Contains(hijack.Request.Uri.Query, "state=FL");

            ThrowsAsync<ArgumentNullException>(async () => await table.UpdateAsync(null));
            ThrowsAsync<ArgumentException>(async () => await table.UpdateAsync(new JsonObject()));
        }

        [AsyncTestMethod]
        public async Task DeleteAsync()
        {
            string appUrl = "http://www.test.com";
            string appKey = "secret...";
            string collection = "tests";
            var userDefinedParameters = new Dictionary<string, string>() { { "state", "WY" } };

            TestServiceFilter hijack = new TestServiceFilter();
            MobileServiceClient service = new MobileServiceClient(appUrl, appKey)
                .WithFilter(hijack);

            JsonObject obj = new JsonObject().Set("id", 12).Set("value", "new");
            IMobileServiceTable table = service.GetTable(collection);
            await table.DeleteAsync(obj, userDefinedParameters);
                
            Assert.Contains(hijack.Request.Uri.ToString(), collection);
            Assert.IsNull(hijack.Request.Content);
            Assert.Contains(hijack.Request.Uri.Query, "state=WY");

            ThrowsAsync<ArgumentNullException>(async () => await table.DeleteAsync(null));
            ThrowsAsync<ArgumentException>(async () => await table.DeleteAsync(new JsonObject()));
        }
    }
}
