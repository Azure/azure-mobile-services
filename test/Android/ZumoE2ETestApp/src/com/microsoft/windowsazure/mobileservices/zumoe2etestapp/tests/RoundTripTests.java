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
package com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests;

import java.security.InvalidParameterException;
import java.util.Calendar;
import java.util.Date;
import java.util.GregorianCalendar;
import java.util.Random;
import java.util.TimeZone;

import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceJsonTable;
import com.microsoft.windowsazure.mobileservices.MobileServiceTable;
import com.microsoft.windowsazure.mobileservices.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.TableJsonOperationCallback;
import com.microsoft.windowsazure.mobileservices.TableOperationCallback;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.ExpectedValueException;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestCase;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestExecutionCallback;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestGroup;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestResult;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestStatus;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.Util;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.types.ComplexType;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.types.ComplexType2;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.types.EnumType;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.types.RoundTripTableElement;

public class RoundTripTests extends TestGroup {

	protected static final String ROUND_TRIP_TABLE_NAME = "droidRoundTripTable";

	public RoundTripTests() {
		super("RoundTrip tests");

		Random rndGen = new Random();

		TestCase createFirstRecord = new TestCase() {

			@Override
			protected void executeTest(MobileServiceClient client,
					final TestExecutionCallback callback) {
				RoundTripTableElement element = new RoundTripTableElement(true);
				final TestCase test = this;
				client.getTable(ROUND_TRIP_TABLE_NAME, RoundTripTableElement.class).insert(element,
						new TableOperationCallback<RoundTripTableElement>() {

							@Override
							public void onCompleted(
									RoundTripTableElement entity,
									Exception exception,
									ServiceFilterResponse response) {
								TestResult result;
								if (exception == null) {
									result = new TestResult();
									result.setStatus(TestStatus.Passed);
								} else {
									result = createResultFromException(exception);
								}

								result.setTestCase(test);
								callback.onTestComplete(test, result);
							}

						});
			}
		};
		createFirstRecord.setName("Create first record");
		this.addTest(createFirstRecord);

		// typed tests
		this.addTest(createSimpleTypedRoundTripTest("String: Empty", "",
				String.class));
		// this.addTest(createSimpleTypedRoundTripTest("String: null", null,
		// String.class));
		this.addTest(createSimpleTypedRoundTripTest("String: random value",
				Util.createSimpleRandomString(rndGen, 10), String.class));
		this.addTest(createSimpleTypedRoundTripTest(
				"String: large (1000 characters)",
				Util.createSimpleRandomString(rndGen, 1000), String.class));
		this.addTest(createSimpleTypedRoundTripTest(
				"String: large (64k+1 characters)",
				Util.createSimpleRandomString(rndGen, 65537), String.class));

		this.addTest(createSimpleTypedRoundTripTest(
				"String: non-ASCII characters - Latin", "ãéìôü ÇñÑ",
				String.class));
		this.addTest(createSimpleTypedRoundTripTest(
				"String: non-ASCII characters - Arabic", "الكتاب على الطاولة",
				String.class));
		this.addTest(createSimpleTypedRoundTripTest(
				"String: non-ASCII characters - Chinese", "这本书在桌子上",
				String.class));
		this.addTest(createSimpleTypedRoundTripTest(
				"String: non-ASCII characters - Chinese 2", "⒈①Ⅻㄨㄩ 啊阿鼾齄 丂丄狚狛 狜狝﨨﨩 ˊˋ˙–〇 㐀㐁䶴䶵",
				String.class));
		this.addTest(createSimpleTypedRoundTripTest(
				"String: non-ASCII characters - Japanese", "本は机の上に",
				String.class));
		this.addTest(createSimpleTypedRoundTripTest(
				"String: non-ASCII characters - Hebrew", "הספר הוא על השולחן",
				String.class));

		Calendar calendar = Calendar.getInstance();
		Calendar calendarUTC = Calendar
				.getInstance(TimeZone.getTimeZone("utc"));

		GregorianCalendar minCalendar = new GregorianCalendar(
				TimeZone.getTimeZone("utc"));
		minCalendar.set(1, 1, 1);

		this.addTest(createSimpleTypedRoundTripTest("Date: now",
				calendar.getTime(), Date.class));
		this.addTest(createSimpleTypedRoundTripTest("Date: now (UTC)",
				calendarUTC.getTime(), Date.class));
		// this.addTest(createSimpleTypedRoundTripTest("Date: null", null,
		// Date.class));
		this.addTest(createSimpleTypedRoundTripTest("Date: min date",
				minCalendar.getTime(), Date.class));
		this.addTest(createSimpleTypedRoundTripTest(
				"Date: specific date, before unix 0", new GregorianCalendar(
						1901, 1, 1).getTime(), Date.class));
		this.addTest(createSimpleTypedRoundTripTest(
				"Date: specific date, after unix 0", new GregorianCalendar(
						2000, 12, 31).getTime(), Date.class));

		this.addTest(createSimpleTypedRoundTripTest("Bool: true", true,
				Boolean.class));
		this.addTest(createSimpleTypedRoundTripTest("Bool: false", false,
				Boolean.class));
		// this.addTest(createSimpleTypedRoundTripTest("Bool: null", null,
		// Boolean.class));

		this.addTest(createSimpleTypedRoundTripTest("Int: zero",
				Integer.valueOf(0), Integer.class));
		this.addTest(createSimpleTypedRoundTripTest("Int: MaxValue",
				Integer.valueOf(Integer.MAX_VALUE), Integer.class));
		this.addTest(createSimpleTypedRoundTripTest("Int: MinValue",
				Integer.valueOf(Integer.MIN_VALUE), Integer.class));

		this.addTest(createSimpleTypedRoundTripTest("Long: zero",
				Long.valueOf(0), Long.class));

		Long maxAllowedValue = 0x0020000000000000L;
		Long minAllowedValue = 0L;
		minAllowedValue = Long.valueOf(0xFFE0000000000000L);

		this.addTest(createSimpleTypedRoundTripTest("Long: max allowed",
				maxAllowedValue, Long.class));
		this.addTest(createSimpleTypedRoundTripTest("Long: min allowed",
				minAllowedValue, Long.class));
		Long largePositiveValue = maxAllowedValue - rndGen.nextInt(5000);
		Long largeNegativeValue = minAllowedValue + rndGen.nextInt(5000);
		this.addTest(createSimpleTypedRoundTripTest(
				"Long: large value, less than max allowed ("
						+ largePositiveValue + ")", largePositiveValue,
				Long.class));
		this.addTest(createSimpleTypedRoundTripTest(
				"Long: large negative value, more than min allowed ("
						+ largeNegativeValue + ")", largeNegativeValue,
				Long.class));

		this.addTest(createSimpleTypedRoundTripTestWithException(
				"(Neg) Long: more than max allowed", maxAllowedValue + 1,
				Long.class, InvalidParameterException.class));
		this.addTest(createSimpleTypedRoundTripTestWithException(
				"(Neg) Long: less than min allowed", minAllowedValue - 1,
				Long.class, InvalidParameterException.class));

		this.addTest(createSimpleTypedRoundTripTest(
				"Enum (with JSON converter): simple value", EnumType.Second,
				EnumType.class));

		this.addTest(createSimpleTypedRoundTripTest(
				"Complex type (custom table serialization): simple value",
				new ComplexType2(rndGen), ComplexType2.class));

		// this.addTest(createSimpleTypedRoundTripTest(
		// "Complex type (custom table serialization): null",
		// null,
		// ComplexType2.class));

		this.addTest(createSimpleTypedRoundTripTest(
				"Complex type (converter): empty array", new ComplexType[0],
				ComplexType[].class));

		ComplexType[] ctArray1 = new ComplexType[1];
		ctArray1[0] = new ComplexType(rndGen);

		this.addTest(createSimpleTypedRoundTripTest(
				"Complex type (converter): 1-element array", ctArray1,
				ComplexType[].class));

		ComplexType[] ctArray2 = new ComplexType[3];
		ctArray2[0] = new ComplexType(rndGen);
		ctArray2[1] = null;
		ctArray2[2] = new ComplexType(rndGen);

		this.addTest(createSimpleTypedRoundTripTest(
				"Complex type (converter): multi-element array", ctArray2,
				ComplexType[].class));

		// this.addTest(createSimpleTypedRoundTripTest(
		// "Complex type (converter): null array",
		// null,
		// ComplexType[].class));

		RoundTripTableElement element1 = new RoundTripTableElement();
		element1.id = 1;
		this.addTest(createSimpleTypedRoundTripTestWithException(
				"(Neg) Insert item with non-default id", element1,
				InvalidParameterException.class, false));

		// untyped tests
		this.addTest(createSimpleUntypedRoundTripTest("Untyped String: Empty",
				"", String.class));
		// this.addTest(createSimpleUntypedRoundTripTest("Untyped String: null",
		// null, String.class));
		this.addTest(createSimpleUntypedRoundTripTest(
				"Untyped String: random value",
				Util.createSimpleRandomString(rndGen, 10), String.class));
		this.addTest(createSimpleUntypedRoundTripTest(
				"Untyped String: large (1000 characters)",
				Util.createSimpleRandomString(rndGen, 1000), String.class));
		this.addTest(createSimpleUntypedRoundTripTest(
				"Untyped String: large (64k+1 characters)",
				Util.createSimpleRandomString(rndGen, 65537), String.class));

		this.addTest(createSimpleUntypedRoundTripTest(
				"Untyped String: non-ASCII characters - Latin", "ãéìôü ÇñÑ",
				String.class));
		this.addTest(createSimpleUntypedRoundTripTest(
				"Untyped String: non-ASCII characters - Arabic",
				"الكتاب على الطاولة", String.class));
		this.addTest(createSimpleUntypedRoundTripTest(
				"Untyped String: non-ASCII characters - Chinese", "这本书在桌子上",
				String.class));
		this.addTest(createSimpleUntypedRoundTripTest(
				"Untyped String: non-ASCII characters - Chinese 2", "⒈①Ⅻㄨㄩ 啊阿鼾齄 丂丄狚狛 狜狝﨨﨩 ˊˋ˙–〇 㐀㐁䶴䶵",
				String.class));
		this.addTest(createSimpleUntypedRoundTripTest(
				"Untyped String: non-ASCII characters - Japanese", "本は机の上に",
				String.class));
		this.addTest(createSimpleUntypedRoundTripTest(
				"Untyped String: non-ASCII characters - Hebrew",
				"הספר הוא על השולחן", String.class));

		this.addTest(createSimpleUntypedRoundTripTest("Untyped Date: now",
				Util.dateToString(calendar.getTime()), Date.class));
		this.addTest(createSimpleUntypedRoundTripTest(
				"Untyped Date: now (UTC)",
				Util.dateToString(calendarUTC.getTime()), Date.class));
		// this.addTest(createSimpleUntypedRoundTripTest("Untyped Date: null",
		// null, Date.class));
		this.addTest(createSimpleUntypedRoundTripTest("Untyped Date: min date",
				Util.dateToString(minCalendar.getTime()), Date.class));
		this.addTest(createSimpleUntypedRoundTripTest(
				"Untyped Date: specific date, before unix 0",
				Util.dateToString(new GregorianCalendar(1901, 1, 1).getTime()),
				Date.class));
		this.addTest(createSimpleUntypedRoundTripTest(
				"Untyped Date: specific date, after unix 0", Util
						.dateToString(new GregorianCalendar(2000, 12, 31)
								.getTime()), Date.class));

		this.addTest(createSimpleUntypedRoundTripTest("Untyped Bool: true",
				true, Boolean.class));
		this.addTest(createSimpleUntypedRoundTripTest("Untyped Bool: false",
				false, Boolean.class));
		// this.addTest(createSimpleUntypedRoundTripTest("Untyped Bool: null",
		// null, Boolean.class));

		this.addTest(createSimpleUntypedRoundTripTest("Untyped Int: zero",
				Integer.valueOf(0), Integer.class));
		this.addTest(createSimpleUntypedRoundTripTest("Untyped Int: MaxValue",
				Integer.valueOf(Integer.MAX_VALUE), Integer.class));
		this.addTest(createSimpleUntypedRoundTripTest("Untyped Int: MinValue",
				Integer.valueOf(Integer.MIN_VALUE), Integer.class));

		this.addTest(createSimpleUntypedRoundTripTest("Untyped Long: zero",
				Long.valueOf(0), Long.class));

		this.addTest(createSimpleUntypedRoundTripTest(
				"Untyped Long: max allowed", maxAllowedValue, Long.class));
		this.addTest(createSimpleUntypedRoundTripTest(
				"Untyped Long: min allowed", minAllowedValue, Long.class));
		this.addTest(createSimpleUntypedRoundTripTest(
				"Untyped Long: large value, less than max allowed ("
						+ largePositiveValue + ")", largePositiveValue,
				Long.class));
		this.addTest(createSimpleUntypedRoundTripTest(
				"Untyped Long: large negative value, more than min allowed ("
						+ largeNegativeValue + ")", largeNegativeValue,
				Long.class));

		this.addTest(createSimpleUntypedRoundTripTestWithException(
				"Untyped Long: more than max allowed",
				maxAllowedValue + 1, Long.class, null));
		this.addTest(createSimpleUntypedRoundTripTestWithException(
				"Untyped Long: less than min allowed",
				minAllowedValue - 1, Long.class, null));

		this.addTest(createSimpleUntypedRoundTripTest(
				"Untyped complex (object): simple value",
				"{\"complexType2\":{\"Name\":\"John Doe\",\"Age\":33,\"Friends\":[\"Jane Roe\", \"John Smith\"]}}"));

		this.addTest(createSimpleUntypedRoundTripTest(
				"Untyped complex (object): null", "{\"complexType2\":null}"));

		this.addTest(createSimpleUntypedRoundTripTest(
				"Untyped complex (array): simple value",
				"{\"complexType1\":[{\"Name\":\"Scooby\",\"Age\":10}, {\"Name\":\"Shaggy\",\"Age\":19}]}"));
		this.addTest(createSimpleUntypedRoundTripTest(
				"Untyped complex (array): empty array", "{\"complexType1\":[]}"));
		this.addTest(createSimpleUntypedRoundTripTest(
				"Untyped complex (array): null", "{\"complexType1\":null}"));

		this.addTest(createSimpleUntypedRoundTripTest(
				"Untyped complex (array): array with null elements",
				"{\"complexType1\":[{\"Name\":\"Scooby\",\"Age\":10}, null, {\"Name\":\"Shaggy\",\"Age\":19}]}"));

		this.addTest(createSimpleUntypedRoundTripTestWithException(
				"(Neg) Insert item with non-default 'id' property",
				"{\"id\":1,\"value\":2}", InvalidParameterException.class));
		this.addTest(createSimpleUntypedRoundTripTestWithException(
				"(Neg) Insert item with non-default 'ID' property",
				"{\"ID\":1,\"value\":2}", InvalidParameterException.class));
		this.addTest(createSimpleUntypedRoundTripTestWithException(
				"(Neg) Insert item with non-default 'Id' property",
				"{\"Id\":1,\"value\":2}", InvalidParameterException.class));

	}

