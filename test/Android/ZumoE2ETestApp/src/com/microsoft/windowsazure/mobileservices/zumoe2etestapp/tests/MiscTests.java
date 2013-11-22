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

import static com.microsoft.windowsazure.mobileservices.MobileServiceQueryOperations.field;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Locale;
import java.util.Random;
import java.util.UUID;

import org.apache.http.Header;
import org.apache.http.client.methods.HttpGet;

import android.util.Pair;

import com.google.gson.JsonArray;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceJsonTable;
import com.microsoft.windowsazure.mobileservices.MobileServiceQuery;
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
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.ExpectedValueException;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.FroyoAndroidHttpClientFactory;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestCase;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestExecutionCallback;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestGroup;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestResult;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestStatus;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.Util;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.types.ParamsTestTableItem;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.types.RoundTripTableElement;

public class MiscTests extends TestGroup {

	protected static final String ROUND_TRIP_TABLE_NAME = "droidRoundTripTable";
	protected static final String PARAM_TEST_TABLE_NAME = "ParamsTestTable";
	protected static final String MOVIES_TABLE_NAME = "droidMovies";
	private static final String APP_API_NAME = "application";
	
	
	public MiscTests() {
		super("Misc tests");
		
		this.addTest(createFilterTestWithMultipleRequests(true));
		this.addTest(createFilterTestWithMultipleRequests(false));
        
		TestCase withFilterDoesNotChangeTheClient = new TestCase() {

			@Override
			protected void executeTest(MobileServiceClient client,
					final TestExecutionCallback callback) {
				final TestResult testResult = new TestResult();
				testResult.setTestCase(this);
				testResult.setStatus(TestStatus.Passed);
				final TestCase testCase = this;
				client.withFilter(new ServiceFilter() {
					
					@Override
					public void handleRequest(ServiceFilterRequest request,
							NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {
						log("executed filter triggering failure");
						testResult.setStatus(TestStatus.Failed);
					}
				});
				
				log("execute query");
				client.getTable(ROUND_TRIP_TABLE_NAME).top(5).execute(new TableJsonQueryCallback() {
					
					@Override
					public void onCompleted(JsonElement result, int count, Exception exception,
							ServiceFilterResponse response) {
						if (exception != null) {
							createResultFromException(testResult, exception);
						}
						
						if (callback != null) callback.onTestComplete(testCase, testResult);
					}
				});
			}
			
		};
		withFilterDoesNotChangeTheClient.setName("Verify that 'withFilter' does not change the client");
		this.addTest(withFilterDoesNotChangeTheClient);
		
		TestCase bypassTest = new TestCase() {
			
			@Override
			protected void executeTest(MobileServiceClient client,
					final TestExecutionCallback callback) {
				
				final TestCase testCase = this;
				final TestResult result = new TestResult();
				result.setStatus(TestStatus.Passed);
				result.setTestCase(this);
				
				final String json = "{'id':1,'name':'John Doe','age':33}".replace('\'', '\"');
                MobileServiceClient filtered = client.withFilter(new ServiceFilter() {
					
					@Override
					public void handleRequest(ServiceFilterRequest request,
							NextServiceFilterCallback nextServiceFilterCallback,
							ServiceFilterResponseCallback responseCallback) {
						log("returning mock response");
						responseCallback.onResponse(new MockResponse(json, 201), null);
					}
				});
                
                log("insert item");
                filtered.getTable("fakeTable").insert(new JsonObject(), new TableJsonOperationCallback() {
					
					@Override
					public void onCompleted(JsonObject jsonEntity, Exception exception,
							ServiceFilterResponse response) {
						JsonObject expectedObject = new JsonParser().parse(json).getAsJsonObject();
						log("verify items are equal");
						if (!Util.compareJson(expectedObject, jsonEntity)) {
							createResultFromException(result, new ExpectedValueException(expectedObject, jsonEntity));
						}
						
						if (callback != null) callback.onTestComplete(testCase, result);
					}
				});
			}
		};
		
		bypassTest.setName("Filter to bypass service");
		this.addTest(bypassTest);
		
		this.addTest(createParameterPassingTest(true));
		this.addTest(createParameterPassingTest(false));
		
		this.addTest(createHttpContentApiTest());
		
		this.addTest(createFroyoFixedRequestTest());

		this.addTest(new TestCase("User-Agent validation") {

			@Override
			protected void executeTest(MobileServiceClient client,
					final TestExecutionCallback callback) {
				final TestCase testCase = this;
				final TestResult testResult = new TestResult();
				testResult.setTestCase(testCase);
				testResult.setStatus(TestStatus.Failed);
				client = client.withFilter(new ServiceFilter() {

					@Override
					public void handleRequest(ServiceFilterRequest request,
							NextServiceFilterCallback nextServiceFilterCallback,
							final ServiceFilterResponseCallback responseCallback) {
						Header[] headers = request.getHeaders();
						for (Header reqHeader : headers) {
							if (reqHeader.getName() == "User-Agent") {
								String userAgent = reqHeader.getValue();
								log("User-Agent: " + userAgent);
								testResult.setStatus(TestStatus.Passed);
								String clientVersion = userAgent;
								if (clientVersion.endsWith(")")) {
									clientVersion = clientVersion.substring(0, clientVersion.length() - 1);
								}
								int indexOfEquals = clientVersion.lastIndexOf('=');
								if (indexOfEquals >= 0) {
									clientVersion = clientVersion.substring(indexOfEquals + 1);
									Util.getGlobalTestParameters().put(ClientVersionKey, clientVersion);
								}
							}
						}

						nextServiceFilterCallback.onNext(request, new ServiceFilterResponseCallback() {

							@Override
							public void onResponse(ServiceFilterResponse response, Exception exception) {
								if (response != null) {
									Header[] respHeaders = response.getHeaders();
									for (Header header : respHeaders) {
										if (header.getName().equalsIgnoreCase("x-zumo-version")) {
											String runtimeVersion = header.getValue();
											Util.getGlobalTestParameters().put(ServerVersionKey, runtimeVersion);
										}
									}
								}
								responseCallback.onResponse(response, exception);
							}
						});
					}
				});

				log("execute query to activate filter");
				client.getTable(ROUND_TRIP_TABLE_NAME).top(5).execute(new TableJsonQueryCallback() {

					@Override
					public void onCompleted(JsonElement result, int count, Exception exception,
							ServiceFilterResponse response) {
						if (exception != null) {
							createResultFromException(testResult, exception);
						}

						if (callback != null) callback.onTestComplete(testCase, testResult);
					}
				});
			}
		});
	}

	private TestCase createFroyoFixedRequestTest() {
			final TestCase test = new TestCase() {
			
			@Override
			protected void executeTest(MobileServiceClient client,
					final TestExecutionCallback callback) {
				final TestResult result = new TestResult();
				result.setTestCase(this);
				result.setStatus(TestStatus.Passed);
				final TestCase testCase = this;

				// duplicate the client
				MobileServiceClient froyoClient = new MobileServiceClient(client);
				
				log("add custom AndroidHttpClientFactory with Froyo support");
				froyoClient.setAndroidHttpClientFactory(new FroyoAndroidHttpClientFactory());
				
				MobileServiceTable<RoundTripTableElement> table = 
						froyoClient.getTable(ROUND_TRIP_TABLE_NAME, RoundTripTableElement.class);
				
				table.where().field("id").eq(1).execute(new TableQueryCallback<RoundTripTableElement>() {
					
					@Override
					public void onCompleted(List<RoundTripTableElement> r, int count,
							Exception exception, ServiceFilterResponse response) {
						if (exception != null) {
							createResultFromException(result, exception);
						}
						callback.onTestComplete(testCase, result);
					}
				});
			}
		};
		
		test.setName("Simple request on Froyo");
		return test;
	}



	private TestCase createParameterPassingTest(final boolean typed) {
		TestCase test = new TestCase() {
			
			@Override
			protected void executeTest(MobileServiceClient client,
					final TestExecutionCallback callback) {
				final TestResult result = new TestResult();
				result.setTestCase(this);
				result.setStatus(TestStatus.Passed);
				final TestCase testCase = this;
				
				final HashMap<String, String> params = createParams();
				
				final JsonObject expectedParameters = new JsonObject();
				
				for (String key : params.keySet()) {
					expectedParameters.addProperty(key, params.get(key));
				}
				
				params.put("operation", "read");
				expectedParameters.addProperty("operation", "read");
				
				log("execute query");
				if (typed) {
					MobileServiceTable<ParamsTestTableItem> table = client.getTable(PARAM_TEST_TABLE_NAME, ParamsTestTableItem.class);
					MobileServiceQuery<TableQueryCallback<ParamsTestTableItem>> query = table.where();
					addParametersToQuery(query, params);
					
					query.execute(new TableQueryCallback<ParamsTestTableItem>() {
						
						@Override
						public void onCompleted(List<ParamsTestTableItem> elements, int count,
								Exception exception, ServiceFilterResponse response) {
							if (exception == null) {
								log("verify size = 0");
								if (elements.size() != 0) {
									JsonObject actualParameters = elements.get(0).getParameters();
									
									log("verify items are equal");
									if (!Util.compareJson(expectedParameters, actualParameters)) {
										createResultFromException(result, new ExpectedValueException(expectedParameters, actualParameters));
									}
									
									if (callback != null) callback.onTestComplete(testCase, result);
								} else {
									createResultFromException(result, new ExpectedValueException("SIZE > 0", "SIZE == 0"));
									if (callback != null) callback.onTestComplete(testCase, result);
								}
							} else {
								createResultFromException(result, exception);
								if (callback != null) callback.onTestComplete(testCase, result);
							}
						}
					});
				} else {
					MobileServiceJsonTable table = client.getTable(PARAM_TEST_TABLE_NAME);
					MobileServiceQuery<TableJsonQueryCallback> query = table.where();
					addParametersToQuery(query, params);
					
					query.execute(new TableJsonQueryCallback() {
						
						@Override
						public void onCompleted(JsonElement elements, int count,
								Exception exception, ServiceFilterResponse response) {
							if (exception == null) {
								log("verify result is JsonArray with at least one element");
								if (elements.isJsonArray() && (elements.getAsJsonArray()).size() != 0) {
									try
									{
										log("get parameters object and recreate json");
										JsonObject actualObject = elements.getAsJsonArray().get(0).getAsJsonObject();
										actualObject.add("parameters", new JsonParser().parse(actualObject.get("parameters").getAsString()));
										JsonObject expectedObject = new JsonObject();
										expectedObject.addProperty("id", 1);
										expectedObject.add("parameters", expectedParameters);
										
										log("verify items are equal");
										if (!Util.compareJson(expectedObject, actualObject)) {
											createResultFromException(result, new ExpectedValueException(expectedObject, actualObject));
										}
									} catch (Exception e) {
										createResultFromException(result, e);
									}
									
									if (callback != null) callback.onTestComplete(testCase, result);
								} else {
									createResultFromException(result, new ExpectedValueException("JSON ARRAY WITH ELEMENTS", "EMPTY RESULT"));
									if (callback != null) callback.onTestComplete(testCase, result);
								}
							} else {
								createResultFromException(result, exception);
								if (callback != null) callback.onTestComplete(testCase, result);
							}
						}
					});
				}
			}

			private void addParametersToQuery(
					MobileServiceQuery<?> query,
					HashMap<String, String> params) {
				for (String key : params.keySet()) {
					query.parameter(key, params.get(key));
				}
			}
		};
		
		test.setName("Parameter passing test - tables (" + (typed ? "typed" : "untyped") + ")");
		return test;
	}

	private HashMap<String, String> createParams() {
		HashMap<String, String> params = new HashMap<String, String>();
		params.put("item", "simple");
		params.put( "empty", "" );
		params.put("spaces", "with spaces" );
		params.put("specialChars", "`!@#$%^&*()-=[]\\;',./~_+{}|:\"<>?" );
		params.put("latin", "ãéìôü ÇñÑ" );
		params.put("arabic", "الكتاب على الطاولة" );
		params.put("chinese", "这本书在桌子上" );
		params.put("japanese", "本は机の上に" );
		params.put("hebrew", "הספר הוא על השולחן" );
		params.put("name+with special&chars", "should just work");
		return params;
	}

	private TestCase createFilterTestWithMultipleRequests(final boolean typed) {
		TestCase test = new TestCase() {
			
			MobileServiceJsonTable mTable;
			
			TestExecutionCallback mCallback;
			int mNumberOfRequest;
			String mUUID;
			
			@Override
			protected void executeTest(MobileServiceClient client,
					final TestExecutionCallback callback) {
				Random rndGen = new Random();
				
				mNumberOfRequest = rndGen.nextInt(3) + 3;
				log("number of requests: " + mNumberOfRequest);
				MobileServiceClient filteredClient = client.withFilter(new MultipleRequestFilter(mNumberOfRequest));
				
				mTable = client.getTable(ROUND_TRIP_TABLE_NAME);
				
				mCallback = callback;
				
				mUUID = UUID.randomUUID().toString();
				log("insert item " + mUUID);
				if (typed)
                {
					MobileServiceTable<RoundTripTableElement> filteredClientTable = 
							filteredClient.getTable(ROUND_TRIP_TABLE_NAME, RoundTripTableElement.class);
					
					RoundTripTableElement item = new RoundTripTableElement();
                    item.string1 = mUUID;
                    
                    filteredClientTable.insert(item, new TableOperationCallback<RoundTripTableElement>() {
						
						@Override
						public void onCompleted(RoundTripTableElement entity, Exception exception,
								ServiceFilterResponse response) {
							requestCompleted(exception, response);
						}
					});
                }
                else
                {
                	MobileServiceJsonTable filteredClientTable = 
							filteredClient.getTable(ROUND_TRIP_TABLE_NAME);
					
                    JsonObject item = new JsonObject();
                    item.addProperty("string1", mUUID);
                    
                    filteredClientTable.insert(item, new TableJsonOperationCallback() {
						
						@Override
						public void onCompleted(JsonObject jsonEntity, Exception exception,
								ServiceFilterResponse response) {
							requestCompleted(exception, response);
						}
					});
                }

			}
			
			private void requestCompleted(Exception exception, ServiceFilterResponse response) {
				final TestResult testResult = new TestResult();
				testResult.setTestCase(this);
				testResult.setStatus(TestStatus.Passed);
				
				if (exception != null) {
					createResultFromException(testResult, exception);
					if (mCallback != null) mCallback.onTestComplete(this, testResult);
					return;
				}
				
				final TestCase testCase = this;
				log("retrieve the original item");
				mTable.where(field("string1").eq(mUUID)).select("string1", "id").execute(new TableJsonQueryCallback() {
					
					@Override
					public void onCompleted(JsonElement result, int count, Exception exception,
							ServiceFilterResponse response) {
						log("verify that there are " + mNumberOfRequest + " elements in the JsonArray");
						
						if (exception != null) {
							createResultFromException(testResult, exception);
						}
						else if (!result.isJsonArray()) {
							createResultFromException(testResult, new ExpectedValueException("JSON ARRAY", result.toString()));
						} else if (result.getAsJsonArray().size() != mNumberOfRequest) {
							createResultFromException(
									testResult, 
									new ExpectedValueException(
											mNumberOfRequest + " Times", 
											result.getAsJsonArray().size() + " Times"));
						}
						
						if (testResult.getStatus() == TestStatus.Failed) {
							if (mCallback != null) mCallback.onTestComplete(testCase, testResult);
							return;
						}
						
						JsonArray jsonArray = result.getAsJsonArray();
						for (int i = 0; i < jsonArray.size(); i++) {
							final boolean doCallback = i == jsonArray.size() - 1;
							log("delete item " + jsonArray.get(i));
							mTable.delete(jsonArray.get(i), new TableDeleteCallback() {
								
								@Override
								public void onCompleted(Exception exception, ServiceFilterResponse response) {
									if (exception != null) {
										createResultFromException(testResult, exception);
									}
									
									if (doCallback) {
										if (mCallback != null) mCallback.onTestComplete(testCase, testResult);
									}
								}
							});
						}
					}
				});
				
			}
		};
		
		String name = String.format(Locale.getDefault(), "Filter which maps one requests to many - %s client", typed ? "typed" : "untyped");        
		test.setName(name);
		
		return test;
	}

	private TestCase createHttpContentApiTest() {
		String name = "Use \"text/plain\" Content and \"identity\" Encoding Headers";
		
		TestCase test = new TestCase(name) {
			MobileServiceClient mClient;
			List<Pair<String, String>> mQuery;
			List<Pair<String, String>> mHeaders;
			TestExecutionCallback mCallback;
			JsonObject mExpectedResult;
			int mExpectedStatusCode;
			String mHttpMethod;
			byte[] mContent;
			
			TestResult mResult;
			
			@Override
			protected void executeTest(MobileServiceClient client,
					TestExecutionCallback callback) {
				mResult = new TestResult();
				mResult.setTestCase(this);
				mResult.setStatus(TestStatus.Passed);
				mClient = client;
				mCallback = callback;
				
				createHttpContentTestInput();
				
				mClient.invokeApi(APP_API_NAME, mContent, mHttpMethod, mHeaders, mQuery, new ServiceFilterResponseCallback() {
					
					@Override
					public void onResponse(ServiceFilterResponse response, Exception exception) {
						if (response == null) {
							createResultFromException(exception);
							mCallback.onTestComplete(mResult.getTestCase(), mResult);
							return;
						}
						
						Exception ex = validateResponse(response);
						if (ex != null) {
							createResultFromException(mResult, ex);
						} else {
							mResult.getTestCase().log("Header validated successfully");
							
							String responseContent = response.getContent();
							
							mResult.getTestCase().log("Response content: " + responseContent);
							
							JsonElement jsonResponse = null;
							String decodedContent = responseContent
									.replace("__{__", "{")
									.replace("__}__", "}")
									.replace("__[__", "[")
									.replace("__]__", "]");
							jsonResponse = new JsonParser().parse(decodedContent);
														
							if (!Util.compareJson(mExpectedResult, jsonResponse)) {
								createResultFromException(mResult, new ExpectedValueException(mExpectedResult, jsonResponse));
							}
						}
						
						mCallback.onTestComplete(mResult.getTestCase(), mResult);
					}

					private Exception validateResponse(ServiceFilterResponse response) {
						if (mExpectedStatusCode != response.getStatus().getStatusCode()) {
							mResult.getTestCase().log("Invalid status code");
							String content = response.getContent();
							if (content != null) {
								mResult.getTestCase().log("Response: " + content);
							}
							return new ExpectedValueException(mExpectedStatusCode, response.getStatus().getStatusCode());
						} else {
							return null;							
						}						
					}
				});
			}
			
			private void createHttpContentTestInput() {
				mHttpMethod = HttpGet.METHOD_NAME;
				log("Method = " + mHttpMethod);
				
				mExpectedResult = new JsonObject();
				mExpectedResult.addProperty("method", mHttpMethod);
				JsonObject user = new JsonObject();
				user.addProperty("level", "anonymous");
				mExpectedResult.add("user", user);
				
				mHeaders = new ArrayList<Pair<String,String>>();
				mHeaders.add(new Pair<String, String>("Accept", "text/plain"));
				mHeaders.add(new Pair<String, String>("Accept-Encoding", "identity"));
				
				mQuery = new ArrayList<Pair<String,String>>();
				mQuery.add(new Pair<String, String>("format", "other"));
				
				mExpectedStatusCode = 200;				
			}
		};
		
		return test;
	}
}
