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

import static com.microsoft.windowsazure.mobileservices.MobileServiceQueryOperations.field;
import static com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.Util.filter;
import static com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.Util.compare;

import java.util.ArrayList;
import java.util.Date;
import java.util.EnumSet;
import java.util.List;
import java.util.Locale;
import java.util.concurrent.CountDownLatch;

import android.annotation.SuppressLint;
import android.os.AsyncTask;
import android.os.Build;
import android.util.Pair;

import com.google.gson.JsonObject;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceJsonTable;
import com.microsoft.windowsazure.mobileservices.MobileServicePreconditionFailedException;
import com.microsoft.windowsazure.mobileservices.MobileServicePreconditionFailedExceptionBase;
import com.microsoft.windowsazure.mobileservices.MobileServiceQuery;
import com.microsoft.windowsazure.mobileservices.MobileServiceSystemProperty;
import com.microsoft.windowsazure.mobileservices.MobileServiceTable;
import com.microsoft.windowsazure.mobileservices.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.TableDeleteCallback;
import com.microsoft.windowsazure.mobileservices.TableJsonOperationCallback;
import com.microsoft.windowsazure.mobileservices.TableOperationCallback;
import com.microsoft.windowsazure.mobileservices.TableQueryCallback;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.ExpectedValueException;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestCase;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestExecutionCallback;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestGroup;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestResult;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestStatus;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.Util.IPredicate;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.types.StringIdJsonElement;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.types.StringIdRoundTripTableElement;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.types.SystemPropertiesTestData;

public class SystemPropertiesTests extends TestGroup {

	class ResultsContainer<T> {
		private Exception mException;
		private T mItem;
		private List<T> mItems;

		public Exception getException() {
			return mException;
		}

		public void setException(Exception exception) {
			this.mException = exception;
		}

		public T getItem() {
			return mItem;
		}

		public void setItem(T item) {
			this.mItem = item;
		}

		public List<T> getItems() {
			return mItems;
		}

		public void setItems(List<T> items) {
			this.mItems = items;
		}

	}

	protected static final String STRING_ID_TABLE_NAME = "stringIdRoundTripTable";

	public SystemPropertiesTests() {
		super("System Properties tests");

		this.addTest(createTypeSystemPropertiesTest("Operations with All System Properties from Type"));
		this.addTest(createCustomSystemPropertiesTest("Operations with Custom System Properties set on Table"));
		for (String systemProperties : SystemPropertiesTestData.ValidSystemPropertyQueryStrings) {
			this.addTest(createQueryParameterSystemPropertiesTest("Operations with Query Parameter System Properties set on Table - " + systemProperties,
					systemProperties));
		}
		this.addTest(createMergeConflictTest("Merge Conflict"));
		this.addTest(createMergeConflictGenericTest("Merge Conflict Generic"));
	}