	private TestCase createSimpleUntypedRoundTripTest(String testName,
			final Object val, final Class<?> elementClass) {
		return createSimpleUntypedRoundTripTestWithException(testName, val,
				elementClass, null);
	}

	private TestCase createSimpleUntypedRoundTripTest(String testName,
			String jsonObject) {
		return createSimpleUntypedRoundTripTestWithException(testName,
				jsonObject, null);
	}

	private TestCase createSimpleUntypedRoundTripTestWithException(
			String testName, final Object val, final Class<?> elementClass,
			Class<?> expectedExceptionClass) {
		String propertyName = null;
		if (elementClass == EnumType.class) {
			propertyName = "enumType";
		} else if (elementClass == String.class) {
			propertyName = "string1";
		} else if (elementClass == Date.class) {
			propertyName = "date1";
		} else if (elementClass == Boolean.class) {
			propertyName = "bool1";
		} else if (elementClass == Double.class) {
			propertyName = "double1";
		} else if (elementClass == Long.class) {
			propertyName = "long1";
		} else if (elementClass == Integer.class) {
			propertyName = "int1";
		}

		String propertyValue;
		if (val == null) {
			propertyValue = "null";
		} else if (val instanceof Number || val instanceof Boolean) {
			propertyValue = val.toString();
		} else {
			propertyValue = "\""
					+ val.toString().replace("\\", "\\\\")
							.replace("\"", "\\\"") + "\"";
		}

		String jsonString = String.format("{\"%s\": %s}", propertyName,
				propertyValue);

		return createSimpleUntypedRoundTripTestWithException(testName,
				jsonString, expectedExceptionClass);
	}

