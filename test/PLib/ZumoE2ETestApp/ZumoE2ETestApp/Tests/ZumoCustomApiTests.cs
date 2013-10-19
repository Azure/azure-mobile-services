// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using ZumoE2ETestApp.Framework;
using ZumoE2ETestApp.Tests.Types;

namespace ZumoE2ETestApp.Tests
{
    public static class ZumoCustomApiTests
    {
        private const string PublicApiName = "public";
        private const string AppApiName = "application";
        private const string UserApiName = "user";
        private const string AdminApiName = "admin";
        private const string MovieFinderApiName = "movieFinder";

        private const string Letters = "abcdefghijklmnopqrstuvwxyz";

        public enum ApiPermissions { Public, Application, User, Admin }
        public enum DataFormat { Json, Xml, Other }
        public enum TypedTestType { GetByTitle, GetByDate, PostByDuration, PostByYear }

        private static readonly Dictionary<ApiPermissions, string> apiNames = new Dictionary<ApiPermissions, string>
        {
            { ApiPermissions.Admin, AdminApiName },
            { ApiPermissions.Application, AppApiName },
            { ApiPermissions.Public, PublicApiName },
            { ApiPermissions.User, UserApiName },
        };

        internal static ZumoTestGroup CreateTests()
        {
            ZumoTestGroup result = new ZumoTestGroup("Custom API tests");

            DateTime now = DateTime.UtcNow;
            int seed = now.Year * 10000 + now.Month * 100 + now.Day;
            Random rndGen = new Random(seed);

#if !NET45
            result.AddTest(ZumoLoginTests.CreateLogoutTest());
#endif

            result.AddTest(CreateHttpContentApiTest(DataFormat.Xml, DataFormat.Json, rndGen));

#if !NET45
            List<ZumoTest> testsWhichNeedAuth = new List<ZumoTest>();

            foreach (ApiPermissions apiPermission in Util.EnumGetValues(typeof(ApiPermissions)))
            {
                testsWhichNeedAuth.Add(CreateJTokenApiTest(apiPermission, false, rndGen));
            }

            testsWhichNeedAuth.Add(ZumoLoginTests.CreateLoginTest(MobileServiceAuthenticationProvider.Google));
            testsWhichNeedAuth.Add(CreateJTokenApiTest(ApiPermissions.User, true, rndGen));
            testsWhichNeedAuth.Add(ZumoLoginTests.CreateLogoutTest());

            foreach (var test in testsWhichNeedAuth)
            {
                test.CanRunUnattended = false;
                result.AddTest(test);
            }
#endif

            foreach (DataFormat inputFormat in Util.EnumGetValues(typeof(DataFormat)))
            {
                foreach (DataFormat outputFormat in Util.EnumGetValues(typeof(DataFormat)))
                {
                    result.AddTest(CreateHttpContentApiTest(inputFormat, outputFormat, rndGen));
                }
            }


            result.AddTest(ZumoQueryTests.CreatePopulateTableTest());
            foreach (TypedTestType testType in Util.EnumGetValues(typeof(TypedTestType)))
            {
                result.AddTest(CreateTypedApiTest(rndGen, testType));
            }

            return result;
        }

