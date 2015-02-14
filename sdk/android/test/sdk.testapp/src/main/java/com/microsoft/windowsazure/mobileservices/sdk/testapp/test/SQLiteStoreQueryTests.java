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
import com.microsoft.windowsazure.mobileservices.table.query.Query;
import com.microsoft.windowsazure.mobileservices.table.query.QueryOperations;
import com.microsoft.windowsazure.mobileservices.table.query.QueryOrder;
import com.microsoft.windowsazure.mobileservices.table.serialization.DateSerializer;
import com.microsoft.windowsazure.mobileservices.table.sync.localstore.ColumnDataType;
import com.microsoft.windowsazure.mobileservices.table.sync.localstore.MobileServiceLocalStoreException;
import com.microsoft.windowsazure.mobileservices.table.sync.localstore.SQLiteLocalStore;

import java.util.ArrayList;
import java.util.Calendar;
import java.util.Date;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

public class SQLiteStoreQueryTests extends InstrumentationTestCase {

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

    public void testQueryOnBool() throws MobileServiceException, MobileServiceLocalStoreException {
        Query query1 = QueryOperations.tableName(TestTable).field("col5").eq(true);
        testQuery(query1, 4);

        Query query2 = QueryOperations.tableName(TestTable).field("col5").eq(false);
        testQuery(query2, 2);
    }

    public void testQueryNotOperatorWithBoolComparison() throws MobileServiceException, MobileServiceLocalStoreException {
        Query query = QueryOperations.tableName(TestTable).not().field("col5").eq(true);
        testQuery(query, 2);
    }

    public void testQueryDateBeforeEpoch() throws MobileServiceException, MobileServiceLocalStoreException {
        Query query = QueryOperations.tableName(TestTable).field("col4").gt(epoch);
        testQuery(query, 6);
    }

    public void testQueryWithTop() throws MobileServiceLocalStoreException {
        Query query = QueryOperations.tableName(TestTable).top(5);
        testQuery(query, 5);
    }

    public void testQueryWithSelection() throws MobileServiceLocalStoreException {
        Query query = QueryOperations.tableName(TestTable).select("col1", "col2");

        JsonArray results = runQuery(query);

        assertEquals(results.size(), 6);

        assertJsonArraysAreEqual(results, getQueryWithSelectionTestData());
    }

    public void testQueryWithOrderingDescending() throws MobileServiceLocalStoreException {
        Query query = QueryOperations.tableName(TestTable).select("col1", "col2").orderBy("col2", QueryOrder.Descending);

        JsonArray results = runQuery(query);

        assertEquals(results.size(), 6);

        assertJsonArraysAreEqual(results, getQueryWithOrderingDescending());
    }

    public void testQueryComplexFilter() throws MobileServiceException, MobileServiceLocalStoreException {
        Query query = QueryOperations.tableName(TestTable).field("col1").eq("brown").or().field("col1").eq("fox").and().field("col2").le(5);

        JsonArray results = runQuery(query);

        assertEquals(results.size(), 1);

        assertEquals(results.get(0).getAsJsonObject().get("id").getAsString(), getTestData()[2].getAsJsonObject().get("id").getAsString());
    }

    // public void testQueryOnStringIndexOf() throws MobileServiceException,
    // MobileServiceLocalStoreException {
    // Query query1 = QueryOperations.tableName(TestTable).indexOf("col1",
    // "ump").eq(1);
    //
    // testQuery(query1, 1);
    //
    // Query query2 = QueryOperations.tableName(TestTable).indexOf("col1",
    // "ump").eq(2);
    //
    // testQuery(query2, 0);
    // }

    public void testQueryOnStringStartsWith() throws MobileServiceException, MobileServiceLocalStoreException {
        Query query1 = QueryOperations.tableName(TestTable).startsWith("col1", "qu");

        testQuery(query1, 1);

        Query query2 = QueryOperations.tableName(TestTable).startsWith("col1", "ump");

        testQuery(query2, 0);
    }

    public void testQueryOnStringEndsWith() throws MobileServiceException, MobileServiceLocalStoreException {
        Query query1 = QueryOperations.tableName(TestTable).endsWith("col1", "ox");

        testQuery(query1, 1);

        Query query2 = QueryOperations.tableName(TestTable).endsWith("col1", "ump");

        testQuery(query2, 0);
    }

