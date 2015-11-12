/*
Copyright (c) Microsoft Open Technologies, Inc.
All Rights Reserved
Apache 2.0 License
 
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at
 
     http://www.apache.org/licenses/LICENSE-2.0
 
   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
 
See the Apache Version 2.0 License for specific language governing permissions and limitations under the License.
 */
package com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests;

import android.util.Pair;

import com.google.common.util.concurrent.FutureCallback;
import com.google.common.util.concurrent.Futures;
import com.google.common.util.concurrent.ListenableFuture;
import com.google.common.util.concurrent.SettableFuture;
import com.google.gson.JsonArray;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.http.NextServiceFilterCallback;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilter;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterRequest;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.table.MobileServiceJsonTable;
import com.microsoft.windowsazure.mobileservices.table.MobileServiceTable;
import com.microsoft.windowsazure.mobileservices.table.query.ExecutableJsonQuery;
import com.microsoft.windowsazure.mobileservices.table.query.ExecutableQuery;
import com.microsoft.windowsazure.mobileservices.table.query.Query;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.ExpectedValueException;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.FroyoAndroidHttpClientFactory;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestCase;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestExecutionCallback;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestGroup;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestResult;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestStatus;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.Util;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.types.IntIdRoundTripTableElement;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.types.ParamsTestTableItem;

import org.apache.http.Header;
import org.apache.http.client.methods.HttpGet;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Locale;
import java.util.Random;
import java.util.UUID;

import static com.microsoft.windowsazure.mobileservices.table.query.QueryOperations.field;

public class MiscTests extends TestGroup {

    protected static final String ROUND_TRIP_TABLE_NAME = "intIdRoundTripTable";
    protected static final String PARAM_TEST_TABLE_NAME = "ParamsTestTable";
    private static final String APP_API_NAME = "applicationPermission";

    public MiscTests() {
        super("Misc tests");

        this.addTest(createFilterTestWithMultipleRequests(true));
        this.addTest(createFilterTestWithMultipleRequests(false));

        TestCase withFilterDoesNotChangeTheClient = new TestCase() {

            @Override
            protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {
                final TestResult testResult = new TestResult();
                testResult.setTestCase(this);
                testResult.setStatus(TestStatus.Passed);
                final TestCase testCase = this;
                client.withFilter(new ServiceFilter() {

                    @Override
                    public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request,
                                                                                 NextServiceFilterCallback nextServiceFilterCallback) {
                        log("executed filter triggering failure");
                        testResult.setStatus(TestStatus.Failed);

                        return nextServiceFilterCallback.onNext(request);
                    }
                });

                log("execute query");

                try {

                    client.getTable(ROUND_TRIP_TABLE_NAME).top(5).execute().get();
                } catch (Exception exception) {
                    createResultFromException(testResult, exception);
                }

                if (callback != null)
                    callback.onTestComplete(testCase, testResult);
            }

        };
        withFilterDoesNotChangeTheClient.setName("Verify that 'withFilter' does not change the client");
        this.addTest(withFilterDoesNotChangeTheClient);

