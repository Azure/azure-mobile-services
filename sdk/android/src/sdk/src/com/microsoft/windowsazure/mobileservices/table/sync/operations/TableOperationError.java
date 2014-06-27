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
package com.microsoft.windowsazure.mobileservices.table.sync.operations;

import java.util.Date;
import java.util.UUID;

import com.google.gson.JsonObject;

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

	public String getId() {
		return this.mId;
	}

	public TableOperationKind getOperationKind() {
		return this.mOperationKind;
	}

	public String getTableName() {
		return this.mTableName;
	}

	public String getItemId() {
		return this.mItemId;
	}

	public JsonObject getClientItem() {
		return this.mClientItem;
	}

	public String getErrorMessage() {
		return this.mErrorMessage;
	}

	public Integer getStatusCode() {
		return this.mStatusCode;
	}

	public String getServerResponse() {
		return this.mServerResponse;
	}

	public JsonObject getServerItem() {
		return this.mServerItem;
	}

	public Date getCreatedAt() {
		return this.mCreatedAt;
	}

	public static TableOperationError parse(String id, TableOperationKind operationKind, String tableName, String itemId, JsonObject clientItem,
			String errorMessage, Integer statusCode, String serverResponse, JsonObject serverItem, Date createdAt) {
		TableOperationError operationError = new TableOperationError(operationKind, tableName, itemId, clientItem, errorMessage, statusCode, serverResponse,
				serverItem);
		operationError.mId = id;
		operationError.mCreatedAt = createdAt;
		return operationError;
	}
}