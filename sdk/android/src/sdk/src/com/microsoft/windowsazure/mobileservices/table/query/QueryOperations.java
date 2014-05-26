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

import com.microsoft.windowsazure.mobileservices.MobileServiceException;

/**
 * Class used to create query operations
 */
public class QueryOperations {

	/**
	 * Creates Query with the requested table name.
	 * 
	 * @param tabledName
	 *            The name of the table
	 * @return The Query
	 */
	public static Query tableName(String tableName) {
		if (tableName == null || tableName.trim().length() == 0) {
			throw new IllegalArgumentException("tableName cannot be null or empty");
		}

		Query query = new QueryBase();

		query.tableName(tableName);

		return query;
	}

	/**
	 * Creates Query representing a field
	 * 
	 * @param fieldName
	 *            The name of the field
	 * @return The Query
	 */
	public static Query field(String fieldName) {
		if (fieldName == null || fieldName.trim().length() == 0) {
			throw new IllegalArgumentException("fieldName cannot be null or empty");
		}

		Query query = new QueryBase();

		FieldNode fieldNode = new FieldNode();
		fieldNode.setFieldName(fieldName);

		query.setQueryNode(fieldNode);

		return query;
	}

	/**
	 * Creates a Query representing a numeric value
	 * 
	 * @param number
	 *            the number to represent
	 * @return the Query
	 */
	public static Query val(Number number) {
		Query query = new QueryBase();

		ConstantNode constantNode = new ConstantNode();
		constantNode.setValue(number);

		query.setQueryNode(constantNode);

		return query;
	}

	/**
	 * Creates a Query representing a boolean value
	 * 
	 * @param val
	 *            the boolean to represent
	 * @return the Query
	 */
	public static Query val(boolean val) {
		Query query = new QueryBase();

		ConstantNode constantNode = new ConstantNode();
		constantNode.setValue(val);

		query.setQueryNode(constantNode);

		return query;
	}

	/**
	 * Creates a Query representing a string value
	 * 
	 * @param s
	 *            the string to represent
	 * @return the Query
	 */
	public static Query val(String s) {
		Query query = new QueryBase();

		ConstantNode constantNode = new ConstantNode();
		constantNode.setValue(s);

		query.setQueryNode(constantNode);

		return query;
	}

	/**
	 * Creates a Query representing a date value
	 * 
	 * @param date
	 *            the date to represent
	 * @return the Query
	 */
	public static Query val(Date date) {
		Query query = new QueryBase();

		ConstantNode constantNode = new ConstantNode();
		constantNode.setValue(date);

		query.setQueryNode(constantNode);

		return query;
	}

	/**
	 * Group query as a single argument.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	public static Query query(Query otherQuery) {
		return unaryOperator(otherQuery, UnaryOperatorKind.Parenthesis);
	}

	/****** Logical Operators ******/

	/**
	 * Conditional and.
	 * 
	 * @return Query
	 */
	public static Query and() {
		return and(null);
	}

	/**
	 * Conditional and.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	public static Query and(Query otherQuery) {
		return binaryOperator(otherQuery, BinaryOperatorKind.And);
	}

	/**
	 * Conditional or.
	 * 
	 * @return Query
	 */
	public static Query or() {
		return or(null);
	}

	/**
	 * Conditional or.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	public static Query or(Query otherQuery) {
		return binaryOperator(otherQuery, BinaryOperatorKind.Or);
	}

	/**
	 * Logical not.
	 * 
	 * @return Query
	 */
	public static Query not() {
		return not(null);
	}

	/**
	 * Logical not.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	public static Query not(Query otherQuery) {
		return unaryOperator(otherQuery, UnaryOperatorKind.Not);
	}

	/**
	 * Logical not.
	 * 
	 * @param booleanValue
	 * @return Query
	 */
	public static Query not(boolean booleanValue) {

		return (Query) not(val(booleanValue));
	}

	/****** Comparison Operators ******/

