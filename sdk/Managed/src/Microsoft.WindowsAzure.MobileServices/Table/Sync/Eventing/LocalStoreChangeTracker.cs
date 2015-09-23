// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Eventing;
using Microsoft.WindowsAzure.MobileServices.Query;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    internal sealed class LocalStoreChangeTracker : IMobileServiceLocalStore
    {
        private readonly IMobileServiceLocalStore store;
        private readonly StoreTrackingContext trackingContext;
        private readonly MobileServiceObjectReader objectReader;
        private StoreOperationsBatch operationsBatch;
        private readonly IMobileServiceEventManager eventManager;
        private int isBatchCompleted = 0;
        private readonly MobileServiceSyncSettingsManager settings;
        private bool trackRecordOperations;
        private bool trackBatches;

        public LocalStoreChangeTracker(IMobileServiceLocalStore store, StoreTrackingContext trackingContext, IMobileServiceEventManager eventManager, MobileServiceSyncSettingsManager settings)
        {
            if (store == null)
            {
                throw new ArgumentNullException("store");
            }

            if (trackingContext == null)
            {
                throw new ArgumentNullException("trackingContext");
            }

            if (eventManager == null)
            {
                throw new ArgumentNullException("eventManager");
            }

            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            this.objectReader = new MobileServiceObjectReader();

            this.store = store;
            this.trackingContext = trackingContext;
            this.eventManager = eventManager;
            this.settings = settings;

            InitializeTracking();
        }

        private void InitializeTracking()
        {
            this.trackRecordOperations = IsRecordTrackingEnabled();
            this.trackBatches = IsBatchTrackingEnabled();

            if (!this.trackRecordOperations & !this.trackBatches)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                    "Tracking notifications are not enabled for the source {0}. To use a change tracker, you must enable record operation notifications, batch notifications or both.",
                    this.trackingContext.Source));
            }

            if (this.trackBatches)
            {
                this.operationsBatch = new StoreOperationsBatch(this.trackingContext.BatchId, this.trackingContext.Source);
            }
        }

        private bool IsBatchTrackingEnabled()
        {
            bool result = false;

            switch (this.trackingContext.Source)
            {
                case StoreOperationSource.Local:
                case StoreOperationSource.LocalPurge:
                case StoreOperationSource.LocalConflictResolution:
                    // No batch notifications for local operations
                    break;
                case StoreOperationSource.ServerPull:
                    result = this.trackingContext.TrackingOptions.HasFlag(StoreTrackingOptions.NotifyServerPullBatch);
                    break;
                case StoreOperationSource.ServerPush:
                    result = this.trackingContext.TrackingOptions.HasFlag(StoreTrackingOptions.NotifyServerPushBatch);
                    break;
                default:
                    throw new InvalidOperationException("Unknown tracking source");
            }

            return result;
        }

        private bool IsRecordTrackingEnabled()
        {
            bool result = false;

            switch (this.trackingContext.Source)
            {
                case StoreOperationSource.Local:
                case StoreOperationSource.LocalPurge:
                    result = this.trackingContext.TrackingOptions.HasFlag(StoreTrackingOptions.NotifyLocalOperations);
                    break;
                case StoreOperationSource.LocalConflictResolution:
                    result = this.trackingContext.TrackingOptions.HasFlag(StoreTrackingOptions.NotifyLocalConflictResolutionOperations);
                    break;
                case StoreOperationSource.ServerPull:
                    result = this.trackingContext.TrackingOptions.HasFlag(StoreTrackingOptions.NotifyServerPullOperations);
                    break;
                case StoreOperationSource.ServerPush:
                    result = this.trackingContext.TrackingOptions.HasFlag(StoreTrackingOptions.NotifyServerPushOperations);
                    break;
                default:
                    throw new InvalidOperationException("Unknown tracking source");
            }

            return result;
        }

        public async Task DeleteAsync(MobileServiceTableQueryDescription query)
        {
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }

            string[] recordIds = null;

            if (!query.TableName.StartsWith(MobileServiceLocalSystemTables.Prefix) && this.trackingContext.Source != StoreOperationSource.LocalPurge)
            {
                QueryResult result = await this.store.QueryAsync(query);
                recordIds = result.Values.Select(j => this.objectReader.GetId((JObject)j)).ToArray();
            }

            await this.store.DeleteAsync(query);

            if (recordIds != null)
            {
                foreach (var id in recordIds)
                {
                    TrackStoreOperation(query.TableName, id, LocalStoreOperationKind.Delete);
                }
            }
        }

        public async Task DeleteAsync(string tableName, IEnumerable<string> ids)
        {
            if (tableName == null)
            {
                throw new ArgumentNullException("tableName");
            }

            if (ids == null)
            {
                throw new ArgumentNullException("ids");
            }

            if (!tableName.StartsWith(MobileServiceLocalSystemTables.Prefix))
            {
                IEnumerable<string> notificationIds = ids;

                if (this.trackingContext.TrackingOptions.HasFlag(StoreTrackingOptions.DetectRecordChanges))
                {
                    IDictionary<string, string> existingRecords = await GetItemsAsync(tableName, ids, false);
                    notificationIds = existingRecords.Select(kvp => kvp.Key);
                }

                await this.store.DeleteAsync(tableName, ids);

                foreach (var id in notificationIds)
                {
                    TrackStoreOperation(tableName, id, LocalStoreOperationKind.Delete);
                }
            }
            else
            {
                await this.store.DeleteAsync(tableName, ids);
            }
        }

        public Task<JObject> LookupAsync(string tableName, string id)
        {
            if (tableName == null)
            {
                throw new ArgumentNullException("tableName");
            }

            if (id == null)
            {
                throw new ArgumentNullException("id");
            }

            return this.store.LookupAsync(tableName, id);
        }

        public Task<JToken> ReadAsync(MobileServiceTableQueryDescription query)
        {
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }

            return this.store.ReadAsync(query);
        }

        public async Task UpsertAsync(string tableName, IEnumerable<JObject> items, bool ignoreMissingColumns)
        {
            if (tableName == null)
            {
                throw new ArgumentNullException("tableName");
            }

            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            if (!tableName.StartsWith(MobileServiceLocalSystemTables.Prefix))
            {
                IDictionary<string, string> existingRecords = null;
                bool analyzeUpserts = this.trackingContext.TrackingOptions.HasFlag(StoreTrackingOptions.DetectInsertsAndUpdates);
                bool supportsVersion = false;

                if (analyzeUpserts)
                {
                    MobileServiceSystemProperties systemProperties = await this.settings.GetSystemPropertiesAsync(tableName);
                    supportsVersion = systemProperties.HasFlag(MobileServiceSystemProperties.Version);

                    existingRecords = await GetItemsAsync(tableName, items.Select(i => this.objectReader.GetId(i)), supportsVersion);
                }

                await this.store.UpsertAsync(tableName, items, ignoreMissingColumns);

                foreach (var item in items)
                {
                    string itemId = this.objectReader.GetId(item);
                    LocalStoreOperationKind operationKind = LocalStoreOperationKind.Upsert;

                    if (analyzeUpserts)
                    {
                        if (existingRecords.ContainsKey(itemId))
                        {
                            operationKind = LocalStoreOperationKind.Update;

                            // If the update isn't a result of a local operation, check if the item exposes a version property
                            // and if we truly have a new version (an actual change) before tracking the change. 
                            // This avoids update notifications for records that haven't changed, which would usually happen as a result of a pull
                            // operation, because of the logic used to pull changes.
                            if (this.trackingContext.Source != StoreOperationSource.Local && supportsVersion
                                && string.Compare(existingRecords[itemId], item[MobileServiceSystemColumns.Version].ToString()) == 0)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            operationKind = LocalStoreOperationKind.Insert;
                        }
                    }

                    TrackStoreOperation(tableName, itemId, operationKind);
                }
            }
            else
            {
                await this.store.UpsertAsync(tableName, items, ignoreMissingColumns);
            }
        }

        public Task InitializeAsync()
        {
            return this.store.InitializeAsync();
        }

        private async Task<IDictionary<string, string>> GetItemsAsync(string tableName, IEnumerable<string> ids, bool includeVersion)
        {
            var query = new MobileServiceTableQueryDescription(tableName);
            BinaryOperatorNode idListFilter = ids.Select(t => new BinaryOperatorNode(BinaryOperatorKind.Equal, new MemberAccessNode(null, MobileServiceSystemColumns.Id), new ConstantNode(t)))
                                                 .Aggregate((aggregate, item) => new BinaryOperatorNode(BinaryOperatorKind.Or, aggregate, item));

            query.Filter = idListFilter;
            query.Selection.Add(MobileServiceSystemColumns.Id);

            if (includeVersion)
            {
                query.Selection.Add(MobileServiceSystemColumns.Version);
            }

            QueryResult result = await this.store.QueryAsync(query);

            return result.Values.ToDictionary(t => this.objectReader.GetId((JObject)t), rec => includeVersion ? rec[MobileServiceSystemColumns.Version].ToString() : null);
        }

        private void TrackStoreOperation(string tableName, string itemId, LocalStoreOperationKind operationKind)
        {
            var operation = new StoreOperation(tableName, itemId, operationKind, this.trackingContext.Source, this.trackingContext.BatchId);

            if (this.trackBatches)
            {
               this.operationsBatch.IncrementOperationCount(operationKind)
                   .ContinueWith(t => t.Exception.Handle(e => true), TaskContinuationOptions.OnlyOnFaulted);
            }

            if (this.trackRecordOperations)
            {
                this.eventManager.BackgroundPublish(new StoreOperationCompletedEvent(operation));
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                CompleteBatch();
            }
        }

        private void CompleteBatch()
        {
            if (Interlocked.Exchange(ref this.isBatchCompleted, 1) == 0)
            {
                if (this.trackBatches)
                {
                    this.eventManager.PublishAsync(new StoreOperationsBatchCompletedEvent(this.operationsBatch));
                }
            }
        }
    }
}
