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

import java.io.IOException;
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
import com.microsoft.windowsazure.mobileservices.MobileServiceException;
import com.microsoft.windowsazure.mobileservices.table.MobileServicePreconditionFailedExceptionBase;
import com.microsoft.windowsazure.mobileservices.table.sync.operations.TableOperation;
import com.microsoft.windowsazure.mobileservices.table.sync.operations.TableOperationError;
import com.microsoft.windowsazure.mobileservices.table.sync.operations.TableOperationProcessor;
import com.microsoft.windowsazure.mobileservices.table.sync.queue.OperationErrorList;
import com.microsoft.windowsazure.mobileservices.table.sync.queue.OperationQueue;
import com.microsoft.windowsazure.mobileservices.table.sync.queue.OperationQueue.Bookmark;
import com.microsoft.windowsazure.mobileservices.threading.MultiLockDictionary;
import com.microsoft.windowsazure.mobileservices.threading.MultiLockDictionary.MultiLock;
import com.microsoft.windowsazure.mobileservices.threading.MultiReadWriteLockDictionary;
import com.microsoft.windowsazure.mobileservices.threading.MultiReadWriteLockDictionary.MultiReadWriteLock;

/**
 * Provides a way to synchronize local database with remote database.
 */
public class MobileServiceSyncContext {
	private static class PushSyncRequest {
		private Bookmark mBookmark;

		private Semaphore mSignalDone;

		private MobileServicePushFailedException mPushException;

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

	private static class LockProtectedOperation {
		private TableOperation mOperation;
		private MultiReadWriteLock<String> mTableLock;
		private MultiLock<String> mIdLock;

		public LockProtectedOperation(TableOperation operation, MultiReadWriteLock<String> tableLock, MultiLock<String> idLock) {
			this.mOperation = operation;
			this.mTableLock = tableLock;
			this.mIdLock = idLock;
		}

		public TableOperation getOperation() {
			return this.mOperation;
		}

		public MultiReadWriteLock<String> getTableLock() {
			return this.mTableLock;
		}

