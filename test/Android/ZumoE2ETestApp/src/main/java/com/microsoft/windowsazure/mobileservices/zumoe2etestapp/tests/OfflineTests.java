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

import com.google.common.util.concurrent.ListenableFuture;
import com.google.gson.Gson;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import com.google.gson.annotations.SerializedName;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceList;
import com.microsoft.windowsazure.mobileservices.authentication.MobileServiceAuthenticationProvider;
import com.microsoft.windowsazure.mobileservices.http.NextServiceFilterCallback;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilter;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterRequest;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.table.MobileServiceJsonTable;
import com.microsoft.windowsazure.mobileservices.table.MobileServicePreconditionFailedExceptionJson;
import com.microsoft.windowsazure.mobileservices.table.MobileServiceTable;
import com.microsoft.windowsazure.mobileservices.table.query.Query;
import com.microsoft.windowsazure.mobileservices.table.query.QueryOperations;
import com.microsoft.windowsazure.mobileservices.table.serialization.JsonEntityParser;
import com.microsoft.windowsazure.mobileservices.table.sync.MobileServiceSyncContext;
import com.microsoft.windowsazure.mobileservices.table.sync.MobileServiceSyncTable;
import com.microsoft.windowsazure.mobileservices.table.sync.localstore.ColumnDataType;
import com.microsoft.windowsazure.mobileservices.table.sync.localstore.MobileServiceLocalStoreException;
import com.microsoft.windowsazure.mobileservices.table.sync.localstore.SQLiteLocalStore;
import com.microsoft.windowsazure.mobileservices.table.sync.operations.RemoteTableOperationProcessor;
import com.microsoft.windowsazure.mobileservices.table.sync.operations.TableOperation;
import com.microsoft.windowsazure.mobileservices.table.sync.operations.TableOperationError;
import com.microsoft.windowsazure.mobileservices.table.sync.push.MobileServicePushCompletionResult;
import com.microsoft.windowsazure.mobileservices.table.sync.push.MobileServicePushFailedException;
import com.microsoft.windowsazure.mobileservices.table.sync.synchandler.MobileServiceSyncHandler;
import com.microsoft.windowsazure.mobileservices.table.sync.synchandler.MobileServiceSyncHandlerException;
import com.microsoft.windowsazure.mobileservices.table.sync.synchandler.SimpleSyncHandler;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestCase;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestExecutionCallback;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestGroup;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestResult;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestStatus;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.Util;

import java.util.ArrayList;
import java.util.Calendar;
import java.util.Date;
import java.util.HashMap;
import java.util.List;
import java.util.Locale;
import java.util.Map;
import java.util.Random;
import java.util.UUID;
import java.util.concurrent.ExecutionException;

public class OfflineTests extends TestGroup {

    protected static final String OFFLINE_TABLE_NAME = "offlineTest.db";
    protected static final String INCREMENTAL_PULL_STRATEGY_TABLE = "__incrementalPullData";

    public OfflineTests() {
        super("Offline tests");


        this.addTest(createClearStoreTest());


        this.addTest(createBasicTest("Basic Test"));
        this.addTest(createNoCollapseInsertOnPreviousPushError("No collapse insert on previous push error"));

        this.addTest(createLocallyDeleteAlreadyDeletedElementTest());

        this.addTest(createInsertDuplicatedElementTest());

        this.addTest(createDeleteSyncConflict());

        this.addTest(createSyncConflictTest(false));
        this.addTest(createSyncConflictTest(true));

        this.addTest(createSyncConflictAndResolveWithMethodTest(false));
        this.addTest(createSyncConflictAndResolveWithMethodTest(true));

        this.addTest(LoginTests.createLogoutTest());
        this.addTest(createSyncTestForAuthenticatedTable(false));
        this.addTest(LoginTests.createLoginTest(MobileServiceAuthenticationProvider.Facebook));

        TestCase noOptimisticConcurrencyTest = createNoOptimisticConcurrencyTest();
        noOptimisticConcurrencyTest.setCanRunUnattended(false);
        this.addTest(noOptimisticConcurrencyTest);

        this.addTest(createSyncTestForAuthenticatedTable(true));
        this.addTest(LoginTests.createLogoutTest());

        this.addTest(createOfflineIncrementalSyncTest(null, false, false));

        this.addTest(createOfflineIncrementalSyncTest("incrementalQuery", false, false));
        this.addTest(createOfflineIncrementalSyncTest("incrementalQuery", false, true));
        this.addTest(createOfflineIncrementalSyncTest("incrementalQuery", true, false));
        this.addTest(createOfflineIncrementalSyncTest("incrementalQuery", true, true));

    }

    private TestCase createBasicTest(String name) {

        final String tableName = "offlineReady";

        final TestCase test = new TestCase() {

            @Override
            protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {

                try {

                    TestCase testCase = this;
                    TestResult result = new TestResult();
                    result.setStatus(TestStatus.Passed);
                    result.setTestCase(testCase);

                    ServiceFilterWithRequestCount serviceFilter = new ServiceFilterWithRequestCount();

                    int requestsSentToServer = 0;

                    client = client.withFilter(serviceFilter);

                    SQLiteLocalStore localStore = new SQLiteLocalStore(client.getContext(), OFFLINE_TABLE_NAME, null, 1);

                    SimpleSyncHandler handler = new SimpleSyncHandler();
                    MobileServiceSyncContext syncContext = client.getSyncContext();

                    syncContext.initialize(localStore, handler).get();

                    log("Defined the table on the local store");

                    Map<String, ColumnDataType> tableDefinition = new HashMap<String, ColumnDataType>();
                    tableDefinition.put("id", ColumnDataType.String);
                    tableDefinition.put("name", ColumnDataType.String);
                    tableDefinition.put("age", ColumnDataType.Integer);
                    tableDefinition.put("float", ColumnDataType.Real);
                    tableDefinition.put("date", ColumnDataType.Date);
                    tableDefinition.put("bool", ColumnDataType.Boolean);
                    tableDefinition.put("__version", ColumnDataType.String);

                    log("Initialized the store and sync context");

                    localStore.defineTable(tableName, tableDefinition);

                    client.getSyncContext().initialize(localStore, new SimpleSyncHandler()).get();

                    MobileServiceSyncTable<OfflineReadyItem> localTable = client.getSyncTable(tableName, OfflineReadyItem.class);

                    MobileServiceTable<OfflineReadyItem> remoteTable = client.getTable(tableName, OfflineReadyItem.class);

                    OfflineReadyItem item = new OfflineReadyItem(new Random());

                    log("Inserted the item to the local store:" + item);

                    item = localTable.insert(item).get();

                    log("Validating that the item is not in the server table");

                    try {
                        requestsSentToServer++;
                        remoteTable.lookUp(item.getId()).get();
                        log("Error, item is present in the server");
                        // return false;
                    } catch (ExecutionException ex) {
                        log("Ok, item is not in the server:" + ex.getMessage());
                    }

                    if (!validateRequestCount(this, serviceFilter.requestCount, requestsSentToServer)) {
                        result.setStatus(TestStatus.Failed);
                        callback.onTestComplete(this, result);
                    }

                    log("Pushing changes to the server");
                    client.getSyncContext().push().get();
                    requestsSentToServer++;

                    if (!validateRequestCount(this, serviceFilter.requestCount, requestsSentToServer)) {
                        result.setStatus(TestStatus.Failed);
                        callback.onTestComplete(this, result);
                    }
                    log("Push done; now verifying that item is in the server");

                    OfflineReadyItem serverItem = remoteTable.lookUp(item.getId()).get();
                    requestsSentToServer++;

                    log("Retrieved item from server:" + serverItem);

                    if (serverItem.equals(item)) {
                        log("Items are the same");
                    } else {
                        log("Items are different. Local: " + item + "; remote: " + serverItem);
                        result.setStatus(TestStatus.Failed);
                        callback.onTestComplete(this, result);
                        return;
                    }

                    log("Now updating the item locally");

                    item.setFlag(!item.getFlag());
                    // item.addProperty("date", DateSerializer.serialize(new
                    // Date()));
                    // item.addProperty("__updatedAt",
                    // DateSerializer.serialize(new Date()));
                    // item.addProperty("__version", "1");

                    // item.Flag = !item.Flag;
                    // item.Age++;
                    // item.Date = new DateTime(now.Year, now.Month, now.Day,
                    // now.Hour, now.Minute, now.Second, now.Millisecond,
                    // DateTimeKind.Utc);

                    localTable.update(item).get();

                    log("Item has been updated");

                    OfflineReadyItem newItem = new OfflineReadyItem(new Random());

                    log("Adding a new item to the local table:" + newItem);
                    newItem = localTable.insert(newItem).get();

                    if (!validateRequestCount(this, serviceFilter.requestCount, requestsSentToServer)) {
                        result.setStatus(TestStatus.Failed);
                        callback.onTestComplete(this, result);
                    }

                    log("Pushing the new changes to the server");
                    client.getSyncContext().push().get();
                    requestsSentToServer += 2;

                    if (!validateRequestCount(this, serviceFilter.requestCount, requestsSentToServer)) {
                        result.setStatus(TestStatus.Failed);
                        callback.onTestComplete(this, result);
                    }

                    log("Push done. Verifying changes on the server");
                    serverItem = remoteTable.lookUp(item.getId()).get();
                    requestsSentToServer++;

                    if (serverItem.equals(item)) {
                        log("Updated items are the same");
                    } else {
                        log("Items are different. Local: " + item + "; remote: " + serverItem);
                        result.setStatus(TestStatus.Failed);
                        callback.onTestComplete(this, result);
                        return;
                    }

                    serverItem = remoteTable.lookUp(newItem.getId()).get();
                    requestsSentToServer++;

                    if (serverItem.equals(newItem)) {
                        log("New inserted item is the same");
                    } else {
                        log("Items are different. Local: " + item + "; remote: " + serverItem);
                        result.setStatus(TestStatus.Failed);
                        callback.onTestComplete(this, result);
                        return;
                    }

                    log("Cleaning up");
                    localTable.delete(item).get();

                    localTable.delete(newItem).get();
                    log("Local table cleaned up. Now sync'ing once more");
                    client.getSyncContext().push().get();

                    requestsSentToServer += 2;
                    if (!validateRequestCount(this, serviceFilter.requestCount, requestsSentToServer)) {
                        result.setStatus(TestStatus.Failed);
                        callback.onTestComplete(this, result);
                        return;
                    }

                    log("Done");

                    callback.onTestComplete(this, result);

                } catch (Exception e) {
                    callback.onTestComplete(this, createResultFromException(e));
                    return;
                }
            }

            ;
        };

        test.setName(name);

        return test;
    }

