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
package com.microsoft.windowsazure.mobileservices.threading;

import java.util.HashMap;
import java.util.Map;
import java.util.concurrent.locks.ReentrantLock;

public class MultiLockDictionary<T> {
	public static class MultiLock<T> {
		private T mKey;

		private int mCount;

		private ReentrantLock mLock;

		public MultiLock() {
			this.mCount = 0;
			this.mLock = new ReentrantLock(true);
		}
	}

	private Map<T, MultiLock<T>> mMap;

	private Object sync;

	public MultiLockDictionary() {
		this.mMap = new HashMap<T, MultiLock<T>>();
	}

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