        private static ZumoTest CreateTypedApiTest(Random seedGenerator, TypedTestType testType)
        {
            string testName = "Typed overload - " + testType;
            return new ZumoTest(testName, async delegate(ZumoTest test)
            {
                var client = ZumoTestGlobals.Instance.Client;
                var apiName = MovieFinderApiName;
                var testResult = true;
                for (int i = 0; i < 10; i++)
                {
                    int seed = seedGenerator.Next();
                    test.AddLog("Test with seed = {0}", seed);
                    Random rndGen = new Random(seed);

                    Movie[] expectedResult = null;
                    AllMovies actualResult = null;
                    Movie inputTemplate = ZumoQueryTestData.AllMovies[rndGen.Next(ZumoQueryTestData.AllMovies.Length)];
                    test.AddLog("Using movie '{0}' as template", inputTemplate.Title);
                    string apiUrl;
                    switch (testType)
                    {
                        case TypedTestType.GetByTitle:
                            apiUrl = apiName + "/title/" + inputTemplate.Title;
                            expectedResult = new Movie[] { inputTemplate };
                            actualResult = await client.InvokeApiAsync<AllMovies>(apiUrl, HttpMethod.Get, null);
                            break;
                        case TypedTestType.GetByDate:
                            var releaseDate = inputTemplate.ReleaseDate;
                            apiUrl = apiName + "/date/" + releaseDate.Year + "/" + releaseDate.Month + "/" + releaseDate.Day;
                            expectedResult = ZumoQueryTestData.AllMovies.Where(m => m.ReleaseDate == releaseDate).ToArray();
                            actualResult = await client.InvokeApiAsync<AllMovies>(apiUrl, HttpMethod.Get, null);
                            break;
                        case TypedTestType.PostByDuration:
                        case TypedTestType.PostByYear:
                            string orderBy = null;
                            switch (rndGen.Next(3))
                            {
                                case 0:
                                    orderBy = null;
                                    break;
                                case 1:
                                    orderBy = "id";
                                    break;
                                case 2:
                                    orderBy = "Title";
                                    break;
                            }

                            Dictionary<string, string> queryParams = orderBy == null ?
                                null :
                                new Dictionary<string, string> { { "orderBy", orderBy } };

                            Func<Movie, bool> predicate;
                            if (testType == TypedTestType.PostByYear)
                            {
                                predicate = m => m.Year == inputTemplate.Year;
                                apiUrl = apiName + "/moviesOnSameYear";
                            }
                            else
                            {
                                predicate = m => m.Duration == inputTemplate.Duration;
                                apiUrl = apiName + "/moviesWithSameDuration";
                            }

                            if (queryParams == null)
                            {
                                actualResult = await client.InvokeApiAsync<Movie, AllMovies>(apiUrl, inputTemplate);
                            }
                            else
                            {
                                actualResult = await client.InvokeApiAsync<Movie, AllMovies>(apiUrl, inputTemplate, HttpMethod.Post, queryParams);
                            }

                            expectedResult = ZumoQueryTestData.AllMovies.Where(predicate).ToArray();
                            if (orderBy == null || orderBy == "Title")
                            {
                                Array.Sort(expectedResult, (m1, m2) => m1.Title.CompareTo(m2.Title));
                            }

                            break;
                        default:
                            throw new ArgumentException("Invalid test type: " + testType);
                    }

                    test.AddLog("  - Sent request to {0}", apiUrl);
                    List<string> errors = new List<string>();
                    if (Util.CompareArrays(expectedResult, actualResult.Movies, errors))
                    {
                        test.AddLog("  - Result is expected");
                    }
                    else
                    {
                        foreach (var error in errors)
                        {
                            test.AddLog("  - {0}", error);
                        }

                        test.AddLog("Expected: {0}", string.Join(", ", expectedResult.Select(m => m.Title)));
                        test.AddLog("Actual: {0}", string.Join(", ", actualResult.Movies.Select(m => m.Title)));
                        testResult = false;
                        break;
                    }
                }

                return testResult;
            });
        }