    private TestCase createNoCollapseInsertOnPreviousPushError(String name) {

        final String tableName = "offlineReady2";

        final TestCase test = new TestCase() {

            @Override
            protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {

                try {

                    TestCase testCase = this;
                    TestResult result = new TestResult();
                    result.setStatus(TestStatus.Passed);
                    result.setTestCase(testCase);

                    ServiceFilterWithRequestCount serviceFilter = new ServiceFilterWithRequestCount();

                    int requestsSentToServer = 0;

                    client = client.withFilter(serviceFilter);

                    SQLiteLocalStore localStore = new SQLiteLocalStore(client.getContext(), OFFLINE_TABLE_NAME, null, 1);

                    SimpleSyncHandler handler = new SimpleSyncHandler();
                    MobileServiceSyncContext syncContext = client.getSyncContext();

                    syncContext.initialize(localStore, handler).get();

                    log("Defined the table on the local store");

                    Map<String, ColumnDataType> tableDefinition = new HashMap<String, ColumnDataType>();
                    tableDefinition.put("id", ColumnDataType.String);
                    tableDefinition.put("name", ColumnDataType.String);
                    tableDefinition.put("age", ColumnDataType.Integer);
                    tableDefinition.put("float", ColumnDataType.Real);
                    tableDefinition.put("date", ColumnDataType.Date);
                    tableDefinition.put("bool", ColumnDataType.Boolean);
                    tableDefinition.put("__version", ColumnDataType.String);

                    log("Initialized the store and sync context");

                    localStore.defineTable(tableName, tableDefinition);

                    client.getSyncContext().initialize(localStore, new SimpleSyncHandler()).get();

                    MobileServiceSyncTable<OfflineReadyItem> localTable = client.getSyncTable(tableName, OfflineReadyItem.class);

                    MobileServiceTable<OfflineReadyItem> remoteTable = client.getTable(tableName, OfflineReadyItem.class);

                    OfflineReadyItem item = new OfflineReadyItem(new Random());

                    log("Inserted the item to the local store:" + item);

                    item = localTable.insert(item).get();

                    log("Validating that the item is not in the server table");

                    try {
                        requestsSentToServer++;
                        remoteTable.lookUp(item.getId()).get();
                        log("Error, item is present in the server");
                        // return false;
                    } catch (ExecutionException ex) {
                        log("Ok, item is not in the server:" + ex.getMessage());
                    }

                    if (!validateRequestCount(this, serviceFilter.requestCount, requestsSentToServer)) {
                        result.setStatus(TestStatus.Failed);
                        callback.onTestComplete(this, result);
                    }

                    if (client.getSyncContext().getPendingOperations() != 1) {
                        result.setStatus(TestStatus.Failed);
                        callback.onTestComplete(this, result);
                    }

                    log("Pushing changes to the server");

                    try {
                        client.getSyncContext().push().get();
                    } catch (Exception ex2) {

                        requestsSentToServer += 1;

                        log("Push should throw error");

                        if (client.getSyncContext().getPendingOperations() != 1) {
                            result.setStatus(TestStatus.Failed);
                            callback.onTestComplete(this, result);
                        }

                        log("adding a delete operation that will not collapse");
                        localTable.delete(item).get();

                        if (client.getSyncContext().getPendingOperations() != 1) {
                            result.setStatus(TestStatus.Failed);
                            callback.onTestComplete(this, result);
                        }

                    }

                    if (!validateRequestCount(this, serviceFilter.requestCount, requestsSentToServer)) {
                        result.setStatus(TestStatus.Failed);
                        callback.onTestComplete(this, result);
                    }

                    log("Cleaning up");

                    client.getContext().deleteDatabase(OFFLINE_TABLE_NAME);

                    log("Done");

                    callback.onTestComplete(this, result);

                } catch (Exception e) {
                    callback.onTestComplete(this, createResultFromException(e));
                    return;
                } catch (Throwable e) {
                    callback.onTestComplete(this, createResultFromException((Exception) e));
                    return;
                }
            }

            ;
        };

        test.setName(name);

        return test;
    }


    private TestCase issue536() {

        final String tableName = "INSTA";

        final TestCase test = new TestCase() {

            @Override
            protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {

                try {

                    TestCase testCase = this;
                    TestResult result = new TestResult();
                    result.setStatus(TestStatus.Passed);
                    result.setTestCase(testCase);

                    ServiceFilterWithRequestCount serviceFilter = new ServiceFilterWithRequestCount();

                    int requestsSentToServer = 0;

                    client = client.withFilter(serviceFilter);

                    SQLiteLocalStore localStore = new SQLiteLocalStore(client.getContext(), OFFLINE_TABLE_NAME, null, 1);

                    SimpleSyncHandler handler = new SimpleSyncHandler();
                    MobileServiceSyncContext syncContext = client.getSyncContext();

                    syncContext.initialize(localStore, handler).get();

                    log("Defined the table on the local store");

                    Map<String, ColumnDataType> instaTableDefinition = new HashMap<String, ColumnDataType>();
                    instaTableDefinition.put("id", ColumnDataType.String);
                    instaTableDefinition.put("inspectedDateTime", ColumnDataType.Date);
                    instaTableDefinition.put("inventory", ColumnDataType.String);
                    instaTableDefinition.put("wall", ColumnDataType.String);
                    instaTableDefinition.put("floor", ColumnDataType.String);
                    instaTableDefinition.put("ceiling", ColumnDataType.String);
                    instaTableDefinition.put("inspected", ColumnDataType.Boolean);
                    instaTableDefinition.put("rejected", ColumnDataType.Boolean);
                    instaTableDefinition.put("inspectionId", ColumnDataType.String);
                    instaTableDefinition.put("levelsInInspectionId", ColumnDataType.String);

                    log("Initialized the store and sync context");

                    localStore.defineTable(tableName, instaTableDefinition);

                    client.getSyncContext().initialize(localStore, new SimpleSyncHandler()).get();

                    MobileServiceSyncTable<InstaItem> localTable = client.getSyncTable(tableName, InstaItem.class);

                    MobileServiceTable<InstaItem> remoteTable = client.getTable(tableName, InstaItem.class);

                    InstaItem item = new InstaItem(new Random());

                    log("Inserted the item to the local store:" + item);

                    item = localTable.insert(item).get();

                    item = localTable.lookUp(item.getId()).get();

                    log("Validating that the item is not in the server table");

                    try {
                        requestsSentToServer++;
                        remoteTable.lookUp(item.getId()).get();
                        log("Error, item is present in the server");
                        // return false;
                    } catch (ExecutionException ex) {
                        log("Ok, item is not in the server:" + ex.getMessage());
                    }

                    if (!validateRequestCount(this, serviceFilter.requestCount, requestsSentToServer)) {
                        result.setStatus(TestStatus.Failed);
                        callback.onTestComplete(this, result);
                    }

                    log("Pushing changes to the server");
                    client.getSyncContext().push().get();
                    requestsSentToServer++;

                    if (!validateRequestCount(this, serviceFilter.requestCount, requestsSentToServer)) {
                        result.setStatus(TestStatus.Failed);
                        callback.onTestComplete(this, result);
                    }
                    log("Push done; now verifying that item is in the server");

                    InstaItem serverItem = remoteTable.lookUp(item.getId()).get();
                    requestsSentToServer++;

                    log("Retrieved item from server:" + serverItem);

                    if (serverItem.equals(item)) {
                        log("Items are the same");
                    } else {
                        log("Items are different. Local: " + item + "; remote: " + serverItem);
                        result.setStatus(TestStatus.Failed);
                        callback.onTestComplete(this, result);
                        return;
                    }

                    log("Now updating the item locally");

                    item.setInspected(!item.getInspected());

                    localTable.update(item).get();

                    log("Item has been updated");

                    InstaItem newItem = new InstaItem(new Random());

                    log("Adding a new item to the local table:" + newItem);
                    newItem = localTable.insert(newItem).get();

                    if (!validateRequestCount(this, serviceFilter.requestCount, requestsSentToServer)) {
                        result.setStatus(TestStatus.Failed);
                        callback.onTestComplete(this, result);
                    }

                    log("Pushing the new changes to the server");
                    client.getSyncContext().push().get();
                    requestsSentToServer += 2;

                    if (!validateRequestCount(this, serviceFilter.requestCount, requestsSentToServer)) {
                        result.setStatus(TestStatus.Failed);
                        callback.onTestComplete(this, result);
                    }

                    log("Push done. Verifying changes on the server");
                    serverItem = remoteTable.lookUp(item.getId()).get();
                    requestsSentToServer++;

                    if (serverItem.equals(item)) {
                        log("Updated items are the same");
                    } else {
                        log("Items are different. Local: " + item + "; remote: " + serverItem);
                        result.setStatus(TestStatus.Failed);
                        callback.onTestComplete(this, result);
                        return;
                    }

                    serverItem = remoteTable.lookUp(newItem.getId()).get();
                    requestsSentToServer++;

                    if (serverItem.equals(newItem)) {
                        log("New inserted item is the same");
                    } else {
                        log("Items are different. Local: " + item + "; remote: " + serverItem);
                        result.setStatus(TestStatus.Failed);
                        callback.onTestComplete(this, result);
                        return;
                    }

                    log("Cleaning up");
                    localTable.delete(item).get();
                    localTable.delete(newItem).get();
                    log("Local table cleaned up. Now sync'ing once more");
                    client.getSyncContext().push().get();

                    requestsSentToServer += 2;
                    if (!validateRequestCount(this, serviceFilter.requestCount, requestsSentToServer)) {
                        result.setStatus(TestStatus.Failed);
                        callback.onTestComplete(this, result);
                        return;
                    }

                    log("Done");

                    callback.onTestComplete(this, result);

                } catch (Exception e) {
                    callback.onTestComplete(this, createResultFromException(e));
                    return;
                }
            }

            ;
        };

        test.setName("issue536");

        return test;
    }

