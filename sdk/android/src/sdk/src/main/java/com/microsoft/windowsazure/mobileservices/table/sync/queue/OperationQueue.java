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
 * OperationQueue.java
 */
package com.microsoft.windowsazure.mobileservices.table.sync.queue;

import com.google.gson.JsonArray;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.microsoft.windowsazure.mobileservices.table.query.QueryOperations;
import com.microsoft.windowsazure.mobileservices.table.query.QueryOrder;
import com.microsoft.windowsazure.mobileservices.table.serialization.DateSerializer;
import com.microsoft.windowsazure.mobileservices.table.sync.localstore.ColumnDataType;
import com.microsoft.windowsazure.mobileservices.table.sync.localstore.MobileServiceLocalStore;
import com.microsoft.windowsazure.mobileservices.table.sync.localstore.MobileServiceLocalStoreException;
import com.microsoft.windowsazure.mobileservices.table.sync.operations.DeleteOperation;
import com.microsoft.windowsazure.mobileservices.table.sync.operations.InsertOperation;
import com.microsoft.windowsazure.mobileservices.table.sync.operations.TableOperation;
import com.microsoft.windowsazure.mobileservices.table.sync.operations.TableOperationCollapser;
import com.microsoft.windowsazure.mobileservices.table.sync.operations.TableOperationKind;
import com.microsoft.windowsazure.mobileservices.table.sync.operations.TableOperationVisitor;
import com.microsoft.windowsazure.mobileservices.table.sync.operations.UpdateOperation;

import java.text.ParseException;
import java.util.Date;
import java.util.HashMap;
import java.util.LinkedList;
import java.util.Map;
import java.util.Queue;
import java.util.concurrent.locks.ReadWriteLock;
import java.util.concurrent.locks.ReentrantReadWriteLock;

/**
 * Queue of all table operations i.e. Push, Pull, Insert, Update, Delete
 */
public class OperationQueue {
    /**
     * Table that stores operation queue items
     */
    private static final String OPERATION_QUEUE_TABLE = "__operations";
    private MobileServiceLocalStore mStore;
    private Queue<OperationQueueItem> mQueue;
    private Queue<BookmarkQueueItem> mBookmarkQueue;
    private Map<String, OperationQueueItem> mIdOperationMap;
    private Map<String, Integer> mTableCountMap;
    private Date mLoadedAt;
    private long mSequence;
    private ReadWriteLock mSyncLock;
    private OperationQueue(MobileServiceLocalStore store) {
        this.mStore = store;

        this.mQueue = new LinkedList<OperationQueueItem>();
        this.mBookmarkQueue = new LinkedList<BookmarkQueueItem>();

        this.mIdOperationMap = new HashMap<String, OperationQueueItem>();
        this.mTableCountMap = new HashMap<String, Integer>();

        this.mLoadedAt = new Date();
        this.mSequence = 0;

        this.mSyncLock = new ReentrantReadWriteLock(true);
    }

    /**
     * Initializes requirements on the local store
     *
     * @param store the local store
     * @throws MobileServiceLocalStoreException
     */
    public static void initializeStore(MobileServiceLocalStore store) throws MobileServiceLocalStoreException {
        Map<String, ColumnDataType> columns = new HashMap<String, ColumnDataType>();
        columns.put("id", ColumnDataType.String);
        columns.put("kind", ColumnDataType.Number);
        columns.put("tablename", ColumnDataType.String);
        columns.put("itemid", ColumnDataType.String);
        columns.put("__createdat", ColumnDataType.Date);
        columns.put("__queueloadedat", ColumnDataType.Date);
        columns.put("sequence", ColumnDataType.Number);

        store.defineTable(OPERATION_QUEUE_TABLE, columns);
    }

    /**
     * Loads the queue of table operations from the local store
     *
     * @param store the local store
     * @return the queue of table operations
     * @throws java.text.ParseException
     * @throws MobileServiceLocalStoreException
     */
    public static OperationQueue load(MobileServiceLocalStore store) throws ParseException, MobileServiceLocalStoreException {
        OperationQueue opQueue = new OperationQueue(store);

        JsonElement operations = store.read(QueryOperations.tableName(OPERATION_QUEUE_TABLE).orderBy("__queueLoadedAt", QueryOrder.Ascending)
                .orderBy("sequence", QueryOrder.Ascending));

        if (operations.isJsonArray()) {
            JsonArray array = (JsonArray) operations;

            for (JsonElement element : array) {
                if (element.isJsonObject()) {
                    OperationQueueItem opQueueItem = deserialize((JsonObject) element);
                    opQueue.mQueue.add(opQueueItem);

                    // '/' is a reserved character that cannot be used on string
                    // ids.
                    // We use it to build a unique compound string from
                    // tableName and itemId
                    String tableItemId = opQueueItem.getTableName() + "/" + opQueueItem.getItemId();

                    opQueue.mIdOperationMap.put(tableItemId, opQueueItem);

                    Integer tableCount = opQueue.mTableCountMap.get(opQueueItem.getTableName());

                    if (tableCount != null) {
                        opQueue.mTableCountMap.put(opQueueItem.getTableName(), tableCount + 1);
                    } else {
                        opQueue.mTableCountMap.put(opQueueItem.getTableName(), 1);
                    }
                }
            }
        }

        return opQueue;
    }

