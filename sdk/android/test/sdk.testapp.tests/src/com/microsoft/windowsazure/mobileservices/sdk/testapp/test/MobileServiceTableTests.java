package com.microsoft.windowsazure.mobileservices.sdk.testapp.test;

import java.net.MalformedURLException;
import java.util.List;
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
import com.microsoft.windowsazure.mobileservices.MobileServiceTable;
import com.microsoft.windowsazure.mobileservices.NextServiceFilterCallback;
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

	public void testNewMobileServiceTableShouldReturnMobileServiceTable()
			throws MalformedURLException {
		String tableName = "MyTableName";
		MobileServiceClient client = new MobileServiceClient(appUrl, appKey);
		MobileServiceTable msTable = new MobileServiceTable(tableName, client);

		assertEquals(client, msTable.getClient());
		assertEquals(tableName, msTable.getTableName());
	}

	public void testNewMobileServiceTableWithNullNameShouldThrowException() {
		MobileServiceClient client = null;
		try {
			client = new MobileServiceClient(appUrl, appKey);
		} catch (MalformedURLException e1) {
			fail("This should not happen");
		}
		try {
			new MobileServiceTable(null, client);
			fail("Expected Exception IllegalArgumentException");
		} catch (IllegalArgumentException e) {
			// do nothing, it's OK
		}
	}

	public void testNewMobileServiceTableWithEmptyNameShouldThrowException() {
		MobileServiceClient client = null;
		try {
			client = new MobileServiceClient(appUrl, appKey);
		} catch (MalformedURLException e1) {
			fail("This should not happen");
		}
		try {
			new MobileServiceTable("", client);
			fail("Expected Exception IllegalArgumentException");
		} catch (IllegalArgumentException e) {
			// do nothing, it's OK
		}
	}

	public void testNewMobileServiceTableWithWhiteSpacedNameShouldThrowException() {
		MobileServiceClient client = null;
		try {
			client = new MobileServiceClient(appUrl, appKey);
		} catch (MalformedURLException e1) {
			fail("This should not happen");
		}
		try {
			new MobileServiceTable(" ", client);
			fail("Expected Exception IllegalArgumentException");
		} catch (IllegalArgumentException e) {
			// do nothing, it's OK
		}
	}

	public void testNewMobileServiceTableWithNullClientShouldThrowException() {
		try {
			new MobileServiceTable("MyTableName", null);
			fail("Expected Exception IllegalArgumentException");
		} catch (IllegalArgumentException e) {
			// do nothing, it's OK
		}
	}

	public void testWhereWithNullQueryShouldThrowException() {
		MobileServiceClient client = null;
		MobileServiceTable msTable = null;
		try {
			client = new MobileServiceClient(appUrl, appKey);
			String tableName = "MyTableName";
			msTable = new MobileServiceTable(tableName, client);
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
					client = new MobileServiceClient(appUrl, appKey);
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(
							ServiceFilterRequest request,
							NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setContent("{\"id\":38, \"firstName\":\"John\", \"lastName\":\"Foo\", \"Age\":29}");
						// call onResponse with the mocked response
						responseCallback.onResponse(response, null);
					}
				});

				// Create get the MobileService table
				MobileServiceTable msTable = client.getTable(tableName);

				// Call the insert method
				msTable.insert(person,
						new TableOperationCallback<PersonTestObject>() {

							@Override
							public void onCompleted(PersonTestObject entity,
									Exception exception,
									ServiceFilterResponse response) {
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

	public void testInsertShouldReturnJSONWithId() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store the object after the insertion, we need this to do
		// the asserts outside the onSuccess method
		final ResultsContainer container = new ResultsContainer();

		// Object to insert
		final PersonTestObject person = new PersonTestObject("John", "Doe", 29);

		final JsonObject jsonPerson = gsonBuilder.create().toJsonTree(person)
				.getAsJsonObject();

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {

				String tableName = "MyTableName";
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey);
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new JSon
				// object with an id defined
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(
							ServiceFilterRequest request,
							NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setContent("{\"id\":38}");
						// call onResponse with the mocked response
						responseCallback.onResponse(response, null);
					}
				});

				// Create get the MobileService table
				MobileServiceTable msTable = client.getTable(tableName);

				// Call the insert method
				msTable.insert(jsonPerson, new TableJsonOperationCallback() {

					@Override
					public void onCompleted(JsonObject jsonEntity,
							Exception exception, ServiceFilterResponse response) {
						container.setResponseValue(jsonEntity.toString());
						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		PersonTestObject p = gsonBuilder.create().fromJson(
				container.getResponseValue(), PersonTestObject.class);
		Assert.assertNotNull("Person expected", p);
		Assert.assertEquals(38, p.getId());
		Assert.assertEquals(person.getFirstName(), p.getFirstName());
		Assert.assertEquals(person.getLastName(), p.getLastName());
		Assert.assertEquals(person.getAge(), p.getAge());
	}

	public void testOperationHandleServerErrorProperly() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {

				String tableName = "MyTableName";
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey);
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new JSon
				// object with an id defined
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(
							ServiceFilterRequest request,
							NextServiceFilterCallback nextServiceFilterCallback,
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
						responseCallback.onResponse(response, new MobileServiceException(
								"ERROR"));
					}
				});

				// Create get the MobileService table
				MobileServiceTable msTable = client.getTable(tableName);

				JsonObject json = new JsonParser().parse(
						"{'myField': 'invalid value'}").getAsJsonObject();

				// Call the insert method
				msTable.insert(json, new TableJsonOperationCallback() {

					@Override
					public void onCompleted(JsonObject jsonEntity,
							Exception exception, ServiceFilterResponse response) {
						if (exception == null) {
							Assert.fail();
						} else {
							assertTrue(exception instanceof MobileServiceException);
						}
						latch.countDown();
					}
				});
			}
		});

		latch.await();
	}

	public void testOperationWithErrorAndNoContentShowStatusCode()
			throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {

				String tableName = "MyTableName";
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey);
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new JSon
				// object with an id defined
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(
							ServiceFilterRequest request,
							NextServiceFilterCallback nextServiceFilterCallback,
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

						response.setContent(null);
						// call onResponse with the mocked response
						responseCallback.onResponse(response,
								new MobileServiceException(
										"Error while processing request",
										new MobileServiceException(
												String.format(
														"{'code': %d}",
														response.getStatus()
																.getStatusCode()))));
					}
				});

				// Create get the MobileService table
				MobileServiceTable msTable = client.getTable(tableName);

				JsonObject json = new JsonParser().parse(
						"{'myField': 'invalid value'}").getAsJsonObject();

				// Call the insert method
				msTable.insert(json, new TableJsonOperationCallback() {

					@Override
					public void onCompleted(JsonObject jsonEntity,
							Exception exception, ServiceFilterResponse response) {
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
			}
		});

		latch.await();
	}

	public void testUpdateShouldReturnEntityWithDifferentNameAndAge()
			throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store the object after the insertion, we need this to do
		// the asserts outside the onSuccess method
		final ResultsContainer container = new ResultsContainer();

		// Object to insert
		final PersonTestObject person = new PersonTestObject("John", "Doe", 29);
		person.setId(10);

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {

				String tableName = "MyTableName";
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey);
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(
							ServiceFilterRequest request,
							NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {
						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setContent("{\"firstName\":\"Mike\", \"age\":50}");
						// call onResponse with the mocked response
						responseCallback.onResponse(response, null);
					}
				});

				// Get the MobileService table
				MobileServiceTable msTable = client.getTable(tableName);

				// Call the delete method
				msTable.update(person,
						new TableOperationCallback<PersonTestObject>() {

							@Override
							public void onCompleted(PersonTestObject entity,
									Exception exception,
									ServiceFilterResponse response) {
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

	public void testUpdateShouldReturnJSONWithDifferentNameAndAge()
			throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store the object after the insertion, we need this to do
		// the asserts outside the onSuccess method
		final ResultsContainer container = new ResultsContainer();

		// Object to insert
		final PersonTestObject person = new PersonTestObject("John", "Doe", 29);
		person.setId(10);

		final JsonObject jsonPerson = gsonBuilder.create().toJsonTree(person)
				.getAsJsonObject();

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {

				String tableName = "MyTableName";
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey);
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(
							ServiceFilterRequest request,
							NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setContent("{\"firstName\":\"Mike\", \"age\":50}");
						// call onResponse with the mocked response
						responseCallback.onResponse(response, null);
					}
				});

				// Get the MobileService table
				MobileServiceTable msTable = client.getTable(tableName);

				// Call the delete method
				msTable.update(jsonPerson, new TableJsonOperationCallback() {

					@Override
					public void onCompleted(JsonObject jsonEntity,
							Exception exception, ServiceFilterResponse response) {
						container.setResponseValue(jsonEntity.toString());
						latch.countDown();	
					}
				});

			}
		});

		latch.await();

		// Asserts
		PersonTestObject p = gsonBuilder.create().fromJson(
				container.getResponseValue(), PersonTestObject.class);
		Assert.assertNotNull("Person expected", p);
		Assert.assertEquals(person.getId(), p.getId());
		Assert.assertEquals("Mike", p.getFirstName());
		Assert.assertEquals(person.getLastName(), p.getLastName());
		Assert.assertEquals(50, p.getAge());
	}

	public void testDeleteUsingEntityShouldReturnTheExpectedRequestUrl()
			throws Throwable {
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
					client = new MobileServiceClient(appUrl, appKey);
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and store the request URL.
				// Send a mock response
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(
							ServiceFilterRequest request,
							NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						container.setRequestUrl(request.getUrl());
						// call onResponse with the mocked response
						responseCallback
								.onResponse(new ServiceFilterResponseMock(), null);
					}
				});

				// Get the MobileService table
				MobileServiceTable msTable = client.getTable(tableName);

				// Call the delete method sending the entity to delete
				msTable.delete(person, new TableDeleteCallback() {

					@Override
					public void onCompleted(Exception exception,
							ServiceFilterResponse response) {
						container.setOperationSucceded(exception == null);
						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Assert.assertTrue("Opperation should have succeded",
				container.getOperationSucceded());
		assertEquals(
				this.appUrl + "tables/" + tableName + "/" + person.getId(),
				container.getRequestUrl());
	}

	public void testDeleteUsingIdShouldReturnTheExpectedRequestUrl()
			throws Throwable {
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
					client = new MobileServiceClient(appUrl, appKey);
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and create a new json
				// object with an id defined
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(
							ServiceFilterRequest request,
							NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						container.setRequestUrl(request.getUrl());
						// call onResponse with the mocked response
						responseCallback
								.onResponse(new ServiceFilterResponseMock(), null);
					}
				});

				// Create get the MobileService table
				MobileServiceTable msTable = client.getTable(tableName);

				// Call the update method
				msTable.delete(personId, new TableDeleteCallback() {

					@Override
					public void onCompleted(Exception exception,
							ServiceFilterResponse response) {
						container.setOperationSucceded(exception == null);
						latch.countDown();
					}
				});

			}
		});

		latch.await();

		// Asserts
		Assert.assertTrue("Opperation should have succeded",
				container.getOperationSucceded());
		assertEquals(this.appUrl + "tables/" + tableName + "/" + personId,
				container.getRequestUrl());
	}

	public void testDeleteUsingJSONShouldReturnTheExpectedRequestUrl()
			throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		// Object to delete
		final PersonTestObject person = new PersonTestObject("John", "Doe", 29);
		person.setId(10);

		final JsonObject jsonPerson = gsonBuilder.create().toJsonTree(person)
				.getAsJsonObject();

		final String tableName = "MyTableName";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;

				try {
					client = new MobileServiceClient(appUrl, appKey);
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				// Add a filter to handle the request and store the request URL.
				// Send a mock response
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(
							ServiceFilterRequest request,
							NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						container.setRequestUrl(request.getUrl());
						// call onResponse with the mocked response
						responseCallback
								.onResponse(new ServiceFilterResponseMock(), null);
					}
				});

				// Get the MobileService table
				MobileServiceTable msTable = client.getTable(tableName);

				// Call the delete method sending the entity to delete
				msTable.delete(jsonPerson, new TableDeleteCallback() {

					@Override
					public void onCompleted(Exception exception,
							ServiceFilterResponse response) {
						container.setOperationSucceded(exception == null);
						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Assert.assertTrue("Opperation should have succeded",
				container.getOperationSucceded());
		assertEquals(
				this.appUrl + "tables/" + tableName + "/" + person.getId(),
				container.getRequestUrl());
	}

	public void testSimpleSelectShouldReturnResults() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;
				try {
					client = new MobileServiceClient(appUrl, appKey);
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(
							ServiceFilterRequest request,
							NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						// PersonTestObject JSon template
						String personJsonTemplate = "{\"id\": %d,\"firstName\":\"%s\",\"lastName\":\"%s\",\"age\":%d}";

						// Generate JSon string with 2 objects
						String responseContent = "[";
						responseContent += String.format(personJsonTemplate, 1,
								"Mike", "Foo", 27) + ",";
						responseContent += String.format(personJsonTemplate, 2,
								"John", "Doe", 35);
						responseContent += "]";

						// Create a mock response and set the mocked JSon
						// content
						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setContent(responseContent);

						responseCallback.onResponse(response, null);
					}
				});

				client.getTable(tableName)
						.all()
						.execute(PersonTestObject.class,
								new TableQueryCallback<PersonTestObject>() {

									@Override
									public void onCompleted(
											List<PersonTestObject> result,
											int count, Exception exception,
											ServiceFilterResponse response) {
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

	public void testSimpleSelectShouldReturnJSONResults() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		// PersonTestObject JSon template
		String personJsonTemplate = "{\"id\":%d,\"firstName\":\"%s\",\"lastName\":\"%s\",\"age\":%d}";

		// Generate JSon string with 2 objects
		final String responseContent = "["
				+ String.format(personJsonTemplate, 1, "Mike", "Foo", 27) + ","
				+ String.format(personJsonTemplate, 2, "John", "Doe", 35) + "]";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;
				try {
					client = new MobileServiceClient(appUrl, appKey);
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(
							ServiceFilterRequest request,
							NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {

						// Create a mock response and set the mocked JSon
						// content
						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setContent(responseContent);

						responseCallback.onResponse(response, null);
					}
				});

				client.getTable(tableName).all()
						.execute(new TableJsonQueryCallback() {

							@Override
							public void onCompleted(JsonElement result,
									int count, Exception exception,
									ServiceFilterResponse response) {
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

	public void testSimpleSelectShouldReturnEmptyArray() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;
				try {
					client = new MobileServiceClient(appUrl, appKey);
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(
							ServiceFilterRequest request,
							NextServiceFilterCallback nextServiceFilterCallback,
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

				client.getTable(tableName)
						.all()
						.execute(PersonTestObject.class,
								new TableQueryCallback<PersonTestObject>() {

									@Override
									public void onCompleted(
											List<PersonTestObject> result,
											int count, Exception exception,
											ServiceFilterResponse response) {
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

	public void testSimpleJSONSelectShouldReturnEmptyJSONArray()
			throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;
				try {
					client = new MobileServiceClient(appUrl, appKey);
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(
							ServiceFilterRequest request,
							NextServiceFilterCallback nextServiceFilterCallback,
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

				client.getTable(tableName).all()
						.execute(new TableJsonQueryCallback() {

							@Override
							public void onCompleted(JsonElement result,
									int count, Exception exception,
									ServiceFilterResponse response) {
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

	public void testInlineCountSelectShouldReturnResultsWithCount()
			throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;
				try {
					client = new MobileServiceClient(appUrl, appKey);
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(
							ServiceFilterRequest request,
							NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {
						// PersonTestObject JSon template
						String personJsonTemplate = "{\"id\": %d,\"firstName\":\"%s\",\"lastName\":\"%s\",\"age\":%d}";

						// Create string with results and count values.
						String responseContent = "{\"results\":[";
						responseContent += String.format(personJsonTemplate, 1,
								"Mike", "Foo", 27) + ",";
						responseContent += String.format(personJsonTemplate, 2,
								"John", "Doe", 35);
						responseContent += "]";
						responseContent += ",\"count\":\"15\"}";

						// Create a mock response and set the mocked JSon
						// content
						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setContent(responseContent);

						responseCallback.onResponse(response, null);
					}
				});

				client.getTable(tableName)
						.all()
						.execute(PersonTestObject.class,
								new TableQueryCallback<PersonTestObject>() {
									
									@Override
									public void onCompleted(
											List<PersonTestObject> result,
											int count, Exception exception,
											ServiceFilterResponse response) {
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
					client = new MobileServiceClient(appUrl, appKey);
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(
							ServiceFilterRequest request,
							NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {
						// Store the request URL
						container.setRequestUrl(request.getUrl());

						// PersonTestObject JSon template
						String personJsonTemplate = "{\"id\": %d,\"firstName\":\"%s\",\"lastName\":\"%s\",\"age\":%d}";

						// Create a mock response and set the mocked JSon
						// content
						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setContent(String.format(personJsonTemplate,
								4, "John", "Doe", 35));

						responseCallback.onResponse(response, null);
					}
				});

				client.getTable(tableName).lookUp(personId,
						PersonTestObject.class,
						new TableOperationCallback<PersonTestObject>() {

							@Override
							public void onCompleted(PersonTestObject entity,
									Exception exception,
									ServiceFilterResponse response) {
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
		assertEquals(this.appUrl + "tables/" + tableName + "/" + personId,
				container.getRequestUrl());
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

		final String personJsonString = String.format(personJsonTemplate, 4,
				"John", "Doe", 35);

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;
				try {
					client = new MobileServiceClient(appUrl, appKey);
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(
							ServiceFilterRequest request,
							NextServiceFilterCallback nextServiceFilterCallback,
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

				client.getTable(tableName).lookUp(personId,
						new TableJsonOperationCallback() {
							@Override
							public void onCompleted(JsonObject jsonEntity,
									Exception exception,
									ServiceFilterResponse response) {
								if (exception == null) {
									container.setResponseValue(jsonEntity
											.toString());
								}
								latch.countDown();
							}
						});
			}
		});

		latch.await();

		// Asserts
		assertEquals(personJsonString, container.getResponseValue());
		assertEquals(this.appUrl + "tables/" + tableName + "/" + personId,
				container.getRequestUrl());
	}

	public void testLookupShouldReturnErrorIfAPersonDoesNotExist()
			throws Throwable {
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
					client = new MobileServiceClient(appUrl, appKey);
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(
							ServiceFilterRequest request,
							NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {
						// Store the request URL
						container.setRequestUrl(request.getUrl());

						// Create a mock response simulating an error
						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setStatus(new StatusLineMock(404));
						response.setContent("{\"error\":404,\"message\":\"entity does not exist\"}");

						// create a mock request to replace the existing one
						ServiceFilterRequestMock requestMock = new ServiceFilterRequestMock(
								response);
						nextServiceFilterCallback.onNext(requestMock,
								responseCallback);
					}
				});

				client.getTable(tableName).lookUp(personId,
						PersonTestObject.class,
						new TableOperationCallback<PersonTestObject>() {

							@Override
							public void onCompleted(PersonTestObject entity,
									Exception exception,
									ServiceFilterResponse response) {
								if (exception != null)
								{
									container.setResponseValue(response
										.getContent());
								}
								
								latch.countDown();
							}
						});
			}
		});

		latch.await();

		// Asserts
		assertEquals(this.appUrl + "tables/" + tableName + "/" + personId,
				container.getRequestUrl());
		assertTrue(container.getResponseValue().contains(
				"{\"error\":404,\"message\":\"entity does not exist\"}"));
	}

	public void testLookupWithJSONShouldReturnErrorIfAPersonDoesNotExist()
			throws Throwable {
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
					client = new MobileServiceClient(appUrl, appKey);
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(
							ServiceFilterRequest request,
							NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {
						// Store the request URL
						container.setRequestUrl(request.getUrl());

						// Create a mock response simulating an error
						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setStatus(new StatusLineMock(404));
						response.setContent("{\"error\":404,\"message\":\"entity does not exist\"}");

						// create a mock request to replace the existing one
						ServiceFilterRequestMock requestMock = new ServiceFilterRequestMock(
								response);
						nextServiceFilterCallback.onNext(requestMock,
								responseCallback);
					}
				});

				client.getTable(tableName).lookUp(personId,
						new TableJsonOperationCallback() {
							@Override
							public void onCompleted(JsonObject jsonEntity,
									Exception exception,
									ServiceFilterResponse response) {
								if (exception != null) {
									container.setResponseValue(response
											.getContent());
								}
								latch.countDown();
								
							}
						});
			}
		});

		latch.await();

		// Asserts
		assertEquals(this.appUrl + "tables/" + tableName + "/" + personId,
				container.getRequestUrl());
		assertTrue(container.getResponseValue().contains(
				"{\"error\":404,\"message\":\"entity does not exist\"}"));
	}
}
