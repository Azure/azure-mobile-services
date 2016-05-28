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
import android.content.SharedPreferences;
import android.content.SharedPreferences.Editor;
import android.os.AsyncTask;
import android.os.Build;
import android.preference.PreferenceManager;

import com.google.android.gcm.GCMRegistrar;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceException;
import com.microsoft.windowsazure.mobileservices.notifications.MobileServicePush;
import com.microsoft.windowsazure.mobileservices.notifications.Registration;
import com.microsoft.windowsazure.mobileservices.notifications.RegistrationCallback;
import com.microsoft.windowsazure.mobileservices.notifications.TemplateRegistration;
import com.microsoft.windowsazure.mobileservices.notifications.TemplateRegistrationCallback;
import com.microsoft.windowsazure.mobileservices.notifications.UnregisterCallback;
import com.microsoft.windowsazure.mobileservices.table.MobileServiceJsonTable;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.MainActivity;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestCase;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestExecutionCallback;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestGroup;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestResult;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestStatus;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.push.GCMMessageHelper;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.push.GCMMessageManager;

import java.util.List;
import java.util.Set;
import java.util.UUID;
import java.util.concurrent.ExecutionException;

public class EnhancedPushTests extends TestGroup {

    private static final String tableName = "droidPushTest";
    private static final String DEFAULT_REGISTRATION_NAME = "$Default";
    private static final String REGISTRATION_NAME_STORAGE_KEY = "__NH_REG_NAME_";
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
        this.addTest(createNativePushTestWithRefresh("Native Notification Roundtrip - Simple payload - With Refresh", "tag1", json));

        json = "'Notification Hub test notification'".replace('\'', '\"');
        this.addTest(createNativePushTest("Native Notification Roundtrip - Simple payload", "tag1", json));

        json = "{'name':'John Doe','age':'33'}".replace('\'', '\"');
        this.addTest(createNativePushTest("Native Notification Roundtrip - Complex payload", "tag2", json));

        this.addTest(createNativePushTest("Native Notification Roundtrip - No Tag", null, json));

        String templateNotification = "{'fullName':'John Doe'}".replace('\'', '\"');
        String template = "{'data':{'user':'$(fullName)'}}".replace('\'', '\"');
        String expectedPayload = "{'user':'John Doe'}".replace('\'', '\"');

        this.addTest(createTemplatePushTest("Template Notification Roundtrip - Tag", "tag4", templateNotification, "templateTag", template, expectedPayload));

        this.addTest(createTemplatePushTest("Template Notification Roundtrip - No Tag", null, templateNotification, "templateNoTag", template, expectedPayload));

        this.addTest(createReRegisterNativeTestCase("Register native - Register / Unregister / Register / Unregister"));

        this.addTest(createReRegisterTemplateTestCase("Register template - Register / Unregister / Register / Unregister", UUID.randomUUID().toString()));

        this.addTest(createUnregisterNativeNonExistingTestCase("Unregister native - Non existing"));
        this.addTest(createUnregisterNativeUnexistingRegistrationTestCase("Unregister native - Unexisting registration"));

        this.addTest(createUnregisterTemplateNonExistingTestCase("Unregister template - Non existing", UUID.randomUUID().toString()));
        this.addTest(createUnregisterTemplateUnexistingRegistrationTestCase("Unregister template - Unexisting registration", UUID.randomUUID().toString()));

        this.addTest(createUnregisterAllUnregisterNativeTestCase(
                "Unregister all - Register native / Register template / Unregister all / Unregister native - Unexisting registration", UUID.randomUUID()
                        .toString()));
        this.addTest(createUnregisterAllUnregisterTemplateTestCase(
                "Unregister all - Register native / Register template / Unregister all / Unregister template - Unexisting registration", UUID.randomUUID()
                        .toString()));

        this.addTest(createCheckIsRefreshNeeded("Retrieve existing registrations on first connection"));

