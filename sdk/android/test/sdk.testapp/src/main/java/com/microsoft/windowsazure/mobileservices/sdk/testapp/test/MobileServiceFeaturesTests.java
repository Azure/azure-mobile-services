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
package com.microsoft.windowsazure.mobileservices.sdk.testapp.test;

import android.net.Uri;
import android.test.InstrumentationTestCase;
import android.util.Pair;

import com.google.common.util.concurrent.ListenableFuture;
import com.google.common.util.concurrent.SettableFuture;
import com.google.gson.JsonObject;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceException;
import com.microsoft.windowsazure.mobileservices.MobileServiceFeatures;
import com.microsoft.windowsazure.mobileservices.http.HttpConstants;
import com.microsoft.windowsazure.mobileservices.http.NextServiceFilterCallback;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilter;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterRequest;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.framework.filters.ServiceFilterResponseMock;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.framework.mocks.MobileServiceLocalStoreMock;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.test.types.Address;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.test.types.PersonTestObjectWithStringId;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.test.types.SystemPropertyTestClasses.VersionType;
import com.microsoft.windowsazure.mobileservices.table.MobileServiceJsonTable;
import com.microsoft.windowsazure.mobileservices.table.MobileServiceSystemProperty;
import com.microsoft.windowsazure.mobileservices.table.MobileServiceTable;
import com.microsoft.windowsazure.mobileservices.table.query.ExecutableJsonQuery;
import com.microsoft.windowsazure.mobileservices.table.query.ExecutableQuery;
import com.microsoft.windowsazure.mobileservices.table.sync.MobileServiceJsonSyncTable;
import com.microsoft.windowsazure.mobileservices.table.sync.MobileServiceSyncTable;
import com.microsoft.windowsazure.mobileservices.table.sync.localstore.ColumnDataType;
import com.microsoft.windowsazure.mobileservices.table.sync.synchandler.SimpleSyncHandler;
import com.squareup.okhttp.Headers;

import java.net.MalformedURLException;
import java.util.ArrayList;
import java.util.EnumSet;
import java.util.HashMap;
import java.util.Hashtable;
import java.util.List;
import java.util.Map;
import java.util.concurrent.ExecutionException;

public class MobileServiceFeaturesTests extends InstrumentationTestCase {
    String appUrl;

    protected void setUp() throws Exception {
        appUrl = "http://myapp.com/";
        super.setUp();
    }

    protected void tearDown() throws Exception {
        super.tearDown();
    }

    public void testFeaturesToStringConversion() {
        Hashtable<EnumSet<MobileServiceFeatures>, String> cases;
        cases = new Hashtable<EnumSet<MobileServiceFeatures>, String>();
        for (MobileServiceFeatures feature : MobileServiceFeatures.class.getEnumConstants()) {
            cases.put(EnumSet.of(feature), feature.getValue());
        }
        cases.put(EnumSet.of(MobileServiceFeatures.TypedTable, MobileServiceFeatures.AdditionalQueryParameters), "QS,TT");
        cases.put(EnumSet.of(MobileServiceFeatures.UntypedTable, MobileServiceFeatures.AdditionalQueryParameters), "QS,TU");
        cases.put(EnumSet.of(MobileServiceFeatures.TypedTable, MobileServiceFeatures.Offline), "OL,TT");
        cases.put(EnumSet.of(MobileServiceFeatures.UntypedTable, MobileServiceFeatures.Offline), "OL,TU");
        cases.put(EnumSet.of(MobileServiceFeatures.TypedApiCall, MobileServiceFeatures.AdditionalQueryParameters), "AT,QS");
        cases.put(EnumSet.of(MobileServiceFeatures.JsonApiCall, MobileServiceFeatures.AdditionalQueryParameters), "AJ,QS");
        cases.put(EnumSet.of(MobileServiceFeatures.OpportunisticConcurrency, MobileServiceFeatures.Offline, MobileServiceFeatures.UntypedTable), "OC,OL,TU");

        for (EnumSet<MobileServiceFeatures> features : cases.keySet()) {
            String expected = cases.get(features);
            String actual = MobileServiceFeatures.featuresToString(features);
            assertEquals(expected, actual);
        }
    }

    // Typed tables
    public void testTypedTableInsertFeatureHeader() {
        testTableFeatureHeader(new TableTestOperation() {

            @Override
            public void executeOperation(MobileServiceTable<PersonTestObjectWithStringId> typedTable, MobileServiceJsonTable jsonTable) throws Exception {
                PersonTestObjectWithStringId pto = new PersonTestObjectWithStringId("John", "Doe", 33);
                typedTable.insert(pto).get();
            }
        }, false, "TT");
    }

