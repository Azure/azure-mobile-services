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

import com.microsoft.windowsazure.mobileservices.MobileServiceException;

import android.util.Pair;

/**
 * Interface that represents a query to a table.
 */
public interface Query {

	/**
	 * Returns the root node of the query
	 */
	public QueryNode getQueryNode();

	/**
	 * Sets the root node of the query
	 * 
	 * @param queryNode
	 *            The node to set
	 */
	public void setQueryNode(QueryNode queryNode);

	/**
	 * Returns true if inline count is requested.
	 */
	public boolean hasInlineCount();

	/**
	 * Returns a list of fields to order by the results, and their respective
	 * ordering direction
	 */
	public List<Pair<String, QueryOrder>> getOrderBy();

	/**
	 * Returns a requested list of projections;
	 */
	public List<String> getProjection();

	/**
	 * Returns a list of custom parameters set by the user
	 */
	public List<Pair<String, String>> getUserDefinedParameters();

	/**
	 * Returns a specified top value;
	 */
	public int getTop();

	/**
	 * Returns a specified skip value;
	 */
	public int getSkip();

	/**
	 * Returns the table name;
	 */
	public String getTableName();

	/**
	 * Sets the table name;
	 */
	public Query tableName(String tableName);

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
	public Query parameter(String parameter, String value);

	/**
	 * Adds a new order by statement
	 * 
	 * @param field
	 *            FieldName
	 * @param order
	 *            Sorting order
	 * @return Query
	 */
	public Query orderBy(String field, QueryOrder order);

	/**
	 * Sets the number of records to return
	 * 
	 * @param top
	 *            Number of records to return
	 * @return Query
	 */
	public Query top(int top);

	/**
	 * Sets the number of records to skip over a given number of elements in a
	 * sequence and then return the remainder.
	 * 
	 * @param skip
	 * @return Query
	 */
	public Query skip(int skip);

	/**
	 * The inlinecount property specifies whether or not to retrieve a property
	 * with the number of records returned.
	 * 
	 * @return Query
	 */
	public Query includeInlineCount();

	/**
	 * Specifies the fields to retrieve
	 * 
	 * @param fields
	 *            Names of the fields to retrieve
	 * @return Query
	 */
	public Query select(String... fields);

	/**** Query Operations ****/

	/**
	 * Specifies the field to use
	 * 
	 * @param fieldName
	 *            The field to use
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query field(String fieldName) throws MobileServiceException;

	/**
	 * Specifies a numeric value
	 * 
	 * @param number
	 *            The numeric value to use
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query val(Number number) throws MobileServiceException;

	/**
	 * Specifies a boolean value
	 * 
	 * @param number
	 *            The boolean value to use
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query val(boolean val) throws MobileServiceException;

	/**
	 * Specifies a string value
	 * 
	 * @param number
	 *            The string value to use
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query val(String s) throws MobileServiceException;

	/**
	 * Specifies a date value
	 * 
	 * @param number
	 *            The date value to use
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query val(Date date) throws MobileServiceException;

	/****** Logical Operators ******/

	/**
	 * Conditional and.
	 * 
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query and() throws MobileServiceException;

	/**
	 * Conditional and.
	 * 
	 * @param otherQuery
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query and(Query otherQuery) throws MobileServiceException;

	/**
	 * Conditional or.
	 * 
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query or() throws MobileServiceException;

	/**
	 * Conditional or.
	 * 
	 * @param otherQuery
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query or(Query otherQuery) throws MobileServiceException;

	/**
	 * Logical not.
	 * 
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query not() throws MobileServiceException;

	/**
	 * Logical not.
	 * 
	 * @param otherQuery
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query not(Query otherQuery) throws MobileServiceException;

	/**
	 * Logical not.
	 * 
	 * @param booleanValue
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query not(boolean booleanValue) throws MobileServiceException;

	/****** Comparison Operators ******/

