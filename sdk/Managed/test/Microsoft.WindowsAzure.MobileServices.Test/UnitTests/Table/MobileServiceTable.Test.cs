// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.TestFramework;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    [Tag("unit")]
    [Tag("table")]
    public class MobileServiceTableTests : TestBase
    {
        #region Read Tests

        [AsyncTestMethod]
        public async Task ReadAsyncWithStringIdResponseContent()
        {
            string[] testIdData = IdTestData.ValidStringIds.Concat(
                                  IdTestData.EmptyStringIds).Concat(
                                  IdTestData.InvalidStringIds).ToArray();

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();

                // Make the testId JSON safe
                string jsonTestId = testId.Replace("\\", "\\\\").Replace("\"", "\\\"");

                hijack.SetResponseContent("[{\"id\":\"" + jsonTestId + "\",\"String\":\"Hey\"}]");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                JToken results = await table.ReadAsync("");
                JToken[] items = results.ToArray();

                Assert.AreEqual(1, items.Count());
                Assert.AreEqual(testId, (string)items[0]["id"]);
                Assert.AreEqual("Hey", (string)items[0]["String"]);
            }
        }

        [AsyncTestMethod]
        public async Task ReadAsyncWithIntIdResponseContent()
        {
            long[] testIdData = IdTestData.ValidIntIds.Concat(
                                 IdTestData.InvalidIntIds).ToArray();

            foreach (long testId in testIdData)
            {
                string stringTestId = testId.ToString();

                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("[{\"id\":" + stringTestId + ",\"String\":\"Hey\"}]");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                JToken results = await table.ReadAsync("");
                JToken[] items = results.ToArray();

                Assert.AreEqual(1, items.Count());
                Assert.AreEqual(testId, (long)items[0]["id"]);
                Assert.AreEqual("Hey", (string)items[0]["String"]);
            }
        }

        [AsyncTestMethod]
        public async Task ReadAsyncWithNonStringAndNonIntIdResponseContent()
        {
            object[] testIdData = IdTestData.NonStringNonIntValidJsonIds;

            foreach (object testId in testIdData)
            {
                string stringTestId = testId.ToString().ToLower();

                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("[{\"id\":" + stringTestId + ",\"String\":\"Hey\"}]");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                JToken results = await table.ReadAsync("");
                JToken[] items = results.ToArray();

                Assert.AreEqual(1, items.Count());
                Assert.AreEqual(testId, items[0]["id"].ToObject(testId.GetType()));
                Assert.AreEqual("Hey", (string)items[0]["String"]);
            }
        }

        [AsyncTestMethod]
        public async Task ReadAsyncWithNullIdResponseContent()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("[{\"id\":null,\"String\":\"Hey\"}]");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable table = service.GetTable("someTable");

            JToken results = await table.ReadAsync("");
            JToken[] items = results.ToArray();
            JObject item0 = items[0] as JObject;

            Assert.AreEqual(1, items.Count());
            Assert.AreEqual(null, (string)items[0]["id"]);
            Assert.AreEqual("Hey", (string)items[0]["String"]);
        }

        [AsyncTestMethod]
        public async Task ReadAsyncWithNoIdResponseContent()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("[{\"String\":\"Hey\"}]");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable table = service.GetTable("someTable");

            JToken results = await table.ReadAsync("");
            JToken[] items = results.ToArray();
            JObject item0 = items[0] as JObject;

            Assert.AreEqual(1, items.Count());
            Assert.IsFalse(item0.Properties().Any(p => p.Name.ToLowerInvariant() == "id"));
            Assert.AreEqual("Hey", (string)items[0]["String"]);
        }

        [AsyncTestMethod]
        public async Task ReadAsyncWithStringIdFilter()
        {
            string[] testIdData = IdTestData.ValidStringIds.Concat(
                                  IdTestData.EmptyStringIds).Concat(
                                  IdTestData.InvalidStringIds).ToArray();

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();

                // Make the testId JSON safe
                string jsonTestId = testId.Replace("\\", "\\\\").Replace("\"", "\\\"");

                hijack.SetResponseContent("[{\"id\":\"" + jsonTestId + "\",\"String\":\"Hey\"}]");

                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                string idForOdataQuery = Uri.EscapeDataString(testId.Replace("'", "''"));
                JToken results = await table.ReadAsync(string.Format("$filter=id eq '{0}'", idForOdataQuery));
                JToken[] items = results.ToArray();
                JObject item0 = items[0] as JObject;

                Uri expectedUri = new Uri(string.Format("http://www.test.com/tables/someTable?$filter=id eq '{0}'", idForOdataQuery));

                Assert.AreEqual(1, items.Count());
                Assert.AreEqual(testId, (string)items[0]["id"]);
                Assert.AreEqual("Hey", (string)items[0]["String"]);
                Assert.AreEqual(hijack.Request.RequestUri.AbsoluteUri, expectedUri.AbsoluteUri);
            }
        }

        [AsyncTestMethod]
        public async Task ReadAsyncWithNullIdFilter()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("[{\"id\":null,\"String\":\"Hey\"}]");

            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable table = service.GetTable("someTable");

            JToken results = await table.ReadAsync("$filter=id eq null");
            JToken[] items = results.ToArray();
            JObject item0 = items[0] as JObject;

            Uri expectedUri = new Uri("http://www.test.com/tables/someTable?$filter=id eq null");

            Assert.AreEqual(1, items.Count());
            Assert.AreEqual(null, (string)items[0]["id"]);
            Assert.AreEqual("Hey", (string)items[0]["String"]);
            Assert.AreEqual(hijack.Request.RequestUri.AbsoluteUri, expectedUri.AbsoluteUri);
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

        #endregion Read Tests

        #region Lookup Tests

        [AsyncTestMethod]
        public async Task LookupAsyncWithStringIdResponseContent()
        {
            string[] testIdData = IdTestData.ValidStringIds.Concat(
                                  IdTestData.EmptyStringIds).Concat(
                                  IdTestData.InvalidStringIds).ToArray();

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();

                // Make the testId JSON safe
                string jsonTestId = testId.Replace("\\", "\\\\").Replace("\"", "\\\"");

                hijack.SetResponseContent("{\"id\":\"" + jsonTestId + "\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                JObject item = await table.LookupAsync("id") as JObject;

                Assert.AreEqual(testId, (string)item["id"]);
                Assert.AreEqual("Hey", (string)item["String"]);
            }
        }

        [AsyncTestMethod]
        public async Task LookupAsyncWithIntIdResponseContent()
        {
            long[] testIdData = IdTestData.ValidIntIds.Concat(
                                 IdTestData.InvalidIntIds).ToArray();

            foreach (long testId in testIdData)
            {
                string stringTestId = testId.ToString();

                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":" + stringTestId + ",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                JObject item = await table.LookupAsync("id") as JObject;

                Assert.AreEqual(testId, (long)item["id"]);
                Assert.AreEqual("Hey", (string)item["String"]);
            }
        }

        [AsyncTestMethod]
        public async Task LookupAsyncWithNonStringAndNonIntIdResponseContent()
        {
            object[] testIdData = IdTestData.NonStringNonIntValidJsonIds;

            foreach (object testId in testIdData)
            {
                string stringTestId = testId.ToString().ToLower();

                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":" + stringTestId + ",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                JObject item = await table.LookupAsync("id") as JObject;

                object value = item["id"].ToObject(testId.GetType());

                Assert.AreEqual(testId, value);
                Assert.AreEqual("Hey", (string)item["String"]);
            }
        }

        [AsyncTestMethod]
        public async Task LookupAsyncWithNullIdResponseContent()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":null,\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable table = service.GetTable("someTable");

            JObject item = await table.LookupAsync("id") as JObject;

            Assert.AreEqual(null, (string)item["id"]);
            Assert.AreEqual("Hey", (string)item["String"]);
        }

        [AsyncTestMethod]
        public async Task LookupAsyncWithNoIdResponseContent()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable table = service.GetTable("someTable");

            JObject item = await table.LookupAsync("id") as JObject;

            Assert.IsFalse(item.Properties().Any(p => p.Name.ToLowerInvariant() == "id"));
            Assert.AreEqual("Hey", (string)item["String"]);
        }

        [AsyncTestMethod]
        public async Task LookupAsyncWithStringIdParameter()
        {
            string[] testIdData = IdTestData.ValidStringIds;

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"" + testId + "\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                JToken item = await table.LookupAsync(testId);

                Uri expectedUri = new Uri(string.Format("http://www.test.com/tables/someTable/{0}", Uri.EscapeDataString(testId)));

                Assert.AreEqual(testId, (string)item["id"]);
                Assert.AreEqual("Hey", (string)item["String"]);
                Assert.AreEqual(hijack.Request.RequestUri.AbsoluteUri, expectedUri.AbsoluteUri);
            }
        }

        [AsyncTestMethod]
        public async Task LookupAsyncWithInvalidStringIdParameter()
        {
            string[] testIdData = IdTestData.EmptyStringIds.Concat(
                                  IdTestData.InvalidStringIds).ToArray();

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"" + testId + "\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable table = service.GetTable("someTable");
                Exception exception = null;
                try
                {
                    JToken item = await table.LookupAsync(testId);
                }
                catch (Exception e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.IsTrue(exception.Message.Contains("The id can not be null or an empty string.") ||
                              exception.Message.Contains("An id must not contain any control characters or the characters") ||
                              exception.Message.Contains("is longer than the max string id length of 255 characters"));
            }
        }

        [AsyncTestMethod]
        public async Task LookupAsyncWithIntIdParameter()
        {
            long[] testIdData = IdTestData.ValidIntIds;

            foreach (long testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":" + testId.ToString() + ",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                JToken item = await table.LookupAsync(testId);

                Uri expectedUri = new Uri(string.Format("http://www.test.com/tables/someTable/{0}", testId));

                Assert.AreEqual(testId, (long)item["id"]);
                Assert.AreEqual("Hey", (string)item["String"]);
                Assert.AreEqual(hijack.Request.RequestUri.AbsoluteUri, expectedUri.AbsoluteUri);
            }
        }

        [AsyncTestMethod]
        public async Task LookupAsyncWithNullIdParameter()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":null,\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable table = service.GetTable("someTable");

            Exception exception = null;
            try
            {
                JToken item = await table.LookupAsync(null);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.IsTrue(exception.Message.Contains("The id can not be null or an empty string."));
        }

        [AsyncTestMethod]
        public async Task LookupAsyncWithZeroIdParameter()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":null,\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable table = service.GetTable("someTable");

            Exception exception = null;
            try
            {
                JToken item = await table.LookupAsync(0L);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.IsTrue(exception.Message.Contains("Specified argument was out of the range of valid values") ||
                          exception.Message.Contains(" is not a positive integer value"));
        }

        [AsyncTestMethod]
        public async Task LookupAsyncWithUserParameters()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"Count\":1, People: [{\"Id\":\"12\", \"String\":\"Hey\"}] }");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            var userDefinedParameters = new Dictionary<string, string>() { { "tags", "#pizza #beer" } };

            IMobileServiceTable table = service.GetTable("tests");

            JToken people = await table.LookupAsync("id ", userDefinedParameters);

            Assert.Contains(hijack.Request.RequestUri.ToString(), "tests");
            Assert.Contains(hijack.Request.RequestUri.AbsoluteUri, "tags=%23pizza%20%23beer");

            Assert.AreEqual(1, (int)people["Count"]);
            Assert.AreEqual(12, (int)people["People"][0]["Id"]);
            Assert.AreEqual("Hey", (string)people["People"][0]["String"]);
        }

        [AsyncTestMethod]
        public async Task LookupAsyncThrowsWithInvalidUserParameters()
        {
            var invalidUserDefinedParameters = new Dictionary<string, string>() { { "$this is invalid", "since it starts with a '$'" } };
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            IMobileServiceTable table = service.GetTable("test");
            ArgumentException expected = null;

            try
            {
                await table.LookupAsync("$filter=id eq 12", invalidUserDefinedParameters);
            }
            catch (ArgumentException e)
            {
                expected = e;
            }

            Assert.IsNotNull(expected);
        }

        #endregion Lookup Tests

        #region Insert Tests

        [AsyncTestMethod]
        public async Task InsertAsyncWithStringIdResponseContent()
        {
            string[] testIdData = IdTestData.ValidStringIds.Concat(
                                  IdTestData.EmptyStringIds).Concat(
                                  IdTestData.InvalidStringIds).ToArray();

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();

                // Make the testId JSON safe
                string jsonTestId = testId.Replace("\\", "\\\\").Replace("\"", "\\\"");

                hijack.SetResponseContent("{\"id\":\"" + jsonTestId + "\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                JObject obj = JToken.Parse("{\"value\":\"new\"}") as JObject;
                JObject item = await table.InsertAsync(obj) as JObject;

                Assert.AreEqual(testId, (string)item["id"]);
                Assert.AreEqual("Hey", (string)item["String"]);
            }
        }

        [AsyncTestMethod]
        public async Task InsertAsyncWithIntIdResponseContent()
        {
            long[] testIdData = IdTestData.ValidIntIds.Concat(
                                 IdTestData.InvalidIntIds).ToArray();

            foreach (long testId in testIdData)
            {
                string stringTestId = testId.ToString();

                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":" + stringTestId + ",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                JObject obj = JToken.Parse("{\"value\":\"new\"}") as JObject;
                JObject item = await table.InsertAsync(obj) as JObject;

                Assert.AreEqual(testId, (long)item["id"]);
                Assert.AreEqual("Hey", (string)item["String"]);
            }
        }

        [AsyncTestMethod]
        public async Task InsertAsyncWithNonStringAndNonIntIdResponseContent()
        {
            object[] testIdData = IdTestData.NonStringNonIntValidJsonIds;

            foreach (object testId in testIdData)
            {
                string stringTestId = testId.ToString().ToLower();

                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":" + stringTestId + ",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                JObject obj = JToken.Parse("{\"value\":\"new\"}") as JObject;
                JObject item = await table.InsertAsync(obj) as JObject;

                object value = item["id"].ToObject(testId.GetType());

                Assert.AreEqual(testId, value);
                Assert.AreEqual("Hey", (string)item["String"]);
            }
        }

        [AsyncTestMethod]
        public async Task InsertAsyncWithNullIdResponseContent()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":null,\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable table = service.GetTable("someTable");

            JObject obj = JToken.Parse("{\"value\":\"new\"}") as JObject;
            JObject item = await table.InsertAsync(obj) as JObject;

            Assert.AreEqual(null, (string)item["id"]);
            Assert.AreEqual("Hey", (string)item["String"]);
        }

        [AsyncTestMethod]
        public async Task InsertAsyncWithNoIdResponseContent()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable table = service.GetTable("someTable");

            JObject obj = JToken.Parse("{\"value\":\"new\"}") as JObject;
            JObject item = await table.InsertAsync(obj) as JObject;

            Assert.IsFalse(item.Properties().Any(p => p.Name.ToLowerInvariant() == "id"));
            Assert.AreEqual("Hey", (string)item["String"]);
        }

        [AsyncTestMethod]
        public async Task InsertAsyncWithStringIdItem()
        {
            string[] testIdData = IdTestData.ValidStringIds;

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"" + testId + "\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                JObject obj = JToken.Parse("{\"id\":\"" + testId + "\",\"String\":\"what?\"}") as JObject;
                JToken item = await table.InsertAsync(obj);

                Assert.AreEqual(testId, (string)item["id"]);
                Assert.AreEqual("Hey", (string)item["String"]);
            }
        }

        [AsyncTestMethod]
        public async Task InsertAsyncWithEmptyStringIdItem()
        {
            string[] testIdData = IdTestData.EmptyStringIds;

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"an id\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                JObject obj = JToken.Parse("{\"id\":\"" + testId + "\",\"String\":\"what?\"}") as JObject;
                JToken item = await table.InsertAsync(obj);

                Assert.AreEqual("an id", (string)item["id"]);
                Assert.AreEqual("Hey", (string)item["String"]);
            }
        }

        [AsyncTestMethod]
        public async Task InsertAsyncWithInvalidStringIdItem()
        {
            string[] testIdData = IdTestData.InvalidStringIds;

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();

                // Make the testId JSON safe
                string jsonTestId = testId.Replace("\\", "\\\\").Replace("\"", "\\\"");

                hijack.SetResponseContent("{\"id\":\"" + jsonTestId + "\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable table = service.GetTable("someTable");
                Exception exception = null;
                try
                {
                    JObject obj = JToken.Parse("{\"id\":\"" + jsonTestId + "\",\"String\":\"what?\"}") as JObject;
                    JToken item = await table.InsertAsync(obj);
                }
                catch (Exception e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.IsTrue(exception.Message.Contains("is longer than the max string id length of 255 characters") ||
                             exception.Message.Contains("An id must not contain any control characters or the characters"));
            }
        }

        [AsyncTestMethod]
        public async Task InsertAsyncWithIntIdItem()
        {
            long[] testIdData = IdTestData.ValidIntIds;

            foreach (long testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":" + testId.ToString() + ",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable table = service.GetTable("someTable");
                Exception exception = null;

                try
                {
                    JObject obj = JToken.Parse("{\"id\":" + testId.ToString() + ",\"String\":\"what?\"}") as JObject;
                    JToken item = await table.InsertAsync(obj);
                }
                catch (Exception e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.IsTrue(exception.Message.Contains("Cannot insert if the id member is already set.") ||
                              exception.Message.Contains("for member id is outside the valid range for numeric columns"));
            }
        }

        [AsyncTestMethod]
        public async Task InsertAsyncWithNullIdItem()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":5,\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable table = service.GetTable("someTable");

            JObject obj = JToken.Parse("{\"id\":null,\"String\":\"what?\"}") as JObject;
            JToken item = await table.InsertAsync(obj);

            Assert.AreEqual(5L, (long)item["id"]);
            Assert.AreEqual("Hey", (string)item["String"]);
        }

        [AsyncTestMethod]
        public async Task InsertAsyncWithNoIdItem()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":5,\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable table = service.GetTable("someTable");

            JObject obj = JToken.Parse("{\"String\":\"what?\"}") as JObject;
            JToken item = await table.InsertAsync(obj);

            Assert.AreEqual(5L, (long)item["id"]);
            Assert.AreEqual("Hey", (string)item["String"]);
        }

        [AsyncTestMethod]
        public async Task InsertAsyncWithZeroIdItem()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":5,\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable table = service.GetTable("someTable");

            JObject obj = JToken.Parse("{\"id\":0,\"String\":\"what?\"}") as JObject;
            JToken item = await table.InsertAsync(obj);

            Assert.AreEqual(5L, (long)item["id"]);
            Assert.AreEqual("Hey", (string)item["String"]);
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
        public async Task InsertAsyncThrowsWhenIdIsID()
        {
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            IMobileServiceTable table = service.GetTable("tests");
            ArgumentException expected = null;

            JObject obj = JToken.Parse("{\"ID\":\"an id\"}") as JObject;
            try
            {
                await table.InsertAsync(obj);
            }
            catch (ArgumentException e)
            {
                expected = e;
            }

            Assert.IsNotNull(expected);
            Assert.IsTrue(expected.Message.Contains("The casing of the 'id' property is invalid."));
        }

        [AsyncTestMethod]
        public async Task InsertAsyncThrowsWhenIdIsId()
        {
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            IMobileServiceTable table = service.GetTable("tests");
            ArgumentException expected = null;

            JObject obj = JToken.Parse("{\"Id\":\"an id\"}") as JObject;
            try
            {
                await table.InsertAsync(obj);
            }
            catch (ArgumentException e)
            {
                expected = e;
            }

            Assert.IsNotNull(expected);
            Assert.IsTrue(expected.Message.Contains("The casing of the 'id' property is invalid."));
        }

        [AsyncTestMethod]
        public async Task InsertAsyncThrowsWhenIdIsiD()
        {
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            IMobileServiceTable table = service.GetTable("tests");
            ArgumentException expected = null;

            JObject obj = JToken.Parse("{\"iD\":\"an id\"}") as JObject;
            try
            {
                await table.InsertAsync(obj);
            }
            catch (ArgumentException e)
            {
                expected = e;
            }

            Assert.IsNotNull(expected);
            Assert.IsTrue(expected.Message.Contains("The casing of the 'id' property is invalid."));
        }

        #endregion Insert Tests

        #region Update Tests

        [AsyncTestMethod]
        public async Task UpdateAsyncWithStringIdResponseContent()
        {
            string[] testIdData = IdTestData.ValidStringIds.Concat(
                                  IdTestData.EmptyStringIds).Concat(
                                  IdTestData.InvalidStringIds).ToArray();

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();

                // Make the testId JSON safe
                string jsonTestId = testId.Replace("\\", "\\\\").Replace("\"", "\\\"");

                hijack.SetResponseContent("{\"id\":\"" + jsonTestId + "\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                JObject obj = JToken.Parse("{\"id\":\"id\",\"value\":\"new\"}") as JObject;
                JObject item = await table.UpdateAsync(obj) as JObject;

                Assert.AreEqual(testId, (string)item["id"]);
                Assert.AreEqual("Hey", (string)item["String"]);
            }
        }

        [AsyncTestMethod]
        public async Task UpdateAsyncWithIntIdResponseContent()
        {
            long[] testIdData = IdTestData.ValidIntIds.Concat(
                                 IdTestData.InvalidIntIds).ToArray();

            foreach (long testId in testIdData)
            {
                string stringTestId = testId.ToString();

                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":" + stringTestId + ",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                JObject obj = JToken.Parse("{\"id\":\"id\",\"value\":\"new\"}") as JObject;
                JObject item = await table.UpdateAsync(obj) as JObject;

                Assert.AreEqual(testId, (long)item["id"]);
                Assert.AreEqual("Hey", (string)item["String"]);
            }
        }

        [AsyncTestMethod]
        public async Task UpdateAsyncWithNonStringAndNonIntIdResponseContent()
        {
            object[] testIdData = IdTestData.NonStringNonIntValidJsonIds;

            foreach (object testId in testIdData)
            {
                string stringTestId = testId.ToString().ToLower();

                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":" + stringTestId + ",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                JObject obj = JToken.Parse("{\"id\":\"id\",\"value\":\"new\"}") as JObject;
                JObject item = await table.UpdateAsync(obj) as JObject;

                object value = item["id"].ToObject(testId.GetType());

                Assert.AreEqual(testId, value);
                Assert.AreEqual("Hey", (string)item["String"]);
            }
        }

        [AsyncTestMethod]
        public async Task UpdateAsyncWithNullIdResponseContent()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":null,\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable table = service.GetTable("someTable");

            JObject obj = JToken.Parse("{\"id\":\"id\",\"value\":\"new\"}") as JObject;
            JObject item = await table.UpdateAsync(obj) as JObject;

            Assert.AreEqual(null, (string)item["id"]);
            Assert.AreEqual("Hey", (string)item["String"]);
        }

        [AsyncTestMethod]
        public async Task UpdateAsyncWithNoIdResponseContent()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable table = service.GetTable("someTable");

            JObject obj = JToken.Parse("{\"id\":\"id\",\"value\":\"new\"}") as JObject;
            JObject item = await table.UpdateAsync(obj) as JObject;

            Assert.IsFalse(item.Properties().Any(p => p.Name.ToLowerInvariant() == "id"));
            Assert.AreEqual("Hey", (string)item["String"]);
        }

        [AsyncTestMethod]
        public async Task UpdateAsyncWithStringIdItem()
        {
            string[] testIdData = IdTestData.ValidStringIds;

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"" + testId + "\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                JObject obj = JToken.Parse("{\"id\":\"" + testId + "\",\"String\":\"what?\"}") as JObject;
                JToken item = await table.UpdateAsync(obj);

                Assert.AreEqual(testId, (string)item["id"]);
                Assert.AreEqual("Hey", (string)item["String"]);
            }
        }

        [AsyncTestMethod]
        public async Task UpdateAsyncWithEmptyStringIdItem()
        {
            string[] testIdData = IdTestData.EmptyStringIds;

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"an id\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable table = service.GetTable("someTable");
                Exception exception = null;
                try
                {
                    JObject obj = JToken.Parse("{\"id\":\"" + testId + "\",\"String\":\"what?\"}") as JObject;
                    JToken item = await table.UpdateAsync(obj);
                }
                catch (Exception e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.IsTrue(exception.Message.Contains("The id can not be null or an empty string."));
            }
        }

        [AsyncTestMethod]
        public async Task UpdateAsyncWithInvalidStringIdItem()
        {
            string[] testIdData = IdTestData.InvalidStringIds;

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();

                // Make the testId JSON safe
                string jsonTestId = testId.Replace("\\", "\\\\").Replace("\"", "\\\"");

                hijack.SetResponseContent("{\"id\":\"" + jsonTestId + "\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable table = service.GetTable("someTable");
                Exception exception = null;
                try
                {
                    JObject obj = JToken.Parse("{\"id\":\"" + jsonTestId + "\",\"String\":\"what?\"}") as JObject;
                    JToken item = await table.UpdateAsync(obj);
                }
                catch (Exception e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.IsTrue(exception.Message.Contains("is longer than the max string id length of 255 characters") ||
                              exception.Message.Contains("An id must not contain any control characters or the characters"));
            }
        }

        [AsyncTestMethod]
        public async Task UpdateAsyncWithIntIdItem()
        {
            long[] testIdData = IdTestData.ValidIntIds;

            foreach (long testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":" + testId.ToString() + ",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable table = service.GetTable("someTable");
                JObject obj = JToken.Parse("{\"id\":" + testId.ToString() + ",\"String\":\"what?\"}") as JObject;
                JToken item = await table.UpdateAsync(obj);

                Assert.AreEqual(testId, (long)item["id"]);
                Assert.AreEqual("Hey", (string)item["String"]);
            }
        }

        [AsyncTestMethod]
        public async Task UpdateAsyncWithNullIdItem()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":5,\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable table = service.GetTable("someTable");
            Exception exception = null;
            try
            {
                JObject obj = JToken.Parse("{\"id\":null,\"String\":\"what?\"}") as JObject;
                JToken item = await table.UpdateAsync(obj);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.IsTrue(exception.Message.Contains("The id can not be null or an empty string"));
        }

        [AsyncTestMethod]
        public async Task UpdateAsyncWithNoIdItem()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":5,\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable table = service.GetTable("someTable");
            Exception exception = null;
            try
            {
                JObject obj = JToken.Parse("{\"String\":\"what?\"}") as JObject;
                JToken item = await table.UpdateAsync(obj);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.IsTrue(exception.Message.Contains("Expected id member not found."));
        }

        [AsyncTestMethod]
        public async Task UpdateAsyncWithZeroIdItem()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":5,\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable table = service.GetTable("someTable");
            Exception exception = null;
            try
            {
                JObject obj = JToken.Parse("{\"id\":0,\"String\":\"what?\"}") as JObject;
                JToken item = await table.UpdateAsync(obj);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.IsTrue(exception.Message.Contains("The integer id '0' is not a positive integer value."));
        }

        [AsyncTestMethod]
        public async Task UpdateAsyncWithUserParameters()
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
            Assert.Contains(hijack.Request.RequestUri.ToString(), "tests/12");
            Assert.Contains(hijack.Request.RequestUri.Query, "state=FL");
        }

        [AsyncTestMethod]
        public async Task UpdateAsyncThrowsWhenIdIsID()
        {
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            IMobileServiceTable table = service.GetTable("tests");
            ArgumentException expected = null;

            JObject obj = JToken.Parse("{\"ID\":5}") as JObject;
            try
            {
                await table.UpdateAsync(obj);
            }
            catch (ArgumentException e)
            {
                expected = e;
            }

            Assert.IsNotNull(expected);
            Assert.IsTrue(expected.Message.Contains("The casing of the 'id' property is invalid."));
        }

        [AsyncTestMethod]
        public async Task UpdateAsyncThrowsWhenIdIsId()
        {
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            IMobileServiceTable table = service.GetTable("tests");
            ArgumentException expected = null;

            JObject obj = JToken.Parse("{\"Id\":5}") as JObject;
            try
            {
                await table.UpdateAsync(obj);
            }
            catch (ArgumentException e)
            {
                expected = e;
            }

            Assert.IsNotNull(expected);
            Assert.IsTrue(expected.Message.Contains("The casing of the 'id' property is invalid."));
        }

        [AsyncTestMethod]
        public async Task UpdateAsyncThrowsWhenIdIsiD()
        {
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            IMobileServiceTable table = service.GetTable("tests");
            ArgumentException expected = null;

            JObject obj = JToken.Parse("{\"iD\":5}") as JObject;
            try
            {
                await table.UpdateAsync(obj);
            }
            catch (ArgumentException e)
            {
                expected = e;
            }

            Assert.IsNotNull(expected);
            Assert.IsTrue(expected.Message.Contains("The casing of the 'id' property is invalid."));
        }

        #endregion Update Tests

        #region Delete Tests

        [AsyncTestMethod]
        public async Task DeleteAsyncWithStringIdResponseContent()
        {
            string[] testIdData = IdTestData.ValidStringIds.Concat(
                                  IdTestData.EmptyStringIds).Concat(
                                  IdTestData.InvalidStringIds).ToArray();

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();

                // Make the testId JSON safe
                string jsonTestId = testId.Replace("\\", "\\\\").Replace("\"", "\\\"");

                hijack.SetResponseContent("{\"id\":\"" + jsonTestId + "\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                JObject obj = JToken.Parse("{\"id\":\"id\",\"value\":\"new\"}") as JObject;
                JObject item = await table.DeleteAsync(obj) as JObject;

                Assert.AreEqual(testId, (string)item["id"]);
                Assert.AreEqual("Hey", (string)item["String"]);
            }
        }

        [AsyncTestMethod]
        public async Task DeleteAsyncWithIntIdResponseContent()
        {
            long[] testIdData = IdTestData.ValidIntIds.Concat(
                                 IdTestData.InvalidIntIds).ToArray();

            foreach (long testId in testIdData)
            {
                string stringTestId = testId.ToString();

                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":" + stringTestId + ",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                JObject obj = JToken.Parse("{\"id\":\"id\",\"value\":\"new\"}") as JObject;
                JObject item = await table.DeleteAsync(obj) as JObject;

                Assert.AreEqual(testId, (long)item["id"]);
                Assert.AreEqual("Hey", (string)item["String"]);
            }
        }

        [AsyncTestMethod]
        public async Task DeleteAsyncWithNonStringAndNonIntIdResponseContent()
        {
            object[] testIdData = IdTestData.NonStringNonIntValidJsonIds;

            foreach (object testId in testIdData)
            {
                string stringTestId = testId.ToString().ToLower();

                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":" + stringTestId + ",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                JObject obj = JToken.Parse("{\"id\":\"id\",\"value\":\"new\"}") as JObject;
                JObject item = await table.DeleteAsync(obj) as JObject;

                object value = item["id"].ToObject(testId.GetType());

                Assert.AreEqual(testId, value);
                Assert.AreEqual("Hey", (string)item["String"]);
            }
        }

        [AsyncTestMethod]
        public async Task DeleteAsyncWithNullIdResponseContent()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":null,\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable table = service.GetTable("someTable");

            JObject obj = JToken.Parse("{\"id\":\"id\",\"value\":\"new\"}") as JObject;
            JObject item = await table.DeleteAsync(obj) as JObject;

            Assert.AreEqual(null, (string)item["id"]);
            Assert.AreEqual("Hey", (string)item["String"]);
        }

        [AsyncTestMethod]
        public async Task DeleteAsyncWithNoIdResponseContent()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable table = service.GetTable("someTable");

            JObject obj = JToken.Parse("{\"id\":\"id\",\"value\":\"new\"}") as JObject;
            JObject item = await table.DeleteAsync(obj) as JObject;

            Assert.IsFalse(item.Properties().Any(p => p.Name.ToLowerInvariant() == "id"));
            Assert.AreEqual("Hey", (string)item["String"]);
        }

        [AsyncTestMethod]
        public async Task DeleteAsyncWithStringIdItem()
        {
            string[] testIdData = IdTestData.ValidStringIds;

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"" + testId + "\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                JObject obj = JToken.Parse("{\"id\":\"" + testId + "\",\"String\":\"what?\"}") as JObject;
                JToken item = await table.DeleteAsync(obj);

                Assert.AreEqual(testId, (string)item["id"]);
                Assert.AreEqual("Hey", (string)item["String"]);
            }
        }

        [AsyncTestMethod]
        public async Task DeleteAsyncWithEmptyStringIdItem()
        {
            string[] testIdData = IdTestData.EmptyStringIds;

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"an id\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable table = service.GetTable("someTable");
                Exception exception = null;
                try
                {
                    JObject obj = JToken.Parse("{\"id\":\"" + testId + "\",\"String\":\"what?\"}") as JObject;
                    JToken item = await table.DeleteAsync(obj);
                }
                catch (Exception e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.IsTrue(exception.Message.Contains("The id can not be null or an empty string."));
            }
        }

        [AsyncTestMethod]
        public async Task DeleteAsyncWithInvalidStringIdItem()
        {
            string[] testIdData = IdTestData.InvalidStringIds;

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();

                // Make the testId JSON safe
                string jsonTestId = testId.Replace("\\", "\\\\").Replace("\"", "\\\"");

                hijack.SetResponseContent("{\"id\":\"" + jsonTestId + "\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable table = service.GetTable("someTable");
                Exception exception = null;
                try
                {
                    JObject obj = JToken.Parse("{\"id\":\"" + jsonTestId + "\",\"String\":\"what?\"}") as JObject;
                    JToken item = await table.DeleteAsync(obj);
                }
                catch (Exception e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.IsTrue(exception.Message.Contains("is longer than the max string id length of 255 characters") ||
                              exception.Message.Contains("An id must not contain any control characters or the characters"));
            }
        }

        [AsyncTestMethod]
        public async Task DeleteAsyncWithIntIdItem()
        {
            long[] testIdData = IdTestData.ValidIntIds;

            foreach (long testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":" + testId.ToString() + ",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable table = service.GetTable("someTable");
                JObject obj = JToken.Parse("{\"id\":" + testId.ToString() + ",\"String\":\"what?\"}") as JObject;
                JToken item = await table.DeleteAsync(obj);

                Assert.AreEqual(testId, (long)item["id"]);
                Assert.AreEqual("Hey", (string)item["String"]);
            }
        }

        [AsyncTestMethod]
        public async Task DeleteAsyncWithNullIdItem()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":5,\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable table = service.GetTable("someTable");
            Exception exception = null;
            try
            {
                JObject obj = JToken.Parse("{\"id\":null,\"String\":\"what?\"}") as JObject;
                JToken item = await table.DeleteAsync(obj);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.IsTrue(exception.Message.Contains("The id can not be null or an empty string."));
        }

        [AsyncTestMethod]
        public async Task DeleteAsyncWithNoIdItem()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":5,\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable table = service.GetTable("someTable");
            Exception exception = null;
            try
            {
                JObject obj = JToken.Parse("{\"String\":\"what?\"}") as JObject;
                JToken item = await table.DeleteAsync(obj);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.IsTrue(exception.Message.Contains("Expected id member not found."));
        }

        [AsyncTestMethod]
        public async Task DeleteAsyncWithZeroIdItem()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":5,\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable table = service.GetTable("someTable");
            Exception exception = null;
            try
            {
                JObject obj = JToken.Parse("{\"id\":0,\"String\":\"what?\"}") as JObject;
                JToken item = await table.DeleteAsync(obj);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.IsTrue(exception.Message.Contains("The integer id '0' is not a positive integer value"));
        }

        [AsyncTestMethod]
        public async Task DeleteAsyncWithUserParameters()
        {
            var userDefinedParameters = new Dictionary<string, string>() { { "state", "FL" } };

            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            IMobileServiceTable table = service.GetTable("tests");

            JObject obj = JToken.Parse("{\"id\":12,\"value\":\"new\"}") as JObject;
            hijack.SetResponseContent("{\"id\":12,\"value\":\"new\",\"other\":\"123\"}");
            JToken newObj = await table.DeleteAsync(obj, userDefinedParameters);

            Assert.AreEqual("123", (string)newObj["other"]);
            Assert.AreNotEqual(newObj, obj);
            Assert.Contains(hijack.Request.RequestUri.ToString(), "tests/12");
            Assert.Contains(hijack.Request.RequestUri.Query, "state=FL");
        }

        [AsyncTestMethod]
        public async Task DeleteAsyncThrowsWhenIdIsID()
        {
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            IMobileServiceTable table = service.GetTable("tests");
            ArgumentException expected = null;

            JObject obj = JToken.Parse("{\"ID\":5}") as JObject;
            try
            {
                await table.DeleteAsync(obj);
            }
            catch (ArgumentException e)
            {
                expected = e;
            }

            Assert.IsNotNull(expected);
            Assert.IsTrue(expected.Message.Contains("The casing of the 'id' property is invalid."));
        }

        [AsyncTestMethod]
        public async Task DeleteAsyncThrowsWhenIdIsId()
        {
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            IMobileServiceTable table = service.GetTable("tests");
            ArgumentException expected = null;

            JObject obj = JToken.Parse("{\"Id\":5}") as JObject;
            try
            {
                await table.DeleteAsync(obj);
            }
            catch (ArgumentException e)
            {
                expected = e;
            }

            Assert.IsNotNull(expected);
            Assert.IsTrue(expected.Message.Contains("The casing of the 'id' property is invalid."));
        }

        [AsyncTestMethod]
        public async Task DeleteAsyncThrowsWhenIdIsiD()
        {
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            IMobileServiceTable table = service.GetTable("tests");
            ArgumentException expected = null;

            JObject obj = JToken.Parse("{\"iD\":5}") as JObject;
            try
            {
                await table.DeleteAsync(obj);
            }
            catch (ArgumentException e)
            {
                expected = e;
            }

            Assert.IsNotNull(expected);
            Assert.IsTrue(expected.Message.Contains("The casing of the 'id' property is invalid."));
        }

        #endregion Delete Tests

        #region System Property Tests

        [AsyncTestMethod]
        public async Task InsertAsync_RemovesSystemProperties_WhenIdIsString_Generic()
        {
            string[] testSystemProperties = SystemPropertiesTestData.ValidSystemProperties;

            foreach (string testSystemProperty in testSystemProperties)
            {                
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"an id\",\"String\":\"Hey\"}");
                var service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
                hijack.OnSendingRequest = async (request) =>
                {
                    string content = await request.Content.ReadAsStringAsync();
                    JObject obj = JToken.Parse(content) as JObject;
                    Assert.IsTrue(obj.Properties().Where(p => p.Name == "id").Any());
                    Assert.IsTrue(obj.Properties().Where(p => p.Name == "String").Any());
                    Assert.IsFalse(obj.Properties().Where(p => p.Name == testSystemProperty).Any());
                    return request;
                };
                var table = service.GetTable<ToDoWithSystemPropertiesType>();

                JObject itemToInsert = JToken.Parse("{\"id\":\"an id\",\"String\":\"what?\",\"" + testSystemProperty + "\":\"a value\"}") as JObject;
                var typedItemToInsert = itemToInsert.ToObject<ToDoWithSystemPropertiesType>();
                await table.InsertAsync(typedItemToInsert);
            }
        }

        [AsyncTestMethod]
        public async Task InsertAsync_DoesNotRemoveSystemProperties_WhenIdIsString()
        {
            await InsertAsync_DoesNotRemoveSystemPropertiesTest(client => client.GetTable("some"));
        }

        [AsyncTestMethod]
        public async Task InsertAsync_JToken_DoesNotRemoveSystemProperties_WhenIdIsString_Generic()
        {
            await InsertAsync_DoesNotRemoveSystemPropertiesTest(client => client.GetTable<ToDoWithSystemPropertiesType>());
        }

        private static async Task InsertAsync_DoesNotRemoveSystemPropertiesTest(Func<IMobileServiceClient, IMobileServiceTable> getTable)
        {
            string[] testSystemProperties = SystemPropertiesTestData.ValidSystemProperties;

            foreach (string testSystemProperty in testSystemProperties)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"an id\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
                IMobileServiceTable table = getTable(service);
                hijack.OnSendingRequest = async (request) =>
                {
                    string content = await request.Content.ReadAsStringAsync();
                    JObject obj = JToken.Parse(content) as JObject;
                    Assert.IsTrue(obj.Properties().Where(p => p.Name == "id").Any());
                    Assert.IsTrue(obj.Properties().Where(p => p.Name == "String").Any());
                    Assert.IsTrue(obj.Properties().Where(p => p.Name == testSystemProperty).Any());
                    return request;
                };

                JObject itemToInsert = JToken.Parse("{\"id\":\"an id\",\"String\":\"what?\",\"" + testSystemProperty + "\":\"a value\"}") as JObject;
                await table.InsertAsync(itemToInsert);
            }
        }

        [AsyncTestMethod]
        public async Task InsertAsyncStringIdNonSystemPropertiesNotRemovedFromRequest()
        {
            string[] testSystemProperties = SystemPropertiesTestData.NonSystemProperties;

            foreach (string testSystemProperty in testSystemProperties)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"an id\",\"String\":\"Hey\",\"" + testSystemProperty + "\":\"a value\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                hijack.OnSendingRequest = async (request) =>
                {
                    string content = await request.Content.ReadAsStringAsync();
                    JObject obj = JToken.Parse(content) as JObject;
                    Assert.IsTrue(obj.Properties().Where(p => p.Name == "id").Any());
                    Assert.IsTrue(obj.Properties().Where(p => p.Name == "String").Any());
                    Assert.IsTrue(obj.Properties().Where(p => p.Name == testSystemProperty).Any());
                    return request;
                };

                JObject itemToInsert = JToken.Parse("{\"id\":\"an id\",\"String\":\"what?\",\"" + testSystemProperty + "\":\"a value\"}") as JObject;
                await table.InsertAsync(itemToInsert);
            }
        }

        [AsyncTestMethod]
        public async Task InsertAsync_DoesNotRemoveSystemProperties_WhenIdIsNull()
        {
            string[] testSystemProperties = SystemPropertiesTestData.ValidSystemProperties;

            foreach (string testSystemProperty in testSystemProperties)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"an id\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                hijack.OnSendingRequest = async (request) =>
                {
                    string content = await request.Content.ReadAsStringAsync();
                    JObject obj = JToken.Parse(content) as JObject;
                    Assert.IsTrue(obj.Properties().Where(p => p.Name == "id").Any());
                    Assert.IsTrue(obj.Properties().Where(p => p.Name == "String").Any());
                    Assert.IsTrue(obj.Properties().Where(p => p.Name == testSystemProperty).Any());
                    return request;
                };

                JObject itemToInsert = JToken.Parse("{\"id\":null,\"String\":\"what?\",\"" + testSystemProperty + "\":\"a value\"}") as JObject;
                await table.InsertAsync(itemToInsert);
            }
        }

        [AsyncTestMethod]
        public async Task InsertAsyncNullIdNonSystemPropertiesNotRemovedFromRequest()
        {
            string[] testSystemProperties = SystemPropertiesTestData.NonSystemProperties;

            foreach (string testSystemProperty in testSystemProperties)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"an id\",\"String\":\"Hey\",\"" + testSystemProperty + "\":\"a value\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                hijack.OnSendingRequest = async (request) =>
                {
                    string content = await request.Content.ReadAsStringAsync();
                    JObject obj = JToken.Parse(content) as JObject;
                    Assert.IsTrue(obj.Properties().Where(p => p.Name == "id").Any());
                    Assert.IsTrue(obj.Properties().Where(p => p.Name == "String").Any());
                    Assert.IsTrue(obj.Properties().Where(p => p.Name == testSystemProperty).Any());
                    return request;
                };

                JObject itemToInsert = JToken.Parse("{\"id\":null,\"String\":\"what?\",\"" + testSystemProperty + "\":\"a value\"}") as JObject;
                await table.InsertAsync(itemToInsert);
            }
        }

        [AsyncTestMethod]
        public async Task UpdateAsyncStringIdSystemPropertiesRemovedFromRequest()
        {
            string[] testSystemProperties = SystemPropertiesTestData.ValidSystemProperties;

            foreach (string testSystemProperty in testSystemProperties)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"an id\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                hijack.OnSendingRequest = async (request) =>
                {
                    string content = await request.Content.ReadAsStringAsync();
                    JObject obj = JToken.Parse(content) as JObject;
                    Assert.IsTrue(obj.Properties().Where(p => p.Name == "id").Any());
                    Assert.IsTrue(obj.Properties().Where(p => p.Name == "String").Any());
                    Assert.IsFalse(obj.Properties().Where(p => p.Name == testSystemProperty).Any());
                    return request;
                };

                JObject itemToUpdate = JToken.Parse("{\"id\":\"an id\",\"String\":\"what?\",\"" + testSystemProperty + "\":\"a value\"}") as JObject;
                await table.UpdateAsync(itemToUpdate);
            }
        }

        [AsyncTestMethod]
        public async Task UpdateAsyncStringIdNonSystemPropertiesNotRemovedFromRequest()
        {
            string[] testSystemProperties = SystemPropertiesTestData.NonSystemProperties;

            foreach (string testSystemProperty in testSystemProperties)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"an id\",\"String\":\"Hey\",\"" + testSystemProperty + "\":\"a value\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                hijack.OnSendingRequest = async (request) =>
                {
                    string content = await request.Content.ReadAsStringAsync();
                    JObject obj = JToken.Parse(content) as JObject;
                    Assert.IsTrue(obj.Properties().Where(p => p.Name == "id").Any());
                    Assert.IsTrue(obj.Properties().Where(p => p.Name == "String").Any());
                    Assert.IsTrue(obj.Properties().Where(p => p.Name == testSystemProperty).Any());
                    return request;
                };

                JObject itemToUpdate = JToken.Parse("{\"id\":\"an id\",\"String\":\"what?\",\"" + testSystemProperty + "\":\"a value\"}") as JObject;
                await table.UpdateAsync(itemToUpdate);
            }
        }

        [AsyncTestMethod]
        public async Task UpdateAsyncIntegerIdNoPropertiesRemovedFromRequest()
        {
            string[] testSystemProperties = SystemPropertiesTestData.NonSystemProperties.Concat(
                                            SystemPropertiesTestData.ValidSystemProperties).ToArray();

            foreach (string testSystemProperty in testSystemProperties)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":5,\"String\":\"Hey\",\"" + testSystemProperty + "\":\"a value\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                hijack.OnSendingRequest = async (request) =>
                {
                    string content = await request.Content.ReadAsStringAsync();
                    JObject obj = JToken.Parse(content) as JObject;
                    Assert.IsTrue(obj.Properties().Where(p => p.Name == "id").Any());
                    Assert.IsTrue(obj.Properties().Where(p => p.Name == "String").Any());
                    Assert.IsTrue(obj.Properties().Where(p => p.Name == testSystemProperty).Any());
                    return request;
                };

                JObject itemToUpdate = JToken.Parse("{\"id\":5,\"String\":\"what?\",\"" + testSystemProperty + "\":\"a value\"}") as JObject;
                await table.UpdateAsync(itemToUpdate);
            }
        }

        [AsyncTestMethod]
        public async Task TableOperationSystemPropertiesQueryStringIsCorrect()
        {
            MobileServiceSystemProperties[] testSystemProperties = SystemPropertiesTestData.SystemProperties;

            Action<MobileServiceSystemProperties, string> ValidateUri = (testSystemProperty, requestUri) =>
            {
                if (testSystemProperty != MobileServiceSystemProperties.None)
                {
                    Assert.IsTrue(requestUri.Contains("__systemproperties"));
                }
                else
                {
                    Assert.IsFalse(requestUri.Contains("__systemproperties"));
                }

                if (testSystemProperty == MobileServiceSystemProperties.All)
                {
                    Assert.IsTrue(requestUri.Contains("__systemproperties=*"));
                }
                else if ((testSystemProperty & MobileServiceSystemProperties.CreatedAt) == MobileServiceSystemProperties.CreatedAt)
                {
                    Assert.IsTrue(requestUri.Contains("__createdAt"));
                }
                else if ((testSystemProperty & MobileServiceSystemProperties.UpdatedAt) == MobileServiceSystemProperties.UpdatedAt)
                {
                    Assert.IsTrue(requestUri.Contains("__updatedAt"));
                }
                else if ((testSystemProperty & MobileServiceSystemProperties.Version) == MobileServiceSystemProperties.Version)
                {
                    Assert.IsTrue(requestUri.Contains("__version"));
                }
            };

            foreach (MobileServiceSystemProperties testSystemProperty in testSystemProperties)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable table = service.GetTable("someTable");
                table.SystemProperties = testSystemProperty;

                // string id
                JObject item = JToken.Parse("{\"id\":\"an id\",\"String\":\"what?\"}") as JObject;
                hijack.SetResponseContent("{\"id\":\"an id\",\"String\":\"Hey\"}");
                await table.InsertAsync(item);
                ValidateUri(testSystemProperty, hijack.Request.RequestUri.ToString());

                hijack.SetResponseContent("{\"id\":\"an id\",\"String\":\"Hey\"}");
                await table.UpdateAsync(item);
                ValidateUri(testSystemProperty, hijack.Request.RequestUri.ToString());

                hijack.SetResponseContent("{\"id\":5,\"String\":\"Hey\"}");
                await table.LookupAsync("an id");
                ValidateUri(testSystemProperty, hijack.Request.RequestUri.ToString());

                hijack.SetResponseContent("{\"id\":\"an id\",\"String\":\"Hey\"}");
                await table.DeleteAsync(item);
                ValidateUri(testSystemProperty, hijack.Request.RequestUri.ToString());

                // int id
                hijack.SetResponseContent("{\"id\":5,\"String\":\"Hey\"}");
                item = JToken.Parse("{\"String\":\"what?\"}") as JObject;
                await table.InsertAsync(item);
                ValidateUri(testSystemProperty, hijack.Request.RequestUri.ToString());

                hijack.SetResponseContent("{\"id\":5,\"String\":\"Hey\"}");
                item = JToken.Parse("{\"id\":5,\"String\":\"what?\"}") as JObject;
                await table.UpdateAsync(item);
                ValidateUri(testSystemProperty, hijack.Request.RequestUri.ToString());

                hijack.SetResponseContent("{\"id\":5,\"String\":\"Hey\"}");
                await table.LookupAsync(5);
                ValidateUri(testSystemProperty, hijack.Request.RequestUri.ToString());

                hijack.SetResponseContent("{\"id\":5,\"String\":\"Hey\"}");
                await table.DeleteAsync(item);
                ValidateUri(testSystemProperty, hijack.Request.RequestUri.ToString());

                // query
                hijack.SetResponseContent("[{\"id\":5,\"String\":\"Hey\"}]");
                await table.ReadAsync("$filter=id eq 5");
                ValidateUri(testSystemProperty, hijack.Request.RequestUri.ToString());

                hijack.SetResponseContent("[{\"id\":5,\"String\":\"Hey\"}]");
                await table.ReadAsync("$select=id,String");
                ValidateUri(testSystemProperty, hijack.Request.RequestUri.ToString());
            }
        }

        [AsyncTestMethod]
        public async Task TableOperationUserParameterWithsystemPropertyQueryStringIsCorrect()
        {
            MobileServiceSystemProperties[] testSystemProperties = SystemPropertiesTestData.SystemProperties;


            foreach (MobileServiceSystemProperties testSystemProperty in testSystemProperties)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable table = service.GetTable("someTable");
                table.SystemProperties = testSystemProperty;

                // string id
                JObject item = JToken.Parse("{\"id\":\"an id\",\"String\":\"what?\"}") as JObject;
                hijack.SetResponseContent("{\"id\":\"an id\",\"String\":\"Hey\"}");
                await table.InsertAsync(item, new Dictionary<string, string>() { { "__systemproperties", "__createdAt" } });
                Assert.IsTrue(hijack.Request.RequestUri.ToString().Contains("__systemproperties=__createdAt"));

                hijack.SetResponseContent("{\"id\":\"an id\",\"String\":\"Hey\"}");
                await table.UpdateAsync(item, new Dictionary<string, string>() { { "__systemproperties", "createdAt" } });
                Assert.IsTrue(hijack.Request.RequestUri.ToString().Contains("__systemproperties=createdAt"));

                hijack.SetResponseContent("{\"id\":5,\"String\":\"Hey\"}");
                await table.LookupAsync("an id", new Dictionary<string, string>() { { "__systemproperties", "CreatedAt" } });
                Assert.IsTrue(hijack.Request.RequestUri.ToString().Contains("__systemproperties=CreatedAt"));

                hijack.SetResponseContent("{\"id\":\"an id\",\"String\":\"Hey\"}");
                await table.DeleteAsync(item, new Dictionary<string, string>() { { "__systemproperties", "unknown" } });
                Assert.IsTrue(hijack.Request.RequestUri.ToString().Contains("__systemproperties=unknown"));

                // int id
                hijack.SetResponseContent("{\"id\":5,\"String\":\"Hey\"}");
                item = JToken.Parse("{\"String\":\"what?\"}") as JObject;
                await table.InsertAsync(item, new Dictionary<string, string>() { { "__systemproperties", "__createdAt" } });
                Assert.IsTrue(hijack.Request.RequestUri.ToString().Contains("__systemproperties=__createdAt"));

                hijack.SetResponseContent("{\"id\":5,\"String\":\"Hey\"}");
                item = JToken.Parse("{\"id\":5,\"String\":\"what?\"}") as JObject;
                await table.UpdateAsync(item, new Dictionary<string, string>() { { "__systemproperties", "__createdAt" } });
                Assert.IsTrue(hijack.Request.RequestUri.ToString().Contains("__systemproperties=__createdAt"));

                hijack.SetResponseContent("{\"id\":5,\"String\":\"Hey\"}");
                await table.LookupAsync(5, new Dictionary<string, string>() { { "__systemproperties", "__createdAt" } });
                Assert.IsTrue(hijack.Request.RequestUri.ToString().Contains("__systemproperties=__createdAt"));

                hijack.SetResponseContent("{\"id\":5,\"String\":\"Hey\"}");
                await table.DeleteAsync(item, new Dictionary<string, string>() { { "__systemproperties", "__createdAt" } });
                Assert.IsTrue(hijack.Request.RequestUri.ToString().Contains("__systemproperties=__createdAt"));

                // query
                hijack.SetResponseContent("[{\"id\":5,\"String\":\"Hey\"}]");
                await table.ReadAsync("$filter=id eq 5", new Dictionary<string, string>() { { "__systemproperties", "__createdAt" } });
                Assert.IsTrue(hijack.Request.RequestUri.ToString().Contains("__systemproperties=__createdAt"));

                hijack.SetResponseContent("[{\"id\":5,\"String\":\"Hey\"}]");
                await table.ReadAsync("$select=id,String", new Dictionary<string, string>() { { "__systemproperties", "__createdAt" } });
                Assert.IsTrue(hijack.Request.RequestUri.ToString().Contains("__systemproperties=__createdAt"));
            }
        }

        #endregion System Property Tests
    }
}
