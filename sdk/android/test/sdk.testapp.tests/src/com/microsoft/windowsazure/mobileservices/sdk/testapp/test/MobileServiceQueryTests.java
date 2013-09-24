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
package com.microsoft.windowsazure.mobileservices.sdk.testapp.test;

import static com.microsoft.windowsazure.mobileservices.MobileServiceQueryOperations.*;

import java.io.UnsupportedEncodingException;
import java.net.URLEncoder;
import java.util.Calendar;
import java.util.Date;
import java.util.GregorianCalendar;
import java.util.TimeZone;

import android.test.InstrumentationTestCase;

import com.google.gson.GsonBuilder;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceJsonTable;
import com.microsoft.windowsazure.mobileservices.MobileServiceQuery;
import com.microsoft.windowsazure.mobileservices.QueryOrder;

public class MobileServiceQueryTests extends InstrumentationTestCase {

	String appUrl = "";
	String appKey = "";
	GsonBuilder gsonBuilder;
	MobileServiceClient client;
	MobileServiceJsonTable table;

	protected void setUp() throws Exception {
		appUrl = "http://myapp.com/";
		appKey = "qwerty";
		gsonBuilder = new GsonBuilder();
		client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
		table = client.getTable("TableName");
		super.setUp();
	}

	protected void tearDown() throws Exception {
		super.tearDown();
	}

	public void testReturnAllRows() throws Throwable {

		// Create empty query
		MobileServiceQuery<?> query = table.where();

		// Assert
		assertEquals("", query.getRowSetModifiers());
		assertEquals("", query.toString());
	}

	public void testSelectSpecificField() throws Throwable {

		// Create query
		MobileServiceQuery<?> query = table.select("Id", "Name");

		// Assert
		String expectedModifiers = "&$select=Id" + encodeString(",") + "Name";
		assertEquals(expectedModifiers, query.getRowSetModifiers());
		assertEquals("", query.toString());
	}

	public void testUserDefinedParameters() throws Throwable {

		// Create query
		MobileServiceQuery<?> query = table.parameter("firstname", "john").parameter("lastname", null);

		// Assert
		String expectedModifiers = "&firstname=john&lastname=null";
		assertEquals(expectedModifiers, query.getRowSetModifiers());
		assertEquals("", query.toString());
	}

	public void testOrderByAscending() throws Throwable {

		// Create query
		MobileServiceQuery<?> query = table.orderBy("Name", QueryOrder.Ascending);

		// Assert
		String expectedModifiers = "&$orderby=Name" + encodeString(" ") + "asc";
		assertEquals(expectedModifiers, query.getRowSetModifiers());
		assertEquals("", query.toString());
	}

	public void testOrderByDescending() throws Throwable {

		// Create query
		MobileServiceQuery<?> query = table.orderBy("Name", QueryOrder.Descending);

		// Assert
		String expectedModifiers = "&$orderby=Name" + encodeString(" desc");
		assertEquals(expectedModifiers, query.getRowSetModifiers());
		assertEquals("", query.toString());
	}

	public void testOrderByWithMultipleFields() throws Throwable {

		// Create query
		MobileServiceQuery<?> query = table.orderBy("Name", QueryOrder.Ascending).orderBy("Age", QueryOrder.Descending);

		// Assert
		String expectedModifiers = "&$orderby=Name" + encodeString(" asc,") + "Age" + encodeString(" desc");
		assertEquals(expectedModifiers, query.getRowSetModifiers());
		assertEquals("", query.toString());
	}

	public void testSkip() throws Throwable {

		// Create query
		MobileServiceQuery<?> query = table.skip(10);

		// Assert
		String expectedModifiers = "&$skip=10";
		assertEquals(expectedModifiers, query.getRowSetModifiers());
		assertEquals("", query.toString());
	}

	public void testTop() throws Throwable {

		// Create query
		MobileServiceQuery<?> query = table.top(5);

		// Assert
		String expectedModifiers = "&$top=5";
		assertEquals(expectedModifiers, query.getRowSetModifiers());
		assertEquals("", query.toString());
	}

	public void testSkipAndTop() throws Throwable {

		// Create query
		MobileServiceQuery<?> query = table.skip(10).top(3);

		// Assert
		String expectedModifiers = "&$top=3&$skip=10";
		assertEquals(expectedModifiers, query.getRowSetModifiers());
		assertEquals("", query.toString());
	}

	public void testGreaterThan() throws Throwable {

		// Create query
		MobileServiceQuery<?> query = table.where().field("age").gt().val(3);

		// Assert
		String expectedFilters = "age gt 3";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());

		// Create query
		query = table.where().field("age").gt(3);

