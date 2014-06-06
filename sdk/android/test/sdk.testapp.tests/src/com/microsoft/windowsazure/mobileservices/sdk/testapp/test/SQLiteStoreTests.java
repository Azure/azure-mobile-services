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

import java.io.IOException;
import java.net.MalformedURLException;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.Date;
import java.util.HashMap;
import java.util.List;
import java.util.Locale;
import java.util.Map;
import java.util.concurrent.Callable;
import java.util.concurrent.ExecutionException;
import org.apache.http.Header;

import android.content.Context;
import android.test.InstrumentationTestCase;

import com.google.common.base.Function;
import com.google.common.util.concurrent.ListenableFuture;
import com.google.gson.GsonBuilder;
import com.google.gson.JsonArray;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceException;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilter;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterRequest;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.http.NextServiceFilterCallback;
import com.microsoft.windowsazure.mobileservices.table.query.Query;
import com.microsoft.windowsazure.mobileservices.table.query.QueryOperations;
import com.microsoft.windowsazure.mobileservices.table.query.QueryOrder;
import com.microsoft.windowsazure.mobileservices.table.sync.MobileServiceJsonSyncTable;
import com.microsoft.windowsazure.mobileservices.table.sync.MobileServiceSyncTable;
import com.microsoft.windowsazure.mobileservices.table.sync.localstore.ColumnDataType;
import com.microsoft.windowsazure.mobileservices.table.sync.localstore.MobileServiceLocalStoreException;
import com.microsoft.windowsazure.mobileservices.table.sync.localstore.SQLiteLocalStore;
import com.microsoft.windowsazure.mobileservices.table.sync.push.MobileServicePushFailedException;
import com.microsoft.windowsazure.mobileservices.table.sync.push.MobileServicePushStatus;
import com.microsoft.windowsazure.mobileservices.table.sync.synchandler.MobileServiceSyncHandler;
import com.microsoft.windowsazure.mobileservices.table.sync.synchandler.SimpleSyncHandler;

public class SQLiteStoreTests extends InstrumentationTestCase {

	private String TestDbName = "queryTest.db";
	private String TestTable = "todo";
	private String MathTestTable = "mathtest";
	private boolean queryTableInitialized;
	private Date epoch;

	protected void setUp() throws Exception {
		super.setUp();

		Calendar cal = Calendar.getInstance();
		cal.set(1970, Calendar.JANUARY, 1);
		epoch = cal.getTime();

	}

	protected void tearDown() throws Exception {
		super.tearDown();
	}

	private List<JsonObject> getTestData() {

		ArrayList<JsonObject> result = new ArrayList<JsonObject>();

		Calendar cal1 = Calendar.getInstance();
		cal1.set(1970, Calendar.JANUARY, 1);
		cal1.add(Calendar.MILLISECOND, 32434);

		JsonObject item1 = new JsonObject();
		item1.addProperty("id", "1");
		item1.addProperty("col1", "the");
		item1.addProperty("col2", 5);
		item1.addProperty("col3", 234f);
		item1.addProperty("col4", cal1.getTime().toString());
		item1.addProperty("col5", false);
		result.add(item1);

		Calendar cal2 = Calendar.getInstance();
		cal2.set(1970, Calendar.JANUARY, 1);
		cal2.add(Calendar.MILLISECOND, 99797);

		JsonObject item2 = new JsonObject();
		item2.addProperty("id", "2");
		item2.addProperty("col1", "quick");
		item2.addProperty("col2", 3);
		item2.addProperty("col3", 9867.12);
		item2.addProperty("col4", cal2.getTime().toString());
		item2.addProperty("col5", true);
		result.add(item2);

		Calendar cal3 = Calendar.getInstance();
		cal3.set(1970, Calendar.JANUARY, 1);
		cal3.add(Calendar.MILLISECOND, 239873840);

		JsonObject item3 = new JsonObject();
		item3.addProperty("id", "3");
		item3.addProperty("col1", "brown");
		item3.addProperty("col2", 1);
		item3.addProperty("col3", 11f);
		item3.addProperty("col4", cal3.getTime().toString());
		item3.addProperty("col5", false);
		result.add(item3);

		Calendar cal4 = Calendar.getInstance();
		cal4.set(1970, Calendar.JANUARY, 1);
		cal4.add(Calendar.MILLISECOND, 888888888);

		JsonObject item4 = new JsonObject();
		item4.addProperty("id", "4");
		item4.addProperty("col1", "fox");
		item4.addProperty("col2", 6);
		item4.addProperty("col3", 23908.99);
		item4.addProperty("col4", cal4.getTime().toString());
		item4.addProperty("col5", true);
		result.add(item4);

		Calendar cal5 = Calendar.getInstance();
		cal5.set(1970, Calendar.JANUARY, 1);
		cal5.add(Calendar.MILLISECOND, 333333332);

		JsonObject item5 = new JsonObject();
		item5.addProperty("id", "5");
		item5.addProperty("col1", "jumped");
		item5.addProperty("col2", 9);
		item5.addProperty("col3", 678.932);
		item5.addProperty("col4", cal5.getTime().toString());
		item5.addProperty("col5", true);
		result.add(item5);

		Calendar cal6 = Calendar.getInstance();
		cal6.set(1970, Calendar.JANUARY, 1);
		cal6.add(Calendar.MILLISECOND, 333333333);

		JsonObject item6 = new JsonObject();
		item6.addProperty("id", "6");
		item6.addProperty("col1", "EndsWithBackslash\\");
		item6.addProperty("col2", 8);
		item6.addProperty("col3", 521f);
		item6.addProperty("col4", cal5.getTime().toString());
		item6.addProperty("col5", true);
		result.add(item6);

		return result;
	}

