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
 * ExecutableJsonQuery.java
 */
package com.microsoft.windowsazure.mobileservices.table.query;

import java.util.Date;
import java.util.List;

import com.google.common.util.concurrent.ListenableFuture;
import com.google.gson.JsonElement;
import com.microsoft.windowsazure.mobileservices.table.MobileServiceJsonTable;
import com.microsoft.windowsazure.mobileservices.table.TableJsonQueryCallback;

import android.util.Pair;

/**
 * Class that represents a query to a specific MobileServiceJsonTable instance
 */
public final class ExecutableJsonQuery implements Query {

	/**
	 * The internal base query
	 */
	private Query mQuery;

	/**
	 * The Table to query
	 */
	private MobileServiceJsonTable mTable;

	/**
	 * Returns the MobileServiceJsonTable to query
	 */
	MobileServiceJsonTable getTable() {
		return this.mTable;
	}

	/**
	 * Sets the MobileServiceJsonTable to query
	 * 
	 * @param table
	 *            The MobileServiceJsonTable to query
	 */
	public void setTable(MobileServiceJsonTable table) {
		this.mTable = table;
	}

	/**
	 * Creates an empty Query
	 */
	public ExecutableJsonQuery() {
		this.mQuery = new QueryBase();
	}

	/**
	 * Creates Query<E> with an existing query as its only internal value
	 * 
	 * @param query
	 *            The query step to add
	 */
	public ExecutableJsonQuery(Query query) {
		if (query.getQueryNode() != null) {
			query = QueryOperations.query(query);
		}

		this.mQuery = query;
	}

	/**
	 * Executes the query
	 */
	public ListenableFuture<JsonElement> execute() {
		return this.mTable.execute(this);
	}

	/**
	 * Executes the query
	 * 
	 * @deprecated use {@link execute()} instead
	 * 
	 * @param callback
	 *            Callback to invoke when the operation is completed
	 */
	public void execute(final TableJsonQueryCallback callback) {
		mTable.execute(this, callback);
	}

	@Override
	public ExecutableJsonQuery deepClone() {
		ExecutableJsonQuery clone = new ExecutableJsonQuery();

		if (this.mQuery != null) {
			clone.mQuery = this.mQuery.deepClone();
		}

		// No need to clone table
		clone.mTable = this.mTable;

		return clone;
	}

	@Override
	public QueryNode getQueryNode() {
		return this.mQuery.getQueryNode();
	}

	@Override
	public void setQueryNode(QueryNode queryNode) {
		this.mQuery.setQueryNode(queryNode);
	}

	@Override
	public boolean hasInlineCount() {
		return this.mQuery.hasInlineCount();
	}

    @Override
    public boolean hasDeleted() {
        return this.mQuery.hasDeleted();
    }

	@Override
	public List<Pair<String, QueryOrder>> getOrderBy() {
		return this.mQuery.getOrderBy();
	}

	@Override
	public List<String> getProjection() {
		return this.mQuery.getProjection();
	}

	@Override
	public List<Pair<String, String>> getUserDefinedParameters() {
		return this.mQuery.getUserDefinedParameters();
	}

	@Override
	public int getTop() {
		return this.mQuery.getTop();
	}

	@Override
	public int getSkip() {
		return this.mQuery.getSkip();
	}

	@Override
	public String getTableName() {
		return this.mQuery.getTableName();
	}

	@Override
	public ExecutableJsonQuery tableName(String tableName) {
		this.mQuery.tableName(tableName);
		return this;
	}

	/**** Row Operations ****/

	@Override
	public ExecutableJsonQuery parameter(String parameter, String value) {
		this.mQuery.parameter(parameter, value);
		return this;
	}

	@Override
	public ExecutableJsonQuery orderBy(String field, QueryOrder order) {
		this.mQuery.orderBy(field, order);
		return this;
	}

	@Override
	public ExecutableJsonQuery top(int top) {
		this.mQuery.top(top);
		return this;
	}

	@Override
	public ExecutableJsonQuery skip(int skip) {
		this.mQuery.skip(skip);
		return this;
	}

	@Override
	public ExecutableJsonQuery includeInlineCount() {
		this.mQuery.includeInlineCount();
		return this;
	}

	@Override
	public ExecutableJsonQuery removeInlineCount() {
		this.mQuery.removeInlineCount();
		return this;
	}