	private TestCase createTypeSystemPropertiesTest(String name) {
		StringIdRoundTripTableElement element = new StringIdRoundTripTableElement(true);
		element.id = "an Id";

		final List<StringIdRoundTripTableElement> elements = new ArrayList<StringIdRoundTripTableElement>();
		elements.add(element);

		TestCase roundtripTest = new TestCase() {

			@Override
			protected void executeTest(final MobileServiceClient client, final TestExecutionCallback callback) {
				final TestCase test = this;

				executeTask(new AsyncTask<Void, Void, Void>() {
					@Override
					protected Void doInBackground(Void... params) {
						TestResult result = new TestResult();
						result.setTestCase(test);

						try {
							MobileServiceTable<StringIdRoundTripTableElement> table = client
									.getTable(STRING_ID_TABLE_NAME, StringIdRoundTripTableElement.class);

							log("Make sure table is empty");
							List<StringIdRoundTripTableElement> existingElements = read(table);

							if (existingElements != null && existingElements.size() > 0) {
								for (StringIdRoundTripTableElement existingElement : existingElements) {
									log("Delete item - " + existingElement.toString());
									delete(table, existingElement);
								}
							}

							for (StringIdRoundTripTableElement element : elements) {
								log("Insert item - " + element.toString());
								StringIdRoundTripTableElement responseItem = insert(table, element);

								log("verify system properties are not null");
								if (responseItem.CreatedAt == null || responseItem.UpdatedAt == null || responseItem.Version == null) {
									throw new Exception("Insert response - System Properties are null");
								}
							}

							List<StringIdRoundTripTableElement> responseElements;

							log("Read table");
							responseElements = read(table);

							log("verify response size");
							if (responseElements == null || responseElements.size() != elements.size()) {
								throw new Exception("Read response - incorrect number of records");
							}

							log("verify system properties are not null");
							for (StringIdRoundTripTableElement responseElement : responseElements) {
								if (responseElement.CreatedAt == null || responseElement.UpdatedAt == null || responseElement.Version == null) {
									throw new Exception("Read response - System Properties are null");
								}
							}

							StringIdRoundTripTableElement firstElement = responseElements.get(0);

							final String versionFilter = firstElement.Version;

							List<StringIdRoundTripTableElement> filteredVersionResponseElements = filter(responseElements,
									new IPredicate<StringIdRoundTripTableElement>() {
										@Override
										public boolean evaluate(StringIdRoundTripTableElement element) {
											return element.Version.equals(versionFilter);
										}
									});

							log("Filter table - Version");
							List<StringIdRoundTripTableElement> filteredVersionElements = read(table, field("__version").eq().val(versionFilter));

							log("verify response size");
							if (filteredVersionElements == null || filteredVersionElements.size() != filteredVersionResponseElements.size()) {
								throw new Exception("Filter response - Version - incorrect number of records");
							}

							log("verify system properties are not null");
							for (StringIdRoundTripTableElement filteredVersionElement : filteredVersionElements) {
								if (filteredVersionElement.Version == null) {
									throw new Exception("Filter response - Version is null");
								} else if (!filteredVersionElement.Version.equals(versionFilter)) {
									throw new ExpectedValueException(versionFilter, filteredVersionElement.Version);
								}
							}

							final Date createdAtFilter = firstElement.CreatedAt;

							List<StringIdRoundTripTableElement> filteredCreatedAtResponseElements = filter(responseElements,
									new IPredicate<StringIdRoundTripTableElement>() {
										@Override
										public boolean evaluate(StringIdRoundTripTableElement element) {
											return element.CreatedAt.equals(createdAtFilter);
										}
									});

							log("Filter table - CreatedAt");
							List<StringIdRoundTripTableElement> filteredCreatedAtElements = read(table, field("__createdAt").eq().val(createdAtFilter));

							log("verify response size");
							if (filteredCreatedAtElements == null || filteredCreatedAtElements.size() != filteredCreatedAtResponseElements.size()) {
								throw new Exception("Filter response - CreatedAt - incorrect number of records");
							}

							log("verify system properties are not null");
							for (StringIdRoundTripTableElement filteredCreatedAtElement : filteredCreatedAtElements) {
								if (filteredCreatedAtElement.CreatedAt == null) {
									throw new Exception("Filter response - CreatedAt is null");
								} else if (!filteredCreatedAtElement.CreatedAt.equals(createdAtFilter)) {
									throw new ExpectedValueException(createdAtFilter, filteredCreatedAtElement.CreatedAt);
								}
							}

							final Date updatedAtFilter = firstElement.UpdatedAt;

							List<StringIdRoundTripTableElement> filteredUpdatedAtResponseElements = filter(responseElements,
									new IPredicate<StringIdRoundTripTableElement>() {
										@Override
										public boolean evaluate(StringIdRoundTripTableElement element) {
											return element.UpdatedAt.equals(updatedAtFilter);
										}
									});

							log("Filter table - UpdatedAt");
							List<StringIdRoundTripTableElement> filteredUpdatedAtElements = read(table, field("__updatedAt").eq().val(updatedAtFilter));

							log("verify response size");
							if (filteredUpdatedAtElements == null || filteredUpdatedAtElements.size() != filteredUpdatedAtResponseElements.size()) {
								throw new Exception("Filter response - UpdatedAt - incorrect number of records");
							}

							log("verify system properties are not null");
							for (StringIdRoundTripTableElement filteredUpdatedAtElement : filteredUpdatedAtElements) {
								if (filteredUpdatedAtElement.UpdatedAt == null) {
									throw new Exception("Filter response - UpdatedAt is null");
								} else if (!filteredUpdatedAtElement.UpdatedAt.equals(updatedAtFilter)) {
									throw new ExpectedValueException(updatedAtFilter, filteredUpdatedAtElement.UpdatedAt);
								}
							}

							String lookUpId = firstElement.id;

							log("LookUp");
							StringIdRoundTripTableElement lookUpElement = lookUp(table, lookUpId);

							log("verify element is not null");
							if (lookUpElement == null) {
								throw new Exception("LookUp response - Element is null");
							}

							if (!compare(firstElement.id, lookUpElement.id)) {
								throw new ExpectedValueException(firstElement.id, lookUpElement.id);
							}

							if (!compare(firstElement.Version, lookUpElement.Version)) {
								throw new ExpectedValueException(firstElement.Version, lookUpElement.Version);
							}

							if (!compare(firstElement.CreatedAt, lookUpElement.CreatedAt)) {
								throw new ExpectedValueException(firstElement.CreatedAt, lookUpElement.CreatedAt);
							}

							if (!compare(firstElement.UpdatedAt, lookUpElement.UpdatedAt)) {
								throw new ExpectedValueException(firstElement.UpdatedAt, lookUpElement.UpdatedAt);
							}

							StringIdRoundTripTableElement updateElement = new StringIdRoundTripTableElement(firstElement);
							updateElement.name = "Other Sample Data";

							log("Update");
							updateElement = update(table, updateElement);

							log("verify element is not null");
							if (updateElement == null) {
								throw new Exception("Update response - Element is null");
							}

							if (!compare(firstElement.id, updateElement.id)) {
								throw new ExpectedValueException(firstElement.id, updateElement.id);
							}

							if (compare(firstElement.Version, updateElement.Version)) {
								throw new Exception("Update response - same Version");
							}

							if (!compare(firstElement.CreatedAt, updateElement.CreatedAt)) {
								throw new ExpectedValueException(firstElement.CreatedAt, updateElement.CreatedAt);
							}

							if (!firstElement.UpdatedAt.before(updateElement.UpdatedAt)) {
								throw new Exception("Update response - incorrect UpdatedAt");
							}

							result.setStatus(TestStatus.Passed);
							if (callback != null) {
								callback.onTestComplete(test, result);
							}
						} catch (Exception ex) {
							result = createResultFromException(result, ex);

							if (callback != null) {
								callback.onTestComplete(test, result);
							}
						}

						return null;
					}
				});
			}
		};

		roundtripTest.setName(name);
		return roundtripTest;
	}

