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
            result.AddTest(CreateUserAgentValidationTest());
            return result;
        }

        private static ZumoTest CreateUserAgentValidationTest()
        {
            return new ZumoTest("Validation User-Agent header", async delegate(ZumoTest test)
            {
                var client = ZumoTestGlobals.Instance.Client;
                var filter = new FilterToCaptureHttpHeaders();
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

        class FilterToCaptureHttpHeaders : IServiceFilter
        {
            public Dictionary<string, string> RequestHeaders { get; private set; }
            public Dictionary<string, string> ResponseHeaders { get; private set; }

            public FilterToCaptureHttpHeaders()
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
                    foreach (var respHeader in t.Result.Headers.Keys)
                    {
                        this.ResponseHeaders.Add(respHeader, t.Result.Headers[respHeader]);
                    }

                    return t.Result;
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
}
