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
import java.util.Map;

import org.apache.http.client.methods.HttpDelete;

import android.net.Uri;
import android.util.Pair;

import com.google.gson.JsonElement;
import com.google.gson.JsonNull;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;

abstract class MobileServiceTableBase<E> {

	/**
	 * Tables URI part
	 */
	public static final String TABLES_URL = "tables/";

	/**
	 * The MobileServiceClient used to invoke table operations
	 */
	protected MobileServiceClient mClient;

	/**
	 * The name of the represented table
	 */
	protected String mTableName;

	protected void initialize(String name, MobileServiceClient client) {
		if (name == null || name.toString().trim().length() == 0) {
			throw new IllegalArgumentException("Invalid Table Name");
		}

		if (client == null) {
			throw new IllegalArgumentException("Invalid Mobile Service Client");
		}

		mClient = client;
		mTableName = name;
	}
	
	public abstract void execute(MobileServiceQuery<?> query, E callback);
	
	/**
	 * Executes a query to retrieve all the table rows
	 * 
	 * @param callback
	 *            Callback to invoke when the operation is completed
	 */
	public void execute(E callback) {
		this.where().execute(callback);
	}

	/**
	 * Returns the name of the represented table
	 */
	public String getTableName() {
		return mTableName;
	}

	/**
	 * Returns the client used for table operations
	 */
	protected MobileServiceClient getClient() {
		return mClient;
	}

	/**
	 * Adds a new user-defined parameter to the query
	 * 
	 * @param parameter
	 *            The parameter name
	 * @param value
	 *            The parameter value
	 * @return MobileServiceQuery
	 */
	public MobileServiceQuery<E> parameter(String parameter, String value) {
		return this.where().parameter(parameter, value);
	}

	/**
	 * Creates a query with the specified order
	 * 
	 * @param field
	 *            Field name
	 * @param order
	 *            Sorting order
	 * @return MobileServiceQuery
	 */
	public MobileServiceQuery<E> orderBy(String field, QueryOrder order) {
		return this.where().orderBy(field, order);
	}

	/**
	 * Sets the number of records to return
	 * 
	 * @param top
	 *            Number of records to return
	 * @return MobileServiceQuery
	 */
	public MobileServiceQuery<E> top(int top) {
		return this.where().top(top);
	}

	/**
	 * Sets the number of records to skip over a given number of elements in a
	 * sequence and then return the remainder.
	 * 
	 * @param skip
	 * @return MobileServiceQuery
	 */
	public MobileServiceQuery<E> skip(int skip) {
		return this.where().skip(skip);
	}

	/**
	 * Specifies the fields to retrieve
	 * 
	 * @param fields
	 *            Names of the fields to retrieve
	 * @return MobileServiceQuery
	 */
	public MobileServiceQuery<E> select(String... fields) {
		return this.where().select(fields);
	}

	/**
	 * Include a property with the number of records returned.
	 * 
	 * @return MobileServiceQuery
	 */
	public MobileServiceQuery<E> includeInlineCount() {
		return this.where().includeInlineCount();
	}

	/**
	 * Starts a filter to query the table
	 * 
	 * @return The MobileServiceQuery<E> representing the filter
	 */
	public MobileServiceQuery<E> where() {
		MobileServiceQuery<E> query = new MobileServiceQuery<E>();
		query.setTable(this);
		return query;
	}

	/**
	 * Starts a filter to query the table with an existing filter
	 * 
	 * @param query
	 *            The existing filter
	 * @return The MobileServiceQuery<E> representing the filter
	 */
	public MobileServiceQuery<E> where(MobileServiceQuery<?> query) {
		if (query == null) {
			throw new IllegalArgumentException("Query must not be null");
		}

		MobileServiceQuery<E> baseQuery = new MobileServiceQuery<E>(query);
		baseQuery.setTable(this);
		return baseQuery;
	}

	/**
	 * Deletes an entity from a Mobile Service Table
	 * 
	 * @param element
	 *            The entity to delete
	 * @param callback
	 *            Callback to invoke when the operation is completed
	 */
	public void delete(Object element, TableDeleteCallback callback) {
		this.delete(element, null, callback);
	}
	