    @Override
    public ExecutableJsonQuery includeDeleted() {
        this.mQuery.includeDeleted();

        return this;
    }

    @Override
    public ExecutableJsonQuery removeDeleted() {
        this.mQuery.removeDeleted();

        return this;
    }

	@Override
	public ExecutableJsonQuery select(String... fields) {
		this.mQuery.select(fields);
		return this;
	}

	/**** Query Operations ****/

	@Override
	public ExecutableJsonQuery field(String fieldName) {
		this.mQuery.field(fieldName);
		return this;
	}

	@Override
	public ExecutableJsonQuery val(Number number) {
		this.mQuery.val(number);
		return this;
	}

	@Override
	public ExecutableJsonQuery val(boolean val) {
		this.mQuery.val(val);
		return this;
	}

	@Override
	public ExecutableJsonQuery val(String s) {
		this.mQuery.val(s);
		return this;
	}

	@Override
	public ExecutableJsonQuery val(Date date) {
		this.mQuery.val(date);
		return this;
	}

	/****** Logical Operators ******/

	@Override
	public ExecutableJsonQuery and() {
		this.mQuery.and();
		return this;
	}

	@Override
	public ExecutableJsonQuery and(Query otherQuery) {
		this.mQuery.and(otherQuery);
		return this;
	}

	@Override
	public ExecutableJsonQuery or() {
		this.mQuery.or();
		return this;
	}

	@Override
	public ExecutableJsonQuery or(Query otherQuery) {
		this.mQuery.or(otherQuery);
		return this;
	}

	@Override
	public ExecutableJsonQuery not() {
		this.mQuery.not();
		return this;
	}

	@Override
	public ExecutableJsonQuery not(Query otherQuery) {
		this.mQuery.not(otherQuery);
		return this;
	}

	@Override
	public ExecutableJsonQuery not(boolean booleanValue) {
		this.mQuery.not(booleanValue);
		return this;
	}

	/****** Comparison Operators ******/

	@Override
	public ExecutableJsonQuery ge() {
		this.mQuery.ge();
		return this;
	}

	@Override
	public ExecutableJsonQuery ge(Query otherQuery) {
		this.mQuery.ge(otherQuery);
		return this;
	}

	@Override
	public ExecutableJsonQuery ge(Number numberValue) {
		this.mQuery.ge(numberValue);
		return this;
	}

	@Override
	public ExecutableJsonQuery ge(Date dateValue) {
		this.mQuery.ge(dateValue);
		return this;
	}

	@Override
	public ExecutableJsonQuery le() {
		this.mQuery.le();
		return this;
	}

	@Override
	public ExecutableJsonQuery le(Query otherQuery) {
		this.mQuery.le(otherQuery);
		return this;
	}

	@Override
	public ExecutableJsonQuery le(Number numberValue) {
		this.mQuery.le(numberValue);
		return this;
	}

	@Override
	public ExecutableJsonQuery le(Date dateValue) {
		this.mQuery.le(dateValue);
		return this;
	}

	@Override
	public ExecutableJsonQuery gt() {
		this.mQuery.gt();
		return this;
	}

	@Override
	public ExecutableJsonQuery gt(Query otherQuery) {
		this.mQuery.gt(otherQuery);
		return this;
	}

	@Override
	public ExecutableJsonQuery gt(Number numberValue) {
		this.mQuery.gt(numberValue);
		return this;
	}

	@Override
	public ExecutableJsonQuery gt(Date dateValue) {
		this.mQuery.gt(dateValue);
		return this;
	}

    @Override
    public ExecutableJsonQuery gt(String stringValue) {
        this.mQuery.gt(stringValue);
        return this;
    }

	@Override
	public ExecutableJsonQuery lt() {
		this.mQuery.lt();
		return this;
	}

	@Override
	public ExecutableJsonQuery lt(Query otherQuery) {
		this.mQuery.lt(otherQuery);
		return this;
	}

	@Override
	public ExecutableJsonQuery lt(Number numberValue) {
		this.mQuery.lt(numberValue);
		return this;
	}

	@Override
	public ExecutableJsonQuery lt(Date dateValue) {
		this.mQuery.lt(dateValue);
		return this;
	}

	@Override
	public ExecutableJsonQuery eq() {
		this.mQuery.eq();
		return this;
	}

	@Override
	public ExecutableJsonQuery eq(Query otherQuery) {
		this.mQuery.eq(otherQuery);
		return this;
	}

