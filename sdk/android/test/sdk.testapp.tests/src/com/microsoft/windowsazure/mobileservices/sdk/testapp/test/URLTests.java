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
import java.util.List;
import java.util.Locale;
import java.util.concurrent.CountDownLatch;

import junit.framework.Assert;

import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.microsoft.windowsazure.mobileservices.MobileServiceAuthenticationProvider;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceUser;
import com.microsoft.windowsazure.mobileservices.NextServiceFilterCallback;
import com.microsoft.windowsazure.mobileservices.ServiceFilter;
import com.microsoft.windowsazure.mobileservices.ServiceFilterRequest;
import com.microsoft.windowsazure.mobileservices.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.ServiceFilterResponseCallback;
import com.microsoft.windowsazure.mobileservices.TableDeleteCallback;
import com.microsoft.windowsazure.mobileservices.TableJsonOperationCallback;
import com.microsoft.windowsazure.mobileservices.TableJsonQueryCallback;
import com.microsoft.windowsazure.mobileservices.TableOperationCallback;
import com.microsoft.windowsazure.mobileservices.UserAuthenticationCallback;

import android.test.InstrumentationTestCase;
import android.util.Pair;

public class URLTests extends InstrumentationTestCase {

	String appUrl = "";
	String appKey = "";

	@Override
	protected void setUp() throws Exception {
		appUrl = "http://myapp.com/";
		appKey = "qwerty";
		super.setUp();
	}

	public void testLoginURL() throws Throwable {
		testLoginURL(MobileServiceAuthenticationProvider.Facebook);
		testLoginURL(MobileServiceAuthenticationProvider.Twitter);
		testLoginURL(MobileServiceAuthenticationProvider.MicrosoftAccount);
		testLoginURL(MobileServiceAuthenticationProvider.Google);

		testLoginURL("facebook");
		testLoginURL("TWITTER");
		testLoginURL("GOOGLE");
		testLoginURL("MicrosoftAccount");
	}