	private TestCase createCustomSystemPropertiesTest(String name) {
		TestCase roundtripTest = new TestCase() {

			@Override
			protected void executeTest(final MobileServiceClient client, final TestExecutionCallback callback) {
				final TestCase test = this;

				executeTask(new AsyncTask<Void, Void, Void>() {
					@Override
					protected Void doInBackground(Void... params) {
						TestResult result = new TestResult();
						result.setTestCase(test);

						try {
							MobileServiceTable<StringIdRoundTripTableElement> table = client
									.getTable(STRING_ID_TABLE_NAME, StringIdRoundTripTableElement.class);

							log("Make sure table is empty");
							List<StringIdRoundTripTableElement> existingElements = read(table);

							if (existingElements != null && existingElements.size() > 0) {
								for (StringIdRoundTripTableElement existingElement : existingElements) {
									log("Delete item - " + existingElement.toString());
									delete(table, existingElement);
								}
							}

							StringIdRoundTripTableElement element1 = new StringIdRoundTripTableElement(true);
							element1.id = "an Id 1";

							log("Insert element 1 with Type System Properties - " + element1.toString());
							StringIdRoundTripTableElement responseElement1 = insert(table, element1);

							log("Verify system properties are not null");
							if (responseElement1.CreatedAt == null || responseElement1.UpdatedAt == null || responseElement1.Version == null) {
								throw new Exception("Insert response - System Properties are null");
							}

							StringIdRoundTripTableElement element2 = new StringIdRoundTripTableElement(true);
							element2.id = "an Id 2";

							EnumSet<MobileServiceSystemProperty> systemProperties2 = EnumSet.noneOf(MobileServiceSystemProperty.class);
							systemProperties2.add(MobileServiceSystemProperty.Version);
							systemProperties2.add(MobileServiceSystemProperty.CreatedAt);

							table.setSystemProperties(systemProperties2);

							log("Insert element 2 with Custom System Properties - Version|CreatedAt - " + element2.toString());
							StringIdRoundTripTableElement responseElement2 = insert(table, element2);

							log("Verify Version|CreatedAt System Properties are not null, and UpdateAt is null or default");
							if (responseElement2.CreatedAt == null || responseElement2.Version == null) {
								throw new Exception("Insert response - Version|CreatedAt System Properties are null");
							}

							if (responseElement2.UpdatedAt != null) {
								throw new Exception("Insert response - UpdateAt System Property is not null nor default");
							}

							EnumSet<MobileServiceSystemProperty> systemProperties3 = EnumSet.noneOf(MobileServiceSystemProperty.class);
							systemProperties3.add(MobileServiceSystemProperty.Version);
							systemProperties3.add(MobileServiceSystemProperty.UpdatedAt);

							table.setSystemProperties(systemProperties3);

							log("Filter element2 id with Custom System Properties - Version|UpdatedAt");
							List<StringIdRoundTripTableElement> responseElements3 = read(table, field("id").eq().val(element2.id));

							log("Verify response size");
							if (responseElements3 == null || responseElements3.size() != 1) {
								throw new Exception("Read response - incorrect number of records");
							}

							StringIdRoundTripTableElement responseElement3 = responseElements3.get(0);

							log("Verify Version|UpdatedAt System Properties are not null, and CreatedAt is null or default");
							if (responseElement3.UpdatedAt == null || responseElement3.Version == null) {
								throw new Exception("Filter response - Version|UpdatedAt System Properties are null");
							}

							if (responseElement3.CreatedAt != null) {
								throw new Exception("Filter response - CreatedAt System Property is not null nor default");
							}

							EnumSet<MobileServiceSystemProperty> systemProperties4 = EnumSet.noneOf(MobileServiceSystemProperty.class);

							table.setSystemProperties(systemProperties4);

							log("Lookup element2 id with No System Properties");
							StringIdRoundTripTableElement responseElement4 = lookUp(table, element2.id);

							log("Verify Version|UpdatedAt System Properties are not null, and CreatedAt is null or default");
							if (responseElement4.CreatedAt != null || responseElement4.UpdatedAt != null || responseElement4.Version != null) {
								throw new Exception("LookUp response - System Properties are not null nor default");
							}

							result.setStatus(TestStatus.Passed);
							if (callback != null) {
								callback.onTestComplete(test, result);
							}
						} catch (Exception ex) {
							result = createResultFromException(result, ex);

							if (callback != null) {
								callback.onTestComplete(test, result);
							}
						}

						return null;
					}
				});
			}
		};

		roundtripTest.setName(name);
		return roundtripTest;
	}

