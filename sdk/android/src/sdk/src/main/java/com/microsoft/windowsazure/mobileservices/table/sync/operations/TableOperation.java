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
 * TableOperation.java
 */
package com.microsoft.windowsazure.mobileservices.table.sync.operations;

import java.util.Date;

/**
 * Interface representing a table operation against remote table.
 */
public interface TableOperation {
    /**
     * Gets the unique id of the operation.
     *
     * @return The unique id.
     */
    String getId();

    /**
     * Gets the kind of table operation.
     *
     * @return The table operation kind.
     */
    TableOperationKind getKind();

    /**
     * Gets the name of the table the operation will be executed against.
     *
     * @return The table name.
     */
    String getTableName();

    /**
     * Gets the id of the item associated with the operation.
     *
     * @return The item id.
     */
    String getItemId();

    /**
     * Gets the creation date of the operation.
     *
     * @return The operation creation date.
     */
    Date getCreatedAt();

    /**
     * Accept a MobileServiceTableOperationVisitor that works against the
     * operation.
     *
     * @param visitor An implementation of the visitor interface.
     * @return An object whose type is determined by the type parameter of the
     * visitor.
     * @throws Throwable
     */
    <T> T accept(TableOperationVisitor<T> visitor) throws Throwable;
}