	/**
	 * Greater than or equal comparison operator.
	 * 
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query ge() throws MobileServiceException;

	/**
	 * Greater than or equal comparison operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query ge(Query otherQuery) throws MobileServiceException;

	/**
	 * Greater than or equal comparison operator.
	 * 
	 * @param numberValue
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query ge(Number numberValue) throws MobileServiceException;

	/**
	 * Greater than or equal comparison operator.
	 * 
	 * @param dateValue
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query ge(Date dateValue) throws MobileServiceException;

	/**
	 * Less than or equal comparison operator.
	 * 
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query le() throws MobileServiceException;

	/**
	 * Less than or equal comparison operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query le(Query otherQuery) throws MobileServiceException;

	/**
	 * Less than or equal comparison operator.
	 * 
	 * @param numberValue
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query le(Number numberValue) throws MobileServiceException;

	/**
	 * Less than or equal comparison operator.
	 * 
	 * @param dateValue
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query le(Date dateValue) throws MobileServiceException;

	/**
	 * Greater than comparison operator.
	 * 
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query gt() throws MobileServiceException;

	/**
	 * Greater than comparison operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query gt(Query otherQuery) throws MobileServiceException;

	/**
	 * Greater than comparison operator.
	 * 
	 * @param numberValue
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query gt(Number numberValue) throws MobileServiceException;

	/**
	 * Greater than comparison operator.
	 * 
	 * @param dateValue
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query gt(Date dateValue) throws MobileServiceException;

	/**
	 * Less than comparison operator.
	 * 
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query lt() throws MobileServiceException;

	/**
	 * Less than comparison operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query lt(Query otherQuery) throws MobileServiceException;

	/**
	 * Less than comparison operator.
	 * 
	 * @param numberValue
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query lt(Number numberValue) throws MobileServiceException;

	/**
	 * Less than comparison operator.
	 * 
	 * @param dateValue
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query lt(Date dateValue) throws MobileServiceException;

	/**
	 * Equal comparison operator.
	 * 
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query eq() throws MobileServiceException;

	/**
	 * Equal comparison operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query eq(Query otherQuery) throws MobileServiceException;

	/**
	 * Equal comparison operator.
	 * 
	 * @param numberValue
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query eq(Number numberValue) throws MobileServiceException;

	/**
	 * Equal comparison operator.
	 * 
	 * @param booleanValue
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query eq(boolean booleanValue) throws MobileServiceException;

	/**
	 * Equal comparison operator.
	 * 
	 * @param stringValue
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query eq(String stringValue) throws MobileServiceException;

	/**
	 * Equal comparison operator.
	 * 
	 * @param dateValue
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query eq(Date dateValue) throws MobileServiceException;

	/**
	 * Not equal comparison operator.
	 * 
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query ne() throws MobileServiceException;

	/**
	 * Not equal comparison operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query ne(Query otherQuery) throws MobileServiceException;

	/**
	 * Not equal comparison operator.
	 * 
	 * @param numberValue
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query ne(Number numberValue) throws MobileServiceException;

	/**
	 * Not equal comparison operator.
	 * 
	 * @param booleanValue
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query ne(boolean booleanValue) throws MobileServiceException;

	/**
	 * Not equal comparison operator.
	 * 
	 * @param stringValue
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query ne(String stringValue) throws MobileServiceException;

	/**
	 * Not equal comparison operator.
	 * 
	 * @param dateValue
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query ne(Date dateValue) throws MobileServiceException;

	/****** Arithmetic Operators ******/

