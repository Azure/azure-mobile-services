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
import java.util.List;

import com.google.common.util.concurrent.ListenableFuture;
import com.microsoft.windowsazure.mobileservices.MobileServiceException;
import com.microsoft.windowsazure.mobileservices.table.MobileServiceTableBase;

import android.util.Pair;

/**
 * Class that represents a query to a table, where E is the callback class to
 * use when executing the query
 */
public final class ExecutableQuery<E> implements Query {

	/**
	 * The Table to query
	 */
	private Query mQuery;

	/**
	 * The Table to query
	 */
	private MobileServiceTableBase<E> mTable;

	/**
	 * Returns the MobileServiceTableBase<E> to query
	 */
	MobileServiceTableBase<E> getTable() {
		return this.mTable;
	}

	/**
	 * Sets the MobileServiceTableBase<E> to query
	 * 
	 * @param table
	 *            The MobileServiceTableBase<E> to query
	 */
	public void setTable(MobileServiceTableBase<E> table) {
		this.mTable = table;
	}

	/**
	 * Creates an empty Query
	 */
	public ExecutableQuery() {
		this.mQuery = new QueryBase();
	}

	/**
	 * Creates Query<E> with an existing query as its only internal value
	 * 
	 * @param query
	 *            The query step to add
	 */
	public ExecutableQuery(Query query) {
		if (query.getQueryNode() != null) {
			query = QueryOperations.query(query);
		}

		this.mQuery = query;
	}

	/**
	 * Executes the query
	 * 
	 * @throws MobileServiceException
	 */
	public ListenableFuture<E> execute() throws MobileServiceException {
		return this.mTable.execute(this);
	}

