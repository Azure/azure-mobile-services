using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation;
using ZumoE2ETestApp.Framework;
using ZumoE2ETestApp.Tests.Types;

namespace ZumoE2ETestApp.Tests
{
    internal static class ZumoMiscTests
    {
        public static ZumoTestGroup CreateTests()
        {
            ZumoTestGroup result = new ZumoTestGroup("Misc tests");
            
            result.AddTest(CreateFilterTestWithMultipleRequests(true));
            result.AddTest(CreateFilterTestWithMultipleRequests(false));
            result.AddTest(new ZumoTest("Validate that 'WithFilter' doesn't change client", async delegate(ZumoTest test)
            {
                var client = ZumoTestGlobals.Instance.Client;
                var filteredClient = client.WithFilter(new FilterWhichThrows());
                var table = client.GetTable<RoundTripTableItem>();
                var items = await table.Take(5).ToListAsync();
                test.AddLog("Retrieved items successfully, without filter which throws affecting request.");
                return true;
            }));

            result.AddTest(new ZumoTest("Validate that filter can bypass service", async delegate(ZumoTest test)
            {
                var client = ZumoTestGlobals.Instance.Client;
                string json = "{'id':1,'name':'John Doe','age':33}".Replace('\'', '\"');
                var filtered = client.WithFilter(new FilterToBypassService(201, "application/json", json));
                var table = filtered.GetTable("TableWhichDoesNotExist");
                var item = new JsonObject();
                await table.InsertAsync(item);
                List<string> errors = new List<string>();
                if (!Util.CompareJson(JsonObject.Parse(json), item, errors))
                {
                    foreach (var error in errors)
                    {
                        test.AddLog(error);
                    }

                    test.AddLog("Error comparing object returned by the filter");
                    return false;
                }
                else
                {
                    return true;
                }
            }));

            result.AddTest(CreateUserAgentValidationTest());
            result.AddTest(CreateParameterPassingTest(true));
            result.AddTest(CreateParameterPassingTest(false));
            return result;
        }

