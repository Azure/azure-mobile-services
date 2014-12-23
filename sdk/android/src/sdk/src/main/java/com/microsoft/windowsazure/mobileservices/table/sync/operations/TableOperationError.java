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
 * TableOperationError.java
 */
package com.microsoft.windowsazure.mobileservices.table.sync.operations;

import com.google.gson.JsonObject;

import java.util.Date;
import java.util.UUID;

/**
 * Class representing a table operation error against remote table.
 */
public class TableOperationError {
    private String mId;
    private TableOperationKind mOperationKind;
    private String mTableName;
    private String mItemId;
    private JsonObject mClientItem;
    private String mErrorMessage;
    private Integer mStatusCode;
    private String mServerResponse;
    private JsonObject mServerItem;
    private Date mCreatedAt;

    /**
     * Constructor for TableOperationError
     *
     * @param operationKind  the kind of table operation
     * @param tableName      the table name
     * @param itemId         the item id
     * @param clientItem     the client item
     * @param errorMessage   the error message
     * @param statusCode     the status code of the response
     * @param serverResponse the server response
     * @param serverItem     the server item
     */
    public TableOperationError(TableOperationKind operationKind, String tableName, String itemId, JsonObject clientItem, String errorMessage,
                               Integer statusCode, String serverResponse, JsonObject serverItem) {
        this.mId = UUID.randomUUID().toString();
        this.mOperationKind = operationKind;
        this.mTableName = tableName;
        this.mItemId = itemId;
        this.mClientItem = clientItem;
        this.mErrorMessage = errorMessage;
        this.mStatusCode = statusCode;
        this.mServerResponse = serverResponse;
        this.mServerItem = serverItem;
        this.mCreatedAt = new Date();
    }

    /**
     * Creates a new table operation error
     *
     * @param id             the table operation error id
     * @param operationKind  the kind of table operation
     * @param tableName      the table name
     * @param itemId         the item id
     * @param clientItem     the client item
     * @param errorMessage   the error message
     * @param statusCode     the status code of the response
     * @param serverResponse the server response
     * @param serverItem     the server item
     * @param createdAt      the creation date of the table operation error
     * @return the table operation error
     */
    public static TableOperationError create(String id, TableOperationKind operationKind, String tableName, String itemId, JsonObject clientItem,
                                             String errorMessage, Integer statusCode, String serverResponse, JsonObject serverItem, Date createdAt) {
        TableOperationError operationError = new TableOperationError(operationKind, tableName, itemId, clientItem, errorMessage, statusCode, serverResponse,
                serverItem);
        operationError.mId = id;
        operationError.mCreatedAt = createdAt;
        return operationError;
    }

    /**
     * Gets the table operation error id
     */
    public String getId() {
        return this.mId;
    }

    /**
     * Gets the kind of table operation
     */
    public TableOperationKind getOperationKind() {
        return this.mOperationKind;
    }

    /**
     * Gets the table name
     */
    public String getTableName() {
        return this.mTableName;
    }

    /**
     * Gets the item id
     */
    public String getItemId() {
        return this.mItemId;
    }

    /**
     * Gets the client item
     */
    public JsonObject getClientItem() {
        return this.mClientItem;
    }

    /**
     * Gets the error message
     */
    public String getErrorMessage() {
        return this.mErrorMessage;
    }

    /**
     * Gets the status code of the response
     */
    public Integer getStatusCode() {
        return this.mStatusCode;
    }

    /**
     * Gets the server response
     */
    public String getServerResponse() {
        return this.mServerResponse;
    }

    /**
     * Gets the server item
     */
    public JsonObject getServerItem() {
        return this.mServerItem;
    }

    /**
     * Gets the creation date of the table operation error
     */
    public Date getCreatedAt() {
        return this.mCreatedAt;
    }
}