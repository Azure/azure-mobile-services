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
package com.microsoft.windowsazure.mobileservices.table;

import java.io.UnsupportedEncodingException;
import java.net.URLEncoder;
import java.util.List;

import org.apache.http.Header;
import org.apache.http.client.methods.HttpGet;
import org.apache.http.client.methods.HttpPost;
import org.apache.http.protocol.HTTP;

import android.net.Uri;
import android.util.Pair;

import com.google.common.util.concurrent.FutureCallback;
import com.google.common.util.concurrent.Futures;
import com.google.common.util.concurrent.ListenableFuture;
import com.google.common.util.concurrent.SettableFuture;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceException;
import com.microsoft.windowsazure.mobileservices.http.HttpPatch;
import com.microsoft.windowsazure.mobileservices.http.MobileServiceConnection;
import com.microsoft.windowsazure.mobileservices.http.RequestAsyncTask;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterRequest;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterRequestImpl;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.table.query.ExecutableJsonQuery;
import com.microsoft.windowsazure.mobileservices.table.query.Query;
import com.microsoft.windowsazure.mobileservices.table.query.QueryODataWriter;
import com.microsoft.windowsazure.mobileservices.table.query.QueryOrder;

/**
 * Represents a Mobile Service Table
 */
public final class MobileServiceJsonTable extends MobileServiceTableBase {

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
	 * Executes a query to retrieve all the table rows
	 * 
	 * @throws MobileServiceException
	 */
	public ListenableFuture<JsonElement> execute() throws MobileServiceException {
		return this.where().execute();
	}

	/**
	 * Executes the query
	 * 
	 * @deprecated use {@link execute()} instead
	 * 
	 * @param callback
	 *            Callback to invoke when the operation is completed
	 * @throws MobileServiceException
	 */
	public void execute(final TableJsonQueryCallback callback) throws MobileServiceException {
		this.where().execute(callback);
	}

	/**
	 * Retrieves a set of rows from the table using a query
	 * 
	 * @param query
	 *            The query used to retrieve the rows
	 * @throws MobileServiceException
	 */
	public ListenableFuture<JsonElement> execute(final Query query) {
		final SettableFuture<JsonElement> future = SettableFuture.create();

		String url = null;
		try {
			String filtersUrl = URLEncoder.encode(QueryODataWriter.getRowFilter(query).trim(), MobileServiceClient.UTF8_ENCODING);
			url = mClient.getAppUrl().toString() + TABLES_URL + URLEncoder.encode(mTableName, MobileServiceClient.UTF8_ENCODING);

			if (filtersUrl.length() > 0) {
				url += "?$filter=" + filtersUrl + QueryODataWriter.getRowSetModifiers(query, this);
			} else {
				String rowSetModifiers = QueryODataWriter.getRowSetModifiers(query, this);

				if (rowSetModifiers.length() > 0) {
					url += "?" + QueryODataWriter.getRowSetModifiers(query, this).substring(1);
				}
			}

		} catch (UnsupportedEncodingException e) {
			/*
			 * if (callback != null) {
			 * 
			 * callback.onCompleted(null, 0, e, null); }
			 */
			future.setException(e);
			return future;
		}

		ListenableFuture<Pair<JsonElement, ServiceFilterResponse>> internalFuture = executeGetRecords(url);

		Futures.addCallback(internalFuture, new FutureCallback<Pair<JsonElement, ServiceFilterResponse>>() {
			@Override
			public void onFailure(Throwable exc) {
				future.setException(exc);
			}

			@Override
			public void onSuccess(Pair<JsonElement, ServiceFilterResponse> result) {
				future.set(result.first);
			}
		});

		return future;
	}

	/**
	 * Retrieves a set of rows from the table using a query
	 * 
	 * @deprecated use {@link execute(final Query query)} instead
	 * 
	 * @param query
	 *            The query used to retrieve the rows
	 * @param callback
	 *            Callback to invoke when the operation is completed
	 */
	public void execute(final Query query, final TableJsonQueryCallback callback) {

		ListenableFuture<JsonElement> executeFuture = execute(query);

		Futures.addCallback(executeFuture, new FutureCallback<JsonElement>() {
			@Override
			public void onFailure(Throwable exception) {
				if (exception instanceof Exception) {
					callback.onCompleted(null, 0, (Exception) exception, MobileServiceException.getServiceResponse(exception));
				}
			}

			@Override
			public void onSuccess(JsonElement result) {
				callback.onCompleted(result, 1, null, null);
			}
		});
	}