	private TestCase createQueryParameterSystemPropertiesTest(String name, final String systemProperties) {
		TestCase roundtripTest = new TestCase() {

			@Override
			protected void executeTest(final MobileServiceClient client, final TestExecutionCallback callback) {
				final TestCase test = this;

				executeTask(new AsyncTask<Void, Void, Void>() {
					@Override
					protected Void doInBackground(Void... params) {
						TestResult result = new TestResult();
						result.setTestCase(test);

						try {
							MobileServiceTable<StringIdRoundTripTableElement> table = client
									.getTable(STRING_ID_TABLE_NAME, StringIdRoundTripTableElement.class);

							log("Make sure table is empty");
							List<StringIdRoundTripTableElement> existingElements = read(table);

							if (existingElements != null && existingElements.size() > 0) {
								for (StringIdRoundTripTableElement existingElement : existingElements) {
									log("Delete item - " + existingElement.toString());
									delete(table, existingElement);
								}
							}

							String[] systemPropertiesKeyValue = systemProperties.split("=");
							String key = systemPropertiesKeyValue[0];
							String value = systemPropertiesKeyValue[1];
							List<Pair<String, String>> userParameters = new ArrayList<Pair<String, String>>();
							userParameters.add(new Pair<String, String>(key, value));

							boolean shouldHaveCreatedAt = value.toLowerCase(Locale.getDefault()).contains("created");
							boolean shouldHaveUpdatedAt = value.toLowerCase(Locale.getDefault()).contains("updated");
							boolean shouldHaveVersion = value.toLowerCase(Locale.getDefault()).contains("version");

							if (value.trim().equals("*")) {
								shouldHaveVersion = shouldHaveUpdatedAt = shouldHaveCreatedAt = true;
							}

							StringIdRoundTripTableElement element1 = new StringIdRoundTripTableElement(true);
							element1.id = "an Id 1";

							log("Insert element 1 with Query Parameter System Properties - " + element1.toString() + " - " + systemProperties);
							StringIdRoundTripTableElement responseElement1 = insert(table, element1, userParameters);

							log("Verify Query Parameter System Properties");

							if ((shouldHaveCreatedAt && responseElement1.CreatedAt == null) || (shouldHaveUpdatedAt && responseElement1.UpdatedAt == null)
									|| (shouldHaveVersion && responseElement1.Version == null)) {
								throw new Exception("Insert response - System Properties are null");
							}

							if ((!shouldHaveCreatedAt && responseElement1.CreatedAt != null) || (!shouldHaveUpdatedAt && responseElement1.UpdatedAt != null)
									|| (!shouldHaveVersion && responseElement1.Version != null)) {
								throw new Exception("Insert response - System Properties are not null nor default");
							}

							log("Read with Query Parameter System Properties - " + systemProperties);
							List<StringIdRoundTripTableElement> responseElements2 = read(table, userParameters);

							log("Verify response size");
							if (responseElements2 == null || responseElements2.size() != 1) {
								throw new Exception("Read response - incorrect number of records");
							}

							StringIdRoundTripTableElement responseElement2 = responseElements2.get(0);

							log("Verify Query Parameter System Properties");

							if ((shouldHaveCreatedAt && responseElement2.CreatedAt == null) || (shouldHaveUpdatedAt && responseElement2.UpdatedAt == null)
									|| (shouldHaveVersion && responseElement2.Version == null)) {
								throw new Exception("Read response - System Properties are null");
							}

							if ((!shouldHaveCreatedAt && responseElement2.CreatedAt != null) || (!shouldHaveUpdatedAt && responseElement2.UpdatedAt != null)
									|| (!shouldHaveVersion && responseElement2.Version != null)) {
								throw new Exception("Read response - System Properties are not null nor default");
							}

							log("Filter element1 id with Query Parameter System Properties - " + systemProperties);
							List<StringIdRoundTripTableElement> responseElements3 = read(table, field("id").eq().val(element1.id), userParameters);

							log("Verify response size");
							if (responseElements3 == null || responseElements3.size() != 1) {
								throw new Exception("Filter response - incorrect number of records");
							}

							StringIdRoundTripTableElement responseElement3 = responseElements3.get(0);

							log("Verify Query Parameter System Properties");

							if ((shouldHaveCreatedAt && responseElement3.CreatedAt == null) || (shouldHaveUpdatedAt && responseElement3.UpdatedAt == null)
									|| (shouldHaveVersion && responseElement3.Version == null)) {
								throw new Exception("Filter response - System Properties are null");
							}

							if ((!shouldHaveCreatedAt && responseElement3.CreatedAt != null) || (!shouldHaveUpdatedAt && responseElement3.UpdatedAt != null)
									|| (!shouldHaveVersion && responseElement3.Version != null)) {
								throw new Exception("Filter response - System Properties are not null nor default");
							}

							log("Lookup element1 id with Query Parameter System Properties - " + systemProperties);
							StringIdRoundTripTableElement responseElement4 = lookUp(table, element1.id, userParameters);

							log("Verify Query Parameter System Properties");

							if ((shouldHaveCreatedAt && responseElement4.CreatedAt == null) || (shouldHaveUpdatedAt && responseElement4.UpdatedAt == null)
									|| (shouldHaveVersion && responseElement4.Version == null)) {
								throw new Exception("Filter response - System Properties are null");
							}

							if ((!shouldHaveCreatedAt && responseElement4.CreatedAt != null) || (!shouldHaveUpdatedAt && responseElement4.UpdatedAt != null)
									|| (!shouldHaveVersion && responseElement4.Version != null)) {
								throw new Exception("Filter response - System Properties are not null nor default");
							}

							StringIdRoundTripTableElement updateElement1 = new StringIdRoundTripTableElement(element1);
							updateElement1.name = "Other Sample Data";

							log("Update element1 with Query Parameter System Properties - " + updateElement1.toString() + " - " + systemProperties);
							StringIdRoundTripTableElement responseElement5 = update(table, updateElement1, userParameters);

							log("Verify Query Parameter System Properties");

							if ((shouldHaveCreatedAt && responseElement5.CreatedAt == null) || (shouldHaveUpdatedAt && responseElement5.UpdatedAt == null)
									|| (shouldHaveVersion && responseElement5.Version == null)) {
								throw new Exception("Update response - System Properties are null");
							}

							if ((!shouldHaveCreatedAt && responseElement5.CreatedAt != null) || (!shouldHaveUpdatedAt && responseElement5.UpdatedAt != null)
									|| (!shouldHaveVersion && responseElement5.Version != null)) {
								throw new Exception("Update response - System Properties are not null nor default");
							}

							result.setStatus(TestStatus.Passed);
							if (callback != null) {
								callback.onTestComplete(test, result);
							}
						} catch (Exception ex) {
							result = createResultFromException(result, ex);

							if (callback != null) {
								callback.onTestComplete(test, result);
							}
						}

						return null;
					}
				});
			}
		};

		roundtripTest.setName(name);
		return roundtripTest;
	}