    public void testQueryOnStringSubstringOf() throws MobileServiceException, MobileServiceLocalStoreException {
        Query query1 = QueryOperations.tableName(TestTable).subStringOf("qu", "col1");

        testQuery(query1, 1);

        Query query2 = QueryOperations.tableName(TestTable).subStringOf("fer", "col1");

        testQuery(query2, 0);
    }

    public void testQueryOnStringTrim() throws MobileServiceException, MobileServiceLocalStoreException {
        Query query1 = QueryOperations.tableName(TestTable).trim("col1").eq("EndsWithBackslash\\");

        testQuery(query1, 1);

        Query query2 = QueryOperations.tableName(TestTable).trim("col1").eq(" EndsWithBackslash\\");

        testQuery(query2, 0);
    }

    public void testQueryOnStringConcatAndCompare() throws MobileServiceLocalStoreException, MobileServiceException {
        Query query1 = QueryOperations.tableName(TestTable).concat(QueryOperations.concat(QueryOperations.field("col1"), "ies"), QueryOperations.field("col2"))
                .eq("brownies1.0");

        testQuery(query1, 1);

        Query query2 = QueryOperations.tableName(TestTable).concat(QueryOperations.concat(QueryOperations.field("col1"), "ies"), QueryOperations.field("col2"))
                .eq("brownies2.0");

        testQuery(query2, 0);
    }

    public void testQueryOnYear() throws MobileServiceLocalStoreException, MobileServiceException {
        Query query1 = QueryOperations.tableName(TestTable).year("col4").eq(1970);

        testQuery(query1, 6);

        Query query2 = QueryOperations.tableName(TestTable).year("col4").eq(2015);

        testQuery(query2, 0);
    }

    public void testQueryOnMonth() throws MobileServiceLocalStoreException, MobileServiceException {
        Query query1 = QueryOperations.tableName(TestTable).month("col4").eq(1);

        testQuery(query1, 6);

        Query query2 = QueryOperations.tableName(TestTable).month("col4").eq(12);

        testQuery(query2, 0);
    }

    public void testQueryOnDay() throws MobileServiceLocalStoreException, MobileServiceException {
        Query query1 = QueryOperations.tableName(TestTable).month("col4").eq(1);

        testQuery(query1, 6);

        Query query2 = QueryOperations.tableName(TestTable).month("col4").eq(31);

        testQuery(query2, 0);
    }

    public void testQueryOnStringReplaceAndCompare() throws MobileServiceException, MobileServiceLocalStoreException {
        Query query1 = QueryOperations.tableName(TestTable).replace("col1", "j", "p").eq("pumped");

        testQuery(query1, 1);

        Query query2 = QueryOperations.tableName(TestTable).replace("col1", "j", "x").eq("pumped");

        testQuery(query2, 0);
    }

    public void testQueryOnStringSubstringAndCompare() throws MobileServiceException, MobileServiceLocalStoreException {

        Query query1 = QueryOperations.tableName(TestTable).subString("col1", 1).eq("ox");

        testQuery(query1, 1);

        Query query2 = QueryOperations.tableName(TestTable).subString("col1", 1).eq("oy");

        testQuery(query2, 0);
    }

    public void testQueryOnStringSubstringToUpper() throws MobileServiceException, MobileServiceLocalStoreException {
        Query query1 = QueryOperations.tableName(TestTable).toUpper("col1").eq("FOX");

        testQuery(query1, 1);

        Query query2 = QueryOperations.tableName(TestTable).toUpper("col1").eq("fox");

        testQuery(query2, 0);
    }

    public void testQueryOnStringSubstringToLower() throws MobileServiceException, MobileServiceLocalStoreException {
        Query query1 = QueryOperations.tableName(TestTable).toLower("col1").eq("fox");

        testQuery(query1, 1);

        Query query2 = QueryOperations.tableName(TestTable).toLower("col1").eq("FOX");

        testQuery(query2, 0);
    }

    public void testQueryOnStringSubstringWithLengthAndCompare() throws MobileServiceException, MobileServiceLocalStoreException {
        Query query1 = QueryOperations.tableName(TestTable).subString("col1", 1, 3).eq("uic");

        testQuery(query1, 1);

        Query query2 = QueryOperations.tableName(TestTable).subString("col1", 1, 3).eq("uix");

        testQuery(query2, 0);
    }

