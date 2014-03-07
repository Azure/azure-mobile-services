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
import java.net.URLEncoder;
import java.util.List;
import java.util.Locale;
import java.util.concurrent.CountDownLatch;

import junit.framework.Assert;

import org.apache.http.ProtocolVersion;
import org.apache.http.StatusLine;

import android.test.InstrumentationTestCase;

import com.google.gson.GsonBuilder;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceException;
import com.microsoft.windowsazure.mobileservices.MobileServiceJsonTable;
import com.microsoft.windowsazure.mobileservices.MobileServiceTable;
import com.microsoft.windowsazure.mobileservices.NextServiceFilterCallback;
import com.microsoft.windowsazure.mobileservices.QueryOrder;
import com.microsoft.windowsazure.mobileservices.ServiceFilter;
import com.microsoft.windowsazure.mobileservices.ServiceFilterRequest;
import com.microsoft.windowsazure.mobileservices.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.ServiceFilterResponseCallback;
import com.microsoft.windowsazure.mobileservices.TableDeleteCallback;
import com.microsoft.windowsazure.mobileservices.TableJsonOperationCallback;
import com.microsoft.windowsazure.mobileservices.TableJsonQueryCallback;
import com.microsoft.windowsazure.mobileservices.TableOperationCallback;
import com.microsoft.windowsazure.mobileservices.TableQueryCallback;

public class MobileServiceTableTests extends InstrumentationTestCase {
	String appUrl = "";
	String appKey = "";
	GsonBuilder gsonBuilder;

	protected void setUp() throws Exception {
		appUrl = "http://myapp.com/";
		appKey = "qwerty";
		gsonBuilder = new GsonBuilder();
		super.setUp();
	}

	protected void tearDown() throws Exception {
		super.tearDown();
	}

	public void testNewMobileServiceTableShouldReturnMobileServiceTable() throws MalformedURLException {
		String tableName = "MyTableName";
		MobileServiceClient client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
		MobileServiceTable<Object> msTable = new MobileServiceTable<Object>(tableName, client, Object.class);

		assertEquals(tableName, msTable.getTableName());

	}

	public void testNewMobileServiceTableWithNameFromClassShouldReturnMobileServiceTable() throws MalformedURLException {
		String tableName = "PersonTestObject";
		MobileServiceClient client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
		MobileServiceTable<PersonTestObject> msTable = client.getTable(PersonTestObject.class);

		assertEquals(tableName.toLowerCase(Locale.getDefault()), msTable.getTableName().toLowerCase(Locale.getDefault()));

	}

	public void testNewMobileServiceTableWithNullNameShouldThrowException() {
		MobileServiceClient client = null;
		try {
			client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
		} catch (MalformedURLException e1) {
			fail("This should not happen");
		}
		try {
			new MobileServiceTable<Object>(null, client, Object.class);
			fail("Expected Exception IllegalArgumentException");
		} catch (IllegalArgumentException e) {
			// do nothing, it's OK
		}
	}

	public void testNewMobileServiceTableWithEmptyNameShouldThrowException() {
		MobileServiceClient client = null;
		try {
			client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
		} catch (MalformedURLException e1) {
			fail("This should not happen");
		}
		try {
			new MobileServiceTable<Object>("", client, Object.class);
			fail("Expected Exception IllegalArgumentException");
		} catch (IllegalArgumentException e) {
			// do nothing, it's OK
		}
	}

	public void testNewMobileServiceTableWithWhiteSpacedNameShouldThrowException() {
		MobileServiceClient client = null;
		try {
			client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
		} catch (MalformedURLException e1) {
			fail("This should not happen");
		}
		try {
			new MobileServiceTable<Object>(" ", client, Object.class);
			fail("Expected Exception IllegalArgumentException");
		} catch (IllegalArgumentException e) {
			// do nothing, it's OK
		}
	}

	public void testNewMobileServiceTableWithNullClientShouldThrowException() {
		try {
			new MobileServiceTable<Object>("MyTableName", null, Object.class);
			fail("Expected Exception IllegalArgumentException");
		} catch (IllegalArgumentException e) {
			// do nothing, it's OK
		}
	}

	public void testWhereWithNullQueryShouldThrowException() {
		MobileServiceClient client = null;
		MobileServiceTable<Object> msTable = null;
		try {
			client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
			String tableName = "MyTableName";
			msTable = new MobileServiceTable<Object>(tableName, client, Object.class);
		} catch (MalformedURLException e1) {
			fail("This should not happen");
		}
		try {
			msTable.where(null);
			fail("Expected Exception IllegalArgumentException");
		} catch (IllegalArgumentException e) {
			// do nothing, it's OK
		}
	}

	public void testShouldThrowExceptionIfObjectDoesNotHaveIdProperty() throws Throwable {
		
		String tableName = "MyTableName";
		MobileServiceClient client = null;

		try {
			client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
		} catch (MalformedURLException e) {
			e.printStackTrace();
		}

		try {
			// Create get the MobileService table
			@SuppressWarnings("unused")
			MobileServiceTable<PersonTestObjectWithoutId> msTable = client.getTable(tableName, PersonTestObjectWithoutId.class);
			fail("The getTable invokation should fail");
		}
		catch (IllegalArgumentException e) {
			// It's ok.
		}
		
	}

	public void testInsertShouldThrowExceptionIfObjectHasIdPropertyDifferentThanZero() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store the object after the insertion, we need this to do
		// the asserts outside the onSuccess method
		final ResultsContainer container = new ResultsContainer();

		// Object to insert
		final JsonObject testObject = new JsonObject();

