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

import android.os.Build;
import android.test.InstrumentationTestCase;
import android.util.Pair;

import com.google.common.util.concurrent.ListenableFuture;
import com.google.common.util.concurrent.SettableFuture;
import com.google.gson.JsonObject;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.authentication.MobileServiceAuthenticationProvider;
import com.microsoft.windowsazure.mobileservices.authentication.MobileServiceUser;
import com.microsoft.windowsazure.mobileservices.http.HttpConstants;
import com.microsoft.windowsazure.mobileservices.http.NextServiceFilterCallback;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilter;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterRequest;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.framework.filters.ServiceFilterRequestMock;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.framework.filters.ServiceFilterResponseMock;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.test.types.IdPropertyTestClasses.IdPropertyMultipleIdsTestObject;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.test.types.IdPropertyTestClasses.IdPropertyWithGsonAnnotation;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.test.types.PersonTestObject;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.test.types.PersonTestObjectWithStringId;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.test.types.PersonTestObjectWithoutId;
import com.microsoft.windowsazure.mobileservices.table.MobileServiceJsonTable;
import com.microsoft.windowsazure.mobileservices.table.MobileServiceTable;
import com.squareup.okhttp.Headers;
import com.squareup.okhttp.Protocol;
import com.squareup.okhttp.internal.http.StatusLine;

import java.net.MalformedURLException;
import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.ExecutionException;

public class MobileServiceClientTests extends InstrumentationTestCase {
    String appUrl = "";

    protected void setUp() throws Exception {
        appUrl = "http://myapp.com/";
        super.setUp();
    }

    protected void tearDown() throws Exception {
        super.tearDown();
    }

    public void testNewMobileServiceClientWithEmptyAppUrlShouldThrowException() {
        try {
            new MobileServiceClient("", getInstrumentation().getTargetContext());
            fail("Expected Exception MalformedURLException");
        } catch (MalformedURLException e) {
            // do nothing, it's OK
        }
    }

    public void testNewMobileServiceClientWithNullAppUrlShouldThrowException() {
        try {
            new MobileServiceClient((String) null, getInstrumentation().getTargetContext());
            fail("Expected Exception MalformedURLException");
        } catch (MalformedURLException e) {
            // do nothing, it's OK
        }
    }

    public void testMobileServiceClientWithNullServiceFilterShouldThrowException() {
        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e1) {
            fail("This should not happen");
        }

