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

import java.util.Random;

import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceException;
import com.microsoft.windowsazure.mobileservices.MobileServiceList;
import com.microsoft.windowsazure.mobileservices.table.MobileServiceJsonTable;
import com.microsoft.windowsazure.mobileservices.table.MobileServiceTable;
import com.microsoft.windowsazure.mobileservices.table.query.ExecutableQuery;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.ExpectedValueException;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestCase;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestExecutionCallback;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestGroup;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestResult;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestStatus;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.Util;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.types.RoundTripTableElement;
import com.microsoft.windowsazure.mobileservices.table.TableDeleteCallback;
import com.microsoft.windowsazure.mobileservices.table.TableOperationCallback;
import com.microsoft.windowsazure.mobileservices.table.TableJsonOperationCallback;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests.types.StringIdRoundTripTableElement;

public class UpdateDeleteTests extends TestGroup {

	protected static final String ROUNDTRIP_TABLE_NAME = "droidRoundTripTable";

    protected static final String STRING_ID_ROUNDTRIP_TABLE_NAME = "stringIdRoundTripTable";

	public UpdateDeleteTests() {
		super("Insert/Update/Delete Tests");

		Random rndGen = new Random();

		// typed update

		this.addTest(createTypedUpdateTest("Update typed item", new RoundTripTableElement(rndGen), new RoundTripTableElement(rndGen), true, null));

		this.addTest(createTypedUpdateTest("Update typed item, setting values to null", new RoundTripTableElement(rndGen), new RoundTripTableElement(false),
				true, null));

		RoundTripTableElement elem1 = new RoundTripTableElement(rndGen);
		RoundTripTableElement elem2 = new RoundTripTableElement(rndGen);
		elem2.id = 1000000000;

		this.addTest(createTypedUpdateTest("(Neg) Update typed item, non-existing item id", elem1, elem2, false, MobileServiceException.class));

		elem1 = new RoundTripTableElement(rndGen);
		elem2 = new RoundTripTableElement(rndGen);
		elem2.id = 0;

		this.addTest(createTypedUpdateTest("(Neg) Update typed item, id = 0", elem1, elem2, false, IllegalArgumentException.class));

		// untyped update
		JsonParser parser = new JsonParser();

		String toInsertJsonString = "{" + "\"string1\":\"hello\"," + "\"bool1\":true," + "\"int1\":-1234," + "\"double1\":123.45," + "\"long1\":1234,"
				+ "\"date1\":\"2012-12-13T09:23:12.000Z\"," + "\"complexType1\":[{\"Name\":\"John Doe\",\"Age\":33}, null],"
				+ "\"complexType2\":{\"Name\":\"John Doe\",\"Age\":33,\"Friends\":[\"Jane\"]}" + "}";

		String toUpdateJsonString = "{" + "\"string1\":\"world\"," + "\"bool1\":false," + "\"int1\":9999," + "\"double1\":888.88," + "\"long1\":77777777,"
				+ "\"date1\":\"1999-05-23T19:15:54.000Z\"," + "\"complexType1\":[{\"Name\":\"Jane Roe\",\"Age\":23}, null],"
				+ "\"complexType2\":{\"Name\":\"Jane Roe\",\"Age\":23,\"Friends\":[\"John\"]}" + "}";

		this.addTest(createUntypedUpdateTest("Update typed item", parser.parse(toInsertJsonString).getAsJsonObject(), parser.parse(toUpdateJsonString)
				.getAsJsonObject(), true, null));

		JsonObject toUpdate = parser.parse(toUpdateJsonString).getAsJsonObject();
		toUpdate.add("string1", null);
		toUpdate.add("bool1", null);
		toUpdate.add("complexType2", null);
		toUpdate.add("complexType1", null);
		toUpdate.add("int1", null);

		this.addTest(createUntypedUpdateTest("Update untyped item, setting values to null", parser.parse(toInsertJsonString).getAsJsonObject(),
				cloneJson(toUpdate), true, null));

		toUpdate.addProperty("id", 1000000000);
		this.addTest(createUntypedUpdateTest("(Neg) Update untyped item, non-existing item id", parser.parse(toInsertJsonString).getAsJsonObject(),
				cloneJson(toUpdate), false, MobileServiceException.class));

		toUpdate.addProperty("id", 0);
		this.addTest(createUntypedUpdateTest("(Neg) Update untyped item, id = 0", parser.parse(toInsertJsonString).getAsJsonObject(), cloneJson(toUpdate),
				false, IllegalArgumentException.class));

		// delete tests
		this.addTest(createDeleteTest("Delete typed item", true, false, true, null));
		this.addTest(createDeleteTest("(Neg) Delete typed item with non-existing id", true, true, true, MobileServiceException.class));
		this.addTest(createDeleteTest("Delete untyped item", false, false, true, null));
		this.addTest(createDeleteTest("(Neg) Delete untyped item with non-existing id", false, true, true, MobileServiceException.class));
		this.addTest(createDeleteTest("(Neg) Delete untyped item without id field", false, false, false, IllegalArgumentException.class));

        // soft delete test
        this.addTest(createSoftDeleteTest("Soft Delete typed item - Return on query", true, false, true, true, null));
        this.addTest(createSoftDeleteTest("Soft Delete typed item - No return on query", true, false, true, false, null));
        this.addTest(createSoftDeleteTest("(Neg)Soft Delete typed item with non-existing id", true, true, true, false, MobileServiceException.class));
        this.addTest(createSoftDeleteTest("Soft Delete untyped item - Return on query", false, false, true, true, null));
        this.addTest(createSoftDeleteTest("Soft Delete untyped item - No return on query", false, false, true, false, null));
        this.addTest(createSoftDeleteTest("(Neg)Soft Delete untyped item with non-existing id", false, true, true, false, MobileServiceException.class));
        this.addTest(createSoftDeleteTest("(Neg)Soft Delete untyped item without id field", false, false, false, false, IllegalArgumentException.class));

        this.addTest(createSoftDeleteUndeleteTest("Soft Delete - Undelete typed item - Return on query", true, null));
        this.addTest(createSoftDeleteUndeleteTest("Soft Delete - Undelete untyped item - No return on query", false,  null));


        // With Callbacks
		this.addTest(createTypedUpdateWithCallbackTest("With Callback - Update typed item", new RoundTripTableElement(rndGen),
				new RoundTripTableElement(rndGen), true, null));
		this.addTest(createUntypedUpdateWithCallbackTest("With Callback - Update untyped item, setting values to null", parser.parse(toInsertJsonString)
				.getAsJsonObject(), cloneJson(toUpdate), true, null));

		this.addTest(createDeleteWithCallbackTest("With Callback - Delete typed item", true, false, true, null));
		this.addTest(createDeleteWithCallbackTest("With Callback - Delete untyped item", false, false, true, null));

	}