    private static JsonObject serialize(OperationQueueItem opQueueItem) throws ParseException {
        JsonObject element = new JsonObject();

        element.addProperty("id", opQueueItem.getId());
        element.addProperty("kind", opQueueItem.getKind().getValue());
        element.addProperty("tablename", opQueueItem.getTableName());
        element.addProperty("itemid", opQueueItem.getItemId());
        element.addProperty("__createdat", DateSerializer.serialize(opQueueItem.getCreatedAt()));
        element.addProperty("__queueloadedat", DateSerializer.serialize(opQueueItem.getQueueLoadedAt()));
        element.addProperty("sequence", opQueueItem.getSequence());

        return element;
    }

    private static OperationQueueItem deserialize(JsonObject element) throws ParseException {
        String id = element.get("id").getAsString();
        int kind = element.get("kind").getAsInt();
        String tableName = element.get("tablename").getAsString();
        String itemId = element.get("itemid").getAsString();
        Date createdAt = DateSerializer.deserialize(element.get("__createdat").getAsString());
        Date queueLoadedAt = DateSerializer.deserialize(element.get("__queueloadedat").getAsString());
        long sequence = element.get("sequence").getAsLong();

        TableOperation operation = null;
        switch (kind) {
            case 0:
                operation = InsertOperation.create(id, tableName, itemId, createdAt);
                break;
            case 1:
                operation = UpdateOperation.create(id, tableName, itemId, createdAt);
                break;
            case 2:
                operation = DeleteOperation.create(id, tableName, itemId, createdAt);
                break;
        }

        return new OperationQueueItem(operation, queueLoadedAt, sequence);
    }

    /**
     * Enqueue a new table operation
     *
     * @param operation the table operation
     * @throws Throwable
     */
    public void enqueue(TableOperation operation) throws Throwable {
        this.mSyncLock.writeLock().lock();

        try {
            // '/' is a reserved character that cannot be used on string ids.
            // We use it to build a unique compound string from tableName and
            // itemId
            String tableItemId = operation.getTableName() + "/" + operation.getItemId();

            if (this.mIdOperationMap.containsKey(tableItemId)) {
                OperationQueueItem prevOpQueueItem = this.mIdOperationMap.get(tableItemId);
                TableOperation prevOperation = prevOpQueueItem.getOperation();
                TableOperation collapsedOperation = prevOperation.accept(new TableOperationCollapser(operation));

                if (collapsedOperation == null || collapsedOperation == operation) {
                    prevOpQueueItem.cancel();

                    removeOperationQueueItem(prevOpQueueItem);

                    if (collapsedOperation == operation) {
                        enqueueOperation(operation);
                    }
                }

                dequeueCancelledOperations();
            } else {
                enqueueOperation(operation);
            }
        } finally {
            this.mSyncLock.writeLock().unlock();
        }
    }

    /**
     * Dequeue the next table operation
     *
     * @return the table operation
     * @throws java.text.ParseException
     * @throws MobileServiceLocalStoreException
     */
    public TableOperation dequeue() throws ParseException, MobileServiceLocalStoreException {
        this.mSyncLock.writeLock().lock();

        try {
            TableOperation result = null;
            OperationQueueItem opQueueItem = this.mQueue.peek();

            if (opQueueItem != null) {
                result = dequeueOperation(opQueueItem);
            }

            return result;
        } finally {
            this.mSyncLock.writeLock().unlock();
        }
    }

    /**
     * Peek the next table operation
     *
     * @return the table operation
     */
    public TableOperation peek() {
        this.mSyncLock.readLock().lock();

        try {
            return this.mQueue.peek() != null ? this.mQueue.peek().getOperation() : null;
        } finally {
            this.mSyncLock.readLock().unlock();
        }
    }

    /**
     * Returns the count of pending table operation
     */
    public int countPending() {
        this.mSyncLock.readLock().lock();

        try {
            return this.mIdOperationMap.size();
        } finally {
            this.mSyncLock.readLock().unlock();
        }
    }