        private static ZumoTest CreateHttpContentApiTest(DataFormat inputFormat, DataFormat outputFormat, Random seedGenerator)
        {
            string testName = string.Format("HttpContent overload - input {0} / output {1}", inputFormat, outputFormat);
            return new ZumoTest(testName, async delegate(ZumoTest test)
            {
                var client = ZumoTestGlobals.Instance.Client;
                var apiName = AppApiName;
                var testResult = true;
                for (int i = 0; i < 10; i++)
                {
                    int seed = seedGenerator.Next();
                    test.AddLog("Test with seed = {0}", seed);
                    Random rndGen = new Random(seed);
                    HttpMethod method;
                    HttpContent content;
                    JObject expectedResult;
                    Dictionary<string, string> headers;
                    Dictionary<string, string> query;
                    HttpStatusCode expectedStatus;
                    CreateHttpContentTestInput(inputFormat, outputFormat, rndGen, out method, out content, out expectedResult, out headers, out query, out expectedStatus);

                    HttpResponseMessage response;
                    try
                    {
                        response = await client.InvokeApiAsync(apiName, content, method, headers, query);
                    }
                    catch (MobileServiceInvalidOperationException ex)
                    {
                        response = ex.Response;
                    }

                    using (response)
                    {
                        test.AddLog("Response: {0}", response);
                        if (!ValidateResponseHeader(test, expectedStatus, headers, response))
                        {
                            testResult = false;
                            break;
                        }

                        test.AddLog("  - All request / response headers validated successfully");

                        string responseContent = null;
                        if (response.Content != null)
                        {
                            responseContent = await response.Content.ReadAsStringAsync();
                            test.AddLog("Response content: {0}", responseContent);
                        }

                        JToken jsonResponse = null;
                        if (outputFormat == DataFormat.Json)
                        {
                            jsonResponse = JToken.Parse(responseContent);
                        }
                        else if (outputFormat == DataFormat.Other)
                        {
                            string decodedContent = responseContent
                                .Replace("__{__", "{")
                                .Replace("__}__", "}")
                                .Replace("__[__", "[")
                                .Replace("__]__", "]");
                            jsonResponse = JToken.Parse(decodedContent);
                        }

                        bool contentIsExpected = false;
                        List<string> errors = new List<string>();
                        switch (outputFormat)
                        {
                            case DataFormat.Json:
                            case DataFormat.Other:
                                contentIsExpected = Util.CompareJson(expectedResult, jsonResponse, errors);
                                break;
                            case DataFormat.Xml:
                                string expectedResultContent = JsonToXml(expectedResult);

                                // Normalize CRLF
                                expectedResultContent = expectedResultContent.Replace("\r\n", "\n");
                                responseContent = responseContent.Replace("\r\n", "\n");

                                contentIsExpected = expectedResultContent == responseContent;
                                if (!contentIsExpected)
                                {
                                    errors.Add(string.Format(
                                        "Error, response content is incorrect. Expected: {0}. Actual: {1}",
                                        expectedResultContent, responseContent));
                                }

                                break;
                        }

                        if (!contentIsExpected)
                        {
                            foreach (var error in errors)
                            {
                                test.AddLog("{0}", error);
                            }

                            testResult = false;
                            break;
                        }

                        test.AddLog("  - Validation completed successfully");
                    }
                }

                return testResult;
            });
        }

        private static bool ValidateResponseHeader(ZumoTest test, HttpStatusCode expectedStatus, Dictionary<string, string> expectedHeaders, HttpResponseMessage response)
        {
            bool result = true;
            if (expectedStatus != response.StatusCode)
            {
                test.AddLog("Error in response status: expected {0}, received {1}", expectedStatus, response.StatusCode);
                result = false;
            }
            else
            {
                foreach (var reqHeader in expectedHeaders.Keys)
                {
                    IEnumerable<string> headerValue;
                    if (!response.Headers.TryGetValues(reqHeader, out headerValue))
                    {
                        test.AddLog("Error, expected header {0} not found", reqHeader);
                        result = false;
                        break;
                    }
                    else
                    {
                        if (!expectedHeaders[reqHeader].Equals(headerValue.FirstOrDefault()))
                        {
                            test.AddLog("Error, header value for {0} is incorrect. Expected {1}, actual {2}",
                                reqHeader, expectedHeaders[reqHeader], headerValue.FirstOrDefault() ?? "<<NULL>>");
                            result = false;
                            break;
                        }
                    }
                }
            }

            return result;
        }