    public void testTypedTableInsertWithParametersFeatureHeader() {
        testTableFeatureHeader(new TableTestOperation() {

            @Override
            public void executeOperation(MobileServiceTable<PersonTestObjectWithStringId> typedTable, MobileServiceJsonTable jsonTable) throws Exception {
                PersonTestObjectWithStringId pto = new PersonTestObjectWithStringId("John", "Doe", 33);
                List<Pair<String, String>> queryParams = new ArrayList<Pair<String, String>>();
                queryParams.add(new Pair<String, String>("a", "b"));
                typedTable.insert(pto, queryParams).get();
            }
        }, false, "QS,TT");
    }

    public void testTypedTableInsertWithEmptyParametersFeatureHeader() {
        testTableFeatureHeader(new TableTestOperation() {

            @Override
            public void executeOperation(MobileServiceTable<PersonTestObjectWithStringId> typedTable, MobileServiceJsonTable jsonTable) throws Exception {
                PersonTestObjectWithStringId pto = new PersonTestObjectWithStringId("John", "Doe", 33);
                List<Pair<String, String>> queryParams = new ArrayList<Pair<String, String>>();
                typedTable.insert(pto, queryParams).get();
            }
        }, false, "TT");
    }

    public void testTypedTableUpdateFeatureHeader() {
        testTableFeatureHeader(new TableTestOperation() {

            @Override
            public void executeOperation(MobileServiceTable<PersonTestObjectWithStringId> typedTable, MobileServiceJsonTable jsonTable) throws Exception {
                PersonTestObjectWithStringId pto = new PersonTestObjectWithStringId("John", "Doe", 33);
                pto.setId("the-id");
                typedTable.update(pto).get();
            }
        }, false, "TT");
    }

    public void testTypedTableUpdateWithParametersFeatureHeader() {
        testTableFeatureHeader(new TableTestOperation() {

            @Override
            public void executeOperation(MobileServiceTable<PersonTestObjectWithStringId> typedTable, MobileServiceJsonTable jsonTable) throws Exception {
                PersonTestObjectWithStringId pto = new PersonTestObjectWithStringId("John", "Doe", 33);
                pto.setId("the-id");
                List<Pair<String, String>> queryParams = new ArrayList<Pair<String, String>>();
                queryParams.add(new Pair<String, String>("a", "b"));
                typedTable.update(pto, queryParams).get();
            }
        }, false, "QS,TT");
    }

    public void testTypedTableDeleteNoParametersNoFeatureHeader() {
        testTableFeatureHeader(new TableTestOperation() {

            @Override
            public void executeOperation(MobileServiceTable<PersonTestObjectWithStringId> typedTable, MobileServiceJsonTable jsonTable) throws Exception {
                PersonTestObjectWithStringId pto = new PersonTestObjectWithStringId("John", "Doe", 33);
                pto.setId("the-id");
                typedTable.delete(pto).get();
            }
        }, false, "TT");
    }

    public void testTypedTableDeleteWithParametersFeatureHeader() {
        testTableFeatureHeader(new TableTestOperation() {

            @Override
            public void executeOperation(MobileServiceTable<PersonTestObjectWithStringId> typedTable, MobileServiceJsonTable jsonTable) throws Exception {
                PersonTestObjectWithStringId pto = new PersonTestObjectWithStringId("John", "Doe", 33);
                pto.setId("the-id");
                List<Pair<String, String>> queryParams = new ArrayList<Pair<String, String>>();
                queryParams.add(new Pair<String, String>("a", "b"));
                typedTable.delete(pto, queryParams).get();
            }
        }, false, "QS,TT");
    }

    public void testTypedTableLookupFeatureHeader() {
        testTableFeatureHeader(new TableTestOperation() {

            @Override
            public void executeOperation(MobileServiceTable<PersonTestObjectWithStringId> typedTable, MobileServiceJsonTable jsonTable) throws Exception {
                typedTable.lookUp("1").get();
            }
        }, false, "TT");
    }

    public void testTypedTableLookupWithParametersFeatureHeader() {
        testTableFeatureHeader(new TableTestOperation() {

            @Override
            public void executeOperation(MobileServiceTable<PersonTestObjectWithStringId> typedTable, MobileServiceJsonTable jsonTable) throws Exception {
                List<Pair<String, String>> queryParams = new ArrayList<Pair<String, String>>();
                queryParams.add(new Pair<String, String>("a", "b"));
                typedTable.lookUp("1", queryParams).get();
            }
        }, false, "QS,TT");
    }

