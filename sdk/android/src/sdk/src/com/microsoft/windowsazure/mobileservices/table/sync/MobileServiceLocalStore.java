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
package com.microsoft.windowsazure.mobileservices.table.sync;

import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.microsoft.windowsazure.mobileservices.table.query.Query;

/**
 * Allows saving and reading data in the local tables.
 */
public interface MobileServiceLocalStore {
	/**
	 * Initializes the store for use.
	 */
	public void initialize() throws MobileServiceLocalStoreException;

	public JsonElement read(Query query) throws MobileServiceLocalStoreException;

	public JsonObject lookup(String tableName, String itemId) throws MobileServiceLocalStoreException;

	public void upsert(String tableName, JsonObject item) throws MobileServiceLocalStoreException;

	public void delete(String tableName, String itemId) throws MobileServiceLocalStoreException;

	public void delete(Query query) throws MobileServiceLocalStoreException;
}
