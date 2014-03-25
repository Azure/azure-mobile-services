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

import java.lang.reflect.Type;
import java.net.MalformedURLException;
import java.util.ArrayList;
import java.util.Date;
import java.util.GregorianCalendar;
import java.util.List;
import java.util.TimeZone;
import java.util.concurrent.CountDownLatch;

import android.test.InstrumentationTestCase;

import com.google.gson.Gson;
import com.google.gson.GsonBuilder;
import com.google.gson.JsonDeserializationContext;
import com.google.gson.JsonDeserializer;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParseException;
import com.google.gson.JsonParser;
import com.google.gson.JsonSerializationContext;
import com.google.gson.JsonSerializer;
import com.google.gson.annotations.Expose;
import com.google.gson.annotations.SerializedName;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceTable;
import com.microsoft.windowsazure.mobileservices.NextServiceFilterCallback;
import com.microsoft.windowsazure.mobileservices.ServiceFilter;
import com.microsoft.windowsazure.mobileservices.ServiceFilterRequest;
import com.microsoft.windowsazure.mobileservices.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.ServiceFilterResponseCallback;
import com.microsoft.windowsazure.mobileservices.TableOperationCallback;

class Person {
	@Expose
	@SerializedName("id")
	private Integer mId;

	@Expose
	@SerializedName("name")
	private String mName;

	@SerializedName("age")
	private Integer mAge;

	public Person(int id) {
		mId = id;
	}

	public Person(int id, String name, int age) {
		mId = id;
		mName = name;
		mAge = age;
	}

	public Person(String name, int age) {
		mName = name;
		mAge = age;
	}

	public Integer getmId() {
		return mId;
	}

	public void setmId(int mId) {
		this.mId = mId;
	}

	public String getmName() {
		return mName;
	}

	public void setmName(String mName) {
		this.mName = mName;
	}

	public Integer getmAge() {
		return mAge;
	}

	public void setmAge(int mAge) {
		this.mAge = mAge;
	}
}

class Group {
	int mId;

	String mName;

	private List<Person> people;

	public Group(int id, String name) {
		mId = id;
		mName = name;
	}

	public List<Person> getPeople() {
		return people;
	}

	public void setPeople(List<Person> people) {
		this.people = people;
	}
}

public class SerializationTests extends InstrumentationTestCase {

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

	public void testSimpleSerialization() throws Throwable {
		// Serialize instance with all fields defined
		Person person = new Person(1, "John", 23);
		String serializedObject = new Gson().toJson(person);
		JsonObject jsonPerson = new JsonParser().parse(serializedObject).getAsJsonObject();

		// Asserts
		assertEquals("{\"age\":23,\"id\":1,\"name\":\"John\"}", serializedObject);
		assertEquals(person.getmAge().intValue(), jsonPerson.get("age").getAsInt());
		assertEquals(person.getmId().intValue(), jsonPerson.get("id").getAsInt());
		assertEquals(person.getmName(), jsonPerson.get("name").getAsString());

		// Serialize instance with not id defined
		Person person2 = new Person("John", 23);
		String serializedObject2 = new Gson().toJson(person2);
		JsonObject jsonPerson2 = new JsonParser().parse(serializedObject2).getAsJsonObject();

		// Asserts
		assertEquals("{\"age\":23,\"name\":\"John\"}", serializedObject2);
		assertEquals(person.getmAge().intValue(), jsonPerson2.get("age").getAsInt());
		assertNull(jsonPerson2.get("id"));
		assertEquals(person.getmName(), jsonPerson2.get("name").getAsString());

		// Serialize instance with all fields null except id
		Person person3 = new Person(1);
		String serializedObject3 = new Gson().toJson(person3);
		JsonObject jsonPerson3 = new JsonParser().parse(serializedObject3).getAsJsonObject();

		// Asserts
		assertEquals("{\"id\":1}", serializedObject3);
		assertNull(jsonPerson3.get("age"));
		assertEquals(person3.getmId().intValue(), jsonPerson3.get("id").getAsInt());
		assertNull(jsonPerson3.get("name"));
	}