		public MultiLock<String> getIdLock() {
			return this.mIdLock;
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
	 * List for operation errors against remote table.
	 */
	private OperationErrorList mOpErrorList;

	/**
	 * Consumer thread that processes push sync requests.
	 */
	private PushSyncRequestConsumer mPushSRConsumer;

	/**
	 * Shared/Exclusive Lock that works together with id and table lock
	 */
	private ReadWriteLock mOpLock;

	/**
	 * Lock by table name (table lock)
	 */
	private MultiReadWriteLockDictionary<String> mTableLockMap;

	/**
	 * Lock by item id (row lock)
	 */
	private MultiLockDictionary<String> mIdLockMap;

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

	private void initializeContext(final MobileServiceLocalStore store, final MobileServiceSyncHandler handler) throws Throwable {
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

						this.mIdLockMap = new MultiLockDictionary<String>();
						this.mTableLockMap = new MultiReadWriteLockDictionary<String>();

						this.mOpQueue = OperationQueue.load(this.mStore);
						this.mPushSRQueue = new LinkedList<PushSyncRequest>();
						this.mOpErrorList = OperationErrorList.load(this.mStore);

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
		PushSyncRequest pushSR = null;

		this.mInitLock.readLock().lock();

		try {
			ensureCorrectlyInitialized();

			this.mPushSRLock.lock();

			try {
				Bookmark bookmark = this.mOpQueue.bookmark();

				try {
					pushSR = new PushSyncRequest(bookmark, new Semaphore(0));
					this.mPushSRQueue.add(pushSR);
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

		if (pushSR != null) {
			pushSR.mSignalDone.acquire();

			if (pushSR.mPushException != null) {
				throw pushSR.mPushException;
			}
		}
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
						} catch (MobileServicePushFailedException pushException) {
							pushSR.mPushException = pushException;
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

	private void pushOperations(Bookmark bookmark) throws MobileServicePushFailedException {
		MobileServicePushCompletionResult pushCompletionResult = new MobileServicePushCompletionResult();

		try {
			LockProtectedOperation lockedOp = peekAndLock(bookmark);

			TableOperation operation = lockedOp != null ? lockedOp.getOperation() : null;

			while (operation != null) {
				try {
					try {
						pushOperation(operation);
					} catch (MobileServiceLocalStoreException localStoreException) {
						pushCompletionResult.setStatus(MobileServicePushStatus.CancelledByLocalStoreError);
						break;
					} catch (MobileServiceSyncHandlerException syncHandlerException) {
						MobileServicePushStatus cancelReason = getPushCancelReason(syncHandlerException);

						if (cancelReason != null) {
							pushCompletionResult.setStatus(cancelReason);
							break;
						} else {
							this.mOpErrorList.add(getTableOperationError(operation, syncHandlerException));
						}
					}

					bookmark.dequeue();
				} finally {
					try {
						this.mIdLockMap.unLock(lockedOp.getIdLock());
					} finally {
						this.mTableLockMap.unLockRead(lockedOp.getTableLock());
					}
				}

				lockedOp = peekAndLock(bookmark);

				operation = lockedOp != null ? lockedOp.getOperation() : null;
			}

			if (pushCompletionResult.getStatus() == null) {
				pushCompletionResult.setStatus(MobileServicePushStatus.Complete);
			}

			pushCompletionResult.setOperationErrors(this.mOpErrorList.getAll());

			this.mHandler.onPushComplete(pushCompletionResult);

			this.mOpErrorList.clear();
		} catch (Throwable internalError) {
			pushCompletionResult.setStatus(MobileServicePushStatus.InternalError);
			pushCompletionResult.setInternalError(internalError);
		}

		if (pushCompletionResult.getStatus() != MobileServicePushStatus.Complete) {
			throw new MobileServicePushFailedException(pushCompletionResult);
		}
	}

	private void pushOperation(TableOperation operation) throws MobileServiceLocalStoreException, MobileServiceSyncHandlerException {
		JsonObject item = this.mStore.lookup(operation.getTableName(), operation.getItemId());

		JsonObject result = this.mHandler.executeTableOperation(new TableOperationProcessor(this.mClient, item), operation);

		if (result != null) {
			this.mStore.upsert(operation.getTableName(), result);
		}
	}

	private LockProtectedOperation peekAndLock(Bookmark bookmark) {
		LockProtectedOperation lockedOp = null;

		// get exclusive lock to prevent on going modifications
		// will release quickly, as soon as peek and retain table/id lock
		// prevent Coffman Circular wait condition: lock resources in same
		// order, independent of unlock order. Op then Table then Id.
		this.mOpLock.writeLock().lock();

		try {
			TableOperation operation = bookmark.peek();

			if (operation != null) {
				// get shared access to table lock
				MultiReadWriteLock<String> tableLock = this.mTableLockMap.lockRead(operation.getTableName());

				// get exclusive access to id lock
				MultiLock<String> idLock = this.mIdLockMap.lock(operation.getItemId());

				lockedOp = new LockProtectedOperation(operation, tableLock, idLock);
			}
		} finally {
			this.mOpLock.writeLock().unlock();
		}

		return lockedOp;
	}

	private MobileServicePushStatus getPushCancelReason(MobileServiceSyncHandlerException syncHandlerException) {
		MobileServicePushStatus reason = null;

		Throwable innerException = syncHandlerException.getCause();

		if (innerException instanceof MobileServiceException) {
			MobileServiceException msEx = (MobileServiceException) innerException;
			if (msEx.getCause() != null && msEx.getCause() instanceof IOException) {
				reason = MobileServicePushStatus.CancelledByNetworkError;
			} else if (msEx.getResponse() != null && msEx.getResponse().getStatus() != null && msEx.getResponse().getStatus().getStatusCode() == 401) {
				reason = MobileServicePushStatus.CancelledByAuthenticationError;
			}
		}

		return reason;
	}

	private TableOperationError getTableOperationError(TableOperation operation, Throwable throwable) throws MobileServiceLocalStoreException {
		JsonObject clientItem = this.mStore.lookup(operation.getTableName(), operation.getItemId());
		Integer statusCode = null;
		String serverResponse = null;
		JsonObject serverItem = null;

		if (throwable instanceof MobileServiceException) {
			MobileServiceException msEx = (MobileServiceException) throwable;
			if (msEx.getResponse() != null) {
				serverResponse = msEx.getResponse().getContent();
				if (msEx.getResponse().getStatus() != null) {
					statusCode = msEx.getResponse().getStatus().getStatusCode();
				}
			}
		}

		if (throwable instanceof MobileServicePreconditionFailedExceptionBase) {
			MobileServicePreconditionFailedExceptionBase mspfEx = (MobileServicePreconditionFailedExceptionBase) throwable;
			serverItem = mspfEx.getValue();
		}

		return new TableOperationError(operation.getKind(), operation.getTableName(), operation.getItemId(), clientItem, throwable.getMessage(), statusCode,
				serverResponse, serverItem);
	}
}