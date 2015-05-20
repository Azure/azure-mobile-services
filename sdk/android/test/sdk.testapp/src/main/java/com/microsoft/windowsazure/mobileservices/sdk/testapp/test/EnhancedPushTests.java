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

import android.content.Context;
import android.content.SharedPreferences;
import android.preference.PreferenceManager;
import android.test.InstrumentationTestCase;

import com.google.common.util.concurrent.ListenableFuture;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.http.NextServiceFilterCallback;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilter;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterRequest;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.notifications.MobileServicePush;
import com.microsoft.windowsazure.mobileservices.notifications.Registration;
import com.microsoft.windowsazure.mobileservices.notifications.RegistrationCallback;
import com.microsoft.windowsazure.mobileservices.notifications.RegistrationGoneException;
import com.microsoft.windowsazure.mobileservices.notifications.TemplateRegistration;
import com.microsoft.windowsazure.mobileservices.notifications.TemplateRegistrationCallback;
import com.microsoft.windowsazure.mobileservices.notifications.UnregisterCallback;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.framework.filters.ServiceFilterRequestMock;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.framework.filters.ServiceFilterResponseMock;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.framework.filters.StatusLineMock;

import junit.framework.Assert;

import org.apache.http.Header;
import org.apache.http.HeaderElement;
import org.apache.http.ParseException;

import java.util.List;
import java.util.UUID;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.ExecutionException;

public class EnhancedPushTests extends InstrumentationTestCase {

    /**
     * Name for default registration
     */
    static final String DEFAULT_REGISTRATION_NAME = "$Default";
    /**
     * Prefix for Storage keys
     */
    private static final String STORAGE_PREFIX = "__NH_";
    /**
     * Prefix for registration information keys in local storage
     */
    private static final String REGISTRATION_NAME_STORAGE_KEY = "REG_NAME_";
    /**
     * New registration location header name
     */
    private static final String NEW_REGISTRATION_LOCATION_HEADER = "Location";

    String appUrl = "";
    String appKey = "";

    private static void forceRefreshSync(MobileServicePush push, String handle) throws InterruptedException, ExecutionException {
        push.unregisterAll(handle).get();
    }

    private static boolean matchTags(final String[] tags, List<String> regTags) {
        if (tags == null || regTags == null) {
            return (tags == null && regTags == null) || (tags == null && regTags.size() == 0) || (regTags == null && tags.length == 0);
        } else if (regTags.size() != tags.length) {
            return false;
        } else {
            for (String tag : tags) {
                if (!regTags.contains(tag)) {
                    return false;
                }
            }
        }

        return true;
    }

    private static ServiceFilter getUpsertTestFilter(final String registrationId) {
        return new ServiceFilter() {

            @Override
            public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {
                ServiceFilterResponseMock response = new ServiceFilterResponseMock();
                response.setStatus(new StatusLineMock(400));

                final String url = request.getUrl();
                String method = request.getMethod();

                if (method == "POST" && url.contains("registrationids/")) {
                    response = new ServiceFilterResponseMock();
                    response.setStatus(new StatusLineMock(201));
                    response.setHeaders(new Header[]{new Header() {

                        @Override
                        public String getValue() {
                            return url + registrationId;
                        }

                        @Override
                        public String getName() {
                            return NEW_REGISTRATION_LOCATION_HEADER;
                        }

                        @Override
                        public HeaderElement[] getElements() throws ParseException {
                            return null;
                        }
                    }});
                } else if (method == "PUT" && url.contains("registrations/" + registrationId)) {
                    response = new ServiceFilterResponseMock();
                    response.setStatus(new StatusLineMock(204));
                } else if (method == "PUT" && url.contains("registrations/")) {
                    response = new ServiceFilterResponseMock();
                    response.setStatus(new StatusLineMock(204));
                } else if (method == "DELETE" && url.contains("registrations/")) {
                    response = new ServiceFilterResponseMock();
                    response.setStatus(new StatusLineMock(204));
                } else if (method == "GET" && url.contains("registrations/")) {
                    response = new ServiceFilterResponseMock();
                    response.setStatus(new StatusLineMock(200));
                    response.setContent("[ ]");
                }

                // create a mock request to replace the existing one
                ServiceFilterRequestMock requestMock = new ServiceFilterRequestMock(response);
                return nextServiceFilterCallback.onNext(requestMock);
            }
        };
    }

