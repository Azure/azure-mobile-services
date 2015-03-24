// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
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

        public async virtual Task<MobileServiceTableOperation> PeekAsync(long prevSequenceId, MobileServiceTableKind tableKind, IEnumerable<string> tableNames)
        {
            MobileServiceTableQueryDescription query = CreateQuery();

            var tableKindNode = Compare(BinaryOperatorKind.Equal, "tableKind", (int)tableKind);
            var sequenceNode = Compare(BinaryOperatorKind.GreaterThan, "sequence", prevSequenceId);

            query.Filter = new BinaryOperatorNode(BinaryOperatorKind.And, tableKindNode, sequenceNode);

            if (tableNames != null && tableNames.Any())
            {
                BinaryOperatorNode nameInList = tableNames.Select(t => Compare(BinaryOperatorKind.Equal, "tableName", t))
                                                          .Aggregate((first, second) => new BinaryOperatorNode(BinaryOperatorKind.Or, first, second));
                query.Filter = new BinaryOperatorNode(BinaryOperatorKind.And, query.Filter, nameInList);
            }

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

        internal void UpdateOperationCount(long delta)
        {
            long current, updated;
            do
            {
                current = this.pendingOperations;
                updated = current + delta;
            }
            while (current != Interlocked.CompareExchange(ref this.pendingOperations, updated, current));
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

        public async Task<MobileServiceTableOperation> GetOperationByItemIdAsync(string tableName, string itemId)
        {
            MobileServiceTableQueryDescription query = CreateQuery();
            query.Filter = new BinaryOperatorNode(BinaryOperatorKind.And,
                                Compare(BinaryOperatorKind.Equal, "tableName", tableName),
                                Compare(BinaryOperatorKind.Equal, "itemId", itemId));
            JObject op = await this.store.FirstOrDefault(query);
            return MobileServiceTableOperation.Deserialize(op);
        }

        public async Task<MobileServiceTableOperation> GetOperationAsync(string id)
        {
            JObject op = await this.store.LookupAsync(MobileServiceLocalSystemTables.OperationQueue, id);
            if (op == null)
            {
                return null;
            }
            return MobileServiceTableOperation.Deserialize(op);
        }

        public async Task EnqueueAsync(MobileServiceTableOperation op)
        {
            op.Sequence = Interlocked.Increment(ref this.sequenceId);
            await this.store.UpsertAsync(MobileServiceLocalSystemTables.OperationQueue, op.Serialize(), fromServer: false);
            Interlocked.Increment(ref this.pendingOperations);
        }

        public virtual async Task<bool> DeleteAsync(string id, long version)
        {
            try
            {
                MobileServiceTableOperation op = await GetOperationAsync(id);
                if (op == null || op.Version != version)
                {
                    return false;
                }

                await this.store.DeleteAsync(MobileServiceLocalSystemTables.OperationQueue, id);
                Interlocked.Decrement(ref this.pendingOperations);
                return true;
            }
            catch (Exception ex)
            {
                throw new MobileServiceLocalStoreException("Failed to delete operation from the local store.", ex);
            }
        }

        public virtual async Task UpdateAsync(MobileServiceTableOperation op)
        {
            try
            {
                await this.store.UpsertAsync(MobileServiceLocalSystemTables.OperationQueue, op.Serialize(), fromServer: false);
            }
            catch (Exception ex)
            {
                throw new MobileServiceLocalStoreException("Failed to update operation in the local store.", ex);
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
            opQueue.sequenceId = result.Values == null ? 0 : result.Values.Select(v => v.Value<long>("sequence")).FirstOrDefault();

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