	@Override
	public ExecutableQuery<E> deepClone() {
		ExecutableQuery<E> clone = new ExecutableQuery<E>();

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
	public ExecutableQuery<E> tableName(String tableName) {
		this.mQuery.tableName(tableName);
		return this;
	}

	/**** Row Operations ****/

	@Override
	public ExecutableQuery<E> parameter(String parameter, String value) {
		this.mQuery.parameter(parameter, value);
		return this;
	}

	@Override
	public ExecutableQuery<E> orderBy(String field, QueryOrder order) {
		this.mQuery.orderBy(field, order);
		return this;
	}

	@Override
	public ExecutableQuery<E> top(int top) {
		this.mQuery.top(top);
		return this;
	}

	@Override
	public ExecutableQuery<E> skip(int skip) {
		this.mQuery.skip(skip);
		return this;
	}

	@Override
	public ExecutableQuery<E> includeInlineCount() {
		this.mQuery.includeInlineCount();
		return this;
	}

	@Override
	public ExecutableQuery<E> removeInlineCount() {
		this.mQuery.removeInlineCount();
		return this;
	}

	@Override
	public ExecutableQuery<E> select(String... fields) {
		this.mQuery.select(fields);
		return this;
	}

	/**** Query Operations ****/

	@Override
	public ExecutableQuery<E> field(String fieldName) throws MobileServiceException {
		this.mQuery.field(fieldName);
		return this;
	}

	@Override
	public ExecutableQuery<E> val(Number number) throws MobileServiceException {
		this.mQuery.val(number);
		return this;
	}

	@Override
	public ExecutableQuery<E> val(boolean val) throws MobileServiceException {
		this.mQuery.val(val);
		return this;
	}

	@Override
	public ExecutableQuery<E> val(String s) throws MobileServiceException {
		this.mQuery.val(s);
		return this;
	}

	@Override
	public ExecutableQuery<E> val(Date date) throws MobileServiceException {
		this.mQuery.val(date);
		return this;
	}

	/****** Logical Operators ******/

	@Override
	public ExecutableQuery<E> and() throws MobileServiceException {
		this.mQuery.and();
		return this;
	}

	@Override
	public ExecutableQuery<E> and(Query otherQuery) throws MobileServiceException {
		this.mQuery.and(otherQuery);
		return this;
	}

	@Override
	public ExecutableQuery<E> or() throws MobileServiceException {
		this.mQuery.or();
		return this;
	}

	@Override
	public ExecutableQuery<E> or(Query otherQuery) throws MobileServiceException {
		this.mQuery.or(otherQuery);
		return this;
	}

	@Override
	public ExecutableQuery<E> not() throws MobileServiceException {
		this.mQuery.not();
		return this;
	}

	@Override
	public ExecutableQuery<E> not(Query otherQuery) throws MobileServiceException {
		this.mQuery.not(otherQuery);
		return this;
	}

	@Override
	public ExecutableQuery<E> not(boolean booleanValue) throws MobileServiceException {
		this.mQuery.not(booleanValue);
		return this;
	}

	/****** Comparison Operators ******/

	@Override
	public ExecutableQuery<E> ge() throws MobileServiceException {
		this.mQuery.ge();
		return this;
	}

	@Override
	public ExecutableQuery<E> ge(Query otherQuery) throws MobileServiceException {
		this.mQuery.ge(otherQuery);
		return this;
	}

	@Override
	public ExecutableQuery<E> ge(Number numberValue) throws MobileServiceException {
		this.mQuery.ge(numberValue);
		return this;
	}

	@Override
	public ExecutableQuery<E> ge(Date dateValue) throws MobileServiceException {
		this.mQuery.ge(dateValue);
		return this;
	}

	@Override
	public ExecutableQuery<E> le() throws MobileServiceException {
		this.mQuery.le();
		return this;
	}

	@Override
	public ExecutableQuery<E> le(Query otherQuery) throws MobileServiceException {
		this.mQuery.le(otherQuery);
		return this;
	}

	@Override
	public ExecutableQuery<E> le(Number numberValue) throws MobileServiceException {
		this.mQuery.le(numberValue);
		return this;
	}

	@Override
	public ExecutableQuery<E> le(Date dateValue) throws MobileServiceException {
		this.mQuery.le(dateValue);
		return this;
	}

	@Override
	public ExecutableQuery<E> gt() throws MobileServiceException {
		this.mQuery.gt();
		return this;
	}

	@Override
	public ExecutableQuery<E> gt(Query otherQuery) throws MobileServiceException {
		this.mQuery.gt(otherQuery);
		return this;
	}

	@Override
	public ExecutableQuery<E> gt(Number numberValue) throws MobileServiceException {
		this.mQuery.gt(numberValue);
		return this;
	}

	@Override
	public ExecutableQuery<E> gt(Date dateValue) throws MobileServiceException {
		this.mQuery.gt(dateValue);
		return this;
	}

	@Override
	public ExecutableQuery<E> lt() throws MobileServiceException {
		this.mQuery.lt();
		return this;
	}

	@Override
	public ExecutableQuery<E> lt(Query otherQuery) throws MobileServiceException {
		this.mQuery.lt(otherQuery);
		return this;
	}

	@Override
	public ExecutableQuery<E> lt(Number numberValue) throws MobileServiceException {
		this.mQuery.lt(numberValue);
		return this;
	}

	@Override
	public ExecutableQuery<E> lt(Date dateValue) throws MobileServiceException {
		this.mQuery.lt(dateValue);
		return this;
	}

	@Override
	public ExecutableQuery<E> eq() throws MobileServiceException {
		this.mQuery.eq();
		return this;
	}

	@Override
	public ExecutableQuery<E> eq(Query otherQuery) throws MobileServiceException {
		this.mQuery.eq(otherQuery);
		return this;
	}

	@Override
	public ExecutableQuery<E> eq(Number numberValue) throws MobileServiceException {
		this.mQuery.eq(numberValue);
		return this;
	}

	@Override
	public ExecutableQuery<E> eq(boolean booleanValue) throws MobileServiceException {
		this.mQuery.eq(booleanValue);
		return this;
	}

	@Override
	public ExecutableQuery<E> eq(String stringValue) throws MobileServiceException {
		this.mQuery.eq(stringValue);
		return this;
	}

	@Override
	public ExecutableQuery<E> eq(Date dateValue) throws MobileServiceException {
		this.mQuery.eq(dateValue);
		return this;
	}

	@Override
	public ExecutableQuery<E> ne() throws MobileServiceException {
		this.mQuery.ne();
		return this;
	}

	@Override
	public ExecutableQuery<E> ne(Query otherQuery) throws MobileServiceException {
		this.mQuery.ne(otherQuery);
		return this;
	}

	@Override
	public ExecutableQuery<E> ne(Number numberValue) throws MobileServiceException {
		this.mQuery.ne(numberValue);
		return this;
	}

	@Override
	public ExecutableQuery<E> ne(boolean booleanValue) throws MobileServiceException {
		this.mQuery.ne(booleanValue);
		return this;
	}

	@Override
	public ExecutableQuery<E> ne(String stringValue) throws MobileServiceException {
		this.mQuery.ne(stringValue);
		return this;
	}

	@Override
	public ExecutableQuery<E> ne(Date dateValue) throws MobileServiceException {
		this.mQuery.ne(dateValue);
		return this;
	}

	/****** Arithmetic Operators ******/

	@Override
	public ExecutableQuery<E> add() throws MobileServiceException {
		this.mQuery.add();
		return this;
	}

	@Override
	public ExecutableQuery<E> add(Query otherQuery) throws MobileServiceException {
		this.mQuery.add(otherQuery);
		return this;
	}

	@Override
	public ExecutableQuery<E> add(Number val) throws MobileServiceException {
		this.mQuery.add(val);
		return this;
	}

	@Override
	public ExecutableQuery<E> sub() throws MobileServiceException {
		this.mQuery.sub();
		return this;
	}

	@Override
	public ExecutableQuery<E> sub(Query otherQuery) throws MobileServiceException {
		this.mQuery.sub(otherQuery);
		return this;
	}

	@Override
	public ExecutableQuery<E> sub(Number val) throws MobileServiceException {
		this.mQuery.sub(val);
		return this;
	}

	@Override
	public ExecutableQuery<E> mul() throws MobileServiceException {
		this.mQuery.mul();
		return this;
	}

	@Override
	public ExecutableQuery<E> mul(Query otherQuery) throws MobileServiceException {
		this.mQuery.mul(otherQuery);
		return this;
	}

	@Override
	public ExecutableQuery<E> mul(Number val) throws MobileServiceException {
		this.mQuery.mul(val);
		return this;
	}

	@Override
	public ExecutableQuery<E> div() throws MobileServiceException {
		this.mQuery.div();
		return this;
	}

	@Override
	public ExecutableQuery<E> div(Query otherQuery) throws MobileServiceException {
		this.mQuery.div(otherQuery);
		return this;
	}

	@Override
	public ExecutableQuery<E> div(Number val) throws MobileServiceException {
		this.mQuery.div(val);
		return this;
	}

	@Override
	public ExecutableQuery<E> mod() throws MobileServiceException {
		this.mQuery.mod();
		return this;
	}

	@Override
	public ExecutableQuery<E> mod(Query otherQuery) throws MobileServiceException {
		this.mQuery.mod(otherQuery);
		return this;
	}

	@Override
	public ExecutableQuery<E> mod(Number val) throws MobileServiceException {
		this.mQuery.mod(val);
		return this;
	}

	/****** Date Operators ******/

	@Override
	public ExecutableQuery<E> year(Query otherQuery) throws MobileServiceException {
		this.mQuery.year(otherQuery);
		return this;
	}

	@Override
	public ExecutableQuery<E> year(String field) throws MobileServiceException {
		this.mQuery.year(field);
		return this;
	}

	@Override
	public ExecutableQuery<E> month(Query otherQuery) throws MobileServiceException {
		this.mQuery.month(otherQuery);
		return this;
	}

	@Override
	public ExecutableQuery<E> month(String field) throws MobileServiceException {
		this.mQuery.month(field);
		return this;
	}

	@Override
	public ExecutableQuery<E> day(Query otherQuery) throws MobileServiceException {
		this.mQuery.day(otherQuery);
		return this;
	}

	@Override
	public ExecutableQuery<E> day(String field) throws MobileServiceException {
		this.mQuery.day(field);
		return this;
	}

	@Override
	public ExecutableQuery<E> hour(Query otherQuery) throws MobileServiceException {
		this.mQuery.hour(otherQuery);
		return this;
	}

	@Override
	public ExecutableQuery<E> hour(String field) throws MobileServiceException {
		this.mQuery.hour(field);
		return this;
	}

	@Override
	public ExecutableQuery<E> minute(Query otherQuery) throws MobileServiceException {
		this.mQuery.minute(otherQuery);
		return this;
	}

	@Override
	public ExecutableQuery<E> minute(String field) throws MobileServiceException {
		this.mQuery.minute(field);
		return this;
	}

	@Override
	public ExecutableQuery<E> second(Query otherQuery) throws MobileServiceException {
		this.mQuery.second(otherQuery);
		return this;
	}

	@Override
	public ExecutableQuery<E> second(String field) throws MobileServiceException {
		this.mQuery.second(field);
		return this;
	}

	/****** Math Functions ******/

	@Override
	public ExecutableQuery<E> floor(Query otherQuery) throws MobileServiceException {
		this.mQuery.floor(otherQuery);
		return this;
	}

	@Override
	public ExecutableQuery<E> ceiling(Query otherQuery) throws MobileServiceException {
		this.mQuery.ceiling(otherQuery);
		return this;
	}

	@Override
	public ExecutableQuery<E> round(Query otherQuery) throws MobileServiceException {
		this.mQuery.round(otherQuery);
		return this;
	}

	/****** String Operators ******/

	@Override
	public ExecutableQuery<E> toLower(Query exp) throws MobileServiceException {
		this.mQuery.toLower(exp);
		return this;
	}

	@Override
	public ExecutableQuery<E> toLower(String field) throws MobileServiceException {
		this.mQuery.toLower(field);
		return this;
	}

	@Override
	public ExecutableQuery<E> toUpper(Query exp) throws MobileServiceException {
		this.mQuery.toUpper(exp);
		return this;
	}

	@Override
	public ExecutableQuery<E> toUpper(String field) throws MobileServiceException {
		this.mQuery.toUpper(field);
		return this;
	}

	@Override
	public ExecutableQuery<E> length(Query exp) throws MobileServiceException {
		this.mQuery.length(exp);
		return this;
	}

	@Override
	public ExecutableQuery<E> length(String field) throws MobileServiceException {
		this.mQuery.length(field);
		return this;
	}

	@Override
	public ExecutableQuery<E> trim(Query exp) throws MobileServiceException {
		this.mQuery.trim(exp);
		return this;
	}

	@Override
	public ExecutableQuery<E> trim(String field) throws MobileServiceException {
		this.mQuery.trim(field);
		return this;
	}

	@Override
	public ExecutableQuery<E> startsWith(Query field, Query start) throws MobileServiceException {
		this.mQuery.startsWith(field, start);
		return this;
	}

	@Override
	public ExecutableQuery<E> startsWith(String field, String start) throws MobileServiceException {
		this.mQuery.startsWith(field, start);
		return this;
	}

	@Override
	public ExecutableQuery<E> endsWith(Query field, Query end) throws MobileServiceException {
		this.mQuery.endsWith(field, end);
		return this;
	}

	@Override
	public ExecutableQuery<E> endsWith(String field, String end) throws MobileServiceException {
		this.mQuery.endsWith(field, end);
		return this;
	}

	@Override
	public ExecutableQuery<E> subStringOf(Query str1, Query str2) throws MobileServiceException {
		this.mQuery.subStringOf(str1, str2);
		return this;
	}

	@Override
	public ExecutableQuery<E> subStringOf(String str, String field) throws MobileServiceException {
		this.mQuery.subStringOf(str, field);
		return this;
	}

	@Override
	public ExecutableQuery<E> concat(Query str1, Query str2) throws MobileServiceException {
		this.mQuery.concat(str1, str2);
		return this;
	}

	@Override
	public ExecutableQuery<E> indexOf(Query haystack, Query needle) throws MobileServiceException {
		this.mQuery.indexOf(haystack, needle);
		return this;
	}

	@Override
	public ExecutableQuery<E> indexOf(String field, String needle) throws MobileServiceException {
		this.mQuery.indexOf(field, needle);
		return this;
	}

	@Override
	public ExecutableQuery<E> subString(Query str, Query pos) throws MobileServiceException {
		this.mQuery.subString(str, pos);
		return this;
	}

	@Override
	public ExecutableQuery<E> subString(String field, int pos) throws MobileServiceException {
		this.mQuery.subString(field, pos);
		return this;
	}

	@Override
	public ExecutableQuery<E> subString(Query str, Query pos, Query length) throws MobileServiceException {
		this.mQuery.subString(str, pos, length);
		return this;
	}

	@Override
	public ExecutableQuery<E> subString(String field, int pos, int length) throws MobileServiceException {
		this.mQuery.subString(field, pos, length);
		return this;
	}

	@Override
	public ExecutableQuery<E> replace(Query str, Query find, Query replace) throws MobileServiceException {
		this.mQuery.replace(str, find, replace);
		return this;
	}

	@Override
	public ExecutableQuery<E> replace(String field, String find, String replace) throws MobileServiceException {
		this.mQuery.replace(field, find, replace);
		return this;
	}
}