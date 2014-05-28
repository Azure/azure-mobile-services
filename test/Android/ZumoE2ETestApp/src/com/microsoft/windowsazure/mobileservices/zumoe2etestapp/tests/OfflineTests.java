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

import java.util.Date;
import java.util.HashMap;
import java.util.Map;
import java.util.UUID;
import java.util.concurrent.ExecutionException;

import com.google.gson.JsonObject;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.table.MobileServiceJsonTable;
import com.microsoft.windowsazure.mobileservices.table.serialization.DateSerializer;
import com.microsoft.windowsazure.mobileservices.table.sync.MobileServiceJsonSyncTable;
import com.microsoft.windowsazure.mobileservices.table.sync.MobileServiceSyncContext;
import com.microsoft.windowsazure.mobileservices.table.sync.localstore.ColumnDataType;
import com.microsoft.windowsazure.mobileservices.table.sync.localstore.SQLiteLocalStore;
import com.microsoft.windowsazure.mobileservices.table.sync.synchandler.SimpleSyncHandler;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.ExpectedValueException;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestCase;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestExecutionCallback;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestGroup;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestResult;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestStatus;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.Util;

public class OfflineTests extends TestGroup {

	protected static final String OFFLINE_TABLE_NAME = "offlineTable";

	public OfflineTests() {
		super("Offline tests");

		this.addTest(createSimpleTest("Offline - Simple Test", null));

		this.addTest(createComplexTest("Offline - Complex Test", null));
	}

	private TestCase createSimpleTest(String name, final Class<?> expectedExceptionClass) {

		final TestCase test = new TestCase() {

			@Override
			protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {

				TestCase testCase = this;
				TestResult result = new TestResult();
				result.setStatus(TestStatus.Passed);
				result.setTestCase(testCase);

				try {

					log("start simple");

					String tableName = "offlineTable";

					Map<String, ColumnDataType> tableDefinition = new HashMap<String, ColumnDataType>();
					tableDefinition.put("id", ColumnDataType.String);
					tableDefinition.put("bool", ColumnDataType.Boolean);
					tableDefinition.put("complex", ColumnDataType.Other);
					tableDefinition.put("date1", ColumnDataType.Date);
					tableDefinition.put("name", ColumnDataType.String);
					tableDefinition.put("number", ColumnDataType.Number);
					tableDefinition.put("__createdAt", ColumnDataType.Date);
					tableDefinition.put("__updatedAt", ColumnDataType.Date);
					tableDefinition.put("__version", ColumnDataType.String);

					SQLiteLocalStore store = new SQLiteLocalStore(client.getContext(), "offlineTest.db", null, 1);
					store.defineTable(tableName, tableDefinition);

					SimpleSyncHandler handler = new SimpleSyncHandler();
					MobileServiceSyncContext syncContext = client.getSyncContext();
					syncContext.initialize(store, handler).get();

					MobileServiceJsonSyncTable syncTable = client.getSyncTable(tableName);
					MobileServiceJsonTable table = client.getTable(tableName);

					JsonObject elem1 = new JsonObject();

					String id1 = UUID.randomUUID().toString();
					elem1.addProperty("id", id1);

					elem1.addProperty("bool", false);

					JsonObject complex1 = new JsonObject();
					complex1.addProperty("data", "some data");
					elem1.add("complex", complex1);

					String date1 = DateSerializer.serialize(new Date());
					elem1.addProperty("date1", date1);

					elem1.addProperty("name", "any name");

					elem1.addProperty("number", 15);

					// Insert Local
					JsonObject elem1copy = syncTable.insert(elem1).get();

					log("insert 1");

					if (!Util.compareJson(elem1, elem1copy)) {
						throw new ExpectedValueException(elem1, elem1copy);
					}

					elem1copy = syncTable.lookUp(id1).get();

					log("lookup 1");

					if (!Util.compareJson(elem1, elem1copy)) {
						throw new ExpectedValueException(elem1, elem1copy);
					}

					// Publish changes
					syncContext.push().get();

					log("push");

					elem1copy = table.lookUp(id1).get();

					log("lookup 1");

					if (!Util.compareJson(elem1, elem1copy)) {
						throw new ExpectedValueException(elem1, elem1copy);
					}

					// Clean local table
					syncTable.purge(null).get();

					log("purge");

					elem1copy = syncTable.lookUp(id1).get();

					log("lookup 1");

					if (!Util.compareJson(null, elem1copy)) {
						throw new ExpectedValueException(null, elem1copy);
					}

					// Retrieve from server
					syncTable.pull(null).get();

					log("pull");

					elem1copy = syncTable.lookUp(id1).get();

					if (!Util.compareJson(elem1, elem1copy)) {
						throw new ExpectedValueException(null, elem1copy);
					}

					log("done simple");
				} catch (Throwable throwable) {
					logException(this, throwable);

					if (throwable instanceof Exception) {
						createResultFromException(result, (Exception) throwable);
					}
				} finally {
					callback.onTestComplete(testCase, result);
				}
			}
		};

		test.setExpectedExceptionClass(expectedExceptionClass);
		test.setName(name);

		return test;
	}

