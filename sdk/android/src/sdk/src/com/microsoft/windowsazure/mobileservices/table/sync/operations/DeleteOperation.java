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

/**
 * Class representing an insert operation against remote table.
 */
public class DeleteOperation implements TableOperation {

	private String mId;

	private String mTableName;

	private String mItemId;

	private Date mCreatedAt;

	public DeleteOperation(String tableName, String itemId) {
		this.mId = UUID.randomUUID().toString();
		this.mTableName = tableName;
		this.mItemId = itemId;
		this.mCreatedAt = new Date();
	}

	@Override
	public String getId() {
		return this.mId;
	}

	@Override
	public TableOperationKind getKind() {
		return TableOperationKind.Delete;
	}

	@Override
	public String getTableName() {
		return this.mTableName;
	}

	@Override
	public String getItemId() {
		return this.mItemId;
	}

	@Override
	public Date getCreatedAt() {
		return this.mCreatedAt;
	}

	@Override
	public <T> T Accept(TableOperationVisitor<T> visitor) throws Throwable {
		return visitor.Visit(this);
	}

	public static DeleteOperation parse(String id, String tableName, String itemId, Date createdAt) {
		DeleteOperation operation = new DeleteOperation(tableName, itemId);
		operation.mId = id;
		operation.mCreatedAt = createdAt;
		return operation;
	}
}