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

import java.net.MalformedURLException;
import java.util.ArrayList;
import java.util.Date;
import java.util.EnumSet;
import java.util.List;
import java.util.Map;
import java.util.Map.Entry;
import java.util.TreeMap;
import java.util.concurrent.CountDownLatch;

import org.apache.http.Header;
import org.apache.http.HeaderElement;
import org.apache.http.ParseException;

import android.test.InstrumentationTestCase;
import android.util.Pair;

import com.google.gson.GsonBuilder;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceJsonTable;
import com.microsoft.windowsazure.mobileservices.MobileServiceSystemProperty;
import com.microsoft.windowsazure.mobileservices.MobileServiceTable;
import com.microsoft.windowsazure.mobileservices.NextServiceFilterCallback;
import com.microsoft.windowsazure.mobileservices.ServiceFilter;
import com.microsoft.windowsazure.mobileservices.ServiceFilterRequest;
import com.microsoft.windowsazure.mobileservices.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.ServiceFilterResponseCallback;
import com.microsoft.windowsazure.mobileservices.TableDeleteCallback;
import com.microsoft.windowsazure.mobileservices.TableJsonOperationCallback;
import com.microsoft.windowsazure.mobileservices.TableJsonQueryCallback;
import com.microsoft.windowsazure.mobileservices.TableOperationCallback;

public class SystemPropertiesTests extends InstrumentationTestCase {
	String appUrl = "";
	String appKey = "";
	GsonBuilder gsonBuilder;

	protected void setUp() throws Exception {
		appUrl = "http://myapp.com/";
		appKey = "qwerty";
		gsonBuilder = new GsonBuilder();
		super.setUp();
	}

	protected void tearDown() throws Exception {
		super.tearDown();
	}

	// Non Generic JSON

	// Insert Tests

	public void testInsertDoesNotRemoveSystemPropertiesWhenIdIsString() throws Throwable {
		String[] systemProperties = SystemPropertiesTestData.ValidSystemProperties;

		for (String systemProperty : systemProperties) {
			insertDoesNotRemovePropertyWhenIdIsString(systemProperty);
		}
	}

	public void testInsertDoesNotRemoveNonSystemPropertiesWhenIdIsString() throws Throwable {
		String[] nonSystemProperties = SystemPropertiesTestData.NonSystemProperties;

		for (String nonSystemProperty : nonSystemProperties) {
			insertDoesNotRemovePropertyWhenIdIsString(nonSystemProperty);
		}
	}

	private void insertDoesNotRemovePropertyWhenIdIsString(final String property) throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		final String jsonTestSystemProperty = property.replace("\\", "\\\\").replace("\"", "\\\"");
		final String responseContent = "{\"id\":\"an id\",\"String\":\"Hey\"}";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(getTestFilter(responseContent));

				client = client.withFilter(new ServiceFilter() {
					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {
						String content = request.getContent();
						JsonObject obj = new JsonParser().parse(content).getAsJsonObject();

						Map<String, JsonElement> properties = new TreeMap<String, JsonElement>(String.CASE_INSENSITIVE_ORDER);

						for (Entry<String, JsonElement> entry : obj.entrySet()) {
							properties.put(entry.getKey(), entry.getValue());
						}

						assertTrue(properties.containsKey("id"));
						assertTrue(properties.containsKey("String"));
						assertTrue(properties.containsKey(property));

						nextServiceFilterCallback.onNext(request, responseCallback);
					}
				});

				// Create get the MobileService table
				MobileServiceJsonTable msTable = client.getTable(tableName);

				JsonObject obj = new JsonParser().parse("{\"id\":\"an id\",\"String\":\"what\",\"" + jsonTestSystemProperty + "\":\"a value\"}")
						.getAsJsonObject();