    /**
     * Returns the count of pending table operation for a specific table
     *
     * @param tableName the table name
     * @return the count of operations
     */
    public int countPending(String tableName) {
        this.mSyncLock.readLock().lock();

        try {
            return this.mTableCountMap.get(tableName) != null ? this.mTableCountMap.get(tableName) : 0;
        } finally {
            this.mSyncLock.readLock().unlock();
        }
    }

    /**
     * Adds a new push sync bookmark
     *
     * @return the bookmark
     */
    public Bookmark bookmark() {
        this.mSyncLock.writeLock().lock();

        try {
            BookmarkQueueItem bookmarkQueueItem = new BookmarkQueueItem(this.mLoadedAt, this.mSequence);
            this.mBookmarkQueue.add(bookmarkQueueItem);
            return new Bookmark(this, bookmarkQueueItem);
        } finally {
            this.mSyncLock.writeLock().unlock();
        }
    }

    /**
     * Remove a push sync bookmark
     *
     * @param bookmark the push sync bookmark
     */
    public void unbookmark(Bookmark bookmark) {
        this.mSyncLock.writeLock().lock();

        try {
            bookmark.mBookmarkQueueItem.mCancelled = true;

            dequeueCancelledBookmarks();
        } finally {
            this.mSyncLock.writeLock().unlock();
        }
    }

    private void enqueueOperation(TableOperation operation) throws ParseException, MobileServiceLocalStoreException {
        OperationQueueItem opQueueItem = new OperationQueueItem(operation, this.mLoadedAt, this.mSequence++);

        this.mStore.upsert(OPERATION_QUEUE_TABLE, serialize(opQueueItem));

        this.mQueue.add(opQueueItem);

        // '/' is a reserved character that cannot be used on string ids.
        // We use it to build a unique compound string from tableName and
        // itemId
        String tableItemId = operation.getTableName() + "/" + operation.getItemId();

        this.mIdOperationMap.put(tableItemId, opQueueItem);

        Integer tableCount = this.mTableCountMap.get(operation.getTableName());

        if (tableCount != null) {
            this.mTableCountMap.put(operation.getTableName(), tableCount + 1);
        } else {
            this.mTableCountMap.put(operation.getTableName(), 1);
        }
    }

    private TableOperation dequeueOperation(OperationQueueItem opQueueItem) throws MobileServiceLocalStoreException {
        this.mQueue.poll();

        removeOperationQueueItem(opQueueItem);
        dequeueCancelledOperations();

        return opQueueItem.getOperation();
    }

    private void removeOperationQueueItem(OperationQueueItem opQueueItem) throws MobileServiceLocalStoreException {
        // '/' is a reserved character that cannot be used on string ids.
        // We use it to build a unique compound string from tableName and
        // itemId
        String tableItemId = opQueueItem.getTableName() + "/" + opQueueItem.getItemId();

        this.mIdOperationMap.remove(tableItemId);

        Integer tableCount = this.mTableCountMap.get(opQueueItem.getTableName());

        if (tableCount != null && tableCount > 1) {
            this.mTableCountMap.put(opQueueItem.getTableName(), tableCount - 1);
        } else {
            this.mTableCountMap.remove(opQueueItem.getTableName());
        }

        this.mStore.delete(OPERATION_QUEUE_TABLE, opQueueItem.getId());
    }

    private void dequeueCancelledOperations() {
        while (this.mQueue.peek() != null && this.mQueue.peek().isCancelled()) {
            this.mQueue.poll();
        }
    }

    private void dequeueCancelledBookmarks() {
        while (this.mBookmarkQueue.peek() != null && this.mBookmarkQueue.peek().mCancelled) {
            this.mBookmarkQueue.poll();
        }
    }

    private TableOperation dequeueBookmarked(BookmarkQueueItem bookmarkQueueItem) throws MobileServiceLocalStoreException {
        this.mSyncLock.writeLock().lock();

        try {
            if (bookmarkQueueItem.mCancelled) {
                throw new IllegalStateException("The bookmark has been cancelled.");
            } else if (!isCurrentBookmark(bookmarkQueueItem)) {
                throw new IllegalStateException("There are other pending bookmarks to be processed.");
            } else {
                TableOperation result = null;
                OperationQueueItem opQueueItem = this.mQueue.peek();

                if (verifyBookmarkedOperation(bookmarkQueueItem, opQueueItem)) {
                    result = dequeueOperation(opQueueItem);
                }

                return result;
            }
        } finally {
            this.mSyncLock.writeLock().unlock();
        }
    }

