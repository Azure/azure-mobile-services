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
package com.microsoft.windowsazure.mobileservices.table.sync.queue;

import java.text.ParseException;
import java.util.ArrayList;
import java.util.Date;
import java.util.List;

import com.google.gson.JsonArray;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.microsoft.windowsazure.mobileservices.table.query.QueryOperations;
import com.microsoft.windowsazure.mobileservices.table.serialization.DateSerializer;
import com.microsoft.windowsazure.mobileservices.table.sync.MobileServiceLocalStore;
import com.microsoft.windowsazure.mobileservices.table.sync.MobileServiceLocalStoreException;
import com.microsoft.windowsazure.mobileservices.table.sync.operations.TableOperationError;
import com.microsoft.windowsazure.mobileservices.table.sync.operations.TableOperationKind;

/**
 * Queue of all operation errors
 */
public class OperationErrorList {
	/**
	 * Table that stores operation errors
	 */
	private static final String OPERATION_ERROR_TABLE = "__errors";

	private MobileServiceLocalStore mStore;

	private List<TableOperationError> mList;

	private OperationErrorList(MobileServiceLocalStore store) {
		this.mStore = store;
	}

	public void add(TableOperationError operationError) throws ParseException, MobileServiceLocalStoreException {
		this.mStore.upsert(OPERATION_ERROR_TABLE, serialize(operationError));

		this.mList.add(operationError);
	}

	public int countPending() {
		return this.mList.size();
	}

	public List<TableOperationError> getAll() {
		return new ArrayList<TableOperationError>(this.mList);
	}

	public void clear() throws MobileServiceLocalStoreException {
		this.mList.clear();

		this.mStore.delete(QueryOperations.tableName(OPERATION_ERROR_TABLE));
	}

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
		element.addProperty("operationKind", operationError.getOperationKind().getValue());
		element.addProperty("tableName", operationError.getTableName());
		element.addProperty("itemId", operationError.getItemId());

		if (operationError.getClientItem() != null) {
			element.add("clientItem", operationError.getClientItem());
		}

		element.addProperty("errorMessage", operationError.getErrorMessage());

		if (operationError.getStatusCode() != null) {
			element.addProperty("statusCode", operationError.getStatusCode());
		}

		if (operationError.getServerResponse() != null) {
			element.addProperty("serverResponse", operationError.getServerResponse());
		}

		if (operationError.getServerItem() != null) {
			element.add("serverItem", operationError.getServerItem());
		}

		element.addProperty("__createdAt", DateSerializer.serialize(operationError.getCreatedAt()));

		return element;
	}

	private static TableOperationError deserialize(JsonObject element) throws ParseException {
		String id = element.get("id").getAsString();
		int operationKind = element.get("operationKind").getAsInt();
		String tableName = element.get("tableName").getAsString();
		String itemId = element.get("itemId").getAsString();
		JsonObject clientItem = element.get("clientItem") != null ? element.get("clientItem").getAsJsonObject() : null;
		String errorMessage = element.get("errorMessage").getAsString();
		Integer statusCode = element.get("statusCode") != null ? element.get("statusCode").getAsInt() : null;
		String serverResponse = element.get("serverResponse") != null ? element.get("serverResponse").getAsString() : null;
		JsonObject serverItem = element.get("serverItem") != null ? element.get("serverItem").getAsJsonObject() : null;
		Date createdAt = DateSerializer.deserialize(element.get("__createdAt").getAsString());

		return TableOperationError.parse(id, TableOperationKind.parse(operationKind), tableName, itemId, clientItem, errorMessage, statusCode, serverResponse,
				serverItem, createdAt);
	}
}