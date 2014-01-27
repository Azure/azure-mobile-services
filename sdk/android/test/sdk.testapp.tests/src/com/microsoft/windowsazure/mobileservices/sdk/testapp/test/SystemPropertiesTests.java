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
import java.util.EnumSet;
import java.util.List;
import java.util.Map;
import java.util.Map.Entry;
import java.util.TreeMap;
import java.util.concurrent.CountDownLatch;

import android.test.InstrumentationTestCase;
import android.util.Pair;

import com.google.gson.GsonBuilder;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceJsonTable;
import com.microsoft.windowsazure.mobileservices.MobileServiceSystemProperty;
import com.microsoft.windowsazure.mobileservices.NextServiceFilterCallback;
import com.microsoft.windowsazure.mobileservices.ServiceFilter;
import com.microsoft.windowsazure.mobileservices.ServiceFilterRequest;
import com.microsoft.windowsazure.mobileservices.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.ServiceFilterResponseCallback;
import com.microsoft.windowsazure.mobileservices.TableDeleteCallback;
import com.microsoft.windowsazure.mobileservices.TableJsonOperationCallback;
import com.microsoft.windowsazure.mobileservices.TableJsonQueryCallback;

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
				
				List<Pair<String,String>> parameters = new ArrayList<Pair<String,String>>();
				parameters.add(new Pair<String,String>("__systemproperties", "__createdAt"));

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
				
				List<Pair<String,String>> parameters = new ArrayList<Pair<String,String>>();
				parameters.add(new Pair<String,String>("__systemproperties", "__createdAt"));

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
				
				List<Pair<String,String>> parameters = new ArrayList<Pair<String,String>>();
				parameters.add(new Pair<String,String>("__systemproperties", "createdAt"));

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
				
				List<Pair<String,String>> parameters = new ArrayList<Pair<String,String>>();
				parameters.add(new Pair<String,String>("__systemproperties", "createdAt"));

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
				
				List<Pair<String,String>> parameters = new ArrayList<Pair<String,String>>();
				parameters.add(new Pair<String,String>("__systemproperties", "CreatedAt"));

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
				
				List<Pair<String,String>> parameters = new ArrayList<Pair<String,String>>();
				parameters.add(new Pair<String,String>("__systemproperties", "CreatedAt"));

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
				
				List<Pair<String,String>> parameters = new ArrayList<Pair<String,String>>();
				parameters.add(new Pair<String,String>("__systemproperties", "unknown"));

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
				
				List<Pair<String,String>> parameters = new ArrayList<Pair<String,String>>();
				parameters.add(new Pair<String,String>("__systemproperties", "unknown"));

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
				msTable.select("Id","String").execute(new TableJsonQueryCallback() {

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
				msTable.select("id","String").execute(new TableJsonQueryCallback() {

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
				msTable.select("Id","String").parameter("__systemproperties", "__createdAt").execute(new TableJsonQueryCallback() {

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
				msTable.select("id","String").parameter("__systemproperties", "__createdAt").execute(new TableJsonQueryCallback() {

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

	/*
	 * public void testInsertWithStringIdTypeAndStringIdResponseContent() throws
	 * Throwable { String[] testIdData =
	 * IdTestData.concat(IdTestData.concat(IdTestData.ValidStringIds,
	 * IdTestData.EmptyStringIds), IdTestData.InvalidStringIds);
	 * 
	 * for (String testId : testIdData) {
	 * insertWithStringIdTypeAndStringIdResponseContent(testId); } }
	 * 
	 * private void insertWithStringIdTypeAndStringIdResponseContent(String
	 * testId) throws Throwable { final CountDownLatch latch = new
	 * CountDownLatch(1);
	 * 
	 * // Container to store callback's results and do the asserts. final
	 * ResultsContainer container = new ResultsContainer();
	 * 
	 * final String tableName = "MyTableName";
	 * 
	 * String jsonTestId = testId.replace("\\", "\\\\").replace("\"", "\\\"");
	 * final String responseContent = "{\"id\":\"" + jsonTestId +
	 * "\",\"String\":\"Hey\"}";
	 * 
	 * final StringIdType item = new StringIdType(); item.Id = "an id";
	 * item.String = "what?";
	 * 
	 * runTestOnUiThread(new Runnable() {
	 * 
	 * @Override public void run() { MobileServiceClient client = null;
	 * 
	 * try { client = new MobileServiceClient(appUrl, appKey,
	 * getInstrumentation().getTargetContext()); } catch (MalformedURLException
	 * e) { e.printStackTrace(); }
	 * 
	 * // Add a filter to handle the request and create a new json // object
	 * with an id defined client =
	 * client.withFilter(getTestFilter(responseContent));
	 * 
	 * // Create get the MobileService table MobileServiceTable<StringIdType>
	 * msTable = client.getTable(tableName, StringIdType.class);
	 * 
	 * // Call the insert method msTable.insert(item, new
	 * TableOperationCallback<StringIdType>() {
	 * 
	 * @Override public void onCompleted(StringIdType entity, Exception
	 * exception, ServiceFilterResponse response) { if (exception != null) {
	 * container.setException(exception); }
	 * 
	 * latch.countDown(); } }); } });
	 * 
	 * latch.await();
	 * 
	 * // Asserts Exception exception = container.getException();
	 * 
	 * if (exception != null) { fail(exception.getMessage()); } else {
	 * assertEquals(testId, item.Id); assertEquals("Hey", item.String); } }
	 */

	// Integer Id Type

	/*
	 * public void testInsertWithIntIdTypeAndIntIdResponseContent() throws
	 * Throwable { long[] testIdData = IdTestData.concat(IdTestData.ValidIntIds,
	 * IdTestData.InvalidIntIds);
	 * 
	 * for (long testId : testIdData) {
	 * insertWithIntIdTypeAndIntIdResponseContent(testId); } }
	 * 
	 * private void insertWithIntIdTypeAndIntIdResponseContent(long testId)
	 * throws Throwable { final CountDownLatch latch = new CountDownLatch(1);
	 * 
	 * // Container to store callback's results and do the asserts. final
	 * ResultsContainer container = new ResultsContainer();
	 * 
	 * final String tableName = "MyTableName";
	 * 
	 * String stringTestId = String.valueOf(testId); final String
	 * responseContent = "{\"id\":" + stringTestId + ",\"String\":\"Hey\"}";
	 * 
	 * final LongIdType item = new LongIdType(); item.Id = 0; item.String =
	 * "what?";
	 * 
	 * runTestOnUiThread(new Runnable() {
	 * 
	 * @Override public void run() { MobileServiceClient client = null;
	 * 
	 * try { client = new MobileServiceClient(appUrl, appKey,
	 * getInstrumentation().getTargetContext()); } catch (MalformedURLException
	 * e) { e.printStackTrace(); }
	 * 
	 * // Add a filter to handle the request and create a new json // object
	 * with an id defined client =
	 * client.withFilter(getTestFilter(responseContent));
	 * 
	 * // Create get the MobileService table MobileServiceTable<LongIdType>
	 * msTable = client.getTable(tableName, LongIdType.class);
	 * 
	 * // Call the insert method msTable.insert(item, new
	 * TableOperationCallback<LongIdType>() {
	 * 
	 * @Override public void onCompleted(LongIdType entity, Exception exception,
	 * ServiceFilterResponse response) { if (exception != null) {
	 * container.setException(exception); }
	 * 
	 * latch.countDown(); } }); } });
	 * 
	 * latch.await();
	 * 
	 * // Asserts Exception exception = container.getException();
	 * 
	 * if (exception != null) { fail(exception.getMessage()); } else {
	 * assertEquals(testId, item.Id); assertEquals("Hey", item.String); } }
	 */

	// Update Tests

	// String Id Type

	/*
	 * public void testUpdateWithStringIdTypeAndStringIdResponseContent() throws
	 * Throwable { String[] testIdData =
	 * IdTestData.concat(IdTestData.concat(IdTestData.ValidStringIds,
	 * IdTestData.EmptyStringIds), IdTestData.InvalidStringIds);
	 * 
	 * for (String testId : testIdData) {
	 * updateWithStringIdTypeAndStringIdResponseContent(testId); } }
	 * 
	 * private void updateWithStringIdTypeAndStringIdResponseContent(String
	 * testId) throws Throwable { final CountDownLatch latch = new
	 * CountDownLatch(1);
	 * 
	 * // Container to store callback's results and do the asserts. final
	 * ResultsContainer container = new ResultsContainer();
	 * 
	 * final String tableName = "MyTableName";
	 * 
	 * String jsonTestId = testId.replace("\\", "\\\\").replace("\"", "\\\"");
	 * final String responseContent = "{\"id\":\"" + jsonTestId +
	 * "\",\"String\":\"Hey\"}";
	 * 
	 * final StringIdType item = new StringIdType(); item.Id = "an id";
	 * item.String = "what?";
	 * 
	 * runTestOnUiThread(new Runnable() {
	 * 
	 * @Override public void run() { MobileServiceClient client = null;
	 * 
	 * try { client = new MobileServiceClient(appUrl, appKey,
	 * getInstrumentation().getTargetContext()); } catch (MalformedURLException
	 * e) { e.printStackTrace(); }
	 * 
	 * // Add a filter to handle the request and create a new json // object
	 * with an id defined client =
	 * client.withFilter(getTestFilter(responseContent));
	 * 
	 * // Create get the MobileService table MobileServiceTable<StringIdType>
	 * msTable = client.getTable(tableName, StringIdType.class);
	 * 
	 * // Call the update method msTable.update(item, new
	 * TableOperationCallback<StringIdType>() {
	 * 
	 * @Override public void onCompleted(StringIdType entity, Exception
	 * exception, ServiceFilterResponse response) { if (exception != null) {
	 * container.setException(exception); }
	 * 
	 * latch.countDown(); } }); } });
	 * 
	 * latch.await();
	 * 
	 * // Asserts Exception exception = container.getException();
	 * 
	 * if (exception != null) { fail(exception.getMessage()); } else {
	 * assertEquals(testId, item.Id); assertEquals("Hey", item.String); } }
	 */

	// Integer Id Type

	/*
	 * public void testUpdateWithIntIdTypeAndIntIdResponseContent() throws
	 * Throwable { long[] testIdData = IdTestData.concat(IdTestData.ValidIntIds,
	 * IdTestData.InvalidIntIds);
	 * 
	 * for (long testId : testIdData) {
	 * updateWithIntIdTypeAndIntIdResponseContent(testId); } }
	 * 
	 * private void updateWithIntIdTypeAndIntIdResponseContent(long testId)
	 * throws Throwable { final CountDownLatch latch = new CountDownLatch(1);
	 * 
	 * // Container to store callback's results and do the asserts. final
	 * ResultsContainer container = new ResultsContainer();
	 * 
	 * final String tableName = "MyTableName";
	 * 
	 * String stringTestId = String.valueOf(testId); final String
	 * responseContent = "{\"id\":" + stringTestId + ",\"String\":\"Hey\"}";
	 * 
	 * final LongIdType item = new LongIdType(); item.Id = 12; item.String =
	 * "what?";
	 * 
	 * runTestOnUiThread(new Runnable() {
	 * 
	 * @Override public void run() { MobileServiceClient client = null;
	 * 
	 * try { client = new MobileServiceClient(appUrl, appKey,
	 * getInstrumentation().getTargetContext()); } catch (MalformedURLException
	 * e) { e.printStackTrace(); }
	 * 
	 * // Add a filter to handle the request and create a new json // object
	 * with an id defined client =
	 * client.withFilter(getTestFilter(responseContent));
	 * 
	 * // Create get the MobileService table MobileServiceTable<LongIdType>
	 * msTable = client.getTable(tableName, LongIdType.class);
	 * 
	 * // Call the update method msTable.update(item, new
	 * TableOperationCallback<LongIdType>() {
	 * 
	 * @Override public void onCompleted(LongIdType entity, Exception exception,
	 * ServiceFilterResponse response) { if (exception != null) {
	 * container.setException(exception); }
	 * 
	 * latch.countDown(); } }); } });
	 * 
	 * latch.await();
	 * 
	 * // Asserts Exception exception = container.getException();
	 * 
	 * if (exception != null) { fail(exception.getMessage()); } else {
	 * assertEquals(testId, item.Id); assertEquals("Hey", item.String); } }
	 */

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
