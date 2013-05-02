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
package com.microsoft.windowsazure.mobileservices;

import java.io.UnsupportedEncodingException;
import java.net.URLEncoder;
import java.util.ArrayList;
import java.util.Date;
import java.util.List;

import android.util.Pair;

/**
 * Class that represents a query to a table, where E is the callback class to
 * use when executing the query
 */
public final class MobileServiceQuery<E> {

	/**
	 * The Table to query
	 */
	private MobileServiceTableBase<E> mTable;

	/**
	 * The main text of the query
	 */
	private String mQueryText = null;

	/**
	 * Indicates if the query should include the inlinecount property
	 */
	private boolean mHasInlineCount = false;

	/**
	 * Query ordering to use
	 */
	private List<Pair<String, QueryOrder>> mOrderBy = new ArrayList<Pair<String, QueryOrder>>();

	/**
	 * Query projection to use
	 */
	private List<String> mProjection = null;

	/**
	 * User-defined properties to use
	 */
	private List<Pair<String, String>> mUserDefinedParameters = new ArrayList<Pair<String, String>>();

	/**
	 * Top rows to retrieve
	 */
	private int mTop = -1;

	/**
	 * Rows to skip
	 */
	private int mSkip = -1;

	/**
	 * List of values to print between parentheses
	 */
	private List<MobileServiceQuery<?>> internalValues = new ArrayList<MobileServiceQuery<?>>();

	/**
	 * Next steps in the query
	 */
	private List<MobileServiceQuery<?>> querySteps = new ArrayList<MobileServiceQuery<?>>();

	/**
	 * Returns the main text of the query
	 */
	public String getQueryText() {
		return mQueryText;
	}

	/**
	 * Sets the main text of the query
	 * 
	 * @param queryText
	 *            The text to set
	 */
	public void setQueryText(String queryText) {
		this.mQueryText = queryText;
	}

	/**
	 * Returns the MobileServiceTableBase<E> to query
	 */
	MobileServiceTableBase<E> getTable() {
		return mTable;
	}

	/**
	 * Sets the MobileServiceTableBase<E> to query
	 * 
	 * @param table
	 *            The MobileServiceTableBase<E> to query
	 */
	void setTable(MobileServiceTableBase<E> table) {
		this.mTable = table;
	}

	/**
	 * Creates an empty MobileServiceQuery
	 */
	public MobileServiceQuery() {

	}

	/**
	 * Creates MobileServiceQuery<E> with an existing query as its only internal
	 * value
	 * 
	 * @param query
	 *            The query step to add
	 */
	MobileServiceQuery(MobileServiceQuery<?> query) {
		internalValues.add(query);
	}

	/**
	 * Adds an internal value to the query
	 * 
	 * @param query
	 *            The value to add
	 */
	void addInternalValue(MobileServiceQuery<?> query) {
		internalValues.add(query);
	}

	/**
	 * Returns the string representation of the query
	 */
	@Override
	public String toString() {
		StringBuilder sb = new StringBuilder();

		if (getQueryText() != null) {
			sb.append(getQueryText());
		}

		if (internalValues.size() > 0) {
			sb.append("(");

			boolean first = true;
			for (MobileServiceQuery<?> val : internalValues) {
				if (first) {
					first = false;
				} else {
					sb.append(",");
				}

				sb.append(val.toString());
			}

			sb.append(")");
		}

		for (MobileServiceQuery<?> step : querySteps) {
			// If the string is not empty and it doesn't end with space or
			// if it ends with ")", then add a space
			if ((!sb.toString().endsWith(" ") && sb.toString().length() > 0)
					|| sb.toString().endsWith(")")) {
				sb.append(" ");
			}

			sb.append(step.toString());
		}

		return sb.toString();
	}

