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
 * MobileServiceSyncContext.java
 */
package com.microsoft.windowsazure.mobileservices.table.sync;

import com.google.common.util.concurrent.ListenableFuture;
import com.google.common.util.concurrent.SettableFuture;
import com.google.gson.JsonArray;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceException;
import com.microsoft.windowsazure.mobileservices.MobileServiceFeatures;
import com.microsoft.windowsazure.mobileservices.table.MobileServiceExceptionBase;
import com.microsoft.windowsazure.mobileservices.table.MobileServiceJsonTable;
import com.microsoft.windowsazure.mobileservices.table.MobileServiceSystemColumns;
import com.microsoft.windowsazure.mobileservices.table.MobileServiceSystemProperty;
import com.microsoft.windowsazure.mobileservices.table.query.Query;
import com.microsoft.windowsazure.mobileservices.table.query.QueryOperations;
import com.microsoft.windowsazure.mobileservices.table.query.QueryOrder;
import com.microsoft.windowsazure.mobileservices.table.sync.localstore.ColumnDataType;
import com.microsoft.windowsazure.mobileservices.table.sync.localstore.MobileServiceLocalStore;
import com.microsoft.windowsazure.mobileservices.table.sync.localstore.MobileServiceLocalStoreException;
import com.microsoft.windowsazure.mobileservices.table.sync.operations.DeleteOperation;
import com.microsoft.windowsazure.mobileservices.table.sync.operations.InsertOperation;
import com.microsoft.windowsazure.mobileservices.table.sync.operations.LocalTableOperationProcessor;
import com.microsoft.windowsazure.mobileservices.table.sync.operations.MobileServiceTableOperationState;
import com.microsoft.windowsazure.mobileservices.table.sync.operations.RemoteTableOperationProcessor;
import com.microsoft.windowsazure.mobileservices.table.sync.operations.TableOperation;
import com.microsoft.windowsazure.mobileservices.table.sync.operations.TableOperationError;
import com.microsoft.windowsazure.mobileservices.table.sync.operations.UpdateOperation;
import com.microsoft.windowsazure.mobileservices.table.sync.pull.IncrementalPullStrategy;
import com.microsoft.windowsazure.mobileservices.table.sync.pull.PullStrategy;
import com.microsoft.windowsazure.mobileservices.table.sync.push.MobileServicePushCompletionResult;
import com.microsoft.windowsazure.mobileservices.table.sync.push.MobileServicePushFailedException;
import com.microsoft.windowsazure.mobileservices.table.sync.push.MobileServicePushStatus;
import com.microsoft.windowsazure.mobileservices.table.sync.queue.OperationErrorList;
import com.microsoft.windowsazure.mobileservices.table.sync.queue.OperationQueue;
import com.microsoft.windowsazure.mobileservices.table.sync.queue.OperationQueue.Bookmark;
import com.microsoft.windowsazure.mobileservices.table.sync.synchandler.MobileServiceSyncHandler;
import com.microsoft.windowsazure.mobileservices.table.sync.synchandler.MobileServiceSyncHandlerException;
import com.microsoft.windowsazure.mobileservices.threading.MultiLockDictionary;
import com.microsoft.windowsazure.mobileservices.threading.MultiLockDictionary.MultiLock;
import com.microsoft.windowsazure.mobileservices.threading.MultiReadWriteLockDictionary;
import com.microsoft.windowsazure.mobileservices.threading.MultiReadWriteLockDictionary.MultiReadWriteLock;

import java.io.IOException;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.LinkedList;
import java.util.List;
import java.util.Locale;
import java.util.Map;
import java.util.Queue;
import java.util.concurrent.ExecutionException;
import java.util.concurrent.Semaphore;
import java.util.concurrent.locks.Lock;
import java.util.concurrent.locks.ReadWriteLock;
import java.util.concurrent.locks.ReentrantLock;
import java.util.concurrent.locks.ReentrantReadWriteLock;

/**
 * Provides a way to synchronize local database with remote database.
 */
public class MobileServiceSyncContext {
    /**
     * Table that stores backed up items
     */
    private static final String ITEM_BACKUP_TABLE = "__itembackups";
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

