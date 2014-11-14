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
 * Query.java
 */
package com.microsoft.windowsazure.mobileservices.table.query;

import java.util.Date;
import java.util.List;

import android.util.Pair;

/**
 * Interface that represents a query to a table.
 */
public interface Query {
	/**
	 * Deep clone the Query instance
	 * 
	 * @return A cloned instance of the Query
	 */
	Query deepClone();

	/**
	 * Returns the root node of the query
	 */
	QueryNode getQueryNode();

	/**
	 * Sets the root node of the query
	 * 
	 * @param queryNode
	 *            The node to set
	 */
	void setQueryNode(QueryNode queryNode);

	/**
	 * Returns true if inline count is requested.
	 */
	boolean hasInlineCount();

    /**
     * Returns true if inline count is requested.
     */
    boolean hasDeleted();

	/**
	 * Returns a list of fields to order by the results, and their respective
	 * ordering direction
	 */
	List<Pair<String, QueryOrder>> getOrderBy();

	/**
	 * Returns a requested list of projections;
	 */
	List<String> getProjection();

	/**
	 * Returns a list of custom parameters set by the user
	 */
	List<Pair<String, String>> getUserDefinedParameters();

	/**
	 * Returns a specified top value;
	 */
	int getTop();

	/**
	 * Returns a specified skip value;
	 */
	int getSkip();

	/**
	 * Returns the table name;
	 */
	String getTableName();

	/**
	 * Sets the table name;
	 */
	Query tableName(String tableName);

	/**** Row Operations ****/

	/**
	 * Adds a new user-defined parameter to the query
	 * 
	 * @param parameter
	 *            The parameter name
	 * @param value
	 *            The parameter value
	 * @return Query
	 */
	Query parameter(String parameter, String value);

	/**
	 * Adds a new order by statement
	 * 
	 * @param field
	 *            FieldName
	 * @param order
	 *            Sorting order
	 * @return Query
	 */
	Query orderBy(String field, QueryOrder order);

	/**
	 * Sets the number of records to return
	 * 
	 * @param top
	 *            Number of records to return
	 * @return Query
	 */
	Query top(int top);

	/**
	 * Sets the number of records to skip over a given number of elements in a
	 * sequence and then return the remainder.
	 * 
	 * @param skip
	 * @return Query
	 */
	Query skip(int skip);

	/**
	 * The inlinecount property specifies whether or not to retrieve a property
	 * with the number of records returned.
	 * 
	 * @return Query
	 */
	Query includeInlineCount();

	/**
	 * Set the inlinecount property to false.
	 * 
	 * @return Query
	 */
	Query removeInlineCount();

    /**
     * Specifies to retrieve soft deleted rows
     *
     * @return Query
     */
    Query includeDeleted();

    /**
     * Set the hasDeleted property to false.
     *
     * @return Query
     */
    Query removeDeleted();

	/**
	 * Specifies the fields to retrieve
	 * 
	 * @param fields
	 *            Names of the fields to retrieve
	 * @return Query
	 */
	Query select(String... fields);

	/**** Query Operations ****/

	/**
	 * Specifies the field to use
	 * 
	 * @param fieldName
	 *            The field to use
	 * @return Query
	 */
	Query field(String fieldName);

	/**
	 * Specifies a numeric value
	 * 
	 * @param number
	 *            The numeric value to use
	 * @return Query
	 */
	Query val(Number number);

	/**
	 * Specifies a boolean value
	 * 
	 * @param number
	 *            The boolean value to use
	 * @return Query
	 */
	Query val(boolean val);

	/**
	 * Specifies a string value
	 * 
	 * @param number
	 *            The string value to use
	 * @return Query
	 */
	Query val(String s);

	/**
	 * Specifies a date value
	 * 
	 * @param number
	 *            The date value to use
	 * @return Query
	 */
	Query val(Date date);

	/****** Logical Operators ******/

	/**
	 * Conditional and.
	 * 
	 * @return Query
	 */
	Query and();

	/**
	 * Conditional and.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	Query and(Query otherQuery);

	/**
	 * Conditional or.
	 * 
	 * @return Query
	 */
	Query or();