	/**
	 * Starts a filter to query the table
	 * 
	 * @return The ExecutableJsonQuery representing the filter
	 */
	public ExecutableJsonQuery where() {
		ExecutableJsonQuery query = new ExecutableJsonQuery();
		query.setTable(this);
		return query;
	}

	/**
	 * Starts a filter to query the table with an existing filter
	 * 
	 * @param query
	 *            The existing filter
	 * @return The ExecutableJsonQuery representing the filter
	 */
	public ExecutableJsonQuery where(Query query) {
		if (query == null) {
			throw new IllegalArgumentException("Query must not be null");
		}

		ExecutableJsonQuery baseQuery = new ExecutableJsonQuery(query);
		baseQuery.setTable(this);
		return baseQuery;
	}

	/**
	 * Adds a new user-defined parameter to the query
	 * 
	 * @param parameter
	 *            The parameter name
	 * @param value
	 *            The parameter value
	 * @return ExecutableJsonQuery
	 */
	public ExecutableJsonQuery parameter(String parameter, String value) {
		return this.where().parameter(parameter, value);
	}

	/**
	 * Creates a query with the specified order
	 * 
	 * @param field
	 *            Field name
	 * @param order
	 *            Sorting order
	 * @return ExecutableJsonQuery
	 */
	public ExecutableJsonQuery orderBy(String field, QueryOrder order) {
		return this.where().orderBy(field, order);
	}

	/**
	 * Sets the number of records to return
	 * 
	 * @param top
	 *            Number of records to return
	 * @return ExecutableQuery
	 */
	public ExecutableJsonQuery top(int top) {
		return this.where().top(top);
	}

	/**
	 * Sets the number of records to skip over a given number of elements in a
	 * sequence and then return the remainder.
	 * 
	 * @param skip
	 * @return ExecutableJsonQuery
	 */
	public ExecutableJsonQuery skip(int skip) {
		return this.where().skip(skip);
	}

	/**
	 * Specifies the fields to retrieve
	 * 
	 * @param fields
	 *            Names of the fields to retrieve
	 * @return ExecutableJsonQuery
	 */
	public ExecutableJsonQuery select(String... fields) {
		return this.where().select(fields);
	}

	/**
	 * Include a property with the number of records returned.
	 * 
	 * @return ExecutableJsonQuery
	 */
	public ExecutableJsonQuery includeInlineCount() {
		return this.where().includeInlineCount();
	}

	/**
	 * Looks up a row in the table and retrieves its JSON value.
	 * 
	 * @param id
	 *            The id of the row
	 */
	public ListenableFuture<JsonObject> lookUp(Object id) {
		return this.lookUp(id, (List<Pair<String, String>>) null);
	}

	/**
	 * Looks up a row in the table and retrieves its JSON value.
	 * 
	 * @deprecated use {@link lookUp(Object id)} instead
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
	 *            A list of user-defined parameters and values to include in the
	 *            request URI query string
	 */
	public ListenableFuture<JsonObject> lookUp(Object id, List<Pair<String, String>> parameters) {
		final SettableFuture<JsonObject> future = SettableFuture.create();

		try {
			validateId(id);
		} catch (Exception e) {
			future.setException(e);
			return future;
		}

		String url;

		Uri.Builder uriBuilder = Uri.parse(mClient.getAppUrl().toString()).buildUpon();
		uriBuilder.path(TABLES_URL);
		uriBuilder.appendPath(mTableName);
		uriBuilder.appendPath(id.toString());

		parameters = addSystemProperties(mSystemProperties, parameters);

		if (parameters != null && parameters.size() > 0) {
			for (Pair<String, String> parameter : parameters) {
				uriBuilder.appendQueryParameter(parameter.first, parameter.second);
			}
		}

		url = uriBuilder.build().toString();

		ListenableFuture<Pair<JsonElement, ServiceFilterResponse>> internalFuture = executeGetRecords(url);

		Futures.addCallback(internalFuture, new FutureCallback<Pair<JsonElement, ServiceFilterResponse>>() {
			@Override
			public void onFailure(Throwable exc) {
				future.setException(exc);
			}

			@Override
			public void onSuccess(Pair<JsonElement, ServiceFilterResponse> results) {
				if (results.first.isJsonArray()) { // empty result
					// callback.onCompleted(null, new
					// MobileServiceException("A record with the specified Id cannot be found"),
					// response);
					future.setException(new MobileServiceException("A record with the specified Id cannot be found", results.second));
				} else { // Lookup result
					JsonObject patchedJson = results.first.getAsJsonObject();

					updateVersionFromETag(results.second, patchedJson);

					future.set(patchedJson);
					// callback.onCompleted(patchedJson, exception,
					// response);
				}
			}
		});

		return future;
	}

