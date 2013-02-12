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

import java.util.HashMap;
import java.util.List;
import java.util.Locale;
import java.util.Random;
import java.util.UUID;

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

}