	/**
	 * Conditional or.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	Query or(Query otherQuery);

	/**
	 * Logical not.
	 * 
	 * @return Query
	 */
	Query not();

	/**
	 * Logical not.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	Query not(Query otherQuery);

	/**
	 * Logical not.
	 * 
	 * @param booleanValue
	 * @return Query
	 */
	Query not(boolean booleanValue);

	/****** Comparison Operators ******/

	/**
	 * Greater than or equal comparison operator.
	 * 
	 * @return Query
	 */
	Query ge();

    /**
     * Greater than or equal comparison operator.
     *
     * @return Query
     */
    Query ge(String stringValue);

	/**
	 * Greater than or equal comparison operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	Query ge(Query otherQuery);

	/**
	 * Greater than or equal comparison operator.
	 * 
	 * @param numberValue
	 * @return Query
	 */
	Query ge(Number numberValue);

	/**
	 * Greater than or equal comparison operator.
	 * 
	 * @param dateValue
	 * @return Query
	 */
	Query ge(Date dateValue);

	/**
	 * Less than or equal comparison operator.
	 * 
	 * @return Query
	 */
	Query le();

	/**
	 * Less than or equal comparison operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	Query le(Query otherQuery);

	/**
	 * Less than or equal comparison operator.
	 * 
	 * @param numberValue
	 * @return Query
	 */
	Query le(Number numberValue);

	/**
	 * Less than or equal comparison operator.
	 * 
	 * @param dateValue
	 * @return Query
	 */
	Query le(Date dateValue);

	/**
	 * Greater than comparison operator.
	 * 
	 * @return Query
	 */
	Query gt();

	/**
	 * Greater than comparison operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	Query gt(Query otherQuery);

	/**
	 * Greater than comparison operator.
	 * 
	 * @param numberValue
	 * @return Query
	 */
	Query gt(Number numberValue);

	/**
	 * Greater than comparison operator.
	 * 
	 * @param dateValue
	 * @return Query
	 */
	Query gt(Date dateValue);

    /**
     * Greater than comparison operator.
     *
     * @param stringValue
     * @return Query
     */
    Query gt(String stringValue);

    /**
	 * Less than comparison operator.
	 * 
	 * @return Query
	 */
	Query lt();

	/**
	 * Less than comparison operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	Query lt(Query otherQuery);

	/**
	 * Less than comparison operator.
	 * 
	 * @param numberValue
	 * @return Query
	 */
	Query lt(Number numberValue);

	/**
	 * Less than comparison operator.
	 * 
	 * @param dateValue
	 * @return Query
	 */
	Query lt(Date dateValue);

	/**
	 * Equal comparison operator.
	 * 
	 * @return Query
	 */
	Query eq();

	/**
	 * Equal comparison operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	Query eq(Query otherQuery);

	/**
	 * Equal comparison operator.
	 * 
	 * @param numberValue
	 * @return Query
	 */
	Query eq(Number numberValue);

	/**
	 * Equal comparison operator.
	 * 
	 * @param booleanValue
	 * @return Query
	 */
	Query eq(boolean booleanValue);

	/**
	 * Equal comparison operator.
	 * 
	 * @param stringValue
	 * @return Query
	 */
	Query eq(String stringValue);

	/**
	 * Equal comparison operator.
	 * 
	 * @param dateValue
	 * @return Query
	 */
	Query eq(Date dateValue);

	/**
	 * Not equal comparison operator.
	 * 
	 * @return Query
	 */
	Query ne();

	/**
	 * Not equal comparison operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	Query ne(Query otherQuery);

	/**
	 * Not equal comparison operator.
	 * 
	 * @param numberValue
	 * @return Query
	 */
	Query ne(Number numberValue);

	/**
	 * Not equal comparison operator.
	 * 
	 * @param booleanValue
	 * @return Query
	 */
	Query ne(boolean booleanValue);

	/**
	 * Not equal comparison operator.
	 * 
	 * @param stringValue
	 * @return Query
	 */
	Query ne(String stringValue);