	/**
	 * Deletes an entity from a Mobile Service Table
	 * 
	 * @param element
	 *            The entity to delete
	 * @param parameters
	 * 			  A list of user-defined parameters and values to include in the request URI query string
	 * @param callback
	 *            Callback to invoke when the operation is completed
	 */
	public void delete(Object element, List<Pair<String, String>> parameters, TableDeleteCallback callback) {
		int id = -1;
		try {
			id = getObjectId(element);
			
			if (id == 0) {
				throw new IllegalArgumentException("You must specify an id property with a valid value for deleting an object.");
			}
		} catch (Exception e) {
			callback.onCompleted(e, null);
			return;
		}

		this.delete(id, parameters, callback);
	}

	/**
	 * Deletes an entity from a Mobile Service Table using a given id
	 * 
	 * @param id
	 *            The id of the entity to delete
	 * @param callback
	 *            Callback to invoke when the operation is completed
	 */
	public void delete(int id, final TableDeleteCallback callback) {
		this.delete(id, null, callback);
	}
	
	/**
	 * Deletes an entity from a Mobile Service Table using a given id
	 * 
	 * @param id
	 *            The id of the entity to delete
	 * @param parameters
	 * 			  A list of user-defined parameters and values to include in the request URI query string            
	 * @param callback
	 *            Callback to invoke when the operation is completed
	 */
	public void delete(int id, List<Pair<String, String>> parameters, final TableDeleteCallback callback) {
		// Create delete request
		ServiceFilterRequest delete;
		try {
			Uri.Builder uriBuilder = Uri.parse(mClient.getAppUrl().toString()).buildUpon();
			uriBuilder.path(TABLES_URL);
			uriBuilder.appendPath(URLEncoder.encode(mTableName, MobileServiceClient.UTF8_ENCODING));
			uriBuilder.appendPath(Integer.valueOf(id).toString());

			if (parameters != null && parameters.size() > 0) {
				for (Pair<String, String> parameter : parameters) {
					uriBuilder.appendQueryParameter(parameter.first, parameter.second);
				}
			}
			delete = new ServiceFilterRequestImpl(new HttpDelete(uriBuilder.build().toString()), mClient.getAndroidHttpClientFactory());			
		} catch (UnsupportedEncodingException e) {
			if (callback != null) {
				callback.onCompleted(e, null);
			}
			return;
		}

		// Create AsyncTask to execute the request
		new RequestAsyncTask(delete, mClient.createConnection()) {
			@Override
			protected void onPostExecute(ServiceFilterResponse result) {
				if (callback != null) {
					callback.onCompleted(mTaskException, result);
				}
			}
		}.execute();
	}

	/**
	 * Patches the original entity with the one returned in the response after
	 * executing the operation
	 * 
	 * @param originalEntity
	 *            The original entity
	 * @param newEntity
	 *            The entity obtained after executing the operation
	 * @return
	 */
	protected JsonObject patchOriginalEntityWithResponseEntity(
			JsonObject originalEntity, JsonObject newEntity) {
		// Patch the object to return with the new values
		JsonObject patchedEntityJson = (JsonObject) new JsonParser()
				.parse(originalEntity.toString());

		for (Map.Entry<String, JsonElement> entry : newEntity.entrySet()) {
			patchedEntityJson.add(entry.getKey(), entry.getValue());
		}

		return patchedEntityJson;
	}

	/**
	 * Gets the id property from a given element
	 * 
	 * @param element
	 *            The element to use
	 * @return The id of the element
	 */
	protected int getObjectId(Object element) {
		if (element == null) {
			throw new InvalidParameterException("Element cannot be null");
		} else if (element instanceof Integer) {
			return ((Integer) element).intValue();
		}

		JsonObject jsonObject;
		if (element instanceof JsonObject) {
			jsonObject = (JsonObject) element;
		} else {
			jsonObject = mClient.getGsonBuilder().create().toJsonTree(element)
					.getAsJsonObject();
		}
		
		updateIdProperty(jsonObject);

		JsonElement idProperty = jsonObject.get("id");
		if (idProperty instanceof JsonNull || idProperty == null) {
			throw new InvalidParameterException(
					"Element must contain id property");
		}

		return idProperty.getAsInt();
	}
	
	/**
	 * Updates the JsonObject to have an id property
	 * @param json
	 *            the element to evaluate
	 */
	protected void updateIdProperty(final JsonObject json) throws IllegalArgumentException {
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
	protected boolean isValidTypeId(JsonElement element) {
		return element.isJsonPrimitive() && element.getAsJsonPrimitive().isNumber();
	}

}
