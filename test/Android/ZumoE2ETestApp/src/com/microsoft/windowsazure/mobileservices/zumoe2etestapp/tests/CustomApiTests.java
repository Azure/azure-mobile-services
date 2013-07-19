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

import java.io.UnsupportedEncodingException;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.Collections;
import java.util.Date;
import java.util.HashMap;
import java.util.List;
import java.util.Locale;
import java.util.Map;
import java.util.Map.Entry;
import java.util.Random;
import java.util.Set;
import java.util.TimeZone;

import org.apache.http.client.methods.HttpDelete;
import org.apache.http.client.methods.HttpGet;
import org.apache.http.client.methods.HttpPost;
import org.apache.http.client.methods.HttpPut;
import org.apache.http.protocol.HTTP;

import android.util.Pair;

import com.google.gson.JsonArray;
import com.google.gson.JsonElement;
import com.google.gson.JsonNull;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import com.google.gson.JsonPrimitive;
import com.microsoft.windowsazure.mobileservices.ApiJsonOperationCallback;
import com.microsoft.windowsazure.mobileservices.ApiOperationCallback;
import com.microsoft.windowsazure.mobileservices.MobileServiceAuthenticationProvider;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.ServiceFilterResponseCallback;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.ExpectedValueException;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestCase;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestExecutionCallback;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestGroup;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestResult;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestStatus;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.Util;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.types.AllMovies;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.types.Movie;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.types.MovieComparator;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.types.SimpleMovieFilter;

public class CustomApiTests extends TestGroup {

	private static final String PUBLIC_API_NAME = "public";
	private static final String APP_API_NAME = "application";
	private static final String USER_API_NAME = "user";
	private static final String ADMIN_API_NAME = "admin";
	private static final String MOVIEFINDER_API_NAME = "movieFinder";
	private static final String PATCH_METHOD_NAME = "PATCH";
	
	private enum ApiPermissions { Public, Application, User, Admin }
	private enum DataFormat {Json, Xml, Other}
	private enum TypedTestType {GetByTitle, GetByDate, PostByDuration, PostByYear}
	
	private Map<ApiPermissions, String> apiNames;
	
	public CustomApiTests() {
		super("Custom API tests");

		apiNames = new HashMap<CustomApiTests.ApiPermissions, String>();
		apiNames.put(ApiPermissions.Admin, ADMIN_API_NAME);
		apiNames.put(ApiPermissions.User, USER_API_NAME);
		apiNames.put(ApiPermissions.Application, APP_API_NAME);
		apiNames.put(ApiPermissions.Public, PUBLIC_API_NAME);
		
		Random rndGen = new Random();

		this.addTest(LoginTests.createLogoutTest());
		
		for (ApiPermissions permission : ApiPermissions.values()) {
			for (int i = 0; i < 10; i++) {
				this.addTest(createJsonApiTest(permission, false, rndGen, i));
			}
		}

		TestCase loginTest = LoginTests.createLoginTest(MobileServiceAuthenticationProvider.Facebook);
		loginTest.setCanRunUnattended(false);
		this.addTest(loginTest);
		TestCase apiAuthenticatedTest = createJsonApiTest(ApiPermissions.User, true, rndGen, 0);
		apiAuthenticatedTest.setCanRunUnattended(false);
		this.addTest(apiAuthenticatedTest);
		this.addTest(LoginTests.createLogoutTest());
		
		for (TypedTestType testType : TypedTestType.values()) {
			this.addTest(createTypedApiTest(rndGen, testType));
		}
		
		for (DataFormat inputFormat: DataFormat.values()) {
			for (DataFormat outputFormat : DataFormat.values()) {
				this.addTest(createHttpContentApiTest(inputFormat, outputFormat, rndGen));
			}
		}
	}
	
