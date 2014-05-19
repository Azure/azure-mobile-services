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
import java.util.concurrent.locks.ReadWriteLock;
import java.util.concurrent.locks.ReentrantReadWriteLock;

public class MultiReadWriteLockDictionary<T> {
	public static class MultiReadWriteLock<T> {
		private T mKey;

		private int mCount;

		private ReadWriteLock mReadWriteLock;

		public MultiReadWriteLock() {
			this.mCount = 0;
			this.mReadWriteLock = new ReentrantReadWriteLock(true);
		}
	}

	private Map<T, MultiReadWriteLock<T>> mMap;

	private Object sync;

	public MultiReadWriteLockDictionary() {
		this.mMap = new HashMap<T, MultiReadWriteLock<T>>();
	}

	public MultiReadWriteLock<T> lockRead(T key) {
		MultiReadWriteLock<T> multiRWLock = increaseLock(key);

		multiRWLock.mReadWriteLock.readLock().lock();

		return multiRWLock;
	}

	public MultiReadWriteLock<T> lockWrite(T key) {
		MultiReadWriteLock<T> multiRWLock = increaseLock(key);

		multiRWLock.mReadWriteLock.writeLock().lock();

		return multiRWLock;
	}

	public void unLockRead(MultiReadWriteLock<T> multiRWLock) {
		multiRWLock.mReadWriteLock.readLock().unlock();

		decreaseLock(multiRWLock);
	}

	public void unLockWrite(MultiReadWriteLock<T> multiRWLock) {
		multiRWLock.mReadWriteLock.writeLock().unlock();

		decreaseLock(multiRWLock);
	}

	private MultiReadWriteLock<T> increaseLock(T key) {
		MultiReadWriteLock<T> multiRWLock = null;

		synchronized (sync) {
			if (!this.mMap.containsKey(key)) {
				this.mMap.put(key, new MultiReadWriteLock<T>());
			}

			multiRWLock = this.mMap.get(key);
			multiRWLock.mCount++;
		}

		return multiRWLock;
	}

	private void decreaseLock(MultiReadWriteLock<T> multiLock) {
		synchronized (sync) {
			multiLock.mCount--;

			if (multiLock.mCount == 0) {
				this.mMap.remove(multiLock.mKey);
			}
		}
	}
}