    private TestCase issue417() {

        final String tableName = "Person";

        final TestCase test = new TestCase() {

            @Override
            protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {

                try {

                    TestCase testCase = this;
                    TestResult result = new TestResult();
                    result.setStatus(TestStatus.Passed);
                    result.setTestCase(testCase);

                    ServiceFilterWithRequestCount serviceFilter = new ServiceFilterWithRequestCount();

                    int requestsSentToServer = 0;

                    client = client.withFilter(serviceFilter);

                    SQLiteLocalStore localStore = new SQLiteLocalStore(client.getContext(), OFFLINE_TABLE_NAME, null, 1);

                    SimpleSyncHandler handler = new SimpleSyncHandler();
                    MobileServiceSyncContext syncContext = client.getSyncContext();

                    syncContext.initialize(localStore, handler).get();

                    log("Defined the table on the local store");

                    Map<String, ColumnDataType> instaTableDefinition = new HashMap<String, ColumnDataType>();
                    instaTableDefinition.put("id", ColumnDataType.String);
                    instaTableDefinition.put("name", ColumnDataType.String);
                    instaTableDefinition.put("age", ColumnDataType.Integer);

                    log("Initialized the store and sync context");

                    localStore.defineTable(tableName, instaTableDefinition);

                    client.getSyncContext().initialize(localStore, new SimpleSyncHandler()).get();

                    MobileServiceSyncTable<PersonItem> localTable = client.getSyncTable(tableName, PersonItem.class);

                    MobileServiceTable<PersonItem> remoteTable = client.getTable(tableName, PersonItem.class);

                    String testFilter = UUID.randomUUID().toString();

                    /*for(int i = 0; i < 50; i++) {
                        PersonItem item = new PersonItem(new Random(), "partition");

                        log("Inserted the item to the local store:" + item);

                        item.setName(testFilter);
                        localTable.insert(item).get();

                    }

                    log("Pushing changes to the server");
                    client.getSyncContext().push().get();
                    log("Push done; now verifying that item is in the server");
*/

                    Query query =
                            QueryOperations
                                    .tableName(tableName)
                                            //.field("name").eq(testFilter)
                                    .top(5);


                    MobileServiceList<PersonItem> personItems = remoteTable.execute(query).get();

                    while (personItems.getNextLink() != null) {
                        log("Querying " + personItems.getNextLink());
                        personItems = remoteTable.execute(personItems.getNextLink()).get();
                    }

                    log("Done");

                    callback.onTestComplete(this, result);

                } catch (Exception e) {
                    callback.onTestComplete(this, createResultFromException(e));
                    return;
                }
            }

            ;
        };

        test.setName("issue417");

        return test;
    }

    private TestCase createLocallyDeleteAlreadyDeletedElementTest() {

        final String tableName = "offlineReady";

        final TestCase test = new TestCase() {

            @Override
            protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {

                try {

                    TestCase testCase = this;
                    TestResult result = new TestResult();
                    result.setStatus(TestStatus.Passed);
                    result.setTestCase(testCase);

                    ServiceFilterWithRequestCount serviceFilter = new ServiceFilterWithRequestCount();

                    int requestsSentToServer = 0;

                    client = client.withFilter(serviceFilter);

                    SQLiteLocalStore localStore = new SQLiteLocalStore(client.getContext(), OFFLINE_TABLE_NAME, null, 1);

                    SimpleSyncHandler handler = new SimpleSyncHandler();
                    MobileServiceSyncContext syncContext = client.getSyncContext();

                    syncContext.initialize(localStore, handler).get();

                    log("Defined the table on the local store");

                    Map<String, ColumnDataType> tableDefinition = new HashMap<String, ColumnDataType>();
                    tableDefinition.put("id", ColumnDataType.String);
                    tableDefinition.put("name", ColumnDataType.String);
                    tableDefinition.put("age", ColumnDataType.Integer);
                    tableDefinition.put("float", ColumnDataType.Real);
                    tableDefinition.put("date", ColumnDataType.Date);
                    tableDefinition.put("bool", ColumnDataType.Boolean);
                    tableDefinition.put("__version", ColumnDataType.String);

                    log("Initialized the store and sync context");

                    localStore.defineTable(tableName, tableDefinition);

                    client.getSyncContext().initialize(localStore, new SimpleSyncHandler()).get();

                    MobileServiceSyncTable<OfflineReadyItem> localTable = client.getSyncTable(tableName, OfflineReadyItem.class);

                    MobileServiceTable<OfflineReadyItem> remoteTable = client.getTable(tableName, OfflineReadyItem.class);

                    OfflineReadyItem item = new OfflineReadyItem(new Random());

                    log("Inserted the item to the local store:" + item);

                    item = localTable.insert(item).get();

                    log("Validating that the item is not in the server table");

                    try {
                        requestsSentToServer++;
                        remoteTable.lookUp(item.getId()).get();
                        log("Error, item is present in the server");
                        // return false;
                    } catch (ExecutionException ex) {
                        log("Ok, item is not in the server:" + ex.getMessage());
                    }

                    if (!validateRequestCount(this, serviceFilter.requestCount, requestsSentToServer)) {
                        result.setStatus(TestStatus.Failed);
                        callback.onTestComplete(this, result);
                    }

                    log("Pushing changes to the server");
                    client.getSyncContext().push().get();
                    requestsSentToServer++;

                    if (!validateRequestCount(this, serviceFilter.requestCount, requestsSentToServer)) {
                        result.setStatus(TestStatus.Failed);
                        callback.onTestComplete(this, result);
                    }

                    log("Push done; now verifying that item is in the server");

                    OfflineReadyItem serverItem = remoteTable.lookUp(item.getId()).get();
                    requestsSentToServer++;

                    log("Retrieved item from server:" + serverItem);

                    if (serverItem.equals(item)) {
                        log("Items are the same");
                    } else {
                        log("Items are different. Local: " + item + "; remote: " + serverItem);
                        result.setStatus(TestStatus.Failed);
                        callback.onTestComplete(this, result);
                        return;
                    }

                    log("Delete the remote item");

                    remoteTable.delete(serverItem).get();

                    log("Delete the local item");

                    localTable.delete(item).get();

                    log("Item has been deleted");

                    log("Pushing the new changes to the server");
                    client.getSyncContext().push().get();
                    requestsSentToServer += 2;

                    if (!validateRequestCount(this, serviceFilter.requestCount, requestsSentToServer)) {
                        result.setStatus(TestStatus.Failed);
                        callback.onTestComplete(this, result);
                    }

                    log("Done");

                    callback.onTestComplete(this, result);

                } catch (Exception e) {
                    callback.onTestComplete(this, createResultFromException(e));
                    return;
                }
            }

            ;
        };

        test.setName("Locally delete an already deleted element Test");

        return test;
    }