	private JsonObject cloneJson(JsonObject json) {
		return new JsonParser().parse(json.toString()).getAsJsonObject();
	}

	private TestCase createTypedUpdateTest(String name, final RoundTripTableElement itemToInsert, final RoundTripTableElement itemToUpdate,
			final boolean setUpdatedId, final Class<?> expectedExceptionClass) {

		final TestCase test = new TestCase() {

			@Override
			protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {

				final MobileServiceTable<RoundTripTableElement> table = client.getTable(ROUNDTRIP_TABLE_NAME, RoundTripTableElement.class);
				final TestCase testCase = this;

				log("insert item");
				itemToInsert.id = null;

				final TestResult result = new TestResult();
				result.setTestCase(testCase);
				result.setStatus(TestStatus.Passed);

				try {

					RoundTripTableElement insertedItem = table.insert(itemToInsert).get();

					if (setUpdatedId) {
						log("update item id " + insertedItem.id);
						itemToUpdate.id = insertedItem.id;
					}

					log("update the item");

					RoundTripTableElement updatedItem = table.update(itemToUpdate).get();

					log("lookup item");

					RoundTripTableElement lookedUpItem = table.lookUp(updatedItem.id).get();

					log("verify items are equal");
					if (Util.compare(updatedItem, lookedUpItem)) { // check
																	// the
																	// items
																	// are
																	// equal
						log("cleanup");

						table.delete(lookedUpItem.id).get();// clean
															// up

						if (callback != null)
							callback.onTestComplete(testCase, result);

					} else {
						createResultFromException(result, new ExpectedValueException(updatedItem, lookedUpItem));
						if (callback != null)
							callback.onTestComplete(testCase, result);
					}
				} catch (Exception exception) {
					createResultFromException(result, exception);
					if (callback != null)
						callback.onTestComplete(testCase, result);
				}
			}
		};

		test.setExpectedExceptionClass(expectedExceptionClass);
		test.setName(name);

		return test;
	}