	/**
	 * Greater than or equal comparison operator.
	 * 
	 * @return Query
	 */
	public static Query ge() {
		Query nullQuery = null;
		return ge(nullQuery);
	}

	/**
	 * Greater than or equal comparison operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	public static Query ge(Query otherQuery) {
		return binaryOperator(otherQuery, BinaryOperatorKind.Ge);
	}

	/**
	 * Greater than or equal comparison operator.
	 * 
	 * @param numberValue
	 * @return Query
	 */
	public static Query ge(Number numberValue) {
		return ge(QueryOperations.val(numberValue));
	}

	/**
	 * Greater than or equal comparison operator.
	 * 
	 * @param dateValue
	 * @return Query
	 */
	public static Query ge(Date dateValue) {
		return ge(QueryOperations.val(dateValue));
	}

	/**
	 * Less than or equal comparison operator.
	 * 
	 * @return Query
	 */
	public static Query le() {
		Query nullQuery = null;
		return le(nullQuery);
	}

	/**
	 * Less than or equal comparison operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	public static Query le(Query otherQuery) {
		return binaryOperator(otherQuery, BinaryOperatorKind.Le);
	}

	/**
	 * Less than or equal comparison operator.
	 * 
	 * @param numberValue
	 * @return Query
	 */
	public static Query le(Number numberValue) {
		return le(QueryOperations.val(numberValue));
	}

	/**
	 * Less than or equal comparison operator.
	 * 
	 * @param dateValue
	 * @return Query
	 */
	public static Query le(Date dateValue) {
		return le(QueryOperations.val(dateValue));
	}

	/**
	 * Greater than comparison operator.
	 * 
	 * @return Query
	 */
	public static Query gt() {
		Query nullQuery = null;
		return gt(nullQuery);
	}

	/**
	 * Greater than comparison operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	public static Query gt(Query otherQuery) {
		return binaryOperator(otherQuery, BinaryOperatorKind.Gt);
	}

	/**
	 * Greater than comparison operator.
	 * 
	 * @param numberValue
	 * @return Query
	 */
	public static Query gt(Number numberValue) {
		return gt(QueryOperations.val(numberValue));
	}

	/**
	 * Greater than comparison operator.
	 * 
	 * @param dateValue
	 * @return Query
	 */
	public static Query gt(Date dateValue) {
		return gt(QueryOperations.val(dateValue));
	}

	/**
	 * Less than comparison operator.
	 * 
	 * @return Query
	 */
	public static Query lt() {
		Query nullQuery = null;
		return lt(nullQuery);
	}

	/**
	 * Less than comparison operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	public static Query lt(Query otherQuery) {
		return binaryOperator(otherQuery, BinaryOperatorKind.Lt);
	}

	/**
	 * Less than comparison operator.
	 * 
	 * @param numberValue
	 * @return Query
	 */
	public static Query lt(Number numberValue) {
		return lt(QueryOperations.val(numberValue));
	}

	/**
	 * Less than comparison operator.
	 * 
	 * @param dateValue
	 * @return Query
	 */
	public static Query lt(Date dateValue) {
		return lt(QueryOperations.val(dateValue));
	}

	/**
	 * Equal comparison operator.
	 * 
	 * @return Query
	 */
	public static Query eq() {
		Query nullQuery = null;
		return eq(nullQuery);
	}

	/**
	 * Equal comparison operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	public static Query eq(Query otherQuery) {
		return binaryOperator(otherQuery, BinaryOperatorKind.Eq);
	}

	/**
	 * Equal comparison operator.
	 * 
	 * @param numberValue
	 * @return Query
	 */
	public static Query eq(Number numberValue) {
		return eq(QueryOperations.val(numberValue));
	}

	/**
	 * Equal comparison operator.
	 * 
	 * @param booleanValue
	 * @return Query
	 */
	public static Query eq(boolean booleanValue) {
		return eq(QueryOperations.val(booleanValue));
	}