    private TestCase createInsertDuplicatedElementTest() {

        final String tableName = "offlineReady";

        final TestCase test = new TestCase() {

            @Override
            protected void executeTest(MobileServiceClient offlineReadyClient, final TestExecutionCallback callback) {

                TestCase testCase = this;
                TestResult result = new TestResult();
                result.setStatus(TestStatus.Passed);
                result.setTestCase(testCase);
                try {

                    SQLiteLocalStore localStore = new SQLiteLocalStore(offlineReadyClient.getContext(), OFFLINE_TABLE_NAME, null, 1);

                    log("Defined the table on the local store");

                    Map<String, ColumnDataType> tableDefinition = new HashMap<String, ColumnDataType>();
                    tableDefinition.put("id", ColumnDataType.String);
                    tableDefinition.put("name", ColumnDataType.String);
                    tableDefinition.put("age", ColumnDataType.Integer);
                    tableDefinition.put("float", ColumnDataType.Real);
                    tableDefinition.put("date", ColumnDataType.Date);
                    tableDefinition.put("bool", ColumnDataType.Boolean);
                    tableDefinition.put("__version", ColumnDataType.String);

                    log("Initialized the store and sync context");

                    localStore.defineTable(tableName, tableDefinition);

                    offlineReadyClient.getSyncContext().initialize(localStore, new SimpleSyncHandler()).get();

                    MobileServiceSyncTable<OfflineReadyItem> localTable = offlineReadyClient.getSyncTable(tableName, OfflineReadyItem.class);

                    MobileServiceTable<OfflineReadyItem> remoteTable = offlineReadyClient.getTable(tableName, OfflineReadyItem.class);

                    localTable.purge(null).get();

                    log("Removed all items from the local table");

                    OfflineReadyItem item = new OfflineReadyItem(new Random());

                    item = remoteTable.insert(item).get();

                    log("Inserted the item to the remote store:" + item);

                    Query pullQuery = QueryOperations.tableName(tableName).field("id").eq(item.getId());

                    localTable.pull(pullQuery).get();

                    log("Pull changes from server");


                    try {
                        item = localTable.insert(item).get();
                    } catch (Exception ex) {

                        MobileServiceLocalStoreException mslse = (MobileServiceLocalStoreException) ex.getCause();

                        if (mslse == null) {
                            log("Expected exception was not thrown.");
                            result.setStatus(TestStatus.Failed);
                            callback.onTestComplete(this, result);
                        }
                        log("Expected exception was thrown.");
                    }

                    log("Cleaning up");
                    localTable.delete(item).get();
                    log("Local table cleaned up. Now sync'ing once more");
                    offlineReadyClient.getSyncContext().push().get();
                    log("Done");

                    callback.onTestComplete(this, result);

                } catch (Exception e) {
                    callback.onTestComplete(this, createResultFromException(e));
                    return;
                }
            }

            ;
        };

        test.setName("Offline - Insert duplicated element Test");

        return test;
    }


    private TestCase createDeleteSyncConflict() {

        final String tableName = "offlineReady";

        final TestCase test = new TestCase() {

            @Override
            protected void executeTest(MobileServiceClient offlineReadyClient, final TestExecutionCallback callback) {

                TestCase testCase = this;
                TestResult result = new TestResult();
                result.setStatus(TestStatus.Passed);
                result.setTestCase(testCase);
                try {

                    SQLiteLocalStore localStore = new SQLiteLocalStore(offlineReadyClient.getContext(), OFFLINE_TABLE_NAME, null, 1);

                    log("Defined the table on the local store");

                    Map<String, ColumnDataType> tableDefinition = new HashMap<String, ColumnDataType>();
                    tableDefinition.put("id", ColumnDataType.String);
                    tableDefinition.put("name", ColumnDataType.String);
                    tableDefinition.put("age", ColumnDataType.Integer);
                    tableDefinition.put("float", ColumnDataType.Real);
                    tableDefinition.put("date", ColumnDataType.Date);
                    tableDefinition.put("bool", ColumnDataType.Boolean);
                    tableDefinition.put("__version", ColumnDataType.String);

                    log("Initialized the store and sync context");

                    localStore.defineTable(tableName, tableDefinition);

                    MobileServiceSyncTable<OfflineReadyItem> localTable = offlineReadyClient.getSyncTable(tableName, OfflineReadyItem.class);

                    MobileServiceTable<OfflineReadyItem> remoteTable = offlineReadyClient.getTable(tableName, OfflineReadyItem.class);

                    offlineReadyClient.getSyncContext().initialize(localStore, new SimpleSyncHandler()).get();

                    localTable.purge(null).get();
                    log("Removed all items from the local table");

                    OfflineReadyItem item = new OfflineReadyItem(new Random());

                    item = localTable.insert(item).get();

                    log("Inserted the item to the local store:" + item);

                    Query pullQuery = QueryOperations.tableName(tableName).field("id").eq(item.getId());

                    localTable.pull(pullQuery).get();

                    log("Changing the item on the server");

                    item.setFlag(!item.getFlag());

                    item = remoteTable.update(item).get();

                    log("Updated the item: " + item);

                    OfflineReadyItem localItem = localTable.lookUp(item.getId()).get();
                    OfflineReadyItem serverItem = remoteTable.lookUp(item.getId()).get();

                    log("Retrieved the item from the local table, now updating it");

                    localItem.setDate(new Date());

                    localTable.update(localItem).get();

                    log("Updated the item on the local table");

                    localTable.delete(localItem);

                    log("Now trying to pull changes from the server (will trigger a push)");

                    try {
                        localTable.pull(pullQuery).get();
                        log("Error, pull (push) should have caused a conflict, but none happened.");
                        result.setStatus(TestStatus.Failed);
                        callback.onTestComplete(this, result);
                        return;
                    } catch (Exception ex) {
                        log("Push exception: " + ex);
                        log("Expected exception was thrown.");

                        MobileServicePushFailedException mspfe = (MobileServicePushFailedException) ex.getCause();

                        if (mspfe == null || mspfe.getPushCompletionResult().getOperationErrors().size() != 1) {
                            result.setStatus(TestStatus.Failed);
                            callback.onTestComplete(this, result);
                            return;
                        }

                        TableOperationError tableOperationError = mspfe.getPushCompletionResult().getOperationErrors().get(0);

                        try {

                            log("Count pending operations");

                            if (offlineReadyClient.getSyncContext().getPendingOperations() != 1) {
                                log("Expected 1 pending operations");

                                result.setStatus(TestStatus.Failed);
                                callback.onTestComplete(this, result);
                                return;
                            }

                            log("Cleaning operation");

                            try {
                                offlineReadyClient.getSyncContext().cancelAndUpdateItem(tableOperationError);
                            } catch (Throwable throwable) {
                                result.setStatus(TestStatus.Failed);
                                callback.onTestComplete(this, result);
                                return;
                            }

                        } catch (Throwable throwable) {
                            result.setStatus(TestStatus.Failed);
                            callback.onTestComplete(this, result);
                            return;
                        }
                    }

                    log("Cleaning up");

                    localTable.delete(serverItem).get();

                    log("Local table cleaned up. Now sync'ing once more");
                    offlineReadyClient.getSyncContext().push().get();
                    log("Done");

                    callback.onTestComplete(this, result);

                } catch (Exception e) {
                    callback.onTestComplete(this, createResultFromException(e));
                    return;
                }
            }
        };

        test.setName("Offline - Test delete a locally updated item");

        return test;
    }

