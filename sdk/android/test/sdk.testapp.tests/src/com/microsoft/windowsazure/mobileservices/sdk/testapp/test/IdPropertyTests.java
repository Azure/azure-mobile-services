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
import java.util.Locale;
import java.util.concurrent.CountDownLatch;

import android.test.InstrumentationTestCase;

import com.google.gson.GsonBuilder;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceJsonTable;
import com.microsoft.windowsazure.mobileservices.NextServiceFilterCallback;
import com.microsoft.windowsazure.mobileservices.ServiceFilter;
import com.microsoft.windowsazure.mobileservices.ServiceFilterRequest;
import com.microsoft.windowsazure.mobileservices.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.ServiceFilterResponseCallback;
import com.microsoft.windowsazure.mobileservices.TableJsonOperationCallback;
import com.microsoft.windowsazure.mobileservices.TableJsonQueryCallback;

public class IdPropertyTests extends InstrumentationTestCase {
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

	// Read Tests

	public void testReadWithStringIdResponseContent() throws Throwable {
		String[] testIdData = IdTestData.concat(IdTestData.concat(IdTestData.ValidStringIds, IdTestData.EmptyStringIds), IdTestData.InvalidStringIds);

		for (String testId : testIdData) {
			readWithStringIdResponseContent(testId);
		}
	}

	private void readWithStringIdResponseContent(String testId) throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		String jsonTestId = testId.replace("\\", "\\\\").replace("\"", "\\\"");
		final String responseContent = "[{\"id\":\"" + jsonTestId + "\",\"String\":\"Hey\"}]";

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
				MobileServiceJsonTable msTable = client.getTable(tableName);

