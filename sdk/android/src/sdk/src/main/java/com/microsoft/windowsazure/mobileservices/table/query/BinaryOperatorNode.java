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
 * BinaryOperatorNode.java
 */
package com.microsoft.windowsazure.mobileservices.table.query;

/**
 * Class that represents a binary operator query node
 */
class BinaryOperatorNode implements QueryNode {
    private BinaryOperatorKind mBinaryOperatorKind;
    private QueryNode mLeftArgument;
    private QueryNode mRightArgument;

    /**
     * Constructor for BinaryOperatorNode
     *
     * @param binaryOperatorKind The binary operator kind
     */
    BinaryOperatorNode(BinaryOperatorKind binaryOperatorKind) {
        this.mBinaryOperatorKind = binaryOperatorKind;
    }

    @Override
    public QueryNode deepClone() {
        BinaryOperatorNode clone = new BinaryOperatorNode(this.mBinaryOperatorKind);

        clone.mLeftArgument = this.mLeftArgument.deepClone();
        clone.mRightArgument = this.mRightArgument.deepClone();

        return clone;
    }

    @Override
    public QueryNodeKind getKind() {
        return QueryNodeKind.BinaryOperator;
    }

    @Override
    public <T> T accept(QueryNodeVisitor<T> visitor) {
        return visitor.visit(this);
    }

    /**
     * Gets the kind of binary operator node.
     */
    BinaryOperatorKind getBinaryOperatorKind() {
        return this.mBinaryOperatorKind;
    }

    /**
     * Gets the left argument query node of the binary operator node.
     */
    QueryNode getLeftArgument() {
        return this.mLeftArgument;
    }

    /**
     * Sets the left argument query node of the binary operator node.
     */
    void setLeftArgument(QueryNode leftArgument) {
        this.mLeftArgument = leftArgument;
    }

    /**
     * Gets the right argument query node of the binary operator node.
     */
    QueryNode getRightArgument() {
        return this.mRightArgument;
    }

    /**
     * Sets the right argument query node of the binary operator node.
     */
    void setRightArgument(QueryNode rightArgument) {
        this.mRightArgument = rightArgument;
    }
}