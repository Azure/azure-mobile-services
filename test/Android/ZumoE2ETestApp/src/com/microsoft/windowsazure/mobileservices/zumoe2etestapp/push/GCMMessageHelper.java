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
package com.microsoft.windowsazure.mobileservices.zumoe2etestapp.push;

import java.util.Set;
import java.util.Map.Entry;

import android.content.Intent;

import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestCase;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestExecutionCallback;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestResult;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestStatus;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.EnhancedPushTests;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.PushTests;

public class GCMMessageHelper {
	public static GCMMessageCallback getRegistrationCallBack(final TestCase test, final TestExecutionCallback callback, final Class<?> clazz) {
		return new GCMMessageCallback() {
			@Override
			public void timeoutElapsed() {
				test.log("Error, registration message did not arrive on time");
				TestResult testResult = new TestResult();
				testResult.setTestCase(test);
				testResult.setStatus(TestStatus.Failed);
				callback.onTestComplete(test, testResult);
			}

			@Override
			public void registrationMessageReceived(boolean isError, String value) {
				TestResult testResult = new TestResult();
				testResult.setTestCase(test);

				if (isError) {
					test.log("Received error during registration: errorId = " + value);
					testResult.setStatus(TestStatus.Failed);
					callback.onTestComplete(test, testResult);
				} else {
					if (clazz.getCanonicalName().equals(EnhancedPushTests.class.getCanonicalName())) {
						EnhancedPushTests.registrationId = value;
					} else if (clazz.getCanonicalName().equals(PushTests.class.getCanonicalName())) {
						PushTests.registrationId = value;
					}

					test.log("Registration completed successfully. RegistrationId = " + value);
					testResult.setStatus(TestStatus.Passed);
					callback.onTestComplete(test, testResult);
				}
			}
		};
	}

	public static GCMMessageCallback getPushCallback(TestCase test, String expectedPayload, TestExecutionCallback callback) {
		return getPushCallback(test, new JsonParser().parse(expectedPayload).getAsJsonObject(), callback);
	}

	public static GCMMessageCallback getPushCallback(final TestCase test, final JsonObject expectedPayload, final TestExecutionCallback callback) {
		return new GCMMessageCallback() {

			@Override
			public void timeoutElapsed() {
				test.log("Did not receive push message on time, test failed");
				TestResult testResult = new TestResult();
				testResult.setTestCase(test);
				testResult.setStatus(TestStatus.Failed);
				callback.onTestComplete(test, testResult);
			}

			@Override
			public void pushMessageReceived(Intent intent) {
				test.log("Received push message: " + intent.toString());
				TestResult testResult = new TestResult();
				testResult.setTestCase(test);
				testResult.setStatus(TestStatus.Passed);

				Set<Entry<String, JsonElement>> payloadEntries = expectedPayload.entrySet();

				for (Entry<String, JsonElement> entry : payloadEntries) {
					String key = entry.getKey();
					String value = entry.getValue().getAsString();
					String intentExtra = intent.getStringExtra(key);

					if (value.equals(intentExtra)) {
						test.log("Retrieved correct value for key " + key);
					} else {
						test.log("Error retrieving value for key " + key + ". Expected: " + value + "; actual: " + intentExtra);
						testResult.setStatus(TestStatus.Failed);
					}
				}

				callback.onTestComplete(test, testResult);
			}
		};
	}

	public static GCMMessageCallback getNegativePushCallback(final TestCase test, final TestExecutionCallback callback) {
		return new GCMMessageCallback() {

			@Override
			public void timeoutElapsed() {
				test.log("Did not receive push message after timeout. Correctly unregistered. Test succeded");
				TestResult testResult = new TestResult();
				testResult.setTestCase(test);
				testResult.setStatus(TestStatus.Passed);
				callback.onTestComplete(test, testResult);
			}

			@Override
			public void pushMessageReceived(Intent intent) {
				test.log("Received push message: " + intent.toString() + ". Incorrectly unregistered. Test failed.");
				TestResult testResult = new TestResult();
				testResult.setTestCase(test);
				testResult.setStatus(TestStatus.Failed);

				callback.onTestComplete(test, testResult);
			}
		};
	}
}