	private TestCase createComplexTest(String name, final Class<?> expectedExceptionClass) {

		final TestCase test = new TestCase() {

			@Override
			protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {

				TestCase testCase = this;
				TestResult result = new TestResult();
				result.setStatus(TestStatus.Passed);
				result.setTestCase(testCase);

				try {

					log("start complex");

					String tableName = "offlineTable";

					Map<String, ColumnDataType> tableDefinition = new HashMap<String, ColumnDataType>();
					tableDefinition.put("id", ColumnDataType.String);
					tableDefinition.put("bool", ColumnDataType.Boolean);
					tableDefinition.put("complex", ColumnDataType.Other);
					tableDefinition.put("date1", ColumnDataType.Date);
					tableDefinition.put("name", ColumnDataType.String);
					tableDefinition.put("number", ColumnDataType.Number);
					tableDefinition.put("__createdAt", ColumnDataType.Date);
					tableDefinition.put("__updatedAt", ColumnDataType.Date);
					tableDefinition.put("__version", ColumnDataType.String);

					SQLiteLocalStore store = new SQLiteLocalStore(client.getContext(), "offlineTest.db", null, 1);
					store.defineTable(tableName, tableDefinition);

					SimpleSyncHandler handler = new SimpleSyncHandler();
					MobileServiceSyncContext syncContext = client.getSyncContext();
					syncContext.initialize(store, handler).get();

					MobileServiceJsonSyncTable syncTable = client.getSyncTable(tableName);

					JsonObject elem1 = new JsonObject();

					String id1 = UUID.randomUUID().toString();
					elem1.addProperty("id", id1);

					elem1.addProperty("bool", false);

					JsonObject complex1 = new JsonObject();
					complex1.addProperty("data", "some data 1");
					elem1.add("complex", complex1);

					String date1 = DateSerializer.serialize(new Date());
					elem1.addProperty("date1", date1);

					elem1.addProperty("name", "any name 1");

					elem1.addProperty("number", 15);

					JsonObject elem2 = new JsonObject();

					String id2 = UUID.randomUUID().toString();
					elem2.addProperty("id", id2);

					elem2.addProperty("bool", true);

					JsonObject complex2 = new JsonObject();
					complex2.addProperty("data", "some data 2");
					elem2.add("complex", complex2);

					String date2 = DateSerializer.serialize(new Date());
					elem2.addProperty("date1", date2);

					elem2.addProperty("name", "any name 2");

					elem2.addProperty("number", 5);

					log("setup items");

					// Insert Local
					syncTable.insert(elem1).get();

					log("insert 1");

					JsonObject elem2copy = syncTable.insert(elem2).get();

					log("insert 2");

					if (!Util.compareJson(elem2, elem2copy)) {
						throw new ExpectedValueException(elem2, elem2copy);
					}

					log("compare 2");

					elem1.remove("number");
					elem1.addProperty("number", 7);

					// Update local
					syncTable.update(elem1).get();

					log("update 1");

					// Clean local table - should publish pending changes
					syncTable.purge(null).get();

					log("purge");

					JsonObject elem1copy = syncTable.lookUp(id1).get();

					log("lookup 1");

					if (!Util.compareJson(null, elem1copy)) {
						throw new ExpectedValueException(null, elem1copy);
					}

					log("compare 1");

					// Retrieve from server
					syncTable.pull(null).get();

					log("pull");

					elem1copy = syncTable.lookUp(id1).get();

					log("lookup 1");

					if (!Util.compareJson(elem1, elem1copy)) {
						throw new ExpectedValueException(elem1, elem1copy);
					}

					log("compare 1");

					elem2copy = syncTable.lookUp(id2).get();

					log("lookup 2");

					if (!Util.compareJson(elem2, elem2copy)) {
						throw new ExpectedValueException(elem2, elem2copy);
					}

					log("compare 2");

					syncTable.delete(id1).get();

					log("delete 1");

					elem1copy = syncTable.lookUp(id1).get();

					log("lookup 1");

					if (!Util.compareJson(null, elem1copy)) {
						throw new ExpectedValueException(null, elem1copy);
					}

					log("compare 1 null");

					// Retrieve from server - should publish pending operations
					syncTable.pull(null).get();

					log("pull");

					elem1copy = syncTable.lookUp(id1).get();

					log("lookup 1");

					if (!Util.compareJson(null, elem1copy)) {
						throw new ExpectedValueException(null, elem1copy);
					}

					log("compare 1 null");

					log("done complex");
				} catch (Throwable throwable) {
					logException(this, throwable);

					if (throwable instanceof Exception) {
						createResultFromException(result, (Exception) throwable);
					}
				} finally {
					callback.onTestComplete(testCase, result);
				}
			}
		};

		test.setExpectedExceptionClass(expectedExceptionClass);
		test.setName(name);

		return test;
	}

	private void logException(TestCase test, Throwable throwable) {
		if (throwable instanceof ExecutionException || throwable instanceof InterruptedException) {
			throwable = throwable.getCause();
		}

		test.log(throwable.getClass().getName());

		if (throwable.getMessage() != null) {
			test.log(throwable.getMessage());
		}

		if (throwable.getStackTrace() != null) {
			for (StackTraceElement stack : throwable.getStackTrace()) {
				test.log(stack.getFileName());
				test.log(String.valueOf(stack.getLineNumber()));
			}
		}
	}
}