	private TestCase createSimpleUntypedRoundTripTestWithException(
			String testName, final String jsonString,
			final Class<?> expectedExceptionClass) {
		TestCase testCase = new TestCase() {

			@Override
			protected void executeTest(final MobileServiceClient client,
					final TestExecutionCallback callback) {
				final MobileServiceJsonTable table = client.getTable(ROUND_TRIP_TABLE_NAME);
				final TestResult result = new TestResult();
				result.setStatus(TestStatus.Passed);
				final TestCase test = this;

				final JsonObject json =  new JsonParser().parse(jsonString)
						.getAsJsonObject();
				
				log("insert item");
				table.insert(json, new TableJsonOperationCallback() {
					@Override
					public void onCompleted(final JsonObject jsonEntity,
							Exception exception, ServiceFilterResponse response) {
						if (exception == null) {
							int id = jsonEntity.get("id").getAsInt();
							log("lookup item " + id);
							table.lookUp(id,
									new TableJsonOperationCallback() {
										@Override
										public void onCompleted(
												JsonObject newJsonEntity,
												Exception exception,
												ServiceFilterResponse response) {
											if (exception == null) {
												log("verify items are equal");
												if (!Util.compareJson(json,
														newJsonEntity)) {
													createResultFromException(result, new ExpectedValueException(
															jsonEntity,
															newJsonEntity));
												}
											} else {
												createResultFromException(
														result, exception);
											}

											if (callback != null)
												callback.onTestComplete(test,
														result);
										}
									});
						} else {
							createResultFromException(result, exception);
							if (callback != null)
								callback.onTestComplete(test, result);
						}
					}
				});
			}
		};

		testCase.setExpectedExceptionClass(expectedExceptionClass);
		testCase.setName(testName);
		return testCase;
	}