		testObject.addProperty("name", "john");
		testObject.addProperty("ID", 38);
		
		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {

				String tableName = "MyTableName";
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Create get the MobileService table
				MobileServiceJsonTable msTable = client.getTable(tableName);

				// Call the insert method
				msTable.insert(testObject, new TableJsonOperationCallback() {

					@Override
					public void onCompleted(JsonObject entity, Exception exception, ServiceFilterResponse response) {
						container.setErrorMessage(exception.getMessage());
						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		assertEquals("The entity to insert should not have a numeric ID property defined.", container.getErrorMessage());
		assertNull(container.getPersonWithoutId());
	}
	
	public void testInsertShouldReturnEntityWithId() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store the object after the insertion, we need this to do
		// the asserts outside the onSuccess method
		final ResultsContainer container = new ResultsContainer();

		// Object to insert
		final PersonTestObject person = new PersonTestObject("John", "Doe", 29);

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {

				String tableName = "MyTableName";
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setContent("{\"id\":38, \"firstName\":\"John\", \"lastName\":\"Foo\", \"Age\":29}");
						// call onResponse with the mocked response
						responseCallback.onResponse(response, null);
					}
				});

				// Create get the MobileService table
				MobileServiceTable<PersonTestObject> msTable = client.getTable(tableName, PersonTestObject.class);

				// Call the insert method
				msTable.insert(person, new TableOperationCallback<PersonTestObject>() {

					@Override
					public void onCompleted(PersonTestObject entity, Exception exception, ServiceFilterResponse response) {
						container.setPerson(entity);
						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		PersonTestObject p = container.getPerson();
		Assert.assertNotNull("Person expected", p);
		Assert.assertEquals(38, p.getId());
		Assert.assertEquals(person.getFirstName(), p.getFirstName());
		Assert.assertEquals("Foo", p.getLastName());
		Assert.assertEquals(person.getAge(), p.getAge());
	}
	
	public void testInsertShouldReturnTheSameMutatedObject() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store the object after the insertion, we need this to do
		// the asserts outside the onSuccess method
		final ResultsContainer container = new ResultsContainer();

		// Object to insert
		final PersonTestObject person = new PersonTestObject("John", "Doe", 29);

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {

				String tableName = "MyTableName";
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setContent("{\"id\":38, \"firstName\":\"John\", \"lastName\":\"Foo\", \"Age\":29}");
						// call onResponse with the mocked response
						responseCallback.onResponse(response, null);
					}
				});

				// Create get the MobileService table
				MobileServiceTable<PersonTestObject> msTable = client.getTable(tableName, PersonTestObject.class);

				// Call the insert method
				msTable.insert(person, new TableOperationCallback<PersonTestObject>() {

					@Override
					public void onCompleted(PersonTestObject entity, Exception exception, ServiceFilterResponse response) {
						container.setPerson(entity);
						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		PersonTestObject p = container.getPerson();
		Assert.assertNotNull("Person expected", p);
		Assert.assertEquals(person, p);
		Assert.assertEquals(38, p.getId());
		Assert.assertEquals("John", p.getFirstName());
		Assert.assertEquals("Foo", p.getLastName());
		Assert.assertEquals(29, p.getAge());
	}
	
	public void testUpdateShouldReturnTheSameMutatedObject() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store the object after the insertion, we need this to do
		// the asserts outside the onSuccess method
		final ResultsContainer container = new ResultsContainer();

		// Object to insert
		final PersonTestObject person = new PersonTestObject("John", "Doe", 29);
		person.setId(38);

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {

				String tableName = "MyTableName";
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setContent("{\"id\":38, \"firstName\":\"John\", \"lastName\":\"Foo\", \"Age\":29}");
						// call onResponse with the mocked response
						responseCallback.onResponse(response, null);
					}
				});

				// Create get the MobileService table
				MobileServiceTable<PersonTestObject> msTable = client.getTable(tableName, PersonTestObject.class);

				// Call the insert method
				msTable.update(person, new TableOperationCallback<PersonTestObject>() {

					@Override
					public void onCompleted(PersonTestObject entity, Exception exception, ServiceFilterResponse response) {
						container.setPerson(entity);
						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		PersonTestObject p = container.getPerson();
		Assert.assertNotNull("Person expected", p);
		Assert.assertEquals(person, p);
		Assert.assertEquals(38, p.getId());
		Assert.assertEquals("John", p.getFirstName());
		Assert.assertEquals("Foo", p.getLastName());
		Assert.assertEquals(29, p.getAge());
	}

	public void testInsertShouldReturnEntityWithIdWhenUsingAnnotationsForIdProperty() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store the object after the insertion, we need this to do
		// the asserts outside the onSuccess method
		final ResultsContainer container = new ResultsContainer();

		// Object to insert
		final IdPropertyWithGsonAnnotation testObject = new IdPropertyWithGsonAnnotation("John");

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {

				String tableName = "MyTableName";
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setContent("{\"id\":38, \"name\":\"John\"}");
						// call onResponse with the mocked response
						responseCallback.onResponse(response, null);
					}
				});

				// Create get the MobileService table
				MobileServiceTable<IdPropertyWithGsonAnnotation> msTable = client.getTable(tableName, IdPropertyWithGsonAnnotation.class);

				// Call the insert method
				msTable.insert(testObject, new TableOperationCallback<IdPropertyWithGsonAnnotation>() {

					@Override
					public void onCompleted(IdPropertyWithGsonAnnotation entity, Exception exception, ServiceFilterResponse response) {
						container.setIdPropertyWithGsonAnnotation(entity);
						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		IdPropertyWithGsonAnnotation o = container.getIdPropertyWithGsonAnnotation();
		Assert.assertNotNull("Entity expected", o);
		Assert.assertEquals(38, o.getId());
		Assert.assertEquals(testObject.getName(), o.getName());
	}

	public void testInsertShouldReturnEntityWithIdWhenUsingDifferentCasingForIdProperty() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store the object after the insertion, we need this to do
		// the asserts outside the onSuccess method
		final ResultsContainer container = new ResultsContainer();

		// Object to insert
		final IdPropertyWithDifferentIdPropertyCasing testObject = new IdPropertyWithDifferentIdPropertyCasing("John");

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {

				String tableName = "MyTableName";
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setContent("{\"id\":38, \"name\":\"John\"}");
						// call onResponse with the mocked response
						responseCallback.onResponse(response, null);
					}
				});

				// Create get the MobileService table
				MobileServiceTable<IdPropertyWithDifferentIdPropertyCasing> msTable = client.getTable(tableName, IdPropertyWithDifferentIdPropertyCasing.class);

				// Call the insert method
				msTable.insert(testObject, new TableOperationCallback<IdPropertyWithDifferentIdPropertyCasing>() {

					@Override
					public void onCompleted(IdPropertyWithDifferentIdPropertyCasing entity, Exception exception, ServiceFilterResponse response) {
						container.setIdPropertyWithDifferentIdPropertyCasing(entity);
						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		IdPropertyWithDifferentIdPropertyCasing o = container.getIdPropertyWithDifferentIdPropertyCasing();
		Assert.assertNotNull("Entity expected", o);
		Assert.assertEquals(38, o.getId());
		Assert.assertEquals(testObject.getName(), o.getName());
	}
	
	public void testInsertShouldReturnEntityWithIdWhenUsingStringId() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store the object after the insertion, we need this to do
		// the asserts outside the onSuccess method
		final ResultsContainer container = new ResultsContainer();

		// Object to insert
		final PersonTestObjectWithStringId person = new PersonTestObjectWithStringId("John", "Doe", 29);

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {

				String tableName = "MyTableName";
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setContent("{\"id\":\"38\", \"firstName\":\"John\", \"lastName\":\"Foo\", \"Age\":29}");
						// call onResponse with the mocked response
						responseCallback.onResponse(response, null);
					}
				});

				// Create get the MobileService table
				MobileServiceTable<PersonTestObjectWithStringId> msTable = client.getTable(tableName, PersonTestObjectWithStringId.class);

				// Call the insert method
				msTable.insert(person, new TableOperationCallback<PersonTestObjectWithStringId>() {

					@Override
					public void onCompleted(PersonTestObjectWithStringId entity, Exception exception, ServiceFilterResponse response) {
						container.setPersonWithStringId(entity);
						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		PersonTestObjectWithStringId p = container.getPersonWithStringId();
		Assert.assertNotNull("Person expected", p);
		Assert.assertEquals(person, p);
		Assert.assertEquals("38", p.getId());
		Assert.assertEquals("John", p.getFirstName());
		Assert.assertEquals("Foo", p.getLastName());
		Assert.assertEquals(29, p.getAge());
	}
	
	public void testInsertShouldMutateOriginalEntity() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Object to insert
		final PersonTestObject originalPerson = new PersonTestObject("John", "Doe", 29);

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {

				String tableName = "MyTableName";
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setContent("{\"id\":38, \"firstName\":\"John\", \"lastName\":\"Foo\", \"Age\":29}");
						// call onResponse with the mocked response
						responseCallback.onResponse(response, null);
					}
				});

				// Create get the MobileService table
				MobileServiceTable<PersonTestObject> msTable = client.getTable(tableName, PersonTestObject.class);

				// Call the insert method
				msTable.insert(originalPerson, new TableOperationCallback<PersonTestObject>() {

					@Override
					public void onCompleted(PersonTestObject entity, Exception exception, ServiceFilterResponse response) {
						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Assert.assertEquals(38, originalPerson.getId());
		Assert.assertEquals("John", originalPerson.getFirstName());
		Assert.assertEquals("Foo", originalPerson.getLastName());
		Assert.assertEquals(29, originalPerson.getAge());
	}

	public void testInsertShouldReturnJSONWithId() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store the object after the insertion, we need this to do
		// the asserts outside the onSuccess method
		final ResultsContainer container = new ResultsContainer();

		// Object to insert
		final PersonTestObject person = new PersonTestObject("John", "Doe", 29);

		final JsonObject jsonPerson = gsonBuilder.create().toJsonTree(person).getAsJsonObject();
		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {

				String tableName = "MyTableName";
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new JSon
				// object with an id defined
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setContent("{\"id\":38}");
						// call onResponse with the mocked response
						responseCallback.onResponse(response, null);
					}
				});

				// Create get the MobileService table
				MobileServiceJsonTable msTable = client.getTable(tableName);

				// Call the insert method
				msTable.insert(jsonPerson, new TableJsonOperationCallback() {

					@Override
					public void onCompleted(JsonObject jsonEntity, Exception exception, ServiceFilterResponse response) {
						container.setResponseValue(jsonEntity.toString());
						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		PersonTestObject p = gsonBuilder.create().fromJson(container.getResponseValue(), PersonTestObject.class);
		Assert.assertNotNull("Person expected", p);
		Assert.assertEquals(38, p.getId());
		Assert.assertEquals(person.getFirstName(), p.getFirstName());
		Assert.assertEquals(person.getLastName(), p.getLastName());
		Assert.assertEquals(person.getAge(), p.getAge());
	}

	public void testInsertShouldRemoveIdsAndInsertIfJSONObjectHasMultipleIdProperties() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store the object after the insertion, we need this to do
		// the asserts outside the onSuccess method
		final ResultsContainer container = new ResultsContainer();

		// Object to insert
		final IdPropertyMultipleIdsTestObject element = new IdPropertyMultipleIdsTestObject("John");

		final JsonObject jsonElement = gsonBuilder.create().toJsonTree(element).getAsJsonObject();
		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {

				String tableName = "MyTableName";
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new JSon
				// object with an id defined
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setContent("{\"id\":38}");
						// call onResponse with the mocked response
						responseCallback.onResponse(response, null);
					}
				});

				// Create get the MobileService table
				MobileServiceJsonTable msTable = client.getTable(tableName);


				msTable.insert(jsonElement, new TableJsonOperationCallback() {

					@Override
					public void onCompleted(JsonObject jsonEntity, Exception exception, ServiceFilterResponse response) {
						container.setResponseValue(jsonEntity.toString());
						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		IdPropertyMultipleIdsTestObject e = gsonBuilder.create().fromJson(container.getResponseValue(), IdPropertyMultipleIdsTestObject.class);
		Assert.assertNotNull("Entity expected", e);
		Assert.assertEquals(38, e.getId());
		Assert.assertEquals("John", e.getName());

	}

	public void testOperationHandleServerErrorProperly() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {

				String tableName = "MyTableName";
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new JSon
				// object with an id defined
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setStatus(new StatusLine() {

							@Override
							public int getStatusCode() {
								return 500;
							}

							@Override
							public String getReasonPhrase() {
								return "Internal server error";
							}

							@Override
							public ProtocolVersion getProtocolVersion() {
								return null;
							}
						});

						response.setContent("{'error': 'Internal server error'}");
						// call onResponse with the mocked response
						responseCallback.onResponse(response, new MobileServiceException("ERROR"));
					}
				});

				// Create get the MobileService table
				MobileServiceJsonTable msTable = client.getTable(tableName);

				JsonObject json = new JsonParser().parse("{'myField': 'invalid value'}").getAsJsonObject();

				// Call the insert method
				try {
					msTable.insert(json, new TableJsonOperationCallback() {

						@Override
						public void onCompleted(JsonObject jsonEntity, Exception exception, ServiceFilterResponse response) {
							if (exception == null) {
								Assert.fail();
							} else {
								assertTrue(exception instanceof MobileServiceException);
							}
							latch.countDown();
						}
					});
				} catch (Exception e) {
					latch.countDown();
				}
			}
		});

		latch.await();
	}

	public void testOperationWithErrorAndNoContentShowStatusCode() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {

				String tableName = "MyTableName";
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new JSon
				// object with an id defined
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setStatus(new StatusLine() {

							@Override
							public int getStatusCode() {
								return 500;
							}

							@Override
							public String getReasonPhrase() {
								return "Internal server error";
							}

							@Override
							public ProtocolVersion getProtocolVersion() {
								return null;
							}
						});

						response.setContent((String)null);
						// call onResponse with the mocked response
						responseCallback.onResponse(
								response,
								new MobileServiceException("Error while processing request", new MobileServiceException(String.format("{'code': %d}", response
										.getStatus().getStatusCode()))));
					}
				});

				// Create get the MobileService table
				MobileServiceJsonTable msTable = client.getTable(tableName);

				JsonObject json = new JsonParser().parse("{'myField': 'invalid value'}").getAsJsonObject();

				// Call the insert method
				try {
					msTable.insert(json, new TableJsonOperationCallback() {

						@Override
						public void onCompleted(JsonObject jsonEntity, Exception exception, ServiceFilterResponse response) {
							if (exception == null) {
								Assert.fail();
							} else {
								assertTrue(exception instanceof MobileServiceException);
								Throwable cause = exception.getCause();
								assertTrue(cause.getMessage().contains("500"));
								latch.countDown();
							}

							latch.countDown();
						}
					});
				} catch (Exception e) {
					latch.countDown();
				}
			}
		});

		latch.await();
	}

	public void testUpdateShouldReturnEntityWithDifferentNameAndAge() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store the object after the update, we need this to do
		// the asserts outside the onSuccess method
		final ResultsContainer container = new ResultsContainer();

		// Object to update
		final PersonTestObject person = new PersonTestObject("John", "Doe", 29);
		person.setId(10);

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {

				String tableName = "MyTableName";
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {
						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setContent("{\"firstName\":\"Mike\", \"age\":50}");
						// call onResponse with the mocked response
						responseCallback.onResponse(response, null);
					}
				});

				// Get the MobileService table
				MobileServiceTable<PersonTestObject> msTable = client.getTable(tableName, PersonTestObject.class);

				// Call the update method
				msTable.update(person, new TableOperationCallback<PersonTestObject>() {

					@Override
					public void onCompleted(PersonTestObject entity, Exception exception, ServiceFilterResponse response) {
						container.setPerson(entity);
						latch.countDown();
					}
				});

			}
		});

		latch.await();

		// Asserts
		PersonTestObject p = container.getPerson();
		Assert.assertNotNull("Person expected", p);
		Assert.assertEquals(person.getId(), p.getId());
		Assert.assertEquals("Mike", p.getFirstName());
		Assert.assertEquals(person.getLastName(), p.getLastName());
		Assert.assertEquals(50, p.getAge());
	}

	public void testUpdateShouldMutateOriginalEntity() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Object to update
		final PersonTestObject originalPerson = new PersonTestObject("John", "Doe", 29);
		originalPerson.setId(10);

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {

				String tableName = "MyTableName";
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {
						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setContent("{\"firstName\":\"Mike\", \"age\":50, \"id\":38}");
						// call onResponse with the mocked response
						responseCallback.onResponse(response, null);
					}
				});

				// Get the MobileService table
				MobileServiceTable<PersonTestObject> msTable = client.getTable(tableName, PersonTestObject.class);

				// Call the update method
				msTable.update(originalPerson, new TableOperationCallback<PersonTestObject>() {

					@Override
					public void onCompleted(PersonTestObject entity, Exception exception, ServiceFilterResponse response) {
						latch.countDown();
					}
				});

			}
		});

		latch.await();

		// Asserts
		Assert.assertEquals(38, originalPerson.getId());
		Assert.assertEquals("Mike", originalPerson.getFirstName());
		Assert.assertEquals("Doe", originalPerson.getLastName());
		Assert.assertEquals(50, originalPerson.getAge());
	}
	
	public void testUpdateShouldMutateOriginalEntityWithStringId() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Object to update
		final PersonTestObjectWithStringId originalPerson = new PersonTestObjectWithStringId("John", "Doe", 29);
		originalPerson.setId("10");

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {

				String tableName = "MyTableName";
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {
						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setContent("{\"firstName\":\"Mike\", \"age\":50, \"id\":\"38\"}");
						// call onResponse with the mocked response
						responseCallback.onResponse(response, null);
					}
				});

				// Get the MobileService table
				MobileServiceTable<PersonTestObjectWithStringId> msTable = client.getTable(tableName, PersonTestObjectWithStringId.class);

				// Call the update method
				msTable.update(originalPerson, new TableOperationCallback<PersonTestObjectWithStringId>() {

					@Override
					public void onCompleted(PersonTestObjectWithStringId entity, Exception exception, ServiceFilterResponse response) {
						latch.countDown();
					}
				});

			}
		});

		latch.await();

		// Asserts
		Assert.assertEquals("38", originalPerson.getId());
		Assert.assertEquals("Mike", originalPerson.getFirstName());
		Assert.assertEquals("Doe", originalPerson.getLastName());
		Assert.assertEquals(50, originalPerson.getAge());
	}

	public void testUpdateShouldReturnJSONWithDifferentNameAndAge() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store the object after the update, we need this to do
		// the asserts outside the onSuccess method
		final ResultsContainer container = new ResultsContainer();

		// Object to update
		final PersonTestObject person = new PersonTestObject("John", "Doe", 29);
		person.setId(10);

		final JsonObject jsonPerson = gsonBuilder.create().toJsonTree(person).getAsJsonObject();

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {

				String tableName = "MyTableName";
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setContent("{\"firstName\":\"Mike\", \"age\":50}");
						// call onResponse with the mocked response
						responseCallback.onResponse(response, null);
					}
				});

				// Get the MobileService table
				MobileServiceJsonTable msTable = client.getTable(tableName);

				// Call the delete method
				msTable.update(jsonPerson, new TableJsonOperationCallback() {

					@Override
					public void onCompleted(JsonObject jsonEntity, Exception exception, ServiceFilterResponse response) {
						container.setResponseValue(jsonEntity.toString());
						latch.countDown();
					}
				});

			}
		});

		latch.await();

		// Asserts
		PersonTestObject p = gsonBuilder.create().fromJson(container.getResponseValue(), PersonTestObject.class);
		Assert.assertNotNull("Person expected", p);
		Assert.assertEquals(person.getId(), p.getId());
		Assert.assertEquals("Mike", p.getFirstName());
		Assert.assertEquals(person.getLastName(), p.getLastName());
		Assert.assertEquals(50, p.getAge());
	}

	public void testUpdateShouldReturnJSONWithDifferentNameWhenUsingIdWithDifferentCasing() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store the object after the update, we need this to do
		// the asserts outside the onSuccess method
		final ResultsContainer container = new ResultsContainer();

		// Object to update
		final IdPropertyWithDifferentIdPropertyCasing objectToUpdate = new IdPropertyWithDifferentIdPropertyCasing("John");
		objectToUpdate.setId(10);

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {

				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setContent("{\"id\":10,\"name\":\"Mike\"}");
						// call onResponse with the mocked response
						responseCallback.onResponse(response, null);
					}
				});

				// Get the MobileService table
				MobileServiceTable<IdPropertyWithDifferentIdPropertyCasing> msTable = client.getTable(IdPropertyWithDifferentIdPropertyCasing.class);

				// Call the delete method
				msTable.update(objectToUpdate, new TableOperationCallback<IdPropertyWithDifferentIdPropertyCasing>() {
					
					@Override
					public void onCompleted(IdPropertyWithDifferentIdPropertyCasing entity,
							Exception exception, ServiceFilterResponse response) {
						container.setIdPropertyWithDifferentIdPropertyCasing(entity);
						latch.countDown();						
					}
				});

			}
		});

		latch.await();

		// Asserts
		IdPropertyWithDifferentIdPropertyCasing o = container.getIdPropertyWithDifferentIdPropertyCasing();
		Assert.assertNotNull("Object expected", o);
		Assert.assertEquals(objectToUpdate.getId(), o.getId());
		Assert.assertEquals("Mike", o.getName());
	}

	public void testUpdateShouldThrowExceptionIfEntityHasNoValidId() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store the object after the update, we need this to do
		// the asserts outside the onSuccess method
		final ResultsContainer container = new ResultsContainer();

		// Object to update
		final PersonTestObject person = new PersonTestObject("John", "Doe", 29);
		person.setId(0);

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {

				String tableName = "MyTableName";
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Get the MobileService table
				MobileServiceTable<PersonTestObject> msTable = client.getTable(tableName, PersonTestObject.class);

				// Call the update method
				msTable.update(person, new TableOperationCallback<PersonTestObject>() {

					@Override
					public void onCompleted(PersonTestObject entity, Exception exception, ServiceFilterResponse response) {
						container.setPerson(entity);
						container.setErrorMessage(exception.getMessage());
						latch.countDown();
					}
				});

			}
		});

		latch.await();

		// Asserts
		PersonTestObject p = container.getPerson();
		Assert.assertNull("Null person expected", p);
		Assert.assertEquals("The entity has an invalid numeric value on id property.", container.getErrorMessage());
	}

	public void testDeleteUsingEntityShouldReturnTheExpectedRequestUrl() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		// Object to delete
		final PersonTestObject person = new PersonTestObject("John", "Doe", 29);
		person.setId(10);

		final String tableName = "MyTableName";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and store the request URL.
				// Send a mock response
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						container.setRequestUrl(request.getUrl());
						// call onResponse with the mocked response
						responseCallback.onResponse(new ServiceFilterResponseMock(), null);
					}
				});

				// Get the MobileService table
				MobileServiceJsonTable msTable = client.getTable(tableName);

				// Call the delete method sending the entity to delete
				msTable.delete(person, new TableDeleteCallback() {

					@Override
					public void onCompleted(Exception exception, ServiceFilterResponse response) {
						container.setOperationSucceded(exception == null);
						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Assert.assertTrue("Opperation should have succeded", container.getOperationSucceded());
		assertEquals(this.appUrl + "tables/" + tableName + "/" + person.getId(), container.getRequestUrl());
	}

	public void testDeleteUsingIdShouldReturnTheExpectedRequestUrl() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		// Object to insert
		final int personId = 10;

		final String tableName = "MyTableName";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						container.setRequestUrl(request.getUrl());
						// call onResponse with the mocked response
						responseCallback.onResponse(new ServiceFilterResponseMock(), null);
					}
				});

				// Create get the MobileService table
				MobileServiceJsonTable msTable = client.getTable(tableName);

				// Call the delete method
				msTable.delete(personId, new TableDeleteCallback() {

					@Override
					public void onCompleted(Exception exception, ServiceFilterResponse response) {
						container.setOperationSucceded(exception == null);
						latch.countDown();
					}
				});

			}
		});

		latch.await();

		// Asserts
		Assert.assertTrue("Opperation should have succeded", container.getOperationSucceded());
		assertEquals(this.appUrl + "tables/" + tableName + "/" + personId, container.getRequestUrl());
	}

	public void testDeleteUsingJSONShouldReturnTheExpectedRequestUrl() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		// Object to delete
		final PersonTestObject person = new PersonTestObject("John", "Doe", 29);
		person.setId(10);

		final JsonObject jsonPerson = gsonBuilder.create().toJsonTree(person).getAsJsonObject();

		final String tableName = "MyTableName";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and store the request URL.
				// Send a mock response
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						container.setRequestUrl(request.getUrl());
						// call onResponse with the mocked response
						responseCallback.onResponse(new ServiceFilterResponseMock(), null);
					}
				});

				// Get the MobileService table
				MobileServiceJsonTable msTable = client.getTable(tableName);

				// Call the delete method sending the entity to delete
				msTable.delete(jsonPerson, new TableDeleteCallback() {

					@Override
					public void onCompleted(Exception exception, ServiceFilterResponse response) {
						container.setOperationSucceded(exception == null);
						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Assert.assertTrue("Opperation should have succeded", container.getOperationSucceded());
		assertEquals(this.appUrl + "tables/" + tableName + "/" + person.getId(), container.getRequestUrl());
	}

	public void testSimpleQueryShouldReturnResults() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

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

						// PersonTestObject JSon template
						String personJsonTemplate = "{\"id\": %d,\"firstName\":\"%s\",\"lastName\":\"%s\",\"age\":%d}";

						// Generate JSon string with 2 objects
						String responseContent = "[";
						responseContent += String.format(personJsonTemplate, 1, "Mike", "Foo", 27) + ",";
						responseContent += String.format(personJsonTemplate, 2, "John", "Doe", 35);
						responseContent += "]";

						// Create a mock response and set the mocked JSon
						// content
						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setContent(responseContent);

						responseCallback.onResponse(response, null);
					}
				});

				client.getTable(tableName, PersonTestObject.class).execute(new TableQueryCallback<PersonTestObject>() {

					@Override
					public void onCompleted(List<PersonTestObject> result, int count, Exception exception, ServiceFilterResponse response) {
						container.setPeopleResult(result);
						container.setCount(count);
						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		List<PersonTestObject> p = container.getPeopleResult();
		assertNotNull("A list of people is expected", p);
		assertEquals(2, p.size());
		assertEquals(p.get(0).getId(), 1);
		assertEquals(p.get(1).getId(), 2);
		assertEquals(p.get(0).getLastName(), "Foo");
		assertEquals(p.get(1).getLastName(), "Doe");
		assertEquals(0, container.getCount());

	}

	public void testSimpleQueryShouldReturnJSONResults() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		// PersonTestObject JSon template
		String personJsonTemplate = "{\"id\":%d,\"firstName\":\"%s\",\"lastName\":\"%s\",\"age\":%d}";

		// Generate JSon string with 2 objects
		final String responseContent = "[" + String.format(personJsonTemplate, 1, "Mike", "Foo", 27) + ","
				+ String.format(personJsonTemplate, 2, "John", "Doe", 35) + "]";

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

						// Create a mock response and set the mocked JSon
						// content
						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setContent(responseContent);

						responseCallback.onResponse(response, null);
					}
				});

				client.getTable(tableName).execute(new TableJsonQueryCallback() {

					@Override
					public void onCompleted(JsonElement result, int count, Exception exception, ServiceFilterResponse response) {
						container.setResponseValue(result.toString());
						container.setCount(count);
						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		assertEquals(responseContent, container.getResponseValue());
		assertEquals(0, container.getCount());

	}

	public void testSimpleQueryShouldReturnEmptyArray() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

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

						// Generate JSon string with 2 objects
						String responseContent = "[]";

						// Create a mock response and set the mocked JSon
						// content
						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setContent(responseContent);

						responseCallback.onResponse(response, null);
					}
				});

				client.getTable(tableName, PersonTestObject.class).execute(new TableQueryCallback<PersonTestObject>() {

					@Override
					public void onCompleted(List<PersonTestObject> result, int count, Exception exception, ServiceFilterResponse response) {
						container.setPeopleResult(result);
						container.setCount(count);
						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		List<PersonTestObject> p = container.getPeopleResult();
		assertNotNull("A list of people is expected", p);
		assertEquals(0, p.size());
		assertEquals(0, container.getCount());

	}

	public void testSimpleJSONSelectShouldReturnEmptyJSONArray() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

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

						// Generate JSon string with 2 objects
						String responseContent = "[]";

						// Create a mock response and set the mocked JSon
						// content
						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setContent(responseContent);

						responseCallback.onResponse(response, null);
					}
				});

				client.getTable(tableName).execute(new TableJsonQueryCallback() {

					@Override
					public void onCompleted(JsonElement result, int count, Exception exception, ServiceFilterResponse response) {
						container.setResponseValue(result.toString());
						container.setCount(count);
						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		assertEquals("[]", container.getResponseValue());
		assertEquals(0, container.getCount());

	}

	public void testInlineCountSelectShouldReturnResultsWithCount() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

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
						// PersonTestObject JSon template
						String personJsonTemplate = "{\"id\": %d,\"firstName\":\"%s\",\"lastName\":\"%s\",\"age\":%d}";

						// Create string with results and count values.
						String responseContent = "{\"results\":[";
						responseContent += String.format(personJsonTemplate, 1, "Mike", "Foo", 27) + ",";
						responseContent += String.format(personJsonTemplate, 2, "John", "Doe", 35);
						responseContent += "]";
						responseContent += ",\"count\":\"15\"}";

						// Create a mock response and set the mocked JSon
						// content
						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setContent(responseContent);

						responseCallback.onResponse(response, null);
					}
				});

				client.getTable(tableName, PersonTestObject.class).execute(new TableQueryCallback<PersonTestObject>() {

					@Override
					public void onCompleted(List<PersonTestObject> result, int count, Exception exception, ServiceFilterResponse response) {
						container.setPeopleResult(result);
						container.setCount(count);
						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		List<PersonTestObject> p = container.getPeopleResult();
		assertNotNull("A list of people is expected", p);
		assertEquals(2, p.size());
		assertEquals(p.get(0).getId(), 1);
		assertEquals(p.get(1).getId(), 2);
		assertEquals(p.get(0).getLastName(), "Foo");
		assertEquals(p.get(1).getLastName(), "Doe");
		assertEquals(15, container.getCount());
	}

	public void testLookupShouldReturnAPerson() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		// Person Id
		final int personId = 4;
		final String tableName = "MyTableName";

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
						// Store the request URL
						container.setRequestUrl(request.getUrl());

						// PersonTestObject JSon template
						String personJsonTemplate = "{\"id\": %d,\"firstName\":\"%s\",\"lastName\":\"%s\",\"age\":%d}";

						// Create a mock response and set the mocked JSon
						// content
						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setContent(String.format(personJsonTemplate, 4, "John", "Doe", 35));

						responseCallback.onResponse(response, null);
					}
				});

				client.getTable(tableName, PersonTestObject.class).lookUp(personId, new TableOperationCallback<PersonTestObject>() {

					@Override
					public void onCompleted(PersonTestObject entity, Exception exception, ServiceFilterResponse response) {
						container.setPerson(entity);
						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		PersonTestObject p = container.getPerson();
		assertNotNull("A person expected", p);
		assertEquals(4, p.getId());
		assertEquals("Doe", p.getLastName());
		assertEquals(this.appUrl + "tables/" + tableName + "/" + personId, container.getRequestUrl());
	}

	public void testLookupShouldReturnAJSONPerson() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		// Person Id
		final int personId = 4;
		final String tableName = "MyTableName";

		// PersonTestObject JSon template
		String personJsonTemplate = "{\"id\":%d,\"firstName\":\"%s\",\"lastName\":\"%s\",\"age\":%d}";

		final String personJsonString = String.format(personJsonTemplate, 4, "John", "Doe", 35);

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
						// Store the request URL
						container.setRequestUrl(request.getUrl());

						// Create a mock response and set the mocked JSon
						// content
						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setContent(personJsonString);

						responseCallback.onResponse(response, null);
					}
				});

				client.getTable(tableName).lookUp(personId, new TableJsonOperationCallback() {
					@Override
					public void onCompleted(JsonObject jsonEntity, Exception exception, ServiceFilterResponse response) {
						if (exception == null) {
							container.setResponseValue(jsonEntity.toString());
						}
						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		assertEquals(personJsonString, container.getResponseValue());
		assertEquals(this.appUrl + "tables/" + tableName + "/" + personId, container.getRequestUrl());
	}

	public void testLookupShouldReturnErrorIfAPersonDoesNotExist() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		// Person Id
		final int personId = 4;
		final String tableName = "MyTableName";

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
						// Store the request URL
						container.setRequestUrl(request.getUrl());

						// Create a mock response simulating an error
						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setStatus(new StatusLineMock(404));
						response.setContent("{\"error\":404,\"message\":\"entity does not exist\"}");

						// create a mock request to replace the existing one
						ServiceFilterRequestMock requestMock = new ServiceFilterRequestMock(response);
						nextServiceFilterCallback.onNext(requestMock, responseCallback);
					}
				});

				client.getTable(tableName, PersonTestObject.class).lookUp(personId, new TableOperationCallback<PersonTestObject>() {

					@Override
					public void onCompleted(PersonTestObject entity, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setResponseValue(response.getContent());
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		assertEquals(this.appUrl + "tables/" + tableName + "/" + personId, container.getRequestUrl());
		assertTrue(container.getResponseValue().contains("{\"error\":404,\"message\":\"entity does not exist\"}"));
	}

	public void testLookupWithJSONShouldReturnErrorIfAPersonDoesNotExist() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		// Person Id
		final int personId = 4;
		final String tableName = "MyTableName";

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
						// Store the request URL
						container.setRequestUrl(request.getUrl());

						// Create a mock response simulating an error
						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setStatus(new StatusLineMock(404));
						response.setContent("{\"error\":404,\"message\":\"entity does not exist\"}");

						// create a mock request to replace the existing one
						ServiceFilterRequestMock requestMock = new ServiceFilterRequestMock(response);
						nextServiceFilterCallback.onNext(requestMock, responseCallback);
					}
				});

				client.getTable(tableName).lookUp(personId, new TableJsonOperationCallback() {
					@Override
					public void onCompleted(JsonObject jsonEntity, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setResponseValue(response.getContent());
						}
						latch.countDown();

					}
				});
			}
		});

		latch.await();

		// Asserts
		assertEquals(this.appUrl + "tables/" + tableName + "/" + personId, container.getRequestUrl());
		assertTrue(container.getResponseValue().contains("{\"error\":404,\"message\":\"entity does not exist\"}"));
	}

	public void testQueryShouldIncludeFilter() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						container.setRequestUrl(request.getUrl());
						// call onResponse with the mocked response
						responseCallback.onResponse(new ServiceFilterResponseMock(), null);
					}
				});

				// Create get the MobileService table
				MobileServiceJsonTable msTable = client.getTable(tableName);

				// Call the update method
				msTable.where().field("fieldName").eq(1).execute(new TableJsonQueryCallback() {

					@Override
					public void onCompleted(JsonElement result, int count, Exception exception, ServiceFilterResponse response) {
						container.setOperationSucceded(exception == null);
						latch.countDown();

					}
				});
			}
		});

		latch.await();

		// Asserts
		Assert.assertTrue("Opperation should have succeded", container.getOperationSucceded());
		assertEquals(queryUrl(tableName) + "?$filter=" + urlencode("fieldName eq (1)"), container.getRequestUrl());
	}

	private String urlencode(String s) {
		try {
			return URLEncoder.encode(s, "utf-8");
		} catch (Exception e) {
			return null;
		}
	}

	private String queryUrl(String tableName) {
		return this.appUrl + "tables/" + tableName;
	}

	public void testQueryShouldIncludeTop() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						container.setRequestUrl(request.getUrl());
						// call onResponse with the mocked response
						responseCallback.onResponse(new ServiceFilterResponseMock(), null);
					}
				});

				// Create get the MobileService table
				MobileServiceJsonTable msTable = client.getTable(tableName);

				// Call the update method
				msTable.top(10).execute(new TableJsonQueryCallback() {

					@Override
					public void onCompleted(JsonElement result, int count, Exception exception, ServiceFilterResponse response) {
						container.setOperationSucceded(exception == null);
						latch.countDown();

					}
				});
			}
		});

		latch.await();

		// Asserts
		Assert.assertTrue("Opperation should have succeded", container.getOperationSucceded());
		assertEquals(queryUrl(tableName) + "?$top=10", container.getRequestUrl());
	}

	public void testQueryShouldIncludeSkip() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						container.setRequestUrl(request.getUrl());
						// call onResponse with the mocked response
						responseCallback.onResponse(new ServiceFilterResponseMock(), null);
					}
				});

				// Create get the MobileService table
				MobileServiceJsonTable msTable = client.getTable(tableName);

				// Call the update method
				msTable.skip(10).execute(new TableJsonQueryCallback() {

					@Override
					public void onCompleted(JsonElement result, int count, Exception exception, ServiceFilterResponse response) {
						container.setOperationSucceded(exception == null);
						latch.countDown();

					}
				});
			}
		});

		latch.await();

		// Asserts
		Assert.assertTrue("Opperation should have succeded", container.getOperationSucceded());
		assertEquals(queryUrl(tableName) + "?$skip=10", container.getRequestUrl());
	}

	public void testQueryShouldIncludeInlineCount() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						container.setRequestUrl(request.getUrl());
						// call onResponse with the mocked response
						responseCallback.onResponse(new ServiceFilterResponseMock(), null);
					}
				});

				// Create get the MobileService table
				MobileServiceJsonTable msTable = client.getTable(tableName);

				// Call the update method
				msTable.includeInlineCount().execute(new TableJsonQueryCallback() {

					@Override
					public void onCompleted(JsonElement result, int count, Exception exception, ServiceFilterResponse response) {
						container.setOperationSucceded(exception == null);
						latch.countDown();

					}
				});
			}
		});

		latch.await();

		// Asserts
		Assert.assertTrue("Opperation should have succeded", container.getOperationSucceded());
		assertEquals(queryUrl(tableName) + "?$inlinecount=allpages", container.getRequestUrl());
	}

	public void testQueryShouldIncludeOrderBy() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						container.setRequestUrl(request.getUrl());
						// call onResponse with the mocked response
						responseCallback.onResponse(new ServiceFilterResponseMock(), null);
					}
				});

				// Create get the MobileService table
				MobileServiceJsonTable msTable = client.getTable(tableName);

				// Call the update method
				msTable.orderBy("myField", QueryOrder.Ascending).execute(new TableJsonQueryCallback() {

					@Override
					public void onCompleted(JsonElement result, int count, Exception exception, ServiceFilterResponse response) {
						container.setOperationSucceded(exception == null);
						latch.countDown();

					}
				});
			}
		});

		latch.await();

		// Asserts
		Assert.assertTrue("Opperation should have succeded", container.getOperationSucceded());
		assertEquals(queryUrl(tableName) + "?$orderby=" + urlencode("myField asc"), container.getRequestUrl());
	}

	public void testQueryShouldIncludeProjection() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						container.setRequestUrl(request.getUrl());
						// call onResponse with the mocked response
						responseCallback.onResponse(new ServiceFilterResponseMock(), null);
					}
				});

				// Create get the MobileService table
				MobileServiceJsonTable msTable = client.getTable(tableName);

				// Call the update method
				msTable.select("myField", "otherField").execute(new TableJsonQueryCallback() {

					@Override
					public void onCompleted(JsonElement result, int count, Exception exception, ServiceFilterResponse response) {
						container.setOperationSucceded(exception == null);
						latch.countDown();

					}
				});
			}
		});

		latch.await();

		// Asserts
		Assert.assertTrue("Opperation should have succeded", container.getOperationSucceded());
		assertEquals(queryUrl(tableName) + "?$select=" + urlencode("myField,otherField"), container.getRequestUrl());
	}
}
