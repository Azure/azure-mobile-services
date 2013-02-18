package com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests;

import java.util.Map.Entry;
import java.util.Set;

import android.content.Intent;
import android.os.Bundle;

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
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.push.GCMMessageCallback;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.push.GCMMessageManager;

public class PushTests extends TestGroup {

	/*
	 * Pointer to the main activity used to register with GCM
	 */
	public static MainActivity mainActivity;
	
	private static String registrationId;
	
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
		
		this.addTest(createGCMUnregisterTest());
	}

	private TestCase createPushTest(String testName, String jsonPayload) {
		return createPushTest(testName, new JsonParser().parse(jsonPayload));
	}
	private TestCase createPushTest(String testName, final JsonElement payload) {
		TestCase result = new TestCase() {

			@Override
			protected void executeTest(MobileServiceClient client,
					final TestExecutionCallback callback) {
				MobileServiceJsonTable table = client.getTable("droidPushTest");
				JsonObject item = new JsonObject();
				item.addProperty("method", "send");
				item.addProperty("registrationId", registrationId);
				item.add("payload", payload);
				final TestCase testCase = this;
				table.insert(item, new TableJsonOperationCallback() {

					@Override
					public void onCompleted(JsonObject jsonObject,
							Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							callback.onTestComplete(
									testCase, createResultFromException(exception));
							return;
						}
						
						log("OnCompleted: " + jsonObject.toString());
						GCMMessageManager.instance.waitForPushMessage(5000, new GCMMessageCallback() {
							@Override
							public void timeoutElapsed() {
								log("Did not receive push message on time, test failed");
								TestResult testResult = new TestResult();
								testResult.setTestCase(testCase);
								testResult.setStatus(TestStatus.Failed);
								callback.onTestComplete(testCase, testResult);
							}

							@Override
							public void pushMessageReceived(Intent intent) {
								log("Received push message: " + intent.toString());
								TestResult testResult = new TestResult();
								testResult.setTestCase(testCase);
								testResult.setStatus(TestStatus.Passed);
								JsonObject expectedPayload;
								if (payload.isJsonObject()) {
									expectedPayload = payload.getAsJsonObject();
								} else {
									expectedPayload = new JsonObject();
									expectedPayload.add("message", payload);
								}

								Set<Entry<String, JsonElement>> payloadEntries;
								payloadEntries = payload.getAsJsonObject().entrySet();
								for (Entry<String, JsonElement> entry : payloadEntries) {
									String key = entry.getKey();
									String value = entry.getValue().getAsString();
									String intentExtra = intent.getStringExtra(key);
									if (value.equals(intentExtra)) {
										testCase.log("Retrieved correct value for key " + key);
									} else {
										testCase.log("Error retrieving value for key " + key + ". Expected: " + value + "; actual: " + intentExtra);
										testResult.setStatus(TestStatus.Failed);
									}
								}

								callback.onTestComplete(testCase, testResult);
							}
						});
					}
				});
				
			}
			
		};
		
		result.setName(testName);
		return result;
	}
	
	private TestCase createGCMUnregisterTest() {
		TestCase testCase = new TestCase() {

			@Override
			protected void executeTest(MobileServiceClient client,
					TestExecutionCallback callback) {
				GCMRegistrar.unregister(mainActivity);
				log("Unregistered from GCM");
				TestResult testResult = new TestResult();
				testResult.setStatus(TestStatus.Passed);
				testResult.setTestCase(this);
				callback.onTestComplete(this, testResult);
			}
			
		};

		testCase.setName("Unregister from GCM");
		return testCase;
	}

	private TestCase createGCMRegisterTest() {
		TestCase testCase = new TestCase() {

			@Override
			protected void executeTest(MobileServiceClient client,
					final TestExecutionCallback callback) {
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
					GCMMessageManager.instance.waitForRegistrationMessage(5000, new GCMMessageCallback() {
						@Override
						public void timeoutElapsed() {
							test.log("Error, registration message did not arrive on time");
							testResult.setStatus(TestStatus.Failed);
							callback.onTestComplete(test, testResult);
						}
						
						@Override
						public void registrationMessageReceived(boolean isError, String value) {
							if (isError) {
								test.log("Received error during registration: errorId = " + value);
								testResult.setStatus(TestStatus.Failed);
								callback.onTestComplete(test, testResult);
							} else {
								PushTests.registrationId = value;
								test.log("Registration completed successfully. RegistrationId = " + value);
								testResult.setStatus(TestStatus.Passed);
								callback.onTestComplete(test, testResult);
							}
						}
					});
				} else {
					testResult.setStatus(TestStatus.Passed);
					callback.onTestComplete(this, testResult);
				}
			}
			
		};
		
		testCase.setName("Register app with GCM");
		return testCase;
	}
}