    public void testQueryMathModuloOperator() throws MobileServiceLocalStoreException, MobileServiceException {
        Query query = QueryOperations.tableName(TestTable).field("col3").mod(3).eq(0);

        JsonArray results = runQuery(query);

        assertEquals(results.size(), 3);
    }

    public void testQueryMathRound() throws MobileServiceException, MobileServiceLocalStoreException {
        List<JsonObject> result = new ArrayList<JsonObject>();

        JsonObject item5 = new JsonObject();
        item5.addProperty("val", -0.0900);
        item5.addProperty("expected", 0);
        result.add(item5);

        JsonObject item6 = new JsonObject();
        item6.addProperty("val", -1.0900);
        item6.addProperty("expected", -1);
        result.add(item6);

        JsonObject item4 = new JsonObject();
        item4.addProperty("val", 1.0900);
        item4.addProperty("expected", 1);
        result.add(item4);

        JsonObject item1 = new JsonObject();
        item1.addProperty("val", 0.0900);
        item1.addProperty("expected", 0);
        result.add(item1);

        JsonObject item2 = new JsonObject();
        item2.addProperty("val", 1.5);
        item2.addProperty("expected", 2);
        result.add(item2);

        JsonObject item3 = new JsonObject();
        item3.addProperty("val", -1.5);
        item3.addProperty("expected", -2);
        result.add(item3);

        Query query = QueryOperations.tableName(MathTestTable).round(QueryOperations.field("val")).eq(QueryOperations.field("expected"));

        testMathQuery(result.toArray(new JsonObject[0]), query);
    }

    public void testQueryMathCeiling() throws MobileServiceException, MobileServiceLocalStoreException {
        List<JsonObject> result = new ArrayList<JsonObject>();

        JsonObject item5 = new JsonObject();
        item5.addProperty("val", -0.0900);
        item5.addProperty("expected", 0);
        result.add(item5);

        JsonObject item6 = new JsonObject();
        item6.addProperty("val", -1.0900);
        item6.addProperty("expected", -1);
        result.add(item6);

        JsonObject item4 = new JsonObject();
        item4.addProperty("val", 1.0900);
        item4.addProperty("expected", 2);
        result.add(item4);

        JsonObject item1 = new JsonObject();
        item1.addProperty("val", 0.0900);
        item1.addProperty("expected", 1);
        result.add(item1);

        Query query = QueryOperations.tableName(MathTestTable).ceiling(QueryOperations.field("val")).eq(QueryOperations.field("expected"));

        testMathQuery(result.toArray(new JsonObject[0]), query);
    }

    public void testQueryOnStringLength() throws MobileServiceException, MobileServiceLocalStoreException {
        Query query1 = QueryOperations.tableName(TestTable).length("col1").eq(19);

        testQuery(query1, 1);

        Query query2 = QueryOperations.tableName(TestTable).length("col1").eq(20);

        testQuery(query2, 0);
    }

    public void testQueryWithPaging() throws MobileServiceLocalStoreException {
        for (int skip = 0; skip < 4; skip++) {
            for (int take = 1; take < 4; take++) {
                Query query = QueryOperations.tableName(TestTable).skip(skip).top(take);

                JsonArray queryResults = runQuery(query);

                if ((skip + take - 1) < getTestData().length) {
                    assertEquals(queryResults.get(take - 1).getAsJsonObject().get("id").getAsString(), getTestData()[skip + take - 1].get("id").getAsString());
                }
            }
        }
    }

    public void testQueryWithTotalCount() throws MobileServiceLocalStoreException {
        Query query = QueryOperations.tableName(TestTable).top(5).includeInlineCount();

        JsonObject queryResults = runQuery(query);

        JsonArray results = queryResults.get("results").getAsJsonArray();
        long resultCount = queryResults.get("count").getAsLong();

        assertEquals(results.size(), 5);
        assertEquals(resultCount, 6);
    }