	/**
	 * Returns the string representation of the rowset's modifiers
	 * 
	 * @throws UnsupportedEncodingException
	 */
	public String getRowSetModifiers() throws UnsupportedEncodingException {
		StringBuilder sb = new StringBuilder();

		if (this.mHasInlineCount) {
			sb.append("&$inlinecount=allpages");
		}

		if (this.mTop > 0) {
			sb.append("&$top=");
			sb.append(this.mTop);
		}

		if (this.mSkip > 0) {
			sb.append("&$skip=");
			sb.append(this.mSkip);
		}

		if (this.mOrderBy.size() > 0) {
			sb.append("&$orderby=");

			boolean first = true;
			for (Pair<String, QueryOrder> order : this.mOrderBy) {
				if (first) {
					first = false;
				} else {
					sb.append(URLEncoder.encode(",",
							MobileServiceClient.UTF8_ENCODING));
				}

				sb.append(URLEncoder.encode(order.first,
						MobileServiceClient.UTF8_ENCODING));
				sb.append(URLEncoder.encode(" ",
						MobileServiceClient.UTF8_ENCODING));
				sb.append(order.second == QueryOrder.Ascending ? "asc" : "desc");

			}
		}

		if (!this.mUserDefinedParameters.isEmpty()) {
			for (Pair<String, String> parameter : this.mUserDefinedParameters) {
				if (parameter.first != null) {
					sb.append("&");

					String key = parameter.first;
					String value = parameter.second;
					if (value == null)
						value = "null";

					sb.append(URLEncoder.encode(key,
							MobileServiceClient.UTF8_ENCODING));
					sb.append("=");
					sb.append(URLEncoder.encode(value,
							MobileServiceClient.UTF8_ENCODING));
				}
			}
		}

		if (this.mProjection != null && this.mProjection.size() > 0) {
			sb.append("&$select=");

			boolean first = true;
			for (String field : this.mProjection) {
				if (first) {
					first = false;
				} else {
					sb.append(URLEncoder.encode(",",
							MobileServiceClient.UTF8_ENCODING));
				}

				sb.append(URLEncoder.encode(field,
						MobileServiceClient.UTF8_ENCODING));
			}
		}

		return sb.toString();
	}

	/**
	 * Executes the query
	 * 
	 * @param callback
	 *            Callback to invoke when the operation is completed
	 */
	public void execute(final E callback) {
		mTable.execute(this, callback);
	}

	/**** Row Operations ****/

	/**
	 * Adds a new user-defined parameter to the query
	 * 
	 * @param parameter
	 *            The parameter name
	 * @param value
	 *            The parameter value
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> parameter(String parameter, String value) {
		this.mUserDefinedParameters.add(new Pair<String, String>(parameter,
				value));
		return this;
	}

	/**
	 * Adds a new order by statement
	 * 
	 * @param field
	 *            FieldName
	 * @param order
	 *            Sorting order
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> orderBy(String field, QueryOrder order) {
		this.mOrderBy.add(new Pair<String, QueryOrder>(field, order));
		return this;
	}

	/**
	 * Sets the number of records to return
	 * 
	 * @param top
	 *            Number of records to return
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> top(int top) {
		if (top > 0) {
			this.mTop = top;
		}

		return this;
	}

	/**
	 * Sets the number of records to skip over a given number of elements in a
	 * sequence and then return the remainder.
	 * 
	 * @param skip
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> skip(int skip) {
		if (skip > 0) {
			this.mSkip = skip;
		}

		return this;
	}

	/**
	 * The inlinecount property specifies whether or not to retrieve a property
	 * with the number of records returned.
	 * 
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> includeInlineCount() {
		this.mHasInlineCount = true;

		return this;
	}

	/**
	 * Specifies the fields to retrieve
	 * 
	 * @param fields
	 *            Names of the fields to retrieve
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> select(String... fields) {
		this.mProjection = new ArrayList<String>();
		for (String field : fields) {
			this.mProjection.add(field);
		}

		return this;
	}

	/**** Query Operations ****/

