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

import java.util.List;
import java.util.UUID;
import java.util.concurrent.CountDownLatch;

import junit.framework.Assert;

import org.apache.http.Header;
import org.apache.http.HeaderElement;
import org.apache.http.ParseException;

import android.content.Context;
import android.content.SharedPreferences;
import android.preference.PreferenceManager;
import android.test.InstrumentationTestCase;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServicePush;
import com.microsoft.windowsazure.mobileservices.NextServiceFilterCallback;
import com.microsoft.windowsazure.mobileservices.Registration;
import com.microsoft.windowsazure.mobileservices.RegistrationCallback;
import com.microsoft.windowsazure.mobileservices.RegistrationGoneException;
import com.microsoft.windowsazure.mobileservices.ServiceFilter;
import com.microsoft.windowsazure.mobileservices.ServiceFilterRequest;
import com.microsoft.windowsazure.mobileservices.ServiceFilterResponseCallback;
import com.microsoft.windowsazure.mobileservices.TemplateRegistration;
import com.microsoft.windowsazure.mobileservices.TemplateRegistrationCallback;
import com.microsoft.windowsazure.mobileservices.UnregisterCallback;

public class EnhancedPushTests extends InstrumentationTestCase {

	/**
	 * Prefix for Storage keys
	 */
	private static final String STORAGE_PREFIX = "__NH_";

	/**
	 * Prefix for registration information keys in local storage
	 */
	private static final String REGISTRATION_NAME_STORAGE_KEY = "REG_NAME_";

	/**
	 * Name for default registration
	 */
	static final String DEFAULT_REGISTRATION_NAME = "$Default";

	/**
	 * New registration location header name
	 */
	private static final String NEW_REGISTRATION_LOCATION_HEADER = "Location";

	String appUrl = "";
	String appKey = "";

	protected void setUp() throws Exception {
		appUrl = "http://myapp.com/";
		appKey = "qwerty";
		super.setUp();
	}

	protected void tearDown() throws Exception {
		super.tearDown();
	}