    public void testTypedTableReadFeatureHeader() {
        testTableFeatureHeader(new TableTestOperation() {

            @Override
            public void executeOperation(MobileServiceTable<PersonTestObjectWithStringId> typedTable, MobileServiceJsonTable jsonTable) throws Exception {
                typedTable.execute().get();
            }
        }, true, "TT");
    }

    public void testTypedQueryFeatureHeader() {
        testTableFeatureHeader(new TableTestOperation() {

            @Override
            public void executeOperation(MobileServiceTable<PersonTestObjectWithStringId> typedTable, MobileServiceJsonTable jsonTable) throws Exception {
                typedTable.parameter("a", "b").execute().get();
            }
        }, true, "QS,TT");
    }

    // JSON tables
    private JsonObject createJsonObject() {
        JsonObject result = new JsonObject();
        result.addProperty("id", "the-id");
        result.addProperty("firstName", "John");
        result.addProperty("lastName", "Doe");
        result.addProperty("age", 33);
        return result;
    }

    public void testJsonTableInsertFeatureHeader() {
        testTableFeatureHeader(new TableTestOperation() {

            @Override
            public void executeOperation(MobileServiceTable<PersonTestObjectWithStringId> typedTable, MobileServiceJsonTable jsonTable) throws Exception {
                JsonObject jo = createJsonObject();
                jsonTable.insert(jo).get();
            }
        }, false, "TU");
    }

    public void testJsonTableInsertWithParametersFeatureHeader() {
        testTableFeatureHeader(new TableTestOperation() {

            @Override
            public void executeOperation(MobileServiceTable<PersonTestObjectWithStringId> typedTable, MobileServiceJsonTable jsonTable) throws Exception {
                JsonObject jo = createJsonObject();
                List<Pair<String, String>> queryParams = new ArrayList<Pair<String, String>>();
                queryParams.add(new Pair<String, String>("a", "b"));
                jsonTable.insert(jo, queryParams).get();
            }
        }, false, "QS,TU");
    }

    public void testJsonTableInsertWithEmptyParametersFeatureHeader() {
        testTableFeatureHeader(new TableTestOperation() {

            @Override
            public void executeOperation(MobileServiceTable<PersonTestObjectWithStringId> typedTable, MobileServiceJsonTable jsonTable) throws Exception {
                JsonObject jo = createJsonObject();
                List<Pair<String, String>> queryParams = new ArrayList<Pair<String, String>>();
                jsonTable.insert(jo, queryParams).get();
            }
        }, false, "TU");
    }

    public void testJsonTableUpdateFeatureHeader() {
        testTableFeatureHeader(new TableTestOperation() {

            @Override
            public void executeOperation(MobileServiceTable<PersonTestObjectWithStringId> typedTable, MobileServiceJsonTable jsonTable) throws Exception {
                JsonObject jo = createJsonObject();
                jsonTable.update(jo).get();
            }
        }, false, "TU");
    }

    public void testJsonTableUpdateWithParametersFeatureHeader() {
        testTableFeatureHeader(new TableTestOperation() {

            @Override
            public void executeOperation(MobileServiceTable<PersonTestObjectWithStringId> typedTable, MobileServiceJsonTable jsonTable) throws Exception {
                JsonObject jo = createJsonObject();
                List<Pair<String, String>> queryParams = new ArrayList<Pair<String, String>>();
                queryParams.add(new Pair<String, String>("a", "b"));
                jsonTable.update(jo, queryParams).get();
            }
        }, false, "QS,TU");
    }

    public void testJsonTableDeleteFeatureHeader() {
        testTableFeatureHeader(new TableTestOperation() {

            @Override
            public void executeOperation(MobileServiceTable<PersonTestObjectWithStringId> typedTable, MobileServiceJsonTable jsonTable) throws Exception {
                JsonObject jo = createJsonObject();
                jsonTable.delete(jo).get();
            }
        }, false, "TU");
    }

    public void testJsonTableDeleteWithParametersFeatureHeader() {
        testTableFeatureHeader(new TableTestOperation() {

            @Override
            public void executeOperation(MobileServiceTable<PersonTestObjectWithStringId> typedTable, MobileServiceJsonTable jsonTable) throws Exception {
                JsonObject jo = createJsonObject();
                List<Pair<String, String>> queryParams = new ArrayList<Pair<String, String>>();
                queryParams.add(new Pair<String, String>("a", "b"));
                jsonTable.delete(jo, queryParams).get();
            }
        }, false, "QS,TU");
    }

