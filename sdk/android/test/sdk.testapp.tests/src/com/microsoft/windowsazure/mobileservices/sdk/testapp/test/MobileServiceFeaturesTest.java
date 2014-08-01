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
import java.util.EnumSet;
import java.util.Hashtable;
import java.util.List;
import java.util.concurrent.ExecutionException;

import org.apache.http.Header;
import com.google.common.util.concurrent.ListenableFuture;
import com.google.common.util.concurrent.SettableFuture;
import com.google.gson.JsonObject;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceException;
import com.microsoft.windowsazure.mobileservices.MobileServiceFeatures;
import com.microsoft.windowsazure.mobileservices.http.NextServiceFilterCallback;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilter;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterRequest;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.table.MobileServiceJsonTable;
import com.microsoft.windowsazure.mobileservices.table.MobileServiceTable;

import android.test.InstrumentationTestCase;
import android.util.Pair;

public class MobileServiceFeaturesTest extends InstrumentationTestCase {
	String appUrl;
	String appKey;

	protected void setUp() throws Exception {
		appUrl = "http://myapp.com/";
		appKey = "qwerty";
		super.setUp();
	}

	protected void tearDown() throws Exception {
		super.tearDown();
	}

	public void testFeaturesToStringConversion() {
		Hashtable<EnumSet<MobileServiceFeatures>, String> cases;
		cases = new Hashtable<EnumSet<MobileServiceFeatures>, String>();
		for (MobileServiceFeatures feature : MobileServiceFeatures.class.getEnumConstants()) {
			cases.put(EnumSet.of(feature), feature.getValue());
		}
		cases.put(EnumSet.of(MobileServiceFeatures.TypedTable, MobileServiceFeatures.AdditionalQueryParameters), "QS,TT");
		cases.put(EnumSet.of(MobileServiceFeatures.UntypedTable, MobileServiceFeatures.AdditionalQueryParameters), "QS,TU");
		cases.put(EnumSet.of(MobileServiceFeatures.TypedTable, MobileServiceFeatures.Offline), "OL,TT");
		cases.put(EnumSet.of(MobileServiceFeatures.UntypedTable, MobileServiceFeatures.Offline), "OL,TU");
		cases.put(EnumSet.of(MobileServiceFeatures.TypedApiCall, MobileServiceFeatures.AdditionalQueryParameters), "AT,QS");
		cases.put(EnumSet.of(MobileServiceFeatures.JsonApiCall, MobileServiceFeatures.AdditionalQueryParameters), "AJ,QS");
		
		for (EnumSet<MobileServiceFeatures> features : cases.keySet()) {
			String expected = cases.get(features);
			String actual = MobileServiceFeatures.featuresToString(features);
			assertEquals(expected, actual);
		}
	}

	// Tests for typed tables
	interface TypedTableTestOperation {
		void executeOperation(MobileServiceTable<PersonTestObjectWithStringId> table) throws Exception;
	}

	public void testTypedTableInsertFeatureHeader() {
		testTypedTableFeatureHeader(new TypedTableTestOperation() {

			@Override
			public void executeOperation(MobileServiceTable<PersonTestObjectWithStringId> table) throws Exception {
				PersonTestObjectWithStringId pto = new PersonTestObjectWithStringId("John", "Doe", 33);
				table.insert(pto).get();
			}
		}, false, "TT");
	}

	public void testTypedTableInsertWithParametersFeatureHeader() {
		testTypedTableFeatureHeader(new TypedTableTestOperation() {

			@Override
			public void executeOperation(MobileServiceTable<PersonTestObjectWithStringId> table) throws Exception {
				PersonTestObjectWithStringId pto = new PersonTestObjectWithStringId("John", "Doe", 33);
				List<Pair<String, String>> queryParams = new ArrayList<Pair<String, String>>();
				queryParams.add(new Pair<String, String>("a", "b"));
				table.insert(pto, queryParams).get();
			}
		}, false, "QS,TT");
	}

	public void testTypedTableInsertWithEmptyParametersFeatureHeader() {
		testTypedTableFeatureHeader(new TypedTableTestOperation() {

			@Override
			public void executeOperation(MobileServiceTable<PersonTestObjectWithStringId> table) throws Exception {
				PersonTestObjectWithStringId pto = new PersonTestObjectWithStringId("John", "Doe", 33);
				List<Pair<String, String>> queryParams = new ArrayList<Pair<String, String>>();
				table.insert(pto, queryParams).get();
			}
		}, false, "TT");
	}