	/**
	 * Equal comparison operator.
	 * 
	 * @param stringValue
	 * @return Query
	 */
	public static Query eq(String stringValue) {
		return eq(QueryOperations.val(stringValue));
	}

	/**
	 * Equal comparison operator.
	 * 
	 * @param dateValue
	 * @return Query
	 */
	public static Query eq(Date dateValue) {
		return eq(QueryOperations.val(dateValue));
	}

	/**
	 * Not equal comparison operator.
	 * 
	 * @return Query
	 */
	public static Query ne() {
		Query nullQuery = null;
		return ne(nullQuery);
	}

	/**
	 * Not equal comparison operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	public static Query ne(Query otherQuery) {
		return binaryOperator(otherQuery, BinaryOperatorKind.Ne);
	}

	/**
	 * Not equal comparison operator.
	 * 
	 * @param numberValue
	 * @return Query
	 */
	public static Query ne(Number numberValue) {
		return ne(QueryOperations.val(numberValue));
	}

	/**
	 * Not equal comparison operator.
	 * 
	 * @param booleanValue
	 * @return Query
	 */
	public static Query ne(boolean booleanValue) {
		return ne(QueryOperations.val(booleanValue));
	}

	/**
	 * Not equal comparison operator.
	 * 
	 * @param stringValue
	 * @return Query
	 */
	public static Query ne(String stringValue) {
		return ne(QueryOperations.val(stringValue));
	}

	/**
	 * Not equal comparison operator.
	 * 
	 * @param dateValue
	 * @return Query
	 */
	public static Query ne(Date dateValue) {
		return ne(QueryOperations.val(dateValue));
	}

	/****** Arithmetic Operators ******/

	/**
	 * Add operator.
	 * 
	 * @return Query
	 */
	public static Query add() {
		Query nullQuery = null;
		return add(nullQuery);
	}

	/**
	 * Add operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	public static Query add(Query otherQuery) {
		return binaryOperator(otherQuery, BinaryOperatorKind.Add);
	}

	/**
	 * Add operator.
	 * 
	 * @param val
	 * @return Query
	 */
	public static Query add(Number val) {
		return add(val(val));
	}

	/**
	 * Subtract operator.
	 * 
	 * @return Query
	 */
	public static Query sub() {
		Query nullQuery = null;
		return sub(nullQuery);
	}

	/**
	 * Subtract operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	public static Query sub(Query otherQuery) {
		return binaryOperator(otherQuery, BinaryOperatorKind.Sub);
	}

	/**
	 * Subtract operator.
	 * 
	 * @param val
	 * @return Query
	 */
	public static Query sub(Number val) {
		return sub(val(val));
	}

	/**
	 * Multiply operator.
	 * 
	 * @return Query
	 */
	public static Query mul() {
		Query nullQuery = null;
		return mul(nullQuery);
	}

	/**
	 * Multiply operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	public static Query mul(Query otherQuery) {
		return binaryOperator(otherQuery, BinaryOperatorKind.Mul);
	}

	/**
	 * Multiply operator.
	 * 
	 * @param val
	 * @return Query
	 */
	public static Query mul(Number val) {
		return mul(val(val));
	}

	/**
	 * Divide operator.
	 * 
	 * @return Query
	 */
	public static Query div() {
		Query nullQuery = null;
		return div(nullQuery);
	}

	/**
	 * Divide operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	public static Query div(Query otherQuery) {
		return binaryOperator(otherQuery, BinaryOperatorKind.Div);
	}

	/**
	 * Divide operator.
	 * 
	 * @param val
	 * @return Query
	 */
	public static Query div(Number val) {
		return div(val(val));
	}

	/**
	 * Reminder (or modulo) operator.
	 * 
	 * @return Query
	 */
	public static Query mod() {
		Query nullQuery = null;
		return mod(nullQuery);
	}

	/**
	 * Reminder (or modulo) operator.
	 * 
	 * @param otherQuery
	 * @return Query
	 */
	public static Query mod(Query otherQuery) {
		return binaryOperator(otherQuery, BinaryOperatorKind.Mod);
	}