	public void testSimpleDeserialization() throws Throwable {
		// Deserialize instance with all fields defined
		String serializedObject = "{\"age\":23,\"id\":1,\"name\":\"John\"}";
		JsonObject jsonPerson = new JsonParser().parse(serializedObject).getAsJsonObject();
		Person person = new Gson().fromJson(serializedObject, Person.class);

		// Asserts
		assertEquals(jsonPerson.get("age").getAsInt(), person.getmAge().intValue());
		assertEquals(jsonPerson.get("id").getAsInt(), person.getmId().intValue());
		assertEquals(jsonPerson.get("name").getAsString(), person.getmName());

		// Serialize instance with not id defined
		String serializedObject2 = "{\"age\":23,\"name\":\"John\"}";
		JsonObject jsonPerson2 = new JsonParser().parse(serializedObject2).getAsJsonObject();
		Person person2 = new Gson().fromJson(serializedObject2, Person.class);

		// Asserts
		assertEquals(jsonPerson2.get("age").getAsInt(), person2.getmAge().intValue());
		assertNull(person2.getmId());
		assertEquals(jsonPerson2.get("name").getAsString(), person2.getmName());

		// Serialize instance with all fields null except id
		String serializedObject3 = "{\"id\":1}";
		JsonObject jsonPerson3 = new JsonParser().parse(serializedObject3).getAsJsonObject();
		Person person3 = new Gson().fromJson(serializedObject3, Person.class);

		// Asserts
		assertEquals(jsonPerson3.get("id").getAsInt(), person3.getmId().intValue());
		assertNull(person3.getmName());
		assertNull(person3.getmAge());
	}

	public void testSerializationExcludingFieldsWithoutExposeAttribute() {
		// Serialize instance with all fields defined, but excluding those
		// without Expose attribute
		Person person = new Person(1, "John", 23);
		String serializedObject = gsonBuilder.excludeFieldsWithoutExposeAnnotation().create().toJson(person);

		JsonObject jsonPerson = new JsonParser().parse(serializedObject).getAsJsonObject();

		// Asserts
		assertEquals("{\"id\":1,\"name\":\"John\"}", serializedObject);
		assertNull(jsonPerson.get("age"));
		assertEquals(person.getmId().intValue(), jsonPerson.get("id").getAsInt());
		assertEquals(person.getmName(), jsonPerson.get("name").getAsString());

	}

	public void testCustomSerializationWithoutUsingMobileServiceTable() {
		ComplexPersonTestObject person = new ComplexPersonTestObject("John", "Doe", new Address("1345 Washington St", 1313, "US"));

		gsonBuilder.registerTypeAdapter(Address.class, new JsonSerializer<Address>() {

			@Override
			public JsonElement serialize(Address arg0, Type arg1, JsonSerializationContext arg2) {

				JsonObject json = new JsonObject();
				json.addProperty("zipcode", arg0.getZipCode());
				json.addProperty("country", arg0.getCountry());
				json.addProperty("streetaddress", arg0.getStreetAddress());

				return json;
			}
		});

		String serializedObject = gsonBuilder.create().toJson(person);

		// Asserts
		assertEquals(
				"{\"address\":{\"zipcode\":1313,\"country\":\"US\",\"streetaddress\":\"1345 Washington St\"},\"firstName\":\"John\",\"lastName\":\"Doe\",\"id\":0}",
				serializedObject);
	}