	private TestCase createMergeConflictTest(String name) {
		TestCase roundtripTest = new TestCase() {

			@Override
			protected void executeTest(final MobileServiceClient client, final TestExecutionCallback callback) {
				final TestCase test = this;

				executeTask(new AsyncTask<Void, Void, Void>() {
					@Override
					protected Void doInBackground(Void... params) {
						TestResult result = new TestResult();
						result.setTestCase(test);

						try {
							MobileServiceTable<StringIdRoundTripTableElement> table = client
									.getTable(STRING_ID_TABLE_NAME, StringIdRoundTripTableElement.class);

							log("Make sure table is empty");
							List<StringIdRoundTripTableElement> existingElements = read(table);

							if (existingElements != null && existingElements.size() > 0) {
								for (StringIdRoundTripTableElement existingElement : existingElements) {
									log("Delete item - " + existingElement.toString());
									delete(table, existingElement);
								}
							}

							MobileServiceJsonTable jsonTable = client.getTable(STRING_ID_TABLE_NAME);

							EnumSet<MobileServiceSystemProperty> systemProperties = EnumSet.noneOf(MobileServiceSystemProperty.class);
							systemProperties.add(MobileServiceSystemProperty.Version);

							jsonTable.setSystemProperties(systemProperties);

							StringIdJsonElement element1 = new StringIdJsonElement(true);

							JsonObject jsonElement1 = client.getGsonBuilder().create().toJsonTree(element1).getAsJsonObject();

							log("Insert Json element 1 - " + jsonElement1.toString());
							JsonObject responseJsonElement1 = insert(jsonTable, jsonElement1);

							responseJsonElement1.remove("__version");
							responseJsonElement1.addProperty("__version", "random");

							log("Update response Json element 1 - " + responseJsonElement1.toString());
							update(jsonTable, responseJsonElement1);

							result.setStatus(TestStatus.Failed);
							if (callback != null) {
								callback.onTestComplete(test, result);
							}
						} catch (Exception ex) {
							result = createResultFromException(result, ex);

							if (callback != null) {
								callback.onTestComplete(test, result);
							}
						}

						return null;
					};
				});
			}
		};

		roundtripTest.setExpectedExceptionClass(MobileServicePreconditionFailedExceptionBase.class);
		roundtripTest.setName(name);
		return roundtripTest;
	}

