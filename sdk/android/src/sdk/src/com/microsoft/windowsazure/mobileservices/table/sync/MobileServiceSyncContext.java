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

import java.text.ParseException;
import java.util.LinkedList;
import java.util.Queue;
import java.util.concurrent.ExecutionException;
import java.util.concurrent.Semaphore;
import java.util.concurrent.locks.Lock;
import java.util.concurrent.locks.ReadWriteLock;
import java.util.concurrent.locks.ReentrantLock;
import java.util.concurrent.locks.ReentrantReadWriteLock;

import com.google.common.util.concurrent.ListenableFuture;
import com.google.common.util.concurrent.SettableFuture;
import com.google.gson.JsonObject;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.table.sync.operations.TableOperation;
import com.microsoft.windowsazure.mobileservices.table.sync.operations.TableOperationProcessor;
import com.microsoft.windowsazure.mobileservices.table.sync.queue.OperationQueue;
import com.microsoft.windowsazure.mobileservices.table.sync.queue.OperationQueue.Bookmark;

/**
 * Provides a way to synchronize local database with remote database.
 */
public class MobileServiceSyncContext {
	private static class PushSyncRequest {
		private Bookmark mBookmark;

		private Semaphore mSignalDone;

		private PushSyncRequest(Bookmark bookmark, Semaphore signalDone) {
			this.mBookmark = bookmark;
			this.mSignalDone = signalDone;
		}
	}

	private static class PushSyncRequestConsumer extends Thread {
		MobileServiceSyncContext mContext;

		public PushSyncRequestConsumer(MobileServiceSyncContext context) {
			this.mContext = context;
		}

		@Override
		public void run() {
			try {
				this.mContext.consumePushSR();
			} catch (InterruptedException e) {
				e.printStackTrace();
			}
		}
	}

	private SettableFuture<Void> mInitialized;

	private MobileServiceClient mClient;

	private MobileServiceLocalStore mStore;

	private MobileServiceSyncHandler mHandler;

	/**
	 * Queue for pending operations (insert,update,delete) against remote table.
	 */
	private OperationQueue mOpQueue;

	/**
	 * Queue for pending push sync requests against remote storage.
	 */
	private Queue<PushSyncRequest> mPushSRQueue;

	/**
	 * Consumer thread that processes push sync requests.
	 */
	private PushSyncRequestConsumer mPushSRConsumer;

	/**
	 * Lock to ensure that multiple insert,update,delete operations don't
	 * interleave as they are added to queue and storage.
	 */
	private ReadWriteLock mOpLock;

	/**
	 * Lock to ensure that multiple push sync requests don't interleave
	 */
	private Lock mPushSRLock;

	/**
	 * Lock to block both operations and sync requests while initializing
	 */
	private ReadWriteLock mInitLock;

	/**
	 * Semaphore to signal pending push requests to consumer thread
	 */
	private Semaphore mPendingPush;

	/**
	 * Semaphore to signal that there are currently no pending push requests
	 */
	private Semaphore mPushSRConsumerIdle;

	public MobileServiceSyncContext(MobileServiceClient mobileServiceClient) {
		this.mClient = mobileServiceClient;
		this.mOpLock = new ReentrantReadWriteLock(true);
		this.mPushSRLock = new ReentrantLock(true);
	}

	/**
	 * Returns an instance of MobileServiceLocalStore.
	 * 
	 * @return The MobileServiceLocalStore instance
	 */
	public MobileServiceLocalStore getStore() {
		return this.mStore;
	}

	/**
	 * Returns an instance of MobileServiceSyncHandler.
	 * 
	 * @return The MobileServiceSyncHandler instance
	 */
	public MobileServiceSyncHandler getHandler() {
		return this.mHandler;
	}

	/**
	 * Indicates whether sync context has been initialized or not.
	 * 
	 * @return The initialization status
	 */
	public boolean isInitialized() {
		this.mInitLock.readLock().lock();

		try {
			boolean result = false;

			if (this.mInitialized != null && this.mInitialized.isDone() && !this.mInitialized.isCancelled()) {
				try {
					this.mInitialized.get();
					result = true;
				} catch (Throwable ex) {
				}
			}

			return result;
		} finally {
			this.mInitLock.readLock().unlock();
		}
	}

	/**
	 * Returns the number of pending operations that are not yet pushed to
	 * remote table.
	 * 
	 * @return The number of pending operations
	 */
	public int getPendingOperations() {
		return -1;
	}

	/**
	 * Initializes the sync context.
	 * 
	 * @param store
	 *            An instance of MobileServiceLocalStore
	 * @param handler
	 *            An instance of MobileServiceSyncHandler
	 * 
	 * @return A ListenableFuture that is done when sync context has
	 *         initialized.
	 */
	public ListenableFuture<Void> initialize(final MobileServiceLocalStore store, final MobileServiceSyncHandler handler) {
		final MobileServiceSyncContext thisContext = this;
		final SettableFuture<Void> result = SettableFuture.create();

		new Thread(new Runnable() {

			@Override
			public void run() {
				try {
					thisContext.initializeContext(store, handler);

					result.set(null);
				} catch (Throwable throwable) {
					result.setException(throwable);
				}
			}
		}).start();

		return result;
	}

