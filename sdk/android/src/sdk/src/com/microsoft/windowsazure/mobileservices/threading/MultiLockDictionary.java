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
 * MultiLockDictionary.java
 */
package com.microsoft.windowsazure.mobileservices.threading;

import java.util.HashMap;
import java.util.Map;
import java.util.concurrent.locks.Lock;
import java.util.concurrent.locks.ReentrantLock;

/**
 * A key-lock dictionary that discards no longer referenced locks
 * 
 * @param <T>
 *            type of the key and param to the MultiLock<T> lock
 */
public class MultiLockDictionary<T> {
	/**
	 * A lock that implements reference count
	 * 
	 * @param <T>
	 *            type of the corresponding key
	 */
	public static class MultiLock<T> {
		private T mKey;
		private int mCount;
		private Lock mLock;

		/**
		 * Constructor for MultiLock
		 */
		public MultiLock() {
			this.mCount = 0;
			this.mLock = new ReentrantLock(true);
		}
	}

	private Map<T, MultiLock<T>> mMap;
	private Object sync;

	/**
	 * Constructor for MultiLockDictionary
	 */
	public MultiLockDictionary() {
		this.mMap = new HashMap<T, MultiLock<T>>();
		this.sync = new Object();
	}

	/**
	 * Aquire a lock for the requested key
	 * 
	 * @param key
	 *            the key
	 * @return the lock
	 */
	public MultiLock<T> lock(T key) {
		MultiLock<T> multiLock = null;

		synchronized (sync) {
			if (!this.mMap.containsKey(key)) {
				this.mMap.put(key, new MultiLock<T>());
			}

			multiLock = this.mMap.get(key);
			multiLock.mCount++;
		}

		multiLock.mLock.lock();

		return multiLock;
	}

	/**
	 * Release the provided lock
	 * 
	 * @param multiLock
	 *            the lock
	 */
	public void unLock(MultiLock<T> multiLock) {
		synchronized (sync) {
			multiLock.mCount--;
			multiLock.mLock.unlock();

			if (multiLock.mCount == 0) {
				this.mMap.remove(multiLock.mKey);
			}
		}
	}
}