	private TestCase createUntypedUpdateTest(String name, final JsonObject itemToInsert, final JsonObject itemToUpdate, final boolean setUpdatedId,
			final Class<?> expectedExceptionClass) {

		final TestCase test = new TestCase() {

			@Override
			protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {

				final MobileServiceJsonTable table = client.getTable(ROUNDTRIP_TABLE_NAME);
				final TestCase testCase = this;

				final TestResult result = new TestResult();
				result.setTestCase(testCase);
				result.setStatus(TestStatus.Passed);

				log("insert item");

				try {

					JsonObject insertedItem = table.insert(itemToInsert).get();

					if (setUpdatedId) {
						int id = insertedItem.get("id").getAsInt();
						log("update item id " + id);
						itemToUpdate.addProperty("id", id);
					}

					log("update the item");
					JsonObject updatedItem = table.update(itemToUpdate).get();

					log("lookup the item");
					JsonObject lookedUpItem = (JsonObject) table.lookUp(updatedItem.get("id").getAsInt()).get();

					log("verify items are equal");
					if (Util.compareJson(updatedItem, lookedUpItem)) { // check
																		// the
																		// items
																		// are
																		// equal
						log("cleanup");
						table.delete(lookedUpItem.get("id").getAsInt()).get(); // clean

						if (callback != null)
							callback.onTestComplete(testCase, result);
					} else {
						createResultFromException(result, new ExpectedValueException(updatedItem, lookedUpItem));
						if (callback != null)
							callback.onTestComplete(testCase, result);
					}
				} catch (Exception exception) {
					createResultFromException(result, exception);
					if (callback != null)
						callback.onTestComplete(testCase, result);
				}
			}
		};

		test.setExpectedExceptionClass(expectedExceptionClass);
		test.setName(name);

		return test;
	}

	private TestCase createSoftDeleteTest(String name, final boolean typed, final boolean useFakeId, final boolean includeId, final boolean includeSoftDeleteInQueries, Class<?> expectedExceptionClass) {
		TestCase testCase = new TestCase() {

			@Override
			protected void executeTest(final MobileServiceClient client, final TestExecutionCallback callback) {

				StringIdRoundTripTableElement element = new StringIdRoundTripTableElement(new Random());

				final MobileServiceTable<StringIdRoundTripTableElement> table = client.getTable(STRING_ID_ROUNDTRIP_TABLE_NAME, StringIdRoundTripTableElement.class);

				final TestResult result = new TestResult();
				result.setStatus(TestStatus.Passed);

				final TestCase testCase = this;
				result.setTestCase(testCase);

				log("insert item");

				try {

                    StringIdRoundTripTableElement entity = table.insert(element).get();

					Object deleteObject;

                    if (useFakeId) {
                        log("use fake id");
                        entity.id = "1000000000";
                    }

                    if (!includeId) {
                        log("include id");
                        entity.id = null;
                    }

					if (typed) {
						deleteObject = entity;
					} else {
						deleteObject = client.getGsonBuilder().create().toJsonTree(entity).getAsJsonObject();
					}


					log("soft delete");

					table.delete(deleteObject).get();

                    log("verifiying if was soft deleted");

                    if (includeSoftDeleteInQueries) {

                        ExecutableQuery<StringIdRoundTripTableElement> query =
                                table.includeDeleted().field("id").eq(entity.id);

                        MobileServiceList<StringIdRoundTripTableElement> results = table.execute(query).get();

                        if (results.size() != 1) {
                            createResultFromException(result, new ExpectedValueException(1, 0));
                        }

                        StringIdRoundTripTableElement deletedElement = results.get(0);

                        if (!deletedElement.Deleted) {
                            createResultFromException(result, new ExpectedValueException(true, false));
                        }
                    } else {

                        try {
                            StringIdRoundTripTableElement resultElement = table.lookUp(entity.id).get();
                        }
                        catch (Exception exception) {

                            MobileServiceException ex = (MobileServiceException) exception.getCause();

                            String message = "An item with id '" + entity.id + "' does not exist.";

                            if (!ex.getMessage().contains(message)) {
                                createResultFromException(result, new ExpectedValueException(message, ex.getMessage()));
                            }
                        }
                    }
				} catch (Exception exception) {
					if (exception != null) {
						createResultFromException(result, exception);
					}
				} finally {
					if (callback != null)
						callback.onTestComplete(testCase, result);
				}
			}
		};

		testCase.setName(name);
		testCase.setExpectedExceptionClass(expectedExceptionClass);

		return testCase;
	}

