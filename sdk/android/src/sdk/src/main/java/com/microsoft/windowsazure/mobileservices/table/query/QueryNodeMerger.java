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
 * QueryNodeMerger.java
 */
package com.microsoft.windowsazure.mobileservices.table.query;

/**
 * Class that represents a query node merger
 */
class QueryNodeMerger implements QueryNodeVisitor<QueryNode> {
    private QueryNode mRightNode;

    /**
     * Constructor for QueryNodeMerger
     *
     * @param rightNode The right query node
     */
    QueryNodeMerger(QueryNode rightNode) {
        this.mRightNode = rightNode;
    }

    /**
     * Gets a QueryException that represents an invalid sequence of query
     * operations
     */
    static QueryException getInvalidSequenceException() {
        return new QueryException("Invalid query operations sequence.");
    }

    @Override
    public QueryNode visit(ConstantNode leftNode) {
        return mRightNode.accept(new ConstantNodeMerger(leftNode));
    }

    @Override
    public QueryNode visit(FieldNode leftNode) {
        return mRightNode.accept(new FieldNodeMerger(leftNode));
    }

    @Override
    public QueryNode visit(UnaryOperatorNode leftNode) {
        return mRightNode.accept(new UnaryOperatorNodeMerger(leftNode));
    }

    @Override
    public QueryNode visit(BinaryOperatorNode leftNode) {
        return mRightNode.accept(new BinaryOperatorNodeMerger(leftNode));
    }

    @Override
    public QueryNode visit(FunctionCallNode leftNode) {
        return mRightNode.accept(new FunctionCallNodeMerger(leftNode));
    }
}