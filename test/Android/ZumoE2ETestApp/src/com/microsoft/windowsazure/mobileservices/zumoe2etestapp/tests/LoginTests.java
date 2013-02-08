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

import java.util.Locale;

import com.google.gson.JsonObject;
import com.microsoft.windowsazure.mobileservices.MobileServiceAuthenticationProvider;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceJsonTable;
import com.microsoft.windowsazure.mobileservices.MobileServiceUser;
import com.microsoft.windowsazure.mobileservices.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.TableDeleteCallback;
import com.microsoft.windowsazure.mobileservices.TableJsonOperationCallback;
import com.microsoft.windowsazure.mobileservices.UserAuthenticationCallback;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.LogServiceFilter;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestCase;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestExecutionCallback;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestGroup;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestResult;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestStatus;

public class LoginTests extends TestGroup {

	protected static final String MOVIES_TABLE_NAME = "movies";

	public LoginTests() {
		super("Login tests");

		this.addTest(createLogoutTest());
		this.addTest(createCRUDTest("application", null, TablePermission.Application, false));
		this.addTest(createCRUDTest("authenticated", null, TablePermission.User, false));
		this.addTest(createCRUDTest("admin", null, TablePermission.Admin, false));

		for (MobileServiceAuthenticationProvider provider : MobileServiceAuthenticationProvider.values()) {
			this.addTest(createLogoutTest());
			this.addTest(createLoginTest(provider));
			this.addTest(createCRUDTest("Application", provider, TablePermission.Application, true));
			this.addTest(createCRUDTest("Authenticated", provider, TablePermission.User, true));
			this.addTest(createCRUDTest("Admin", provider, TablePermission.Admin, true));
		}
	}

	private TestCase createLoginTest(final MobileServiceAuthenticationProvider provider) {
		TestCase test = new TestCase() {

			@Override
			protected void executeTest(final MobileServiceClient client, final TestExecutionCallback callback) {
				final TestCase testCase = this;
				client.login(provider, new UserAuthenticationCallback() {

					@Override
					public void onCompleted(MobileServiceUser user, Exception exception, ServiceFilterResponse response) {
						TestResult result = new TestResult();
						result.setStatus(client.getCurrentUser() != null ? TestStatus.Passed : TestStatus.Failed);
						result.setTestCase(testCase);

						callback.onTestComplete(testCase, result);
					}
				});
			}
		};

		test.setName("Login with " + provider.toString());
		return test;
	}

	enum TablePermission {
		Public, Application, User, Admin
	}

	private TestCase createLogoutTest() {

		TestCase test = new TestCase() {

			@Override
			protected void executeTest(MobileServiceClient client, TestExecutionCallback callback) {

				client.logout();
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