	private TestCase createMergeConflictGenericTest(String name) {
		TestCase roundtripTest = new TestCase() {

			@Override
			protected void executeTest(final MobileServiceClient client, final TestExecutionCallback callback) {
				final TestCase test = this;

				executeTask(new AsyncTask<Void, Void, Void>() {
					@Override
					protected Void doInBackground(Void... params) {
						TestResult result = new TestResult();
						result.setTestCase(test);

						try {
							MobileServiceTable<StringIdRoundTripTableElement> table = client
									.getTable(STRING_ID_TABLE_NAME, StringIdRoundTripTableElement.class);

							log("Make sure table is empty");
							List<StringIdRoundTripTableElement> existingElements = read(table);

							if (existingElements != null && existingElements.size() > 0) {
								for (StringIdRoundTripTableElement existingElement : existingElements) {
									log("Delete item - " + existingElement.toString());
									delete(table, existingElement);
								}
							}

							StringIdRoundTripTableElement element1 = new StringIdRoundTripTableElement(true);

							log("Insert element 1 - " + element1.toString());
							StringIdRoundTripTableElement responseElement1 = insert(table, element1);

							responseElement1.Version = "random";

							log("Update response element 1 - " + responseElement1.toString());
							update(table, responseElement1);

							result.setStatus(TestStatus.Failed);
							if (callback != null) {
								callback.onTestComplete(test, result);
							}
						} catch (Exception ex) {
							result = createResultFromException(result, ex);

							if (callback != null) {
								callback.onTestComplete(test, result);
							}
						}

						return null;
					}
				});
			}
		};

		roundtripTest.setExpectedExceptionClass(MobileServicePreconditionFailedException.class);
		roundtripTest.setName(name);
		return roundtripTest;
	}