        private static ZumoTest CreateParameterPassingTest(bool useTypedTable)
        {
            return new ZumoTest("Parameter passing test - " + (useTypedTable ? "typed" : "untyped") + " tables", async delegate(ZumoTest test)
            {
                var client = ZumoTestGlobals.Instance.Client;
                var typed = client.GetTable<ParamsTestTableItem>();
                var untyped = client.GetTable(ZumoTestGlobals.ParamsTestTableName);
                var dict = new Dictionary<string, string>
                {
                    { "item", "simple" },
                    { "empty", "" },
                    { "spaces", "with spaces" },
                    { "specialChars", "`!@#$%^&*()-=[]\\;',./~_+{}|:\"<>?" },
                    { "latin", "ãéìôü ÇñÑ" },
                    { "arabic", "الكتاب على الطاولة" },
                    { "chinese", "这本书在桌子上" },
                    { "japanese", "本は机の上に" },
                    { "hebrew", "הספר הוא על השולחן" },
                    { "name+with special&chars", "should just work" }
                };

                JsonObject expectedParameters = new JsonObject();
                foreach (var key in dict.Keys)
                {
                    expectedParameters.Add(key, JsonValue.CreateStringValue(dict[key]));
                }

                bool testPassed = true;

                ParamsTestTableItem typedItem = new ParamsTestTableItem();
                JsonObject untypedItem = new JsonObject();
                JsonObject actualParameters;

                dict["operation"] = "insert";
                expectedParameters.Add("operation", JsonValue.CreateStringValue("insert"));
                if (useTypedTable)
                {
                    await typed.InsertAsync(typedItem, dict);
                    actualParameters = JsonObject.Parse(typedItem.parameters);
                }
                else
                {
                    await untyped.InsertAsync(untypedItem, dict);
                    actualParameters = JsonObject.Parse(untypedItem["parameters"].GetString());
                }

                testPassed = testPassed && ValidateParameters(test, "insert", expectedParameters, actualParameters);

                dict["operation"] = "update";
                expectedParameters["operation"] = JsonValue.CreateStringValue("update");
                if (useTypedTable)
                {
                    await typed.UpdateAsync(typedItem, dict);
                    actualParameters = JsonObject.Parse(typedItem.parameters);
                }
                else
                {
                    await untyped.UpdateAsync(untypedItem, dict);
                    actualParameters = JsonObject.Parse(untypedItem["parameters"].GetString());
                }

                testPassed = testPassed && ValidateParameters(test, "update", expectedParameters, actualParameters);

                dict["operation"] = "lookup";
                expectedParameters["operation"] = JsonValue.CreateStringValue("lookup");
                if (useTypedTable)
                {
                    var temp = await typed.LookupAsync(1, dict);
                    actualParameters = JsonObject.Parse(temp.parameters);
                }
                else
                {
                    var temp = await untyped.LookupAsync(1, dict);
                    actualParameters = JsonObject.Parse(temp.GetObject()["parameters"].GetString());
                }

                testPassed = testPassed && ValidateParameters(test, "lookup", expectedParameters, actualParameters);

                dict["operation"] = "read";
                expectedParameters["operation"] = JsonValue.CreateStringValue("read");
                if (useTypedTable)
                {
                    var temp = await typed.Where(t => t.Id >= 1).WithParameters(dict).ToListAsync();
                    actualParameters = JsonObject.Parse(temp[0].parameters);
                }
                else
                {
                    var temp = await untyped.ReadAsync("$filter=id gt 1", dict);
                    actualParameters = JsonObject.Parse(temp.GetArray()[0].GetObject()["parameters"].GetString());
                }

                testPassed = testPassed && ValidateParameters(test, "read", expectedParameters, actualParameters);

                // Delete operation doesn't populate the object with the response, so we'll use a filter to capture that
                var filter = new FilterToCaptureHttpTraffic();
                client = client.WithFilter(filter);
                typed = client.GetTable<ParamsTestTableItem>();
                untyped = client.GetTable(ZumoTestGlobals.ParamsTestTableName);

                dict["operation"] = "delete";
                expectedParameters["operation"] = JsonValue.CreateStringValue("delete");
                if (useTypedTable)
                {
                    await typed.DeleteAsync(typedItem, dict);
                }
                else
                {
                    await untyped.DeleteAsync(untypedItem, dict);
                }

                JsonObject response = JsonObject.Parse(filter.ResponseBody);
                actualParameters = JsonObject.Parse(response["parameters"].GetString());

                testPassed = testPassed && ValidateParameters(test, "delete", expectedParameters, actualParameters);

                return testPassed;
            });
        }

        private static bool ValidateParameters(ZumoTest test, string operation, JsonObject expected, JsonObject actual)
        {
            test.AddLog("Called {0}, now validating parameters", operation);
            List<string> errors = new List<string>();
            if (!Util.CompareJson(expected, actual, errors))
            {
                foreach (var error in errors)
                {
                    test.AddLog(error);
                }

                test.AddLog("Parameters passing for the {0} operation failed", operation);
                test.AddLog("Expected: {0}", expected.Stringify());
                test.AddLog("Actual: {0}", actual.Stringify());
                return false;
            }
            else
            {
                test.AddLog("Parameters passing for the {0} operation succeeded", operation);
                return true;
            }
        }

        private static ZumoTest CreateUserAgentValidationTest()
        {
            return new ZumoTest("Validation User-Agent header", async delegate(ZumoTest test)
            {
                var client = ZumoTestGlobals.Instance.Client;
                var filter = new FilterToCaptureHttpTraffic();
                client = client.WithFilter(filter);
                var table = client.GetTable<RoundTripTableItem>();
                var item = new RoundTripTableItem { String1 = "hello" };
                await table.InsertAsync(item);
                Action<string> dumpHeaders = delegate(string operation)
                {
                    test.AddLog("Headers for {0}:", operation);
                    test.AddLog("  Request:");
                    foreach (var header in filter.RequestHeaders.Keys)
                    {
                        test.AddLog("    {0}: {1}", header, filter.RequestHeaders[header]);
                    }

                    test.AddLog("  Response:");
                    foreach (var header in filter.ResponseHeaders.Keys)
                    {
                        test.AddLog("    {0}: {1}", header, filter.ResponseHeaders[header]);
                    }
                };

                dumpHeaders("Insert");

                item.Double1 = 123;
                await table.UpdateAsync(item);
                dumpHeaders("Update");

                var item2 = await table.LookupAsync(item.Id);
                dumpHeaders("Read");

                await table.DeleteAsync(item);
                dumpHeaders("Delete");

                return true;
            });
        }

