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
    [Tag("table")]
    class MobileServiceTableTests : TestBase
    {
        [AsyncTestMethod]
        public async Task ReadAsync()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"Count\":1, People: [{\"Id\":\"12\", \"String\":\"Hey\"}] }");

            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable table = service.GetTable("tests");
            JToken people = await table.ReadAsync("$filter=id eq 12");

            Assert.Contains(hijack.Request.RequestUri.ToString(), "tests");
            Assert.Contains(hijack.Request.RequestUri.ToString(), "$filter=id eq 12");

            Assert.AreEqual(1, (int)people["Count"]);
            Assert.AreEqual(12, (int)people["People"][0]["Id"]);
            Assert.AreEqual("Hey", (string)people["People"][0]["String"]);
        }

        [AsyncTestMethod]
        public async Task ReadAsyncWithUserParameters()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"Count\":1, People: [{\"Id\":\"12\", \"String\":\"Hey\"}] }");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            var userDefinedParameters = new Dictionary<string, string>() { { "tags", "#pizza #beer" } };

            IMobileServiceTable table = service.GetTable("tests");

            JToken people = await table.ReadAsync("$filter=id eq 12", userDefinedParameters);

            Assert.Contains(hijack.Request.RequestUri.ToString(), "tests");
            Assert.Contains(hijack.Request.RequestUri.AbsoluteUri, "tags=%23pizza%20%23beer");
            Assert.Contains(hijack.Request.RequestUri.ToString(), "$filter=id eq 12");

            Assert.AreEqual(1, (int)people["Count"]);
            Assert.AreEqual(12, (int)people["People"][0]["Id"]);
            Assert.AreEqual("Hey", (string)people["People"][0]["String"]);
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
        public async Task LookupAsync()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            IMobileServiceTable table = service.GetTable("tests");

            hijack.SetResponseContent("{\"id\":12,\"String\":\"Hello\"}");

            JToken expected = await table.LookupAsync(12);

            Assert.AreEqual(12, (int)expected["id"]);
            Assert.AreEqual("Hello", (string)expected["String"]);
        }

        [AsyncTestMethod]
        public async Task LookupAsyncWithUserParameters()
        {
            var userDefinedParameters = new Dictionary<string, string>() { { "state", "CA" } };

            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            IMobileServiceTable table = service.GetTable("tests");

            hijack.SetResponseContent("{\"id\":12,\"String\":\"Hello\"}");

            JToken expected = await table.LookupAsync(12, userDefinedParameters);

            Assert.Contains(hijack.Request.RequestUri.Query, "state=CA");
            Assert.AreEqual(12, (int)expected["id"]);
            Assert.AreEqual("Hello", (string)expected["String"]);
        }

        [AsyncTestMethod]
        public async Task LookupAsyncThrowsWhenNotFound()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            IMobileServiceTable table = service.GetTable("tests");

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
        public async Task LookupAsyncThrowsWhenIdNull()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            IMobileServiceTable table = service.GetTable("tests");

            hijack.Response.StatusCode = HttpStatusCode.BadRequest;
            ArgumentException expected = null;
            try
            {
                await table.LookupAsync(null);
            }
            catch (ArgumentException e)
            {
                expected = e;
            }

            Assert.IsNotNull(expected);
        }

        [AsyncTestMethod]
        public async Task LookupAsyncThrowsWhenIdZero()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            IMobileServiceTable table = service.GetTable("tests");

            hijack.Response.StatusCode = HttpStatusCode.BadRequest;
            ArgumentException expected = null;
            try
            {
                await table.LookupAsync(0);
            }
            catch (ArgumentException e)
            {
                expected = e;
            }

            Assert.IsNotNull(expected);
        }

        [AsyncTestMethod]
        public async Task InsertAsync()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            IMobileServiceTable table = service.GetTable("tests");

            JObject obj = JToken.Parse("{\"value\":\"new\"}") as JObject;
            hijack.SetResponseContent("{\"id\":12,\"value\":\"new\"}");
            JToken newObj = await table.InsertAsync(obj);

            Assert.AreEqual(12, (int)newObj["id"]);
            Assert.AreNotEqual(newObj, obj);
            Assert.Contains(hijack.Request.RequestUri.ToString(), "tests");
        }

        [AsyncTestMethod]
        public async Task InsertAsyncWithUserParameters()
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
        public async Task InsertAsyncThrowsIfIDExists()
        {
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            IMobileServiceTable table = service.GetTable("tests");
            ArgumentException expected = null;

            JObject obj = JToken.Parse("{\"ID\":12}") as JObject;
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
        public async Task InsertAsyncOKWithIdZero()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            IMobileServiceTable table = service.GetTable("tests");
            JObject obj = JToken.Parse("{\"value\":\"new\", \"id\":0}") as JObject;
            hijack.SetResponseContent("{\"id\":12, \"value\":\"new\"}");

            JToken newObj = await table.InsertAsync(obj);

            Assert.AreEqual(12, (int)newObj["id"]);
            Assert.AreNotEqual(newObj, obj);
            Assert.Contains(hijack.Request.RequestUri.ToString(), "tests");
        }

        [AsyncTestMethod]
        public async Task UpdateAsync()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            IMobileServiceTable table = service.GetTable("tests");

            JObject obj = JToken.Parse("{\"id\":12,\"value\":\"new\"}") as JObject;
            hijack.SetResponseContent("{\"id\":12,\"value\":\"new\",\"other\":\"123\"}");
            JToken newObj = await table.UpdateAsync(obj);

            Assert.AreEqual("123", (string)newObj["other"]);
            Assert.AreNotEqual(newObj, obj);
            Assert.Contains(hijack.Request.RequestUri.ToString(), "tests");
        }

        [AsyncTestMethod]
        public async Task UpdateAsyncWithParameters()
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
        public async Task UpdateAsyncThrowsWhenIdIsZero()
        {
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            IMobileServiceTable table = service.GetTable("tests");
            ArgumentException expected = null;

            JObject obj = JToken.Parse("{\"id\":0}") as JObject;
            try
            {
                await table.UpdateAsync(obj);
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
            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            IMobileServiceTable table = service.GetTable("tests");

            JObject obj = JToken.Parse("{\"id\":12,\"value\":\"new\"}") as JObject;
            JToken token = await table.DeleteAsync(obj);

            Assert.IsNull(token);
            Assert.Contains(hijack.Request.RequestUri.ToString(), "tests");
            Assert.IsNull(hijack.Request.Content);
        }

        [AsyncTestMethod]
        public async Task DeleteAsyncWithParameters()
        {
            var userDefinedParameters = new Dictionary<string, string>() { { "state", "WY" } };

            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            IMobileServiceTable table = service.GetTable("tests");

            JObject obj = JToken.Parse("{\"id\":12,\"value\":\"new\"}") as JObject;
            JToken token = await table.DeleteAsync(obj, userDefinedParameters);

            Assert.IsNull(token);
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
        public async Task DeleteAsyncThrowsWhenIdIsZero()
        {
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            IMobileServiceTable table = service.GetTable("tests");
            ArgumentException expected = null;

            JObject obj = JToken.Parse("{\"id\":0}") as JObject;
            try
            {
                await table.DeleteAsync(obj);
            }
            catch (ArgumentException e)
            {
                expected = e;
            }

            Assert.IsNotNull(expected);
        }
    }
}