				// Call the select method
				msTable.execute(new TableJsonQueryCallback() {

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
		} else {
			assertTrue(container.getJsonResult().isJsonArray());
			assertTrue(container.getJsonResult().getAsJsonArray().size() == 1);
			assertTrue(container.getJsonResult().getAsJsonArray().get(0).isJsonObject());
			assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().has("id"));
			assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().has("String"));
			assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("id").isJsonPrimitive());
			assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("String").isJsonPrimitive());
			assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("id").getAsJsonPrimitive().isString());
			assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("String").getAsJsonPrimitive().isString());
			assertEquals(testId, container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("id").getAsJsonPrimitive().getAsString());
			assertEquals("Hey", container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("String").getAsJsonPrimitive().getAsString());
		}
	}

	public void testReadWithIntIdResponseContent() throws Throwable {
		long[] testIdData = IdTestData.concat(IdTestData.ValidIntIds, IdTestData.InvalidIntIds);

		for (long testId : testIdData) {
			readWithIntIdResponseContent(testId);
		}
	}

	private void readWithIntIdResponseContent(long testId) throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		String jsonTestId = String.valueOf(testId);

		final String responseContent = "[{\"id\":" + jsonTestId + ",\"String\":\"Hey\"}]";

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
				MobileServiceJsonTable msTable = client.getTable(tableName);

				// Call the select method
				msTable.execute(new TableJsonQueryCallback() {

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
		} else {
			assertTrue(container.getJsonResult().isJsonArray());
			assertTrue(container.getJsonResult().getAsJsonArray().size() == 1);
			assertTrue(container.getJsonResult().getAsJsonArray().get(0).isJsonObject());
			assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().has("id"));
			assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().has("String"));
			assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("id").isJsonPrimitive());
			assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("String").isJsonPrimitive());
			assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("id").getAsJsonPrimitive().isNumber());
			assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("String").getAsJsonPrimitive().isString());
			assertEquals(testId, container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("id").getAsJsonPrimitive().getAsLong());
			assertEquals("Hey", container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("String").getAsJsonPrimitive().getAsString());
		}
	}

	public void testReadWithNonStringAndNonIntIdResponseContent() throws Throwable {
		Object[] testIdData = IdTestData.NonStringNonIntValidJsonIds;

		for (Object testId : testIdData) {
			readWithNonStringAndNonIntIdResponseContent(testId);
		}
	}

	private void readWithNonStringAndNonIntIdResponseContent(Object testId) throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		String jsonTestId = testId.toString().toLowerCase(Locale.getDefault());

		final String responseContent = "[{\"id\":" + jsonTestId + ",\"String\":\"Hey\"}]";

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
				MobileServiceJsonTable msTable = client.getTable(tableName);

				// Call the select method
				msTable.execute(new TableJsonQueryCallback() {

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
		} else {
			assertTrue(container.getJsonResult().isJsonArray());

			assertTrue(container.getJsonResult().getAsJsonArray().size() == 1);

			assertTrue(container.getJsonResult().getAsJsonArray().get(0).isJsonObject());

			assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().has("id"));
			assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().has("String"));

			assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("String").isJsonPrimitive());

			assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("String").getAsJsonPrimitive().isString());

			if (container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("id").isJsonNull()) {
				assertTrue(testId == null);
			} else if (container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("id").isJsonPrimitive()) {
				if (container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("id").getAsJsonPrimitive().isBoolean()) {
					assertEquals(testId, container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("id").getAsJsonPrimitive().getAsBoolean());
				} else if (container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("id").getAsJsonPrimitive().isNumber()) {
					assertEquals(testId, container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("id").getAsJsonPrimitive().getAsDouble());
				} else if (container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("id").getAsJsonPrimitive().isString()) {
					assertEquals(testId, container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("id").getAsJsonPrimitive().getAsString());
				}
			} else if (container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("id").isJsonObject()) {
				assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("id").getAsJsonObject().equals(testId));
			} else if (container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("id").isJsonArray()) {
				assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("id").getAsJsonArray().equals(testId));
			}

			assertEquals("Hey", container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("String").getAsJsonPrimitive().getAsString());
		}
	}
	
	public void testReadWithNullIdResponseContent() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		final String responseContent = "[{\"id\":null,\"String\":\"Hey\"}]";

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
				MobileServiceJsonTable msTable = client.getTable(tableName);

				// Call the select method
				msTable.execute(new TableJsonQueryCallback() {

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
		} else {
			assertTrue(container.getJsonResult().isJsonArray());

			assertTrue(container.getJsonResult().getAsJsonArray().size() == 1);

			assertTrue(container.getJsonResult().getAsJsonArray().get(0).isJsonObject());

			assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().has("id"));
			assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().has("String"));

			assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("id").isJsonNull());
			assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("String").isJsonPrimitive());

			assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("String").getAsJsonPrimitive().isString());

			assertEquals("Hey", container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("String").getAsJsonPrimitive().getAsString());
		}
	}

	public void testReadWithNoIdResponseContent() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		final String responseContent = "[{\"String\":\"Hey\"}]";

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
				MobileServiceJsonTable msTable = client.getTable(tableName);

				// Call the select method
				msTable.execute(new TableJsonQueryCallback() {

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
		} else {
			assertTrue(container.getJsonResult().isJsonArray());

			assertTrue(container.getJsonResult().getAsJsonArray().size() == 1);

			assertTrue(container.getJsonResult().getAsJsonArray().get(0).isJsonObject());

			assertTrue(!container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().has("id"));
			assertTrue(!container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().has("Id"));
			assertTrue(!container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().has("iD"));
			assertTrue(!container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().has("ID"));
			
			assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().has("String"));

			assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("String").isJsonPrimitive());

			assertTrue(container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("String").getAsJsonPrimitive().isString());

			assertEquals("Hey", container.getJsonResult().getAsJsonArray().get(0).getAsJsonObject().get("String").getAsJsonPrimitive().getAsString());
		}
	}
	
	// Lookup Tests

	public void testLookupWithStringIdResponseContent() throws Throwable {
		String[] testIdData = IdTestData.concat(IdTestData.concat(IdTestData.ValidStringIds, IdTestData.EmptyStringIds), IdTestData.InvalidStringIds);

		for (String testId : testIdData) {
			lookupWithStringIdResponseContent(testId);
		}
	}

	private void lookupWithStringIdResponseContent(final String testId) throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		String jsonTestId = testId.replace("\\", "\\\\").replace("\"", "\\\"");
		final String responseContent = "{\"id\":\"" + jsonTestId + "\",\"String\":\"Hey\"}";

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
				MobileServiceJsonTable msTable = client.getTable(tableName);

				// Call the lookup method
				msTable.lookUp("myId", new TableJsonOperationCallback() {

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
		} else {
			assertTrue(container.getJsonResult().isJsonObject());
			assertTrue(container.getJsonResult().getAsJsonObject().has("id"));
			assertTrue(container.getJsonResult().getAsJsonObject().has("String"));
			assertTrue(container.getJsonResult().getAsJsonObject().get("id").isJsonPrimitive());
			assertTrue(container.getJsonResult().getAsJsonObject().get("String").isJsonPrimitive());
			assertTrue(container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().isString());
			assertTrue(container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().isString());
			assertEquals(testId, container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().getAsString());
			assertEquals("Hey", container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().getAsString());
		}
	}

	public void testLookupWithIntIdResponseContent() throws Throwable {
		long[] testIdData = IdTestData.concat(IdTestData.ValidIntIds, IdTestData.InvalidIntIds);

		for (long testId : testIdData) {
			lookupWithIntIdResponseContent(testId);
		}
	}

	private void lookupWithIntIdResponseContent(long testId) throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		String jsonTestId = String.valueOf(testId);

		final String responseContent = "{\"id\":" + jsonTestId + ",\"String\":\"Hey\"}";

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
				MobileServiceJsonTable msTable = client.getTable(tableName);

				// Call the lookup method
				msTable.lookUp("myId", new TableJsonOperationCallback() {

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
		} else {
			assertTrue(container.getJsonResult().isJsonObject());
			assertTrue(container.getJsonResult().getAsJsonObject().has("id"));
			assertTrue(container.getJsonResult().getAsJsonObject().has("String"));
			assertTrue(container.getJsonResult().getAsJsonObject().get("id").isJsonPrimitive());
			assertTrue(container.getJsonResult().getAsJsonObject().get("String").isJsonPrimitive());
			assertTrue(container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().isNumber());
			assertTrue(container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().isString());
			assertEquals(testId, container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().getAsLong());
			assertEquals("Hey", container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().getAsString());
		}
	}

	public void testLookupWithNonStringAndNonIntIdResponseContent() throws Throwable {
		Object[] testIdData = IdTestData.NonStringNonIntValidJsonIds;

		for (Object testId : testIdData) {
			lookupWithNonStringAndNonIntIdResponseContent(testId);
		}
	}

	private void lookupWithNonStringAndNonIntIdResponseContent(Object testId) throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		String jsonTestId = testId.toString().toLowerCase(Locale.getDefault());

		final String responseContent = "{\"id\":" + jsonTestId + ",\"String\":\"Hey\"}";

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
				MobileServiceJsonTable msTable = client.getTable(tableName);

				// Call the lookup method
				msTable.lookUp("myId", new TableJsonOperationCallback() {

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
		} else {
			assertTrue(container.getJsonResult().isJsonObject());

			assertTrue(container.getJsonResult().getAsJsonObject().has("id"));
			assertTrue(container.getJsonResult().getAsJsonObject().has("String"));

			assertTrue(container.getJsonResult().getAsJsonObject().get("String").isJsonPrimitive());

			assertTrue(container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().isString());

			if (container.getJsonResult().getAsJsonObject().get("id").isJsonNull()) {
				assertTrue(testId == null);
			} else if (container.getJsonResult().getAsJsonObject().get("id").isJsonPrimitive()) {
				if (container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().isBoolean()) {
					assertEquals(testId, container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().getAsBoolean());
				} else if (container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().isNumber()) {
					assertEquals(testId, container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().getAsDouble());
				} else if (container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().isString()) {
					assertEquals(testId, container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().getAsString());
				}
			} else if (container.getJsonResult().getAsJsonObject().get("id").isJsonObject()) {
				assertTrue(container.getJsonResult().getAsJsonObject().get("id").getAsJsonObject().equals(testId));
			} else if (container.getJsonResult().getAsJsonObject().get("id").isJsonArray()) {
				assertTrue(container.getJsonResult().getAsJsonObject().get("id").getAsJsonArray().equals(testId));
			}

			assertEquals("Hey", container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().getAsString());
		}
	}
	
	public void testLookupWithNullIdResponseContent() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		final String responseContent = "{\"id\":null,\"String\":\"Hey\"}";

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
				MobileServiceJsonTable msTable = client.getTable(tableName);

				// Call the lookup method
				msTable.lookUp("myId", new TableJsonOperationCallback() {

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
		} else {
			assertTrue(container.getJsonResult().isJsonObject());

			assertTrue(container.getJsonResult().getAsJsonObject().has("id"));
			assertTrue(container.getJsonResult().getAsJsonObject().has("String"));

			assertTrue(container.getJsonResult().getAsJsonObject().get("id").isJsonNull());
			assertTrue(container.getJsonResult().getAsJsonObject().get("String").isJsonPrimitive());

			assertTrue(container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().isString());

			assertEquals("Hey", container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().getAsString());
		}
	}

	public void testLookupWithNoIdResponseContent() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		final String responseContent = "{\"String\":\"Hey\"}";

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
				MobileServiceJsonTable msTable = client.getTable(tableName);

				// Call the lookup method
				msTable.lookUp("myId", new TableJsonOperationCallback() {

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
		} else {
			assertTrue(container.getJsonResult().isJsonObject());

			assertTrue(!container.getJsonResult().getAsJsonObject().has("id"));
			assertTrue(!container.getJsonResult().getAsJsonObject().has("Id"));
			assertTrue(!container.getJsonResult().getAsJsonObject().has("iD"));
			assertTrue(!container.getJsonResult().getAsJsonObject().has("ID"));
			
			assertTrue(container.getJsonResult().getAsJsonObject().has("String"));

			assertTrue(container.getJsonResult().getAsJsonObject().get("String").isJsonPrimitive());

			assertTrue(container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().isString());

			assertEquals("Hey", container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().getAsString());
		}
	}
	
	public void testLookupWithStringIdParameter() throws Throwable {
		String[] testIdData = IdTestData.ValidStringIds;

		for (String testId : testIdData) {
			lookupWithStringIdParameter(testId);
		}
	}

	private void lookupWithStringIdParameter(final String testId) throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		String jsonTestId = testId.replace("\\", "\\\\").replace("\"", "\\\"");
		final String responseContent = "{\"id\":\"" + jsonTestId + "\",\"String\":\"Hey\"}";
		
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
				MobileServiceJsonTable msTable = client.getTable(tableName);

				// Call the lookup method
				msTable.lookUp(testId, new TableJsonOperationCallback() {

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
		} else {
			assertTrue(container.getJsonResult().isJsonObject());
			assertTrue(container.getJsonResult().getAsJsonObject().has("id"));
			assertTrue(container.getJsonResult().getAsJsonObject().has("String"));
			assertTrue(container.getJsonResult().getAsJsonObject().get("id").isJsonPrimitive());
			assertTrue(container.getJsonResult().getAsJsonObject().get("String").isJsonPrimitive());
			assertTrue(container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().isString());
			assertTrue(container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().isString());
			assertEquals(testId, container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().getAsString());
			assertEquals("Hey", container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().getAsString());
		}
	}

	public void testLookupWithInvalidStringIdParameter() throws Throwable {
		String[] testIdData = IdTestData.concat(IdTestData.EmptyStringIds, IdTestData.InvalidStringIds);

		for (String testId : testIdData) {
			lookupWithInvalidStringIdParameter(testId);
		}
	}

	private void lookupWithInvalidStringIdParameter(final String testId) throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		String jsonTestId = testId.replace("\\", "\\\\").replace("\"", "\\\"");
		final String responseContent = "{\"id\":\"" + jsonTestId + "\",\"String\":\"Hey\"}";

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
				MobileServiceJsonTable msTable = client.getTable(tableName);

				// Call the lookup method
				msTable.lookUp(testId, new TableJsonOperationCallback() {

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

		if (exception == null || !(exception instanceof IllegalArgumentException)) {
			fail("Expected Exception IllegalArgumentException");
		}
	}

	public void testLookupWithIntIdParameter() throws Throwable {
		long[] testIdData = IdTestData.ValidIntIds;

		for (long testId : testIdData) {
			lookupWithIntIdParameter(testId);
		}
	}

	private void lookupWithIntIdParameter(final long testId) throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		String jsonTestId = String.valueOf(testId);

		final String responseContent = "{\"id\":" + jsonTestId + ",\"String\":\"Hey\"}";

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
				MobileServiceJsonTable msTable = client.getTable(tableName);

				// Call the lookup method
				msTable.lookUp(testId, new TableJsonOperationCallback() {

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
		} else {
			assertTrue(container.getJsonResult().isJsonObject());
			assertTrue(container.getJsonResult().getAsJsonObject().has("id"));
			assertTrue(container.getJsonResult().getAsJsonObject().has("String"));
			assertTrue(container.getJsonResult().getAsJsonObject().get("id").isJsonPrimitive());
			assertTrue(container.getJsonResult().getAsJsonObject().get("String").isJsonPrimitive());
			assertTrue(container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().isNumber());
			assertTrue(container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().isString());
			assertEquals(testId, container.getJsonResult().getAsJsonObject().get("id").getAsJsonPrimitive().getAsLong());
			assertEquals("Hey", container.getJsonResult().getAsJsonObject().get("String").getAsJsonPrimitive().getAsString());
		}
	}

	public void testLookupWithInvalidIntIdParameter() throws Throwable {
		long[] testIdData = IdTestData.InvalidIntIds;

		for (long testId : testIdData) {
			lookupWithInvalidIntIdParameter(testId);
		}
	}

	private void lookupWithInvalidIntIdParameter(final long testId) throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		String jsonTestId = String.valueOf(testId);

		final String responseContent = "{\"id\":" + jsonTestId + ",\"String\":\"Hey\"}";

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
				MobileServiceJsonTable msTable = client.getTable(tableName);

				// Call the lookup method
				msTable.lookUp(testId, new TableJsonOperationCallback() {

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

		if (exception == null || !(exception instanceof IllegalArgumentException)) {
			fail("Expected Exception IllegalArgumentException");
		}
	}
	
	public void testLookupWithNullIdParameter() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		final String responseContent = "{\"id\":null,\"String\":\"Hey\"}";

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
				MobileServiceJsonTable msTable = client.getTable(tableName);

				// Call the lookup method
				msTable.lookUp(null, new TableJsonOperationCallback() {

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

		if (exception == null || !(exception instanceof IllegalArgumentException)) {
			fail("Expected Exception IllegalArgumentException");
		}
	}

	public void testLookupWithZeroIdParameter() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		final String responseContent = "{\"id\":0,\"String\":\"Hey\"}";

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
				MobileServiceJsonTable msTable = client.getTable(tableName);

				// Call the lookup method
				msTable.lookUp(0L, new TableJsonOperationCallback() {

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

		if (exception == null || !(exception instanceof IllegalArgumentException)) {
			fail("Expected Exception IllegalArgumentException");
		}
	}
	
	
	
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
}