	public void testInitializeAsyncInitializesTheStore() throws MobileServiceLocalStoreException {
		SQLiteLocalStore store = new SQLiteLocalStore(this.getContext(), TestDbName, null, 1);

		Map<String, ColumnDataType> tableDefinition = new HashMap<String, ColumnDataType>();
		tableDefinition.put("id", ColumnDataType.String);
		tableDefinition.put("__createdAt", ColumnDataType.Date);

		store.defineTable(TestTable, tableDefinition);

		store.initialize();
	}

	private Context getContext() {
		// TODO Auto-generated method stub
		return getInstrumentation().getTargetContext();
	}

	public void testLookupAsyncThrowsWhenStoreIsNotInitialized() {
		CustomFunctionOneParameter<SQLiteLocalStore, Void> storeAction = new CustomFunctionOneParameter<SQLiteLocalStore, Void>() {
			public Void apply(SQLiteLocalStore store) throws MobileServiceLocalStoreException {

				store.lookup("asdf", "asdf");
				return null;
			}
		};

		testStoreThrowOnUninitialized(storeAction);
	}

	public void testLookupAsyncReadsItem() throws MobileServiceLocalStoreException {

		prepareTodoTable();

		Date testDate = new Date();

		long date = (long) (testDate.getTime() - epoch.getTime());

		// insert a row and make sure it is inserted
		SQLiteStoreTestsUtilities.executeNonQuery(this.getContext(), TestDbName, "INSERT INTO todo (id, __createdat) VALUES ('abc', " + date + ")");

		long count = SQLiteStoreTestsUtilities.countRows(this.getContext(), TestDbName, TestTable);

		assertEquals(count, 1L);

		SQLiteLocalStore store = new SQLiteLocalStore(this.getContext(), TestDbName, null, 1);

		defineTestTable(store);
		store.initialize();

		JsonObject item = store.lookup(TestTable, "abc");
		assertNotNull(item);
		assertEquals(item.get("id").getAsString(), "abc");
		assertEquals(item.get("__createdat").getAsLong(), date);
	}

	public void testReadAsyncThrowsWhenStoreIsNotInitialized() {
		CustomFunctionOneParameter<SQLiteLocalStore, Void> storeAction = new CustomFunctionOneParameter<SQLiteLocalStore, Void>() {
			public Void apply(SQLiteLocalStore store) throws MobileServiceLocalStoreException {

				Query q = QueryOperations.tableName("abc");

				store.read(q);

				return null;
			}
		};

		testStoreThrowOnUninitialized(storeAction);
	}