	/**
	 * Reminder (or modulo) operator.
	 * 
	 * @param val
	 * @return Query
	 */
	public static Query mod(Number val) {
		return mod(val(val));
	}

	/****** Date Functions ******/

	/**
	 * The year component value of the parameter value.
	 * 
	 * @param exp
	 * @return Query
	 */
	public static Query year(Query exp) {
		return function(FunctionCallKind.Year, exp);
	}

	/**
	 * The year component value of the parameter value.
	 * 
	 * @param field
	 * @return Query
	 */
	public static Query year(String field) {
		return function(FunctionCallKind.Year, field(field));
	}

	/**
	 * The month component value of the parameter value.
	 * 
	 * @param exp
	 * @return Query
	 */
	public static Query month(Query exp) {
		return function(FunctionCallKind.Month, exp);
	}

	/**
	 * The month component value of the parameter value.
	 * 
	 * @param field
	 * @return Query
	 */
	public static Query month(String field) {
		return function(FunctionCallKind.Month, field(field));
	}

	/**
	 * The day component value of the parameter value.
	 * 
	 * @param exp
	 * @return Query
	 */
	public static Query day(Query exp) {
		return function(FunctionCallKind.Day, exp);
	}

	/**
	 * The day component value of the parameter value.
	 * 
	 * @param field
	 * @return Query
	 */
	public static Query day(String field) {
		return function(FunctionCallKind.Day, field(field));
	}

	/**
	 * The hour component value of the parameter value.
	 * 
	 * @param exp
	 * @return Query
	 */
	public static Query hour(Query exp) {
		return function(FunctionCallKind.Hour, exp);
	}

	/**
	 * The hour component value of the parameter value.
	 * 
	 * @param field
	 * @return Query
	 */
	public static Query hour(String field) {
		return function(FunctionCallKind.Hour, field(field));
	}

	/**
	 * The minute component value of the parameter value.
	 * 
	 * @param exp
	 * @return Query
	 */
	public static Query minute(Query exp) {
		return function(FunctionCallKind.Minute, exp);
	}

	/**
	 * The minute component value of the parameter value.
	 * 
	 * @param field
	 * @return Query
	 */
	public static Query minute(String field) {
		return function(FunctionCallKind.Minute, field(field));
	}

	/**
	 * The second component value of the parameter value.
	 * 
	 * @param exp
	 * @return Query
	 */
	public static Query second(Query exp) {
		return function(FunctionCallKind.Second, exp);
	}

	/**
	 * The second component value of the parameter value.
	 * 
	 * @param field
	 * @return Query
	 */
	public static Query second(String field) {
		return function(FunctionCallKind.Second, field(field));
	}

	/****** Math Functions ******/

	/**
	 * The largest integral value less than or equal to the parameter value.
	 * 
	 * @param exp
	 * @return Query
	 */
	public static Query floor(Query exp) {
		return function(FunctionCallKind.Floor, exp);
	}

	/**
	 * The smallest integral value greater than or equal to the parameter value.
	 * 
	 * @param exp
	 * @return Query
	 */
	public static Query ceiling(Query exp) {
		return function(FunctionCallKind.Ceiling, exp);
	}

	/**
	 * The nearest integral value to the parameter value.
	 * 
	 * @param exp
	 * @return Query
	 */
	public static Query round(Query exp) {
		return function(FunctionCallKind.Round, exp);
	}

	/****** String Functions ******/

	/**
	 * String value with the contents of the parameter value converted to lower
	 * case.
	 * 
	 * @param exp
	 * @return Query
	 */
	public static Query toLower(Query exp) {
		return function(FunctionCallKind.ToLower, exp);
	}

	/**
	 * String value with the contents of the parameter value converted to lower
	 * case.
	 * 
	 * @param field
	 * @return Query
	 */
	public static Query toLower(String field) {
		return toLower(field(field));
	}