    private TableOperation peekBookmarked(BookmarkQueueItem bookmarkQueueItem) {
        this.mSyncLock.readLock().lock();

        try {
            if (bookmarkQueueItem.mCancelled) {
                throw new IllegalStateException("The bookmark has been cancelled.");
            } else if (!isCurrentBookmark(bookmarkQueueItem)) {
                throw new IllegalStateException("There are other pending bookmarks to be processed.");
            } else {
                TableOperation result = null;
                OperationQueueItem opQueueItem = this.mQueue.peek();

                if (verifyBookmarkedOperation(bookmarkQueueItem, opQueueItem)) {
                    result = opQueueItem.getOperation();
                }

                return result;
            }
        } finally {
            this.mSyncLock.readLock().unlock();
        }
    }

    private boolean isCurrentBookmark(BookmarkQueueItem bookmarkQueueItem) {
        this.mSyncLock.readLock().lock();

        try {
            return this.mBookmarkQueue.peek() == bookmarkQueueItem;
        } finally {
            this.mSyncLock.readLock().unlock();
        }
    }

    private boolean verifyBookmarkedOperation(BookmarkQueueItem bookmarkQueueItem, OperationQueueItem opQueueItem) {
        return bookmarkQueueItem != null
                && opQueueItem != null
                && (opQueueItem.getQueueLoadedAt().before(bookmarkQueueItem.mQueueLoadedAt) || (opQueueItem.getQueueLoadedAt().equals(
                bookmarkQueueItem.mQueueLoadedAt) && opQueueItem.getSequence() < bookmarkQueueItem.mSequence));
    }

    private static class OperationQueueItem implements TableOperation {
        private TableOperation mOperation;
        private Date mQueueLoadedAt;
        private long mSequence;
        private boolean mCancelled;

        private OperationQueueItem(TableOperation operation, Date queueLoadedAt, long sequence) {
            this.mOperation = operation;
            this.mQueueLoadedAt = queueLoadedAt;
            this.mSequence = sequence;
            this.mCancelled = false;
        }

        @Override
        public String getId() {
            return this.mOperation.getId();
        }

        @Override
        public TableOperationKind getKind() {
            return this.mOperation.getKind();
        }

        @Override
        public String getTableName() {
            return this.mOperation.getTableName();
        }

        @Override
        public String getItemId() {
            return this.mOperation.getItemId();
        }

        @Override
        public Date getCreatedAt() {
            return this.mOperation.getCreatedAt();
        }

        private TableOperation getOperation() {
            return this.mOperation;
        }

        private Date getQueueLoadedAt() {
            return this.mQueueLoadedAt;
        }

        private long getSequence() {
            return this.mSequence;
        }

        private boolean isCancelled() {
            return this.mCancelled;
        }

        private void cancel() {
            this.mCancelled = true;
        }

        @Override
        public <T> T accept(TableOperationVisitor<T> visitor) throws Throwable {
            return this.mOperation.accept(visitor);
        }
    }

    private static class BookmarkQueueItem {
        private Date mQueueLoadedAt;
        private long mSequence;
        private boolean mCancelled;

        private BookmarkQueueItem(Date queueLoadedAt, long sequence) {
            this.mQueueLoadedAt = queueLoadedAt;
            this.mSequence = sequence;
            this.mCancelled = false;
        }
    }

    /**
     * Class that represents a push sync bookmark, and table operations within
     * it
     */
    public static class Bookmark {
        private OperationQueue mOpQueue;
        private BookmarkQueueItem mBookmarkQueueItem;

        private Bookmark(OperationQueue opQueue, BookmarkQueueItem bookmarkQueueItem) {
            this.mOpQueue = opQueue;
            this.mBookmarkQueueItem = bookmarkQueueItem;
        }

        /**
         * Dequeue the next bookmarked table operation
         *
         * @return the table operation
         * @throws MobileServiceLocalStoreException
         */
        public TableOperation dequeue() throws MobileServiceLocalStoreException {
            return this.mOpQueue.dequeueBookmarked(this.mBookmarkQueueItem);
        }

        /**
         * Peek the next bookmarked table operation
         *
         * @return the table operation
         */
        public TableOperation peek() {
            return this.mOpQueue.peekBookmarked(this.mBookmarkQueueItem);
        }

        /**
         * Returns true if the bookmark is the first and current in the queue
         */
        public boolean isCurrentBookmark() {
            return this.mOpQueue.isCurrentBookmark(this.mBookmarkQueueItem);
        }

        /**
         * Returns true if the bookmark is canceled
         */
        public boolean isCancelled() {
            return this.mBookmarkQueueItem.mCancelled;
        }
    }
}