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
 * RemoteTableOperationProcessor.java
 */
package com.microsoft.windowsazure.mobileservices.table.sync.operations;

import com.google.common.util.concurrent.ListenableFuture;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceException;
import com.microsoft.windowsazure.mobileservices.MobileServiceFeatures;
import com.microsoft.windowsazure.mobileservices.table.MobileServiceJsonTable;
import com.microsoft.windowsazure.mobileservices.table.MobileServiceSystemProperty;

import java.util.EnumSet;
import java.util.Locale;
import java.util.Map.Entry;
import java.util.concurrent.ExecutionException;

/**
 * Processes a table operation against a remote store.
 */
public class RemoteTableOperationProcessor implements TableOperationVisitor<JsonObject> {
    private MobileServiceClient mClient;
    private JsonObject mItem;

    /**
     * Constructor for RemoteTableOperationProcessor
     *
     * @param client the mobile service client
     * @param item   the item to process
     */
    public RemoteTableOperationProcessor(MobileServiceClient client, JsonObject item) {
        this.mClient = client;
        this.mItem = item;
    }

    private static EnumSet<MobileServiceSystemProperty> getSystemProperties(JsonObject instance) {
        EnumSet<MobileServiceSystemProperty> systemProperties = EnumSet.noneOf(MobileServiceSystemProperty.class);

        for (Entry<String, JsonElement> property : instance.entrySet()) {
            String propertyName = property.getKey().trim().toLowerCase(Locale.getDefault());

            switch (propertyName) {
                case "__createdat":
                    systemProperties.add(MobileServiceSystemProperty.CreatedAt);
                    break;
                case "__updatedat":
                    systemProperties.add(MobileServiceSystemProperty.UpdatedAt);
                    break;
                case "__version":
                    systemProperties.add(MobileServiceSystemProperty.Version);
                    break;
                default:
                    break;
            }
        }

        return systemProperties;
    }

    private static JsonObject removeSystemProperties(JsonObject instance) {
        boolean haveCloned = false;

        for (Entry<String, JsonElement> property : instance.entrySet()) {
            if (property.getKey().startsWith("__")) {
                if (!haveCloned) {
                    instance = (JsonObject) new JsonParser().parse(instance.toString());
                    haveCloned = true;
                }

                instance.remove(property.getKey());
            }
        }

        return instance;
    }

    @Override
    public JsonObject visit(InsertOperation operation) throws Throwable {
        MobileServiceJsonTable table = this.getRemoteTable(operation.getTableName());
        table.setSystemProperties(EnumSet.allOf(MobileServiceSystemProperty.class));

        JsonObject item = removeSystemProperties(this.mItem);

        ListenableFuture<JsonObject> future = table.insert(item);

        try {
            return future.get();
        } catch (ExecutionException ex) {
            throw ex.getCause();
        }
    }

    @Override
    public JsonObject visit(UpdateOperation operation) throws Throwable {
        MobileServiceJsonTable table = this.getRemoteTable(operation.getTableName());
        table.setSystemProperties(getSystemProperties(this.mItem));

        ListenableFuture<JsonObject> future = table.update(this.mItem);

        try {
            return future.get();
        } catch (ExecutionException ex) {
            throw ex.getCause();
        }
    }

    @Override
    public JsonObject visit(DeleteOperation operation) throws Throwable {
        MobileServiceJsonTable table = this.getRemoteTable(operation.getTableName());
        ListenableFuture<Void> future = table.delete(this.mItem);

        try {
            future.get();

            return null;
        } catch (ExecutionException ex) {

            if (!ExceptionIs404NotFound(ex)) {
                throw ex.getCause();
            }

            return null;
        }
    }

    private boolean ExceptionIs404NotFound(ExecutionException ex) {

        MobileServiceException mse = (MobileServiceException) ex.getCause();

        if (ex == null) {
            return false;
        }

        int statusCode = mse.getResponse().getStatus().getStatusCode();

        if (statusCode != 404) {
            return false;
        }

        return true;

    }

    /**
     * Gets the item to process
     */
    public JsonObject getItem() {
        return this.mItem;
    }

    /**
     * Sets the item to process
     */
    public void setItem(JsonObject item) {
        this.mItem = item;
    }

    /**
     * Returns an instance of a remote table to be used by this processor
     *
     * @param tableName the name of the remote table
     * @return an instance of a remote table with the given name
     */
    private MobileServiceJsonTable getRemoteTable(String tableName) {
        MobileServiceJsonTable table = this.mClient.getTable(tableName);
        table.addFeature(MobileServiceFeatures.Offline);
        return table;
    }
}