	/**
	 * String value with the contents of the parameter value converted to upper
	 * case
	 * 
	 * @param exp
	 * @return Query
	 */
	public static Query toUpper(Query exp) {
		return function(FunctionCallKind.ToUpper, exp);
	}

	/**
	 * String value with the contents of the parameter value converted to upper
	 * case
	 * 
	 * @param field
	 * @return Query
	 */
	public static Query toUpper(String field) {
		return toUpper(field(field));
	}

	/**
	 * The number of characters in the specified parameter value.
	 * 
	 * @param exp
	 * @return Query
	 */
	public static Query length(Query exp) {
		return function(FunctionCallKind.Length, exp);
	}

	/**
	 * The number of characters in the specified parameter value.
	 * 
	 * @param field
	 * @return Query
	 */
	public static Query length(String field) {
		return length(field(field));
	}

	/**
	 * String value with the contents of the parameter value with all leading
	 * and trailing white-space characters removed.
	 * 
	 * @param exp
	 * @return Query
	 */
	public static Query trim(Query exp) {
		return function(FunctionCallKind.Trim, exp);
	}

	/**
	 * String value with the contents of the parameter value with all leading
	 * and trailing white-space characters removed.
	 * 
	 * @param exp
	 * @return Query
	 */
	public static Query trim(String field) {
		return trim(field(field));
	}

	/**
	 * Whether the beginning of the first parameter values matches the second
	 * parameter value.
	 * 
	 * @param field
	 *            The field to evaluate.
	 * @param start
	 *            Start value.
	 * @return Query
	 */
	public static Query startsWith(Query field, Query start) {
		return function(FunctionCallKind.StartsWith, field, start);
	}

	/**
	 * Whether the beginning of the first parameter values matches the second
	 * parameter value.
	 * 
	 * @param field
	 *            The field to evaluate.
	 * @param start
	 *            Start value.
	 * @return Query
	 */
	public static Query startsWith(String field, String start) {
		return startsWith(field(field), val(start));
	}

	/**
	 * Whether the end of the first parameter value matches the second parameter
	 * value.
	 * 
	 * @param field
	 *            The field to evaluate.
	 * @param end
	 *            End value.
	 * @return Query
	 */
	public static Query endsWith(Query field, Query end) {
		return function(FunctionCallKind.EndsWith, field, end);
	}

	/**
	 * Whether the end of the first parameter value matches the second parameter
	 * value.
	 * 
	 * @param field
	 *            The field to evaluate.
	 * @param end
	 *            End value.
	 * @return Query
	 */
	public static Query endsWith(String field, String end) {
		return endsWith(field(field), val(end));
	}

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
	public static Query subStringOf(Query str1, Query str2) {
		return function(FunctionCallKind.SubstringOf, str1, str2);
	}

	/**
	 * Whether the string parameter occurs in the field
	 * 
	 * @param str
	 *            String to search
	 * @param field
	 *            Field to search in
	 * @return Query
	 */
	public static Query subStringOf(String str, String field) {
		return subStringOf(val(str), field(field));
	}

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
	public static Query concat(Query str1, Query str2) {
		return function(FunctionCallKind.Concat, str1, str2);
	}

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
	public static Query indexOf(Query haystack, Query needle) {
		return function(FunctionCallKind.IndexOf, haystack, needle);
	}

