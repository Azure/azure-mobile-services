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

import java.net.MalformedURLException;
import java.util.concurrent.CountDownLatch;

import junit.framework.Assert;

import org.apache.http.Header;
import org.apache.http.protocol.HTTP;

import android.os.Build;
import android.test.InstrumentationTestCase;

import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.microsoft.windowsazure.mobileservices.MobileServiceAuthenticationProvider;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceJsonTable;
import com.microsoft.windowsazure.mobileservices.MobileServiceTable;
import com.microsoft.windowsazure.mobileservices.MobileServiceUser;
import com.microsoft.windowsazure.mobileservices.NextServiceFilterCallback;
import com.microsoft.windowsazure.mobileservices.ServiceFilter;
import com.microsoft.windowsazure.mobileservices.ServiceFilterRequest;
import com.microsoft.windowsazure.mobileservices.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.ServiceFilterResponseCallback;
import com.microsoft.windowsazure.mobileservices.TableDeleteCallback;
import com.microsoft.windowsazure.mobileservices.TableJsonOperationCallback;
import com.microsoft.windowsazure.mobileservices.TableJsonQueryCallback;
import com.microsoft.windowsazure.mobileservices.UserAuthenticationCallback;

public class MobileServiceClientTests extends InstrumentationTestCase {
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

	public void testNewMobileServiceClientShouldReturnMobileServiceClient() throws MalformedURLException {
		MobileServiceClient client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
		assertEquals(appUrl, client.getAppUrl().toString());
		assertEquals(appKey, client.getAppKey());
	}

	public void testNewMobileServiceClientWithEmptyAppUrlShouldThrowException() {
		try {
			new MobileServiceClient("", appKey, getInstrumentation().getTargetContext());
			fail("Expected Exception MalformedURLException");
		} catch (MalformedURLException e) {
			// do nothing, it's OK
		}
	}

	public void testNewMobileServiceClientWithNullAppUrlShouldThrowException() {
		try {
			new MobileServiceClient((String) null, appKey, getInstrumentation().getTargetContext());
			fail("Expected Exception MalformedURLException");
		} catch (MalformedURLException e) {
			// do nothing, it's OK
		}
	}

	public void testNewMobileServiceClientWithEmptyAppKeyShouldThrowException() {
		try {
			new MobileServiceClient(appUrl, "", getInstrumentation().getTargetContext());
			fail("Expected Exception MalformedURLException");
		} catch (IllegalArgumentException e) {
			// do nothing, it's OK
		} catch (MalformedURLException e) {
			fail("This should not happen");
		}
	}

	public void testNewMobileServiceClientWithNullAppKeyShouldThrowException() {
		try {
			new MobileServiceClient(appUrl, null, getInstrumentation().getTargetContext());
			fail("Expected Exception MalformedURLException");
		} catch (IllegalArgumentException e) {
			// do nothing, it's OK
		} catch (MalformedURLException e) {
			fail("This should not happen");
		}
	}

	public void testMobileServiceClientWithNullServiceFilterShouldThrowException() {
		MobileServiceClient client = null;
		try {
			client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
		} catch (MalformedURLException e1) {
			fail("This should not happen");
		}

		try {
			client.withFilter(null);
			fail("Expected Exception IllegalArgumentException");
		} catch (IllegalArgumentException e) {
			// do nothing, it's OK
		}
	}

	public void testIsLoginInProgressShouldReturnFalse() throws MalformedURLException {
		MobileServiceClient client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
		assertFalse(client.isLoginInProgress());
	}

	public void testSetAndGetCurrentUserShouldReturnUser() throws MalformedURLException {
		String userId = "myUserId";
		MobileServiceUser user = new MobileServiceUser(userId);
		MobileServiceClient client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());

		client.setCurrentUser(user);
		MobileServiceUser currentUser = client.getCurrentUser();

