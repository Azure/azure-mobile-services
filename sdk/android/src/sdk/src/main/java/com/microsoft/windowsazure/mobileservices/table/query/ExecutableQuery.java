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
 * ExecutableQuery.java
 */
package com.microsoft.windowsazure.mobileservices.table.query;

import android.util.Pair;

import com.google.common.util.concurrent.ListenableFuture;
import com.microsoft.windowsazure.mobileservices.MobileServiceList;
import com.microsoft.windowsazure.mobileservices.table.MobileServiceTable;
import com.microsoft.windowsazure.mobileservices.table.TableQueryCallback;

import java.util.Date;
import java.util.List;

/**
 * Class that represents a query to a specific MobileServiceTable<E> instance
 */
public final class ExecutableQuery<E> implements Query {

    /**
     * The internal base query
     */
    private Query mQuery;

    /**
     * The Table to query
     */
    private MobileServiceTable<E> mTable;

    /**
     * Creates an empty Query
     */
    public ExecutableQuery() {
        this.mQuery = new QueryBase();
    }

    /**
     * Creates Query<E> with an existing query as its only internal value
     *
     * @param query The query step to add
     */
    public ExecutableQuery(Query query) {
        if (query.getQueryNode() != null) {
            query = QueryOperations.query(query);
        }

        this.mQuery = query;
    }

    /**
     * Returns the MobileServiceTable<E> to query
     */
    MobileServiceTable<E> getTable() {
        return this.mTable;
    }

    /**
     * Sets the MobileServiceTable<E> to query
     *
     * @param table The MobileServiceTable<E> to query
     */
    public void setTable(MobileServiceTable<E> table) {
        this.mTable = table;
    }

    /**
     * Executes the query
     */
    public ListenableFuture<MobileServiceList<E>> execute() {
        return this.mTable.execute(this);
    }