	/**
	 * Specifies the field to use
	 * 
	 * @param fieldName
	 *            The field to use
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> field(String fieldName) {
		this.querySteps.add(MobileServiceQueryOperations.field(fieldName));
		return this;
	}

	/**
	 * Specifies a numeric value
	 * 
	 * @param number
	 *            The numeric value to use
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> val(Number number) {
		this.querySteps.add(MobileServiceQueryOperations.val(number));
		return this;
	}

	/**
	 * Specifies a boolean value
	 * 
	 * @param number
	 *            The boolean value to use
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> val(boolean val) {
		this.querySteps.add(MobileServiceQueryOperations.val(val));
		return this;
	}

	/**
	 * Specifies a string value
	 * 
	 * @param number
	 *            The string value to use
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> val(String s) {
		this.querySteps.add(MobileServiceQueryOperations.val(s));
		return this;
	}

	/**
	 * Specifies a date value
	 * 
	 * @param number
	 *            The date value to use
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> val(Date date) {
		this.querySteps.add(MobileServiceQueryOperations.val(date));
		return this;
	}

	/****** Logical Operators ******/

	/**
	 * Conditional and.
	 * 
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> and() {
		this.querySteps.add(MobileServiceQueryOperations.and());
		return this;
	}

	/**
	 * Conditional and.
	 * 
	 * @param otherQuery
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> and(MobileServiceQuery<?> otherQuery) {
		this.querySteps.add(MobileServiceQueryOperations.and(otherQuery));
		return this;
	}

	/**
	 * Conditional or.
	 * 
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> or() {
		this.querySteps.add(MobileServiceQueryOperations.or());
		return this;
	}

	/**
	 * Conditional or.
	 * 
	 * @param otherQuery
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> or(MobileServiceQuery<?> otherQuery) {
		this.querySteps.add(MobileServiceQueryOperations.or(otherQuery));
		return this;
	}

	/**
	 * Logical not.
	 * 
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> not() {
		this.querySteps.add(MobileServiceQueryOperations.not());
		return this;
	}

	/**
	 * Logical not.
	 * 
	 * @param otherQuery
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> not(MobileServiceQuery<?> otherQuery) {
		this.querySteps.add(MobileServiceQueryOperations.not(otherQuery));
		return this;
	}

	/**
	 * Logical not.
	 * 
	 * @param booleanValue
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> not(boolean booleanValue) {
		this.querySteps.add(MobileServiceQueryOperations
				.not(MobileServiceQueryOperations.val(booleanValue)));
		return this;
	}

	/****** Comparison Operators ******/