	public void testRegisterUnregisterNative() throws Throwable {
		try {
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

			push.register(handle, new String[] { "tag1" }, new RegistrationCallback() {

				@Override
				public void onRegister(Registration registration, Exception exception) {
					if (exception != null) {
						container.exception = exception;

						latch.countDown();
					} else {
						container.registrationId = registration.getRegistrationId();

						container.storedRegistrationId = sharedPreferences.getString(
								STORAGE_PREFIX + REGISTRATION_NAME_STORAGE_KEY + DEFAULT_REGISTRATION_NAME, null);

						push.unregister(new UnregisterCallback() {

							@Override
							public void onUnregister(Exception exception) {
								if (exception != null) {
									container.exception = exception;
								} else {
									container.unregister = sharedPreferences.getString(STORAGE_PREFIX + REGISTRATION_NAME_STORAGE_KEY
											+ DEFAULT_REGISTRATION_NAME, null);
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
		} catch (Exception e) {
			e.printStackTrace();
		}
	}

	public void testRegisterUnregisterTemplate() throws Throwable {
		try {
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

			push.registerTemplate(handle, templateName, "{ }", new String[] { "tag1" }, new TemplateRegistrationCallback() {

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
		} catch (Exception e) {
			e.printStackTrace();
		}
	}

	public void testRegisterFailNative() throws Throwable {
		try {
			final CountDownLatch latch = new CountDownLatch(1);

			Context context = getInstrumentation().getTargetContext();
			final SharedPreferences sharedPreferences = PreferenceManager.getDefaultSharedPreferences(context.getApplicationContext());

			final Container container = new Container();
			final String handle = "handle";

			String registrationId = "registrationId";

			MobileServiceClient client = new MobileServiceClient(appUrl, appKey, context);

			client = client.withFilter(getUpsertFailTestFilter(registrationId));

			final MobileServicePush push = client.getPush();

			forceRefreshSync(push, handle);

			push.register(handle, new String[] { "tag1" }, new RegistrationCallback() {

				@Override
				public void onRegister(Registration registration, Exception exception) {
					if (exception != null) {
						container.exception = exception;
						container.storedRegistrationId = sharedPreferences.getString(
								STORAGE_PREFIX + REGISTRATION_NAME_STORAGE_KEY + DEFAULT_REGISTRATION_NAME, null);
					}

					latch.countDown();
				}
			});

			latch.await();

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

	public void testRegisterFailTemplate() throws Throwable {
		try {
			final CountDownLatch latch = new CountDownLatch(1);

			Context context = getInstrumentation().getTargetContext();
			final SharedPreferences sharedPreferences = PreferenceManager.getDefaultSharedPreferences(context.getApplicationContext());

			final Container container = new Container();
			final String handle = "handle";
			final String templateName = "templateName";

			String registrationId = "registrationId";

			MobileServiceClient client = new MobileServiceClient(appUrl, appKey, context);

			client = client.withFilter(getUpsertFailTestFilter(registrationId));

			final MobileServicePush push = client.getPush();

			forceRefreshSync(push, handle);

			push.registerTemplate(handle, templateName, "{ }", new String[] { "tag1" }, new TemplateRegistrationCallback() {

				@Override
				public void onRegister(TemplateRegistration registration, Exception exception) {
					if (exception != null) {
						container.exception = exception;
						container.storedRegistrationId = sharedPreferences.getString(STORAGE_PREFIX + REGISTRATION_NAME_STORAGE_KEY + templateName, null);
					}

					latch.countDown();
				}
			});

			latch.await();

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

	public void testReRegisterNative() throws Throwable {
		try {
			final CountDownLatch latch = new CountDownLatch(1);

			Context context = getInstrumentation().getTargetContext();
			final SharedPreferences sharedPreferences = PreferenceManager.getDefaultSharedPreferences(context.getApplicationContext());

			final Container container = new Container();
			final String handle = "handle";

			String registrationId1 = "registrationId1";
			String registrationId2 = "registrationId2";

			String[] tags1 = new String[] { "tag1" };
			final String[] tags2 = new String[] { "tag2" };

			MobileServiceClient client = new MobileServiceClient(appUrl, appKey, context);

			MobileServiceClient registrationclient = client.withFilter(getUpsertTestFilter(registrationId1));
			MobileServiceClient reRegistrationclient = client.withFilter(getUpsertTestFilter(registrationId2));

			final MobileServicePush registrationPush = registrationclient.getPush();
			final MobileServicePush reRegistrationPush = reRegistrationclient.getPush();

			forceRefreshSync(registrationPush, handle);
			forceRefreshSync(reRegistrationPush, handle);

			registrationPush.register(handle, tags1, new RegistrationCallback() {

				@Override
				public void onRegister(Registration registration, Exception exception) {
					if (exception != null) {
						container.exception = exception;

						latch.countDown();
					} else {
						reRegistrationPush.register(handle, tags2, new RegistrationCallback() {

							@Override
							public void onRegister(Registration registration, Exception exception) {
								if (exception != null) {
									container.exception = exception;
								} else {
									container.storedRegistrationId = sharedPreferences.getString(STORAGE_PREFIX + REGISTRATION_NAME_STORAGE_KEY
											+ DEFAULT_REGISTRATION_NAME, null);
									container.tags = registration.getTags();
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
				Assert.assertEquals(registrationId2, container.storedRegistrationId);
				Assert.assertTrue(matchTags(tags2, container.tags));
			}
		} catch (Exception e) {
			e.printStackTrace();
		}
	}

	public void testReRegisterTemplate() throws Throwable {
		try {
			final CountDownLatch latch = new CountDownLatch(1);

			Context context = getInstrumentation().getTargetContext();
			final SharedPreferences sharedPreferences = PreferenceManager.getDefaultSharedPreferences(context.getApplicationContext());

			final Container container = new Container();
			final String handle = "handle";
			final String templateName = "templateName";

			String registrationId1 = "registrationId1";
			String registrationId2 = "registrationId2";

			String[] tags1 = new String[] { "tag1" };
			final String[] tags2 = new String[] { "tag2" };

			String templateBody1 = "\"data\"={\"text\"=\"$message1\"}";
			final String templateBody2 = "\"data\"={\"text\"=\"$message2\"}";

			MobileServiceClient client = new MobileServiceClient(appUrl, appKey, context);

			MobileServiceClient registrationclient = client.withFilter(getUpsertTestFilter(registrationId1));
			MobileServiceClient reRegistrationclient = client.withFilter(getUpsertTestFilter(registrationId2));

			final MobileServicePush registrationPush = registrationclient.getPush();
			final MobileServicePush reRegistrationPush = reRegistrationclient.getPush();

			forceRefreshSync(registrationPush, handle);
			forceRefreshSync(reRegistrationPush, handle);

			registrationPush.registerTemplate(handle, templateName, templateBody1, tags1, new TemplateRegistrationCallback() {

				@Override
				public void onRegister(TemplateRegistration registration, Exception exception) {
					if (exception != null) {
						container.exception = exception;

						latch.countDown();
					} else {
						reRegistrationPush.registerTemplate(handle, templateName, templateBody2, tags2, new TemplateRegistrationCallback() {

							@Override
							public void onRegister(TemplateRegistration registration, Exception exception) {
								if (exception != null) {
									container.exception = exception;
								} else {
									container.storedRegistrationId = sharedPreferences.getString(STORAGE_PREFIX + REGISTRATION_NAME_STORAGE_KEY + templateName,
											null);
									container.tags = registration.getTags();
									container.templateBody = registration.getTemplateBody();
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
			final CountDownLatch latch = new CountDownLatch(1);

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

			registrationPush.register(handle, new String[] { "tag1" }, new RegistrationCallback() {

				@Override
				public void onRegister(Registration registration, Exception exception) {
					if (exception != null) {
						container.exception = exception;

						latch.countDown();
					} else {
						reRegistrationPush.register(handle, new String[] { "tag1" }, new RegistrationCallback() {

							@Override
							public void onRegister(Registration registration, Exception exception) {
								if (exception != null) {
									container.exception = exception;
									container.storedRegistrationId = sharedPreferences.getString(STORAGE_PREFIX + REGISTRATION_NAME_STORAGE_KEY
											+ DEFAULT_REGISTRATION_NAME, null);
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
			final CountDownLatch latch = new CountDownLatch(1);

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

			registrationPush.registerTemplate(handle, templateName, "{ }", new String[] { "tag1" }, new TemplateRegistrationCallback() {

				@Override
				public void onRegister(TemplateRegistration registration, Exception exception) {
					if (exception != null) {
						container.exception = exception;

						latch.countDown();
					} else {
						reRegistrationPush.registerTemplate(handle, templateName, "{ }", new String[] { "tag1" }, new TemplateRegistrationCallback() {

							@Override
							public void onRegister(TemplateRegistration registration, Exception exception) {
								if (exception != null) {
									container.exception = exception;
									container.storedRegistrationId = sharedPreferences.getString(STORAGE_PREFIX + REGISTRATION_NAME_STORAGE_KEY + templateName,
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
			final CountDownLatch latch = new CountDownLatch(1);
			final Container container = new Container();

			Context context = getInstrumentation().getTargetContext();

			MobileServiceClient client = new MobileServiceClient(appUrl, appKey, context);
			client.getPush().register("", new String[] { "tag1" }, new RegistrationCallback() {

				@Override
				public void onRegister(Registration registration, Exception exception) {
					if (exception != null) {
						container.exception = exception;
					}

					latch.countDown();
				}
			});

			latch.await();

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
			final CountDownLatch latch = new CountDownLatch(1);
			final Container container = new Container();

			Context context = getInstrumentation().getTargetContext();

			MobileServiceClient client = new MobileServiceClient(appUrl, appKey, context);
			client.getPush().registerTemplate("", "template1", "{\"data\"={\"text\"=\"$message\"}}", new String[] { "tag1" },
					new TemplateRegistrationCallback() {

						@Override
						public void onRegister(TemplateRegistration registration, Exception exception) {
							if (exception != null) {
								container.exception = exception;
							}

							latch.countDown();
						}
					});

			latch.await();

			// Asserts
			Exception exception = container.exception;

			if (!(exception instanceof IllegalArgumentException)) {
				fail("Expected Exception IllegalArgumentException");
			}
		} catch (Exception e) {
			e.printStackTrace();
		}
	}

	public void testRegisterTemplateEmptyTemplateName() throws Throwable {
		try {
			final CountDownLatch latch = new CountDownLatch(1);
			final Container container = new Container();

			Context context = getInstrumentation().getTargetContext();

			MobileServiceClient client = new MobileServiceClient(appUrl, appKey, context);
			client.getPush().registerTemplate(UUID.randomUUID().toString(), "", "{\"data\"={\"text\"=\"$message\"}}", new String[] { "tag1" },
					new TemplateRegistrationCallback() {

						@Override
						public void onRegister(TemplateRegistration registration, Exception exception) {
							if (exception != null) {
								container.exception = exception;
							}

							latch.countDown();
						}
					});

			latch.await();

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
			final CountDownLatch latch = new CountDownLatch(1);
			final Container container = new Container();

			Context context = getInstrumentation().getTargetContext();

			MobileServiceClient client = new MobileServiceClient(appUrl, appKey, context);
			client.getPush().registerTemplate(UUID.randomUUID().toString(), "template1", "", new String[] { "tag1" }, new TemplateRegistrationCallback() {

				@Override
				public void onRegister(TemplateRegistration registration, Exception exception) {
					if (exception != null) {
						container.exception = exception;
					}

					latch.countDown();
				}
			});

			latch.await();

			// Asserts
			Exception exception = container.exception;

			if (!(exception instanceof IllegalArgumentException)) {
				fail("Expected Exception IllegalArgumentException");
			}
		} catch (Exception e) {
			e.printStackTrace();
		}
	}

	private static void forceRefreshSync(MobileServicePush push, String handle) throws InterruptedException {
		final CountDownLatch unregisterLatch = new CountDownLatch(1);

		// Called to force refresh
		push.unregisterAll(handle, new UnregisterCallback() {

			@Override
			public void onUnregister(Exception exception) {
				unregisterLatch.countDown();
			}
		});

		unregisterLatch.await();
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

	// Test Filter

	private static ServiceFilter getUpsertTestFilter(final String registrationId) {
		return new ServiceFilter() {

			@Override
			public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
					ServiceFilterResponseCallback responseCallback) {
				ServiceFilterResponseMock response = new ServiceFilterResponseMock();
				response.setStatus(new StatusLineMock(400));

				final String url = request.getUrl();
				String method = request.getMethod();

				if (method == "POST" && url.contains("registrationids/")) {
					response = new ServiceFilterResponseMock();
					response.setStatus(new StatusLineMock(201));
					response.setHeaders(new Header[] { new Header() {

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
					} });
				} else if (method == "PUT" && url.contains("registrations/" + registrationId)) {
					response = new ServiceFilterResponseMock();
					response.setStatus(new StatusLineMock(204));
				} else if (method == "PUT" && url.contains("registrations/")) {
					response = new ServiceFilterResponseMock();
					response.setStatus(new StatusLineMock(410));
				} else if (method == "DELETE" && url.contains("registrations/" + registrationId)) {
					response = new ServiceFilterResponseMock();
					response.setStatus(new StatusLineMock(204));
				} else if (method == "GET" && url.contains("registrations/")) {
					response = new ServiceFilterResponseMock();
					response.setStatus(new StatusLineMock(200));
					response.setContent("[ ]");
				}

				// create a mock request to replace the existing one
				ServiceFilterRequestMock requestMock = new ServiceFilterRequestMock(response);
				nextServiceFilterCallback.onNext(requestMock, responseCallback);
			}
		};
	}

	private static ServiceFilter getUpsertFailTestFilter(final String registrationId) {
		return new ServiceFilter() {

			@Override
			public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
					ServiceFilterResponseCallback responseCallback) {
				ServiceFilterResponseMock response = new ServiceFilterResponseMock();
				response.setStatus(new StatusLineMock(400));

				final String url = request.getUrl();
				String method = request.getMethod();

				if (method == "POST" && url.contains("registrationids/")) {
					response = new ServiceFilterResponseMock();
					response.setStatus(new StatusLineMock(201));
					response.setHeaders(new Header[] { new Header() {

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
					} });
				} else if (method == "PUT" && url.contains("registrations/")) {
					response = new ServiceFilterResponseMock();
					response.setStatus(new StatusLineMock(410));
				} else if (method == "DELETE" && url.contains("registrations/" + registrationId)) {
					response = new ServiceFilterResponseMock();
					response.setStatus(new StatusLineMock(204));
				} else if (method == "GET" && url.contains("registrations/")) {
					response = new ServiceFilterResponseMock();
					response.setStatus(new StatusLineMock(200));
					response.setContent("[ ]");
				}

				// create a mock request to replace the existing one
				ServiceFilterRequestMock requestMock = new ServiceFilterRequestMock(response);
				nextServiceFilterCallback.onNext(requestMock, responseCallback);
			}
		};
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