    private static ServiceFilter getUpsertFailTestFilter(final String registrationId) {
        return new ServiceFilter() {

            @Override
            public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {
                ServiceFilterResponseMock response = new ServiceFilterResponseMock();
                response.setStatus(new StatusLineMock(400));

                final String url = request.getUrl();
                String method = request.getMethod();

                if (method == "POST" && url.contains("registrationids/")) {
                    response = new ServiceFilterResponseMock();
                    response.setStatus(new StatusLineMock(201));
                    response.setHeaders(new Header[]{new Header() {

                        @Override
                        public String getValue() {
                            return url + registrationId;
                        }

                        @Override
                        public String getName() {
                            return NEW_REGISTRATION_LOCATION_HEADER;
                        }

                        @Override
                        public HeaderElement[] getElements() throws ParseException {
                            return null;
                        }
                    }});
                } else if (method == "PUT" && url.contains("registrations/")) {
                    response = new ServiceFilterResponseMock();
                    response.setStatus(new StatusLineMock(410));
                } else if (method == "DELETE" && url.contains("registrations/")) {
                    response = new ServiceFilterResponseMock();
                    response.setStatus(new StatusLineMock(204));
                } else if (method == "GET" && url.contains("registrations/")) {
                    response = new ServiceFilterResponseMock();
                    response.setStatus(new StatusLineMock(200));
                    response.setContent("[ ]");
                }

                // create a mock request to replace the existing one
                ServiceFilterRequestMock requestMock = new ServiceFilterRequestMock(response);
                return nextServiceFilterCallback.onNext(requestMock);
            }
        };
    }

    protected void setUp() throws Exception {
        appUrl = "http://myapp.com/";
        appKey = "qwerty";
        super.setUp();
    }

    protected void tearDown() throws Exception {
        super.tearDown();
    }

    public void testRegisterUnregisterNative() throws Throwable {
        Context context = getInstrumentation().getTargetContext();
        final SharedPreferences sharedPreferences = PreferenceManager.getDefaultSharedPreferences(context.getApplicationContext());

        final Container container = new Container();
        final String handle = "handle";

        String registrationId = "registrationId";

        MobileServiceClient client = new MobileServiceClient(appUrl, appKey, context);

        client = client.withFilter(getUpsertTestFilter(registrationId));

        final MobileServicePush push = client.getPush();

        forceRefreshSync(push, handle);

        try {

            Registration registration = push.register(handle, new String[]{"tag1"}).get();

            container.registrationId = registration.getRegistrationId();

            container.storedRegistrationId = sharedPreferences.getString(STORAGE_PREFIX + REGISTRATION_NAME_STORAGE_KEY + DEFAULT_REGISTRATION_NAME, null);

            push.unregister().get();

            container.unregister = sharedPreferences.getString(STORAGE_PREFIX + REGISTRATION_NAME_STORAGE_KEY + DEFAULT_REGISTRATION_NAME, null);

        } catch (Exception exception) {

            if (exception instanceof ExecutionException) {
                container.exception = (Exception) exception.getCause();
            } else {
                container.exception = exception;
            }

            fail(container.exception.getMessage());
        }

        Assert.assertEquals(registrationId, container.storedRegistrationId);
        Assert.assertEquals(registrationId, container.registrationId);
        Assert.assertNull(container.unregister);
    }