	private <T> List<T> read(final MobileServiceTable<T> table) throws Exception {
		return read(table, null, null);
	}

	private <T> List<T> read(final MobileServiceTable<T> table, final List<Pair<String, String>> parameters) throws Exception {
		return read(table, null, parameters);
	}

	private <T> List<T> read(final MobileServiceTable<T> table, final MobileServiceQuery<?> filter) throws Exception {
		return read(table, filter, null);
	}

	private <T> List<T> read(final MobileServiceTable<T> table, final MobileServiceQuery<?> filter, final List<Pair<String, String>> parameters)
			throws Exception {
		final CountDownLatch latch = new CountDownLatch(1);
		final ResultsContainer<T> container = new ResultsContainer<T>();

		MobileServiceQuery<TableQueryCallback<T>> query;

		if (filter != null) {
			query = table.where(filter);
		} else {
			query = table.where();
		}

		if (parameters != null) {
			for (Pair<String, String> parameter : parameters) {
				query.parameter(parameter.first, parameter.second);
			}
		}

		table.execute(query, new TableQueryCallback<T>() {

			@Override
			public void onCompleted(List<T> responseElements, int count, Exception executeException, ServiceFilterResponse response) {
				if (executeException == null) {
					container.setItems(responseElements);
				} else {
					container.setException(executeException);
				}

				latch.countDown();
			}
		});

		latch.await();

		Exception ex = container.getException();

		if (ex != null) {
			throw ex;
		} else {
			return container.getItems();
		}
	}

	private <T> T lookUp(final MobileServiceTable<T> table, final Object id) throws Exception {
		return lookUp(table, id, null);
	}

	private <T> T lookUp(final MobileServiceTable<T> table, final Object id, final List<Pair<String, String>> parameters) throws Exception {
		final ResultsContainer<T> container = new ResultsContainer<T>();
		final CountDownLatch latch = new CountDownLatch(1);

		table.lookUp(id, parameters, new TableOperationCallback<T>() {

			@Override
			public void onCompleted(T responseElement, Exception exception, ServiceFilterResponse response) {
				if (exception == null) {
					container.setItem(responseElement);
				} else {
					container.setException(exception);
				}

				latch.countDown();
			}
		});

		latch.await();

		Exception ex = container.getException();

		if (ex != null) {
			throw ex;
		} else {
			return container.getItem();
		}
	}

	private <T> T insert(final MobileServiceTable<T> table, final T element) throws Exception {
		return insert(table, element, null);
	}