	/**
	 * Looks up a row in the table and retrieves its JSON value.
	 * 
	 * @deprecated use {@link lookUp(Object id, List<Pair<String, String>>
	 *             parameters)} instead
	 * 
	 * @param id
	 *            The id of the row
	 * @param parameters
	 *            A list of user-defined parameters and values to include in the
	 *            request URI query string
	 * @param callback
	 *            Callback to invoke after the operation is completed
	 */
	public void lookUp(Object id, List<Pair<String, String>> parameters, final TableJsonOperationCallback callback) {

		ListenableFuture<JsonObject> lookUpFuture = lookUp(id, parameters);

		Futures.addCallback(lookUpFuture, new FutureCallback<JsonObject>() {
			@Override
			public void onFailure(Throwable exception) {
				if (exception instanceof Exception) {
					callback.onCompleted(null, (Exception) exception, MobileServiceException.getServiceResponse(exception));
				}
			}

			@Override
			public void onSuccess(JsonObject result) {
				callback.onCompleted(result, null, null);
			}
		});
	}

	/**
	 * Inserts a JsonObject into a Mobile Service table
	 * 
	 * @param element
	 *            The JsonObject to insert
	 * @throws IllegalArgumentException
	 *             if the element has an id property set with a numeric value
	 *             other than default (0), or an invalid string value
	 */
	public ListenableFuture<JsonObject> insert(final JsonObject element) {
		return this.insert(element, (List<Pair<String, String>>) null);
	}

	/**
	 * Inserts a JsonObject into a Mobile Service table
	 * 
	 * @deprecated use {@link insert(final JsonObject element)} instead
	 * 
	 * @param element
	 *            The JsonObject to insert
	 * @param callback
	 *            Callback to invoke when the operation is completed
	 * @throws IllegalArgumentException
	 *             if the element has an id property set with a numeric value
	 *             other than default (0), or an invalid string value
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
	 *            A list of user-defined parameters and values to include in the
	 *            request URI query string
	 * @throws IllegalArgumentException
	 *             if the element has an id property set with a numeric value
	 *             other than default (0), or an invalid string value
	 */
	public ListenableFuture<JsonObject> insert(final JsonObject element, List<Pair<String, String>> parameters) {
		final SettableFuture<JsonObject> future = SettableFuture.create();
		try {
			validateIdOnInsert(element);
		} catch (Exception e) {
			future.setException(e);
			return future;
		}

		String content = element.toString();

		ServiceFilterRequest post;

		Uri.Builder uriBuilder = Uri.parse(mClient.getAppUrl().toString()).buildUpon();
		uriBuilder.path(TABLES_URL);
		uriBuilder.appendPath(mTableName);

		parameters = addSystemProperties(mSystemProperties, parameters);

		if (parameters != null && parameters.size() > 0) {
			for (Pair<String, String> parameter : parameters) {
				uriBuilder.appendQueryParameter(parameter.first, parameter.second);
			}
		}
		post = new ServiceFilterRequestImpl(new HttpPost(uriBuilder.build().toString()), mClient.getAndroidHttpClientFactory());
		post.addHeader(HTTP.CONTENT_TYPE, MobileServiceConnection.JSON_CONTENTTYPE);

		try {
			post.setContent(content);
		} catch (Exception e) {
			/*
			 * if (callback != null) { callback.onCompleted(null, e, null); }
			 */
			future.setException(e);
			return future;
		}

		ListenableFuture<Pair<JsonObject, ServiceFilterResponse>> internalFuture = executeTableOperation(post);

		Futures.addCallback(internalFuture, new FutureCallback<Pair<JsonObject, ServiceFilterResponse>>() {
			@Override
			public void onFailure(Throwable exc) {
				future.setException(exc);
			}

			@Override
			public void onSuccess(Pair<JsonObject, ServiceFilterResponse> result) {
				JsonObject patchedJson = patchOriginalEntityWithResponseEntity(element, result.first);

				updateVersionFromETag(result.second, patchedJson);

				future.set(patchedJson);
				// callback.onCompleted(patchedJson, exception,
				// response);
			}
		});

		return future;
	}

