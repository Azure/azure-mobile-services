// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
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

            result.AddTest(new ZumoTest("Validate that filter can bypass service", async delegate(ZumoTest test)
            {
                string json = "{'id':1,'name':'John Doe','age':33}".Replace('\'', '\"');
                var client = new MobileServiceClient(
                    ZumoTestGlobals.Instance.Client.ApplicationUri,
                    ZumoTestGlobals.Instance.Client.ApplicationKey,
                    new HandlerToBypassService(201, "application/json", json));
                var table = client.GetTable("TableWhichDoesNotExist");
                var item = new JObject();
                var inserted = await table.InsertAsync(item);
                List<string> errors = new List<string>();
                if (!Util.CompareJson(JObject.Parse(json), inserted, errors))
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

            result.AddTest(CreateOptimisticConcurrencyTest("Conflicts - client wins", (clientItem, serverItem) =>
            {
                var mergeResult = clientItem.Clone();
                mergeResult.Version = serverItem.Version;
                return mergeResult;
            }));
            result.AddTest(CreateOptimisticConcurrencyTest("Conflicts - server wins", (clientItem, serverItem) =>
            {
                return serverItem;
            }));
            result.AddTest(CreateOptimisticConcurrencyTest("Conflicts - Name from client, Number from server", (clientItem, serverItem) =>
            {
                var mergeResult = serverItem.Clone();
                mergeResult.Name = clientItem.Name;
                return mergeResult;
            }));

            result.AddTest(CreateSystemPropertiesTest(true));
            result.AddTest(CreateSystemPropertiesTest(false));

            return result;
        }

        private static ZumoTest CreateSystemPropertiesTest(bool useTypedTable)
        {
            return new ZumoTest("System properties in " + (useTypedTable ? "" : "un") + "typed tables", async delegate(ZumoTest test)
            {
                var client = ZumoTestGlobals.Instance.Client;
                var typedTable = client.GetTable<VersionedType>();
                var untypedTable = client.GetTable(ZumoTestGlobals.StringIdRoundTripTableName);
                untypedTable.SystemProperties =
                    MobileServiceSystemProperties.CreatedAt |
                    MobileServiceSystemProperties.UpdatedAt |
                    MobileServiceSystemProperties.Version;
                DateTime now = DateTime.UtcNow;
                int seed = now.Year * 10000 + now.Month * 100 + now.Day;
                test.AddLog("Using seed: {0}", seed);
                Random rndGen = new Random(seed);
                VersionedType item = null;
                JObject untypedItem = null;
                DateTime createdAt, updatedAt;
                string id;
                if (useTypedTable)
                {
                    item = new VersionedType(rndGen);
                    await typedTable.InsertAsync(item);
                    test.AddLog("Inserted: {0}", item);
                    id = item.Id;
                    createdAt = item.CreatedAt;
                    updatedAt = item.UpdatedAt;
                }
                else
                {
                    untypedItem = new JObject();
                    untypedItem.Add("name", "unused");
                    untypedItem = (JObject)(await untypedTable.InsertAsync(untypedItem));
                    test.AddLog("Inserted: {0}", untypedItem);
                    id = (string)untypedItem["id"];
                    createdAt = untypedItem["__createdAt"].ToObject<DateTime>();
                    updatedAt = untypedItem["__updatedAt"].ToObject<DateTime>();
                }

                test.AddLog("Now adding a new item");
                DateTime otherCreatedAt, otherUpdatedAt;
                string otherId;
                if (useTypedTable)
                {
                    item = new VersionedType(rndGen);
                    await typedTable.InsertAsync(item);
                    test.AddLog("Inserted: {0}", item);
                    otherId = item.Id;
                    otherCreatedAt = item.CreatedAt;
                    otherUpdatedAt = item.UpdatedAt;
                }
                else
                {
                    untypedItem = new JObject();
                    untypedItem.Add("name", "unused");
                    untypedItem = (JObject)(await untypedTable.InsertAsync(untypedItem));
                    test.AddLog("Inserted: {0}", untypedItem);
                    otherId = (string)untypedItem["id"];
                    otherCreatedAt = untypedItem["__createdAt"].ToObject<DateTime>();
                    otherUpdatedAt = untypedItem["__updatedAt"].ToObject<DateTime>();
                }

                if (createdAt >= otherCreatedAt)
                {
                    test.AddLog("Error, first __createdAt value is not smaller than second one");
                    return false;
                }

                if (updatedAt >= otherUpdatedAt)
                {
                    test.AddLog("Error, first __updatedAt value is not smaller than second one");
                    return false;
                }

                createdAt = otherCreatedAt;
                updatedAt = otherUpdatedAt;

                test.AddLog("Now updating the item");
                if (useTypedTable)
                {
                    item = new VersionedType(rndGen) { Id = otherId };
                    await typedTable.UpdateAsync(item);
                    test.AddLog("Updated: {0}", item);
                    otherUpdatedAt = item.UpdatedAt;
                    otherCreatedAt = item.CreatedAt;
                }
                else
                {
                    untypedItem = new JObject(new JProperty("id", otherId), new JProperty("name", "other name"));
                    untypedItem = (JObject)(await untypedTable.UpdateAsync(untypedItem));
                    test.AddLog("Updated: {0}", untypedItem);
                    otherCreatedAt = untypedItem["__createdAt"].ToObject<DateTime>();
                    otherUpdatedAt = untypedItem["__updatedAt"].ToObject<DateTime>();
                }

                if (createdAt != otherCreatedAt)
                {
                    test.AddLog("Error, update changed the value of the __createdAt property");
                    return false;
                }

                if (otherUpdatedAt <= updatedAt)
                {
                    test.AddLog("Error, update did not change the __updatedAt property to a later value");
                    return false;
                }

                test.AddLog("Cleanup: deleting items");
                await untypedTable.DeleteAsync(new JObject(new JProperty("id", id)));
                await untypedTable.DeleteAsync(new JObject(new JProperty("id", otherId)));
                return true;
            });
        }

        private static ZumoTest CreateOptimisticConcurrencyTest(string testName, Func<VersionedType, VersionedType, VersionedType> mergingPolicy)
        {
            return new ZumoTest(testName, async delegate(ZumoTest test)
            {
                var client = ZumoTestGlobals.Instance.Client;
                var table = client.GetTable<VersionedType>();
                DateTime now = DateTime.UtcNow;
                int seed = now.Year * 10000 + now.Month * 100 + now.Day;
                test.AddLog("Using seed: {0}", seed);
                Random rndGen = new Random(seed);
                var item = new VersionedType(rndGen);
                await table.InsertAsync(item);
                test.AddLog("[client 1] Inserted item: {0}", item);

                var client2 = new MobileServiceClient(client.ApplicationUri, client.ApplicationKey);
                var table2 = client.GetTable<VersionedType>();
                var item2 = await table2.LookupAsync(item.Id);
                test.AddLog("[client 2] Retrieved the item");
                item2.Name = Util.CreateSimpleRandomString(rndGen, 20);
                item2.Number = rndGen.Next(100000);
                test.AddLog("[client 2] Updated the item, will update on the server now");
                await table2.UpdateAsync(item2);
                test.AddLog("[client 2] Item has been updated: {0}", item2);

                test.AddLog("[client 1] Will try to update; should fail");
                MobileServicePreconditionFailedException<VersionedType> ex = null;
                try
                {
                    item.Name = Util.CreateSimpleRandomString(rndGen, 20);
                    await table.UpdateAsync(item);
                    test.AddLog("[client 1] Error, the update succeeded, but it should have failed. Item = {0}", item);
                    return false;
                }
                catch (MobileServicePreconditionFailedException<VersionedType> e)
                {
                    test.AddLog("[client 1] Received expected exception; server item = {0}", e.Item);
                    ex = e;
                }

                var serverItem = ex.Item;
                if (serverItem.Version != item2.Version)
                {
                    test.AddLog("[client 1] Error, server item's version is not the same as the second item version");
                    return false;
                }

                var cachedMergedItem = mergingPolicy(item, serverItem);
                var mergedItem = mergingPolicy(item, serverItem);
                test.AddLog("[client 1] Merged item: {0}", mergedItem);
                test.AddLog("[client 1] Trying to update it again, should succeed this time");

                await table.UpdateAsync(mergedItem);
                test.AddLog("[client 1] Updated the item: {0}", mergedItem);

                if (!cachedMergedItem.Equals(mergedItem))
                {
                    test.AddLog("[client 1] Error, the server version of the merged item doesn't match the client one");
                    return false;
                }

                test.AddLog("[client 2] Refreshing the item");
                await table2.RefreshAsync(item2);
                test.AddLog("[client 2] Refreshed the item: {0}", item2);

                if (!item2.Equals(mergedItem))
                {
                    test.AddLog("[client] Error, item is different than the item from the client 1");
                    return false;
                }

                return true;
            });
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

                var expectedParameters = new JObject();
                foreach (var key in dict.Keys)
                {
                    expectedParameters.Add(key, dict[key]);
                }

                bool testPassed = true;

                ParamsTestTableItem typedItem = new ParamsTestTableItem();
                var untypedItem = new JObject();
                JObject actualParameters;

                dict["operation"] = "insert";
                expectedParameters.Add("operation", "insert");
                if (useTypedTable)
                {
                    await typed.InsertAsync(typedItem, dict);
                    actualParameters = JObject.Parse(typedItem.parameters);
                }
                else
                {
                    var inserted = await untyped.InsertAsync(untypedItem, dict);
                    untypedItem = inserted as JObject;
                    actualParameters = JObject.Parse(untypedItem["parameters"].Value<string>());
                }

                testPassed = testPassed && ValidateParameters(test, "insert", expectedParameters, actualParameters);

                dict["operation"] = "update";
                expectedParameters["operation"] = "update";
                if (useTypedTable)
                {
                    await typed.UpdateAsync(typedItem, dict);
                    actualParameters = JObject.Parse(typedItem.parameters);
                }
                else
                {
                    var updated = await untyped.UpdateAsync(untypedItem, dict);
                    actualParameters = JObject.Parse(updated["parameters"].Value<string>());
                }

                testPassed = testPassed && ValidateParameters(test, "update", expectedParameters, actualParameters);

                dict["operation"] = "lookup";
                expectedParameters["operation"] = "lookup";
                if (useTypedTable)
                {
                    var temp = await typed.LookupAsync(1, dict);
                    actualParameters = JObject.Parse(temp.parameters);
                }
                else
                {
                    var temp = await untyped.LookupAsync(1, dict);
                    actualParameters = JObject.Parse(temp["parameters"].Value<string>());
                }

                testPassed = testPassed && ValidateParameters(test, "lookup", expectedParameters, actualParameters);

                dict["operation"] = "read";
                expectedParameters["operation"] = "read";
                if (useTypedTable)
                {
                    var temp = await typed.Where(t => t.Id >= 1).WithParameters(dict).ToListAsync();
                    actualParameters = JObject.Parse(temp[0].parameters);
                }
                else
                {
                    var temp = await untyped.ReadAsync("$filter=id gt 1", dict);
                    actualParameters = JObject.Parse(temp[0]["parameters"].Value<string>());
                }

                testPassed = testPassed && ValidateParameters(test, "read", expectedParameters, actualParameters);

                if (useTypedTable)
                {
                    // Refresh operation only exists for typed tables
                    dict["operation"] = "read";
                    expectedParameters["operation"] = "read";
                    typedItem.Id = 1;
                    typedItem.parameters = "";
                    await typed.RefreshAsync(typedItem, dict);
                    actualParameters = JObject.Parse(typedItem.parameters);
                    testPassed = testPassed && ValidateParameters(test, "refresh", expectedParameters, actualParameters);
                }

                // Delete operation doesn't populate the object with the response, so we'll use a filter to capture that
                var handler = new HandlerToCaptureHttpTraffic();
                var filteredClient = new MobileServiceClient(client.ApplicationUri, client.ApplicationKey, handler);
                typed = filteredClient.GetTable<ParamsTestTableItem>();
                untyped = filteredClient.GetTable(ZumoTestGlobals.ParamsTestTableName);

                dict["operation"] = "delete";
                expectedParameters["operation"] = "delete";
                if (useTypedTable)
                {
                    await typed.DeleteAsync(typedItem, dict);
                }
                else
                {
                    await untyped.DeleteAsync(untypedItem, dict);
                }

                JObject response = JObject.Parse(handler.ResponseBody);
                actualParameters = JObject.Parse(response["parameters"].Value<string>());

                testPassed = testPassed && ValidateParameters(test, "delete", expectedParameters, actualParameters);

                return testPassed;
            });
        }

        private static bool ValidateParameters(ZumoTest test, string operation, JObject expected, JObject actual)
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
                test.AddLog("Expected: {0}", expected);
                test.AddLog("Actual: {0}", actual);
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
                var handler = new HandlerToCaptureHttpTraffic();
                MobileServiceClient client = new MobileServiceClient(
                    ZumoTestGlobals.Instance.Client.ApplicationUri,
                    ZumoTestGlobals.Instance.Client.ApplicationKey,
                    handler);
                var table = client.GetTable<RoundTripTableItem>();
                var item = new RoundTripTableItem { String1 = "hello" };
                await table.InsertAsync(item);
                Action<string> dumpAndValidateHeaders = delegate(string operation)
                {
                    test.AddLog("Headers for {0}:", operation);
                    test.AddLog("  Request:");
                    foreach (var header in handler.RequestHeaders.Keys)
                    {
                        test.AddLog("    {0}: {1}", header, handler.RequestHeaders[header]);
                    }

                    test.AddLog("  Response:");
                    foreach (var header in handler.ResponseHeaders.Keys)
                    {
                        test.AddLog("    {0}: {1}", header, handler.ResponseHeaders[header]);
                    }

                    string userAgent;
                    if (!handler.RequestHeaders.TryGetValue("User-Agent", out userAgent))
                    {
                        test.AddLog("No user-agent header in the request");
                        throw new InvalidOperationException("This will fail the test");
                    }
                    else
                    {
                        Regex expected = new Regex(@"^ZUMO\/\d.\d");
                        if (expected.IsMatch(userAgent))
                        {
                            test.AddLog("User-Agent validated correclty");
                        }
                        else
                        {
                            test.AddLog("User-Agent didn't validate properly.");
                            throw new InvalidOperationException("This will fail the test");
                        }
                    }
                };

                dumpAndValidateHeaders("Insert");

                item.Double1 = 123;
                await table.UpdateAsync(item);
                dumpAndValidateHeaders("Update");

                var item2 = await table.LookupAsync(item.Id);
                dumpAndValidateHeaders("Read");

                await table.DeleteAsync(item);
                dumpAndValidateHeaders("Delete");

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
                var handler = new HandlerWithMultipleRequests(test, numberOfRequests);
                test.AddLog("Created a filter which will replay the request {0} times", numberOfRequests);
                var filteredClient = new MobileServiceClient(client.ApplicationUri, client.ApplicationKey, handler);

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
                    var item = new JObject(new JProperty("string1", uniqueId));
                    await untypedTable.InsertAsync(item);
                }

                if (handler.TestFailed)
                {
                    test.AddLog("Filter reported a test failure. Aborting.");
                    return false;
                }

                test.AddLog("Inserted the data; now retrieving it to see how many items we have inserted.");
                handler.NumberOfRequests = 1; // no need to send it multiple times anymore

                var items = await untypedTable.ReadAsync("$select=string1,id&$filter=string1 eq '" + uniqueId + "'");
                var array = (JArray)items;
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
                    await untypedTable.DeleteAsync(item as JObject);
                }

                test.AddLog("Cleanup: removed added items.");
                return passed;
            });
        }

        class HandlerWhichThrows : DelegatingHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        class HandlerToBypassService : DelegatingHandler
        {
            HttpStatusCode statusCode;
            string contentType;
            string content;

            public HandlerToBypassService(int statusCode, string contentType, string content)
            {
                this.statusCode = (HttpStatusCode)statusCode;
                this.contentType = contentType;
                this.content = content;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                TaskCompletionSource<HttpResponseMessage> tcs = new TaskCompletionSource<HttpResponseMessage>();
                HttpResponseMessage result = new HttpResponseMessage(this.statusCode);
                result.Content = new StringContent(this.content, Encoding.UTF8, this.contentType);
                tcs.SetResult(result);
                return tcs.Task;
            }
        }

        class HandlerToCaptureHttpTraffic : DelegatingHandler
        {
            public Dictionary<string, string> RequestHeaders { get; private set; }
            public Dictionary<string, string> ResponseHeaders { get; private set; }
            public string ResponseBody { get; set; }

            public HandlerToCaptureHttpTraffic()
            {
                this.RequestHeaders = new Dictionary<string, string>();
                this.ResponseHeaders = new Dictionary<string, string>();
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                this.RequestHeaders.Clear();
                foreach (var header in request.Headers)
                {
                    this.RequestHeaders.Add(header.Key, string.Join(", ", header.Value));
                    if (header.Key.Equals("user-agent", StringComparison.OrdinalIgnoreCase))
                    {
                        string userAgent = this.RequestHeaders[header.Key];
                        userAgent.TrimEnd(')');
                        int equalsIndex = userAgent.LastIndexOf('=');
                        if (equalsIndex >= 0)
                        {
                            var clientVersion = userAgent.Substring(equalsIndex + 1);
                            ZumoTestGlobals.Instance.GlobalTestParams[ZumoTestGlobals.ClientVersionKeyName] = clientVersion;
                        }
                    }
                }

                var response = await base.SendAsync(request, cancellationToken);
                this.ResponseHeaders.Clear();
                foreach (var header in response.Headers)
                {
                    this.ResponseHeaders.Add(header.Key, string.Join(", ", header.Value));
                    if (header.Key.Equals("x-zumo-version", StringComparison.OrdinalIgnoreCase))
                    {
                        ZumoTestGlobals.Instance.GlobalTestParams[ZumoTestGlobals.RuntimeVersionKeyName] = this.ResponseHeaders[header.Key];
                    }
                }

                this.ResponseBody = await response.Content.ReadAsStringAsync();
                return response;
            }
        }

        class HandlerWithMultipleRequests : DelegatingHandler
        {
            private ZumoTest test;

            public bool TestFailed { get; private set; }
            public int NumberOfRequests { get; set; }

            public HandlerWithMultipleRequests(ZumoTest test, int numberOfRequests)
            {
                this.test = test;
                this.NumberOfRequests = numberOfRequests;
                this.TestFailed = false;

                if (numberOfRequests < 1)
                {
                    throw new ArgumentOutOfRangeException("numberOfRequests", "Number of requests must be at least 1.");
                }
            }

            private static async Task<HttpRequestMessage> CloneRequest(HttpRequestMessage request)
            {
                HttpRequestMessage result = new HttpRequestMessage(request.Method, request.RequestUri);
                if (request.Content != null)
                {
                    string content = await request.Content.ReadAsStringAsync();
                    string mediaType = request.Content.Headers.ContentType.MediaType;
                    result.Content = new StringContent(content, Encoding.UTF8, mediaType);
                }

                foreach (var header in request.Headers)
                {
                    if (!header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                    {
                        result.Headers.Add(header.Key, header.Value);
                    }
                }

                return result;
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                HttpResponseMessage response = null;
                try
                {
                    for (int i = 0; i < this.NumberOfRequests; i++)
                    {
                        HttpRequestMessage clonedRequest = await CloneRequest(request);
                        response = await base.SendAsync(clonedRequest, cancellationToken);
                        if (i < this.NumberOfRequests - 1)
                        {
                            response.Dispose();
                            response = null;
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

    [DataTable(ZumoTestGlobals.ParamsTestTableName)]
    public class ParamsTestTableItem
    {
        public int Id { get; set; }
        public string parameters { get; set; }
    }
}
