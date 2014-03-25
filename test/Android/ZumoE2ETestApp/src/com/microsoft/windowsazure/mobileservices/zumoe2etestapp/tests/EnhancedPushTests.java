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

import java.util.List;
import java.util.Set;
import java.util.UUID;

import android.content.SharedPreferences;
import android.content.SharedPreferences.Editor;
import android.preference.PreferenceManager;

import com.google.android.gcm.GCMRegistrar;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceException;
import com.microsoft.windowsazure.mobileservices.MobileServiceJsonTable;
import com.microsoft.windowsazure.mobileservices.MobileServicePush;
import com.microsoft.windowsazure.mobileservices.Registration;
import com.microsoft.windowsazure.mobileservices.RegistrationCallback;
import com.microsoft.windowsazure.mobileservices.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.TableJsonOperationCallback;
import com.microsoft.windowsazure.mobileservices.TemplateRegistration;
import com.microsoft.windowsazure.mobileservices.TemplateRegistrationCallback;
import com.microsoft.windowsazure.mobileservices.UnregisterCallback;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.MainActivity;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestCase;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestExecutionCallback;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestGroup;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestResult;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestStatus;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.push.GCMMessageHelper;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.push.GCMMessageManager;

public class EnhancedPushTests extends TestGroup {

	private static final String tableName = "droidPushTest";

	/*
	 * Pointer to the main activity used to register with GCM
	 */
	public static MainActivity mainActivity;

	public static String registrationId;

	private static final String DEFAULT_REGISTRATION_NAME = "$Default";
	private static final String REGISTRATION_NAME_STORAGE_KEY = "__NH_REG_NAME_";

	private void register(TestCase test, MobileServicePush hub, String gcmId, String[] tags, final RegistrationCallback callback) {
		test.log("Register Native with GCMID = " + gcmId);
		if (tags != null && tags.length > 0) {
			for (String tag : tags) {
				test.log("Using tag: " + tag);
			}
		}

		hub.register(gcmId, tags, callback);
	}

	private void unregister(TestCase test, MobileServicePush hub, UnregisterCallback callback) {
		test.log("Unregister Native");
		hub.unregister(callback);
	}

	private void registerTemplate(TestCase test, MobileServicePush hub, String gcmId, String templateName, String[] tags, TemplateRegistrationCallback callback) {
		String template = "{\"time_to_live\": 108, \"delay_while_idle\": true, \"data\": { \"message\": \"$(msg)\" } }";
		registerTemplate(test, hub, gcmId, templateName, template, tags, callback);
	}

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

	private void unregisterTemplate(TestCase test, MobileServicePush hub, String templateName, final UnregisterCallback callback) {
		test.log("UnregisterTemplate with templateName = " + templateName);

		hub.unregisterTemplate(templateName, callback);
	}

