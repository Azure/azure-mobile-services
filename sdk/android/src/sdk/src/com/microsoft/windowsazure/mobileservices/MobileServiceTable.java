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
/*
 * MobileServiceTable.java
 */

package com.microsoft.windowsazure.mobileservices;

import java.lang.reflect.Field;
import java.security.InvalidParameterException;
import java.util.List;

import android.util.Pair;

import com.google.gson.Gson;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;

/**
 * Represents a Mobile Service Table
 */
public final class MobileServiceTable<E> extends
MobileServiceTableBase<TableQueryCallback<E>> {

	private MobileServiceJsonTable mInternalTable;

	private Class<E> mClazz;

	class ParseResultTableQueryCallback implements TableJsonQueryCallback {
		TableQueryCallback<E> mCallback;

		public ParseResultTableQueryCallback(TableQueryCallback<E> callback) {
			mCallback = callback;
		}

		@Override
		public void onCompleted(JsonElement result, int count,
				Exception exception, ServiceFilterResponse response) {
			if (mCallback != null) {
				if (exception == null && result != null) {
					Exception ex = null;
					List<E> list = null;
					try {
						list = parseResults(result);
					} catch (Exception e) {
						ex = e;
					}
					mCallback.onCompleted(list, count, ex, response);
				} else {
					mCallback.onCompleted(null, count, exception, response);
				}
			}
		}
	}

	class ParseResultOperationCallback implements TableJsonOperationCallback {
		private TableOperationCallback<E> mCallback;
		private E mOriginalEntity;

		public ParseResultOperationCallback(TableOperationCallback<E> callback) {
			this(callback, null);
		}

		public ParseResultOperationCallback(TableOperationCallback<E> callback,
				E originalEntity) {
			mCallback = callback;
			mOriginalEntity = originalEntity;
		}

		@Override
		public void onCompleted(JsonObject jsonEntity, Exception exception,
				ServiceFilterResponse response) {
			if (exception == null && jsonEntity != null) {
				E entity = null;
				Exception ex = null;
				try {
					entity = parseResults(jsonEntity).get(0);
					if (entity != null && mOriginalEntity != null) {
						copyFields(entity, mOriginalEntity);
						entity = mOriginalEntity;
					}
				} catch (Exception e) {
					ex = e;
				}

				if (mCallback != null)
					mCallback.onCompleted(entity, ex, response);
			} else {
				if (mCallback != null)
					mCallback.onCompleted(null, exception, response);
			}

		}
	}

	/**
	 * Constructor for MobileServiceTable
	 * 
	 * @param name
	 *            The name of the represented table
	 * @param client
	 *            The MobileServiceClient used to invoke table operations
	 */
	public MobileServiceTable(String name, MobileServiceClient client,
			Class<E> clazz) {

		initialize(name, client);
		mInternalTable = new MobileServiceJsonTable(name, client);
		mClazz = clazz;
	}

	/**
	 * Executes a query to retrieve all the table rows
	 * 
	 * @param callback
	 *            Callback to invoke when the operation is completed
	 */
	public void execute(TableQueryCallback<E> callback) {
		mInternalTable.execute(new ParseResultTableQueryCallback(callback));
	}

	/**
	 * Executes a query to retrieve all the table rows
	 * 
	 * @param query
	 *            The MobileServiceQuery instance to execute
	 * @param callback
	 *            Callback to invoke when the operation is completed
	 */
	public void execute(MobileServiceQuery<?> query,
			final TableQueryCallback<E> callback) {
		mInternalTable.execute(query, new ParseResultTableQueryCallback(
				callback));
	}

	/**
	 * Looks up a row in the table. Deserializes the row using the given class.
	 * 
	 * @param id
	 *            The id of the row
	 * @param clazz
	 *            The class used to deserialize the row
	 * @param callback
	 *            Callback to invoke after the operation is completed
	 */
	public void lookUp(Object id, final TableOperationCallback<E> callback) {

		mInternalTable.lookUp(id, null, new ParseResultOperationCallback(callback));
	}
	
	/**
	 * Looks up a row in the table. Deserializes the row using the given class.
	 * 
	 * @param id
	 *            The id of the row
	 * @param clazz
	 *            The class used to deserialize the row
	 * @param parameters
	 *            A list of user-defined parameters and values to include in the request URI query string
	 * @param callback
	 *            Callback to invoke after the operation is completed
	 */
	public void lookUp(Object id, List<Pair<String, String>> parameters, final TableOperationCallback<E> callback) {

		mInternalTable.lookUp(id, parameters, new ParseResultOperationCallback(callback));
	}

	/**
	 * Inserts an entity into a Mobile Service Table
	 * 
	 * @param element
	 *            The entity to insert
	 * @param callback
	 *            Callback to invoke when the operation is completed
	 */
	public void insert(final E element, final TableOperationCallback<E> callback) {
		this.insert(element, null, callback);
	}
	
	/**
	 * Inserts an entity into a Mobile Service Table
	 * 
	 * @param element
	 *            The entity to insert
	 * @param parameters
	 *            A list of user-defined parameters and values to include in the request URI query string
	 * @param callback
	 *            Callback to invoke when the operation is completed
	 */
	public void insert(final E element, List<Pair<String, String>> parameters, final TableOperationCallback<E> callback) {
		JsonObject json = null;
		try {
			json = mClient.getGsonBuilder().create().toJsonTree(element).getAsJsonObject();
		} catch (InvalidParameterException e) {
			if (callback != null) {
				callback.onCompleted(null, e, null);
			}

			return;
		}

		mInternalTable.insert(json, parameters, new ParseResultOperationCallback(callback,
				element));
	}

	/**
	 * Updates an entity from a Mobile Service Table
	 * 
	 * @param element
	 *            The entity to update
	 * @param callback
	 *            Callback to invoke when the operation is completed
	 */
	public void update(final E element, 
			final TableOperationCallback<E> callback) {
		this.update(element, null, callback);
	}
	
	/**
	 * Updates an entity from a Mobile Service Table
	 * 
	 * @param element
	 *            The entity to update
	 * @param parameters
	 *            A list of user-defined parameters and values to include in the request URI query string
	 * @param callback
	 *            Callback to invoke when the operation is completed
	 */
	public void update(final E element, 
			final List<Pair<String, String>> parameters,
			final TableOperationCallback<E> callback) {
		JsonObject json = null;
		
		try {
			json = mClient.getGsonBuilder().create().toJsonTree(element).getAsJsonObject();
		} catch (InvalidParameterException e) {
			if (callback != null) {
				callback.onCompleted(null, e, null);
			}

			return;
		}
		
		mInternalTable.update(json, parameters, new ParseResultOperationCallback(callback,
				element));
	}

	/**
	 * Parses the JSON object to a typed list
	 * 
	 * @param results
	 *            JSON results
	 * @return List of entities
	 */
	private List<E> parseResults(JsonElement results) {
		Gson gson = mClient.getGsonBuilder().create();
		return JsonEntityParser.parseResults(results, gson, mClazz);
	}

	/**
	 * Copy object field values from source to target object
	 * 
	 * @param source
	 *            The object to copy the values from
	 * @param target
	 *            The destination object
	 * @throws IllegalAccessException
	 * @throws IllegalArgumentException
	 */
	private void copyFields(Object source, Object target)
			throws IllegalArgumentException, IllegalAccessException {
		if (source != null && target != null) {
			for (Field field : source.getClass().getDeclaredFields()) {
				field.setAccessible(true);
				field.set(target, field.get(source));
			}
		}
	}

}