	/**
	 * Inserts a JsonObject into a Mobile Service Table
	 * 
	 * @deprecated use {@link insert(final JsonObject element, List<Pair<String,
	 *             String>> parameters)} instead
	 * 
	 * @param element
	 *            The JsonObject to insert
	 * @param parameters
	 *            A list of user-defined parameters and values to include in the
	 *            request URI query string
	 * @param callback
	 *            Callback to invoke when the operation is completed
	 * @throws IllegalArgumentException
	 *             if the element has an id property set with a numeric value
	 *             other than default (0), or an invalid string value
	 */
	public void insert(final JsonObject element, List<Pair<String, String>> parameters, final TableJsonOperationCallback callback) {

		ListenableFuture<JsonObject> insertFuture = insert(element, parameters);

		Futures.addCallback(insertFuture, new FutureCallback<JsonObject>() {
			@Override
			public void onFailure(Throwable exception) {
				if (exception instanceof Exception) {
					callback.onCompleted(null, (Exception) exception, MobileServiceException.getServiceResponse(exception));
				}
			}

			@Override
			public void onSuccess(JsonObject result) {
				callback.onCompleted(result, null, null);
			}
		});
	}

	/**
	 * Updates an element from a Mobile Service Table
	 * 
	 * @param element
	 *            The JsonObject to update
	 */
	public ListenableFuture<JsonObject> update(final JsonObject element) {
		return this.update(element, (List<Pair<String, String>>) null);
	}

	/**
	 * Updates an element from a Mobile Service Table
	 * 
	 * @deprecated use {@link update(final JsonObject element)} instead
	 * 
	 * @param element
	 *            The JsonObject to update
	 * @param callback
	 *            Callback to invoke when the operation is completed
	 */
	public void update(final JsonObject element, final TableJsonOperationCallback callback) {
		this.update(element, null, callback);
	}

	/**
	 * Updates an element from a Mobile Service Table
	 * 
	 * @param element
	 *            The JsonObject to update
	 * @param parameters
	 *            A list of user-defined parameters and values to include in the
	 *            request URI query string
	 */
	public ListenableFuture<JsonObject> update(final JsonObject element, List<Pair<String, String>> parameters) {
		final SettableFuture<JsonObject> future = SettableFuture.create();

		Object id = null;
		String version = null;
		String content = null;

		try {
			id = validateId(element);
		} catch (Exception e) {
			future.setException(e);
			return future;
		}

		if (!isNumericType(id)) {
			version = getVersionSystemProperty(element);
			content = removeSystemProperties(element).toString();
		} else {
			content = element.toString();
		}

		ServiceFilterRequest patch;

		Uri.Builder uriBuilder = Uri.parse(mClient.getAppUrl().toString()).buildUpon();
		uriBuilder.path(TABLES_URL);
		uriBuilder.appendPath(mTableName);
		uriBuilder.appendPath(id.toString());

		parameters = addSystemProperties(mSystemProperties, parameters);

		if (parameters != null && parameters.size() > 0) {
			for (Pair<String, String> parameter : parameters) {
				uriBuilder.appendQueryParameter(parameter.first, parameter.second);
			}
		}

		patch = new ServiceFilterRequestImpl(new HttpPatch(uriBuilder.build().toString()), mClient.getAndroidHttpClientFactory());
		patch.addHeader(HTTP.CONTENT_TYPE, MobileServiceConnection.JSON_CONTENTTYPE);

		if (version != null) {
			patch.addHeader("If-Match", getEtagFromValue(version));
		}

		try {
			patch.setContent(content);
		} catch (Exception e) {
			future.setException(e);
			return future;
		}

		ListenableFuture<Pair<JsonObject, ServiceFilterResponse>> internalFuture = executeTableOperation(patch);

		Futures.addCallback(internalFuture, new FutureCallback<Pair<JsonObject, ServiceFilterResponse>>() {
			@Override
			public void onFailure(Throwable exc) {
				if (exc instanceof MobileServiceException) {
					MobileServiceException msExcep = (MobileServiceException) exc;

					if (msExcep.getResponse().getStatus() != null && msExcep.getResponse().getStatus().getStatusCode() == 412) {
						String content = msExcep.getResponse().getContent();

						JsonObject serverEntity = null;

						if (content != null) {
							serverEntity = new JsonParser().parse(content).getAsJsonObject();
						}

						future.setException(new MobileServicePreconditionFailedExceptionBase(msExcep, serverEntity));
					} else {
						future.setException(exc);
						// callback.onCompleted(jsonEntity, exception,
						// response);
					}
				} else {
					future.setException(exc);
				}

			}

			@Override
			public void onSuccess(Pair<JsonObject, ServiceFilterResponse> result) {
				JsonObject patchedJson = patchOriginalEntityWithResponseEntity(element, result.first);

				updateVersionFromETag(result.second, patchedJson);

				future.set(patchedJson);
				// callback.onCompleted(patchedJson, exception,
				// response);
			}
		});

		return future;
	}

