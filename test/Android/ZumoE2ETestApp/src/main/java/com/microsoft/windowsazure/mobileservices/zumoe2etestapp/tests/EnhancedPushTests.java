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
package com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests;

import android.annotation.TargetApi;
import android.os.AsyncTask;
import android.os.Build;
import android.util.Pair;

import com.google.android.gcm.GCMRegistrar;
import com.google.common.util.concurrent.FutureCallback;
import com.google.common.util.concurrent.Futures;
import com.google.common.util.concurrent.ListenableFuture;
import com.google.common.util.concurrent.SettableFuture;
import com.google.gson.Gson;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.notifications.MobileServicePush;
import com.microsoft.windowsazure.mobileservices.notifications.RegistrationCallback;
import com.microsoft.windowsazure.mobileservices.notifications.UnregisterCallback;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.MainActivity;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestCase;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestExecutionCallback;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestGroup;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestResult;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestStatus;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.push.GCMMessageHelper;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.push.GCMMessageManager;

import java.util.ArrayList;
import java.util.List;
import java.util.UUID;
import java.util.concurrent.ExecutionException;

public class EnhancedPushTests extends TestGroup {

    private boolean isNetBackend = true;

    /*
     * Pointer to the main activity used to register with GCM
     */
    public static MainActivity mainActivity;
    public static String registrationId;

    public EnhancedPushTests(boolean isNetBackend) {
        super("Enhanced Push tests");

        this.isNetBackend = isNetBackend;

        // Notification Roundtrip Tests
        this.addTest(createGCMRegisterTest());

        String json = "'Notification Hub test notification'".replace('\'', '\"');

        json = "'Notification Hub test notification'".replace('\'', '\"');
        this.addTest(createNativePushTest("Native Notification Roundtrip - Simple payload", json));

        json = "{'name':'John Doe','age':'33'}".replace('\'', '\"');
        this.addTest(createNativePushTest("Native Notification Roundtrip - Complex payload", json));

        String templateNotification = "{'fullName':'John Doe'}".replace('\'', '\"');
        String template = "{'data':{'user':'$(fullName)'}}".replace('\'', '\"');
        String expectedPayload = "{'user':'John Doe'}".replace('\'', '\"');

        this.addTest(createTemplatePushTest("Template Notification Roundtrip", templateNotification, "templateGCM", template, expectedPayload));

        this.addTest(createUnregisterTestCase("Unregister"));

        // With Callbacks
        this.addTest(RegisterNativeWithCallbackTestCase("Register Native With Callback"));
        this.addTest(RegisterTemplateWithCallbackTestCase("Register Template With Callback", "template1"));
        this.addTest(UnRegisterWithCallbackTestCase("UnRegister With Callback"));

        this.addTest(createGCMUnregisterTest());
    }

    private Void register(TestCase test, MobileServicePush hub, String gcmId) throws InterruptedException, ExecutionException {
        test.log("Register Native with GCMID = " + gcmId);

        return hub.register(gcmId).get();
    }

    private void unregister(TestCase test, MobileServicePush hub) throws InterruptedException, ExecutionException {
        test.log("Unregister Native");

        hub.unregister().get();
    }

    private Void registerTemplate(TestCase test, MobileServicePush hub, String gcmId, String templateName)
            throws InterruptedException, ExecutionException {
        String template = "{\"time_to_live\": 108, \"delay_while_idle\": true, \"data\": { \"message\": \"$(msg)\" } }";

        return registerTemplate(test, hub, gcmId, templateName, template);
    }

    private Void registerTemplate(TestCase test, MobileServicePush hub, String gcmId, String templateName, String template)
            throws InterruptedException, ExecutionException {

        test.log("Register with GCMID = " + gcmId);
        test.log("Register with templateName = " + templateName);

        return hub.registerTemplate(gcmId, templateName, template).get();
    }

    private void unregisterAll(TestCase test, MobileServicePush hub, MobileServiceClient client, String registrationId) throws InterruptedException, ExecutionException {
        test.log("Unregister Native");

        hub.unregister().get();

        deleteRegistrationsForChannel(client, registrationId).get();
    }

    private TestCase createUnregisterTestCase(String name) {
        TestCase register = new TestCase() {

            @Override
            protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {

                try {
                    final MobileServicePush MobileServicePush = client.getPush();
                    final TestResult result = new TestResult();
                    result.setStatus(TestStatus.Passed);
                    result.setTestCase(this);

                    unregister(this, MobileServicePush);

                    callback.onTestComplete(this, result);
                    return;

                } catch (Exception e) {
                    callback.onTestComplete(this, createResultFromException(e));
                    return;
                }
            }
        };

        register.setName(name);

        return register;
    }