    private JsonObject[] getTestData() {

        ArrayList<JsonObject> result = new ArrayList<JsonObject>();

        Calendar cal1 = Calendar.getInstance();
        cal1.set(1970, Calendar.JANUARY, 1);
        cal1.add(Calendar.MILLISECOND, 32434);

        JsonObject item1 = new JsonObject();
        item1.addProperty("id", "1");
        item1.addProperty("col1", "the");
        item1.addProperty("col2", 5.0);
        item1.addProperty("col3", 234f);
        item1.addProperty("col4", DateSerializer.serialize(cal1.getTime()));
        item1.addProperty("col5", false);
        result.add(item1);

        Calendar cal2 = Calendar.getInstance();
        cal2.set(1970, Calendar.JANUARY, 1);
        cal2.add(Calendar.MILLISECOND, 99797);

        JsonObject item2 = new JsonObject();
        item2.addProperty("id", "2");
        item2.addProperty("col1", "quick");
        item2.addProperty("col2", 3.0);
        item2.addProperty("col3", 9867.12);
        item2.addProperty("col4", DateSerializer.serialize(cal2.getTime()));
        item2.addProperty("col5", true);
        result.add(item2);

        Calendar cal3 = Calendar.getInstance();
        cal3.set(1970, Calendar.JANUARY, 1);
        cal3.add(Calendar.MILLISECOND, 239873840);

        JsonObject item3 = new JsonObject();
        item3.addProperty("id", "3");
        item3.addProperty("col1", "brown");
        item3.addProperty("col2", 1.0);
        item3.addProperty("col3", 11f);
        item3.addProperty("col4", DateSerializer.serialize(cal3.getTime()));
        item3.addProperty("col5", false);
        result.add(item3);

        Calendar cal4 = Calendar.getInstance();
        cal4.set(1970, Calendar.JANUARY, 1);
        cal4.add(Calendar.MILLISECOND, 888888888);

        JsonObject item4 = new JsonObject();
        item4.addProperty("id", "4");
        item4.addProperty("col1", "fox");
        item4.addProperty("col2", 6.0);
        item4.addProperty("col3", 23908.99);
        item4.addProperty("col4", DateSerializer.serialize(cal4.getTime()));
        item4.addProperty("col5", true);
        result.add(item4);

        Calendar cal5 = Calendar.getInstance();
        cal5.set(1970, Calendar.JANUARY, 1);
        cal5.add(Calendar.MILLISECOND, 333333332);

        JsonObject item5 = new JsonObject();
        item5.addProperty("id", "5");
        item5.addProperty("col1", "jumped");
        item5.addProperty("col2", 9.0);
        item5.addProperty("col3", 678.932);
        item5.addProperty("col4", DateSerializer.serialize(cal5.getTime()));
        item5.addProperty("col5", true);
        result.add(item5);

        Calendar cal6 = Calendar.getInstance();
        cal6.set(1970, Calendar.JANUARY, 1);
        cal6.add(Calendar.MILLISECOND, 333333333);

        JsonObject item6 = new JsonObject();
        item6.addProperty("id", "6");
        item6.addProperty("col1", " EndsWithBackslash\\");
        item6.addProperty("col2", 8.0);
        item6.addProperty("col3", 521f);
        item6.addProperty("col4", DateSerializer.serialize(cal6.getTime()));
        item6.addProperty("col5", true);
        result.add(item6);

        return result.toArray(new JsonObject[0]);
    }

    private JsonArray getQueryWithSelectionTestData() {

        JsonArray result = new JsonArray();

        JsonObject item1 = new JsonObject();
        item1.addProperty("col1", "the");
        item1.addProperty("col2", 5.0);
        result.add(item1);

        JsonObject item2 = new JsonObject();
        item2.addProperty("col1", "quick");
        item2.addProperty("col2", 3.0);
        result.add(item2);

        JsonObject item3 = new JsonObject();
        item3.addProperty("col1", "brown");
        item3.addProperty("col2", 1.0);
        result.add(item3);

        JsonObject item4 = new JsonObject();
        item4.addProperty("col1", "fox");
        item4.addProperty("col2", 6.0);
        result.add(item4);

        JsonObject item5 = new JsonObject();
        item5.addProperty("col1", "jumped");
        item5.addProperty("col2", 9.0);
        result.add(item5);

        JsonObject item6 = new JsonObject();
        item6.addProperty("col1", " EndsWithBackslash\\");
        item6.addProperty("col2", 8.0);
        result.add(item6);

        return result;
    }