    public void testJsonTableLookupFeatureHeader() {
        testTableFeatureHeader(new TableTestOperation() {

            @Override
            public void executeOperation(MobileServiceTable<PersonTestObjectWithStringId> typedTable, MobileServiceJsonTable jsonTable) throws Exception {
                jsonTable.lookUp("1").get();
            }
        }, false, "TU");
    }

    public void testJsonTableLookupWithParametersFeatureHeader() {
        testTableFeatureHeader(new TableTestOperation() {

            @Override
            public void executeOperation(MobileServiceTable<PersonTestObjectWithStringId> typedTable, MobileServiceJsonTable jsonTable) throws Exception {
                List<Pair<String, String>> queryParams = new ArrayList<Pair<String, String>>();
                queryParams.add(new Pair<String, String>("a", "b"));
                jsonTable.lookUp("1", queryParams).get();
            }
        }, false, "QS,TU");
    }

    public void testJsonTableReadFeatureHeader() {
        testTableFeatureHeader(new TableTestOperation() {

            @Override
            public void executeOperation(MobileServiceTable<PersonTestObjectWithStringId> typedTable, MobileServiceJsonTable jsonTable) throws Exception {
                jsonTable.execute().get();
            }
        }, true, "TU");
    }

    public void testJsonQueryFeatureHeader() {
        testTableFeatureHeader(new TableTestOperation() {

            @Override
            public void executeOperation(MobileServiceTable<PersonTestObjectWithStringId> typedTable, MobileServiceJsonTable jsonTable) throws Exception {
                jsonTable.parameter("a", "b").execute().get();
            }
        }, true, "QS,TU");
    }

    private void testTableFeatureHeader(TableTestOperation operation, final boolean responseIsArray, final String expectedFeaturesHeader) {
        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a new filter to the client
        client = client.withFilter(new ServiceFilter() {

            @Override
            public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

                final SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture.create();
                String featuresHeaderName = "X-ZUMO-FEATURES";

                Headers headers = request.getHeaders();
                String features = null;
                for (int i = 0; i < headers.size(); i++) {
                    if (headers.name(i) == featuresHeaderName) {
                        features = headers.value(i);
                    }
                }

                if (features == null) {
                    resultFuture.setException(new Exception("No " + featuresHeaderName + " header on API call"));
                } else if (!features.equals(expectedFeaturesHeader)) {
                    resultFuture.setException(new Exception("Incorrect features header; expected " + expectedFeaturesHeader + ", actual " + features));
                } else {
                    ServiceFilterResponseMock response = new ServiceFilterResponseMock();
                    String content = "{\"id\":\"the-id\",\"firstName\":\"John\",\"lastName\":\"Doe\",\"age\":33}";
                    if (responseIsArray) {
                        content = "[" + content + "]";
                    }
                    response.setContent(content);
                    resultFuture.set(response);
                }

                return resultFuture;
            }
        });