	private TestCase createHttpContentApiTest(final DataFormat inputFormat, final DataFormat outputFormat, final Random rndGen) {
		String name = String.format("HttpContent Overload - Input: %s - Output: %s", 
				inputFormat,
				outputFormat);
		
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
				mHeaders = new ArrayList<Pair<String,String>>();
				
				createHttpContentTestInput(inputFormat, outputFormat, rndGen);
				
				mClient.invokeApi(APP_API_NAME, mContent, mHttpMethod, mHeaders, mQuery, new ServiceFilterResponseCallback() {
					
					@Override
					public void onResponse(ServiceFilterResponse response, Exception exception) {
						if (response == null) {
							createResultFromException(exception);
							mCallback.onTestComplete(mResult.getTestCase(), mResult);
							return;
						}
						
						Exception ex = validateResponseHeaders(response);
						if (ex != null) {
							createResultFromException(mResult, ex);
						} else {
							mResult.getTestCase().log("Header validated successfully");
							
							String responseContent = response.getContent();
							
							mResult.getTestCase().log("Response content: " + responseContent);
							
							JsonElement jsonResponse = null;
							if (outputFormat == DataFormat.Json) {
								jsonResponse = new JsonParser().parse(responseContent);
							}
							else if (outputFormat == DataFormat.Other) {
								String decodedContent = responseContent
										.replace("__{__", "{")
										.replace("__}__", "}")
										.replace("__[__", "[")
										.replace("__]__", "]");
								jsonResponse = new JsonParser().parse(decodedContent);
							}
							
							switch (outputFormat) {
							case Json:
							case Other:
								if (!Util.compareJson(mExpectedResult, jsonResponse)) {
									createResultFromException(mResult, new ExpectedValueException(mExpectedResult, jsonResponse));
								}
								break;
							default:
								String expectedResultContent = jsonToXml(mExpectedResult);
								// Normalize CRLF
								expectedResultContent = expectedResultContent.replace("\r\n", "\n");
								responseContent = responseContent.replace("\r\n", "\n");
								
								if (!expectedResultContent.equals(responseContent)) {
									createResultFromException(mResult, new ExpectedValueException(expectedResultContent, responseContent));	
								}
								break;
							}
						}
						
						mCallback.onTestComplete(mResult.getTestCase(), mResult);
					}

					private Exception validateResponseHeaders(ServiceFilterResponse response) {
						if (mExpectedStatusCode != response.getStatus().getStatusCode()) {
							mResult.getTestCase().log("Invalid status code");
							String content = response.getContent();
							if (content != null) {
								mResult.getTestCase().log("Response: " + content);
							}
							return new ExpectedValueException(mExpectedStatusCode, response.getStatus().getStatusCode());
						} else {
							
							for (Pair<String, String> header : mHeaders) {
								if (!header.first.equals(HTTP.CONTENT_TYPE)) {
									if (!Util.responseContainsHeader(response, header.first)) {
										mResult.getTestCase().log("Header " + header.first + " not found");
										return new ExpectedValueException("Header: " + header.first, "");
									} else {
										String headerValue = Util.getHeaderValue(response, header.first);
										if (!header.second.equals(headerValue)) {
											mResult.getTestCase().log("Invalid Header value for " + header.first);
											return new ExpectedValueException(header.second, headerValue);
										}
									}
								}
							}
						}
						
						return null;
					}
				});
			}
			