	@Override
	public ExecutableJsonQuery eq(Number numberValue) {
		this.mQuery.eq(numberValue);
		return this;
	}

	@Override
	public ExecutableJsonQuery eq(boolean booleanValue) {
		this.mQuery.eq(booleanValue);
		return this;
	}

	@Override
	public ExecutableJsonQuery eq(String stringValue) {
		this.mQuery.eq(stringValue);
		return this;
	}

	@Override
	public ExecutableJsonQuery eq(Date dateValue) {
		this.mQuery.eq(dateValue);
		return this;
	}

	@Override
	public ExecutableJsonQuery ne() {
		this.mQuery.ne();
		return this;
	}

	@Override
	public ExecutableJsonQuery ne(Query otherQuery) {
		this.mQuery.ne(otherQuery);
		return this;
	}

	@Override
	public ExecutableJsonQuery ne(Number numberValue) {
		this.mQuery.ne(numberValue);
		return this;
	}

	@Override
	public ExecutableJsonQuery ne(boolean booleanValue) {
		this.mQuery.ne(booleanValue);
		return this;
	}

	@Override
	public ExecutableJsonQuery ne(String stringValue) {
		this.mQuery.ne(stringValue);
		return this;
	}

	@Override
	public ExecutableJsonQuery ne(Date dateValue) {
		this.mQuery.ne(dateValue);
		return this;
	}

	/****** Arithmetic Operators ******/

	@Override
	public ExecutableJsonQuery add() {
		this.mQuery.add();
		return this;
	}

	@Override
	public ExecutableJsonQuery add(Query otherQuery) {
		this.mQuery.add(otherQuery);
		return this;
	}

	@Override
	public ExecutableJsonQuery add(Number val) {
		this.mQuery.add(val);
		return this;
	}

	@Override
	public ExecutableJsonQuery sub() {
		this.mQuery.sub();
		return this;
	}

	@Override
	public ExecutableJsonQuery sub(Query otherQuery) {
		this.mQuery.sub(otherQuery);
		return this;
	}

	@Override
	public ExecutableJsonQuery sub(Number val) {
		this.mQuery.sub(val);
		return this;
	}

	@Override
	public ExecutableJsonQuery mul() {
		this.mQuery.mul();
		return this;
	}

	@Override
	public ExecutableJsonQuery mul(Query otherQuery) {
		this.mQuery.mul(otherQuery);
		return this;
	}

	@Override
	public ExecutableJsonQuery mul(Number val) {
		this.mQuery.mul(val);
		return this;
	}

	@Override
	public ExecutableJsonQuery div() {
		this.mQuery.div();
		return this;
	}

	@Override
	public ExecutableJsonQuery div(Query otherQuery) {
		this.mQuery.div(otherQuery);
		return this;
	}

	@Override
	public ExecutableJsonQuery div(Number val) {
		this.mQuery.div(val);
		return this;
	}

	@Override
	public ExecutableJsonQuery mod() {
		this.mQuery.mod();
		return this;
	}

	@Override
	public ExecutableJsonQuery mod(Query otherQuery) {
		this.mQuery.mod(otherQuery);
		return this;
	}

	@Override
	public ExecutableJsonQuery mod(Number val) {
		this.mQuery.mod(val);
		return this;
	}

	/****** Date Operators ******/

	@Override
	public ExecutableJsonQuery year(Query otherQuery) {
		this.mQuery.year(otherQuery);
		return this;
	}

	@Override
	public ExecutableJsonQuery year(String field) {
		this.mQuery.year(field);
		return this;
	}

	@Override
	public ExecutableJsonQuery month(Query otherQuery) {
		this.mQuery.month(otherQuery);
		return this;
	}

	@Override
	public ExecutableJsonQuery month(String field) {
		this.mQuery.month(field);
		return this;
	}

	@Override
	public ExecutableJsonQuery day(Query otherQuery) {
		this.mQuery.day(otherQuery);
		return this;
	}

	@Override
	public ExecutableJsonQuery day(String field) {
		this.mQuery.day(field);
		return this;
	}

	@Override
	public ExecutableJsonQuery hour(Query otherQuery) {
		this.mQuery.hour(otherQuery);
		return this;
	}

	@Override
	public ExecutableJsonQuery hour(String field) {
		this.mQuery.hour(field);
		return this;
	}