	public void testTypedTableUpdateFeatureHeader() {
		testTypedTableFeatureHeader(new TypedTableTestOperation() {

			@Override
			public void executeOperation(MobileServiceTable<PersonTestObjectWithStringId> table) throws Exception {
				PersonTestObjectWithStringId pto = new PersonTestObjectWithStringId("John", "Doe", 33);
				pto.setId("the-id");
				table.update(pto).get();
			}
		}, false, "TT");
	}

	public void testTypedTableUpdateWithParametersFeatureHeader() {
		testTypedTableFeatureHeader(new TypedTableTestOperation() {

			@Override
			public void executeOperation(MobileServiceTable<PersonTestObjectWithStringId> table) throws Exception {
				PersonTestObjectWithStringId pto = new PersonTestObjectWithStringId("John", "Doe", 33);
				pto.setId("the-id");
				List<Pair<String, String>> queryParams = new ArrayList<Pair<String, String>>();
				queryParams.add(new Pair<String, String>("a", "b"));
				table.update(pto, queryParams).get();
			}
		}, false, "QS,TT");
	}

	public void testTypedTableDeleteNoParametersNoFeatureHeader() {
		testTypedTableFeatureHeader(new TypedTableTestOperation() {

			@Override
			public void executeOperation(MobileServiceTable<PersonTestObjectWithStringId> table) throws Exception {
				PersonTestObjectWithStringId pto = new PersonTestObjectWithStringId("John", "Doe", 33);
				pto.setId("the-id");
				table.delete(pto).get();
			}
		}, false, "TT");
	}

	public void testTypedTableDeleteWithParametersFeatureHeader() {
		testTypedTableFeatureHeader(new TypedTableTestOperation() {

			@Override
			public void executeOperation(MobileServiceTable<PersonTestObjectWithStringId> table) throws Exception {
				PersonTestObjectWithStringId pto = new PersonTestObjectWithStringId("John", "Doe", 33);
				pto.setId("the-id");
				List<Pair<String, String>> queryParams = new ArrayList<Pair<String, String>>();
				queryParams.add(new Pair<String, String>("a", "b"));
				table.delete(pto, queryParams).get();
			}
		}, false, "QS,TT");
	}

	public void testTypedTableLookupFeatureHeader() {
		testTypedTableFeatureHeader(new TypedTableTestOperation() {

			@Override
			public void executeOperation(MobileServiceTable<PersonTestObjectWithStringId> table) throws Exception {
				table.lookUp("1").get();
			}
		}, false, "TT");
	}

	public void testTypedTableLookupWithParametersFeatureHeader() {
		testTypedTableFeatureHeader(new TypedTableTestOperation() {

			@Override
			public void executeOperation(MobileServiceTable<PersonTestObjectWithStringId> table) throws Exception {
				List<Pair<String, String>> queryParams = new ArrayList<Pair<String, String>>();
				queryParams.add(new Pair<String, String>("a", "b"));
				table.lookUp("1", queryParams).get();
			}
		}, false, "QS,TT");
	}

	public void testTypedTableReadFeatureHeader() {
		testTypedTableFeatureHeader(new TypedTableTestOperation() {

			@Override
			public void executeOperation(MobileServiceTable<PersonTestObjectWithStringId> table) throws Exception {
				table.execute().get();
			}
		}, true, "TT");
	}

	public void testTypedQueryFeatureHeader() {
		testTypedTableFeatureHeader(new TypedTableTestOperation() {

			@Override
			public void executeOperation(MobileServiceTable<PersonTestObjectWithStringId> table) throws Exception {
				table.parameter("a", "b").execute().get();
			}
		}, true, "QS,TT");
	}