	private void unregisterAll(TestCase test, MobileServicePush hub, String gcmId, final UnregisterCallback callback) {
		test.log("Unregister Native");
		hub.unregisterAll(gcmId, callback);
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

	private void addUnexistingNativeRegistration(String registrationId) {
		addUnexistingTemplateRegistration(DEFAULT_REGISTRATION_NAME, registrationId);
	}

	private void addUnexistingTemplateRegistration(String templateName, String registrationId) {
		SharedPreferences sharedPreferences = PreferenceManager.getDefaultSharedPreferences(MainActivity.getInstance());

		Editor editor = sharedPreferences.edit();
		editor.putString(REGISTRATION_NAME_STORAGE_KEY + templateName, registrationId);
		editor.commit();
	}

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

	// Register Native Tests

	private TestCase createReRegisterNativeTestCase(String name) {
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

							unregister(that, MobileServicePush, new UnregisterCallback() {

								@Override
								public void onUnregister(Exception exception) {
									if (exception != null) {
										callback.onTestComplete(that, createResultFromException(exception));
										return;
									}

									register(that, MobileServicePush, gcmId, (String[]) null, new RegistrationCallback() {

										@Override
										public void onRegister(Registration reg2, Exception exception) {

											if (exception != null) {
												callback.onTestComplete(that, createResultFromException(exception));
												return;
											}

											if (reg2.getRegistrationId().equals(reg1.getRegistrationId())) {
												result.setStatus(TestStatus.Failed);
											}

											unregister(that, MobileServicePush, new UnregisterCallback() {

												@Override
												public void onUnregister(Exception exception) {
													if (exception != null) {
														callback.onTestComplete(that, createResultFromException(exception));
														return;
													}

													callback.onTestComplete(that, result);
												}
											});
										}
									});
								}
							});
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

	// Register Template Tests

	private TestCase createReRegisterTemplateTestCase(String name, final String templateName) {
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

							unregisterTemplate(that, MobileServicePush, templateName, new UnregisterCallback() {

								@Override
								public void onUnregister(Exception exception) {
									if (exception != null) {
										callback.onTestComplete(that, createResultFromException(exception));
										return;
									}

									registerTemplate(that, MobileServicePush, gcmId, templateName, (String[]) null, new TemplateRegistrationCallback() {

										@Override
										public void onRegister(TemplateRegistration reg2, Exception exception) {
											if (exception != null) {
												callback.onTestComplete(that, createResultFromException(exception));
												return;
											}

											if (reg2.getRegistrationId().equals(reg1.getRegistrationId())) {
												result.setStatus(TestStatus.Failed);
											}

											unregisterTemplate(that, MobileServicePush, templateName, new UnregisterCallback() {

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
										}
									});
								}
							});
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

	// Unregister Native Tests

	private TestCase createUnregisterNativeNonExistingTestCase(String name) {
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

	private TestCase createUnregisterNativeUnexistingRegistrationTestCase(String name) {
		TestCase register = new TestCase() {

			@Override
			protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {

				final TestCase that = this;

				try {
					final MobileServicePush MobileServicePush = client.getPush();
					final TestResult result = new TestResult();
					result.setStatus(TestStatus.Passed);
					result.setTestCase(this);

					String gcmId = UUID.randomUUID().toString();

					register(this, MobileServicePush, gcmId, (String[]) null, new RegistrationCallback() {

						@Override
						public void onRegister(Registration registration, Exception exception) {
							if (exception != null) {
								callback.onTestComplete(that, createResultFromException(exception));
								return;
							}

							final String registrationId = registration.getRegistrationId();

							unregister(that, MobileServicePush, new UnregisterCallback() {

								@Override
								public void onUnregister(Exception exception) {
									if (exception != null) {
										callback.onTestComplete(that, createResultFromException(exception));
										return;
									}

									addUnexistingNativeRegistration(registrationId);

									unregister(that, MobileServicePush, new UnregisterCallback() {

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
								}
							});
						}
					});
				} catch (Exception e) {
					callback.onTestComplete(this, createResultFromException(e));
					return;
				}
			}
		};

		register.setName(name);
		register.setExpectedExceptionClass(MobileServiceException.class);

		return register;
	}

	// Unregister Template Tests

	private TestCase createUnregisterTemplateNonExistingTestCase(String name, final String templateName) {
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

					registerTemplate(this, MobileServicePush, UUID.randomUUID().toString(), templateName, (String[]) null, new TemplateRegistrationCallback() {

						@Override
						public void onRegister(TemplateRegistration templateRegistration, Exception exception) {
							if (exception != null) {
								callback.onTestComplete(that, createResultFromException(exception));
								return;
							}

							final String registrationId = templateRegistration.getRegistrationId();
							unregisterTemplate(that, MobileServicePush, templateName, new UnregisterCallback() {

								@Override
								public void onUnregister(Exception exception) {
									if (exception != null) {
										callback.onTestComplete(that, createResultFromException(exception));
										return;
									}

									addUnexistingTemplateRegistration(templateName, registrationId);

									unregisterTemplate(that, MobileServicePush, templateName, new UnregisterCallback() {

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
								}
							});
						}
					});
				} catch (Exception e) {
					callback.onTestComplete(this, createResultFromException(e));
					return;
				}
			}
		};

		register.setName(name);
		register.setExpectedExceptionClass(MobileServiceException.class);

		return register;
	}

	// Unregister All Tests

	private TestCase createUnregisterAllUnregisterNativeTestCase(String name, final String templateName) {
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

							register(that, MobileServicePush, gcmId, (String[]) null, new RegistrationCallback() {

								@Override
								public void onRegister(Registration nativeRegistration, Exception exception) {
									if (exception != null) {
										callback.onTestComplete(that, createResultFromException(exception));
										return;
									}

									final String registrationId = nativeRegistration.getRegistrationId();

									registerTemplate(that, MobileServicePush, gcmId, templateName, (String[]) null, new TemplateRegistrationCallback() {

										@Override
										public void onRegister(TemplateRegistration templateRegistration, Exception exception) {
											if (exception != null) {
												callback.onTestComplete(that, createResultFromException(exception));
												return;
											}

											unregisterAll(that, MobileServicePush, gcmId, new UnregisterCallback() {

												@Override
												public void onUnregister(Exception exception) {
													if (exception != null) {
														callback.onTestComplete(that, createResultFromException(exception));
														return;
													}

													addUnexistingNativeRegistration(registrationId);

													unregister(that, MobileServicePush, new UnregisterCallback() {

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
												}
											});
										}
									});
								}
							});
						}
					});
				} catch (Exception e) {
					callback.onTestComplete(this, createResultFromException(e));
					return;
				}
			}
		};

		register.setName(name);
		register.setExpectedExceptionClass(MobileServiceException.class);

		return register;
	}

	private TestCase createUnregisterAllUnregisterTemplateTestCase(String name, final String templateName) {
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

							register(that, MobileServicePush, gcmId, (String[]) null, new RegistrationCallback() {

								@Override
								public void onRegister(Registration nativeRegistration, Exception exception) {
									if (exception != null) {
										callback.onTestComplete(that, createResultFromException(exception));
										return;
									}

									registerTemplate(that, MobileServicePush, gcmId, templateName, (String[]) null, new TemplateRegistrationCallback() {

										@Override
										public void onRegister(TemplateRegistration templateRegistration, Exception exception) {
											if (exception != null) {
												callback.onTestComplete(that, createResultFromException(exception));
												return;
											}

											final String registrationId = templateRegistration.getRegistrationId();

											unregisterAll(that, MobileServicePush, gcmId, new UnregisterCallback() {

												@Override
												public void onUnregister(Exception exception) {
													if (exception != null) {
														callback.onTestComplete(that, createResultFromException(exception));
														return;
													}

													addUnexistingTemplateRegistration(templateName, registrationId);

													unregisterTemplate(that, MobileServicePush, templateName, new UnregisterCallback() {

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
												}
											});
										}
									});
								}
							});
						}
					});
				} catch (Exception e) {
					callback.onTestComplete(this, createResultFromException(e));
					return;
				}
			}
		};

