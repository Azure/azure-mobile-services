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
    internal class OperationQueue
    {
        private readonly AsyncLockDictionary tableLocks = new AsyncLockDictionary();
        private readonly AsyncLockDictionary itemLocks = new AsyncLockDictionary();
        private readonly IMobileServiceLocalStore store;
        private long sequenceId;
        private long pendingOperations;

        public OperationQueue(IMobileServiceLocalStore store)
        {
            this.store = store;
        }

        public async virtual Task<MobileServiceTableOperation> PeekAsync()
        {
            MobileServiceTableQueryDescription query = CreateQuery();
            query.Ordering.Add(new OrderByNode(new MemberAccessNode(null, "__createdAt"), OrderByDirection.Ascending));
            query.Ordering.Add(new OrderByNode(new MemberAccessNode(null, "sequence"), OrderByDirection.Ascending));
            query.Top = 1;

            JObject op = await this.store.FirstOrDefault(query);
            if (op == null)
            {
                return null;
            }

            return MobileServiceTableOperation.Deserialize(op);
        }

        public long PendingOperations 
        {
            get { return pendingOperations; }
        }

        public async Task<long> CountPending(string tableName)
        {
            MobileServiceTableQueryDescription query = CreateQuery();
            query.Filter = new BinaryOperatorNode(BinaryOperatorKind.Equal, new MemberAccessNode(null, "tableName"), new ConstantNode(tableName));
            return await this.store.CountAsync(query);
        }

        public virtual Task<IDisposable> LockTableAsync(string name, CancellationToken cancellationToken)
        {
            return this.tableLocks.Acquire(name, cancellationToken);
        }

        public Task<IDisposable> LockItemAsync(string id, CancellationToken cancellationToken)
        {
            return this.itemLocks.Acquire(id, cancellationToken);
        }

        public async Task<MobileServiceTableOperation> GetOperationAsync(string id)
        {
            MobileServiceTableQueryDescription query = CreateQuery();
            query.Filter = new BinaryOperatorNode(BinaryOperatorKind.Equal, new MemberAccessNode(null, "itemId"), new ConstantNode(id));
            JObject op = await this.store.FirstOrDefault(query);
            return MobileServiceTableOperation.Deserialize(op);
        }

        public async Task EnqueueAsync(MobileServiceTableOperation op)
        {
            op.Sequence = Interlocked.Increment(ref this.sequenceId);

            await this.store.UpsertAsync(MobileServiceLocalSystemTables.OperationQueue, op.Serialize(), fromServer: false);
            Interlocked.Increment(ref this.pendingOperations);
        }

        public virtual async Task DeleteAsync(MobileServiceTableOperation op)
        {
            try
            {
                await this.store.DeleteAsync(MobileServiceLocalSystemTables.OperationQueue, op.Id);
                Interlocked.Decrement(ref this.pendingOperations);
            }
            catch (Exception ex)
            {
                throw new MobileServiceLocalStoreException(Resources.SyncStore_FailedToDeleteOperation, ex);
            }            
        }

        public static async Task<OperationQueue> LoadAsync(IMobileServiceLocalStore store)
        {
            var opQueue = new OperationQueue(store);

            opQueue.pendingOperations = await store.CountAsync(MobileServiceLocalSystemTables.OperationQueue);

            return opQueue;
        }        

        private static MobileServiceTableQueryDescription CreateQuery()
        {
            var query = new MobileServiceTableQueryDescription(MobileServiceLocalSystemTables.OperationQueue);
            return query;
        }
    }
}