	/**
	 * Greater than or equal comparison operator.
	 * 
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> ge() {
		this.querySteps.add(MobileServiceQueryOperations.ge());
		return this;
	}

	/**
	 * Greater than or equal comparison operator.
	 * 
	 * @param otherQuery
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> ge(MobileServiceQuery<?> otherQuery) {
		this.querySteps.add(MobileServiceQueryOperations.ge(otherQuery));
		return this;
	}

	/**
	 * Greater than or equal comparison operator.
	 * 
	 * @param numberValue
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> ge(Number numberValue) {
		this.querySteps.add(MobileServiceQueryOperations
				.ge(MobileServiceQueryOperations.val(numberValue)));
		return this;
	}

	/**
	 * Greater than or equal comparison operator.
	 * 
	 * @param dateValue
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> ge(Date dateValue) {
		this.querySteps.add(MobileServiceQueryOperations
				.ge(MobileServiceQueryOperations.val(dateValue)));
		return this;
	}

	/**
	 * Less than or equal comparison operator.
	 * 
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> le() {
		this.querySteps.add(MobileServiceQueryOperations.le());
		return this;
	}

	/**
	 * Less than or equal comparison operator.
	 * 
	 * @param otherQuery
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> le(MobileServiceQuery<?> otherQuery) {
		this.querySteps.add(MobileServiceQueryOperations.le(otherQuery));
		return this;
	}

	/**
	 * Less than or equal comparison operator.
	 * 
	 * @param numberValue
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> le(Number numberValue) {
		this.querySteps.add(MobileServiceQueryOperations
				.le(MobileServiceQueryOperations.val(numberValue)));
		return this;
	}

	/**
	 * Less than or equal comparison operator.
	 * 
	 * @param dateValue
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> le(Date dateValue) {
		this.querySteps.add(MobileServiceQueryOperations
				.le(MobileServiceQueryOperations.val(dateValue)));
		return this;
	}

	/**
	 * Greater than comparison operator.
	 * 
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> gt() {
		this.querySteps.add(MobileServiceQueryOperations.gt());
		return this;
	}

	/**
	 * Greater than comparison operator.
	 * 
	 * @param otherQuery
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> gt(MobileServiceQuery<?> otherQuery) {
		this.querySteps.add(MobileServiceQueryOperations.gt(otherQuery));
		return this;
	}

	/**
	 * Greater than comparison operator.
	 * 
	 * @param numberValue
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> gt(Number numberValue) {
		this.querySteps.add(MobileServiceQueryOperations
				.gt(MobileServiceQueryOperations.val(numberValue)));
		return this;
	}

	/**
	 * Greater than comparison operator.
	 * 
	 * @param dateValue
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> gt(Date dateValue) {
		this.querySteps.add(MobileServiceQueryOperations
				.gt(MobileServiceQueryOperations.val(dateValue)));
		return this;
	}

	/**
	 * Less than comparison operator.
	 * 
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> lt() {
		this.querySteps.add(MobileServiceQueryOperations.lt());
		return this;
	}

	/**
	 * Less than comparison operator.
	 * 
	 * @param otherQuery
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> lt(MobileServiceQuery<?> otherQuery) {
		this.querySteps.add(MobileServiceQueryOperations.lt(otherQuery));
		return this;
	}

	/**
	 * Less than comparison operator.
	 * 
	 * @param numberValue
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> lt(Number numberValue) {
		this.querySteps.add(MobileServiceQueryOperations
				.lt(MobileServiceQueryOperations.val(numberValue)));
		return this;
	}

	/**
	 * Less than comparison operator.
	 * 
	 * @param dateValue
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> lt(Date dateValue) {
		this.querySteps.add(MobileServiceQueryOperations
				.lt(MobileServiceQueryOperations.val(dateValue)));
		return this;
	}

	/**
	 * Equal comparison operator.
	 * 
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> eq() {
		this.querySteps.add(MobileServiceQueryOperations.eq());
		return this;
	}

	/**
	 * Equal comparison operator.
	 * 
	 * @param otherQuery
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> eq(MobileServiceQuery<?> otherQuery) {
		this.querySteps.add(MobileServiceQueryOperations.eq(otherQuery));
		return this;
	}

	/**
	 * Equal comparison operator.
	 * 
	 * @param numberValue
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> eq(Number numberValue) {
		this.querySteps.add(MobileServiceQueryOperations
				.eq(MobileServiceQueryOperations.val(numberValue)));
		return this;
	}

	/**
	 * Equal comparison operator.
	 * 
	 * @param booleanValue
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> eq(boolean booleanValue) {
		this.querySteps.add(MobileServiceQueryOperations
				.eq(MobileServiceQueryOperations.val(booleanValue)));
		return this;
	}

	/**
	 * Equal comparison operator.
	 * 
	 * @param stringValue
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> eq(String stringValue) {
		this.querySteps.add(MobileServiceQueryOperations
				.eq(MobileServiceQueryOperations.val(stringValue)));
		return this;
	}

	/**
	 * Equal comparison operator.
	 * 
	 * @param dateValue
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> eq(Date dateValue) {
		this.querySteps.add(MobileServiceQueryOperations
				.eq(MobileServiceQueryOperations.val(dateValue)));
		return this;
	}

	/**
	 * Not equal comparison operator.
	 * 
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> ne() {
		this.querySteps.add(MobileServiceQueryOperations.ne());
		return this;
	}

	/**
	 * Not equal comparison operator.
	 * 
	 * @param otherQuery
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> ne(MobileServiceQuery<?> otherQuery) {
		this.querySteps.add(MobileServiceQueryOperations.ne(otherQuery));
		return this;
	}

	/**
	 * Not equal comparison operator.
	 * 
	 * @param numberValue
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> ne(Number numberValue) {
		this.querySteps.add(MobileServiceQueryOperations
				.ne(MobileServiceQueryOperations.val(numberValue)));
		return this;
	}

	/**
	 * Not equal comparison operator.
	 * 
	 * @param booleanValue
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> ne(boolean booleanValue) {
		this.querySteps.add(MobileServiceQueryOperations
				.ne(MobileServiceQueryOperations.val(booleanValue)));
		return this;
	}

	/**
	 * Not equal comparison operator.
	 * 
	 * @param stringValue
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> ne(String stringValue) {
		this.querySteps.add(MobileServiceQueryOperations
				.ne(MobileServiceQueryOperations.val(stringValue)));
		return this;
	}

	/**
	 * Not equal comparison operator.
	 * 
	 * @param dateValue
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> ne(Date dateValue) {
		this.querySteps.add(MobileServiceQueryOperations
				.ne(MobileServiceQueryOperations.val(dateValue)));
		return this;
	}

	/****** Arithmetic Operators ******/