    private TestCase createSyncConflictTest(final boolean autoResolve) {

        final String tableName = "offlineReady";

        final TestCase test = new TestCase() {

            @Override
            protected void executeTest(MobileServiceClient offlineReadyClient, final TestExecutionCallback callback) {

                TestCase testCase = this;
                TestResult result = new TestResult();
                result.setStatus(TestStatus.Passed);
                result.setTestCase(testCase);
                try {

                    boolean resolveConflictsOnClient = autoResolve;

                    SQLiteLocalStore localStore = new SQLiteLocalStore(offlineReadyClient.getContext(), OFFLINE_TABLE_NAME, null, 1);

                    log("Defined the table on the local store");

                    Map<String, ColumnDataType> tableDefinition = new HashMap<String, ColumnDataType>();
                    tableDefinition.put("id", ColumnDataType.String);
                    tableDefinition.put("name", ColumnDataType.String);
                    tableDefinition.put("age", ColumnDataType.Integer);
                    tableDefinition.put("float", ColumnDataType.Real);
                    tableDefinition.put("date", ColumnDataType.Date);
                    tableDefinition.put("bool", ColumnDataType.Boolean);
                    tableDefinition.put("__version", ColumnDataType.String);

                    log("Initialized the store and sync context");

                    localStore.defineTable(tableName, tableDefinition);

                    MobileServiceSyncTable<OfflineReadyItem> localTable = offlineReadyClient.getSyncTable(tableName, OfflineReadyItem.class);

                    MobileServiceTable<OfflineReadyItem> remoteTable = offlineReadyClient.getTable(tableName, OfflineReadyItem.class);

                    ConflictResolvingSyncHandler conflictResolvingSyncHandler = new ConflictResolvingSyncHandler(this, offlineReadyClient);

                    if (resolveConflictsOnClient) {
                        offlineReadyClient.getSyncContext().initialize(localStore, conflictResolvingSyncHandler).get();
                    } else {
                        offlineReadyClient.getSyncContext().initialize(localStore, new SimpleSyncHandler()).get();
                    }

                    localTable.purge(null).get();
                    log("Removed all items from the local table");

                    OfflineReadyItem item = new OfflineReadyItem(new Random());

                    item = localTable.insert(item).get();

                    log("Inserted the item to the local store:" + item);

                    Query pullQuery = QueryOperations.tableName(tableName).field("id").eq(item.getId());

                    localTable.pull(pullQuery).get();

                    log("Changing the item on the server");

                    item.setFlag(!item.getFlag());

                    item = remoteTable.update(item).get();

                    log("Updated the item: " + item);

                    OfflineReadyItem localItem = localTable.lookUp(item.getId()).get();

                    log("Retrieved the item from the local table, now updating it");

                    localItem.setDate(new Date());

                    localTable.update(localItem).get();
                    log("Updated the item on the local table");

                    log("Now trying to pull changes from the server (will trigger a push)");

                    try {
                        localTable.pull(pullQuery).get();
                        if (!autoResolve) {
                            log("Error, pull (push) should have caused a conflict, but none happened.");
                            result.setStatus(TestStatus.Failed);
                            callback.onTestComplete(this, result);
                            return;
                        } else {
                            OfflineReadyItem expectedMergedItem = conflictResolvingSyncHandler.conflictResolution(localItem, item);
                            OfflineReadyItem localMergedItem = localTable.lookUp(item.getId()).get();
                            if (localMergedItem.equals(expectedMergedItem)) {
                                log("Item was merged correctly.");
                            } else {
                                log("Error, item not merged correctly. Expected: " + expectedMergedItem + " Actual: " + localMergedItem);
                                result.setStatus(TestStatus.Failed);
                                callback.onTestComplete(this, result);
                                return;
                            }
                        }
                    } catch (Exception ex) {
                        log("Push exception: " + ex);

                        MobileServicePushFailedException mspfe = (MobileServicePushFailedException) ex.getCause();

                        if (autoResolve) {
                            log("Error, push should have succeeded.");
                            result.setStatus(TestStatus.Failed);
                            callback.onTestComplete(this, result);
                            return;
                        } else {
                            log("Expected exception was thrown.");

                            try {
                                offlineReadyClient.getSyncContext().cancelAndUpdateItem(mspfe.getPushCompletionResult().getOperationErrors().get(0));
                            } catch (Throwable throwable) {
                                log("Error, cancel And Update Item should have succeeded.");
                                result.setStatus(TestStatus.Failed);
                                callback.onTestComplete(this, result);
                                return;
                            }
                        }
                    }

                    log("Cleaning up");

                    localTable.delete(item).get();

                    log("Local table cleaned up. Now sync'ing once more");
                    offlineReadyClient.getSyncContext().push().get();
                    log("Done");

                    callback.onTestComplete(this, result);

                } catch (Exception e) {
                    callback.onTestComplete(this, createResultFromException(e));
                    return;
                }
            }

            ;
        };

        test.setName("Offline - dealing with conflicts - " + (autoResolve ? "client resolves conflicts" : "push fails after conflicts"));

        return test;
    }

    private TestCase createSyncConflictAndResolveWithMethodTest(final boolean useCancelAndUpdateItem) {

        final String tableName = "offlineReady";

        final TestCase test = new TestCase() {

            @Override
            protected void executeTest(MobileServiceClient offlineReadyClient, final TestExecutionCallback callback) {

                TestCase testCase = this;
                TestResult result = new TestResult();
                result.setStatus(TestStatus.Passed);
                result.setTestCase(testCase);
                try {

                    SQLiteLocalStore localStore = new SQLiteLocalStore(offlineReadyClient.getContext(), OFFLINE_TABLE_NAME, null, 1);

                    log("Defined the table on the local store");

                    Map<String, ColumnDataType> tableDefinition = new HashMap<String, ColumnDataType>();
                    tableDefinition.put("id", ColumnDataType.String);
                    tableDefinition.put("name", ColumnDataType.String);
                    tableDefinition.put("age", ColumnDataType.Integer);
                    tableDefinition.put("float", ColumnDataType.Real);
                    tableDefinition.put("date", ColumnDataType.Date);
                    tableDefinition.put("bool", ColumnDataType.Boolean);
                    tableDefinition.put("__version", ColumnDataType.String);

                    log("Initialized the store and sync context");

                    localStore.defineTable(tableName, tableDefinition);

                    MobileServiceSyncTable<OfflineReadyItem> localTable = offlineReadyClient.getSyncTable(tableName, OfflineReadyItem.class);

                    MobileServiceTable<OfflineReadyItem> remoteTable = offlineReadyClient.getTable(tableName, OfflineReadyItem.class);

                    ConflictResolvingSyncHandler conflictResolvingSyncHandler = new ConflictResolvingSyncHandler(this, offlineReadyClient);

                    offlineReadyClient.getSyncContext().initialize(localStore, new SimpleSyncHandler()).get();

                    localTable.purge(null).get();
                    log("Removed all items from the local table");

                    OfflineReadyItem item = new OfflineReadyItem(new Random());

                    item = localTable.insert(item).get();

                    log("Inserted the item to the local store:" + item);

                    Query pullQuery = QueryOperations.tableName(tableName).field("id").eq(item.getId());

                    localTable.pull(pullQuery).get();

                    log("Changing the item on the server");

                    item.setFlag(!item.getFlag());

                    item = remoteTable.update(item).get();

                    log("Updated the item: " + item);

                    OfflineReadyItem localItem = localTable.lookUp(item.getId()).get();
                    OfflineReadyItem serverItem = remoteTable.lookUp(item.getId()).get();

                    log("Retrieved the item from the local table, now updating it");

                    localItem.setDate(new Date());

                    localTable.update(localItem).get();
                    log("Updated the item on the local table");

                    log("Now trying to pull changes from the server (will trigger a push)");

                    try {
                        localTable.pull(pullQuery).get();
                        log("Error, pull (push) should have caused a conflict, but none happened.");
                        result.setStatus(TestStatus.Failed);
                        callback.onTestComplete(this, result);
                        return;
                    } catch (Exception ex) {
                        log("Push exception: " + ex);
                        log("Expected exception was thrown.");

                        MobileServicePushFailedException mspfe = (MobileServicePushFailedException) ex.getCause();

                        if (mspfe == null || mspfe.getPushCompletionResult().getOperationErrors().size() != 1) {
                            result.setStatus(TestStatus.Failed);
                            callback.onTestComplete(this, result);
                            return;
                        }

                        TableOperationError tableOperationError = mspfe.getPushCompletionResult().getOperationErrors().get(0);

                        try {

                            log("Count pending operations");

                            if (offlineReadyClient.getSyncContext().getPendingOperations() != 1) {
                                log("Expected 1 pending operations");

                                result.setStatus(TestStatus.Failed);
                                callback.onTestComplete(this, result);
                                return;
                            }
                        } catch (Throwable throwable) {
                            result.setStatus(TestStatus.Failed);
                            callback.onTestComplete(this, result);
                            return;
                        }

                        if (useCancelAndUpdateItem) {
                            try {
                                offlineReadyClient.getSyncContext().cancelAndUpdateItem(tableOperationError);
                            } catch (Throwable throwable) {
                                throwable.printStackTrace();
                            }

                            OfflineReadyItem resolvedLocalItem = localTable.lookUp(tableOperationError.getItemId()).get();

                            if (!serverItem.equals(resolvedLocalItem)) {
                                result.setStatus(TestStatus.Failed);
                                result.setStatus(TestStatus.Failed);
                                callback.onTestComplete(this, result);
                                return;
                            }

                        } else {
                            try {
                                offlineReadyClient.getSyncContext().cancelAndDiscardItem(tableOperationError);
                            } catch (Throwable throwable) {
                                result.setStatus(TestStatus.Failed);
                                callback.onTestComplete(this, result);
                                return;
                            }

                            OfflineReadyItem resolvedItem = localTable.lookUp(tableOperationError.getItemId()).get();

                            if (resolvedItem != null) {
                                result.setStatus(TestStatus.Failed);
                                callback.onTestComplete(this, result);
                            }
                        }
                    }

                    try {
                        if (offlineReadyClient.getSyncContext().getPendingOperations() != 0) {
                            log("Expected 0 pending operations");

                            result.setStatus(TestStatus.Failed);
                            callback.onTestComplete(this, result);
                            return;
                        }
                    } catch (Throwable throwable) {
                        result.setStatus(TestStatus.Failed);
                        callback.onTestComplete(this, result);
                        return;
                    }

                    log("Cleaning up");
                    localTable.delete(item).get();

                    log("Local table cleaned up. Now sync'ing once more");
                    offlineReadyClient.getSyncContext().push().get();
                    log("Done");

                    callback.onTestComplete(this, result);

                } catch (Exception e) {
                    callback.onTestComplete(this, createResultFromException(e));
                    return;
                }
            }
        };

        test.setName("Offline - dealing with conflicts - with " + (useCancelAndUpdateItem ? "cancelAndUpdateItem" : "cancelAndDiscardItem"));

        return test;
    }