	private void testTypedTableFeatureHeader(TypedTableTestOperation operation, final boolean responseIsArray, final String expectedFeaturesHeader) {
		MobileServiceClient client = null;
		try {
			client = new MobileServiceClient(appUrl, appKey,
					getInstrumentation().getTargetContext());
		} catch (MalformedURLException e) {
			e.printStackTrace();
		}

		// Add a new filter to the client
		client = client.withFilter(new ServiceFilter() {

			@Override
			public ListenableFuture<ServiceFilterResponse> handleRequest(
					ServiceFilterRequest request,
					NextServiceFilterCallback nextServiceFilterCallback) {

				final SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture
						.create();
				String featuresHeaderName = "X-ZUMO-FEATURES";
				
				Header[] headers = request.getHeaders();
				String features = null;
				for (int i = 0; i < headers.length; i++) {
					if (headers[i].getName() == featuresHeaderName) {
						features = headers[i].getValue();
					}
				}

				boolean error = false;
				if (features == null) {
					if (expectedFeaturesHeader != null) {
						resultFuture.setException(new Exception("No " + featuresHeaderName + " header on API call"));
						error = true;
					}
				} else if (!features.equals(expectedFeaturesHeader)) {
					resultFuture.setException(new Exception("Incorrect features header; expected " + 
						expectedFeaturesHeader + ", actual " + features));
					error = true;
				}
				
				if (!error) {
					ServiceFilterResponseMock response = new ServiceFilterResponseMock();
					String content = "{\"id\":\"the-id\",\"firstName\":\"John\",\"lastName\":\"Doe\",\"age\":33}";
					if (responseIsArray) {
						content = "[" + content + "]";
					}
					response.setContent(content);
					resultFuture.set(response);
				}

				return resultFuture;
			}
		});

		try {
			MobileServiceTable<PersonTestObjectWithStringId> table = client.getTable(PersonTestObjectWithStringId.class);
			operation.executeOperation(table);
		} catch (Exception exception) {
			Throwable ex = exception;
			while (ex instanceof ExecutionException || ex instanceof MobileServiceException) {
				ex = ex.getCause();
			}
			ex.printStackTrace();
			fail(ex.getMessage());
		}
	}

	// Tests for untyped/JSON tables
	interface JsonTableTestOperation {
		void executeOperation(MobileServiceJsonTable table) throws Exception;
	}

	private JsonObject createJsonObject() {
		JsonObject result = new JsonObject();
		result.addProperty("id", "the-id");
		result.addProperty("firstName", "John");
		result.addProperty("lastName", "Doe");
		result.addProperty("age", 33);
		return result;
	}

	public void testJsonTableInsertFeatureHeader() {
		testJsonTableFeatureHeader(new JsonTableTestOperation() {

			@Override
			public void executeOperation(MobileServiceJsonTable table) throws Exception {
				JsonObject jo = createJsonObject();
				table.insert(jo).get();
			}
		}, false, "TU");
	}

	public void testJsonTableInsertWithParametersFeatureHeader() {
		testJsonTableFeatureHeader(new JsonTableTestOperation() {

			@Override
			public void executeOperation(MobileServiceJsonTable table) throws Exception {
				JsonObject jo = createJsonObject();
				List<Pair<String, String>> queryParams = new ArrayList<Pair<String, String>>();
				queryParams.add(new Pair<String, String>("a", "b"));
				table.insert(jo, queryParams).get();
			}
		}, false, "QS,TU");
	}

	public void testJsonTableInsertWithEmptyParametersFeatureHeader() {
		testJsonTableFeatureHeader(new JsonTableTestOperation() {

			@Override
			public void executeOperation(MobileServiceJsonTable table) throws Exception {
				JsonObject jo = createJsonObject();
				List<Pair<String, String>> queryParams = new ArrayList<Pair<String, String>>();
				table.insert(jo, queryParams).get();
			}
		}, false, "TU");
	}

	public void testJsonTableUpdateFeatureHeader() {
		testJsonTableFeatureHeader(new JsonTableTestOperation() {

			@Override
			public void executeOperation(MobileServiceJsonTable table) throws Exception {
				JsonObject jo = createJsonObject();
				table.update(jo).get();
			}
		}, false, "TU");
	}

	public void testJsonTableUpdateWithParametersFeatureHeader() {
		testJsonTableFeatureHeader(new JsonTableTestOperation() {

			@Override
			public void executeOperation(MobileServiceJsonTable table) throws Exception {
				JsonObject jo = createJsonObject();
				List<Pair<String, String>> queryParams = new ArrayList<Pair<String, String>>();
				queryParams.add(new Pair<String, String>("a", "b"));
				table.update(jo, queryParams).get();
			}
		}, false, "QS,TU");
	}

	public void testJsonTableDeleteFeatureHeader() {
		testJsonTableFeatureHeader(new JsonTableTestOperation() {

			@Override
			public void executeOperation(MobileServiceJsonTable table) throws Exception {
				JsonObject jo = createJsonObject();
				table.delete(jo).get();
			}
		}, false, "TU");
	}