    /**
     * Constructor for MobileServiceSyncContext
     *
     * @param client The MobileServiceClient used to invoke table operations
     */
    public MobileServiceSyncContext(MobileServiceClient client) {
        this.mClient = client;
        this.mInitLock = new ReentrantReadWriteLock(true);
        this.mOpLock = new ReentrantReadWriteLock(true);
        this.mPushSRLock = new ReentrantLock(true);
    }

    private static void initializeStore(MobileServiceLocalStore store) throws MobileServiceLocalStoreException {
        Map<String, ColumnDataType> columns = new HashMap<String, ColumnDataType>();
        columns.put("id", ColumnDataType.String);
        columns.put("tablename", ColumnDataType.String);
        columns.put("itemid", ColumnDataType.String);
        columns.put("clientitem", ColumnDataType.Other);

        store.defineTable(ITEM_BACKUP_TABLE, columns);
    }

    private static boolean isDeleted(JsonObject item) {

        JsonElement deletedToken = item.get(MobileServiceSystemColumns.Deleted);

        boolean isDeleted = deletedToken != null && deletedToken.getAsBoolean();

        return isDeleted;
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
     * remote tables.
     *
     * @return The number of pending operations
     * @throws Throwable
     */
    public int getPendingOperations() throws Throwable {

        int result = -1;

        this.mInitLock.readLock().lock();

        try {
            ensureCorrectlyInitialized();

            // get SHARED access to op lock
            this.mOpLock.readLock().lock();

            try {
                result = this.mOpQueue.countPending();
            } finally {
                this.mOpLock.readLock().unlock();
            }

        } finally {
            this.mInitLock.readLock().unlock();
        }

        return result;
    }

    /**
     * Initializes the sync context.
     *
     * @param store   An instance of MobileServiceLocalStore
     * @param handler An instance of MobileServiceSyncHandler
     * @return A ListenableFuture that is done when sync context has
     * initialized.
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
     * Pushes all pending operations up to the remote store.
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

    /**
     * Performs a query against the remote table and stores results.
     *
     * @param tableName the remote table name
     * @param query     an optional query to filter results
     */
    void pull(String tableName, Query query, String queryId) throws Throwable {
        this.mInitLock.readLock().lock();

        try {
            ensureCorrectlyInitialized();

            String invTableName = tableName != null ? tableName.trim().toLowerCase(Locale.getDefault()) : null;

            boolean busyPullDone = false;

            while (!busyPullDone) {
                ListenableFuture<Void> pushFuture = null;

                // prevent Coffman Circular wait condition: lock resources in
                // same order, independent of unlock order. Op then Table then
                // Id.

                // get SHARED access to op lock
                this.mOpLock.readLock().lock();

                try {
                    // get EXCLUSIVE access to table lock
                    MultiReadWriteLock<String> multiRWLock = this.mTableLockMap.lockWrite(invTableName);

                    try {
                        int pendingTable = this.mOpQueue.countPending(invTableName);

                        if (pendingTable > 0) {
                            pushFuture = push();
                        } else {
                            processPull(invTableName, query, queryId);
                        }
                    } finally {
                        this.mTableLockMap.unLockWrite(multiRWLock);
                    }
                } finally {
                    this.mOpLock.readLock().unlock();
                }

                if (pushFuture != null) {
                    try {
                        pushFuture.get();
                    } catch (ExecutionException e) {
                        throw e.getCause();
                    }
                } else {
                    busyPullDone = true;
                }
            }
        } finally {
            this.mInitLock.readLock().unlock();
        }
    }

    /**
     * Performs a query against the local table and deletes the results.
     *
     * @param tableName the local table name
     * @param query     an optional query to filter results
     */
    void purge(String tableName, Query query) throws Throwable {
        this.mInitLock.readLock().lock();

        try {
            ensureCorrectlyInitialized();

            String invTableName = tableName != null ? tableName.trim().toLowerCase(Locale.getDefault()) : null;
            // prevent Coffman Circular wait condition: lock resources in
            // same order, independent of unlock order. Op then Table then
            // Id.

            this.mOpLock.readLock().lock();

            try {
                MultiReadWriteLock<String> multiRWLock = this.mTableLockMap.lockWrite(invTableName);

                try {
                    int pendingTable = this.mOpQueue.countPending(invTableName);

                    if (pendingTable > 0) {
                        throw new MobileServiceException("The table cannot be purged because it has pending operations");
                    } else {
                        processPurge(invTableName, query);
                    }
                } finally {
                    this.mTableLockMap.unLockWrite(multiRWLock);
                }
            } finally {
                this.mOpLock.readLock().unlock();
            }
        } finally {
            this.mInitLock.readLock().unlock();
        }
    }

    /**
     * Retrieve results from the local table.
     *
     * @param tableName the local table name
     * @param query     an optional query to filter results
     * @return a JsonElement with the results
     */
    JsonElement read(String tableName, Query query) throws MobileServiceLocalStoreException {
        String invTableName = tableName != null ? tableName.trim().toLowerCase(Locale.getDefault()) : null;

        if (query == null) {
            query = QueryOperations.tableName(invTableName);
        } else if (query.getTableName() == null || !query.getTableName().equals(invTableName)) {
            query = query.tableName(invTableName);
        }

        return this.mStore.read(query);
    }

    /**
     * Looks up an item from the local table.
     *
     * @param tableName the local table name
     * @param itemId    the id of the item to look up
     * @return the item found
     */
    JsonObject lookUp(String tableName, String itemId) throws MobileServiceLocalStoreException {
        String invTableName = tableName != null ? tableName.trim().toLowerCase(Locale.getDefault()) : null;

        return this.mStore.lookup(invTableName, itemId);
    }

    /**
     * Insert an item into the local table and enqueue the operation to be
     * synchronized on context push.
     *
     * @param tableName the local table name
     * @param item      the item to be inserted
     */
    void insert(String tableName, String itemId, JsonObject item) throws Throwable {
        String invTableName = tableName != null ? tableName.trim().toLowerCase(Locale.getDefault()) : null;

        InsertOperation operation = new InsertOperation(invTableName, itemId);
        processOperation(operation, item);
    }

    /**
     * Update an item in the local table and enqueue the operation to be
     * synchronized on context push.
     *
     * @param tableName the local table name
     * @param item      the item to be updated
     */
    void update(String tableName, String itemId, JsonObject item) throws Throwable {
        String invTableName = tableName != null ? tableName.trim().toLowerCase(Locale.getDefault()) : null;

        UpdateOperation operation = new UpdateOperation(invTableName, itemId);
        processOperation(operation, item);
    }

    /**
     * Delete an item from the local table and enqueue the operation to be
     * synchronized on context push.
     *
     * @param tableName the local table name
     * @param itemId    the id of the item to be deleted
     */
    void delete(String tableName, String itemId) throws Throwable {
        String invTableName = tableName != null ? tableName.trim().toLowerCase(Locale.getDefault()) : null;

        DeleteOperation operation = new DeleteOperation(invTableName, itemId);
        processOperation(operation, null);
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

                        OperationQueue.initializeStore(this.mStore);
                        OperationErrorList.initializeStore(this.mStore);
                        IncrementalPullStrategy.initializeStore(this.mStore);

                        initializeStore(this.mStore);

                        this.mStore.initialize();

                        this.mIdLockMap = new MultiLockDictionary<String>();
                        this.mTableLockMap = new MultiReadWriteLockDictionary<String>();

                        this.mOpQueue = OperationQueue.load(this.mStore);
                        this.mPushSRQueue = new LinkedList<PushSyncRequest>();
                        this.mOpErrorList = OperationErrorList.load(this.mStore);

                        if (this.mPushSRConsumer == null) {
                            this.mPendingPush = new Semaphore(0, true);

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

                pushSR = new PushSyncRequest(bookmark, new Semaphore(0));
                this.mPushSRQueue.add(pushSR);
                this.mPendingPush.release();
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

    private void processPull(String tableName, Query query, String queryId) throws Throwable {

        try {

            MobileServiceJsonTable table = this.mClient.getTable(tableName);

            table.addFeature(MobileServiceFeatures.Offline);

            if (query == null) {
                query = table.top(1000).orderBy("id", QueryOrder.Ascending);
            } else {
                query = query.deepClone();
            }

            PullStrategy strategy;

            if (queryId != null) {
                strategy = new IncrementalPullStrategy(query, queryId, this.mStore, table);
            } else {
                strategy = new PullStrategy(query, table);
            }

            strategy.initialize();

            JsonArray elements = null;

            do {

                JsonElement result = table.execute(strategy.getLastQuery()).get();

                if (result != null) {

                    if (result.isJsonObject()) {
                        JsonObject jsonObject = result.getAsJsonObject();

                        if (jsonObject.has("results") && jsonObject.get("results").isJsonArray()) {
                            elements = jsonObject.get("results").getAsJsonArray();
                        }

                    } else if (result.isJsonArray()) {
                        elements = result.getAsJsonArray();
                    }

                    processElements(tableName, elements);

                    strategy.onResultsProcessed(elements);
                }

            }
            while (strategy.moveToNextPage(elements.size()));

        } catch (ExecutionException e) {
            throw e.getCause();
        } catch (RuntimeException e) {
            throw e.getCause();
        }
    }

    private void processElements(String tableName, JsonArray elements) throws Throwable {
        if (elements != null) {

            List<JsonObject> updatedJsonObjects = new ArrayList<JsonObject>();
            List<String> deletedIds = new ArrayList<String>();


            for (JsonElement element : elements) {

                JsonObject jsonObject = element.getAsJsonObject();
                JsonElement elementId = jsonObject.get(MobileServiceSystemColumns.Id);

                if (elementId == null) {
                    continue;
                }

                if (isDeleted(jsonObject)) {
                    deletedIds.add(elementId.getAsString());
                } else {
                    updatedJsonObjects.add(jsonObject);
                }
            }

            if (deletedIds.size() > 0) {
                this.mStore.delete(tableName,
                        deletedIds.toArray(new String[deletedIds.size()]));
            }

            if (updatedJsonObjects.size() > 0) {
                this.mStore.upsert(tableName,
                        updatedJsonObjects.toArray(new JsonObject[updatedJsonObjects.size()]), true);
            }
        }
    }

    private void processPurge(String tableName, Query query) throws MobileServiceLocalStoreException {
        if (query == null) {
            query = QueryOperations.tableName(tableName);
        } else if (query.getTableName() == null || !query.getTableName().equals(tableName)) {
            query = query.tableName(tableName);
        }

        this.mStore.delete(query);
    }

    private void waitPendingPushSR() throws InterruptedException {
        this.mPushSRLock.lock();

        try {
            if (this.mPushSRQueue != null && !this.mPushSRQueue.isEmpty()) {
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
                            this.mOpQueue.unbookmark(pushSR.mBookmark);
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

                        operation.setOperationState(MobileServiceTableOperationState.Failed);
                        break;
                    } catch (MobileServiceSyncHandlerException syncHandlerException) {
                        MobileServicePushStatus cancelReason = getPushCancelReason(syncHandlerException);

                        if (cancelReason != null) {
                            pushCompletionResult.setStatus(cancelReason);

                            operation.setOperationState(MobileServiceTableOperationState.Failed);
                            break;
                        } else {
                            this.mOpErrorList.add(getTableOperationError(operation, syncHandlerException));
                        }
                    }

                    operation.setOperationState(MobileServiceTableOperationState.Failed);

                    // '/' is a reserved character that cannot be used on string
                    // ids.
                    // We use it to build a unique compound string from
                    // tableName and
                    // itemId
                    String tableItemId = operation.getTableName() + "/" + operation.getItemId();

                    this.mStore.delete(ITEM_BACKUP_TABLE, tableItemId);

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

            this.mOpErrorList.clear();

            this.mHandler.onPushComplete(pushCompletionResult);

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

        if (item == null) {
            // '/' is a reserved character that cannot be used on string ids.
            // We use it to build a unique compound string from tableName and
            // itemId
            String tableItemId = operation.getTableName() + "/" + operation.getItemId();

            JsonObject backedUpItem = this.mStore.lookup(ITEM_BACKUP_TABLE, tableItemId);

            if (backedUpItem != null) {
                item = backedUpItem.get("clientitem").isJsonObject() ? backedUpItem.getAsJsonObject("clientitem") : null;
            }
        }

        operation.setOperationState(MobileServiceTableOperationState.Attempted);

        JsonObject result = this.mHandler.executeTableOperation(new RemoteTableOperationProcessor(this.mClient, item), operation);

        if (result != null) {
            this.mStore.upsert(operation.getTableName(), result, true);
        }
    }

    private LockProtectedOperation peekAndLock(Bookmark bookmark) {
        LockProtectedOperation lockedOp = null;

        // prevent Coffman Circular wait condition: lock resources in same
        // order, independent of unlock order. Op then Table then Id.

        // get EXCLUSIVE access to op lock, for a short time
        this.mOpLock.writeLock().lock();

        try {
            TableOperation operation = bookmark.peek();

            if (operation != null) {
                // get SHARED access to table lock
                MultiReadWriteLock<String> tableLock = this.mTableLockMap.lockRead(operation.getTableName());

                // '/' is a reserved character that cannot be used on string
                // ids.
                // We use it to build a unique compound string from tableName
                // and itemId
                String tableItemId = operation.getTableName() + "/" + operation.getItemId();

                // get EXCLUSIVE access to id lock
                MultiLock<String> idLock = this.mIdLockMap.lock(tableItemId);

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

        if (clientItem == null) {
            // '/' is a reserved character that cannot be used on string ids.
            // We use it to build a unique compound string from tableName and
            // itemId
            String tableItemId = operation.getTableName() + "/" + operation.getItemId();

            JsonObject backedUpItem = this.mStore.lookup(ITEM_BACKUP_TABLE, tableItemId);

            if (backedUpItem != null) {
                clientItem = backedUpItem.get("clientitem").isJsonObject() ? backedUpItem.getAsJsonObject("clientitem") : null;
            }
        }

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

        if (throwable instanceof MobileServiceExceptionBase) {
            MobileServiceExceptionBase mspfEx = (MobileServiceExceptionBase) throwable;
            serverItem = mspfEx.getValue();
        }

        return new TableOperationError(operation.getKind(), operation.getTableName(), operation.getItemId(), clientItem, throwable.getMessage(), statusCode,
                serverResponse, serverItem);
    }

    private void processOperation(TableOperation operation, JsonObject item) throws Throwable {
        this.mInitLock.readLock().lock();

        try {
            ensureCorrectlyInitialized();

            // prevent Coffman Circular wait condition: lock resources in same
            // order, independent of unlock order. Op then Table then Id.

            // get SHARED access to op lock
            this.mOpLock.readLock().lock();

            try {
                // get SHARED access to table lock
                MultiReadWriteLock<String> tableLock = this.mTableLockMap.lockRead(operation.getTableName());

                try {
                    // '/' is a reserved character that cannot be used on string
                    // ids.
                    // We use it to build a unique compound string from
                    // tableName
                    // and itemId
                    String tableItemId = operation.getTableName() + "/" + operation.getItemId();

                    // get EXCLUSIVE access to id lock
                    MultiLock<String> idLock = this.mIdLockMap.lock(tableItemId);

                    try {
                        operation.accept(new LocalTableOperationProcessor(this.mStore, item, ITEM_BACKUP_TABLE));
                        this.mOpQueue.enqueue(operation);
                    } finally {
                        this.mIdLockMap.unLock(idLock);
                    }
                } finally {
                    this.mTableLockMap.unLockRead(tableLock);
                }
            } finally {
                this.mOpLock.readLock().unlock();
            }
        } finally {
            this.mInitLock.readLock().unlock();
        }
    }

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
        private MobileServiceSyncContext mContext;

        private PushSyncRequestConsumer(MobileServiceSyncContext context) {
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

        private LockProtectedOperation(TableOperation operation, MultiReadWriteLock<String> tableLock, MultiLock<String> idLock) {
            this.mOperation = operation;
            this.mTableLock = tableLock;
            this.mIdLock = idLock;
        }

        private TableOperation getOperation() {
            return this.mOperation;
        }

        private MultiReadWriteLock<String> getTableLock() {
            return this.mTableLock;
        }

        private MultiLock<String> getIdLock() {
            return this.mIdLock;
        }
    }
}
