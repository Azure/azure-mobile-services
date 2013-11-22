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
import java.util.List;

import org.apache.http.client.methods.HttpGet;
import org.apache.http.client.methods.HttpPost;
import org.apache.http.protocol.HTTP;

import android.net.Uri;
import android.util.Pair;

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
		this.lookUp(id, null, callback);
	}

	/**
	 * Looks up a row in the table and retrieves its JSON value.
	 * 
	 * @param id
	 *            The id of the row
	 * @param parameters           
	 *            A list of user-defined parameters and values to include in the request URI query string
	 * @param callback
	 *            Callback to invoke after the operation is completed
	 */
	public void lookUp(Object id, List<Pair<String, String>> parameters, final TableJsonOperationCallback callback) {
		// Create request URL
		try {	
			validateId(id);
		} catch (Exception e) {
			if (callback != null) {
				callback.onCompleted(null, e, null);
			}
				
			return;
		}
		
		String url;
		try {
			Uri.Builder uriBuilder = Uri.parse(mClient.getAppUrl().toString()).buildUpon();
			uriBuilder.path(TABLES_URL);
			uriBuilder.appendPath(URLEncoder.encode(mTableName, MobileServiceClient.UTF8_ENCODING));
			uriBuilder.appendPath(URLEncoder.encode(id.toString(), MobileServiceClient.UTF8_ENCODING));
			
			if (parameters != null && parameters.size() > 0) {
				for (Pair<String, String> parameter : parameters) {
					uriBuilder.appendQueryParameter(parameter.first, parameter.second);
				}
			}
			url = uriBuilder.build().toString();
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
					if (exception == null && results != null) {
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
	 * Inserts a JsonObject into a Mobile Service table
	 * 
	 * @param element
	 *            The JsonObject to insert
	 * @param callback
	 *            Callback to invoke when the operation is completed
	 * @throws InvalidParameterException   
	 */
	public void insert(final JsonObject element, TableJsonOperationCallback callback) {
		this.insert(element, null, callback);
	}
	
	/**
	 * Inserts a JsonObject into a Mobile Service Table
	 * 
	 * @param element
	 *            The JsonObject to insert
	 * @param parameters
	 * 			  A list of user-defined parameters and values to include in the request URI query string
	 * @param callback
	 *            Callback to invoke when the operation is completed
	 * @throws InvalidParameterException
	 */
	public void insert(final JsonObject element, List<Pair<String, String>> parameters,
			final TableJsonOperationCallback callback) {

		try {
			validateIdOnInsert(element);
		} catch (Exception e) {
			if (callback != null) {
				callback.onCompleted(null, e, null);
			}
			return;
		}

		String content = element.toString();

		ServiceFilterRequest post;
		try {
			Uri.Builder uriBuilder = Uri.parse(mClient.getAppUrl().toString()).buildUpon();
			uriBuilder.path(TABLES_URL);
			uriBuilder.appendPath(URLEncoder.encode(mTableName, MobileServiceClient.UTF8_ENCODING));

			if (parameters != null && parameters.size() > 0) {
				for (Pair<String, String> parameter : parameters) {
					uriBuilder.appendQueryParameter(parameter.first, parameter.second);
				}
			}
			post = new ServiceFilterRequestImpl(new HttpPost(uriBuilder.build().toString()), mClient.getAndroidHttpClientFactory());
			post.addHeader(HTTP.CONTENT_TYPE, MobileServiceConnection.JSON_CONTENTTYPE);
			
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
					if (exception == null && jsonEntity != null) {
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
		this.update(element, null, callback);
	}

	/**
	 * Updates an element from a Mobile Service Table
	 * 
	 * @param element
	 *            The JsonObject to update
	 * @param parameters
	 * 			  A list of user-defined parameters and values to include in the request URI query string
	 * @param callback
	 *            Callback to invoke when the operation is completed
	 */
	public void update(final JsonObject element, final List<Pair<String, String>> parameters, final TableJsonOperationCallback callback) {
		try {			
			validateId(element);
		} catch (Exception e) {
			if (callback != null) {
				callback.onCompleted(null, e, null);
			}
			
			return;
		}

		String content = element.toString();

		ServiceFilterRequest patch;
		
		try {
			Uri.Builder uriBuilder = Uri.parse(mClient.getAppUrl().toString()).buildUpon();
			uriBuilder.path(TABLES_URL);
			uriBuilder.appendPath(URLEncoder.encode(mTableName, MobileServiceClient.UTF8_ENCODING));
			uriBuilder.appendPath(getObjectId(element).toString());

			if (parameters != null && parameters.size() > 0) {
				for (Pair<String, String> parameter : parameters) {
					uriBuilder.appendQueryParameter(parameter.first, parameter.second);
				}
			}
			patch = new ServiceFilterRequestImpl(new HttpPatch(uriBuilder.build().toString()), mClient.getAndroidHttpClientFactory());
			patch.addHeader(HTTP.CONTENT_TYPE, MobileServiceConnection.JSON_CONTENTTYPE);	
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
					if (exception == null && jsonEntity != null) {
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
				new HttpGet(url), mClient.getAndroidHttpClientFactory());

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
	 * Validates the Id property from a JsonObject on an Insert Action
	 * 
	 * @param json
	 *            The JsonObject to modify
	 */
	private void validateIdOnInsert(final JsonObject json) {
		// Remove id property if exists
		String[] idPropertyNames = new String[] { "id", "Id", "iD", "ID" };
		
		for (int i = 0; i < 4; i++) {
			String idProperty = idPropertyNames[i];
			
			if (json.has(idProperty)) {
				JsonElement idElement = json.get(idProperty);
				
				if(isStringType(idElement)) {
					String id = getStringValue(idElement);
					
					if (!isValidStringId(id)) {
						throw new IllegalArgumentException("The entity to insert has an invalid string value on " + idProperty + " property.");
					}
				} else if (isNumericType(idElement)) {
					long id = getNumericValue(idElement);
					
					if (!isDefaultNumericId(id)) {
						throw new IllegalArgumentException("The entity to insert should not have a numeric " + idProperty + " property defined.");
					}
					
					json.remove(idProperty);
				} else if (idElement.isJsonNull()) {
					json.remove(idProperty);
				} else {
					throw new IllegalArgumentException("The entity to insert should not have an " + idProperty + " defined with an invalid value");
				}
			}
		}
	}
}