	/**
	 * Not equal comparison operator.
	 * 
	 * @param dateValue
	 * @return Query
	 */
	Query ne(Date dateValue);

	/****** Arithmetic Operators ******/

	/**
	 * Add operator.
	 * 
	 * @return Query
	 */
	Query add();

	/**
	 * Add operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	Query add(Query otherQuery);

	/**
	 * Add operator.
	 * 
	 * @param val
	 * @return Query
	 */
	Query add(Number val);

	/**
	 * Subtract operator.
	 * 
	 * @return Query
	 */
	Query sub();

	/**
	 * Subtract operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	Query sub(Query otherQuery);

	/**
	 * Subtract operator.
	 * 
	 * @param val
	 * @return Query
	 */
	Query sub(Number val);

	/**
	 * Multiply operator.
	 * 
	 * @return Query
	 */
	Query mul();

	/**
	 * Multiply operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	Query mul(Query otherQuery);

	/**
	 * Multiply operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	Query mul(Number val);

	/**
	 * Divide operator.
	 * 
	 * @return Query
	 */
	Query div();

	/**
	 * Divide operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	Query div(Query otherQuery);

	/**
	 * Divide operator.
	 * 
	 * @param val
	 * @return Query
	 */
	Query div(Number val);

	/**
	 * Reminder (or modulo) operator.
	 * 
	 * @return Query
	 */
	Query mod();

	/**
	 * Reminder (or modulo) operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	Query mod(Query otherQuery);

	/**
	 * Reminder (or modulo) operator.
	 * 
	 * @param val
	 * @return Query
	 */
	Query mod(Number val);

	/****** Date Operators ******/

	/**
	 * The year component value of the parameter value.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	Query year(Query otherQuery);

	/**
	 * The year component value of the parameter value.
	 * 
	 * @param field
	 * @return Query
	 */
	Query year(String field);

	/**
	 * The month component value of the parameter value.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	Query month(Query otherQuery);

	/**
	 * The month component value of the parameter value.
	 * 
	 * @param field
	 * @return Query
	 */
	Query month(String field);

	/**
	 * The day component value of the parameter value.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	Query day(Query otherQuery);

	/**
	 * The day component value of the parameter value.
	 * 
	 * @param field
	 * @return Query
	 */
	Query day(String field);

	/**
	 * The hour component value of the parameter value.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	Query hour(Query otherQuery);

	/**
	 * The hour component value of the parameter value.
	 * 
	 * @param field
	 * @return Query
	 */
	Query hour(String field);

	/**
	 * The minute component value of the parameter value.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	Query minute(Query otherQuery);

	/**
	 * The minute component value of the parameter value.
	 * 
	 * @param field
	 * @return Query
	 */
	Query minute(String field);

	/**
	 * The second component value of the parameter value.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	Query second(Query otherQuery);

	/**
	 * The second component value of the parameter value.
	 * 
	 * @param field
	 * @return Query
	 */
	Query second(String field);

	/****** Math Functions ******/

	/**
	 * The largest integral value less than or equal to the parameter value.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	Query floor(Query otherQuery);

	/**
	 * The smallest integral value greater than or equal to the parameter value.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	Query ceiling(Query otherQuery);

	/**
	 * The nearest integral value to the parameter value.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	Query round(Query otherQuery);

	/****** String Operators ******/

	/**
	 * String value with the contents of the parameter value converted to lower
	 * case.
	 * 
	 * @param exp
	 * @return Query
	 */
	Query toLower(Query exp);

	/**
	 * String value with the contents of the parameter value converted to lower
	 * case.
	 * 
	 * @param field
	 * @return Query
	 */
	Query toLower(String field);

	/**
	 * String value with the contents of the parameter value converted to upper
	 * case.
	 * 
	 * @param exp
	 * @return Query
	 */
	Query toUpper(Query exp);

	/**
	 * String value with the contents of the parameter value converted to upper
	 * case.
	 * 
	 * @param field
	 * @return Query
	 */
	Query toUpper(String field);