	/**
	 * Add operator.
	 * 
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query add() throws MobileServiceException;

	/**
	 * Add operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query add(Query otherQuery) throws MobileServiceException;

	/**
	 * Add operator.
	 * 
	 * @param val
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query add(Number val) throws MobileServiceException;

	/**
	 * Subtract operator.
	 * 
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query sub() throws MobileServiceException;

	/**
	 * Subtract operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query sub(Query otherQuery) throws MobileServiceException;

	/**
	 * Subtract operator.
	 * 
	 * @param val
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query sub(Number val) throws MobileServiceException;

	/**
	 * Multiply operator.
	 * 
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query mul() throws MobileServiceException;

	/**
	 * Multiply operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query mul(Query otherQuery) throws MobileServiceException;

	/**
	 * Multiply operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query mul(Number val) throws MobileServiceException;

	/**
	 * Divide operator.
	 * 
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query div() throws MobileServiceException;

	/**
	 * Divide operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query div(Query otherQuery) throws MobileServiceException;

	/**
	 * Divide operator.
	 * 
	 * @param val
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query div(Number val) throws MobileServiceException;

	/**
	 * Reminder (or modulo) operator.
	 * 
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query mod() throws MobileServiceException;

	/**
	 * Reminder (or modulo) operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query mod(Query otherQuery) throws MobileServiceException;

	/**
	 * Reminder (or modulo) operator.
	 * 
	 * @param val
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query mod(Number val) throws MobileServiceException;

	/****** Date Operators ******/

	/**
	 * The year component value of the parameter value.
	 * 
	 * @param otherQuery
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query year(Query otherQuery) throws MobileServiceException;

	/**
	 * The year component value of the parameter value.
	 * 
	 * @param field
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query year(String field) throws MobileServiceException;

	/**
	 * The month component value of the parameter value.
	 * 
	 * @param otherQuery
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query month(Query otherQuery) throws MobileServiceException;

	/**
	 * The month component value of the parameter value.
	 * 
	 * @param field
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query month(String field) throws MobileServiceException;

	/**
	 * The day component value of the parameter value.
	 * 
	 * @param otherQuery
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query day(Query otherQuery) throws MobileServiceException;

	/**
	 * The day component value of the parameter value.
	 * 
	 * @param field
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query day(String field) throws MobileServiceException;

	/**
	 * The hour component value of the parameter value.
	 * 
	 * @param otherQuery
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query hour(Query otherQuery) throws MobileServiceException;

	/**
	 * The hour component value of the parameter value.
	 * 
	 * @param field
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query hour(String field) throws MobileServiceException;

	/**
	 * The minute component value of the parameter value.
	 * 
	 * @param otherQuery
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query minute(Query otherQuery) throws MobileServiceException;

	/**
	 * The minute component value of the parameter value.
	 * 
	 * @param field
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query minute(String field) throws MobileServiceException;

	/**
	 * The second component value of the parameter value.
	 * 
	 * @param otherQuery
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query second(Query otherQuery) throws MobileServiceException;

	/**
	 * The second component value of the parameter value.
	 * 
	 * @param field
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query second(String field) throws MobileServiceException;

	/****** Math Functions ******/

	/**
	 * The largest integral value less than or equal to the parameter value.
	 * 
	 * @param otherQuery
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query floor(Query otherQuery) throws MobileServiceException;

	/**
	 * The smallest integral value greater than or equal to the parameter value.
	 * 
	 * @param otherQuery
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query ceiling(Query otherQuery) throws MobileServiceException;

	/**
	 * The nearest integral value to the parameter value.
	 * 
	 * @param otherQuery
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query round(Query otherQuery) throws MobileServiceException;

	/****** String Operators ******/