	private <T> T insert(final MobileServiceTable<T> table, final T element, final List<Pair<String, String>> parameters) throws Exception {
		final ResultsContainer<T> container = new ResultsContainer<T>();
		final CountDownLatch latch = new CountDownLatch(1);

		table.insert(element, parameters, new TableOperationCallback<T>() {

			@Override
			public void onCompleted(T responseElement, Exception exception, ServiceFilterResponse response) {
				if (exception == null) {
					container.setItem(responseElement);
				} else {
					container.setException(exception);
				}

				latch.countDown();
			}
		});

		latch.await();

		Exception ex = container.getException();

		if (ex != null) {
			throw ex;
		} else {
			return container.getItem();
		}
	}

	private JsonObject insert(final MobileServiceJsonTable table, final JsonObject element) throws Exception {
		return insert(table, element, null);
	}

	private JsonObject insert(final MobileServiceJsonTable table, final JsonObject element, final List<Pair<String, String>> parameters) throws Exception {
		final ResultsContainer<JsonObject> container = new ResultsContainer<JsonObject>();
		final CountDownLatch latch = new CountDownLatch(1);

		table.insert(element, parameters, new TableJsonOperationCallback() {

			@Override
			public void onCompleted(JsonObject jsonObject, Exception exception, ServiceFilterResponse response) {
				if (exception == null) {
					container.setItem(jsonObject);
				} else {
					container.setException(exception);
				}

				latch.countDown();
			}
		});

		latch.await();

		Exception ex = container.getException();

		if (ex != null) {
			throw ex;
		} else {
			return container.getItem();
		}
	}

	private <T> T update(final MobileServiceTable<T> table, final T element) throws Exception {
		return update(table, element, null);
	}

	private <T> T update(final MobileServiceTable<T> table, final T element, final List<Pair<String, String>> parameters) throws Exception {
		final ResultsContainer<T> container = new ResultsContainer<T>();
		final CountDownLatch latch = new CountDownLatch(1);

		table.update(element, parameters, new TableOperationCallback<T>() {

			@Override
			public void onCompleted(T responseElement, Exception exception, ServiceFilterResponse response) {
				if (exception == null) {
					container.setItem(responseElement);
				} else {
					container.setException(exception);
				}

				latch.countDown();
			}
		});

		latch.await();

		Exception ex = container.getException();

		if (ex != null) {
			throw ex;
		} else {
			return container.getItem();
		}
	}

	private JsonObject update(final MobileServiceJsonTable table, final JsonObject element) throws Exception {
		return update(table, element, null);
	}

	private JsonObject update(final MobileServiceJsonTable table, final JsonObject element, final List<Pair<String, String>> parameters) throws Exception {
		final ResultsContainer<JsonObject> container = new ResultsContainer<JsonObject>();
		final CountDownLatch latch = new CountDownLatch(1);

		table.update(element, parameters, new TableJsonOperationCallback() {

			@Override
			public void onCompleted(JsonObject jsonObject, Exception exception, ServiceFilterResponse response) {
				if (exception == null) {
					container.setItem(jsonObject);
				} else {
					container.setException(exception);
				}

				latch.countDown();
			}
		});

		latch.await();

		Exception ex = container.getException();

		if (ex != null) {
			throw ex;
		} else {
			return container.getItem();
		}
	}

	private <T> void delete(final MobileServiceTable<T> table, final T element) throws Exception {
		final ResultsContainer<T> container = new ResultsContainer<T>();
		final CountDownLatch latch = new CountDownLatch(1);

		table.delete(element, new TableDeleteCallback() {

			@Override
			public void onCompleted(Exception exception, ServiceFilterResponse response) {
				if (exception != null) {
					container.setException(exception);
				}

				latch.countDown();
			}
		});

		latch.await();

		Exception ex = container.getException();

		if (ex != null) {
			throw ex;
		}
	}

	@SuppressLint("NewApi")
	private void executeTask(AsyncTask<Void, Void, Void> task) {
		// If it's running with Honeycomb or greater, it must execute each
		// request in a different thread
		if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.HONEYCOMB) {
			task.executeOnExecutor(AsyncTask.THREAD_POOL_EXECUTOR);
		} else {
			task.execute();
		}
	}

}
