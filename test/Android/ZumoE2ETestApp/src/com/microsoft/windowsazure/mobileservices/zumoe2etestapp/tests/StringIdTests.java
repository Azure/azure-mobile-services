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

import static com.microsoft.windowsazure.mobileservices.MobileServiceQueryOperations.subString;

import java.util.ArrayList;
import java.util.List;
import java.util.UUID;

import android.util.Pair;

import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceJsonTable;
import com.microsoft.windowsazure.mobileservices.MobileServiceQuery;
import com.microsoft.windowsazure.mobileservices.MobileServiceTable;
import com.microsoft.windowsazure.mobileservices.QueryOrder;
import com.microsoft.windowsazure.mobileservices.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.TableJsonOperationCallback;
import com.microsoft.windowsazure.mobileservices.TableOperationCallback;
import com.microsoft.windowsazure.mobileservices.TableQueryCallback;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.ExpectedValueException;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestCase;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestExecutionCallback;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestGroup;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestResult;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestStatus;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.Util;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.types.FilterResult;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.types.ListFilter;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.types.SimpleFilter;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.types.StringIdTableItem;

public class StringIdTests extends TestGroup {

	protected static final String STRING_ID_TABLE_NAME = "droidStringIdTable";

	public StringIdTests() {
		super("String Id tests");
		
		List<StringIdTableItem> allItems = loadInitialItems();

		this.addTest(createQueryTest("Data begining with 0-7", allItems, subString("data", 0).ge().val("0").and().subString("data", 0).lt().val("8"),
				new SimpleFilter<StringIdTableItem>() {

					@Override
					protected boolean criteria(StringIdTableItem element) {
						return element.data.substring(0, 1).compareToIgnoreCase("0") >= 0 && element.data.substring(0, 1).compareToIgnoreCase("8") < 0;
					}
				}));
		
		this.addTest(createQueryTest("Id begining with 0-7", allItems, subString("id", 0).ge().val("0").and().subString("id", 0).lt().val("8"),
				new SimpleFilter<StringIdTableItem>() {

					@Override
					protected boolean criteria(StringIdTableItem element) {
						return element.id.substring(0, 1).compareToIgnoreCase("0") >= 0 && element.id.substring(0, 1).compareToIgnoreCase("8") < 0;
					}
				}));

		String jsonItem = "{\"data\": \"" + UUID.randomUUID().toString() + "\"}";
		
		this.addTest(createSimpleUntypedRoundTripTestWithException("Round Trip Untyped", jsonItem, null));
		
		StringIdTableItem item = new StringIdTableItem();
		item.data = UUID.randomUUID().toString();
		
		this.addTest(createSimpleTypedRoundTripTestWithException("Round Trip Typed", item, null, false));
	}
	

	private List<StringIdTableItem> loadInitialItems() {
		List<StringIdTableItem> allItems = new ArrayList<StringIdTableItem>();
		
		for (int i = 0; i < 10; i++) {
			final StringIdTableItem item =  new StringIdTableItem();
			item.data = UUID.randomUUID().toString();
			
			TestCase test = new TestCase() {

				@Override
				protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {

					final TestCase testCase = this;

					client.getTable(STRING_ID_TABLE_NAME, StringIdTableItem.class).insert(item, new TableOperationCallback<StringIdTableItem>() {

						@Override
						public void onCompleted(StringIdTableItem entity, Exception exception, ServiceFilterResponse response) {
							TestResult result = null;

							if (exception == null) {
								result = new TestResult();
								result.setTestCase(testCase);
								result.setStatus(TestStatus.Passed);
							} else {
								result = createResultFromException(exception);
							}

							if (callback != null)
								callback.onTestComplete(testCase, result);
						}
					});
				}
			};
			
			allItems.add(item);
			
			test.setName("Add initial item - Index: " + i);
			this.addTest(test);
		}
		
		return allItems;
	}


	private TestCase createQueryTest(String name, List<StringIdTableItem> allItems, final MobileServiceQuery<?> filter, final ListFilter<StringIdTableItem> expectedResultFilter) {

		return createQueryTest(name, allItems, filter, expectedResultFilter, null, null, null, null, false, null);
	}

