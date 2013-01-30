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

	public void testClientWithFilterShouldReturnResponseWithHeaderAddedInFilter()
			throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container for results
		final ResultsContainer container = new ResultsContainer();

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				// Create client
				MobileServiceClient client = null;
				try {
					client = new MobileServiceClient(appUrl, appKey);
				} catch (MalformedURLException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				}

				// Add a new filter to the client
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(
							ServiceFilterRequest request,
							NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						container.setCount(1);
						container.setRequestContent("Filter1");

						responseCallback
								.onResponse(new ServiceFilterResponseMock());
					}
				});

				// Execute any action in order to generate a request
				client.getTable("TestTable").lookUp(1,
						new TableJsonOperationCallback() {

							@Override
							public void onSuccess(JsonObject jsonEntity) {
								latch.countDown();

							}

							@Override
							public void onError(Exception exception,
									ServiceFilterResponse response) {
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

	public void testClientWithFilterShouldReturnResponseWithHeadersAddedInOrder()
			throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container for results
		final ResultsContainer container = new ResultsContainer();

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				// Create client
				MobileServiceClient client = null;
				try {
					client = new MobileServiceClient(appUrl, appKey);
				} catch (MalformedURLException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				}

				// Add 2 new filters to the client.
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(
							ServiceFilterRequest request,
							NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						int currentCount = container.getCount() + 1;
						container.setCount(currentCount);
						String currentContent = container.getRequestContent()
								+ "Filter1";
						container.setRequestContent(currentContent);

						nextServiceFilterCallback.onNext(request,
								responseCallback);
					}
				}).withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(
							ServiceFilterRequest request,
							NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						int currentCount = container.getCount() + 1;
						container.setCount(currentCount);
						String currentContent = "Filter2";
						container.setRequestContent(currentContent);

						nextServiceFilterCallback.onNext(request,
								responseCallback);
					}
				});

				// Execute any action in order to generate a request
				client.getTable("TestTable").lookUp(1,
						new TableJsonOperationCallback() {

							@Override
							public void onSuccess(JsonObject jsonEntity) {
								latch.countDown();
							}

							@Override
							public void onError(Exception exception,
									ServiceFilterResponse response) {
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

	public void testClientWithFilterShouldNotExecuteTheFirstFilter()
			throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container for results
		final ResultsContainer container = new ResultsContainer();

		runTestOnUiThread(new Runnable() {
			@Override
			public void run() {
				// Create client
				MobileServiceClient client = null;
				try {
					client = new MobileServiceClient(appUrl, appKey);
				} catch (MalformedURLException e1) {
					// TODO Auto-generated catch block
					e1.printStackTrace();
				}

				// Add 2 new filters to the client.
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(
							ServiceFilterRequest request,
							NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						int currentCount = container.getCount() + 1;
						container.setCount(currentCount);
						String currentContent = container.getRequestContent()
								+ "Filter1";
						container.setRequestContent(currentContent);

						nextServiceFilterCallback.onNext(request,
								responseCallback);
					}

				}).withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(
							ServiceFilterRequest request,
							NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						int currentCount = container.getCount() + 1;
						container.setCount(currentCount);
						String currentContent = "Filter2";
						container.setRequestContent(currentContent);

						ServiceFilterResponse response = null;
						for (int i = 0; i < 3; i++) {
							try {
								response = request.execute();
							} catch (Exception e) {
								responseCallback.onError(e, response);
							}
						}

						responseCallback.onResponse(response);
					}
				});

				// Execute any action in order to generate a request
				client.getTable("TestTable").lookUp(1,
						new TableJsonOperationCallback() {

							@Override
							public void onSuccess(JsonObject jsonEntity) {
								latch.countDown();
							}

							@Override
							public void onError(Exception exception,
									ServiceFilterResponse response) {
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

	public void testClientWithFilterShouldThrowExceptionWhenExecutingFilter()
			throws Throwable {
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
					client = new MobileServiceClient(appUrl, appKey);
				} catch (MalformedURLException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				}

				// Create ServiceFilterRequestMock that returns the given
				// response
				ServiceFilterRequestMock request = new ServiceFilterRequestMock(
						response);
				request.setHasErrorOnExecute(true);

				// Add a new filter to the client
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(
							ServiceFilterRequest request,
							NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						responseCallback.onError(new MobileServiceException(
								"Error in filter 1"), response);
					}
				});

				// Execute any action in order to generate a request
				client.getTable("TestTable").lookUp(1,
						new TableJsonOperationCallback() {

							@Override
							public void onSuccess(JsonObject jsonEntity) {
								latch.countDown();
							}

							@Override
							public void onError(Exception exception,
									ServiceFilterResponse response) {
								container.setErrorMessage(exception
										.getMessage());
								latch.countDown();
							}
						});
			}
		});

		latch.await();

		// Assert
		assertTrue(container.getErrorMessage().startsWith("Error in filter 1"));
	}

	public void testClientWithFilterShouldThrowExceptionWhenExecutingTheFirstFilter()
			throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		final ResultsContainer container = new ResultsContainer();

		runTestOnUiThread(new Runnable() {
			@Override
			public void run() {
				// Create client
				MobileServiceClient client = null;
				try {
					client = new MobileServiceClient(appUrl, appKey);
				} catch (MalformedURLException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				}

				// Create ServiceFilterResponseMock
				final ServiceFilterResponseMock response = new ServiceFilterResponseMock();
				response.setContent("Response Content");
				response.setStatus(new StatusLineMock(200));

				// Create ServiceFilterRequestMock that returns the given
				// response
				ServiceFilterRequestMock request = new ServiceFilterRequestMock(
						response);
				request.setHasErrorOnExecute(true);
				// Add a new filter to the client
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(
							ServiceFilterRequest request,
							NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						container
								.setErrorMessage("this handler should never be called");
					}
				}).withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(
							ServiceFilterRequest request,
							NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {
						responseCallback.onError(new MobileServiceException(
								"Error in filter 2"), response);
					}
				});

				// Execute any action in order to generate a request
				client.getTable("TestTable").lookUp(1,
						new TableJsonOperationCallback() {

							@Override
							public void onSuccess(JsonObject jsonEntity) {
								latch.countDown();
							}

							@Override
							public void onError(Exception exception,
									ServiceFilterResponse response) {
								container.setErrorMessage(exception
										.getMessage());
								latch.countDown();
							}
						});
			}
		});

		latch.await();

		// Assert
		assertTrue(container.getErrorMessage().startsWith("Error in filter 2"));
	}
}
