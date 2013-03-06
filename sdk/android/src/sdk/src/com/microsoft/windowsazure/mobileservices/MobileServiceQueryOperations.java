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

import java.security.InvalidParameterException;
import java.util.Date;
import java.util.Locale;

/**
 * Class used to create query operations
 */
public class MobileServiceQueryOperations {

	/**
	 * Creates a MobileServiceQuery<?> representing a function call
	 * 
	 * @param functionName
	 *            The function name
	 * @param parameters
	 *            The function parameters
	 * @return The MobileServiceQuery<?> representing a function call
	 */
	private static MobileServiceQuery<?> function(String functionName,
			MobileServiceQuery<?>... parameters) {

		MobileServiceQuery<Object> query = new MobileServiceQuery<Object>();

		query.setQueryText(functionName);

		for (MobileServiceQuery<?> p : parameters) {
			query.addInternalValue(p);
		}

		return query;
	}

	/**
	 * Creates a MobileServiceQuery<?> representing an operator
	 * 
	 * @param otherQuery
	 *            The query to operateWith
	 * @param operator
	 *            The operator
	 * @return The MobileServiceQuery<?> representing an operation
	 */
	private static MobileServiceQuery<?> simpleOperator(
			MobileServiceQuery<?> otherQuery, String operator) {
		MobileServiceQuery<Object> query = new MobileServiceQuery<Object>();

		query.setQueryText(operator + " ");

		if (otherQuery != null) {
			query.addInternalValue(otherQuery);
		}

		return query;
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

	/**
	 * Creates MobileServiceQuery<?> with an existing query as its only internal
	 * value
	 * 
	 * @param query
	 *            The query step to add
	 * @return The MobileServiceQuery
	 */
	public static MobileServiceQuery<?> query(MobileServiceQuery<?> subQuery) {
		MobileServiceQuery<Object> query = new MobileServiceQuery<Object>();

		query.addInternalValue(subQuery);

		return query;
	}

	/**
	 * Creates MobileServiceQuery<?> representing a field
	 * 
	 * @param fieldName
	 *            The name of the field
	 * @return The MobileServiceQuery
	 */
	public static MobileServiceQuery<?> field(String fieldName) {
		if (fieldName == null || fieldName.trim().length() == 0) {
			throw new InvalidParameterException(
					"fieldName cannot be null or empty");
		}

		MobileServiceQuery<Object> query = new MobileServiceQuery<Object>();

		query.setQueryText(fieldName);

		return query;
	}

	/**
	 * Creates a MobileServiceQuery<?> representing a numeric value
	 * 
	 * @param number
	 *            the number to represent
	 * @return the MobileServiceQuery
	 */
	public static MobileServiceQuery<?> val(Number number) {
		MobileServiceQuery<?> query = new MobileServiceQuery<Object>();

		if (number == null) {
			query.setQueryText("null");
		} else {
			query.setQueryText(number.toString());
		}

		return query;
	}

	/**
	 * Creates a MobileServiceQuery<?> representing a boolean value
	 * 
	 * @param val
	 *            the boolean to represent
	 * @return the MobileServiceQuery
	 */
	public static MobileServiceQuery<?> val(boolean val) {
		MobileServiceQuery<?> query = new MobileServiceQuery<Object>();

		query.setQueryText(Boolean.valueOf(val).toString()
				.toLowerCase(Locale.getDefault()));

		return query;
	}

	/**
	 * Creates a MobileServiceQuery<?> representing a string value
	 * 
	 * @param s
	 *            the string to represent
	 * @return the MobileServiceQuery
	 */
	public static MobileServiceQuery<?> val(String s) {

		MobileServiceQuery<?> query = new MobileServiceQuery<Object>();

		if (s == null) {
			query.setQueryText("null");
		} else {
			query.setQueryText("'" + sanitize(s) + "'");
		}

		return query;
	}

	/**
	 * Creates a MobileServiceQuery<?> representing a date value
	 * 
	 * @param date
	 *            the date to represent
	 * @return the MobileServiceQuery
	 */
	public static MobileServiceQuery<?> val(Date date) {

		MobileServiceQuery<?> query = new MobileServiceQuery<Object>();

		if (date == null) {
			query.setQueryText("null");
		} else {
			query.setQueryText("'" + sanitize(DateSerializer.serialize(date))
					+ "'");
		}

		return query;
	}

	/****** Logical Operators ******/

	/**
	 * Conditional and.
	 * 
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> and() {
		return and(null);
	}

	/**
	 * Conditional and.
	 * 
	 * @param otherQuery
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> and(MobileServiceQuery<?> otherQuery) {
		return simpleOperator(otherQuery, "and");
	}

	/**
	 * Conditional or.
	 * 
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> or() {
		return or(null);
	}

	/**
	 * Conditional or.
	 * 
	 * @param otherQuery
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> or(MobileServiceQuery<?> otherQuery) {
		return simpleOperator(otherQuery, "or");
	}

	/**
	 * Logical not.
	 * 
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> not() {
		return not(null);
	}

	/**
	 * Logical not.
	 * 
	 * @param otherQuery
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> not(MobileServiceQuery<?> otherQuery) {
		return simpleOperator(otherQuery, "not");
	}

	/**
	 * Logical not.
	 * 
	 * @param booleanValue
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> not(boolean booleanValue) {

		return (MobileServiceQuery<?>) not(val(booleanValue));
	}

	/****** Comparison Operators ******/

	/**
	 * Greater than or equal comparison operator.
	 * 
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> ge() {
		MobileServiceQuery<?> nullQuery = null;
		return ge(nullQuery);
	}

	/**
	 * Greater than or equal comparison operator.
	 * 
	 * @param otherQuery
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> ge(MobileServiceQuery<?> otherQuery) {
		return simpleOperator(otherQuery, "ge");
	}

	/**
	 * Greater than or equal comparison operator.
	 * 
	 * @param numberValue
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> ge(Number numberValue) {
		return ge(MobileServiceQueryOperations.val(numberValue));
	}

	/**
	 * Greater than or equal comparison operator.
	 * 
	 * @param dateValue
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> ge(Date dateValue) {
		return ge(MobileServiceQueryOperations.val(dateValue));
	}

	/**
	 * Less than or equal comparison operator.
	 * 
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> le() {
		MobileServiceQuery<?> nullQuery = null;
		return le(nullQuery);
	}

	/**
	 * Less than or equal comparison operator.
	 * 
	 * @param otherQuery
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> le(MobileServiceQuery<?> otherQuery) {
		return simpleOperator(otherQuery, "le");
	}

	/**
	 * Less than or equal comparison operator.
	 * 
	 * @param numberValue
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> le(Number numberValue) {
		return le(MobileServiceQueryOperations.val(numberValue));
	}

	/**
	 * Less than or equal comparison operator.
	 * 
	 * @param dateValue
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> le(Date dateValue) {
		return le(MobileServiceQueryOperations.val(dateValue));
	}

	/**
	 * Greater than comparison operator.
	 * 
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> gt() {
		MobileServiceQuery<?> nullQuery = null;
		return gt(nullQuery);
	}

	/**
	 * Greater than comparison operator.
	 * 
	 * @param otherQuery
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> gt(MobileServiceQuery<?> otherQuery) {
		return simpleOperator(otherQuery, "gt");
	}

	/**
	 * Greater than comparison operator.
	 * 
	 * @param numberValue
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> gt(Number numberValue) {
		return gt(MobileServiceQueryOperations.val(numberValue));
	}

	/**
	 * Greater than comparison operator.
	 * 
	 * @param dateValue
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> gt(Date dateValue) {
		return gt(MobileServiceQueryOperations.val(dateValue));
	}

	/**
	 * Less than comparison operator.
	 * 
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> lt() {
		MobileServiceQuery<?> nullQuery = null;
		return lt(nullQuery);
	}

	/**
	 * Less than comparison operator.
	 * 
	 * @param otherQuery
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> lt(MobileServiceQuery<?> otherQuery) {
		return simpleOperator(otherQuery, "lt");
	}

	/**
	 * Less than comparison operator.
	 * 
	 * @param numberValue
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> lt(Number numberValue) {
		return lt(MobileServiceQueryOperations.val(numberValue));
	}

	/**
	 * Less than comparison operator.
	 * 
	 * @param dateValue
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> lt(Date dateValue) {
		return lt(MobileServiceQueryOperations.val(dateValue));
	}

	/**
	 * Equal comparison operator.
	 * 
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> eq() {
		MobileServiceQuery<?> nullQuery = null;
		return eq(nullQuery);
	}

	/**
	 * Equal comparison operator.
	 * 
	 * @param otherQuery
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> eq(MobileServiceQuery<?> otherQuery) {
		return simpleOperator(otherQuery, "eq");
	}

	/**
	 * Equal comparison operator.
	 * 
	 * @param numberValue
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> eq(Number numberValue) {
		return eq(MobileServiceQueryOperations.val(numberValue));
	}

	/**
	 * Equal comparison operator.
	 * 
	 * @param booleanValue
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> eq(boolean booleanValue) {
		return eq(MobileServiceQueryOperations.val(booleanValue));
	}

	/**
	 * Equal comparison operator.
	 * 
	 * @param stringValue
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> eq(String stringValue) {
		return eq(MobileServiceQueryOperations.val(stringValue));
	}

	/**
	 * Equal comparison operator.
	 * 
	 * @param dateValue
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> eq(Date dateValue) {
		return eq(MobileServiceQueryOperations.val(dateValue));
	}

	/**
	 * Not equal comparison operator.
	 * 
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> ne() {
		MobileServiceQuery<?> nullQuery = null;
		return ne(nullQuery);
	}

	/**
	 * Not equal comparison operator.
	 * 
	 * @param otherQuery
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> ne(MobileServiceQuery<?> otherQuery) {
		return simpleOperator(otherQuery, "ne");
	}

	/**
	 * Not equal comparison operator.
	 * 
	 * @param numberValue
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> ne(Number numberValue) {
		return ne(MobileServiceQueryOperations.val(numberValue));
	}

	/**
	 * Not equal comparison operator.
	 * 
	 * @param booleanValue
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> ne(boolean booleanValue) {
		return ne(MobileServiceQueryOperations.val(booleanValue));
	}

	/**
	 * Not equal comparison operator.
	 * 
	 * @param stringValue
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> ne(String stringValue) {
		return ne(MobileServiceQueryOperations.val(stringValue));
	}

	/**
	 * Not equal comparison operator.
	 * 
	 * @param dateValue
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> ne(Date dateValue) {
		return ne(MobileServiceQueryOperations.val(dateValue));
	}

	/****** Arithmetic Operators ******/

	/**
	 * Add operator.
	 * 
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> add() {
		MobileServiceQuery<?> nullQuery = null;
		return add(nullQuery);
	}

	/**
	 * Add operator.
	 * 
	 * @param otherQuery
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> add(MobileServiceQuery<?> otherQuery) {
		return simpleOperator(otherQuery, "add");
	}

	/**
	 * Add operator.
	 * 
	 * @param val
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> add(Number val) {
		return add(val(val));
	}

	/**
	 * Subtract operator.
	 * 
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> sub() {
		MobileServiceQuery<?> nullQuery = null;
		return sub(nullQuery);
	}

	/**
	 * Subtract operator.
	 * 
	 * @param otherQuery
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> sub(MobileServiceQuery<?> otherQuery) {
		return simpleOperator(otherQuery, "sub");
	}

	/**
	 * Subtract operator.
	 * 
	 * @param val
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> sub(Number val) {
		return sub(val(val));
	}

	/**
	 * Multiply operator.
	 * 
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> mul() {
		MobileServiceQuery<?> nullQuery = null;
		return mul(nullQuery);
	}

	/**
	 * Multiply operator.
	 * 
	 * @param otherQuery
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> mul(MobileServiceQuery<?> otherQuery) {
		return simpleOperator(otherQuery, "mul");
	}

	/**
	 * Multiply operator.
	 * 
	 * @param val
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> mul(Number val) {
		return mul(val(val));
	}

	/**
	 * Divide operator.
	 * 
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> div() {
		MobileServiceQuery<?> nullQuery = null;
		return div(nullQuery);
	}

	/**
	 * Divide operator.
	 * 
	 * @param otherQuery
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> div(MobileServiceQuery<?> otherQuery) {
		return simpleOperator(otherQuery, "div");
	}

	/**
	 * Divide operator.
	 * 
	 * @param val
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> div(Number val) {
		return div(val(val));
	}

	/**
	 * Reminder (or modulo) operator.
	 * 
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> mod() {
		MobileServiceQuery<?> nullQuery = null;
		return mod(nullQuery);
	}

	/**
	 * Reminder (or modulo) operator.
	 * 
	 * @param otherQuery
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> mod(MobileServiceQuery<?> otherQuery) {
		return simpleOperator(otherQuery, "mod");
	}

	/**
	 * Reminder (or modulo) operator.
	 * 
	 * @param val
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> mod(Number val) {
		return mod(val(val));
	}

	/****** Date Functions ******/

	/**
	 * The year component value of the parameter value.
	 * 
	 * @param exp
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> year(MobileServiceQuery<?> exp) {
		return function("year", exp);
	}

	/**
	 * The year component value of the parameter value.
	 * 
	 * @param field
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> year(String field) {
		return function("year", field(field));
	}

	/**
	 * The month component value of the parameter value.
	 * 
	 * @param exp
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> month(MobileServiceQuery<?> exp) {
		return function("month", exp);
	}

	/**
	 * The month component value of the parameter value.
	 * 
	 * @param field
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> month(String field) {
		return function("month", field(field));
	}

	/**
	 * The day component value of the parameter value.
	 * 
	 * @param exp
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> day(MobileServiceQuery<?> exp) {
		return function("day", exp);
	}

	/**
	 * The day component value of the parameter value.
	 * 
	 * @param field
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> day(String field) {
		return function("day", field(field));
	}

	/**
	 * The hour component value of the parameter value.
	 * 
	 * @param exp
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> hour(MobileServiceQuery<?> exp) {
		return function("hour", exp);
	}

	/**
	 * The hour component value of the parameter value.
	 * 
	 * @param field
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> hour(String field) {
		return function("hour", field(field));
	}

	/**
	 * The minute component value of the parameter value.
	 * 
	 * @param exp
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> minute(MobileServiceQuery<?> exp) {
		return function("minute", exp);
	}

	/**
	 * The minute component value of the parameter value.
	 * 
	 * @param field
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> minute(String field) {
		return function("minute", field(field));
	}

	/**
	 * The second component value of the parameter value.
	 * 
	 * @param exp
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> second(MobileServiceQuery<?> exp) {
		return function("second", exp);
	}

	/**
	 * The second component value of the parameter value.
	 * 
	 * @param field
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> second(String field) {
		return function("second", field(field));
	}

	/****** Math Functions ******/

	/**
	 * The largest integral value less than or equal to the parameter value.
	 * 
	 * @param exp
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> floor(MobileServiceQuery<?> exp) {
		return function("floor", exp);
	}

	/**
	 * The smallest integral value greater than or equal to the parameter value.
	 * 
	 * @param exp
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> ceiling(MobileServiceQuery<?> exp) {
		return function("ceiling", exp);
	}

	/**
	 * The nearest integral value to the parameter value.
	 * 
	 * @param exp
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> round(MobileServiceQuery<?> exp) {
		return function("round", exp);
	}

	/****** String Functions ******/

	/**
	 * String value with the contents of the parameter value converted to lower
	 * case.
	 * 
	 * @param exp
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> toLower(MobileServiceQuery<?> exp) {
		return function("tolower", exp);
	}

	/**
	 * String value with the contents of the parameter value converted to lower
	 * case.
	 * 
	 * @param field
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> toLower(String field) {
		return toLower(field(field));
	}

	/**
	 * String value with the contents of the parameter value converted to upper
	 * case
	 * 
	 * @param exp
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> toUpper(MobileServiceQuery<?> exp) {
		return function("toupper", exp);
	}

	/**
	 * String value with the contents of the parameter value converted to upper
	 * case
	 * 
	 * @param field
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> toUpper(String field) {
		return toUpper(field(field));
	}

	/**
	 * The number of characters in the specified parameter value.
	 * 
	 * @param exp
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> length(MobileServiceQuery<?> exp) {
		return function("length", exp);
	}

	/**
	 * The number of characters in the specified parameter value.
	 * 
	 * @param field
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> length(String field) {
		return length(field(field));
	}

	/**
	 * String value with the contents of the parameter value with all leading
	 * and trailing white-space characters removed.
	 * 
	 * @param exp
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> trim(MobileServiceQuery<?> exp) {
		return function("trim", exp);
	}

	/**
	 * String value with the contents of the parameter value with all leading
	 * and trailing white-space characters removed.
	 * 
	 * @param exp
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> trim(String field) {
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
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> startsWith(MobileServiceQuery<?> field,
			MobileServiceQuery<?> start) {
		return function("startswith", field, start);
	}

	/**
	 * Whether the beginning of the first parameter values matches the second
	 * parameter value.
	 * 
	 * @param field
	 *            The field to evaluate.
	 * @param start
	 *            Start value.
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> startsWith(String field, String start) {
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
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> endsWith(MobileServiceQuery<?> field,
			MobileServiceQuery<?> end) {
		return function("endswith", field, end);
	}

	/**
	 * Whether the end of the first parameter value matches the second parameter
	 * value.
	 * 
	 * @param field
	 *            The field to evaluate.
	 * @param end
	 *            End value.
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> endsWith(String field, String end) {
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
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> subStringOf(MobileServiceQuery<?> str1,
			MobileServiceQuery<?> str2) {
		return function("substringof", str1, str2);
	}

	/**
	 * Whether the string parameter occurs in the field
	 * 
	 * @param str
	 *            String to search
	 * @param field
	 *            Field to search in
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> subStringOf(String str, String field) {
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
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> concat(MobileServiceQuery<?> str1,
			MobileServiceQuery<?> str2) {
		return function("concat", str1, str2);
	}

	/**
	 * Index of the first occurrence of the second parameter value in the first
	 * parameter value or -1 otherwise.
	 * 
	 * @param haystack
	 *            String content
	 * @param needle
	 *            Value to search for
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> indexOf(MobileServiceQuery<?> haystack,
			MobileServiceQuery<?> needle) {
		return function("indexof", haystack, needle);
	}

	/**
	 * Index of the first occurrence of the second parameter value in the first
	 * parameter value or -1 otherwise.
	 * 
	 * @param field
	 *            Field to seach in
	 * @param str
	 *            Value to search for
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> indexOf(String field, String str) {
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
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> subString(MobileServiceQuery<?> str,
			MobileServiceQuery<?> pos) {
		return function("substring", str, pos);
	}

	/**
	 * String value starting at the character index specified by the second
	 * parameter value in the first parameter string value.
	 * 
	 * @param field
	 *            Field to scan
	 * @param pos
	 *            Starting position
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> subString(String field, int pos) {
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
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> subString(MobileServiceQuery<?> str,
			MobileServiceQuery<?> pos, MobileServiceQuery<?> length) {
		return function("substring", str, pos, length);
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
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> subString(String field, int pos,
			int length) {
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
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> replace(MobileServiceQuery<?> str,
			MobileServiceQuery<?> find, MobileServiceQuery<?> replace) {
		return function("replace", str, find, replace);
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
	 * @return MobileServiceQuery
	 */
	public static MobileServiceQuery<?> replace(String field, String find,
			String replace) {
		return replace(field(field), val(find), val(replace));
	}
}
