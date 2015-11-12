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

import android.annotation.TargetApi;
import android.os.Build;
import android.test.InstrumentationTestCase;
import android.util.Pair;

import com.google.common.util.concurrent.ListenableFuture;
import com.google.common.util.concurrent.SettableFuture;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.authentication.MobileServiceAuthenticationProvider;
import com.microsoft.windowsazure.mobileservices.http.NextServiceFilterCallback;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilter;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterRequest;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.framework.filters.ServiceFilterResponseMock;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.test.helpers.EncodingUtilities;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.test.types.PersonTestObject;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.test.types.ResultsContainer;

import junit.framework.Assert;

import java.net.MalformedURLException;
import java.util.ArrayList;
import java.util.List;
import java.util.Locale;

public class URLTests extends InstrumentationTestCase {

    String appUrl = "";

    @Override
    protected void setUp() throws Exception {
        appUrl = "http://myapp.com/";
        super.setUp();
    }

    public void testLoginURL() throws Throwable {
        testLoginURL(MobileServiceAuthenticationProvider.Facebook);
        testLoginURL(MobileServiceAuthenticationProvider.Twitter);
        testLoginURL(MobileServiceAuthenticationProvider.MicrosoftAccount);
        testLoginURL(MobileServiceAuthenticationProvider.Google);
        testLoginURL(MobileServiceAuthenticationProvider.WindowsAzureActiveDirectory);

        testLoginURL("facebook");
        testLoginURL("TWITTER");
        testLoginURL("GOOGLE");
        testLoginURL("MicrosoftAccount");
        testLoginURL("windowsAZUREactiveDIRECTORY");
        testLoginURL("AAD");
        testLoginURL("aad");
    }

    @TargetApi(Build.VERSION_CODES.HONEYCOMB)
    private void testLoginURL(final Object provider) throws Throwable {

        final ResultsContainer result = new ResultsContainer();

        // Create client
        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
        }