        try {
            client.withFilter(null);
            fail("Expected Exception IllegalArgumentException");
        } catch (IllegalArgumentException e) {
            // do nothing, it's OK
        }
    }

    public void testIsLoginInProgressShouldReturnFalse() throws MalformedURLException {
        MobileServiceClient client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        assertFalse(client.isLoginInProgress());
    }

    public void testSetAndGetCurrentUserShouldReturnUser() throws MalformedURLException {
        String userId = "myUserId";
        MobileServiceUser user = new MobileServiceUser(userId);
        MobileServiceClient client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());

        client.setCurrentUser(user);
        MobileServiceUser currentUser = client.getCurrentUser();

        assertEquals(user, currentUser);
        assertEquals(userId, user.getUserId());
        assertEquals(userId, currentUser.getUserId());
    }

    public void testGetCurrentUserWithNoLoggedUserShouldReturnNull() throws MalformedURLException {
        MobileServiceClient client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        MobileServiceUser currentUser = client.getCurrentUser();

        assertNull(currentUser);
    }

    public void testGetTableShouldReturnTableWithGivenNameAndClient() throws MalformedURLException {
        MobileServiceClient client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        MobileServiceJsonTable table = client.getTable("MyTable");

        assertNotNull(table);
    }

    public void testGetTableWithEmptyNameShouldThrowException() {
        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e1) {
            fail("This should not fail");
        }

        try {
            client.getTable("");
            fail("Expected Exception IllegalArgumentException");
        } catch (IllegalArgumentException e) {
            // do nothing, it's OK
        }
    }

    public void testGetTableWithWhiteSpacedNameShouldThrowException() {
        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e1) {
            fail("This should not fail");
        }

        try {

            client.getTable(" ");
            fail("Expected Exception IllegalArgumentException");
        } catch (IllegalArgumentException e) {
            // do nothing, it's OK
        }
    }

    public void testGetTableWithNullNameShouldThrowException() {
        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e1) {
            fail("This should not happen");
        }
        try {
            client.getTable((String) null);
            fail("Expected Exception IllegalArgumentException");
        } catch (IllegalArgumentException e) {
            // do nothing, it's OK
        }
    }

    public void testGetTableWithClassWithIdMemberShouldWork() {
        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e1) {
            fail("This should not happen");
        }

        MobileServiceTable<PersonTestObject> table = client.getTable(PersonTestObject.class);
        assertEquals("PersonTestObject", table.getTableName());
    }

    public void testGetTableWithClassWithStringIdMemberShouldWork() {
        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e1) {
            fail("This should not happen");
        }

        MobileServiceTable<PersonTestObjectWithStringId> table = client.getTable(PersonTestObjectWithStringId.class);
        assertEquals("PersonTestObjectWithStringId", table.getTableName());
    }

    public void testGetTableWithClassWithIdAnnotationShouldWork() {
        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e1) {
            fail("This should not happen");
        }

        MobileServiceTable<IdPropertyWithGsonAnnotation> table = client.getTable(IdPropertyWithGsonAnnotation.class);
        assertEquals("IdPropertyWithGsonAnnotation", table.getTableName());
    }

    public void testGetTableWithClassWithoutIdShouldFail() {
        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e1) {
            fail("This should not happen");
        }

        try {
            @SuppressWarnings("unused")
            MobileServiceTable<PersonTestObjectWithoutId> table = client.getTable(PersonTestObjectWithoutId.class);
            fail("This should not happen");
        } catch (IllegalArgumentException e) {
            // do nothing, it's OK
        }
    }

    public void testGetTableWithClassWithMultipleIdPropertiesShouldFail() throws Throwable {
        String tableName = "MyTableName";
        MobileServiceClient client = null;

        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            fail("This should not happen");
        }

        try {
            // Get the MobileService table
            client.getTable(tableName, IdPropertyMultipleIdsTestObject.class);
            fail("This should fail");
        } catch (IllegalArgumentException e) {
            // It's ok
        }

    }

    public void testGetTableWithInterfaceShouldThrowException() {
        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e1) {
            fail("This should not fail");
        }

        try {

            client.getTable(MyInterface.class);
            fail("Expected Exception IllegalArgumentException");
        } catch (IllegalArgumentException e) {
            // do nothing, it's OK
        }
    }

    public void testGetTableWithAbstractClassShouldThrowException() {
        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e1) {
            fail("This should not fail");
        }

        try {

            client.getTable(MyAbstractClass.class);
            fail("Expected Exception IllegalArgumentException");
        } catch (IllegalArgumentException e) {
            // do nothing, it's OK
        }
    }

    public void testLoginShouldParseJsonUserCorreclty() throws Throwable {

        final String userId = "id";
        final String userToken = "userToken";

        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        client = client.withFilter(new ServiceFilter() {

            @Override
            public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {
                // User JSon Template
                String userJsonTemplate = "{\"user\":{\"userId\":\"%s\"}, \"authenticationToken\":\"%s\"}";
                // Create a mock response simulating an error
                ServiceFilterResponseMock response = new ServiceFilterResponseMock();
                response.setStatus(new StatusLine(Protocol.HTTP_2, 200, ""));
                response.setContent(String.format(userJsonTemplate, userId, userToken));

                // create a mock request to replace the existing one
                ServiceFilterRequestMock requestMock = new ServiceFilterRequestMock(response);
                return nextServiceFilterCallback.onNext(requestMock);
            }
        });

        MobileServiceUser user = client.login(MobileServiceAuthenticationProvider.Facebook, "oAuthToken").get();
        // Asserts
        assertNotNull(user);
        assertEquals(userId, user.getUserId());
        assertEquals(userToken, user.getAuthenticationToken());
    }

    public void testOperationShouldAddHeaders() throws Throwable {

        // Create client
        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a new filter to the client
        client = client.withFilter(new ServiceFilter() {

            @Override
            public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

                final SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture.create();

                int zumoInstallationHeaderIndex = -1;
                int zumoApiVersionHeader = -1;
                int zumoVersionHeader = -1;
                int userAgentHeaderIndex = -1;
                int acceptHeaderIndex = -1;
                int acceptEncodingHeaderIndex = -1;

                String installationHeader = "X-ZUMO-INSTALLATION-ID";
                String apiVersionHeader = "ZUMO-API-VERSION";
                String versionHeader = "X-ZUMO-VERSION";
                String userAgentHeader = "User-Agent";
                String acceptHeader = "Accept";
                String acceptEncodingHeader = "Accept-Encoding";
                String versionNumber = "2.0.2";
                String apiVersionNumber = "2.0.0";

                Headers headers = request.getHeaders();
                for (int i = 0; i < headers.size(); i++) {
                    if (headers.name(i) == installationHeader) {
                        zumoInstallationHeaderIndex = i;
                    } else if (headers.name(i) == apiVersionHeader) {
                        zumoApiVersionHeader = i;
                    } else if (headers.name(i) == versionHeader) {
                        zumoVersionHeader = i;
                    } else if (headers.name(i) == userAgentHeader) {
                        userAgentHeaderIndex = i;
                    } else if (headers.name(i) == acceptHeader) {
                        acceptHeaderIndex = i;
                    } else if (headers.name(i) == acceptEncodingHeader) {
                        acceptEncodingHeaderIndex = i;
                    }
                }

                if (zumoInstallationHeaderIndex == -1) {
                    resultFuture.setException(new Exception("zumoInstallationHeaderIndex == -1"));
                    return resultFuture;
                }
                if (zumoApiVersionHeader == -1) {
                    resultFuture.setException(new Exception("zumoApiVersionHeader == -1"));
                    return resultFuture;
                }
                if (zumoVersionHeader == -1) {
                    resultFuture.setException(new Exception("zumoVersionHeader == -1"));
                    return resultFuture;
                }
                if (userAgentHeaderIndex == -1) {
                    resultFuture.setException(new Exception("userAgentHeaderIndex == -1"));
                    return resultFuture;
                }
                if (acceptHeaderIndex == -1) {
                    resultFuture.setException(new Exception("acceptHeaderIndex == -1"));
                    return resultFuture;
                }
                if (acceptEncodingHeaderIndex == -1) {
                    resultFuture.setException(new Exception("acceptEncodingHeaderIndex == -1"));
                    return resultFuture;
                }

                String expectedUserAgent = String.format("ZUMO/%s (lang=%s; os=%s; os_version=%s; arch=%s; version=%s)", "1.0", "Java", "Android",
                        Build.VERSION.RELEASE, Build.CPU_ABI, versionNumber);

                if (headers.value(zumoInstallationHeaderIndex) == null) {
                    resultFuture.setException(new Exception("headers[zumoInstallationHeaderIndex] == null"));
                    return resultFuture;
                }

                if (!apiVersionNumber.equals(headers.value(zumoApiVersionHeader))) {
                    resultFuture.setException(new Exception("expectedAppKey != headers[zumoApiVersionHeader]"));
                    return resultFuture;
                }

                if (!expectedUserAgent.equals(headers.value(userAgentHeaderIndex))) {
                    resultFuture.setException(new Exception("expectedUserAgent != headers[userAgentHeaderIndex]"));
                    return resultFuture;
                }

                if (!versionNumber.equals(headers.value(zumoVersionHeader))) {
                    resultFuture.setException(new Exception(versionNumber + "!= headers[zumoVersionHeader]"));
                    return resultFuture;
                }

                if (!"application/json".equals(headers.value(acceptHeaderIndex))) {
                    resultFuture.setException(new Exception("application/json != headers[acceptHeaderIndex]"));
                    return resultFuture;
                }

                if (!"gzip".equals(headers.value(acceptEncodingHeaderIndex))) {
                    resultFuture.setException(new Exception("gzip != headers[acceptEncodingHeaderIndex]"));
                    return resultFuture;
                }

                ServiceFilterResponseMock response = new ServiceFilterResponseMock();
                response.setContent("{}");

                resultFuture.set(response);

                return resultFuture;
            }
        });

        try {
            client.getTable("dummy").execute().get();
        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                fail(exception.getCause().getMessage());
            } else {
                fail(exception.getMessage());
            }
        }
    }

    public void testOperationShouldNotReplaceWithDefaultHeaders() throws Throwable {

        // Create client
        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        final String acceptHeaderKey = "Accept";
        final String acceptEncodingHeaderKey = "Accept-Encoding";
        final String acceptHeaderValue = "text/plain";
        final String acceptEncodingHeaderValue = "deflate";

        List<Pair<String, String>> headers = new ArrayList<Pair<String, String>>();
        headers.add(new Pair<String, String>(acceptHeaderKey, acceptHeaderValue));
        headers.add(new Pair<String, String>(acceptEncodingHeaderKey, acceptEncodingHeaderValue));

        // Add a new filter to the client
        client = client.withFilter(new ServiceFilter() {

            @Override
            public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {
                int acceptHeaderIndex = -1;
                int acceptEncodingHeaderIndex = -1;
                int acceptHeaderCount = 0;
                int acceptEncodingHeaderCount = 0;

                final SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture.create();

                Headers headers = request.getHeaders();
                for (int i = 0; i < headers.size(); i++) {
                    if (headers.name(i) == acceptHeaderKey) {
                        acceptHeaderIndex = i;
                        acceptHeaderCount++;
                    } else if (headers.name(i) == acceptEncodingHeaderKey) {
                        acceptEncodingHeaderIndex = i;
                        acceptEncodingHeaderCount++;
                    }
                }

                if (acceptHeaderIndex == -1) {
                    resultFuture.setException(new Exception("acceptHeaderIndex == -1"));
                    return resultFuture;
                }
                if (acceptHeaderCount == -1) {
                    resultFuture.setException(new Exception("acceptHeaderCount == -1"));
                    return resultFuture;
                }
                if (acceptEncodingHeaderIndex == -1) {
                    resultFuture.setException(new Exception("acceptEncodingHeaderIndex == -1"));
                    return resultFuture;
                }
                if (acceptEncodingHeaderCount == -1) {
                    resultFuture.setException(new Exception("acceptEncodingHeaderIndex == -1"));
                    return resultFuture;
                }

                if (acceptHeaderValue.equals(headers.value(acceptHeaderIndex))) {
                    resultFuture.setException(new Exception("acceptHeaderValue != headers[acceptHeaderIndex]"));
                    return resultFuture;
                }

                if (acceptEncodingHeaderValue.equals(headers.value(acceptEncodingHeaderIndex))) {
                    resultFuture.setException(new Exception("acceptEncodingHeaderValue != headers[acceptEncodingHeaderIndex]"));
                    return resultFuture;
                }

                ServiceFilterResponseMock response = new ServiceFilterResponseMock();

                response.setContent("{}");

                resultFuture.set(response);

                return resultFuture;
            }
        });

        try {
            client.invokeApi("myApi", null, "POST", headers).get();
        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                fail(exception.getCause().getMessage());
            } else {
                fail(exception.getMessage());
            }
        }
    }

    public void testOperationDefaultHeadersShouldBeIdempotent() throws Throwable {

        // Create client
        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        final String acceptHeaderKey = "Accept";
        final String acceptEncodingHeaderKey = "Accept-Encoding";
        final String acceptHeaderValue = "application/json";
        final String acceptEncodingHeaderValue = "gzip";

        List<Pair<String, String>> headers = new ArrayList<Pair<String, String>>();
        headers.add(new Pair<String, String>(acceptHeaderKey, acceptHeaderValue));
        headers.add(new Pair<String, String>(acceptEncodingHeaderKey, acceptEncodingHeaderValue));

        // Add a new filter to the client
        client = client.withFilter(new ServiceFilter() {

            @Override
            public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {
                int acceptHeaderIndex = -1;
                int acceptEncodingHeaderIndex = -1;
                int acceptHeaderCount = 0;
                int acceptEncodingHeaderCount = 0;

                final SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture.create();

                Headers headers = request.getHeaders();
                for (int i = 0; i < headers.size(); i++) {
                    if (headers.name(i) == acceptHeaderKey) {
                        acceptHeaderIndex = i;
                        acceptHeaderCount++;
                    } else if (headers.name(i) == acceptEncodingHeaderKey) {
                        acceptEncodingHeaderIndex = i;
                        acceptEncodingHeaderCount++;
                    }
                }

                if (acceptHeaderIndex == -1) {
                    resultFuture.setException(new Exception("acceptHeaderIndex == -1"));
                    return resultFuture;
                }
                if (acceptHeaderCount == -1) {
                    resultFuture.setException(new Exception("acceptHeaderCount == -1"));
                    return resultFuture;
                }
                if (acceptEncodingHeaderIndex == -1) {
                    resultFuture.setException(new Exception("acceptEncodingHeaderIndex == -1"));
                    return resultFuture;
                }
                if (acceptEncodingHeaderCount == -1) {
                    resultFuture.setException(new Exception("acceptEncodingHeaderIndex == -1"));
                    return resultFuture;
                }

                ServiceFilterResponseMock response = new ServiceFilterResponseMock();
                response.setContent("{}");

                resultFuture.set(response);

                return resultFuture;
            }
        });

        try {
            client.invokeApi("myApi", null, "POST", headers).get();
        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                fail(exception.getCause().getMessage());
            } else {
                fail(exception.getMessage());
            }
        }
    }

    public void testInsertUpdateShouldAddContentTypeJson() throws Throwable {

        final SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture.create();

        // Create client
        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a new filter to the client
        client = client.withFilter(new ServiceFilter() {

            @Override
            public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

                Headers headers = request.getHeaders();

                boolean headerPresent = false;
                for (int i = 0; i < headers.size(); i++) {
                    if (headers.name(i).equals(HttpConstants.ContentType) && headers.value(i).equals("application/json")) {
                        headerPresent = true;
                    }
                }

                if (!headerPresent) {
                    resultFuture.setException(new Exception("!headerPresent"));
                    return resultFuture;
                }

                ServiceFilterResponseMock response = new ServiceFilterResponseMock();
                response.setContent("{}");

                resultFuture.set(response);

                return resultFuture;
            }
        });

        try {
            JsonObject jsonObject = new JsonObject();

            jsonObject.addProperty("someValue", 42);

            client.getTable("dummy").insert(jsonObject).get();

            jsonObject.addProperty("id", 1);

            client.getTable("dummy").update(jsonObject).get();
        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                fail(exception.getCause().getMessage());
            } else {
                fail(exception.getMessage());
            }
        }

    }

    public void testDeleteQueryShouldNotAddContentTypeJson() throws Throwable {

        // Create client
        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        }

        // Add a new filter to the client
        client = client.withFilter(new ServiceFilter() {

            @Override
            public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

                final SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture.create();

                Headers headers = request.getHeaders();

                boolean headerPresent = false;
                for (int i = 0; i < headers.size(); i++) {
                    if (headers.name(i).equals(HttpConstants.ContentType) && headers.value(i).equals("application/json")) {
                        headerPresent = true;
                    }
                }

                if (headerPresent) {
                    resultFuture.setException(new Exception("!headerPresent"));
                    return resultFuture;
                }

                ServiceFilterResponseMock response = new ServiceFilterResponseMock();
                response.setContent("{}");

                resultFuture.set(response);

                return resultFuture;
            }
        });

        try {
            client.getTable("dummy").delete(42).get();

            client.getTable("dummy").execute().get();
        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                fail(exception.getCause().getMessage());
            } else {
                fail(exception.getMessage());
            }
        }
    }

    interface MyInterface {

    }

    abstract class MyAbstractClass {
        int id;
        String name;
    }
}
