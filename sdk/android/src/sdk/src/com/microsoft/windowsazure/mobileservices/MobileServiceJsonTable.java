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
package com.microsoft.windowsazure.mobileservices;

import java.io.UnsupportedEncodingException;
import java.net.URLEncoder;
import java.security.InvalidParameterException;
import java.util.Map;

import org.apache.http.client.methods.HttpGet;
import org.apache.http.client.methods.HttpPatch;
import org.apache.http.client.methods.HttpPost;

import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;

/**
 * Represents a Mobile Service Table
 */
public final class MobileServiceJsonTable extends
MobileServiceTableBase<TableJsonQueryCallback> {

	/**
	 * Constructor for MobileServiceJsonTable
	 * 
	 * @param name
	 *            The name of the represented table
	 * @param client
	 *            The MobileServiceClient used to invoke table operations
	 */
	public MobileServiceJsonTable(String name, MobileServiceClient client) {
		initialize(name, client);
	}

	/**
	 * Retrieves a set of rows from the table using a query
	 * 
	 * @param query
	 *            The query used to retrieve the rows
	 * @param callback
	 *            Callback to invoke when the operation is completed
	 */
	public void execute(final MobileServiceQuery<?> query,
			final TableJsonQueryCallback callback) {
		String url = null;
		try {
			String filtersUrl = URLEncoder.encode(query.toString().trim(),
					MobileServiceClient.UTF8_ENCODING);
			url = mClient.getAppUrl().toString()
					+ TABLES_URL
					+ URLEncoder.encode(mTableName,
							MobileServiceClient.UTF8_ENCODING);

			if (filtersUrl.length() > 0) {
				url += "?$filter=" + filtersUrl + query.getRowSetModifiers();
			} else {
				String rowSetModifiers = query.getRowSetModifiers();
				if (rowSetModifiers.length() > 0) {
					url += "?" + query.getRowSetModifiers().substring(1);
				}
			}

		} catch (UnsupportedEncodingException e) {
			if (callback != null) {
				callback.onCompleted(null, 0, e, null);
			}
			return;
		}

		executeGetRecords(url, callback);
	}

	/**
	 * Looks up a row in the table and retrieves its JSON value.
	 * 
	 * @param id
	 *            The id of the row
	 * @param callback
	 *            Callback to invoke after the operation is completed
	 */
	public void lookUp(Object id, final TableJsonOperationCallback callback) {
		// Create request URL
		String url = null;
		try {
			url = mClient.getAppUrl().toString()
					+ TABLES_URL
					+ URLEncoder.encode(mTableName,
							MobileServiceClient.UTF8_ENCODING)
							+ "/"
							+ URLEncoder.encode(id.toString(),
									MobileServiceClient.UTF8_ENCODING);
		} catch (UnsupportedEncodingException e) {
			if (callback != null) {
				callback.onCompleted(null, e, null);
			}
			return;
		}

		executeGetRecords(url, new TableJsonQueryCallback() {

			@Override
			public void onCompleted(JsonElement results, int count,
					Exception exception, ServiceFilterResponse response) {
				if (callback != null) {
					if (exception == null) {
						if (results.isJsonArray()) { // empty result
							callback.onCompleted(
									null,
									new MobileServiceException(
											"A record with the specified Id cannot be found"),
											response);
						} else { // Lookup result
							callback.onCompleted(results.getAsJsonObject(),
									exception, response);
						}
					} else {
						callback.onCompleted(null, exception, response);
					}
				}
			}
		});
	}

	/**
	 * Removes the Id property from a JsonObject
	 * 
	 * @param json
	 *            The JsonObject to modify
	 */
	private void removeIdFromJson(final JsonObject json) {
		// Remove id property if exists
		String[] idPropertyNames = new String[] { "id", "Id", "iD", "ID" };
		for (int i = 0; i < 4; i++) {
			String idProperty = idPropertyNames[i];
			if (json.has(idProperty)) {
				JsonElement idElement = json.get(idProperty);
				if(isValidTypeId(idElement) && idElement.getAsInt() != 0) {
					throw new InvalidParameterException(
							"The entity to insert should not have "
									+ idProperty + " property defined");
				}

				json.remove(idProperty);
			}
		}
	}

	/**
	 * Inserts a JsonObject into a Mobile Service Table
	 * 
	 * @param element
	 *            The JsonObject to insert
	 * @param callback
	 *            Callback to invoke when the operation is completed
	 * @throws InvalidParameterException
	 */
	public void insert(final JsonObject element,
			final TableJsonOperationCallback callback) {

		try {
			removeIdFromJson(element);
		} catch (InvalidParameterException e) {
			if (callback != null) {
				callback.onCompleted(null, e, null);
			}
			return;
		}

		String content = element.toString();

		ServiceFilterRequest post;
		try {
			post = new ServiceFilterRequestImpl(new HttpPost(mClient
					.getAppUrl().toString()
					+ TABLES_URL
					+ URLEncoder.encode(mTableName,
							MobileServiceClient.UTF8_ENCODING)));
		} catch (UnsupportedEncodingException e) {
			if (callback != null) {
				callback.onCompleted(null, e, null);
			}
			return;
		}

		try {
			post.setContent(content);
		} catch (Exception e) {
			if (callback != null) {
				callback.onCompleted(null, e, null);
			}
			return;
		}

		executeTableOperation(post, new TableJsonOperationCallback() {

			@Override
			public void onCompleted(JsonObject jsonEntity, Exception exception,
					ServiceFilterResponse response) {
				if (callback != null) {
					if (exception == null) {
						JsonObject patchedJson = patchOriginalEntityWithResponseEntity(
								element, jsonEntity);

						callback.onCompleted(patchedJson, exception, response);
					} else {
						callback.onCompleted(jsonEntity, exception, response);
					}
				}
			}
		});
	}

	/**
	 * Updates an element from a Mobile Service Table
	 * 
	 * @param element
	 *            The JsonObject to update
	 * @param callback
	 *            Callback to invoke when the operation is completed
	 */
	public void update(final JsonObject element,
			final TableJsonOperationCallback callback) {

		try {
			updateIdProperty(element);

			if (!element.has("id") || element.get("id").getAsInt() == 0) {
				throw new IllegalArgumentException("You must specify an id property with a valid value for updating an object.");
			}
		} catch (Exception e) {
			if (callback != null) {
				callback.onCompleted(null, e, null);
			}
			return;
		}

		String content = element.toString();

		ServiceFilterRequest patch;
		try {
			patch = new ServiceFilterRequestImpl(new HttpPatch(mClient
					.getAppUrl().toString()
					+ TABLES_URL
					+ URLEncoder.encode(mTableName,
							MobileServiceClient.UTF8_ENCODING)
							+ "/"
							+ Integer.valueOf(getObjectId(element)).toString()));
		} catch (UnsupportedEncodingException e) {
			if (callback != null) {
				callback.onCompleted(null, e, null);
			}
			return;
		}

		try {
			patch.setContent(content);
		} catch (Exception e) {
			if (callback != null) {
				callback.onCompleted(null, e, null);
			}
			return;
		}

		executeTableOperation(patch, new TableJsonOperationCallback() {

			@Override
			public void onCompleted(JsonObject jsonEntity, Exception exception,
					ServiceFilterResponse response) {
				if (callback != null) {
					if (exception == null) {
						JsonObject patchedJson = patchOriginalEntityWithResponseEntity(
								element, jsonEntity);
						callback.onCompleted(patchedJson, exception, response);
					} else {
						callback.onCompleted(jsonEntity, exception, response);
					}
				}
			}
		});
	}

	/**
	 * Executes the query against the table
	 * 
	 * @param request
	 *            Request to execute
	 * @param callback
	 *            Callback to invoke when the operation is completed
	 */
	private void executeTableOperation(ServiceFilterRequest request,
			final TableJsonOperationCallback callback) {
		// Create AsyncTask to execute the operation
		new RequestAsyncTask(request, mClient.createConnection()) {
			@Override
			protected void onPostExecute(ServiceFilterResponse result) {
				if (callback != null) {
					JsonObject newEntityJson = null;
					if (mTaskException == null && result != null) {
						String content = null;
						content = result.getContent();

						newEntityJson = new JsonParser().parse(content)
								.getAsJsonObject();

						callback.onCompleted(newEntityJson, null, result);

					} else {
						callback.onCompleted(null, mTaskException, result);
						;
					}
				}
			}
		}.execute();
	}

	/**
	 * Retrieves a set of rows from using the specified URL
	 * 
	 * @param query
	 *            The URL used to retrieve the rows
	 * @param callback
	 *            Callback to invoke when the operation is completed
	 */
	private void executeGetRecords(final String url,
			final TableJsonQueryCallback callback) {
		ServiceFilterRequest request = new ServiceFilterRequestImpl(
				new HttpGet(url));

		MobileServiceConnection conn = mClient.createConnection();
		// Create AsyncTask to execute the request and parse the results
		new RequestAsyncTask(request, conn) {
			@Override
			protected void onPostExecute(ServiceFilterResponse response) {
				if (callback != null) {
					if (mTaskException == null && response != null) {
						JsonElement results = null;

						int count = 0;

						try {
							// Parse the results using the given Entity class
							String content = response.getContent();
							JsonElement json = new JsonParser().parse(content);

							if (json.isJsonObject()) {
								JsonObject jsonObject = json.getAsJsonObject();
								// If the response has count property, store its
								// value
								if (jsonObject.has("results")
										&& jsonObject.has("count")) { // inlinecount
									// result
									count = jsonObject.get("count").getAsInt();
									results = jsonObject.get("results");
								} else {
									results = json;
								}
							} else {
								results = json;
							}
						} catch (Exception e) {
							callback.onCompleted(
									null,
									0,
									new MobileServiceException(
											"Error while retrieving data from response.",
											e), response);
							return;
						}

						callback.onCompleted(results, count, null, response);

					} else {
						callback.onCompleted(null, 0, mTaskException, response);
					}
				}
			}
		}.execute();
	}

	/**
	 * Updates the JsonObject to have an id property
	 * @param json
	 *            the element to evaluate
	 */
	private void updateIdProperty(final JsonObject json) throws IllegalArgumentException {
		for (Map.Entry<String,JsonElement> entry : json.entrySet()){
			String key = entry.getKey();
			if (key.equalsIgnoreCase("id")) {
				JsonElement element = entry.getValue();
				if (isValidTypeId(element)) {
					if (!key.equals("id")) {
						//force the id name to 'id', no matter the casing 
						json.remove(key);
						// Create a new id property using the given property name
						json.addProperty("id", entry.getValue().getAsNumber());
					}
					return;
				} else {
					throw new IllegalArgumentException("The id must be numeric");
				}
			}
		}
	}

	/**
	 * Validates if the id property is numeric.
	 * @param element
	 * @return
	 */
	private boolean isValidTypeId(JsonElement element) {
		return element.isJsonPrimitive() && element.getAsJsonPrimitive().isNumber();
	}
}
