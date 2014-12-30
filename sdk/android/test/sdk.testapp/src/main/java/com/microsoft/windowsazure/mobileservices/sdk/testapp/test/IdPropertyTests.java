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

import android.test.InstrumentationTestCase;

import com.google.common.util.concurrent.ListenableFuture;
import com.google.gson.GsonBuilder;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import com.google.gson.JsonSyntaxException;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.http.NextServiceFilterCallback;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilter;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterRequest;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.framework.filters.ServiceFilterRequestMock;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.framework.filters.ServiceFilterResponseMock;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.framework.filters.StatusLineMock;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.test.types.IdPropertyTestClasses.LongIdType;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.test.types.IdPropertyTestClasses.StringIdType;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.test.types.ResultsContainer;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.test.types.data.IdTestData;
import com.microsoft.windowsazure.mobileservices.table.MobileServiceJsonTable;
import com.microsoft.windowsazure.mobileservices.table.MobileServiceTable;

import java.net.MalformedURLException;
import java.util.List;
import java.util.Locale;
import java.util.concurrent.ExecutionException;

import static com.microsoft.windowsazure.mobileservices.table.query.QueryOperations.field;

public class IdPropertyTests extends InstrumentationTestCase {
    String appUrl = "";
    String appKey = "";
    GsonBuilder gsonBuilder;

    protected void setUp() throws Exception {
        appUrl = "http://myapp.com/";
        appKey = "qwerty";
        gsonBuilder = new GsonBuilder();
        super.setUp();
    }

    protected void tearDown() throws Exception {
        super.tearDown();
    }

    // Non Generic JSON

    // Read Tests

    public void testReadWithStringIdResponseContent() throws Throwable {
        String[] testIdData = IdTestData.concat(IdTestData.concat(IdTestData.ValidStringIds, IdTestData.EmptyStringIds), IdTestData.InvalidStringIds);

        for (String testId : testIdData) {
            readWithStringIdResponseContent(testId);
        }
    }