    // Notification Roundtrip Tests

    private TestCase createNativePushTest(String testName, String jsonPayload) {
        final JsonElement orginalPayload = new JsonParser().parse(jsonPayload);

        JsonObject newPayload;
        if (orginalPayload.isJsonObject()) {
            newPayload = orginalPayload.getAsJsonObject();
        } else {
            newPayload = new JsonObject();
            newPayload.add("message", orginalPayload);
        }

        final JsonObject payload = newPayload;

        TestCase test = new TestCase(testName) {

            @Override
            protected void executeTest(final MobileServiceClient client, final TestExecutionCallback callback) {

                try {

                    final TestResult result = new TestResult();
                    result.setStatus(TestStatus.Passed);
                    result.setTestCase(this);

                    final MobileServicePush MobileServicePush = client.getPush();

                    unregisterAll(this, MobileServicePush, client, registrationId);

                    JsonElement unregisterResult = verifyUnregisterInstallationResult(client).get();

                    if (!unregisterResult.getAsBoolean()) {
                        this.log("Unregister failed");
                        result.setStatus(TestStatus.Failed);

                        callback.onTestComplete(this, result);
                        return;
                    }

                    register(this, MobileServicePush, registrationId);

                    JsonElement registerResult = verifyRegisterInstallationResult(client, registrationId).get();

                    if (!registerResult.getAsBoolean()) {
                        this.log("Register failed");
                        result.setStatus(TestStatus.Failed);

                        callback.onTestComplete(this, result);
                        return;
                    }

                    JsonObject item = new JsonObject();
                    item.addProperty("method", "send");
                    item.addProperty("token", "dummy");
                    item.addProperty("type", "gcm");

                    JsonObject sentPayload = new JsonObject();
                    sentPayload.add("data", payload);
                    item.add("payload", sentPayload);

                    JsonElement jsonObject = client.invokeApi("Push", item).get();

                    this.log("OnCompleted: " + jsonObject.toString());
                    TestExecutionCallback nativeUnregisterTestExecutionCallback = getNativeUnregisterTestExecutionCallback(client, payload, callback);
                    GCMMessageManager.instance.waitForPushMessage(60000, GCMMessageHelper.getPushCallback(this, payload, nativeUnregisterTestExecutionCallback));

                } catch (Exception e) {
                    callback.onTestComplete(this, createResultFromException(e));
                    return;
                }
            }
        };

        test.setName(testName);

        return test;
    }

    private TestCase createTemplatePushTest(String testName, final String templateNotification, final String templateName,
                                            final String template, final String expectedPayload) {

        TestCase test = new TestCase(testName) {

            @Override
            protected void executeTest(final MobileServiceClient client, final TestExecutionCallback callback) {

                try {

                    final TestResult result = new TestResult();
                    result.setStatus(TestStatus.Passed);
                    result.setTestCase(this);

                    final MobileServicePush MobileServicePush = client.getPush();

                    unregisterAll(this, MobileServicePush, client, registrationId);

                    JsonElement unregisterResult = verifyUnregisterInstallationResult(client).get();

                    if (!unregisterResult.getAsBoolean()) {
                        this.log("Unregister failed");
                        result.setStatus(TestStatus.Failed);

                        callback.onTestComplete(this, result);
                        return;
                    }

                    registerTemplate(this, MobileServicePush, registrationId, templateName, template);

                    JsonElement registerResult = verifyRegisterInstallationResult(client, registrationId, templateName, template).get();

                    if (!registerResult.getAsBoolean()) {
                        this.log("Register failed");
                        result.setStatus(TestStatus.Failed);

                        callback.onTestComplete(this, result);
                        return;
                    }

                    GCMMessageManager.instance.clearPushMessages();

                    JsonElement templateNotificationJsonElement = new JsonParser().parse(templateNotification);

                    JsonObject item = new JsonObject();
                    item.addProperty("method", "send");
                    item.addProperty("token", "dummy");
                    item.addProperty("type", "template");
                    item.add("payload", templateNotificationJsonElement);

                    JsonElement jsonObject = client.invokeApi("Push", item).get();

                    log("OnCompleted: " + jsonObject.toString());
                    TestExecutionCallback nativeUnregisterTestExecutionCallback = getTemplateUnregisterTestExecutionCallback(client, templateName,
                            template, templateNotification, callback);
                    GCMMessageManager.instance.waitForPushMessage(60000,
                            GCMMessageHelper.getPushCallback(this, expectedPayload, nativeUnregisterTestExecutionCallback));

                } catch (Exception e) {
                    callback.onTestComplete(this, createResultFromException(e));
                    return;
                }
            }
        };

        test.setName(testName);

        return test;
    }