	/**
	 * Add operator.
	 * 
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> add() {
		this.querySteps.add(MobileServiceQueryOperations.add());
		return this;
	}

	/**
	 * Add operator.
	 * 
	 * @param otherQuery
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> add(MobileServiceQuery<?> otherQuery) {
		this.querySteps.add(MobileServiceQueryOperations.add(otherQuery));
		return this;
	}

	/**
	 * Add operator.
	 * 
	 * @param val
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> add(Number val) {
		this.querySteps.add(MobileServiceQueryOperations.add(val));
		return this;
	}

	/**
	 * Subtract operator.
	 * 
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> sub() {
		this.querySteps.add(MobileServiceQueryOperations.sub());
		return this;
	}

	/**
	 * Subtract operator.
	 * 
	 * @param otherQuery
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> sub(MobileServiceQuery<?> otherQuery) {
		this.querySteps.add(MobileServiceQueryOperations.sub(otherQuery));
		return this;
	}

	/**
	 * Subtract operator.
	 * 
	 * @param val
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> sub(Number val) {
		this.querySteps.add(MobileServiceQueryOperations.sub(val));
		return this;
	}

	/**
	 * Multiply operator.
	 * 
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> mul() {
		this.querySteps.add(MobileServiceQueryOperations.mul());
		return this;
	}

	/**
	 * Multiply operator.
	 * 
	 * @param otherQuery
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> mul(MobileServiceQuery<?> otherQuery) {
		this.querySteps.add(MobileServiceQueryOperations.mul(otherQuery));
		return this;
	}

	/**
	 * Multiply operator.
	 * 
	 * @param otherQuery
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> mul(Number val) {
		this.querySteps.add(MobileServiceQueryOperations.mul(val));
		return this;
	}

	/**
	 * Divide operator.
	 * 
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> div() {
		this.querySteps.add(MobileServiceQueryOperations.div());
		return this;
	}

	/**
	 * Divide operator.
	 * 
	 * @param otherQuery
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> div(MobileServiceQuery<?> otherQuery) {
		this.querySteps.add(MobileServiceQueryOperations.div(otherQuery));
		return this;
	}

	/**
	 * Divide operator.
	 * 
	 * @param val
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> div(Number val) {
		this.querySteps.add(MobileServiceQueryOperations.div(val));
		return this;
	}

	/**
	 * Reminder (or modulo) operator.
	 * 
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> mod() {
		this.querySteps.add(MobileServiceQueryOperations.mod());
		return this;
	}

	/**
	 * Reminder (or modulo) operator.
	 * 
	 * @param otherQuery
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> mod(MobileServiceQuery<?> otherQuery) {
		this.querySteps.add(MobileServiceQueryOperations.mod(otherQuery));
		return this;
	}

	/**
	 * Reminder (or modulo) operator.
	 * 
	 * @param val
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> mod(Number val) {
		this.querySteps.add(MobileServiceQueryOperations.mod(val));
		return this;
	}

	/****** Date Operators ******/

