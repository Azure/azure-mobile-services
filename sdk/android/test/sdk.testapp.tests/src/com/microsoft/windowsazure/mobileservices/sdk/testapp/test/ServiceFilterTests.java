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

import android.test.InstrumentationTestCase;

import com.google.common.util.concurrent.ListenableFuture;
import com.google.common.util.concurrent.SettableFuture;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceException;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.http.NextServiceFilterCallback;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilter;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterRequest;

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

		// Container for results
		final ResultsContainer container = new ResultsContainer();
		// Create client
		MobileServiceClient client = null;
		try {
			client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
		} catch (MalformedURLException e) {

		}

		// Add a new filter to the client
		client = client.withFilter(new ServiceFilter() {

			@Override
			public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

				container.setCount(1);
				container.setRequestContent("Filter1");

				final SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture.create();

				resultFuture.set(new ServiceFilterResponseMock());

				return resultFuture;
			}
		});

		try {
			// Call the lookup method
			client.getTable("TestTable").lookUp(1).get();

		} catch (Exception exception) {
			fail(exception.getMessage());
		}

		// Assert
		assertEquals(1, container.getCount());
		assertEquals("Filter1", container.getRequestContent());
	}

	public void testClientWithFilterShouldReturnResponseWithHeadersAddedInOrder() throws Throwable {
		// Container for results
		final ResultsContainer container = new ResultsContainer();

		// Create client
		MobileServiceClient client = null;
		try {
			client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
		} catch (MalformedURLException e) {
		}

		// Add 2 new filters to the client.
		client = client.withFilter(new ServiceFilter() {

			@Override
			public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

				int currentCount = container.getCount() + 1;
				container.setCount(currentCount);
				String currentContent = container.getRequestContent() + "Filter1";
				container.setRequestContent(currentContent);

				return nextServiceFilterCallback.onNext(request);
			}
		}).withFilter(new ServiceFilter() {

			@Override
			public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

				int currentCount = container.getCount() + 1;
				container.setCount(currentCount);
				String currentContent = "Filter2";
				container.setRequestContent(currentContent);

				return nextServiceFilterCallback.onNext(request);
			}
		});

		try {
			// Call the lookup method
			client.getTable("TestTable").lookUp(1).get();

		} catch (Exception exception) {
		}

		// Assert
		assertEquals(2, container.getCount());
		assertEquals("Filter2Filter1", container.getRequestContent());
	}

	public void testClientWithFilterShouldNotExecuteTheFirstFilter() throws Throwable {
		// Container for results
		final ResultsContainer container = new ResultsContainer();

		// Create client
		MobileServiceClient client = null;
		try {
			client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
		} catch (MalformedURLException e1) {
		}

		// Add 2 new filters to the client.
		client = client.withFilter(new ServiceFilter() {

			@Override
			public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

				int currentCount = container.getCount() + 1;
				container.setCount(currentCount);
				String currentContent = container.getRequestContent() + "Filter1";
				container.setRequestContent(currentContent);

				return nextServiceFilterCallback.onNext(request);
			}

		}).withFilter(new ServiceFilter() {

			@Override
			public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

				int currentCount = container.getCount() + 1;
				container.setCount(currentCount);
				String currentContent = "Filter2";
				container.setRequestContent(currentContent);

				final SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture.create();

				resultFuture.setException(new Exception("Dummy Exception"));

				return resultFuture;

			}
		});

		boolean hasException = false;

		try {
			// Call the lookup method
			client.getTable("TestTable").lookUp(1).get();

		} catch (Exception exception) {

			hasException = true;

			// Assert
			assertEquals(1, container.getCount());
			assertEquals("Filter2", container.getRequestContent());
		}

		assertTrue(hasException);
	}

	public void testClientWithFilterShouldThrowExceptionWhenExecutingFilter() throws Throwable {

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
			public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

				final SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture.create();

				resultFuture.setException(new MobileServiceException("Error in filter 1"));

				return resultFuture;
			}
		});

		boolean hasException = false;

		try {
			// Call the lookup method
			client.getTable("TestTable").lookUp(1).get();

		} catch (Exception exception) {
			assertTrue(exception.getCause().getMessage().startsWith("Error in filter 1"));
			hasException = true;
		}

		assertTrue(hasException);
	}

	public void testClientWithFilterShouldThrowExceptionWhenExecutingTheFirstFilter() throws Throwable {

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
			public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

				final SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture.create();

				resultFuture.set(null);

				return resultFuture;
			}
		}).withFilter(new ServiceFilter() {

			@Override
			public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

				final SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture.create();

				resultFuture.setException(new MobileServiceException("Error in filter 2"));

				return resultFuture;
			}
		});

		boolean hasException = false;

		try {
			// Call the lookup method
			client.getTable("TestTable").lookUp(1).get();

		} catch (Exception exception) {
			// Assert
			hasException = true;
			assertTrue(exception.getCause().getMessage().startsWith("Error in filter 2"));
		}

		assertTrue(hasException);
	}

	public void testRequestShouldntFailWith401ResponseAndWithoutOnResponseCallbackInvokation() throws Throwable {

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
		final ServiceFilterRequestMock requestMock = new ServiceFilterRequestMock(response);
		requestMock.setHasErrorOnExecute(true);

		// Add a new filter to the client
		client = client.withFilter(new ServiceFilter() {

			@Override
			public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

				return nextServiceFilterCallback.onNext(requestMock);
			}
		});

		// Add a new filter to the client
		client = client.withFilter(new ServiceFilter() {

			@Override
			public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

				final SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture.create();

				resultFuture.set(new ServiceFilterResponseMock());

				return resultFuture;
			}
		});

		try {
			// Call the lookup method
			PersonTestObject entity = client.getTable(PersonTestObject.class).lookUp(1).get();

			if (entity.getId() != 0) {
				fail("Since no onResponse wasn't invoked from the filter, there shouldn't be any kind of result (exception or entity)");
			}

		} catch (Exception exception) {
			if (exception != null) {
				fail("Since no onResponse wasn't invoked from the filter, there shouldn't be any kind of result (exception or entity)");
			}
		}
	}
}