	private TestCase createQueryTest(String name, final List<StringIdTableItem> allItems, final MobileServiceQuery<?> filter, final ListFilter<StringIdTableItem> expectedResultFilter, final Integer top,
			final Integer skip, final List<Pair<String, QueryOrder>> orderBy, final String[] projection, final boolean includeInlineCount,
			final Class<?> expectedExceptionClass) {

		final TestCase test = new TestCase() {

			@Override
			protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {

				MobileServiceQuery<TableQueryCallback<StringIdTableItem>> query;

				if (filter != null) {
					log("add filter");
					query = client.getTable(STRING_ID_TABLE_NAME, StringIdTableItem.class).where(filter);
				} else {
					query = client.getTable(STRING_ID_TABLE_NAME, StringIdTableItem.class).where();
				}

				if (top != null) {
					log("add top");
					query = query.top(top);
				}

				if (skip != null) {
					log("add skip");
					query = query.skip(skip);
				}

				if (orderBy != null) {
					log("add orderby");
					for (Pair<String, QueryOrder> order : orderBy) {
						query = query.orderBy(order.first, order.second);
					}
				}

				if (projection != null) {
					log("add projection");
					query = query.select(projection);
				}

				if (includeInlineCount) {
					log("add inlinecount");
					query.includeInlineCount();
				}

				final TestCase testCase = this;
				query.execute(new TableQueryCallback<StringIdTableItem>() {

					@Override
					public void onCompleted(List<StringIdTableItem> elements, int count, Exception exception, ServiceFilterResponse response) {
						TestResult result = new TestResult();
						result.setStatus(TestStatus.Passed);
						result.setTestCase(testCase);

						if (exception == null) {
							FilterResult<StringIdTableItem> expectedData = expectedResultFilter.filter(allItems);

							log("verify result");
							if (Util.compareLists(expectedData.elements, elements)) {

								if (includeInlineCount) {
									log("verify inline count");
									if (expectedData.totalCount != count) {
										createResultFromException(result, new ExpectedValueException(expectedData.totalCount, count));
									}
								}
							} else {
								createResultFromException(result,
										new ExpectedValueException(Util.listToString(expectedData.elements), Util.listToString(elements)));
							}
						} else {
							createResultFromException(result, exception);
						}

						if (callback != null)
							callback.onTestComplete(testCase, result);
					}

				});
			}
		};

		test.setExpectedExceptionClass(expectedExceptionClass);
		test.setName(name);

		return test;
	}
	
	private TestCase createSimpleUntypedRoundTripTestWithException(String testName, final String jsonString, final Class<?> expectedExceptionClass) {
		TestCase testCase = new TestCase() {

			@Override
			protected void executeTest(final MobileServiceClient client,
					final TestExecutionCallback callback) {
				final MobileServiceJsonTable table = client.getTable(STRING_ID_TABLE_NAME);
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
							String id = jsonEntity.get("id").getAsString();
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

	private TestCase createSimpleTypedRoundTripTestWithException(String testName, final StringIdTableItem element, final Class<?> expectedExceptionClass, final boolean removeId) {
		TestCase testCase = new TestCase() {

			@Override
			protected void executeTest(final MobileServiceClient client, final TestExecutionCallback callback) {
				final MobileServiceTable<StringIdTableItem> table = client.getTable(STRING_ID_TABLE_NAME, StringIdTableItem.class);
				final TestResult result = new TestResult();
				result.setStatus(TestStatus.Passed);
				final TestCase test = this;

				if (removeId) {
					element.id = null;
				}
				
				log("insert item");
				table.insert(element,
						new TableOperationCallback<StringIdTableItem>() {
							@Override
							public void onCompleted(final StringIdTableItem entity, Exception exception, ServiceFilterResponse response) {
								if (exception == null) {
									log("lookup item " + entity.id);
									table.lookUp(
											entity.id,
											new TableOperationCallback<StringIdTableItem>() {
												@Override
												public void onCompleted(StringIdTableItem newEntity, Exception exception, ServiceFilterResponse response) {
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
