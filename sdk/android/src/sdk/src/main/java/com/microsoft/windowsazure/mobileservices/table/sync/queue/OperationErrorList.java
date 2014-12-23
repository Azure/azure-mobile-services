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

/**
 * OperationErrorList.java
 */
package com.microsoft.windowsazure.mobileservices.table.sync.queue;

import com.google.gson.JsonArray;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.microsoft.windowsazure.mobileservices.table.query.QueryOperations;
import com.microsoft.windowsazure.mobileservices.table.serialization.DateSerializer;
import com.microsoft.windowsazure.mobileservices.table.sync.localstore.ColumnDataType;
import com.microsoft.windowsazure.mobileservices.table.sync.localstore.MobileServiceLocalStore;
import com.microsoft.windowsazure.mobileservices.table.sync.localstore.MobileServiceLocalStoreException;
import com.microsoft.windowsazure.mobileservices.table.sync.operations.TableOperationError;
import com.microsoft.windowsazure.mobileservices.table.sync.operations.TableOperationKind;

import java.text.ParseException;
import java.util.ArrayList;
import java.util.Date;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.concurrent.locks.ReadWriteLock;
import java.util.concurrent.locks.ReentrantReadWriteLock;

/**
 * List of all table operation errors
 */
public class OperationErrorList {
    /**
     * Table that stores operation errors
     */
    private static final String OPERATION_ERROR_TABLE = "__errors";

    private MobileServiceLocalStore mStore;

    private List<TableOperationError> mList;

    private ReadWriteLock mSyncLock;

    private OperationErrorList(MobileServiceLocalStore store) {
        this.mStore = store;
        this.mList = new ArrayList<TableOperationError>();
        this.mSyncLock = new ReentrantReadWriteLock(true);
    }

    /**
     * Initializes requirements on the local store
     *
     * @param store the local store
     * @throws MobileServiceLocalStoreException
     */
    public static void initializeStore(MobileServiceLocalStore store) throws MobileServiceLocalStoreException {
        Map<String, ColumnDataType> columns = new HashMap<String, ColumnDataType>();
        columns.put("id", ColumnDataType.String);
        columns.put("tablename", ColumnDataType.String);
        columns.put("itemid", ColumnDataType.String);
        columns.put("clientitem", ColumnDataType.Other);
        columns.put("errormessage", ColumnDataType.String);
        columns.put("statuscode", ColumnDataType.Number);
        columns.put("serverresponse", ColumnDataType.String);
        columns.put("serveritem", ColumnDataType.Other);
        columns.put("__createdat", ColumnDataType.Date);

        store.defineTable(OPERATION_ERROR_TABLE, columns);
    }

    /**
     * Loads the list of table operation errors from the local store
     *
     * @param store the local store
     * @return the list of table operation errors
     * @throws java.text.ParseException
     * @throws MobileServiceLocalStoreException
     */
    public static OperationErrorList load(MobileServiceLocalStore store) throws ParseException, MobileServiceLocalStoreException {
        OperationErrorList opQueue = new OperationErrorList(store);

        JsonElement operations = store.read(QueryOperations.tableName(OPERATION_ERROR_TABLE));

        if (operations.isJsonArray()) {
            JsonArray array = (JsonArray) operations;

            for (JsonElement element : array) {
                if (element.isJsonObject()) {
                    TableOperationError operationError = deserialize((JsonObject) element);
                    opQueue.mList.add(operationError);
                }
            }
        }

        return opQueue;
    }

    private static JsonObject serialize(TableOperationError operationError) throws ParseException {
        JsonObject element = new JsonObject();

        element.addProperty("id", operationError.getId());
        element.addProperty("operationkind", operationError.getOperationKind().getValue());
        element.addProperty("tablename", operationError.getTableName());
        element.addProperty("itemid", operationError.getItemId());

        if (operationError.getClientItem() != null) {
            element.add("clientitem", operationError.getClientItem());
        }

        element.addProperty("errormessage", operationError.getErrorMessage());

        if (operationError.getStatusCode() != null) {
            element.addProperty("statuscode", operationError.getStatusCode());
        }

        if (operationError.getServerResponse() != null) {
            element.addProperty("serverresponse", operationError.getServerResponse());
        }

        if (operationError.getServerItem() != null) {
            element.add("serveritem", operationError.getServerItem());
        }

        element.addProperty("__createdat", DateSerializer.serialize(operationError.getCreatedAt()));

        return element;
    }

    private static TableOperationError deserialize(JsonObject element) throws ParseException {
        String id = element.get("id").getAsString();
        int operationKind = element.get("operationkind").getAsInt();
        String tableName = element.get("tablename").getAsString();
        String itemId = element.get("itemid").getAsString();
        JsonObject clientItem = element.get("clientitem") != null ? element.get("clientitem").getAsJsonObject() : null;
        String errorMessage = element.get("errormessage").getAsString();
        Integer statusCode = element.get("statuscode") != null ? element.get("statuscode").getAsInt() : null;
        String serverResponse = element.get("serverresponse") != null ? element.get("serverresponse").getAsString() : null;
        JsonObject serverItem = element.get("serveritem") != null ? element.get("serveritem").getAsJsonObject() : null;
        Date createdAt = DateSerializer.deserialize(element.get("__createdat").getAsString());

        return TableOperationError.create(id, TableOperationKind.parse(operationKind), tableName, itemId, clientItem, errorMessage, statusCode, serverResponse,
                serverItem, createdAt);
    }

    /**
     * Adds a new table operation error
     *
     * @param operationError the table operation error
     * @throws java.text.ParseException
     * @throws MobileServiceLocalStoreException
     */
    public void add(TableOperationError operationError) throws ParseException, MobileServiceLocalStoreException {
        this.mSyncLock.writeLock().lock();

        try {
            this.mStore.upsert(OPERATION_ERROR_TABLE, serialize(operationError));

            this.mList.add(operationError);
        } finally {
            this.mSyncLock.writeLock().unlock();
        }
    }

    /**
     * Returns the count of pending table operation errors
     */
    public int countPending() {
        this.mSyncLock.readLock().lock();

        try {
            return this.mList.size();
        } finally {
            this.mSyncLock.readLock().unlock();
        }
    }

    /**
     * Returns the list of all pending table operation errors
     */
    public List<TableOperationError> getAll() {
        this.mSyncLock.readLock().lock();

        try {
            return new ArrayList<TableOperationError>(this.mList);
        } finally {
            this.mSyncLock.readLock().unlock();
        }
    }

    /**
     * Empties the list of pending table operation errors
     *
     * @throws MobileServiceLocalStoreException
     */
    public void clear() throws MobileServiceLocalStoreException {
        this.mSyncLock.writeLock().lock();

        try {
            this.mList.clear();

            this.mStore.delete(QueryOperations.tableName(OPERATION_ERROR_TABLE));
        } finally {
            this.mSyncLock.writeLock().unlock();
        }
    }
}