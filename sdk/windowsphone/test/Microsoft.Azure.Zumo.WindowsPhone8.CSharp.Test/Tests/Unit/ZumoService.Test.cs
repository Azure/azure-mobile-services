// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Zumo.WindowsPhone8.Test;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using System.IO.IsolatedStorage;

namespace Microsoft.Azure.Zumo.WindowsPhone8.CSharp.Test
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

            string settings = IsolatedStorageSettings.ApplicationSettings["MobileServices.Installation.config"] as string;
            string id = JValue.Parse(settings).Get("applicationInstallationId").AsString();
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
            hijack.Response.Content = new JArray().ToString();
            JToken response = await service.GetTable("foo").ReadAsync("bar");

            // Verify the filter was in the loop
            Assert.StartsWith(hijack.Request.Uri.ToString(), appUrl);

            Throws<ArgumentNullException>(() => service.WithFilter(null));            
        }

        [AsyncTestMethod]
        public async Task LoginAsync()
        {
            TestServiceFilter hijack = new TestServiceFilter();
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...")
                .WithFilter(hijack);

            // Send back a successful login response
            hijack.Response.Content =
                new JObject()
                    .Set("authenticationToken", "rhubarb")
                    .Set("user",
                        new JObject()
                            .Set("userId", "123456")).ToString();
            MobileServiceUser current = await service.LoginAsync("donkey");
                
            Assert.IsNotNull(current);
            Assert.AreEqual("123456", current.UserId);
            Assert.EndsWith(hijack.Request.Uri.ToString(), "login");
            string input = JToken.Parse(hijack.Request.Content).Get("authenticationToken").AsString();
            Assert.AreEqual("donkey", input);
            Assert.AreEqual("POST", hijack.Request.Method);
            Assert.AreSame(current, service.CurrentUser);

            // Verify that the user token is sent with each request
            JToken response = await service.GetTable("foo").ReadAsync("bar");
            Assert.AreEqual("rhubarb", hijack.Request.Headers["X-ZUMO-AUTH"]);
                
            // Verify error cases
            ThrowsAsync<ArgumentNullException>(async () => await service.LoginAsync(null));
            ThrowsAsync<ArgumentException>(async () => await service.LoginAsync(""));

            // Send back a failure and ensure it throws
            hijack.Response.Content =
                new JObject().Set("error", "login failed").ToString();
            hijack.Response.StatusCode = 401;
            ThrowsAsync<InvalidOperationException>(async () =>
            {
                current = await service.LoginAsync("donkey");
            });
        }

        [AsyncTestMethod]
        public async Task Logout()
        {
            TestServiceFilter hijack = new TestServiceFilter();
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...")
                .WithFilter(hijack);

            // Send back a successful login response
            hijack.Response.Content =
                new JObject()
                    .Set("authenticationToken", "rhubarb")
                    .Set("user",
                        new JObject()
                            .Set("userId", "123456")).ToString();
            MobileServiceUser current = await service.LoginAsync("donkey");
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

            hijack.Response.Content =
                new JArray()
                    .Append(new JObject().Set("id", 12).Set("value", "test"))
                    .ToString();
            JToken response = await service.GetTable(collection).ReadAsync(query);

            Assert.IsNotNull(hijack.Request.Headers["X-ZUMO-INSTALLATION-ID"]);
            Assert.AreEqual("secret...", hijack.Request.Headers["X-ZUMO-APPLICATION"]);
            Assert.AreEqual("application/json", hijack.Request.Accept);    
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
                new JObject()
                    .Set("error", "error message")
                    .Set("other", "donkey")
                    .ToString();
            hijack.Response.StatusCode = 401;
            hijack.Response.StatusDescription = "YOU SHALL NOT PASS.";
            try
            {
                JToken response = await service.GetTable(collection).ReadAsync(query);
            }
            catch (InvalidOperationException ex)
            {
                Assert.Equals(ex.Message, "error message");
            }

            // Verify all of the exception parameters
            hijack.Response.Content =
                new JObject()
                    .Set("error", "error message")
                    .Set("other", "donkey")
                    .ToString();
            hijack.Response.StatusCode = 401;
            hijack.Response.StatusDescription = "YOU SHALL NOT PASS.";
            try
            {
                JToken response = await service.GetTable(collection).ReadAsync(query);
            }
            catch (MobileServiceInvalidOperationException ex)
            {
                Assert.Equals(ex.Message, "error message");
                Assert.AreEqual((int)HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                Assert.Contains(ex.Response.Content, "donkey");
                Assert.StartsWith(ex.Request.Uri.ToString(), appUrl);
                Assert.AreEqual("YOU SHALL NOT PASS.", ex.Response.StatusDescription);
            }

            // If no error message in the response, we'll use the
            // StatusDescription instead
            hijack.Response.Content =
                new JObject()
                    .Set("other", "donkey")
                    .ToString();
            try
            {
                JToken response = await service.GetTable(collection).ReadAsync(query);
            }
            catch (InvalidOperationException ex)
            {
                Assert.Equals("The request could not be completed.  (YOU SHALL NOT PASS).", ex.Message);
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

            TestServiceFilter hijack = new TestServiceFilter();
            MobileServiceClient service = new MobileServiceClient(appUrl, appKey)
                .WithFilter(hijack);

            hijack.Response.Content =
                new JArray()
                    .Append(new JObject().Set("id", 12).Set("value", "test"))
                    .ToString();
            JToken response = await service.GetTable(collection).ReadAsync(query);

            Assert.Contains(hijack.Request.Uri.ToString(), collection);
            Assert.EndsWith(hijack.Request.Uri.ToString(), query);

            ThrowsAsync<ArgumentNullException>(async () => await service.GetTable(null).ReadAsync(query));
            ThrowsAsync<ArgumentException>(async () => await service.GetTable("").ReadAsync(query));
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
                new JArray()
                    .Append(new JObject().Set("id", 12).Set("Name", "Bob"))
                    .ToString();

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

            TestServiceFilter hijack = new TestServiceFilter();
            MobileServiceClient service = new MobileServiceClient(appUrl, appKey)
                .WithFilter(hijack);
            hijack.Response.Content =
                new JObject()
                    .Set("id", 12)
                    .Set("Name", "Bob")
                    .ToString();

            IMobileServiceTable<Person> table = service.GetTable<Person>();
            Person bob = await table.LookupAsync(12);
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

            TestServiceFilter hijack = new TestServiceFilter();
            MobileServiceClient service = new MobileServiceClient(appUrl, appKey)
                .WithFilter(hijack);
                
            JObject obj = new JObject().Set("value", "new") as JObject;
            hijack.Response.Content =
                new JObject().Set("id", 12).Set("value", "new").ToString();
            await service.GetTable(collection).InsertAsync(obj);

            Assert.AreEqual(12, obj.Get("id").AsInteger());
            Assert.Contains(hijack.Request.Uri.ToString(), collection);

            ThrowsAsync<ArgumentNullException>(
                async () => await service.GetTable(collection).InsertAsync(null));
            
            // Verify we throw if ID is set on both JSON and strongly typed
            // instances
            ThrowsAsync<ArgumentException>(
                async () => await service.GetTable(collection).InsertAsync(
                    new JObject().Set("id", 15) as JObject));
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
                    new JObject().Set("id", 15) as JObject));
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

            TestServiceFilter hijack = new TestServiceFilter();
            MobileServiceClient service = new MobileServiceClient(appUrl, appKey)
                .WithFilter(hijack);

            JObject obj = new JObject().Set("id", 12).Set("value", "new") as JObject;
            hijack.Response.Content =
                new JObject()
                    .Set("id", 12)
                    .Set("value", "new")
                    .Set("other", "123")
                    .ToString();
            IMobileServiceTable table = service.GetTable(collection);
            await table.UpdateAsync(obj);

            Assert.AreEqual("123", obj.Get("other").AsString());
            Assert.Contains(hijack.Request.Uri.ToString(), collection);

            ThrowsAsync<ArgumentNullException>(async () => await table.UpdateAsync(null));
            ThrowsAsync<ArgumentException>(async () => await table.UpdateAsync(new JObject()));
        }

        [AsyncTestMethod]
        public async Task DeleteAsync()
        {
            string appUrl = "http://www.test.com";
            string appKey = "secret...";
            string collection = "tests";

            TestServiceFilter hijack = new TestServiceFilter();
            MobileServiceClient service = new MobileServiceClient(appUrl, appKey)
                .WithFilter(hijack);

            JObject obj = new JObject().Set("id", 12).Set("value", "new") as JObject;
            IMobileServiceTable table = service.GetTable(collection);
            await table.DeleteAsync(obj);
                
            Assert.Contains(hijack.Request.Uri.ToString(), collection);

            ThrowsAsync<ArgumentNullException>(async () => await table.DeleteAsync(null));
            ThrowsAsync<ArgumentException>(async () => await table.DeleteAsync(new JObject()));
        }
    }
}