	public void testReadAsyncReadsItems() throws MobileServiceLocalStoreException, MobileServiceException {
		prepareTodoTable();

		// insert a row and make sure it is inserted
		SQLiteStoreTestsUtilities
				.executeNonQuery(this.getContext(), TestDbName, "INSERT INTO todo (id, __createdAt) VALUES ('abc', 1), ('def', 2), ('ghi', 3)");

		long count = SQLiteStoreTestsUtilities.countRows(this.getContext(), TestDbName, TestTable);

		assertEquals(count, 3L);

		SQLiteLocalStore store = new SQLiteLocalStore(this.getContext(), TestDbName, null, 1);

		defineTestTable(store);
		store.initialize();

		Query query = QueryOperations.tableName("todo").field("__createdAt").gt(1).includeInlineCount();

		JsonObject queryResults = store.read(query).getAsJsonObject();

		assertNotNull(queryResults);

		JsonArray results = queryResults.get("results").getAsJsonArray();
		long resultCount = queryResults.get("count").getAsLong();

		assertEquals(results.size(), 2);
		assertEquals(resultCount, 2L);
	}

	public void testDeleteAsyncByQueryThrowsWhenStoreIsNotInitialized() {
		CustomFunctionOneParameter<SQLiteLocalStore, Void> storeAction = new CustomFunctionOneParameter<SQLiteLocalStore, Void>() {
			public Void apply(SQLiteLocalStore store) throws MobileServiceLocalStoreException {

				Query q = QueryOperations.tableName("abc");

				store.delete(q);

				return null;
			}
		};

		testStoreThrowOnUninitialized(storeAction);
	}

	public void testDeleteAsyncByIdThrowsWhenStoreIsNotInitialized() {
		CustomFunctionOneParameter<SQLiteLocalStore, Void> storeAction = new CustomFunctionOneParameter<SQLiteLocalStore, Void>() {
			public Void apply(SQLiteLocalStore store) throws MobileServiceLocalStoreException {

				store.delete("abc", "");

				return null;
			}
		};

		testStoreThrowOnUninitialized(storeAction);
	}

	public void testDeleteAsyncDeletesTheRowWhenTheyMatchTheQuery() throws MobileServiceLocalStoreException, MobileServiceException {
		prepareTodoTable();

		// insert a row and make sure it is inserted
		SQLiteStoreTestsUtilities
				.executeNonQuery(this.getContext(), TestDbName, "INSERT INTO todo (id, __createdAt) VALUES ('abc', 1), ('def', 2), ('ghi', 3)");

		long count = SQLiteStoreTestsUtilities.countRows(this.getContext(), TestDbName, TestTable);

		assertEquals(count, 3L);

		// delete the row
		SQLiteLocalStore store = new SQLiteLocalStore(this.getContext(), TestDbName, null, 1);

		defineTestTable(store);
		store.initialize();

		Query query = QueryOperations.tableName("todo").field("__createdAt").gt(1).includeInlineCount();

		store.delete(query);
		count = SQLiteStoreTestsUtilities.countRows(this.getContext(), TestDbName, TestTable);

		assertEquals(count, 1L);
	}

	public void testDeleteAsyncDeletesTheRow() throws MobileServiceLocalStoreException {
		prepareTodoTable();

		// insert a row and make sure it is inserted
		SQLiteStoreTestsUtilities.executeNonQuery(this.getContext(), TestDbName, "INSERT INTO todo (id, __createdAt) VALUES ('abc', 123)");

		long count = SQLiteStoreTestsUtilities.countRows(this.getContext(), TestDbName, TestTable);

		assertEquals(count, 1L);

		// delete the row
		SQLiteLocalStore store = new SQLiteLocalStore(this.getContext(), TestDbName, null, 1);

		defineTestTable(store);
		store.initialize();

		store.delete(TestTable, "abc");
		count = SQLiteStoreTestsUtilities.countRows(this.getContext(), TestDbName, TestTable);

		assertEquals(count, 0L);

		// rows should be zero now
		count = SQLiteStoreTestsUtilities.countRows(this.getContext(), TestDbName, TestTable);
		assertEquals(count, 0L);
	}

