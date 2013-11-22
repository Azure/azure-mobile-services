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

import java.util.ArrayList;
import java.util.Date;
import java.util.List;
import java.util.Locale;
import java.util.Random;


import com.google.gson.JsonObject;
import com.microsoft.windowsazure.mobileservices.MobileServiceAuthenticationProvider;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceJsonTable;
import com.microsoft.windowsazure.mobileservices.MobileServiceUser;
import com.microsoft.windowsazure.mobileservices.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.TableDeleteCallback;
import com.microsoft.windowsazure.mobileservices.TableJsonOperationCallback;
import com.microsoft.windowsazure.mobileservices.UserAuthenticationCallback;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.MainActivity;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.LogServiceFilter;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestCase;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestExecutionCallback;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestGroup;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestResult;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestStatus;

public class LoginTests extends TestGroup {

	protected static final String APPLICATION_PERMISSION_TABLE_NAME = "droidApplication";
	protected static final String USER_PERMISSION_TABLE_NAME = "droidAuthenticated";
	protected static final String ADMIN_PERMISSION_TABLE_NAME = "droidAdmin";

	private static JsonObject lastUserIdentityObject;

	public LoginTests() {
		super("Login tests");

		this.addTest(createLogoutTest());
		this.addTest(createCRUDTest(APPLICATION_PERMISSION_TABLE_NAME, null, TablePermission.Application, false));
		this.addTest(createCRUDTest(USER_PERMISSION_TABLE_NAME, null, TablePermission.User, false));
		this.addTest(createCRUDTest(ADMIN_PERMISSION_TABLE_NAME, null, TablePermission.Admin, false));

		int indexOfStartAuthenticationTests = this.getTestCases().size();
		
		ArrayList<MobileServiceAuthenticationProvider> providersWithRecycledTokenSupport = new ArrayList<MobileServiceAuthenticationProvider>();
		providersWithRecycledTokenSupport.add(MobileServiceAuthenticationProvider.Facebook);
		// Known bug - Drop login via Google token until Google client flow is reintroduced
		//providersWithRecycledTokenSupport.add(MobileServiceAuthenticationProvider.Google);

		for (MobileServiceAuthenticationProvider provider : MobileServiceAuthenticationProvider.values()) {
			this.addTest(createLogoutTest());
			this.addTest(createLoginTest(provider));
			this.addTest(createCRUDTest(APPLICATION_PERMISSION_TABLE_NAME, provider, TablePermission.Application, true));
			this.addTest(createCRUDTest(USER_PERMISSION_TABLE_NAME, provider, TablePermission.User, true));
			this.addTest(createCRUDTest(ADMIN_PERMISSION_TABLE_NAME, provider, TablePermission.Admin, true));

			if (providersWithRecycledTokenSupport.contains(provider)) {
				this.addTest(createLogoutTest());
				this.addTest(createClientSideLoginTest(provider));
				this.addTest(createCRUDTest(USER_PERMISSION_TABLE_NAME, provider, TablePermission.User, true));
			}
		}

		this.addTest(createLogoutTest());
		this.addTest(createLoginWithGoogleAccountTest(true, null));

		this.addTest(createLogoutTest());
		this.addTest(createLoginWithGoogleAccountTest(true, MobileServiceClient.GOOGLE_USER_INFO_SCOPE + " https://www.googleapis.com/auth/userinfo.email"));

		this.addTest(createLogoutTest());
		this.addTest(createLoginWithGoogleAccountTest(false, null));
		
		List<TestCase> testCases = this.getTestCases();
		for (int i = indexOfStartAuthenticationTests; i < testCases.size(); i++) {
			testCases.get(i).setCanRunUnattended(false);
		}
	}