        try {
            MobileServiceTable<PersonTestObjectWithStringId> typedTable = client.getTable(PersonTestObjectWithStringId.class);
            MobileServiceJsonTable jsonTable = client.getTable("Person");
            operation.executeOperation(typedTable, jsonTable);
        } catch (Exception exception) {
            Throwable ex = exception;
            while (ex instanceof ExecutionException || ex instanceof MobileServiceException) {
                ex = ex.getCause();
            }
            ex.printStackTrace();
            fail(ex.getMessage());
        }
    }

    public void testJsonApiFeatureHeader() {
        testInvokeApiFeatureHeader(new ClientTestOperation() {

            @Override
            public void executeOperation(MobileServiceClient client) throws Exception {
                client.invokeApi("foo").get();
            }

        }, "AJ");
    }

    public void testJsonApiWithQueryParametersFeatureHeader() {
        testInvokeApiFeatureHeader(new ClientTestOperation() {

            @Override
            public void executeOperation(MobileServiceClient client) throws Exception {
                List<Pair<String, String>> queryParams = new ArrayList<Pair<String, String>>();
                queryParams.add(new Pair<String, String>("a", "b"));
                client.invokeApi("apiName", HttpConstants.DeleteMethod, queryParams).get();
            }

        }, "AJ,QS");
    }

    public void testTypedApiFeatureHeader() {
        testInvokeApiFeatureHeader(new ClientTestOperation() {

            @Override
            public void executeOperation(MobileServiceClient client) throws Exception {
                client.invokeApi("apiName", Address.class).get();
            }

        }, "AT");
    }

    public void testTypedApiWithQueryParametersFeatureHeader() {
        testInvokeApiFeatureHeader(new ClientTestOperation() {

            @Override
            public void executeOperation(MobileServiceClient client) throws Exception {
                List<Pair<String, String>> queryParams = new ArrayList<Pair<String, String>>();
                queryParams.add(new Pair<String, String>("a", "b"));
                client.invokeApi("apiName", HttpConstants.GetMethod, queryParams, Address.class).get();
            }

        }, "AT,QS");
    }

    public void testGenericApiFeatureHeader() {
        testInvokeApiFeatureHeader(new ClientTestOperation() {

            @Override
            public void executeOperation(MobileServiceClient client) throws Exception {
                List<Pair<String, String>> queryParams = new ArrayList<Pair<String, String>>();
                queryParams.add(new Pair<String, String>("a", "b"));
                List<Pair<String, String>> requestHeaders = new ArrayList<Pair<String, String>>();
                requestHeaders.add(new Pair<String, String>(HttpConstants.ContentType, "text/plain"));
                byte[] content = "hello world".getBytes();
                client.invokeApi("apiName", content, HttpConstants.PostMethod, requestHeaders, queryParams).get();
            }

        }, "AG");
    }

    public void testGenericApiDoesNotOverrideExistingFeatureHeader() {
        testInvokeApiFeatureHeader(new ClientTestOperation() {

            @Override
            public void executeOperation(MobileServiceClient client) throws Exception {
                List<Pair<String, String>> queryParams = new ArrayList<Pair<String, String>>();
                queryParams.add(new Pair<String, String>("a", "b"));
                List<Pair<String, String>> requestHeaders = new ArrayList<Pair<String, String>>();
                requestHeaders.add(new Pair<String, String>(HttpConstants.ContentType, "text/plain"));
                requestHeaders.add(new Pair<String, String>("X-ZUMO-FEATURES", "something"));
                byte[] content = "hello world".getBytes();
                client.invokeApi("apiName", content, HttpConstants.PostMethod, requestHeaders, queryParams).get();
            }

        }, "something");
    }

    private void testInvokeApiFeatureHeader(ClientTestOperation operation, final String expectedFeaturesHeader) {
        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a new filter to the client
        client = client.withFilter(new ServiceFilter() {

            @Override
            public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

                final SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture.create();
                String featuresHeaderName = "X-ZUMO-FEATURES";

                Headers headers = request.getHeaders();
                String features = null;
                for (int i = 0; i < headers.size(); i++) {
                    if (headers.name(i) == featuresHeaderName) {
                        features = headers.value(i);
                    }
                }

                if (features == null) {
                    resultFuture.setException(new Exception("No " + featuresHeaderName + " header on API call"));
                } else if (!features.equals(expectedFeaturesHeader)) {
                    resultFuture.setException(new Exception("Incorrect features header; expected " + expectedFeaturesHeader + ", actual " + features));
                } else {
                    ServiceFilterResponseMock response = new ServiceFilterResponseMock();
                    response.setContent("{}");
                    resultFuture.set(response);
                }

                return resultFuture;
            }
        });

        try {
            operation.executeOperation(client);
        } catch (Exception exception) {
            Throwable ex = exception;
            while (ex instanceof ExecutionException || ex instanceof MobileServiceException) {
                ex = ex.getCause();
            }
            fail(ex.getMessage());
        }
    }

    public void testTypedSyncTablePullFeatureHeader() {
        // Both typed and untyped sync tables are treated as untyped
        // in the offline implementation
        testSyncTablePullOperationsFeatureHeader(new OfflineTableTestOperation() {

            @Override
            public void executeOperation(MobileServiceClient client, MobileServiceSyncTable<PersonTestObjectWithStringId> typedTable,
                                         MobileServiceJsonSyncTable jsonTable) throws Exception {
                ExecutableQuery<PersonTestObjectWithStringId> query = client.getTable(PersonTestObjectWithStringId.class).where();
                typedTable.pull(query).get();
            }
        }, "OL,TU");
    }

    public void testJsonSyncTablePullFeatureHeader() {
        // Both typed and untyped sync tables are treated as untyped
        // in the offline implementation
        testSyncTablePullOperationsFeatureHeader(new OfflineTableTestOperation() {

            @Override
            public void executeOperation(MobileServiceClient client, MobileServiceSyncTable<PersonTestObjectWithStringId> typedTable,
                                         MobileServiceJsonSyncTable jsonTable) throws Exception {
                ExecutableJsonQuery query = client.getTable("Person").where();
                jsonTable.pull(query).get();
            }
        }, "OL,TU");
    }

    public void testTypedSyncTablePushFeatureHeader() {
        // Both typed and untyped sync tables are treated as untyped
        // in the offline implementation
        testSyncTableOperationsFeatureHeader(new OfflineTableTestOperation() {

            @Override
            public void executeOperation(MobileServiceClient client, MobileServiceSyncTable<PersonTestObjectWithStringId> typedTable,
                                         MobileServiceJsonSyncTable jsonTable) throws Exception {
                PersonTestObjectWithStringId pto = new PersonTestObjectWithStringId("John", "Doe", 33);
                typedTable.insert(pto).get();
                client.getSyncContext().push().get();
            }
        }, "OL,TU");
    }

    public void testJsonSyncTablePushFeatureHeader() {
        // Both typed and untyped sync tables are treated as untyped
        // in the offline implementation
        testSyncTableOperationsFeatureHeader(new OfflineTableTestOperation() {

            @Override
            public void executeOperation(MobileServiceClient client, MobileServiceSyncTable<PersonTestObjectWithStringId> typedTable,
                                         MobileServiceJsonSyncTable jsonTable) throws Exception {
                JsonObject obj = createJsonObject();
                jsonTable.insert(obj).get();
                client.getSyncContext().push().get();
            }
        }, "OL,TU");
    }

    public void testTypedSyncTablePushMultipleItemsFeatureHeader() {
        // Both typed and untyped sync tables are treated as untyped
        // in the offline implementation
        testSyncTableOperationsFeatureHeader(new OfflineTableTestOperation() {

            @Override
            public void executeOperation(MobileServiceClient client, MobileServiceSyncTable<PersonTestObjectWithStringId> typedTable,
                                         MobileServiceJsonSyncTable jsonTable) throws Exception {
                PersonTestObjectWithStringId pto = new PersonTestObjectWithStringId("John", "Doe", 33);
                typedTable.insert(pto).get();
                pto = new PersonTestObjectWithStringId("Jane", "Roe", 34);
                typedTable.insert(pto).get();
                client.getSyncContext().push().get();
            }
        }, "OL,TU");
    }

    public void testJsonSyncTablePushMultipleItemsFeatureHeader() {
        // Both typed and untyped sync tables are treated as untyped
        // in the offline implementation
        testSyncTableOperationsFeatureHeader(new OfflineTableTestOperation() {

            @Override
            public void executeOperation(MobileServiceClient client, MobileServiceSyncTable<PersonTestObjectWithStringId> typedTable,
                                         MobileServiceJsonSyncTable jsonTable) throws Exception {
                JsonObject obj = createJsonObject();
                jsonTable.insert(obj).get();
                obj = createJsonObject();
                obj.remove("id");
                obj.addProperty("id", "another-id");
                jsonTable.insert(obj).get();
                client.getSyncContext().push().get();
            }
        }, "OL,TU");
    }

    private void testSyncTableOperationsFeatureHeader(OfflineTableTestOperation operation, final String expectedFeaturesHeader) {
        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        boolean fistPullPage = false;

        MobileServiceLocalStoreMock store = new MobileServiceLocalStoreMock();
        // Add a new filter to the client
        client = client.withFilter(new ServiceFilter() {

            @Override
            public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

                final SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture.create();
                String featuresHeaderName = "X-ZUMO-FEATURES";

                Headers headers = request.getHeaders();
                String features = null;
                for (int i = 0; i < headers.size(); i++) {
                    if (headers.name(i) == featuresHeaderName) {
                        features = headers.value(i);
                    }
                }

                if (features == null) {
                    resultFuture.setException(new Exception("No " + featuresHeaderName + " header on API call"));
                } else if (!features.equals(expectedFeaturesHeader)) {
                    resultFuture.setException(new Exception("Incorrect features header; expected " + expectedFeaturesHeader + ", actual " + features));
                } else {
                    ServiceFilterResponseMock response = new ServiceFilterResponseMock();
                    Uri requestUri = Uri.parse(request.getUrl());

                    String content = "[]";

                    //if (fistPullPage) {
                        content = "{\"id\":\"the-id\",\"firstName\":\"John\",\"lastName\":\"Doe\",\"age\":33}";

                        if (request.getMethod().equalsIgnoreCase(HttpConstants.GetMethod) && requestUri.getPathSegments().size() == 2) {
                            // GET which should return an array of results
                            content = "[" + content + "]";
                        }

                        //fistPullPage = false;
                    //}

                    response.setContent(content);
                    resultFuture.set(response);
                }

                return resultFuture;
            }
        });

        try {
            Map<String, ColumnDataType> tableDefinition = new HashMap<String, ColumnDataType>();
            tableDefinition.put("id", ColumnDataType.String);
            tableDefinition.put("firstName", ColumnDataType.String);
            tableDefinition.put("lastName", ColumnDataType.String);
            tableDefinition.put("age", ColumnDataType.Integer);
            store.defineTable("Person", tableDefinition);

            client.getSyncContext().initialize(store, new SimpleSyncHandler()).get();

            MobileServiceSyncTable<PersonTestObjectWithStringId> typedTable = client.getSyncTable(PersonTestObjectWithStringId.class);
            MobileServiceJsonSyncTable jsonTable = client.getSyncTable("Person");
            operation.executeOperation(client, typedTable, jsonTable);
        } catch (Exception exception) {
            Throwable ex = exception;
            while (ex instanceof ExecutionException || ex instanceof MobileServiceException) {
                ex = ex.getCause();
            }
            fail(ex.getMessage());
        }
    }

    private void testSyncTablePullOperationsFeatureHeader(OfflineTableTestOperation operation, final String expectedFeaturesHeader) {
        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        MobileServiceLocalStoreMock store = new MobileServiceLocalStoreMock();
        // Add a new filter to the client
        client = client.withFilter(new ServiceFilter() {

            @Override
            public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

                boolean isFirstPage = request.getUrl().contains("$skip=0");

                final SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture.create();
                String featuresHeaderName = "X-ZUMO-FEATURES";

                Headers headers = request.getHeaders();
                String features = null;
                for (int i = 0; i < headers.size(); i++) {
                    if (headers.name(i) == featuresHeaderName) {
                        features = headers.value(i);
                    }
                }

                if (features == null) {
                    resultFuture.setException(new Exception("No " + featuresHeaderName + " header on API call"));
                } else if (!features.equals(expectedFeaturesHeader)) {
                    resultFuture.setException(new Exception("Incorrect features header; expected " + expectedFeaturesHeader + ", actual " + features));
                } else {
                    ServiceFilterResponseMock response = new ServiceFilterResponseMock();
                    Uri requestUri = Uri.parse(request.getUrl());

                    String content = "[]";

                    if (isFirstPage) {
                        content = "{\"id\":\"the-id\",\"firstName\":\"John\",\"lastName\":\"Doe\",\"age\":33}";

                        if (request.getMethod().equalsIgnoreCase(HttpConstants.GetMethod) && requestUri.getPathSegments().size() == 2) {
                            // GET which should return an array of results
                            content = "[" + content + "]";
                        }

                    }

                    response.setContent(content);
                    resultFuture.set(response);
                }

                return resultFuture;
            }
        });

        try {
            Map<String, ColumnDataType> tableDefinition = new HashMap<String, ColumnDataType>();
            tableDefinition.put("id", ColumnDataType.String);
            tableDefinition.put("firstName", ColumnDataType.String);
            tableDefinition.put("lastName", ColumnDataType.String);
            tableDefinition.put("age", ColumnDataType.Integer);
            store.defineTable("Person", tableDefinition);

            client.getSyncContext().initialize(store, new SimpleSyncHandler()).get();

            MobileServiceSyncTable<PersonTestObjectWithStringId> typedTable = client.getSyncTable(PersonTestObjectWithStringId.class);
            MobileServiceJsonSyncTable jsonTable = client.getSyncTable("Person");
            operation.executeOperation(client, typedTable, jsonTable);
        } catch (Exception exception) {
            Throwable ex = exception;
            while (ex instanceof ExecutionException || ex instanceof MobileServiceException) {
                ex = ex.getCause();
            }
            fail(ex.getMessage());
        }
    }

    public void testJsonTableUpdateWithVersionFeatureHeader() {
        testOpportunisticConcurrencyOperationsFeatureHeader(new OpportunisticConcurrencyTestOperation() {

            @Override
            public void executeOperation(MobileServiceClient client) throws Exception {
                MobileServiceJsonTable jsonTable = client.getTable("Person");
                jsonTable.setSystemProperties(EnumSet.of(MobileServiceSystemProperty.Version));
                JsonObject jo = createJsonObject();
                jo.addProperty("version", "abc");
                List<Pair<String, String>> queryParams = new ArrayList<Pair<String, String>>();
                queryParams.add(new Pair<String, String>("a", "b"));
                jsonTable.update(jo, queryParams).get();
            }
        }, "OC,QS,TU");
    }

    public void testTypedTableUpdateWithVersionFeatureHeader() {
        testOpportunisticConcurrencyOperationsFeatureHeader(new OpportunisticConcurrencyTestOperation() {

            @Override
            public void executeOperation(MobileServiceClient client) throws Exception {
                MobileServiceTable<VersionType> table = client.getTable(VersionType.class);
                VersionType obj = new VersionType();
                obj.Version = "abc";
                obj.Id = "the-id";
                table.update(obj).get();
            }
        }, "OC,TT");
    }

    public void testOfflinePushWithVersionFeatureHeader() {
        testOpportunisticConcurrencyOperationsFeatureHeader(new OpportunisticConcurrencyTestOperation() {

            @Override
            public void executeOperation(MobileServiceClient client) throws Exception {
                MobileServiceSyncTable<VersionType> table = client.getSyncTable(VersionType.class);
                VersionType obj = new VersionType();
                obj.Version = "abc";
                obj.Id = "the-id";
                table.update(obj).get();
                client.getSyncContext().push().get();
            }
        }, "OC,OL,TU");
    }

    private void testOpportunisticConcurrencyOperationsFeatureHeader(OpportunisticConcurrencyTestOperation operation, final String expectedFeaturesHeader) {
        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        MobileServiceLocalStoreMock store = new MobileServiceLocalStoreMock();
        // Add a new filter to the client
        client = client.withFilter(new ServiceFilter() {

            @Override
            public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

                final SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture.create();
                String featuresHeaderName = "X-ZUMO-FEATURES";

                Headers headers = request.getHeaders();
                String features = null;
                for (int i = 0; i < headers.size(); i++) {
                    if (headers.name(i) == featuresHeaderName) {
                        features = headers.value(i);
                    }
                }

                if (features == null) {
                    resultFuture.setException(new Exception("No " + featuresHeaderName + " header on API call"));
                } else if (!features.equals(expectedFeaturesHeader)) {
                    resultFuture.setException(new Exception("Incorrect features header; expected " + expectedFeaturesHeader + ", actual " + features));
                } else {
                    ServiceFilterResponseMock response = new ServiceFilterResponseMock();
                    Uri requestUri = Uri.parse(request.getUrl());
                    String content = "{\"id\":\"the-id\",\"firstName\":\"John\",\"lastName\":\"Doe\",\"age\":33}";
                    if (request.getMethod().equalsIgnoreCase(HttpConstants.GetMethod) && requestUri.getPathSegments().size() == 2) {
                        // GET which should return an array of results
                        content = "[" + content + "]";
                    }
                    response.setContent(content);
                    resultFuture.set(response);
                }

                return resultFuture;
            }
        });

        try {
            Map<String, ColumnDataType> tableDefinition = new HashMap<String, ColumnDataType>();
            tableDefinition.put("id", ColumnDataType.String);
            tableDefinition.put("firstName", ColumnDataType.String);
            tableDefinition.put("lastName", ColumnDataType.String);
            tableDefinition.put("age", ColumnDataType.Integer);
            store.defineTable("Person", tableDefinition);

            client.getSyncContext().initialize(store, new SimpleSyncHandler()).get();

            operation.executeOperation(client);
        } catch (Exception exception) {
            Throwable ex = exception;
            while (ex instanceof ExecutionException || ex instanceof MobileServiceException) {
                ex = ex.getCause();
            }
            fail(ex.getMessage());
        }
    }

    // Tests for tables
    interface TableTestOperation {
        void executeOperation(MobileServiceTable<PersonTestObjectWithStringId> typedTable, MobileServiceJsonTable jsonTable) throws Exception;
    }

    // Tests for custom APIs
    interface ClientTestOperation {
        void executeOperation(MobileServiceClient client) throws Exception;
    }

    // Tests for offline push / pull
    interface OfflineTableTestOperation {
        void executeOperation(MobileServiceClient client, MobileServiceSyncTable<PersonTestObjectWithStringId> typedTable, MobileServiceJsonSyncTable jsonTable)
                throws Exception;
    }

    // Tests for opportunistic concurrency (conditional updates)
    interface OpportunisticConcurrencyTestOperation {
        void executeOperation(MobileServiceClient client) throws Exception;
    }
}