    private TestCase createSyncTestForAuthenticatedTable(final boolean isLoggedIn) {

        final String tableName = "offlineReadyAuthenticated";

        final TestCase test = new TestCase() {

            @Override
            protected void executeTest(MobileServiceClient offlineReadyClient, final TestExecutionCallback callback) {

                try {

                    TestCase test = this;
                    TestResult result = new TestResult();
                    result.setStatus(TestStatus.Passed);
                    result.setTestCase(test);

                    Calendar now = Calendar.getInstance();
                    int seed = (now.get(Calendar.YEAR) - 1900) * 10000 + now.get(Calendar.MONTH) * 100 + now.get(Calendar.DAY_OF_MONTH);
                    test.log("Using random seed: " + seed);
                    Random rndGen = new Random(seed);

                    SQLiteLocalStore localStore = new SQLiteLocalStore(offlineReadyClient.getContext(), OFFLINE_TABLE_NAME, null, 1);

                    log("Defined the table on the local store");

                    Map<String, ColumnDataType> tableDefinition = new HashMap<String, ColumnDataType>();
                    tableDefinition.put("id", ColumnDataType.String);
                    tableDefinition.put("name", ColumnDataType.String);
                    tableDefinition.put("age", ColumnDataType.Integer);
                    tableDefinition.put("float", ColumnDataType.Real);
                    tableDefinition.put("date", ColumnDataType.Date);
                    tableDefinition.put("bool", ColumnDataType.Boolean);
                    tableDefinition.put("__version", ColumnDataType.String);

                    log("Initialized the store and sync context");

                    localStore.defineTable(tableName, tableDefinition);

                    offlineReadyClient.getSyncContext().initialize(localStore, new SimpleSyncHandler()).get();

                    MobileServiceSyncTable<OfflineReadyItem> localTable = offlineReadyClient.getSyncTable(tableName, OfflineReadyItem.class);
                    MobileServiceTable<OfflineReadyItem> remoteTable = offlineReadyClient.getTable(tableName, OfflineReadyItem.class);

                    OfflineReadyItem item = new OfflineReadyItem(rndGen);
                    item = localTable.insert(item).get();
                    test.log("Inserted the item to the local store: " + item);

                    try {
                        offlineReadyClient.getSyncContext().push().get();
                        test.log("Pushed the changes to the server");
                        if (isLoggedIn) {
                            test.log("As expected, push succeeded");
                        } else {
                            test.log("Error, table should only work with authenticated access, but user is not logged in");
                            result.setStatus(TestStatus.Failed);
                            callback.onTestComplete(this, result);
                            return;
                        }
                    } catch (Throwable exception) {

                        MobileServicePushFailedException ex = (MobileServicePushFailedException) exception.getCause();

                        if (isLoggedIn) {
                            test.log("Error, user is logged in but push operation failed: " + ex);
                            result.setStatus(TestStatus.Failed);
                            callback.onTestComplete(this, result);
                            return;
                        }

                        test.log("Got expected exception: " + ex.getMessage());
                        // Exception inner = ex.getCause();
                        // while (inner != null) {
                        // test.log("  {0}: {1}", inner.GetType().FullName,
                        // inner.Message);
                        // inner = inner.InnerException;
                        // }
                    }

                    if (!isLoggedIn) {
                        test.log("Push should have failed, so now will try to log in to complete the push operation");
                        offlineReadyClient.login(MobileServiceAuthenticationProvider.Facebook).get();
                        test.log("Logged in as " + offlineReadyClient.getCurrentUser().getUserId());
                        offlineReadyClient.getSyncContext().push().get();
                        test.log("Push succeeded");
                    }

                    localTable.purge(null).get();
                    test.log("Purged the local table");
                    Query query = QueryOperations.field("id").eq(item.getId());
                    localTable.pull(query).get();
                    test.log("Pulled the data into the local table");
                    List<OfflineReadyItem> serverItems = localTable.read(null).get();
                    test.log("Retrieved items from the local table");

                    test.log("Removing item from the remote table");
                    remoteTable.delete(item).get();

                    if (!isLoggedIn) {
                        offlineReadyClient.logout();
                        test.log("Logged out again");
                    }

                    OfflineReadyItem firstServerItem = serverItems.get(0);

                    if (item.equals(firstServerItem)) {
                        test.log("Data round-tripped successfully");
                    } else {
                        test.log("Error, data did not round-trip successfully. Expected: " + item + ", actual: " + firstServerItem);
                        result.setStatus(TestStatus.Failed);
                        callback.onTestComplete(this, result);
                        return;
                    }

                    test.log("Cleaning up");
                    localTable.purge(null).get();
                    test.log("Done");

                    callback.onTestComplete(this, result);
                } catch (Exception e) {
                    callback.onTestComplete(this, createResultFromException(e));
                    return;
                }
            }

            ;
        };

        test.setName("Sync test for authenticated table, with user " + (isLoggedIn ? "logged in" : "not logged in"));

        return test;
    }


    private TestCase createNoOptimisticConcurrencyTest() {

        final String tableName = "offlineReadyAuthenticated";

        // If a table does not have a __version column, then offline will still
        // work, but there will be no conflicts
        final TestCase test = new TestCase() {

            @Override
            protected void executeTest(MobileServiceClient offlineReadyClient, final TestExecutionCallback callback) {

                try {

                    TestCase test = this;
                    TestResult result = new TestResult();
                    result.setStatus(TestStatus.Passed);
                    result.setTestCase(test);

                    Calendar now = Calendar.getInstance();
                    int seed = (now.get(Calendar.YEAR) - 1900) * 10000 + now.get(Calendar.MONTH) * 100 + now.get(Calendar.DAY_OF_MONTH);
                    test.log("Using random seed: " + seed);
                    Random rndGen = new Random(seed);

                    SQLiteLocalStore localStore = new SQLiteLocalStore(offlineReadyClient.getContext(), OFFLINE_TABLE_NAME, null, 1);

                    log("Defined the table on the local store");

                    Map<String, ColumnDataType> tableDefinition = new HashMap<String, ColumnDataType>();
                    tableDefinition.put("id", ColumnDataType.String);
                    tableDefinition.put("name", ColumnDataType.String);
                    tableDefinition.put("age", ColumnDataType.Integer);
                    tableDefinition.put("float", ColumnDataType.Real);
                    tableDefinition.put("date", ColumnDataType.Date);
                    tableDefinition.put("bool", ColumnDataType.Boolean);

                    log("Initialized the store and sync context");

                    localStore.defineTable(tableName, tableDefinition);

                    offlineReadyClient.getSyncContext().initialize(localStore, new SimpleSyncHandler()).get();

                    MobileServiceSyncTable<OfflineReadyItemNoVersion> localTable = offlineReadyClient.getSyncTable(tableName, OfflineReadyItemNoVersion.class);
                    MobileServiceTable<OfflineReadyItemNoVersion> remoteTable = offlineReadyClient.getTable(tableName, OfflineReadyItemNoVersion.class);

                    OfflineReadyItemNoVersion item = new OfflineReadyItemNoVersion(rndGen);
                    item = localTable.insert(item).get();
                    test.log("Inserted the item to the local store: " + item);

                    offlineReadyClient.getSyncContext().push().get();
                    test.log("Pushed the changes to the server");

                    OfflineReadyItemNoVersion serverItem = remoteTable.lookUp(item.getId()).get();
                    serverItem.setName("changed name");
                    serverItem.setAge(0);
                    serverItem = remoteTable.update(serverItem).get();
                    test.log("Server item updated (changes will be overwritten later");

                    item.setAge(item.getAge() + 1);
                    item.setName(item.getName() + " - modified");

                    localTable.update(item).get();
                    test.log("Updated item locally, will now push changes to the server: " + item);
                    offlineReadyClient.getSyncContext().push().get();

                    serverItem = remoteTable.lookUp(item.getId()).get();
                    test.log("Retrieved the item from the server: " + serverItem);

                    if (serverItem.equals(item)) {
                        test.log("Items are the same");
                    } else {
                        test.log("Items are different. Local: " + item + "remote: " + serverItem);
                        result.setStatus(TestStatus.Failed);
                        callback.onTestComplete(this, result);
                        return;
                    }

                    test.log("Cleaning up");
                    localTable.delete(item).get();
                    test.log("Local table cleaned up. Now sync'ing once more");
                    offlineReadyClient.getSyncContext().push().get();
                    test.log("Done");

                    callback.onTestComplete(this, result);

                } catch (Exception e) {
                    callback.onTestComplete(this, createResultFromException(e));
                    return;
                }
            }

            ;
        };

        test.setName("Offline without version column");

        return test;
    }

