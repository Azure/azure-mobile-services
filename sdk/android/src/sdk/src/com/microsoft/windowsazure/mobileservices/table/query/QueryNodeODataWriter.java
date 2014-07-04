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

import java.util.Date;
import java.util.Locale;

import com.microsoft.windowsazure.mobileservices.table.serialization.DateSerializer;

/**
 * Query node visitor used to generate OData filter.
 */
public class QueryNodeODataWriter implements QueryNodeVisitor<QueryNode> {

	StringBuilder mBuilder;

	public QueryNodeODataWriter() {
		this.mBuilder = new StringBuilder();
	}

	public StringBuilder getBuilder() {
		return this.mBuilder;
	}

	@Override
	public QueryNode visit(ConstantNode node) {
		Object value = node.getValue();
		String constant = value != null ? value.toString() : "null";

		if (value instanceof String) {
			constant = process((String) value);
		} else if (value instanceof Date) {
			constant = process((Date) value);
		}

		this.mBuilder.append(constant);

		return node;
	}

	@Override
	public QueryNode visit(FieldNode node) {
		this.mBuilder.append(node.getFieldName());

		return node;
	}

	@Override
	public QueryNode visit(UnaryOperatorNode node) {
		if (node.getUnaryOperatorKind() == UnaryOperatorKind.Parenthesis) {
			this.mBuilder.append("(");

			if (node.getArgument() != null) {
				node.getArgument().accept(this);
			}

			this.mBuilder.append(")");
		} else {
			this.mBuilder.append(node.getUnaryOperatorKind().name().toLowerCase(Locale.getDefault()));

			if (node.getArgument() != null) {
				this.mBuilder.append(" ");
				node.getArgument().accept(this);
			}
		}

		return node;
	}

	@Override
	public QueryNode visit(BinaryOperatorNode node) {
		if (node.getLeftArgument() != null) {
			node.getLeftArgument().accept(this);
			this.mBuilder.append(" ");
		}

		this.mBuilder.append(node.getBinaryOperatorKind().name().toLowerCase(Locale.getDefault()));

		if (node.getRightArgument() != null) {
			this.mBuilder.append(" ");
			node.getRightArgument().accept(this);
		}

		return node;
	}

	@Override
	public QueryNode visit(FunctionCallNode node) {
		this.mBuilder.append(node.getFunctionCallKind().name().toLowerCase(Locale.getDefault()));
		this.mBuilder.append("(");

		boolean first = true;

		for (QueryNode argument : node.getArguments()) {
			if (!first) {
				this.mBuilder.append(",");
			} else {
				first = false;
			}

			argument.accept(this);
		}

		this.mBuilder.append(")");

		return node;
	}

	private static String process(String s) {
		return "'" + sanitize(s) + "'";
	}

	private static String process(Date date) {
		return "'" + sanitize(DateSerializer.serialize(date)) + "'";
	}

	/**
	 * Sanitizes the string to use in a oData query
	 * 
	 * @param s
	 *            The string to sanitize
	 * @return The sanitized string
	 */
	private static String sanitize(String s) {
		if (s != null) {
			return s.replace("'", "''");
		} else {
			return null;
		}
	}
}