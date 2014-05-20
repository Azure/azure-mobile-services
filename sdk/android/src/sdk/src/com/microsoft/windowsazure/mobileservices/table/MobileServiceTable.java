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

package com.microsoft.windowsazure.mobileservices.table;

import java.lang.reflect.Field;
import java.util.EnumSet;
import java.util.List;

import android.util.Pair;

import com.google.common.util.concurrent.FutureCallback;
import com.google.common.util.concurrent.Futures;
import com.google.common.util.concurrent.ListenableFuture;
import com.google.common.util.concurrent.SettableFuture;
import com.google.gson.Gson;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceException;
import com.microsoft.windowsazure.mobileservices.MobileServiceList;
import com.microsoft.windowsazure.mobileservices.table.query.Query;
import com.microsoft.windowsazure.mobileservices.table.serialization.JsonEntityParser;

/**
 * Represents a Mobile Service Table
 */
public final class MobileServiceTable<E> extends MobileServiceTableBase<MobileServiceList<E>> {

	private MobileServiceJsonTable mInternalTable;

	private Class<E> mClazz;

	/**
	 * Constructor for MobileServiceTable
	 * 
	 * @param name
	 *            The name of the represented table
	 * @param client
	 *            The MobileServiceClient used to invoke table operations
	 */
	public MobileServiceTable(String name, MobileServiceClient client, Class<E> clazz) {
		initialize(name, client);

		mInternalTable = new MobileServiceJsonTable(name, client);
		mClazz = clazz;

		mSystemProperties = getSystemProperties(clazz);
		mInternalTable.setSystemProperties(mSystemProperties);
	}

	public EnumSet<MobileServiceSystemProperty> getSystemProperties() {
		return mInternalTable.getSystemProperties();
	}

	public void setSystemProperties(EnumSet<MobileServiceSystemProperty> systemProperties) {
		this.mSystemProperties = systemProperties;
		this.mInternalTable.setSystemProperties(systemProperties);
	}

	/**
	 * Executes a query to retrieve all the table rows
	 * 
	 * @throws MobileServiceException
	 */
	public ListenableFuture<MobileServiceList<E>> execute() throws MobileServiceException {
		// mInternalTable.execute(new ParseResultTableQueryCallback(callback));
		final SettableFuture<MobileServiceList<E>> future = SettableFuture.create();
		ListenableFuture<JsonElement> internalFuture = mInternalTable.execute();
		Futures.addCallback(internalFuture, new FutureCallback<JsonElement>() {
			@Override
			public void onFailure(Throwable exc) {
				future.setException(exc);
			}

			@Override
			public void onSuccess(JsonElement result) {
				try {
					if (result.isJsonObject()) {
						JsonObject jsonObject = result.getAsJsonObject();

						int count = jsonObject.get("count").getAsInt();
						JsonElement elements = jsonObject.get("results");

						List<E> list = parseResults(elements);
						future.set(new MobileServiceList<E>(list, count));
					} else {
						List<E> list = parseResults(result);
						future.set(new MobileServiceList<E>(list, list.size()));
					}
				} catch (Exception e) {
					future.setException(e);
				}
			}
		});

		return future;
	}

