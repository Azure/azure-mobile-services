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
 * MobileServiceLocalStore.java
 */
package com.microsoft.windowsazure.mobileservices.table.sync.localstore;

import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.microsoft.windowsazure.mobileservices.table.query.Query;

import java.util.Map;

/**
 * Allows saving and reading data in the local tables.
 */
public interface MobileServiceLocalStore {
    /**
     * Initializes the store for use.
     */
    void initialize() throws MobileServiceLocalStoreException;

    /**
     * Defines a table to be created/updated on initialization
     *
     * @param tableName the table name
     * @param columns   a Map of column names and their respective data type
     * @throws MobileServiceLocalStoreException
     */
    void defineTable(String tableName, Map<String, ColumnDataType> columns) throws MobileServiceLocalStoreException;

    /**
     * Retrieve results from the local store.
     *
     * @param query a query to specify the local table and filter results
     * @return A JsonElement with the results
     * @throws MobileServiceLocalStoreException
     */
    JsonElement read(Query query) throws MobileServiceLocalStoreException;

    /**
     * Looks up an item from the local store.
     *
     * @param tableName the local table name
     * @param itemId    the id of the item to look up
     * @return the item found
     * @throws MobileServiceLocalStoreException
     */
    JsonObject lookup(String tableName, String itemId) throws MobileServiceLocalStoreException;

    /**
     * Insert or Update an item in the local store.
     *
     * @param tableName the local table name
     * @param item      the item to be inserted
     * @throws MobileServiceLocalStoreException
     */
    void upsert(String tableName, JsonObject item) throws MobileServiceLocalStoreException;

    /**
     * Insert or Update a list of items in the local store.
     *
     * @param tableName the local table name
     * @param items     the list of items to be inserted
     * @throws MobileServiceLocalStoreException
     */
    void upsert(String tableName, JsonObject[] items) throws MobileServiceLocalStoreException;

    /**
     * Delete an item from the local store.
     *
     * @param tableName the local table name
     * @param itemId    the id of the item to be deleted
     * @throws MobileServiceLocalStoreException
     */
    void delete(String tableName, String itemId) throws MobileServiceLocalStoreException;

    /**
     * Delete an item from the local store.
     *
     * @param tableName the local table name
     * @param itemsIds  the list of ids of the items to be deleted
     * @throws MobileServiceLocalStoreException
     */
    void delete(String tableName, String[] itemsIds) throws MobileServiceLocalStoreException;

    /**
     * Delete items from the local store.
     *
     * @param query a query to specify the local table and filter items
     * @throws MobileServiceLocalStoreException
     */
    void delete(Query query) throws MobileServiceLocalStoreException;
}