	public void testCustomDeserializationUsingWithoutUsingMobileServiceTable() {
		String serializedObject = "{\"address\":{\"zipcode\":1313,\"country\":\"US\",\"streetaddress\":\"1345 Washington St\"},\"firstName\":\"John\",\"lastName\":\"Doe\"}";
		JsonObject jsonObject = new JsonParser().parse(serializedObject).getAsJsonObject();

		gsonBuilder.registerTypeAdapter(Address.class, new JsonDeserializer<Address>() {

			@Override
			public Address deserialize(JsonElement arg0, Type arg1, JsonDeserializationContext arg2) throws JsonParseException {
				Address a = new Address(arg0.getAsJsonObject().get("streetaddress").getAsString(), arg0.getAsJsonObject().get("zipcode").getAsInt(), arg0
						.getAsJsonObject().get("country").getAsString());

				return a;
			}

		});

		ComplexPersonTestObject deserializedPerson = gsonBuilder.create().fromJson(jsonObject, ComplexPersonTestObject.class);

		// Asserts
		assertEquals("John", deserializedPerson.getFirstName());
		assertEquals("Doe", deserializedPerson.getLastName());
		assertEquals(1313, deserializedPerson.getAddress().getZipCode());
		assertEquals("US", deserializedPerson.getAddress().getCountry());
		assertEquals("1345 Washington St", deserializedPerson.getAddress().getStreetAddress());
	}

	public void testSimpleTreeSerialization() {
		Group group = new Group(1, "Group1");
		List<Person> list = new ArrayList<Person>();
		list.add(new Person(2, "John", 23));
		list.add(new Person(4, "Paul", 18));
		list.add(new Person(5, "Maria", 25));
		group.setPeople(list);

		String serializedObject = gsonBuilder.create().toJson(group);
		// Asserts
		assertEquals(
				"{\"people\":[{\"age\":23,\"id\":2,\"name\":\"John\"},{\"age\":18,\"id\":4,\"name\":\"Paul\"},{\"age\":25,\"id\":5,\"name\":\"Maria\"}],\"mName\":\"Group1\",\"mId\":1}",
				serializedObject);
	}

	public void testDateSerializationShouldReturnExpectedJson() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		final GregorianCalendar date = new GregorianCalendar(2013, 0, 22, 10, 30, 40);
		date.setTimeZone(TimeZone.getTimeZone("GMT-4"));