	/**
	 * Executes a query to retrieve all the table rows
	 * 
	 * @param query
	 *            The Query instance to execute
	 * @throws MobileServiceException
	 */
	public ListenableFuture<MobileServiceList<E>> execute(Query query) throws MobileServiceException {
		// mInternalTable.execute(query, new
		// ParseResultTableQueryCallback(callback));

		final SettableFuture<MobileServiceList<E>> future = SettableFuture.create();
		ListenableFuture<JsonElement> internalFuture = mInternalTable.execute(query);
		Futures.addCallback(internalFuture, new FutureCallback<JsonElement>() {
			@Override
			public void onFailure(Throwable exc) {
				future.setException(exc);
			}

			@Override
			public void onSuccess(JsonElement result) {
				try {
					if (result.isJsonObject()) {
						JsonObject jsonObject = result.getAsJsonObject();

						int count = jsonObject.get("count").getAsInt();
						JsonElement elements = jsonObject.get("results");

						List<E> list = parseResults(elements);
						future.set(new MobileServiceList<E>(list, count));
					} else {
						List<E> list = parseResults(result);
						future.set(new MobileServiceList<E>(list, list.size()));
					}
				} catch (Exception e) {
					future.setException(e);
				}
			}
		});

		return future;
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
	public ListenableFuture<E> lookUp(Object id) {

		// mInternalTable.lookUp(id, null, new
		// ParseResultOperationCallback(callback));

		return lookUp(id, null);
	}

	/**
	 * Looks up a row in the table. Deserializes the row using the given class.
	 * 
	 * @param id
	 *            The id of the row
	 * @param clazz
	 *            The class used to deserialize the row
	 * @param parameters
	 *            A list of user-defined parameters and values to include in the
	 *            request URI query string
	 * @param callback
	 *            Callback to invoke after the operation is completed
	 */
	public ListenableFuture<E> lookUp(Object id, List<Pair<String, String>> parameters) {

		// mInternalTable.lookUp(id, parameters, new
		// ParseResultOperationCallback(callback));
		final SettableFuture<E> future = SettableFuture.create();

		ListenableFuture<JsonObject> internalFuture = mInternalTable.lookUp(id, parameters);
		Futures.addCallback(internalFuture, new FutureCallback<JsonElement>() {
			@Override
			public void onFailure(Throwable exc) {
				if (exc instanceof MobileServicePreconditionFailedExceptionBase) {
					MobileServicePreconditionFailedExceptionBase ex = (MobileServicePreconditionFailedExceptionBase) exc;

					E entity = null;

					try {
						entity = parseResults(ex.getValue()).get(0);
					} catch (Exception e) {
					}

					future.setException(new MobileServicePreconditionFailedException(ex, entity));
				} else {
					future.setException(exc);
				}
			}

			@Override
			public void onSuccess(JsonElement result) {
				try {
					future.set(parseResults(result).get(0));
				} catch (Exception e) {
					future.setException(e);
				}
			}
		});

		return future;
	}

	/**
	 * Inserts an entity into a Mobile Service Table
	 * 
	 * @param element
	 *            The entity to insert
	 * @param callback
	 *            Callback to invoke when the operation is completed
	 */
	public ListenableFuture<E> insert(final E element) {
		return this.insert(element, null);
	}

	/**
	 * Inserts an entity into a Mobile Service Table
	 * 
	 * @param element
	 *            The entity to insert
	 * @param parameters
	 *            A list of user-defined parameters and values to include in the
	 *            request URI query string
	 * @param callback
	 *            Callback to invoke when the operation is completed
	 */
	public ListenableFuture<E> insert(final E element, List<Pair<String, String>> parameters) {
		final SettableFuture<E> future = SettableFuture.create();
		JsonObject json = null;
		try {
			json = mClient.getGsonBuilder().create().toJsonTree(element).getAsJsonObject();
		} catch (IllegalArgumentException e) {
			future.setException(e);
			/*
			 * if (callback != null) { callback.onCompleted(null, e, null); }
			 */

			return future;
		}

		Class<?> idClazz = getIdPropertyClass(element.getClass());

		if (idClazz != null && !isIntegerClass(idClazz)) {
			json = removeSystemProperties(json);
		}

		// mInternalTable.insert(json, parameters, new
		// ParseResultOperationCallback(callback, element));

		ListenableFuture<JsonObject> internalFuture = mInternalTable.insert(json, parameters);
		Futures.addCallback(internalFuture, new FutureCallback<JsonElement>() {
			@Override
			public void onFailure(Throwable exc) {
				if (exc instanceof MobileServicePreconditionFailedExceptionBase) {
					MobileServicePreconditionFailedExceptionBase ex = (MobileServicePreconditionFailedExceptionBase) exc;

					E entity = null;

					try {
						entity = parseResults(ex.getValue()).get(0);

						if (entity != null && element != null) {
							copyFields(entity, element);
							entity = element;
						}
					} catch (Exception e) {
					}

					future.setException(new MobileServicePreconditionFailedException(ex, entity));

				} else {
					future.setException(exc);
				}
			}

			@Override
			public void onSuccess(JsonElement result) {
				E entity = null;
				try {
					entity = parseResults(result).get(0);
					if (entity != null && element != null) {
						copyFields(entity, element);
						entity = element;
					}
					future.set(entity);
				} catch (Exception e) {
					future.setException(e);
				}
			}
		});

		return future;
	}

	/**
	 * Updates an entity from a Mobile Service Table
	 * 
	 * @param element
	 *            The entity to update
	 * @param callback
	 *            Callback to invoke when the operation is completed
	 */
	public ListenableFuture<E> update(final E element) {
		return this.update(element, null);
	}

	/**
	 * Updates an entity from a Mobile Service Table
	 * 
	 * @param element
	 *            The entity to update
	 * @param parameters
	 *            A list of user-defined parameters and values to include in the
	 *            request URI query string
	 * @param callback
	 *            Callback to invoke when the operation is completed
	 */
	public ListenableFuture<E> update(final E element, final List<Pair<String, String>> parameters) {
		final SettableFuture<E> future = SettableFuture.create();

		JsonObject json = null;

		try {
			json = mClient.getGsonBuilder().create().toJsonTree(element).getAsJsonObject();
		} catch (IllegalArgumentException e) {
			/*
			 * if (callback != null) { callback.onCompleted(null, e, null); }
			 */
			future.setException(e);
			return future;
		}

		// mInternalTable.update(json, parameters, new
		// ParseResultOperationCallback(callback, element));

		ListenableFuture<JsonObject> internalFuture = mInternalTable.update(json, parameters);
		Futures.addCallback(internalFuture, new FutureCallback<JsonElement>() {
			@Override
			public void onFailure(Throwable exc) {
				if (exc instanceof MobileServicePreconditionFailedExceptionBase) {
					MobileServicePreconditionFailedExceptionBase ex = (MobileServicePreconditionFailedExceptionBase) exc;

					E entity = null;

					try {
						entity = parseResults(ex.getValue()).get(0);

						if (entity != null && element != null) {
							copyFields(entity, element);
							entity = element;
						}
					} catch (Exception e) {
					}

					future.setException(new MobileServicePreconditionFailedException(ex, entity));

				} else {
					future.setException(exc);
				}
			}

			@Override
			public void onSuccess(JsonElement result) {
				E entity = null;
				try {
					entity = parseResults(result).get(0);
					if (entity != null && element != null) {
						copyFields(entity, element);
						entity = element;
					}
					future.set(entity);
				} catch (Exception e) {
					future.setException(e);
				}
			}
		});

		return future;
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
	private void copyFields(Object source, Object target) throws IllegalArgumentException, IllegalAccessException {
		if (source != null && target != null) {
			for (Field field : source.getClass().getDeclaredFields()) {
				field.setAccessible(true);
				field.set(target, field.get(source));
			}
		}
	}

}