        private static void CreateHttpContentTestInput(DataFormat inputFormat, DataFormat outputFormat, Random rndGen, out HttpMethod method, out HttpContent content, out JObject expectedResult, out Dictionary<string, string> headers, out Dictionary<string, string> query, out HttpStatusCode expectedStatus)
        {
            method = CreateHttpMethod(rndGen);
            content = null;
            expectedResult = new JObject();
            expectedResult.Add("method", method.Method);
            expectedResult.Add("user", new JObject(new JProperty("level", "anonymous")));
            JToken body = null;
            string textBody = null;
            if (method.Method != "GET" && method.Method != "DELETE")
            {
                body = CreateJson(rndGen);
                if (outputFormat == DataFormat.Xml || inputFormat == DataFormat.Xml)
                {
                    // to prevent non-XML names from interfering with checks
                    body = SanitizeJsonXml(body);
                }

                switch (inputFormat)
                {
                    case DataFormat.Json:
                        // JSON
                        content = new StringContent(body.ToString(), Encoding.UTF8, "application/json");
                        break;
                    case DataFormat.Xml:
                        textBody = JsonToXml(body);
                        content = new StringContent(textBody, Encoding.UTF8, "text/xml");
                        break;
                    default:
                        textBody = body.ToString().Replace("{", "<").Replace("}", ">").Replace("[", "__[__").Replace("]", "__]__");
                        content = new StringContent(textBody, Encoding.UTF8, "text/plain");
                        break;
                }
            }

            if (body != null)
            {
                if (inputFormat == DataFormat.Json)
                {
                    expectedResult.Add("body", body);
                }
                else
                {
                    expectedResult.Add("body", textBody);
                }
            }

            headers = new Dictionary<string, string>();
            var choice = rndGen.Next(5);
            for (int j = 0; j < choice; j++)
            {
                string name = "x-test-zumo-" + j;
                string value = CreateString(rndGen, 1, 10, Letters);
                headers.Add(name, value);
            }

            query = CreateQueryParams(rndGen) ?? new Dictionary<string, string>();
            if (query.Count > 0)
            {
                JObject outputQuery = new JObject();
                expectedResult.Add("query", outputQuery);
                foreach (var kvp in query)
                {
                    outputQuery.Add(kvp.Key, kvp.Value);
                }
            }

            query.Add("format", outputFormat.ToString().ToLowerInvariant());
            expectedStatus = HttpStatusCode.OK;
            if (rndGen.Next(4) == 0)
            {
                // non-200 responses
                int[] options = new[] { 400, 404, 500, 201 };
                int status = options[rndGen.Next(options.Length)];
                expectedStatus = (HttpStatusCode)status;
                query.Add("status", status.ToString(CultureInfo.InvariantCulture));
            }
        }

        private static JToken SanitizeJsonXml(JToken body)
        {
            switch (body.Type)
            {
                case JTokenType.Null:
                    return JToken.Parse("null");
                case JTokenType.Boolean:
                case JTokenType.String:
                case JTokenType.Integer:
                case JTokenType.Float:
                    return new JValue((JValue)body);
                case JTokenType.Array:
                    JArray array = (JArray)body;
                    return new JArray(array.Select(jt => SanitizeJsonXml(jt)));
                case JTokenType.Object:
                    JObject obj = (JObject)body;
                    return new JObject(
                        obj.Properties().Select((jp, i) =>
                            new JProperty("member" + i, SanitizeJsonXml(jp.Value))));
                default:
                    throw new ArgumentException("Invalid type: " + body.Type);

            }
        }

        private static string JsonToXml(JToken json)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<root>");
            JsonToXml(json, sb);
            sb.Append("</root>");
            return sb.ToString();
        }

        private static void JsonToXml(JToken json, StringBuilder sb)
        {
            if (json == null)
            {
                json = "";
            }

            switch (json.Type)
            {
                case JTokenType.Null:
                    sb.Append("null");
                    break;
                case JTokenType.Boolean:
                    sb.Append(json.ToString().ToLowerInvariant());
                    break;
                case JTokenType.Float:
                case JTokenType.Integer:
                    sb.Append(json.ToString());
                    break;
                case JTokenType.String:
                    sb.Append(json.ToObject<string>());
                    break;
                case JTokenType.Array:
                    sb.Append("<array>");
                    JArray array = (JArray)json;
                    for (int i = 0; i < array.Count; i++)
                    {
                        sb.Append("<item>");
                        JsonToXml(array[i], sb);
                        sb.Append("</item>");
                    }

                    sb.Append("</array>");
                    break;
                case JTokenType.Object:
                    JObject obj = (JObject)json;
                    var keys = obj.Properties().Select(p => p.Name).ToArray();
                    Array.Sort(keys);
                    foreach (var key in keys)
                    {
                        sb.Append("<" + key + ">");
                        JsonToXml(obj[key], sb);
                        sb.Append("</" + key + ">");
                    }

                    break;
                default:
                    throw new ArgumentException("Type " + json.Type + " is not supported");
            }
        }

