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
package com.microsoft.windowsazure.mobileservices.table.query;

import com.microsoft.windowsazure.mobileservices.MobileServiceException;

/**
 * Class that represents a binary operator query node
 */
public class BinaryOperatorNode implements QueryNode {
	private BinaryOperatorKind mBinaryOperatorKind;

	private QueryNode mLeftArgument;
	private QueryNode mRightArgument;

	public BinaryOperatorNode(BinaryOperatorKind binaryOperatorKind) {
		this.mBinaryOperatorKind = binaryOperatorKind;
	}

	@Override
	public QueryNodeKind getKind() {
		return QueryNodeKind.BinaryOperator;
	}

	@Override
	public <T> T Accept(QueryNodeVisitor<T> visitor) throws MobileServiceException {
		return visitor.Visit(this);
	}

	public BinaryOperatorKind getBinaryOperatorKind() {
		return this.mBinaryOperatorKind;
	}

	public QueryNode getLeftArgument() {
		return this.mLeftArgument;
	}

	public void setLeftArgument(QueryNode leftArgument) {
		this.mLeftArgument = leftArgument;
	}

	public QueryNode getRightArgument() {
		return this.mRightArgument;
	}

	public void setRightArgument(QueryNode rightArgument) {
		this.mRightArgument = rightArgument;
	}
}