    private TestCase createOfflineIncrementalSyncTest(final String queryKey, final boolean cleanStore, final boolean complexQuery) {

        final String tableName = "offlineReady";

        final TestCase test = new TestCase() {

            @Override
            protected void executeTest(MobileServiceClient offlineReadyClient, final TestExecutionCallback callback) {

                TestCase testCase = this;
                TestResult result = new TestResult();
                result.setStatus(TestStatus.Passed);
                result.setTestCase(testCase);
                try {

                    SQLiteLocalStore localStore = new SQLiteLocalStore(offlineReadyClient.getContext(), OFFLINE_TABLE_NAME, null, 1);

                    log("Defined the table on the local store");

                    Map<String, ColumnDataType> tableDefinition = new HashMap<String, ColumnDataType>();
                    tableDefinition.put("id", ColumnDataType.String);
                    tableDefinition.put("name", ColumnDataType.String);
                    tableDefinition.put("age", ColumnDataType.Integer);
                    tableDefinition.put("float", ColumnDataType.Real);
                    tableDefinition.put("date", ColumnDataType.Date);
                    tableDefinition.put("bool", ColumnDataType.Boolean);

                    log("Initialized the store and sync context");

                    localStore.defineTable(tableName, tableDefinition);

                    offlineReadyClient.getSyncContext().initialize(localStore, new SimpleSyncHandler()).get();

                    MobileServiceSyncTable<OfflineReadyItem> localTable = offlineReadyClient.getSyncTable(tableName, OfflineReadyItem.class);

                    MobileServiceTable<OfflineReadyItem> remoteTable = offlineReadyClient.getTable(tableName, OfflineReadyItem.class);

                    offlineReadyClient.getSyncContext().initialize(localStore, new SimpleSyncHandler()).get();

                    if (cleanStore) {
                        localStore.delete(INCREMENTAL_PULL_STRATEGY_TABLE, tableName + "_" + queryKey);
                    }

                    localTable.purge(null).get();

                    log("Removed all items from the local table");

                    Random rand = new Random();

                    int elementsCount = rand.nextInt((100 - 50) + 1) + 50;

                    String testFilter = UUID.randomUUID().toString();

                    for (int i = 0; i < elementsCount; i++) {

                        OfflineReadyItem item = new OfflineReadyItem(new Random());
                        item.setName(testFilter);

                        remoteTable.insert(item).get();

                    }

                    log("Inserted New Items on table");
                    Query pullQuery =
                            QueryOperations
                                    .tableName(tableName)
                                    .field("name").eq(testFilter);

                    localTable.pull(pullQuery, queryKey).get();

                    log("Pull new Elements");

                    MobileServiceList<OfflineReadyItem> localElements = localTable
                            .read(null).get();

                    if (localElements.size() != elementsCount) {
                        log("Error, elements count should be the same.  Actual " + localElements.size() + " - Expected " + elementsCount);
                        result.setStatus(TestStatus.Failed);
                        callback.onTestComplete(this, result);
                        return;
                    }

                    log("Elements count are the same");

                    log("Done");

                    callback.onTestComplete(this, result);

                } catch (Exception e) {
                    callback.onTestComplete(this, createResultFromException(e));
                    return;
                }
            }

            ;
        };

        String testName = "Offline - Incremental Sync";

        if (queryKey == null) {
            testName += "  - Simple Pull";
        } else {
            testName += "  - Incremental Pull";
        }

        if (cleanStore) {
            testName += "  - Clear Store";
        } else {
            testName += "  - Maintain Store";
        }

        if (complexQuery) {
            testName += "  - Complex Query";
        } else {
            testName += "  - Simple Query";
        }

        test.setName(testName);

        return test;
    }


    private TestCase createClearStoreTest() {
        final TestCase test = new TestCase() {

            @Override
            protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {

                TestCase testCase = this;
                TestResult result = new TestResult();
                result.setStatus(TestStatus.Passed);
                result.setTestCase(testCase);

                try {
                    client.getContext().deleteDatabase(OFFLINE_TABLE_NAME);
                } catch (Exception e) {
                    callback.onTestComplete(this, createResultFromException(e));
                    return;
                }

                callback.onTestComplete(this, result);
            }

            ;
        };

        test.setName("Clear store");

        return test;
    }

    private boolean validateRequestCount(TestCase testCase, int currentCount, int expectedCount) {
        testCase.log("So far " + currentCount + " requests sent to the server");

        if (currentCount != expectedCount) {
            testCase.log("Error, expected " + expectedCount + " requests to have been sent to the server");
            return false;
        }
        return true;
    }
}

class ServiceFilterWithRequestCount implements ServiceFilter {

    public int requestCount = 0;

    @Override
    public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

        requestCount++;

        return nextServiceFilterCallback.onNext(request);
    }
}

class ConflictResolvingSyncHandler implements MobileServiceSyncHandler {
    // public delegate T ConflictResolution(T clientItem, T serverItem);
    MobileServiceClient client;
    // ConflictResolution conflictResolution;
    TestCase test;

    public ConflictResolvingSyncHandler(TestCase test, MobileServiceClient client) {
        this.test = test;
        this.client = client;
    }

    @Override
    public JsonObject executeTableOperation(RemoteTableOperationProcessor processor, TableOperation operation) throws MobileServiceSyncHandlerException {

        MobileServicePreconditionFailedExceptionJson ex = null;
        JsonObject result = null;
        do {
            ex = null;
            try {
                test.log("Attempting to execute the operation");
                result = operation.accept(processor);
            } catch (MobileServicePreconditionFailedExceptionJson e) {
                ex = e;
            } catch (Throwable e) {
                ex = (MobileServicePreconditionFailedExceptionJson) e.getCause();
            }

            if (ex != null) {
                test.log("A MobileServicePreconditionFailedException was thrown, ex.Value = " + ex.getValue());
                JsonObject serverItem = ex.getValue();

                if (serverItem == null) {
                    test.log("Item not returned in the exception, trying to retrieve it from the server");
                    try {
                        serverItem = (JsonObject) (client.getTable(operation.getTableName()).lookUp(operation.getItemId())).get();
                    } catch (InterruptedException e) {
                        e.printStackTrace();
                    } catch (ExecutionException e) {
                        e.printStackTrace();
                    }
                }

                OfflineReadyItem typedClientItem = JsonEntityParser.parseResults(processor.getItem(), client.getGsonBuilder().create(), OfflineReadyItem.class)
                        .get(0);
                OfflineReadyItem typedServerItem = JsonEntityParser.parseResults(serverItem, client.getGsonBuilder().create(), OfflineReadyItem.class).get(0);

                OfflineReadyItem typedMergedItem = conflictResolution(typedClientItem, typedServerItem);

                String mergedItemJson = client.getGsonBuilder().create().toJson(typedMergedItem);

                JsonParser jsonParser = new JsonParser();
                JsonObject mergedItem = (JsonObject) jsonParser.parse(mergedItemJson);

                String serverVersion = serverItem.get("__version").getAsString();

                mergedItem.addProperty("__version", serverVersion);

                test.log("Merged the items, will try to resubmit the operation");

                //client.getSyncContext().removeTableOperation(ex);

                processor.setItem(mergedItem);
            }
        } while (ex != null);

        return result;

    }

    public OfflineReadyItem conflictResolution(OfflineReadyItem client, OfflineReadyItem server) {

        OfflineReadyItem result = new OfflineReadyItem();

        result.setId(client.getId());
        result.setAge(client.getAge() > server.getAge() ? client.getAge() : server.getAge());
        result.setDate(client.getDate().after(server.getDate()) ? client.getDate() : server.getDate());
        result.setFlag(client.getFlag() || server.getFlag());
        result.setFloatingNumber(client.getFloatingNumber() > server.getFloatingNumber() ? client.getFloatingNumber() : server.getFloatingNumber());
        result.setName(client.getName());

        return result;
    }

    @Override
    public void onPushComplete(MobileServicePushCompletionResult pushCompletionResult) throws MobileServiceSyncHandlerException {

    }
}

class InstaItem {

    @SerializedName("id")
    private String id;
    @SerializedName("inspectedDateTime")
    private Date mInspectedDateTime;
    @SerializedName("inventory")
    private String mInventory;
    @SerializedName("wall")
    private String mWall;
    @SerializedName("floor")
    private String mFloor;
    @SerializedName("ceiling")
    private String mCeiling;
    @SerializedName("inspected")
    private boolean mInspected;
    @SerializedName("rejected")
    private boolean mRejected;
    @SerializedName("inspectionId")
    private String mInspectionId;
    @SerializedName("levelsInInspectionId")
    private String mLevelsInInspectionId;
    @SerializedName("__version")
    private String mVersion;
    @SerializedName("__deleted")
    private boolean mDeleted;

    public InstaItem() {
        id = "0";
    }


    public InstaItem(Random rndGen) {
        this.mInventory = "";//Objects.toString(rndGen.nextLong(), null);
        this.mWall = "";// rndGen.nextLong();
        this.mFloor = "";// rndGen.nextLong();
        this.mCeiling = "";// rndGen.nextLong();
        this.mInspected = rndGen.nextInt(2) == 0;
        this.mRejected = rndGen.nextInt(2) == 0;
        this.mInspectionId = "943E68F1-A912-4C2C-B9C1-CB8E24A4E301";
        this.mLevelsInInspectionId = "97743F4F-7F35-4A60-9776-530E4BA0E3E3";
        this.mInspectedDateTime = new Date();
    }

    public String getId() {
        return id;
    }

    public void setId(String id) {
        this.id = id;
    }

    public Date getInspectedDateTime() {
        return mInspectedDateTime;
    }

    public void setInspectedDateTime(Date mInspectedDateTime) {
        this.mInspectedDateTime = mInspectedDateTime;
    }

    public String getInventory() {
        return mInventory;
    }

    public void setInventory(String mAge) {
        this.mInventory = mInventory;
    }

    public String getWall() {
        return mWall;
    }

    public void setWall(String mWall) {
        this.mWall = mWall;
    }

    public String getFloor() {
        return mFloor;
    }

    public void setFloor(String mFloor) {
        this.mFloor = mFloor;
    }

    public String getCeiling() {
        return mCeiling;
    }

    public void setCeiling(String mCeiling) {
        this.mCeiling = mCeiling;
    }

    public boolean getInspected() {
        return mInspected;
    }