	/**
	 * String value with the contents of the parameter value converted to lower
	 * case.
	 * 
	 * @param exp
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query toLower(Query exp) throws MobileServiceException;

	/**
	 * String value with the contents of the parameter value converted to lower
	 * case.
	 * 
	 * @param field
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query toLower(String field) throws MobileServiceException;

	/**
	 * String value with the contents of the parameter value converted to upper
	 * case.
	 * 
	 * @param exp
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query toUpper(Query exp) throws MobileServiceException;

	/**
	 * String value with the contents of the parameter value converted to upper
	 * case.
	 * 
	 * @param field
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query toUpper(String field) throws MobileServiceException;

	/**
	 * The number of characters in the specified parameter value.
	 * 
	 * @param exp
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query length(Query exp) throws MobileServiceException;

	/**
	 * The number of characters in the specified parameter value.
	 * 
	 * @param field
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query length(String field) throws MobileServiceException;

	/**
	 * String value with the contents of the parameter value with all leading
	 * and trailing white-space characters removed.
	 * 
	 * @param exp
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query trim(Query exp) throws MobileServiceException;

	/**
	 * String value with the contents of the parameter value with all leading
	 * and trailing white-space characters removed.
	 * 
	 * @param field
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query trim(String field) throws MobileServiceException;

	/**
	 * Whether the beginning of the first parameter values matches the second
	 * parameter value.
	 * 
	 * @param field
	 *            The field to evaluate
	 * @param start
	 *            Start value
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query startsWith(Query field, Query start) throws MobileServiceException;

	/**
	 * Whether the beginning of the first parameter values matches the second
	 * parameter value.
	 * 
	 * @param field
	 *            The field to evaluate
	 * @param start
	 *            Start value
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query startsWith(String field, String start) throws MobileServiceException;

	/**
	 * Whether the end of the first parameter value matches the second parameter
	 * value.
	 * 
	 * @param field
	 *            The field to evaluate
	 * @param end
	 *            End value
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query endsWith(Query field, Query end) throws MobileServiceException;

	/**
	 * Whether the end of the first parameter value matches the second parameter
	 * value.
	 * 
	 * @param field
	 *            The field to evaluate
	 * @param end
	 *            End value
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query endsWith(String field, String end) throws MobileServiceException;

	/**
	 * Whether the first parameter string value occurs in the second parameter
	 * string value.
	 * 
	 * @param str1
	 *            First string
	 * @param str2
	 *            Second string
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query subStringOf(Query str1, Query str2) throws MobileServiceException;

	/**
	 * Whether the string parameter occurs in the field
	 * 
	 * @param str2
	 *            String to search
	 * @param field
	 *            Field to search in
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query subStringOf(String str, String field) throws MobileServiceException;

	/**
	 * String value which is the first and second parameter values merged
	 * together with the first parameter value coming first in the result.
	 * 
	 * @param str1
	 *            First string
	 * @param str2
	 *            Second string
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query concat(Query str1, Query str2) throws MobileServiceException;

	/**
	 * Index of the first occurrence of the second parameter value in the first
	 * parameter value or -1 otherwise.
	 * 
	 * @param haystack
	 *            String content
	 * @param needle
	 *            Value to search for
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query indexOf(Query haystack, Query needle) throws MobileServiceException;

	/**
	 * Index of the first occurrence of the second parameter value in the first
	 * parameter value or -1 otherwise.
	 * 
	 * @param field
	 *            Field to search in
	 * @param str
	 *            Value to search for
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query indexOf(String field, String needle) throws MobileServiceException;

	/**
	 * String value starting at the character index specified by the second
	 * parameter value in the first parameter string value.
	 * 
	 * @param str
	 *            String content
	 * @param pos
	 *            Starting position
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query subString(Query str, Query pos) throws MobileServiceException;

	/**
	 * String value starting at the character index specified by the second
	 * parameter value in the first parameter string value.
	 * 
	 * @param field
	 *            Field to scan
	 * @param pos
	 *            Starting position
	 * @return Query
	 * @throws MobileServiceException
	 */
	public Query subString(String field, int pos) throws MobileServiceException;

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
	 * @throws MobileServiceException
	 */
	public Query subString(Query str, Query pos, Query length) throws MobileServiceException;

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
	 * @throws MobileServiceException
	 */
	public Query subString(String field, int pos, int length) throws MobileServiceException;

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
	 * @throws MobileServiceException
	 */
	public Query replace(Query str, Query find, Query replace) throws MobileServiceException;

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
	 * @throws MobileServiceException
	 */
	public Query replace(String field, String find, String replace) throws MobileServiceException;
}