    private JsonArray getQueryWithOrderingDescending() {

        JsonArray result = new JsonArray();

        JsonObject item5 = new JsonObject();
        item5.addProperty("col1", "jumped");
        item5.addProperty("col2", 9.0);
        result.add(item5);

        JsonObject item6 = new JsonObject();
        item6.addProperty("col1", " EndsWithBackslash\\");
        item6.addProperty("col2", 8.0);
        result.add(item6);

        JsonObject item4 = new JsonObject();
        item4.addProperty("col1", "fox");
        item4.addProperty("col2", 6.0);
        result.add(item4);

        JsonObject item1 = new JsonObject();
        item1.addProperty("col1", "the");
        item1.addProperty("col2", 5.0);
        result.add(item1);

        JsonObject item2 = new JsonObject();
        item2.addProperty("col1", "quick");
        item2.addProperty("col2", 3.0);
        result.add(item2);

        JsonObject item3 = new JsonObject();
        item3.addProperty("col1", "brown");
        item3.addProperty("col2", 1.0);
        result.add(item3);

        return result;
    }

    private void assertJsonArraysAreEqual(JsonArray results, JsonArray expected) {
        String actualResult = results.toString();
        String expectedResult = expected.toString();
        assertEquals(actualResult, expectedResult);
    }

    private void testMathQuery(JsonObject[] mathTestData, Query query) throws MobileServiceLocalStoreException {
        SQLiteLocalStore store = setupMathTestTable(mathTestData);

        JsonArray results = runQuery(store, query);

        assertEquals(results.size(), mathTestData.length);
    }

    private void testQuery(Query query, int expectedResults) throws MobileServiceLocalStoreException {

        JsonArray results = runQuery(query);

        assertEquals(results.size(), expectedResults);
    }

    private <T> T runQuery(Query query) throws MobileServiceLocalStoreException // where
    // T:JToken
    {
        SQLiteLocalStore store = setupTestTable();

        return runQuery(store, query);
    }

    @SuppressWarnings("unchecked")
    private <T> T runQuery(SQLiteLocalStore store, Query query) throws MobileServiceLocalStoreException // where
    // T:JToken
    {
        T read = (T) store.read(query);

        return read;
    }

    private SQLiteLocalStore setupMathTestTable(JsonObject[] mathTestData) throws MobileServiceLocalStoreException {
        SQLiteStoreTestsUtilities.dropTestTable(this.getContext(), TestDbName, MathTestTable);

        // first create a table called todo
        SQLiteLocalStore store = new SQLiteLocalStore(this.getContext(), TestDbName, null, 1);

        Map<String, ColumnDataType> tableDefinition = new HashMap<String, ColumnDataType>();
        tableDefinition.put("val", ColumnDataType.Real);
        tableDefinition.put("expected", ColumnDataType.Real);

        store.defineTable(MathTestTable, tableDefinition);

        store.initialize();

        insertAll(store, MathTestTable, mathTestData);

        return store;
    }

    private SQLiteLocalStore setupTestTable() throws MobileServiceLocalStoreException {
        if (!queryTableInitialized) {
            SQLiteStoreTestsUtilities.dropTestTable(this.getContext(), TestDbName, TestTable);
        }

        // first create a table called todo
        SQLiteLocalStore store = new SQLiteLocalStore(this.getContext(), TestDbName, null, 1);

        Map<String, ColumnDataType> tableDefinition = new HashMap<String, ColumnDataType>();
        tableDefinition.put("col1", ColumnDataType.String);
        tableDefinition.put("col2", ColumnDataType.Real);
        tableDefinition.put("col3", ColumnDataType.Real);
        tableDefinition.put("col4", ColumnDataType.Date);
        tableDefinition.put("col5", ColumnDataType.Boolean);

        store.defineTable(TestTable, tableDefinition);

        store.initialize();

        if (!queryTableInitialized) {
            insertAll(store, TestTable, getTestData());
        }

        queryTableInitialized = true;

        return store;
    }

    private void insertAll(SQLiteLocalStore store, String tableName, JsonObject[] items) throws MobileServiceLocalStoreException {
        store.upsert(tableName, items, false);
    }

    private Context getContext() {
        return getInstrumentation().getTargetContext();
    }
}