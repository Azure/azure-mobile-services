// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Query;
using Microsoft.WindowsAzure.MobileServices.Threading;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    /// <summary>
    /// Queue of all operations i.e. Push, Pull, Insert, Update, Delete
    /// </summary>
    internal sealed class OperationQueue: IDisposable
    {
        private readonly AsyncLockDictionary tableLocks = new AsyncLockDictionary();
        private readonly AsyncLockDictionary itemLocks = new AsyncLockDictionary();
        private readonly Dictionary<string, MobileServiceTableOperation> idOpMap = new Dictionary<string, MobileServiceTableOperation>();        
        private readonly AsyncLock storeQueueLock = new AsyncLock();
        private readonly IMobileServiceLocalStore store;
        private Queue<MobileServiceTableOperation> queue;
        private long sequenceId;

        public OperationQueue(IMobileServiceLocalStore store)
        {
            this.queue = new Queue<MobileServiceTableOperation>();
            this.store = store;
        }

        public MobileServiceTableOperation Peek()
        {
            lock (this.queue)
            {
                return this.queue.Peek();
            }
        }

        public int CountPending()
        {
            lock (this.queue)
            {
                int count = this.queue.Count(o => !(o is BookmarkOperation || o.IsCancelled));
                return count;
            }
        }

        public int CountPending(string tableName)
        {
            lock (this.queue)
            {
                int count = this.queue.Count(o => o.TableName == tableName);
                return count;
            }
        }

        public async Task<MobileServiceTableOperation> DequeueAsync()
        {
            using (await this.storeQueueLock.Acquire(CancellationToken.None))
            {
                MobileServiceTableOperation op;
                lock (this.queue)
                {
                    op = this.queue.Dequeue();
                    if (!(op is BookmarkOperation))
                    {
                        this.idOpMap.Remove(op.ItemId);
                    }
                }
                try
                {
                    await this.store.DeleteAsync(LocalSystemTables.OperationQueue, op.Id);
                }
                catch (Exception ex)
                {
                    throw new MobileServiceLocalStoreException(Resources.SyncStore_FailedToDeleteOperation, ex);
                }
                return op;
            }
        }

        public Task<IDisposable> LockTableAsync(string name, CancellationToken cancellationToken)
        {
            return this.tableLocks.Acquire(name, cancellationToken);
        }

        public Task<IDisposable> LockItemAsync(string id, CancellationToken cancellationToken)
        {
            return this.itemLocks.Acquire(id, cancellationToken);
        }

        public bool TryGetOperation(string id, out MobileServiceTableOperation op)
        {
            lock (this.queue)
            {
                return this.idOpMap.TryGetValue(id, out op);
            }
        }

        public async Task EnqueueAsync(MobileServiceTableOperation op)
        {
            using (await this.storeQueueLock.Acquire(CancellationToken.None))
            {
                bool isBookmark = op is BookmarkOperation;
                if (!isBookmark)
                {
                    op.Sequence = this.sequenceId++;
                    await this.store.UpsertAsync(LocalSystemTables.OperationQueue, op.Serialize());
                }

                lock (this.queue)
                {
                    this.queue.Enqueue(op);
                    if (!isBookmark)
                    {
                        this.idOpMap[op.ItemId] = op;
                    }
                }
            }
        }

        public async Task DeleteAsync(MobileServiceTableOperation op)
        {
            await this.store.DeleteAsync(LocalSystemTables.OperationQueue, op.Id);
        }

        public static async Task<OperationQueue> LoadAsync(IMobileServiceLocalStore store, MobileServiceClient client)
        {
            var opQueue = new OperationQueue(store);

            JToken result = await store.ReadAsync(new MobileServiceTableQueryDescription(LocalSystemTables.OperationQueue));
            if (result is JArray)
            {
                var unorderedOps = new List<MobileServiceTableOperation>();

                var ops = (JArray)result;
                foreach (JObject obj in ops)
                {
                    var op = MobileServiceTableOperation.Deserialize(obj);
                    op.Table = client.GetTable(op.TableName) as MobileServiceTable;
                    op.Table.SystemProperties = MobileServiceSystemProperties.Version;
                    unorderedOps.Add(op);
                }

                IOrderedEnumerable<MobileServiceTableOperation> orderedOps = unorderedOps.OrderBy(o => o.CreatedAt)
                                                                                         .ThenBy(o => o.Sequence);
                opQueue.queue = new Queue<MobileServiceTableOperation>(orderedOps);
            }

            return opQueue;
        }

        public void Dispose()
        {
            this.storeQueueLock.Dispose();
        }
    }
}