    private TestCase createGCMUnregisterTest() {
        TestCase testCase = new TestCase("Unregister from GCM") {

            @Override
            protected void executeTest(MobileServiceClient client, TestExecutionCallback callback) {
                GCMRegistrar.unregister(mainActivity);
                log("Unregistered from GCM");
                TestResult testResult = new TestResult();
                testResult.setStatus(TestStatus.Passed);
                testResult.setTestCase(this);
                callback.onTestComplete(this, testResult);
            }
        };

        return testCase;
    }

    private TestCase createGCMRegisterTest() {
        TestCase testCase = new TestCase("Register app with GCM") {

            @Override
            protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {

                try {

                    GCMRegistrar.checkDevice(mainActivity);
                    GCMRegistrar.checkManifest(mainActivity);
                    String registrationId = GCMRegistrar.getRegistrationId(mainActivity);
                    EnhancedPushTests.registrationId = registrationId;

                    log("Registration ID: " + EnhancedPushTests.registrationId);

                    if ("".equals(registrationId)) {
                        GCMRegistrar.register(mainActivity, mainActivity.getGCMSenderId());
                        log("Called GCMRegistrar.register");
                        GCMMessageManager.instance.waitForRegistrationMessage(20000,
                                GCMMessageHelper.getRegistrationCallBack(this, callback, EnhancedPushTests.class));
                    } else {
                        TestResult testResult = new TestResult();
                        testResult.setTestCase(this);
                        testResult.setStatus(TestStatus.Passed);
                        callback.onTestComplete(this, testResult);
                    }
                } catch (Exception e) {
                    TestResult testResult = new TestResult();
                    testResult.setTestCase(this);
                    testResult.setStatus(TestStatus.Failed);
                    callback.onTestComplete(this, testResult);
                }
            }
        };

        return testCase;
    }

    private TestExecutionCallback getNativeUnregisterTestExecutionCallback(final MobileServiceClient client, final JsonObject payload,
                                                                           final TestExecutionCallback callback) {
        return new TestExecutionCallback() {

            @Override
            public void onTestStart(TestCase test) {
                return;
            }

            @Override
            public void onTestGroupComplete(TestGroup group, List<TestResult> results) {
                return;
            }

            @Override
            public void onTestComplete(TestCase test, TestResult result) {
                if (result.getStatus() == TestStatus.Failed) {
                    callback.onTestComplete(test, result);
                } else {
                    nativeUnregisterTestExecution(client, test, payload, callback);
                }
            }
        };
    }

    private void nativeUnregisterTestExecution(final MobileServiceClient client, final TestCase test, final JsonObject payload,
                                               final TestExecutionCallback callback) {

        new AsyncTask<Void, Void, Void>() {
            @Override
            protected Void doInBackground(Void... arg0) {

                try {

                    unregisterAll(test, client.getPush(), client, registrationId);

                    //SystemClock.sleep(60000);

                    JsonElement unregisterResult = verifyUnregisterInstallationResult(client).get();

                    GCMMessageManager.instance.clearPushMessages();

                    JsonObject item = new JsonObject();
                    item.addProperty("method", "send");
                    item.addProperty("token", "dummy");
                    item.addProperty("type", "gcm");

                    JsonObject sentPayload = new JsonObject();
                    sentPayload.add("data", payload);
                    item.add("payload", sentPayload);
                    item.addProperty("usingNH", true);

                    JsonElement pushResult = client.invokeApi("Push", item).get();

                    test.log("OnCompleted: " + pushResult.toString());
                    GCMMessageManager.instance.waitForPushMessage(60000, GCMMessageHelper.getNegativePushCallback(test, callback));
                } catch (Exception exception) {
                    callback.onTestComplete(test, test.createResultFromException(exception));
                    // return;
                }
                return null;
            }
        }.executeOnExecutor(AsyncTask.THREAD_POOL_EXECUTOR);

    }