			private void createHttpContentTestInput(DataFormat inputFormat, DataFormat outputFormat, Random rndGen) {
				mHttpMethod = createHttpMethod(rndGen);
				log("Method = " + mHttpMethod);
				
				mExpectedResult = new JsonObject();
				mExpectedResult.addProperty("method", mHttpMethod);
				JsonObject user = new JsonObject();
				user.addProperty("level", "anonymous");
				mExpectedResult.add("user", user);
				
				JsonElement body = null;
				String textBody = null;
				if (!mHttpMethod.equals(HttpGet.METHOD_NAME) && !mHttpMethod.equals(HttpDelete.METHOD_NAME))
				{
					body = createJson(rndGen, 0, true);
					if (outputFormat == DataFormat.Xml || inputFormat == DataFormat.Xml) {
						// to prevent non-XML names from interfering with checks
						body = sanitizeJsonXml(body);
					}
					
					try {
						switch (inputFormat) {
						case Json:
							mContent = body.toString().getBytes("utf-8");
							mHeaders.add(new Pair<String, String>(HTTP.CONTENT_TYPE, "application/json"));
							break;
							
						case Xml:
							textBody = jsonToXml(body);
							mContent = textBody.getBytes("utf-8");
							mHeaders.add(new Pair<String, String>(HTTP.CONTENT_TYPE, "text/xml"));
							break;
						default:
							textBody = body.toString().replace('{', '<').replace('}', '>').replace("[", "__[__").replace("]", "__]__");
							mContent = textBody.getBytes("utf-8");
							mHeaders.add(new Pair<String, String>(HTTP.CONTENT_TYPE, "text/plain"));
							break;
						}
					}
					catch (UnsupportedEncodingException e) {
						//this will never happen
					}
				}
				
				if (body != null) {
					if (inputFormat == DataFormat.Json) {
						mExpectedResult.add("body", body);
					} else {
						mExpectedResult.addProperty("body", textBody);
					}
				}
				
				int choice = rndGen.nextInt(5);
				for (int j = 0; j < choice; j++) {
					String name = "x-test-zumo-" + j;
					String value = Util.createSimpleRandomString(rndGen, rndGen.nextInt(10) + 1, 'a', 'z');
					mHeaders.add(new Pair<String, String>(name, value));
				}
				
				mQuery = createQuery(rndGen);
				if (mQuery == null) {
					mQuery = new ArrayList<Pair<String,String>>();
				}
				
				if (mQuery.size() > 0) {
					JsonObject outputQuery = new JsonObject();
					for (Pair<String, String> element : mQuery) {
						outputQuery.addProperty(element.first, element.second);
					}
					
					mExpectedResult.add("query", outputQuery);
				}
				
				mQuery.add(new Pair<String, String>("format", outputFormat.toString().toLowerCase(Locale.getDefault())));
				mExpectedStatusCode = 200;
				
				if (rndGen.nextInt(4) == 0) {
					// non-200 responses
					int[] options = new int[] { 400, 404, 500, 201 };
					int status = options[rndGen.nextInt(options.length)];
					mExpectedStatusCode = status;
					mQuery.add(new Pair<String, String>("status", Integer.valueOf(status).toString()));
				}
			}

			private String jsonToXml(JsonElement json)
			{
				StringBuilder sb = new StringBuilder();
				sb.append("<root>");
				jsonToXml(json, sb);
				sb.append("</root>");
				return sb.toString();
			}

			private void jsonToXml(JsonElement json, StringBuilder sb)
			{
				if (json == null)
				{
					json = new JsonPrimitive("");
				}

				if (json.isJsonNull()) {
					sb.append("null");
				} else if (json.isJsonPrimitive() && json.getAsJsonPrimitive().isBoolean()) {
					sb.append(json.toString().toLowerCase(Locale.getDefault()));
				} else if (json.isJsonPrimitive() && json.getAsJsonPrimitive().isNumber()) {
					sb.append(json.toString());
				} else if (json.isJsonPrimitive() && json.getAsJsonPrimitive().isString()) {
					sb.append(json.getAsJsonPrimitive().getAsString());
				} else if (json.isJsonArray()) {
					sb.append("<array>");
					JsonArray array = json.getAsJsonArray();
					for (int i = 0; i < array.size(); i++) {
						sb.append("<item>");
						jsonToXml(array.get(i), sb);
						sb.append("</item>");
					}
					sb.append("</array>");
				} else {
					Set<Entry<String, JsonElement>> entrySet = json.getAsJsonObject().entrySet();
					
					List<String> keys = new ArrayList<String>();
					for (Entry<String, JsonElement> entry : entrySet) {
						keys.add(entry.getKey());
					}
					
					Collections.sort(keys);
					for (String key : keys) {
						sb.append("<" + key + ">");
						jsonToXml(json.getAsJsonObject().get(key), sb);
						sb.append("</" + key + ">");
					}
					
				}
			}

