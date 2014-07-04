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

import android.util.Pair;

/**
 * Interface that represents a query to a table.
 */
public interface Query {

	public Query deepClone();

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
	 * Set the inlinecount property to false.
	 * 
	 * @return Query
	 */
	public Query removeInlineCount();

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
	 */
	public Query field(String fieldName);

	/**
	 * Specifies a numeric value
	 * 
	 * @param number
	 *            The numeric value to use
	 * @return Query
	 */
	public Query val(Number number);

	/**
	 * Specifies a boolean value
	 * 
	 * @param number
	 *            The boolean value to use
	 * @return Query
	 */
	public Query val(boolean val);

	/**
	 * Specifies a string value
	 * 
	 * @param number
	 *            The string value to use
	 * @return Query
	 */
	public Query val(String s);

	/**
	 * Specifies a date value
	 * 
	 * @param number
	 *            The date value to use
	 * @return Query
	 */
	public Query val(Date date);

	/****** Logical Operators ******/

	/**
	 * Conditional and.
	 * 
	 * @return Query
	 */
	public Query and();

	/**
	 * Conditional and.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	public Query and(Query otherQuery);

	/**
	 * Conditional or.
	 * 
	 * @return Query
	 */
	public Query or();

	/**
	 * Conditional or.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	public Query or(Query otherQuery);

	/**
	 * Logical not.
	 * 
	 * @return Query
	 */
	public Query not();

	/**
	 * Logical not.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	public Query not(Query otherQuery);

	/**
	 * Logical not.
	 * 
	 * @param booleanValue
	 * @return Query
	 */
	public Query not(boolean booleanValue);

	/****** Comparison Operators ******/

	/**
	 * Greater than or equal comparison operator.
	 * 
	 * @return Query
	 */
	public Query ge();

	/**
	 * Greater than or equal comparison operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	public Query ge(Query otherQuery);

	/**
	 * Greater than or equal comparison operator.
	 * 
	 * @param numberValue
	 * @return Query
	 */
	public Query ge(Number numberValue);

	/**
	 * Greater than or equal comparison operator.
	 * 
	 * @param dateValue
	 * @return Query
	 */
	public Query ge(Date dateValue);

	/**
	 * Less than or equal comparison operator.
	 * 
	 * @return Query
	 */
	public Query le();

	/**
	 * Less than or equal comparison operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	public Query le(Query otherQuery);

	/**
	 * Less than or equal comparison operator.
	 * 
	 * @param numberValue
	 * @return Query
	 */
	public Query le(Number numberValue);

	/**
	 * Less than or equal comparison operator.
	 * 
	 * @param dateValue
	 * @return Query
	 */
	public Query le(Date dateValue);

	/**
	 * Greater than comparison operator.
	 * 
	 * @return Query
	 */
	public Query gt();

	/**
	 * Greater than comparison operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	public Query gt(Query otherQuery);

	/**
	 * Greater than comparison operator.
	 * 
	 * @param numberValue
	 * @return Query
	 */
	public Query gt(Number numberValue);

	/**
	 * Greater than comparison operator.
	 * 
	 * @param dateValue
	 * @return Query
	 */
	public Query gt(Date dateValue);

	/**
	 * Less than comparison operator.
	 * 
	 * @return Query
	 */
	public Query lt();

	/**
	 * Less than comparison operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	public Query lt(Query otherQuery);

	/**
	 * Less than comparison operator.
	 * 
	 * @param numberValue
	 * @return Query
	 */
	public Query lt(Number numberValue);

	/**
	 * Less than comparison operator.
	 * 
	 * @param dateValue
	 * @return Query
	 */
	public Query lt(Date dateValue);

	/**
	 * Equal comparison operator.
	 * 
	 * @return Query
	 */
	public Query eq();

	/**
	 * Equal comparison operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	public Query eq(Query otherQuery);

	/**
	 * Equal comparison operator.
	 * 
	 * @param numberValue
	 * @return Query
	 */
	public Query eq(Number numberValue);

	/**
	 * Equal comparison operator.
	 * 
	 * @param booleanValue
	 * @return Query
	 */
	public Query eq(boolean booleanValue);

	/**
	 * Equal comparison operator.
	 * 
	 * @param stringValue
	 * @return Query
	 */
	public Query eq(String stringValue);

	/**
	 * Equal comparison operator.
	 * 
	 * @param dateValue
	 * @return Query
	 */
	public Query eq(Date dateValue);

	/**
	 * Not equal comparison operator.
	 * 
	 * @return Query
	 */
	public Query ne();

	/**
	 * Not equal comparison operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	public Query ne(Query otherQuery);

	/**
	 * Not equal comparison operator.
	 * 
	 * @param numberValue
	 * @return Query
	 */
	public Query ne(Number numberValue);

	/**
	 * Not equal comparison operator.
	 * 
	 * @param booleanValue
	 * @return Query
	 */
	public Query ne(boolean booleanValue);

	/**
	 * Not equal comparison operator.
	 * 
	 * @param stringValue
	 * @return Query
	 */
	public Query ne(String stringValue);

	/**
	 * Not equal comparison operator.
	 * 
	 * @param dateValue
	 * @return Query
	 */
	public Query ne(Date dateValue);

	/****** Arithmetic Operators ******/

	/**
	 * Add operator.
	 * 
	 * @return Query
	 */
	public Query add();

