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

import android.content.Context;
import android.test.InstrumentationTestCase;

import com.google.gson.JsonArray;
import com.google.gson.JsonObject;
import com.microsoft.windowsazure.mobileservices.MobileServiceException;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.test.helpers.SQLiteStoreTestsUtilities;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.test.types.CustomFunctionOneParameter;
import com.microsoft.windowsazure.mobileservices.table.query.Query;
import com.microsoft.windowsazure.mobileservices.table.query.QueryOperations;
import com.microsoft.windowsazure.mobileservices.table.sync.localstore.ColumnDataType;
import com.microsoft.windowsazure.mobileservices.table.sync.localstore.MobileServiceLocalStoreException;
import com.microsoft.windowsazure.mobileservices.table.sync.localstore.SQLiteLocalStore;

import java.util.Calendar;
import java.util.Date;
import java.util.HashMap;
import java.util.Map;

public class SQLiteStoreTests extends InstrumentationTestCase {

    private String TestDbName = "queryTest.db";
    private String TestTable = "todo";
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

    public void testInitializeInitializesTheStore() throws MobileServiceLocalStoreException {
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

    public void testLookupThrowsWhenStoreIsNotInitialized() {
        CustomFunctionOneParameter<SQLiteLocalStore, Void> storeAction = new CustomFunctionOneParameter<SQLiteLocalStore, Void>() {
            public Void apply(SQLiteLocalStore store) throws MobileServiceLocalStoreException {

                store.lookup("asdf", "asdf");
                return null;
            }
        };

        testStoreThrowOnUninitialized(storeAction);
    }

    public void testLookupReadsItem() throws MobileServiceLocalStoreException {

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

    public void testReadThrowsWhenStoreIsNotInitialized() {
        CustomFunctionOneParameter<SQLiteLocalStore, Void> storeAction = new CustomFunctionOneParameter<SQLiteLocalStore, Void>() {
            public Void apply(SQLiteLocalStore store) throws MobileServiceLocalStoreException {

                Query q = QueryOperations.tableName("abc");

                store.read(q);

                return null;
            }
        };

        testStoreThrowOnUninitialized(storeAction);
    }

    public void testReadReadsItems() throws MobileServiceLocalStoreException, MobileServiceException {
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

    public void testDeleteByQueryThrowsWhenStoreIsNotInitialized() {
        CustomFunctionOneParameter<SQLiteLocalStore, Void> storeAction = new CustomFunctionOneParameter<SQLiteLocalStore, Void>() {
            public Void apply(SQLiteLocalStore store) throws MobileServiceLocalStoreException {

                Query q = QueryOperations.tableName("abc");

                store.delete(q);

                return null;
            }
        };

        testStoreThrowOnUninitialized(storeAction);
    }

    public void testDeleteByIdThrowsWhenStoreIsNotInitialized() {
        CustomFunctionOneParameter<SQLiteLocalStore, Void> storeAction = new CustomFunctionOneParameter<SQLiteLocalStore, Void>() {
            public Void apply(SQLiteLocalStore store) throws MobileServiceLocalStoreException {

                store.delete("abc", "");

                return null;
            }
        };

        testStoreThrowOnUninitialized(storeAction);
    }

    public void testDeleteDeletesTheRowWhenTheyMatchTheQuery() throws MobileServiceLocalStoreException, MobileServiceException {
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

    public void testDeleteDeletesTheRow() throws MobileServiceLocalStoreException {
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

    public void testUpsertThrowsWhenStoreIsNotInitialized() {
        CustomFunctionOneParameter<SQLiteLocalStore, Void> storeAction = new CustomFunctionOneParameter<SQLiteLocalStore, Void>() {
            public Void apply(SQLiteLocalStore store) throws MobileServiceLocalStoreException {

                store.upsert("asdf", new JsonObject(), false);

                return null;
            }
        };

        testStoreThrowOnUninitialized(storeAction);
    }

    public void testUpsertNoThrowsWhenColumnInItemIsNotDefinedAndItIsFromServer() throws MobileServiceLocalStoreException {
        SQLiteLocalStore store = new SQLiteLocalStore(this.getContext(), TestDbName, null, 1);

        Map<String, ColumnDataType> tableDefinition = new HashMap<String, ColumnDataType>();
        tableDefinition.put("dob", ColumnDataType.Date);

        store.defineTable(TestTable, tableDefinition);
        store.initialize();

        try {

            JsonObject item = new JsonObject();
            item.addProperty("notDefined", "okok");

            store.upsert(TestTable, item, true);

        } catch (Exception ex) {
            assertNull(ex);
        }
    }

    public void testUpsertNoThrowsWhenOnColumnIsItemIsDefinedAndAnotherColumnInItemIsNotDefinedAndItIsFromServer() throws MobileServiceLocalStoreException {
        SQLiteLocalStore store = new SQLiteLocalStore(this.getContext(), TestDbName, null, 1);

        Map<String, ColumnDataType> tableDefinition = new HashMap<String, ColumnDataType>();
        tableDefinition.put("id", ColumnDataType.String);
        tableDefinition.put("dob", ColumnDataType.Date);

        store.defineTable(TestTable, tableDefinition);
        store.initialize();

        try {

            JsonObject item = new JsonObject();
            item.addProperty("id", "1");
            item.addProperty("notDefined", "okok");

            store.upsert(TestTable, item, true);

        } catch (Exception ex) {
            assertNull(ex);
        }
    }

    public void testUpsertThrowsWhenColumnInItemIsNotDefinedAndItIsLocal() throws MobileServiceLocalStoreException {
        SQLiteLocalStore store = new SQLiteLocalStore(this.getContext(), TestDbName, null, 1);

        Map<String, ColumnDataType> tableDefinition = new HashMap<String, ColumnDataType>();
        tableDefinition.put("dob", ColumnDataType.Date);

        store.defineTable(TestTable, tableDefinition);
        store.initialize();

        try {

            JsonObject item = new JsonObject();
            item.addProperty("notDefined", "okok");

            store.upsert(TestTable, item, false);
        } catch (Exception ex) {
            assertTrue(ex instanceof MobileServiceLocalStoreException);
            assertTrue(ex.getCause().getMessage().contains(
                    "table todo has no column named notdefined (code 1): , while compiling: INSERT OR REPLACE INTO \"todo\" (\"notdefined\") VALUES (@p0)"));
        }
    }

    public void testUpsertNoThrowsWhenOnColumnIsItemIsDefinedAndAnotherColumnInItemIsNotDefinedAndItIsLocal() throws MobileServiceLocalStoreException {
        SQLiteLocalStore store = new SQLiteLocalStore(this.getContext(), TestDbName, null, 1);

        Map<String, ColumnDataType> tableDefinition = new HashMap<String, ColumnDataType>();
        tableDefinition.put("id", ColumnDataType.String);
        tableDefinition.put("dob", ColumnDataType.Date);

        store.defineTable(TestTable, tableDefinition);
        store.initialize();

        try {

            JsonObject item = new JsonObject();
            item.addProperty("id", "1");
            item.addProperty("notDefined", "okok");

            store.upsert(TestTable, item, false);
        } catch (Exception ex) {
            assertTrue(ex instanceof MobileServiceLocalStoreException);
            assertTrue(ex.getCause().getMessage().contains(
                    "table todo has no column named notdefined (code 1): , while compiling: INSERT OR REPLACE INTO \"todo\" (\"id\",\"notdefined\") VALUES (@p0,@p1)"));
        }
    }

    public void testUpsertInsertsTheRowWhenItemHasNullValues() throws MobileServiceLocalStoreException {
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

        store.upsert(TestTable, inserted, false);

        JsonObject read = store.lookup(TestTable, "abc");

        assertNotNull(read);
        assertEquals(inserted.get("id").getAsString(), read.get("id").getAsString());
    }

    public void testUpsertInsertsTheRowWhenItDoesNotExist() throws MobileServiceLocalStoreException {
        prepareTodoTable();

        // insert a row and make sure it is inserted
        SQLiteLocalStore store = new SQLiteLocalStore(this.getContext(), TestDbName, null, 1);

        defineTestTable(store);
        store.initialize();

        JsonObject inserted = new JsonObject();
        inserted.addProperty("id", "abc");
        inserted.addProperty("__createdAt", new Date().toString());

        // insert a row and make sure it is inserted
        store.upsert(TestTable, inserted, false);

        long count = SQLiteStoreTestsUtilities.countRows(this.getContext(), TestDbName, TestTable);
        assertEquals(count, 1L);
    }

    public void testUpsertUpdatesTheRowWhenItExists() throws MobileServiceLocalStoreException {
        prepareTodoTable();

        // insert a row and make sure it is inserted
        SQLiteLocalStore store = new SQLiteLocalStore(this.getContext(), TestDbName, null, 1);

        defineTestTable(store);
        store.initialize();

        JsonObject inserted = new JsonObject();
        inserted.addProperty("id", "abc");
        inserted.addProperty("__createdAt", new Date().toString());

        // insert a row and make sure it is inserted
        store.upsert(TestTable, inserted, false);

        JsonObject updated = new JsonObject();
        updated.addProperty("id", "abc");
        updated.addProperty("__createdAt", new Date().toString());

        store.upsert(TestTable, updated, false);

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
        store.upsert(TestTable, originalItem, false);

        // read the item back
        JsonObject itemRead = store.lookup(TestTable, "abc");

        // make sure everything was persisted
        assertNotNull(itemRead);

        // change the item
        originalItem.addProperty("double", 111.222d);

        // upsert the item
        store.upsert(TestTable, originalItem, false);

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