	private TestCase createLoginWithGoogleAccountTest(final boolean useDefaultAccount, final String customScope) {
		StringBuilder name = new StringBuilder();
		
		name.append("Login with Google Account - ");
		if (useDefaultAccount) {
			name.append("Using default account - ");
		} else {
			name.append("Using null account - ");
		}
		
		if (customScope == null) {
			name.append("Using default scope");
		} else {
			name.append("Using custom scope");
		}
		
		TestCase test = new TestCase(name.toString()) {
			
			@Override
			protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {
				final TestCase testCase = this;
				
				final TestResult testResult = new TestResult();
				testResult.setTestCase(testCase);
				testResult.setStatus(TestStatus.Passed);
				
				UserAuthenticationCallback userAuthCallback = new UserAuthenticationCallback() {
					
					@Override
					public void onCompleted(MobileServiceUser user, Exception exception,
							ServiceFilterResponse response) {
						if (user != null) {
							log("User successfully authenticated. UserId: " + user.getUserId());
							callback.onTestComplete(testCase, testResult);
						} else {
							log("User was not authenticated");
							callback.onTestComplete(testCase, createResultFromException(testResult, exception));
						}
					}
				};
				
				String scope = customScope;
				
				if (scope == null) {
					scope = MobileServiceClient.GOOGLE_USER_INFO_SCOPE;
				}
				
				if (useDefaultAccount) {
					client.loginWithGoogleAccount(MainActivity.getInstance(), scope, userAuthCallback);
				} else {
					testCase.setExpectedExceptionClass(IllegalArgumentException.class);
					client.loginWithGoogleAccount(MainActivity.getInstance(), null, scope, userAuthCallback);
				}
			}
		};
		
		return test;
	}

	private TestCase createClientSideLoginTest(final MobileServiceAuthenticationProvider provider) {
		TestCase test = new TestCase("Login via token for " + provider.toString()) {

			@Override
			protected void executeTest(MobileServiceClient client,
					final TestExecutionCallback callback) {

				final TestCase testCase = this;
				long seed = new Date().getTime();
				final Random rndGen = new Random(seed);

				if (lastUserIdentityObject == null) {
					log("Last identity is null. Cannot run this test.");
					TestResult testResult = new TestResult();
					testResult.setTestCase(testCase);
					testResult.setStatus(TestStatus.Failed);
					callback.onTestComplete(testCase, testResult);
					return;
				}

				JsonObject lastIdentity = lastUserIdentityObject;
				lastUserIdentityObject = null;
				JsonObject providerIdentity = lastIdentity.getAsJsonObject(provider.toString().toLowerCase(Locale.US));
				if (providerIdentity == null) {
					log("Cannot find identity for specified provider. Cannot run this test.");
					TestResult testResult = new TestResult();
					testResult.setTestCase(testCase);
					testResult.setStatus(TestStatus.Failed);
					callback.onTestComplete(testCase, testResult);
					return;
				}

				JsonObject token = new JsonObject();
				token.addProperty("access_token", providerIdentity.get("accessToken").getAsString());
				UserAuthenticationCallback authCallback = new UserAuthenticationCallback() {

					@Override
					public void onCompleted(MobileServiceUser user,
							Exception exception, ServiceFilterResponse response) {
						TestResult testResult = new TestResult();
						testResult.setTestCase(testCase);
						if (exception != null) {
							log("Exception during login: " + exception.toString());
							testResult.setStatus(TestStatus.Failed);
						} else {
							log("Logged in as " + user.getUserId());
							testResult.setStatus(TestStatus.Passed);
						}

						callback.onTestComplete(testCase, testResult);
					}
				};
				boolean useEnumOverload = rndGen.nextBoolean();
				if (useEnumOverload) {
					log("Calling the overload MobileServiceClient.login(MobileServiceAuthenticationProvider, JsonObject, UserAuthenticationCallback)");
					client.login(provider, token, authCallback);
				} else {
					log("Calling the overload MobileServiceClient.login(String, JsonObject, UserAuthenticationCallback)");
					client.login(provider.toString(), token, authCallback);
				}
			}
		};

		return test;
	}

	public static TestCase createLoginTest(final MobileServiceAuthenticationProvider provider) {
		TestCase test = new TestCase("Login with " + provider.toString()) {

			@Override
			protected void executeTest(final MobileServiceClient client, final TestExecutionCallback callback) {
				final TestCase testCase = this;
				long seed = new Date().getTime();
				final Random rndGen = new Random(seed);

				UserAuthenticationCallback authCallback = new UserAuthenticationCallback() {

					@Override
					public void onCompleted(MobileServiceUser user, Exception exception, ServiceFilterResponse response) {
						TestResult result = new TestResult();
						String userName;
						if(user == null)
						{
							log("Error during login, user == null");
							if (exception != null) {
								log("Exception: " + exception.toString());
							}

							userName = "NULL";
						}
						else
						{
							userName = user.getUserId();
						}
						
						log("Logged in as " + userName);
						result.setStatus(client.getCurrentUser() != null ? TestStatus.Passed : TestStatus.Failed);
						result.setTestCase(testCase);

						callback.onTestComplete(testCase, result);
					}
				};

				boolean useEnumOverload = rndGen.nextBoolean();
				if (useEnumOverload) {
					log("Calling the overload MobileServiceClient.login(MobileServiceAuthenticationProvider, UserAuthenticationCallback)");
					client.login(provider, authCallback);
				} else {
					log("Calling the overload MobileServiceClient.login(String, UserAuthenticationCallback)");
					client.login(provider.toString(), authCallback);
				}
			}
		};

		return test;
	}

