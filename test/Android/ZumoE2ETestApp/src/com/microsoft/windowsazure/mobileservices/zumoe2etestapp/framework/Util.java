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
package com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework;

import java.text.SimpleDateFormat;
import java.util.Calendar;
import java.util.Date;
import java.util.GregorianCalendar;
import java.util.Hashtable;
import java.util.List;
import java.util.Locale;
import java.util.Map.Entry;
import java.util.Random;
import java.util.Set;
import java.util.TimeZone;

import org.apache.http.Header;

import com.google.gson.JsonArray;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonPrimitive;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.ServiceFilterResponse;

public class Util {

	public final static String LogTimeFormat = "yyyy-MM-dd HH:mm:ss'.'SSS";
	private final static Hashtable<String, String> globalTestParameters = new Hashtable<String, String>();

	public static Hashtable<String, String> getGlobalTestParameters() {
		return globalTestParameters;
	}

	public static String createComplexRandomString(Random rndGen, int size) {
		if (rndGen.nextInt(3) > 0) {
			return createSimpleRandomString(rndGen, size);
		} else {
			return createSimpleRandomString(rndGen, size, ' ', 0xfffe);
		}
	}
	public static String createSimpleRandomString(Random rndGen, int size) {
		int minChar = ' ';
		int maxChar = '~';

		return createSimpleRandomString(rndGen, size, minChar, maxChar);
	}
	
	public static String createSimpleRandomString(Random rndGen, int size, int minChar, int maxChar) {
		StringBuilder sb = new StringBuilder();
		for (int i = 0; i < size; i++) {
			
			int charRand;
			char c;
			do {
				charRand = rndGen.nextInt(maxChar - minChar);
				c = (char) (minChar + charRand);
			} while (Character.isLowSurrogate(c) || Character.isHighSurrogate(c));
			
			sb.append(c);
		}

		return sb.toString();
	}

	public static <E> boolean compareLists(List<E> l1, List<E> l2) {
		return compareArrays(l1.toArray(), l2.toArray());
	}

	public static boolean compareArrays(Object[] arr1, Object[] arr2) {
		if (arr1 == null && arr2 == null) {
			return true;
		}

		if (arr1 == null || arr2 == null) {
			return false;
		}

		if (arr1.length != arr2.length) {
			return false;
		}

		for (int i = 0; i < arr1.length; i++) {
			Object o1 = arr1[i];
			Object o2 = arr2[i];

			if (!compare(o1, o2)) {
				return false;
			}
		}
		return true;
	}

	public static <E> String listToString(List<E> list) {
		return arrayToString(list.toArray());
	}

	public static String arrayToString(Object[] arr) {
		if (arr == null) {
			return "<<NULL>>";
		} else {
			StringBuilder sb = new StringBuilder();
			sb.append("[");

			for (int i = 0; i < arr.length; i++) {
				Object elem = arr[i];
				sb.append(elem.toString());

				if (i != arr.length - 1) {
					sb.append(", ");
				}
			}

			sb.append("]");

			return sb.toString();
		}
	}

	public static String dateToString(Date date) {
		return dateToString(date, "yyyy-MM-dd'T'HH:mm:ss'.'SSS'Z'");
	}
	
	public static String dateToString(Date date, String dateFormatStr) {
		if (date == null) {
			return "NULL";
		}
		SimpleDateFormat dateFormat = new SimpleDateFormat(dateFormatStr, Locale.getDefault());
		dateFormat.setTimeZone(TimeZone.getTimeZone("UTC"));

		String formatted = dateFormat.format(date);

		return formatted;
	}
	
	public static boolean compare(Object o1, Object o2) {
		if (o1 == null && o2 == null) {
			return true;
		}

		if (o1 == null || o2 == null) {
			return false;
		}

		return o1.equals(o2);
	}
	
	public static TestCase createSeparatorTest(String testName) {
		return new TestCase(testName) {

			@Override
			protected void executeTest(MobileServiceClient client,
					TestExecutionCallback callback) {
				TestResult testResult = new TestResult();
				testResult.setTestCase(this);
				testResult.setStatus(TestStatus.Passed);
				callback.onTestComplete(this, testResult);
			}
			
		}; 
	}

	public static boolean compareJson(JsonElement e1, JsonElement e2) {
		// NOTE: if every property defined in e1 is in e2, the objects are
		// considered equal.

		if (e1 == null && e2 == null) {
			return true;
		}

		if (e1 == null || e2 == null) {
			return false;
		}

		if (e1.getClass() != e2.getClass()) {
			return false;
		}

		if (e1 instanceof JsonPrimitive) {
			if (!e1.equals(e2)) {
				return false;
			}
		} else if (e1 instanceof JsonArray) {
			JsonArray a1 = (JsonArray) e1;
			JsonArray a2 = (JsonArray) e2;

			if (a1.size() != a2.size()) {
				return false;
			}

			for (int i = 0; i < a1.size(); i++) {
				if (!compareJson(a1.get(i), a2.get(i))) {
					return false;
				}
			}

		} else if (e1 instanceof JsonObject) {

			JsonObject o1 = (JsonObject) e1;
			JsonObject o2 = (JsonObject) e2;

			Set<Entry<String, JsonElement>> entrySet1 = o1.entrySet();

			for (Entry<String, JsonElement> entry : entrySet1) {
				if (entry.getKey().toLowerCase(Locale.getDefault()).equals("id")) {
					continue;
				}

				String propertyName1 = entry.getKey();
				String propertyName2 = null;
				for (Entry<String, JsonElement> entry2 : o2.entrySet()) {
					if (propertyName1.toLowerCase(Locale.getDefault()).equals(entry2.getKey().toLowerCase(Locale.getDefault()))) {
						propertyName2 = entry2.getKey();
					}
				}

				if (propertyName2 == null) {
					return false;
				}

				if (!compareJson(entry.getValue(), o2.get(propertyName2))) {
					return false;
				}
			}
		}

		return true;
	}
	
	public static Date getUTCNow() {
		return new GregorianCalendar(TimeZone.getTimeZone("utc")).getTime();
	}
	
	public static Date getUTCDate(int year, int month, int day) {

		return getUTCDate(year, month, day, 0, 0, 0);
	}

	public static Date getUTCDate(int year, int month, int day, int hour, int minute, int second) {
		GregorianCalendar calendar = new GregorianCalendar(TimeZone.getTimeZone("utc"));
		int dateMonth = month - 1;
		calendar.set(year, dateMonth, day, hour, minute, second);
		calendar.set(Calendar.MILLISECOND, 0);

		return calendar.getTime();
	}

	public static Calendar getUTCCalendar(Date date) {
		Calendar cal = Calendar.getInstance(TimeZone.getTimeZone("utc"), Locale.getDefault());
		cal.setTime(date);

		return cal;
	}
	
	public static boolean responseContainsHeader(ServiceFilterResponse response, String headerName) {
		for (Header header : response.getHeaders()) {
			if (header.getName().equals(headerName)) {
				return true;
			}
		}
		
		return false;
	}
	

	public static String getHeaderValue(ServiceFilterResponse response, String headerName) {
		for (Header header : response.getHeaders()) {
			if (header.getName().equals(headerName)) {
				return header.getValue();
			}
		}
		
		return null;
	}
}
