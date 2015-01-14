package com.microsoft.windowsazure.mobileservices.sdk.testapp.framework.mocks;

import com.google.common.base.Function;
import com.google.gson.JsonArray;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import com.microsoft.windowsazure.mobileservices.table.query.Query;
import com.microsoft.windowsazure.mobileservices.table.sync.localstore.ColumnDataType;
import com.microsoft.windowsazure.mobileservices.table.sync.localstore.MobileServiceLocalStore;
import com.microsoft.windowsazure.mobileservices.table.sync.localstore.MobileServiceLocalStoreException;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.LinkedHashMap;
import java.util.LinkedList;
import java.util.List;
import java.util.Map;
import java.util.Queue;

public class MobileServiceLocalStoreMock implements MobileServiceLocalStore {

    // / <summary>
    // / Table that stores sync errors
    // / </summary>
    public static final String SyncErrors = "__errors";
    // / <summary>
    // / Table that stores operation queue items
    // / </summary>
    private static final String OperationQueue = "__operations";
    public Map<String, LinkedHashMap<String, JsonObject>> Tables = new HashMap<String, LinkedHashMap<String, JsonObject>>();
    public List<Query> ReadQueries = new ArrayList<Query>();
    ;
    public List<Query> DeleteQueries = new ArrayList<Query>();
    public Queue<String> ReadResponses = new LinkedList<String>();
    public Function<Query, JsonElement> readAsyncFunc;

    @Override
    public void initialize() throws MobileServiceLocalStoreException {

    }

    @Override
    public void defineTable(String tableName, Map<String, ColumnDataType> columns) throws MobileServiceLocalStoreException {

    }

    @Override
    public JsonElement read(Query query) throws MobileServiceLocalStoreException {

        if (query.getTableName() == OperationQueue || query.getTableName() == SyncErrors) {
            JsonArray array = new JsonArray();

            for (JsonObject jsonObject : GetTable(query.getTableName()).values()) {
                array.add(jsonObject);
            }

            // we don't query the queue specially, we just need all records
            return array;
        }

        this.ReadQueries.add(query);

        JsonElement result;

        if (readAsyncFunc != null) {
            result = readAsyncFunc.apply(query);
        } else {

            String value = ReadResponses.peek();

            if (value == null) {
                return null;
            }

            result = new JsonParser().parse(value);
        }

        return result;
    }

    @Override
    public JsonObject lookup(String tableName, String itemId) throws MobileServiceLocalStoreException {

        Map<String, JsonObject> table = GetTable(tableName);

        JsonObject item = null;

        if (table.containsKey(itemId)) {
            item = table.get(itemId);
        }

        return item;

    }

    @Override
    public void upsert(String tableName, JsonObject item, boolean fromServer) throws MobileServiceLocalStoreException {

        Map<String, JsonObject> table = GetTable(tableName);

        table.put(item.get("id").getAsString(), item);

        return;
    }

    @Override
    public void upsert(String tableName, JsonObject[] items, boolean fromServer) throws MobileServiceLocalStoreException {

        Map<String, JsonObject> table = GetTable(tableName);

        for (JsonObject item : items) {
            table.put(item.get("id").getAsString(), item);
        }

        return;
    }

    @Override
    public void delete(String tableName, String itemId) throws MobileServiceLocalStoreException {

        Map<String, JsonObject> table = GetTable(tableName);

        table.remove(itemId);

        return;
    }

    @Override
    public void delete(String tableName, String[] itemsIds) throws MobileServiceLocalStoreException {

        Map<String, JsonObject> table = GetTable(tableName);

        for (String itemId : itemsIds) {
            table.remove(itemId);
        }

        return;
    }

    @Override
    public void delete(Query query) throws MobileServiceLocalStoreException {
        this.DeleteQueries.add(query);

        this.Tables.get(query.getTableName()).clear();

        return;
    }

    private Map<String, JsonObject> GetTable(String tableName) {
        if (!this.Tables.containsKey(tableName)) {
            this.Tables.put(tableName, new LinkedHashMap<String, JsonObject>());
        }

        return this.Tables.get(tableName);
    }
}