	public void testJsonTableDeleteWithParametersFeatureHeader() {
		testJsonTableFeatureHeader(new JsonTableTestOperation() {

			@Override
			public void executeOperation(MobileServiceJsonTable table) throws Exception {
				JsonObject jo = createJsonObject();
				List<Pair<String, String>> queryParams = new ArrayList<Pair<String, String>>();
				queryParams.add(new Pair<String, String>("a", "b"));
				table.delete(jo, queryParams).get();
			}
		}, false, "QS,TU");
	}

	public void testJsonTableLookupFeatureHeader() {
		testJsonTableFeatureHeader(new JsonTableTestOperation() {

			@Override
			public void executeOperation(MobileServiceJsonTable table) throws Exception {
				table.lookUp("1").get();
			}
		}, false, "TU");
	}

	public void testJsonTableLookupWithParametersFeatureHeader() {
		testJsonTableFeatureHeader(new JsonTableTestOperation() {

			@Override
			public void executeOperation(MobileServiceJsonTable table) throws Exception {
				List<Pair<String, String>> queryParams = new ArrayList<Pair<String, String>>();
				queryParams.add(new Pair<String, String>("a", "b"));
				table.lookUp("1", queryParams).get();
			}
		}, false, "QS,TU");
	}

	public void testJsonTableReadFeatureHeader() {
		testJsonTableFeatureHeader(new JsonTableTestOperation() {

			@Override
			public void executeOperation(MobileServiceJsonTable table) throws Exception {
				table.execute().get();
			}
		}, true, "TU");
	}

	public void testJsonQueryFeatureHeader() {
		testJsonTableFeatureHeader(new JsonTableTestOperation() {

			@Override
			public void executeOperation(MobileServiceJsonTable table) throws Exception {
				table.parameter("a", "b").execute().get();
			}
		}, true, "QS,TU");
	}

	private void testJsonTableFeatureHeader(JsonTableTestOperation operation, final boolean responseIsArray, final String expectedFeaturesHeader) {
		MobileServiceClient client = null;
		try {
			client = new MobileServiceClient(appUrl, appKey,
					getInstrumentation().getTargetContext());
		} catch (MalformedURLException e) {
			e.printStackTrace();
		}

		// Add a new filter to the client
		client = client.withFilter(new ServiceFilter() {

			@Override
			public ListenableFuture<ServiceFilterResponse> handleRequest(
					ServiceFilterRequest request,
					NextServiceFilterCallback nextServiceFilterCallback) {

				final SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture
						.create();
				String featuresHeaderName = "X-ZUMO-FEATURES";

				String requestUri = request.getUrl();
				System.out.println(requestUri);
				Header[] headers = request.getHeaders();
				String features = null;
				for (int i = 0; i < headers.length; i++) {
					if (headers[i].getName() == featuresHeaderName) {
						features = headers[i].getValue();
					}
				}

				boolean error = false;
				if (features == null) {
					if (expectedFeaturesHeader != null) {
						resultFuture.setException(new Exception("No " + featuresHeaderName + " header on API call"));
						error = true;
					}
				} else if (!features.equals(expectedFeaturesHeader)) {
					resultFuture.setException(new Exception("Incorrect features header; expected " + 
						expectedFeaturesHeader + ", actual " + features));
					error = true;
				}

				if (!error) {
					ServiceFilterResponseMock response = new ServiceFilterResponseMock();
					String content = "{\"id\":\"1\",\"firstName\":\"John\",\"lastName\":\"Doe\",\"age\":33}";
					if (responseIsArray) {
						content = "[" + content + "]";
					}
					response.setContent(content);
					resultFuture.set(response);
				}

				return resultFuture;
			}
		});

		try {
			MobileServiceJsonTable table = client.getTable("Person");
			operation.executeOperation(table);
		} catch (Exception exception) {
			Throwable ex = exception;
			while (ex instanceof ExecutionException || ex instanceof MobileServiceException) {
				ex = ex.getCause();
			}
			fail(ex.getMessage());
		}
	}

	// Tests for custom APIs
	interface ClientTestOperation {
		void executeOperation(MobileServiceClient client) throws Exception;
	}

	public void testJsonApiFeatureHeader() {
		testInvokeApiFeatureHeader(new ClientTestOperation() {

			@Override
			public void executeOperation(MobileServiceClient client) throws Exception {
				client.invokeApi("foo").get();
			}
			
		}, "AJ");
	}

