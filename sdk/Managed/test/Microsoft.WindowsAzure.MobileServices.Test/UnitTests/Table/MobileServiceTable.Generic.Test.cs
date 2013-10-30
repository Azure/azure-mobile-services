// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.TestFramework;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    [Tag("unit")]
    [Tag("table")]
    public class MobileServiceTableGenericTests :TestBase
    {
        #region Read Tests

        [AsyncTestMethod]
        public async Task ReadAsyncWithStringIdTypeAndStringIdResponseContent()
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

                IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();

                IEnumerable<StringIdType> results = await table.ReadAsync();
                StringIdType[] items = results.ToArray();

                Assert.AreEqual(1, items.Count());
                Assert.AreEqual(testId, items[0].Id);
                Assert.AreEqual("Hey", items[0].String);
            }
        }

        [AsyncTestMethod]
        public async Task ReadAsyncWithStringIdTypeAndNonStringIdResponseContent()
        {
            object[] testIdData = IdTestData.ValidIntIds.Concat(
                                  IdTestData.InvalidIntIds).Cast<object>().Concat(
                                  IdTestData.NonStringNonIntValidJsonIds).ToArray();

            foreach (object testId in testIdData)
            {
                string stringTestId = testId.ToString().ToLower();

                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("[{\"id\":" + stringTestId + ",\"String\":\"Hey\"}]");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();

                IEnumerable<StringIdType> results = await table.ReadAsync();
                StringIdType[] items = results.ToArray();

                Assert.AreEqual(1, items.Count());
                Assert.AreEqual(testId.ToString(), items[0].Id);
                Assert.AreEqual("Hey", items[0].String);
            }
        }

        [AsyncTestMethod]
        public async Task ReadAsyncWithStringIdTypeAndNullIdResponseContent()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("[{\"id\":null,\"String\":\"Hey\"}]");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();

            IEnumerable<StringIdType> results = await table.ReadAsync();
            StringIdType[] items = results.ToArray();

            Assert.AreEqual(1, items.Count());
            Assert.AreEqual(null, items[0].Id);
            Assert.AreEqual("Hey", items[0].String);
        }

        [AsyncTestMethod]
        public async Task ReadAsyncWithStringIdTypeAndNoIdResponseContent()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("[{\"String\":\"Hey\"}]");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();

            IEnumerable<StringIdType> results = await table.ReadAsync();
            StringIdType[] items = results.ToArray();

            Assert.AreEqual(1, items.Count());
            Assert.AreEqual(null, items[0].Id);
            Assert.AreEqual("Hey", items[0].String);
        }

        [AsyncTestMethod]
        public async Task ReadAsyncWithStringIdTypeAndStringIdFilter()
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

                IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();

                List<StringIdType> items = await table.Where(t => t.Id == testId).ToListAsync();
                string idForOdataQuery = Uri.EscapeDataString(testId.Replace("'", "''"));
                Uri expectedUri = new Uri(string.Format("http://www.test.com/tables/StringIdType?$filter=(id eq '{0}')", idForOdataQuery));

                Assert.AreEqual(1, items.Count());
                Assert.AreEqual(testId, items[0].Id);
                Assert.AreEqual(hijack.Request.RequestUri.AbsoluteUri, expectedUri.AbsoluteUri);
            }
        }

        [AsyncTestMethod]
        public async Task ReadAsyncWithStringIdTypeAndNullIdFilter()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("[{\"id\":null,\"String\":\"Hey\"}]");

            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();

            List<StringIdType> items = await table.Where(t => t.Id == null).ToListAsync();
            Uri expectedUri = new Uri("http://www.test.com/tables/StringIdType?$filter=(id eq null)");

            Assert.AreEqual(1, items.Count());
            Assert.AreEqual(null, items[0].Id);
            Assert.AreEqual(hijack.Request.RequestUri.AbsoluteUri, expectedUri.AbsoluteUri);
        }

        [AsyncTestMethod]
        public async Task ReadAsyncWithStringIdTypeAndStringIdProjection()
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

                IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();

                var items = await table.Select(s => new { Id = s.Id, Message = s.String }).ToListAsync();
                Uri expectedUri = new Uri("http://www.test.com/tables/StringIdType?$select=id,String");

                Assert.AreEqual(1, items.Count());
                Assert.AreEqual(testId, items[0].Id);
                Assert.AreEqual("Hey", items[0].Message);
                Assert.AreEqual(hijack.Request.RequestUri.AbsoluteUri, expectedUri.AbsoluteUri);
            }
        }

        [AsyncTestMethod]
        public async Task ReadAsyncWithStringIdTypeAndNullIdProjection()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("[{\"id\":null,\"String\":\"Hey\"}]");

            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();

            var items = await table.Select(s => new { Id = s.Id, Message = s.String }).ToListAsync();
            Uri expectedUri = new Uri("http://www.test.com/tables/StringIdType?$select=id,String");

            Assert.AreEqual(1, items.Count());
            Assert.AreEqual(null, items[0].Id);
            Assert.AreEqual("Hey", items[0].Message);
            Assert.AreEqual(hijack.Request.RequestUri.AbsoluteUri, expectedUri.AbsoluteUri);
        }

        [AsyncTestMethod]
        public async Task ReadAsyncWithStringIdTypeAndNoIdProjection()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("[{\"String\":\"Hey\"}]");

            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();

            var items = await table.Select(s => new { Id = s.Id, Message = s.String }).ToListAsync();
            Uri expectedUri = new Uri("http://www.test.com/tables/StringIdType?$select=id,String");

            Assert.AreEqual(1, items.Count());
            Assert.AreEqual(null, items[0].Id);
            Assert.AreEqual("Hey", items[0].Message);
            Assert.AreEqual(hijack.Request.RequestUri.AbsoluteUri, expectedUri.AbsoluteUri);
        }

        [AsyncTestMethod]
        public async Task ReadAsyncWithStringIdTypeAndOrderByAscending()
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

                IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();

                var items = await table.OrderBy(s => s.Id).ToListAsync();
                Uri expectedUri = new Uri("http://www.test.com/tables/StringIdType?$orderby=id");

                Assert.AreEqual(1, items.Count());
                Assert.AreEqual(testId, items[0].Id);
                Assert.AreEqual("Hey", items[0].String);
                Assert.AreEqual(hijack.Request.RequestUri.AbsoluteUri, expectedUri.AbsoluteUri);
            }
        }

        [AsyncTestMethod]
        public async Task ReadAsyncWithStringIdTypeAndOrderByDescending()
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

                IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();

                var items = await table.OrderByDescending(s => s.Id).ToListAsync();
                Uri expectedUri = new Uri("http://www.test.com/tables/StringIdType?$orderby=id desc");

                Assert.AreEqual(1, items.Count());
                Assert.AreEqual(testId, items[0].Id);
                Assert.AreEqual("Hey", items[0].String);
                Assert.AreEqual(hijack.Request.RequestUri.AbsoluteUri, expectedUri.AbsoluteUri);
            }
        }

        [AsyncTestMethod]
        public async Task ReadAsyncWithIntIdTypeAndIntIdResponseContent()
        {
            long[] testIdData = IdTestData.ValidIntIds.Concat(
                                IdTestData.InvalidIntIds).ToArray();

            foreach (long testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("[{\"id\":" + testId + ",\"String\":\"Hey\"}]");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();

                IEnumerable<LongIdType> results = await table.ReadAsync();
                LongIdType[] items = results.ToArray();

                Assert.AreEqual(1, items.Count());
                Assert.AreEqual(testId, items[0].Id);
                Assert.AreEqual("Hey", items[0].String);
            }
        }

        [AsyncTestMethod]
        public async Task ReadAsyncWithIntIdTypeAndIntParseableIdResponseContent()
        {
            object[] testIdData = IdTestData.NonStringNonIntValidJsonIds;

            foreach (object testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("[{\"id\":" + testId.ToString().ToLower() + ",\"String\":\"Hey\"}]");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();

                IEnumerable<LongIdType> results = await table.ReadAsync();
                LongIdType[] items = results.ToArray();

                Assert.AreEqual(1, items.Count());
                Assert.AreEqual(Convert.ToInt64(testId), items[0].Id);
                Assert.AreEqual("Hey", items[0].String);
            }
        }

        [AsyncTestMethod]
        public async Task ReadAsyncWithIntIdTypeAndNullIdResponseContent()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("[{\"id\":null,\"String\":\"Hey\"}]");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();

            IEnumerable<LongIdType> results = await table.ReadAsync();
            LongIdType[] items = results.ToArray();

            Assert.AreEqual(1, items.Count());
            Assert.AreEqual(0L, items[0].Id);
            Assert.AreEqual("Hey", items[0].String);
        }

        [AsyncTestMethod]
        public async Task ReadAsyncWithIntIdTypeAndNoIdResponseContent()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("[{\"String\":\"Hey\"}]");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();

            IEnumerable<LongIdType> results = await table.ReadAsync();
            LongIdType[] items = results.ToArray();

            Assert.AreEqual(1, items.Count());
            Assert.AreEqual(0L, items[0].Id);
            Assert.AreEqual("Hey", items[0].String);
        }

        [AsyncTestMethod]
        public async Task ReadAsyncWithIntIdTypeAndStringIdResponseContent()
        {
            string[] testIdData = IdTestData.ValidStringIds.Concat(
                                  IdTestData.EmptyStringIds).ToArray();

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("[{\"id\":\"" + testId + "\",\"String\":\"Hey\"}]");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();

                Exception exception = null;
                try
                {
                    IEnumerable<LongIdType> results = await table.ReadAsync();
                    LongIdType[] items = results.ToArray();
                }
                catch (Exception e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.IsTrue(exception.Message.Contains("Error converting value"));
            }
        }

        [AsyncTestMethod]
        public async Task ReadAsyncWithIntIdTypeAndIntIdFilter()
        {
            long[] testIdData = IdTestData.ValidIntIds.Concat(
                                IdTestData.InvalidIntIds).ToArray();

            foreach (long testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("[{\"id\":" + testId + ",\"String\":\"Hey\"}]");

                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();

                List<LongIdType> items = await table.Where(t => t.Id == testId).ToListAsync();
                Uri expectedUri = new Uri(string.Format("http://www.test.com/tables/LongIdType?$filter=(id eq {0}L)", testId));

                Assert.AreEqual(1, items.Count());
                Assert.AreEqual(testId, items[0].Id);
                Assert.AreEqual(hijack.Request.RequestUri.AbsoluteUri, expectedUri.AbsoluteUri);
            }
        }

        [AsyncTestMethod]
        public async Task ReadAsyncWithIntIdTypeAndNullIdFilter()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("[{\"id\":null,\"String\":\"Hey\"}]");

            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();

            List<LongIdType> items = await table.Where(t => t.Id == null).ToListAsync();
            Uri expectedUri = new Uri("http://www.test.com/tables/LongIdType?$filter=(id eq null)");

            Assert.AreEqual(1, items.Count());
            Assert.AreEqual(0L, items[0].Id);
            Assert.AreEqual(hijack.Request.RequestUri.AbsoluteUri, expectedUri.AbsoluteUri);
        }

        [AsyncTestMethod]
        public async Task ReadAsyncWithIntIdTypeAndIntIdProjection()
        {
            long[] testIdData = IdTestData.ValidIntIds.Concat(
                                IdTestData.InvalidIntIds).ToArray();

            foreach (long testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("[{\"id\":" + testId + ",\"String\":\"Hey\"}]");

                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();

                var items = await table.Select(s => new { Id = s.Id, Message = s.String }).ToListAsync();
                Uri expectedUri = new Uri("http://www.test.com/tables/LongIdType?$select=id,String");

                Assert.AreEqual(1, items.Count());
                Assert.AreEqual(testId, items[0].Id);
                Assert.AreEqual("Hey", items[0].Message);
                Assert.AreEqual(hijack.Request.RequestUri.AbsoluteUri, expectedUri.AbsoluteUri);
            }
        }

        [AsyncTestMethod]
        public async Task ReadAsyncWithIntIdTypeAndNullIdProjection()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("[{\"id\":null,\"String\":\"Hey\"}]");

            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();

            var items = await table.Select(s => new { Id = s.Id, Message = s.String }).ToListAsync();
            Uri expectedUri = new Uri("http://www.test.com/tables/LongIdType?$select=id,String");

            Assert.AreEqual(1, items.Count());
            Assert.AreEqual(0L, items[0].Id);
            Assert.AreEqual("Hey", items[0].Message);
            Assert.AreEqual(hijack.Request.RequestUri.AbsoluteUri, expectedUri.AbsoluteUri);
        }

        [AsyncTestMethod]
        public async Task ReadAsyncWithIntIdTypeAndNoIdProjection()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("[{\"String\":\"Hey\"}]");

            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();

            var items = await table.Select(s => new { Id = s.Id, Message = s.String }).ToListAsync();
            Uri expectedUri = new Uri("http://www.test.com/tables/LongIdType?$select=id,String");

            Assert.AreEqual(1, items.Count());
            Assert.AreEqual(0L, items[0].Id);
            Assert.AreEqual("Hey", items[0].Message);
            Assert.AreEqual(hijack.Request.RequestUri.AbsoluteUri, expectedUri.AbsoluteUri);
        }

        [AsyncTestMethod]
        public async Task ReadAsyncWithIntIdTypeAndOrderByAscending()
        {
            long[] testIdData = IdTestData.ValidIntIds.Concat(
                                IdTestData.InvalidIntIds).ToArray();

            foreach (long testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("[{\"id\":" + testId + ",\"String\":\"Hey\"}]");

                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();

                var items = await table.OrderBy(s => s.Id).ToListAsync();
                Uri expectedUri = new Uri("http://www.test.com/tables/LongIdType?$orderby=id");

                Assert.AreEqual(1, items.Count());
                Assert.AreEqual(testId, items[0].Id);
                Assert.AreEqual("Hey", items[0].String);
                Assert.AreEqual(hijack.Request.RequestUri.AbsoluteUri, expectedUri.AbsoluteUri);
            }
        }

        [AsyncTestMethod]
        public async Task ReadAsyncWithIntIdTypeAndOrderByDescending()
        {
            long[] testIdData = IdTestData.ValidIntIds.Concat(
                                IdTestData.InvalidIntIds).ToArray();

            foreach (long testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("[{\"id\":\"" + testId + "\",\"String\":\"Hey\"}]");

                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();

                var items = await table.OrderByDescending(s => s.Id).ToListAsync();
                Uri expectedUri = new Uri("http://www.test.com/tables/LongIdType?$orderby=id desc");

                Assert.AreEqual(1, items.Count());
                Assert.AreEqual(testId, items[0].Id);
                Assert.AreEqual("Hey", items[0].String);
                Assert.AreEqual(hijack.Request.RequestUri.AbsoluteUri, expectedUri.AbsoluteUri);
            }
        }

        #endregion Read Tests

        #region Lookup Tests

        [AsyncTestMethod]
        public async Task LookupAsyncWithStringIdTypeAndStringIdResponseContent()
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

                IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();

                StringIdType item = await table.LookupAsync("an id");

                Assert.AreEqual(testId, item.Id);
                Assert.AreEqual("Hey", item.String);
            }
        }

        [AsyncTestMethod]
        public async Task LookupAsyncWithStringIdTypeAndNonStringIdResponseContent()
        {
            object[] testIdData = IdTestData.ValidIntIds.Concat(
                                  IdTestData.InvalidIntIds).Cast<object>().Concat(
                                  IdTestData.NonStringNonIntValidJsonIds).ToArray();

            foreach (object testId in testIdData)
            {
                string stringTestId = testId.ToString().ToLower();

                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":" + stringTestId + ",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();

                StringIdType item = await table.LookupAsync("an id");

                Assert.AreEqual(testId.ToString(), item.Id);
                Assert.AreEqual("Hey", item.String);
            }
        }

        [AsyncTestMethod]
        public async Task LookupAsyncWithStringIdTypeAndNullIdResponseContent()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":null,\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();

            StringIdType item = await table.LookupAsync("an id");

            Assert.AreEqual(null, item.Id);
            Assert.AreEqual("Hey", item.String);
        }

        [AsyncTestMethod]
        public async Task LookupAsyncWithStringIdTypeAndNoIdResponseContent()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();

            StringIdType item = await table.LookupAsync("an id");

            Assert.AreEqual(null, item.Id);
            Assert.AreEqual("Hey", item.String);
        }

        [AsyncTestMethod]
        public async Task LookupAsyncWithStringIdTypeAndStringIdParameter()
        {
            string[] testIdData = IdTestData.ValidStringIds.ToArray();

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"" + testId + "\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();

                StringIdType item = await table.LookupAsync(testId);

                Uri expectedUri = new Uri(string.Format("http://www.test.com/tables/StringIdType/{0}", Uri.EscapeDataString(testId)));

                Assert.AreEqual(testId, item.Id);
                Assert.AreEqual("Hey", item.String);
                Assert.AreEqual(hijack.Request.RequestUri.AbsoluteUri, expectedUri.AbsoluteUri);
            }
        }

        [AsyncTestMethod]
        public async Task LookupAsyncWithStringIdTypeAndInvalidStringIdParameter()
        {
            string[] testIdData = IdTestData.EmptyStringIds.Concat(
                                  IdTestData.InvalidStringIds).ToArray();

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"" + testId + "\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();
                Exception exception = null;
                try
                {
                    StringIdType item = await table.LookupAsync(testId);
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
        public async Task LookupAsyncWithStringIdTypeAndIntIdParameter()
        {
            long[] testIdData = IdTestData.ValidIntIds.Concat(
                                IdTestData.InvalidIntIds).ToArray();

            foreach (long testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
                IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();

                Exception exception = null;
                try
                {
                    StringIdType item = await table.LookupAsync(testId);
                }
                catch (Exception e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.IsTrue(exception.Message.Contains("is invalid for looking up items of type"));
            }
        }

        [AsyncTestMethod]
        public async Task LookupAsyncWithStringIdTypeAndNullIdParameter()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":null,\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();

            Exception exception = null;
            try
            {
                StringIdType item = await table.LookupAsync(null);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.IsTrue(exception.Message.Contains("The id can not be null or an empty string."));
        }

        [AsyncTestMethod]
        public async Task LookupAsyncWithIntIdTypeAndIntIdResponseContent()
        {
            long[] testIdData = IdTestData.ValidIntIds.Concat(
                    IdTestData.InvalidIntIds).ToArray();

            foreach (long testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"" + testId + "\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();

                LongIdType item = await table.LookupAsync(10);

                Assert.AreEqual(testId, item.Id);
                Assert.AreEqual("Hey", item.String);
            }
        }

        [AsyncTestMethod]
        public async Task LookupAsyncWithIntIdTypeAndIntParseableIdResponseContent()
        {
            object[] testIdData = IdTestData.NonStringNonIntValidJsonIds; ;

            foreach (object testId in testIdData)
            {
                string stringTestId = testId.ToString().ToLower();

                TestHttpHandler hijack = new TestHttpHandler();

                hijack.SetResponseContent("{\"id\":" + stringTestId + ",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();

                LongIdType item = await table.LookupAsync(10);

                Assert.AreEqual(Convert.ToInt64(testId), item.Id);
                Assert.AreEqual("Hey", item.String);
            }
        }

        [AsyncTestMethod]
        public async Task LookupAsyncWithIntIdTypeAndNullIdResponseContent()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":null,\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();

            LongIdType item = await table.LookupAsync(10);

            Assert.AreEqual(0L, item.Id);
            Assert.AreEqual("Hey", item.String);
        }

        [AsyncTestMethod]
        public async Task LookupAsyncWithIntIdTypeAndNoIdResponseContent()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();

            LongIdType item = await table.LookupAsync(10);

            Assert.AreEqual(0L, item.Id);
            Assert.AreEqual("Hey", item.String);
        }

        [AsyncTestMethod]
        public async Task LookupAsyncWithIntIdTypeAndStringIdResponseContent()
        {
            string[] testIdData = IdTestData.ValidStringIds.Concat(
                                  IdTestData.EmptyStringIds).ToArray();

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"" + testId + "\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();

                Exception exception = null;
                try
                {
                    LongIdType item = await table.LookupAsync(10);
                }
                catch (Exception e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.IsTrue(exception.Message.Contains("Error converting value"));
            }
        }

        [AsyncTestMethod]
        public async Task LookupAsyncWithIntIdTypeAndIntIdParameter()
        {
            long[] testIdData = IdTestData.ValidIntIds;

            foreach (long testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"" + testId + "\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();

                LongIdType item = await table.LookupAsync(testId);

                Uri expectedUri = new Uri(string.Format("http://www.test.com/tables/LongIdType/{0}", testId));

                Assert.AreEqual(testId, item.Id);
                Assert.AreEqual("Hey", item.String);
                Assert.AreEqual(hijack.Request.RequestUri.AbsoluteUri, expectedUri.AbsoluteUri);
            }
        }

        [AsyncTestMethod]
        public async Task LookupAsyncWithIntIdTypeAndInvalidIntIdParameter()
        {
            long[] testIdData = IdTestData.InvalidIntIds;

            foreach (long testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"" + testId + "\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();
                Exception exception = null;
                try
                {
                    LongIdType item = await table.LookupAsync(testId);
                }
                catch (Exception e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.IsTrue(exception.Message.Contains("Specified argument was out of the range of valid values") ||
                              exception.Message.Contains(" is not a positive integer value"));
            }
        }

        [AsyncTestMethod]
        public async Task LookupAsyncWithIntIdTypeAndStringIdParameter()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();

            Exception exception = null;
            try
            {
                LongIdType item = await table.LookupAsync("a string");
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.IsTrue(exception.Message.Contains("is invalid for looking up items of type"));
        }

        [AsyncTestMethod]
        public async Task LookupAsyncWithIntIdTypeAndNullIdParameter()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":null,\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();

            Exception exception = null;
            try
            {
                LongIdType item = await table.LookupAsync(null);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.IsTrue(exception.Message.Contains(" is invalid for looking up items of type "));
        }

        [AsyncTestMethod]
        public async Task LookupAsyncWithIntIdTypeAndZeroIdParameter()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":null,\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();

            Exception exception = null;
            try
            {
                LongIdType item = await table.LookupAsync(0L);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.IsTrue(exception.Message.Contains(" is not a positive integer value"));
        }

        [AsyncTestMethod]
        public async Task LookupAsyncWithUserParameters()
        {
            var userDefinedParameters = new Dictionary<string, string>() { { "state", "CA" } };

            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            IMobileServiceTable<StringType> table = service.GetTable<StringType>();

            hijack.SetResponseContent("{\"id\":12,\"String\":\"Hello\"}");

            StringType expected = await table.LookupAsync(12, userDefinedParameters);

            Assert.Contains(hijack.Request.RequestUri.ToString(), "12");
            Assert.Contains(hijack.Request.RequestUri.Query, "state=CA");
            Assert.AreEqual(12, expected.Id);
            Assert.AreEqual("Hello", expected.String);
        }

        #endregion Lookup Tests

        #region Refresh Tests

        [AsyncTestMethod]
        public async Task RefreshAsyncWithStringIdTypeAndStringIdResponseContent()
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

                IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();

                StringIdType item = new StringIdType() { Id = "an id", String = "what?" };
                await table.RefreshAsync(item);

                Assert.AreEqual(testId, item.Id);
                Assert.AreEqual("Hey", item.String);
            }
        }

        [AsyncTestMethod]
        public async Task RefreshAsyncWithStringIdTypeAndNonStringIdResponseContent()
        {
            object[] testIdData = IdTestData.ValidIntIds.Concat(
                                  IdTestData.InvalidIntIds).Cast<object>().Concat(
                                  IdTestData.NonStringNonIntValidJsonIds).ToArray();

            foreach (object testId in testIdData)
            {
                string stringTestId = testId.ToString().ToLower();

                TestHttpHandler hijack = new TestHttpHandler();

                hijack.SetResponseContent("{\"id\":" + stringTestId + ",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();

                StringIdType item = new StringIdType() { Id = "an id", String = "what?" };
                await table.RefreshAsync(item);

                Assert.AreEqual(testId.ToString(), item.Id);
                Assert.AreEqual("Hey", item.String);
            }
        }

        [AsyncTestMethod]
        public async Task RefreshAsyncWithStringIdTypeAndNullIdResponseContent()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":null,\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();

            StringIdType item = new StringIdType() { Id = "an id", String = "what?" };
            await table.RefreshAsync(item);

            Assert.AreEqual("an id", item.Id);
            Assert.AreEqual("Hey", item.String);
        }

        [AsyncTestMethod]
        public async Task RefreshAsyncWithStringIdTypeAndNoIdResponseContent()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();

            StringIdType item = new StringIdType() { Id = "an id", String = "what?" };
            await table.RefreshAsync(item);

            Assert.AreEqual("an id", item.Id);
            Assert.AreEqual("Hey", item.String);
        }

        [AsyncTestMethod]
        public async Task RefreshAsyncWithStringIdTypeAndStringIdItem()
        {
            string[] testIdData = IdTestData.ValidStringIds.ToArray();

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("[{\"id\":\"" + testId + "\",\"String\":\"Hey\"}]");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();

                StringIdType item = new StringIdType() { Id = testId, String = "what?" };
                await table.RefreshAsync(item);
                
                string idForOdataQuery = Uri.EscapeDataString(testId.Replace("'", "''"));
                Uri expectedUri = new Uri(string.Format("http://www.test.com/tables/StringIdType?$filter=(id eq '{0}')", idForOdataQuery));

                Assert.AreEqual(testId, item.Id);
                Assert.AreEqual("Hey", item.String);
                Assert.AreEqual(hijack.Request.RequestUri.AbsoluteUri, expectedUri.AbsoluteUri);
            }
        }

        [AsyncTestMethod]
        public async Task RefreshAsyncWithStringIdTypeAndEmptyStringIdItem()
        {
            string[] testIdData = IdTestData.EmptyStringIds;

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("[{\"id\":\"" + testId + "\",\"String\":\"Hey\"}]");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();

                StringIdType item = new StringIdType() { Id = testId, String = "what?" };
                await table.RefreshAsync(item);

                Assert.AreEqual(testId, item.Id);
                Assert.AreEqual("what?", item.String);
            }
        }

        [AsyncTestMethod]
        public async Task RefreshAsyncWithStringIdTypeAndInvalidStringIdParameter()
        {
            string[] testIdData = IdTestData.InvalidStringIds;

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"" + testId + "\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();
                Exception exception = null;
                try
                {
                    StringIdType item = new StringIdType() { Id = testId, String = "what?" };
                    await table.RefreshAsync(item);
                }
                catch (Exception e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.IsTrue(exception.Message.Contains("An id must not contain any control characters or the characters") || 
                              exception.Message.Contains("is longer than the max string id length of 255 characters"));
            }
        }

        [AsyncTestMethod]
        public async Task RefreshAsyncWithStringIdTypeAndNullIdParameter()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();

            StringIdType item = new StringIdType() { String = "Hey" };
            await table.RefreshAsync(item);

            Assert.AreEqual(null, item.Id);
            Assert.AreEqual("Hey", item.String);
        }

        [AsyncTestMethod]
        public async Task RefreshAsyncWithIntIdTypeAndIntIdResponseContent()
        {
            long[] testIdData = IdTestData.ValidIntIds.Concat(
                                IdTestData.InvalidIntIds).ToArray();

            foreach (long testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":" + testId + ",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();

                LongIdType item = new LongIdType() { Id = 3, String = "what?" };
                await table.RefreshAsync(item);

                Assert.AreEqual(testId, item.Id);
                Assert.AreEqual("Hey", item.String);
            }
        }

        [AsyncTestMethod]
        public async Task RefreshAsyncWithIntIdTypeParseableIdResponseContent()
        {
            object[] testIdData = IdTestData.NonStringNonIntValidJsonIds;

            foreach (object testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":" + testId.ToString().ToLower() + ",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();

                LongIdType item = new LongIdType() { Id = 3, String = "what?" };
                await table.RefreshAsync(item);

                long expectedId = Convert.ToInt64(testId);
                if (expectedId == default(long))
                {
                    expectedId = 3;
                }

                Assert.AreEqual(expectedId, item.Id);
                Assert.AreEqual("Hey", item.String);
            }
        }

        [AsyncTestMethod]
        public async Task RefreshAsyncWithIntIdTypeAndNullIdResponseContent()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":null,\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();

            LongIdType item = new LongIdType() { Id = 3, String = "what?" };
            await table.RefreshAsync(item);

            Assert.AreEqual(3L, item.Id);
            Assert.AreEqual("Hey", item.String);
        }

        [AsyncTestMethod]
        public async Task RefreshAsyncWithIntIdTypeAndNoIdResponseContent()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();

            LongIdType item = new LongIdType() { Id = 3, String = "what?" };
            await table.RefreshAsync(item);

            Assert.AreEqual(3L, item.Id);
            Assert.AreEqual("Hey", item.String);
        }

        [AsyncTestMethod]
        public async Task RefreshAsyncWithIntIdTypeAndStringIdResponseContent()
        {
            string[] testIdData = IdTestData.ValidStringIds.Concat(
                                  IdTestData.EmptyStringIds).ToArray();

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"" + testId + "\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();

                Exception exception = null;
                try
                {
                    LongIdType item = new LongIdType() { Id = 3, String = "what?" };
                    await table.RefreshAsync(item);
                }
                catch (Exception e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.IsTrue(exception.Message.Contains("Error converting value"));
            }
        }

        [AsyncTestMethod]
        public async Task RefreshAsyncWithIntIdTypeAndIntIdItem()
        {
            long[] testIdData = IdTestData.ValidIntIds;

            foreach (long testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("[{\"id\":" + testId + ",\"String\":\"Hey\"}]");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();

                LongIdType item = new LongIdType() { Id = testId, String = "what?" };
                await table.RefreshAsync(item);

                Uri expectedUri = new Uri(string.Format("http://www.test.com/tables/LongIdType?$filter=(id eq {0}L)", testId));

                Assert.AreEqual(testId, item.Id);
                Assert.AreEqual("Hey", item.String);
                Assert.AreEqual(hijack.Request.RequestUri.AbsoluteUri, expectedUri.AbsoluteUri);
            }
        }

        [AsyncTestMethod]
        public async Task RefreshAsyncWithIntIdTypeAndZeroIdItem()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();
            LongIdType item = new LongIdType() { Id = 0, String = "what?" };
            await table.RefreshAsync(item);

            Assert.AreEqual(0L, item.Id);
            Assert.AreEqual("what?", item.String);
        }

        [AsyncTestMethod]
        public async Task RefreshAsyncWithIntIdTypeAndInvalidIntIdParameter()
        {
            long[] testIdData = IdTestData.InvalidIntIds;

            foreach (long testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"" + testId + "\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();
                Exception exception = null;
                try
                {
                    LongIdType item = new LongIdType() { Id = testId, String = "what?" };
                    await table.RefreshAsync(item);
                }
                catch (Exception e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.IsTrue(exception.Message.Contains(" is not a positive integer value"));
            }
        }

        [AsyncTestMethod]
        public async Task RefreshAsyncWithUserParameters()
        {
            var userDefinedParameters = new Dictionary<string, string>() { { "state", "CA" } };

            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            IMobileServiceTable<StringType> table = service.GetTable<StringType>();

            StringType expected = new StringType();
            expected.Id = 12;

            hijack.SetResponseContent("{\"id\":12,\"String\":\"Goodbye\"}");
            await table.RefreshAsync(expected, userDefinedParameters);

            Assert.Contains(hijack.Request.RequestUri.ToString(), "$filter=(id eq 12)");
            Assert.Contains(hijack.Request.RequestUri.Query, "state=CA");
            Assert.AreEqual(12, expected.Id);
            Assert.AreEqual("Goodbye", expected.String);
        }

        [AsyncTestMethod]
        public async Task RefreshAsyncThrowsWhenNotFound()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            IMobileServiceTable<StringType> table = service.GetTable<StringType>();

            hijack.Response.StatusCode = HttpStatusCode.NotFound;
            InvalidOperationException expected = null;

            try
            {
                await table.RefreshAsync(new StringType() { Id = 5, String = "Just Created" });
            }
            catch (InvalidOperationException e)
            {
                expected = e;
            }

            Assert.IsNotNull(expected);
        }

        #endregion Refresh Tests

        #region Insert Tests

        [AsyncTestMethod]
        public async Task InsertAsyncWithStringIdTypeAndStringIdResponseContent()
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

                IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();

                StringIdType item = new StringIdType() { Id = "an id", String = "what?" };
                await table.InsertAsync(item);

                Assert.AreEqual(testId, item.Id);
                Assert.AreEqual("Hey", item.String);
            }
        }

        [AsyncTestMethod]
        public async Task InsertAsyncWithStringIdTypeAndNonStringIdResponseContent()
        {
            object[] testIdData = IdTestData.ValidIntIds.Concat(
                                  IdTestData.InvalidIntIds).Cast<object>().Concat(
                                  IdTestData.NonStringNonIntValidJsonIds).ToArray();

            foreach (object testId in testIdData)
            {
                string stringTestId = testId.ToString().ToLower();

                TestHttpHandler hijack = new TestHttpHandler();

                hijack.SetResponseContent("{\"id\":" + stringTestId + ",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();

                StringIdType item = new StringIdType() { Id = "an id", String = "what?" };
                await table.InsertAsync(item);

                Assert.AreEqual(testId.ToString(), item.Id);
                Assert.AreEqual("Hey", item.String);
            }
        }

        [AsyncTestMethod]
        public async Task InsertAsyncWithStringIdTypeAndNullIdResponseContent()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":null,\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();

            StringIdType item = new StringIdType() { Id = "an id", String = "what?" };
            await table.InsertAsync(item);

            Assert.AreEqual("an id", item.Id);
            Assert.AreEqual("Hey", item.String);
        }

        [AsyncTestMethod]
        public async Task InsertAsyncWithStringIdTypeAndNoIdResponseContent()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();

            StringIdType item = new StringIdType() { Id = "an id", String = "what?" };
            await table.InsertAsync(item);

            Assert.AreEqual("an id", item.Id);
            Assert.AreEqual("Hey", item.String);
        }

        [AsyncTestMethod]
        public async Task InsertAsyncWithStringIdTypeAndStringIdItem()
        {
            string[] testIdData = IdTestData.ValidStringIds.ToArray();

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"" + testId + "\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();

                hijack.OnSendingRequest = async request =>
                {
                    string requestContent = await request.Content.ReadAsStringAsync();
                    JObject itemAsJObject = JObject.Parse(requestContent);
                    Assert.AreEqual(testId, (string)itemAsJObject["id"]);
                    Assert.AreEqual("what?", (string)itemAsJObject["String"]);
                    return request;
                };

                StringIdType item = new StringIdType() { Id = testId, String = "what?" };
                await table.InsertAsync(item);

                Assert.AreEqual(testId, item.Id);
                Assert.AreEqual("Hey", item.String);
            }
        }

        [AsyncTestMethod]
        public async Task InsertAsyncWithStringIdTypeAndEmptyStringIdItem()
        {
            string[] testIdData = IdTestData.EmptyStringIds;

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"an id\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();

                hijack.OnSendingRequest = async request =>
                {
                    string requestContent = await request.Content.ReadAsStringAsync();
                    JObject itemAsJObject = JObject.Parse(requestContent);
                    Assert.AreEqual(testId, (string)itemAsJObject["id"]);
                    Assert.AreEqual("what?", (string)itemAsJObject["String"]);
                    return request;
                };

                StringIdType item = new StringIdType() { Id = testId, String = "what?" };
                await table.InsertAsync(item);

                Assert.AreEqual("an id", item.Id);
                Assert.AreEqual("Hey", item.String);
            }
        }

        [AsyncTestMethod]
        public async Task InsertAsyncWithStringIdTypeAndNullIdItem()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":\"an id\",\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();

            hijack.OnSendingRequest = async request =>
            {
                string requestContent = await request.Content.ReadAsStringAsync();
                JObject itemAsJObject = JObject.Parse(requestContent);
                Assert.AreEqual(null, (string)itemAsJObject["id"]);
                Assert.AreEqual("what?", (string)itemAsJObject["String"]);
                return request;
            };

            StringIdType item = new StringIdType() { Id = null, String = "what?" };
            await table.InsertAsync(item);

            Assert.AreEqual("an id", item.Id);
            Assert.AreEqual("Hey", item.String);
        }

        [AsyncTestMethod]
        public async Task InsertAsyncWithStringIdTypeAndInvalidStringIdParameter()
        {
            string[] testIdData = IdTestData.InvalidStringIds;

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"" + testId + "\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();
                Exception exception = null;
                try
                {
                    StringIdType item = new StringIdType() { Id = testId, String = "what?" };
                    await table.UpdateAsync(item);
                }
                catch (Exception e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.IsTrue(exception.Message.Contains("An id must not contain any control characters or the characters") || 
                              exception.Message.Contains("is longer than the max string id length of 255 characters"));
            }
        }

        [AsyncTestMethod]
        public async Task InsertAsyncWithIntIdTypeAndIntIdResponseContent()
        {
            long[] testIdData = IdTestData.ValidIntIds.Concat(
                                IdTestData.InvalidIntIds).ToArray();

            foreach (long testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":" + testId + ",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();

                LongIdType item = new LongIdType() { String = "what?" };
                await table.InsertAsync(item);

                Assert.AreEqual(testId, item.Id);
                Assert.AreEqual("Hey", item.String);
            }
        }

        [AsyncTestMethod]
        public async Task InsertAsyncWithIntIdTypeParseableIdResponseContent()
        {
            object[] testIdData = IdTestData.NonStringNonIntValidJsonIds;

            foreach (object testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":" + testId.ToString().ToLower() + ",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();

                LongIdType item = new LongIdType() { String = "what?" };
                await table.InsertAsync(item);

                long expectedId = Convert.ToInt64(testId);
                Assert.AreEqual(expectedId, item.Id);
                Assert.AreEqual("Hey", item.String);
            }
        }

        [AsyncTestMethod]
        public async Task InsertAsyncWithIntIdTypeAndNullIdResponseContent()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":null,\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();

            LongIdType item = new LongIdType() { String = "what?" };
            await table.InsertAsync(item);

            Assert.AreEqual(0L, item.Id);
            Assert.AreEqual("Hey", item.String);
        }

        [AsyncTestMethod]
        public async Task InsertAsyncWithIntIdTypeAndNoIdResponseContent()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();

            LongIdType item = new LongIdType() { String = "what?" };
            await table.InsertAsync(item);

            Assert.AreEqual(0L, item.Id);
            Assert.AreEqual("Hey", item.String);
        }

        [AsyncTestMethod]
        public async Task InsertAsyncWithIntIdTypeAndStringIdResponseContent()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":\"an id\",\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();

            Exception exception = null;
            try
            {
                LongIdType item = new LongIdType() { String = "what?" };
                await table.InsertAsync(item);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.IsTrue(exception.Message.Contains("Error converting value"));
        }

        [AsyncTestMethod]
        public async Task InsertAsyncWithIntIdTypeAndIntIdItem()
        {
            long[] testIdData = IdTestData.ValidIntIds;

            foreach (long testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":" + testId + ",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();

                Exception exception = null;
                try
                {
                    LongIdType item = new LongIdType() { Id = testId, String = "what?" };
                    await table.InsertAsync(item);
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
        public async Task InsertAsyncWithIntIdTypeAndZeroIdItem()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":10,\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();
            LongIdType item = new LongIdType() { Id = 0, String = "what?" };
            await table.InsertAsync(item);

            Assert.AreEqual(10L, item.Id);
            Assert.AreEqual("Hey", item.String);
        }

        [AsyncTestMethod]
        public async Task InsertAsyncWithIntIdTypeAndInvalidIntIdParameter()
        {
            long[] testIdData = IdTestData.InvalidIntIds;

            foreach (long testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"" + testId + "\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();
                Exception exception = null;
                try
                {
                    LongIdType item = new LongIdType() { Id = testId, String = "what?" };
                    await table.InsertAsync(item);
                }
                catch (Exception e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.IsTrue(exception.Message.Contains(" is not a positive integer value") ||
                              exception.Message.Contains("for member id is outside the valid range for numeric columns"));
            }
        }

        [AsyncTestMethod]
        public async Task InsertAsyncGenericWithUserParameters()
        {
            var userDefinedParameters = new Dictionary<string, string>() { { "state", "CA" } };

            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            IMobileServiceTable<StringType> table = service.GetTable<StringType>();

            StringType obj = new StringType();
            obj.String = "new";

            hijack.SetResponseContent("{\"id\":12,\"value\":\"new\"}");
            await table.InsertAsync(obj, userDefinedParameters);

            Assert.AreEqual(12, obj.Id);
            Assert.Contains(hijack.Request.RequestUri.ToString(), "StringType");
            Assert.Contains(hijack.Request.RequestUri.Query, "state=CA");
        }

        #endregion Insert Tests

        #region Update Tests

        [AsyncTestMethod]
        public async Task UpdateAsyncWithStringIdTypeAndStringIdResponseContent()
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

                IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();

                StringIdType item = new StringIdType() { Id = "an id", String = "what?" };
                await table.UpdateAsync(item);

                Assert.AreEqual(testId, item.Id);
                Assert.AreEqual("Hey", item.String);
            }
        }

        [AsyncTestMethod]
        public async Task UpdateAsyncWithStringIdTypeAndNonStringIdResponseContent()
        {
            object[] testIdData = IdTestData.ValidIntIds.Concat(
                                  IdTestData.InvalidIntIds).Cast<object>().Concat(
                                  IdTestData.NonStringNonIntValidJsonIds).ToArray();

            foreach (object testId in testIdData)
            {
                string stringTestId = testId.ToString().ToLower();

                TestHttpHandler hijack = new TestHttpHandler();

                hijack.SetResponseContent("{\"id\":" + stringTestId.ToLower() + ",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();

                StringIdType item = new StringIdType() { Id = "an id", String = "what?" };
                await table.UpdateAsync(item);

                Assert.AreEqual(testId.ToString(), item.Id);
                Assert.AreEqual("Hey", item.String);
            }
        }

        [AsyncTestMethod]
        public async Task UpdateAsyncWithStringIdTypeAndNullIdResponseContent()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":null,\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();

            StringIdType item = new StringIdType() { Id = "an id", String = "what?" };
            await table.UpdateAsync(item);

            Assert.AreEqual("an id", item.Id);
            Assert.AreEqual("Hey", item.String);
        }

        [AsyncTestMethod]
        public async Task UpdateAsyncWithStringIdTypeAndNoIdResponseContent()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();

            StringIdType item = new StringIdType() { Id = "an id", String = "what?" };
            await table.UpdateAsync(item);

            Assert.AreEqual("an id", item.Id);
            Assert.AreEqual("Hey", item.String);
        }

        [AsyncTestMethod]
        public async Task UpdateAsyncWithStringIdTypeAndStringIdItem()
        {
            string[] testIdData = IdTestData.ValidStringIds.ToArray();

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"" + testId + "\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();

                hijack.OnSendingRequest = async request =>
                {
                    string requestContent = await request.Content.ReadAsStringAsync();
                    JObject itemAsJObject = JObject.Parse(requestContent);
                    Assert.AreEqual(testId, (string)itemAsJObject["id"]);
                    Assert.AreEqual("what?", (string)itemAsJObject["String"]);
                    string idForUri = Uri.EscapeDataString(testId);
                    Uri expectedUri = new Uri(string.Format("http://www.test.com/tables/StringIdType/{0}", idForUri));
                    Assert.AreEqual(request.RequestUri.AbsoluteUri, expectedUri.AbsoluteUri);
                    return request;
                };

                StringIdType item = new StringIdType() { Id = testId, String = "what?" };
                await table.UpdateAsync(item);

                Assert.AreEqual(testId, item.Id);
                Assert.AreEqual("Hey", item.String);
            }
        }

        [AsyncTestMethod]
        public async Task UpdateAsyncWithStringIdTypeAndEmptyStringIdItem()
        {
            string[] testIdData = IdTestData.EmptyStringIds;

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"an id\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();
                Exception exception = null;
                try
                {
                    StringIdType item = new StringIdType() { Id = testId, String = "what?" };
                    await table.UpdateAsync(item);
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
        public async Task UpdateAsyncWithStringIdTypeAndNullIdItem()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":\"an id\",\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();

            Exception exception = null;
            try
            {
                StringIdType item = new StringIdType() { Id = null, String = "what?" };
                await table.UpdateAsync(item);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.IsTrue(exception.Message.Contains("Expected id member not found."));
        }

        [AsyncTestMethod]
        public async Task UpdateAsyncWithStringIdTypeAndInvalidStringIdParameter()
        {
            string[] testIdData = IdTestData.InvalidStringIds;

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"" + testId + "\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();
                Exception exception = null;
                try
                {
                    StringIdType item = new StringIdType() { Id = testId, String = "what?" };
                    await table.UpdateAsync(item);
                }
                catch (Exception e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.IsTrue(exception.Message.Contains("An id must not contain any control characters or the characters") || 
                              exception.Message.Contains("is longer than the max string id length of 255 characters"));
            }
        }

        [AsyncTestMethod]
        public async Task UpdateAsyncWithIntIdTypeAndIntIdResponseContent()
        {
            long[] testIdData = IdTestData.ValidIntIds.Concat(
                                IdTestData.InvalidIntIds).ToArray();

            foreach (long testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":" + testId + ",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();

                LongIdType item = new LongIdType() { Id = 12, String = "what?" };
                await table.UpdateAsync(item);

                Assert.AreEqual(testId, item.Id);
                Assert.AreEqual("Hey", item.String);
            }
        }

        [AsyncTestMethod]
        public async Task UpdateAsyncWithIntIdTypeParseableIdResponseContent()
        {
            object[] testIdData = IdTestData.NonStringNonIntValidJsonIds;

            foreach (object testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":" + testId.ToString().ToLower() + ",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();

                LongIdType item = new LongIdType() { Id = 12, String = "what?" };
                await table.UpdateAsync(item);

                long expectedId = Convert.ToInt64(testId);
                if (expectedId == 0L)
                {
                    expectedId = 12;
                }

                Assert.AreEqual(expectedId, item.Id);
                Assert.AreEqual("Hey", item.String);
            }
        }

        [AsyncTestMethod]
        public async Task UpdateAsyncWithIntIdTypeAndNullIdResponseContent()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":null,\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();

            LongIdType item = new LongIdType() { Id = 12, String = "what?" };
            await table.UpdateAsync(item);

            Assert.AreEqual(12L, item.Id);
            Assert.AreEqual("Hey", item.String);
        }

        [AsyncTestMethod]
        public async Task UpdateAsyncWithIntIdTypeAndNoIdResponseContent()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();

            LongIdType item = new LongIdType() { Id = 12, String = "what?" };
            await table.UpdateAsync(item);

            Assert.AreEqual(12L, item.Id);
            Assert.AreEqual("Hey", item.String);
        }

        [AsyncTestMethod]
        public async Task UpdateAsyncWithIntIdTypeAndStringIdResponseContent()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":\"an id\",\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();

            Exception exception = null;
            try
            {
                LongIdType item = new LongIdType() { Id = 12, String = "what?" };
                await table.UpdateAsync(item);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.IsTrue(exception.Message.Contains("Error converting value"));
        }

        [AsyncTestMethod]
        public async Task UpdateAsyncWithIntIdTypeAndIntIdItem()
        {
            long[] testIdData = IdTestData.ValidIntIds
                                          .Where( id => id != long.MaxValue) // Max value fails for serialization reasons; not because of id constraints
                                          .ToArray();

            foreach (long testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":" + testId + ",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();
                LongIdType item = new LongIdType() { Id = testId, String = "what?" };
                await table.UpdateAsync(item);

                Assert.AreEqual(testId, item.Id);
                Assert.AreEqual("Hey", item.String);
            }
        }

        [AsyncTestMethod]
        public async Task UpdateAsyncWithIntIdTypeAndZeroIdItem()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":10,\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();
            Exception exception = null;
            try
            {
                LongIdType item = new LongIdType() { Id = 0, String = "what?" };
                await table.UpdateAsync(item);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.IsTrue(exception.Message.Contains("Expected id member not found."));
        }

        [AsyncTestMethod]
        public async Task UpdateAsyncWithIntIdTypeAndInvalidIntIdParameter()
        {
            long[] testIdData = IdTestData.InvalidIntIds;

            foreach (long testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"" + testId + "\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();
                Exception exception = null;
                try
                {
                    LongIdType item = new LongIdType() { Id = testId, String = "what?" };
                    await table.UpdateAsync(item);
                }
                catch (Exception e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.IsTrue(exception.Message.Contains(" is not a positive integer value") ||
                              exception.Message.Contains("for member id is outside the valid range for numeric columns"));
            }
        }

        [AsyncTestMethod]
        public async Task UpdateAsyncGenericWithUserParameters()
        {
            var userDefinedParameters = new Dictionary<string, string>() { { "state", "FL" } };

            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            IMobileServiceTable<StringType> table = service.GetTable<StringType>();

            StringType obj = new StringType();
            obj.Id = 12;
            obj.String = "new";

            hijack.SetResponseContent("{\"Id\":12,\"String\":\"new1\"}");

            await table.UpdateAsync(obj, userDefinedParameters);

            Assert.AreEqual("new1", obj.String);
            Assert.Contains(hijack.Request.RequestUri.ToString(), "StringType");
            Assert.Contains(hijack.Request.RequestUri.Query, "state=FL");
        }

        #endregion Update Tests

        #region Delete Tests

        [AsyncTestMethod]
        public async Task DeleteAsyncWithStringIdTypeAndStringIdItem()
        {
            string[] testIdData = IdTestData.ValidStringIds;

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"" + testId + "\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();

                hijack.OnSendingRequest = request =>
                {
                    Assert.IsNull(request.Content);
                    string idForUri = Uri.EscapeDataString(testId);
                    Uri expectedUri = new Uri(string.Format("http://www.test.com/tables/StringIdType/{0}", idForUri));
                    Assert.AreEqual(request.RequestUri.AbsoluteUri, expectedUri.AbsoluteUri);
                    return new TaskFactory<HttpRequestMessage>().StartNew(() => request);
                };

                StringIdType item = new StringIdType() { Id = testId, String = "what?" };
                await table.DeleteAsync(item);

                Assert.AreEqual(null, item.Id);
                Assert.AreEqual("what?", item.String);
            }
        }

        [AsyncTestMethod]
        public async Task DeleteAsyncWithStringIdTypeAndEmptyStringIdItem()
        {
            string[] testIdData = IdTestData.EmptyStringIds;

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"an id\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();
                Exception exception = null;
                try
                {
                    StringIdType item = new StringIdType() { Id = testId, String = "what?" };
                    await table.DeleteAsync(item);
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
        public async Task DeleteAsyncWithStringIdTypeAndNullIdItem()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":\"an id\",\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();

            Exception exception = null;
            try
            {
                StringIdType item = new StringIdType() { Id = null, String = "what?" };
                await table.DeleteAsync(item);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.IsTrue(exception.Message.Contains("Expected id member not found."));
        }

        [AsyncTestMethod]
        public async Task DeleteAsyncWithStringIdTypeAndInvalidStringIdParameter()
        {
            string[] testIdData = IdTestData.InvalidStringIds;

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"" + testId + "\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();
                Exception exception = null;
                try
                {
                    StringIdType item = new StringIdType() { Id = testId, String = "what?" };
                    await table.DeleteAsync(item);
                }
                catch (Exception e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.IsTrue(exception.Message.Contains("An id must not contain any control characters or the characters") || 
                              exception.Message.Contains("is longer than the max string id length of 255 characters"));
            }
        }

        [AsyncTestMethod]
        public async Task DeleteAsyncWithIntIdTypeAndIntIdItem()
        {
            long[] testIdData = IdTestData.ValidIntIds
                                          .Where(id => id != long.MaxValue) // Max value fails for serialization reasons; not because of id constraints
                                          .ToArray();

            foreach (long testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":" + testId + ",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();
                LongIdType item = new LongIdType() { Id = testId, String = "what?" };
                await table.DeleteAsync(item);

                Assert.AreEqual(0L, item.Id);
                Assert.AreEqual("what?", item.String);
            }
        }

        [AsyncTestMethod]
        public async Task DeleteAsyncWithIntIdTypeAndZeroIdItem()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":10,\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();
            Exception exception = null;
            try
            {
                LongIdType item = new LongIdType() { Id = 0, String = "what?" };
                await table.DeleteAsync(item);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.IsTrue(exception.Message.Contains("Expected id member not found."));
        }

        [AsyncTestMethod]
        public async Task DeleteAsyncWithIntIdTypeAndInvalidIntIdParameter()
        {
            long[] testIdData = IdTestData.InvalidIntIds;

            foreach (long testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"" + testId + "\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<LongIdType> table = service.GetTable<LongIdType>();
                Exception exception = null;
                try
                {
                    LongIdType item = new LongIdType() { Id = testId, String = "what?" };
                    await table.DeleteAsync(item);
                }
                catch (Exception e)
                {
                    exception = e;
                }

                Assert.IsNotNull(exception);
                Assert.IsTrue(exception.Message.Contains(" is not a positive integer value") ||
                              exception.Message.Contains("for member id is outside the valid range for numeric columns"));
            }
        }

        [AsyncTestMethod]
        public async Task DeleteAsyncWithUserParameters()
        {
            var userDefinedParameters = new Dictionary<string, string>() { { "state", "WY" } };

            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            IMobileServiceTable<StringType> table = service.GetTable<StringType>();

            StringType obj = new StringType();
            obj.Id = 12;
            obj.String = "new";

            await table.DeleteAsync(obj, userDefinedParameters);

            Assert.AreEqual(0, obj.Id);
            Assert.Contains(hijack.Request.RequestUri.ToString(), "StringType");
            Assert.IsNull(hijack.Request.Content);
            Assert.Contains(hijack.Request.RequestUri.Query, "state=WY");
        }

        #endregion Delete Tests

        #region Query Tests

        [TestMethod]
        public void CreateQueryGeneric()
        {
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            IMobileServiceTable<StringType> table = service.GetTable<StringType>();

            IMobileServiceTableQuery<StringType> query = table.CreateQuery();

            Assert.IsNotNull(query);
        }

        [AsyncTestMethod]
        public async Task IncludeTotalGenericList()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"results\":[{\"id\":12,\"String\":\"Hey\"}], \"count\":1}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<StringType> table = service.GetTable<StringType>();

            TotalCountList<StringType> people = (TotalCountList<StringType>) await table.IncludeTotalCount().ToListAsync();

            Assert.Contains(hijack.Request.RequestUri.ToString(), "StringType");
            Assert.Contains(hijack.Request.RequestUri.ToString(), "$inlinecount=allpages");

            Assert.AreEqual((long)1, people.TotalCount);
            Assert.AreEqual(12, people[0].Id);
            Assert.AreEqual("Hey", people[0].String);
        }

        [AsyncTestMethod]
        public async Task IncludeTotalGenericEnum()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"results\":[{\"id\":12,\"String\":\"Hey\"}], \"count\":1}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<StringType> table = service.GetTable<StringType>();

            TotalCountEnumerable<StringType> results = (TotalCountEnumerable<StringType>)await table.IncludeTotalCount().ToEnumerableAsync();
            StringType[] people = results.Cast<StringType>().ToArray();

            Assert.Contains(hijack.Request.RequestUri.ToString(), "StringType");
            Assert.Contains(hijack.Request.RequestUri.ToString(), "$inlinecount=allpages");

            Assert.AreEqual((long)1, results.TotalCount);
            Assert.AreEqual(12, people[0].Id);
            Assert.AreEqual("Hey", people[0].String);
        }

        [AsyncTestMethod]
        public async Task WithParametersAsyncGeneric()
        {
            var userDefinedParameters = new Dictionary<string, string>() { { "state", "WY" } };

            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("[{\"id\":12,\"String\":\"Hey\"}]");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<StringType> table = service.GetTable<StringType>();
            List<StringType> people = await table.WithParameters(userDefinedParameters).ToListAsync();

            Assert.Contains(hijack.Request.RequestUri.ToString(), "StringType");
            Assert.Contains(hijack.Request.RequestUri.Query, "state=WY");

            Assert.AreEqual(1, people.Count);
            Assert.AreEqual(12, people[0].Id);
            Assert.AreEqual("Hey", people[0].String);
        }

        [AsyncTestMethod]
        public async Task WhereAsyncGeneric()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("[{\"id\":12,\"String\":\"Hey\"}]");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<StringType> table = service.GetTable<StringType>();
            List<StringType> people = await table.Where(p => p.Id == 12).ToListAsync();

            Assert.Contains(hijack.Request.RequestUri.ToString(), "StringType");
            Assert.Contains(hijack.Request.RequestUri.ToString(), "$filter=(id eq 12)");

            Assert.AreEqual(1, people.Count);
            Assert.AreEqual(12, people[0].Id);
            Assert.AreEqual("Hey", people[0].String);
        }

        [AsyncTestMethod]
        public async Task WhereAsyncWithStringIdGeneric()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("[{\"id\":12,\"String\":\"Hey\"}]");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<StringIdType> table = service.GetTable<StringIdType>();
            List<StringIdType> people = await table.Where(p => p.Id == "12").ToListAsync();

            Assert.Contains(hijack.Request.RequestUri.ToString(), "StringIdType");
            Assert.Contains(hijack.Request.RequestUri.ToString(), "$filter=(id eq '12')");

            Assert.AreEqual(1, people.Count);
            Assert.AreEqual("12", people[0].Id);
            Assert.AreEqual("Hey", people[0].String);
        }

        [AsyncTestMethod]
        public async Task SelectAsyncGeneric()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("[{\"id\":12,\"String\":\"Hey\"}]");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<StringType> table = service.GetTable<StringType>();
            StringType me = new StringType();
            me.Id = 10;
            me.String = "apple";

            List<string> people = await table.Select(p => p.String).ToListAsync();
            
            Assert.Contains(hijack.Request.RequestUri.ToString(), "StringType");

            Assert.AreEqual(1, people.Count);
            Assert.AreEqual("Hey", people[0]);
        }

        [AsyncTestMethod]
        public async Task OrderByAsyncGeneric()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("[]");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<StringType> table = service.GetTable<StringType>();
            List<StringType> people = await table.OrderBy(p => p.Id).ThenBy(p => p.String).ToListAsync();

            Assert.Contains(hijack.Request.RequestUri.ToString(), "StringType");
            Assert.Contains(hijack.Request.RequestUri.ToString(), "orderby=id,String");
        }

        [AsyncTestMethod]
        public async Task OrderByDoubleAsyncGeneric()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("[]");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<StringType> table = service.GetTable<StringType>();
            List<StringType> people = await table.OrderBy(p => p.Id).OrderBy(p => p.String).ToListAsync();

            Assert.Contains(hijack.Request.RequestUri.ToString(), "StringType");
            Assert.Contains(hijack.Request.RequestUri.ToString(), "orderby=String,id");
        }

        [AsyncTestMethod]
        public async Task OrderByDescAsyncGeneric()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("[]");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<StringType> table = service.GetTable<StringType>();
            List<StringType> people = await table.OrderByDescending(p => p.Id).ThenByDescending(p => p.String).ToListAsync();

            Assert.Contains(hijack.Request.RequestUri.ToString(), "StringType");
            Assert.Contains(hijack.Request.RequestUri.ToString(), "orderby=id desc,String desc");
        }

        [AsyncTestMethod]
        public async Task OrderByAscDescAsyncGeneric()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("[]");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<StringType> table = service.GetTable<StringType>();
            List<StringType> people = await table.OrderBy(p => p.Id).ThenByDescending(p => p.String).ToListAsync();

            Assert.Contains(hijack.Request.RequestUri.ToString(), "StringType");
            Assert.Contains(hijack.Request.RequestUri.ToString(), "orderby=id,String desc");
        }

        [AsyncTestMethod]
        public async Task SkipAndTakeAsyncGeneric()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("[]");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<StringType> table = service.GetTable<StringType>();
            List<StringType> people = await table.Skip(100).Take(10).ToListAsync();

            Assert.Contains(hijack.Request.RequestUri.ToString(), "StringType");
            Assert.Contains(hijack.Request.RequestUri.ToString(), "$skip=100");
            Assert.Contains(hijack.Request.RequestUri.ToString(), "$top=10");
        }

        #endregion Query Tests

        #region System Properties Tests

        [AsyncTestMethod]
        public async Task InsertAsyncStringIdSystemPropertiesRemovedFromRequest()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":\"an id\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<CreatedAtType> createdAtTable = service.GetTable<CreatedAtType>();

            hijack.OnSendingRequest = async (request) =>
            {
                string content = await request.Content.ReadAsStringAsync();
                JObject obj = JToken.Parse(content) as JObject;
                Assert.IsTrue(obj.Properties().Where(p => p.Name == "id").Any());
                Assert.IsFalse(obj.Properties().Where(p => p.Name.Contains("created")).Any());
                return request;
            };

            await createdAtTable.InsertAsync(new CreatedAtType() { CreatedAt = new DateTime(2012, 1, 8), Id = "an id" });

            hijack.SetResponseContent("{\"id\":\"an id\"}");

            IMobileServiceTable<NamedSystemPropertiesType> namedCreatedAtTable = service.GetTable<NamedSystemPropertiesType>();

            hijack.OnSendingRequest = async (request) =>
            {
                string content = await request.Content.ReadAsStringAsync();
                JObject obj = JToken.Parse(content) as JObject;
                Assert.IsTrue(obj.Properties().Where(p => p.Name == "id").Any());
                Assert.IsFalse(obj.Properties().Where(p => p.Name.Contains("__createdAt")).Any());
                return request;
            };

            await namedCreatedAtTable.InsertAsync(new NamedSystemPropertiesType() { __createdAt = new DateTime(2012, 1, 8), Id = "an id" });

            hijack.SetResponseContent("{\"id\":\"an id\"}");

            IMobileServiceTable<UpdatedAtType> updatedAtTable = service.GetTable<UpdatedAtType>();

            hijack.OnSendingRequest = async (request) =>
            {
                string content = await request.Content.ReadAsStringAsync();
                JObject obj = JToken.Parse(content) as JObject;
                Assert.IsTrue(obj.Properties().Where(p => p.Name == "id").Any());
                Assert.IsFalse(obj.Properties().Where(p => p.Name.Contains("updated")).Any());
                return request;
            };

            await updatedAtTable.InsertAsync(new UpdatedAtType() { UpdatedAt = new DateTime(2012, 1, 8), Id = "an id" });

            hijack.SetResponseContent("{\"id\":\"an id\"}");

            IMobileServiceTable<VersionType> versionTable = service.GetTable<VersionType>();

            hijack.OnSendingRequest = async (request) =>
            {
                string content = await request.Content.ReadAsStringAsync();
                JObject obj = JToken.Parse(content) as JObject;
                Assert.IsTrue(obj.Properties().Where(p => p.Name == "id").Any());
                Assert.IsFalse(obj.Properties().Where(p => p.Name.Contains("version")).Any());
                return request;
            };

            await versionTable.InsertAsync(new VersionType() { Version = "a version", Id = "an id" });

            hijack.SetResponseContent("{\"id\":\"an id\"}");

            IMobileServiceTable<AllSystemPropertiesType> allsystemPropertiesTable = service.GetTable<AllSystemPropertiesType>();

            hijack.OnSendingRequest = async (request) =>
            {
                string content = await request.Content.ReadAsStringAsync();
                JObject obj = JToken.Parse(content) as JObject;
                Assert.IsTrue(obj.Properties().Where(p => p.Name == "id").Any());
                Assert.IsFalse(obj.Properties().Where(p => p.Name.Contains("version")).Any());
                Assert.IsFalse(obj.Properties().Where(p => p.Name.Contains("created")).Any());
                Assert.IsFalse(obj.Properties().Where(p => p.Name.Contains("updated")).Any());
                return request;
            };

            await allsystemPropertiesTable.InsertAsync(new AllSystemPropertiesType() 
            { 
                Version = "a version", 
                UpdatedAt = new DateTime(2012, 1, 8),
                CreatedAt = new DateTime(2012, 1, 8),
                Id = "an id"
            });
        }

        [AsyncTestMethod]
        public async Task InsertAsyncStringIdNonSystemPropertiesNotRemovedFromRequest()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":\"an id\"}");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<NotSystemPropertyCreatedAtType> createdAtTable = service.GetTable<NotSystemPropertyCreatedAtType>();

            hijack.OnSendingRequest = async (request) =>
            {
                string content = await request.Content.ReadAsStringAsync();
                JObject obj = JToken.Parse(content) as JObject;
                Assert.IsTrue(obj.Properties().Where(p => p.Name == "id").Any());
                Assert.IsTrue(obj.Properties().Where(p => p.Name.Contains("Created")).Any());
                return request;
            };

            await createdAtTable.InsertAsync(new NotSystemPropertyCreatedAtType() { CreatedAt = new DateTime(2012, 1, 8), Id = "an id" });

            hijack.SetResponseContent("{\"id\":\"an id\"}");

            IMobileServiceTable<NotSystemPropertyUpdatedAtType> updatedAtTable = service.GetTable<NotSystemPropertyUpdatedAtType>();

            hijack.OnSendingRequest = async (request) =>
            {
                string content = await request.Content.ReadAsStringAsync();
                JObject obj = JToken.Parse(content) as JObject;
                Assert.IsTrue(obj.Properties().Where(p => p.Name == "id").Any());
                Assert.IsTrue(obj.Properties().Where(p => p.Name.Contains("Updated")).Any());
                return request;
            };

            await updatedAtTable.InsertAsync(new NotSystemPropertyUpdatedAtType() { _UpdatedAt = new DateTime(2012, 1, 8), Id = "an id" });

            hijack.SetResponseContent("{\"id\":\"an id\"}");

            IMobileServiceTable<NotSystemPropertyVersionType> versionTable = service.GetTable<NotSystemPropertyVersionType>();

            hijack.OnSendingRequest = async (request) =>
            {
                string content = await request.Content.ReadAsStringAsync();
                JObject obj = JToken.Parse(content) as JObject;
                Assert.IsTrue(obj.Properties().Where(p => p.Name == "id").Any());
                Assert.IsTrue(obj.Properties().Where(p => p.Name.Contains("version")).Any());
                return request;
            };

            await versionTable.InsertAsync(new NotSystemPropertyVersionType() { version = "a version", Id = "an id" });
        }

        [TestMethod]
        public void SystemPropertiesPropertySetCorrectly()
        {
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");

            IMobileServiceTable<StringIdType> stringIdTable = service.GetTable<StringIdType>();
            Assert.AreEqual(stringIdTable.SystemProperties, MobileServiceSystemProperties.None);

            IMobileServiceTable<StringType> stringTable = service.GetTable<StringType>();
            Assert.AreEqual(stringTable.SystemProperties, MobileServiceSystemProperties.None);

            IMobileServiceTable<NotSystemPropertyCreatedAtType> notSystemPropertyTable = service.GetTable<NotSystemPropertyCreatedAtType>();
            Assert.AreEqual(notSystemPropertyTable.SystemProperties, MobileServiceSystemProperties.None);

            IMobileServiceTable<IntegerIdNotSystemPropertyCreatedAtType> integerIdNotsystemPropertyTable = service.GetTable<IntegerIdNotSystemPropertyCreatedAtType>();
            Assert.AreEqual(integerIdNotsystemPropertyTable.SystemProperties, MobileServiceSystemProperties.None);

            IMobileServiceTable<NotSystemPropertyUpdatedAtType> notSystemPropertyUpdatedTable = service.GetTable<NotSystemPropertyUpdatedAtType>();
            Assert.AreEqual(notSystemPropertyUpdatedTable.SystemProperties, MobileServiceSystemProperties.None);

            IMobileServiceTable<NotSystemPropertyVersionType> notSystemPropertyVersionTable = service.GetTable<NotSystemPropertyVersionType>();
            Assert.AreEqual(notSystemPropertyVersionTable.SystemProperties, MobileServiceSystemProperties.None);

            IMobileServiceTable<IntegerIdWithNamedSystemPropertiesType> integerIdWithNamedSystemPropertyTable = service.GetTable<IntegerIdWithNamedSystemPropertiesType>();
            Assert.AreEqual(integerIdWithNamedSystemPropertyTable.SystemProperties, MobileServiceSystemProperties.None);

            IMobileServiceTable<LongIdWithNamedSystemPropertiesType> longIdWithNamedSystemPropertyTable = service.GetTable<LongIdWithNamedSystemPropertiesType>();
            Assert.AreEqual(longIdWithNamedSystemPropertyTable.SystemProperties, MobileServiceSystemProperties.None);

            IMobileServiceTable<CreatedAtType> createdAtTable = service.GetTable<CreatedAtType>();
            Assert.AreEqual(createdAtTable.SystemProperties, MobileServiceSystemProperties.CreatedAt);

            IMobileServiceTable<DoubleNamedSystemPropertiesType> doubleNamedCreatedAtTable = service.GetTable<DoubleNamedSystemPropertiesType>();
            Assert.AreEqual(doubleNamedCreatedAtTable.SystemProperties, MobileServiceSystemProperties.CreatedAt);

            IMobileServiceTable<NamedSystemPropertiesType> namedCreatedAtTable = service.GetTable<NamedSystemPropertiesType>();
            Assert.AreEqual(namedCreatedAtTable.SystemProperties, MobileServiceSystemProperties.CreatedAt);

            IMobileServiceTable<NamedDifferentCasingSystemPropertiesType> namedDifferentCasingCreatedAtTable = service.GetTable<NamedDifferentCasingSystemPropertiesType>();
            Assert.AreEqual(namedDifferentCasingCreatedAtTable.SystemProperties, MobileServiceSystemProperties.CreatedAt);

            IMobileServiceTable<UpdatedAtType> updatedAtTable = service.GetTable<UpdatedAtType>();
            Assert.AreEqual(updatedAtTable.SystemProperties, MobileServiceSystemProperties.UpdatedAt);

            IMobileServiceTable<VersionType> versionTable = service.GetTable<VersionType>();
            Assert.AreEqual(versionTable.SystemProperties, MobileServiceSystemProperties.Version);

            IMobileServiceTable<AllSystemPropertiesType> allsystemPropertiesTable = service.GetTable<AllSystemPropertiesType>();
            Assert.AreEqual(allsystemPropertiesTable.SystemProperties, MobileServiceSystemProperties.Version |
                                                                       MobileServiceSystemProperties.CreatedAt |
                                                                       MobileServiceSystemProperties.UpdatedAt);
        }

        [TestMethod]
        public void IntegerIdTypesCanNotHaveSystemPropertyAttributes()
        {
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            Exception exception = null;

            try
            {
                IMobileServiceTable<IntegerIdWithSystemPropertiesType> stringIdTable = service.GetTable<IntegerIdWithSystemPropertiesType>();
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.IsTrue(exception.Message.Contains("has an integer id member and therefore can not have any members with the system property attribute"));
            exception = null;

            try
            {
                IMobileServiceTable<LongIdWithSystemPropertiesType> stringIdTable = service.GetTable<LongIdWithSystemPropertiesType>();
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.IsTrue(exception.Message.Contains("has an integer id member and therefore can not have any members with the system property attribute"));
            
        }

        [TestMethod]
        public void TypesCanNotHaveMultiplePropertiesWithTheSameSystemAttribute()
        {
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            Exception exception = null;

            try
            {
                IMobileServiceTable<MultipleSystemPropertiesType> stringIdTable = service.GetTable<MultipleSystemPropertiesType>();
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.IsTrue(exception.Message.Contains("Only one member may have the property name"));
            exception = null;

            try
            {
                IMobileServiceTable<NamedAndAttributedSystemPropertiesType> stringIdTable = service.GetTable<NamedAndAttributedSystemPropertiesType>();
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.IsTrue(exception.Message.Contains("Only one member may have the property name"));

            try
            {
                IMobileServiceTable<DoubleJsonPropertyNamedSystemPropertiesType> stringIdTable = service.GetTable<DoubleJsonPropertyNamedSystemPropertiesType>();
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.IsTrue(exception.Message.Contains("Only one member may have the property name"));
        }

        [AsyncTestMethod]
        public async Task CreatedAtSystemPropertyDeserializesToDatetimeOrString()
        {
            TestHttpHandler hijack = new TestHttpHandler();

            hijack.SetResponseContent("[{\"id\":\"an id\",\"__createdAt\":\"2000-01-01T07:59:59.000Z\"}]");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<CreatedAtType> table = service.GetTable<CreatedAtType>();

            IEnumerable<CreatedAtType> results = await table.ReadAsync();
            CreatedAtType[] items = results.ToArray();

            Assert.AreEqual(1, items.Count());
            Assert.AreEqual("an id", items[0].Id);
            Assert.AreEqual(new DateTime(1999, 12, 31, 23, 59, 59), items[0].CreatedAt);

            hijack.SetResponseContent("[{\"id\":\"an id\",\"__createdAt\":\"2000-01-01T07:59:59.000Z\"}]");
            IMobileServiceTable<StringCreatedAtType> stringTable = service.GetTable<StringCreatedAtType>();

            IEnumerable<StringCreatedAtType> stringResults = await stringTable.ReadAsync();
            StringCreatedAtType[] stringItems = stringResults.ToArray();

            Assert.AreEqual(1, stringItems.Count());
            Assert.AreEqual("an id", stringItems[0].Id);
            Assert.AreEqual("01/01/2000 07:59:59", stringItems[0].CreatedAt);
        }

        [AsyncTestMethod]
        public async Task UpdatedAtSystemPropertyDeserializesToDatetimeOrString()
        {
            TestHttpHandler hijack = new TestHttpHandler();

            hijack.SetResponseContent("[{\"id\":\"an id\",\"__updatedAt\":\"2000-01-01T07:59:59.000Z\"}]");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<UpdatedAtType> table = service.GetTable<UpdatedAtType>();

            IEnumerable<UpdatedAtType> results = await table.ReadAsync();
            UpdatedAtType[] items = results.ToArray();

            Assert.AreEqual(1, items.Count());
            Assert.AreEqual("an id", items[0].Id);
            Assert.AreEqual(new DateTime(1999, 12, 31, 23, 59, 59), items[0].UpdatedAt);

            hijack.SetResponseContent("[{\"id\":\"an id\",\"__updatedAt\":\"2000-01-01T07:59:59.000Z\"}]");
            IMobileServiceTable<StringUpdatedAtType> stringTable = service.GetTable<StringUpdatedAtType>();

            IEnumerable<StringUpdatedAtType> stringResults = await stringTable.ReadAsync();
            StringUpdatedAtType[] stringItems = stringResults.ToArray();

            Assert.AreEqual(1, stringItems.Count());
            Assert.AreEqual("an id", stringItems[0].Id);
            Assert.AreEqual("01/01/2000 07:59:59", stringItems[0].UpdatedAt);
        }

        [AsyncTestMethod]
        public async Task VersionSystemPropertyDeserializesToString()
        {
            TestHttpHandler hijack = new TestHttpHandler();

            hijack.SetResponseContent("[{\"id\":\"an id\",\"__version\":\"AAAAAAAAH2o=\"}]");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<VersionType> table = service.GetTable<VersionType>();

            IEnumerable<VersionType> results = await table.ReadAsync();
            VersionType[] items = results.ToArray();

            Assert.AreEqual(1, items.Count());
            Assert.AreEqual("an id", items[0].Id);
            Assert.AreEqual("AAAAAAAAH2o=", items[0].Version);
        }

        #endregion System Properties Tests

        [AsyncTestMethod]
        public async Task VersionSystemPropertySetsIfMatchHeader()
        {
            List<Tuple<string, string>> testCases = new List<Tuple<string,string>>() {
                new Tuple<string, string>("AAAAAAAAH2o=", "\"AAAAAAAAH2o=\""),
                new Tuple<string, string>("a version", "\"a version\""),
                new Tuple<string, string>("a version with a \" quote", "\"a version with a \\\" quote\""),
                new Tuple<string, string>("a version with an already escaped \\\" quote", "\"a version with an already escaped \\\" quote\""),
                new Tuple<string, string>("\"a version with a quote at the start", "\"\\\"a version with a quote at the start\""),
                new Tuple<string, string>("a version with a quote at the end\"", "\"a version with a quote at the end\\\"\""),
                new Tuple<string, string>("datetime'2013-10-08T04%3A12%3A36.96Z'", "\"datetime'2013-10-08T04%3A12%3A36.96Z'\""),
            };

            foreach (Tuple<string, string> testcase in testCases)
            {
                TestHttpHandler hijack = new TestHttpHandler();

                hijack.SetResponseContent("{\"id\":\"an id\",\"__version\":\"AAAAAAAAH2o=\"}");

                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<VersionType> table = service.GetTable<VersionType>();

                hijack.OnSendingRequest = async (request) =>
                {
                    string content = await request.Content.ReadAsStringAsync();
                    JObject jobject = JObject.Parse(content);
                    Assert.AreEqual(request.Headers.IfMatch.First().Tag, testcase.Item2);
                    return request;
                };

                VersionType item = new VersionType() { Id = "an id", Version = testcase.Item1 };
                await table.UpdateAsync(item);

            }
        }

        [AsyncTestMethod]
        public async Task VersionSystemPropertySetFromEtagHeader()
        {
            List<Tuple<string, string>> testCases = new List<Tuple<string, string>>() {
                new Tuple<string, string>("AAAAAAAAH2o=", "\"AAAAAAAAH2o=\""),
                new Tuple<string, string>("a version", "\"a version\""),
                new Tuple<string, string>("a version with a \" quote", "\"a version with a \\\" quote\""),
                new Tuple<string, string>("a version with an already escaped \" quote", "\"a version with an already escaped \\\" quote\""),
                new Tuple<string, string>("\"a version with a quote at the start", "\"\\\"a version with a quote at the start\""),
                new Tuple<string, string>("a version with a quote at the end\"", "\"a version with a quote at the end\\\"\""),
                new Tuple<string, string>("datetime'2013-10-08T04%3A12%3A36.96Z'", "\"datetime'2013-10-08T04%3A12%3A36.96Z'\""),
            };

            foreach (Tuple<string, string> testcase in testCases)
            {
                TestHttpHandler hijack = new TestHttpHandler();

                hijack.SetResponseContent("{\"id\":\"an id\"}");
                hijack.Response.Headers.ETag = new EntityTagHeaderValue(testcase.Item2);

                IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

                IMobileServiceTable<VersionType> table = service.GetTable<VersionType>();

                VersionType item = new VersionType() { Id = "an id" };
                await table.UpdateAsync(item);

                Assert.AreEqual(item.Version, testcase.Item1);
            }
        }

        #region ETag Tests

        #endregion
    }
}