	/**
	 * Updates an element from a Mobile Service Table
	 * 
	 * @deprecated use {@link update(final JsonObject element, List<Pair<String,
	 *             String>> parameters)} instead
	 * 
	 * @param element
	 *            The JsonObject to update
	 * @param parameters
	 *            A list of user-defined parameters and values to include in the
	 *            request URI query string
	 * @param callback
	 *            Callback to invoke when the operation is completed
	 */
	public void update(final JsonObject element, List<Pair<String, String>> parameters, final TableJsonOperationCallback callback) {
		ListenableFuture<JsonObject> updateFuture = update(element, parameters);

		Futures.addCallback(updateFuture, new FutureCallback<JsonObject>() {
			@Override
			public void onFailure(Throwable exception) {
				if (exception instanceof Exception) {
					callback.onCompleted(null, (Exception) exception, MobileServiceException.getServiceResponse(exception));
				}
			}

			@Override
			public void onSuccess(JsonObject result) {
				callback.onCompleted(result, null, null);
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
	private ListenableFuture<Pair<JsonObject, ServiceFilterResponse>> executeTableOperation(ServiceFilterRequest request) {
		final SettableFuture<Pair<JsonObject, ServiceFilterResponse>> future = SettableFuture.create();

		// Create AsyncTask to execute the operation
		new RequestAsyncTask(request, mClient.createConnection()) {
			@Override
			protected void onPostExecute(ServiceFilterResponse result) {
				JsonObject newEntityJson = null;
				if (mTaskException == null && result != null) {
					String content = null;
					content = result.getContent();

					newEntityJson = new JsonParser().parse(content).getAsJsonObject();

					// callback.onCompleted(newEntityJson, null, result);
					future.set(Pair.create(newEntityJson, result));
				} else {
					future.setException(mTaskException);
					// callback.onCompleted(null, mTaskException, result);
				}

			}
		}.executeTask();

		return future;
	}

	/**
	 * Retrieves a set of rows from using the specified URL
	 * 
	 * @param query
	 *            The URL used to retrieve the rows
	 * @param callback
	 *            Callback to invoke when the operation is completed
	 */
	private ListenableFuture<Pair<JsonElement, ServiceFilterResponse>> executeGetRecords(final String url) {
		final SettableFuture<Pair<JsonElement, ServiceFilterResponse>> future = SettableFuture.create();

		ServiceFilterRequest request = new ServiceFilterRequestImpl(new HttpGet(url), mClient.getAndroidHttpClientFactory());

		MobileServiceConnection conn = mClient.createConnection();
		// Create AsyncTask to execute the request and parse the results
		new RequestAsyncTask(request, conn) {
			@Override
			protected void onPostExecute(ServiceFilterResponse response) {
				if (mTaskException == null && response != null) {
					JsonElement results = null;

					try {
						// Parse the results using the given Entity class
						String content = response.getContent();
						JsonElement json = new JsonParser().parse(content);

						results = json;

						future.set(Pair.create(results, response));
					} catch (Exception e) {
						future.setException(new MobileServiceException("Error while retrieving data from response.", e, response));
						// callback.onCompleted(null, 0, new
						// MobileServiceException("Error while retrieving data from response.",
						// e), response);
						// return;
					}

					// callback.onCompleted(results, count, null, response);

				} else {
					// callback.onCompleted(null, 0, mTaskException, response);
					future.setException(mTaskException);
				}

			}
		}.executeTask();

		return future;
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

				if (isStringType(idElement)) {
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

	/**
	 * Updates the Version System Property in the Json Object with the ETag
	 * information
	 * 
	 * @param response
	 *            The response containing the ETag Header
	 * 
	 * @param json
	 *            The JsonObject to modify
	 */
	private static void updateVersionFromETag(ServiceFilterResponse response, JsonObject json) {
		if (response != null && response.getHeaders() != null) {
			for (Header header : response.getHeaders()) {
				if (header.getName().equalsIgnoreCase("ETag")) {
					json.remove(VersionSystemPropertyName);
					json.addProperty(VersionSystemPropertyName, getValueFromEtag(header.getValue()));
					break;
				}
			}
		}
	}
}