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

import com.google.common.base.Function;
import com.google.common.util.concurrent.ListenableFuture;
import com.google.gson.GsonBuilder;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceException;
import com.microsoft.windowsazure.mobileservices.http.NextServiceFilterCallback;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilter;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterRequest;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.framework.filters.ServiceFilterRequestMock;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.framework.filters.ServiceFilterResponseMock;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.framework.filters.StatusLineMock;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.framework.mocks.MobileServiceLocalStoreMock;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.framework.mocks.MobileServiceSyncHandlerMock;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.test.helpers.EncodingUtilities;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.test.types.CustomFunctionTwoParameters;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.test.types.IdPropertyTestClasses.StringIdType;
import com.microsoft.windowsazure.mobileservices.table.MobileServiceExceptionBase;
import com.microsoft.windowsazure.mobileservices.table.query.Query;
import com.microsoft.windowsazure.mobileservices.table.query.QueryOperations;
import com.microsoft.windowsazure.mobileservices.table.query.QueryOrder;
import com.microsoft.windowsazure.mobileservices.table.sync.MobileServiceJsonSyncTable;
import com.microsoft.windowsazure.mobileservices.table.sync.MobileServiceSyncTable;
import com.microsoft.windowsazure.mobileservices.table.sync.operations.TableOperationError;
import com.microsoft.windowsazure.mobileservices.table.sync.push.MobileServicePushFailedException;
import com.microsoft.windowsazure.mobileservices.table.sync.push.MobileServicePushStatus;
import com.microsoft.windowsazure.mobileservices.table.sync.synchandler.SimpleSyncHandler;

import org.apache.http.Header;

import java.io.IOException;
import java.net.MalformedURLException;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Locale;
import java.util.TimeZone;
import java.util.concurrent.ExecutionException;

public class MobileServiceSyncTableTests extends InstrumentationTestCase {
    String appUrl = "";
    String appKey = "";
    GsonBuilder gsonBuilder;
    String OperationQueue = "__operations";
    String SyncErrors = "__errors";

    protected void setUp() throws Exception {
        appUrl = "http://myapp.com/";
        appKey = "qwerty";
        gsonBuilder = new GsonBuilder();
        super.setUp();
    }

    protected void tearDown() throws Exception {
        super.tearDown();
    }

    public void testPushExecutesThePendingOperationsInOrder() throws InterruptedException, ExecutionException, MalformedURLException {
        MobileServiceLocalStoreMock store = new MobileServiceLocalStoreMock();
        ServiceFilterContainer serviceFilterContainer1 = new ServiceFilterContainer();

        MobileServiceClient client1 = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());

        client1 = client1.withFilter(getTestFilter(serviceFilterContainer1, ""));

        client1.getSyncContext().initialize(store, new SimpleSyncHandler()).get();

        MobileServiceJsonSyncTable table = client1.getSyncTable("someTable");

        JsonObject item1 = new JsonObject();
        item1.addProperty("id", "abc");

        JsonObject item2 = new JsonObject();
        item2.addProperty("id", "def");

        table.insert(item1).get();
        table.insert(item2).get();

        assertEquals(serviceFilterContainer1.Requests.size(), 0);

        // create a new service to test that operations are loaded from store
        ServiceFilterContainer serviceFilterContainer2 = new ServiceFilterContainer();

