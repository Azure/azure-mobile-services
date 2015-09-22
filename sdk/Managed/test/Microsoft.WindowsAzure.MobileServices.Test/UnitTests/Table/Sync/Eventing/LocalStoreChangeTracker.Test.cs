using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Eventing;
using Microsoft.WindowsAzure.MobileServices.Query;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Microsoft.WindowsAzure.MobileServices.Test.Mocks;
using Microsoft.WindowsAzure.MobileServices.TestFramework;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Test.UnitTests.Table.Sync.Eventing
{
    [Tag("unit")]
    [Tag("eventing")]
    [Tag("synceventing")]
    public class LocalStoreChangeTrackerTests : TestBase
    {
        [TestMethod]
        public void Constructor_Throws_WhenTrackingOptionsAreInvalid()
        {
            var store = new MobileServiceLocalStoreMock();
            var trackingContext = new StoreTrackingContext(StoreOperationSource.Local, string.Empty, StoreTrackingOptions.None);
            var eventManager = new MobileServiceEventManager();
            var settings = new MobileServiceSyncSettingsManager(store);

            AssertEx.Throws<InvalidOperationException>(() => new LocalStoreChangeTracker(store, trackingContext, eventManager, settings));
        }


        [TestMethod]
        public void Disposing_CompletesBatch()
        {
            var store = new MobileServiceLocalStoreMock();
            var trackingContext = new StoreTrackingContext(StoreOperationSource.ServerPull, string.Empty);
            var eventManager = new MobileServiceEventManagerMock<IMobileServiceEvent>();
            var settings = new MobileServiceSyncSettingsManager(store);

            StoreOperationsBatchCompletedEvent batchEvent = null;
            eventManager.PublishAsyncFunc = e =>
            {
                batchEvent = e as StoreOperationsBatchCompletedEvent;
                return Task.FromResult(0);
            };

            var changeTracker = new LocalStoreChangeTracker(store, trackingContext, eventManager, settings);
            changeTracker.Dispose();

            Assert.IsNotNull(batchEvent);
        }

        [AsyncTestMethod]
        public async Task BatchNotification_ReportsOperationCount()
        {
            var store = new MobileServiceLocalStoreMock();
            var trackingContext = new StoreTrackingContext(StoreOperationSource.ServerPull, string.Empty);
            var eventManager = new MobileServiceEventManagerMock<IMobileServiceEvent>();
            var settings = new MobileServiceSyncSettingsManager(store);
            var changeTracker = new LocalStoreChangeTracker(store, trackingContext, eventManager, settings);

            EnqueueSimpleObjectResponse(store, "123", "XXX", "789");

            await changeTracker.UpsertAsync("test", new JObject() { { "id", "123" }, { "__version", "2" } }, true); // Update
            await changeTracker.UpsertAsync("test", new JObject() { { "id", "456" }, { "__version", "2" } }, true); // Insert
            await changeTracker.DeleteAsync("test", "789"); // Delete

            StoreOperationsBatchCompletedEvent batchEvent = null;
            eventManager.PublishAsyncFunc = e =>
            {
                batchEvent = e as StoreOperationsBatchCompletedEvent;
                return Task.FromResult(0);
            };

            changeTracker.Dispose();

            Assert.IsNotNull(batchEvent);
            Assert.AreEqual(batchEvent.Batch.OperationCount, 3);
            Assert.AreEqual(batchEvent.Batch.GetOperationCountByKind(LocalStoreOperationKind.Update), 1);
            Assert.AreEqual(batchEvent.Batch.GetOperationCountByKind(LocalStoreOperationKind.Insert), 1);
            Assert.AreEqual(batchEvent.Batch.GetOperationCountByKind(LocalStoreOperationKind.Delete), 1);
        }

        [AsyncTestMethod]
        public async Task UpsertAsync_SuppressesUpdateNotifications_WhenServerChangesMatchesLocalRecordVersion()
        {
            var operationSources = new[] { StoreOperationSource.ServerPull, StoreOperationSource.ServerPush};

            await AssertNotificationResultWithMatchingLocalRecordVersion(operationSources, false);
        }
        
        [AsyncTestMethod]
        public async Task UpsertAsync_DoesNotSuppressUpdateNotifications_WhenLocalChangesMatchesLocalRecordVersion()
        {
            await AssertNotificationResultWithMatchingLocalRecordVersion(new[] { StoreOperationSource.Local}, true);
        }

        [AsyncTestMethod]
        public async Task DeleteAsync_WithTableNameAndRecordIds_SendsNotification()
        {
            var store = new MobileServiceLocalStoreMock();
            var trackingContext = new StoreTrackingContext(StoreOperationSource.Local, string.Empty);
            var eventManager = new MobileServiceEventManagerMock<IMobileServiceEvent>();
            var settings = new MobileServiceSyncSettingsManager(store);
            var changeTracker = new LocalStoreChangeTracker(store, trackingContext, eventManager, settings);

            JObject item = EnqueueSimpleObjectResponse(store);

            StoreOperationCompletedEvent operationEvent = null;
            eventManager.PublishAsyncFunc = t =>
            {
                operationEvent = t as StoreOperationCompletedEvent;
                return Task.FromResult(0);
            };

            await changeTracker.DeleteAsync("test", "123");

            Assert.IsNotNull(operationEvent);
            Assert.AreEqual(operationEvent.Operation.Kind, LocalStoreOperationKind.Delete);
            Assert.AreEqual(operationEvent.Operation.RecordId, "123");
            Assert.AreEqual(operationEvent.Operation.TableName, "test");
        }

        [AsyncTestMethod]
        public async Task DeleteAsync_WithQuery_SendsNotification()
        {
            var store = new MobileServiceLocalStoreMock();
            var trackingContext = new StoreTrackingContext(StoreOperationSource.Local, string.Empty);
            var eventManager = new MobileServiceEventManagerMock<IMobileServiceEvent>();
            var settings = new MobileServiceSyncSettingsManager(store);
            var changeTracker = new LocalStoreChangeTracker(store, trackingContext, eventManager, settings);

            JObject item = EnqueueSimpleObjectResponse(store);

            StoreOperationCompletedEvent operationEvent = null;
            eventManager.PublishAsyncFunc = t =>
            {
                operationEvent = t as StoreOperationCompletedEvent;
                return Task.FromResult(0);
            };

            MobileServiceTableQueryDescription query = new MobileServiceTableQueryDescription("test");
            query.Filter = new BinaryOperatorNode(BinaryOperatorKind.Equal, new MemberAccessNode(null, MobileServiceSystemColumns.Id), new ConstantNode("123"));

            await changeTracker.DeleteAsync(query);

            Assert.IsNotNull(operationEvent);
            Assert.AreEqual(operationEvent.Operation.Kind, LocalStoreOperationKind.Delete);
            Assert.AreEqual(operationEvent.Operation.RecordId, "123");
            Assert.AreEqual(operationEvent.Operation.TableName, "test");
        }

        private async Task AssertNotificationResultWithMatchingLocalRecordVersion(StoreOperationSource[] operationSources, bool shouldNotify)
        {
            foreach (var operationSource in operationSources)
            {
                var store = new MobileServiceLocalStoreMock();
                var trackingContext = new StoreTrackingContext(operationSource, string.Empty);
                var eventManager = new MobileServiceEventManagerMock<IMobileServiceEvent>();
                var settings = new MobileServiceSyncSettingsManager(store);
                var changeTracker = new LocalStoreChangeTracker(store, trackingContext, eventManager, settings);

                JObject item = EnqueueSimpleObjectResponse(store);

                bool notificationSent = false;
                eventManager.PublishAsyncFunc = t =>
                {
                    notificationSent = true;
                    return Task.FromResult(0);
                };

                await changeTracker.UpsertAsync("test", item, true);

                Assert.AreEqual(notificationSent, shouldNotify, string.Format("Incorrect notification result with source {0}", operationSource));
            }
        }

        private JObject EnqueueSimpleObjectResponse(MobileServiceLocalStoreMock store)
        {
            return EnqueueSimpleObjectResponse(store, "123").First();
        }

        private IEnumerable<JObject> EnqueueSimpleObjectResponse(MobileServiceLocalStoreMock store, params string[] ids)
        {
            var results = new List<JObject>();
            foreach (var id in ids)
            {
                var item = new JObject() { { "id", id }, { "__version", "1" } };
                store.ReadResponses.Enqueue(string.Format("[{0}]", item.ToString()));

                results.Add(item);
            }
            
            return results;
        }
    }
}
