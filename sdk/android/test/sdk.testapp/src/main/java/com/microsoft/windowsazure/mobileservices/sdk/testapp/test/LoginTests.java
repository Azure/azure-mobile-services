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

import android.test.InstrumentationTestCase;

import com.google.common.util.concurrent.ListenableFuture;
import com.google.common.util.concurrent.SettableFuture;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceException;
import com.microsoft.windowsazure.mobileservices.UserAuthenticationCallback;
import com.microsoft.windowsazure.mobileservices.authentication.MobileServiceAuthenticationProvider;
import com.microsoft.windowsazure.mobileservices.authentication.MobileServiceUser;
import com.microsoft.windowsazure.mobileservices.http.NextServiceFilterCallback;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilter;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterRequest;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.framework.filters.ServiceFilterResponseMock;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.test.types.ResultsContainer;

import junit.framework.Assert;

import org.apache.http.Header;
import org.apache.http.ProtocolVersion;
import org.apache.http.StatusLine;

import java.net.MalformedURLException;
import java.util.HashMap;
import java.util.Locale;
import java.util.concurrent.CountDownLatch;

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

        testLoginOperation("FaCeBoOk");
        testLoginOperation("twitter");
        testLoginOperation("MicrosoftAccount");
        testLoginOperation("GOOGLE");
    }

    private void testLoginOperation(final Object provider) throws Throwable {
        final ResultsContainer result = new ResultsContainer();

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

                result.setRequestUrl(request.getUrl());

                ServiceFilterResponseMock response = new ServiceFilterResponseMock();
                response.setContent("{authenticationToken:'123abc', user:{userId:'123456'}}");

                final SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture.create();

                resultFuture.set(response);

                return resultFuture;
            }
        });

        try {

            MobileServiceUser user = null;

            if (provider.getClass().equals(MobileServiceAuthenticationProvider.class)) {
                user = client.login((MobileServiceAuthenticationProvider) provider, "{myToken:123}").get();

            } else {
                user = client.login((String) provider, "{myToken:123}").get();
            }

            assertEquals("123456", user.getUserId());
            assertEquals("123abc", user.getAuthenticationToken());

        } catch (Exception exception) {
            Assert.fail();
        }

        // Assert
        String expectedURL = appUrl + "login/" + provider.toString().toLowerCase(Locale.getDefault());
        assertEquals(expectedURL, result.getRequestUrl());
    }

    public void testLoginOperationWithParameter() throws Throwable {
        testLoginOperationWithParameter(MobileServiceAuthenticationProvider.Facebook);
        testLoginOperationWithParameter(MobileServiceAuthenticationProvider.Twitter);
        testLoginOperationWithParameter(MobileServiceAuthenticationProvider.MicrosoftAccount);
        testLoginOperationWithParameter(MobileServiceAuthenticationProvider.Google);

        testLoginOperationWithParameter("FaCeBoOk");
        testLoginOperationWithParameter("twitter");
        testLoginOperationWithParameter("MicrosoftAccount");
        testLoginOperationWithParameter("GOOGLE");
    }

    private void testLoginOperationWithParameter(final Object provider) throws Throwable {
        final ResultsContainer result = new ResultsContainer();

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

                result.setRequestUrl(request.getUrl());

                ServiceFilterResponseMock response = new ServiceFilterResponseMock();
                response.setContent("{authenticationToken:'123abc', user:{userId:'123456'}}");

                final SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture.create();

                resultFuture.set(response);

                return resultFuture;
            }
        });


        HashMap<String, String> parameters = new HashMap<>();

        parameters.put("p1", "p1value");
        parameters.put("p2", "p2value");


        String parameterQueryString = "?p2=p2value&p1=p1value";

        try {
            MobileServiceUser user;

            if (provider.getClass().equals(MobileServiceAuthenticationProvider.class)) {
                client.login((MobileServiceAuthenticationProvider) provider, "{myToken:123}", parameters).get();
            } else {
                client.login((String) provider, "{myToken:123}", parameters).get();
            }

        } catch (Exception exception) {
            Assert.fail();
        }

        // Assert
        String expectedURL = appUrl + "login/" + provider.toString().toLowerCase(Locale.getDefault()) + parameterQueryString;
        assertEquals(expectedURL, result.getRequestUrl());
    }

    public void testLoginCallbackOperation() throws Throwable {
        testLoginCallbackOperation(MobileServiceAuthenticationProvider.Facebook);
        testLoginCallbackOperation(MobileServiceAuthenticationProvider.Twitter);
        testLoginCallbackOperation(MobileServiceAuthenticationProvider.MicrosoftAccount);
        testLoginCallbackOperation(MobileServiceAuthenticationProvider.Google);

        testLoginCallbackOperation("FaCeBoOk");
        testLoginCallbackOperation("twitter");
        testLoginCallbackOperation("MicrosoftAccount");
        testLoginCallbackOperation("GOOGLE");
    }

    @SuppressWarnings("deprecation")
    private void testLoginCallbackOperation(final Object provider) throws Throwable {
        final CountDownLatch latch = new CountDownLatch(1);
        final ResultsContainer result = new ResultsContainer();

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

                result.setRequestUrl(request.getUrl());

                ServiceFilterResponseMock response = new ServiceFilterResponseMock();
                response.setContent("{authenticationToken:'123abc', user:{userId:'123456'}}");

                final SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture.create();

                resultFuture.set(response);

                return resultFuture;
            }
        });

        UserAuthenticationCallback callback = new UserAuthenticationCallback() {

            @Override
            public void onCompleted(MobileServiceUser user, Exception exception, ServiceFilterResponse response) {
                if (exception == null) {
                    assertEquals("123456", user.getUserId());
                    assertEquals("123abc", user.getAuthenticationToken());
                } else {
                    Assert.fail();
                }

                latch.countDown();
            }
        };

        if (provider.getClass().equals(MobileServiceAuthenticationProvider.class)) {
            client.login((MobileServiceAuthenticationProvider) provider, "{myToken:123}", callback);
        } else {
            client.login((String) provider, "{myToken:123}", callback);
        }

        latch.await();

        // Assert
        String expectedURL = appUrl + "login/" + provider.toString().toLowerCase(Locale.getDefault());
        assertEquals(expectedURL, result.getRequestUrl());
    }

    public void testLoginShouldThrowError() throws Throwable {

        testLoginShouldThrowError(MobileServiceAuthenticationProvider.Facebook);
        testLoginShouldThrowError(MobileServiceAuthenticationProvider.MicrosoftAccount);
        testLoginShouldThrowError(MobileServiceAuthenticationProvider.Twitter);
        testLoginShouldThrowError(MobileServiceAuthenticationProvider.Google);
    }

    private void testLoginShouldThrowError(final MobileServiceAuthenticationProvider provider) throws Throwable {
        final ResultsContainer result = new ResultsContainer();
        final String errorMessage = "fake error";
        final String errorJson = "{error:'" + errorMessage + "'}";

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

                final SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture.create();

                resultFuture.set(response);

                return resultFuture;
            }
        });

        try {
            client.login(provider, "{myToken:123}").get();
            Assert.fail();
        } catch (Exception exception) {
            assertTrue(exception.getCause() instanceof MobileServiceException);
            MobileServiceException cause = (MobileServiceException) exception.getCause().getCause();
            assertEquals(errorMessage, cause.getMessage());
        }
    }

    public void testAuthenticatedRequest() throws Throwable {
        final MobileServiceUser user = new MobileServiceUser("dummyUser");
        user.setAuthenticationToken("123abc");

        // Create client
        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
            client.setCurrentUser(user);
        } catch (MalformedURLException e) {
        }

        // Add a new filter to the client
        client = client.withFilter(new ServiceFilter() {

            @Override
            public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

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

                assertEquals(user.getAuthenticationToken(), headers[headerIndex].getValue());

                ServiceFilterResponseMock response = new ServiceFilterResponseMock();
                response.setContent("{}");

                final SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture.create();

                resultFuture.set(response);

                return resultFuture;
            }
        });

        client.getTable("dummy").execute().get();
    }

    public void testLoginWithEmptyTokenShouldFail() throws Throwable {

        // Create client
        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
        }

        String token = null;
        try {
            client.login(MobileServiceAuthenticationProvider.Facebook, token).get();
            Assert.fail();
        } catch (IllegalArgumentException e) {
            // It should throw an exception
        }

        try {
            client.login(MobileServiceAuthenticationProvider.Facebook, "").get();
            Assert.fail();
        } catch (IllegalArgumentException e) {
            // It should throw an exception
        }
    }

    public void testLoginWithEmptyProviderShouldFail() throws Throwable {

        // Create client
        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
        }

        String provider = null;
        try {
            client.login(provider, "{myToken:123}").get();
            Assert.fail();
        } catch (IllegalArgumentException e) {
            // It should throw an exception
        }

        try {
            client.login("", "{myToken:123}").get();
            Assert.fail();
        } catch (IllegalArgumentException e) {
            // It should throw an exception
        }
    }

    public void testLogout() {

        // Create client
        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
        }

        client.setCurrentUser(new MobileServiceUser("abc"));

        client.logout();

        assertNull(client.getCurrentUser());
    }
}