		register.setName(name);
		register.setExpectedExceptionClass(MobileServiceException.class);

		return register;
	}

	private TestCase createCheckIsRefreshNeeded(String name) {
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

							register(that, MobileServicePush, gcmId, (String[]) null, new RegistrationCallback() {

								@Override
								public void onRegister(Registration nativeRegistration, Exception exception) {
									if (exception != null) {
										callback.onTestComplete(that, createResultFromException(exception));
										return;
									}

									registerTemplate(that, MobileServicePush, gcmId, UUID.randomUUID().toString(), (String[]) null,
											new TemplateRegistrationCallback() {

												@Override
												public void onRegister(TemplateRegistration templateRegistration, Exception exception) {
													if (exception != null) {
														callback.onTestComplete(that, createResultFromException(exception));
														return;
													}

													if (getRegistrationCountInLocalStorage() != 2) {
														result.setStatus(TestStatus.Failed);
													}

													callback.onTestComplete(that, result);
													return;
												}
											});
								}
							});
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

	// Notification Roundtrip Tests

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
				final TestCase test = this;
				String[] tags = tag != null ? new String[] { tag } : null;
				client.getPush().register(registrationId, tags, new RegistrationCallback() {

					@Override
					public void onRegister(Registration registration, Exception exception) {
						if (exception != null) {
							callback.onTestComplete(test, createResultFromException(exception));
							return;
						}

						GCMMessageManager.instance.clearPushMessages();
						MobileServiceJsonTable table = client.getTable(tableName);
						JsonObject item = new JsonObject();
						item.addProperty("method", "send");
						item.addProperty("tag", tag);
						JsonObject sentPayload = new JsonObject();
						sentPayload.add("data", payload);
						item.add("payload", sentPayload);
						item.addProperty("usingNH", true);

						table.insert(item, new TableJsonOperationCallback() {

							@Override
							public void onCompleted(JsonObject jsonObject, Exception exception, ServiceFilterResponse response) {
								if (exception != null) {
									callback.onTestComplete(test, createResultFromException(exception));
									return;
								}

								test.log("OnCompleted: " + jsonObject.toString());
								TestExecutionCallback nativeUnregisterTestExecutionCallback = getNativeUnregisterTestExecutionCallback(client, tag, payload,
										callback);
								GCMMessageManager.instance.waitForPushMessage(5000,
										GCMMessageHelper.getPushCallback(test, payload, nativeUnregisterTestExecutionCallback));
							}
						});
					}
				});
			}
		};

		return result;
	}

	private TestCase createTemplatePushTest(String testName, final String tag, final String templateNotification, final String templateName,
			final String template, final String expectedPayload) {
		TestCase result = new TestCase(testName) {

			@Override
			protected void executeTest(final MobileServiceClient client, final TestExecutionCallback callback) {
				final TestCase test = this;
				String[] tags = tag != null ? new String[] { tag } : null;
				client.getPush().registerTemplate(registrationId, templateName, template, tags, new TemplateRegistrationCallback() {

					@Override
					public void onRegister(TemplateRegistration registration, Exception exception) {
						if (exception != null) {
							callback.onTestComplete(test, createResultFromException(exception));
							return;
						}

						GCMMessageManager.instance.clearPushMessages();
						MobileServiceJsonTable table = client.getTable(tableName);
						JsonObject item = new JsonObject();
						item.addProperty("method", "send");
						item.addProperty("tag", tag);
						item.addProperty("payload", "not used");
						item.addProperty("templatePush", true);
						item.addProperty("templateNotification", templateNotification);
						item.addProperty("usingNH", true);

						table.insert(item, new TableJsonOperationCallback() {

							@Override
							public void onCompleted(JsonObject jsonObject, Exception exception, ServiceFilterResponse response) {
								if (exception != null) {
									callback.onTestComplete(test, createResultFromException(exception));
									return;
								}

								log("OnCompleted: " + jsonObject.toString());
								TestExecutionCallback nativeUnregisterTestExecutionCallback = getTemplateUnregisterTestExecutionCallback(client, tag,
										templateName, template, templateNotification, callback);
								GCMMessageManager.instance.waitForPushMessage(5000,
										GCMMessageHelper.getPushCallback(test, expectedPayload, nativeUnregisterTestExecutionCallback));
							}
						});
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
				GCMRegistrar.checkDevice(mainActivity);
				GCMRegistrar.checkManifest(mainActivity);
				String registrationId = GCMRegistrar.getRegistrationId(mainActivity);
				EnhancedPushTests.registrationId = registrationId;
				log("Registration ID: " + EnhancedPushTests.registrationId);

				if ("".equals(registrationId)) {
					GCMRegistrar.register(mainActivity, mainActivity.getGCMSenderId());
					log("Called GCMRegistrar.register");
					GCMMessageManager.instance.waitForRegistrationMessage(5000,
							GCMMessageHelper.getRegistrationCallBack(test, callback, EnhancedPushTests.class));
				} else {
					TestResult testResult = new TestResult();
					testResult.setTestCase(this);
					testResult.setStatus(TestStatus.Passed);
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

	private void nativeUnregisterTestExecution(final MobileServiceClient client, final TestCase test, final String tag, final JsonObject payload,
			final TestExecutionCallback callback) {
		client.getPush().unregister(new UnregisterCallback() {

			@Override
			public void onUnregister(Exception exception) {
				if (exception != null) {
					callback.onTestComplete(test, test.createResultFromException(exception));
					return;
				}

				GCMMessageManager.instance.clearPushMessages();
				MobileServiceJsonTable table = client.getTable(tableName);
				JsonObject item = new JsonObject();
				item.addProperty("method", "send");
				item.addProperty("tag", tag);

				JsonObject sentPayload = new JsonObject();
				sentPayload.add("data", payload);
				item.add("payload", sentPayload);

				item.addProperty("usingNH", true);

				table.insert(item, new TableJsonOperationCallback() {

					@Override
					public void onCompleted(JsonObject jsonObject, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							callback.onTestComplete(test, test.createResultFromException(exception));
							return;
						}

						test.log("OnCompleted: " + jsonObject.toString());
						GCMMessageManager.instance.waitForPushMessage(5000, GCMMessageHelper.getNegativePushCallback(test, callback));
					}
				});
			}
		});
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

	private void templateUnregisterTestExecution(final MobileServiceClient client, final TestCase test, final String tag, final String templateName,
			final String template, final String templateNotification, final TestExecutionCallback callback) {
		client.getPush().registerTemplate(registrationId, templateName, template, new String[] { tag }, new TemplateRegistrationCallback() {

			@Override
			public void onRegister(TemplateRegistration registration, Exception exception) {
				if (exception != null) {
					callback.onTestComplete(test, test.createResultFromException(exception));
					return;
				}

				client.getPush().unregisterTemplate(templateName, new UnregisterCallback() {

					@Override
					public void onUnregister(Exception exception) {
						if (exception != null) {
							callback.onTestComplete(test, test.createResultFromException(exception));
							return;
						}

						GCMMessageManager.instance.clearPushMessages();
						MobileServiceJsonTable table = client.getTable(tableName);
						JsonObject item = new JsonObject();
						item.addProperty("method", "send");
						item.addProperty("tag", tag);

						item.addProperty("payload", "not used");

						item.addProperty("templatePush", true);
						item.addProperty("templateNotification", templateNotification);

						item.addProperty("usingNH", true);

						table.insert(item, new TableJsonOperationCallback() {

							@Override
							public void onCompleted(JsonObject jsonObject, Exception exception, ServiceFilterResponse response) {
								if (exception != null) {
									callback.onTestComplete(test, test.createResultFromException(exception));
									return;
								}

								test.log("OnCompleted: " + jsonObject.toString());
								GCMMessageManager.instance.waitForPushMessage(5000, GCMMessageHelper.getNegativePushCallback(test, callback));
							}
						});
					}
				});
			}
		});
	}

	public EnhancedPushTests() {
		super("Enhanced Push tests");

		// Notification Roundtrip Tests

		this.addTest(createGCMRegisterTest());

		String json = "'Notification Hub test notification'".replace('\'', '\"');
		this.addTest(createNativePushTest("Native Notification Roundtrip - Simple payload", "tag1", json));

		json = "{'name':'John Doe','age':'33'}".replace('\'', '\"');
		this.addTest(createNativePushTest("Native Notification Roundtrip - Complex payload", "tag2", json));

		this.addTest(createNativePushTest("Native Notification Roundtrip - No Tag", null, json));

		String templateNotification = "{'fullName':'John Doe'}".replace('\'', '\"');
		String template = "{'data':{'user':'$(fullName)'}}".replace('\'', '\"');
		String expectedPayload = "{'user':'John Doe'}".replace('\'', '\"');

		this.addTest(createTemplatePushTest("Template Notification Roundtrip - Tag", "tag4", templateNotification, "template1", template, expectedPayload));

		this.addTest(createTemplatePushTest("Template Notification Roundtrip - No Tag", null, templateNotification, "template1", template, expectedPayload));

		this.addTest(createGCMUnregisterTest());

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
	}
}