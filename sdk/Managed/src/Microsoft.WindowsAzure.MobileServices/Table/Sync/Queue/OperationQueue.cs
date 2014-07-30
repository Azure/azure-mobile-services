﻿// ----------------------------------------------------------------------------
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

        public async virtual Task<MobileServiceTableOperation> PeekAsync(long prevSequenceId)
        {
            MobileServiceTableQueryDescription query = CreateQuery();

            query.Filter = Compare(BinaryOperatorKind.GreaterThan, "sequence", prevSequenceId);
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

        public virtual async Task<long> CountPending(string tableName)
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

        public virtual async Task DeleteAsync(string id)
        {
            try
            {
                await this.store.DeleteAsync(MobileServiceLocalSystemTables.OperationQueue, id);
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

            var query = CreateQuery();
            // to know how many pending operations are there
            query.IncludeTotalCount = true;
            // to get the max sequence id, order by sequence desc
            query.Ordering.Add(new OrderByNode(new MemberAccessNode(null, "sequence"), OrderByDirection.Descending));
            // we just need the highest value, not all the operations
            query.Top = 1;

            QueryResult result = await store.QueryAsync(query);
            opQueue.pendingOperations = result.TotalCount;
            opQueue.sequenceId = result.Values == null ? 0: result.Values.Select(v=>v.Value<long>("sequence")).FirstOrDefault();

            return opQueue;
        }        

        private static MobileServiceTableQueryDescription CreateQuery()
        {
            var query = new MobileServiceTableQueryDescription(MobileServiceLocalSystemTables.OperationQueue);
            return query;
        }

        private static BinaryOperatorNode Compare(BinaryOperatorKind kind, string member, object value)
        {
            return new BinaryOperatorNode(kind, new MemberAccessNode(null, member), new ConstantNode(value));
        }
    }
}