	/**
	 * The number of characters in the specified parameter value.
	 * 
	 * @param exp
	 * @return Query
	 */
	Query length(Query exp);

	/**
	 * The number of characters in the specified parameter value.
	 * 
	 * @param field
	 * @return Query
	 */
	Query length(String field);

	/**
	 * String value with the contents of the parameter value with all leading
	 * and trailing white-space characters removed.
	 * 
	 * @param exp
	 * @return Query
	 */
	Query trim(Query exp);

	/**
	 * String value with the contents of the parameter value with all leading
	 * and trailing white-space characters removed.
	 * 
	 * @param field
	 * @return Query
	 */
	Query trim(String field);

	/**
	 * Whether the beginning of the first parameter values matches the second
	 * parameter value.
	 * 
	 * @param field
	 *            The field to evaluate
	 * @param start
	 *            Start value
	 * @return Query
	 */
	Query startsWith(Query field, Query start);

	/**
	 * Whether the beginning of the first parameter values matches the second
	 * parameter value.
	 * 
	 * @param field
	 *            The field to evaluate
	 * @param start
	 *            Start value
	 * @return Query
	 */
	Query startsWith(String field, String start);

	/**
	 * Whether the end of the first parameter value matches the second parameter
	 * value.
	 * 
	 * @param field
	 *            The field to evaluate
	 * @param end
	 *            End value
	 * @return Query
	 */
	Query endsWith(Query field, Query end);

	/**
	 * Whether the end of the first parameter value matches the second parameter
	 * value.
	 * 
	 * @param field
	 *            The field to evaluate
	 * @param end
	 *            End value
	 * @return Query
	 */
	Query endsWith(String field, String end);

	/**
	 * Whether the first parameter string value occurs in the second parameter
	 * string value.
	 * 
	 * @param str1
	 *            First string
	 * @param str2
	 *            Second string
	 * @return Query
	 */
	Query subStringOf(Query str1, Query str2);

	/**
	 * Whether the string parameter occurs in the field
	 * 
	 * @param str2
	 *            String to search
	 * @param field
	 *            Field to search in
	 * @return Query
	 */
	Query subStringOf(String str, String field);

	/**
	 * String value which is the first and second parameter values merged
	 * together with the first parameter value coming first in the result.
	 * 
	 * @param str1
	 *            First string
	 * @param str2
	 *            Second string
	 * @return Query
	 */
	Query concat(Query str1, Query str2);

	/**
	 * String value which is the first and second parameter values merged
	 * together with the first parameter value coming first in the result.
	 * 
	 * @param str1
	 *            First string
	 * @param str2
	 *            Second string
	 * @return Query
	 */
	Query concat(Query str1, String str2);

	/**
	 * Index of the first occurrence of the second parameter value in the first
	 * parameter value or -1 otherwise.
	 * 
	 * @param haystack
	 *            String content
	 * @param needle
	 *            Value to search for
	 * @return Query
	 */
	Query indexOf(Query haystack, Query needle);

	/**
	 * Index of the first occurrence of the second parameter value in the first
	 * parameter value or -1 otherwise.
	 * 
	 * @param field
	 *            Field to search in
	 * @param str
	 *            Value to search for
	 * @return Query
	 */
	Query indexOf(String field, String needle);

	/**
	 * String value starting at the character index specified by the second
	 * parameter value in the first parameter string value.
	 * 
	 * @param str
	 *            String content
	 * @param pos
	 *            Starting position
	 * @return Query
	 */
	Query subString(Query str, Query pos);

	/**
	 * String value starting at the character index specified by the second
	 * parameter value in the first parameter string value.
	 * 
	 * @param field
	 *            Field to scan
	 * @param pos
	 *            Starting position
	 * @return Query
	 */
	Query subString(String field, int pos);

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
	 * @return Query
	 */
	Query subString(Query str, Query pos, Query length);

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
	 * @return Query
	 */
	Query subString(String field, int pos, int length);

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
	 * @return Query
	 */
	Query replace(Query str, Query find, Query replace);

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
	 * @return Query
	 */
	Query replace(String field, String find, String replace);
}