	public void testUpsertAsyncThrowsWhenStoreIsNotInitialized() {
		CustomFunctionOneParameter<SQLiteLocalStore, Void> storeAction = new CustomFunctionOneParameter<SQLiteLocalStore, Void>() {
			public Void apply(SQLiteLocalStore store) throws MobileServiceLocalStoreException {

				store.upsert("asdf", new JsonObject());

				return null;
			}
		};

		testStoreThrowOnUninitialized(storeAction);
	}

	public void testUpsertAsyncThrowsWhenColumnInItemIsNotDefinedAndItIsLocal() throws MobileServiceLocalStoreException {
		SQLiteLocalStore store = new SQLiteLocalStore(this.getContext(), TestDbName, null, 1);

		Map<String, ColumnDataType> tableDefinition = new HashMap<String, ColumnDataType>();
		tableDefinition.put("id", ColumnDataType.String);
		tableDefinition.put("dob", ColumnDataType.Date);

		store.defineTable(TestTable, tableDefinition);
		store.initialize();

		try {

			JsonObject item = new JsonObject();
			item.addProperty("notDefined", "okok");

			store.upsert(TestTable, item);
		} catch (Exception ex) {
			assertTrue(ex instanceof MobileServiceLocalStoreException);
			assertEquals(ex.getCause().getMessage(),
					"table todo has no column named notdefined (code 1): , while compiling: INSERT OR REPLACE INTO \"todo\" (\"notdefined\") VALUES (@p1);");

		}
	}

	public void testUpsertAsyncInsertsTheRowWhenItemHasNullValues() throws MobileServiceLocalStoreException {
		SQLiteStoreTestsUtilities.dropTestTable(this.getContext(), TestDbName, TestTable);

		// insert a row and make sure it is inserted

		SQLiteLocalStore store = new SQLiteLocalStore(this.getContext(), TestDbName, null, 1);

		Map<String, ColumnDataType> tableDefinition = new HashMap<String, ColumnDataType>();
		tableDefinition.put("id", ColumnDataType.String);
		tableDefinition.put("dob", ColumnDataType.Date);
		tableDefinition.put("age", ColumnDataType.Number);
		tableDefinition.put("weight", ColumnDataType.Number);
		tableDefinition.put("code", ColumnDataType.String);
		tableDefinition.put("options", ColumnDataType.String);
		tableDefinition.put("friends", ColumnDataType.String);
		tableDefinition.put("__version", ColumnDataType.String);

		store.defineTable(TestTable, tableDefinition);
		store.initialize();

		JsonObject inserted = new JsonObject();
		inserted.addProperty("id", "abc");
		inserted.addProperty("dob", (String) null);
		inserted.addProperty("age", (Integer) null);
		inserted.addProperty("weight", (Integer) null);
		inserted.addProperty("code", (String) null);
		inserted.addProperty("options", (String) null);
		inserted.addProperty("friends", (String) null);
		inserted.addProperty("__version", (String) null);

		store.upsert(TestTable, inserted);

		JsonObject read = store.lookup(TestTable, "abc");

		assertNotNull(read);
		assertEquals(inserted.get("id").getAsString(), read.get("id").getAsString());
	}

	public void testUpsertAsyncInsertsTheRowWhenItDoesNotExist() throws MobileServiceLocalStoreException {
		prepareTodoTable();

		// insert a row and make sure it is inserted
		SQLiteLocalStore store = new SQLiteLocalStore(this.getContext(), TestDbName, null, 1);

		defineTestTable(store);
		store.initialize();

		JsonObject inserted = new JsonObject();
		inserted.addProperty("id", "abc");
		inserted.addProperty("__createdAt", new Date().toString());

		// insert a row and make sure it is inserted
		store.upsert(TestTable, inserted);

		long count = SQLiteStoreTestsUtilities.countRows(this.getContext(), TestDbName, TestTable);
		assertEquals(count, 1L);
	}