	enum TablePermission {
		Public, Application, User, Admin
	}

	public static TestCase createLogoutTest() {

		TestCase test = new TestCase() {

			@Override
			protected void executeTest(MobileServiceClient client, TestExecutionCallback callback) {
				
				client.logout();
				log("Logged out");
				TestResult result = new TestResult();
				result.setTestCase(this);
				result.setStatus(client.getCurrentUser() == null ? TestStatus.Passed : TestStatus.Failed);

				callback.onTestComplete(this, result);
			}
		};

		test.setName("Logout");

		return test;
	}

	private TestCase createCRUDTest(final String tableName, final MobileServiceAuthenticationProvider provider, final TablePermission tableType,
			final boolean userIsAuthenticated) {
		final TestCase test = new TestCase() {

			@Override
			protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {

				final TestResult result = new TestResult();
				result.setStatus(TestStatus.Passed);
				result.setTestCase(this);
				final TestCase testCase = this;

				MobileServiceClient logClient = client.withFilter(new LogServiceFilter());

				final MobileServiceJsonTable table = logClient.getTable(tableName);
				final boolean crudShouldWork = tableType == TablePermission.Public || tableType == TablePermission.Application
						|| (tableType == TablePermission.User && userIsAuthenticated);
				final JsonObject item = new JsonObject();
				item.addProperty("name", "John Doe");
				log("insert item");
				table.insert(item, new TableJsonOperationCallback() {

					@Override
					public void onCompleted(JsonObject jsonEntity, Exception exception, ServiceFilterResponse response) {
						int id = 1;
						if (exception == null) {
							id = jsonEntity.get("id").getAsInt();
						}

						item.addProperty("id", id);
						if (!validateExecution(crudShouldWork, exception, result)) {
							callback.onTestComplete(testCase, result);
							return;
						}

						item.addProperty("name", "Jane Doe");
						log("update item");
						table.update(item, new TableJsonOperationCallback() {

							@Override
							public void onCompleted(JsonObject jsonEntity, Exception exception, ServiceFilterResponse response) {

								if (!validateExecution(crudShouldWork, exception, result)) {
									callback.onTestComplete(testCase, result);
									return;
								}

								log("lookup item");
								table.lookUp(item.get("id").getAsInt(), new TableJsonOperationCallback() {

									@Override
									public void onCompleted(JsonObject jsonEntity, Exception exception, ServiceFilterResponse response) {
										if (!validateExecution(crudShouldWork, exception, result)) {
											callback.onTestComplete(testCase, result);
											return;
										}

										if (userIsAuthenticated && tableType == TablePermission.User) {
											lastUserIdentityObject = jsonEntity.getAsJsonObject("Identities");
										}

										log("delete item");
										table.delete(item.get("id").getAsInt(), new TableDeleteCallback() {

											@Override
											public void onCompleted(Exception exception, ServiceFilterResponse response) {
												validateExecution(crudShouldWork, exception, result);

												callback.onTestComplete(testCase, result);
												return;
											}
										});
									}
								});
							}
						});
					}
				});
			}

			private boolean validateExecution(boolean crudShouldWork, Exception exception, TestResult result) {
				if (crudShouldWork && exception != null || !crudShouldWork && exception == null) {
					createResultFromException(result, exception);
					result.setStatus(TestStatus.Failed);
					return false;
				} else {
					return true;
				}
			}
		};

		String testKind;
		if (userIsAuthenticated) {
			testKind = "auth by " + provider.toString();
		} else {
			testKind = "unauthenticated";
		}

		String testName = String.format(Locale.getDefault(), "CRUD, %s, table with %s permissions", testKind, tableType.toString());
		test.setName(testName);

		return test;
	}

}