	private void testLoginURL(final Object provider) throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);
		final ResultsContainer result = new ResultsContainer();

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				// Create client
				MobileServiceClient client = null;
				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
				}

				// Add a new filter to the client
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						result.setRequestUrl(request.getUrl());
						assertEquals("POST", request.getMethod());

						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setContent("{authenticationToken:'123abc', user:{userId:'123456'}}");

						responseCallback.onResponse(response, null);
					}
				});

				UserAuthenticationCallback callback = new UserAuthenticationCallback() {

					@Override
					public void onCompleted(MobileServiceUser user, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							Assert.fail();
						}

						latch.countDown();
					}
				};

				if (provider.getClass().equals(MobileServiceAuthenticationProvider.class)) {
					client.login((MobileServiceAuthenticationProvider)provider, "{\"myToken\":123}", callback);
				} else {
					client.login((String)provider, "{\"myToken\":123}", callback);
				}
			}
		});

		latch.await();

		// Assert
		String expectedURL = appUrl + "login/" + provider.toString().toLowerCase(Locale.getDefault());
		assertEquals(expectedURL, result.getRequestUrl());

	}

	public void testUpdateURL() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);
		final ResultsContainer result = new ResultsContainer();

		final String tableName = "dummy";
		final PersonTestObject person = new PersonTestObject("john", "doe", 10);
		person.setId(10);

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				// Create client
				MobileServiceClient client = null;
				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
				}

				// Add a new filter to the client
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						result.setRequestUrl(request.getUrl());
						assertEquals("PATCH", request.getMethod());

						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setContent("{}");

						responseCallback.onResponse(response, null);
					}
				});

				client.getTable(tableName, PersonTestObject.class).update(person, new TableOperationCallback<PersonTestObject>() {

					@Override
					public void onCompleted(PersonTestObject entity, Exception exception, ServiceFilterResponse response) {
						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Assert
		String expectedURL = appUrl + "tables/" + tableName + "/" + person.getId();
		assertEquals(expectedURL, result.getRequestUrl());
	}
	
	public void testUpdateURLWithParameters() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);
		final ResultsContainer result = new ResultsContainer();

		final String tableName = "dummy";
		final PersonTestObject person = new PersonTestObject("john", "doe", 10);
		person.setId(10);
		final List<Pair<String, String>> parameters = new ArrayList<Pair<String, String>>();
		parameters.add(new Pair<String, String>("a key", "my <>&=?@ value"));

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				// Create client
				MobileServiceClient client = null;
				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
				}

				// Add a new filter to the client
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						result.setRequestUrl(request.getUrl());
						assertEquals("PATCH", request.getMethod());

						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setContent("{}");

						responseCallback.onResponse(response, null);
					}
				});

				client.getTable(tableName, PersonTestObject.class).update(person, parameters, new TableOperationCallback<PersonTestObject>() {

					@Override
					public void onCompleted(PersonTestObject entity, Exception exception, ServiceFilterResponse response) {
						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Assert
		String expectedURL = appUrl + "tables/" + tableName + "/" + person.getId() + "?a%20key=my%20%3C%3E%26%3D%3F%40%20value";
		assertEquals(expectedURL, result.getRequestUrl());
	}

	public void testInsertURL() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);
		final ResultsContainer result = new ResultsContainer();

		final String tableName = "dummy";
		final PersonTestObject person = new PersonTestObject("john", "doe", 10);

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				// Create client
				MobileServiceClient client = null;
				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
				}

				// Add a new filter to the client
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						result.setRequestUrl(request.getUrl());
						assertEquals("POST", request.getMethod());

						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setContent("{}");

						responseCallback.onResponse(response, null);
					}
				});

				client.getTable(tableName, PersonTestObject.class).insert(person, new TableOperationCallback<PersonTestObject>() {

					@Override
					public void onCompleted(PersonTestObject entity, Exception exception, ServiceFilterResponse response) {
						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Assert
		String expectedURL = appUrl + "tables/" + tableName;
		assertEquals(expectedURL, result.getRequestUrl());
	}
	
	public void testInsertURLWithParameters() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);
		final ResultsContainer result = new ResultsContainer();

		final String tableName = "dummy";
		final PersonTestObject person = new PersonTestObject("john", "doe", 10);
		final List<Pair<String, String>> parameters = new ArrayList<Pair<String, String>>();
		parameters.add(new Pair<String, String>("a key", "my <>&=?@ value"));

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				// Create client
				MobileServiceClient client = null;
				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
				}

				// Add a new filter to the client
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						result.setRequestUrl(request.getUrl());
						assertEquals("POST", request.getMethod());

						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setContent("{}");

						responseCallback.onResponse(response, null);
					}
				});

				client.getTable(tableName, PersonTestObject.class).insert(person, parameters, new TableOperationCallback<PersonTestObject>() {

					@Override
					public void onCompleted(PersonTestObject entity, Exception exception, ServiceFilterResponse response) {
						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Assert
		String expectedURL = appUrl + "tables/" + tableName + "?a%20key=my%20%3C%3E%26%3D%3F%40%20value";
		assertEquals(expectedURL, result.getRequestUrl());
	}

	public void testLookupURL() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);
		final ResultsContainer result = new ResultsContainer();

		final String tableName = "dummy";
		final int id = 10;

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				// Create client
				MobileServiceClient client = null;
				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
				}

				// Add a new filter to the client
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						result.setRequestUrl(request.getUrl());
						assertEquals("GET", request.getMethod());

						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setContent("{}");

						responseCallback.onResponse(response, null);
					}
				});

				client.getTable(tableName).lookUp(id, new TableJsonOperationCallback() {

					@Override
					public void onCompleted(JsonObject jsonEntity, Exception exception, ServiceFilterResponse response) {
						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Assert
		String expectedURL = appUrl + "tables/" + tableName + "/" + id;
		assertEquals(expectedURL, result.getRequestUrl());
	}
	
	public void testLookupURLWithParameters() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);
		final ResultsContainer result = new ResultsContainer();

		final String tableName = "dummy";
		final int id = 10;
		final List<Pair<String, String>> parameters = new ArrayList<Pair<String, String>>();
		parameters.add(new Pair<String, String>("a key", "my <>&=?@ value"));

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				// Create client
				MobileServiceClient client = null;
				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
				}

				// Add a new filter to the client
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						result.setRequestUrl(request.getUrl());
						assertEquals("GET", request.getMethod());

						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setContent("{}");

						responseCallback.onResponse(response, null);
					}
				});

				client.getTable(tableName).lookUp(id, parameters, new TableJsonOperationCallback() {

					@Override
					public void onCompleted(JsonObject jsonEntity, Exception exception, ServiceFilterResponse response) {
						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Assert
		String expectedURL = appUrl + "tables/" + tableName + "/" + id + "?a%20key=my%20%3C%3E%26%3D%3F%40%20value";
		assertEquals(expectedURL, result.getRequestUrl());
	}

	public void testDeleteURL() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);
		final ResultsContainer result = new ResultsContainer();

		final String tableName = "dummy";
		final int id = 10;

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				// Create client
				MobileServiceClient client = null;
				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
				}

				// Add a new filter to the client
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						result.setRequestUrl(request.getUrl());
						assertEquals("DELETE", request.getMethod());

						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setContent("{}");

						responseCallback.onResponse(response, null);
					}
				});

				client.getTable(tableName).delete(id, new TableDeleteCallback() {

					@Override
					public void onCompleted(Exception exception, ServiceFilterResponse response) {
						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Assert
		String expectedURL = appUrl + "tables/" + tableName + "/" + id;
		assertEquals(expectedURL, result.getRequestUrl());
	}
	
	public void testDeleteURLWithParameters() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);
		final ResultsContainer result = new ResultsContainer();

		final String tableName = "dummy";
		final int id = 10;
		final List<Pair<String, String>> parameters = new ArrayList<Pair<String, String>>();
		parameters.add(new Pair<String, String>("a key", "my <>&=?@ value"));

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				// Create client
				MobileServiceClient client = null;
				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
				}

				// Add a new filter to the client
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						result.setRequestUrl(request.getUrl());
						assertEquals("DELETE", request.getMethod());

						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setContent("{}");

						responseCallback.onResponse(response, null);
					}
				});

				client.getTable(tableName).delete(id, parameters, new TableDeleteCallback() {

					@Override
					public void onCompleted(Exception exception, ServiceFilterResponse response) {
						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Assert
		String expectedURL = appUrl + "tables/" + tableName + "/" + id + "?a%20key=my%20%3C%3E%26%3D%3F%40%20value";
		assertEquals(expectedURL, result.getRequestUrl());
	}

	public void testQueryURL() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);
		final ResultsContainer result = new ResultsContainer();

		final String tableName = "dummy";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				// Create client
				MobileServiceClient client = null;
				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
				}

				// Add a new filter to the client
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						result.setRequestUrl(request.getUrl());
						assertEquals("GET", request.getMethod());

						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setContent("{}");

						responseCallback.onResponse(response, null);
					}
				});

				client.getTable(tableName).execute(new TableJsonQueryCallback() {

					@Override
					public void onCompleted(JsonElement result, int count, Exception exception, ServiceFilterResponse response) {
						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Assert
		String expectedURL = appUrl + "tables/" + tableName;
		assertTrue(result.getRequestUrl().startsWith(expectedURL));
	}
}