			private JsonElement sanitizeJsonXml(JsonElement body) {
				if (body.isJsonArray()) {
					JsonArray array = new JsonArray();
					for (JsonElement element : body.getAsJsonArray()) {
						array.add(sanitizeJsonXml(element));
					}
					
					return array;
				} else if (body.isJsonObject()) {
					JsonObject object = new JsonObject();
					Set<Entry<String, JsonElement>> entrySet = body.getAsJsonObject().entrySet();

					int i = 0;
					for (Entry<String, JsonElement> entry : entrySet) {
						object.add("memeber" + i, sanitizeJsonXml(entry.getValue()));
						i++;
					}
					
					return object;
				} else {
					return body;
				}
			}
		};
		
		return test;
	}

	private TestCase createTypedApiTest(final Random rndGen, final TypedTestType testType) {
		
		String name = String.format("Typed Overload - %s", 
				testType);
		
		TestCase test = new TestCase(name) {
			MobileServiceClient mClient;
			List<Pair<String, String>> mQuery; 
			TestExecutionCallback mCallback;
			Movie[] mExpectedResult = null;
			
			TestResult mResult;
			
			@Override
			protected void executeTest(MobileServiceClient client,
					TestExecutionCallback callback) {
				mResult = new TestResult();
				mResult.setTestCase(this);
				mResult.setStatus(TestStatus.Passed);
				mClient = client;
				mCallback = callback;
				
				final Movie inputTemplate = QueryTestData.getRandomMovie(rndGen);
				log("Using movie: " + inputTemplate.getTitle());
				
				String apiName = MOVIEFINDER_API_NAME;
				String apiUrl;
				switch (testType) {
				case GetByTitle:
					apiUrl = apiName + "/title/" + inputTemplate.getTitle();
					log("API: " + apiUrl);
					mExpectedResult = new Movie[] {inputTemplate};
					mClient.invokeApi(apiUrl, HttpGet.METHOD_NAME, null, AllMovies.class, typedTestCallback());
					break;
					
				case GetByDate:
					final Date releaseDate = inputTemplate.getReleaseDate();
					TimeZone tz = TimeZone.getTimeZone("UTC");
					Calendar c = Calendar.getInstance(tz);
					c.setTime(releaseDate);
					apiUrl = apiName + "/date/" + c.get(Calendar.YEAR) + "/" + (c.get(Calendar.MONTH) + 1) + "/" + c.get(Calendar.DATE);
					log("API: " + apiUrl);
					SimpleMovieFilter dateFilter = new SimpleMovieFilter() {
						
						@Override
						protected boolean criteria(Movie movie) {
							return movie.getReleaseDate().equals(releaseDate);
						}
					};
					
					mExpectedResult = dateFilter.filter(QueryTestData.getAllMovies()).elements.toArray(new Movie[0]);
					
					mClient.invokeApi(apiUrl, HttpGet.METHOD_NAME, null, AllMovies.class, typedTestCallback());
					break;

				case PostByDuration:
				case PostByYear:
					String orderBy = null;
					switch (rndGen.nextInt(3)) {
					case 0:
						orderBy = null;
						break;
					case 1:
						orderBy = "id";
						break;
					
					case 2:
						orderBy = "title";
						break;
					}
					
					
					mQuery = null;
					if (orderBy != null) {
						mQuery = new ArrayList<Pair<String,String>>();
						mQuery.add(new Pair<String, String>("orderBy", orderBy));
						
						log ("OrderBy: " + orderBy);
					}
					
					if (testType == TypedTestType.PostByYear) {
						apiUrl = apiName + "/moviesOnSameYear";
					} else {
						apiUrl = apiName + "/moviesWithSameDuration";
					}
					
					log("API: " + apiUrl);
					
					final String orderByCopy = orderBy;
					SimpleMovieFilter movieFilter = new SimpleMovieFilter() {
						
						@Override
						protected boolean criteria(Movie movie) {
							if (testType == TypedTestType.PostByYear) {
								return movie.getYear() == inputTemplate.getYear();
							} else {
								return movie.getDuration() == inputTemplate.getDuration();
							}
						}
						
						@Override
						protected List<Movie> applyOrder(List<Movie> movies) {
							if (orderByCopy == null || orderByCopy.equals("title")) {
								Collections.sort(movies, new MovieComparator("getTitle"));
							} 

							return movies;
						}
					};
					
					mExpectedResult = movieFilter.filter(QueryTestData.getAllMovies()).elements.toArray(new Movie[0]);
					
					if (mQuery == null) {
						mClient.invokeApi(apiUrl,inputTemplate, AllMovies.class, typedTestCallback());
					} else {
						mClient.invokeApi(apiUrl,inputTemplate, HttpPost.METHOD_NAME, mQuery, AllMovies.class, typedTestCallback());
					}
					break;
				}
			}
			
			private ApiOperationCallback<AllMovies> typedTestCallback() {
				return new ApiOperationCallback<AllMovies>() {
					
					@Override
					public void onCompleted(AllMovies result, Exception exception, ServiceFilterResponse response) {
						
						if (exception != null) {
							createResultFromException(mResult, exception);
						} else {
							if (!Util.compareArrays(mExpectedResult, result.getMovies())) {
								createResultFromException(mResult, new ExpectedValueException(Util.arrayToString(mExpectedResult), Util.arrayToString(result.getMovies())));
							}
						}
						
						mCallback.onTestComplete(mResult.getTestCase(), mResult);
					}
				};
			}
		};
		
		return test;
	}
	
	private TestCase createJsonApiTest(final ApiPermissions permission, final boolean isAuthenticated, final Random rndGen, int testNumber) {
		
		String name = String.format("Json Overload - %s - authenticated=%s - Instance=%s", 
				permission.toString(),
				isAuthenticated,
				testNumber + 1);
		
		TestCase test = new TestCase(name) {
			MobileServiceClient mClient;
			List<Pair<String, String>> mQuery; 
			TestExecutionCallback mCallback;
			boolean mExpected401;
			TestResult mResult;
			
			@Override
			protected void executeTest(MobileServiceClient client,
					TestExecutionCallback callback) {
				mResult = new TestResult();
				mResult.setTestCase(this);
				mResult.setStatus(TestStatus.Passed);

				if (permission == ApiPermissions.Public) {
					client = client.withFilter(new RemoveAuthenticationServiceFilter());
				}
				
				mExpected401 = permission == ApiPermissions.Admin || (permission == ApiPermissions.User && !isAuthenticated);
				mClient = client;
				mCallback = callback;
				
				String method = createHttpMethod(rndGen);
				log("Method = " + method);
				
				JsonElement body = null;
				if (!method.equals(HttpGet.METHOD_NAME) && !method.equals(HttpDelete.METHOD_NAME)) {
					if (method.equals(PATCH_METHOD_NAME) || method.equals(HttpPut.METHOD_NAME)) {
						body = createJson(rndGen, 0, false);
					} else {
						body = createJson(rndGen, 0, true);
					}
				}
		
				if (body == null) {
					log("Body: NULL");
				} else {
					log("Body: " + body.toString());
				}
				
				mQuery = createQuery(rndGen);
				
				JsonElement queryJson = createQueryObject(mQuery);
				log ("Query: " + queryJson.toString());
				
				String api = apiNames.get(permission);
				log ("API: " + api);
				
				if (body == null && method.equals(HttpPost.METHOD_NAME) && mQuery == null) {
					client.invokeApi(api, jsonTestCallback());
				} else if (body != null && method.equals(HttpPost.METHOD_NAME) && mQuery == null) {
					client.invokeApi(api, body, jsonTestCallback());
				} else if (body == null) {
					client.invokeApi(api, method, mQuery, jsonTestCallback());
				} else {
					client.invokeApi(api, body, method, mQuery, jsonTestCallback());
				}
			}
			
			private ApiJsonOperationCallback jsonTestCallback() {
				return new ApiJsonOperationCallback() {
					
					@Override
					public void onCompleted(JsonElement jsonElement, Exception exception, ServiceFilterResponse response) {
						if (mExpected401) {
							if (response == null || response.getStatus().getStatusCode() != 401) {
								mResult.setStatus(TestStatus.Failed);
								mResult.setException(exception);
								mResult.getTestCase().log("Expected 401");
							}
						} else {
							if (exception != null) {
								createResultFromException(mResult, exception);
							} else {
								JsonObject expectedResult = new JsonObject();
								expectedResult.add("user", createUserObject(mClient));
								
								if (mQuery != null && mQuery.size() > 0) {
									expectedResult.add("query", createQueryObject(mQuery));
								}
								
								if (!Util.compareJson(expectedResult, jsonElement)) {
									createResultFromException(mResult, new ExpectedValueException(expectedResult, jsonElement));
								}
							}
						}
						
						mCallback.onTestComplete(mResult.getTestCase(), mResult);
					}
				};
			}
		};
		
		return test;
	}
	
	protected List<Pair<String, String>> createQuery(Random rndGen) {
		if (rndGen.nextInt(2) == 0) {
			return null;
		} else {
			List<Pair<String, String>> query = new ArrayList<Pair<String,String>>();
			
			int size = rndGen.nextInt(5);
			
			for (int i = 0; i < size; i++) {
				query.add(new Pair<String, String>(Util.createSimpleRandomString(rndGen, rndGen.nextInt(10) + 1, 'a', 'z'), Util.createComplexRandomString(rndGen, rndGen.nextInt(30))));
			}
			
			return query;
		}
	}

	private JsonObject createUserObject(MobileServiceClient client) {
		JsonObject json = new JsonObject();
		
		if (client.getCurrentUser() == null) {
			json.addProperty("level", "anonymous");
		} else {
			json.addProperty("level", "authenticated");
			json.addProperty("userid", client.getCurrentUser().getUserId());
		}
		
		return json;
	}
	
	private JsonElement createQueryObject(List<Pair<String, String>> query) {
		JsonObject json = new JsonObject();
		if (query != null) {
			for (Pair<String, String> element : query) {
				json.addProperty(element.first, element.second);
			}
			
			return json;
		} else {
			return JsonNull.INSTANCE;
		}
	}
	
	private JsonElement createJson(Random rndGen, int currentDepth, boolean canBeNull) {
		final int maxDepth = 3;
		
		int kind = rndGen.nextInt(15);
		
		if (currentDepth == 0) {
			kind = rndGen.nextInt(8) + 8;
		}
		
		switch (kind) {
		case 0:
			return new JsonPrimitive(true);

		case 1:
			return new JsonPrimitive(false);

		case 2:
			return new JsonPrimitive(rndGen.nextInt());

		case 3:
			return new JsonPrimitive(rndGen.nextInt() >> rndGen.nextInt(10));

		case 4:
		case 5:
		case 6:
			return new JsonPrimitive(Util.createComplexRandomString(rndGen, rndGen.nextInt(10)));

		case 7:
			if (canBeNull) {
				return JsonNull.INSTANCE;
			} else {
				return new JsonPrimitive(Util.createComplexRandomString(rndGen, rndGen.nextInt(10)));
			}
			
		case 8:
		case 9:
		case 10:
			if (currentDepth > maxDepth) {
				return new JsonPrimitive("max depth");
			} else {
				int size = rndGen.nextInt(5);
				JsonArray array = new JsonArray();
				for (int i = 0; i < size; i++) {
					array.add(createJson(rndGen, currentDepth + 1, true));
				}
				
				return array;
			}

		default:
			if (currentDepth > maxDepth) {
				return new JsonPrimitive("max depth");
			} else {
				int size = rndGen.nextInt(5);
				JsonObject result = new JsonObject();
				
				for (int i = 0; i < size; i++) {
					String key;
					do {
						key = Util.createComplexRandomString(rndGen, rndGen.nextInt(3) + 3);
					} while (result.has(key));
					
					result.add(key, createJson(rndGen, currentDepth + 1, true));
				}
				
				return result;
			}
		}
	}

	private String createHttpMethod(final Random rndGen) {
		switch (rndGen.nextInt(10))
		{
			case 0: case 1: case 2:
				return HttpPost.METHOD_NAME;
			case 3: case 4: case 5: case 6:
				return HttpGet.METHOD_NAME;
			case 7:
				return HttpPut.METHOD_NAME;
			case 8:
				return HttpDelete.METHOD_NAME;
			default:
				return PATCH_METHOD_NAME;
		}
	}
}
