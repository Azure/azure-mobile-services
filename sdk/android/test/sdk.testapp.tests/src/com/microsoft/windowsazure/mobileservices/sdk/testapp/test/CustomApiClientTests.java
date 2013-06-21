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

import org.apache.http.client.methods.HttpGet;
import org.apache.http.client.methods.HttpHead;
import org.apache.http.client.methods.HttpPost;

import android.test.InstrumentationTestCase;
import android.util.Pair;

import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import com.microsoft.windowsazure.mobileservices.ApiJsonOperationCallback;
import com.microsoft.windowsazure.mobileservices.ApiOperationCallback;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceException;
import com.microsoft.windowsazure.mobileservices.NextServiceFilterCallback;
import com.microsoft.windowsazure.mobileservices.ServiceFilter;
import com.microsoft.windowsazure.mobileservices.ServiceFilterRequest;
import com.microsoft.windowsazure.mobileservices.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.ServiceFilterResponseCallback;

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
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;
				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
					
					client = client.withFilter(new ServiceFilter() {
						
						@Override
						public void handleRequest(ServiceFilterRequest request,
								NextServiceFilterCallback nextServiceFilterCallback,
								ServiceFilterResponseCallback responseCallback) {

							ServiceFilterResponseMock mockResponse = new ServiceFilterResponseMock();
							mockResponse.setStatus(new StatusLineMock(418)); //I'm a teapot status code
							ServiceFilterRequestMock mockRequest = new ServiceFilterRequestMock(mockResponse);
							
							nextServiceFilterCallback.onNext(mockRequest, responseCallback);
						}
					});
					
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				client.invokeApi("myApi", new byte[] {1, 2, 3, 4}, HttpPost.METHOD_NAME, null, null, new ServiceFilterResponseCallback() {

					@Override
					public void onResponse(ServiceFilterResponse response, Exception exception) {
						container.setException(exception);
						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();
		if (!(exception instanceof MobileServiceException)) {
			fail("Expected Exception MobileServiceException");
		}
	}

	public void testInvokeWithNullApiShouldThrowException() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;
				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				client.invokeApi(null, null, HttpPost.METHOD_NAME, null, null, new ServiceFilterResponseCallback() {

					@Override
					public void onResponse(ServiceFilterResponse response, Exception exception) {
						container.setException(exception);
						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		// Asserts
		Exception exception = container.getException();
		if (!(exception instanceof IllegalArgumentException)) {
			fail("Expected Exception IllegalArgumentException");
		}
	}

	public void testInvokeWithNullMethodShouldThrowException() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;
				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				client.invokeApi("myApi", null, null, null, null, new ServiceFilterResponseCallback() {

					@Override
					public void onResponse(ServiceFilterResponse response, Exception exception) {
						container.setException(exception);
						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();
		if (!(exception instanceof IllegalArgumentException)) {
			fail("Expected Exception IllegalArgumentException");
		}
	}

	public void testInvokeWithNotSupportedMethodShouldThrowException() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;
				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				client.invokeApi("myApi", null, HttpHead.METHOD_NAME, null, null, new ServiceFilterResponseCallback() {

					@Override
					public void onResponse(ServiceFilterResponse response, Exception exception) {
						container.setException(exception);
						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();
		if (!(exception instanceof IllegalArgumentException)) {
			fail("Expected Exception IllegalArgumentException");
		}
	}

	public void testInvokeGenericWithNullClassShouldThrowException() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;
				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				client.invokeApi("myApi", null, HttpPost.METHOD_NAME, null, null, new ApiOperationCallback<String>() {

					@Override
					public void onCompleted(String result, Exception exception, ServiceFilterResponse response) {
						container.setException(exception);

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();
		if (!(exception instanceof IllegalArgumentException)) {
			fail("Expected Exception IllegalArgumentException");
		}
	}

	public void testInvokeTypedSingleObject() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final PersonTestObject p = new PersonTestObject("john", "doe", 30);

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
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
	
	public void testInvokeTypedSingleString() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final String s = "Hello world";

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;
				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				client = client.withFilter(new EchoFilter());

				client.invokeApi("myApi", s, HttpPost.METHOD_NAME, null, String.class, new ApiOperationCallback<String>() {

					@Override
					public void onCompleted(String result, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						} else if (result == null) {
							container.setException(new Exception("Expected one string result"));
						} else {
							container.setCustomResult(result);
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();
		if (exception != null) {
			fail(exception.getMessage());
		} else {
			assertEquals(s, container.getCustomResult());
		}
	}
	
	public void testInvokeTypedSingleInteger() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final Integer i = 42;

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;
				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				client = client.withFilter(new EchoFilter());

				client.invokeApi("myApi", i, HttpPost.METHOD_NAME, null, Integer.class, new ApiOperationCallback<Integer>() {

					@Override
					public void onCompleted(Integer result, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						} else if (result == null) {
							container.setException(new Exception("Expected one integer result"));
						} else {
							container.setCustomResult(result);
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();
		if (exception != null) {
			fail(exception.getMessage());
		} else {
			assertEquals(i, container.getCustomResult());
		}
	}
	
	public void testInvokeTypedSingleFloat() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final Float f = 3.14f;

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;
				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				client = client.withFilter(new EchoFilter());

				client.invokeApi("myApi", f, HttpPost.METHOD_NAME, null, Float.class, new ApiOperationCallback<Float>() {

					@Override
					public void onCompleted(Float result, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						} else if (result == null) {
							container.setException(new Exception("Expected one float result"));
						} else {
							container.setCustomResult(result);
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();
		if (exception != null) {
			fail(exception.getMessage());
		} else {
			assertEquals(f, container.getCustomResult());
		}
	}
	
	public void testInvokeTypedSingleBoolean() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final Boolean b = true;

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;
				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				client = client.withFilter(new EchoFilter());

				client.invokeApi("myApi", b, HttpPost.METHOD_NAME, null, Boolean.class, new ApiOperationCallback<Boolean>() {

					@Override
					public void onCompleted(Boolean result, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						} else if (result == null) {
							container.setException(new Exception("Expected one boolean result"));
						} else {
							container.setCustomResult(result);
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();
		if (exception != null) {
			fail(exception.getMessage());
		} else {
			assertEquals(b, container.getCustomResult());
		}
	}

	public void testInvokeTypedMultipleObject() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final PersonTestObject p1 = new PersonTestObject("john", "doe", 30);
		final PersonTestObject p2 = new PersonTestObject("jane", "does", 31);

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;
				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				client = client.withFilter(new EchoFilter());

				List<PersonTestObject> people = new ArrayList<PersonTestObject>();
				people.add(p1);
				people.add(p2);

				client.invokeApi("myApi", people, HttpPost.METHOD_NAME, null, PersonTestObject[].class, new ApiOperationCallback<PersonTestObject[]>() {

					@Override
					public void onCompleted(PersonTestObject[] entities, Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							container.setException(exception);
						} else if (entities == null || entities.length != 2) {
							container.setException(new Exception("Expected two person result"));
						} else {
							container.setPeopleResult(entities);
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

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
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final JsonObject json = new JsonParser().parse("{\"message\": \"hello world\"}").getAsJsonObject();

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
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
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		final byte[] content = UUID.randomUUID().toString().getBytes(MobileServiceClient.UTF8_ENCODING);

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;
				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				client = client.withFilter(new EchoFilter());

				client.invokeApi("myApi", content, HttpPost.METHOD_NAME, null, null, new ServiceFilterResponseCallback() {

					@Override
					public void onResponse(ServiceFilterResponse response, Exception exception) {
						if (exception != null) {
							container.setException(exception);
						} else if (response == null || response.getRawContent() == null) {
							container.setException(new Exception("Expected response"));
						} else {
							container.setRawResponseContent(response.getRawContent());
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

		// Asserts
		Exception exception = container.getException();
		if (exception != null) {
			fail(exception.getMessage());
		} else {
			assertEquals(content, container.getRawResponseContent());
		}
	}

	public void testInvokeMethodEcho() throws Throwable {
		final CountDownLatch latch = new CountDownLatch(1);

		// Container to store callback's results and do the asserts.
		final ResultsContainer container = new ResultsContainer();

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;
				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				client = client.withFilter(new HttpMetaEchoFilter());

				client.invokeApi("myApi", null, HttpGet.METHOD_NAME, null, null, new ServiceFilterResponseCallback() {

					@Override
					public void onResponse(ServiceFilterResponse response, Exception exception) {
						if (exception != null) {
							container.setException(exception);
						} else if (response == null || response.getContent() == null) {
							container.setException(new Exception("Expected response"));
						} else {
							container.setResponseValue(response.getContent());
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

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
		final CountDownLatch latch = new CountDownLatch(1);

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

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;
				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				client = client.withFilter(new HttpMetaEchoFilter());

				client.invokeApi("myApi", null, HttpPost.METHOD_NAME, headers, null, new ServiceFilterResponseCallback() {

					@Override
					public void onResponse(ServiceFilterResponse response, Exception exception) {
						if (exception != null) {
							container.setException(exception);
						} else if (response == null || response.getContent() == null) {
							container.setException(new Exception("Expected response"));
						} else {
							container.setResponseValue(response.getContent());
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

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
		final CountDownLatch latch = new CountDownLatch(1);

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

		runTestOnUiThread(new Runnable() {

			@Override
			public void run() {
				MobileServiceClient client = null;
				try {
					client = new MobileServiceClient(appUrl, appKey, getInstrumentation().getTargetContext());
				} catch (MalformedURLException e) {
					e.printStackTrace();
				}

				client = client.withFilter(new HttpMetaEchoFilter());

				client.invokeApi("myApi", null, HttpPost.METHOD_NAME, null, parameters, new ServiceFilterResponseCallback() {

					@Override
					public void onResponse(ServiceFilterResponse response, Exception exception) {
						if (exception != null) {
							container.setException(exception);
						} else if (response == null || response.getContent() == null) {
							container.setException(new Exception("Expected response"));
						} else {
							container.setResponseValue(response.getContent());
						}

						latch.countDown();
					}
				});
			}
		});

		latch.await();

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