	/**
	 * Pushes all pending operations up to the remote table.
	 * 
	 * @return A ListenableFuture that is done when operations have been pushed.
	 */
	public ListenableFuture<Void> push() {
		final MobileServiceSyncContext thisContext = this;
		final SettableFuture<Void> result = SettableFuture.create();

		new Thread(new Runnable() {

			@Override
			public void run() {
				try {
					thisContext.pushContext();

					result.set(null);
				} catch (Throwable throwable) {
					result.setException(throwable);
				}
			}
		}).start();

		return result;
	}

	private void initializeContext(final MobileServiceLocalStore store, final MobileServiceSyncHandler handler) throws ParseException, InterruptedException {
		this.mInitLock.writeLock().lock();

		try {
			waitPendingPushSR();

			this.mOpLock.writeLock().lock();

			try {
				this.mPushSRLock.lock();

				try {
					this.mInitialized = SettableFuture.create();

					try {
						this.mHandler = handler;
						this.mStore = store;

						this.mOpQueue = OperationQueue.load(this.mStore);
						this.mPushSRQueue = new LinkedList<PushSyncRequest>();

						this.mPendingPush = new Semaphore(0, true);

						this.mStore.initialize();

						if (this.mPushSRConsumer == null) {
							this.mPushSRConsumer = new PushSyncRequestConsumer(this);
							this.mPushSRConsumer.start();
						}

						this.mInitialized.set(null);
					} catch (Throwable throwable) {
						this.mInitialized.setException(throwable);
						throw throwable;
					}
				} finally {
					this.mPushSRLock.unlock();
				}
			} finally {
				this.mOpLock.writeLock().unlock();
			}
		} finally {
			this.mInitLock.writeLock().unlock();
		}
	}

	private void pushContext() throws Throwable {
		Semaphore signalDone = new Semaphore(0);

		this.mInitLock.readLock().lock();

		try {
			ensureCorrectlyInitialized();

			this.mPushSRLock.lock();

			try {
				Bookmark bookmark = this.mOpQueue.bookmark();

				try {
					this.mPushSRQueue.add(new PushSyncRequest(bookmark, signalDone));
					this.mPendingPush.release();
				} finally {
					this.mOpQueue.unbookmark(bookmark);
				}
			} finally {
				this.mPushSRLock.unlock();
			}
		} finally {
			this.mInitLock.readLock().unlock();
		}

		signalDone.acquire();
	}

	private void ensureCorrectlyInitialized() throws Throwable {
		if (this.mInitialized != null && this.mInitialized.isDone() && !this.mInitialized.isCancelled()) {
			try {
				this.mInitialized.get();
			} catch (ExecutionException e) {
				throw e.getCause();
			}
		} else {
			throw new IllegalStateException("SyncContext is not yet initialized.");
		}
	}

	private void waitPendingPushSR() throws InterruptedException {
		this.mPushSRLock.lock();

		try {
			if (!this.mPushSRQueue.isEmpty()) {
				this.mPushSRConsumerIdle = new Semaphore(0, true);
			}
		} finally {
			this.mPushSRLock.unlock();
		}

		if (this.mPushSRConsumerIdle != null) {
			this.mPushSRConsumerIdle.acquire();
			this.mPushSRConsumerIdle = null;
		}
	}

	private void consumePushSR() throws InterruptedException {
		while (true) {
			this.mPendingPush.acquire();

			this.mPushSRLock.lock();

			try {
				try {
					PushSyncRequest pushSR = this.mPushSRQueue.poll();

					if (pushSR != null) {
						try {
							pushOperations(pushSR.mBookmark);
						} finally {
							pushSR.mSignalDone.release();
						}
					}
				} finally {
					if (this.mPushSRConsumerIdle != null && this.mPushSRQueue.isEmpty()) {
						this.mPushSRConsumerIdle.release();
					}
				}
			} finally {
				this.mPushSRLock.unlock();
			}
		}
	}

	private void pushOperations(Bookmark bookmark) {
		TableOperation operation = bookmark.peek(true);

		while (operation != null) {
			try {
				pushOperation(operation);
				bookmark.dequeue();
			} catch (Throwable throwable) {
				// TODO handle exception

				break;
			} finally {
				bookmark.release(operation);
			}

			operation = bookmark.peek(true);
		}

		this.mOpQueue.unbookmark(bookmark);
	}

	private void pushOperation(TableOperation operation) throws Throwable {
		JsonObject item = this.mStore.lookup(operation.getTableName(), operation.getItemId());

		JsonObject result = this.mHandler.executeTableOperation(new TableOperationProcessor(this.mClient, item), operation);

		if (result != null) {
			this.mStore.upsert(operation.getTableName(), result);
		}
	}
}