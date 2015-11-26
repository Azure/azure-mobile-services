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

import android.net.Uri;
import android.test.InstrumentationTestCase;

import com.google.common.util.concurrent.ListenableFuture;
import com.microsoft.windowsazure.mobileservices.MobileServiceApplication;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.http.HttpConstants;
import com.microsoft.windowsazure.mobileservices.http.NextServiceFilterCallback;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilter;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterRequest;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.notifications.MobileServicePush;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.framework.filters.ServiceFilterRequestMock;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.framework.filters.ServiceFilterResponseMock;
import com.squareup.okhttp.Protocol;
import com.squareup.okhttp.internal.http.StatusLine;

import junit.framework.Assert;

import java.util.concurrent.ExecutionException;

public class EnhancedPushTests extends InstrumentationTestCase {

    final String appUrl = "http://myapp.com/";
    final String pnsApiUrl = "push";

    public void testUnregister() throws Throwable {

        final Container container = new Container();

        MobileServiceClient client = null;

        String installationId = MobileServiceApplication.getInstallationId(getInstrumentation().getTargetContext());

        final String expectedUrl = appUrl + pnsApiUrl + "/installations/" + Uri.encode(installationId);

        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());

            client = client.withFilter(new ServiceFilter() {

                @Override
                public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

                    container.requestUrl = request.getUrl();
                    container.requestMethod = request.getMethod();

                    ServiceFilterResponseMock mockResponse = new ServiceFilterResponseMock();
                    mockResponse.setStatus(new StatusLine(Protocol.HTTP_2, 204, ""));

                    ServiceFilterRequestMock mockRequest = new ServiceFilterRequestMock(mockResponse);

                    return nextServiceFilterCallback.onNext(mockRequest);
                }
            });

            final MobileServicePush push = client.getPush();
            push.unregister().get();

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.exception = (Exception) exception.getCause();
            } else {
                container.exception = exception;
            }

            fail(container.exception.getMessage());
        }

        // Asserts
        Assert.assertEquals(expectedUrl, container.requestUrl);
        Assert.assertEquals(HttpConstants.DeleteMethod, container.requestMethod);
    }

    public void testRegister() throws Throwable {

        final Container container = new Container();

        MobileServiceClient client = null;
        final String handle = "handle";

        String installationId = MobileServiceApplication.getInstallationId(getInstrumentation().getTargetContext());

        final String expectedUrl = appUrl + pnsApiUrl + "/installations/" + Uri.encode(installationId);
        final String expectedContent = "{\"pushChannel\":\"handle\",\"platform\":\"gcm\"}";
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());

            client = client.withFilter(new ServiceFilter() {

                @Override
                public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

                    container.requestUrl = request.getUrl();
                    container.requestContent = request.getContent();
                    container.requestMethod = request.getMethod();

                    ServiceFilterResponseMock mockResponse = new ServiceFilterResponseMock();
                    mockResponse.setStatus(new StatusLine(Protocol.HTTP_2, 204, ""));

                    ServiceFilterRequestMock mockRequest = new ServiceFilterRequestMock(mockResponse);

                    return nextServiceFilterCallback.onNext(mockRequest);
                }
            });

            final MobileServicePush push = client.getPush();
            push.register(handle).get();

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.exception = (Exception) exception.getCause();
            } else {
                container.exception = exception;
            }

            fail(container.exception.getMessage());
        }

        // Asserts
        Assert.assertEquals(expectedUrl, container.requestUrl);
        Assert.assertEquals(expectedContent, container.requestContent);
        Assert.assertEquals(HttpConstants.PutMethod, container.requestMethod);
    }

    public void testRegisterTemplate() throws Throwable {

        final Container container = new Container();

        MobileServiceClient client = null;
        final String handle = "handle";

        String installationId = MobileServiceApplication.getInstallationId(getInstrumentation().getTargetContext());

        final String expectedUrl = appUrl + pnsApiUrl + "/installations/" + Uri.encode(installationId);
        final String expectedContent =
                "{\"pushChannel\":\"handle\",\"platform\":\"gcm\",\"templates\":{\"template1\":{\"body\":\"{\\\"data\\\":\\\"abc\\\"}\"}}}";
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());

            client = client.withFilter(new ServiceFilter() {

                @Override
                public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

                    container.requestUrl = request.getUrl();
                    container.requestContent = request.getContent();
                    container.requestMethod = request.getMethod();

                    ServiceFilterResponseMock mockResponse = new ServiceFilterResponseMock();
                    mockResponse.setStatus(new StatusLine(Protocol.HTTP_2, 204, ""));

                    ServiceFilterRequestMock mockRequest = new ServiceFilterRequestMock(mockResponse);

                    return nextServiceFilterCallback.onNext(mockRequest);
                }
            });

            final MobileServicePush push = client.getPush();
            push.registerTemplate(handle, "template1", "{\"data\":\"abc\"}").get();

        } catch (Exception exception) {
            if (exception instanceof ExecutionException) {
                container.exception = (Exception) exception.getCause();
            } else {
                container.exception = exception;
            }

            fail(container.exception.getMessage());
        }

        // Asserts
        Assert.assertEquals(expectedUrl, container.requestUrl);
        Assert.assertEquals(expectedContent, container.requestContent);
        Assert.assertEquals(HttpConstants.PutMethod, container.requestMethod);
    }
    private class Container {

        public String requestContent;
        public String requestUrl;
        public String requestMethod;

        public Exception exception;
    }
}