	public void testUpsertAsyncUpdatesTheRowWhenItExists() throws MobileServiceLocalStoreException {
		prepareTodoTable();

		// insert a row and make sure it is inserted
		SQLiteLocalStore store = new SQLiteLocalStore(this.getContext(), TestDbName, null, 1);

		defineTestTable(store);
		store.initialize();

		JsonObject inserted = new JsonObject();
		inserted.addProperty("id", "abc");
		inserted.addProperty("__createdAt", new Date().toString());

		// insert a row and make sure it is inserted
		store.upsert(TestTable, inserted);

		JsonObject updated = new JsonObject();
		updated.addProperty("id", "abc");
		updated.addProperty("__createdAt", new Date().toString());

		store.upsert(TestTable, updated);

		long count = SQLiteStoreTestsUtilities.countRows(this.getContext(), TestDbName, TestTable);
		assertEquals(count, 1L);
	}

	public void testUpsertThenLookupThenUpsertThenDeleteThenLookup() throws MobileServiceLocalStoreException {
		SQLiteStoreTestsUtilities.dropTestTable(this.getContext(), TestDbName, TestTable);

		SQLiteLocalStore store = new SQLiteLocalStore(this.getContext(), TestDbName, null, 1);

		Map<String, ColumnDataType> tableDefinition = new HashMap<String, ColumnDataType>();
		tableDefinition.put("id", ColumnDataType.String);
		tableDefinition.put("bool", ColumnDataType.Boolean);
		tableDefinition.put("int", ColumnDataType.Number);
		tableDefinition.put("double", ColumnDataType.Number);
		tableDefinition.put("date", ColumnDataType.String);
		tableDefinition.put("guid", ColumnDataType.String);
		tableDefinition.put("options", ColumnDataType.String);
		tableDefinition.put("friends", ColumnDataType.String);

		// create the table
		store.defineTable(TestTable, tableDefinition);
		store.initialize();

		JsonObject originalItem = new JsonObject();
		originalItem.addProperty("id", "abc");
		originalItem.addProperty("bool", true);
		originalItem.addProperty("int", 45);
		originalItem.addProperty("double", 123.45d);
		originalItem.addProperty("guid", "");
		originalItem.addProperty("date", new Date().toString());
		originalItem.addProperty("options", "");
		originalItem.addProperty("friends", "");

		// first add an item
		store.upsert(TestTable, originalItem);

		// read the item back
		JsonObject itemRead = store.lookup(TestTable, "abc");

		// make sure everything was persisted
		assertNotNull(itemRead);

		// change the item
		originalItem.addProperty("double", 111.222d);

		// upsert the item
		store.upsert(TestTable, originalItem);

		// read the updated item
		JsonObject updatedItem = store.lookup(TestTable, "abc");

		// make sure everything was persisted
		assertNotNull(updatedItem);
		
		// make sure the float was updated
		assertEquals(updatedItem.get("double").getAsDouble(), 111.222d);

		// make sure the item is same as updated item
		assertEquals(originalItem.get("id").getAsString(), updatedItem.get("id").getAsString());

		// now delete the item
		store.delete(TestTable, "abc");

		// now read it back
		JsonObject lastItem = store.lookup(TestTable, "abc");

		// it should be null because it doesn't exist
		assertNull(lastItem);
	}

	private void prepareTodoTable() throws MobileServiceLocalStoreException {
		SQLiteStoreTestsUtilities.dropTestTable(this.getContext(), TestDbName, TestTable);

		// first create a table called todo
		SQLiteLocalStore store = new SQLiteLocalStore(this.getContext(), TestDbName, null, 1);

		defineTestTable(store);

		store.initialize();

	}

	private void testStoreThrowOnUninitialized(CustomFunctionOneParameter<SQLiteLocalStore, Void> storeAction) {
		SQLiteLocalStore store = new SQLiteLocalStore(this.getContext(), TestDbName, null, 1);

		try {
			storeAction.apply(store);
			fail("MobileServiceLocalStoreException expected");
		} catch (Exception ex) {
			assertTrue(ex instanceof MobileServiceLocalStoreException);
		}
	}

	public void defineTestTable(SQLiteLocalStore store) throws MobileServiceLocalStoreException {
		Map<String, ColumnDataType> tableDefinition = new HashMap<String, ColumnDataType>();
		tableDefinition.put("id", ColumnDataType.String);
		tableDefinition.put("__createdat", ColumnDataType.Date);

		store.defineTable(TestTable, tableDefinition);
	}
}