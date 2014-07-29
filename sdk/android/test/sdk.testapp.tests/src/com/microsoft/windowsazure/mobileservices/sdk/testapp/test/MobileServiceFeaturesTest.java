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
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceException;
import com.microsoft.windowsazure.mobileservices.MobileServiceFeatures;
import com.microsoft.windowsazure.mobileservices.http.NextServiceFilterCallback;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilter;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterRequest;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterResponse;

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