	/**
	 * Add operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	public Query add(Query otherQuery);

	/**
	 * Add operator.
	 * 
	 * @param val
	 * @return Query
	 */
	public Query add(Number val);

	/**
	 * Subtract operator.
	 * 
	 * @return Query
	 */
	public Query sub();

	/**
	 * Subtract operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	public Query sub(Query otherQuery);

	/**
	 * Subtract operator.
	 * 
	 * @param val
	 * @return Query
	 */
	public Query sub(Number val);

	/**
	 * Multiply operator.
	 * 
	 * @return Query
	 */
	public Query mul();

	/**
	 * Multiply operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	public Query mul(Query otherQuery);

	/**
	 * Multiply operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	public Query mul(Number val);

	/**
	 * Divide operator.
	 * 
	 * @return Query
	 */
	public Query div();

	/**
	 * Divide operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	public Query div(Query otherQuery);

	/**
	 * Divide operator.
	 * 
	 * @param val
	 * @return Query
	 */
	public Query div(Number val);

	/**
	 * Reminder (or modulo) operator.
	 * 
	 * @return Query
	 */
	public Query mod();

	/**
	 * Reminder (or modulo) operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	public Query mod(Query otherQuery);

	/**
	 * Reminder (or modulo) operator.
	 * 
	 * @param val
	 * @return Query
	 */
	public Query mod(Number val);

	/****** Date Operators ******/

	/**
	 * The year component value of the parameter value.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	public Query year(Query otherQuery);

	/**
	 * The year component value of the parameter value.
	 * 
	 * @param field
	 * @return Query
	 */
	public Query year(String field);

	/**
	 * The month component value of the parameter value.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	public Query month(Query otherQuery);

	/**
	 * The month component value of the parameter value.
	 * 
	 * @param field
	 * @return Query
	 */
	public Query month(String field);

	/**
	 * The day component value of the parameter value.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	public Query day(Query otherQuery);

	/**
	 * The day component value of the parameter value.
	 * 
	 * @param field
	 * @return Query
	 */
	public Query day(String field);

	/**
	 * The hour component value of the parameter value.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	public Query hour(Query otherQuery);

	/**
	 * The hour component value of the parameter value.
	 * 
	 * @param field
	 * @return Query
	 */
	public Query hour(String field);

	/**
	 * The minute component value of the parameter value.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	public Query minute(Query otherQuery);

	/**
	 * The minute component value of the parameter value.
	 * 
	 * @param field
	 * @return Query
	 */
	public Query minute(String field);

	/**
	 * The second component value of the parameter value.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	public Query second(Query otherQuery);

	/**
	 * The second component value of the parameter value.
	 * 
	 * @param field
	 * @return Query
	 */
	public Query second(String field);

	/****** Math Functions ******/

	/**
	 * The largest integral value less than or equal to the parameter value.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	public Query floor(Query otherQuery);

	/**
	 * The smallest integral value greater than or equal to the parameter value.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	public Query ceiling(Query otherQuery);

	/**
	 * The nearest integral value to the parameter value.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	public Query round(Query otherQuery);

	/****** String Operators ******/

	/**
	 * String value with the contents of the parameter value converted to lower
	 * case.
	 * 
	 * @param exp
	 * @return Query
	 */
	public Query toLower(Query exp);

	/**
	 * String value with the contents of the parameter value converted to lower
	 * case.
	 * 
	 * @param field
	 * @return Query
	 */
	public Query toLower(String field);

	/**
	 * String value with the contents of the parameter value converted to upper
	 * case.
	 * 
	 * @param exp
	 * @return Query
	 */
	public Query toUpper(Query exp);

	/**
	 * String value with the contents of the parameter value converted to upper
	 * case.
	 * 
	 * @param field
	 * @return Query
	 */
	public Query toUpper(String field);

	/**
	 * The number of characters in the specified parameter value.
	 * 
	 * @param exp
	 * @return Query
	 */
	public Query length(Query exp);

	/**
	 * The number of characters in the specified parameter value.
	 * 
	 * @param field
	 * @return Query
	 */
	public Query length(String field);

	/**
	 * String value with the contents of the parameter value with all leading
	 * and trailing white-space characters removed.
	 * 
	 * @param exp
	 * @return Query
	 */
	public Query trim(Query exp);

	/**
	 * String value with the contents of the parameter value with all leading
	 * and trailing white-space characters removed.
	 * 
	 * @param field
	 * @return Query
	 */
	public Query trim(String field);

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
	public Query startsWith(Query field, Query start);

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
	public Query startsWith(String field, String start);

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
	public Query endsWith(Query field, Query end);

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
	public Query endsWith(String field, String end);

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
	public Query subStringOf(Query str1, Query str2);

	/**
	 * Whether the string parameter occurs in the field
	 * 
	 * @param str2
	 *            String to search
	 * @param field
	 *            Field to search in
	 * @return Query
	 */
	public Query subStringOf(String str, String field);

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
	public Query concat(Query str1, Query str2);

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
	public Query concat(Query str1, String str2);

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
	public Query indexOf(Query haystack, Query needle);

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
	public Query indexOf(String field, String needle);

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
	public Query subString(Query str, Query pos);

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
	public Query subString(String field, int pos);

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
	public Query subString(Query str, Query pos, Query length);

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
	public Query subString(String field, int pos, int length);

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
	public Query replace(Query str, Query find, Query replace);

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
	public Query replace(String field, String find, String replace);
}