    private TestCase createSoftDeleteUndeleteTest(String name, final boolean typed, Class<?> expectedExceptionClass) {
        TestCase testCase = new TestCase() {

            @Override
            protected void executeTest(final MobileServiceClient client, final TestExecutionCallback callback) {

                StringIdRoundTripTableElement element = new StringIdRoundTripTableElement(new Random());

                final MobileServiceTable<StringIdRoundTripTableElement> table = client.getTable(STRING_ID_ROUNDTRIP_TABLE_NAME, StringIdRoundTripTableElement.class);

                final TestResult result = new TestResult();
                result.setStatus(TestStatus.Passed);

                final TestCase testCase = this;
                result.setTestCase(testCase);

                log("insert item");

                try {

                    StringIdRoundTripTableElement entity = table.insert(element).get();

                    Object deleteObject;

                    if (typed) {
                        deleteObject = entity;
                    } else {
                        deleteObject = client.getGsonBuilder().create().toJsonTree(entity).getAsJsonObject();
                    }

                    log("soft delete");

                    table.delete(deleteObject).get();

                    log("verifiying if was soft deleted");

                    try {
                        StringIdRoundTripTableElement resultElement = table.lookUp(entity.id).get();
                    }
                    catch (Exception exception) {

                        MobileServiceException ex = (MobileServiceException) exception.getCause();

                        String message = "An item with id '" + entity.id + "' does not exist.";

                        if (!ex.getMessage().contains(message)) {
                            createResultFromException(result, new ExpectedValueException(message, ex.getMessage()));
                        }
                    }

                    log("undelete element");

                    if (typed) {
                        table.undelete(entity).get();
                    } else {

                        MobileServiceJsonTable jsonTable = client.getTable(STRING_ID_ROUNDTRIP_TABLE_NAME);

                        jsonTable.undelete((JsonObject) deleteObject).get();
                    }

                    log("read undeleted element");
                    StringIdRoundTripTableElement resultElement = table.lookUp(entity.id).get();


                } catch (Exception exception) {
                    if (exception != null) {
                        createResultFromException(result, exception);
                    }
                } finally {
                    if (callback != null)
                        callback.onTestComplete(testCase, result);
                }
            }
        };

        testCase.setName(name);
        testCase.setExpectedExceptionClass(expectedExceptionClass);

        return testCase;
    }


    private TestCase createDeleteTest(String name, final boolean typed, final boolean useFakeId, final boolean includeId, Class<?> expectedExceptionClass) {
        TestCase testCase = new TestCase() {

            @Override
            protected void executeTest(final MobileServiceClient client, final TestExecutionCallback callback) {
                RoundTripTableElement element = new RoundTripTableElement(new Random());
                final MobileServiceTable<RoundTripTableElement> table = client.getTable(ROUNDTRIP_TABLE_NAME, RoundTripTableElement.class);

                final TestResult result = new TestResult();
                result.setStatus(TestStatus.Passed);

                final TestCase testCase = this;
                result.setTestCase(testCase);

                log("insert item");

                try {

                    RoundTripTableElement entity = table.insert(element).get();

                    Object deleteObject;

                    if (useFakeId) {
                        log("use fake id");
                        entity.id = 1000000000;
                    }

                    if (!includeId) {
                        log("include id");
                        entity.id = null;
                    }

                    if (typed) {
                        deleteObject = entity;
                    } else {
                        deleteObject = client.getGsonBuilder().create().toJsonTree(entity).getAsJsonObject();
                    }

                    log("delete");

                    table.delete(deleteObject).get();

                } catch (Exception exception) {
                    if (exception != null) {
                        createResultFromException(result, exception);
                    }
                } finally {
                    if (callback != null)
                        callback.onTestComplete(testCase, result);
                }
            }
        };

        testCase.setName(name);
        testCase.setExpectedExceptionClass(expectedExceptionClass);

        return testCase;
    }