		// Assert
		expectedFilters = "age gt (3)";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());
	}

	public void testGreaterThanOrEquals() throws Throwable {

		// Create query
		MobileServiceQuery<?> query = table.where().field("age").ge().val(3);

		// Assert
		String expectedFilters = "age ge 3";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());

		// Create query
		query = table.where().field("age").ge(3);

		// Assert
		expectedFilters = "age ge (3)";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());

	}

	public void testLessThan() throws Throwable {

		// Create query
		MobileServiceQuery<?> query = table.where().field("age").le().val(3);

		// Assert
		String expectedFilters = "age le 3";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());

		// Create query
		query = table.where().field("age").le(3);

		// Assert
		expectedFilters = "age le (3)";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());

	}

	public void testLessThanOrEquals() throws Throwable {

		// Create query
		MobileServiceQuery<?> query = table.where().field("age").le().val(3);

		// Assert
		String expectedFilters = "age le 3";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());

		// Create query
		query = table.where().field("age").le(3);

		// Assert
		expectedFilters = "age le (3)";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());

	}

	public void testEquals() throws Throwable {

		// Create query
		MobileServiceQuery<?> query = table.where().field("age").eq().val(3);

		// Assert
		String expectedFilters = "age eq 3";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());

		// Create query
		query = table.where().field("age").eq(3);

		// Assert
		expectedFilters = "age eq (3)";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());
	}

	public void testNotEquals() throws Throwable {

		// Create query
		MobileServiceQuery<?> query = table.where().field("age").ne().val(3);

		// Assert
		String expectedFilters = "age ne 3";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());

		// Create query
		query = table.where().field("age").ne(3);

		// Assert
		expectedFilters = "age ne (3)";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());
	}

	private static Date getUTCDate(int year, int month, int day, int hour, int minute, int second) {
		GregorianCalendar calendar = new GregorianCalendar(TimeZone.getTimeZone("utc"));
		int dateMonth = month - 1;
		calendar.set(year, dateMonth, day, hour, minute, second);
		calendar.set(Calendar.MILLISECOND, 0);

		return calendar.getTime();
	}

	public void testDate() throws Throwable {

		// Create query

		MobileServiceQuery<?> query = table.where().field("birthdate").eq().val(getUTCDate(1986, 6, 30, 0, 0, 0));

		// Assert
		String expectedFilters = "birthdate eq '1986-06-30T00:00:00.000Z'";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());

		// Create query

		query = table.where().field("birthdate").eq(getUTCDate(1986, 6, 30, 0, 0, 0));

		// Assert
		expectedFilters = "birthdate eq ('1986-06-30T00:00:00.000Z')";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());
	}

	public void testAnd() throws Throwable {

		// Create query
		MobileServiceQuery<?> query = table.where().field("age").eq().val(18).and().field("name").eq().val("John");

		// Assert
		String expectedFilters = "age eq 18 and name eq 'John'";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());

		// Create query
		query = table.where().field("age").eq().val(18).and(field("name").eq().val("John"));

		// Assert
		expectedFilters = "age eq 18 and (name eq 'John')";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());

	}

	public void testOr() throws Throwable {

		// Create query
		MobileServiceQuery<?> query = table.where().field("age").eq().val(18).or().field("name").eq().val("John");

		// Assert
		String expectedFilters = "age eq 18 or name eq 'John'";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());

		// Create query
		query = table.where().field("age").eq().val(18).or(field("name").eq().val("John").and().field("lastname").eq().val("Doe"));

		// Assert
		expectedFilters = "age eq 18 or (name eq 'John' and lastname eq 'Doe')";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());
	}

	public void testNot() throws Throwable {

		// Create query
		MobileServiceQuery<?> query = table.where().not(field("age").eq().val(15));

		// Assert
		String expectedFilters = "not (age eq 15)";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());

		// Create query
		query = table.where().not(field("age").eq(val(15)));

		// Assert
		expectedFilters = "not (age eq (15))";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());

	}

	public void testAdd() throws Throwable {

		// Create query
		MobileServiceQuery<?> query = table.where().field("age").add().val(2).eq().val(18);

		// Assert
		String expectedFilters = "age add 2 eq 18";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());

		// Create query
		query = table.where().field("age").add(val(2)).eq().val(18);

		// Assert
		expectedFilters = "age add (2) eq 18";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());
	}

	public void testSub() throws Throwable {

		// Create query
		MobileServiceQuery<?> query = table.where().field("age").sub().val(2).eq().val(16);

		// Assert
		String expectedFilters = "age sub 2 eq 16";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());

		// Create query
		query = table.where().field("age").sub(val(2)).eq().val(16);

		// Assert
		expectedFilters = "age sub (2) eq 16";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());
	}

	public void testMul() throws Throwable {

		// Create query
		MobileServiceQuery<?> query = table.where().field("age").mul().val(2).eq().val(16);

		// Assert
		String expectedFilters = "age mul 2 eq 16";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());

		// Create query
		query = table.where().field("age").mul(val(2)).eq().val(16);

		// Assert
		expectedFilters = "age mul (2) eq 16";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());

	}

	public void testDiv() throws Throwable {

		// Create query
		MobileServiceQuery<?> query = table.where().field("age").div().val(2).eq().val(8);

		// Assert
		String expectedFilters = "age div 2 eq 8";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());

		// Create query
		query = table.where().field("age").div(val(2)).eq().val(8);

		// Assert
		expectedFilters = "age div (2) eq 8";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());

	}

	public void testMod() throws Throwable {

		// Create query
		MobileServiceQuery<?> query = table.where().field("price").mod().val(2).eq().val(1);

		// Assert
		String expectedFilters = "price mod 2 eq 1";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());

		// Create query
		query = table.where().field("price").mod(val(2)).eq().val(1);

		// Assert
		expectedFilters = "price mod (2) eq 1";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());
	}

	public void testYear() throws Throwable {

		// Create query
		MobileServiceQuery<?> query = table.where().year(field("date")).eq().val(2013);

		// Assert
		String expectedFilters = "year(date) eq 2013";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());
	}

	public void testMonth() throws Throwable {

		// Create query
		MobileServiceQuery<?> query = table.where().month(field("date")).eq().val(8);

		// Assert
		String expectedFilters = "month(date) eq 8";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());
	}

	public void testDay() throws Throwable {

		// Create query
		MobileServiceQuery<?> query = table.where().day(field("date")).eq().val(3);

		// Assert
		String expectedFilters = "day(date) eq 3";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());
	}

	public void testHour() throws Throwable {

		// Create query
		MobileServiceQuery<?> query = table.where().hour(field("date")).eq().val(10);

		// Assert
		String expectedFilters = "hour(date) eq 10";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());
	}

	public void testMinute() throws Throwable {

		// Create query
		MobileServiceQuery<?> query = table.where().minute(field("date")).eq().val(15);

		// Assert
		String expectedFilters = "minute(date) eq 15";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());
	}

	public void testSecond() throws Throwable {

		// Create query
		MobileServiceQuery<?> query = table.where().second(field("date")).eq().val(11);

		// Assert
		String expectedFilters = "second(date) eq 11";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());
	}

	public void testFloor() throws Throwable {

		// Create query
		MobileServiceQuery<?> query = table.where().floor(field("price")).gt().val(10);

		// Assert
		String expectedFilters = "floor(price) gt 10";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());
	}

	public void testCeiling() throws Throwable {

		// Create query
		MobileServiceQuery<?> query = table.where().ceiling(field("price")).gt().val(10);

		// Assert
		String expectedFilters = "ceiling(price) gt 10";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());
	}

	public void testRound() throws Throwable {

		// Create query
		MobileServiceQuery<?> query = table.where().round(field("price")).gt().val(5);

		// Assert
		String expectedFilters = "round(price) gt 5";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());
	}

	public void testStarstWith() throws Throwable {

		// Create query
		MobileServiceQuery<?> query = table.where().startsWith(field("Name"), val("Jo"));

		// Assert
		String expectedFilters = "startswith(Name,'Jo')";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());
	}

	public void testEndsWith() throws Throwable {

		// Create query
		MobileServiceQuery<?> query = table.where().endsWith(field("Name"), val("in"));

		// Assert
		String expectedFilters = "endswith(Name,'in')";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());
	}

	public void testSubstringOf() throws Throwable {

		// Create query
		MobileServiceQuery<?> query = table.where().subStringOf(field("FirstName"), field("LastName"));

		// Assert
		String expectedFilters = "substringof(FirstName,LastName)";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());
	}

	public void testConcat() throws Throwable {

		// Create query
		MobileServiceQuery<?> query = table.where().concat(field("FirstName"), field("LastName")).eq().val("JohnDoe");

		// Assert
		String expectedFilters = "concat(FirstName,LastName) eq 'JohnDoe'";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());
	}

	public void testIndexOf() throws Throwable {

		// Create query
		MobileServiceQuery<?> query = table.where().indexOf(field("Name"), val("do")).ne().val(-1);

		// Assert
		String expectedFilters = "indexof(Name,'do') ne -1";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());
	}

	public void testSubstring() throws Throwable {

		// Create query
		MobileServiceQuery<?> query = table.where().subString(field("ProductCode"), val(3)).eq().val("FOO");

		// Assert
		String expectedFilters = "substring(ProductCode,3) eq 'FOO'";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());

		// Create query
		query = table.where().subString(field("ProductCode"), val(1), val(2)).eq().val("FC");
		expectedFilters = "substring(ProductCode,1,2) eq 'FC'";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());
	}

	public void testReplace() throws Throwable {

		// Create query
		MobileServiceQuery<?> query = table.where().replace(field("Description"), val(" "), val("-")).eq().val("Code-1");

		// Assert
		String expectedFilters = "replace(Description,' ','-') eq 'Code-1'";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());
	}

	public void testToLower() throws Throwable {

		// Create query
		MobileServiceQuery<?> query = table.where().toLower(field("Description")).eq("code-1");

		// Assert
		String expectedFilters = "tolower(Description) eq ('code-1')";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());

		query = table.where().toLower("Description").eq("code-1");

		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());
	}

	public void testToUpper() throws Throwable {

		// Create query
		MobileServiceQuery<?> query = table.where().toUpper(field("Description")).eq("code-1");

		// Assert
		String expectedFilters = "toupper(Description) eq ('code-1')";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());

		query = table.where().toUpper("Description").eq("code-1");

		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());
	}

	public void testTrim() throws Throwable {

		// Create query
		MobileServiceQuery<?> query = table.where().trim(field("Description")).eq("code-1");

		// Assert
		String expectedFilters = "trim(Description) eq ('code-1')";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());

		query = table.where().trim("Description").eq("code-1");

		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());
	}

	public void testLength() throws Throwable {

		// Create query
		MobileServiceQuery<?> query = table.where().length(field("Description")).eq(5);

		// Assert
		String expectedFilters = "length(Description) eq (5)";
		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());

		query = table.where().length("Description").eq(5);

		assertEquals("", query.getRowSetModifiers());
		assertEquals(expectedFilters, query.toString());
	}

	public void testComplexQueries() throws Throwable {

		// Create query
		MobileServiceQuery<?> query = table.where().field("firstName").eq().val("John").and().field("age").gt().val(20).select("Id", "Name")
				.orderBy("Name", QueryOrder.Ascending).skip(5).top(3);
		// Asserts
		String expectedFilters = "firstName eq 'John' and age gt 20";
		assertEquals(expectedFilters, query.toString());
		String expectedModifiers = "&$top=3&$skip=5&$orderby=Name+asc&$select=Id%2CName";
		assertEquals(expectedModifiers, query.getRowSetModifiers());

		// Create query
		MobileServiceQuery<?> query2 = table.where(field("id").gt().val(1)).and(field("complete").eq().val(true));
		// Asserts
		assertEquals("(id gt 1) and (complete eq true)", query2.toString());

		// Create query
		MobileServiceQuery<?> query3 = table.where(field("id").gt().val(1)).and(field("age").eq().val(13).or().field("complete").eq().val(true));
		// Asserts
		assertEquals("(id gt 1) and (age eq 13 or complete eq true)", query3.toString());

		// Create query
		MobileServiceQuery<?> query4 = table.where().field("id").gt().val(1).and(field("age").eq().val(13).or().field("complete").eq().val(true));
		// Asserts
		assertEquals("id gt 1 and (age eq 13 or complete eq true)", query4.toString());
	}

	public void testComplexQueriesWithStringId() throws Throwable {

		// Create query
		MobileServiceQuery<?> query = table.where().field("firstName").eq().val("John").and().field("age").gt().val(20).select("Id", "Name")
				.orderBy("Name", QueryOrder.Ascending).skip(5).top(3);
		// Asserts
		String expectedFilters = "firstName eq 'John' and age gt 20";
		assertEquals(expectedFilters, query.toString());
		String expectedModifiers = "&$top=3&$skip=5&$orderby=Name+asc&$select=Id%2CName";
		assertEquals(expectedModifiers, query.getRowSetModifiers());

		// Create query
		MobileServiceQuery<?> query2 = table.where(field("id").gt().val("1")).and(field("complete").eq().val(true));
		// Asserts
		assertEquals("(id gt '1') and (complete eq true)", query2.toString());

		// Create query
		MobileServiceQuery<?> query3 = table.where(field("id").gt().val("1")).and(field("age").eq().val(13).or().field("complete").eq().val(true));
		// Asserts
		assertEquals("(id gt '1') and (age eq 13 or complete eq true)", query3.toString());

		// Create query
		MobileServiceQuery<?> query4 = table.where().field("id").eq().val("1").and(field("age").eq().val(13).or().field("complete").eq().val(true));
		// Asserts
		assertEquals("id eq '1' and (age eq 13 or complete eq true)", query4.toString());
	}

	private String encodeString(String s) {
		try {
			return URLEncoder.encode(s, "UTF-8");
		} catch (UnsupportedEncodingException e) {
			return "error";
		}
	}
}
