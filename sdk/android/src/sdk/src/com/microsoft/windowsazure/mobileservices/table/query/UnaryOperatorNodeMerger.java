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
 * Interface of a query node visitor used to extend functionality.
 */
public class UnaryOperatorNodeMerger implements QueryNodeVisitor<QueryNode> {

	UnaryOperatorNode mLeftNode;

	public UnaryOperatorNodeMerger(UnaryOperatorNode leftNode) {
		this.mLeftNode = leftNode;
	}

	@Override
	public QueryNode visit(ConstantNode rightNode) throws MobileServiceException {
		return mergeLeft(rightNode);
	}

	@Override
	public QueryNode visit(FieldNode rightNode) throws MobileServiceException {
		return mergeLeft(rightNode);
	}

	@Override
	public QueryNode visit(UnaryOperatorNode rightNode) throws MobileServiceException {
		return mergeLeft(rightNode);
	}

	@Override
	public QueryNode visit(BinaryOperatorNode rightNode) throws MobileServiceException {
		if (this.mLeftNode.getArgument() != null) {
			if (rightNode.getLeftArgument() != null) {
				throw QueryNodeMerger.getInvalidSequenceException();
			}

			rightNode.setLeftArgument(this.mLeftNode);

			return rightNode;
		} else {
			this.mLeftNode.setArgument(rightNode);

			return this.mLeftNode;
		}
	}

	@Override
	public QueryNode visit(FunctionCallNode rightNode) throws MobileServiceException {
		return mergeLeft(rightNode);
	}

	private QueryNode mergeLeft(QueryNode rightNode) throws MobileServiceException {
		if (this.mLeftNode.getArgument() != null) {
			throw QueryNodeMerger.getInvalidSequenceException();
		}

		this.mLeftNode.setArgument(rightNode);

		return this.mLeftNode;
	}
}