    private void readWithStringIdResponseContent(String testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        String jsonTestId = testId.replace("\\", "\\\\").replace("\"", "\\\"");
        final String responseContent = "[{\"id\":\"" + jsonTestId + "\",\"String\":\"Hey\"}]";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        try {
            // Call the select method
            JsonElement result = msTable.execute().get();
            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setJsonResult(result);
            }
        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }

        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            assertTrue(container.getJsonResult().isJsonArray());
            assertTrue(container.getJsonResult().getAsJsonArray().size() == 1);
            assertTrue(container.getJsonResult().getAsJsonArray().get(0).isJsonObject());
            assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().has("id"));
            assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().has("String"));
            assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("id").isJsonPrimitive());
            assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("String").isJsonPrimitive());
            assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("id").getAsJsonPrimitive().isString());
            assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("String").getAsJsonPrimitive().isString());
            assertEquals(testId, container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("id").getAsJsonPrimitive().getAsString());
            assertEquals("Hey", container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("String").getAsJsonPrimitive().getAsString());
        }
    }

    public void testReadWithIntIdResponseContent() throws Throwable {
        long[] testIdData = IdTestData.concat(IdTestData.ValidIntIds, IdTestData.InvalidIntIds);

        for (long testId : testIdData) {
            readWithIntIdResponseContent(testId);
        }
    }

    private void readWithIntIdResponseContent(long testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        String stringTestId = String.valueOf(testId);

        final String responseContent = "[{\"id\":" + stringTestId + ",\"String\":\"Hey\"}]";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        try {
            // Call the select method
            JsonElement result = msTable.execute().get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setJsonResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            assertTrue(container.getJsonResult().isJsonArray());
            assertTrue(container.getJsonResult().getAsJsonArray().size() == 1);
            assertTrue(container.getJsonResult().getAsJsonArray().get(0).isJsonObject());
            assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().has("id"));
            assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().has("String"));
            assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("id").isJsonPrimitive());
            assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("String").isJsonPrimitive());
            assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("id").getAsJsonPrimitive().isNumber());
            assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("String").getAsJsonPrimitive().isString());
            assertEquals(testId, container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("id").getAsJsonPrimitive().getAsLong());
            assertEquals("Hey", container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("String").getAsJsonPrimitive().getAsString());
        }
    }

    public void testReadWithNonStringAndNonIntIdResponseContent() throws Throwable {
        Object[] testIdData = IdTestData.NonStringNonIntValidJsonIds;

        for (Object testId : testIdData) {
            readWithNonStringAndNonIntIdResponseContent(testId);
        }
    }

    private void readWithNonStringAndNonIntIdResponseContent(Object testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        String stringTestId = testId.toString().toLowerCase(Locale.getDefault());

        final String responseContent = "[{\"id\":" + stringTestId + ",\"String\":\"Hey\"}]";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        try {
            // Call the select method
            JsonElement result = msTable.execute().get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setJsonResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            assertTrue(container.getJsonResult().isJsonArray());

            assertTrue(container.getJsonResult().getAsJsonArray().size() == 1);

            assertTrue(container.getJsonResult().getAsJsonArray().get(0).isJsonObject());

            assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().has("id"));
            assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().has("String"));

            assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("String").isJsonPrimitive());

            assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("String").getAsJsonPrimitive().isString());

            if (container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("id").isJsonNull()) {
                assertTrue(testId == null);
            } else if (container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("id").isJsonPrimitive()) {
                if (container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("id").getAsJsonPrimitive().isBoolean()) {
                    assertEquals(testId, container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("id").getAsJsonPrimitive().getAsBoolean());
                } else if (container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("id").getAsJsonPrimitive().isNumber()) {
                    assertEquals(testId, container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("id").getAsJsonPrimitive().getAsDouble());
                } else if (container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("id").getAsJsonPrimitive().isString()) {
                    assertEquals(testId, container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("id").getAsJsonPrimitive().getAsString());
                }
            } else if (container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("id").isJsonObject()) {
                assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("id").getAsJsonObject().equals(testId));
            } else if (container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("id").isJsonArray()) {
                assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("id").getAsJsonArray().equals(testId));
            }

            assertEquals("Hey", container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("String").getAsJsonPrimitive().getAsString());
        }
    }

    public void testReadWithNullIdResponseContent() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "[{\"id\":null,\"String\":\"Hey\"}]";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        try {
            // Call the select method
            JsonElement result = msTable.execute().get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setJsonResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            assertTrue(container.getJsonResult().isJsonArray());

            assertTrue(container.getJsonResult().getAsJsonArray().size() == 1);

            assertTrue(container.getJsonResult().getAsJsonArray().get(0).isJsonObject());

            assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().has("id"));
            assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().has("String"));

            assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("id").isJsonNull());
            assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("String").isJsonPrimitive());

            assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("String").getAsJsonPrimitive().isString());

            assertEquals("Hey", container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("String").getAsJsonPrimitive().getAsString());
        }
    }

    public void testReadWithNoIdResponseContent() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "[{\"String\":\"Hey\"}]";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        try {
            // Call the select method
            JsonElement result = msTable.execute().get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setJsonResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            assertTrue(container.getJsonResult().isJsonArray());

            assertTrue(container.getJsonResult().getAsJsonArray().size() == 1);

            assertTrue(container.getJsonResult().getAsJsonArray().get(0).isJsonObject());

            assertTrue(!container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().has("id"));
            assertTrue(!container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().has("Id"));
            assertTrue(!container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().has("iD"));
            assertTrue(!container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().has("ID"));

            assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().has("String"));

            assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("String").isJsonPrimitive());

            assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("String").getAsJsonPrimitive().isString());

            assertEquals("Hey", container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("String").getAsJsonPrimitive().getAsString());
        }
    }

    // Lookup Tests

    public void testLookupWithStringIdResponseContent() throws Throwable {
        String[] testIdData = IdTestData.concat(IdTestData.concat(IdTestData.ValidStringIds, IdTestData.EmptyStringIds), IdTestData.InvalidStringIds);

        for (String testId : testIdData) {
            lookupWithStringIdResponseContent(testId);
        }
    }

    private void lookupWithStringIdResponseContent(final String testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        String jsonTestId = testId.replace("\\", "\\\\").replace("\"", "\\\"");
        final String responseContent = "{\"id\":\"" + jsonTestId + "\",\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        try {
            // Call the select method
            JsonElement result = msTable.lookUp("myId").get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setJsonResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            assertTrue(container.getJsonResult().isJsonObject());
            assertTrue(container.getJsonResult().getAsJsonObject().has("id"));
            assertTrue(container.getJsonResult().getAsJsonObject().has("String"));
            assertTrue(container.getJsonResult().getAsJsonObject().get("id").isJsonPrimitive());
            assertTrue(container.getJsonResult().getAsJsonObject().get("String").isJsonPrimitive());
            assertTrue(container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().isString());
            assertTrue(container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().isString());
            assertEquals(testId, container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().getAsString());
            assertEquals("Hey", container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().getAsString());
        }
    }

    public void testLookupWithIntIdResponseContent() throws Throwable {
        long[] testIdData = IdTestData.concat(IdTestData.ValidIntIds, IdTestData.InvalidIntIds);

        for (long testId : testIdData) {
            lookupWithIntIdResponseContent(testId);
        }
    }

    private void lookupWithIntIdResponseContent(long testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        String stringTestId = String.valueOf(testId);

        final String responseContent = "{\"id\":" + stringTestId + ",\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        try {
            // Call the select method
            JsonElement result = msTable.lookUp("myId").get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setJsonResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }
        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            assertTrue(container.getJsonResult().isJsonObject());
            assertTrue(container.getJsonResult().getAsJsonObject().has("id"));
            assertTrue(container.getJsonResult().getAsJsonObject().has("String"));
            assertTrue(container.getJsonResult().getAsJsonObject().get("id").isJsonPrimitive());
            assertTrue(container.getJsonResult().getAsJsonObject().get("String").isJsonPrimitive());
            assertTrue(container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().isNumber());
            assertTrue(container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().isString());
            assertEquals(testId, container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().getAsLong());
            assertEquals("Hey", container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().getAsString());
        }
    }

    public void testLookupWithNonStringAndNonIntIdResponseContent() throws Throwable {
        Object[] testIdData = IdTestData.NonStringNonIntValidJsonIds;

        for (Object testId : testIdData) {
            lookupWithNonStringAndNonIntIdResponseContent(testId);
        }
    }

    private void lookupWithNonStringAndNonIntIdResponseContent(Object testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        String stringTestId = testId.toString().toLowerCase(Locale.getDefault());

        final String responseContent = "{\"id\":" + stringTestId + ",\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        try {
            // Call the select method
            JsonElement result = msTable.lookUp("myId").get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setJsonResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            assertTrue(container.getJsonResult().isJsonObject());

            assertTrue(container.getJsonResult().getAsJsonObject().has("id"));
            assertTrue(container.getJsonResult().getAsJsonObject().has("String"));

            assertTrue(container.getJsonResult().getAsJsonObject().get("String").isJsonPrimitive());

            assertTrue(container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().isString());

            if (container.getJsonResult().getAsJsonObject().get("id").isJsonNull()) {
                assertTrue(testId == null);
            } else if (container.getJsonResult().getAsJsonObject().get("id").isJsonPrimitive()) {
                if (container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().isBoolean()) {
                    assertEquals(testId, container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().getAsBoolean());
                } else if (container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().isNumber()) {
                    assertEquals(testId, container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().getAsDouble());
                } else if (container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().isString()) {
                    assertEquals(testId, container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().getAsString());
                }
            } else if (container.getJsonResult().getAsJsonObject().get("id").isJsonObject()) {
                assertTrue(container.getJsonResult().getAsJsonObject().get("id").getAsJsonObject().equals(testId));
            } else if (container.getJsonResult().getAsJsonObject().get("id").isJsonArray()) {
                assertTrue(container.getJsonResult().getAsJsonObject().get("id").getAsJsonArray().equals(testId));
            }

            assertEquals("Hey", container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().getAsString());
        }
    }

    public void testLookupWithNullIdResponseContent() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"id\":null,\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        try {
            // Call the select method
            JsonElement result = msTable.lookUp("myId").get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setJsonResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            assertTrue(container.getJsonResult().isJsonObject());

            assertTrue(container.getJsonResult().getAsJsonObject().has("id"));
            assertTrue(container.getJsonResult().getAsJsonObject().has("String"));

            assertTrue(container.getJsonResult().getAsJsonObject().get("id").isJsonNull());
            assertTrue(container.getJsonResult().getAsJsonObject().get("String").isJsonPrimitive());

            assertTrue(container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().isString());

            assertEquals("Hey", container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().getAsString());
        }
    }

    public void testLookupWithNoIdResponseContent() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        try {
            // Call the select method
            JsonElement result = msTable.lookUp("myId").get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setJsonResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            assertTrue(container.getJsonResult().isJsonObject());

            assertTrue(!container.getJsonResult().getAsJsonObject().has("id"));
            assertTrue(!container.getJsonResult().getAsJsonObject().has("Id"));
            assertTrue(!container.getJsonResult().getAsJsonObject().has("iD"));
            assertTrue(!container.getJsonResult().getAsJsonObject().has("ID"));

            assertTrue(container.getJsonResult().getAsJsonObject().has("String"));

            assertTrue(container.getJsonResult().getAsJsonObject().get("String").isJsonPrimitive());

            assertTrue(container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().isString());

            assertEquals("Hey", container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().getAsString());
        }
    }

    public void testLookupWithStringIdParameter() throws Throwable {
        String[] testIdData = IdTestData.ValidStringIds;

        for (String testId : testIdData) {
            lookupWithStringIdParameter(testId);
        }
    }

    private void lookupWithStringIdParameter(final String testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        String jsonTestId = testId.replace("\\", "\\\\").replace("\"", "\\\"");
        final String responseContent = "{\"id\":\"" + jsonTestId + "\",\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        try {
            // Call the select method
            JsonElement result = msTable.lookUp(testId).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setJsonResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            assertTrue(container.getJsonResult().isJsonObject());
            assertTrue(container.getJsonResult().getAsJsonObject().has("id"));
            assertTrue(container.getJsonResult().getAsJsonObject().has("String"));
            assertTrue(container.getJsonResult().getAsJsonObject().get("id").isJsonPrimitive());
            assertTrue(container.getJsonResult().getAsJsonObject().get("String").isJsonPrimitive());
            assertTrue(container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().isString());
            assertTrue(container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().isString());
            assertEquals(testId, container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().getAsString());
            assertEquals("Hey", container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().getAsString());
        }
    }

    public void testLookupWithInvalidStringIdParameter() throws Throwable {
        String[] testIdData = IdTestData.concat(IdTestData.EmptyStringIds, IdTestData.InvalidStringIds);

        for (String testId : testIdData) {
            lookupWithInvalidStringIdParameter(testId);
        }
    }

    private void lookupWithInvalidStringIdParameter(final String testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"id\":\"an id\",\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        try {
            // Call the select method
            JsonElement result = msTable.lookUp(testId).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setJsonResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception == null || !(exception instanceof IllegalArgumentException)) {
            fail("Expected Exception IllegalArgumentException");
        }
    }

    public void testLookupWithIntIdParameter() throws Throwable {
        long[] testIdData = IdTestData.ValidIntIds;

        for (long testId : testIdData) {
            lookupWithIntIdParameter(testId);
        }
    }

    private void lookupWithIntIdParameter(final long testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        String stringTestId = String.valueOf(testId);

        final String responseContent = "{\"id\":" + stringTestId + ",\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        try {
            // Call the select method
            JsonElement result = msTable.lookUp(testId).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setJsonResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            assertTrue(container.getJsonResult().isJsonObject());
            assertTrue(container.getJsonResult().getAsJsonObject().has("id"));
            assertTrue(container.getJsonResult().getAsJsonObject().has("String"));
            assertTrue(container.getJsonResult().getAsJsonObject().get("id").isJsonPrimitive());
            assertTrue(container.getJsonResult().getAsJsonObject().get("String").isJsonPrimitive());
            assertTrue(container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().isNumber());
            assertTrue(container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().isString());
            assertEquals(testId, container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().getAsLong());
            assertEquals("Hey", container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().getAsString());
        }
    }

    public void testLookupWithInvalidIntIdParameter() throws Throwable {
        long[] testIdData = IdTestData.InvalidIntIds;

        for (long testId : testIdData) {
            lookupWithInvalidIntIdParameter(testId);
        }
    }

    private void lookupWithInvalidIntIdParameter(final long testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"id\":5,\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        try {
            // Call the select method
            JsonElement result = msTable.lookUp(testId).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setJsonResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception == null || !(exception instanceof IllegalArgumentException)) {
            fail("Expected Exception IllegalArgumentException");
        }
    }

    public void testLookupWithNullIdParameter() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"id\":\"an id\",\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        try {
            // Call the select method
            JsonElement result = msTable.lookUp(null).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setJsonResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                if (exception instanceof ExecutionException) {
                    container.setException(exception.getCause());
                } else {
                    container.setException(exception);
                }
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception == null || !(exception instanceof IllegalArgumentException)) {
            fail("Expected Exception IllegalArgumentException");
        }
    }

    public void testLookupWithZeroIdParameter() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"id\":5,\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        try {
            // Call the select method
            JsonElement result = msTable.lookUp(0L).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setJsonResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception == null || !(exception instanceof IllegalArgumentException)) {
            fail("Expected Exception IllegalArgumentException");
        }
    }

    // Insert Tests

    public void testInsertWithStringIdResponseContent() throws Throwable {
        String[] testIdData = IdTestData.concat(IdTestData.concat(IdTestData.ValidStringIds, IdTestData.EmptyStringIds), IdTestData.InvalidStringIds);

        for (String testId : testIdData) {
            insertWithStringIdResponseContent(testId);
        }
    }

    private void insertWithStringIdResponseContent(final String testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        String jsonTestId = testId.replace("\\", "\\\\").replace("\"", "\\\"");
        final String responseContent = "{\"id\":\"" + jsonTestId + "\",\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        JsonObject obj = new JsonParser().parse("{\"value\":\"new\"}").getAsJsonObject();

        try {
            // Call the insert method
            JsonElement result = msTable.insert(obj).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setJsonResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            assertTrue(container.getJsonResult().isJsonObject());
            assertTrue(container.getJsonResult().getAsJsonObject().has("id"));
            assertTrue(container.getJsonResult().getAsJsonObject().has("String"));
            assertTrue(container.getJsonResult().getAsJsonObject().get("id").isJsonPrimitive());
            assertTrue(container.getJsonResult().getAsJsonObject().get("String").isJsonPrimitive());
            assertTrue(container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().isString());
            assertTrue(container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().isString());
            assertEquals(testId, container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().getAsString());
            assertEquals("Hey", container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().getAsString());
        }
    }

    public void testInsertWithIntIdResponseContent() throws Throwable {
        long[] testIdData = IdTestData.concat(IdTestData.ValidIntIds, IdTestData.InvalidIntIds);

        for (long testId : testIdData) {
            insertWithIntIdResponseContent(testId);
        }
    }

    private void insertWithIntIdResponseContent(long testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        String stringTestId = String.valueOf(testId);

        final String responseContent = "{\"id\":" + stringTestId + ",\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        JsonObject obj = new JsonParser().parse("{\"value\":\"new\"}").getAsJsonObject();

        try {
            // Call the insert method
            JsonElement result = msTable.insert(obj).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setJsonResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            assertTrue(container.getJsonResult().isJsonObject());
            assertTrue(container.getJsonResult().getAsJsonObject().has("id"));
            assertTrue(container.getJsonResult().getAsJsonObject().has("String"));
            assertTrue(container.getJsonResult().getAsJsonObject().get("id").isJsonPrimitive());
            assertTrue(container.getJsonResult().getAsJsonObject().get("String").isJsonPrimitive());
            assertTrue(container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().isNumber());
            assertTrue(container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().isString());
            assertEquals(testId, container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().getAsLong());
            assertEquals("Hey", container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().getAsString());
        }
    }

    public void testInsertWithNonStringAndNonIntIdResponseContent() throws Throwable {
        Object[] testIdData = IdTestData.NonStringNonIntValidJsonIds;

        for (Object testId : testIdData) {
            insertWithNonStringAndNonIntIdResponseContent(testId);
        }
    }

    private void insertWithNonStringAndNonIntIdResponseContent(Object testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        String stringTestId = testId.toString().toLowerCase(Locale.getDefault());

        final String responseContent = "{\"id\":" + stringTestId + ",\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        JsonObject obj = new JsonParser().parse("{\"value\":\"new\"}").getAsJsonObject();

        try {
            // Call the insert method
            JsonElement result = msTable.insert(obj).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setJsonResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            assertTrue(container.getJsonResult().isJsonObject());

            assertTrue(container.getJsonResult().getAsJsonObject().has("id"));
            assertTrue(container.getJsonResult().getAsJsonObject().has("String"));

            assertTrue(container.getJsonResult().getAsJsonObject().get("String").isJsonPrimitive());

            assertTrue(container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().isString());

            if (container.getJsonResult().getAsJsonObject().get("id").isJsonNull()) {
                assertTrue(testId == null);
            } else if (container.getJsonResult().getAsJsonObject().get("id").isJsonPrimitive()) {
                if (container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().isBoolean()) {
                    assertEquals(testId, container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().getAsBoolean());
                } else if (container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().isNumber()) {
                    assertEquals(testId, container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().getAsDouble());
                } else if (container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().isString()) {
                    assertEquals(testId, container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().getAsString());
                }
            } else if (container.getJsonResult().getAsJsonObject().get("id").isJsonObject()) {
                assertTrue(container.getJsonResult().getAsJsonObject().get("id").getAsJsonObject().equals(testId));
            } else if (container.getJsonResult().getAsJsonObject().get("id").isJsonArray()) {
                assertTrue(container.getJsonResult().getAsJsonObject().get("id").getAsJsonArray().equals(testId));
            }

            assertEquals("Hey", container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().getAsString());
        }
    }

    public void testInsertWithNullIdResponseContent() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"id\":null,\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        JsonObject obj = new JsonParser().parse("{\"value\":\"new\"}").getAsJsonObject();

        try {
            // Call the insert method
            JsonElement result = msTable.insert(obj).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setJsonResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            assertTrue(container.getJsonResult().isJsonObject());

            assertTrue(container.getJsonResult().getAsJsonObject().has("id"));
            assertTrue(container.getJsonResult().getAsJsonObject().has("String"));

            assertTrue(container.getJsonResult().getAsJsonObject().get("id").isJsonNull());
            assertTrue(container.getJsonResult().getAsJsonObject().get("String").isJsonPrimitive());

            assertTrue(container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().isString());

            assertEquals("Hey", container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().getAsString());
        }
    }

    public void testInsertWithNoIdResponseContent() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        JsonObject obj = new JsonParser().parse("{\"value\":\"new\"}").getAsJsonObject();

        try {
            // Call the insert method
            JsonElement result = msTable.insert(obj).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setJsonResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            assertTrue(container.getJsonResult().isJsonObject());

            assertTrue(!container.getJsonResult().getAsJsonObject().has("id"));
            assertTrue(!container.getJsonResult().getAsJsonObject().has("Id"));
            assertTrue(!container.getJsonResult().getAsJsonObject().has("iD"));
            assertTrue(!container.getJsonResult().getAsJsonObject().has("ID"));

            assertTrue(container.getJsonResult().getAsJsonObject().has("String"));

            assertTrue(container.getJsonResult().getAsJsonObject().get("String").isJsonPrimitive());

            assertTrue(container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().isString());

            assertEquals("Hey", container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().getAsString());
        }
    }

    public void testInsertWithStringIdItem() throws Throwable {
        String[] testIdData = IdTestData.ValidStringIds;

        for (String testId : testIdData) {
            insertWithStringIdItem(testId);
        }
    }

    private void insertWithStringIdItem(final String testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String jsonTestId = testId.replace("\\", "\\\\").replace("\"", "\\\"");
        final String responseContent = "{\"id\":\"" + jsonTestId + "\",\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        JsonObject obj = new JsonParser().parse("{\"id\":\"" + jsonTestId + "\",\"String\":\"what?\"}").getAsJsonObject();

        try {
            // Call the insert method
            JsonElement result = msTable.insert(obj).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setJsonResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            assertTrue(container.getJsonResult().isJsonObject());
            assertTrue(container.getJsonResult().getAsJsonObject().has("id"));
            assertTrue(container.getJsonResult().getAsJsonObject().has("String"));
            assertTrue(container.getJsonResult().getAsJsonObject().get("id").isJsonPrimitive());
            assertTrue(container.getJsonResult().getAsJsonObject().get("String").isJsonPrimitive());
            assertTrue(container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().isString());
            assertTrue(container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().isString());
            assertEquals(testId, container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().getAsString());
            assertEquals("Hey", container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().getAsString());
        }
    }

    public void testInsertWithEmptyStringIdItem() throws Throwable {
        String[] testIdData = IdTestData.EmptyStringIds;

        for (String testId : testIdData) {
            insertWithEmptyStringIdItem(testId);
        }
    }

    private void insertWithEmptyStringIdItem(final String testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"id\":\"an id\",\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        JsonObject obj = new JsonParser().parse("{\"id\":\"" + testId + "\",\"String\":\"what?\"}").getAsJsonObject();

        try {
            // Call the insert method
            JsonElement result = msTable.insert(obj).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setJsonResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            assertTrue(container.getJsonResult().isJsonObject());
            assertTrue(container.getJsonResult().getAsJsonObject().has("id"));
            assertTrue(container.getJsonResult().getAsJsonObject().has("String"));
            assertTrue(container.getJsonResult().getAsJsonObject().get("id").isJsonPrimitive());
            assertTrue(container.getJsonResult().getAsJsonObject().get("String").isJsonPrimitive());
            assertTrue(container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().isString());
            assertTrue(container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().isString());
            assertEquals("an id", container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().getAsString());
            assertEquals("Hey", container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().getAsString());
        }
    }

    public void testInsertWithInvalidStringIdItem() throws Throwable {
        String[] testIdData = IdTestData.InvalidStringIds;

        for (String testId : testIdData) {
            insertWithInvalidStringIdItem(testId);
        }
    }

    private void insertWithInvalidStringIdItem(final String testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String jsonTestId = testId.replace("\\", "\\\\").replace("\"", "\\\"");

        final String responseContent = "{\"id\":\"an id\",\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        JsonObject obj = new JsonParser().parse("{\"id\":\"" + jsonTestId + "\",\"String\":\"what?\"}").getAsJsonObject();

        try {
            // Call the insert method
            JsonElement result = msTable.insert(obj).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setJsonResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception == null || !(exception instanceof IllegalArgumentException)) {
            fail("Expected Exception IllegalArgumentException");
        }
    }

    public void testInsertWithIntIdItem() throws Throwable {
        long[] testIdData = IdTestData.concat(IdTestData.ValidIntIds, IdTestData.InvalidIntIds);

        for (long testId : testIdData) {
            insertWithIntIdItem(testId);
        }
    }

    private void insertWithIntIdItem(final long testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String stringTestId = String.valueOf(testId);

        final String responseContent = "{\"id\":5,\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        JsonObject obj = new JsonParser().parse("{\"id\":" + stringTestId + ",\"String\":\"what?\"}").getAsJsonObject();

        try {
            // Call the insert method
            JsonElement result = msTable.insert(obj).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setJsonResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception == null || !(exception instanceof IllegalArgumentException)) {
            fail("Expected Exception IllegalArgumentException");
        }
    }

    public void testInsertWithNullIdItem() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"id\":5,\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        JsonObject obj = new JsonParser().parse("{\"id\":null,\"String\":\"what?\"}").getAsJsonObject();

        try {
            // Call the insert method
            JsonElement result = msTable.insert(obj).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setJsonResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            assertTrue(container.getJsonResult().isJsonObject());
            assertTrue(container.getJsonResult().getAsJsonObject().has("id"));
            assertTrue(container.getJsonResult().getAsJsonObject().has("String"));
            assertTrue(container.getJsonResult().getAsJsonObject().get("id").isJsonPrimitive());
            assertTrue(container.getJsonResult().getAsJsonObject().get("String").isJsonPrimitive());
            assertTrue(container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().isNumber());
            assertTrue(container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().isString());
            assertEquals(5L, container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().getAsLong());
            assertEquals("Hey", container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().getAsString());
        }
    }

    public void testInsertWithNoIdItem() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"id\":5,\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        JsonObject obj = new JsonParser().parse("{\"String\":\"what?\"}").getAsJsonObject();

        try {
            // Call the insert method
            JsonElement result = msTable.insert(obj).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setJsonResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            assertTrue(container.getJsonResult().isJsonObject());
            assertTrue(container.getJsonResult().getAsJsonObject().has("id"));
            assertTrue(container.getJsonResult().getAsJsonObject().has("String"));
            assertTrue(container.getJsonResult().getAsJsonObject().get("id").isJsonPrimitive());
            assertTrue(container.getJsonResult().getAsJsonObject().get("String").isJsonPrimitive());
            assertTrue(container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().isNumber());
            assertTrue(container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().isString());
            assertEquals(5L, container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().getAsLong());
            assertEquals("Hey", container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().getAsString());
        }
    }

    public void testInsertWithZeroIdItem() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"id\":5,\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        JsonObject obj = new JsonParser().parse("{\"id\":0,\"String\":\"what?\"}").getAsJsonObject();

        try {
            // Call the insert method
            JsonElement result = msTable.insert(obj).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setJsonResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            assertTrue(container.getJsonResult().isJsonObject());
            assertTrue(container.getJsonResult().getAsJsonObject().has("id"));
            assertTrue(container.getJsonResult().getAsJsonObject().has("String"));
            assertTrue(container.getJsonResult().getAsJsonObject().get("id").isJsonPrimitive());
            assertTrue(container.getJsonResult().getAsJsonObject().get("String").isJsonPrimitive());
            assertTrue(container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().isNumber());
            assertTrue(container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().isString());
            assertEquals(5L, container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().getAsLong());
            assertEquals("Hey", container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().getAsString());
        }
    }

    // Update Tests

    public void testUpdateWithStringIdResponseContent() throws Throwable {
        String[] testIdData = IdTestData.concat(IdTestData.concat(IdTestData.ValidStringIds, IdTestData.EmptyStringIds), IdTestData.InvalidStringIds);

        for (String testId : testIdData) {
            updateWithStringIdResponseContent(testId);
        }
    }

    private void updateWithStringIdResponseContent(final String testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        String jsonTestId = testId.replace("\\", "\\\\").replace("\"", "\\\"");
        final String responseContent = "{\"id\":\"" + jsonTestId + "\",\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        JsonObject obj = new JsonParser().parse("{\"id\":\"id\",\"value\":\"new\"}").getAsJsonObject();

        try {
            // Call the insert method
            JsonElement result = msTable.insert(obj).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setJsonResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            assertTrue(container.getJsonResult().isJsonObject());
            assertTrue(container.getJsonResult().getAsJsonObject().has("id"));
            assertTrue(container.getJsonResult().getAsJsonObject().has("String"));
            assertTrue(container.getJsonResult().getAsJsonObject().get("id").isJsonPrimitive());
            assertTrue(container.getJsonResult().getAsJsonObject().get("String").isJsonPrimitive());
            assertTrue(container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().isString());
            assertTrue(container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().isString());
            assertEquals(testId, container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().getAsString());
            assertEquals("Hey", container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().getAsString());
        }
    }

    public void testUpdateWithIntIdResponseContent() throws Throwable {
        long[] testIdData = IdTestData.concat(IdTestData.ValidIntIds, IdTestData.InvalidIntIds);

        for (long testId : testIdData) {
            updateWithIntIdResponseContent(testId);
        }
    }

    private void updateWithIntIdResponseContent(long testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        String stringTestId = String.valueOf(testId);

        final String responseContent = "{\"id\":" + stringTestId + ",\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        JsonObject obj = new JsonParser().parse("{\"id\":\"id\",\"value\":\"new\"}").getAsJsonObject();

        try {
            // Call the insert method
            JsonElement result = msTable.insert(obj).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setJsonResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            assertTrue(container.getJsonResult().isJsonObject());
            assertTrue(container.getJsonResult().getAsJsonObject().has("id"));
            assertTrue(container.getJsonResult().getAsJsonObject().has("String"));
            assertTrue(container.getJsonResult().getAsJsonObject().get("id").isJsonPrimitive());
            assertTrue(container.getJsonResult().getAsJsonObject().get("String").isJsonPrimitive());
            assertTrue(container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().isNumber());
            assertTrue(container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().isString());
            assertEquals(testId, container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().getAsLong());
            assertEquals("Hey", container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().getAsString());
        }
    }

    public void testUpdateWithNonStringAndNonIntIdResponseContent() throws Throwable {
        Object[] testIdData = IdTestData.NonStringNonIntValidJsonIds;

        for (Object testId : testIdData) {
            updateWithNonStringAndNonIntIdResponseContent(testId);
        }
    }

    private void updateWithNonStringAndNonIntIdResponseContent(Object testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        String stringTestId = testId.toString().toLowerCase(Locale.getDefault());

        final String responseContent = "{\"id\":" + stringTestId + ",\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        JsonObject obj = new JsonParser().parse("{\"id\":\"id\",\"value\":\"new\"}").getAsJsonObject();

        try {
            // Call the insert method
            JsonElement result = msTable.update(obj).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setJsonResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            assertTrue(container.getJsonResult().isJsonObject());

            assertTrue(container.getJsonResult().getAsJsonObject().has("id"));
            assertTrue(container.getJsonResult().getAsJsonObject().has("String"));

            assertTrue(container.getJsonResult().getAsJsonObject().get("String").isJsonPrimitive());

            assertTrue(container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().isString());

            if (container.getJsonResult().getAsJsonObject().get("id").isJsonNull()) {
                assertTrue(testId == null);
            } else if (container.getJsonResult().getAsJsonObject().get("id").isJsonPrimitive()) {
                if (container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().isBoolean()) {
                    assertEquals(testId, container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().getAsBoolean());
                } else if (container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().isNumber()) {
                    assertEquals(testId, container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().getAsDouble());
                } else if (container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().isString()) {
                    assertEquals(testId, container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().getAsString());
                }
            } else if (container.getJsonResult().getAsJsonObject().get("id").isJsonObject()) {
                assertTrue(container.getJsonResult().getAsJsonObject().get("id").getAsJsonObject().equals(testId));
            } else if (container.getJsonResult().getAsJsonObject().get("id").isJsonArray()) {
                assertTrue(container.getJsonResult().getAsJsonObject().get("id").getAsJsonArray().equals(testId));
            }

            assertEquals("Hey", container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().getAsString());
        }
    }

    public void testUpdateWithNullIdResponseContent() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"id\":null,\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        JsonObject obj = new JsonParser().parse("{\"id\":\"id\",\"value\":\"new\"}").getAsJsonObject();

        try {
            // Call the insert method
            JsonElement result = msTable.update(obj).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setJsonResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            assertTrue(container.getJsonResult().isJsonObject());

            assertTrue(container.getJsonResult().getAsJsonObject().has("id"));
            assertTrue(container.getJsonResult().getAsJsonObject().has("String"));

            assertTrue(container.getJsonResult().getAsJsonObject().get("id").isJsonNull());
            assertTrue(container.getJsonResult().getAsJsonObject().get("String").isJsonPrimitive());

            assertTrue(container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().isString());

            assertEquals("Hey", container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().getAsString());
        }
    }

    public void testUpdateWithNoIdResponseContent() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        JsonObject obj = new JsonParser().parse("{\"id\":\"id\",\"value\":\"new\"}").getAsJsonObject();

        try {
            // Call the insert method
            JsonElement result = msTable.update(obj).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setJsonResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            assertTrue(container.getJsonResult().isJsonObject());
            assertTrue(container.getJsonResult().getAsJsonObject().has("id"));
            assertTrue(container.getJsonResult().getAsJsonObject().has("String"));
            assertTrue(container.getJsonResult().getAsJsonObject().get("id").isJsonPrimitive());
            assertTrue(container.getJsonResult().getAsJsonObject().get("String").isJsonPrimitive());
            assertTrue(container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().isString());
            assertTrue(container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().isString());
            assertEquals("id", container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().getAsString());
            assertEquals("Hey", container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().getAsString());
        }
    }

    public void testUpdateWithStringIdItem() throws Throwable {
        String[] testIdData = IdTestData.ValidStringIds;

        for (String testId : testIdData) {
            updateWithStringIdItem(testId);
        }
    }

    private void updateWithStringIdItem(final String testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String jsonTestId = testId.replace("\\", "\\\\").replace("\"", "\\\"");
        final String responseContent = "{\"id\":\"" + jsonTestId + "\",\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        JsonObject obj = new JsonParser().parse("{\"id\":\"" + jsonTestId + "\",\"String\":\"what?\"}").getAsJsonObject();

        try {
            // Call the insert method
            JsonElement result = msTable.update(obj).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setJsonResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            assertTrue(container.getJsonResult().isJsonObject());
            assertTrue(container.getJsonResult().getAsJsonObject().has("id"));
            assertTrue(container.getJsonResult().getAsJsonObject().has("String"));
            assertTrue(container.getJsonResult().getAsJsonObject().get("id").isJsonPrimitive());
            assertTrue(container.getJsonResult().getAsJsonObject().get("String").isJsonPrimitive());
            assertTrue(container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().isString());
            assertTrue(container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().isString());
            assertEquals(testId, container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().getAsString());
            assertEquals("Hey", container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().getAsString());
        }
    }

    public void testUpdateWithEmptyStringIdItem() throws Throwable {
        String[] testIdData = IdTestData.EmptyStringIds;

        for (String testId : testIdData) {
            updateWithEmptyStringIdItem(testId);
        }
    }

    private void updateWithEmptyStringIdItem(final String testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"id\":5,\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        JsonObject obj = new JsonParser().parse("{\"id\":\"" + testId + "\",\"String\":\"what?\"}").getAsJsonObject();

        try {
            // Call the insert method
            JsonElement result = msTable.update(obj).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setJsonResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception == null || !(exception instanceof IllegalArgumentException)) {
            fail("Expected Exception IllegalArgumentException");
        }
    }

    public void testUpdatetWithInvalidStringIdItem() throws Throwable {
        String[] testIdData = IdTestData.InvalidStringIds;

        for (String testId : testIdData) {
            updateWithInvalidStringIdItem(testId);
        }
    }

    private void updateWithInvalidStringIdItem(final String testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String jsonTestId = testId.replace("\\", "\\\\").replace("\"", "\\\"");

        final String responseContent = "{\"id\":\"an id\",\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        JsonObject obj = new JsonParser().parse("{\"id\":\"" + jsonTestId + "\",\"String\":\"what?\"}").getAsJsonObject();

        try {
            // Call the insert method
            JsonElement result = msTable.update(obj).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setJsonResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception == null || !(exception instanceof IllegalArgumentException)) {
            fail("Expected Exception IllegalArgumentException");
        }
    }

    public void testUpdateWithIntIdItem() throws Throwable {
        long[] testIdData = IdTestData.ValidIntIds;

        for (long testId : testIdData) {
            updateWithIntIdItem(testId);
        }
    }

    private void updateWithIntIdItem(final long testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String stringTestId = String.valueOf(testId);

        final String responseContent = "{\"id\":" + stringTestId + ",\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        JsonObject obj = new JsonParser().parse("{\"id\":" + stringTestId + ",\"String\":\"what?\"}").getAsJsonObject();

        try {
            // Call the insert method
            JsonElement result = msTable.update(obj).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setJsonResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            assertTrue(container.getJsonResult().isJsonObject());
            assertTrue(container.getJsonResult().getAsJsonObject().has("id"));
            assertTrue(container.getJsonResult().getAsJsonObject().has("String"));
            assertTrue(container.getJsonResult().getAsJsonObject().get("id").isJsonPrimitive());
            assertTrue(container.getJsonResult().getAsJsonObject().get("String").isJsonPrimitive());
            assertTrue(container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().isNumber());
            assertTrue(container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().isString());
            assertEquals(testId, container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().getAsLong());
            assertEquals("Hey", container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().getAsString());
        }
    }

    public void testUpdateWithNullIdItem() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"id\":\"an id\",\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        JsonObject obj = new JsonParser().parse("{\"id\":null,\"String\":\"what?\"}").getAsJsonObject();

        try {
            // Call the insert method
            JsonElement result = msTable.update(obj).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setJsonResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception == null || !(exception instanceof IllegalArgumentException)) {
            fail("Expected Exception IllegalArgumentException");
        }
    }

    public void testUpdateWithNoIdItem() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"id\":5,\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        JsonObject obj = new JsonParser().parse("{\"String\":\"what?\"}").getAsJsonObject();

        try {
            // Call the insert method
            JsonElement result = msTable.update(obj).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setJsonResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception == null || !(exception instanceof IllegalArgumentException)) {
            fail("Expected Exception IllegalArgumentException");
        }
    }

    public void testUpdateWithZeroIdItem() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"id\":5,\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        JsonObject obj = new JsonParser().parse("{\"id\":0,\"String\":\"what?\"}").getAsJsonObject();

        try {
            // Call the insert method
            JsonElement result = msTable.update(obj).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setJsonResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception == null || !(exception instanceof IllegalArgumentException)) {
            fail("Expected Exception IllegalArgumentException");
        }
    }

    // Delete Tests

    public void testDeleteWithStringIdResponseContent() throws Throwable {
        String[] testIdData = IdTestData.concat(IdTestData.concat(IdTestData.ValidStringIds, IdTestData.EmptyStringIds), IdTestData.InvalidStringIds);

        for (String testId : testIdData) {
            deleteWithStringIdResponseContent(testId);
        }
    }

    private void deleteWithStringIdResponseContent(final String testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        String jsonTestId = testId.replace("\\", "\\\\").replace("\"", "\\\"");
        final String responseContent = "{\"id\":\"" + jsonTestId + "\",\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        JsonObject obj = new JsonParser().parse("{\"id\":\"id\",\"value\":\"new\"}").getAsJsonObject();

        try {
            // Call the delete method
            msTable.delete(obj).get();

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        }
    }

    public void testDeleteWithIntIdResponseContent() throws Throwable {
        long[] testIdData = IdTestData.concat(IdTestData.ValidIntIds, IdTestData.InvalidIntIds);

        for (long testId : testIdData) {
            deleteWithIntIdResponseContent(testId);
        }
    }

    private void deleteWithIntIdResponseContent(long testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        String stringTestId = String.valueOf(testId);

        final String responseContent = "{\"id\":" + stringTestId + ",\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        JsonObject obj = new JsonParser().parse("{\"id\":\"id\",\"value\":\"new\"}").getAsJsonObject();

        try {
            // Call the delete method
            msTable.delete(obj).get();

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        }
    }

    public void testDeleteWithNonStringAndNonIntIdResponseContent() throws Throwable {
        Object[] testIdData = IdTestData.NonStringNonIntValidJsonIds;

        for (Object testId : testIdData) {
            deleteWithNonStringAndNonIntIdResponseContent(testId);
        }
    }

    private void deleteWithNonStringAndNonIntIdResponseContent(Object testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        String stringTestId = testId.toString().toLowerCase(Locale.getDefault());

        final String responseContent = "{\"id\":" + stringTestId + ",\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        JsonObject obj = new JsonParser().parse("{\"id\":\"id\",\"value\":\"new\"}").getAsJsonObject();

        try {
            // Call the delete method
            msTable.delete(obj).get();

        } catch (Exception exception) {
            container.setException(exception);
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        }
    }

    public void testDeleteWithNullIdResponseContent() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"id\":null,\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        JsonObject obj = new JsonParser().parse("{\"id\":\"id\",\"value\":\"new\"}").getAsJsonObject();

        try {
            // Call the delete method
            msTable.delete(obj).get();

        } catch (Exception exception) {
            container.setException(exception);
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        }
    }

    public void testDeleteWithNoIdResponseContent() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        JsonObject obj = new JsonParser().parse("{\"id\":\"id\",\"value\":\"new\"}").getAsJsonObject();

        try {
            // Call the delete method
            msTable.delete(obj).get();

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        }
    }

    public void testDeleteWithStringIdItem() throws Throwable {
        String[] testIdData = IdTestData.ValidStringIds;

        for (String testId : testIdData) {
            deleteWithStringIdItem(testId);
        }
    }

    private void deleteWithStringIdItem(final String testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String jsonTestId = testId.replace("\\", "\\\\").replace("\"", "\\\"");
        final String responseContent = "{\"id\":\"" + jsonTestId + "\",\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        JsonObject obj = new JsonParser().parse("{\"id\":\"" + jsonTestId + "\",\"String\":\"what?\"}").getAsJsonObject();

        try {
            // Call the delete method
            msTable.delete(obj).get();

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        }
    }

    public void testDeleteWithEmptyStringIdItem() throws Throwable {
        String[] testIdData = IdTestData.EmptyStringIds;

        for (String testId : testIdData) {
            deleteWithEmptyStringIdItem(testId);
        }
    }

    private void deleteWithEmptyStringIdItem(final String testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"id\":\"an id\",\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        JsonObject obj = new JsonParser().parse("{\"id\":\"" + testId + "\",\"String\":\"what?\"}").getAsJsonObject();

        try {
            // Call the delete method
            msTable.delete(obj).get();

        } catch (Exception exception) {
            container.setException(exception);
        }

        // Asserts
        Exception exception = container.getException();

        if (exception == null || !(exception instanceof IllegalArgumentException)) {
            fail("Expected Exception IllegalArgumentException");
        }
    }

    public void testDeleteWithInvalidStringIdItem() throws Throwable {
        String[] testIdData = IdTestData.InvalidStringIds;

        for (String testId : testIdData) {
            deleteWithInvalidStringIdItem(testId);
        }
    }

    private void deleteWithInvalidStringIdItem(final String testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String jsonTestId = testId.replace("\\", "\\\\").replace("\"", "\\\"");
        final String responseContent = "{\"id\":\"an id\",\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        JsonObject obj = new JsonParser().parse("{\"id\":\"" + jsonTestId + "\",\"String\":\"what?\"}").getAsJsonObject();

        try {
            // Call the delete method
            msTable.delete(obj).get();

        } catch (Exception exception) {
            container.setException(exception);
        }

        // Asserts
        Exception exception = container.getException();

        if (exception == null || !(exception instanceof IllegalArgumentException)) {
            fail("Expected Exception IllegalArgumentException");
        }
    }

    public void testDeleteWithIntIdItem() throws Throwable {
        long[] testIdData = IdTestData.ValidIntIds;

        for (long testId : testIdData) {
            deleteWithIntIdItem(testId);
        }
    }

    private void deleteWithIntIdItem(final long testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String stringTestId = String.valueOf(testId);

        final String responseContent = "{\"id\":" + stringTestId + ",\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        JsonObject obj = new JsonParser().parse("{\"id\":" + stringTestId + ",\"String\":\"what?\"}").getAsJsonObject();

        try {
            // Call the delete method
            msTable.delete(obj).get();

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        }
    }

    public void testDeleteWithNullIdItem() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"id\":\"an id\",\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        JsonObject obj = new JsonParser().parse("{\"id\":null,\"String\":\"what?\"}").getAsJsonObject();

        try {
            // Call the delete method
            msTable.delete(obj).get();
        } catch (Exception exception) {
            container.setException(exception);
        }
        // Asserts
        Exception exception = container.getException();

        if (exception == null || !(exception instanceof IllegalArgumentException)) {
            fail("Expected Exception IllegalArgumentException");
        }
    }

    public void testDeleteWithNoIdItem() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"id\":\"an id\",\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        JsonObject obj = new JsonParser().parse("{\"String\":\"what?\"}").getAsJsonObject();

        try {
            // Call the delete method
            msTable.delete(obj).get();
        } catch (Exception exception) {
            container.setException(exception);
        }

        // Asserts
        Exception exception = container.getException();

        if (exception == null || !(exception instanceof IllegalArgumentException)) {
            fail("Expected Exception IllegalArgumentException");
        }
    }

    public void testDeleteWithZeroIdItem() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"id\":5,\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceJsonTable msTable = client.getTable(tableName);

        JsonObject obj = new JsonParser().parse("{\"id\":0,\"String\":\"what?\"}").getAsJsonObject();

        try {
            // Call the delete method
            msTable.delete(obj).get();
        } catch (Exception exception) {
            container.setException(exception);
        }

        // Asserts
        Exception exception = container.getException();

        if (exception == null || !(exception instanceof IllegalArgumentException)) {
            fail("Expected Exception IllegalArgumentException");
        }
    }

    // Generic

    // Read Tests

    // String Id Type

    public void testReadWithStringIdTypeAndStringIdResponseContent() throws Throwable {
        String[] testIdData = IdTestData.concat(IdTestData.concat(IdTestData.ValidStringIds, IdTestData.EmptyStringIds), IdTestData.InvalidStringIds);

        for (String testId : testIdData) {
            readWithStringIdTypeAndStringIdResponseContent(testId);
        }
    }

    private void readWithStringIdTypeAndStringIdResponseContent(String testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        String jsonTestId = testId.replace("\\", "\\\\").replace("\"", "\\\"");
        final String responseContent = "[{\"id\":\"" + jsonTestId + "\",\"String\":\"Hey\"}]";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<StringIdType> msTable = client.getTable(tableName, StringIdType.class);

        try {
            // Call the select method
            List<StringIdType> result = msTable.execute().get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            Object result = container.getCustomResult();
            assertTrue(result instanceof List<?>);

            List<?> resultList = (List<?>) result;
            assertTrue(resultList.size() == 1);

            Object obj = resultList.get(0);
            assertTrue(obj instanceof StringIdType);

            StringIdType elem = (StringIdType) obj;

            assertEquals(testId, elem.Id);
            assertEquals("Hey", elem.String);
        }
    }

    public void testReadWithStringIdTypeAndNonStringIdResponseContent() throws Throwable {
        Object[] testIdData = IdTestData.concat(IdTestData.convert(IdTestData.concat(IdTestData.ValidIntIds, IdTestData.InvalidIntIds)),
                IdTestData.NonStringNonIntValidJsonIds);

        for (Object testId : testIdData) {
            readWithStringIdTypeAndNonStringIdResponseContent(testId);
        }
    }

    private void readWithStringIdTypeAndNonStringIdResponseContent(Object testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        String stringTestId = testId.toString().toLowerCase(Locale.getDefault());
        final String responseContent = "[{\"id\":" + stringTestId + ",\"String\":\"Hey\"}]";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<StringIdType> msTable = client.getTable(tableName, StringIdType.class);

        try {
            // Call the select method
            List<StringIdType> result = msTable.execute().get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            Object result = container.getCustomResult();
            assertTrue(result instanceof List<?>);

            List<?> resultList = (List<?>) result;
            assertTrue(resultList.size() == 1);

            Object obj = resultList.get(0);
            assertTrue(obj instanceof StringIdType);

            StringIdType elem = (StringIdType) obj;

            assertEquals(stringTestId, elem.Id);
            assertEquals("Hey", elem.String);
        }
    }

    public void testReadWithStringIdTypeAndNullIdResponseContent() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "[{\"id\":null,\"String\":\"Hey\"}]";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<StringIdType> msTable = client.getTable(tableName, StringIdType.class);

        try {
            // Call the select method
            List<StringIdType> result = msTable.execute().get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            Object result = container.getCustomResult();
            assertTrue(result instanceof List<?>);

            List<?> resultList = (List<?>) result;
            assertTrue(resultList.size() == 1);

            Object obj = resultList.get(0);
            assertTrue(obj instanceof StringIdType);

            StringIdType elem = (StringIdType) obj;

            assertEquals(null, elem.Id);
            assertEquals("Hey", elem.String);
        }
    }

    public void testReadWithStringIdTypeAndNoIdResponseContent() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "[{\"String\":\"Hey\"}]";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<StringIdType> msTable = client.getTable(tableName, StringIdType.class);

        try {
            // Call the select method
            List<StringIdType> result = msTable.execute().get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            Object result = container.getCustomResult();
            assertTrue(result instanceof List<?>);

            List<?> resultList = (List<?>) result;
            assertTrue(resultList.size() == 1);

            Object obj = resultList.get(0);
            assertTrue(obj instanceof StringIdType);

            StringIdType elem = (StringIdType) obj;

            assertEquals(null, elem.Id);
            assertEquals("Hey", elem.String);
        }
    }

    public void testReadWithStringIdTypeAndStringIdFilter() throws Throwable {
        String[] testIdData = IdTestData.concat(IdTestData.concat(IdTestData.ValidStringIds, IdTestData.EmptyStringIds), IdTestData.InvalidStringIds);

        for (String testId : testIdData) {
            readWithStringIdTypeAndStringIdFilter(testId);
        }
    }

    private void readWithStringIdTypeAndStringIdFilter(final String testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        String jsonTestId = testId.replace("\\", "\\\\").replace("\"", "\\\"");
        final String responseContent = "[{\"id\":\"" + jsonTestId + "\",\"String\":\"Hey\"}]";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<StringIdType> msTable = client.getTable(tableName, StringIdType.class);

        try {
            // Call the select method
            List<StringIdType> result = msTable.where(field("Id").eq().val(testId)).execute().get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            Object result = container.getCustomResult();
            assertTrue(result instanceof List<?>);

            List<?> resultList = (List<?>) result;
            assertTrue(resultList.size() == 1);

            Object obj = resultList.get(0);
            assertTrue(obj instanceof StringIdType);

            StringIdType elem = (StringIdType) obj;

            assertEquals(testId, elem.Id);
            assertEquals("Hey", elem.String);
        }
    }

    public void testReadWithStringIdTypeAndNullIdFilter() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "[{\"id\":null,\"String\":\"Hey\"}]";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<StringIdType> msTable = client.getTable(tableName, StringIdType.class);

        try {
            // Call the select method
            List<StringIdType> result = msTable.where(field("Id").eq().val((String) null)).execute().get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            Object result = container.getCustomResult();
            assertTrue(result instanceof List<?>);

            List<?> resultList = (List<?>) result;
            assertTrue(resultList.size() == 1);

            Object obj = resultList.get(0);
            assertTrue(obj instanceof StringIdType);

            StringIdType elem = (StringIdType) obj;

            assertEquals(null, elem.Id);
            assertEquals("Hey", elem.String);
        }
    }

    public void testReadWithStringIdTypeAndStringIdProjection() throws Throwable {
        String[] testIdData = IdTestData.concat(IdTestData.concat(IdTestData.ValidStringIds, IdTestData.EmptyStringIds), IdTestData.InvalidStringIds);

        for (String testId : testIdData) {
            readWithStringIdTypeAndStringIdProjection(testId);
        }
    }

    private void readWithStringIdTypeAndStringIdProjection(final String testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        String jsonTestId = testId.replace("\\", "\\\\").replace("\"", "\\\"");
        final String responseContent = "[{\"id\":\"" + jsonTestId + "\",\"String\":\"Hey\"}]";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<StringIdType> msTable = client.getTable(tableName, StringIdType.class);

        try {
            // Call the select method
            List<StringIdType> result = msTable.select("Id,String").execute().get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            Object result = container.getCustomResult();
            assertTrue(result instanceof List<?>);

            List<?> resultList = (List<?>) result;
            assertTrue(resultList.size() == 1);

            Object obj = resultList.get(0);
            assertTrue(obj instanceof StringIdType);

            StringIdType elem = (StringIdType) obj;

            assertEquals(testId, elem.Id);
            assertEquals("Hey", elem.String);
        }
    }

    public void testReadWithStringIdTypeAndNullIdProjection() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "[{\"id\":null,\"String\":\"Hey\"}]";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<StringIdType> msTable = client.getTable(tableName, StringIdType.class);

        try {
            // Call the select method
            List<StringIdType> result = msTable.select("Id,String").execute().get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            Object result = container.getCustomResult();
            assertTrue(result instanceof List<?>);

            List<?> resultList = (List<?>) result;
            assertTrue(resultList.size() == 1);

            Object obj = resultList.get(0);
            assertTrue(obj instanceof StringIdType);

            StringIdType elem = (StringIdType) obj;

            assertEquals(null, elem.Id);
            assertEquals("Hey", elem.String);
        }
    }

    public void testReadWithStringIdTypeAndNoIdProjection() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "[{\"String\":\"Hey\"}]";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<StringIdType> msTable = client.getTable(tableName, StringIdType.class);

        try {
            // Call the select method
            List<StringIdType> result = msTable.select("Id,String").execute().get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }
        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            Object result = container.getCustomResult();
            assertTrue(result instanceof List<?>);

            List<?> resultList = (List<?>) result;
            assertTrue(resultList.size() == 1);

            Object obj = resultList.get(0);
            assertTrue(obj instanceof StringIdType);

            StringIdType elem = (StringIdType) obj;

            assertEquals(null, elem.Id);
            assertEquals("Hey", elem.String);
        }
    }

    // Integer Id Type

    public void testReadWithIntIdTypeAndIntIdResponseContent() throws Throwable {
        long[] testIdData = IdTestData.concat(IdTestData.ValidIntIds, IdTestData.InvalidIntIds);

        for (long testId : testIdData) {
            readWithIntIdTypeAndIntIdResponseContent(testId);
        }
    }

    private void readWithIntIdTypeAndIntIdResponseContent(long testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        String stringTestId = String.valueOf(testId);
        final String responseContent = "[{\"id\":" + stringTestId + ",\"String\":\"Hey\"}]";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<LongIdType> msTable = client.getTable(tableName, LongIdType.class);

        try {
            // Call the select method
            List<LongIdType> result = msTable.execute().get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            Object result = container.getCustomResult();
            assertTrue(result instanceof List<?>);

            List<?> resultList = (List<?>) result;
            assertTrue(resultList.size() == 1);

            Object obj = resultList.get(0);
            assertTrue(obj instanceof LongIdType);

            LongIdType elem = (LongIdType) obj;

            assertEquals(testId, elem.Id);
            assertEquals("Hey", elem.String);
        }
    }

	/*
     * public void testReadWithIntIdTypeAndIntParseableIdResponseContent()
	 * throws Throwable { Object[] testIdData =
	 * IdTestData.NonStringNonIntValidJsonIds;
	 * 
	 * for (Object testId : testIdData) {
	 * readWithIntIdTypeAndIntParseableIdResponseContent(testId); } }
	 * 
	 * private void readWithIntIdTypeAndIntParseableIdResponseContent(Object
	 * testId) throws Throwable { final CountDownLatch latch = new
	 * CountDownLatch(1);
	 * 
	 * // Container to store callback's results and do the asserts. final
	 * ResultsContainer container = new ResultsContainer();
	 * 
	 * final String tableName = "MyTableName";
	 * 
	 * String stringTestId = testId.toString().toLowerCase(Locale.getDefault());
	 * final String responseContent = "[{\"id\":" + stringTestId +
	 * ",\"String\":\"Hey\"}]";
	 * 
	 * runTestOnUiThread(new Runnable() {
	 * 
	 * @Override public void run() { MobileServiceClient client = null;
	 * 
	 * try { client = new MobileServiceClient(appUrl, appKey,
	 * getInstrumentation().getTargetContext()); } catch (MalformedURLException
	 * e) { e.printStackTrace(); }
	 * 
	 * // Add a filter to handle the request and create a new json // object
	 * with an id defined client =
	 * client.withFilter(getTestFilter(responseContent));
	 * 
	 * // Create get the MobileService table MobileServiceTable<LongIdType>
	 * msTable = client.getTable(tableName, LongIdType.class);
	 * 
	 * // Call the select method msTable.execute(new
	 * TableQueryCallback<LongIdType>() {
	 * 
	 * @Override public void onCompleted(List<LongIdType> result, int count,
	 * Exception exception, ServiceFilterResponse response) { if (exception !=
	 * null) { container.setException(exception); } else if (result == null) {
	 * container.setException(new Exception("Expected result")); } else {
	 * container.setCustomResult(result); }
	 * 
	 * latch.countDown(); } }); } });
	 * 
	 * latch.await();
	 * 
	 * // Asserts Exception exception = container.getException();
	 * 
	 * if (exception != null) { fail(exception.getMessage()); } else { Object
	 * result = container.getCustomResult(); assertTrue(result instanceof
	 * List<?>);
	 * 
	 * List<?> resultList = (List<?>)result; assertTrue(resultList.size() == 1);
	 * 
	 * Object obj = resultList.get(0); assertTrue(obj instanceof LongIdType);
	 * 
	 * LongIdType elem = (LongIdType)obj;
	 * 
	 * assertEquals(Long.valueOf(stringTestId), Long.valueOf(elem.Id));
	 * assertEquals("Hey", elem.String); } }
	 */

    public void testReadWithIntIdTypeAndNullIdResponseContent() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "[{\"id\":null,\"String\":\"Hey\"}]";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<LongIdType> msTable = client.getTable(tableName, LongIdType.class);

        try {
            // Call the select method
            List<LongIdType> result = msTable.execute().get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            Object result = container.getCustomResult();
            assertTrue(result instanceof List<?>);

            List<?> resultList = (List<?>) result;
            assertTrue(resultList.size() == 1);

            Object obj = resultList.get(0);
            assertTrue(obj instanceof LongIdType);

            LongIdType elem = (LongIdType) obj;

            assertEquals(0L, elem.Id);
            assertEquals("Hey", elem.String);
        }
    }

    public void testReadWithIntIdTypeAndNoIdResponseContent() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "[{\"String\":\"Hey\"}]";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<LongIdType> msTable = client.getTable(tableName, LongIdType.class);

        try {
            // Call the select method
            List<LongIdType> result = msTable.execute().get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            Object result = container.getCustomResult();
            assertTrue(result instanceof List<?>);

            List<?> resultList = (List<?>) result;
            assertTrue(resultList.size() == 1);

            Object obj = resultList.get(0);
            assertTrue(obj instanceof LongIdType);

            LongIdType elem = (LongIdType) obj;

            assertEquals(0L, elem.Id);
            assertEquals("Hey", elem.String);
        }
    }

    public void testReadWithIntIdTypeAndStringIdResponseContent() throws Throwable {
        String[] testIdData = IdTestData.concat(IdTestData.concat(IdTestData.ValidStringIds, IdTestData.EmptyStringIds), IdTestData.InvalidStringIds);

        for (String testId : testIdData) {
            readWithIntIdTypeAndStringIdResponseContent(testId);
        }
    }

    private void readWithIntIdTypeAndStringIdResponseContent(String testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        String jsonTestId = testId.replace("\\", "\\\\").replace("\"", "\\\"");
        final String responseContent = "[{\"id\":\"" + jsonTestId + "\",\"String\":\"Hey\"}]";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<LongIdType> msTable = client.getTable(tableName, LongIdType.class);

        try {
            // Call the select method
            List<LongIdType> result = msTable.execute().get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts

        Exception exception = container.getException();

        if (exception == null || !(exception instanceof JsonSyntaxException) || !(exception.getCause() instanceof NumberFormatException)) {
            fail("Expected Exception IllegalArgumentException");
        }
    }

    public void testReadWithIntIdTypeAndIntIdFilter() throws Throwable {
        long[] testIdData = IdTestData.concat(IdTestData.ValidIntIds, IdTestData.InvalidIntIds);

        for (long testId : testIdData) {
            readWithIntIdTypeAndIntIdFilter(testId);
        }
    }

    private void readWithIntIdTypeAndIntIdFilter(final long testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        String stringTestId = String.valueOf(testId);
        final String responseContent = "[{\"id\":" + stringTestId + ",\"String\":\"Hey\"}]";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<LongIdType> msTable = client.getTable(tableName, LongIdType.class);

        try {
            // Call the select method
            List<LongIdType> result = msTable.where(field("Id").eq().val(testId)).execute().get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            Object result = container.getCustomResult();
            assertTrue(result instanceof List<?>);

            List<?> resultList = (List<?>) result;
            assertTrue(resultList.size() == 1);

            Object obj = resultList.get(0);
            assertTrue(obj instanceof LongIdType);

            LongIdType elem = (LongIdType) obj;

            assertEquals(testId, elem.Id);
            assertEquals("Hey", elem.String);
        }
    }

    public void testReadWithIntIdTypeAndNullIdFilter() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "[{\"id\":null,\"String\":\"Hey\"}]";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<LongIdType> msTable = client.getTable(tableName, LongIdType.class);

        try {
            // Call the select method
            List<LongIdType> result = msTable.where(field("Id").eq().val((Long) null)).execute().get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            Object result = container.getCustomResult();
            assertTrue(result instanceof List<?>);

            List<?> resultList = (List<?>) result;
            assertTrue(resultList.size() == 1);

            Object obj = resultList.get(0);
            assertTrue(obj instanceof LongIdType);

            LongIdType elem = (LongIdType) obj;

            assertEquals(0L, elem.Id);
            assertEquals("Hey", elem.String);
        }
    }

    public void testReadWithIntIdTypeAndIntIdProjection() throws Throwable {
        long[] testIdData = IdTestData.concat(IdTestData.ValidIntIds, IdTestData.InvalidIntIds);

        for (long testId : testIdData) {
            readWithIntIdTypeAndIntIdProjection(testId);
        }
    }

    private void readWithIntIdTypeAndIntIdProjection(long testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        String stringTestId = String.valueOf(testId);
        final String responseContent = "[{\"id\":" + stringTestId + ",\"String\":\"Hey\"}]";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<LongIdType> msTable = client.getTable(tableName, LongIdType.class);

        try {
            // Call the select method
            List<LongIdType> result = msTable.select("Id,String").execute().get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            Object result = container.getCustomResult();
            assertTrue(result instanceof List<?>);

            List<?> resultList = (List<?>) result;
            assertTrue(resultList.size() == 1);

            Object obj = resultList.get(0);
            assertTrue(obj instanceof LongIdType);

            LongIdType elem = (LongIdType) obj;

            assertEquals(testId, elem.Id);
            assertEquals("Hey", elem.String);
        }
    }

    public void testReadWithIntIdTypeAndNullIdProjection() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "[{\"id\":null,\"String\":\"Hey\"}]";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<LongIdType> msTable = client.getTable(tableName, LongIdType.class);

        try {
            // Call the select method
            List<LongIdType> result = msTable.select("Id,String").execute().get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            Object result = container.getCustomResult();
            assertTrue(result instanceof List<?>);

            List<?> resultList = (List<?>) result;
            assertTrue(resultList.size() == 1);

            Object obj = resultList.get(0);
            assertTrue(obj instanceof LongIdType);

            LongIdType elem = (LongIdType) obj;

            assertEquals(0L, elem.Id);
            assertEquals("Hey", elem.String);
        }
    }

    public void testReadWithIntIdTypeAndNoIdProjection() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "[{\"String\":\"Hey\"}]";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<LongIdType> msTable = client.getTable(tableName, LongIdType.class);

        try {
            // Call the select method
            List<LongIdType> result = msTable.select("Id,String").execute().get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            Object result = container.getCustomResult();
            assertTrue(result instanceof List<?>);

            List<?> resultList = (List<?>) result;
            assertTrue(resultList.size() == 1);

            Object obj = resultList.get(0);
            assertTrue(obj instanceof LongIdType);

            LongIdType elem = (LongIdType) obj;

            assertEquals(0L, elem.Id);
            assertEquals("Hey", elem.String);
        }
    }

    // Lookup Tests

    // String Id Type

    public void testLookupWithStringIdTypeAndStringIdResponseContent() throws Throwable {
        String[] testIdData = IdTestData.concat(IdTestData.concat(IdTestData.ValidStringIds, IdTestData.EmptyStringIds), IdTestData.InvalidStringIds);

        for (String testId : testIdData) {
            lookupWithStringIdTypeAndStringIdResponseContent(testId);
        }
    }

    private void lookupWithStringIdTypeAndStringIdResponseContent(String testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        String jsonTestId = testId.replace("\\", "\\\\").replace("\"", "\\\"");
        final String responseContent = "{\"id\":\"" + jsonTestId + "\",\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<StringIdType> msTable = client.getTable(tableName, StringIdType.class);

        try {
            // Call the lookup method
            StringIdType result = msTable.lookUp("an id").get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            Object result = container.getCustomResult();
            assertTrue(result instanceof StringIdType);

            StringIdType elem = (StringIdType) result;

            assertEquals(testId, elem.Id);
            assertEquals("Hey", elem.String);
        }
    }

    public void testLookupWithStringIdTypeAndNonStringIdResponseContent() throws Throwable {
        Object[] testIdData = IdTestData.concat(IdTestData.convert(IdTestData.concat(IdTestData.ValidIntIds, IdTestData.InvalidIntIds)),
                IdTestData.NonStringNonIntValidJsonIds);

        for (Object testId : testIdData) {
            lookupWithStringIdTypeAndNonStringIdResponseContent(testId);
        }
    }

    private void lookupWithStringIdTypeAndNonStringIdResponseContent(Object testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        String stringTestId = testId.toString().toLowerCase(Locale.getDefault());
        final String responseContent = "{\"id\":" + stringTestId + ",\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<StringIdType> msTable = client.getTable(tableName, StringIdType.class);

        try {
            // Call the lookup method
            StringIdType result = msTable.lookUp("an id").get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            Object result = container.getCustomResult();
            assertTrue(result instanceof StringIdType);

            StringIdType elem = (StringIdType) result;

            assertEquals(stringTestId, elem.Id);
            assertEquals("Hey", elem.String);
        }
    }

    public void testLookupWithStringIdTypeAndNullIdResponseContent() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"id\":null,\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<StringIdType> msTable = client.getTable(tableName, StringIdType.class);

        try {
            // Call the lookup method
            StringIdType result = msTable.lookUp("an id").get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            Object result = container.getCustomResult();
            assertTrue(result instanceof StringIdType);

            StringIdType elem = (StringIdType) result;

            assertEquals(null, elem.Id);
            assertEquals("Hey", elem.String);
        }
    }

    public void testLookupWithStringIdTypeAndNoIdResponseContent() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<StringIdType> msTable = client.getTable(tableName, StringIdType.class);

        try {
            // Call the lookup method
            StringIdType result = msTable.lookUp("an id").get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            Object result = container.getCustomResult();
            assertTrue(result instanceof StringIdType);

            StringIdType elem = (StringIdType) result;

            assertEquals(null, elem.Id);
            assertEquals("Hey", elem.String);
        }
    }

    public void testLookupWithStringIdTypeAndStringIdParameter() throws Throwable {
        String[] testIdData = IdTestData.ValidStringIds;

        for (String testId : testIdData) {
            lookupWithStringIdTypeAndStringIdParameter(testId);
        }
    }

    private void lookupWithStringIdTypeAndStringIdParameter(final String testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        String jsonTestId = testId.replace("\\", "\\\\").replace("\"", "\\\"");
        final String responseContent = "{\"id\":\"" + jsonTestId + "\",\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<StringIdType> msTable = client.getTable(tableName, StringIdType.class);

        try {
            // Call the lookup method
            StringIdType result = msTable.lookUp(testId).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            Object result = container.getCustomResult();
            assertTrue(result instanceof StringIdType);

            StringIdType elem = (StringIdType) result;

            assertEquals(testId, elem.Id);
            assertEquals("Hey", elem.String);
        }
    }

    public void testLookupWithStringIdTypeAndInvalidStringIdParameter() throws Throwable {
        String[] testIdData = IdTestData.concat(IdTestData.EmptyStringIds, IdTestData.InvalidStringIds);

        for (String testId : testIdData) {
            lookupWithStringIdTypeAndInvalidStringIdParameter(testId);
        }
    }

    private void lookupWithStringIdTypeAndInvalidStringIdParameter(final String testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"id\":\"an id\",\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<StringIdType> msTable = client.getTable(tableName, StringIdType.class);

        try {
            // Call the lookup method
            StringIdType result = msTable.lookUp(testId).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }
        // Asserts
        Exception exception = container.getException();

        if (exception == null || !(exception instanceof IllegalArgumentException)) {
            fail("Expected Exception IllegalArgumentException");
        }
    }

	/*
     * public void testLookupWithStringIdTypeAndIntIdParameter() throws
	 * Throwable { long[] testIdData = IdTestData.concat(IdTestData.ValidIntIds,
	 * IdTestData.InvalidIntIds);
	 * 
	 * for (long testId : testIdData) {
	 * lookupWithStringIdTypeAndIntIdParameter(testId); } }
	 * 
	 * private void lookupWithStringIdTypeAndIntIdParameter(final long testId)
	 * throws Throwable {
	 * 
	 * // Container to store callback's results and do the asserts. final
	 * ResultsContainer container = new ResultsContainer();
	 * 
	 * final String tableName = "MyTableName";
	 * 
	 * final String responseContent = "{\"id\":\"an id\",\"String\":\"Hey\"}";
	 * 
	 * runTestOnUiThread(new Runnable() {
	 * 
	 * @Override public void run() { MobileServiceClient client = null;
	 * 
	 * try { client = new MobileServiceClient(appUrl, appKey,
	 * getInstrumentation().getTargetContext()); } catch (MalformedURLException
	 * e) { e.printStackTrace(); }
	 * 
	 * // Add a filter to handle the request and create a new json // object
	 * with an id defined client =
	 * client.withFilter(getTestFilter(responseContent));
	 * 
	 * // Create get the MobileService table MobileServiceTable<StringIdType>
	 * msTable = client.getTable(tableName, StringIdType.class);
	 * 
	 * // Call the lookup method msTable.lookUp(testId, new
	 * TableOperationCallback<StringIdType>() {
	 * 
	 * @Override public void onCompleted(StringIdType entity, Exception
	 * exception, ServiceFilterResponse response) { if (exception != null) {
	 * container.setException(exception); } else if (entity == null) {
	 * container.setException(new Exception("Expected result")); } else {
	 * container.setCustomResult(entity); }
	 * 
	 * latch.countDown(); } }); } });
	 * 
	 * latch.await();
	 * 
	 * // Asserts Exception exception = container.getException();
	 * 
	 * if (exception == null || !(exception instanceof
	 * IllegalArgumentException)) {
	 * fail("Expected Exception IllegalArgumentException"); } }
	 */

    public void testLookupWithStringIdTypeAndNonStringNonIntIdParameter() throws Throwable {
        Object[] testIdData = IdTestData.NonStringNonIntValidJsonIds;

        for (Object testId : testIdData) {
            lookupWithStringIdTypeAndNonStringNonIntIdParameter(testId);
        }
    }

    private void lookupWithStringIdTypeAndNonStringNonIntIdParameter(final Object testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"id\":\"an id\",\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<StringIdType> msTable = client.getTable(tableName, StringIdType.class);

        try {
            // Call the lookup method
            StringIdType result = msTable.lookUp(testId).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception == null) {
            fail("Expected Exception IllegalArgumentException");
        }
    }

    public void testLookupWithStringIdTypeAndNullIdParameter() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"id\":\"an id\",\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<StringIdType> msTable = client.getTable(tableName, StringIdType.class);

        try {
            // Call the lookup method
            StringIdType result = msTable.lookUp(null).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception == null || !(exception instanceof IllegalArgumentException)) {
            fail("Expected Exception IllegalArgumentException");
        }
    }

    // Integer Id Type

    public void testLookupWithIntIdTypeAndIntIdResponseContent() throws Throwable {
        long[] testIdData = IdTestData.concat(IdTestData.ValidIntIds, IdTestData.InvalidIntIds);

        for (long testId : testIdData) {
            lookupWithIntIdTypeAndIntIdResponseContent(testId);
        }
    }

    private void lookupWithIntIdTypeAndIntIdResponseContent(long testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        String stringTestId = String.valueOf(testId);
        final String responseContent = "{\"id\":" + stringTestId + ",\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<LongIdType> msTable = client.getTable(tableName, LongIdType.class);

        try {
            // Call the lookup method
            LongIdType result = msTable.lookUp(10).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            Object result = container.getCustomResult();
            assertTrue(result instanceof LongIdType);

            LongIdType elem = (LongIdType) result;

            assertEquals(testId, elem.Id);
            assertEquals("Hey", elem.String);
        }
    }

	/*
	 * public void testLookupWithIntIdTypeAndIntParseableIdResponseContent()
	 * throws Throwable { Object[] testIdData =
	 * IdTestData.NonStringNonIntValidJsonIds;
	 * 
	 * for (Object testId : testIdData) {
	 * lookupWithIntIdTypeAndIntParseableIdResponseContent(testId); } }
	 * 
	 * private void lookupWithIntIdTypeAndIntParseableIdResponseContent(Object
	 * testId) throws Throwable { final CountDownLatch latch = new
	 * CountDownLatch(1);
	 * 
	 * // Container to store callback's results and do the asserts. final
	 * ResultsContainer container = new ResultsContainer();
	 * 
	 * final String tableName = "MyTableName";
	 * 
	 * String stringTestId = testId.toString().toLowerCase(Locale.getDefault());
	 * final String responseContent = "{\"id\":" + stringTestId +
	 * ",\"String\":\"Hey\"}";
	 * 
	 * runTestOnUiThread(new Runnable() {
	 * 
	 * @Override public void run() { MobileServiceClient client = null;
	 * 
	 * try { client = new MobileServiceClient(appUrl, appKey,
	 * getInstrumentation().getTargetContext()); } catch (MalformedURLException
	 * e) { e.printStackTrace(); }
	 * 
	 * // Add a filter to handle the request and create a new json // object
	 * with an id defined client =
	 * client.withFilter(getTestFilter(responseContent));
	 * 
	 * // Create get the MobileService table MobileServiceTable<LongIdType>
	 * msTable = client.getTable(tableName, LongIdType.class);
	 * 
	 * // Call the lookup method msTable.lookUp(10, new
	 * TableOperationCallback<LongIdType>() {
	 * 
	 * @Override public void onCompleted(LongIdType entity, Exception exception,
	 * ServiceFilterResponse response) { if (exception != null) {
	 * container.setException(exception); } else if (entity == null) {
	 * container.setException(new Exception("Expected result")); } else {
	 * container.setCustomResult(entity); }
	 * 
	 * latch.countDown(); } }); } });
	 * 
	 * latch.await();
	 * 
	 * // Asserts Exception exception = container.getException();
	 * 
	 * if (exception != null) { fail(exception.getMessage()); } else { Object
	 * result = container.getCustomResult(); assertTrue(result instanceof
	 * LongIdType);
	 * 
	 * LongIdType elem = (LongIdType)result;
	 * 
	 * assertEquals(Long.valueOf(stringTestId), Long.valueOf(elem.Id));
	 * assertEquals("Hey", elem.String); } }
	 */

    public void testLookupWithIntIdTypeAndNullIdResponseContent() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"id\":null,\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<LongIdType> msTable = client.getTable(tableName, LongIdType.class);

        try {
            // Call the lookup method
            LongIdType result = msTable.lookUp(10).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            Object result = container.getCustomResult();
            assertTrue(result instanceof LongIdType);

            LongIdType elem = (LongIdType) result;

            assertEquals(0L, elem.Id);
            assertEquals("Hey", elem.String);
        }
    }

    public void testLookupWithIntIdTypeAndNoIdResponseContent() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<LongIdType> msTable = client.getTable(tableName, LongIdType.class);

        try {
            // Call the lookup method
            LongIdType result = msTable.lookUp(10).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            Object result = container.getCustomResult();
            assertTrue(result instanceof LongIdType);

            LongIdType elem = (LongIdType) result;

            assertEquals(0L, elem.Id);
            assertEquals("Hey", elem.String);
        }
    }

    public void testLookupWithIntIdTypeAndStringIdResponseContent() throws Throwable {
        String[] testIdData = IdTestData.concat(IdTestData.concat(IdTestData.ValidStringIds, IdTestData.EmptyStringIds), IdTestData.InvalidStringIds);

        for (String testId : testIdData) {
            lookupWithIntIdTypeAndStringIdResponseContent(testId);
        }
    }

    private void lookupWithIntIdTypeAndStringIdResponseContent(String testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        String jsonTestId = testId.replace("\\", "\\\\").replace("\"", "\\\"");
        final String responseContent = "{\"id\":\"" + jsonTestId + "\",\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<LongIdType> msTable = client.getTable(tableName, LongIdType.class);

        try {
            // Call the lookup method
            LongIdType result = msTable.lookUp(10).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts

        Exception exception = container.getException();

        if (exception == null || !(exception instanceof JsonSyntaxException) || !(exception.getCause() instanceof NumberFormatException)) {
            fail("Expected Exception IllegalArgumentException");
        }
    }

    public void testLookupWithIntIdTypeAndIntIdParameter() throws Throwable {
        long[] testIdData = IdTestData.ValidIntIds;

        for (long testId : testIdData) {
            lookupWithIntIdTypeAndIntIdParameter(testId);
        }
    }

    private void lookupWithIntIdTypeAndIntIdParameter(final long testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        String stringTestId = String.valueOf(testId);
        final String responseContent = "{\"id\":" + stringTestId + ",\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<LongIdType> msTable = client.getTable(tableName, LongIdType.class);

        try {
            // Call the lookup method
            LongIdType result = msTable.lookUp(testId).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            Object result = container.getCustomResult();
            assertTrue(result instanceof LongIdType);

            LongIdType elem = (LongIdType) result;

            assertEquals(testId, elem.Id);
            assertEquals("Hey", elem.String);
        }
    }

    public void testLookupWithIntIdTypeAndInvalidIntIdParameter() throws Throwable {
        long[] testIdData = IdTestData.InvalidIntIds;

        for (long testId : testIdData) {
            lookupWithIntIdTypeAndInvalidIntIdParameter(testId);
        }
    }

    private void lookupWithIntIdTypeAndInvalidIntIdParameter(final long testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"id\":5,\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<LongIdType> msTable = client.getTable(tableName, LongIdType.class);

        try {
            // Call the lookup method
            LongIdType result = msTable.lookUp(testId).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception == null || !(exception instanceof IllegalArgumentException)) {
            fail("Expected Exception IllegalArgumentException");
        }
    }

	/*
	 * public void testLookupWithIntIdTypeAndStringIdParameter() throws
	 * Throwable { String[] testIdData =
	 * IdTestData.concat(IdTestData.ValidStringIds,
	 * IdTestData.concat(IdTestData.EmptyStringIds,
	 * IdTestData.InvalidStringIds));
	 * 
	 * for (String testId : testIdData) {
	 * lookupWithIntIdTypeAndStringIdParameter(testId); } }
	 * 
	 * private void lookupWithIntIdTypeAndStringIdParameter(final String testId)
	 * throws Throwable {
	 * 
	 * // Container to store callback's results and do the asserts. final
	 * ResultsContainer container = new ResultsContainer();
	 * 
	 * final String tableName = "MyTableName";
	 * 
	 * final String responseContent = "{\"id\":5,\"String\":\"Hey\"}";
	 * 
	 * runTestOnUiThread(new Runnable() {
	 * 
	 * @Override public void run() { MobileServiceClient client = null;
	 * 
	 * try { client = new MobileServiceClient(appUrl, appKey,
	 * getInstrumentation().getTargetContext()); } catch (MalformedURLException
	 * e) { e.printStackTrace(); }
	 * 
	 * // Add a filter to handle the request and create a new json // object
	 * with an id defined client =
	 * client.withFilter(getTestFilter(responseContent));
	 * 
	 * // Create get the MobileService table MobileServiceTable<LongIdType>
	 * msTable = client.getTable(tableName, LongIdType.class);
	 * 
	 * // Call the lookup method msTable.lookUp(testId, new
	 * TableOperationCallback<LongIdType>() {
	 * 
	 * @Override public void onCompleted(LongIdType entity, Exception exception,
	 * ServiceFilterResponse response) { if (exception != null) {
	 * container.setException(exception); } else if (entity == null) {
	 * container.setException(new Exception("Expected result")); } else {
	 * container.setCustomResult(entity); }
	 * 
	 * latch.countDown(); } }); } });
	 * 
	 * latch.await();
	 * 
	 * // Asserts Exception exception = container.getException();
	 * 
	 * if (exception == null) {
	 * fail("Expected Exception IllegalArgumentException"); } }
	 */

    public void testLookupWithIntIdTypeAndNonStringNonIntIdParameter() throws Throwable {
        Object[] testIdData = IdTestData.NonStringNonIntValidJsonIds;

        for (Object testId : testIdData) {
            lookupWithIntIdTypeAndNonStringNonIntIdParameter(testId);
        }
    }

    private void lookupWithIntIdTypeAndNonStringNonIntIdParameter(final Object testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"id\":5,\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<LongIdType> msTable = client.getTable(tableName, LongIdType.class);

        try {
            // Call the lookup method
            LongIdType result = msTable.lookUp(testId).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception == null) {
            fail("Expected Exception IllegalArgumentException");
        }
    }

    public void testLookupWithIntIdTypeAndNullParameter() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"id\":5,\"String\":\"Hey\"}";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<LongIdType> msTable = client.getTable(tableName, LongIdType.class);

        try {
            // Call the lookup method
            LongIdType result = msTable.lookUp(null).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception == null || !(exception instanceof IllegalArgumentException)) {
            fail("Expected Exception IllegalArgumentException");
        }
    }

    // Insert Tests

    // String Id Type

    public void testInsertWithStringIdTypeAndStringIdResponseContent() throws Throwable {
        String[] testIdData = IdTestData.concat(IdTestData.concat(IdTestData.ValidStringIds, IdTestData.EmptyStringIds), IdTestData.InvalidStringIds);

        for (String testId : testIdData) {
            insertWithStringIdTypeAndStringIdResponseContent(testId);
        }
    }

    private void insertWithStringIdTypeAndStringIdResponseContent(String testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        String jsonTestId = testId.replace("\\", "\\\\").replace("\"", "\\\"");
        final String responseContent = "{\"id\":\"" + jsonTestId + "\",\"String\":\"Hey\"}";

        final StringIdType item = new StringIdType();
        item.Id = "an id";
        item.String = "what?";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<StringIdType> msTable = client.getTable(tableName, StringIdType.class);

        try {
            // Call the insert method
            StringIdType result = msTable.insert(item).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            assertEquals(testId, item.Id);
            assertEquals("Hey", item.String);
        }
    }

    public void testInsertWithStringIdTypeAndNonStringIdResponseContent() throws Throwable {
        Object[] testIdData = IdTestData.concat(IdTestData.convert(IdTestData.concat(IdTestData.ValidIntIds, IdTestData.InvalidIntIds)),
                IdTestData.NonStringNonIntValidJsonIds);

        for (Object testId : testIdData) {
            insertWithStringIdTypeAndNonStringIdResponseContent(testId);
        }
    }

    private void insertWithStringIdTypeAndNonStringIdResponseContent(Object testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        String stringTestId = testId.toString().toLowerCase(Locale.getDefault());
        final String responseContent = "{\"id\":" + stringTestId + ",\"String\":\"Hey\"}";

        final StringIdType item = new StringIdType();
        item.Id = "an id";
        item.String = "what?";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<StringIdType> msTable = client.getTable(tableName, StringIdType.class);

        try {
            // Call the insert method
            StringIdType result = msTable.insert(item).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            assertEquals(stringTestId, item.Id);
            assertEquals("Hey", item.String);
        }
    }

	/*
	 * public void testInsertWithStringIdTypeAndNullIdResponseContent() throws
	 * Throwable {
	 * 
	 * // Container to store callback's results and do the asserts. final
	 * ResultsContainer container = new ResultsContainer();
	 * 
	 * final String tableName = "MyTableName";
	 * 
	 * final String responseContent = "{\"id\":null,\"String\":\"Hey\"}";
	 * 
	 * final StringIdType item = new StringIdType(); item.Id = "an id";
	 * item.String = "what?";
	 * 
	 * runTestOnUiThread(new Runnable() {
	 * 
	 * @Override public void run() { MobileServiceClient client = null;
	 * 
	 * try { client = new MobileServiceClient(appUrl, appKey,
	 * getInstrumentation().getTargetContext()); } catch (MalformedURLException
	 * e) { e.printStackTrace(); }
	 * 
	 * // Add a filter to handle the request and create a new json // object
	 * with an id defined client =
	 * client.withFilter(getTestFilter(responseContent));
	 * 
	 * // Create get the MobileService table MobileServiceTable<StringIdType>
	 * msTable = client.getTable(tableName, StringIdType.class);
	 * 
	 * // Call the insert method msTable.insert(item, new
	 * TableOperationCallback<StringIdType>() {
	 * 
	 * @Override public void onCompleted(StringIdType entity, Exception
	 * exception, ServiceFilterResponse response) { if (exception != null) {
	 * container.setException(exception); }
	 * 
	 * latch.countDown(); } }); } });
	 * 
	 * latch.await();
	 * 
	 * // Asserts Exception exception = container.getException();
	 * 
	 * if (exception != null) { fail(exception.getMessage()); } else {
	 * assertEquals("an id", item.Id); assertEquals("Hey", item.String); } }
	 */

    public void testInsertWithStringIdTypeAndNoIdResponseContent() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"String\":\"Hey\"}";

        final StringIdType item = new StringIdType();
        item.Id = "an id";
        item.String = "what?";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<StringIdType> msTable = client.getTable(tableName, StringIdType.class);

        try {
            // Call the insert method
            StringIdType result = msTable.insert(item).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            assertEquals("an id", item.Id);
            assertEquals("Hey", item.String);
        }
    }

    public void testInsertWithStringIdTypeAndStringIdItem() throws Throwable {
        String[] testIdData = IdTestData.ValidStringIds;

        for (String testId : testIdData) {
            insertWithStringIdTypeAndStringIdItem(testId);
        }
    }

    private void insertWithStringIdTypeAndStringIdItem(final String testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        String jsonTestId = testId.replace("\\", "\\\\").replace("\"", "\\\"");
        final String responseContent = "{\"id\":\"" + jsonTestId + "\",\"String\":\"Hey\"}";

        final StringIdType item = new StringIdType();
        item.Id = testId;
        item.String = "what?";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<StringIdType> msTable = client.getTable(tableName, StringIdType.class);

        try {
            // Call the insert method
            StringIdType result = msTable.insert(item).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            assertEquals(testId, item.Id);
            assertEquals("Hey", item.String);
        }
    }

    public void testInsertWithStringIdTypeAndEmptyStringIdItem() throws Throwable {
        String[] testIdData = IdTestData.EmptyStringIds;

        for (String testId : testIdData) {
            insertWithStringIdTypeAndEmptyStringIdItem(testId);
        }
    }

    private void insertWithStringIdTypeAndEmptyStringIdItem(final String testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"id\":\"an id\",\"String\":\"Hey\"}";

        final StringIdType item = new StringIdType();
        item.Id = testId;
        item.String = "what?";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<StringIdType> msTable = client.getTable(tableName, StringIdType.class);

        try {
            // Call the insert method
            StringIdType result = msTable.insert(item).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            assertEquals("an id", item.Id);
            assertEquals("Hey", item.String);
        }
    }

    public void testInsertWithStringIdTypeAndNullIdItem() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"id\":\"an id\",\"String\":\"Hey\"}";

        final StringIdType item = new StringIdType();
        item.Id = null;
        item.String = "what?";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<StringIdType> msTable = client.getTable(tableName, StringIdType.class);

        try {
            // Call the insert method
            StringIdType result = msTable.insert(item).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            assertEquals("an id", item.Id);
            assertEquals("Hey", item.String);
        }
    }

    public void testInsertWithStringIdTypeAndInvalidStringIdItem() throws Throwable {
        String[] testIdData = IdTestData.InvalidStringIds;

        for (String testId : testIdData) {
            insertWithStringIdTypeAndInvalidStringIdItem(testId);
        }
    }

    private void insertWithStringIdTypeAndInvalidStringIdItem(final String testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"id\":\"an id\",\"String\":\"Hey\"}";

        final StringIdType item = new StringIdType();
        item.Id = testId;
        item.String = "what?";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<StringIdType> msTable = client.getTable(tableName, StringIdType.class);

        try {
            // Call the insert method
            StringIdType result = msTable.insert(item).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception == null || !(exception instanceof IllegalArgumentException)) {
            fail("Expected Exception IllegalArgumentException");
        }
    }

    // Integer Id Type

    public void testInsertWithIntIdTypeAndIntIdResponseContent() throws Throwable {
        long[] testIdData = IdTestData.concat(IdTestData.ValidIntIds, IdTestData.InvalidIntIds);

        for (long testId : testIdData) {
            insertWithIntIdTypeAndIntIdResponseContent(testId);
        }
    }

    private void insertWithIntIdTypeAndIntIdResponseContent(long testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        String stringTestId = String.valueOf(testId);
        final String responseContent = "{\"id\":" + stringTestId + ",\"String\":\"Hey\"}";

        final LongIdType item = new LongIdType();
        item.Id = 0;
        item.String = "what?";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<LongIdType> msTable = client.getTable(tableName, LongIdType.class);

        try {
            // Call the insert method
            LongIdType result = msTable.insert(item).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            assertEquals(testId, item.Id);
            assertEquals("Hey", item.String);
        }
    }

	/*
	 * public void testInsertWithIntIdTypeAndIntParseableIdResponseContent()
	 * throws Throwable { Object[] testIdData =
	 * IdTestData.NonStringNonIntValidJsonIds;
	 * 
	 * for (Object testId : testIdData) {
	 * insertWithIntIdTypeAndIntParseableIdResponseContent(testId); } }
	 * 
	 * private void insertWithIntIdTypeAndIntParseableIdResponseContent(Object
	 * testId) throws Throwable { final CountDownLatch latch = new
	 * CountDownLatch(1);
	 * 
	 * // Container to store callback's results and do the asserts. final
	 * ResultsContainer container = new ResultsContainer();
	 * 
	 * final String tableName = "MyTableName";
	 * 
	 * String stringTestId = testId.toString().toLowerCase(Locale.getDefault());
	 * final String responseContent = "{\"id\":" + stringTestId +
	 * ",\"String\":\"Hey\"}";
	 * 
	 * final LongIdType item = new LongIdType(); item.Id = 0; item.String =
	 * "what?";
	 * 
	 * runTestOnUiThread(new Runnable() {
	 * 
	 * @Override public void run() { MobileServiceClient client = null;
	 * 
	 * try { client = new MobileServiceClient(appUrl, appKey,
	 * getInstrumentation().getTargetContext()); } catch (MalformedURLException
	 * e) { e.printStackTrace(); }
	 * 
	 * // Add a filter to handle the request and create a new json // object
	 * with an id defined client =
	 * client.withFilter(getTestFilter(responseContent));
	 * 
	 * // Create get the MobileService table MobileServiceTable<LongIdType>
	 * msTable = client.getTable(tableName, LongIdType.class);
	 * 
	 * // Call the insert method msTable.insert(item, new
	 * TableOperationCallback<LongIdType>() {
	 * 
	 * @Override public void onCompleted(LongIdType entity, Exception exception,
	 * ServiceFilterResponse response) { if (exception != null) {
	 * container.setException(exception); }
	 * 
	 * latch.countDown(); } }); } });
	 * 
	 * latch.await();
	 * 
	 * // Asserts Exception exception = container.getException();
	 * 
	 * if (exception != null) { fail(exception.getMessage()); } else {
	 * assertEquals(Long.valueOf(stringTestId), Long.valueOf(item.Id));
	 * assertEquals("Hey", item.String); } }
	 */

    public void testInsertWithIntIdTypeAndNullIdResponseContent() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"id\":null,\"String\":\"Hey\"}";

        final LongIdType item = new LongIdType();
        item.Id = 0;
        item.String = "what?";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<LongIdType> msTable = client.getTable(tableName, LongIdType.class);

        try {
            // Call the insert method
            LongIdType result = msTable.insert(item).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            assertEquals(0L, item.Id);
            assertEquals("Hey", item.String);
        }
    }

    public void testInsertWithIntIdTypeAndNoIdResponseContent() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"String\":\"Hey\"}";

        final LongIdType item = new LongIdType();
        item.Id = 0;
        item.String = "what?";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<LongIdType> msTable = client.getTable(tableName, LongIdType.class);

        try {
            // Call the insert method
            LongIdType result = msTable.insert(item).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            assertEquals(0L, item.Id);
            assertEquals("Hey", item.String);
        }
    }

    public void testInsertWithIntIdTypeAndStringIdResponseContent() throws Throwable {
        String[] testIdData = IdTestData.concat(IdTestData.concat(IdTestData.ValidStringIds, IdTestData.EmptyStringIds), IdTestData.InvalidStringIds);

        for (String testId : testIdData) {
            insertWithIntIdTypeAndStringIdResponseContent(testId);
        }
    }

    private void insertWithIntIdTypeAndStringIdResponseContent(String testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        String jsonTestId = testId.replace("\\", "\\\\").replace("\"", "\\\"");
        final String responseContent = "{\"id\":\"" + jsonTestId + "\",\"String\":\"Hey\"}";

        final LongIdType item = new LongIdType();
        item.Id = 0;
        item.String = "what?";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<LongIdType> msTable = client.getTable(tableName, LongIdType.class);

        try {
            // Call the insert method
            LongIdType result = msTable.insert(item).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception == null || !(exception instanceof JsonSyntaxException) || !(exception.getCause() instanceof NumberFormatException)) {
            fail("Expected Exception IllegalArgumentException");
        }
    }

    public void testInsertWithIntIdTypeAndIntIdItem() throws Throwable {
        long[] testIdData = IdTestData.ValidIntIds;

        for (long testId : testIdData) {
            insertWithIntIdTypeAndIntIdItem(testId);
        }
    }

    private void insertWithIntIdTypeAndIntIdItem(final long testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"id\":5,\"String\":\"Hey\"}";

        final LongIdType item = new LongIdType();
        item.Id = testId;
        item.String = "what?";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<LongIdType> msTable = client.getTable(tableName, LongIdType.class);

        try {
            // Call the insert method
            LongIdType result = msTable.insert(item).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception == null || !(exception instanceof IllegalArgumentException)) {
            fail("Expected Exception IllegalArgumentException");
        }
    }

    // Update Tests

    // String Id Type

    public void testUpdateWithStringIdTypeAndStringIdResponseContent() throws Throwable {
        String[] testIdData = IdTestData.concat(IdTestData.concat(IdTestData.ValidStringIds, IdTestData.EmptyStringIds), IdTestData.InvalidStringIds);

        for (String testId : testIdData) {
            updateWithStringIdTypeAndStringIdResponseContent(testId);
        }
    }

    private void updateWithStringIdTypeAndStringIdResponseContent(String testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        String jsonTestId = testId.replace("\\", "\\\\").replace("\"", "\\\"");
        final String responseContent = "{\"id\":\"" + jsonTestId + "\",\"String\":\"Hey\"}";

        final StringIdType item = new StringIdType();
        item.Id = "an id";
        item.String = "what?";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<StringIdType> msTable = client.getTable(tableName, StringIdType.class);

        try {
            // Call the update method
            StringIdType result = msTable.update(item).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            assertEquals(testId, item.Id);
            assertEquals("Hey", item.String);
        }
    }

    public void testUpdateWithStringIdTypeAndNonStringIdResponseContent() throws Throwable {
        Object[] testIdData = IdTestData.concat(IdTestData.convert(IdTestData.concat(IdTestData.ValidIntIds, IdTestData.InvalidIntIds)),
                IdTestData.NonStringNonIntValidJsonIds);

        for (Object testId : testIdData) {
            updateWithStringIdTypeAndNonStringIdResponseContent(testId);
        }
    }

    private void updateWithStringIdTypeAndNonStringIdResponseContent(Object testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        String stringTestId = testId.toString().toLowerCase(Locale.getDefault());
        final String responseContent = "{\"id\":" + stringTestId + ",\"String\":\"Hey\"}";

        final StringIdType item = new StringIdType();
        item.Id = "an id";
        item.String = "what?";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<StringIdType> msTable = client.getTable(tableName, StringIdType.class);

        try {
            // Call the update method
            StringIdType result = msTable.update(item).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            assertEquals(stringTestId, item.Id);
            assertEquals("Hey", item.String);
        }
    }

	/*
	 * public void testUpdateWithStringIdTypeAndNullIdResponseContent() throws
	 * Throwable {
	 * 
	 * // Container to store callback's results and do the asserts. final
	 * ResultsContainer container = new ResultsContainer();
	 * 
	 * final String tableName = "MyTableName";
	 * 
	 * final String responseContent = "{\"id\":null,\"String\":\"Hey\"}";
	 * 
	 * final StringIdType item = new StringIdType(); item.Id = "an id";
	 * item.String = "what?";
	 * 
	 * runTestOnUiThread(new Runnable() {
	 * 
	 * @Override public void run() { MobileServiceClient client = null;
	 * 
	 * try { client = new MobileServiceClient(appUrl, appKey,
	 * getInstrumentation().getTargetContext()); } catch (MalformedURLException
	 * e) { e.printStackTrace(); }
	 * 
	 * // Add a filter to handle the request and create a new json // object
	 * with an id defined client =
	 * client.withFilter(getTestFilter(responseContent));
	 * 
	 * // Create get the MobileService table MobileServiceTable<StringIdType>
	 * msTable = client.getTable(tableName, StringIdType.class);
	 * 
	 * // Call the update method msTable.update(item, new
	 * TableOperationCallback<StringIdType>() {
	 * 
	 * @Override public void onCompleted(StringIdType entity, Exception
	 * exception, ServiceFilterResponse response) { if (exception != null) {
	 * container.setException(exception); }
	 * 
	 * latch.countDown(); } }); } });
	 * 
	 * latch.await();
	 * 
	 * // Asserts Exception exception = container.getException();
	 * 
	 * if (exception != null) { fail(exception.getMessage()); } else {
	 * assertEquals("an id", item.Id); assertEquals("Hey", item.String); } }
	 */

    public void testUpdateWithStringIdTypeAndNoIdResponseContent() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"String\":\"Hey\"}";

        final StringIdType item = new StringIdType();
        item.Id = "an id";
        item.String = "what?";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<StringIdType> msTable = client.getTable(tableName, StringIdType.class);

        try {
            // Call the update method
            StringIdType result = msTable.update(item).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            assertEquals("an id", item.Id);
            assertEquals("Hey", item.String);
        }
    }

    public void testUpdateWithStringIdTypeAndStringIdItem() throws Throwable {
        String[] testIdData = IdTestData.ValidStringIds;

        for (String testId : testIdData) {
            updateWithStringIdTypeAndStringIdItem(testId);
        }
    }

    private void updateWithStringIdTypeAndStringIdItem(final String testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        String jsonTestId = testId.replace("\\", "\\\\").replace("\"", "\\\"");
        final String responseContent = "{\"id\":\"" + jsonTestId + "\",\"String\":\"Hey\"}";

        final StringIdType item = new StringIdType();
        item.Id = testId;
        item.String = "what?";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<StringIdType> msTable = client.getTable(tableName, StringIdType.class);

        try {
            // Call the update method
            StringIdType result = msTable.update(item).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            assertEquals(testId, item.Id);
            assertEquals("Hey", item.String);
        }
    }

    public void testUpdateWithStringIdTypeAndEmptyStringIdItem() throws Throwable {
        String[] testIdData = IdTestData.EmptyStringIds;

        for (String testId : testIdData) {
            updateWithStringIdTypeAndEmptyStringIdItem(testId);
        }
    }

    private void updateWithStringIdTypeAndEmptyStringIdItem(final String testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"id\":\"an id\",\"String\":\"Hey\"}";

        final StringIdType item = new StringIdType();
        item.Id = testId;
        item.String = "what?";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<StringIdType> msTable = client.getTable(tableName, StringIdType.class);

        try {
            // Call the update method
            StringIdType result = msTable.update(item).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception == null || !(exception instanceof IllegalArgumentException)) {
            fail("Expected Exception IllegalArgumentException");
        }
    }

    public void testUpdateWithStringIdTypeAndNullIdItem() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"id\":\"an id\",\"String\":\"Hey\"}";

        final StringIdType item = new StringIdType();
        item.Id = null;
        item.String = "what?";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<StringIdType> msTable = client.getTable(tableName, StringIdType.class);

        try {
            // Call the update method
            StringIdType result = msTable.update(item).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception == null || !(exception instanceof IllegalArgumentException)) {
            fail("Expected Exception IllegalArgumentException");
        }
    }

    public void testUpdateWithStringIdTypeAndInvalidStringIdItem() throws Throwable {
        String[] testIdData = IdTestData.InvalidStringIds;

        for (String testId : testIdData) {
            updateWithStringIdTypeAndInvalidStringIdItem(testId);
        }
    }

    private void updateWithStringIdTypeAndInvalidStringIdItem(final String testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"id\":\"an id\",\"String\":\"Hey\"}";

        final StringIdType item = new StringIdType();
        item.Id = testId;
        item.String = "what?";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<StringIdType> msTable = client.getTable(tableName, StringIdType.class);

        try {
            // Call the update method
            StringIdType result = msTable.update(item).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception == null || !(exception instanceof IllegalArgumentException)) {
            fail("Expected Exception IllegalArgumentException");
        }
    }

    // Integer Id Type

    public void testUpdateWithIntIdTypeAndIntIdResponseContent() throws Throwable {
        long[] testIdData = IdTestData.concat(IdTestData.ValidIntIds, IdTestData.InvalidIntIds);

        for (long testId : testIdData) {
            updateWithIntIdTypeAndIntIdResponseContent(testId);
        }
    }

    private void updateWithIntIdTypeAndIntIdResponseContent(long testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        String stringTestId = String.valueOf(testId);
        final String responseContent = "{\"id\":" + stringTestId + ",\"String\":\"Hey\"}";

        final LongIdType item = new LongIdType();
        item.Id = 12;
        item.String = "what?";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<LongIdType> msTable = client.getTable(tableName, LongIdType.class);

        try {
            // Call the update method
            LongIdType result = msTable.update(item).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            assertEquals(testId, item.Id);
            assertEquals("Hey", item.String);
        }
    }

	/*
	 * public void testUpdateWithIntIdTypeAndIntParseableIdResponseContent()
	 * throws Throwable { Object[] testIdData =
	 * IdTestData.NonStringNonIntValidJsonIds;
	 * 
	 * for (Object testId : testIdData) {
	 * updateWithIntIdTypeAndIntParseableIdResponseContent(testId); } }
	 * 
	 * private void updateWithIntIdTypeAndIntParseableIdResponseContent(Object
	 * testId) throws Throwable { final CountDownLatch latch = new
	 * CountDownLatch(1);
	 * 
	 * // Container to store callback's results and do the asserts. final
	 * ResultsContainer container = new ResultsContainer();
	 * 
	 * final String tableName = "MyTableName";
	 * 
	 * String stringTestId = testId.toString().toLowerCase(Locale.getDefault());
	 * final String responseContent = "{\"id\":" + stringTestId +
	 * ",\"String\":\"Hey\"}";
	 * 
	 * final LongIdType item = new LongIdType(); item.Id = 12; item.String =
	 * "what?";
	 * 
	 * runTestOnUiThread(new Runnable() {
	 * 
	 * @Override public void run() { MobileServiceClient client = null;
	 * 
	 * try { client = new MobileServiceClient(appUrl, appKey,
	 * getInstrumentation().getTargetContext()); } catch (MalformedURLException
	 * e) { e.printStackTrace(); }
	 * 
	 * // Add a filter to handle the request and create a new json // object
	 * with an id defined client =
	 * client.withFilter(getTestFilter(responseContent));
	 * 
	 * // Create get the MobileService table MobileServiceTable<LongIdType>
	 * msTable = client.getTable(tableName, LongIdType.class);
	 * 
	 * // Call the update method msTable.update(item, new
	 * TableOperationCallback<LongIdType>() {
	 * 
	 * @Override public void onCompleted(LongIdType entity, Exception exception,
	 * ServiceFilterResponse response) { if (exception != null) {
	 * container.setException(exception); }
	 * 
	 * latch.countDown(); } }); } });
	 * 
	 * latch.await();
	 * 
	 * // Asserts Exception exception = container.getException();
	 * 
	 * if (exception != null) { fail(exception.getMessage()); } else {
	 * assertEquals(Long.valueOf(stringTestId), Long.valueOf(item.Id));
	 * assertEquals("Hey", item.String); } }
	 */

	/*
	 * public void testUpdateWithIntIdTypeAndNullIdResponseContent() throws
	 * Throwable {
	 * 
	 * // Container to store callback's results and do the asserts. final
	 * ResultsContainer container = new ResultsContainer();
	 * 
	 * final String tableName = "MyTableName";
	 * 
	 * final String responseContent = "{\"id\":null,\"String\":\"Hey\"}";
	 * 
	 * final LongIdType item = new LongIdType(); item.Id = 12; item.String =
	 * "what?";
	 * 
	 * runTestOnUiThread(new Runnable() {
	 * 
	 * @Override public void run() { MobileServiceClient client = null;
	 * 
	 * try { client = new MobileServiceClient(appUrl, appKey,
	 * getInstrumentation().getTargetContext()); } catch (MalformedURLException
	 * e) { e.printStackTrace(); }
	 * 
	 * // Add a filter to handle the request and create a new json // object
	 * with an id defined client =
	 * client.withFilter(getTestFilter(responseContent));
	 * 
	 * // Create get the MobileService table MobileServiceTable<LongIdType>
	 * msTable = client.getTable(tableName, LongIdType.class);
	 * 
	 * // Call the update method msTable.update(item, new
	 * TableOperationCallback<LongIdType>() {
	 * 
	 * @Override public void onCompleted(LongIdType entity, Exception exception,
	 * ServiceFilterResponse response) { if (exception != null) {
	 * container.setException(exception); }
	 * 
	 * latch.countDown(); } }); } });
	 * 
	 * latch.await();
	 * 
	 * // Asserts Exception exception = container.getException();
	 * 
	 * if (exception != null) { fail(exception.getMessage()); } else {
	 * assertEquals(12L, item.Id); assertEquals("Hey", item.String); } }
	 */

    public void testUpdateWithIntIdTypeAndNoIdResponseContent() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"String\":\"Hey\"}";

        final LongIdType item = new LongIdType();
        item.Id = 12;
        item.String = "what?";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<LongIdType> msTable = client.getTable(tableName, LongIdType.class);

        try {
            // Call the update method
            LongIdType result = msTable.update(item).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            assertEquals(12L, item.Id);
            assertEquals("Hey", item.String);
        }
    }

    public void testUpdateWithIntIdTypeAndStringIdResponseContent() throws Throwable {
        String[] testIdData = IdTestData.concat(IdTestData.concat(IdTestData.ValidStringIds, IdTestData.EmptyStringIds), IdTestData.InvalidStringIds);

        for (String testId : testIdData) {
            updateWithIntIdTypeAndStringIdResponseContent(testId);
        }
    }

    private void updateWithIntIdTypeAndStringIdResponseContent(String testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        String jsonTestId = testId.replace("\\", "\\\\").replace("\"", "\\\"");
        final String responseContent = "{\"id\":\"" + jsonTestId + "\",\"String\":\"Hey\"}";

        final LongIdType item = new LongIdType();
        item.Id = 12;
        item.String = "what?";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<LongIdType> msTable = client.getTable(tableName, LongIdType.class);

        try {
            // Call the update method
            LongIdType result = msTable.update(item).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception == null || !(exception instanceof JsonSyntaxException) || !(exception.getCause() instanceof NumberFormatException)) {
            fail("Expected Exception IllegalArgumentException");
        }
    }

    public void testUpdateWithIntIdTypeAndIntIdItem() throws Throwable {
        long[] testIdData = IdTestData.ValidIntIds;

        for (long testId : testIdData) {
            updateWithIntIdTypeAndIntIdItem(testId);
        }
    }

    private void updateWithIntIdTypeAndIntIdItem(final long testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        String stringTestId = String.valueOf(testId);
        final String responseContent = "{\"id\":" + stringTestId + ",\"String\":\"Hey\"}";

        final LongIdType item = new LongIdType();
        item.Id = testId;
        item.String = "what?";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<LongIdType> msTable = client.getTable(tableName, LongIdType.class);

        try {
            // Call the update method
            LongIdType result = msTable.update(item).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            assertEquals(testId, item.Id);
            assertEquals("Hey", item.String);
        }
    }

    public void testUpdateWithIntIdTypeAndZeroIdItem() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"id\":5,\"String\":\"Hey\"}";

        final LongIdType item = new LongIdType();
        item.Id = 0;
        item.String = "what?";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<LongIdType> msTable = client.getTable(tableName, LongIdType.class);

        try {
            // Call the update method
            LongIdType result = msTable.update(item).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception == null || !(exception instanceof IllegalArgumentException)) {
            fail("Expected Exception IllegalArgumentException");
        }
    }

    public void testUpdateWithIntIdTypeAndInvalidIntIdItem() throws Throwable {
        long[] testIdData = IdTestData.InvalidIntIds;

        for (long testId : testIdData) {
            updateWithIntIdTypeAndInvalidIntIdItem(testId);
        }
    }

    private void updateWithIntIdTypeAndInvalidIntIdItem(final long testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"id\":5,\"String\":\"Hey\"}";

        final LongIdType item = new LongIdType();
        item.Id = testId;
        item.String = "what?";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<LongIdType> msTable = client.getTable(tableName, LongIdType.class);

        try {
            // Call the update method
            LongIdType result = msTable.update(item).get();

            if (result == null) {
                container.setException(new Exception("Expected result"));
            } else {
                container.setCustomResult(result);
            }

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.setException(exception.getCause());
            } else {
                container.setException(exception);
            }
        }

        // Asserts
        Exception exception = container.getException();

        if (exception == null || !(exception instanceof IllegalArgumentException)) {
            fail("Expected Exception IllegalArgumentException");
        }
    }

    // Delete Tests

    // String Id Type

	/*
	 * public void testDeleteWithStringIdTypeAndStringIdItem() throws Throwable
	 * { String[] testIdData = IdTestData.ValidStringIds;
	 * 
	 * for (String testId : testIdData) {
	 * deleteWithStringIdTypeAndStringIdItem(testId); } }
	 * 
	 * private void deleteWithStringIdTypeAndStringIdItem(final String testId)
	 * throws Throwable {
	 * 
	 * // Container to store callback's results and do the asserts. final
	 * ResultsContainer container = new ResultsContainer();
	 * 
	 * final String tableName = "MyTableName";
	 * 
	 * String jsonTestId = testId.replace("\\", "\\\\").replace("\"", "\\\"");
	 * final String responseContent = "{\"id\":\"" + jsonTestId +
	 * "\",\"String\":\"Hey\"}";
	 * 
	 * final StringIdType item = new StringIdType(); item.Id = testId;
	 * item.String = "what?";
	 * 
	 * runTestOnUiThread(new Runnable() {
	 * 
	 * @Override public void run() { MobileServiceClient client = null;
	 * 
	 * try { client = new MobileServiceClient(appUrl, appKey,
	 * getInstrumentation().getTargetContext()); } catch (MalformedURLException
	 * e) { e.printStackTrace(); }
	 * 
	 * // Add a filter to handle the request and create a new json // object
	 * with an id defined client =
	 * client.withFilter(getTestFilter(responseContent));
	 * 
	 * // Create get the MobileService table MobileServiceTable<StringIdType>
	 * msTable = client.getTable(tableName, StringIdType.class);
	 * 
	 * // Call the delete method msTable.delete(item, new TableDeleteCallback()
	 * {
	 * 
	 * @Override public void onCompleted(Exception exception,
	 * ServiceFilterResponse response) { if (exception != null) {
	 * container.setException(exception); }
	 * 
	 * latch.countDown(); } }); } });
	 * 
	 * latch.await();
	 * 
	 * // Asserts Exception exception = container.getException();
	 * 
	 * if (exception != null) { fail(exception.getMessage()); } else {
	 * assertEquals(null, item.Id); assertEquals("what?", item.String); } }
	 */

    public void testDeleteWithStringIdTypeAndEmptyStringIdItem() throws Throwable {
        String[] testIdData = IdTestData.EmptyStringIds;

        for (String testId : testIdData) {
            deleteWithStringIdTypeAndEmptyStringIdItem(testId);
        }
    }

    private void deleteWithStringIdTypeAndEmptyStringIdItem(final String testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"id\":\"an id\",\"String\":\"Hey\"}";

        final StringIdType item = new StringIdType();
        item.Id = testId;
        item.String = "what?";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<StringIdType> msTable = client.getTable(tableName, StringIdType.class);

        try {
            // Call the delete method
            msTable.delete(item).get();

        } catch (Exception exception) {
            container.setException(exception);
        }

        // Asserts
        Exception exception = container.getException();

        if (exception == null || !(exception instanceof IllegalArgumentException)) {
            fail("Expected Exception IllegalArgumentException");
        }
    }

    public void testDeleteWithStringIdTypeAndNullIdItem() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"id\":\"an id\",\"String\":\"Hey\"}";

        final StringIdType item = new StringIdType();
        item.Id = null;
        item.String = "what?";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<StringIdType> msTable = client.getTable(tableName, StringIdType.class);

        try {
            // Call the delete method
            msTable.delete(item).get();

        } catch (Exception exception) {
            container.setException(exception);
        }

        // Asserts
        Exception exception = container.getException();

        if (exception == null || !(exception instanceof IllegalArgumentException)) {
            fail("Expected Exception IllegalArgumentException");
        }
    }

    public void testDeleteWithStringIdTypeAndInvalidStringIdItem() throws Throwable {
        String[] testIdData = IdTestData.InvalidStringIds;

        for (String testId : testIdData) {
            deleteWithStringIdTypeAndInvalidStringIdItem(testId);
        }
    }

    private void deleteWithStringIdTypeAndInvalidStringIdItem(final String testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"id\":\"an id\",\"String\":\"Hey\"}";

        final StringIdType item = new StringIdType();
        item.Id = testId;
        item.String = "what?";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<StringIdType> msTable = client.getTable(tableName, StringIdType.class);

        try {
            // Call the delete method
            msTable.delete(item).get();

        } catch (Exception exception) {
            container.setException(exception);
        }

        // Asserts
        Exception exception = container.getException();

        if (exception == null || !(exception instanceof IllegalArgumentException)) {
            fail("Expected Exception IllegalArgumentException");
        }
    }

    // Integer Id Type

	/*
	 * public void testDeleteWithIntIdTypeAndIntIdItem() throws Throwable {
	 * long[] testIdData = IdTestData.ValidIntIds;
	 * 
	 * for (long testId : testIdData) { deleteWithIntIdTypeAndIntIdItem(testId);
	 * } }
	 * 
	 * private void deleteWithIntIdTypeAndIntIdItem(final long testId) throws
	 * Throwable {
	 * 
	 * // Container to store callback's results and do the asserts. final
	 * ResultsContainer container = new ResultsContainer();
	 * 
	 * final String tableName = "MyTableName";
	 * 
	 * String stringTestId = String.valueOf(testId); final String
	 * responseContent = "{\"id\":" + stringTestId + ",\"String\":\"Hey\"}";
	 * 
	 * final LongIdType item = new LongIdType(); item.Id = testId; item.String =
	 * "what?";
	 * 
	 * runTestOnUiThread(new Runnable() {
	 * 
	 * @Override public void run() { MobileServiceClient client = null;
	 * 
	 * try { client = new MobileServiceClient(appUrl, appKey,
	 * getInstrumentation().getTargetContext()); } catch (MalformedURLException
	 * e) { e.printStackTrace(); }
	 * 
	 * // Add a filter to handle the request and create a new json // object
	 * with an id defined client =
	 * client.withFilter(getTestFilter(responseContent));
	 * 
	 * // Create get the MobileService table MobileServiceTable<LongIdType>
	 * msTable = client.getTable(tableName, LongIdType.class);
	 * 
	 * // Call the delete method msTable.delete(item, new TableDeleteCallback()
	 * {
	 * 
	 * @Override public void onCompleted(Exception exception,
	 * ServiceFilterResponse response) { if (exception != null) {
	 * container.setException(exception); }
	 * 
	 * latch.countDown(); } }); } });
	 * 
	 * latch.await();
	 * 
	 * // Asserts Exception exception = container.getException();
	 * 
	 * if (exception != null) { fail(exception.getMessage()); } else {
	 * assertEquals(0L, item.Id); assertEquals("Hey", item.String); } }
	 */

    public void testDeleteWithIntIdTypeAndZeroIdItem() throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"id\":5,\"String\":\"Hey\"}";

        final LongIdType item = new LongIdType();
        item.Id = 0;
        item.String = "what?";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<LongIdType> msTable = client.getTable(tableName, LongIdType.class);

        try {
            // Call the delete method
            msTable.delete(item).get();

        } catch (Exception exception) {
            container.setException(exception);
        }

        // Asserts
        Exception exception = container.getException();

        if (exception == null || !(exception instanceof IllegalArgumentException)) {
            fail("Expected Exception IllegalArgumentException");
        }
    }

    public void testDeleteWithIntIdTypeAndInvalidIntIdItem() throws Throwable {
        long[] testIdData = IdTestData.InvalidIntIds;

        for (long testId : testIdData) {
            deleteWithIntIdTypeAndInvalidIntIdItem(testId);
        }
    }

    private void deleteWithIntIdTypeAndInvalidIntIdItem(final long testId) throws Throwable {

        // Container to store callback's results and do the asserts.
        final ResultsContainer container = new ResultsContainer();

        final String tableName = "MyTableName";

        final String responseContent = "{\"id\":5,\"String\":\"Hey\"}";

        final LongIdType item = new LongIdType();
        item.Id = testId;
        item.String = "what?";

        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a filter to handle the request and create a new json
        // object with an id defined
        client = client.withFilter(getTestFilter(responseContent));

        // Create get the MobileService table
        MobileServiceTable<LongIdType> msTable = client.getTable(tableName, LongIdType.class);

        try {
            // Call the delete method
            msTable.delete(item).get();

        } catch (Exception exception) {
            container.setException(exception);
        }

        // Asserts
        Exception exception = container.getException();

        if (exception == null || !(exception instanceof IllegalArgumentException)) {
            fail("Expected Exception IllegalArgumentException");
        }
    }

    // Test Filter

    private ServiceFilter getTestFilter(String content) {
        return getTestFilter(200, content);
    }

    private ServiceFilter getTestFilter(final int statusCode, final String content) {
        return new ServiceFilter() {

            @Override
            public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

                // Create a mock response simulating an error
                ServiceFilterResponseMock response = new ServiceFilterResponseMock();
                response.setStatus(new StatusLineMock(statusCode));
                response.setContent(content);

                // create a mock request to replace the existing one
                ServiceFilterRequestMock requestMock = new ServiceFilterRequestMock(response);
                return nextServiceFilterCallback.onNext(requestMock);
            }
        };
    }
}