		final DateTestObject dateObject = new DateTestObject(date.getTime());

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
						// Store the request content
						container.setRequestContent(request.getContent());
						ServiceFilterResponseMock mockedResponse = new ServiceFilterResponseMock();
						mockedResponse.setContent("{}");
						responseCallback.onResponse(mockedResponse, null);
					}
				});

				MobileServiceTable<DateTestObject> table = client.getTable(tableName, DateTestObject.class);

				table.insert(dateObject, new TableOperationCallback<DateTestObject>() {

					@Override
					public void onCompleted(DateTestObject entity, Exception exception, ServiceFilterResponse response) {
						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		// Date should have UTC format (+4 that date value)
		assertEquals("{\"date\":\"2013-01-22T14:30:40.000Z\"}", container.getRequestContent());
	}

	@SuppressWarnings("deprecation")
	public void testDateDeserializationShouldReturnExpectedEntity() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		final GregorianCalendar calendar = new GregorianCalendar(2013, 0, 22, 10, 30, 40);
		calendar.setTimeZone(TimeZone.getTimeZone("GMT-4"));

		final DateTestObject dateObject = new DateTestObject(calendar.getTime());

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

						// Create a mock response simulating an error
						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setStatus(new StatusLineMock(404));
						response.setContent("{\"date\":\"2013-01-22T14:30:40.000Z\"}");

						responseCallback.onResponse(response, null);
					}
				});

				MobileServiceTable<DateTestObject> table = client.getTable(tableName, DateTestObject.class);

				table.insert(dateObject, new TableOperationCallback<DateTestObject>() {

					@Override
					public void onCompleted(DateTestObject entity, Exception exception, ServiceFilterResponse response) {
						container.setDateTestObject(entity);
						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Date expctedDate = dateObject.getDate();

		DateTestObject returnedDateObject = container.getDateTestObject();
		assertNotNull("DateTestObject should not be null", returnedDateObject);
		Date d = returnedDateObject.getDate();
		assertNotNull("Date should not be null", d);
		assertEquals(expctedDate.getYear(), d.getYear());
		assertEquals(expctedDate.getMonth(), d.getMonth());
		assertEquals(expctedDate.getDay(), d.getDay());
		assertEquals(expctedDate.getHours(), d.getHours());
		assertEquals(expctedDate.getMinutes(), d.getMinutes());
		assertEquals(expctedDate.getSeconds(), d.getSeconds());
	}

	public void testSerializationWithComplexObjectsShouldReturnExpectedJsonUsingMobileServiceTable() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		final ComplexPersonTestObject person = new ComplexPersonTestObject("John", "Doe", new Address("1345 Washington St", 1313, "US"));

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
						// Store the request content
						container.setRequestContent(request.getContent());

						ServiceFilterResponseMock mockedResponse = new ServiceFilterResponseMock();
						mockedResponse.setContent("{}");

						responseCallback.onResponse(mockedResponse, null);
					}
				});

				MobileServiceTable<ComplexPersonTestObject> table = client.getTable(tableName, ComplexPersonTestObject.class);

				client.registerSerializer(Address.class, new JsonSerializer<Address>() {

					@Override
					public JsonElement serialize(Address arg0, Type arg1, JsonSerializationContext arg2) {

						JsonObject json = new JsonObject();
						json.addProperty("zipcode", arg0.getZipCode());
						json.addProperty("country", arg0.getCountry());
						json.addProperty("streetaddress", arg0.getStreetAddress());

						return json;
					}
				});

				table.insert(person, new TableOperationCallback<ComplexPersonTestObject>() {

					@Override
					public void onCompleted(ComplexPersonTestObject entity, Exception exception, ServiceFilterResponse response) {
						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		assertEquals(
				"{\"address\":{\"zipcode\":1313,\"country\":\"US\",\"streetaddress\":\"1345 Washington St\"},\"firstName\":\"John\",\"lastName\":\"Doe\"}",
				container.getRequestContent());
	}

	public void testDeserializationWithComplexObjectsShouldReturnExpectedEntityUsingMobileServiceTable() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String tableName = "MyTableName";

		final ComplexPersonTestObject person = new ComplexPersonTestObject("John", "Doe", new Address("1345 Washington St", 1313, "US"));

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

						// Create a mock response simulating an error
						ServiceFilterResponseMock response = new ServiceFilterResponseMock();
						response.setStatus(new StatusLineMock(404));
						response.setContent("{\"address\":{\"zipcode\":1313,\"country\":\"US\",\"streetaddress\":\"1345 Washington St\"},\"firstName\":\"John\",\"lastName\":\"Doe\"}");

						responseCallback.onResponse(response, null);
					}
				});

				MobileServiceTable<ComplexPersonTestObject> table = client.getTable(tableName, ComplexPersonTestObject.class);

				client.registerDeserializer(Address.class, new JsonDeserializer<Address>() {

					@Override
					public Address deserialize(JsonElement arg0, Type arg1, JsonDeserializationContext arg2) throws JsonParseException {

						Address a = new Address(arg0.getAsJsonObject().get("streetaddress").getAsString(), arg0.getAsJsonObject().get("zipcode").getAsInt(),
								arg0.getAsJsonObject().get("country").getAsString());

						return a;
					}
				});

				table.insert(person, new TableOperationCallback<ComplexPersonTestObject>() {

					@Override
					public void onCompleted(ComplexPersonTestObject entity, Exception exception, ServiceFilterResponse response) {
						container.setComplexPerson(entity);
						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Address expectedAddress = person.getAddress();

		ComplexPersonTestObject p = container.getComplexPerson();
		assertNotNull("Person should not be null", p);
		Address a = p.getAddress();
		assertNotNull("Address should not be null", a);
		assertEquals(expectedAddress.getCountry(), a.getCountry());
		assertEquals(expectedAddress.getStreetAddress(), a.getStreetAddress());
		assertEquals(expectedAddress.getZipCode(), a.getZipCode());
	}

}