        // With Callbacks
        this.addTest(RegisterNativeWithCallbackTestCase("Register Native With Callback"));
        this.addTest(RegisterTemplateWithCallbackTestCase("Register Template With Callback", "template1"));
        this.addTest(UnRegisterNativeWithCallbackTestCase("UnRegister Native With Callback"));
        this.addTest(UnRegisterTemplateWithCallbackTestCase("UnRegister Template With Callback", "template1"));
        this.addTest(UnRegisterAllNativeWithCallbackTestCase("UnRegister All Native With Callback"));
        this.addTest(UnRegisterAllTemplateWithCallbackTestCase("UnRegister All Template With Callback", "template1"));

        this.addTest(createGCMUnregisterTest());
    }

    public static void clearNotificationHubStorageData() {
        SharedPreferences sharedPreferences = PreferenceManager.getDefaultSharedPreferences(MainActivity.getInstance());
        Editor editor = sharedPreferences.edit();
        Set<String> keys = sharedPreferences.getAll().keySet();

        for (String key : keys) {
            if (key.startsWith("__NH_")) {
                editor.remove(key);
            }
        }

        editor.commit();
    }

    public static void clearRegistrationsStorageData() {
        SharedPreferences sharedPreferences = PreferenceManager.getDefaultSharedPreferences(MainActivity.getInstance());
        Editor editor = sharedPreferences.edit();
        Set<String> keys = sharedPreferences.getAll().keySet();

        for (String key : keys) {
            if (key.startsWith("REGISTRATION_NAME_STORAGE_KEY")) {
                editor.remove(key);
            }
        }

        editor.commit();
    }

    private Registration register(TestCase test, MobileServicePush hub, String gcmId, String[] tags) throws InterruptedException, ExecutionException {
        test.log("Register Native with GCMID = " + gcmId);
        if (tags != null && tags.length > 0) {
            for (String tag : tags) {
                test.log("Using tag: " + tag);
            }
        }

        return hub.register(gcmId, tags).get();
    }

    private void unregister(TestCase test, MobileServicePush hub) throws InterruptedException, ExecutionException {
        test.log("Unregister Native");

        hub.unregister().get();
    }

    private TemplateRegistration registerTemplate(TestCase test, MobileServicePush hub, String gcmId, String templateName, String[] tags)
            throws InterruptedException, ExecutionException {
        String template = "{\"time_to_live\": 108, \"delay_while_idle\": true, \"data\": { \"message\": \"$(msg)\" } }";

        return registerTemplate(test, hub, gcmId, templateName, template, tags);
    }

    private TemplateRegistration registerTemplate(TestCase test, MobileServicePush hub, String gcmId, String templateName, String template, String[] tags)
            throws InterruptedException, ExecutionException {

        test.log("Register with GCMID = " + gcmId);
        test.log("Register with templateName = " + templateName);

        if (tags != null && tags.length > 0) {
            for (String tag : tags) {
                test.log("Using tag: " + tag);
            }
        }

        return hub.registerTemplate(gcmId, templateName, template, tags).get();
    }

    private void unregisterTemplate(TestCase test, MobileServicePush hub, String templateName) throws InterruptedException, ExecutionException {
        test.log("UnregisterTemplate with templateName = " + templateName);

        hub.unregisterTemplate(templateName).get();
    }

    private void unregisterAll(TestCase test, MobileServicePush hub, String gcmId) throws InterruptedException, ExecutionException {
        test.log("Unregister Native");

        hub.unregisterAll(gcmId).get();
    }

    private void addUnexistingNativeRegistration(String registrationId) {
        addUnexistingTemplateRegistration(DEFAULT_REGISTRATION_NAME, registrationId);
    }

    private void addUnexistingTemplateRegistration(String templateName, String registrationId) {
        SharedPreferences sharedPreferences = PreferenceManager.getDefaultSharedPreferences(MainActivity.getInstance());

        Editor editor = sharedPreferences.edit();
        editor.putString(REGISTRATION_NAME_STORAGE_KEY + templateName, registrationId);
        editor.commit();
    }

    // Register Native Tests

    private int getRegistrationCountInLocalStorage() {
        SharedPreferences preferences = PreferenceManager.getDefaultSharedPreferences(MainActivity.getInstance());
        int regCount = 0;

        for (String key : preferences.getAll().keySet()) {
            if (key.startsWith("__NH_REG_NAME_")) {
                regCount++;
            }
        }

        return regCount;
    }

    // Register Template Tests

    private TestCase createReRegisterNativeTestCase(String name) {
        TestCase register = new TestCase() {

            @Override
            protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {

                try {

                    final MobileServicePush MobileServicePush = client.getPush();
                    final TestResult result = new TestResult();
                    result.setStatus(TestStatus.Passed);
                    result.setTestCase(this);

                    final String gcmId = UUID.randomUUID().toString();

                    Registration reg1 = register(this, MobileServicePush, gcmId, (String[]) null);

                    unregister(this, MobileServicePush);

                    Registration reg2 = register(this, MobileServicePush, gcmId, (String[]) null);

                    if (reg2.getRegistrationId().equals(reg1.getRegistrationId())) {
                        result.setStatus(TestStatus.Failed);
                    }

                    unregister(this, MobileServicePush);

                    callback.onTestComplete(this, result);

                } catch (Exception e) {
                    callback.onTestComplete(this, createResultFromException(e));
                    return;
                }
            }
        };

        register.setName(name);

        return register;
    }

    // Unregister Native Tests

    private TestCase createReRegisterTemplateTestCase(String name, final String templateName) {
        TestCase register = new TestCase() {

            @Override
            protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {

                try {
                    final MobileServicePush MobileServicePush = client.getPush();
                    final TestResult result = new TestResult();
                    result.setStatus(TestStatus.Passed);
                    result.setTestCase(this);

                    final String gcmId = UUID.randomUUID().toString();

                    TemplateRegistration reg1 = registerTemplate(this, MobileServicePush, gcmId, templateName, (String[]) null);

                    unregisterTemplate(this, MobileServicePush, templateName);

                    TemplateRegistration reg2 = registerTemplate(this, MobileServicePush, gcmId, templateName + "1", (String[]) null);

                    if (reg2.getRegistrationId().equals(reg1.getRegistrationId())) {
                        result.setStatus(TestStatus.Failed);
                    }

                    unregisterTemplate(this, MobileServicePush, templateName);

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

    private TestCase createUnregisterNativeNonExistingTestCase(String name) {
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

    // Unregister Template Tests

    private TestCase createUnregisterNativeUnexistingRegistrationTestCase(String name) {
        TestCase register = new TestCase() {

            @Override
            protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {

                try {
                    final MobileServicePush MobileServicePush = client.getPush();
                    final TestResult result = new TestResult();
                    result.setStatus(TestStatus.Passed);
                    result.setTestCase(this);

                    String gcmId = UUID.randomUUID().toString();

                    Registration registration = register(this, MobileServicePush, gcmId, (String[]) null);

                    final String registrationId = registration.getRegistrationId();

                    unregister(this, MobileServicePush);

                    addUnexistingNativeRegistration(registrationId);

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

        if (!isNetBackend) {
            register.setExpectedExceptionClass(MobileServiceException.class);
        }

        return register;
    }

    private TestCase createUnregisterTemplateNonExistingTestCase(String name, final String templateName) {
        TestCase register = new TestCase() {

            @Override
            protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {
                try {
                    final MobileServicePush MobileServicePush = client.getPush();
                    final TestResult result = new TestResult();
                    result.setStatus(TestStatus.Passed);
                    result.setTestCase(this);

                    unregisterTemplate(this, MobileServicePush, templateName);

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

    // Unregister All Tests

    private TestCase createUnregisterTemplateUnexistingRegistrationTestCase(String name, final String templateName) {
        TestCase register = new TestCase() {

            @Override
            protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {
                final TestCase that = this;

                try {
                    final MobileServicePush MobileServicePush = client.getPush();
                    final TestResult result = new TestResult();
                    result.setStatus(TestStatus.Passed);
                    result.setTestCase(this);

                    TemplateRegistration templateRegistration = registerTemplate(this, MobileServicePush, UUID.randomUUID().toString(), templateName,
                            (String[]) null);

                    final String registrationId = templateRegistration.getRegistrationId();

                    unregisterTemplate(that, MobileServicePush, templateName);

                    addUnexistingTemplateRegistration(templateName, registrationId);

                    unregisterTemplate(that, MobileServicePush, templateName);

                    callback.onTestComplete(this, result);
                    return;

                } catch (Exception e) {
                    callback.onTestComplete(this, createResultFromException(e));
                    return;
                }
            }
        };

        register.setName(name);

        if (!isNetBackend) {
            register.setExpectedExceptionClass(MobileServiceException.class);
        }

        return register;
    }

    private TestCase createUnregisterAllUnregisterNativeTestCase(String name, final String templateName) {
        TestCase register = new TestCase() {

            @Override
            protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {

                try {
                    final MobileServicePush MobileServicePush = client.getPush();
                    final TestResult result = new TestResult();
                    result.setStatus(TestStatus.Passed);
                    result.setTestCase(this);

                    final String gcmId = UUID.randomUUID().toString();

                    unregisterAll(this, MobileServicePush, gcmId);

                    Registration nativeRegistration = register(this, MobileServicePush, gcmId, (String[]) null);

                    final String registrationId = nativeRegistration.getRegistrationId();

                    registerTemplate(this, MobileServicePush, gcmId, templateName, (String[]) null);

                    unregisterAll(this, MobileServicePush, gcmId);

                    addUnexistingNativeRegistration(registrationId);

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

        if (!isNetBackend) {
            register.setExpectedExceptionClass(MobileServiceException.class);
        }

        return register;
    }

    private TestCase createUnregisterAllUnregisterTemplateTestCase(String name, final String templateName) {
        TestCase register = new TestCase() {

            @Override
            protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {

                try {
                    final MobileServicePush MobileServicePush = client.getPush();
                    final TestResult result = new TestResult();
                    result.setStatus(TestStatus.Passed);
                    result.setTestCase(this);

                    final String gcmId = UUID.randomUUID().toString();

                    unregisterAll(this, MobileServicePush, gcmId);

                    register(this, MobileServicePush, gcmId, (String[]) null);

                    TemplateRegistration templateRegistration = registerTemplate(this, MobileServicePush, gcmId, templateName, (String[]) null);

                    final String registrationId = templateRegistration.getRegistrationId();

                    unregisterAll(this, MobileServicePush, gcmId);

                    addUnexistingTemplateRegistration(templateName, registrationId);

                    unregisterTemplate(this, MobileServicePush, templateName);

                    callback.onTestComplete(this, result);
                    return;

                } catch (Exception e) {
                    callback.onTestComplete(this, createResultFromException(e));
                    return;
                }
            }
        };

        register.setName(name);
        if (!isNetBackend) {
            register.setExpectedExceptionClass(MobileServiceException.class);
        }

        return register;
    }

    // Notification Roundtrip Tests

    private TestCase createCheckIsRefreshNeeded(String name) {
        TestCase register = new TestCase() {

            @Override
            protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {

                try {
                    final MobileServicePush MobileServicePush = client.getPush();
                    final TestResult result = new TestResult();
                    result.setStatus(TestStatus.Passed);
                    result.setTestCase(this);

                    final String gcmId = UUID.randomUUID().toString();

                    unregisterAll(this, MobileServicePush, gcmId);

                    register(this, MobileServicePush, gcmId, (String[]) null);

                    registerTemplate(this, MobileServicePush, gcmId, UUID.randomUUID().toString(), (String[]) null);

                    if (getRegistrationCountInLocalStorage() != 2) {
                        result.setStatus(TestStatus.Failed);
                    }

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

    private TestCase createNativePushTest(String testName, final String tag, String jsonPayload) {
        final JsonElement orginalPayload = new JsonParser().parse(jsonPayload);

        JsonObject newPayload;
        if (orginalPayload.isJsonObject()) {
            newPayload = orginalPayload.getAsJsonObject();
        } else {
            newPayload = new JsonObject();
            newPayload.add("message", orginalPayload);
        }

        final JsonObject payload = newPayload;

        TestCase result = new TestCase(testName) {

            @Override
            protected void executeTest(final MobileServiceClient client, final TestExecutionCallback callback) {

                try {

                    final MobileServicePush MobileServicePush = client.getPush();
                    String[] tags = tag != null ? new String[]{tag} : null;

                    unregisterAll(this, MobileServicePush, registrationId);

                    register(this, MobileServicePush, registrationId, tags);

                    GCMMessageManager.instance.clearPushMessages();
                    MobileServiceJsonTable table = client.getTable(tableName);
                    JsonObject item = new JsonObject();
                    item.addProperty("method", "send");
                    item.addProperty("tag", tag);
                    JsonObject sentPayload = new JsonObject();
                    sentPayload.add("data", payload);
                    item.add("payload", sentPayload);
                    item.addProperty("usingNH", true);

                    JsonObject jsonObject = table.insert(item).get();

                    this.log("OnCompleted: " + jsonObject.toString());
                    TestExecutionCallback nativeUnregisterTestExecutionCallback = getNativeUnregisterTestExecutionCallback(client, tag, payload, callback);
                    GCMMessageManager.instance.waitForPushMessage(20000, GCMMessageHelper.getPushCallback(this, payload, nativeUnregisterTestExecutionCallback));

                } catch (Exception e) {
                    callback.onTestComplete(this, createResultFromException(e));
                    return;
                }
            }
        };

        result.setName(testName);

        return result;
    }

    private TestCase createTemplatePushTest(String testName, final String tag, final String templateNotification, final String templateName,
                                            final String template, final String expectedPayload) {

        TestCase result = new TestCase(testName) {

            @Override
            protected void executeTest(final MobileServiceClient client, final TestExecutionCallback callback) {

                try {

                    String[] tags = tag != null ? new String[]{tag} : null;

                    final MobileServicePush MobileServicePush = client.getPush();

                    unregisterAll(this, MobileServicePush, registrationId);

                    registerTemplate(this, MobileServicePush, registrationId, templateName, template, tags);

                    JsonElement templateNotificationJsonElement = new JsonParser().parse(templateNotification);

                    GCMMessageManager.instance.clearPushMessages();
                    MobileServiceJsonTable table = client.getTable(tableName);
                    JsonObject item = new JsonObject();
                    item.addProperty("method", "send");
                    item.addProperty("tag", tag);
                    item.addProperty("payload", "not used");
                    item.addProperty("templatePush", true);
                    item.add("templateNotification", templateNotificationJsonElement);
                    item.addProperty("usingNH", true);

                    JsonObject jsonObject = table.insert(item).get();

                    log("OnCompleted: " + jsonObject.toString());
                    TestExecutionCallback nativeUnregisterTestExecutionCallback = getTemplateUnregisterTestExecutionCallback(client, tag, templateName,
                            template, templateNotification, callback);
                    GCMMessageManager.instance.waitForPushMessage(20000,
                            GCMMessageHelper.getPushCallback(this, expectedPayload, nativeUnregisterTestExecutionCallback));

                } catch (Exception e) {
                    callback.onTestComplete(this, createResultFromException(e));
                    return;
                }
            }
        };

        result.setName(testName);

        return result;
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

    private TestExecutionCallback getNativeUnregisterTestExecutionCallback(final MobileServiceClient client, final String tag, final JsonObject payload,
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
                    nativeUnregisterTestExecution(client, test, tag, payload, callback);
                }
            }
        };
    }

    @TargetApi(Build.VERSION_CODES.HONEYCOMB)
    private void nativeUnregisterTestExecution(final MobileServiceClient client, final TestCase test, final String tag, final JsonObject payload,
                                               final TestExecutionCallback callback) {

        new AsyncTask<Void, Void, Void>() {
            @Override
            protected Void doInBackground(Void... arg0) {

                try {
                    unregister(test, client.getPush());

                    GCMMessageManager.instance.clearPushMessages();
                    MobileServiceJsonTable table = client.getTable(tableName);
                    JsonObject item = new JsonObject();
                    item.addProperty("method", "send");
                    item.addProperty("tag", tag);

                    JsonObject sentPayload = new JsonObject();
                    sentPayload.add("data", payload);
                    item.add("payload", sentPayload);

                    item.addProperty("usingNH", true);

                    JsonObject jsonObject = table.insert(item).get();

                    test.log("OnCompleted: " + jsonObject.toString());
                    GCMMessageManager.instance.waitForPushMessage(20000, GCMMessageHelper.getNegativePushCallback(test, callback));
                } catch (Exception exception) {
                    callback.onTestComplete(test, test.createResultFromException(exception));
                    // return;
                }
                return null;
            }
        }.executeOnExecutor(AsyncTask.THREAD_POOL_EXECUTOR);

    }

    private TestExecutionCallback getTemplateUnregisterTestExecutionCallback(final MobileServiceClient client, final String tag, final String templateName,
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
                    templateUnregisterTestExecution(client, test, tag, templateName, template, templateNotification, callback);
                }
            }
        };
    }

    @TargetApi(Build.VERSION_CODES.HONEYCOMB)
    private void templateUnregisterTestExecution(final MobileServiceClient client, final TestCase test, final String tag, final String templateName,
                                                 final String template, final String templateNotification, final TestExecutionCallback callback) {

        new AsyncTask<Void, Void, Void>() {
            @Override
            protected Void doInBackground(Void... arg0) {
                try {

                    String[] tags = tag != null ? new String[]{tag} : null;

                    registerTemplate(test, client.getPush(), templateName, template, tags);

                    unregisterTemplate(test, client.getPush(), templateName);

                    JsonElement templateNotificationJsonElement = new JsonParser().parse(templateNotification);

                    GCMMessageManager.instance.clearPushMessages();
                    MobileServiceJsonTable table = client.getTable(tableName);
                    JsonObject item = new JsonObject();
                    item.addProperty("method", "send");
                    item.addProperty("tag", tag);
                    item.addProperty("payload", "not used");
                    item.addProperty("templatePush", true);
                    item.add("templateNotification", templateNotificationJsonElement);
                    item.addProperty("usingNH", true);

                    JsonObject jsonObject = table.insert(item).get();
                    test.log("OnCompleted: " + jsonObject.toString());
                    GCMMessageManager.instance.waitForPushMessage(10000, GCMMessageHelper.getNegativePushCallback(test, callback));
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
    private void register(TestCase test, MobileServicePush hub, String gcmId, String[] tags, final RegistrationCallback callback) {
        test.log("Register Native with GCMID = " + gcmId);
        if (tags != null && tags.length > 0) {
            for (String tag : tags) {
                test.log("Using tag: " + tag);
            }
        }

        hub.register(gcmId, tags, callback);
    }

    private void registerTemplate(TestCase test, MobileServicePush hub, String gcmId, String templateName, String[] tags, TemplateRegistrationCallback callback) {
        String template = "{\"time_to_live\": 108, \"delay_while_idle\": true, \"data\": { \"message\": \"$(msg)\" } }";
        registerTemplate(test, hub, gcmId, templateName, template, tags, callback);
    }

    @SuppressWarnings("deprecation")
    private void registerTemplate(TestCase test, MobileServicePush hub, String gcmId, String templateName, String template, String[] tags,
                                  TemplateRegistrationCallback callback) {

        test.log("Register with GCMID = " + gcmId);
        test.log("Register with templateName = " + templateName);

        if (tags != null && tags.length > 0) {
            for (String tag : tags) {
                test.log("Using tag: " + tag);
            }
        }

        hub.registerTemplate(gcmId, templateName, template, tags, callback);
    }

    @SuppressWarnings("deprecation")
    private void unregister(TestCase test, MobileServicePush hub, UnregisterCallback callback) {
        test.log("Unregister Native");
        hub.unregister(callback);
    }

    @SuppressWarnings("deprecation")
    private void unregisterTemplate(TestCase test, MobileServicePush hub, String templateName, final UnregisterCallback callback) {
        test.log("UnregisterTemplate with templateName = " + templateName);

        hub.unregisterTemplate(templateName, callback);
    }

    @SuppressWarnings("deprecation")
    private void unregisterAll(TestCase test, MobileServicePush hub, String gcmId, final UnregisterCallback callback) {
        test.log("Unregister Native");
        hub.unregisterAll(gcmId, callback);
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

                    register(this, MobileServicePush, gcmId, (String[]) null, new RegistrationCallback() {

                        @Override
                        public void onRegister(final Registration reg1, Exception exception) {

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

                    registerTemplate(this, MobileServicePush, gcmId, templateName, (String[]) null, new TemplateRegistrationCallback() {

                        @Override
                        public void onRegister(final TemplateRegistration reg1, Exception exception) {
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

    private TestCase UnRegisterNativeWithCallbackTestCase(String name) {
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

    private TestCase UnRegisterTemplateWithCallbackTestCase(String name, final String templateName) {
        TestCase register = new TestCase() {

            @Override
            protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {
                final TestCase that = this;
                try {
                    final MobileServicePush MobileServicePush = client.getPush();
                    final TestResult result = new TestResult();
                    result.setStatus(TestStatus.Passed);
                    result.setTestCase(this);

                    unregisterTemplate(this, MobileServicePush, templateName, new UnregisterCallback() {

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

    private TestCase UnRegisterAllNativeWithCallbackTestCase(String name) {
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

                    unregisterAll(that, MobileServicePush, gcmId, new UnregisterCallback() {

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
        // register.setExpectedExceptionClass(MobileServiceException.class);

        return register;
    }

    private TestCase UnRegisterAllTemplateWithCallbackTestCase(String name, final String templateName) {
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

                    unregisterAll(that, MobileServicePush, gcmId, new UnregisterCallback() {

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
        // register.setExpectedExceptionClass(MobileServiceException.class);

        return register;
    }

    private TestCase createNativePushTestWithRefresh(String testName, final String tag, String jsonPayload) {
        final JsonElement orginalPayload = new JsonParser().parse(jsonPayload);

        JsonObject newPayload;
        if (orginalPayload.isJsonObject()) {
            newPayload = orginalPayload.getAsJsonObject();
        } else {
            newPayload = new JsonObject();
            newPayload.add("message", orginalPayload);
        }

        final JsonObject payload = newPayload;

        TestCase result = new TestCase(testName) {

            @Override
            protected void executeTest(final MobileServiceClient client, final TestExecutionCallback callback) {

                try {

                    final MobileServicePush mobileServicePush = client.getPush();
                    String[] tags = tag != null ? new String[]{tag} : null;

                    unregisterAll(this, mobileServicePush, registrationId);

                    removeStorageVersion();

                    MobileServiceClient client2 = new MobileServiceClient(client);

                    final MobileServicePush mobileServicePush2 = client2.getPush();

                    register(this, mobileServicePush2, registrationId, tags);

                    GCMMessageManager.instance.clearPushMessages();
                    MobileServiceJsonTable table = client.getTable(tableName);
                    JsonObject item = new JsonObject();
                    item.addProperty("method", "send");
                    item.addProperty("tag", tag);
                    JsonObject sentPayload = new JsonObject();
                    sentPayload.add("data", payload);
                    item.add("payload", sentPayload);
                    item.addProperty("usingNH", true);

                    JsonObject jsonObject = table.insert(item).get();

                    this.log("OnCompleted: " + jsonObject.toString());
                    TestExecutionCallback nativeUnregisterTestExecutionCallback = getNativeUnregisterTestExecutionCallback(client, tag, payload, callback);
                    GCMMessageManager.instance.waitForPushMessage(20000, GCMMessageHelper.getPushCallback(this, payload, nativeUnregisterTestExecutionCallback));

                } catch (Exception e) {
                    callback.onTestComplete(this, createResultFromException(e));
                    return;
                }
            }
        };

        result.setName(testName);

        return result;
    }

    private void removeStorageVersion() {

        SharedPreferences sharedPreferences = PreferenceManager.getDefaultSharedPreferences(MainActivity.getInstance());

        Editor editor = sharedPreferences.edit();
        editor.remove("__NH_STORAGE_VERSION");

        editor.commit();
    }
}