    private TestExecutionCallback getTemplateUnregisterTestExecutionCallback(final MobileServiceClient client, final String templateName,
                                                                             final String template, final String templateNotification, final TestExecutionCallback callback) {
        return new TestExecutionCallback() {

            @Override
            public void onTestStart(TestCase test) {
                return;
            }

            @Override
            public void onTestGroupComplete(TestGroup group, List<TestResult> results) {
                return;
            }

            @Override
            public void onTestComplete(TestCase test, TestResult result) {
                if (result.getStatus() == TestStatus.Failed) {
                    callback.onTestComplete(test, result);
                } else {
                    templateUnregisterTestExecution(client, test, templateName, template, templateNotification, callback);
                }
            }
        };
    }

    @TargetApi(Build.VERSION_CODES.HONEYCOMB)
    private void templateUnregisterTestExecution(final MobileServiceClient client, final TestCase test, final String templateName,
                                                 final String template, final String templateNotification, final TestExecutionCallback callback) {

        new AsyncTask<Void, Void, Void>() {
            @Override
            protected Void doInBackground(Void... arg0) {
                try {

                    registerTemplate(test, client.getPush(), templateName, template);

                    unregister(test, client.getPush());

                    JsonElement templateNotificationJsonElement = new JsonParser().parse(templateNotification);

                    GCMMessageManager.instance.clearPushMessages();
                    JsonObject item = new JsonObject();
                    item.addProperty("method", "send");
                    item.addProperty("token", "dummy");
                    item.addProperty("type", "template");
                    item.add("payload", templateNotificationJsonElement);

                    JsonElement jsonObject = client.invokeApi("Push", item).get();

                    test.log("OnCompleted: " + jsonObject.toString());
                    GCMMessageManager.instance.waitForPushMessage(60000, GCMMessageHelper.getNegativePushCallback(test, callback));
                } catch (Exception exception) {
                    callback.onTestComplete(test, test.createResultFromException(exception));
                    // return;
                }
                return null;
            }
        }.executeOnExecutor(AsyncTask.THREAD_POOL_EXECUTOR);
    }

    // Test Callbacks
    @SuppressWarnings("deprecation")
    private void register(TestCase test, MobileServicePush hub, String gcmId, final RegistrationCallback callback) {
        test.log("Register Native with GCMID = " + gcmId);

        hub.register(gcmId, callback);
    }

    private void registerTemplate(TestCase test, MobileServicePush hub, String gcmId, String templateName, RegistrationCallback callback) {
        String template = "{\"time_to_live\": 108, \"delay_while_idle\": true, \"data\": { \"message\": \"$(msg)\" } }";
        registerTemplate(test, hub, gcmId, templateName, template, callback);
    }

    @SuppressWarnings("deprecation")
    private void registerTemplate(TestCase test, MobileServicePush hub, String gcmId, String templateName, String template,
                                  RegistrationCallback callback) {

        test.log("Register with GCMID = " + gcmId);
        test.log("Register with templateName = " + templateName);

        hub.registerTemplate(gcmId, templateName, template, callback);
    }

    @SuppressWarnings("deprecation")
    private void unregister(TestCase test, MobileServicePush hub, UnregisterCallback callback) {
        test.log("Unregister Native");
        hub.unregister(callback);
    }

    private TestCase RegisterNativeWithCallbackTestCase(String name) {
        TestCase register = new TestCase() {

            @Override
            protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {
                final TestCase that = this;

                try {
                    final MobileServicePush MobileServicePush = client.getPush();
                    final TestResult result = new TestResult();
                    result.setStatus(TestStatus.Passed);
                    result.setTestCase(this);

                    final String gcmId = UUID.randomUUID().toString();

                    register(this, MobileServicePush, gcmId, new RegistrationCallback() {

                        @Override
                        public void onRegister(Exception exception) {

                            if (exception != null) {
                                callback.onTestComplete(that, createResultFromException(exception));
                                return;
                            }

                            callback.onTestComplete(that, result);

                            return;
                        }
                    });
                } catch (Exception e) {
                    callback.onTestComplete(this, createResultFromException(e));
                    return;
                }
            }
        };

        register.setName(name);

        return register;
    }

