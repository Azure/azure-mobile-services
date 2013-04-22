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
import java.util.concurrent.CountDownLatch;

import android.test.InstrumentationTestCase;

import com.google.gson.JsonObject;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceException;
import com.microsoft.windowsazure.mobileservices.NextServiceFilterCallback;
import com.microsoft.windowsazure.mobileservices.ServiceFilter;
import com.microsoft.windowsazure.mobileservices.ServiceFilterRequest;
import com.microsoft.windowsazure.mobileservices.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.ServiceFilterResponseCallback;
import com.microsoft.windowsazure.mobileservices.TableJsonOperationCallback;
import com.microsoft.windowsazure.mobileservices.TableOperationCallback;

public class ServiceFilterTests extends InstrumentationTestCase {
	String appUrl = "";
	String appKey = "";

	@Override
	protected void setUp() throws Exception {
		appUrl = "http://myapp.com";
		appKey = "qwerty";
		super.setUp();
	}

	@Override
	protected void tearDown() throws Exception {
		super.tearDown();
	}

	public void testClientWithFilterShouldReturnResponseWithHeaderAddedInFilter() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container for results
		final ResultsContainer container = new ResultsContainer();

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

						container.setCount(1);
						container.setRequestContent("Filter1");

						responseCallback.onResponse(new ServiceFilterResponseMock(), null);
					}
				});

				// Execute any action in order to generate a request
				client.getTable("TestTable").lookUp(1, new TableJsonOperationCallback() {

					@Override
					public void onCompleted(JsonObject jsonEntity, Exception exception, ServiceFilterResponse response) {
						latch.countDown();

					}
				});
			}
		});

		latch.await();

		// Assert
		assertEquals(1, container.getCount());
		assertEquals("Filter1", container.getRequestContent());
	}

	public void testClientWithFilterShouldReturnResponseWithHeadersAddedInOrder() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container for results
		final ResultsContainer container = new ResultsContainer();

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				// Create client
				MobileServiceClient client = null;
				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
				}

				// Add 2 new filters to the client.
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						int currentCount = container.getCount() + 1;
						container.setCount(currentCount);
						String currentContent = container.getRequestContent() + "Filter1";
						container.setRequestContent(currentContent);

						nextServiceFilterCallback.onNext(request, responseCallback);
					}
				}).withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						int currentCount = container.getCount() + 1;
						container.setCount(currentCount);
						String currentContent = "Filter2";
						container.setRequestContent(currentContent);

						nextServiceFilterCallback.onNext(request, responseCallback);
					}
				});

				// Execute any action in order to generate a request
				client.getTable("TestTable").lookUp(1, new TableJsonOperationCallback() {

					@Override
					public void onCompleted(JsonObject jsonEntity, Exception exception, ServiceFilterResponse response) {
						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Assert
		assertEquals(2, container.getCount());
		assertEquals("Filter2Filter1", container.getRequestContent());
	}

	public void testClientWithFilterShouldNotExecuteTheFirstFilter() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container for results
		final ResultsContainer container = new ResultsContainer();

		runTestOnUiThread(new Runnable() {
			@Override
			public void run() {
				// Create client
				MobileServiceClient client = null;
				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e1) {
				}

				// Add 2 new filters to the client.
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						int currentCount = container.getCount() + 1;
						container.setCount(currentCount);
						String currentContent = container.getRequestContent() + "Filter1";
						container.setRequestContent(currentContent);

						nextServiceFilterCallback.onNext(request, responseCallback);
					}

				}).withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						int currentCount = container.getCount() + 1;
						container.setCount(currentCount);
						String currentContent = "Filter2";
						container.setRequestContent(currentContent);

						ServiceFilterResponse response = null;
						responseCallback.onResponse(response, new Exception("Dummy Exception"));
					}
				});

				// Execute any action in order to generate a request
				client.getTable("TestTable").lookUp(1, new TableJsonOperationCallback() {

					@Override
					public void onCompleted(JsonObject jsonEntity, Exception exception, ServiceFilterResponse response) {
						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Assert
		assertEquals(1, container.getCount());
		assertEquals("Filter2", container.getRequestContent());
	}

	public void testClientWithFilterShouldThrowExceptionWhenExecutingFilter() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		final ResultsContainer container = new ResultsContainer();

		runTestOnUiThread(new Runnable() {
			@Override
			public void run() {
				// Create ServiceFilterResponseMock
				final ServiceFilterResponseMock response = new ServiceFilterResponseMock();
				response.setContent("Response Content");
				response.setStatus(new StatusLineMock(200));

				// Create client and connection
				MobileServiceClient client = null;
				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
				}

				// Create ServiceFilterRequestMock that returns the given
				// response
				ServiceFilterRequestMock request = new ServiceFilterRequestMock(response);
				request.setHasErrorOnExecute(true);

				// Add a new filter to the client
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						responseCallback.onResponse(response, new MobileServiceException("Error in filter 1"));
					}
				});

				// Execute any action in order to generate a request
				client.getTable("TestTable").lookUp(1, new TableJsonOperationCallback() {

					@Override
					public void onCompleted(JsonObject jsonEntity, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setErrorMessage(exception.getMessage());
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Assert
		assertTrue(container.getErrorMessage().startsWith("Error in filter 1"));
	}

	public void testClientWithFilterShouldThrowExceptionWhenExecutingTheFirstFilter() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		final ResultsContainer container = new ResultsContainer();

		runTestOnUiThread(new Runnable() {
			@Override
			public void run() {
				// Create client
				MobileServiceClient client = null;
				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
				}

				// Create ServiceFilterResponseMock
				final ServiceFilterResponseMock response = new ServiceFilterResponseMock();
				response.setContent("Response Content");
				response.setStatus(new StatusLineMock(200));

				// Create ServiceFilterRequestMock that returns the given
				// response
				ServiceFilterRequestMock request = new ServiceFilterRequestMock(response);
				request.setHasErrorOnExecute(true);
				// Add a new filter to the client
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						container.setErrorMessage("this handler should never be called");
					}
				}).withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {
						responseCallback.onResponse(response, new MobileServiceException("Error in filter 2"));
					}
				});

				// Execute any action in order to generate a request
				client.getTable("TestTable").lookUp(1, new TableJsonOperationCallback() {

					@Override
					public void onCompleted(JsonObject jsonEntity, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setErrorMessage(exception.getMessage());
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Assert
		assertTrue(container.getErrorMessage().startsWith("Error in filter 2"));
	}
	
	public void testRequestShouldntFailWith401ResponseAndWithoutOnResponseCallbackInvokation() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		runTestOnUiThread(new Runnable() {
			@Override
			public void run() {
				// Create client
				MobileServiceClient client = null;
				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
				}

				// Create ServiceFilterResponseMock
				final ServiceFilterResponseMock response = new ServiceFilterResponseMock();
				response.setContent("{'error': 'Unauthorized'}");
				response.setStatus(new StatusLineMock(401));

				// Create ServiceFilterRequestMock that returns the given
				// response
				ServiceFilterRequestMock request = new ServiceFilterRequestMock(response);
				request.setHasErrorOnExecute(true);
				// Add a new filter to the client
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {
					}
				});

				// Execute any action in order to generate a request
				client.getTable(PersonTestObject.class).lookUp(1, new TableOperationCallback<PersonTestObject>() {

					@Override
					public void onCompleted(PersonTestObject entity, Exception exception, ServiceFilterResponse response) {
						if (exception != null || entity != null) {
							fail("Since no onResponse wasn't invoked from the filter, there shouldn't be any kind of result (exception or entity)");
						}
						
						latch.countDown();
					}
				});
			}
		});

		latch.await();
	}
}
