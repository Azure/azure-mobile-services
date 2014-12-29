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
 * ConstantNode.java
 */
package com.microsoft.windowsazure.mobileservices.table.query;

import java.util.Date;

/**
 * Class that represents a constant query node
 */
class ConstantNode implements QueryNode {
    private Object mValue;

    @Override
    public QueryNode deepClone() {
        ConstantNode clone = new ConstantNode();

        if (this.mValue instanceof Date) {
            clone.mValue = new Date(((Date) this.mValue).getTime());
        } else {
            clone.mValue = this.mValue;
        }

        return clone;
    }

    @Override
    public QueryNodeKind getKind() {
        return QueryNodeKind.Constant;
    }

    @Override
    public <T> T accept(QueryNodeVisitor<T> visitor) {
        return visitor.visit(this);
    }

    /**
     * Gets the constant node value.
     */
    Object getValue() {
        return this.mValue;
    }

    /**
     * Sets the constant node value.
     */
    void setValue(Object value) {
        this.mValue = value;
    }
}