    /**
     * Executes the query
     *
     * @param callback Callback to invoke when the operation is completed
     * @deprecated use {@link execute()} instead
     */
    public void execute(final TableQueryCallback<E> callback) {
        mTable.execute(this, callback);
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
    public ExecutableQuery<E> tableName(String tableName) {
        this.mQuery.tableName(tableName);
        return this;
    }

    /**
     * * Row Operations ***
     */

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
    public ExecutableQuery<E> includeDeleted() {
        this.mQuery.includeDeleted();

        return this;
    }

    @Override
    public ExecutableQuery<E> removeDeleted() {
        this.mQuery.removeDeleted();

        return this;
    }

    @Override
    public ExecutableQuery<E> select(String... fields) {
        this.mQuery.select(fields);
        return this;
    }

    /**
     * * Query Operations ***
     */

    @Override
    public ExecutableQuery<E> field(String fieldName) {
        this.mQuery.field(fieldName);
        return this;
    }

    @Override
    public ExecutableQuery<E> val(Number number) {
        this.mQuery.val(number);
        return this;
    }

    @Override
    public ExecutableQuery<E> val(boolean val) {
        this.mQuery.val(val);
        return this;
    }

    @Override
    public ExecutableQuery<E> val(String s) {
        this.mQuery.val(s);
        return this;
    }

    @Override
    public ExecutableQuery<E> val(Date date) {
        this.mQuery.val(date);
        return this;
    }

    /**
     * *** Logical Operators *****
     */

    @Override
    public ExecutableQuery<E> and() {
        this.mQuery.and();
        return this;
    }

    @Override
    public ExecutableQuery<E> and(Query otherQuery) {
        this.mQuery.and(otherQuery);
        return this;
    }

    @Override
    public ExecutableQuery<E> or() {
        this.mQuery.or();
        return this;
    }

    @Override
    public ExecutableQuery<E> or(Query otherQuery) {
        this.mQuery.or(otherQuery);
        return this;
    }

    @Override
    public ExecutableQuery<E> not() {
        this.mQuery.not();
        return this;
    }

    @Override
    public ExecutableQuery<E> not(Query otherQuery) {
        this.mQuery.not(otherQuery);
        return this;
    }

    @Override
    public ExecutableQuery<E> not(boolean booleanValue) {
        this.mQuery.not(booleanValue);
        return this;
    }

    /**
     * *** Comparison Operators *****
     */

    @Override
    public ExecutableQuery<E> ge() {
        this.mQuery.ge();
        return this;
    }

    @Override
    public ExecutableQuery<E> ge(String stringValue) {
        this.mQuery.ge(stringValue);
        return this;
    }

    @Override
    public ExecutableQuery<E> ge(Query otherQuery) {
        this.mQuery.ge(otherQuery);
        return this;
    }

    @Override
    public ExecutableQuery<E> ge(Number numberValue) {
        this.mQuery.ge(numberValue);
        return this;
    }

    @Override
    public ExecutableQuery<E> ge(Date dateValue) {
        this.mQuery.ge(dateValue);
        return this;
    }

    @Override
    public ExecutableQuery<E> le() {
        this.mQuery.le();
        return this;
    }

    @Override
    public ExecutableQuery<E> le(Query otherQuery) {
        this.mQuery.le(otherQuery);
        return this;
    }

    @Override
    public ExecutableQuery<E> le(Number numberValue) {
        this.mQuery.le(numberValue);
        return this;
    }

    @Override
    public ExecutableQuery<E> le(Date dateValue) {
        this.mQuery.le(dateValue);
        return this;
    }

    @Override
    public ExecutableQuery<E> gt() {
        this.mQuery.gt();
        return this;
    }

    @Override
    public ExecutableQuery<E> gt(Query otherQuery) {
        this.mQuery.gt(otherQuery);
        return this;
    }

    @Override
    public ExecutableQuery<E> gt(Number numberValue) {
        this.mQuery.gt(numberValue);
        return this;
    }

    @Override
    public ExecutableQuery<E> gt(Date dateValue) {
        this.mQuery.gt(dateValue);
        return this;
    }

    @Override
    public ExecutableQuery<E> gt(String stringValue) {
        this.mQuery.gt(stringValue);
        return this;
    }

    @Override
    public ExecutableQuery<E> lt() {
        this.mQuery.lt();
        return this;
    }

    @Override
    public ExecutableQuery<E> lt(Query otherQuery) {
        this.mQuery.lt(otherQuery);
        return this;
    }

    @Override
    public ExecutableQuery<E> lt(Number numberValue) {
        this.mQuery.lt(numberValue);
        return this;
    }

    @Override
    public ExecutableQuery<E> lt(Date dateValue) {
        this.mQuery.lt(dateValue);
        return this;
    }

    @Override
    public ExecutableQuery<E> eq() {
        this.mQuery.eq();
        return this;
    }

    @Override
    public ExecutableQuery<E> eq(Query otherQuery) {
        this.mQuery.eq(otherQuery);
        return this;
    }

    @Override
    public ExecutableQuery<E> eq(Number numberValue) {
        this.mQuery.eq(numberValue);
        return this;
    }

    @Override
    public ExecutableQuery<E> eq(boolean booleanValue) {
        this.mQuery.eq(booleanValue);
        return this;
    }

    @Override
    public ExecutableQuery<E> eq(String stringValue) {
        this.mQuery.eq(stringValue);
        return this;
    }

    @Override
    public ExecutableQuery<E> eq(Date dateValue) {
        this.mQuery.eq(dateValue);
        return this;
    }

    @Override
    public ExecutableQuery<E> ne() {
        this.mQuery.ne();
        return this;
    }

    @Override
    public ExecutableQuery<E> ne(Query otherQuery) {
        this.mQuery.ne(otherQuery);
        return this;
    }

    @Override
    public ExecutableQuery<E> ne(Number numberValue) {
        this.mQuery.ne(numberValue);
        return this;
    }

    @Override
    public ExecutableQuery<E> ne(boolean booleanValue) {
        this.mQuery.ne(booleanValue);
        return this;
    }

    @Override
    public ExecutableQuery<E> ne(String stringValue) {
        this.mQuery.ne(stringValue);
        return this;
    }

    @Override
    public ExecutableQuery<E> ne(Date dateValue) {
        this.mQuery.ne(dateValue);
        return this;
    }

    /**
     * *** Arithmetic Operators *****
     */

    @Override
    public ExecutableQuery<E> add() {
        this.mQuery.add();
        return this;
    }

    @Override
    public ExecutableQuery<E> add(Query otherQuery) {
        this.mQuery.add(otherQuery);
        return this;
    }

    @Override
    public ExecutableQuery<E> add(Number val) {
        this.mQuery.add(val);
        return this;
    }

    @Override
    public ExecutableQuery<E> sub() {
        this.mQuery.sub();
        return this;
    }

    @Override
    public ExecutableQuery<E> sub(Query otherQuery) {
        this.mQuery.sub(otherQuery);
        return this;
    }

    @Override
    public ExecutableQuery<E> sub(Number val) {
        this.mQuery.sub(val);
        return this;
    }

    @Override
    public ExecutableQuery<E> mul() {
        this.mQuery.mul();
        return this;
    }

    @Override
    public ExecutableQuery<E> mul(Query otherQuery) {
        this.mQuery.mul(otherQuery);
        return this;
    }

    @Override
    public ExecutableQuery<E> mul(Number val) {
        this.mQuery.mul(val);
        return this;
    }

    @Override
    public ExecutableQuery<E> div() {
        this.mQuery.div();
        return this;
    }

    @Override
    public ExecutableQuery<E> div(Query otherQuery) {
        this.mQuery.div(otherQuery);
        return this;
    }

    @Override
    public ExecutableQuery<E> div(Number val) {
        this.mQuery.div(val);
        return this;
    }

    @Override
    public ExecutableQuery<E> mod() {
        this.mQuery.mod();
        return this;
    }

    @Override
    public ExecutableQuery<E> mod(Query otherQuery) {
        this.mQuery.mod(otherQuery);
        return this;
    }

    @Override
    public ExecutableQuery<E> mod(Number val) {
        this.mQuery.mod(val);
        return this;
    }

    /**
     * *** Date Operators *****
     */

    @Override
    public ExecutableQuery<E> year(Query otherQuery) {
        this.mQuery.year(otherQuery);
        return this;
    }

    @Override
    public ExecutableQuery<E> year(String field) {
        this.mQuery.year(field);
        return this;
    }

    @Override
    public ExecutableQuery<E> month(Query otherQuery) {
        this.mQuery.month(otherQuery);
        return this;
    }

    @Override
    public ExecutableQuery<E> month(String field) {
        this.mQuery.month(field);
        return this;
    }

    @Override
    public ExecutableQuery<E> day(Query otherQuery) {
        this.mQuery.day(otherQuery);
        return this;
    }

    @Override
    public ExecutableQuery<E> day(String field) {
        this.mQuery.day(field);
        return this;
    }

    @Override
    public ExecutableQuery<E> hour(Query otherQuery) {
        this.mQuery.hour(otherQuery);
        return this;
    }

    @Override
    public ExecutableQuery<E> hour(String field) {
        this.mQuery.hour(field);
        return this;
    }

    @Override
    public ExecutableQuery<E> minute(Query otherQuery) {
        this.mQuery.minute(otherQuery);
        return this;
    }

    @Override
    public ExecutableQuery<E> minute(String field) {
        this.mQuery.minute(field);
        return this;
    }

    @Override
    public ExecutableQuery<E> second(Query otherQuery) {
        this.mQuery.second(otherQuery);
        return this;
    }

    @Override
    public ExecutableQuery<E> second(String field) {
        this.mQuery.second(field);
        return this;
    }

    /**
     * *** Math Functions *****
     */

    @Override
    public ExecutableQuery<E> floor(Query otherQuery) {
        this.mQuery.floor(otherQuery);
        return this;
    }

    @Override
    public ExecutableQuery<E> ceiling(Query otherQuery) {
        this.mQuery.ceiling(otherQuery);
        return this;
    }

    @Override
    public ExecutableQuery<E> round(Query otherQuery) {
        this.mQuery.round(otherQuery);
        return this;
    }

    /**
     * *** String Operators *****
     */

    @Override
    public ExecutableQuery<E> toLower(Query exp) {
        this.mQuery.toLower(exp);
        return this;
    }

    @Override
    public ExecutableQuery<E> toLower(String field) {
        this.mQuery.toLower(field);
        return this;
    }

    @Override
    public ExecutableQuery<E> toUpper(Query exp) {
        this.mQuery.toUpper(exp);
        return this;
    }

    @Override
    public ExecutableQuery<E> toUpper(String field) {
        this.mQuery.toUpper(field);
        return this;
    }

    @Override
    public ExecutableQuery<E> length(Query exp) {
        this.mQuery.length(exp);
        return this;
    }

    @Override
    public ExecutableQuery<E> length(String field) {
        this.mQuery.length(field);
        return this;
    }

    @Override
    public ExecutableQuery<E> trim(Query exp) {
        this.mQuery.trim(exp);
        return this;
    }

    @Override
    public ExecutableQuery<E> trim(String field) {
        this.mQuery.trim(field);
        return this;
    }

    @Override
    public ExecutableQuery<E> startsWith(Query field, Query start) {
        this.mQuery.startsWith(field, start);
        return this;
    }

    @Override
    public ExecutableQuery<E> startsWith(String field, String start) {
        this.mQuery.startsWith(field, start);
        return this;
    }

    @Override
    public ExecutableQuery<E> endsWith(Query field, Query end) {
        this.mQuery.endsWith(field, end);
        return this;
    }

    @Override
    public ExecutableQuery<E> endsWith(String field, String end) {
        this.mQuery.endsWith(field, end);
        return this;
    }

    @Override
    public ExecutableQuery<E> subStringOf(Query str1, Query str2) {
        this.mQuery.subStringOf(str1, str2);
        return this;
    }

    @Override
    public ExecutableQuery<E> subStringOf(String str, String field) {
        this.mQuery.subStringOf(str, field);
        return this;
    }

    @Override
    public ExecutableQuery<E> concat(Query str1, Query str2) {
        this.mQuery.concat(str1, str2);
        return this;
    }

    @Override
    public ExecutableQuery<E> concat(Query str1, String str2) {
        this.mQuery.concat(str1, str2);
        return this;
    }

    @Override
    public ExecutableQuery<E> indexOf(Query haystack, Query needle) {
        this.mQuery.indexOf(haystack, needle);
        return this;
    }

    @Override
    public ExecutableQuery<E> indexOf(String field, String needle) {
        this.mQuery.indexOf(field, needle);
        return this;
    }

    @Override
    public ExecutableQuery<E> subString(Query str, Query pos) {
        this.mQuery.subString(str, pos);
        return this;
    }

    @Override
    public ExecutableQuery<E> subString(String field, int pos) {
        this.mQuery.subString(field, pos);
        return this;
    }

    @Override
    public ExecutableQuery<E> subString(Query str, Query pos, Query length) {
        this.mQuery.subString(str, pos, length);
        return this;
    }

    @Override
    public ExecutableQuery<E> subString(String field, int pos, int length) {
        this.mQuery.subString(field, pos, length);
        return this;
    }

    @Override
    public ExecutableQuery<E> replace(Query str, Query find, Query replace) {
        this.mQuery.replace(str, find, replace);
        return this;
    }

    @Override
    public ExecutableQuery<E> replace(String field, String find, String replace) {
        this.mQuery.replace(field, find, replace);
        return this;
    }
}