        TestCase bypassTest = new TestCase() {

            @Override
            protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {

                final TestCase testCase = this;
                final TestResult result = new TestResult();
                result.setStatus(TestStatus.Passed);
                result.setTestCase(this);

                final String json = "{'id':1,'name':'John Doe','age':33}".replace('\'', '\"');
                MobileServiceClient filtered = client.withFilter(new ServiceFilter() {

                    @Override
                    public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request,
                                                                                 NextServiceFilterCallback nextServiceFilterCallback) {

                        final SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture.create();

                        // ListenableFuture<ServiceFilterResponse> notifyFuture
                        // = nextServiceFilterCallback.onNext(request);
                        //
                        // Futures.addCallback(notifyFuture, new
                        // FutureCallback<ServiceFilterResponse>() {
                        //
                        // @Override
                        // public void onFailure(Throwable exception) {
                        // resultFuture.setException(exception);
                        // }
                        //
                        // @Override
                        // public void onSuccess(ServiceFilterResponse v) {
                        resultFuture.set(new MockResponse(json, 201));
                        // return;
                        // }
                        // });

                        return resultFuture;
                    }
                });

                log("insert item");

                try {

                    JsonObject jsonEntity = filtered.getTable("fakeTable").insert(new JsonObject()).get();

                    JsonObject expectedObject = new JsonParser().parse(json).getAsJsonObject();
                    log("verify items are equal");
                    if (!Util.compareJson(expectedObject, jsonEntity)) {
                        createResultFromException(result, new ExpectedValueException(expectedObject, jsonEntity));
                    }
                } catch (Exception exception) {

                    createResultFromException(result, exception);
                } finally {
                    if (callback != null)
                        callback.onTestComplete(testCase, result);
                }
            }
        };

        bypassTest.setName("Filter to bypass service");
        this.addTest(bypassTest);

        this.addTest(createParameterPassingTest(true));
        this.addTest(createParameterPassingTest(false));

        this.addTest(createHttpContentApiTest());

        this.addTest(createFroyoFixedRequestTest());

        this.addTest(new TestCase("User-Agent validation") {

            @Override
            protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {
                final TestCase testCase = this;
                final TestResult testResult = new TestResult();
                testResult.setTestCase(testCase);
                testResult.setStatus(TestStatus.Failed);
                client = client.withFilter(new ServiceFilter() {

                    public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request,
                                                                                 NextServiceFilterCallback nextServiceFilterCallback) {

                        final SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture.create();

                        Header[] headers = request.getHeaders();
                        for (Header reqHeader : headers) {
                            if (reqHeader.getName() == "User-Agent") {
                                String userAgent = reqHeader.getValue();
                                log("User-Agent: " + userAgent);
                                testResult.setStatus(TestStatus.Passed);
                                String clientVersion = userAgent;
                                if (clientVersion.endsWith(")")) {
                                    clientVersion = clientVersion.substring(0, clientVersion.length() - 1);
                                }
                                int indexOfEquals = clientVersion.lastIndexOf('=');
                                if (indexOfEquals >= 0) {
                                    clientVersion = clientVersion.substring(indexOfEquals + 1);
                                    Util.getGlobalTestParameters().put(ClientVersionKey, clientVersion);
                                }
                            }
                        }

                        ListenableFuture<ServiceFilterResponse> notifyFuture = nextServiceFilterCallback.onNext(request);

                        Futures.addCallback(notifyFuture, new FutureCallback<ServiceFilterResponse>() {

                            @Override
                            public void onFailure(Throwable exception) {
                            }

                            @Override
                            public void onSuccess(ServiceFilterResponse response) {
                                if (response != null) {
                                    Header[] respHeaders = response.getHeaders();
                                    for (Header header : respHeaders) {
                                        if (header.getName().equalsIgnoreCase("x-zumo-version")) {
                                            String runtimeVersion = header.getValue();
                                            Util.getGlobalTestParameters().put(ServerVersionKey, runtimeVersion);
                                        }
                                    }
                                }

                                resultFuture.set(response);

                            }
                        });

                        return resultFuture;
                    }
                });

                log("execute query to activate filter");

                try {

                    client.getTable(ROUND_TRIP_TABLE_NAME).top(5).execute().get();
                } catch (Exception exception) {
                    createResultFromException(testResult, exception);
                } finally {

                    if (callback != null)
                        callback.onTestComplete(testCase, testResult);
                }
            }
        });
    }

    private TestCase createFroyoFixedRequestTest() {
        final TestCase test = new TestCase() {

            @Override
            protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {
                final TestResult result = new TestResult();
                result.setTestCase(this);
                result.setStatus(TestStatus.Passed);
                final TestCase testCase = this;

                // duplicate the client
                MobileServiceClient froyoClient = new MobileServiceClient(client.getAppUrl(), client.getContext());

                log("add custom AndroidHttpClientFactory with Froyo support");
                froyoClient.setAndroidHttpClientFactory(new FroyoAndroidHttpClientFactory());

                MobileServiceTable<IntIdRoundTripTableElement> table = froyoClient.getTable(ROUND_TRIP_TABLE_NAME, IntIdRoundTripTableElement.class);

                try {

                    table.where().field("id").eq("1").execute().get();
                } catch (Exception exception) {
                    createResultFromException(result, exception);
                } finally {

                    if (callback != null)
                        callback.onTestComplete(testCase, result);
                }
            }
        };

        test.setName("Simple request on Froyo");
        return test;
    }

    private TestCase createParameterPassingTest(final boolean typed) {
        TestCase test = new TestCase() {

            @Override
            protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {
                final TestResult result = new TestResult();
                result.setTestCase(this);
                result.setStatus(TestStatus.Passed);
                final TestCase testCase = this;

                final HashMap<String, String> params = createParams();

                final JsonObject expectedParameters = new JsonObject();

                for (String key : params.keySet()) {
                    expectedParameters.addProperty(key, params.get(key));
                }

                params.put("operation", "read");
                expectedParameters.addProperty("operation", "read");

                log("execute query");
                if (typed) {
                    MobileServiceTable<ParamsTestTableItem> table = client.getTable(PARAM_TEST_TABLE_NAME, ParamsTestTableItem.class);
                    ExecutableQuery<ParamsTestTableItem> query = table.where();
                    addParametersToQuery(query, params);

                    try {
                        List<ParamsTestTableItem> elements = query.execute().get();

                        log("verify size = 0");
                        if (elements.size() != 0) {
                            JsonObject actualParameters = elements.get(0).getParameters();

                            log("verify items are equal");
                            if (!Util.compareJson(expectedParameters, actualParameters)) {
                                createResultFromException(result, new ExpectedValueException(expectedParameters, actualParameters));
                            }

                            if (callback != null)
                                callback.onTestComplete(testCase, result);
                        } else {
                            createResultFromException(result, new ExpectedValueException("SIZE > 0", "SIZE == 0"));
                            if (callback != null)
                                callback.onTestComplete(testCase, result);
                        }

                    } catch (Exception exception) {

                        createResultFromException(result, exception);
                    } finally {
                        if (callback != null)
                            callback.onTestComplete(testCase, result);
                    }
                } else {
                    MobileServiceJsonTable table = client.getTable(PARAM_TEST_TABLE_NAME);
                    ExecutableJsonQuery query = table.where();
                    addParametersToQuery(query, params);

                    try {
                        JsonElement elements = query.execute().get();

                        log("verify result is JsonArray with at least one element");
                        if (elements.isJsonArray() && (elements.getAsJsonArray()).size() != 0) {
                            try {
                                log("get parameters object and recreate json");
                                JsonObject actualObject = elements.getAsJsonArray().get(0).getAsJsonObject();
                                actualObject.add("parameters", new JsonParser().parse(actualObject.get("parameters").getAsString()));
                                JsonObject expectedObject = new JsonObject();
                                expectedObject.addProperty("id", 1);
                                expectedObject.add("parameters", expectedParameters);

                                log("verify items are equal");
                                if (!Util.compareJson(expectedObject, actualObject)) {
                                    createResultFromException(result, new ExpectedValueException(expectedObject, actualObject));
                                }
                            } catch (Exception e) {
                                createResultFromException(result, e);
                            }

                        }
                    } catch (Exception exception) {
                        createResultFromException(result, new ExpectedValueException("JSON ARRAY WITH ELEMENTS", "EMPTY RESULT"));
                    } finally {
                        if (callback != null)
                            callback.onTestComplete(testCase, result);
                    }

                }
            }

            private void addParametersToQuery(Query query, HashMap<String, String> params) {
                for (String key : params.keySet()) {
                    query.parameter(key, params.get(key));
                }
            }
        };

        test.setName("Parameter passing test - tables (" + (typed ? "typed" : "untyped") + ")");
        return test;
    }

    private HashMap<String, String> createParams() {
        HashMap<String, String> params = new HashMap<String, String>();
        params.put("item", "simple");
        params.put("empty", "");
        params.put("spaces", "with spaces");
        params.put("specialChars", "`!@#$%^&*()-=[]\\;',./~_+{}|:\"<>?");
        params.put("latin", "ãéìôü ÇñÑ");
        params.put("arabic", "الكتاب على الطاولة");
        params.put("chinese", "这本书在桌子上");
        params.put("japanese", "本は机の上に");
        params.put("hebrew", "הספר הוא על השולחן");
        params.put("name+with special&chars", "should just work");
        return params;
    }

    private TestCase createFilterTestWithMultipleRequests(final boolean typed) {
        TestCase test = new TestCase() {

            MobileServiceJsonTable mTable;

            TestExecutionCallback mCallback;
            int mNumberOfRequest;
            String mUUID;

            @Override
            protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {
                Random rndGen = new Random();

                mNumberOfRequest = rndGen.nextInt(3) + 3;
                log("number of requests: " + mNumberOfRequest);
                MobileServiceClient filteredClient = client.withFilter(new MultipleRequestFilter(mNumberOfRequest));

                mTable = client.getTable(ROUND_TRIP_TABLE_NAME);

                mCallback = callback;

                mUUID = UUID.randomUUID().toString();
                log("insert item " + mUUID);
                if (typed) {
                    MobileServiceTable<IntIdRoundTripTableElement> filteredClientTable = filteredClient.getTable(ROUND_TRIP_TABLE_NAME, IntIdRoundTripTableElement.class);

                    IntIdRoundTripTableElement item = new IntIdRoundTripTableElement();
                    item.name = mUUID;

                    try {
                        filteredClientTable.insert(item).get();
                        requestCompleted(null);
                    } catch (Exception exception) {
                        requestCompleted(exception);
                    }
                } else {
                    MobileServiceJsonTable filteredClientTable = filteredClient.getTable(ROUND_TRIP_TABLE_NAME);

                    JsonObject item = new JsonObject();
                    item.addProperty("name", mUUID);

                    try {
                        filteredClientTable.insert(item).get();
                        requestCompleted(null);
                    } catch (Exception exception) {
                        requestCompleted(exception);
                    }
                }
            }

            private void requestCompleted(Exception exception) {// ,
                // ServiceFilterResponse
                // response) {
                final TestResult testResult = new TestResult();
                testResult.setTestCase(this);
                testResult.setStatus(TestStatus.Passed);

                if (exception != null) {
                    createResultFromException(testResult, exception);
                    if (mCallback != null)
                        mCallback.onTestComplete(this, testResult);
                    return;
                }

                final TestCase testCase = this;
                log("retrieve the original item");

                try {

                    JsonElement result = mTable.where(field("name").eq(mUUID)).select("name", "id").execute().get();

                    log("verify that there are " + mNumberOfRequest + " elements in the JsonArray");

                    if (!result.isJsonArray()) {
                        createResultFromException(testResult, new ExpectedValueException("JSON ARRAY", result.toString()));
                    } else if (result.getAsJsonArray().size() != mNumberOfRequest) {
                        createResultFromException(testResult,
                                new ExpectedValueException(mNumberOfRequest + " Times", result.getAsJsonArray().size() + " Times"));
                    }

                    if (testResult.getStatus() == TestStatus.Failed) {
                        if (mCallback != null)
                            mCallback.onTestComplete(testCase, testResult);
                        return;
                    }

                    JsonArray jsonArray = result.getAsJsonArray();
                    for (int i = 0; i < jsonArray.size(); i++) {
                        final boolean doCallback = i == jsonArray.size() - 1;
                        log("delete item " + jsonArray.get(i));

                        try {
                            mTable.delete(jsonArray.get(i).getAsJsonObject()).get();

                        } catch (Exception exception2) {
                            createResultFromException(testResult, exception2);
                        } finally {
                            if (doCallback) {
                                if (mCallback != null)
                                    mCallback.onTestComplete(testCase, testResult);
                            }
                        }
                    }
                } catch (Exception exception3) {
                    createResultFromException(testResult, exception3);

                    if (mCallback != null)
                        mCallback.onTestComplete(testCase, testResult);
                    return;
                }
            }
        };

        String name = String.format(Locale.getDefault(), "Filter which maps one requests to many - %s client", typed ? "typed" : "untyped");
        test.setName(name);

        return test;
    }

    private TestCase createHttpContentApiTest() {
        String name = "Use \"text/plain\" Content and \"identity\" Encoding Headers";

        TestCase test = new TestCase(name) {
            MobileServiceClient mClient;
            List<Pair<String, String>> mQuery;
            List<Pair<String, String>> mHeaders;
            TestExecutionCallback mCallback;
            JsonObject mExpectedResult;
            int mExpectedStatusCode;
            String mHttpMethod;
            byte[] mContent;

            TestResult mResult;

            @Override
            protected void executeTest(MobileServiceClient client, TestExecutionCallback callback) {
                mResult = new TestResult();
                mResult.setTestCase(this);
                mResult.setStatus(TestStatus.Passed);
                mClient = client;
                mCallback = callback;

                createHttpContentTestInput();

                try {

                    ServiceFilterResponse response = mClient.invokeApi(APP_API_NAME, mContent, mHttpMethod, mHeaders, mQuery).get();

                    Exception ex = validateResponse(response);
                    if (ex != null) {
                        createResultFromException(mResult, ex);
                    } else {
                        mResult.getTestCase().log("Header validated successfully");

                        String responseContent = response.getContent();

                        mResult.getTestCase().log("Response content: " + responseContent);

                        JsonElement jsonResponse = null;
                        String decodedContent = responseContent.replace("__{__", "{").replace("__}__", "}").replace("__[__", "[").replace("__]__", "]");
                        jsonResponse = new JsonParser().parse(decodedContent);

                        if (!Util.compareJson(mExpectedResult, jsonResponse)) {
                            createResultFromException(mResult, new ExpectedValueException(mExpectedResult, jsonResponse));
                        }
                    }

                    mCallback.onTestComplete(mResult.getTestCase(), mResult);
                } catch (Exception exception) {
                    createResultFromException(exception);
                    mCallback.onTestComplete(mResult.getTestCase(), mResult);
                    return;
                }
            }

            ;

            private Exception validateResponse(ServiceFilterResponse response) {
                if (mExpectedStatusCode != response.getStatus().getStatusCode()) {
                    mResult.getTestCase().log("Invalid status code");
                    String content = response.getContent();
                    if (content != null) {
                        mResult.getTestCase().log("Response: " + content);
                    }
                    return new ExpectedValueException(mExpectedStatusCode, response.getStatus().getStatusCode());
                } else {
                    return null;
                }
            }

            private void createHttpContentTestInput() {
                mHttpMethod = HttpGet.METHOD_NAME;
                log("Method = " + mHttpMethod);

                mExpectedResult = new JsonObject();
                mExpectedResult.addProperty("method", mHttpMethod);
                JsonObject user = new JsonObject();
                user.addProperty("level", "anonymous");
                mExpectedResult.add("user", user);

                mHeaders = new ArrayList<Pair<String, String>>();
                mHeaders.add(new Pair<String, String>("Accept", "text/plain"));
                mHeaders.add(new Pair<String, String>("Accept-Encoding", "identity"));

                mQuery = new ArrayList<Pair<String, String>>();
                mQuery.add(new Pair<String, String>("format", "other"));

                mExpectedStatusCode = 200;
            }
        };

        return test;
    }
}
