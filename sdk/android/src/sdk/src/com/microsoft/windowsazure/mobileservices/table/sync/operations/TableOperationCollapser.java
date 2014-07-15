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
 * TableOperationCollapser.java
 */
package com.microsoft.windowsazure.mobileservices.table.sync.operations;

/**
 * Class that encapsulates collapse logic for new table operation.
 */
public class TableOperationCollapser implements TableOperationVisitor<TableOperation> {
	private TableOperation mNewOperation;

	/**
	 * Constructor for TableOperationCollapser
	 * 
	 * @param newOperation
	 *            the new operation to collapse
	 */
	public TableOperationCollapser(TableOperation newOperation) {
		this.mNewOperation = newOperation;
	}

	@Override
	public TableOperation visit(InsertOperation existingOperation) throws Throwable {
		return mNewOperation.accept(new InsertOperationCollapser(existingOperation));
	}

	@Override
	public TableOperation visit(UpdateOperation existingOperation) throws Throwable {
		return mNewOperation.accept(new UpdateOperationCollapser(existingOperation));
	}

	@Override
	public TableOperation visit(DeleteOperation existingOperation) throws Throwable {
		return mNewOperation.accept(new DeleteOperationCollapser(existingOperation));
	}
}