	private TestCase createSimpleTypedRoundTripTest(String testName,
			final Object val, final Class<?> elementClass) {
		return createSimpleTypedRoundTripTestWithException(testName, val,
				elementClass, null);
	}

	private TestCase createSimpleTypedRoundTripTestWithException(
			String testName, final Object val, final Class<?> elementClass,
			Class<?> expectedExceptionClass) {
		final RoundTripTableElement element = new RoundTripTableElement();

		if (elementClass == EnumType.class) {
			element.enumType = (EnumType) val;
		} else if (elementClass == String.class) {
			element.string1 = (String) val;
		} else if (elementClass == Date.class) {
			element.date1 = (Date) val;
		} else if (elementClass == Boolean.class) {
			element.bool1 = (Boolean) val;
		} else if (elementClass == Double.class) {
			element.double1 = (Double) val;
		} else if (elementClass == Long.class) {
			element.long1 = (Long) val;
		} else if (elementClass == Integer.class) {
			element.int1 = (Integer) val;
		} else if (elementClass == ComplexType2.class) {
			element.complexType2 = (ComplexType2) val;
		} else if (elementClass == ComplexType[].class) {
			element.complexType1 = (ComplexType[]) val;
		}

		return createSimpleTypedRoundTripTestWithException(testName, element,
				expectedExceptionClass, true);
	}