        // Add a new filter to the client
        client = client.withFilter(new ServiceFilter() {

            @Override
            public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

                result.setRequestUrl(request.getUrl());
                assertEquals("POST", request.getMethod());

                ServiceFilterResponseMock response = new ServiceFilterResponseMock();
                response.setContent("{authenticationToken:'123abc', user:{userId:'123456'}}");

                final SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture.create();

                resultFuture.set(response);

                return resultFuture;
            }
        });

        try {
            if (provider.getClass().equals(MobileServiceAuthenticationProvider.class)) {
                client.login((MobileServiceAuthenticationProvider) provider, "{\"myToken\":123}").get();
            } else {
                client.login((String) provider, "{\"myToken\":123}").get();
            }
        } catch (Exception exception) {
            Assert.fail();
        }

        String normalizedProvider = provider.toString().toLowerCase(Locale.getDefault());
        if (normalizedProvider.equals("windowsazureactivedirectory")) {
            normalizedProvider = "aad";
        }

        // Assert
        String expectedURL = appUrl + "login/" + normalizedProvider;
        assertEquals(expectedURL, result.getRequestUrl());

    }

    public void testUpdateURL() throws Throwable {
        final ResultsContainer result = new ResultsContainer();

        final String tableName = "dummy";
        final PersonTestObject person = new PersonTestObject("john", "doe", 10);
        person.setId(10);

        // Create client
        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
        }

        // Add a new filter to the client
        client = client.withFilter(new ServiceFilter() {

            @Override
            public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

                result.setRequestUrl(request.getUrl());
                assertEquals("PATCH", request.getMethod());

                ServiceFilterResponseMock response = new ServiceFilterResponseMock();
                response.setContent("{}");

                final SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture.create();

                resultFuture.set(response);

                return resultFuture;
            }
        });

        client.getTable(tableName, PersonTestObject.class).update(person).get();

        // Assert
        String expectedURL = appUrl + "tables/" + tableName + "/" + person.getId();
        assertEquals(expectedURL, result.getRequestUrl());
    }

    public void testUpdateURLWithParameters() throws Throwable {
        final ResultsContainer result = new ResultsContainer();

        final String tableName = "dummy";
        final PersonTestObject person = new PersonTestObject("john", "doe", 10);
        person.setId(10);
        final List<Pair<String, String>> parameters = new ArrayList<Pair<String, String>>();
        parameters.add(new Pair<String, String>("a key", "my <>&=?@ value"));

        // Create client
        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
        }

        // Add a new filter to the client
        client = client.withFilter(new ServiceFilter() {

            @Override
            public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

                result.setRequestUrl(request.getUrl());
                assertEquals("PATCH", request.getMethod());

                ServiceFilterResponseMock response = new ServiceFilterResponseMock();
                response.setContent("{}");

                final SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture.create();

                resultFuture.set(response);

                return resultFuture;
            }
        });

        client.getTable(tableName, PersonTestObject.class).update(person, parameters).get();

        // Assert
        String expectedURL = appUrl + "tables/" + tableName + "/" + person.getId()
                + EncodingUtilities.percentEncodeSpaces("?a key=my %3C%3E%26%3D%3F%40 value");
        assertEquals(expectedURL, result.getRequestUrl());
    }

    public void testInsertURL() throws Throwable {
        final ResultsContainer result = new ResultsContainer();

        final String tableName = "dummy";
        final PersonTestObject person = new PersonTestObject("john", "doe", 10);

        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
        }

        // Add a new filter to the client
        client = client.withFilter(new ServiceFilter() {

            @Override
            public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

                result.setRequestUrl(request.getUrl());
                assertEquals("POST", request.getMethod());

                ServiceFilterResponseMock response = new ServiceFilterResponseMock();
                response.setContent("{}");

                final SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture.create();

                resultFuture.set(response);

                return resultFuture;
            }
        });

        client.getTable(tableName, PersonTestObject.class).insert(person).get();

        // Assert
        String expectedURL = appUrl + "tables/" + tableName;
        assertEquals(expectedURL, result.getRequestUrl());
    }

    public void testInsertURLWithParameters() throws Throwable {
        final ResultsContainer result = new ResultsContainer();

        final String tableName = "dummy";
        final PersonTestObject person = new PersonTestObject("john", "doe", 10);
        final List<Pair<String, String>> parameters = new ArrayList<Pair<String, String>>();
        parameters.add(new Pair<String, String>("a key", "my <>&=?@ value"));

        // Create client
        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
        }

        // Add a new filter to the client
        client = client.withFilter(new ServiceFilter() {

            @Override
            public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

                result.setRequestUrl(request.getUrl());
                assertEquals("POST", request.getMethod());

                ServiceFilterResponseMock response = new ServiceFilterResponseMock();
                response.setContent("{}");

                final SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture.create();

                resultFuture.set(response);

                return resultFuture;
            }
        });

        client.getTable(tableName, PersonTestObject.class).insert(person, parameters).get();

        // Assert
        String expectedURL = appUrl + "tables/" + tableName + EncodingUtilities.percentEncodeSpaces("?a key=my %3C%3E%26%3D%3F%40 value");
        assertEquals(expectedURL, result.getRequestUrl());
    }

    public void testLookupURL() throws Throwable {
        final ResultsContainer result = new ResultsContainer();

        final String tableName = "dummy";
        final int id = 10;

        // Create client
        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
        }

        // Add a new filter to the client
        client = client.withFilter(new ServiceFilter() {

            @Override
            public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

                result.setRequestUrl(request.getUrl());
                assertEquals("GET", request.getMethod());

                ServiceFilterResponseMock response = new ServiceFilterResponseMock();
                response.setContent("{}");

                final SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture.create();

                resultFuture.set(response);

                return resultFuture;
            }
        });

        client.getTable(tableName).lookUp(id).get();

        // Assert
        String expectedURL = appUrl + "tables/" + tableName + "/" + id;
        assertEquals(expectedURL, result.getRequestUrl());
    }

    public void testLookupURLWithParameters() throws Throwable {
        final ResultsContainer result = new ResultsContainer();

        final String tableName = "dummy";
        final int id = 10;
        final List<Pair<String, String>> parameters = new ArrayList<Pair<String, String>>();
        parameters.add(new Pair<String, String>("a key", "my <>&=?@ value"));

        // Create client
        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
        }

        // Add a new filter to the client
        client = client.withFilter(new ServiceFilter() {

            @Override
            public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

                result.setRequestUrl(request.getUrl());
                assertEquals("GET", request.getMethod());

                ServiceFilterResponseMock response = new ServiceFilterResponseMock();
                response.setContent("{}");

                final SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture.create();

                resultFuture.set(response);

                return resultFuture;
            }
        });

        client.getTable(tableName).lookUp(id, parameters).get();

        // Assert
        String expectedURL = appUrl + "tables/" + tableName + "/" + id + EncodingUtilities.percentEncodeSpaces("?a key=my %3C%3E%26%3D%3F%40 value");
        assertEquals(expectedURL, result.getRequestUrl());
    }

    @TargetApi(Build.VERSION_CODES.HONEYCOMB)
    public void testDeleteURL() throws Throwable {
        final ResultsContainer result = new ResultsContainer();

        final String tableName = "dummy";
        final int id = 10;

        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
        }

        // Add a new filter to the client
        client = client.withFilter(new ServiceFilter() {

            @Override
            public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

                result.setRequestUrl(request.getUrl());
                assertEquals("DELETE", request.getMethod());

                ServiceFilterResponseMock response = new ServiceFilterResponseMock();
                response.setContent("{}");

                final SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture.create();

                resultFuture.set(response);

                return resultFuture;
            }
        });

        client.getTable(tableName).delete(id).get();

        // Assert
        String expectedURL = appUrl + "tables/" + tableName + "/" + id;
        assertEquals(expectedURL, result.getRequestUrl());
    }

    public void testDeleteURLWithParameters() throws Throwable {
        final ResultsContainer result = new ResultsContainer();

        final String tableName = "dummy";
        final int id = 10;
        final List<Pair<String, String>> parameters = new ArrayList<Pair<String, String>>();
        parameters.add(new Pair<String, String>("a key", "my <>&=?@ value"));

        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
        }

        // Add a new filter to the client
        client = client.withFilter(new ServiceFilter() {

            @Override
            public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

                result.setRequestUrl(request.getUrl());
                assertEquals("DELETE", request.getMethod());

                ServiceFilterResponseMock response = new ServiceFilterResponseMock();
                response.setContent("{}");

                final SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture.create();

                resultFuture.set(response);

                return resultFuture;
            }
        });

        client.getTable(tableName).delete(id, parameters).get();

        // Assert
        String expectedURL = appUrl + "tables/" + tableName + "/" + id + EncodingUtilities.percentEncodeSpaces("?a key=my %3C%3E%26%3D%3F%40 value");
        assertEquals(expectedURL, result.getRequestUrl());
    }

    public void testQueryURL() throws Throwable {
        final ResultsContainer result = new ResultsContainer();

        final String tableName = "dummy";

        // Create client
        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
        }

        // Add a new filter to the client
        client = client.withFilter(new ServiceFilter() {

            @Override
            public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

                result.setRequestUrl(request.getUrl());
                assertEquals("GET", request.getMethod());

                ServiceFilterResponseMock response = new ServiceFilterResponseMock();
                response.setContent("{}");

                final SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture.create();

                resultFuture.set(response);

                return resultFuture;
            }
        });

        client.getTable(tableName).execute().get();

        // Assert
        String expectedURL = appUrl + "tables/" + tableName;
        assertTrue(result.getRequestUrl().startsWith(expectedURL));
    }
}
