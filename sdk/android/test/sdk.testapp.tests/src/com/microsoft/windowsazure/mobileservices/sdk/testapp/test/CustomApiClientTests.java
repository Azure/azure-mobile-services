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
import java.util.ArrayList;
import java.util.List;
import java.util.UUID;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.ExecutionException;

import org.apache.http.client.methods.HttpGet;
import org.apache.http.client.methods.HttpHead;
import org.apache.http.client.methods.HttpPost;

import android.test.InstrumentationTestCase;
import android.util.Pair;

import com.google.common.util.concurrent.ListenableFuture;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import com.microsoft.windowsazure.mobileservices.ApiJsonOperationCallback;
import com.microsoft.windowsazure.mobileservices.ApiOperationCallback;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceException;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilter;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterRequest;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.http.NextServiceFilterCallback;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.test.types.PersonTestObject;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.test.types.ResultsContainer;

public class CustomApiClientTests extends InstrumentationTestCase {
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

	public void testResponseWithNon2xxStatusShouldThrowException() throws Throwable {

		final ResultsContainer container = new ResultsContainer();

		MobileServiceClient client = null;

		try {
			client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());

			client = client.withFilter(new ServiceFilter() {

				@Override
				public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

					ServiceFilterResponseMock mockResponse = new ServiceFilterResponseMock();
					mockResponse.setStatus(new StatusLineMock(418)); // I'm a
																		// teapot
																		// status
																		// code
					ServiceFilterRequestMock mockRequest = new ServiceFilterRequestMock(mockResponse);

					return nextServiceFilterCallback.onNext(mockRequest);
				}
			});

			List<Pair<String, String>> mockHeaders = new ArrayList<Pair<String, String>>();
			List<Pair<String, String>> mockParameters = new ArrayList<Pair<String, String>>();