        MobileServiceClient client2 = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());

        client2 = client2.withFilter(getTestFilter(serviceFilterContainer2, "{\"id\":\"abc\",\"String\":\"Hey\"}", "{\"id\":\"def\",\"String\":\"What\"}"));

        client2.getSyncContext().initialize(store, new SimpleSyncHandler()).get();

        assertEquals(serviceFilterContainer2.Requests.size(), 0);
        client2.getSyncContext().push().get();
        assertEquals(serviceFilterContainer2.Requests.size(), 2);

        assertEquals(serviceFilterContainer2.Requests.get(0).Content, item1.toString());
        // Assert.AreEqual(hijack.Requests[0].Headers.GetValues("X-ZUMO-FEATURES").First(),
        // "OL");
        assertEquals(serviceFilterContainer2.Requests.get(1).Content, item2.toString());
        // Assert.AreEqual(hijack.Requests[1].Headers.GetValues("X-ZUMO-FEATURES").First(),
        // "OL");

        // create yet another service to make sure the old items were purged
        // from queue
        ServiceFilterContainer serviceFilterContainer3 = new ServiceFilterContainer();
        MobileServiceClient client3 = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());

        client3 = client3.withFilter(getTestFilter(serviceFilterContainer3, "{\"id\":\"abc\",\"String\":\"Hey\"}"));

        client3.getSyncContext().initialize(store, new SimpleSyncHandler()).get();

        assertEquals(serviceFilterContainer3.Requests.size(), 0);
        client3.getSyncContext().push().get();
        assertEquals(serviceFilterContainer3.Requests.size(), 0);
    }

    public void testPullThrowsWhenPushThrows() throws MalformedURLException, InterruptedException, ExecutionException {
        MobileServiceLocalStoreMock store = new MobileServiceLocalStoreMock();
        ServiceFilterContainer serviceFilterContainer = new ServiceFilterContainer();

        MobileServiceClient client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        client.getSyncContext().initialize(store, new SimpleSyncHandler()).get();

        client = client.withFilter(getTestFilter(serviceFilterContainer, 401, "")); // for
        // push

        client.getSyncContext().initialize(store, new SimpleSyncHandler()).get();

        // insert an item but don't push
        MobileServiceJsonSyncTable table = client.getSyncTable("someTable");

        JsonObject jsonObject = new JsonObject();
        jsonObject.addProperty("id", "abc");

        table.insert(jsonObject).get();

        assertEquals(store.Tables.get(table.getName().toLowerCase(Locale.getDefault())).size(), 1); // item
        // is
        // inserted
        // this should trigger a push
        try {
            table.pull(null).get();
            fail("MobileServicePushFailedException expected");
            return;
        } catch (Exception ex) {
            if (ex.getCause() instanceof MobileServicePushFailedException) {
                MobileServicePushFailedException mobileServicePushFailedException = (MobileServicePushFailedException) ex.getCause();

                assertEquals(mobileServicePushFailedException.getPushCompletionResult().getOperationErrors().size(), 0);
                assertEquals(serviceFilterContainer.Requests.size(), 1);
            } else {
                fail("MobileServicePushFailedException expected");
            }
        }
    }

    public void testPullDoesNotPurgeWhenItemIsMissing() throws InterruptedException, ExecutionException, MalformedURLException {

        MobileServiceLocalStoreMock store = new MobileServiceLocalStoreMock();
        ServiceFilterContainer serviceFilterContainer = new ServiceFilterContainer();

        MobileServiceClient client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());

        client = client.withFilter(getTestFilter(serviceFilterContainer, false, "{\"id\":\"abc\",\"String\":\"Hey\"}", "[{\"id\":\"def\",\"String\":\"World\"}]"));

        client.getSyncContext().initialize(store, new SimpleSyncHandler()).get();

        MobileServiceJsonSyncTable table = client.getSyncTable("someTable");

        JsonObject jsonObject = new JsonObject();
        jsonObject.addProperty("id", "abc");

        table.insert(jsonObject).get(); // insert an item

        client.getSyncContext().push().get(); // push to clear the queue

        // now pull
        table.pull(null).get();

        assertEquals(store.Tables.get(table.getName().toLowerCase(Locale.getDefault())).size(), 2); // 1
        // from
        // remote
        // and
        // 1
        // from
        // local
        assertEquals(serviceFilterContainer.Requests.size(), 3);
    }

    public void testPullDoesNotTriggerPushWhenThereIsNoOperationInTable() throws InterruptedException, ExecutionException, MalformedURLException {
        MobileServiceLocalStoreMock store = new MobileServiceLocalStoreMock();
        ServiceFilterContainer serviceFilterContainer = new ServiceFilterContainer();

        store.ReadResponses.add("[{\"id\":\"abc\",\"String\":\"Hey\"},{\"id\":\"def\",\"String\":\"World\"}]"); // for
        // pull

        MobileServiceClient client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());

        client = client.withFilter(getTestFilter(serviceFilterContainer, false, "{\"id\":\"abc\",\"String\":\"Hey\"}",
                "[{\"id\":\"abc\",\"String\":\"Hey\"},{\"id\":\"def\",\"String\":\"World\"}]"));

        client.getSyncContext().initialize(store, new SimpleSyncHandler()).get();

        // insert item in pull table
        MobileServiceJsonSyncTable table1 = client.getSyncTable("someTable");

        JsonObject jsonObject = new JsonObject();
        jsonObject.addProperty("id", "abc");

        table1.insert(jsonObject).get(); // insert an item

        // but push to clear the queue
        client.getSyncContext().push().get();

        assertEquals(store.Tables.get(table1.getName().toLowerCase(Locale.getDefault())).size(), 1); // item
        // is
        // inserted
        assertEquals(serviceFilterContainer.Requests.size(), 1); // first push

        // then insert item in other table
        MobileServiceSyncTable<StringIdType> table2 = client.getSyncTable(StringIdType.class);

        StringIdType item = new StringIdType();

        item.Id = "an id";
        item.String = "what?";

        table2.insert(item).get();

        table1.pull(null).get();

        assertEquals(store.Tables.get(table1.getName().toLowerCase(Locale.getDefault())).size(), 2); // table
        // should
        // contain
        // 2
        // pulled
        // items
        assertEquals(serviceFilterContainer.Requests.size(), 3); // 1 for push
        // and 1 for
        // pull
        assertEquals(store.Tables.get(table2.getName().toLowerCase(Locale.getDefault())).size(), 1); // this
        // table
        // should
        // not
        // be
        // touched
    }

    public void testPullTriggersPushWhenThereIsOperationInTable() throws InterruptedException, ExecutionException, MalformedURLException {
        MobileServiceLocalStoreMock store = new MobileServiceLocalStoreMock();
        ServiceFilterContainer serviceFilterContainer = new ServiceFilterContainer();

        store.ReadResponses.add("[{\"id\":\"abc\",\"String\":\"Hey\"},{\"id\":\"def\",\"String\":\"World\"}]"); // for
        // pull

        MobileServiceClient client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());

        client = client.withFilter(getTestFilter(serviceFilterContainer, false, "{\"id\":\"abc\",\"String\":\"Hey\"}", // for
                // insert
                "[{\"id\":\"abc\",\"String\":\"Hey\"},{\"id\":\"def\",\"String\":\"World\"}]" // remote
                // item
        ));

        client.getSyncContext().initialize(store, new SimpleSyncHandler()).get();

        // insert an item but don't push
        MobileServiceJsonSyncTable table1 = client.getSyncTable("someTable");

        JsonObject jsonObject = new JsonObject();
        jsonObject.addProperty("id", "abc");

        table1.insert(jsonObject).get(); // insert an item

        assertEquals(store.Tables.get(table1.getName().toLowerCase(Locale.getDefault())).size(), 1); // item
        // is
        // inserted

        // this should trigger a push
        table1.pull(null).get();

        assertEquals(serviceFilterContainer.Requests.size(), 3); // 1 for push
        // and 2 for
        // pull
        assertEquals(store.Tables.get(table1.getName().toLowerCase(Locale.getDefault())).size(), 2); // table
        // is
        // populated
    }

    // REVISAR EL TEMA DE LOS HANDLERS Y LAS EXCEPCIONES
    // public void testPullThrowsWhenPushThrows()
    // throws MalformedURLException, InterruptedException,
    // ExecutionException {
    // MobileServiceLocalStoreMock store = new MobileServiceLocalStoreMock();
    //
    // MobileServiceClient client = null;
    //
    // client = new MobileServiceClient(appUrl, appKey, getInstrumentation()
    // .getTargetContext());
    //
    // client = client.withFilter(getTestFilter(404)); // for push
    //
    // client.getSyncContext().initialize(store, new SimpleSyncHandler())
    // .get();
    //
    // // insert an item but don't push
    // MobileServiceJsonSyncTable table = client.getSyncTable("someTable");
    //
    // JsonObject jsonObject = new JsonObject();
    // jsonObject.addProperty("id", "abc");
    //
    // table.insert(jsonObject).get(); // insert an item
    //
    // assertEquals(store.Tables.get(table.getName().toLowerCase()).size(), 1);
    // // item
    // // is
    // // inserted
    //
    // try {
    // table.pull(null).get();
    // } catch (Throwable throwable) {
    //
    // if (throwable instanceof MobileServicePushFailedException) {
    // MobileServicePushFailedException ex = (MobileServicePushFailedException)
    // throwable;
    // assertEquals(ex.getPushCompletionResult().getOperationErrors()
    // .size(), 1);
    // }
    // }
    //
    // // Assert.AreEqual(hijack.Requests.Count, 1); // 1 for push
    // }

    public void testPullSucceds() throws MalformedURLException, InterruptedException, ExecutionException, MobileServiceException {

        MobileServiceLocalStoreMock store = new MobileServiceLocalStoreMock();
        ServiceFilterContainer serviceFilterContainer = new ServiceFilterContainer();

        MobileServiceClient client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());

        SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSS'Z'");
        sdf.setTimeZone(TimeZone.getTimeZone("UTC"));

        String updatedAt = sdf.format(new Date());

        client = client.withFilter(getTestFilter(serviceFilterContainer, false,
                "{\"count\":\"2\",\"results\":[{\"id\":\"abc\",\"String\":\"Hey\",\"__updatedAt\":\"" + updatedAt + "\"},{\"id\":\"def\",\"String\":\"World\",\"__updatedAt\":\"" + updatedAt + "\"}]}"// remote
                // item
        ));

        client.getSyncContext().initialize(store, new SimpleSyncHandler()).get();

        MobileServiceSyncTable<StringIdType> table = client.getSyncTable(StringIdType.class);

        Query query = QueryOperations.tableName(table.getName()).skip(5).top(3).field("String").eq("world").orderBy("Id", QueryOrder.Descending)
                .includeInlineCount().select("String");

        table.pull(query).get();

        assertEquals(
                serviceFilterContainer.Requests.get(0).Url,
                EncodingUtilities
                        .percentEncodeSpaces(
                                "http://myapp.com/tables/stringidtype?$filter=String%20eq%20('world')&$top=3&$skip=5&$orderby=Id%20desc&__includeDeleted=true&__systemproperties=__version,__deleted"));
    }

    public void testPullNoSkipSucceds() throws MalformedURLException, InterruptedException, ExecutionException, MobileServiceException {

        MobileServiceLocalStoreMock store = new MobileServiceLocalStoreMock();
        ServiceFilterContainer serviceFilterContainer = new ServiceFilterContainer();

        MobileServiceClient client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());

        SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSS'Z'");
        sdf.setTimeZone(TimeZone.getTimeZone("UTC"));

        String updatedAt = sdf.format(new Date());

        client = client.withFilter(getTestFilter(serviceFilterContainer, false,
                "{\"count\":\"2\",\"results\":[{\"id\":\"abc\",\"String\":\"Hey\",\"__updatedAt\":\"" + updatedAt + "\"},{\"id\":\"def\",\"String\":\"World\",\"__updatedAt\":\"" + updatedAt + "\"}]}"// remote
                // item
        ));

        client.getSyncContext().initialize(store, new SimpleSyncHandler()).get();

        MobileServiceSyncTable<StringIdType> table = client.getSyncTable(StringIdType.class);

        Query query = QueryOperations.tableName(table.getName()).top(3).field("String").eq("world").orderBy("Id", QueryOrder.Descending)
                .includeInlineCount().select("String");

        table.pull(query).get();

        assertEquals(
                serviceFilterContainer.Requests.get(0).Url,
                EncodingUtilities
                        .percentEncodeSpaces(
                                "http://myapp.com/tables/stringidtype?$filter=String%20eq%20('world')&$top=3&$skip=0&$orderby=Id%20desc&__includeDeleted=true&__systemproperties=__version,__deleted"));
    }

    public void testPullSuccedsNoTopNoOrderBy() throws MalformedURLException, InterruptedException, ExecutionException, MobileServiceException {

        MobileServiceLocalStoreMock store = new MobileServiceLocalStoreMock();
        ServiceFilterContainer serviceFilterContainer = new ServiceFilterContainer();

        MobileServiceClient client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());

        SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSS'Z'");
        sdf.setTimeZone(TimeZone.getTimeZone("UTC"));

        String updatedAt = sdf.format(new Date());

        client = client.withFilter(getTestFilter(serviceFilterContainer, false,
                "{\"count\":\"2\",\"results\":[{\"id\":\"abc\",\"String\":\"Hey\",\"__updatedAt\":\"" + updatedAt + "\"},{\"id\":\"def\",\"String\":\"World\",\"__updatedAt\":\"" + updatedAt + "\"}]}"// remote
                // item
        ));

        client.getSyncContext().initialize(store, new SimpleSyncHandler()).get();

        MobileServiceSyncTable<StringIdType> table = client.getSyncTable(StringIdType.class);

        Query query = QueryOperations.tableName(table.getName()).field("String").eq("world").orderBy("Id", QueryOrder.Descending)
                .includeInlineCount().select("String");

        table.pull(query).get();

        assertEquals(
                serviceFilterContainer.Requests.get(0).Url,
                EncodingUtilities
                        .percentEncodeSpaces(
                                "http://myapp.com/tables/stringidtype?$filter=String%20eq%20('world')&$top=50&$skip=0&$orderby=Id%20desc&__includeDeleted=true&__systemproperties=__version,__deleted"));
    }

    public void testIncrementalPullSucceds() throws MalformedURLException, InterruptedException, ExecutionException, MobileServiceException {

        MobileServiceLocalStoreMock store = new MobileServiceLocalStoreMock();
        ServiceFilterContainer serviceFilterContainer = new ServiceFilterContainer();
        String queryKey = "QueryKey";

        MobileServiceClient client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());

        SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSS'Z'");
        sdf.setTimeZone(TimeZone.getTimeZone("UTC"));

        String updatedAt1 = sdf.format(new Date());
        String updatedAt2 = sdf.format(new Date());

        client = client.withFilter(getTestFilter(serviceFilterContainer, false,
                "{\"count\":\"4\",\"results\":[{\"id\":\"abc\",\"String\":\"Hey\",\"__updatedAt\":\"" + updatedAt1 + "\"},{\"id\":\"def\",\"String\":\"World\",\"__updatedAt\":\"" + updatedAt1 + "\"}]}",
                "[{\"id\":\"abc\",\"String\":\"Hey\",\"__updatedAt\":\"" + updatedAt2 + "\"},{\"id\":\"def\",\"String\":\"World\",\"__updatedAt\":\"" + updatedAt2 + "\"}]"
                // remote
                // item
        ));

        client.getSyncContext().initialize(store, new SimpleSyncHandler()).get();

        MobileServiceSyncTable<StringIdType> table = client.getSyncTable(StringIdType.class);

        Query query = QueryOperations.tableName(table.getName()).top(2);

        table.pull(query, "QueryKey").get();

        assertEquals(
                serviceFilterContainer.Requests.get(0).Url,
                EncodingUtilities
                        .percentEncodeSpaces(
                                "http://myapp.com/tables/stringidtype?$top=50&$orderby=__updatedAt%20asc,id%20asc&__includeDeleted=true&__systemproperties=__updatedAt,__version,__deleted"));

        assertEquals(
                serviceFilterContainer.Requests.get(1).Url,
                EncodingUtilities
                        .percentEncodeSpaces(
                                "http://myapp.com/tables/stringidtype?$filter=__updatedAt%20gt%20(datetime'" + updatedAt1 +
                                        "')%20or%20(__updatedAt%20ge%20(datetime'" + updatedAt1 +
                                        "')%20and%20id%20gt%20('def'))&$top=50&$orderby=__updatedAt%20asc,id%20asc&__includeDeleted=true&__systemproperties=__updatedAt,__version,__deleted"));
    }

    public void testIncrementalPullSaveLastUpdatedAtDate() throws MalformedURLException, InterruptedException, ExecutionException, MobileServiceException {

        MobileServiceLocalStoreMock store = new MobileServiceLocalStoreMock();
        ServiceFilterContainer serviceFilterContainer = new ServiceFilterContainer();
        String queryKey = "QueryKey";
        String incrementalPullStrategyTable = "__incrementalPullData";

        MobileServiceClient client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());

        SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSS'Z'");
        sdf.setTimeZone(TimeZone.getTimeZone("UTC"));

        String updatedAt1 = sdf.format(new Date());
        String updatedAt2 = sdf.format(new Date());

        client = client.withFilter(getTestFilter(serviceFilterContainer, false,
                "{\"count\":\"4\",\"results\":[{\"id\":\"abc\",\"String\":\"Hey\",\"__updatedAt\":\"" + updatedAt1 + "\"},{\"id\":\"def\",\"String\":\"World\",\"__updatedAt\":\"" + updatedAt1 + "\"}]}",
                "[{\"id\":\"abc\",\"String\":\"Hey\",\"__updatedAt\":\"" + updatedAt2 + "\"},{\"id\":\"def\",\"String\":\"World\",\"__updatedAt\":\"" + updatedAt2 + "\"}]"
                // remote
                // item
        ));

        client.getSyncContext().initialize(store, new SimpleSyncHandler()).get();

        MobileServiceSyncTable<StringIdType> table = client.getSyncTable(StringIdType.class);

        Query query = QueryOperations.tableName(table.getName()).top(2);

        table.pull(query, queryKey).get();

        LinkedHashMap<String, JsonObject> tableContent = store.Tables.get(incrementalPullStrategyTable);

        JsonElement result = tableContent.get(table.getName() + "_" + queryKey);

        String stringMaxUpdatedDate = result.getAsJsonObject()
                .get("maxupdateddate").getAsString();

        assertEquals(updatedAt2, stringMaxUpdatedDate);
    }

    public void testPurgeDoesNotThrowExceptionWhenThereIsNoOperationInTable() throws MalformedURLException, InterruptedException, ExecutionException {
        MobileServiceLocalStoreMock store = new MobileServiceLocalStoreMock();
        ServiceFilterContainer serviceFilterContainer = new ServiceFilterContainer();

        MobileServiceClient client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());

        client = client.withFilter(getTestFilter(serviceFilterContainer, "{\"id\":\"abc\",\"String\":\"Hey\"}"));

        client.getSyncContext().initialize(store, new SimpleSyncHandler()).get();

        // insert item in purge table
        MobileServiceJsonSyncTable table1 = client.getSyncTable("someTable");
        JsonObject jsonObject = new JsonObject();
        jsonObject.addProperty("id", "abc");

        table1.insert(jsonObject).get();

        // but push to clear the queue
        client.getSyncContext().push().get();

        assertEquals(store.Tables.get(table1.getName().toLowerCase(Locale.getDefault())).size(), 1);// item
        // is
        // inserted

        // then insert item in other table
        MobileServiceSyncTable<StringIdType> table2 = client.getSyncTable(StringIdType.class);

        StringIdType item = new StringIdType();

        item.Id = "an id";
        item.String = "what?";

        table2.insert(item).get();

        // try purge on first table now
        table1.purge(null).get();

        assertEquals(store.DeleteQueries.get(0).getTableName(), SyncErrors); // push
        // deletes
        // all
        // sync
        // erros

        assertEquals(store.DeleteQueries.get(1).getTableName(), table1.getName().toLowerCase(Locale.getDefault()));
        // purged table
        assertEquals(serviceFilterContainer.Requests.size(), 1); // still 1
        // means no
        // other
        // push happened
        assertEquals(store.Tables.get(table2.getName().toLowerCase(Locale.getDefault())).size(), 1); // this
        // table
        // should
        // not
        // be
        // touched
    }

    public void testPurgeThrowsExceptionWhenThereIsOperationInTable() throws InterruptedException, ExecutionException, MalformedURLException {
        MobileServiceLocalStoreMock store = new MobileServiceLocalStoreMock();
        ServiceFilterContainer serviceFilterContainer = new ServiceFilterContainer();

        MobileServiceClient client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());

        client = client.withFilter(getTestFilter(serviceFilterContainer, "{\"id\":\"abc\",\"String\":\"Hey\"}"));

        client.getSyncContext().initialize(store, new SimpleSyncHandler()).get();

        // insert an item but don't push
        MobileServiceJsonSyncTable table1 = client.getSyncTable("someTable");
        JsonObject jsonObject = new JsonObject();
        jsonObject.addProperty("id", "abc");

        table1.insert(jsonObject).get();

        assertEquals(store.Tables.get(table1.getName().toLowerCase(Locale.getDefault())).size(), 1); // item
        // is
        // inserted

        try {
            // this should throw an exception
            table1.purge(null).get();
        } catch (Throwable throwable) {
            if (throwable.getCause() instanceof MobileServiceException) {
                assertEquals(store.DeleteQueries.size(), 0); // no purge queries
                return;
            }
        }

        fail("MobileServiceException expected");

    }

    public void testPushExecutesThePendingOperations() throws InterruptedException, ExecutionException, MalformedURLException {
        MobileServiceLocalStoreMock store = new MobileServiceLocalStoreMock();
        ServiceFilterContainer serviceFilterContainer = new ServiceFilterContainer();

        MobileServiceClient client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());

        client = client.withFilter(getTestFilter(serviceFilterContainer, "{\"id\":\"abc\",\"String\":\"Hey\"}"));

        client.getSyncContext().initialize(store, new SimpleSyncHandler()).get();

        MobileServiceSyncTable<StringIdType> table = client.getSyncTable(StringIdType.class);

        StringIdType item = new StringIdType();

        item.Id = "an id";
        item.String = "what?";

        table.insert(item).get();

        assertEquals(store.Tables.get(table.getName().toLowerCase(Locale.getDefault())).size(), 1);

        client.getSyncContext().push().get();
    }

    public void testDeleteDoesNotUpsertResultOnStoreWhenOperationIsPushed() throws InterruptedException, ExecutionException, MalformedURLException {
        MobileServiceLocalStoreMock store = new MobileServiceLocalStoreMock();
        ServiceFilterContainer serviceFilterContainer = new ServiceFilterContainer();

        MobileServiceClient client = null;

        client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());

        client = client.withFilter(getTestFilter(serviceFilterContainer, "{\"id\":\"abc\",\"String\":\"Hey\"}", // for
                // insert
                "{\"id\":\"abc\",\"String\":\"Hey\"}"// for delete

        ));

        client.getSyncContext().initialize(store, new SimpleSyncHandler()).get();

        MobileServiceSyncTable<StringIdType> table = client.getSyncTable(StringIdType.class);

        StringIdType item = new StringIdType();

        item.Id = "abc";
        item.String = "what?";

        // first add an item
        table.insert(item).get();

        assertEquals(store.Tables.get(table.getName().toLowerCase(Locale.getDefault())).size(), 1);

        // for good measure also push it
        client.getSyncContext().push().get();

        table.delete(item).get();

        assertEquals(store.Tables.get(table.getName().toLowerCase(Locale.getDefault())).size(), 0);

        // now play it on server
        client.getSyncContext().push().get();

        // wait we don't want to upsert the result back because its delete
        // operation
        assertEquals(store.Tables.get(table.getName().toLowerCase(Locale.getDefault())).size(), 0);
        // looks good
    }

    public void testDeleteCancelsAllWhenInsertIsInQueue() throws Throwable {
        MobileServiceLocalStoreMock store = new MobileServiceLocalStoreMock();
        ServiceFilterContainer serviceFilterContainer = new ServiceFilterContainer();

        MobileServiceClient client = null;

        client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());

        client = client.withFilter(getTestFilter(serviceFilterContainer, "[{\"id\":\"abc\",\"String\":\"Hey\"}]"));

        client.getSyncContext().initialize(store, new SimpleSyncHandler()).get();

        MobileServiceSyncTable<StringIdType> table = client.getSyncTable(StringIdType.class);

        StringIdType item = new StringIdType();

        item.Id = "abc";
        item.String = "what?";

        table.insert(item).get();
        assertEquals(client.getSyncContext().getPendingOperations(), 1);

        table.delete(item).get();
        assertEquals(client.getSyncContext().getPendingOperations(), 0);

        client.getSyncContext().push().get();

        assertEquals(client.getSyncContext().getPendingOperations(), 0);
    }

    public void testCancelAndDiscardItem() throws Throwable {
        MobileServiceLocalStoreMock store = new MobileServiceLocalStoreMock();
        final ServiceFilterContainer serviceFilterContainer = new ServiceFilterContainer();
        final ThrownExceptionFlag thrownExceptionFlag = new ThrownExceptionFlag();

        thrownExceptionFlag.Thrown = true;

        MobileServiceClient client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());

        Function<ServiceFilterRequest, Void> onHandleRequest = new Function<ServiceFilterRequest, Void>() {
            public Void apply(ServiceFilterRequest request) {
                try {
                    if (thrownExceptionFlag.Thrown) {
                        throw new Exception();
                    }
                } catch (Exception e) {
                    serviceFilterContainer.Exception = e;
                }

                return null;
            }
        };

        client = client.withFilter(getTestFilter(serviceFilterContainer, onHandleRequest, "{\"id\":\"abc\",\"String\":\"Hey\"}"));

        client.getSyncContext().initialize(store, new SimpleSyncHandler()).get();

        MobileServiceSyncTable<StringIdType> table = client.getSyncTable(StringIdType.class);

        StringIdType item = new StringIdType();

        item.Id = "abc";
        item.String = "what?";

        table.insert(item).get();
        assertEquals(client.getSyncContext().getPendingOperations(), 1);

        try {
            client.getSyncContext().push().get();
        }
        catch(Exception ex) {
            MobileServicePushFailedException mspfe = (MobileServicePushFailedException) ex.getCause();

            assertEquals(mspfe.getPushCompletionResult().getStatus(), MobileServicePushStatus.InternalError);
            assertEquals(mspfe.getPushCompletionResult().getOperationErrors().size(), 1);
            assertEquals(client.getSyncContext().getPendingOperations(), 1);

            TableOperationError tableOperationError =mspfe.getPushCompletionResult().getOperationErrors().get(0);

            client.getSyncContext().cancelAndDiscardItem(tableOperationError);

            assertEquals(client.getSyncContext().getPendingOperations(), 0);

            StringIdType result = table.lookUp(item.Id).get();

            assertEquals(null, result);

            return;
        }

        assertTrue(false);
    }

    public void testCancelAndUpdateItem() throws Throwable {
        MobileServiceLocalStoreMock store = new MobileServiceLocalStoreMock();
        final ServiceFilterContainer serviceFilterContainer = new ServiceFilterContainer();
        final ThrownExceptionFlag thrownExceptionFlag = new ThrownExceptionFlag();

        thrownExceptionFlag.Thrown = true;

        MobileServiceClient client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());

        final JsonObject serverItem = new JsonObject();
        serverItem.addProperty("id", "abc");
        serverItem.addProperty("String", "fooo");


        Function<ServiceFilterRequest, Void> onHandleRequest = new Function<ServiceFilterRequest, Void>() {
            public Void apply(ServiceFilterRequest request) {
                try {
                    if (thrownExceptionFlag.Thrown) {
                        throw new MobileServiceExceptionBase(new MobileServiceException(new Exception()), serverItem);
                    }
                } catch (Exception e) {
                    serviceFilterContainer.Exception = e;
                }

                return null;
            }
        };

        client = client.withFilter(getTestFilter(serviceFilterContainer, onHandleRequest, "{\"id\":\"abc\",\"String\":\"Hey\"}"));

        client.getSyncContext().initialize(store, new SimpleSyncHandler()).get();

        MobileServiceSyncTable<StringIdType> table = client.getSyncTable(StringIdType.class);

        StringIdType item = new StringIdType();

        item.Id = "abc";
        item.String = "what?";

        table.insert(item).get();
        assertEquals(client.getSyncContext().getPendingOperations(), 1);

        try {
            client.getSyncContext().push().get();
        }
        catch(Exception ex) {
            MobileServicePushFailedException mspfe = (MobileServicePushFailedException) ex.getCause();

            assertEquals(mspfe.getPushCompletionResult().getStatus(), MobileServicePushStatus.InternalError);
            assertEquals(mspfe.getPushCompletionResult().getOperationErrors().size(), 1);
            assertEquals(client.getSyncContext().getPendingOperations(), 1);

            TableOperationError tableOperationError =mspfe.getPushCompletionResult().getOperationErrors().get(0);

            client.getSyncContext().cancelAndUpdateItem(tableOperationError);

            assertEquals(client.getSyncContext().getPendingOperations(), 0);

            StringIdType result = table.lookUp(item.Id).get();

            assertEquals(item.Id, result.Id);
            assertEquals("fooo", result.String);

            return;
        }

        assertTrue(false);
    }

    public void testPushAbortOnNewtorkError() throws Throwable {
        MobileServiceLocalStoreMock store = new MobileServiceLocalStoreMock();
        final ServiceFilterContainer serviceFilterContainer = new ServiceFilterContainer();
        final ThrownExceptionFlag thrownExceptionFlag = new ThrownExceptionFlag();

        thrownExceptionFlag.Thrown = true;

        MobileServiceClient client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());

        final MobileServiceException innerException = new MobileServiceException(new IOException());

        Function<ServiceFilterRequest, Void> onHandleRequest = new Function<ServiceFilterRequest, Void>() {
            public Void apply(ServiceFilterRequest request) {
                try {
                    if (thrownExceptionFlag.Thrown) {
                        throw innerException;
                    }
                } catch (Exception e) {
                    serviceFilterContainer.Exception = e;
                }

                return null;
            }
        };

        client = client.withFilter(getTestFilter(serviceFilterContainer, onHandleRequest, "{\"id\":\"abc\",\"String\":\"Hey\"}"));

        client.getSyncContext().initialize(store, new SimpleSyncHandler()).get();

        MobileServiceSyncTable<StringIdType> table = client.getSyncTable(StringIdType.class);

        StringIdType item = new StringIdType();

        item.Id = "abc";
        item.String = "what?";

        table.insert(item).get();
        assertEquals(client.getSyncContext().getPendingOperations(), 1);

        try {
            client.getSyncContext().push().get();
        }
        catch(Exception ex) {

            MobileServicePushFailedException mspfe = (MobileServicePushFailedException) ex.getCause();

            assertEquals(mspfe.getPushCompletionResult().getStatus(), MobileServicePushStatus.CancelledByNetworkError);
            assertEquals(mspfe.getPushCompletionResult().getOperationErrors().size(), 0);
            assertEquals(client.getSyncContext().getPendingOperations(), 1);

            thrownExceptionFlag.Thrown = false;

            client.getSyncContext().push().get();

            assertEquals(client.getSyncContext().getPendingOperations(), 0);
            assertEquals(serviceFilterContainer.Requests.get(1).Method, "POST");

            return;
        }

        assertTrue(false);
    }

    public void testPushAbortOnAuthentication() throws Throwable {
        MobileServiceLocalStoreMock store = new MobileServiceLocalStoreMock();
        final ServiceFilterContainer serviceFilterContainer = new ServiceFilterContainer();
        final ThrownExceptionFlag thrownExceptionFlag = new ThrownExceptionFlag();

        thrownExceptionFlag.Thrown = true;

        MobileServiceClient client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());

        ServiceFilterResponseMock response = new ServiceFilterResponseMock();
        response.setStatus(new StatusLineMock(401));

        final MobileServiceException innerException = new MobileServiceException("", response);

        Function<ServiceFilterRequest, Void> onHandleRequest = new Function<ServiceFilterRequest, Void>() {
            public Void apply(ServiceFilterRequest request) {
                try {
                    if (thrownExceptionFlag.Thrown) {
                        throw innerException;
                    }
                } catch (Exception e) {
                    serviceFilterContainer.Exception = e;
                }

                return null;
            }
        };

        client = client.withFilter(getTestFilter(serviceFilterContainer, onHandleRequest, "{\"id\":\"abc\",\"String\":\"Hey\"}"));

        client.getSyncContext().initialize(store, new SimpleSyncHandler()).get();

        MobileServiceSyncTable<StringIdType> table = client.getSyncTable(StringIdType.class);

        StringIdType item = new StringIdType();

        item.Id = "abc";
        item.String = "what?";

        table.insert(item).get();
        assertEquals(client.getSyncContext().getPendingOperations(), 1);

        try {
            client.getSyncContext().push().get();
        }
        catch(Exception ex) {

            MobileServicePushFailedException mspfe = (MobileServicePushFailedException) ex.getCause();

            assertEquals(mspfe.getPushCompletionResult().getStatus(), MobileServicePushStatus.CancelledByAuthenticationError);
            assertEquals(mspfe.getPushCompletionResult().getOperationErrors().size(), 0);
            assertEquals(client.getSyncContext().getPendingOperations(), 1);

            thrownExceptionFlag.Thrown = false;

            client.getSyncContext().push().get();

            assertEquals(client.getSyncContext().getPendingOperations(), 0);
            assertEquals(serviceFilterContainer.Requests.get(1).Method, "POST");

            return;
        }

        assertTrue(false);
    }

    public void testPushAbortOnOperationError() throws Throwable {
        MobileServiceLocalStoreMock store = new MobileServiceLocalStoreMock();
        final ServiceFilterContainer serviceFilterContainer = new ServiceFilterContainer();
        final ThrownExceptionFlag thrownExceptionFlag = new ThrownExceptionFlag();

        thrownExceptionFlag.Thrown = true;

        MobileServiceClient client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());

        Function<ServiceFilterRequest, Void> onHandleRequest = new Function<ServiceFilterRequest, Void>() {
            public Void apply(ServiceFilterRequest request) {
                try {
                    if (thrownExceptionFlag.Thrown) {
                        throw new Exception();
                    }
                } catch (Exception e) {
                    serviceFilterContainer.Exception = e;
                }

                return null;
            }
        };

        client = client.withFilter(getTestFilter(serviceFilterContainer, onHandleRequest, "{\"id\":\"abc\",\"String\":\"Hey\"}"));

        client.getSyncContext().initialize(store, new SimpleSyncHandler()).get();

        MobileServiceSyncTable<StringIdType> table = client.getSyncTable(StringIdType.class);

        StringIdType item = new StringIdType();

        item.Id = "abc";
        item.String = "what?";

        table.insert(item).get();
        assertEquals(client.getSyncContext().getPendingOperations(), 1);

        try {
            client.getSyncContext().push().get();
        }
        catch(Exception ex) {
            MobileServicePushFailedException mspfe = (MobileServicePushFailedException) ex.getCause();

            assertEquals(mspfe.getPushCompletionResult().getStatus(), MobileServicePushStatus.InternalError);
            assertEquals(mspfe.getPushCompletionResult().getOperationErrors().size(), 1);
            assertEquals(client.getSyncContext().getPendingOperations(), 1);

            thrownExceptionFlag.Thrown = false;

            client.getSyncContext().push().get();

            assertEquals(client.getSyncContext().getPendingOperations(), 0);
            assertEquals(serviceFilterContainer.Requests.get(1).Method, "POST");

            return;
        }

        assertTrue(false);
    }


    public void testDeleteNoCollapseInsertWhenFailedInsertIsInQueueOnNewtorkError() throws Throwable {
        MobileServiceLocalStoreMock store = new MobileServiceLocalStoreMock();
        final ServiceFilterContainer serviceFilterContainer = new ServiceFilterContainer();
        final ThrownExceptionFlag thrownExceptionFlag = new ThrownExceptionFlag();
        String expectedUrl = "http://myapp.com/tables/stringidtype/abc";

        thrownExceptionFlag.Thrown = true;

        MobileServiceClient client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());

        final MobileServiceException innerException = new MobileServiceException(new IOException());

        Function<ServiceFilterRequest, Void> onHandleRequest = new Function<ServiceFilterRequest, Void>() {
            public Void apply(ServiceFilterRequest request) {
                try {
                    if (thrownExceptionFlag.Thrown) {
                        throw innerException;
                    }
                } catch (Exception e) {
                    serviceFilterContainer.Exception = e;
                }

                return null;
            }
        };

        client = client.withFilter(getTestFilter(serviceFilterContainer, onHandleRequest, "{\"id\":\"abc\",\"String\":\"Hey\"}"));

        client.getSyncContext().initialize(store, new SimpleSyncHandler()).get();

        MobileServiceSyncTable<StringIdType> table = client.getSyncTable(StringIdType.class);

        StringIdType item = new StringIdType();

        item.Id = "abc";
        item.String = "what?";

        table.insert(item).get();
        assertEquals(client.getSyncContext().getPendingOperations(), 1);

        try {
            client.getSyncContext().push().get();
        }
        catch(Exception ex) {
            table.delete(item).get();

            assertEquals(client.getSyncContext().getPendingOperations(), 1);

            thrownExceptionFlag.Thrown = false;

            client.getSyncContext().push().get();

            assertEquals(client.getSyncContext().getPendingOperations(), 0);

            assertEquals(serviceFilterContainer.Requests.get(1).Url, expectedUrl);
            assertEquals(serviceFilterContainer.Requests.get(1).Method, "DELETE");

            return;
        }

        assertTrue(false);
    }

    public void testDeleteNoCollapseInsertWhenFailedInsertIsInQueueOnAuthentication() throws Throwable {
        MobileServiceLocalStoreMock store = new MobileServiceLocalStoreMock();
        final ServiceFilterContainer serviceFilterContainer = new ServiceFilterContainer();
        final ThrownExceptionFlag thrownExceptionFlag = new ThrownExceptionFlag();
        String expectedUrl = "http://myapp.com/tables/stringidtype/abc";

        thrownExceptionFlag.Thrown = true;

        MobileServiceClient client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());

        ServiceFilterResponseMock response = new ServiceFilterResponseMock();
        response.setStatus(new StatusLineMock(401));

        final MobileServiceException innerException = new MobileServiceException("", response);

        Function<ServiceFilterRequest, Void> onHandleRequest = new Function<ServiceFilterRequest, Void>() {
            public Void apply(ServiceFilterRequest request) {
                try {
                    if (thrownExceptionFlag.Thrown) {
                        throw innerException;
                    }
                } catch (Exception e) {
                    serviceFilterContainer.Exception = e;
                }

                return null;
            }
        };

        client = client.withFilter(getTestFilter(serviceFilterContainer, onHandleRequest, "{\"id\":\"abc\",\"String\":\"Hey\"}"));

        client.getSyncContext().initialize(store, new SimpleSyncHandler()).get();

        MobileServiceSyncTable<StringIdType> table = client.getSyncTable(StringIdType.class);

        StringIdType item = new StringIdType();

        item.Id = "abc";
        item.String = "what?";

        table.insert(item).get();
        assertEquals(client.getSyncContext().getPendingOperations(), 1);

        try {
            client.getSyncContext().push().get();
        }
        catch(Exception ex) {
            table.delete(item).get();

            assertEquals(client.getSyncContext().getPendingOperations(), 1);

            thrownExceptionFlag.Thrown = false;

            client.getSyncContext().push().get();

            assertEquals(client.getSyncContext().getPendingOperations(), 0);

            assertEquals(serviceFilterContainer.Requests.get(1).Url, expectedUrl);
            assertEquals(serviceFilterContainer.Requests.get(1).Method, "DELETE");

            return;
        }

        assertTrue(false);
    }

    public void testDeleteNoCollapseInsertWhenFailedInsertIsInQueueOnOperationError() throws Throwable {
        MobileServiceLocalStoreMock store = new MobileServiceLocalStoreMock();
        final ServiceFilterContainer serviceFilterContainer = new ServiceFilterContainer();
        final ThrownExceptionFlag thrownExceptionFlag = new ThrownExceptionFlag();
        String expectedUrl = "http://myapp.com/tables/stringidtype/abc";

        thrownExceptionFlag.Thrown = true;

        MobileServiceClient client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());

        Function<ServiceFilterRequest, Void> onHandleRequest = new Function<ServiceFilterRequest, Void>() {
            public Void apply(ServiceFilterRequest request) {
                try {
                    if (thrownExceptionFlag.Thrown) {
                        throw new Exception();
                    }
                } catch (Exception e) {
                    serviceFilterContainer.Exception = e;
                }

                return null;
            }
        };

        client = client.withFilter(getTestFilter(serviceFilterContainer, onHandleRequest, "{\"id\":\"abc\",\"String\":\"Hey\"}"));

        client.getSyncContext().initialize(store, new SimpleSyncHandler()).get();

        MobileServiceSyncTable<StringIdType> table = client.getSyncTable(StringIdType.class);

        StringIdType item = new StringIdType();

        item.Id = "abc";
        item.String = "what?";

        table.insert(item).get();
        assertEquals(client.getSyncContext().getPendingOperations(), 1);

        try {
            client.getSyncContext().push().get();
        }
        catch(Exception ex) {
            table.delete(item).get();

            assertEquals(client.getSyncContext().getPendingOperations(), 1);

            thrownExceptionFlag.Thrown = false;

            client.getSyncContext().push().get();

            assertEquals(client.getSyncContext().getPendingOperations(), 0);

            assertEquals(serviceFilterContainer.Requests.get(1).Url, expectedUrl);
            assertEquals(serviceFilterContainer.Requests.get(1).Method, "DELETE");

            return;
        }

        assertTrue(false);
    }


    public void testDeleteCancelsUpdateWhenUpdateIsInQueue() throws Throwable {
        CustomFunctionTwoParameters<MobileServiceSyncTable<StringIdType>, StringIdType, Void> firstOperationOnItem1 = new CustomFunctionTwoParameters<MobileServiceSyncTable<StringIdType>, StringIdType, Void>() {

            @Override
            public Void apply(MobileServiceSyncTable<StringIdType> table, StringIdType item) throws Exception {
                try {
                    table.update(item).get();
                } catch (Exception e) {
                    throw e;
                }

                return null;
            }

        };

        CustomFunctionTwoParameters<MobileServiceSyncTable<StringIdType>, StringIdType, Void> operationOnItem2 = new CustomFunctionTwoParameters<MobileServiceSyncTable<StringIdType>, StringIdType, Void>() {

            @Override
            public Void apply(MobileServiceSyncTable<StringIdType> table, StringIdType item) throws Exception {
                try {
                    table.insert(item).get();
                } catch (Exception e) {
                    throw e;
                }

                return null;
            }

        };

        CustomFunctionTwoParameters<MobileServiceSyncTable<StringIdType>, StringIdType, Void> secondOperationOnItem1 = new CustomFunctionTwoParameters<MobileServiceSyncTable<StringIdType>, StringIdType, Void>() {

            @Override
            public Void apply(MobileServiceSyncTable<StringIdType> table, StringIdType item) throws Exception {
                try {
                    table.delete(item).get();
                } catch (Exception e) {
                    throw e;
                }

                return null;
            }

        };

        CustomFunctionTwoParameters<ServiceFilterRequest, Integer, Void> assertRequest = new CustomFunctionTwoParameters<ServiceFilterRequest, Integer, Void>() {

            @Override
            public Void apply(ServiceFilterRequest serviceFilterRequest, Integer executed) {
                if (executed == 1) // order is maintained by doing insert first
                // and delete after that. This means first
                // update was cancelled, not the second one.
                {
                    assertEquals(serviceFilterRequest.getMethod(), "POST");
                } else {
                    assertEquals(serviceFilterRequest.getMethod(), "DELETE");
                }

                return null;
            }

        };

        this.TestCollapseCancel(firstOperationOnItem1, operationOnItem2, secondOperationOnItem1, assertRequest);
    }

    public void testDeleteThrowsWhenDeleteIsInQueue() throws Throwable {
        CustomFunctionTwoParameters<MobileServiceSyncTable<StringIdType>, StringIdType, Void> firstOperation = new CustomFunctionTwoParameters<MobileServiceSyncTable<StringIdType>, StringIdType, Void>() {

            @Override
            public Void apply(MobileServiceSyncTable<StringIdType> table, StringIdType item) throws Exception {
                try {
                    table.delete(item).get();
                } catch (Exception e) {
                    throw e;
                }

                return null;
            }

        };

        CustomFunctionTwoParameters<MobileServiceSyncTable<StringIdType>, StringIdType, Void> secondOperation = new CustomFunctionTwoParameters<MobileServiceSyncTable<StringIdType>, StringIdType, Void>() {

            @Override
            public Void apply(MobileServiceSyncTable<StringIdType> table, StringIdType item) throws Exception {
                try {
                    table.delete(item).get();
                } catch (Exception e) {
                    throw e;
                }

                return null;
            }

        };

        this.TestCollapseThrow(firstOperation, secondOperation);
    }

    public void testInsertThrowsWhenUpdateIsInQueue() throws Throwable {
        CustomFunctionTwoParameters<MobileServiceSyncTable<StringIdType>, StringIdType, Void> firstOperation = new CustomFunctionTwoParameters<MobileServiceSyncTable<StringIdType>, StringIdType, Void>() {

            @Override
            public Void apply(MobileServiceSyncTable<StringIdType> table, StringIdType item) throws Exception {
                try {
                    table.update(item).get();
                } catch (Exception e) {
                    throw e;
                }

                return null;
            }

        };

        CustomFunctionTwoParameters<MobileServiceSyncTable<StringIdType>, StringIdType, Void> secondOperation = new CustomFunctionTwoParameters<MobileServiceSyncTable<StringIdType>, StringIdType, Void>() {

            @Override
            public Void apply(MobileServiceSyncTable<StringIdType> table, StringIdType item) throws Exception {
                try {
                    table.insert(item).get();
                } catch (Exception e) {
                    throw e;
                }

                return null;
            }

        };

        this.TestCollapseThrow(firstOperation, secondOperation);
    }

    public void testInsertThrowsWhenInsertIsInQueue() throws Throwable {
        CustomFunctionTwoParameters<MobileServiceSyncTable<StringIdType>, StringIdType, Void> firstOperation = new CustomFunctionTwoParameters<MobileServiceSyncTable<StringIdType>, StringIdType, Void>() {

            @Override
            public Void apply(MobileServiceSyncTable<StringIdType> table, StringIdType item) throws Exception {
                try {
                    table.insert(item).get();
                } catch (Exception e) {
                    throw e;
                }

                return null;
            }

        };

        CustomFunctionTwoParameters<MobileServiceSyncTable<StringIdType>, StringIdType, Void> secondOperation = new CustomFunctionTwoParameters<MobileServiceSyncTable<StringIdType>, StringIdType, Void>() {

            @Override
            public Void apply(MobileServiceSyncTable<StringIdType> table, StringIdType item) throws Exception {
                try {
                    table.insert(item).get();
                } catch (Exception e) {
                    throw e;
                }

                return null;
            }

        };

        this.TestCollapseThrow(firstOperation, secondOperation);
    }

    public void testUpdateThrowsWhenDeleteIsInQueue() throws Throwable {
        CustomFunctionTwoParameters<MobileServiceSyncTable<StringIdType>, StringIdType, Void> firstOperation = new CustomFunctionTwoParameters<MobileServiceSyncTable<StringIdType>, StringIdType, Void>() {

            @Override
            public Void apply(MobileServiceSyncTable<StringIdType> table, StringIdType item) throws Exception {
                try {
                    table.delete(item).get();
                } catch (Exception e) {
                    throw e;
                }

                return null;
            }

        };

        CustomFunctionTwoParameters<MobileServiceSyncTable<StringIdType>, StringIdType, Void> secondOperation = new CustomFunctionTwoParameters<MobileServiceSyncTable<StringIdType>, StringIdType, Void>() {

            @Override
            public Void apply(MobileServiceSyncTable<StringIdType> table, StringIdType item) throws Exception {
                try {
                    table.update(item).get();
                } catch (Exception e) {
                    throw e;
                }

                return null;
            }

        };

        this.TestCollapseThrow(firstOperation, secondOperation);
    }

    // public void testUpdateCancelsSecondUpdateWhenUpdateIsInQueue()
    // throws Throwable
    // {
    // CustomFunctionTwoParameters<MobileServiceSyncTable<StringIdType>,
    // StringIdType, Void>
    // firstOperationOnItem1 = new
    // CustomFunctionTwoParameters<MobileServiceSyncTable<StringIdType>,
    // StringIdType,
    // Void>() {
    //
    // @Override
    // public Void apply(MobileServiceSyncTable<StringIdType> table,
    // StringIdType item) throws Exception {
    // try {
    // table.update(item).get();
    // } catch (Exception e) {
    // throw e;
    // }
    //
    // return null;
    // }
    //
    // };
    //
    // CustomFunctionTwoParameters<MobileServiceSyncTable<StringIdType>,
    // StringIdType, Void>
    // operationOnItem2 = new
    // CustomFunctionTwoParameters<MobileServiceSyncTable<StringIdType>,
    // StringIdType,
    // Void>() {
    //
    // @Override
    // public Void apply(MobileServiceSyncTable<StringIdType> table,
    // StringIdType item) throws Exception {
    // try {
    // table.delete(item).get();
    // } catch (Exception e) {
    // throw e;
    // }
    //
    // return null;
    // }
    //
    // };
    //
    // CustomFunctionTwoParameters<MobileServiceSyncTable<StringIdType>,
    // StringIdType, Void>
    // secondOperationOnItem1 = new
    // CustomFunctionTwoParameters<MobileServiceSyncTable<StringIdType>,
    // StringIdType,
    // Void>() {
    //
    // @Override
    // public Void apply(MobileServiceSyncTable<StringIdType> table,
    // StringIdType item) throws Exception {
    // try {
    // table.update(item).get();
    // } catch (Exception e) {
    // throw e;
    // }
    //
    // return null;
    // }
    //
    // };
    //
    // CustomFunctionTwoParameters<ServiceFilterRequest, Integer, Void>
    // assertRequest = new
    // CustomFunctionTwoParameters<ServiceFilterRequest, Integer, Void>() {
    //
    // @Override
    // public Void apply(ServiceFilterRequest serviceFilterRequest, Integer
    // executed) {
    //
    // // if (executed == 1) // order is maintained by doing insert first
    // // // and delete after that. This means first
    // // // update was cancelled, not the second one.
    // // {
    // // assertEquals(serviceFilterRequest.getMethod(), "PATCH");
    // // } else {
    // // assertEquals(serviceFilterRequest.getMethod(), "DELETE");
    // // }
    //
    // return null;
    // }
    //
    // };
    //
    // this.TestCollapseCancel(firstOperationOnItem1, operationOnItem2,
    // secondOperationOnItem1, assertRequest);
    // }

    public void testUpdateCancelsSecondUpdateWhenInsertIsInQueue() throws Throwable {
        CustomFunctionTwoParameters<MobileServiceSyncTable<StringIdType>, StringIdType, Void> firstOperationOnItem1 = new CustomFunctionTwoParameters<MobileServiceSyncTable<StringIdType>, StringIdType, Void>() {

            @Override
            public Void apply(MobileServiceSyncTable<StringIdType> table, StringIdType item) throws Exception {
                try {
                    table.insert(item).get();
                } catch (Exception e) {
                    throw e;
                }

                return null;
            }

        };

        CustomFunctionTwoParameters<MobileServiceSyncTable<StringIdType>, StringIdType, Void> operationOnItem2 = new CustomFunctionTwoParameters<MobileServiceSyncTable<StringIdType>, StringIdType, Void>() {

            @Override
            public Void apply(MobileServiceSyncTable<StringIdType> table, StringIdType item) throws Exception {
                try {
                    table.delete(item).get();
                } catch (Exception e) {
                    throw e;
                }

                return null;
            }

        };

        CustomFunctionTwoParameters<MobileServiceSyncTable<StringIdType>, StringIdType, Void> secondOperationOnItem1 = new CustomFunctionTwoParameters<MobileServiceSyncTable<StringIdType>, StringIdType, Void>() {

            @Override
            public Void apply(MobileServiceSyncTable<StringIdType> table, StringIdType item) throws Exception {
                try {
                    table.update(item).get();
                } catch (Exception e) {
                    throw e;
                }

                return null;
            }

        };

        CustomFunctionTwoParameters<ServiceFilterRequest, Integer, Void> assertRequest = new CustomFunctionTwoParameters<ServiceFilterRequest, Integer, Void>() {

            @Override
            public Void apply(ServiceFilterRequest serviceFilterRequest, Integer executed) {
                if (executed == 1) // order is maintained by doing insert first
                // and delete after that. This means first
                // update was cancelled, not the second one.
                {
                    assertEquals(serviceFilterRequest.getMethod(), "POST");
                } else {
                    assertEquals(serviceFilterRequest.getMethod(), "DELETE");
                }

                return null;
            }

        };

        this.TestCollapseCancel(firstOperationOnItem1, operationOnItem2, secondOperationOnItem1, assertRequest);
    }

    public void testPushIsAbortedOnNetworkError() throws Throwable {
        this.TestPushAbort(new IOException(), MobileServicePushStatus.CancelledByNetworkError);
    }

    public void testPushIsAbortedOnAuthenticationError() throws Throwable {
        // Create a mock response simulating an error
        ServiceFilterResponseMock response = new ServiceFilterResponseMock();
        response.setStatus(new StatusLineMock(401));

        MobileServiceException authError = new MobileServiceException("", response);
        TestPushAbort(authError, MobileServicePushStatus.CancelledByAuthenticationError);
    }

    private void TestCollapseCancel(CustomFunctionTwoParameters<MobileServiceSyncTable<StringIdType>, StringIdType, Void> firstOperationOnItem1,
                                    CustomFunctionTwoParameters<MobileServiceSyncTable<StringIdType>, StringIdType, Void> operationOnItem2,
                                    CustomFunctionTwoParameters<MobileServiceSyncTable<StringIdType>, StringIdType, Void> secondOperationOnItem1,
                                    final CustomFunctionTwoParameters<ServiceFilterRequest, Integer, Void> assertRequest) throws Throwable {

        MobileServiceLocalStoreMock store = new MobileServiceLocalStoreMock();
        final ServiceFilterContainer serviceFilterContainer = new ServiceFilterContainer();

        MobileServiceClient client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());

        Function<ServiceFilterRequest, Void> onHandleRequest = new Function<ServiceFilterRequest, Void>() {
            public Void apply(ServiceFilterRequest request) {
                try {
                    assertRequest.apply(request, serviceFilterContainer.Requests.size());
                } catch (Exception e) {
                    // throw e;
                }

                return null;
            }
        };

        client = client.withFilter(getTestFilter(serviceFilterContainer, onHandleRequest, "{\"id\":\"abc\",\"String\":\"Hey\"}"));

        client.getSyncContext().initialize(store, new SimpleSyncHandler()).get();

        StringIdType item1 = new StringIdType();

        item1.Id = "an id";
        item1.String = "what?";

        StringIdType item2 = new StringIdType();

        item2.Id = "two";
        item2.String = "this";

        MobileServiceSyncTable<StringIdType> table = client.getSyncTable(StringIdType.class);

        firstOperationOnItem1.apply(table, item1);
        assertEquals(client.getSyncContext().getPendingOperations(), 1);

        operationOnItem2.apply(table, item2);
        assertEquals(client.getSyncContext().getPendingOperations(), 2);

        secondOperationOnItem1.apply(table, item1);
        assertEquals(client.getSyncContext().getPendingOperations(), 2);

        client.getSyncContext().push().get();

        assertEquals(client.getSyncContext().getPendingOperations(), 0);
        assertEquals(serviceFilterContainer.Requests.size(), 2); // total two
        // operations
        // executed
    }

    private void TestCollapseThrow(CustomFunctionTwoParameters<MobileServiceSyncTable<StringIdType>, StringIdType, Void> firstOperation,
                                   CustomFunctionTwoParameters<MobileServiceSyncTable<StringIdType>, StringIdType, Void> secondOperation) throws Throwable {
        MobileServiceLocalStoreMock store = new MobileServiceLocalStoreMock();

        MobileServiceClient client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());

        client = client.withFilter(getTestFilter());

        client.getSyncContext().initialize(store, new SimpleSyncHandler()).get();

        StringIdType item = new StringIdType();

        item.Id = "an id";
        item.String = "what?";

        MobileServiceSyncTable<StringIdType> table = client.getSyncTable(StringIdType.class);

        firstOperation.apply(table, item);
        assertEquals(client.getSyncContext().getPendingOperations(), 1);

        try {
            secondOperation.apply(table, item);
        } catch (Exception e) {
            e.printStackTrace();

            assertEquals(client.getSyncContext().getPendingOperations(), 1);
            return;
        }

        fail("Exception expected");

    }

    private void TestPushAbort(final Exception toThrow, MobileServicePushStatus expectedStatus) throws Throwable {
        MobileServiceSyncHandlerMock operationHandler = new MobileServiceSyncHandlerMock();

        final ThrownExceptionFlag thrownExceptionFlag = new ThrownExceptionFlag();
        thrownExceptionFlag.Thrown = false;

        MobileServiceLocalStoreMock store = new MobileServiceLocalStoreMock();
        final ServiceFilterContainer serviceFilterContainer = new ServiceFilterContainer();

        MobileServiceClient client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());

        Function<ServiceFilterRequest, Void> onHandleRequest = new Function<ServiceFilterRequest, Void>() {
            public Void apply(ServiceFilterRequest request) {
                try {
                    if (!thrownExceptionFlag.Thrown) {
                        thrownExceptionFlag.Thrown = true;
                        throw toThrow;
                    }
                } catch (Exception e) {
                    serviceFilterContainer.Exception = e;
                }

                return null;
            }
        };

        client = client.withFilter(getTestFilter(serviceFilterContainer, onHandleRequest, "{\"id\":\"abc\",\"String\":\"Hey\"}"));

        client.getSyncContext().initialize(store, operationHandler).get();

        StringIdType item = new StringIdType();

        item.Id = "an id";
        item.String = "what?";

        MobileServiceSyncTable<StringIdType> table = client.getSyncTable(StringIdType.class);

        table.insert(item).get();

        assertEquals(client.getSyncContext().getPendingOperations(), 1);

        try {
            client.getSyncContext().push().get();

            fail("MobileServicePushFailedException expected");
            return;
        } catch (Exception ex) {
            if (ex.getCause() instanceof MobileServicePushFailedException) {
                MobileServicePushFailedException mobileServicePushFailedException = (MobileServicePushFailedException) ex.getCause();

                assertEquals(mobileServicePushFailedException.getPushCompletionResult().getStatus(), expectedStatus);
                assertEquals(mobileServicePushFailedException.getPushCompletionResult().getOperationErrors().size(), 0);
            } else {
                fail("MobileServicePushFailedException expected");
            }
        }

        assertEquals(operationHandler.PushCompletionResult.getStatus(), expectedStatus);

        // the insert operation is still in queue
        assertEquals(client.getSyncContext().getPendingOperations(), 1);

        client.getSyncContext().push().get();

        assertEquals(client.getSyncContext().getPendingOperations(), 0);

        assertEquals(operationHandler.PushCompletionResult.getStatus(), MobileServicePushStatus.Complete);
    }

    // Test Filter
    private ServiceFilter getTestFilter(String... content) {
        return getTestFilter(new ServiceFilterContainer(), 200, content);
    }

    // Test Filter
    private ServiceFilter getTestFilter(ServiceFilterContainer serviceFilterContainer, Function<ServiceFilterRequest, Void> onHandleRequest, String... content) {
        return getTestFilter(serviceFilterContainer, 200, onHandleRequest, true, content);
    }

    // Test Filter
    private ServiceFilter getTestFilter(ServiceFilterContainer serviceFilterContainer, String... content) {
        return getTestFilter(serviceFilterContainer, 200, content);
    }

    // Test Filter
    private ServiceFilter getTestFilter(ServiceFilterContainer serviceFilterContainer, final boolean getLastContentAsDefault, String... content) {
        return getTestFilter(serviceFilterContainer, 200, null, getLastContentAsDefault, content);
    }


    // Test Filter
    private ServiceFilter getTestFilter(ServiceFilterContainer serviceFilterContainer, int statusCode, String... content) {
        return getTestFilter(serviceFilterContainer, statusCode, null, true, content);
    }

    // Test Filter
    private ServiceFilter getTestFilter(final ServiceFilterContainer serviceFilterContainer, final int statusCode,
                                        final Function<ServiceFilterRequest, Void> onHandleRequest, final boolean getLastContentAsDefault, final String... contents) {

        return new ServiceFilter() {

            @Override
            public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

                // Create a mock response simulating an error
                ServiceFilterResponseMock response = new ServiceFilterResponseMock();
                response.setStatus(new StatusLineMock(statusCode));

                String content = "";

                if (contents.length > serviceFilterContainer.Requests.size()) {
                    content = contents[serviceFilterContainer.Requests.size()];
                } else {
                    if (getLastContentAsDefault) {
                        content = contents[contents.length - 1];
                    }
                    else {
                        content = "[]";
                    }
                }

                response.setContent(content);

                ServiceFilterRequestData serviceFilterRequestData = new ServiceFilterRequestData();
                serviceFilterRequestData.Headers = request.getHeaders();
                serviceFilterRequestData.Content = request.getContent();
                serviceFilterRequestData.Url = request.getUrl();
                serviceFilterRequestData.Method = request.getMethod();

                serviceFilterContainer.Url = request.getUrl();
                serviceFilterContainer.Requests.add(serviceFilterRequestData);

                if (onHandleRequest != null) {
                    onHandleRequest.apply(request);
                }

                // create a mock request to replace the existing one
                ServiceFilterRequestMock requestMock = new ServiceFilterRequestMock(response);

                if (serviceFilterContainer.Exception != null) {
                    requestMock.setExceptionToThrow(serviceFilterContainer.Exception);
                    requestMock.setHasErrorOnExecute(true);

                    // used exception
                    serviceFilterContainer.Exception = null;
                }

                return nextServiceFilterCallback.onNext(requestMock);
            }

        };
    }

    public class ServiceFilterContainer {
        public String Url;

        public List<ServiceFilterRequestData> Requests = new ArrayList<ServiceFilterRequestData>();

        public Exception Exception;
    }

    public class ThrownExceptionFlag {
        public boolean Thrown;
    }

    public class ServiceFilterRequestData {
        public Header[] Headers;

        public String Content;

        public String Url;

        public String Method;

        public int Count;

        public String getHeaderValue(String headerName) {

            for (Header header : Headers) {
                if (header.getName().equals(headerName)) {
                    return header.getValue();
                }
            }

            return null;
        }
    }
}