    @SuppressWarnings("deprecation")
    public void testRegisterUnregisterNativeCallback() throws Throwable {
        final CountDownLatch latch = new CountDownLatch(1);

        Context context = getInstrumentation().getTargetContext();
        final SharedPreferences sharedPreferences = PreferenceManager.getDefaultSharedPreferences(context.getApplicationContext());

        final Container container = new Container();
        final String handle = "handle";

        String registrationId = "registrationId";

        MobileServiceClient client = new MobileServiceClient(appUrl, appKey, context);

        client = client.withFilter(getUpsertTestFilter(registrationId));

        final MobileServicePush push = client.getPush();

        forceRefreshSync(push, handle);

        push.register(handle, new String[]{"tag1"}, new RegistrationCallback() {

            @Override
            public void onRegister(Registration registration, Exception exception) {
                if (exception != null) {
                    container.exception = exception;

                    latch.countDown();
                } else {
                    container.registrationId = registration.getRegistrationId();

                    container.storedRegistrationId = sharedPreferences.getString(STORAGE_PREFIX + REGISTRATION_NAME_STORAGE_KEY + DEFAULT_REGISTRATION_NAME,
                            null);

                    push.unregister(new UnregisterCallback() {

                        @Override
                        public void onUnregister(Exception exception) {
                            if (exception != null) {
                                container.exception = exception;
                            } else {
                                container.unregister = sharedPreferences.getString(STORAGE_PREFIX + REGISTRATION_NAME_STORAGE_KEY + DEFAULT_REGISTRATION_NAME,
                                        null);
                            }

                            latch.countDown();
                        }
                    });
                }
            }
        });

        latch.await();

        // Asserts
        Exception exception = container.exception;

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            Assert.assertEquals(registrationId, container.storedRegistrationId);
            Assert.assertEquals(registrationId, container.registrationId);
            Assert.assertNull(container.unregister);
        }
    }

    public void testRegisterUnregisterTemplate() throws Throwable {

        Context context = getInstrumentation().getTargetContext();
        final SharedPreferences sharedPreferences = PreferenceManager.getDefaultSharedPreferences(context.getApplicationContext());

        final Container container = new Container();
        final String handle = "handle";
        final String templateName = "templateName";

        String registrationId = "registrationId";

        MobileServiceClient client = new MobileServiceClient(appUrl, appKey, context);

        client = client.withFilter(getUpsertTestFilter(registrationId));

        final MobileServicePush push = client.getPush();

        forceRefreshSync(push, handle);

        try {
            TemplateRegistration registration = push.registerTemplate(handle, templateName, "{ }", new String[]{"tag1"}).get();

            container.registrationId = registration.getRegistrationId();

            container.storedRegistrationId = sharedPreferences.getString(STORAGE_PREFIX + REGISTRATION_NAME_STORAGE_KEY + templateName, null);

            push.unregisterTemplate(templateName).get();

            container.unregister = sharedPreferences.getString(STORAGE_PREFIX + REGISTRATION_NAME_STORAGE_KEY + templateName, null);

            Assert.assertEquals(registrationId, container.storedRegistrationId);
            Assert.assertEquals(registrationId, container.registrationId);
            Assert.assertNull(container.unregister);

        } catch (Exception exception) {

            if (exception instanceof ExecutionException) {
                container.exception = (Exception) exception.getCause();
            } else {
                container.exception = exception;
            }

            fail(container.exception.getMessage());
        }
    }

    @SuppressWarnings("deprecation")
    public void testRegisterUnregisterTemplateCallback() throws Throwable {
        final CountDownLatch latch = new CountDownLatch(1);

        Context context = getInstrumentation().getTargetContext();
        final SharedPreferences sharedPreferences = PreferenceManager.getDefaultSharedPreferences(context.getApplicationContext());

        final Container container = new Container();
        final String handle = "handle";
        final String templateName = "templateName";

        String registrationId = "registrationId";

        MobileServiceClient client = new MobileServiceClient(appUrl, appKey, context);

        client = client.withFilter(getUpsertTestFilter(registrationId));

        final MobileServicePush push = client.getPush();

        forceRefreshSync(push, handle);

        push.registerTemplate(handle, templateName, "{ }", new String[]{"tag1"}, new TemplateRegistrationCallback() {

            @Override
            public void onRegister(TemplateRegistration registration, Exception exception) {
                if (exception != null) {
                    container.exception = exception;

                    latch.countDown();
                } else {
                    container.registrationId = registration.getRegistrationId();

                    container.storedRegistrationId = sharedPreferences.getString(STORAGE_PREFIX + REGISTRATION_NAME_STORAGE_KEY + templateName, null);

                    push.unregisterTemplate(templateName, new UnregisterCallback() {

                        @Override
                        public void onUnregister(Exception exception) {
                            if (exception != null) {
                                container.exception = exception;
                            } else {
                                container.unregister = sharedPreferences.getString(STORAGE_PREFIX + REGISTRATION_NAME_STORAGE_KEY + templateName, null);
                            }

                            latch.countDown();
                        }
                    });
                }
            }
        });

        latch.await();

        // Asserts
        Exception exception = container.exception;

        if (exception != null) {
            fail(exception.getMessage());
        } else {
            Assert.assertEquals(registrationId, container.storedRegistrationId);
            Assert.assertEquals(registrationId, container.registrationId);
            Assert.assertNull(container.unregister);
        }
    }

    public void testRegisterFailNative() throws Throwable {

        final Container container = new Container();
        Context context = getInstrumentation().getTargetContext();
        final SharedPreferences sharedPreferences = PreferenceManager.getDefaultSharedPreferences(context.getApplicationContext());

        final String handle = "handle";

        String registrationId = "registrationId";

        MobileServiceClient client = new MobileServiceClient(appUrl, appKey, context);

        client = client.withFilter(getUpsertFailTestFilter(registrationId));

        final MobileServicePush push = client.getPush();

        forceRefreshSync(push, handle);

        try {
            push.register(handle, new String[]{"tag1"}).get();
            fail("Expected Exception RegistrationGoneException");
        } catch (Exception exception) {

            if (exception instanceof ExecutionException) {
                container.exception = (Exception) exception.getCause();
            } else {
                container.exception = exception;
            }

            if (!(container.exception instanceof RegistrationGoneException)) {
                fail("Expected Exception RegistrationGoneException");
            }

            Assert.assertNull(sharedPreferences.getString(STORAGE_PREFIX + REGISTRATION_NAME_STORAGE_KEY + DEFAULT_REGISTRATION_NAME, null));
        }
    }

    public void testRegisterFailTemplate() throws Throwable {

        final Container container = new Container();
        Context context = getInstrumentation().getTargetContext();
        final SharedPreferences sharedPreferences = PreferenceManager.getDefaultSharedPreferences(context.getApplicationContext());

        final String handle = "handle";
        final String templateName = "templateName";

        String registrationId = "registrationId";

        MobileServiceClient client = new MobileServiceClient(appUrl, appKey, context);

        client = client.withFilter(getUpsertFailTestFilter(registrationId));

        final MobileServicePush push = client.getPush();

        forceRefreshSync(push, handle);

        try {
            push.registerTemplate(handle, templateName, "{ }", new String[]{"tag1"}).get();
            fail("Expected Exception RegistrationGoneException");
        } catch (Exception exception) {

            if (exception instanceof ExecutionException) {
                container.exception = (Exception) exception.getCause();
            } else {
                container.exception = exception;
            }

            if (!(container.exception instanceof RegistrationGoneException)) {
                fail("Expected Exception RegistrationGoneException");
            }

            Assert.assertNull(sharedPreferences.getString(STORAGE_PREFIX + REGISTRATION_NAME_STORAGE_KEY + DEFAULT_REGISTRATION_NAME, null));
        }
    }

    public void testReRegisterNative() throws Throwable {
        try {

            Context context = getInstrumentation().getTargetContext();
            final SharedPreferences sharedPreferences = PreferenceManager.getDefaultSharedPreferences(context.getApplicationContext());

            final Container container = new Container();
            final String handle = "handle";

            String registrationId1 = "registrationId1";
            String registrationId2 = "registrationId2";

            String[] tags1 = new String[]{"tag1"};
            final String[] tags2 = new String[]{"tag2"};

            MobileServiceClient client = new MobileServiceClient(appUrl, appKey, context);

            MobileServiceClient registrationclient = client.withFilter(getUpsertTestFilter(registrationId1));
            MobileServiceClient reRegistrationclient = client.withFilter(getUpsertTestFilter(registrationId2));

            final MobileServicePush registrationPush = registrationclient.getPush();
            final MobileServicePush reRegistrationPush = reRegistrationclient.getPush();

            forceRefreshSync(registrationPush, handle);
            forceRefreshSync(reRegistrationPush, handle);

            try {
                registrationPush.register(handle, tags1).get();

                Registration registration2 = reRegistrationPush.register(handle, tags2).get();

                container.storedRegistrationId = sharedPreferences.getString(STORAGE_PREFIX + REGISTRATION_NAME_STORAGE_KEY + DEFAULT_REGISTRATION_NAME, null);
                container.tags = registration2.getTags();

            } catch (Exception exception) {
                container.exception = exception;
            }

            // Asserts
            Exception exception = container.exception;

            if (exception != null) {
                fail(exception.getMessage());
            } else {
                Assert.assertEquals(registrationId2, container.storedRegistrationId);
                Assert.assertTrue(matchTags(tags2, container.tags));
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    public void testReRegisterTemplate() throws Throwable {
        try {

            Context context = getInstrumentation().getTargetContext();
            final SharedPreferences sharedPreferences = PreferenceManager.getDefaultSharedPreferences(context.getApplicationContext());

            final Container container = new Container();
            final String handle = "handle";
            final String templateName = "templateName";

            String registrationId1 = "registrationId1";
            String registrationId2 = "registrationId2";

            String[] tags1 = new String[]{"tag1"};
            final String[] tags2 = new String[]{"tag2"};

            String templateBody1 = "\"data\"={\"text\"=\"$message1\"}";
            final String templateBody2 = "\"data\"={\"text\"=\"$message2\"}";

            MobileServiceClient client = new MobileServiceClient(appUrl, appKey, context);

            MobileServiceClient registrationclient = client.withFilter(getUpsertTestFilter(registrationId1));
            MobileServiceClient reRegistrationclient = client.withFilter(getUpsertTestFilter(registrationId2));

            final MobileServicePush registrationPush = registrationclient.getPush();
            final MobileServicePush reRegistrationPush = reRegistrationclient.getPush();

            forceRefreshSync(registrationPush, handle);
            forceRefreshSync(reRegistrationPush, handle);

            try {
                registrationPush.registerTemplate(handle, templateName, templateBody1, tags1).get();

                TemplateRegistration registration2 = reRegistrationPush.registerTemplate(handle, templateName, templateBody2, tags2).get();

                container.storedRegistrationId = sharedPreferences.getString(STORAGE_PREFIX + REGISTRATION_NAME_STORAGE_KEY + templateName, null);
                container.tags = registration2.getTags();
                container.templateBody = registration2.getTemplateBody();
            } catch (Exception exception) {
                container.exception = exception;
            }

            // Asserts
            Exception exception = container.exception;

            if (exception != null) {
                fail(exception.getMessage());
            } else {
                Assert.assertEquals(registrationId2, container.storedRegistrationId);
                Assert.assertTrue(matchTags(tags2, container.tags));
                Assert.assertEquals(templateBody2, container.templateBody);
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    public void testReRegisterFailNative() throws Throwable {
        try {

            Context context = getInstrumentation().getTargetContext();
            final SharedPreferences sharedPreferences = PreferenceManager.getDefaultSharedPreferences(context.getApplicationContext());

            final Container container = new Container();
            final String handle = "handle";

            String registrationId1 = "registrationId1";
            String registrationId2 = "registrationId2";

            MobileServiceClient client = new MobileServiceClient(appUrl, appKey, context);

            MobileServiceClient registrationclient = client.withFilter(getUpsertTestFilter(registrationId1));
            MobileServiceClient reRegistrationclient = client.withFilter(getUpsertFailTestFilter(registrationId2));

            final MobileServicePush registrationPush = registrationclient.getPush();
            final MobileServicePush reRegistrationPush = reRegistrationclient.getPush();

            forceRefreshSync(registrationPush, handle);
            forceRefreshSync(reRegistrationPush, handle);

            try {
                registrationPush.register(handle, new String[]{"tag1"}).get();
            } catch (Exception exception) {
                fail(exception.getMessage());
            }

            try {
                reRegistrationPush.register(handle, new String[]{"tag1"}).get();

            } catch (Exception exception) {
                if (exception instanceof ExecutionException) {
                    container.exception = (Exception) exception.getCause();
                } else {
                    container.exception = exception;
                }

                container.storedRegistrationId = sharedPreferences.getString(STORAGE_PREFIX + REGISTRATION_NAME_STORAGE_KEY + DEFAULT_REGISTRATION_NAME, null);
            }

            // Asserts
            Exception exception = container.exception;

            if (!(exception instanceof RegistrationGoneException)) {
                fail("Expected Exception RegistrationGoneException");
            }

            Assert.assertNull(container.storedRegistrationId);
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    public void testReRegisterFailTemplate() throws Throwable {
        try {

            Context context = getInstrumentation().getTargetContext();
            final SharedPreferences sharedPreferences = PreferenceManager.getDefaultSharedPreferences(context.getApplicationContext());

            final Container container = new Container();
            final String handle = "handle";
            final String templateName = "templateName";

            String registrationId1 = "registrationId1";
            String registrationId2 = "registrationId2";

            MobileServiceClient client = new MobileServiceClient(appUrl, appKey, context);

            MobileServiceClient registrationclient = client.withFilter(getUpsertTestFilter(registrationId1));
            MobileServiceClient reRegistrationclient = client.withFilter(getUpsertFailTestFilter(registrationId2));

            final MobileServicePush registrationPush = registrationclient.getPush();
            final MobileServicePush reRegistrationPush = reRegistrationclient.getPush();

            forceRefreshSync(registrationPush, handle);
            forceRefreshSync(reRegistrationPush, handle);

            try {
                registrationPush.registerTemplate(handle, templateName, "{ }", new String[]{"tag1"}).get();
            } catch (Exception exception) {
                fail(exception.getMessage());
            }

            try {
                reRegistrationPush.registerTemplate(handle, templateName, "{ }", new String[]{"tag1"}).get();
            } catch (Exception exception) {
                if (exception instanceof ExecutionException) {
                    container.exception = (Exception) exception.getCause();
                } else {
                    container.exception = exception;
                }

                container.storedRegistrationId = sharedPreferences.getString(STORAGE_PREFIX + REGISTRATION_NAME_STORAGE_KEY + templateName, null);
            }

            // Asserts
            Exception exception = container.exception;

            if (!(exception instanceof RegistrationGoneException)) {
                fail("Expected Exception RegistrationGoneException");
            }

            Assert.assertNull(container.storedRegistrationId);
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    public void testRegisterNativeEmptyGcmRegistrationId() throws Throwable {
        try {

            final Container container = new Container();

            Context context = getInstrumentation().getTargetContext();

            MobileServiceClient client = new MobileServiceClient(appUrl, appKey, context);

            try {
                client.getPush().register("", new String[]{"tag1"}).get();
            } catch (Exception exception) {
                if (exception instanceof ExecutionException) {
                    container.exception = (Exception) exception.getCause();
                } else {
                    container.exception = exception;
                }
            }

            // Asserts
            Exception exception = container.exception;

            if (!(exception instanceof IllegalArgumentException)) {
                fail("Expected Exception IllegalArgumentException");
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    public void testRegisterTemplateEmptyGcmRegistrationId() throws Throwable {
        try {
            final Container container = new Container();

            Context context = getInstrumentation().getTargetContext();

            MobileServiceClient client = new MobileServiceClient(appUrl, appKey, context);

            try {
                client.getPush().registerTemplate("", "template1", "{\"data\"={\"text\"=\"$message\"}}", new String[]{"tag1"}).get();
            } catch (Exception exception) {
                if (exception instanceof ExecutionException) {
                    container.exception = (Exception) exception.getCause();
                } else {
                    container.exception = exception;
                }
            }

            // Asserts
            Exception exception = container.exception;

            if (!(exception instanceof IllegalArgumentException)) {
                fail("Expected Exception IllegalArgumentException");
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    // Test Filter

    public void testRegisterTemplateEmptyTemplateName() throws Throwable {
        try {

            final Container container = new Container();

            Context context = getInstrumentation().getTargetContext();

            MobileServiceClient client = new MobileServiceClient(appUrl, appKey, context);

            try {
                client.getPush().registerTemplate(UUID.randomUUID().toString(), "", "{\"data\"={\"text\"=\"$message\"}}", new String[]{"tag1"}).get();
            } catch (Exception exception) {
                if (exception instanceof ExecutionException) {
                    container.exception = (Exception) exception.getCause();
                } else {
                    container.exception = exception;
                }
            }

            // Asserts
            Exception exception = container.exception;

            if (!(exception instanceof IllegalArgumentException)) {
                fail("Expected Exception IllegalArgumentException");
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    public void testRegisterTemplateEmptyTemplateBody() throws Throwable {
        try {

            final Container container = new Container();

            Context context = getInstrumentation().getTargetContext();

            MobileServiceClient client = new MobileServiceClient(appUrl, appKey, context);

            try {
                client.getPush().registerTemplate(UUID.randomUUID().toString(), "template1", "", new String[]{"tag1"}).get();
            } catch (Exception exception) {
                if (exception instanceof ExecutionException) {
                    container.exception = (Exception) exception.getCause();
                } else {
                    container.exception = exception;
                }
            }

            // Asserts
            Exception exception = container.exception;

            if (!(exception instanceof IllegalArgumentException)) {
                fail("Expected Exception IllegalArgumentException");
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    private class Container {
        public String storedRegistrationId;
        public String registrationId;

        public List<String> tags;
        public String templateBody;

        public String unregister;

        public Exception exception;
    }
}