			client.invokeApi("myApi", new byte[] { 1, 2, 3, 4 }, HttpPost.METHOD_NAME, mockHeaders, mockParameters).get();

		} catch (Exception exception) {
			if (exception instanceof ExecutionException) {
				container.setException((Exception) exception.getCause());
			} else {
				container.setException(exception);
			}
		}

		// Asserts
		Exception exception = container.getException();
		if (!(exception instanceof MobileServiceException)) {
			fail("Expected Exception MobileServiceException");
		}
	}

	public void testInvokeWithNullApiShouldThrowException() throws Throwable {
		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		MobileServiceClient client = null;
		try {
			client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
			client.invokeApi(null, new Object(), null).get();
		} catch (Exception exception) {
			container.setException(exception);
		}

		// Asserts
		Exception exception = container.getException();
		if (!(exception instanceof IllegalArgumentException)) {
			fail("Expected Exception IllegalArgumentException");
		}
	}

	public void testInvokeWithNullMethodShouldThrowException() throws Throwable {

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		MobileServiceClient client = null;
		try {
			client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
			client.invokeApi("myApi", new Object(), null, null, null).get();

		} catch (Exception exception) {
			container.setException(exception);
		}

		// Asserts
		Exception exception = container.getException();
		if (!(exception instanceof IllegalArgumentException)) {
			fail("Expected Exception IllegalArgumentException");
		}
	}

	public void testInvokeWithNotSupportedMethodShouldThrowException() throws Throwable {

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		MobileServiceClient client = null;
		try {
			client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
			client.invokeApi("myApi", new Object(), HttpHead.METHOD_NAME, null, null).get();
		} catch (Exception exception) {
			container.setException(exception);
		}

		// Asserts
		Exception exception = container.getException();
		if (!(exception instanceof IllegalArgumentException)) {
			fail("Expected Exception IllegalArgumentException");
		}
	}

	public void testInvokeGenericWithNullClassShouldThrowException() throws Throwable {

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		MobileServiceClient client = null;
		try {
			client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
			client.invokeApi("myApi", new Object(), HttpPost.METHOD_NAME, null, null).get();

		} catch (Exception exception) {
			container.setException(exception);
		}

		// Asserts
		Exception exception = container.getException();
		if (!(exception instanceof IllegalArgumentException)) {
			fail("Expected Exception IllegalArgumentException");
		}
	}

	public void testInvokeTypedSingleObject() throws Throwable {

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final PersonTestObject p = new PersonTestObject("john", "doe", 30);

		MobileServiceClient client = null;
		try {

			client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());

			client = client.withFilter(new EchoFilter());

			PersonTestObject result = client.invokeApi("myApi", p, HttpPost.METHOD_NAME, null, PersonTestObject.class).get();

			if (result == null) {
				container.setException(new Exception("Expected one person result"));
			} else {
				container.setPerson(result);
			}

		} catch (Exception exception) {
			container.setException(exception);
		}

		// Asserts
		Exception exception = container.getException();
		if (exception != null) {
			fail(exception.getMessage());
		} else {
			assertEquals(p.getFirstName(), container.getPerson().getFirstName());
			assertEquals(p.getLastName(), container.getPerson().getLastName());
			assertEquals(p.getAge(), container.getPerson().getAge());
		}
	}

	@SuppressWarnings("deprecation")
	public void testInvokeTypedSingleObjectCallback() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final PersonTestObject p = new PersonTestObject("john", "doe", 30);

		MobileServiceClient client = null;
		try {
			client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
		} catch (MalformedURLException e) {
			e.printStackTrace();
		}

		client = client.withFilter(new EchoFilter());

		client.invokeApi("myApi", p, HttpPost.METHOD_NAME, null, PersonTestObject.class, new ApiOperationCallback<PersonTestObject>() {

			@Override
			public void onCompleted(PersonTestObject result, Exception exception, ServiceFilterResponse response) {
				if (exception != null) {
					container.setException(exception);
				} else if (result == null) {
					container.setException(new Exception("Expected one person result"));
				} else {
					container.setPerson(result);
				}

				latch.countDown();
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();
		if (exception != null) {
			fail(exception.getMessage());
		} else {
			assertEquals(p.getFirstName(), container.getPerson().getFirstName());
			assertEquals(p.getLastName(), container.getPerson().getLastName());
			assertEquals(p.getAge(), container.getPerson().getAge());
		}
	}

	public void testInvokeTypedJsonObjectAsInput() throws Throwable {
		final JsonObject input = new JsonObject();
		input.addProperty("firstName", "john");
		input.addProperty("lastName", "doe");
		input.addProperty("age", 30);

		MobileServiceClient client = null;
		try {
			client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
		} catch (MalformedURLException e) {
			e.printStackTrace();
		}

		client = client.withFilter(new EchoFilter());

		try {
			PersonTestObject result = client.invokeApi("myApi", input, HttpPost.METHOD_NAME, null, PersonTestObject.class).get();
			if (result == null) {
				fail("Expected one person result");
			} else {
				assertEquals(input.get("firstName").getAsString(), result.getFirstName());
				assertEquals(input.get("lastName").getAsString(), result.getLastName());
				assertEquals(input.get("age").getAsInt(), result.getAge());
			}

		} catch (Exception exception) {
			if (exception instanceof ExecutionException) {
				fail(exception.getCause().getMessage());
			} else {
				fail(exception.getMessage());
			}
		}
	}

	public void testInvokeTypedSingleString() throws Throwable {

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String s = "Hello world";

		MobileServiceClient client = null;
		try {
			client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
			client = client.withFilter(new EchoFilter());
			String result = client.invokeApi("myApi", s, HttpPost.METHOD_NAME, null, String.class).get();

			if (result == null) {
				container.setException(new Exception("Expected one string result"));
			} else {
				container.setCustomResult(result);
			}

		} catch (Exception exception) {
			container.setException(exception);
		}

		// Asserts
		Exception exception = container.getException();
		if (exception != null) {
			fail(exception.getMessage());
		} else {
			assertEquals(s, container.getCustomResult());
		}
	}

	public void testInvokeTypedSingleInteger() throws Throwable {

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final Integer i = 42;

		MobileServiceClient client = null;

		try {
			client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());

			client = client.withFilter(new EchoFilter());

			Integer result = client.invokeApi("myApi", i, HttpPost.METHOD_NAME, null, Integer.class).get();

			if (result == null) {
				container.setException(new Exception("Expected one integer result"));
			} else {
				container.setCustomResult(result);
			}

		} catch (Exception exception) {
			container.setException(exception);
		}

		// Asserts
		Exception exception = container.getException();
		if (exception != null) {
			fail(exception.getMessage());
		} else {
			assertEquals(i, container.getCustomResult());
		}
	}

	public void testInvokeTypedSingleFloat() throws Throwable {

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final Float f = 3.14f;

		MobileServiceClient client = null;

		try {
			client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
			client = client.withFilter(new EchoFilter());

			Float result = client.invokeApi("myApi", f, HttpPost.METHOD_NAME, null, Float.class).get();

			if (result == null) {
				container.setException(new Exception("Expected one float result"));
			} else {
				container.setCustomResult(result);
			}

		} catch (Exception exception) {
			container.setException(exception);

		}

		// Asserts
		Exception exception = container.getException();
		if (exception != null) {
			fail(exception.getMessage());
		} else {
			assertEquals(f, container.getCustomResult());
		}
	}

	public void testInvokeTypedSingleBoolean() throws Throwable {

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final Boolean b = true;

		MobileServiceClient client = null;
		try {
			client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());

			client = client.withFilter(new EchoFilter());

			Boolean result = client.invokeApi("myApi", b, HttpPost.METHOD_NAME, null, Boolean.class).get();

			if (result == null) {
				container.setException(new Exception("Expected one boolean result"));
			} else {
				container.setCustomResult(result);
			}

		} catch (Exception exception) {
			container.setException(exception);
		}

		// Asserts
		Exception exception = container.getException();
		if (exception != null) {
			fail(exception.getMessage());
		} else {
			assertEquals(b, container.getCustomResult());
		}
	}

	public void testInvokeTypedMultipleObject() throws Throwable {

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final PersonTestObject p1 = new PersonTestObject("john", "doe", 30);
		final PersonTestObject p2 = new PersonTestObject("jane", "does", 31);

		MobileServiceClient client = null;
		try {
			client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());

			client = client.withFilter(new EchoFilter());

			List<PersonTestObject> people = new ArrayList<PersonTestObject>();
			people.add(p1);
			people.add(p2);

			PersonTestObject[] entities = client.invokeApi("myApi", people, HttpPost.METHOD_NAME, null, PersonTestObject[].class).get();

			if (entities == null || entities.length != 2) {
				container.setException(new Exception("Expected two person result"));
			} else {
				container.setPeopleResult(entities);
			}

		} catch (Exception exception) {
			container.setException(exception);
		}

		// Asserts
		Exception exception = container.getException();
		if (exception != null) {
			fail(exception.getMessage());
		} else {
			assertEquals(p1.getFirstName(), container.getPeopleResult().get(0).getFirstName());
			assertEquals(p1.getLastName(), container.getPeopleResult().get(0).getLastName());
			assertEquals(p1.getAge(), container.getPeopleResult().get(0).getAge());

			assertEquals(p2.getFirstName(), container.getPeopleResult().get(1).getFirstName());
			assertEquals(p2.getLastName(), container.getPeopleResult().get(1).getLastName());
			assertEquals(p2.getAge(), container.getPeopleResult().get(1).getAge());
		}
	}

	public void testInvokeJsonEcho() throws Throwable {

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final JsonObject json = new JsonParser().parse("{\"message\": \"hello world\"}").getAsJsonObject();

		MobileServiceClient client = null;
		try {
			client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());

			client = client.withFilter(new EchoFilter());

			JsonElement result = client.invokeApi("myApi", json, HttpPost.METHOD_NAME, null).get();

			if (result == null) {
				container.setException(new Exception("Expected result"));
			} else {
				container.setJsonResult(result);
			}

		} catch (Exception exception) {
			container.setException(exception);
		}

		// Asserts
		Exception exception = container.getException();
		if (exception != null) {
			fail(exception.getMessage());
		} else {
			assertEquals(1, container.getJsonResult().getAsJsonObject().entrySet().size());
			assertTrue(container.getJsonResult().getAsJsonObject().has("message"));
			assertEquals(json.get("message"), container.getJsonResult().getAsJsonObject().get("message"));
		}
	}

	@SuppressWarnings("deprecation")
	public void testInvokeJsonEchoCallback() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final JsonObject json = new JsonParser().parse("{\"message\": \"hello world\"}").getAsJsonObject();

		MobileServiceClient client = null;
		try {
			client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
		} catch (MalformedURLException e) {
			e.printStackTrace();
		}

		client = client.withFilter(new EchoFilter());

		client.invokeApi("myApi", json, HttpPost.METHOD_NAME, null, new ApiJsonOperationCallback() {

			@Override
			public void onCompleted(JsonElement result, Exception exception, ServiceFilterResponse response) {
				if (exception != null) {
					container.setException(exception);
				} else if (result == null) {
					container.setException(new Exception("Expected result"));
				} else {
					container.setJsonResult(result);
				}

				latch.countDown();
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();
		if (exception != null) {
			fail(exception.getMessage());
		} else {
			assertEquals(1, container.getJsonResult().getAsJsonObject().entrySet().size());
			assertTrue(container.getJsonResult().getAsJsonObject().has("message"));
			assertEquals(json.get("message"), container.getJsonResult().getAsJsonObject().get("message"));
		}
	}

	public void testInvokeRandomByteEcho() throws Throwable {

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final byte[] content = UUID.randomUUID().toString().getBytes(MobileServiceClient.UTF8_ENCODING);

		MobileServiceClient client = null;
		try {
			client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());

			client = client.withFilter(new EchoFilter());

			List<Pair<String, String>> mockHeaders = new ArrayList<Pair<String, String>>();
			List<Pair<String, String>> mockParameters = new ArrayList<Pair<String, String>>();

			ServiceFilterResponse response = client.invokeApi("myApi", content, HttpPost.METHOD_NAME, mockHeaders, mockParameters).get();

			if (response == null || response.getRawContent() == null) {
				container.setException(new Exception("Expected response"));
			} else {
				container.setRawResponseContent(response.getRawContent());
			}

		} catch (Exception exception) {
			container.setException(exception);
		}

		// Asserts
		Exception exception = container.getException();
		if (exception != null) {
			fail(exception.getMessage());
		} else {
			assertEquals(content, container.getRawResponseContent());
		}
	}

	public void testInvokeBytesShouldNotModifyLists() {
		final byte[] content = new byte[] { 1, 2, 3, 4 };
		try {
			MobileServiceClient client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
			client = client.withFilter(new EchoFilter());

			List<Pair<String, String>> expectedHeaders = new ArrayList<Pair<String, String>>();
			expectedHeaders.add(new Pair<String, String>("header1", "value1"));
			expectedHeaders.add(new Pair<String, String>("header2", "value2"));
			List<Pair<String, String>> expectedQueryParameters = new ArrayList<Pair<String, String>>();
			expectedQueryParameters.add(new Pair<String, String>("query1", "value1"));
			expectedQueryParameters.add(new Pair<String, String>("query2", "value2"));

			List<Pair<String, String>> headers = new ArrayList<Pair<String, String>>(expectedHeaders); 
			List<Pair<String, String>> queryParameters = new ArrayList<Pair<String, String>>(expectedQueryParameters); 
			client.invokeApi("myApi", content, "POST", headers, queryParameters).get();

			assertEquals(expectedHeaders.size(), headers.size());
			for (int i = 0; i < expectedHeaders.size(); i++) {
				assertEquals(expectedHeaders.get(i).first, headers.get(i).first);
				assertEquals(expectedHeaders.get(i).second, headers.get(i).second);
			}

			assertEquals(expectedQueryParameters.size(), queryParameters.size());
			for (int i = 0; i < expectedQueryParameters.size(); i++) {
				assertEquals(expectedQueryParameters.get(i).first, queryParameters.get(i).first);
				assertEquals(expectedQueryParameters.get(i).second, queryParameters.get(i).second);
			}
		} catch (Exception exception) {
			fail(exception.getMessage());
		}
	}

	public void testInvokeMethodEcho() throws Throwable {

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		MobileServiceClient client = null;
		try {
			client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());

			client = client.withFilter(new HttpMetaEchoFilter());

			List<Pair<String, String>> mockHeaders = new ArrayList<Pair<String, String>>();
			List<Pair<String, String>> mockParameters = new ArrayList<Pair<String, String>>();

			ServiceFilterResponse response = client.invokeApi("myApi", null, HttpGet.METHOD_NAME, mockHeaders, mockParameters).get();

			if (response == null || response.getContent() == null) {
				container.setException(new Exception("Expected response"));
			} else {
				container.setResponseValue(response.getContent());
			}

		} catch (Exception exception) {
			container.setException(exception);
		}

		// Asserts
		Exception exception = container.getException();
		if (exception != null) {
			fail(exception.getMessage());
		} else {
			JsonObject jResponse = (new JsonParser()).parse(container.getResponseValue()).getAsJsonObject();
			assertEquals(HttpGet.METHOD_NAME, jResponse.get("method").getAsString());
		}
	}

	public void testInvokeHeadersEcho() throws Throwable {

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();
		final List<Pair<String, String>> headers = new ArrayList<Pair<String, String>>();
		final List<String> headerNames = new ArrayList<String>();

		for (int i = 0; i < 10; i++) {
			String name;

			do {
				name = UUID.randomUUID().toString();
			} while (headerNames.contains(name));

			headers.add(new Pair<String, String>(name, UUID.randomUUID().toString()));
			headerNames.add(name);
		}

		MobileServiceClient client = null;
		try {
			client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());

			client = client.withFilter(new HttpMetaEchoFilter());

			List<Pair<String, String>> fakeParameters = new ArrayList<Pair<String, String>>();

			ServiceFilterResponse response = client.invokeApi("myApi", null, HttpPost.METHOD_NAME, headers, fakeParameters).get();

			if (response == null || response.getContent() == null) {
				container.setException(new Exception("Expected response"));
			} else {
				container.setResponseValue(response.getContent());
			}

		} catch (Exception exception) {
			container.setException(exception);
		}

		// Asserts
		Exception exception = container.getException();
		if (exception != null) {
			fail(exception.getMessage());
		} else {
			JsonObject jResponse = (new JsonParser()).parse(container.getResponseValue()).getAsJsonObject();

			if (jResponse.has("headers") && jResponse.get("headers").isJsonObject()) {
				JsonObject jHeaders = jResponse.getAsJsonObject("headers");

				for (Pair<String, String> header : headers) {
					if (jHeaders.has(header.first)) {
						assertEquals(header.second, jHeaders.get(header.first).getAsString());
					} else {
						fail("Expected header.");
					}
				}

			} else {
				fail("Expected headers.");
			}

		}
	}

	public void testInvokeParametersEcho() throws Throwable {

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();
		final List<Pair<String, String>> parameters = new ArrayList<Pair<String, String>>();
		final List<String> parameterNames = new ArrayList<String>();

		for (int i = 0; i < 10; i++) {
			String name;

			do {
				name = UUID.randomUUID().toString();
			} while (parameterNames.contains(name));

			parameters.add(new Pair<String, String>(name, UUID.randomUUID().toString()));
			parameterNames.add(name);
		}

		MobileServiceClient client = null;
		try {
			client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());

			client = client.withFilter(new HttpMetaEchoFilter());

			ServiceFilterResponse response = client.invokeApi("myApi", null, HttpPost.METHOD_NAME, null, parameters).get();

			if (response == null || response.getContent() == null) {
				container.setException(new Exception("Expected response"));
			} else {
				container.setResponseValue(response.getContent());
			}

		} catch (Exception exception) {
			container.setException(exception);
		}

		// Asserts
		Exception exception = container.getException();
		if (exception != null) {
			fail(exception.getMessage());
		} else {
			JsonObject jResponse = (new JsonParser()).parse(container.getResponseValue()).getAsJsonObject();

			if (jResponse.has("parameters") && jResponse.get("parameters").isJsonObject()) {
				JsonObject jParameters = jResponse.getAsJsonObject("parameters");

				for (Pair<String, String> parameter : parameters) {
					if (jParameters.has(parameter.first)) {
						assertEquals(parameter.second, jParameters.get(parameter.first).getAsString());
					} else {
						fail("Expected parameter.");
					}
				}

			} else {
				fail("Expected parameters.");
			}
		}
	}
}