    private TestCase RegisterTemplateWithCallbackTestCase(String name, final String templateName) {
        TestCase register = new TestCase() {

            @Override
            protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {
                final TestCase that = this;

                try {
                    final MobileServicePush MobileServicePush = client.getPush();
                    final TestResult result = new TestResult();
                    result.setStatus(TestStatus.Passed);
                    result.setTestCase(this);

                    final String gcmId = UUID.randomUUID().toString();

                    registerTemplate(this, MobileServicePush, gcmId, templateName, new RegistrationCallback() {

                        @Override
                        public void onRegister(Exception exception) {
                            if (exception != null) {
                                callback.onTestComplete(that, createResultFromException(exception));
                                return;
                            }

                            callback.onTestComplete(that, result);
                            return;
                        }
                    });
                } catch (Exception e) {
                    callback.onTestComplete(this, createResultFromException(e));
                    return;
                }
            }
        };

        register.setName(name);

        return register;
    }

    private TestCase UnRegisterWithCallbackTestCase(String name) {
        TestCase register = new TestCase() {

            @Override
            protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {

                final TestCase that = this;

                try {
                    final MobileServicePush MobileServicePush = client.getPush();
                    final TestResult result = new TestResult();
                    result.setStatus(TestStatus.Passed);
                    result.setTestCase(this);

                    unregister(this, MobileServicePush, new UnregisterCallback() {

                        @Override
                        public void onUnregister(Exception exception) {
                            if (exception != null) {
                                callback.onTestComplete(that, createResultFromException(exception));
                                return;
                            }
                            callback.onTestComplete(that, result);
                            return;
                        }
                    });
                } catch (Exception e) {
                    callback.onTestComplete(this, createResultFromException(e));
                    return;
                }
            }
        };

        register.setName(name);

        return register;
    }

    public ListenableFuture<Void> deleteRegistrationsForChannel(final MobileServiceClient client, String registrationId) {

        final SettableFuture<Void> resultFuture = SettableFuture.create();

        ArrayList<Pair<String, String>> parameters = new ArrayList<Pair<String, String>>();

        parameters.add(new Pair<String, String>("channelUri", registrationId));

        ListenableFuture<JsonElement> serviceFilterFuture = client.invokeApi("deleteRegistrationsForChannel", "DELETE", parameters);

        Futures.addCallback(serviceFilterFuture, new FutureCallback<JsonElement>() {
            @Override
            public void onFailure(Throwable exception) {
                resultFuture.setException(exception);
            }

            @Override
            public void onSuccess(JsonElement response) {
                resultFuture.set(null);
            }
        });

        return resultFuture;
    }

    public ListenableFuture<JsonElement> verifyUnregisterInstallationResult(final MobileServiceClient client) {

        final SettableFuture<JsonElement> resultFuture = SettableFuture.create();

        ListenableFuture<JsonElement> serviceFilterFuture = client.invokeApi("verifyUnregisterInstallationResult", "GET", new ArrayList<Pair<String, String>>());

        Futures.addCallback(serviceFilterFuture, new FutureCallback<JsonElement>() {
            @Override
            public void onFailure(Throwable exception) {
                resultFuture.setException(exception);
            }

            @Override
            public void onSuccess(JsonElement response) {
                resultFuture.set(response);
            }
        });

        return resultFuture;
    }

    public ListenableFuture<JsonElement> verifyRegisterInstallationResult(MobileServiceClient client, String registrationId) {
        return verifyRegisterInstallationResult(client, registrationId, null, null);
    }

    public ListenableFuture<JsonElement> verifyRegisterInstallationResult(MobileServiceClient client, String registrationId, String templateName, String templateBody) {

        final SettableFuture<JsonElement> resultFuture = SettableFuture.create();

        ArrayList<Pair<String, String>> parameters = new ArrayList<Pair<String, String>>();

        parameters.add(new Pair<>("channelUri", registrationId));

        if (templateName != null && templateBody != null) {
            JsonObject templateObject = GetTemplateObject(templateName, templateBody);
            parameters.add(new Pair<>("templates", templateObject.toString()));
        }

        ListenableFuture<JsonElement> serviceFilterFuture = client.invokeApi("verifyRegisterInstallationResult", "GET", parameters);

        Futures.addCallback(serviceFilterFuture, new FutureCallback<JsonElement>() {
            @Override
            public void onFailure(Throwable exception) {
                resultFuture.setException(exception);
            }

            @Override
            public void onSuccess(JsonElement response) {
                resultFuture.set(response);
            }
        });

        return resultFuture;
    }

    private JsonObject GetTemplateObject(String templateName, String templateBody) {
        JsonObject templateDetailObject = new JsonObject();
        templateDetailObject.addProperty("body", templateBody);

        JsonObject templateObject = new JsonObject();
        templateObject.add(templateName, templateDetailObject);

        return templateObject;
    }
}