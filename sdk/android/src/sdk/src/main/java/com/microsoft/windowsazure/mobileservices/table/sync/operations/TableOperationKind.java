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
 * TableOperationKind.java
 */
package com.microsoft.windowsazure.mobileservices.table.sync.operations;

import android.annotation.SuppressLint;
import java.util.HashMap;
import java.util.Map;

/**
 * Enumeration for kinds of table operations.
 */
@SuppressLint("UseSparseArrays")
public enum TableOperationKind {

	/**
	 * Insert operation.
	 */
	Insert(0),

	/**
	 * Update operation.
	 */
	Update(1),

	/**
	 * Delete operation.
	 */
	Delete(2);

	private final int mValue;

	private static final Map<Integer, TableOperationKind> mValuesMap;

	static {
		mValuesMap = new HashMap<Integer, TableOperationKind>(3);
		mValuesMap.put(0, TableOperationKind.Insert);
		mValuesMap.put(1, TableOperationKind.Update);
		mValuesMap.put(2, TableOperationKind.Delete);
	}

	private TableOperationKind(int value) {
		this.mValue = value;
	}

	/**
	 * Return the int value associated to the enum
	 */
	public int getValue() {
		return this.mValue;
	}

	/**
	 * Return the TableOperationKind with the provided int value
	 * 
	 * @param value
	 *            the int value
	 * @return the matching TableOperationKind
	 */
	public static TableOperationKind parse(int value) {
		return mValuesMap.get(value);
	}
}