    @SuppressWarnings("deprecation")
	private TestCase createTypedUpdateWithCallbackTest(String name, final RoundTripTableElement itemToInsert, final RoundTripTableElement itemToUpdate,
			final boolean setUpdatedId, final Class<?> expectedExceptionClass) {

		final TestCase test = new TestCase() {

			@Override
			protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {

				final MobileServiceTable<RoundTripTableElement> table = client.getTable(ROUNDTRIP_TABLE_NAME, RoundTripTableElement.class);
				final TestCase testCase = this;

				log("insert item");
				itemToInsert.id = null;

				table.insert(itemToInsert, new TableOperationCallback<RoundTripTableElement>() {

					@Override
					public void onCompleted(final RoundTripTableElement insertedItem, Exception exception, ServiceFilterResponse response) {
						final TestResult result = new TestResult();
						result.setTestCase(testCase);
						result.setStatus(TestStatus.Passed);

						if (exception == null) { // if it was ok

							if (setUpdatedId) {
								log("update item id " + insertedItem.id);
								itemToUpdate.id = insertedItem.id;
							}

							log("update the item");
							table.update(itemToUpdate, new TableOperationCallback<RoundTripTableElement>() {

								@Override
								public void onCompleted(final RoundTripTableElement updatedItem, Exception exception, ServiceFilterResponse response) {

									if (exception == null) { // if it was ok

										log("lookup item");
										table.lookUp(updatedItem.id, new TableOperationCallback<RoundTripTableElement>() {

											@Override
											public void onCompleted(RoundTripTableElement lookedUpItem, Exception exception, ServiceFilterResponse response) {
												if (exception == null) { // if
																			// it
																			// was
																			// ok

													log("verify items are equal");
													if (Util.compare(updatedItem, lookedUpItem)) { // check
																									// the
																									// items
																									// are
																									// equal
														log("cleanup");
														table.delete(lookedUpItem.id, new TableDeleteCallback() { // clean
																													// up

																	@Override
																	public void onCompleted(Exception exception, ServiceFilterResponse response) {
																		if (exception != null) {
																			createResultFromException(result, exception);
																		}

																		// callback
																		// with
																		// success
																		// or
																		// error
																		// on
																		// cleanup
																		if (callback != null)
																			callback.onTestComplete(testCase, result);
																	}
																});
													} else {
														createResultFromException(result, new ExpectedValueException(insertedItem, lookedUpItem));
														if (callback != null)
															callback.onTestComplete(testCase, result);
													}
												} else {
													createResultFromException(result, exception);
													if (callback != null)
														callback.onTestComplete(testCase, result);
												}
											}

										});
									} else {
										createResultFromException(result, exception);
										if (callback != null)
											callback.onTestComplete(testCase, result);
									}
								}

							});
						} else {
							createResultFromException(result, exception);
							if (callback != null)
								callback.onTestComplete(testCase, result);
						}
					}

				});

			}
		};

		test.setExpectedExceptionClass(expectedExceptionClass);
		test.setName(name);

		return test;
	}