    public void setInspected(boolean mInspected) {
        this.mInspected = mInspected;
    }

    public boolean getRejected() {
        return mRejected;
    }

    public void setRejected(boolean mRejected) {
        this.mRejected = mRejected;
    }

    public String getInspectionId() {
        return mInspectionId;
    }

    public void setInspectionId(String mInspectionId) {
        this.mInspectionId = mInspectionId;
    }

    public String getLevelsIninspectionId() {
        return mLevelsInInspectionId;
    }

    public void setLevelsIninspectionId(String mLevelsIninspectionId) {
        this.mLevelsInInspectionId = mLevelsIninspectionId;
    }

    public String getVersion() {
        return mVersion;
    }

    public void setVersion(String mVersion) {
        this.mVersion = mVersion;
    }

    public boolean getDeleted() {
        return mDeleted;
    }

    public void setDeleted(boolean mDeleted) {
        this.mDeleted = mDeleted;
    }

    @Override
    public boolean equals(Object o) {
        if (o == null)
            return false;

        if (!(o instanceof InstaItem))
            return false;

        InstaItem m = (InstaItem) o;

        if (!Util.compare(mInventory, m.getInventory()))
            return false;
        if (!Util.compare(mWall, m.getWall()))
            return false;
        if (!Util.compare(mFloor, m.getFloor()))
            return false;
        if (!Util.compare(mCeiling, m.getCeiling()))
            return false;
        if (!Util.compare(mInspected, m.getInspected()))
            return false;
        if (!Util.compare(mRejected, m.getRejected()))
            return false;
        if (!Util.compare(mInspectionId, m.getInspectionId()))
            return false;
        if (!Util.compare(mLevelsInInspectionId, m.getLevelsIninspectionId()))
            return false;
        //if (!Util.compare(mVersion, m.getVersion()))
        //    return false;
        if (!Util.compare(mDeleted, m.getDeleted()))
            return false;
        //if (mInspectedDateTime != null) {
        //    if (m.getInspectedDateTime() == null)
        //        return false;
        //    if (!Util.compare(Util.dateToString(mInspectedDateTime), Util.dateToString(m.getInspectedDateTime())))
        //        return false;
        //}

        return true;
    }

    @Override
    public String toString() {
        return String.format(Locale.getDefault(), "InstaItem[Id={0},Inventory={1},Wall={2},Floor={3},Ceiling={4},Inspected={5}, Rejected={6}, InspectionId={7}, LevelsIninspectionId={8}, Version={9}, Deleted={9}, InspectedDateTime={10}]",
                id, mInventory, mWall, mFloor, mCeiling, mInspected, mRejected, mInspectionId, mLevelsInInspectionId, mVersion, mDeleted,
                Util.dateToString(mInspectedDateTime));
    }
}

class PersonItem {

    @SerializedName("id")
    private String id;
    @SerializedName("name")
    private String mName;
    @SerializedName("age")
    private int mAge;

    public PersonItem() {
        id = "0";
    }


    public PersonItem(Random rndGen, String partition) {

        this.id = partition + "," + UUID.randomUUID().toString();
        this.mName = Integer.toString(rndGen.nextInt(2));
        this.mAge = rndGen.nextInt(99);
    }

    public String getId() {
        return id;
    }

    public void setId(String id) {
        this.id = id;
    }

    public String getName() {
        return mName;
    }

    public void setName(String mName) {
        this.mName = mName;
    }

    public int getAge() {
        return mAge;
    }

    public void setAge(int mAge) {
        this.mAge = mAge;
    }


    @Override
    public boolean equals(Object o) {
        if (o == null)
            return false;

        if (!(o instanceof InstaItem))
            return false;

        PersonItem m = (PersonItem) o;

        if (!Util.compare(mName, m.getName()))
            return false;
        if (!Util.compare(mAge, m.getAge()))
            return false;

        return true;
    }

    @Override
    public String toString() {
        return String.format(Locale.getDefault(), "PersonItem[Id={0},Name={1},Age={2}]",
                id, mName, mAge);
    }
}

class OfflineReadyItem {

    @SerializedName("id")
    private String id;
    @SerializedName("name")
    private String mName;
    @SerializedName("age")
    private int mAge;
    @SerializedName("float")
    private double mFloatingNumber;
    @SerializedName("date")
    private Date mDate;
    @SerializedName("bool")
    private boolean mFlag;
    @SerializedName("__version")
    private String mVersion;

    public OfflineReadyItem() {
        id = "0";
    }

    public OfflineReadyItem(Random rndGen) {
        this.id = java.util.UUID.randomUUID().toString();
        this.mName = "";// rndGen.nextLong();
        this.mAge = 20;//rndGen.nextInt();
        this.mFloatingNumber = rndGen.nextInt() * rndGen.nextDouble();
        this.mDate = new Date();
        this.mFlag = rndGen.nextInt(2) == 0;
    }

    public String getId() {
        return id;
    }

    public void setId(String id) {
        this.id = id;
    }

    public String getName() {
        return mName;
    }

    public void setName(String mName) {
        this.mName = mName;
    }

    public int getAge() {
        return mAge;
    }

    public void setAge(int mAge) {
        this.mAge = mAge;
    }

    public double getFloatingNumber() {
        return mFloatingNumber;
    }

    public void setFloatingNumber(double mFloatingNumber) {
        this.mFloatingNumber = mFloatingNumber;
    }

    public Date getDate() {
        return mDate;
    }

    public void setDate(Date mDate) {
        this.mDate = mDate;
    }

    public boolean getFlag() {
        return mFlag;
    }

    public void setFlag(boolean mFlag) {
        this.mFlag = mFlag;
    }

    public String getVersion() {
        return mVersion;
    }

    public void setVersion(String version) {
        this.mVersion = version;
    }

    @Override
    public boolean equals(Object o) {
        if (o == null)
            return false;

        if (!(o instanceof OfflineReadyItem))
            return false;
        OfflineReadyItem m = (OfflineReadyItem) o;

        if (!Util.compare(mName, m.getName()))
            return false;
        if (!Util.compare(mAge, m.getAge()))
            return false;
        if (!Util.compare(mFloatingNumber, m.getFloatingNumber()))
            return false;
        if (mDate != null) {
            if (m.mDate == null)
                return false;
            if (!Util.compare(mDate, m.getDate()))
                return false;
        }
        if (!Util.compare(mFlag, m.getFlag()))
            return false;
        return true;
    }

    @Override
    public String toString() {
        return String.format(Locale.getDefault(), "OfflineReadyItem[Id={0},Name={1},Age={2},FloatingNumber={3},Date={4},Flag={5},Version={6}]", id, mName,
                mAge, mFloatingNumber, Util.dateToString(mDate), mFlag, mVersion);
    }
}

class OfflineReadyItemNoVersion {

    @SerializedName("id")
    private String id;
    @SerializedName("name")
    private String mName;
    @SerializedName("age")
    private int mAge;
    @SerializedName("float")
    private double mFloatingNumber;
    @SerializedName("date")
    private Date mDate;
    @SerializedName("bool")
    private boolean mFlag;

    public OfflineReadyItemNoVersion() {
        id = "0";
    }

    public OfflineReadyItemNoVersion(Random rndGen) {
        this.id = java.util.UUID.randomUUID().toString();
        this.mName = "";// rndGen.nextLong();
        this.mAge = 20;//rndGen.nextInt();
        this.mFloatingNumber = rndGen.nextInt() * rndGen.nextDouble();
        this.mDate = new Date();
        this.mFlag = rndGen.nextInt(2) == 0;
    }

    public String getId() {
        return id;
    }

    public void setId(String id) {
        this.id = id;
    }

    public String getName() {
        return mName;
    }

    public void setName(String mName) {
        this.mName = mName;
    }

    public int getAge() {
        return mAge;
    }

    public void setAge(int mAge) {
        this.mAge = mAge;
    }

    public double getFloatingNumber() {
        return mFloatingNumber;
    }

    public void setFloatingNumber(double mFloatingNumber) {
        this.mFloatingNumber = mFloatingNumber;
    }

    public Date getDate() {
        return mDate;
    }

    public void setDate(Date mDate) {
        this.mDate = mDate;
    }

    public boolean getFlag() {
        return mFlag;
    }

    public void setFlag(boolean mFlag) {
        this.mFlag = mFlag;
    }

    @Override
    public boolean equals(Object o) {
        if (o == null)
            return false;

        if (!(o instanceof OfflineReadyItemNoVersion))
            return false;
        OfflineReadyItemNoVersion m = (OfflineReadyItemNoVersion) o;

        if (!Util.compare(mName, m.getName()))
            return false;
        if (!Util.compare(mAge, m.getAge()))
            return false;
        if (!Util.compare(mFloatingNumber, m.getFloatingNumber()))
            return false;
        if (mDate != null) {
            if (m.mDate == null)
                return false;
            if (!Util.compare(mDate, m.getDate()))
                return false;
        }
        if (!Util.compare(mFlag, m.getFlag()))
            return false;
        return true;
    }

    @Override
    public String toString() {
        return String.format(Locale.getDefault(), "OfflineReadyItem[Id={0},Name={1},Age={2},FloatingNumber={3},Date={4},Flag={5}]", id, mName,
                mAge, mFloatingNumber, Util.dateToString(mDate), mFlag);
    }
}