        private static ZumoTest CreateJTokenApiTest(ApiPermissions apiPermission, bool isAuthenticated, Random seedGenerator)
        {
            string testName = string.Format(CultureInfo.InvariantCulture, "JToken overload - {0}{1}",
                apiPermission, isAuthenticated ? " (user authenticated)" : "");
            var expecting401 = apiPermission == ApiPermissions.Admin || apiPermission == ApiPermissions.User && !isAuthenticated;
            return new ZumoTest(testName, async delegate(ZumoTest test)
            {
                var client = ZumoTestGlobals.Instance.Client;
                if (apiPermission == ApiPermissions.Public)
                {
                    test.AddLog("Public permission, using a client without an application key");
                    client = new MobileServiceClient(client.ApplicationUri);
                }

                var testResult = true;

                for (int i = 0; i < 10; i++)
                {
                    int seed = seedGenerator.Next();
                    test.AddLog("Test with seed = {0}", seed);
                    Random rndGen = new Random(seed);
                    var method = CreateHttpMethod(rndGen);
                    JToken body = null;
                    if (method.Method != "GET" && method.Method != "DELETE")
                    {
                        if (method.Method == "PATCH" || method.Method == "PUT")
                        {
                            // verbs which require a body
                            body = CreateJson(rndGen, 0, false);
                        }
                        else if (rndGen.Next(4) > 0)
                        {
                            body = CreateJson(rndGen);
                        }
                    }

                    var query = CreateQueryParams(rndGen);

                    JToken apiResult = null;
                    MobileServiceInvalidOperationException exception = null;
                    try
                    {
                        if (body == null && method == HttpMethod.Post && query == null)
                        {
                            test.AddLog("  -> Using the InvokeApiAsync(string) overload");
                            apiResult = await client.InvokeApiAsync(apiNames[apiPermission]);
                        }
                        else if (method == HttpMethod.Post && query == null)
                        {
                            test.AddLog("  -> Using the InvokeApiAsync(string, JToken) overload");
                            apiResult = await client.InvokeApiAsync(apiNames[apiPermission], body);
                        }
                        else if (body == null)
                        {
                            test.AddLog("  -> Using the InvokeApiAsync(string, HttpMethod ({0}), Dictionary<string, string>) overload", method.Method);
                            apiResult = await client.InvokeApiAsync(apiNames[apiPermission], method, query);
                        }
                        else
                        {
                            test.AddLog("  -> Using the InvokeApiAsync(string, JToken, HttpMethod ({0}), Dictionary<string, string>) overload", method.Method);
                            apiResult = await client.InvokeApiAsync(apiNames[apiPermission], body, method, query);
                        }
                    }
                    catch (MobileServiceInvalidOperationException ex)
                    {
                        exception = ex;
                    }

                    if (expecting401 && exception == null)
                    {
                        throw new InvalidOperationException("Test should have failed, but didn't - result = " + apiResult);
                    }

                    if (expecting401)
                    {
                        if (exception.Response.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            test.AddLog("  -> Succeeded");
                        }
                        else
                        {
                            test.AddLog("Unexpected exception: {0}", exception);
                            test.AddLog("Response: {0}", exception.Response);
                            var respBody = await exception.Response.Content.ReadAsStringAsync();
                            test.AddLog("Response body: {0}", respBody);
                            testResult = false;
                            break;
                        }
                    }
                    else
                    {
                        if (exception != null)
                        {
                            test.AddLog("Unexpected exception: {0}", exception);
                            test.AddLog("Response: {0}", exception.Response);
                            var respBody = await exception.Response.Content.ReadAsStringAsync();
                            test.AddLog("Response body: {0}", respBody);
                            testResult = false;
                            break;
                        }

                        JObject expectedResult = new JObject();
                        expectedResult.Add("user", GetUserObject(client));
                        if (query != null && query.Count > 0)
                        {
                            expectedResult.Add("query", GetQueryObject(query));
                        }

                        List<string> errors = new List<string>();
                        if (Util.CompareJson(expectedResult, apiResult, errors))
                        {
                            test.AddLog("  -> Succeeded");
                        }
                        else
                        {
                            test.AddLog("Results are different");
                            foreach (var error in errors)
                            {
                                test.AddLog("{0}", error);
                            }

                            testResult = false;
                            break;
                        }
                    }
                }

                return testResult;
            });
        }

