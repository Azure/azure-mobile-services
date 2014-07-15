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
 * FunctionCallNode.java
 */
package com.microsoft.windowsazure.mobileservices.table.query;

import java.util.ArrayList;
import java.util.List;

/**
 * Class that represents a function call query node
 */
class FunctionCallNode implements QueryNode {
	private FunctionCallKind mFunctionCallKind;

	private List<QueryNode> mArguments;

	/**
	 * Constructor for FunctionCallNode
	 * 
	 * @param functionCallKind
	 *            The kind of function call
	 */
	FunctionCallNode(FunctionCallKind functionCallKind) {
		this.mFunctionCallKind = functionCallKind;
		this.mArguments = new ArrayList<QueryNode>();
	}

	@Override
	public QueryNode deepClone() {
		FunctionCallNode clone = new FunctionCallNode(this.mFunctionCallKind);

		if (this.mArguments != null) {
			clone.mArguments = new ArrayList<QueryNode>();

			for (QueryNode queryNode : this.mArguments) {
				clone.mArguments.add(queryNode.deepClone());
			}
		}

		return clone;
	}

	@Override
	public QueryNodeKind getKind() {
		return QueryNodeKind.FunctionCall;
	}

	@Override
	public <T> T accept(QueryNodeVisitor<T> visitor) {
		return visitor.visit(this);
	}

	/**
	 * Gets the function call kind
	 */
	public FunctionCallKind getFunctionCallKind() {
		return this.mFunctionCallKind;
	}

	/**
	 * Gets the list of arguments
	 */
	public List<QueryNode> getArguments() {
		return this.mArguments;
	}

	/**
	 * Adds and argument to the exiting list
	 * 
	 * @param argument
	 *            The argument to add
	 */
	public void addArgument(QueryNode argument) {
		this.mArguments.add(argument);
	}
}