        private static ZumoTest CreateFilterTestWithMultipleRequests(bool typed)
        {
            string testName = string.Format(CultureInfo.InvariantCulture, "Filter which maps one requests to many - {0} client", typed ? "typed" : "untyped");
            return new ZumoTest(testName, async delegate(ZumoTest test)
            {
                var client = ZumoTestGlobals.Instance.Client;
                int numberOfRequests = new Random().Next(2, 5);
                var filter = new FilterWithMultipleRequests(test, numberOfRequests);
                test.AddLog("Created a filter which will replay the request {0} times", numberOfRequests);
                var filteredClient = client.WithFilter(filter);

                var typedTable = filteredClient.GetTable<RoundTripTableItem>();
                var untypedTable = filteredClient.GetTable(ZumoTestGlobals.RoundTripTableName);
                var uniqueId = Guid.NewGuid().ToString("N");
                if (typed)
                {
                    var item = new RoundTripTableItem { String1 = uniqueId };
                    await typedTable.InsertAsync(item);
                }
                else
                {
                    var item = new JsonObject();
                    item.Add("string1", JsonValue.CreateStringValue(uniqueId));
                    await untypedTable.InsertAsync(item);
                }

                if (filter.TestFailed)
                {
                    test.AddLog("Filter reported a test failure. Aborting.");
                    return false;
                }

                test.AddLog("Inserted the data; now retrieving it to see how many items we have inserted.");
                filter.NumberOfRequests = 1; // no need to send it multiple times anymore

                var items = await untypedTable.ReadAsync("$select=string1,id&$filter=string1 eq '" + uniqueId + "'");
                var array = items.GetArray();
                bool passed;
                if (array.Count == numberOfRequests)
                {
                    test.AddLog("Filter inserted correct number of items.");
                    passed = true;
                }
                else
                {
                    test.AddLog("Error, filtered client should have inserted {0} items, but there are {1}", numberOfRequests, array.Count);
                    passed = false;
                }

                // Cleanup
                foreach (var item in array)
                {
                    await untypedTable.DeleteAsync(item.GetObject());
                }

                test.AddLog("Cleanup: removed added items.");
                return passed;
            });
        }

        class FilterWhichThrows : IServiceFilter
        {
            public IAsyncOperation<IServiceFilterResponse> Handle(IServiceFilterRequest request, IServiceFilterContinuation continuation)
            {
                throw new NotImplementedException();
            }
        }

        class FilterToBypassService : IServiceFilter
        {
            int statusCode;
            string statusDescription;
            string contentType;
            string content;

            public FilterToBypassService(int statusCode, string contentType, string content)
            {
                this.statusCode = statusCode;
                this.statusDescription = Enum.Parse(typeof(System.Net.HttpStatusCode), statusCode.ToString(CultureInfo.InvariantCulture)).ToString();
                this.contentType = contentType;
                this.content = content;
            }

            public IAsyncOperation<IServiceFilterResponse> Handle(IServiceFilterRequest request, IServiceFilterContinuation continuation)
            {
                TaskCompletionSource<IServiceFilterResponse> tcs = new TaskCompletionSource<IServiceFilterResponse>();
                tcs.SetResult(new MyResponse(this.statusCode, this.statusDescription, this.contentType, this.content));
                return tcs.Task.AsAsyncOperation();
            }