				// Call the insert method
				msTable.insert(obj, new TableJsonOperationCallback() {

					@Override
					public void onCompleted(JsonObject jsonObject, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						} else if (jsonObject == null) {
							container.setException(new Exception("Expected result"));
						} else {
							container.setJsonResult(jsonObject);
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();

		if (exception != null) {
			fail(exception.getMessage());
		}
	}

	public void testInsertDoesNotRemovesSystemPropertiesWhenIdIsNull() throws Throwable {
		String[] systemProperties = SystemPropertiesTestData.ValidSystemProperties;

		for (String systemProperty : systemProperties) {
			insertDoesNotRemovePropertyWhenIdIsNull(systemProperty);
		}
	}

	public void testInsertDoesNotRemovesNonSystemPropertiesWhenIdIsNull() throws Throwable {
		String[] nonSystemProperties = SystemPropertiesTestData.NonSystemProperties;

		for (String nonSystemProperty : nonSystemProperties) {
			insertDoesNotRemovePropertyWhenIdIsNull(nonSystemProperty);
		}
	}

	private void insertDoesNotRemovePropertyWhenIdIsNull(final String property) throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		final String jsonTestSystemProperty = property.replace("\\", "\\\\").replace("\"", "\\\"");
		final String responseContent = "{\"id\":\"an id\",\"String\":\"Hey\"}";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(getTestFilter(responseContent));

				client = client.withFilter(new ServiceFilter() {
					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {
						String content = request.getContent();
						JsonObject obj = new JsonParser().parse(content).getAsJsonObject();

						Map<String, JsonElement> properties = new TreeMap<String, JsonElement>(String.CASE_INSENSITIVE_ORDER);

						for (Entry<String, JsonElement> entry : obj.entrySet()) {
							properties.put(entry.getKey(), entry.getValue());
						}

						assertFalse(properties.containsKey("id"));
						assertTrue(properties.containsKey("String"));
						assertTrue(properties.containsKey(property));

						nextServiceFilterCallback.onNext(request, responseCallback);
					}
				});

				// Create get the MobileService table
				MobileServiceJsonTable msTable = client.getTable(tableName);

				JsonObject obj = new JsonParser().parse("{\"id\":null,\"String\":\"what\",\"" + jsonTestSystemProperty + "\":\"a value\"}").getAsJsonObject();

				// Call the insert method
				msTable.insert(obj, new TableJsonOperationCallback() {

					@Override
					public void onCompleted(JsonObject jsonObject, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						} else if (jsonObject == null) {
							container.setException(new Exception("Expected result"));
						} else {
							container.setJsonResult(jsonObject);
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();

		if (exception != null) {
			fail(exception.getMessage());
		}
	}

	public void testInsertQueryStringWithSystemProperties() throws Throwable {
		List<EnumSet<MobileServiceSystemProperty>> allSystemProperties = SystemPropertiesTestData.AllSystemProperties;

		for (EnumSet<MobileServiceSystemProperty> systemProperties : allSystemProperties) {
			insertQueryStringWithSystemProperties(systemProperties);
		}
	}

	private void insertQueryStringWithSystemProperties(final EnumSet<MobileServiceSystemProperty> systemProperties) throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		final String responseContent = "{\"id\":\"an id\",\"String\":\"Hey\"}";
		final JsonObject obj = new JsonParser().parse("{\"id\":\"an id\",\"String\":\"what\"}").getAsJsonObject();

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(getTestFilter(responseContent));

				client = client.withFilter(new ServiceFilter() {
					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						validateUri(systemProperties, request.getUrl());

						nextServiceFilterCallback.onNext(request, responseCallback);
					}
				});

				// Create get the MobileService table
				MobileServiceJsonTable msTable = client.getTable(tableName);

				msTable.setSystemProperties(systemProperties);

				// Call the insert method
				msTable.insert(obj, new TableJsonOperationCallback() {

					@Override
					public void onCompleted(JsonObject jsonObject, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						} else if (jsonObject == null) {
							container.setException(new Exception("Expected result"));
						} else {
							container.setJsonResult(jsonObject);
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();

		if (exception != null) {
			fail(exception.getMessage());
		}
	}

	public void testInsertQueryStringWithSystemPropertiesIntId() throws Throwable {
		List<EnumSet<MobileServiceSystemProperty>> allSystemProperties = SystemPropertiesTestData.AllSystemProperties;

		for (EnumSet<MobileServiceSystemProperty> systemProperties : allSystemProperties) {
			insertQueryStringWithSystemPropertiesIntId(systemProperties);
		}
	}

	private void insertQueryStringWithSystemPropertiesIntId(final EnumSet<MobileServiceSystemProperty> systemProperties) throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		final String responseContent = "{\"id\":5,\"String\":\"Hey\"}";
		final JsonObject obj = new JsonParser().parse("{\"String\":\"what\"}").getAsJsonObject();

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(getTestFilter(responseContent));

				client = client.withFilter(new ServiceFilter() {
					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						validateUri(systemProperties, request.getUrl());

						nextServiceFilterCallback.onNext(request, responseCallback);
					}
				});

				// Create get the MobileService table
				MobileServiceJsonTable msTable = client.getTable(tableName);

				msTable.setSystemProperties(systemProperties);

				// Call the insert method
				msTable.insert(obj, new TableJsonOperationCallback() {

					@Override
					public void onCompleted(JsonObject jsonObject, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						} else if (jsonObject == null) {
							container.setException(new Exception("Expected result"));
						} else {
							container.setJsonResult(jsonObject);
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();

		if (exception != null) {
			fail(exception.getMessage());
		}
	}

	public void testInsertUserParameterWithSystemProperties() throws Throwable {
		List<EnumSet<MobileServiceSystemProperty>> allSystemProperties = SystemPropertiesTestData.AllSystemProperties;

		for (EnumSet<MobileServiceSystemProperty> systemProperties : allSystemProperties) {
			insertUserParameterWithSystemProperties(systemProperties);
		}
	}

	private void insertUserParameterWithSystemProperties(final EnumSet<MobileServiceSystemProperty> systemProperties) throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		final String responseContent = "{\"id\":\"an id\",\"String\":\"Hey\"}";
		final JsonObject obj = new JsonParser().parse("{\"id\":\"an id\",\"String\":\"what\"}").getAsJsonObject();

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(getTestFilter(responseContent));

				client = client.withFilter(new ServiceFilter() {
					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						assertTrue(request.getUrl().contains("__systemproperties=__createdAt"));

						nextServiceFilterCallback.onNext(request, responseCallback);
					}
				});

				// Create get the MobileService table
				MobileServiceJsonTable msTable = client.getTable(tableName);

				msTable.setSystemProperties(systemProperties);

				List<Pair<String, String>> parameters = new ArrayList<Pair<String, String>>();
				parameters.add(new Pair<String, String>("__systemproperties", "__createdAt"));

				// Call the insert method
				msTable.insert(obj, parameters, new TableJsonOperationCallback() {

					@Override
					public void onCompleted(JsonObject jsonObject, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						} else if (jsonObject == null) {
							container.setException(new Exception("Expected result"));
						} else {
							container.setJsonResult(jsonObject);
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();

		if (exception != null) {
			fail(exception.getMessage());
		}
	}

	public void testInsertUserParameterWithSystemPropertiesIntId() throws Throwable {
		List<EnumSet<MobileServiceSystemProperty>> allSystemProperties = SystemPropertiesTestData.AllSystemProperties;

		for (EnumSet<MobileServiceSystemProperty> systemProperties : allSystemProperties) {
			insertUserParameterWithSystemPropertiesIntId(systemProperties);
		}
	}

	private void insertUserParameterWithSystemPropertiesIntId(final EnumSet<MobileServiceSystemProperty> systemProperties) throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		final String responseContent = "{\"id\":5,\"String\":\"Hey\"}";
		final JsonObject obj = new JsonParser().parse("{\"String\":\"what\"}").getAsJsonObject();

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(getTestFilter(responseContent));

				client = client.withFilter(new ServiceFilter() {
					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						assertTrue(request.getUrl().contains("__systemproperties=__createdAt"));

						nextServiceFilterCallback.onNext(request, responseCallback);
					}
				});

				// Create get the MobileService table
				MobileServiceJsonTable msTable = client.getTable(tableName);

				msTable.setSystemProperties(systemProperties);

				List<Pair<String, String>> parameters = new ArrayList<Pair<String, String>>();
				parameters.add(new Pair<String, String>("__systemproperties", "__createdAt"));

				// Call the insert method
				msTable.insert(obj, parameters, new TableJsonOperationCallback() {

					@Override
					public void onCompleted(JsonObject jsonObject, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						} else if (jsonObject == null) {
							container.setException(new Exception("Expected result"));
						} else {
							container.setJsonResult(jsonObject);
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();

		if (exception != null) {
			fail(exception.getMessage());
		}
	}

	// Update Tests

	public void testUpdateRemovesSystemPropertiesWhenIdIsString() throws Throwable {
		String[] systemProperties = SystemPropertiesTestData.ValidSystemProperties;

		for (String systemProperty : systemProperties) {
			updateRemovesPropertyWhenIdIsString(systemProperty);
		}
	}

	private void updateRemovesPropertyWhenIdIsString(final String property) throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		final String jsonTestSystemProperty = property.replace("\\", "\\\\").replace("\"", "\\\"");
		final String responseContent = "{\"id\":\"an id\",\"String\":\"Hey\"}";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(getTestFilter(responseContent));

				client = client.withFilter(new ServiceFilter() {
					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {
						String content = request.getContent();
						JsonObject obj = new JsonParser().parse(content).getAsJsonObject();

						Map<String, JsonElement> properties = new TreeMap<String, JsonElement>(String.CASE_INSENSITIVE_ORDER);

						for (Entry<String, JsonElement> entry : obj.entrySet()) {
							properties.put(entry.getKey(), entry.getValue());
						}

						assertTrue(properties.containsKey("id"));
						assertTrue(properties.containsKey("String"));
						assertFalse(properties.containsKey(property));

						nextServiceFilterCallback.onNext(request, responseCallback);
					}
				});

				// Create get the MobileService table
				MobileServiceJsonTable msTable = client.getTable(tableName);

				JsonObject obj = new JsonParser().parse("{\"id\":\"an id\",\"String\":\"what\",\"" + jsonTestSystemProperty + "\":\"a value\"}")
						.getAsJsonObject();

				// Call the update method
				msTable.update(obj, new TableJsonOperationCallback() {

					@Override
					public void onCompleted(JsonObject jsonObject, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						} else if (jsonObject == null) {
							container.setException(new Exception("Expected result"));
						} else {
							container.setJsonResult(jsonObject);
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();

		if (exception != null) {
			fail(exception.getMessage());
		}
	}

	public void testUpdateDoesNotRemoveNonSystemPropertiesWhenIdIsString() throws Throwable {
		String[] nonSystemProperties = SystemPropertiesTestData.NonSystemProperties;

		for (String nonSystemProperty : nonSystemProperties) {
			updateDoesNotRemovePropertyWhenIdIsString(nonSystemProperty);
		}
	}

	private void updateDoesNotRemovePropertyWhenIdIsString(final String property) throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		final String jsonTestSystemProperty = property.replace("\\", "\\\\").replace("\"", "\\\"");
		final String responseContent = "{\"id\":\"an id\",\"String\":\"Hey\"}";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(getTestFilter(responseContent));

				client = client.withFilter(new ServiceFilter() {
					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {
						String content = request.getContent();
						JsonObject obj = new JsonParser().parse(content).getAsJsonObject();

						Map<String, JsonElement> properties = new TreeMap<String, JsonElement>(String.CASE_INSENSITIVE_ORDER);

						for (Entry<String, JsonElement> entry : obj.entrySet()) {
							properties.put(entry.getKey(), entry.getValue());
						}

						assertTrue(properties.containsKey("id"));
						assertTrue(properties.containsKey("String"));
						assertTrue(properties.containsKey(property));

						nextServiceFilterCallback.onNext(request, responseCallback);
					}
				});

				// Create get the MobileService table
				MobileServiceJsonTable msTable = client.getTable(tableName);

				JsonObject obj = new JsonParser().parse("{\"id\":\"an id\",\"String\":\"what\",\"" + jsonTestSystemProperty + "\":\"a value\"}")
						.getAsJsonObject();

				// Call the update method
				msTable.update(obj, new TableJsonOperationCallback() {

					@Override
					public void onCompleted(JsonObject jsonObject, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						} else if (jsonObject == null) {
							container.setException(new Exception("Expected result"));
						} else {
							container.setJsonResult(jsonObject);
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();

		if (exception != null) {
			fail(exception.getMessage());
		}
	}

	public void testUpdateDoesNotRemovesSystemPropertiesWhenIdIsInteger() throws Throwable {
		String[] systemProperties = SystemPropertiesTestData.ValidSystemProperties;

		for (String systemProperty : systemProperties) {
			updateDoesNotRemovePropertyWhenIdIsInteger(systemProperty);
		}
	}

	public void testUpdateDoesNotRemovesNonSystemPropertiesWhenIdIsInteger() throws Throwable {
		String[] nonSystemProperties = SystemPropertiesTestData.NonSystemProperties;

		for (String nonSystemProperty : nonSystemProperties) {
			updateDoesNotRemovePropertyWhenIdIsInteger(nonSystemProperty);
		}
	}

	private void updateDoesNotRemovePropertyWhenIdIsInteger(final String property) throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		final String jsonTestSystemProperty = property.replace("\\", "\\\\").replace("\"", "\\\"");
		final String responseContent = "{\"id\":5,\"String\":\"Hey\"}";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(getTestFilter(responseContent));

				client = client.withFilter(new ServiceFilter() {
					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {
						String content = request.getContent();
						JsonObject obj = new JsonParser().parse(content).getAsJsonObject();

						Map<String, JsonElement> properties = new TreeMap<String, JsonElement>(String.CASE_INSENSITIVE_ORDER);

						for (Entry<String, JsonElement> entry : obj.entrySet()) {
							properties.put(entry.getKey(), entry.getValue());
						}

						assertTrue(properties.containsKey("id"));
						assertTrue(properties.containsKey("String"));
						assertTrue(properties.containsKey(property));

						nextServiceFilterCallback.onNext(request, responseCallback);
					}
				});

				// Create get the MobileService table
				MobileServiceJsonTable msTable = client.getTable(tableName);

				JsonObject obj = new JsonParser().parse("{\"id\":5,\"String\":\"what\",\"" + jsonTestSystemProperty + "\":\"a value\"}").getAsJsonObject();

				// Call the update method
				msTable.update(obj, new TableJsonOperationCallback() {

					@Override
					public void onCompleted(JsonObject jsonObject, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						} else if (jsonObject == null) {
							container.setException(new Exception("Expected result"));
						} else {
							container.setJsonResult(jsonObject);
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();

		if (exception != null) {
			fail(exception.getMessage());
		}
	}

	public void testUpdateQueryStringWithSystemProperties() throws Throwable {
		List<EnumSet<MobileServiceSystemProperty>> allSystemProperties = SystemPropertiesTestData.AllSystemProperties;

		for (EnumSet<MobileServiceSystemProperty> systemProperties : allSystemProperties) {
			updateQueryStringWithSystemProperties(systemProperties);
		}
	}

	private void updateQueryStringWithSystemProperties(final EnumSet<MobileServiceSystemProperty> systemProperties) throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		final String responseContent = "{\"id\":\"an id\",\"String\":\"Hey\"}";
		final JsonObject obj = new JsonParser().parse("{\"id\":\"an id\",\"String\":\"what\"}").getAsJsonObject();

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(getTestFilter(responseContent));

				client = client.withFilter(new ServiceFilter() {
					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						validateUri(systemProperties, request.getUrl());

						nextServiceFilterCallback.onNext(request, responseCallback);
					}
				});

				// Create get the MobileService table
				MobileServiceJsonTable msTable = client.getTable(tableName);

				msTable.setSystemProperties(systemProperties);

				// Call the update method
				msTable.update(obj, new TableJsonOperationCallback() {

					@Override
					public void onCompleted(JsonObject jsonObject, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						} else if (jsonObject == null) {
							container.setException(new Exception("Expected result"));
						} else {
							container.setJsonResult(jsonObject);
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();

		if (exception != null) {
			fail(exception.getMessage());
		}
	}

	public void testUpdateQueryStringWithSystemPropertiesIntId() throws Throwable {
		List<EnumSet<MobileServiceSystemProperty>> allSystemProperties = SystemPropertiesTestData.AllSystemProperties;

		for (EnumSet<MobileServiceSystemProperty> systemProperties : allSystemProperties) {
			updateQueryStringWithSystemPropertiesIntId(systemProperties);
		}
	}

	private void updateQueryStringWithSystemPropertiesIntId(final EnumSet<MobileServiceSystemProperty> systemProperties) throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		final String responseContent = "{\"id\":5,\"String\":\"Hey\"}";
		final JsonObject obj = new JsonParser().parse("{\"id\":5,\"String\":\"what\"}").getAsJsonObject();

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(getTestFilter(responseContent));

				client = client.withFilter(new ServiceFilter() {
					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						validateUri(systemProperties, request.getUrl());

						nextServiceFilterCallback.onNext(request, responseCallback);
					}
				});

				// Create get the MobileService table
				MobileServiceJsonTable msTable = client.getTable(tableName);

				msTable.setSystemProperties(systemProperties);

				// Call the update method
				msTable.update(obj, new TableJsonOperationCallback() {

					@Override
					public void onCompleted(JsonObject jsonObject, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						} else if (jsonObject == null) {
							container.setException(new Exception("Expected result"));
						} else {
							container.setJsonResult(jsonObject);
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();

		if (exception != null) {
			fail(exception.getMessage());
		}
	}

	public void testUpdateUserParameterWithSystemProperties() throws Throwable {
		List<EnumSet<MobileServiceSystemProperty>> allSystemProperties = SystemPropertiesTestData.AllSystemProperties;

		for (EnumSet<MobileServiceSystemProperty> systemProperties : allSystemProperties) {
			updateUserParameterWithSystemProperties(systemProperties);
		}
	}

	private void updateUserParameterWithSystemProperties(final EnumSet<MobileServiceSystemProperty> systemProperties) throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		final String responseContent = "{\"id\":\"an id\",\"String\":\"Hey\"}";
		final JsonObject obj = new JsonParser().parse("{\"id\":\"an id\",\"String\":\"what\"}").getAsJsonObject();

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(getTestFilter(responseContent));

				client = client.withFilter(new ServiceFilter() {
					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						assertTrue(request.getUrl().contains("__systemproperties=createdAt"));

						nextServiceFilterCallback.onNext(request, responseCallback);
					}
				});

				// Create get the MobileService table
				MobileServiceJsonTable msTable = client.getTable(tableName);

				msTable.setSystemProperties(systemProperties);

				List<Pair<String, String>> parameters = new ArrayList<Pair<String, String>>();
				parameters.add(new Pair<String, String>("__systemproperties", "createdAt"));

				// Call the update method
				msTable.update(obj, parameters, new TableJsonOperationCallback() {

					@Override
					public void onCompleted(JsonObject jsonObject, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						} else if (jsonObject == null) {
							container.setException(new Exception("Expected result"));
						} else {
							container.setJsonResult(jsonObject);
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();

		if (exception != null) {
			fail(exception.getMessage());
		}
	}

	public void testUpdateUserParameterWithSystemPropertiesIntId() throws Throwable {
		List<EnumSet<MobileServiceSystemProperty>> allSystemProperties = SystemPropertiesTestData.AllSystemProperties;

		for (EnumSet<MobileServiceSystemProperty> systemProperties : allSystemProperties) {
			updateUserParameterWithSystemPropertiesIntId(systemProperties);
		}
	}

	private void updateUserParameterWithSystemPropertiesIntId(final EnumSet<MobileServiceSystemProperty> systemProperties) throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		final String responseContent = "{\"id\":5,\"String\":\"Hey\"}";
		final JsonObject obj = new JsonParser().parse("{\"id\":5,\"String\":\"what\"}").getAsJsonObject();

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(getTestFilter(responseContent));

				client = client.withFilter(new ServiceFilter() {
					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						assertTrue(request.getUrl().contains("__systemproperties=createdAt"));

						nextServiceFilterCallback.onNext(request, responseCallback);
					}
				});

				// Create get the MobileService table
				MobileServiceJsonTable msTable = client.getTable(tableName);

				msTable.setSystemProperties(systemProperties);

				List<Pair<String, String>> parameters = new ArrayList<Pair<String, String>>();
				parameters.add(new Pair<String, String>("__systemproperties", "createdAt"));

				// Call the update method
				msTable.update(obj, parameters, new TableJsonOperationCallback() {

					@Override
					public void onCompleted(JsonObject jsonObject, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						} else if (jsonObject == null) {
							container.setException(new Exception("Expected result"));
						} else {
							container.setJsonResult(jsonObject);
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();

		if (exception != null) {
			fail(exception.getMessage());
		}
	}

	// Lookup Tests

	public void testLookupQueryStringWithSystemProperties() throws Throwable {
		List<EnumSet<MobileServiceSystemProperty>> allSystemProperties = SystemPropertiesTestData.AllSystemProperties;

		for (EnumSet<MobileServiceSystemProperty> systemProperties : allSystemProperties) {
			lookupQueryStringWithSystemProperties(systemProperties);
		}
	}

	private void lookupQueryStringWithSystemProperties(final EnumSet<MobileServiceSystemProperty> systemProperties) throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		final String responseContent = "{\"id\":\"an id\",\"String\":\"Hey\"}";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(getTestFilter(responseContent));

				client = client.withFilter(new ServiceFilter() {
					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						validateUri(systemProperties, request.getUrl());

						nextServiceFilterCallback.onNext(request, responseCallback);
					}
				});

				// Create get the MobileService table
				MobileServiceJsonTable msTable = client.getTable(tableName);

				msTable.setSystemProperties(systemProperties);

				// Call the lookup method
				msTable.lookUp("an id", new TableJsonOperationCallback() {

					@Override
					public void onCompleted(JsonObject jsonObject, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						} else if (jsonObject == null) {
							container.setException(new Exception("Expected result"));
						} else {
							container.setJsonResult(jsonObject);
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();

		if (exception != null) {
			fail(exception.getMessage());
		}
	}

	public void testLookupQueryStringWithSystemPropertiesIntId() throws Throwable {
		List<EnumSet<MobileServiceSystemProperty>> allSystemProperties = SystemPropertiesTestData.AllSystemProperties;

		for (EnumSet<MobileServiceSystemProperty> systemProperties : allSystemProperties) {
			lookupQueryStringWithSystemPropertiesIntId(systemProperties);
		}
	}

	private void lookupQueryStringWithSystemPropertiesIntId(final EnumSet<MobileServiceSystemProperty> systemProperties) throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		final String responseContent = "{\"id\":5,\"String\":\"Hey\"}";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(getTestFilter(responseContent));

				client = client.withFilter(new ServiceFilter() {
					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						validateUri(systemProperties, request.getUrl());

						nextServiceFilterCallback.onNext(request, responseCallback);
					}
				});

				// Create get the MobileService table
				MobileServiceJsonTable msTable = client.getTable(tableName);

				msTable.setSystemProperties(systemProperties);

				// Call the lookup method
				msTable.lookUp(5, new TableJsonOperationCallback() {

					@Override
					public void onCompleted(JsonObject jsonObject, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						} else if (jsonObject == null) {
							container.setException(new Exception("Expected result"));
						} else {
							container.setJsonResult(jsonObject);
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();

		if (exception != null) {
			fail(exception.getMessage());
		}
	}

	public void testLookupUserParameterWithSystemProperties() throws Throwable {
		List<EnumSet<MobileServiceSystemProperty>> allSystemProperties = SystemPropertiesTestData.AllSystemProperties;

		for (EnumSet<MobileServiceSystemProperty> systemProperties : allSystemProperties) {
			lookupUserParameterWithSystemProperties(systemProperties);
		}
	}

	private void lookupUserParameterWithSystemProperties(final EnumSet<MobileServiceSystemProperty> systemProperties) throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		final String responseContent = "{\"id\":\"an id\",\"String\":\"Hey\"}";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(getTestFilter(responseContent));

				client = client.withFilter(new ServiceFilter() {
					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						assertTrue(request.getUrl().contains("__systemproperties=CreatedAt"));

						nextServiceFilterCallback.onNext(request, responseCallback);
					}
				});

				// Create get the MobileService table
				MobileServiceJsonTable msTable = client.getTable(tableName);

				msTable.setSystemProperties(systemProperties);

				List<Pair<String, String>> parameters = new ArrayList<Pair<String, String>>();
				parameters.add(new Pair<String, String>("__systemproperties", "CreatedAt"));

				// Call the lookup method
				msTable.lookUp("an id", parameters, new TableJsonOperationCallback() {

					@Override
					public void onCompleted(JsonObject jsonObject, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						} else if (jsonObject == null) {
							container.setException(new Exception("Expected result"));
						} else {
							container.setJsonResult(jsonObject);
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();

		if (exception != null) {
			fail(exception.getMessage());
		}
	}

	public void testLookupUserParameterWithSystemPropertiesIntId() throws Throwable {
		List<EnumSet<MobileServiceSystemProperty>> allSystemProperties = SystemPropertiesTestData.AllSystemProperties;

		for (EnumSet<MobileServiceSystemProperty> systemProperties : allSystemProperties) {
			lookupUserParameterWithSystemPropertiesIntId(systemProperties);
		}
	}

	private void lookupUserParameterWithSystemPropertiesIntId(final EnumSet<MobileServiceSystemProperty> systemProperties) throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		final String responseContent = "{\"id\":5,\"String\":\"Hey\"}";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(getTestFilter(responseContent));

				client = client.withFilter(new ServiceFilter() {
					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						assertTrue(request.getUrl().contains("__systemproperties=CreatedAt"));

						nextServiceFilterCallback.onNext(request, responseCallback);
					}
				});

				// Create get the MobileService table
				MobileServiceJsonTable msTable = client.getTable(tableName);

				msTable.setSystemProperties(systemProperties);

				List<Pair<String, String>> parameters = new ArrayList<Pair<String, String>>();
				parameters.add(new Pair<String, String>("__systemproperties", "CreatedAt"));

				// Call the lookup method
				msTable.lookUp(5, parameters, new TableJsonOperationCallback() {

					@Override
					public void onCompleted(JsonObject jsonObject, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						} else if (jsonObject == null) {
							container.setException(new Exception("Expected result"));
						} else {
							container.setJsonResult(jsonObject);
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();

		if (exception != null) {
			fail(exception.getMessage());
		}
	}

	// Delete Tests

	public void testDeleteQueryStringWithSystemProperties() throws Throwable {
		List<EnumSet<MobileServiceSystemProperty>> allSystemProperties = SystemPropertiesTestData.AllSystemProperties;

		for (EnumSet<MobileServiceSystemProperty> systemProperties : allSystemProperties) {
			deleteQueryStringWithSystemProperties(systemProperties);
		}
	}

	private void deleteQueryStringWithSystemProperties(final EnumSet<MobileServiceSystemProperty> systemProperties) throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		final String responseContent = "{\"id\":\"an id\",\"String\":\"Hey\"}";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(getTestFilter(responseContent));

				client = client.withFilter(new ServiceFilter() {
					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						validateUri(systemProperties, request.getUrl());

						nextServiceFilterCallback.onNext(request, responseCallback);
					}
				});

				// Create get the MobileService table
				MobileServiceJsonTable msTable = client.getTable(tableName);

				msTable.setSystemProperties(systemProperties);

				// Call the delete method
				msTable.delete("an id", new TableDeleteCallback() {

					@Override
					public void onCompleted(Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();

		if (exception != null) {
			fail(exception.getMessage());
		}
	}

	public void testDeleteQueryStringWithSystemPropertiesIntId() throws Throwable {
		List<EnumSet<MobileServiceSystemProperty>> allSystemProperties = SystemPropertiesTestData.AllSystemProperties;

		for (EnumSet<MobileServiceSystemProperty> systemProperties : allSystemProperties) {
			deleteQueryStringWithSystemPropertiesIntId(systemProperties);
		}
	}

	private void deleteQueryStringWithSystemPropertiesIntId(final EnumSet<MobileServiceSystemProperty> systemProperties) throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		final String responseContent = "{\"id\":5,\"String\":\"Hey\"}";
		final JsonObject obj = new JsonParser().parse("{\"id\":5,\"String\":\"what\"}").getAsJsonObject();

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(getTestFilter(responseContent));

				client = client.withFilter(new ServiceFilter() {
					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						validateUri(systemProperties, request.getUrl());

						nextServiceFilterCallback.onNext(request, responseCallback);
					}
				});

				// Create get the MobileService table
				MobileServiceJsonTable msTable = client.getTable(tableName);

				msTable.setSystemProperties(systemProperties);

				// Call the delete method
				msTable.delete(obj, new TableDeleteCallback() {

					@Override
					public void onCompleted(Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();

		if (exception != null) {
			fail(exception.getMessage());
		}
	}

	public void testDeleteUserParameterWithSystemProperties() throws Throwable {
		List<EnumSet<MobileServiceSystemProperty>> allSystemProperties = SystemPropertiesTestData.AllSystemProperties;

		for (EnumSet<MobileServiceSystemProperty> systemProperties : allSystemProperties) {
			deleteUserParameterWithSystemProperties(systemProperties);
		}
	}

	private void deleteUserParameterWithSystemProperties(final EnumSet<MobileServiceSystemProperty> systemProperties) throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		final String responseContent = "{\"id\":\"an id\",\"String\":\"Hey\"}";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(getTestFilter(responseContent));

				client = client.withFilter(new ServiceFilter() {
					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						assertTrue(request.getUrl().contains("__systemproperties=unknown"));

						nextServiceFilterCallback.onNext(request, responseCallback);
					}
				});

				// Create get the MobileService table
				MobileServiceJsonTable msTable = client.getTable(tableName);

				msTable.setSystemProperties(systemProperties);

				List<Pair<String, String>> parameters = new ArrayList<Pair<String, String>>();
				parameters.add(new Pair<String, String>("__systemproperties", "unknown"));

				// Call the delete method
				msTable.delete("an id", parameters, new TableDeleteCallback() {

					@Override
					public void onCompleted(Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();

		if (exception != null) {
			fail(exception.getMessage());
		}
	}

	public void testDeleteUserParameterWithSystemPropertiesIntId() throws Throwable {
		List<EnumSet<MobileServiceSystemProperty>> allSystemProperties = SystemPropertiesTestData.AllSystemProperties;

		for (EnumSet<MobileServiceSystemProperty> systemProperties : allSystemProperties) {
			deleteUserParameterWithSystemPropertiesIntId(systemProperties);
		}
	}

	private void deleteUserParameterWithSystemPropertiesIntId(final EnumSet<MobileServiceSystemProperty> systemProperties) throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		final String responseContent = "{\"id\":5,\"String\":\"Hey\"}";
		final JsonObject obj = new JsonParser().parse("{\"id\":5,\"String\":\"what\"}").getAsJsonObject();

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(getTestFilter(responseContent));

				client = client.withFilter(new ServiceFilter() {
					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						assertTrue(request.getUrl().contains("__systemproperties=unknown"));

						nextServiceFilterCallback.onNext(request, responseCallback);
					}
				});

				// Create get the MobileService table
				MobileServiceJsonTable msTable = client.getTable(tableName);

				msTable.setSystemProperties(systemProperties);

				List<Pair<String, String>> parameters = new ArrayList<Pair<String, String>>();
				parameters.add(new Pair<String, String>("__systemproperties", "unknown"));

				// Call the delete method
				msTable.delete(obj, parameters, new TableDeleteCallback() {

					@Override
					public void onCompleted(Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();

		if (exception != null) {
			fail(exception.getMessage());
		}
	}

	// Query Tests

	public void testSelectQueryStringWithSystemProperties() throws Throwable {
		List<EnumSet<MobileServiceSystemProperty>> allSystemProperties = SystemPropertiesTestData.AllSystemProperties;

		for (EnumSet<MobileServiceSystemProperty> systemProperties : allSystemProperties) {
			selectQueryStringWithSystemProperties(systemProperties);
		}
	}

	private void selectQueryStringWithSystemProperties(final EnumSet<MobileServiceSystemProperty> systemProperties) throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		final String responseContent = "{\"id\":\"an id\",\"String\":\"Hey\"}";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(getTestFilter(responseContent));

				client = client.withFilter(new ServiceFilter() {
					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						validateUri(systemProperties, request.getUrl());

						nextServiceFilterCallback.onNext(request, responseCallback);
					}
				});

				// Create get the MobileService table
				MobileServiceJsonTable msTable = client.getTable(tableName);

				msTable.setSystemProperties(systemProperties);

				// Call the execute method with projection
				msTable.select("Id", "String").execute(new TableJsonQueryCallback() {

					@Override
					public void onCompleted(JsonElement result, int count, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						} else if (result == null) {
							container.setException(new Exception("Expected result"));
						} else {
							container.setJsonResult(result);
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();

		if (exception != null) {
			fail(exception.getMessage());
		}
	}

	public void testFilterQueryStringWithSystemProperties() throws Throwable {
		List<EnumSet<MobileServiceSystemProperty>> allSystemProperties = SystemPropertiesTestData.AllSystemProperties;

		for (EnumSet<MobileServiceSystemProperty> systemProperties : allSystemProperties) {
			filterQueryStringWithSystemProperties(systemProperties);
		}
	}

	private void filterQueryStringWithSystemProperties(final EnumSet<MobileServiceSystemProperty> systemProperties) throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		final String responseContent = "{\"id\":\"an id\",\"String\":\"Hey\"}";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(getTestFilter(responseContent));

				client = client.withFilter(new ServiceFilter() {
					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						validateUri(systemProperties, request.getUrl());

						nextServiceFilterCallback.onNext(request, responseCallback);
					}
				});

				// Create get the MobileService table
				MobileServiceJsonTable msTable = client.getTable(tableName);

				msTable.setSystemProperties(systemProperties);

				// Call the execute method with filter
				msTable.where().field("id").eq().val("an id").execute(new TableJsonQueryCallback() {

					@Override
					public void onCompleted(JsonElement result, int count, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						} else if (result == null) {
							container.setException(new Exception("Expected result"));
						} else {
							container.setJsonResult(result);
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();

		if (exception != null) {
			fail(exception.getMessage());
		}
	}

	public void testSelectQueryStringWithSystemPropertiesIntId() throws Throwable {
		List<EnumSet<MobileServiceSystemProperty>> allSystemProperties = SystemPropertiesTestData.AllSystemProperties;

		for (EnumSet<MobileServiceSystemProperty> systemProperties : allSystemProperties) {
			selectQueryStringWithSystemPropertiesIntId(systemProperties);
		}
	}

	private void selectQueryStringWithSystemPropertiesIntId(final EnumSet<MobileServiceSystemProperty> systemProperties) throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		final String responseContent = "{\"id\":5,\"String\":\"Hey\"}";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(getTestFilter(responseContent));

				client = client.withFilter(new ServiceFilter() {
					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						validateUri(systemProperties, request.getUrl());

						nextServiceFilterCallback.onNext(request, responseCallback);
					}
				});

				// Create get the MobileService table
				MobileServiceJsonTable msTable = client.getTable(tableName);

				msTable.setSystemProperties(systemProperties);

				// Call the execute method with projection
				msTable.select("id", "String").execute(new TableJsonQueryCallback() {

					@Override
					public void onCompleted(JsonElement result, int count, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						} else if (result == null) {
							container.setException(new Exception("Expected result"));
						} else {
							container.setJsonResult(result);
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();

		if (exception != null) {
			fail(exception.getMessage());
		}
	}

	public void testFilterQueryStringWithSystemPropertiesIntId() throws Throwable {
		List<EnumSet<MobileServiceSystemProperty>> allSystemProperties = SystemPropertiesTestData.AllSystemProperties;

		for (EnumSet<MobileServiceSystemProperty> systemProperties : allSystemProperties) {
			filterQueryStringWithSystemPropertiesIntId(systemProperties);
		}
	}

	private void filterQueryStringWithSystemPropertiesIntId(final EnumSet<MobileServiceSystemProperty> systemProperties) throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		final String responseContent = "{\"id\":5,\"String\":\"Hey\"}";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(getTestFilter(responseContent));

				client = client.withFilter(new ServiceFilter() {
					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						validateUri(systemProperties, request.getUrl());

						nextServiceFilterCallback.onNext(request, responseCallback);
					}
				});

				// Create get the MobileService table
				MobileServiceJsonTable msTable = client.getTable(tableName);

				msTable.setSystemProperties(systemProperties);

				// Call the execute method with filter
				msTable.where().field("id").eq().val(5).execute(new TableJsonQueryCallback() {

					@Override
					public void onCompleted(JsonElement result, int count, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						} else if (result == null) {
							container.setException(new Exception("Expected result"));
						} else {
							container.setJsonResult(result);
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();

		if (exception != null) {
			fail(exception.getMessage());
		}
	}

	public void testSelectUserParameterWithSystemProperties() throws Throwable {
		List<EnumSet<MobileServiceSystemProperty>> allSystemProperties = SystemPropertiesTestData.AllSystemProperties;

		for (EnumSet<MobileServiceSystemProperty> systemProperties : allSystemProperties) {
			selectUserParameterWithSystemProperties(systemProperties);
		}
	}

	private void selectUserParameterWithSystemProperties(final EnumSet<MobileServiceSystemProperty> systemProperties) throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		final String responseContent = "{\"id\":\"an id\",\"String\":\"Hey\"}";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(getTestFilter(responseContent));

				client = client.withFilter(new ServiceFilter() {
					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						assertTrue(request.getUrl().contains("__systemproperties=__createdAt"));

						nextServiceFilterCallback.onNext(request, responseCallback);
					}
				});

				// Create get the MobileService table
				MobileServiceJsonTable msTable = client.getTable(tableName);

				msTable.setSystemProperties(systemProperties);

				// Call the execute method with projection
				msTable.select("Id", "String").parameter("__systemproperties", "__createdAt").execute(new TableJsonQueryCallback() {

					@Override
					public void onCompleted(JsonElement result, int count, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						} else if (result == null) {
							container.setException(new Exception("Expected result"));
						} else {
							container.setJsonResult(result);
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();

		if (exception != null) {
			fail(exception.getMessage());
		}
	}

	public void testFilterUserParameterWithSystemProperties() throws Throwable {
		List<EnumSet<MobileServiceSystemProperty>> allSystemProperties = SystemPropertiesTestData.AllSystemProperties;

		for (EnumSet<MobileServiceSystemProperty> systemProperties : allSystemProperties) {
			filterUserParameterWithSystemProperties(systemProperties);
		}
	}

	private void filterUserParameterWithSystemProperties(final EnumSet<MobileServiceSystemProperty> systemProperties) throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		final String responseContent = "{\"id\":\"an id\",\"String\":\"Hey\"}";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(getTestFilter(responseContent));

				client = client.withFilter(new ServiceFilter() {
					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						assertTrue(request.getUrl().contains("__systemproperties=__createdAt"));

						nextServiceFilterCallback.onNext(request, responseCallback);
					}
				});

				// Create get the MobileService table
				MobileServiceJsonTable msTable = client.getTable(tableName);

				msTable.setSystemProperties(systemProperties);

				// Call the execute method with filter
				msTable.where().field("id").eq().val("an id").parameter("__systemproperties", "__createdAt").execute(new TableJsonQueryCallback() {

					@Override
					public void onCompleted(JsonElement result, int count, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						} else if (result == null) {
							container.setException(new Exception("Expected result"));
						} else {
							container.setJsonResult(result);
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();

		if (exception != null) {
			fail(exception.getMessage());
		}
	}

	public void testSelectUserParameterWithSystemPropertiesIntId() throws Throwable {
		List<EnumSet<MobileServiceSystemProperty>> allSystemProperties = SystemPropertiesTestData.AllSystemProperties;

		for (EnumSet<MobileServiceSystemProperty> systemProperties : allSystemProperties) {
			selectUserParameterWithSystemPropertiesIntId(systemProperties);
		}
	}

	private void selectUserParameterWithSystemPropertiesIntId(final EnumSet<MobileServiceSystemProperty> systemProperties) throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		final String responseContent = "{\"id\":5,\"String\":\"Hey\"}";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(getTestFilter(responseContent));

				client = client.withFilter(new ServiceFilter() {
					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						assertTrue(request.getUrl().contains("__systemproperties=__createdAt"));

						nextServiceFilterCallback.onNext(request, responseCallback);
					}
				});

				// Create get the MobileService table
				MobileServiceJsonTable msTable = client.getTable(tableName);

				msTable.setSystemProperties(systemProperties);

				// Call the execute method with projection
				msTable.select("id", "String").parameter("__systemproperties", "__createdAt").execute(new TableJsonQueryCallback() {

					@Override
					public void onCompleted(JsonElement result, int count, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						} else if (result == null) {
							container.setException(new Exception("Expected result"));
						} else {
							container.setJsonResult(result);
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();

		if (exception != null) {
			fail(exception.getMessage());
		}
	}

	public void testFilterUserParameterWithSystemPropertiesIntId() throws Throwable {
		List<EnumSet<MobileServiceSystemProperty>> allSystemProperties = SystemPropertiesTestData.AllSystemProperties;

		for (EnumSet<MobileServiceSystemProperty> systemProperties : allSystemProperties) {
			filterUserParameterWithSystemPropertiesIntId(systemProperties);
		}
	}

	private void filterUserParameterWithSystemPropertiesIntId(final EnumSet<MobileServiceSystemProperty> systemProperties) throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		final String responseContent = "{\"id\":5,\"String\":\"Hey\"}";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(getTestFilter(responseContent));

				client = client.withFilter(new ServiceFilter() {
					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						assertTrue(request.getUrl().contains("__systemproperties=__createdAt"));

						nextServiceFilterCallback.onNext(request, responseCallback);
					}
				});

				// Create get the MobileService table
				MobileServiceJsonTable msTable = client.getTable(tableName);

				msTable.setSystemProperties(systemProperties);

				// Call the execute method with filter
				msTable.where().field("id").eq().val(5).parameter("__systemproperties", "__createdAt").execute(new TableJsonQueryCallback() {

					@Override
					public void onCompleted(JsonElement result, int count, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						} else if (result == null) {
							container.setException(new Exception("Expected result"));
						} else {
							container.setJsonResult(result);
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();

		if (exception != null) {
			fail(exception.getMessage());
		}
	}

	// Generic

	// Insert Tests

	// String Id Type

	public void testInsertRemovesCreatedAtSerializedNameStringId() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String responseContent = "{\"id\":\"an id\"}";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(getTestFilter(responseContent));

				client = client.withFilter(new ServiceFilter() {
					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {
						String content = request.getContent();
						JsonObject obj = new JsonParser().parse(content).getAsJsonObject();

						Map<String, JsonElement> properties = new TreeMap<String, JsonElement>(String.CASE_INSENSITIVE_ORDER);

						for (Entry<String, JsonElement> entry : obj.entrySet()) {
							properties.put(entry.getKey(), entry.getValue());
						}

						assertTrue(properties.containsKey("id"));
						assertFalse(properties.containsKey("__createdAt"));

						nextServiceFilterCallback.onNext(request, responseCallback);
					}
				});

				// Create get the MobileService table
				MobileServiceTable<CreatedAtType> msTable = client.getTable(CreatedAtType.class);

				CreatedAtType element = new CreatedAtType();
				element.Id = "an id";
				element.CreatedAt = new Date(2012, 0, 18);

				// Call the insert method
				msTable.insert(element, new TableOperationCallback<CreatedAtType>() {

					@Override
					public void onCompleted(CreatedAtType entity, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						} else if (entity == null) {
							container.setException(new Exception("Expected result"));
						} else {
							container.setCustomResult(entity);
						}

						latch.countDown();

					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();

		if (exception != null) {
			fail(exception.getMessage());
		}
	}

	public void testInsertRemovesCreatedAtPropertyNameStringId() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String responseContent = "{\"id\":\"an id\"}";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(getTestFilter(responseContent));

				client = client.withFilter(new ServiceFilter() {
					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {
						String content = request.getContent();
						JsonObject obj = new JsonParser().parse(content).getAsJsonObject();

						Map<String, JsonElement> properties = new TreeMap<String, JsonElement>(String.CASE_INSENSITIVE_ORDER);

						for (Entry<String, JsonElement> entry : obj.entrySet()) {
							properties.put(entry.getKey(), entry.getValue());
						}

						assertTrue(properties.containsKey("id"));
						assertFalse(properties.containsKey("__createdAt"));

						nextServiceFilterCallback.onNext(request, responseCallback);
					}
				});

				// Create get the MobileService table
				MobileServiceTable<NamedSystemPropertiesType> msTable = client.getTable(NamedSystemPropertiesType.class);

				NamedSystemPropertiesType element = new NamedSystemPropertiesType();
				element.Id = "an id";
				element.__createdAt = new Date(2012, 0, 18);

				// Call the insert method
				msTable.insert(element, new TableOperationCallback<NamedSystemPropertiesType>() {

					@Override
					public void onCompleted(NamedSystemPropertiesType entity, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						} else if (entity == null) {
							container.setException(new Exception("Expected result"));
						} else {
							container.setCustomResult(entity);
						}

						latch.countDown();

					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();

		if (exception != null) {
			fail(exception.getMessage());
		}
	}

	public void testInsertRemovesUpdatedAtSerializedNameStringId() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String responseContent = "{\"id\":\"an id\"}";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(getTestFilter(responseContent));

				client = client.withFilter(new ServiceFilter() {
					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {
						String content = request.getContent();
						JsonObject obj = new JsonParser().parse(content).getAsJsonObject();

						Map<String, JsonElement> properties = new TreeMap<String, JsonElement>(String.CASE_INSENSITIVE_ORDER);

						for (Entry<String, JsonElement> entry : obj.entrySet()) {
							properties.put(entry.getKey(), entry.getValue());
						}

						assertTrue(properties.containsKey("id"));
						assertFalse(properties.containsKey("__updatedAt"));

						nextServiceFilterCallback.onNext(request, responseCallback);
					}
				});

				// Create get the MobileService table
				MobileServiceTable<UpdatedAtType> msTable = client.getTable(UpdatedAtType.class);

				UpdatedAtType element = new UpdatedAtType();
				element.Id = "an id";
				element.UpdatedAt = new Date(2012, 0, 18);

				// Call the insert method
				msTable.insert(element, new TableOperationCallback<UpdatedAtType>() {

					@Override
					public void onCompleted(UpdatedAtType entity, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						} else if (entity == null) {
							container.setException(new Exception("Expected result"));
						} else {
							container.setCustomResult(entity);
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();

		if (exception != null) {
			fail(exception.getMessage());
		}
	}

	public void testInsertRemovesVersionSerializedNameStringId() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String responseContent = "{\"id\":\"an id\"}";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(getTestFilter(responseContent));

				client = client.withFilter(new ServiceFilter() {
					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {
						String content = request.getContent();
						JsonObject obj = new JsonParser().parse(content).getAsJsonObject();

						Map<String, JsonElement> properties = new TreeMap<String, JsonElement>(String.CASE_INSENSITIVE_ORDER);

						for (Entry<String, JsonElement> entry : obj.entrySet()) {
							properties.put(entry.getKey(), entry.getValue());
						}

						assertTrue(properties.containsKey("id"));
						assertFalse(properties.containsKey("__version"));

						nextServiceFilterCallback.onNext(request, responseCallback);
					}
				});

				// Create get the MobileService table
				MobileServiceTable<VersionType> msTable = client.getTable(VersionType.class);

				VersionType element = new VersionType();
				element.Id = "an id";
				element.Version = "a version";

				// Call the insert method
				msTable.insert(element, new TableOperationCallback<VersionType>() {

					@Override
					public void onCompleted(VersionType entity, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						} else if (entity == null) {
							container.setException(new Exception("Expected result"));
						} else {
							container.setCustomResult(entity);
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();

		if (exception != null) {
			fail(exception.getMessage());
		}
	}

	public void testInsertRemovesAllSystemSerializedNameStringId() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String responseContent = "{\"id\":\"an id\"}";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(getTestFilter(responseContent));

				client = client.withFilter(new ServiceFilter() {
					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {
						String content = request.getContent();
						JsonObject obj = new JsonParser().parse(content).getAsJsonObject();

						Map<String, JsonElement> properties = new TreeMap<String, JsonElement>(String.CASE_INSENSITIVE_ORDER);

						for (Entry<String, JsonElement> entry : obj.entrySet()) {
							properties.put(entry.getKey(), entry.getValue());
						}

						assertTrue(properties.containsKey("id"));
						assertFalse(properties.containsKey("__createdAt"));
						assertFalse(properties.containsKey("__updatedAt"));
						assertFalse(properties.containsKey("__version"));

						nextServiceFilterCallback.onNext(request, responseCallback);
					}
				});

				// Create get the MobileService table
				MobileServiceTable<AllSystemPropertiesType> msTable = client.getTable(AllSystemPropertiesType.class);

				AllSystemPropertiesType element = new AllSystemPropertiesType();
				element.Id = "an id";
				element.CreatedAt = new Date(2012, 0, 18);
				element.UpdatedAt = new Date(2012, 0, 18);
				element.Version = "a version";

				// Call the insert method
				msTable.insert(element, new TableOperationCallback<AllSystemPropertiesType>() {

					@Override
					public void onCompleted(AllSystemPropertiesType entity, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						} else if (entity == null) {
							container.setException(new Exception("Expected result"));
						} else {
							container.setCustomResult(entity);
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();

		if (exception != null) {
			fail(exception.getMessage());
		}
	}

	public void testInsertDoesNotRemoveNonSystemCreatedAtPropertyNameStringId() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String responseContent = "{\"id\":\"an id\"}";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(getTestFilter(responseContent));

				client = client.withFilter(new ServiceFilter() {
					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {
						String content = request.getContent();
						JsonObject obj = new JsonParser().parse(content).getAsJsonObject();

						Map<String, JsonElement> properties = new TreeMap<String, JsonElement>(String.CASE_INSENSITIVE_ORDER);

						for (Entry<String, JsonElement> entry : obj.entrySet()) {
							properties.put(entry.getKey(), entry.getValue());
						}

						assertTrue(properties.containsKey("id"));
						assertTrue(properties.containsKey("CreatedAt"));

						nextServiceFilterCallback.onNext(request, responseCallback);
					}
				});

				// Create get the MobileService table
				MobileServiceTable<NotSystemPropertyCreatedAtType> msTable = client.getTable(NotSystemPropertyCreatedAtType.class);

				NotSystemPropertyCreatedAtType element = new NotSystemPropertyCreatedAtType();
				element.Id = "an id";
				element.CreatedAt = new Date(2012, 0, 18);

				// Call the insert method
				msTable.insert(element, new TableOperationCallback<NotSystemPropertyCreatedAtType>() {

					@Override
					public void onCompleted(NotSystemPropertyCreatedAtType entity, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						} else if (entity == null) {
							container.setException(new Exception("Expected result"));
						} else {
							container.setCustomResult(entity);
						}

						latch.countDown();

					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();

		if (exception != null) {
			fail(exception.getMessage());
		}
	}

	public void testInsertDoesNotRemoveNonSystemUpdatedAtPropertyNameStringId() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String responseContent = "{\"id\":\"an id\"}";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(getTestFilter(responseContent));

				client = client.withFilter(new ServiceFilter() {
					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {
						String content = request.getContent();
						JsonObject obj = new JsonParser().parse(content).getAsJsonObject();

						Map<String, JsonElement> properties = new TreeMap<String, JsonElement>(String.CASE_INSENSITIVE_ORDER);

						for (Entry<String, JsonElement> entry : obj.entrySet()) {
							properties.put(entry.getKey(), entry.getValue());
						}

						assertTrue(properties.containsKey("id"));
						assertTrue(properties.containsKey("_UpdatedAt"));

						nextServiceFilterCallback.onNext(request, responseCallback);
					}
				});

				// Create get the MobileService table
				MobileServiceTable<NotSystemPropertyUpdatedAtType> msTable = client.getTable(NotSystemPropertyUpdatedAtType.class);

				NotSystemPropertyUpdatedAtType element = new NotSystemPropertyUpdatedAtType();
				element.Id = "an id";
				element._UpdatedAt = new Date(2012, 0, 18);

				// Call the insert method
				msTable.insert(element, new TableOperationCallback<NotSystemPropertyUpdatedAtType>() {

					@Override
					public void onCompleted(NotSystemPropertyUpdatedAtType entity, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						} else if (entity == null) {
							container.setException(new Exception("Expected result"));
						} else {
							container.setCustomResult(entity);
						}

						latch.countDown();

					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();

		if (exception != null) {
			fail(exception.getMessage());
		}
	}

	public void testInsertDoesNotRemoveNonSystemVersionPropertyNameStringId() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String responseContent = "{\"id\":\"an id\"}";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(getTestFilter(responseContent));

				client = client.withFilter(new ServiceFilter() {
					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {
						String content = request.getContent();
						JsonObject obj = new JsonParser().parse(content).getAsJsonObject();

						Map<String, JsonElement> properties = new TreeMap<String, JsonElement>(String.CASE_INSENSITIVE_ORDER);

						for (Entry<String, JsonElement> entry : obj.entrySet()) {
							properties.put(entry.getKey(), entry.getValue());
						}

						assertTrue(properties.containsKey("id"));
						assertTrue(properties.containsKey("version"));

						nextServiceFilterCallback.onNext(request, responseCallback);
					}
				});

				// Create get the MobileService table
				MobileServiceTable<NotSystemPropertyVersionType> msTable = client.getTable(NotSystemPropertyVersionType.class);

				NotSystemPropertyVersionType element = new NotSystemPropertyVersionType();
				element.Id = "an id";
				element.version = "a version";

				// Call the insert method
				msTable.insert(element, new TableOperationCallback<NotSystemPropertyVersionType>() {

					@Override
					public void onCompleted(NotSystemPropertyVersionType entity, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						} else if (entity == null) {
							container.setException(new Exception("Expected result"));
						} else {
							container.setCustomResult(entity);
						}

						latch.countDown();

					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();

		if (exception != null) {
			fail(exception.getMessage());
		}
	}

	//

	public void testSystemPropertiesPropertySetCorrectly() throws Throwable {
		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				MobileServiceTable<StringIdType> stringIdTable = client.getTable(StringIdType.class);
				assertTrue(stringIdTable.getSystemProperties().isEmpty());

				MobileServiceTable<StringType> stringTable = client.getTable(StringType.class);
				assertTrue(stringTable.getSystemProperties().isEmpty());

				MobileServiceTable<NotSystemPropertyCreatedAtType> notSystemPropertyTable = client.getTable(NotSystemPropertyCreatedAtType.class);
				assertTrue(notSystemPropertyTable.getSystemProperties().isEmpty());

				MobileServiceTable<IntegerIdNotSystemPropertyCreatedAtType> integerIdNotsystemPropertyTable = client
						.getTable(IntegerIdNotSystemPropertyCreatedAtType.class);
				assertTrue(integerIdNotsystemPropertyTable.getSystemProperties().isEmpty());

				MobileServiceTable<NotSystemPropertyUpdatedAtType> notSystemPropertyUpdatedTable = client.getTable(NotSystemPropertyUpdatedAtType.class);
				assertTrue(notSystemPropertyUpdatedTable.getSystemProperties().isEmpty());

				MobileServiceTable<NotSystemPropertyVersionType> notSystemPropertyVersionTable = client.getTable(NotSystemPropertyVersionType.class);
				assertTrue(notSystemPropertyVersionTable.getSystemProperties().isEmpty());

				MobileServiceTable<IntegerIdWithNamedSystemPropertiesType> integerIdWithNamedSystemPropertyTable = client
						.getTable(IntegerIdWithNamedSystemPropertiesType.class);
				assertTrue(integerIdWithNamedSystemPropertyTable.getSystemProperties().isEmpty());

				MobileServiceTable<LongIdWithNamedSystemPropertiesType> longIdWithNamedSystemPropertyTable = client
						.getTable(LongIdWithNamedSystemPropertiesType.class);
				assertTrue(longIdWithNamedSystemPropertyTable.getSystemProperties().isEmpty());

				MobileServiceTable<CreatedAtType> createdAtTable = client.getTable(CreatedAtType.class);
				assertTrue(createdAtTable.getSystemProperties().size() == 1
						&& createdAtTable.getSystemProperties().contains(MobileServiceSystemProperty.CreatedAt));

				MobileServiceTable<DoubleNamedSystemPropertiesType> doubleNamedCreatedAtTable = client.getTable(DoubleNamedSystemPropertiesType.class);
				assertTrue(doubleNamedCreatedAtTable.getSystemProperties().size() == 1
						&& doubleNamedCreatedAtTable.getSystemProperties().contains(MobileServiceSystemProperty.CreatedAt));

				MobileServiceTable<NamedSystemPropertiesType> namedCreatedAtTable = client.getTable(NamedSystemPropertiesType.class);
				assertTrue(namedCreatedAtTable.getSystemProperties().size() == 1
						&& namedCreatedAtTable.getSystemProperties().contains(MobileServiceSystemProperty.CreatedAt));

				MobileServiceTable<NamedDifferentCasingSystemPropertiesType> namedDifferentCasingCreatedAtTable = client
						.getTable(NamedDifferentCasingSystemPropertiesType.class);
				assertTrue(namedDifferentCasingCreatedAtTable.getSystemProperties().size() == 1
						&& namedDifferentCasingCreatedAtTable.getSystemProperties().contains(MobileServiceSystemProperty.CreatedAt));

				MobileServiceTable<UpdatedAtType> updatedAtTable = client.getTable(UpdatedAtType.class);
				assertTrue(updatedAtTable.getSystemProperties().size() == 1
						&& updatedAtTable.getSystemProperties().contains(MobileServiceSystemProperty.UpdatedAt));

				MobileServiceTable<VersionType> versionTable = client.getTable(VersionType.class);
				assertTrue(versionTable.getSystemProperties().size() == 1 && versionTable.getSystemProperties().contains(MobileServiceSystemProperty.Version));

				MobileServiceTable<AllSystemPropertiesType> allsystemPropertiesTable = client.getTable(AllSystemPropertiesType.class);
				assertTrue(allsystemPropertiesTable.getSystemProperties().size() == 3
						&& allsystemPropertiesTable.getSystemProperties().contains(MobileServiceSystemProperty.Version)
						&& allsystemPropertiesTable.getSystemProperties().contains(MobileServiceSystemProperty.CreatedAt)
						&& allsystemPropertiesTable.getSystemProperties().contains(MobileServiceSystemProperty.UpdatedAt));
			}
		});
	}

	public void testLookupDeserializesCreateAtToDate() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String responseContent = "{\"id\":\"an id\",\"__createdAt\":\"2000-01-01T07:59:59.000Z\"}";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(getTestFilter(responseContent));

				// Create get the MobileService table
				MobileServiceTable<CreatedAtType> msTable = client.getTable(CreatedAtType.class);

				// Call the insert method
				msTable.lookUp("an id", new TableOperationCallback<CreatedAtType>() {

					@Override
					public void onCompleted(CreatedAtType entity, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						} else if (entity == null) {
							container.setException(new Exception("Expected result"));
						} else {
							container.setCustomResult(entity);
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();

		if (exception != null) {
			fail(exception.getMessage());
		} else {
			Object result = container.getCustomResult();
			assertTrue(result instanceof CreatedAtType);

			CreatedAtType elem = (CreatedAtType) result;

			assertEquals("an id", elem.Id);
			assertEquals(new Date(Date.UTC(100, 00, 01, 07, 59, 59)), elem.CreatedAt);
		}
	}

	public void testLookupDeserializesCreateAtToString() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String responseContent = "{\"id\":\"an id\",\"__createdAt\":\"2000-01-01T07:59:59.000Z\"}";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(getTestFilter(responseContent));

				// Create get the MobileService table
				MobileServiceTable<StringCreatedAtType> msTable = client.getTable(StringCreatedAtType.class);

				// Call the insert method
				msTable.lookUp("an id", new TableOperationCallback<StringCreatedAtType>() {

					@Override
					public void onCompleted(StringCreatedAtType entity, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						} else if (entity == null) {
							container.setException(new Exception("Expected result"));
						} else {
							container.setCustomResult(entity);
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();

		if (exception != null) {
			fail(exception.getMessage());
		} else {
			Object result = container.getCustomResult();
			assertTrue(result instanceof StringCreatedAtType);

			StringCreatedAtType elem = (StringCreatedAtType) result;

			assertEquals("an id", elem.Id);
			assertEquals("2000-01-01T07:59:59.000Z", elem.CreatedAt);
		}
	}

	public void testLookupDeserializesUpdateAtToDate() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String responseContent = "{\"id\":\"an id\",\"__updatedAt\":\"2000-01-01T07:59:59.000Z\"}";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(getTestFilter(responseContent));

				// Create get the MobileService table
				MobileServiceTable<UpdatedAtType> msTable = client.getTable(UpdatedAtType.class);

				// Call the insert method
				msTable.lookUp("an id", new TableOperationCallback<UpdatedAtType>() {

					@Override
					public void onCompleted(UpdatedAtType entity, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						} else if (entity == null) {
							container.setException(new Exception("Expected result"));
						} else {
							container.setCustomResult(entity);
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();

		if (exception != null) {
			fail(exception.getMessage());
		} else {
			Object result = container.getCustomResult();
			assertTrue(result instanceof UpdatedAtType);

			UpdatedAtType elem = (UpdatedAtType) result;

			assertEquals("an id", elem.Id);
			assertEquals(new Date(Date.UTC(100, 00, 01, 07, 59, 59)), elem.UpdatedAt);
		}
	}

	public void testLookupDeserializesUpdateAtToString() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String responseContent = "{\"id\":\"an id\",\"__updatedAt\":\"2000-01-01T07:59:59.000Z\"}";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(getTestFilter(responseContent));

				// Create get the MobileService table
				MobileServiceTable<StringUpdatedAtType> msTable = client.getTable(StringUpdatedAtType.class);

				// Call the insert method
				msTable.lookUp("an id", new TableOperationCallback<StringUpdatedAtType>() {

					@Override
					public void onCompleted(StringUpdatedAtType entity, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						} else if (entity == null) {
							container.setException(new Exception("Expected result"));
						} else {
							container.setCustomResult(entity);
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();

		if (exception != null) {
			fail(exception.getMessage());
		} else {
			Object result = container.getCustomResult();
			assertTrue(result instanceof StringUpdatedAtType);

			StringUpdatedAtType elem = (StringUpdatedAtType) result;

			assertEquals("an id", elem.Id);
			assertEquals("2000-01-01T07:59:59.000Z", elem.UpdatedAt);
		}
	}

	public void testLookupDeserializesVersionToString() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String responseContent = "{\"id\":\"an id\",\"__version\":\"AAAAAAAAH2o=\"}";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(getTestFilter(responseContent));

				// Create get the MobileService table
				MobileServiceTable<VersionType> msTable = client.getTable(VersionType.class);

				// Call the insert method
				msTable.lookUp("an id", new TableOperationCallback<VersionType>() {

					@Override
					public void onCompleted(VersionType entity, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						} else if (entity == null) {
							container.setException(new Exception("Expected result"));
						} else {
							container.setCustomResult(entity);
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();

		if (exception != null) {
			fail(exception.getMessage());
		} else {
			Object result = container.getCustomResult();
			assertTrue(result instanceof VersionType);

			VersionType elem = (VersionType) result;

			assertEquals("an id", elem.Id);
			assertEquals("AAAAAAAAH2o=", elem.Version);
		}
	}

	public void testUpdateSetsIfMatchWithVersion() throws Throwable {
		List<Pair<String, String>> versions = SystemPropertiesTestData.VersionsSerialize;

		for (Pair<String, String> version : versions) {
			updateSetsIfMatchWithVersion(version);
		}
	}

	private void updateSetsIfMatchWithVersion(final Pair<String, String> version) throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String responseContent = "{\"id\":\"an id\",\"__version\":\"AAAAAAAAH2o=\"}";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(getTestFilter(responseContent));

				client = client.withFilter(new ServiceFilter() {
					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {
						boolean hasHeaderIfMatch = false;

						for (Header header : request.getHeaders()) {
							if (header.getName().equalsIgnoreCase("If-Match")) {
								assertTrue(header.getValue().equalsIgnoreCase(version.second));

								hasHeaderIfMatch = true;
							}
						}

						assertTrue(hasHeaderIfMatch);

						nextServiceFilterCallback.onNext(request, responseCallback);
					}
				});

				// Create get the MobileService table
				MobileServiceTable<VersionType> msTable = client.getTable(VersionType.class);

				VersionType element = new VersionType();
				element.Id = "an id";
				element.Version = version.first;

				// Call the insert method
				msTable.update(element, new TableOperationCallback<VersionType>() {

					@Override
					public void onCompleted(VersionType entity, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						} else if (entity == null) {
							container.setException(new Exception("Expected result"));
						} else {
							container.setCustomResult(entity);
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();

		if (exception != null) {
			fail(exception.getMessage());
		}
	}

	public void testUpdateSetsVersionWithETag() throws Throwable {
		List<Pair<String, String>> versions = SystemPropertiesTestData.VersionsDeserialize;

		for (Pair<String, String> version : versions) {
			updateSetsVersionWithEtag(version);
		}
	}

	private void updateSetsVersionWithEtag(final Pair<String, String> version) throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String responseContent = "{\"id\":\"an id\"}";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(new ServiceFilter() {
					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {
						// Create a mock response simulating an error
						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setStatus(new StatusLineMock(200));
						response.setContent(responseContent);
						response.setHeaders(new Header[] { new Header() {

							@Override
							public String getValue() {
								return version.second;
							}

							@Override
							public String getName() {
								return "ETag";
							}

							@Override
							public HeaderElement[] getElements() throws ParseException {
								return null;
							}
						} });

						// create a mock request to replace the existing one
						ServiceFilterRequestMock requestMock = new ServiceFilterRequestMock(response);
						nextServiceFilterCallback.onNext(requestMock, responseCallback);
					}
				});

				// Create get the MobileService table
				MobileServiceTable<VersionType> msTable = client.getTable(VersionType.class);

				VersionType element = new VersionType();
				element.Id = "an id";

				// Call the insert method
				msTable.update(element, new TableOperationCallback<VersionType>() {

					@Override
					public void onCompleted(VersionType entity, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						} else if (entity == null) {
							container.setException(new Exception("Expected result"));
						} else {
							container.setCustomResult(entity);
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();

		if (exception != null) {
			fail(exception.getMessage());
		} else {
			Object result = container.getCustomResult();
			assertTrue(result instanceof VersionType);

			VersionType elem = (VersionType) result;

			assertEquals("an id", elem.Id);
			assertEquals(version.first, elem.Version);
		}

	}

	// Test Filter

	private ServiceFilter getTestFilter(String content) {
		return getTestFilter(200, content);
	}

	private ServiceFilter getTestFilter(final int statusCode, final String content) {
		return new ServiceFilter() {

			@Override
			public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
					ServiceFilterResponseCallback responseCallback) {

				// Create a mock response simulating an error
				ServiceFilterResponseMock response = new ServiceFilterResponseMock();
				response.setStatus(new StatusLineMock(statusCode));
				response.setContent(content);

				// create a mock request to replace the existing one
				ServiceFilterRequestMock requestMock = new ServiceFilterRequestMock(response);
				nextServiceFilterCallback.onNext(requestMock, responseCallback);
			}
		};
	}

	private void validateUri(EnumSet<MobileServiceSystemProperty> systemProperties, String requestUri) {
		if (!systemProperties.isEmpty()) {
			assertTrue(requestUri.contains("__systemproperties"));
		} else {
			assertFalse(requestUri.contains("__systemproperties"));
		}

		if (EnumSet.complementOf(systemProperties).isEmpty()) {
			assertTrue(requestUri.contains("__systemproperties=*"));
		} else if (systemProperties.contains(MobileServiceSystemProperty.CreatedAt)) {
			assertTrue(requestUri.contains("__createdAt"));
		} else if (systemProperties.contains(MobileServiceSystemProperty.UpdatedAt)) {
			assertTrue(requestUri.contains("__updatedAt"));
		} else if (systemProperties.contains(MobileServiceSystemProperty.Version)) {
			assertTrue(requestUri.contains("__version"));
		}
	};
}
