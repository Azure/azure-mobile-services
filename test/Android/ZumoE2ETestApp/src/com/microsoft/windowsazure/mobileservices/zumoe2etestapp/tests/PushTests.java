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

import com.google.android.gcm.GCMRegistrar;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceJsonTable;
import com.microsoft.windowsazure.mobileservices.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.TableJsonOperationCallback;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.MainActivity;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestCase;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestExecutionCallback;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestGroup;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestResult;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestStatus;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.push.GCMMessageHelper;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.push.GCMMessageManager;

public class PushTests extends TestGroup {

	private static final String tableName = "droidPushTest";

	/*
	 * Pointer to the main activity used to register with GCM
	 */
	public static MainActivity mainActivity;

	public static String registrationId;

	public PushTests() {
		super("Push tests");

		this.addTest(createGCMRegisterTest());

		String json = "{'name':'John Doe','age':'33'}".replace('\'', '\"');
		this.addTest(createPushTest("Push - Simple data", json));
		json = "{'ticker':'MSFT'}".replace('\'', '\"');
		this.addTest(createPushTest("Push - Single value", json));
		json = "{'non-ASCII':'Latin-ãéìôü ÇñÑ, arabic-لكتاب على الطاولة, chinese-这本书在桌子上'}";
		this.addTest(createPushTest("Push - non-ASCII characters", json));
		json = "\"A single string\"";
		this.addTest(createPushTest("Push - Single string", json));

		this.addTest(createNegativePushTest("(Neg) Invalid payload type (array)", "[{\"name\":\"John Doe\"}]"));
		this.addTest(createNegativePushTest("(Neg) Invalid payload type (number)", "1234"));
		this.addTest(createNegativePushTest("(Neg) Invalid payload value (number)", "{\"name\":1234}"));
		this.addTest(createNegativePushTest("(Neg) Invalid payload value (object)", "{'name':{'first':'John','last':'Doe'}}".replace('\'', '\"')));

		this.addTest(createGCMUnregisterTest());
	}

	private TestCase createNegativePushTest(String testName, String jsonPayload) {
		final JsonElement payload = new JsonParser().parse(jsonPayload);
		return new TestCase(testName) {

			@Override
			protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {
				MobileServiceJsonTable table = client.getTable(tableName);
				JsonObject item = new JsonObject();
				item.addProperty("method", "send");
				item.addProperty("registrationId", registrationId);
				item.add("payload", payload);
				final TestCase testCase = this;
				table.insert(item, new TableJsonOperationCallback() {

					@Override
					public void onCompleted(JsonObject jsonObject, Exception exception, ServiceFilterResponse response) {
						TestResult testResult = new TestResult();
						testResult.setTestCase(testCase);
						if (exception == null) {
							log("Error, expected exception, but got none.");
							log("Returned jsonObject: " + jsonObject.toString());
							testResult.setStatus(TestStatus.Failed);
						} else {
							log("Received expected exception: " + exception.toString());
							log("Response: " + response.getStatus().getStatusCode());
							log("Response body: " + response.getContent());
							testResult.setStatus(TestStatus.Passed);
						}

						callback.onTestComplete(testCase, testResult);
					}
				});
			}
		};
	}

	private TestCase createPushTest(String testName, String jsonPayload) {
		final JsonElement payload = new JsonParser().parse(jsonPayload);
		TestCase result = new TestCase(testName) {

			@Override
			protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {
				MobileServiceJsonTable table = client.getTable(tableName);
				JsonObject item = new JsonObject();
				item.addProperty("method", "send");
				item.addProperty("registrationId", registrationId);
				item.add("payload", payload);

				final TestCase test = this;
				table.insert(item, new TableJsonOperationCallback() {

					@Override
					public void onCompleted(JsonObject jsonObject, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							callback.onTestComplete(test, createResultFromException(exception));
							return;
						}

						final JsonObject expectedPayload;
						if (payload.isJsonObject()) {
							expectedPayload = payload.getAsJsonObject();
						} else {
							expectedPayload = new JsonObject();
							expectedPayload.add("message", payload);
						}

						log("OnCompleted: " + jsonObject.toString());
						GCMMessageManager.instance.waitForPushMessage(5000, GCMMessageHelper.getPushCallback(test, expectedPayload, callback));
					}
				});
			}
		};

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
				final TestCase test = this;
				final TestResult testResult = new TestResult();
				testResult.setTestCase(this);
				GCMRegistrar.checkDevice(mainActivity);
				GCMRegistrar.checkManifest(mainActivity);
				String registrationId = GCMRegistrar.getRegistrationId(mainActivity);
				PushTests.registrationId = registrationId;
				log("Registration ID: " + PushTests.registrationId);
				if ("".equals(registrationId)) {
					GCMRegistrar.register(mainActivity, mainActivity.getGCMSenderId());
					log("Called GCMRegistrar.register");
					GCMMessageManager.instance.waitForRegistrationMessage(5000, GCMMessageHelper.getRegistrationCallBack(test, callback, PushTests.class));
				} else {
					testResult.setStatus(TestStatus.Passed);
					callback.onTestComplete(this, testResult);
				}
			}

		};

		return testCase;
	}
}
