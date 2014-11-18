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

/**
 * MobileServiceJsonSyncTable.java
 */
package com.microsoft.windowsazure.mobileservices.table.sync;

import java.util.EnumSet;
import java.util.UUID;

import com.google.common.util.concurrent.ListenableFuture;
import com.google.common.util.concurrent.SettableFuture;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import com.google.gson.JsonSyntaxException;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceFeatures;
import com.microsoft.windowsazure.mobileservices.table.query.Query;
import com.microsoft.windowsazure.mobileservices.table.sync.localstore.MobileServiceLocalStoreException;

/**
 * Provides operations on local table.
 */
public class MobileServiceJsonSyncTable {
	private String mName;
	private MobileServiceClient mClient;

	protected EnumSet<MobileServiceFeatures> mFeatures;

	/**
	 * Constructor for MobileServiceJsonSyncTable
	 * 
	 * @param name
	 *            The name of the represented table
	 * @param client
	 *            The MobileServiceClient used to invoke table operations
	 */
	public MobileServiceJsonSyncTable(String name, MobileServiceClient client) {
		this.mName = name;
		this.mClient = client;
		this.mFeatures = EnumSet.noneOf(MobileServiceFeatures.class);
	}

	/**
	 * Returns the name of the represented table
	 */
	public String getName() {
		return mName;
	}

	/**
	 * Performs a query against the remote table and stores results.
	 * 
	 * @param query
	 *            an optional query to filter results
	 * 
	 * @return A ListenableFuture that is done when results have been pulled.
	 */
	public ListenableFuture<Void> pull(final Query query, final String queryId) {
		final MobileServiceJsonSyncTable thisTable = this;
		final SettableFuture<Void> result = SettableFuture.create();

		new Thread(new Runnable() {

			@Override
			public void run() {
				try {
					thisTable.mClient.getSyncContext().pull(thisTable.mName, query, queryId);

					result.set(null);
				} catch (Throwable throwable) {
					result.setException(throwable);
				}
			}
		}).start();

		return result;
	}

    /**
     * Performs a query against the remote table and stores results.
     *
     * @param query
     *            an optional query to filter results
     *
     * @return A ListenableFuture that is done when results have been pulled.
     */
    public ListenableFuture<Void> pull(final Query query) {

        return pull(query, null);
    }

	/**
	 * Performs a query against the local table and deletes the results.
	 * 
	 * @param query
	 *            an optional query to filter results
	 * 
	 * @return A ListenableFuture that is done when results have been purged.
	 */
	public ListenableFuture<Void> purge(final Query query) {
		final MobileServiceJsonSyncTable thisTable = this;
		final SettableFuture<Void> result = SettableFuture.create();

		new Thread(new Runnable() {

			@Override
			public void run() {
				try {
					thisTable.mClient.getSyncContext().purge(thisTable.mName, query);

					result.set(null);
				} catch (Throwable throwable) {
					result.setException(throwable);
				}
			}
		}).start();

		return result;
	}

	/**
	 * Insert an item into the local table and enqueue the operation to be
	 * synchronized on context push.
	 * 
	 * @param item
	 *            the item to be inserted
	 * 
	 * @return A ListenableFuture that is done when the item has been inserted,
	 *         returning a copy of the inserted item including id.
	 */
	public ListenableFuture<JsonObject> insert(final JsonObject item) {
		final MobileServiceJsonSyncTable thisTable = this;
		final SettableFuture<JsonObject> result = SettableFuture.create();

		new Thread(new Runnable() {

			@Override
			public void run() {
				try {
					JsonObject newItem = thisTable.insertContext(item);

					result.set(newItem);
				} catch (Throwable throwable) {
					result.setException(throwable);
				}
			}
		}).start();

		return result;
	}

	/**
	 * Retrieve results from the local table.
	 * 
	 * @param query
	 *            an optional query to filter results
	 * 
	 * @return A ListenableFuture that is done when the results have been
	 *         retrieved.
	 */
	public ListenableFuture<JsonElement> read(final Query query) {
		final MobileServiceJsonSyncTable thisTable = this;
		final SettableFuture<JsonElement> result = SettableFuture.create();

		new Thread(new Runnable() {

			@Override
			public void run() {
				try {
					JsonElement results = thisTable.readContext(query);

					result.set(results);
				} catch (Throwable throwable) {
					result.setException(throwable);
				}
			}
		}).start();

		return result;
	}