		assertEquals(user, currentUser);
		assertEquals(userId, user.getUserId());
		assertEquals(userId, currentUser.getUserId());
	}

	public void testGetCurrentUserWithNoLoggedUserShouldReturnNull() throws MalformedURLException {
		MobileServiceClient client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
		MobileServiceUser currentUser = client.getCurrentUser();

		assertNull(currentUser);
	}

	public void testGetTableShouldReturnTableWithGivenNameAndClient() throws MalformedURLException {
		MobileServiceClient client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
		MobileServiceJsonTable table = client.getTable("MyTable");

		assertNotNull(table);
	}

	public void testGetTableWithEmptyNameShouldThrowException() {
		MobileServiceClient client = null;
		try {
			client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
		} catch (MalformedURLException e1) {
			fail("This should not fail");
		}

		try {
			client.getTable("");
			fail("Expected Exception IllegalArgumentException");
		} catch (IllegalArgumentException e) {
			// do nothing, it's OK
		}
	}

	public void testGetTableWithWhiteSpacedNameShouldThrowException() {
		MobileServiceClient client = null;
		try {
			client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
		} catch (MalformedURLException e1) {
			fail("This should not fail");
		}

		try {

			client.getTable(" ");
			fail("Expected Exception IllegalArgumentException");
		} catch (IllegalArgumentException e) {
			// do nothing, it's OK
		}
	}

	public void testGetTableWithNullNameShouldThrowException() {
		MobileServiceClient client = null;
		try {
			client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
		} catch (MalformedURLException e1) {
			fail("This should not happen");
		}
		try {
			client.getTable((String) null);
			fail("Expected Exception IllegalArgumentException");
		} catch (IllegalArgumentException e) {
			// do nothing, it's OK
		}
	}
	
	public void testGetTableWithClassWithIdMemberShouldWork() {
		MobileServiceClient client = null;
		try {
			client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
		} catch (MalformedURLException e1) {
			fail("This should not happen");
		}
		
		MobileServiceTable<PersonTestObject> table = client.getTable(PersonTestObject.class);
		assertEquals("PersonTestObject", table.getTableName());
	}
	
	public void testGetTableWithClassWithIdAnnotationShouldWork() {
		MobileServiceClient client = null;
		try {
			client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
		} catch (MalformedURLException e1) {
			fail("This should not happen");
		}
		
		MobileServiceTable<IdPropertyWithGsonAnnotation> table = client.getTable(IdPropertyWithGsonAnnotation.class);
		assertEquals("IdPropertyWithGsonAnnotation", table.getTableName());
	}
	
	public void testGetTableWithClassWithoutIdShouldFail() {
		MobileServiceClient client = null;
		try {
			client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
		} catch (MalformedURLException e1) {
			fail("This should not happen");
		}
		
		try {
			@SuppressWarnings("unused")
			MobileServiceTable<PersonTestObjectWithoutId> table = client.getTable(PersonTestObjectWithoutId.class);
			fail("This should not happen");
		} catch (IllegalArgumentException e) {
			// do nothing, it's OK
		}
	}
	
	public void testGetTableWithClassWithMultipleIdPropertiesShouldFail() throws Throwable {
		String tableName = "MyTableName";
		MobileServiceClient client = null;

		try {
			client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
		} catch (MalformedURLException e) {
			fail("This should not happen");
		}

		try {
			// Get the MobileService table
			client.getTable(tableName, IdPropertyMultipleIdsTestObject.class);
			fail("This should fail");
		} catch (IllegalArgumentException e) {
			// It's ok
		}
		
	}
	
	public void testGetTableWithInterfaceShouldThrowException() {
		MobileServiceClient client = null;
		try {
			client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
		} catch (MalformedURLException e1) {
			fail("This should not fail");
		}

		try {

			client.getTable(MyInterface.class);
			fail("Expected Exception IllegalArgumentException");
		} catch (IllegalArgumentException e) {
			// do nothing, it's OK
		}
	}
	
	public void testGetTableWithAbstractClassShouldThrowException() {
		MobileServiceClient client = null;
		try {
			client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
		} catch (MalformedURLException e1) {
			fail("This should not fail");
		}

		try {

			client.getTable(MyAbstractClass.class);
			fail("Expected Exception IllegalArgumentException");
		} catch (IllegalArgumentException e) {
			// do nothing, it's OK
		}
	}
	
	interface MyInterface 
	{

	}
	
	abstract class MyAbstractClass
	{
		int id;
		String name;
	}

	public void testLoginShouldParseJsonUserCorreclty() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String userId = "id";
		final String userToken = "userToken";
		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;
				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {
						// User JSon Template
						String userJsonTemplate = "{\"user\":{\"userId\":\"%s\"}, \"authenticationToken\":\"%s\"}";
						// Create a mock response simulating an error
						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setStatus(new StatusLineMock(200));
						response.setContent(String.format(userJsonTemplate, userId, userToken));

						// create a mock request to replace the existing one
						ServiceFilterRequestMock requestMock = new ServiceFilterRequestMock(response);
						nextServiceFilterCallback.onNext(requestMock, responseCallback);
					}
				});

				try {
					client.login(MobileServiceAuthenticationProvider.Facebook, "oAuthToken", new UserAuthenticationCallback() {

						@Override
						public void onCompleted(MobileServiceUser user, Exception exception, ServiceFilterResponse response) {
							container.setUser(user);
							latch.countDown();
						}
					});
				} catch (Exception e) {
					latch.countDown();
				}
			}
		});

		latch.await();

		// Asserts
		MobileServiceUser user = container.getUser();
		assertNotNull(user);
		assertEquals(userId, user.getUserId());
		assertEquals(userToken, user.getAuthenticationToken());
	}

	public void testOperationShouldAddHeaders() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				// Create client
				MobileServiceClient client = null;
				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				final String expectedAppKey = client.getAppKey();

				// Add a new filter to the client
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						int zumoInstallationHeaderIndex = -1;
						int zumoAppHeaderIndex = -1;
						int userAgentHeaderIndex = -1;
						int acceptHeaderIndex = -1;

						String installationHeader = "X-ZUMO-INSTALLATION-ID";
						String appHeader = "X-ZUMO-APPLICATION";
						String userAgentHeader = HTTP.USER_AGENT;
						String acceptHeader = "Accept";

						Header[] headers = request.getHeaders();
						for (int i = 0; i < headers.length; i++) {
							if (headers[i].getName() == installationHeader) {
								zumoInstallationHeaderIndex = i;
							} else if (headers[i].getName() == appHeader) {
								zumoAppHeaderIndex = i;
							} else if (headers[i].getName() == userAgentHeader) {
								userAgentHeaderIndex = i;
							} else if (headers[i].getName() == acceptHeader) {
								acceptHeaderIndex = i;
							}
						}

						if (zumoInstallationHeaderIndex == -1) {
							Assert.fail();
						}
						if (zumoAppHeaderIndex == -1) {
							Assert.fail();
						}
						if (userAgentHeaderIndex == -1) {
							Assert.fail();
						}
						if (acceptHeaderIndex == -1) {
							Assert.fail();
						}

						String expectedUserAgent = String.format("ZUMO/%s (lang=%s; os=%s; os_version=%s; arch=%s)", "1.0", "Java", "Android",
								Build.VERSION.RELEASE, Build.CPU_ABI);

						assertNotNull(headers[zumoInstallationHeaderIndex].getValue());
						assertEquals(expectedAppKey, headers[zumoAppHeaderIndex].getValue());
						assertEquals(expectedUserAgent, headers[userAgentHeaderIndex].getValue());
						assertEquals("application/json", headers[acceptHeaderIndex].getValue());

						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setContent("{}");

						responseCallback.onResponse(response, null);
					}
				});

				client.getTable("dummy").execute(new TableJsonQueryCallback() {

					@Override
					public void onCompleted(JsonElement result, int count, Exception exception, ServiceFilterResponse response) {
						latch.countDown();
					}
				});
			}
		});

		latch.await();
	}
	
	public void testInsertUpdateShouldAddContentTypeJson() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(2);

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				// Create client
				MobileServiceClient client = null;
				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a new filter to the client
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						Header[] headers = request.getHeaders();
						
						boolean headerPresent = false;
						for (int i = 0; i < headers.length; i++) {
							if (headers[i].getName().equals(HTTP.CONTENT_TYPE) && headers[i].getValue().equals("application/json")) {
								headerPresent = true;
							}
						}

						assertTrue("Header not present", headerPresent);

						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setContent("{}");

						responseCallback.onResponse(response, null);
					}
				});

				JsonObject jsonObject = new JsonObject();
				jsonObject.addProperty("id", 42);
				
				client.getTable("dummy").insert(jsonObject, new TableJsonOperationCallback() {
					
					@Override
					public void onCompleted(JsonObject jsonObject, Exception exception,
							ServiceFilterResponse response) {
						latch.countDown();
					}
				});
				
				client.getTable("dummy").update(jsonObject, new TableJsonOperationCallback() {
					
					@Override
					public void onCompleted(JsonObject jsonObject, Exception exception,
							ServiceFilterResponse response) {
						latch.countDown();
					}
				});
			}
		});

		latch.await();
	}
	

	public void testDeleteQueryShouldNotAddContentTypeJson() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(2);

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				// Create client
				MobileServiceClient client = null;
				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a new filter to the client
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						Header[] headers = request.getHeaders();
						
						boolean headerPresent = false;
						for (int i = 0; i < headers.length; i++) {
							if (headers[i].getName().equals(HTTP.CONTENT_TYPE) && headers[i].getValue().equals("application/json")) {
								headerPresent = true;
							}
						}

						assertFalse("Header is present", headerPresent);

						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setContent("{}");

						responseCallback.onResponse(response, null);
					}
				});

				client.getTable("dummy").delete(42, new TableDeleteCallback() {
					
					@Override
					public void onCompleted(Exception exception, ServiceFilterResponse response) {
						latch.countDown();
					}
				});
				
				client.getTable("dummy").execute(new TableJsonQueryCallback() {
					
					@Override
					public void onCompleted(JsonElement result, int count, Exception exception,
							ServiceFilterResponse response) {
						latch.countDown();
					}
				});
			}
		});

		latch.await();
	}

}