	/**
	 * Index of the first occurrence of the second parameter value in the first
	 * parameter value or -1 otherwise.
	 * 
	 * @param field
	 *            Field to seach in
	 * @param str
	 *            Value to search for
	 * @return Query
	 */
	public static Query indexOf(String field, String str) {
		return indexOf(field(field), val(str));
	}

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
	public static Query subString(Query str, Query pos) {
		return function(FunctionCallKind.Substring, str, pos);
	}

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
	public static Query subString(String field, int pos) {
		return subString(field(field), val(pos));
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
	 * @return Query
	 */
	public static Query subString(Query str, Query pos, Query length) {
		return function(FunctionCallKind.Substring, str, pos, length);
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
	 * @return Query
	 */
	public static Query subString(String field, int pos, int length) {
		return subString(field(field), val(pos), val(length));
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
	 * @return Query
	 */
	public static Query replace(Query str, Query find, Query replace) {
		return function(FunctionCallKind.Replace, str, find, replace);
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
	 * @return Query
	 */
	public static Query replace(String field, String find, String replace) {
		return replace(field(field), val(find), val(replace));
	}

	/**
	 * Join the left and right queries, modifying the left query.
	 * 
	 * @param leftQuery
	 *            The destination query to be modified.
	 * @param rightQuery
	 *            The source query to be joined with the destination query.
	 * @throws MobileServiceException
	 */
	static void join(Query leftQuery, Query rightQuery) throws MobileServiceException {
		if (leftQuery == null) {
			throw new IllegalArgumentException("Left Mobile Service query cannot be null.");
		}

		if (rightQuery == null) {
			throw new IllegalArgumentException("Right Mobile Service query cannot be null.");
		}

		if (leftQuery.getQueryNode() == null) {
			leftQuery.setQueryNode(rightQuery.getQueryNode());
		} else if (rightQuery.getQueryNode() != null) {
			leftQuery.setQueryNode(merge(leftQuery.getQueryNode(), rightQuery.getQueryNode()));
		}
	}

	private static QueryNode merge(QueryNode leftNode, QueryNode rightNode) throws MobileServiceException {
		return leftNode.accept(new QueryNodeMerger(rightNode));
	}

	/**
	 * Creates a Query representing a binary operator
	 * 
	 * @param otherQuery
	 *            The query to operateWith
	 * @param operatorKind
	 *            The binary operator kind
	 * @return The Query representing an operation
	 */
	private static Query unaryOperator(Query otherQuery, UnaryOperatorKind operatorKind) {
		Query query = new QueryBase();

		UnaryOperatorNode unaryOperatorNode = new UnaryOperatorNode(operatorKind);

		if (otherQuery != null && otherQuery.getQueryNode() != null) {
			if (operatorKind != UnaryOperatorKind.Parenthesis) {
				UnaryOperatorNode parenthesisNode = new UnaryOperatorNode(UnaryOperatorKind.Parenthesis);
				parenthesisNode.setArgument(otherQuery.getQueryNode());

				unaryOperatorNode.setArgument(parenthesisNode);
			} else {
				unaryOperatorNode.setArgument(otherQuery.getQueryNode());
			}
		}

		query.setQueryNode(unaryOperatorNode);

		return query;
	}

	/**
	 * Creates a Query representing a binary operator
	 * 
	 * @param otherQuery
	 *            The query to operateWith
	 * @param operatorKind
	 *            The binary operator kind
	 * @return The Query representing an operation
	 */
	private static Query binaryOperator(Query otherQuery, BinaryOperatorKind operatorKind) {
		Query query = new QueryBase();

		BinaryOperatorNode binaryOperatorNode = new BinaryOperatorNode(operatorKind);

		if (otherQuery != null && otherQuery.getQueryNode() != null) {
			UnaryOperatorNode parenthesisNode = new UnaryOperatorNode(UnaryOperatorKind.Parenthesis);
			parenthesisNode.setArgument(otherQuery.getQueryNode());

			binaryOperatorNode.setRightArgument(parenthesisNode);
		}

		query.setQueryNode(binaryOperatorNode);

		return query;
	}

	/**
	 * Creates a Query representing a function call
	 * 
	 * @param functionName
	 *            The function name
	 * @param parameters
	 *            The function parameters
	 * @return The Query representing a function call
	 */
	private static Query function(FunctionCallKind operatorKind, Query... parameters) {

		Query query = new QueryBase();

		FunctionCallNode functionCallNode = new FunctionCallNode(operatorKind);

		for (Query p : parameters) {
			if (p.getQueryNode() != null) {
				functionCallNode.addArgument(p.getQueryNode());
			}
		}

		query.setQueryNode(functionCallNode);

		return query;
	}
}