	/**
	 * The year component value of the parameter value.
	 * 
	 * @param otherQuery
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> year(MobileServiceQuery<?> otherQuery) {
		this.querySteps.add(MobileServiceQueryOperations.year(otherQuery));
		return this;
	}

	/**
	 * The year component value of the parameter value.
	 * 
	 * @param field
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> year(String field) {
		this.querySteps.add(MobileServiceQueryOperations.year(field));
		return this;
	}

	/**
	 * The month component value of the parameter value.
	 * 
	 * @param otherQuery
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> month(MobileServiceQuery<?> otherQuery) {
		this.querySteps.add(MobileServiceQueryOperations.month(otherQuery));
		return this;
	}

	/**
	 * The month component value of the parameter value.
	 * 
	 * @param field
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> month(String field) {
		this.querySteps.add(MobileServiceQueryOperations.month(field));
		return this;
	}

	/**
	 * The day component value of the parameter value.
	 * 
	 * @param otherQuery
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> day(MobileServiceQuery<?> otherQuery) {
		this.querySteps.add(MobileServiceQueryOperations.day(otherQuery));
		return this;
	}

	/**
	 * The day component value of the parameter value.
	 * 
	 * @param field
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> day(String field) {
		this.querySteps.add(MobileServiceQueryOperations.day(field));
		return this;
	}

	/**
	 * The hour component value of the parameter value.
	 * 
	 * @param otherQuery
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> hour(MobileServiceQuery<?> otherQuery) {
		this.querySteps.add(MobileServiceQueryOperations.hour(otherQuery));
		return this;
	}

	/**
	 * The hour component value of the parameter value.
	 * 
	 * @param field
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> hour(String field) {
		this.querySteps.add(MobileServiceQueryOperations.hour(field));
		return this;
	}

	/**
	 * The minute component value of the parameter value.
	 * 
	 * @param otherQuery
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> minute(MobileServiceQuery<?> otherQuery) {
		this.querySteps.add(MobileServiceQueryOperations.minute(otherQuery));
		return this;
	}

	/**
	 * The minute component value of the parameter value.
	 * 
	 * @param field
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> minute(String field) {
		this.querySteps.add(MobileServiceQueryOperations.minute(field));
		return this;
	}

	/**
	 * The second component value of the parameter value.
	 * 
	 * @param otherQuery
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> second(MobileServiceQuery<?> otherQuery) {
		this.querySteps.add(MobileServiceQueryOperations.second(otherQuery));
		return this;
	}

	/**
	 * The second component value of the parameter value.
	 * 
	 * @param field
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> second(String field) {
		this.querySteps.add(MobileServiceQueryOperations.second(field));
		return this;
	}

	/****** Math Functions ******/

	/**
	 * The largest integral value less than or equal to the parameter value.
	 * 
	 * @param otherQuery
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> floor(MobileServiceQuery<?> otherQuery) {
		this.querySteps.add(MobileServiceQueryOperations.floor(otherQuery));
		return this;
	}

	/**
	 * The smallest integral value greater than or equal to the parameter value.
	 * 
	 * @param otherQuery
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> ceiling(MobileServiceQuery<?> otherQuery) {
		this.querySteps.add(MobileServiceQueryOperations.ceiling(otherQuery));
		return this;
	}

	/**
	 * The nearest integral value to the parameter value.
	 * 
	 * @param otherQuery
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> round(MobileServiceQuery<?> otherQuery) {
		this.querySteps.add(MobileServiceQueryOperations.round(otherQuery));
		return this;
	}

	/****** String Operators ******/