            class MyResponse : IServiceFilterResponse
            {
                int statusCode;
                string statusDescription;
                string contentType;
                string content;

                public MyResponse(int statusCode, string statusDescription, string contentType, string content)
                {
                    this.statusCode = statusCode;
                    this.statusDescription = statusDescription;
                    this.contentType = contentType;
                    this.content = content;
                }

                public string Content
                {
                    get { return this.content; }
                }

                public string ContentType
                {
                    get { return this.contentType; }
                }

                public IDictionary<string, string> Headers
                {
                    get { return new Dictionary<string, string> { { "Content-Type", this.contentType } }; }
                }

                public ServiceFilterResponseStatus ResponseStatus
                {
                    get { return ServiceFilterResponseStatus.Success; }
                }

                public int StatusCode
                {
                    get { return this.statusCode; }
                }

                public string StatusDescription
                {
                    get { return this.statusDescription; }
                }
            }
        }

        class FilterToCaptureHttpTraffic : IServiceFilter
        {
            public Dictionary<string, string> RequestHeaders { get; private set; }
            public Dictionary<string, string> ResponseHeaders { get; private set; }
            public string ResponseBody { get; set; }

            public FilterToCaptureHttpTraffic()
            {
                this.RequestHeaders = new Dictionary<string, string>();
                this.ResponseHeaders = new Dictionary<string, string>();
            }

            public IAsyncOperation<IServiceFilterResponse> Handle(IServiceFilterRequest request, IServiceFilterContinuation continuation)
            {
                this.RequestHeaders.Clear();
                foreach (var reqHeader in request.Headers.Keys)
                {
                    this.RequestHeaders.Add(reqHeader, request.Headers[reqHeader]);
                }

                return continuation.Handle(request).AsTask().ContinueWith<IServiceFilterResponse>(t =>
                {
                    this.ResponseHeaders.Clear();
                    var response = t.Result;
                    foreach (var respHeader in response.Headers.Keys)
                    {
                        this.ResponseHeaders.Add(respHeader, response.Headers[respHeader]);
                    }

                    this.ResponseBody = response.Content;

                    return response;
                }).AsAsyncOperation();
            }
        }

        class FilterWithMultipleRequests : IServiceFilter
        {
            private ZumoTest test;

            public bool TestFailed { get; private set; }
            public int NumberOfRequests { get; set; }

            public FilterWithMultipleRequests(ZumoTest test, int numberOfRequests)
            {
                this.test = test;
                this.NumberOfRequests = numberOfRequests;
                this.TestFailed = false;

                if (numberOfRequests < 1)
                {
                    throw new ArgumentOutOfRangeException("numberOfRequests", "Number of requests must be at least 1.");
                }
            }

            public IAsyncOperation<IServiceFilterResponse> Handle(IServiceFilterRequest request, IServiceFilterContinuation continuation)
            {
                return this.HandleRequest(request, continuation).AsAsyncOperation();
            }

            public async Task<IServiceFilterResponse> HandleRequest(IServiceFilterRequest request, IServiceFilterContinuation continuation)
            {
                IServiceFilterResponse response = null;
                try
                {
                    for (int i = 0; i < this.NumberOfRequests; i++)
                    {
                        response = await continuation.Handle(request);
                        test.AddLog("Sent the request number {0}", i + 1);
                        test.AddLog("Response: {0} - {1}", response.StatusCode, response.StatusDescription);
                        if (response.StatusCode >= 400)
                        {
                            test.AddLog("Invalid response. Content-Type: {0}", response.ContentType);
                            test.AddLog("Response content: {0}", response.Content);
                            throw new InvalidOperationException();
                        }
                    }
                }
                catch (Exception ex)
                {
                    test.AddLog("Exception while calling continuation: {0}", ex);
                    this.TestFailed = true;
                    throw;
                }

                return response;
            }
        }
    }

    [DataTable(Name = ZumoTestGlobals.ParamsTestTableName)]
    public class ParamsTestTableItem
    {
        public int Id { get; set; }
        public string parameters { get; set; }
    }
}