	private TestCase createSimpleTypedRoundTripTestWithException(
			String testName, final RoundTripTableElement element,
			final Class<?> expectedExceptionClass, final boolean removeId) {
		TestCase testCase = new TestCase() {

			@Override
			protected void executeTest(final MobileServiceClient client,
					final TestExecutionCallback callback) {
				final MobileServiceTable<RoundTripTableElement> table = client
						.getTable(ROUND_TRIP_TABLE_NAME, RoundTripTableElement.class);
				final TestResult result = new TestResult();
				result.setStatus(TestStatus.Passed);
				final TestCase test = this;

				if (removeId) {
					element.id = null;
				}
				
				log("insert item");
				table.insert(element,
						new TableOperationCallback<RoundTripTableElement>() {
							@Override
							public void onCompleted(
									final RoundTripTableElement entity,
									Exception exception,
									ServiceFilterResponse response) {
								if (exception == null) {
									log("lookup item " + entity.id);
									table.lookUp(
											entity.id,
											new TableOperationCallback<RoundTripTableElement>() {
												@Override
												public void onCompleted(
														RoundTripTableElement newEntity,
														Exception exception,
														ServiceFilterResponse response) {
													if (exception == null) {
														entity.id = newEntity.id; // patch
																					// to
																					// make
																					// "equals"
																					// works
														log("verify items are equal");
														if (!Util.compare(
																entity,
																newEntity)) {
															result.setException(new ExpectedValueException(
																	entity,
																	newEntity));
															result.setStatus(TestStatus.Failed);
														}
													} else {
														createResultFromException(
																result,
																exception);
													}

													if (callback != null)
														callback.onTestComplete(
																test, result);
												}
											});
								} else {
									createResultFromException(result, exception);
									if (callback != null)
										callback.onTestComplete(test, result);
								}
							}

						});
			}
		};

		testCase.setExpectedExceptionClass(expectedExceptionClass);
		testCase.setName(testName);
		return testCase;
	}

}