	/**
	 * Looks up an item from the local table.
	 * 
	 * @param itemId
	 *            the id of the item to look up
	 * 
	 * @return A ListenableFuture that is done when the item has been looked up.
	 */
	public ListenableFuture<JsonObject> lookUp(final String itemId) {
		final MobileServiceJsonSyncTable thisTable = this;
		final SettableFuture<JsonObject> result = SettableFuture.create();

		new Thread(new Runnable() {

			@Override
			public void run() {
				try {
					JsonObject item = thisTable.lookUpContext(itemId);

					result.set(item);
				} catch (Throwable throwable) {
					result.setException(throwable);
				}
			}
		}).start();

		return result;
	}

	/**
	 * Update an item in the local table and enqueue the operation to be
	 * synchronized on context push.
	 * 
	 * @param item
	 *            the item to be updated
	 * 
	 * @return A ListenableFuture that is done when the item has been updated.
	 */
	public ListenableFuture<Void> update(final JsonObject item) {
		final MobileServiceJsonSyncTable thisTable = this;
		final SettableFuture<Void> result = SettableFuture.create();

		new Thread(new Runnable() {

			@Override
			public void run() {
				try {
					thisTable.updateContext(item);

					result.set(null);
				} catch (Throwable throwable) {
					result.setException(throwable);
				}
			}
		}).start();

		return result;
	}

	/**
	 * Delete an item from the local table and enqueue the operation to be
	 * synchronized on context push.
	 * 
	 * @param item
	 *            the item to be deleted
	 * 
	 * @return A ListenableFuture that is done when the item has been deleted.
	 */
	public ListenableFuture<Void> delete(final JsonObject item) {
		final MobileServiceJsonSyncTable thisTable = this;
		final SettableFuture<Void> result = SettableFuture.create();

		new Thread(new Runnable() {

			@Override
			public void run() {
				try {
					thisTable.deleteContext(item);

					result.set(null);
				} catch (Throwable throwable) {
					result.setException(throwable);
				}
			}
		}).start();

		return result;
	}

	/**
	 * Delete an item from the local table and enqueue the operation to be
	 * synchronized on context push.
	 * 
	 * @param itemId
	 *            the id of the item to be deleted
	 * 
	 * @return A ListenableFuture that is done when the item has been deleted.
	 */
	public ListenableFuture<Void> delete(final String itemId) {
		final MobileServiceJsonSyncTable thisTable = this;
		final SettableFuture<Void> result = SettableFuture.create();

		new Thread(new Runnable() {

			@Override
			public void run() {
				try {
					thisTable.deleteContext(itemId);

					result.set(null);
				} catch (Throwable throwable) {
					result.setException(throwable);
				}
			}
		}).start();

		return result;
	}

	private JsonElement readContext(Query query) throws MobileServiceLocalStoreException {
		return this.mClient.getSyncContext().read(this.mName, query);
	}

	private JsonObject lookUpContext(String itemId) throws MobileServiceLocalStoreException {
		if (!isValidStringId(itemId)) {
			throw new IllegalArgumentException("The entity id has an invalid string value.");
		}

		return this.mClient.getSyncContext().lookUp(this.mName, itemId);
	}

	private JsonObject insertContext(JsonObject item) throws Throwable {
		JsonObject newItem = validateIdOnInsert(item);

		this.mClient.getSyncContext().insert(this.mName, newItem.get("id").getAsString(), newItem);

		return newItem;
	}

	private void updateContext(JsonObject item) throws Throwable {
		JsonObject newItem = validateIdOnUpdateOrDelete(item);

		this.mClient.getSyncContext().update(this.mName, newItem.get("id").getAsString(), newItem);
	}

	private void deleteContext(JsonObject item) throws Throwable {
		JsonObject newItem = validateIdOnUpdateOrDelete(item);

		this.mClient.getSyncContext().delete(this.mName, newItem.get("id").getAsString());
	}

	private void deleteContext(String itemId) throws Throwable {
		if (!isValidStringId(itemId)) {
			throw new IllegalArgumentException("The entity id has an invalid string value.");
		}

		this.mClient.getSyncContext().delete(this.mName, itemId);
	}

