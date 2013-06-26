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
    public class MobileServiceTableGenericTests :TestBase
    {
        [AsyncTestMethod]
        public async Task ReadAsyncGeneric()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("[{\"id\":12,\"String\":\"Hey\"}]");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);

            IMobileServiceTable<StringType> table = service.GetTable<StringType>();

            IEnumerable<StringType> results = await table.ReadAsync();
            StringType[] people = results.Cast<StringType>().ToArray();

            Assert.Contains(hijack.Request.RequestUri.ToString(), "StringType");

            Assert.AreEqual(1, people.Count());
            Assert.AreEqual(12, people[0].Id);
            Assert.AreEqual("Hey", people[0].String);
        }

        [AsyncTestMethod]
        public async Task ReadAsyncWithQueryGeneric()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("[{\"id\":12,\"String\":\"Hey\"}]");
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);


            IMobileServiceTable<StringType> table = service.GetTable<StringType>();
            IMobileServiceTableQuery<StringType> query = table.Where(p => p.Id == 12);
            IEnumerable<StringType> results = await table.ReadAsync(query);
            StringType[] people = results.Cast<StringType>().ToArray();

            Assert.Contains(hijack.Request.RequestUri.ToString(), "StringType");
            Assert.Contains(hijack.Request.RequestUri.ToString(), "$filter=(id eq 12)");
            Assert.AreEqual(1, people.Count());
            Assert.AreEqual(12, people[0].Id);
            Assert.AreEqual("Hey", people[0].String);
        }

        [AsyncTestMethod]
        public async Task LookupAsyncGeneric()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            IMobileServiceTable<StringType> table = service.GetTable<StringType>();

            hijack.SetResponseContent("{\"id\":12,\"String\":\"Hello\"}");

            StringType expected = await table.LookupAsync(12);

            Assert.Contains(hijack.Request.RequestUri.ToString(), "12");
            Assert.AreEqual(12, expected.Id);
            Assert.AreEqual("Hello", expected.String);
        }


        [AsyncTestMethod]
        public async Task LookupAsyncGenericWithUserParameters()
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

        [AsyncTestMethod]
        public async Task LookupAsyncGenericThrowsWhenNotFound()
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
        public async Task LookupAsyncGenericThrowsWhenIdNull()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            IMobileServiceTable<StringType> table = service.GetTable<StringType>();

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
        public async Task LookupAsyncGenericThrowsWhenIdZero()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            IMobileServiceTable<StringType> table = service.GetTable<StringType>();

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
        public async Task RefreshAsyncGeneric()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            IMobileServiceTable<StringType> table = service.GetTable<StringType>();

            StringType expected = new StringType();
            expected.Id = 12;

            hijack.SetResponseContent("{\"id\":12,\"String\":\"Goodbye\"}");
            await table.RefreshAsync(expected);

            Assert.Contains(hijack.Request.RequestUri.ToString(), "$filter=id eq 12");
            Assert.AreEqual(12, expected.Id);
            Assert.AreEqual("Goodbye", expected.String);
        }


        [AsyncTestMethod]
        public async Task RefreshAsyncGenericWithUserParameters()
        {
            var userDefinedParameters = new Dictionary<string, string>() { { "state", "CA" } };

            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            IMobileServiceTable<StringType> table = service.GetTable<StringType>();

            StringType expected = new StringType();
            expected.Id = 12;

            hijack.SetResponseContent("{\"id\":12,\"String\":\"Goodbye\"}");
            await table.RefreshAsync(expected, userDefinedParameters);

            Assert.Contains(hijack.Request.RequestUri.ToString(), "$filter=id eq 12");
            Assert.Contains(hijack.Request.RequestUri.Query, "state=CA");
            Assert.AreEqual(12, expected.Id);
            Assert.AreEqual("Goodbye", expected.String);
        }

        [AsyncTestMethod]
        public async Task InsertAsyncGeneric()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            IMobileServiceTable<StringType> table = service.GetTable<StringType>();

            StringType obj = new StringType();
            obj.String = "new";

            hijack.SetResponseContent("{\"id\":12,\"value\":\"new\"}");
            await table.InsertAsync(obj);

            Assert.AreEqual(12, obj.Id);
            Assert.Contains(hijack.Request.RequestUri.ToString(), "StringType");
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

        [AsyncTestMethod]
        public async Task InsertAsyncAsyncThrowsOnNull()
        {
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            IMobileServiceTable<StringType> table = service.GetTable<StringType>();
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
        public async Task UpdateAsyncGeneric()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            IMobileServiceTable<StringType> table = service.GetTable<StringType>();

            StringType obj = new StringType();
            obj.Id = 12;
            obj.String = "new";

            hijack.SetResponseContent("{\"Id\":12,\"String\":\"new1\"}");

            await table.UpdateAsync(obj);

            Assert.AreEqual("new1", obj.String);
            Assert.Contains(hijack.Request.RequestUri.ToString(), "StringType");
        }

        [AsyncTestMethod]
        public async Task UpdateAsyncGenericWithParameters()
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

        [AsyncTestMethod]
        public async Task UpdateAsyncGenericThrowsOnNull()
        {
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            IMobileServiceTable<StringType> table = service.GetTable<StringType>();
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
        public async Task UpdateAsyncGenericThrowsWhenObjectHasDefaultId()
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
        public async Task DeleteAsyncGeneric()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...", hijack);
            IMobileServiceTable<StringType> table = service.GetTable<StringType>();

            StringType obj = new StringType();
            obj.Id = 12;
            obj.String = "new";

            await table.DeleteAsync(obj);

            Assert.AreEqual(0, obj.Id);
            Assert.Contains(hijack.Request.RequestUri.ToString(), "StringType");
            Assert.IsNull(hijack.Request.Content);
        }

        [AsyncTestMethod]
        public async Task DeleteAsyncGenericWithParameters()
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

        [AsyncTestMethod]
        public async Task DeleteAsyncGenericThrowsOnNull()
        {
            MobileServiceClient service = new MobileServiceClient("http://www.test.com", "secret...");
            IMobileServiceTable<StringType> table = service.GetTable<StringType>();
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
        public async Task DeleteAsyncGenericThrowsWhenObjectHasDefaultId()
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
    }
}