	@Override
	public ExecutableJsonQuery minute(Query otherQuery) {
		this.mQuery.minute(otherQuery);
		return this;
	}

	@Override
	public ExecutableJsonQuery minute(String field) {
		this.mQuery.minute(field);
		return this;
	}

	@Override
	public ExecutableJsonQuery second(Query otherQuery) {
		this.mQuery.second(otherQuery);
		return this;
	}

	@Override
	public ExecutableJsonQuery second(String field) {
		this.mQuery.second(field);
		return this;
	}

	/****** Math Functions ******/

	@Override
	public ExecutableJsonQuery floor(Query otherQuery) {
		this.mQuery.floor(otherQuery);
		return this;
	}

	@Override
	public ExecutableJsonQuery ceiling(Query otherQuery) {
		this.mQuery.ceiling(otherQuery);
		return this;
	}

	@Override
	public ExecutableJsonQuery round(Query otherQuery) {
		this.mQuery.round(otherQuery);
		return this;
	}

	/****** String Operators ******/

	@Override
	public ExecutableJsonQuery toLower(Query exp) {
		this.mQuery.toLower(exp);
		return this;
	}

	@Override
	public ExecutableJsonQuery toLower(String field) {
		this.mQuery.toLower(field);
		return this;
	}

	@Override
	public ExecutableJsonQuery toUpper(Query exp) {
		this.mQuery.toUpper(exp);
		return this;
	}

	@Override
	public ExecutableJsonQuery toUpper(String field) {
		this.mQuery.toUpper(field);
		return this;
	}

	@Override
	public ExecutableJsonQuery length(Query exp) {
		this.mQuery.length(exp);
		return this;
	}

	@Override
	public ExecutableJsonQuery length(String field) {
		this.mQuery.length(field);
		return this;
	}

	@Override
	public ExecutableJsonQuery trim(Query exp) {
		this.mQuery.trim(exp);
		return this;
	}

	@Override
	public ExecutableJsonQuery trim(String field) {
		this.mQuery.trim(field);
		return this;
	}

	@Override
	public ExecutableJsonQuery startsWith(Query field, Query start) {
		this.mQuery.startsWith(field, start);
		return this;
	}

	@Override
	public ExecutableJsonQuery startsWith(String field, String start) {
		this.mQuery.startsWith(field, start);
		return this;
	}

	@Override
	public ExecutableJsonQuery endsWith(Query field, Query end) {
		this.mQuery.endsWith(field, end);
		return this;
	}

	@Override
	public ExecutableJsonQuery endsWith(String field, String end) {
		this.mQuery.endsWith(field, end);
		return this;
	}

	@Override
	public ExecutableJsonQuery subStringOf(Query str1, Query str2) {
		this.mQuery.subStringOf(str1, str2);
		return this;
	}

	@Override
	public ExecutableJsonQuery subStringOf(String str, String field) {
		this.mQuery.subStringOf(str, field);
		return this;
	}

	@Override
	public ExecutableJsonQuery concat(Query str1, Query str2) {
		this.mQuery.concat(str1, str2);
		return this;
	}

	@Override
	public ExecutableJsonQuery concat(Query str1, String str2) {
		this.mQuery.concat(str1, str2);
		return this;
	}

	@Override
	public ExecutableJsonQuery indexOf(Query haystack, Query needle) {
		this.mQuery.indexOf(haystack, needle);
		return this;
	}

	@Override
	public ExecutableJsonQuery indexOf(String field, String needle) {
		this.mQuery.indexOf(field, needle);
		return this;
	}

	@Override
	public ExecutableJsonQuery subString(Query str, Query pos) {
		this.mQuery.subString(str, pos);
		return this;
	}

	@Override
	public ExecutableJsonQuery subString(String field, int pos) {
		this.mQuery.subString(field, pos);
		return this;
	}

	@Override
	public ExecutableJsonQuery subString(Query str, Query pos, Query length) {
		this.mQuery.subString(str, pos, length);
		return this;
	}

	@Override
	public ExecutableJsonQuery subString(String field, int pos, int length) {
		this.mQuery.subString(field, pos, length);
		return this;
	}

	@Override
	public ExecutableJsonQuery replace(Query str, Query find, Query replace) {
		this.mQuery.replace(str, find, replace);
		return this;
	}

	@Override
	public ExecutableJsonQuery replace(String field, String find, String replace) {
		this.mQuery.replace(field, find, replace);
		return this;
	}
}