	private JsonObject validateIdOnInsert(JsonObject item) throws JsonSyntaxException, IllegalArgumentException {
		JsonObject newItem = (JsonObject) new JsonParser().parse(item.toString());
		String itemId = null;
		String idProperty = getIdProperty(newItem);

		if (idProperty != null) {
			if (newItem.get(idProperty).isJsonPrimitive() && newItem.get(idProperty).getAsJsonPrimitive().isString()) {
				itemId = newItem.get(idProperty).getAsJsonPrimitive().getAsString();

				if (!isValidStringId(itemId)) {
					throw new IllegalArgumentException("The entity to insert has an invalid string value on " + idProperty + " property.");
				}
			} else if (newItem.get(idProperty).isJsonNull()) {
				itemId = UUID.randomUUID().toString();
			} else {
				throw new IllegalArgumentException("The entity to insert should not have an " + idProperty + " defined with a non string value");
			}

			newItem.remove(idProperty);
			newItem.addProperty("id", itemId);
		} else {
			itemId = UUID.randomUUID().toString();
			newItem.addProperty("id", itemId);
		}

		return newItem;
	}

	private JsonObject validateIdOnUpdateOrDelete(JsonObject item) throws JsonSyntaxException, IllegalArgumentException {
		JsonObject newItem = (JsonObject) new JsonParser().parse(item.toString());
		String itemId = null;
		String idProperty = getIdProperty(newItem);

		if (idProperty != null) {
			if (newItem.get(idProperty).isJsonPrimitive() && newItem.get(idProperty).getAsJsonPrimitive().isString()) {
				itemId = newItem.get(idProperty).getAsJsonPrimitive().getAsString();

				if (!isValidStringId(itemId)) {
					throw new IllegalArgumentException("The entity to update/delete has an invalid string value on " + idProperty + " property.");
				}
			} else if (newItem.get(idProperty).isJsonNull()) {
				throw new IllegalArgumentException("The entity to update/delete should have an id defined with a string value");
			} else {
				throw new IllegalArgumentException("The entity to update/delete should not have an " + idProperty + " defined with a non string value");
			}

			newItem.remove(idProperty);
			newItem.addProperty("id", itemId);
		} else {
			throw new IllegalArgumentException("The entity to update/delete should have an id defined with a string value");
		}

		return newItem;
	}

	private String getIdProperty(JsonObject item) {
		String result = null;
		String[] idPropertyNames = new String[] { "id", "Id", "iD", "ID" };

		for (String idPropertyName : idPropertyNames) {
			if (item.has(idPropertyName)) {
				if (result != null) {
					throw new IllegalArgumentException("The entity to insert should not have more than one " + result + " property defined.");
				}

				result = idPropertyName;
			}
		}

		return result;
	}

	private boolean isDefaultStringId(String id) {
		return (id == null) || (id.equals(""));
	}

	private boolean isValidStringId(String id) {
		return !isDefaultStringId(id) && id.length() <= 255 && !containsControlCharacter(id) && !containsSpecialCharacter(id) && !id.equals(".")
				&& !id.equals("..");
	}

	private boolean containsControlCharacter(String s) {
		boolean result = false;

		final int length = s.length();

		for (int offset = 0; offset < length;) {
			final int codepoint = s.codePointAt(offset);

			if (Character.isISOControl(codepoint)) {
				result = true;
				break;
			}

			offset += Character.charCount(codepoint);
		}

		return result;
	}

	private boolean containsSpecialCharacter(String s) {
		boolean result = false;

		final int length = s.length();

		final int cpQuotationMark = 0x0022;
		final int cpPlusSign = 0x002B;
		final int cpSolidus = 0x002F;
		final int cpQuestionMark = 0x003F;
		final int cpReverseSolidus = 0x005C;
		final int cpGraveAccent = 0x0060;

		for (int offset = 0; offset < length;) {
			final int codepoint = s.codePointAt(offset);

			if (codepoint == cpQuotationMark || codepoint == cpPlusSign || codepoint == cpSolidus || codepoint == cpQuestionMark
					|| codepoint == cpReverseSolidus || codepoint == cpGraveAccent) {
				result = true;
				break;
			}

			offset += Character.charCount(codepoint);
		}

		return result;
	}
}