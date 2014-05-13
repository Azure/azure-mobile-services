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

import java.util.concurrent.ExecutionException;
import com.google.common.util.concurrent.ListenableFuture;
import com.google.gson.JsonObject;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.table.MobileServiceJsonTable;

public class TableOperationProcessor implements TableOperationVisitor<JsonObject> {
	MobileServiceClient mClient;

	JsonObject mItem;

	public TableOperationProcessor(MobileServiceClient client, JsonObject item) {
		this.mClient = client;
		this.mItem = item;
	}

	@Override
	public JsonObject Visit(InsertOperation operation) throws Throwable {
		MobileServiceJsonTable table = this.mClient.getTable(operation.getTableName());

		ListenableFuture<JsonObject> future = table.insert(this.mItem);

		try {
			return future.get();
		} catch (ExecutionException ex) {
			throw ex.getCause();
		}
	}

	@Override
	public JsonObject Visit(UpdateOperation operation) throws Throwable {
		MobileServiceJsonTable table = this.mClient.getTable(operation.getTableName());

		ListenableFuture<JsonObject> future = table.update(this.mItem);

		try {
			return future.get();
		} catch (ExecutionException ex) {
			throw ex.getCause();
		}
	}

	@Override
	public JsonObject Visit(DeleteOperation operation) throws Throwable {
		MobileServiceJsonTable table = this.mClient.getTable(operation.getTableName());

		ListenableFuture<Void> future = table.delete(operation.getItemId());

		try {
			future.get();

			return null;
		} catch (ExecutionException ex) {
			throw ex.getCause();
		}
	}

	public JsonObject getItem() {
		return this.mItem;
	}

	public void setItem(JsonObject item) {
		this.mItem = item;
	}
}