using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Eventing;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Microsoft.WindowsAzure.MobileServices.TestFramework;

namespace Microsoft.WindowsAzure.MobileServices.Test.UnitTests.Table.Sync.Eventing
{
    [Tag("unit")]
    [Tag("eventing")]
    [Tag("synceventing")]
    public class StoreChangeTrackerFactoryTest : TestBase
    {
        [TestMethod]
        public void CreateTrackedStore_ReturnsUntrackedProxyForLocalSource_WhenNoNotificationsAreEnabledForSource()
        {
            StoreTrackingOptions trackingOptions = StoreTrackingOptions.AllNotificationsAndChangeDetection & ~StoreTrackingOptions.NotifyLocalRecordOperations;

            AssertUntrackedStoreForSourceWithOptions(StoreOperationSource.Local, trackingOptions);
        }

        [TestMethod]
        public void CreateTrackedStore_ReturnsUntrackedProxyForServerPullSource_WhenNoNotificationsAreEnabledForSource()
        {
            StoreTrackingOptions trackingOptions = StoreTrackingOptions.AllNotificationsAndChangeDetection & ~(StoreTrackingOptions.NotifyServerPullBatch | StoreTrackingOptions.NotifyServerPullRecordOperations);

            AssertUntrackedStoreForSourceWithOptions(StoreOperationSource.ServerPull, trackingOptions);
        }

        [TestMethod]
        public void CreateTrackedStore_ReturnsUntrackedProxyForServerPushSource_WhenNoNotificationsAreEnabledForSource()
        {
            StoreTrackingOptions trackingOptions = StoreTrackingOptions.AllNotificationsAndChangeDetection & ~(StoreTrackingOptions.NotifyServerPushBatch| StoreTrackingOptions.NotifyServerPushRecordOperations);

            AssertUntrackedStoreForSourceWithOptions(StoreOperationSource.ServerPush, trackingOptions);
        }

        private void AssertUntrackedStoreForSourceWithOptions(StoreOperationSource source, StoreTrackingOptions trackingOptions)
        {
            var store = new MobileServiceLocalStoreMock();
            var trackingContext = new StoreTrackingContext(StoreOperationSource.Local, string.Empty, StoreTrackingOptions.None);
            var eventManager = new MobileServiceEventManager();
            var settings = new MobileServiceSyncSettingsManager(store);

            IMobileServiceLocalStore trackedStore = StoreChangeTrackerFactory.CreateTrackedStore(store, source, trackingOptions, eventManager, settings);

            Assert.IsNotNull(trackedStore);
            Assert.IsTrue(trackedStore is LocalStoreProxy);
        }
    }
}