	/**
	 * String value with the contents of the parameter value converted to lower
	 * case.
	 * 
	 * @param exp
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> toLower(MobileServiceQuery<?> exp) {
		this.querySteps.add(MobileServiceQueryOperations.toLower(exp));
		return this;
	}

	/**
	 * String value with the contents of the parameter value converted to lower
	 * case.
	 * 
	 * @param field
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> toLower(String field) {
		this.querySteps.add(MobileServiceQueryOperations.toLower(field));
		return this;
	}

	/**
	 * String value with the contents of the parameter value converted to upper
	 * case.
	 * 
	 * @param exp
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> toUpper(MobileServiceQuery<?> exp) {
		this.querySteps.add(MobileServiceQueryOperations.toUpper(exp));
		return this;
	}

	/**
	 * String value with the contents of the parameter value converted to upper
	 * case.
	 * 
	 * @param field
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> toUpper(String field) {
		this.querySteps.add(MobileServiceQueryOperations.toUpper(field));
		return this;
	}

	/**
	 * The number of characters in the specified parameter value.
	 * 
	 * @param exp
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> length(MobileServiceQuery<?> exp) {
		this.querySteps.add(MobileServiceQueryOperations.length(exp));
		return this;
	}

	/**
	 * The number of characters in the specified parameter value.
	 * 
	 * @param field
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> length(String field) {
		this.querySteps.add(MobileServiceQueryOperations.length(field));
		return this;
	}

	/**
	 * String value with the contents of the parameter value with all leading
	 * and trailing white-space characters removed.
	 * 
	 * @param exp
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> trim(MobileServiceQuery<?> exp) {
		this.querySteps.add(MobileServiceQueryOperations.trim(exp));
		return this;
	}

	/**
	 * String value with the contents of the parameter value with all leading
	 * and trailing white-space characters removed.
	 * 
	 * @param field
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> trim(String field) {
		this.querySteps.add(MobileServiceQueryOperations.trim(field));
		return this;
	}

	/**
	 * Whether the beginning of the first parameter values matches the second
	 * parameter value.
	 * 
	 * @param field
	 *            The field to evaluate
	 * @param start
	 *            Start value
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> startsWith(MobileServiceQuery<?> field,
			MobileServiceQuery<?> start) {
		this.querySteps.add(MobileServiceQueryOperations.startsWith(field,
				start));
		return this;
	}

	/**
	 * Whether the beginning of the first parameter values matches the second
	 * parameter value.
	 * 
	 * @param field
	 *            The field to evaluate
	 * @param start
	 *            Start value
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> startsWith(String field, String start) {
		this.querySteps.add(MobileServiceQueryOperations.startsWith(field,
				start));
		return this;
	}

	/**
	 * Whether the end of the first parameter value matches the second parameter
	 * value.
	 * 
	 * @param field
	 *            The field to evaluate
	 * @param end
	 *            End value
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> endsWith(MobileServiceQuery<?> field,
			MobileServiceQuery<?> end) {
		this.querySteps.add(MobileServiceQueryOperations.endsWith(field, end));
		return this;
	}

	/**
	 * Whether the end of the first parameter value matches the second parameter
	 * value.
	 * 
	 * @param field
	 *            The field to evaluate
	 * @param end
	 *            End value
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> endsWith(String field, String end) {
		this.querySteps.add(MobileServiceQueryOperations.endsWith(field, end));
		return this;
	}

	/**
	 * Whether the first parameter string value occurs in the second parameter
	 * string value.
	 * 
	 * @param str1
	 *            First string
	 * @param str2
	 *            Second string
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> subStringOf(MobileServiceQuery<?> str1,
			MobileServiceQuery<?> str2) {
		this.querySteps.add(MobileServiceQueryOperations
				.subStringOf(str1, str2));
		return this;
	}

	/**
	 * Whether the string parameter occurs in the field
	 * 
	 * @param str2
	 *            String to search
	 * @param field
	 *            Field to search in
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> subStringOf(String str, String field) {
		this.querySteps.add(MobileServiceQueryOperations
				.subStringOf(str, field));
		return this;
	}

	/**
	 * String value which is the first and second parameter values merged
	 * together with the first parameter value coming first in the result.
	 * 
	 * @param str1
	 *            First string
	 * @param str2
	 *            Second string
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> concat(MobileServiceQuery<?> str1,
			MobileServiceQuery<?> str2) {
		this.querySteps.add(MobileServiceQueryOperations.concat(str1, str2));
		return this;
	}

	/**
	 * Index of the first occurrence of the second parameter value in the first
	 * parameter value or -1 otherwise.
	 * 
	 * @param haystack
	 *            String content
	 * @param needle
	 *            Value to search for
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> indexOf(MobileServiceQuery<?> haystack,
			MobileServiceQuery<?> needle) {
		this.querySteps.add(MobileServiceQueryOperations.indexOf(haystack,
				needle));
		return this;
	}

	/**
	 * Index of the first occurrence of the second parameter value in the first
	 * parameter value or -1 otherwise.
	 * 
	 * @param field
	 *            Field to search in
	 * @param str
	 *            Value to search for
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> indexOf(String field, String needle) {
		this.querySteps
				.add(MobileServiceQueryOperations.indexOf(field, needle));
		return this;
	}

	/**
	 * String value starting at the character index specified by the second
	 * parameter value in the first parameter string value.
	 * 
	 * @param str
	 *            String content
	 * @param pos
	 *            Starting position
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> subString(MobileServiceQuery<?> str,
			MobileServiceQuery<?> pos) {
		this.querySteps.add(MobileServiceQueryOperations.subString(str, pos));
		return this;
	}

	/**
	 * String value starting at the character index specified by the second
	 * parameter value in the first parameter string value.
	 * 
	 * @param field
	 *            Field to scan
	 * @param pos
	 *            Starting position
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> subString(String field, int pos) {
		this.querySteps.add(MobileServiceQueryOperations.subString(field, pos));
		return this;
	}

	/**
	 * String value starting at the character index specified by the second
	 * parameter value in the first parameter string value.
	 * 
	 * @param str
	 *            String content
	 * @param pos
	 *            Starting position
	 * @param length
	 *            Length
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> subString(MobileServiceQuery<?> str,
			MobileServiceQuery<?> pos, MobileServiceQuery<?> length) {
		this.querySteps.add(MobileServiceQueryOperations.subString(str, pos,
				length));
		return this;
	}

	/**
	 * String value starting at the character index specified by the second
	 * parameter value in the first parameter string value.
	 * 
	 * @param field
	 *            Field to scan
	 * @param pos
	 *            Starting position
	 * @param length
	 *            Length
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> subString(String field, int pos, int length) {
		this.querySteps.add(MobileServiceQueryOperations.subString(field, pos,
				length));
		return this;
	}

	/**
	 * Finds the second string parameter in the first parameter string value and
	 * replaces it with the third parameter value.
	 * 
	 * @param str
	 *            String content
	 * @param find
	 *            Search value
	 * @param replace
	 *            Replace value
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> replace(MobileServiceQuery<?> str,
			MobileServiceQuery<?> find, MobileServiceQuery<?> replace) {
		this.querySteps.add(MobileServiceQueryOperations.replace(str, find,
				replace));
		return this;
	}

	/**
	 * Finds the second string parameter in the first parameter string value and
	 * replaces it with the third parameter value.
	 * 
	 * @param field
	 *            Field to scan
	 * @param find
	 *            Search value
	 * @param replace
	 *            Replace value
	 * @return MobileServiceQuery<E>
	 */
	public MobileServiceQuery<E> replace(String field, String find,
			String replace) {
		this.querySteps.add(MobileServiceQueryOperations.replace(field, find,
				replace));
		return this;
	}
}
