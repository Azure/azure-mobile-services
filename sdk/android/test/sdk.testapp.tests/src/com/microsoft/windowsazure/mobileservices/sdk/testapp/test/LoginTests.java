package com.microsoft.windowsazure.mobileservices.sdk.testapp.test;

import java.net.MalformedURLException;
import java.util.Locale;
import java.util.concurrent.CountDownLatch;

import junit.framework.Assert;

import org.apache.http.Header;
import org.apache.http.ProtocolVersion;
import org.apache.http.StatusLine;

import android.test.InstrumentationTestCase;

import com.google.gson.JsonElement;
import com.microsoft.windowsazure.mobileservices.MobileServiceAuthenticationProvider;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceException;
import com.microsoft.windowsazure.mobileservices.MobileServiceUser;
import com.microsoft.windowsazure.mobileservices.NextServiceFilterCallback;
import com.microsoft.windowsazure.mobileservices.ServiceFilter;
import com.microsoft.windowsazure.mobileservices.ServiceFilterRequest;
import com.microsoft.windowsazure.mobileservices.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.ServiceFilterResponseCallback;
import com.microsoft.windowsazure.mobileservices.TableJsonQueryCallback;
import com.microsoft.windowsazure.mobileservices.UserAuthenticationCallback;

public class LoginTests extends InstrumentationTestCase {

	String appUrl = "";
	String appKey = "";

	@Override
	protected void setUp() throws Exception {
		appUrl = "http://myapp.com/";
		appKey = "qwerty";
		super.setUp();
	}

	public void testLoginOperation() throws Throwable {
		testLoginOperation(MobileServiceAuthenticationProvider.Facebook);
		testLoginOperation(MobileServiceAuthenticationProvider.Twitter);
		testLoginOperation(MobileServiceAuthenticationProvider.MicrosoftAccount);
		testLoginOperation(MobileServiceAuthenticationProvider.Google);
	}

	private void testLoginOperation(
			final MobileServiceAuthenticationProvider provider)
			throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);
		final ResultsContainer result = new ResultsContainer();

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				// Create client
				MobileServiceClient client = null;
				try {
					client = new MobileServiceClient(appUrl, appKey);
				} catch (MalformedURLException e) {
				}

				// Add a new filter to the client
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(
							ServiceFilterRequest request,
							NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						result.setRequestUrl(request.getUrl());

						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setContent("{authenticationToken:'123abc', user:{userId:'123456'}}");

						responseCallback.onResponse(response);
					}
				});

				client.login(provider, "{myToken:123}",
						new UserAuthenticationCallback() {

							@Override
							public void onSuccess(MobileServiceUser user) {
								assertEquals("123456", user.getUserId());
								assertEquals("123abc",
										user.getAuthenticationToken());
								latch.countDown();
							}

							@Override
							public void onError(Exception exception,
									ServiceFilterResponse response) {
								Assert.fail();
								latch.countDown();
							}
						});
			}
		});

		latch.await();

		// Assert
		String expectedURL = appUrl + "login/"
				+ provider.toString().toLowerCase(Locale.getDefault());
		assertEquals(expectedURL, result.getRequestUrl());

	}

	public void testLoginShouldThrowError() throws Throwable {

		testLoginShouldThrowError(MobileServiceAuthenticationProvider.Facebook);
		testLoginShouldThrowError(MobileServiceAuthenticationProvider.MicrosoftAccount);
		testLoginShouldThrowError(MobileServiceAuthenticationProvider.Twitter);
		testLoginShouldThrowError(MobileServiceAuthenticationProvider.Google);
	}

	private void testLoginShouldThrowError(
			final MobileServiceAuthenticationProvider provider)
			throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);
		final ResultsContainer result = new ResultsContainer();
		final String errorMessage = "fake error";
		final String errorJson = "{error:'" + errorMessage + "'}";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				// Create client
				MobileServiceClient client = null;
				try {
					client = new MobileServiceClient(appUrl, appKey);
				} catch (MalformedURLException e) {
				}

				// Add a new filter to the client
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(
							ServiceFilterRequest request,
							NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						result.setRequestUrl(request.getUrl());

						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setContent(errorJson);
						response.setStatus(new StatusLine() {

							@Override
							public int getStatusCode() {
								return 400;
							}

							@Override
							public String getReasonPhrase() {
								return errorMessage;
							}

							@Override
							public ProtocolVersion getProtocolVersion() {
								return null;
							}
						});

						responseCallback.onResponse(response);
					}
				});

				client.login(provider, "{myToken:123}",
						new UserAuthenticationCallback() {

							@Override
							public void onSuccess(MobileServiceUser user) {
								Assert.fail();
								latch.countDown();
							}

							@Override
							public void onError(Exception exception,
									ServiceFilterResponse response) {
								assertTrue(exception instanceof MobileServiceException);
								MobileServiceException cause = (MobileServiceException) exception
										.getCause();
								assertEquals(errorMessage, cause.getMessage());
								latch.countDown();
							}
						});
			}
		});

		latch.await();
	}

	public void testAuthenticatedRequest() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);
		final MobileServiceUser user = new MobileServiceUser("dummyUser");
		user.setAuthenticationToken("123abc");

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				// Create client
				MobileServiceClient client = null;
				try {
					client = new MobileServiceClient(appUrl, appKey);
					client.setCurrentUser(user);
				} catch (MalformedURLException e) {
				}

				// Add a new filter to the client
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(
							ServiceFilterRequest request,
							NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						int headerIndex = -1;
						Header[] headers = request.getHeaders();
						for (int i = 0; i < headers.length; i++) {
							if (headers[i].getName() == "X-ZUMO-AUTH") {
								headerIndex = i;
							}
						}
						if (headerIndex == -1) {
							Assert.fail();
						}

						assertEquals(user.getAuthenticationToken(),
								headers[headerIndex].getValue());

						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setContent("{}");

						responseCallback.onResponse(response);
					}
				});

				client.getTable("dummy").all()
						.execute(new TableJsonQueryCallback() {

							@Override
							public void onSuccess(JsonElement result, int count) {
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
	}

	public void testLoginWithEmptyTokenShouldFail() throws Throwable {

		// Create client
		MobileServiceClient client = null;
		try {
			client = new MobileServiceClient(appUrl, appKey);
		} catch (MalformedURLException e) {
		}

		String token = null;
		try {
			client.login(MobileServiceAuthenticationProvider.Facebook, token,
					null);
			Assert.fail();
		} catch (IllegalArgumentException e) {
			// It should throw an exception
		}

		try {
			client.login(MobileServiceAuthenticationProvider.Facebook, "", null);
			Assert.fail();
		} catch (IllegalArgumentException e) {
			// It should throw an exception
		}
	}

	public void testLogout() {

		// Create client
		MobileServiceClient client = null;
		try {
			client = new MobileServiceClient(appUrl, appKey);
		} catch (MalformedURLException e) {
		}

		client.setCurrentUser(new MobileServiceUser("abc"));

		client.logout();

		assertNull(client.getCurrentUser());
	}
}