	@SuppressWarnings("deprecation")
	private TestCase createUntypedUpdateWithCallbackTest(String name, final JsonObject itemToInsert, final JsonObject itemToUpdate, final boolean setUpdatedId,
			final Class<?> expectedExceptionClass) {

		final TestCase test = new TestCase() {

			@Override
			protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {

				final MobileServiceJsonTable table = client.getTable(ROUNDTRIP_TABLE_NAME);
				final TestCase testCase = this;

				log("insert item");
				table.insert(itemToInsert, new TableJsonOperationCallback() {

					@Override
					public void onCompleted(final JsonObject insertedItem, Exception exception, ServiceFilterResponse response) {
						final TestResult result = new TestResult();
						result.setTestCase(testCase);
						result.setStatus(TestStatus.Passed);

						if (exception == null) { // if it was ok

							if (setUpdatedId) {
								int id = insertedItem.get("id").getAsInt();
								log("update item id " + id);
								itemToUpdate.addProperty("id", id);
							}

							log("update the item");
							table.update(itemToUpdate, new TableJsonOperationCallback() {

								@Override
								public void onCompleted(final JsonObject updatedItem, Exception exception, ServiceFilterResponse response) {

									if (exception == null) { // if it was ok

										log("lookup the item");
										table.lookUp(updatedItem.get("id").getAsInt(), new TableJsonOperationCallback() {

											@Override
											public void onCompleted(JsonObject lookedUpItem, Exception exception, ServiceFilterResponse response) {
												if (exception == null) { // if
																			// it
																			// was
																			// ok
													log("verify items are equal");
													if (Util.compareJson(updatedItem, lookedUpItem)) { // check
																										// the
																										// items
																										// are
																										// equal
														log("cleanup");
														table.delete(lookedUpItem.get("id").getAsInt(), new TableDeleteCallback() { // clean
																																	// up

																	@Override
																	public void onCompleted(Exception exception, ServiceFilterResponse response) {
																		if (exception != null) {
																			createResultFromException(result, exception);
																		}

																		// callback
																		// with
																		// success
																		// or
																		// error
																		// on
																		// cleanup
																		if (callback != null)
																			callback.onTestComplete(testCase, result);
																	}
																});
													} else {
														createResultFromException(result, new ExpectedValueException(insertedItem, lookedUpItem));
														if (callback != null)
															callback.onTestComplete(testCase, result);
													}
												} else {
													createResultFromException(result, exception);
													if (callback != null)
														callback.onTestComplete(testCase, result);
												}
											}

										});
									} else {
										createResultFromException(result, exception);
										if (callback != null)
											callback.onTestComplete(testCase, result);
									}
								}

							});
						} else {
							createResultFromException(result, exception);
							if (callback != null)
								callback.onTestComplete(testCase, result);
						}
					}

				});

			}
		};

		test.setExpectedExceptionClass(expectedExceptionClass);
		test.setName(name);

		return test;
	}

	@SuppressWarnings("deprecation")
	private TestCase createDeleteWithCallbackTest(String name, final boolean typed, final boolean useFakeId, final boolean includeId,
			Class<?> expectedExceptionClass) {
		TestCase testCase = new TestCase() {

			@Override
			protected void executeTest(final MobileServiceClient client, final TestExecutionCallback callback) {
				RoundTripTableElement element = new RoundTripTableElement(new Random());
				final MobileServiceTable<RoundTripTableElement> table = client.getTable(ROUNDTRIP_TABLE_NAME, RoundTripTableElement.class);

				final TestCase testCase = this;
				log("insert item");
				table.insert(element, new TableOperationCallback<RoundTripTableElement>() {

					@Override
					public void onCompleted(RoundTripTableElement entity, Exception exception, ServiceFilterResponse response) {
						final TestResult result = new TestResult();
						result.setStatus(TestStatus.Passed);
						result.setTestCase(testCase);

						if (exception == null) {
							Object deleteObject;

							if (useFakeId) {
								log("use fake id");
								entity.id = 1000000000;
							}

							if (!includeId) {
								log("include id");
								entity.id = null;
							}

							if (typed) {
								deleteObject = entity;
							} else {
								deleteObject = client.getGsonBuilder().create().toJsonTree(entity).getAsJsonObject();
							}

							log("delete");
							table.delete(deleteObject, new TableDeleteCallback() {

								@Override
								public void onCompleted(Exception exception, ServiceFilterResponse response) {
									if (exception != null) {
										createResultFromException(result, exception);
									}

									if (callback != null)
										callback.onTestComplete(testCase, result);
								}
							});

						} else {
							createResultFromException(result, exception);
							if (callback != null)
								callback.onTestComplete(testCase, result);
						}
					}

				});
			}
		};

		testCase.setName(name);
		testCase.setExpectedExceptionClass(expectedExceptionClass);

		return testCase;
	}
}
