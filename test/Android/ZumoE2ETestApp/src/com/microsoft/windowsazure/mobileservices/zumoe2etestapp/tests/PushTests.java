package com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests;

import com.google.android.gcm.GCMRegistrar;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
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
		this.addTest(createGCMUnregisterTest());
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