	public void testJsonApiWithQueryParametersFeatureHeader() {
		testInvokeApiFeatureHeader(new ClientTestOperation() {

			@Override
			public void executeOperation(MobileServiceClient client) throws Exception {
				List<Pair<String, String>> queryParams = new ArrayList<Pair<String, String>>();
				queryParams.add(new Pair<String, String>("a", "b"));
				client.invokeApi("apiName", "DELETE", queryParams).get();
			}
			
		}, "AJ,QS");
	}

	public void testTypedApiFeatureHeader() {
		testInvokeApiFeatureHeader(new ClientTestOperation() {

			@Override
			public void executeOperation(MobileServiceClient client) throws Exception {
				client.invokeApi("apiName", Address.class).get();
			}
			
		}, "AT");
	}

	public void testTypedApiWithQueryParametersFeatureHeader() {
		testInvokeApiFeatureHeader(new ClientTestOperation() {

			@Override
			public void executeOperation(MobileServiceClient client) throws Exception {
				List<Pair<String, String>> queryParams = new ArrayList<Pair<String, String>>();
				queryParams.add(new Pair<String, String>("a", "b"));
				client.invokeApi("apiName", "GET", queryParams, Address.class).get();
			}
			
		}, "AT,QS");
	}

	public void testGenericApiFeatureHeader() {
		testInvokeApiFeatureHeader(new ClientTestOperation() {

			@Override
			public void executeOperation(MobileServiceClient client) throws Exception {
				List<Pair<String, String>> queryParams = new ArrayList<Pair<String, String>>();
				queryParams.add(new Pair<String, String>("a", "b"));
				List<Pair<String, String>> requestHeaders = new ArrayList<Pair<String, String>>();
				requestHeaders.add(new Pair<String, String>("Content-Type", "text/plain"));
				byte[] content = "hello world".getBytes();
				client.invokeApi("apiName", content, "POST", requestHeaders , queryParams).get();
			}
			
		}, "AG");
	}

	public void testGenericApiDoesNotOverrideExistingFeatureHeader() {
		testInvokeApiFeatureHeader(new ClientTestOperation() {

			@Override
			public void executeOperation(MobileServiceClient client) throws Exception {
				List<Pair<String, String>> queryParams = new ArrayList<Pair<String, String>>();
				queryParams.add(new Pair<String, String>("a", "b"));
				List<Pair<String, String>> requestHeaders = new ArrayList<Pair<String, String>>();
				requestHeaders.add(new Pair<String, String>("Content-Type", "text/plain"));
				requestHeaders.add(new Pair<String, String>("X-ZUMO-FEATURES", "something"));
				byte[] content = "hello world".getBytes();
				client.invokeApi("apiName", content, "POST", requestHeaders, queryParams).get();
			}
			
		}, "something");
	}

	private void testInvokeApiFeatureHeader(ClientTestOperation operation, final String expectedFeaturesHeader) {
		MobileServiceClient client = null;
		try {
			client = new MobileServiceClient(appUrl, appKey,
					getInstrumentation().getTargetContext());
		} catch (MalformedURLException e) {
			e.printStackTrace();
		}

		// Add a new filter to the client
		client = client.withFilter(new ServiceFilter() {

			@Override
			public ListenableFuture<ServiceFilterResponse> handleRequest(
					ServiceFilterRequest request,
					NextServiceFilterCallback nextServiceFilterCallback) {

				final SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture
						.create();
				String featuresHeaderName = "X-ZUMO-FEATURES";
				
				Header[] headers = request.getHeaders();
				String features = null;
				for (int i = 0; i < headers.length; i++) {
					if (headers[i].getName() == featuresHeaderName) {
						features = headers[i].getValue();
					}
				}

				if (features == null) {
					resultFuture.setException(new Exception("No " + featuresHeaderName + " header on API call"));
				} else if (!features.equals(expectedFeaturesHeader)) {
					resultFuture.setException(new Exception("Incorrect features header; expected " + 
						expectedFeaturesHeader + ", actual " + features));
				} else {
					ServiceFilterResponseMock response = new ServiceFilterResponseMock();
					response.setContent("{}");
					resultFuture.set(response);
				}

				return resultFuture;
			}
		});

		try {
			operation.executeOperation(client);
		} catch (Exception exception) {
			Throwable ex = exception;
			while (ex instanceof ExecutionException || ex instanceof MobileServiceException) {
				ex = ex.getCause();
			}
			fail(ex.getMessage());
		}
	}
}