        private static JObject GetQueryObject(Dictionary<string, string> query)
        {
            return new JObject(query.Select(kvp => new JProperty(kvp.Key, kvp.Value)).ToArray());
        }

        private static JObject GetUserObject(MobileServiceClient client)
        {
            if (client.CurrentUser == null)
            {
                return new JObject(new JProperty("level", "anonymous"));
            }
            else
            {
                return new JObject(
                    new JProperty("level", "authenticated"),
                    new JProperty("userId", client.CurrentUser.UserId));
            }
        }

        private static Dictionary<string, string> CreateQueryParams(Random rndGen)
        {
            if (rndGen.Next(2) == 0)
            {
                return null;
            }

            var result = new Dictionary<string, string>();
            var size = rndGen.Next(5);
            for (int i = 0; i < size; i++)
            {
                var name = CreateString(rndGen, 1, 10, Letters);
                var value = CreateString(rndGen);
                if (!result.ContainsKey(name))
                {
                    result.Add(name, value);
                }
            }

            return result;
        }

        private static string CreateString(Random rndGen, int minLength = 0, int maxLength = 30, string specificChars = null)
        {
            int length = rndGen.Next(minLength, maxLength);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                if (specificChars == null)
                {
                    if (rndGen.Next(3) > 0)
                    {
                        // common case, ascii characters
                        sb.Append((char)rndGen.Next(' ', '~'));
                    }
                    else
                    {
                        // all unicode, except surrogate
                        char c;
                        do
                        {
                            c = (char)rndGen.Next(' ', 0xfffe);
                        } while (Char.IsSurrogate(c));
                        sb.Append(c);
                    }
                }
                else
                {
                    sb.Append(specificChars[rndGen.Next(specificChars.Length)]);
                }
            }

            return sb.ToString();
        }

        private static JToken CreateJson(Random rndGen, int currentDepth = 0, bool canBeNull = true)
        {
            const int maxDepth = 3;
            int kind = rndGen.Next(15);

            // temp workaround
            if (currentDepth == 0)
            {
                kind = rndGen.Next(8, 15);
            }

            switch (kind)
            {
                case 0:
                    return true;
                case 1:
                    return false;
                case 2:
                    return rndGen.Next();
                case 3:
                    return rndGen.Next() >> rndGen.Next(10);
                case 4:
                case 5:
                case 6:
                    return CreateString(rndGen, 0, 10);
                case 7:
                    if (canBeNull)
                    {
                        return JToken.Parse("null");
                    }
                    else
                    {
                        return CreateString(rndGen, 0, 10);
                    }
                case 8:
                case 9:
                case 10:
                    if (currentDepth > maxDepth)
                    {
                        return "max depth";
                    }
                    else
                    {
                        int size = rndGen.Next(5);
                        JArray result = new JArray();
                        for (int i = 0; i < size; i++)
                        {
                            result.Add(CreateJson(rndGen, currentDepth + 1));
                        }

                        return result;
                    }
                default:
                    if (currentDepth > maxDepth)
                    {
                        return "max depth";
                    }
                    else
                    {
                        int size = rndGen.Next(5);
                        JObject result = new JObject();
                        for (int i = 0; i < size; i++)
                        {
                            string key;
                            do
                            {
                                key = CreateString(rndGen, 3, 5);
                            } while (result[key] != null);
                            result.Add(key, CreateJson(rndGen, currentDepth + 1));
                        }

                        return result;
                    }
            }
        }

        private static HttpMethod CreateHttpMethod(Random rndGen)
        {
            switch (rndGen.Next(10))
            {
                case 0:
                case 1:
                case 2:
                    return HttpMethod.Post;
                case 3:
                case 4:
                case 5:
                case 6:
                    return HttpMethod.Get;
                case 7:
                    return HttpMethod.Put;
                case 8:
                    return HttpMethod.Delete;
                default:
                    return new HttpMethod("PATCH");
            }
        }
    }
}
