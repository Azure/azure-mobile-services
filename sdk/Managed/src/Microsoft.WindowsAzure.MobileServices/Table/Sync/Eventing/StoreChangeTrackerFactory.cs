using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Eventing;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    internal static class StoreChangeTrackerFactory
    {
        internal static IMobileServiceLocalStore CreateTrackedStore(IMobileServiceLocalStore targetStore, StoreOperationSource source, StoreTrackingOptions trackingOptions, 
            IMobileServiceEventManager eventManager, MobileServiceSyncSettingsManager settings)
        {
            if (IsTrackingEnabled(trackingOptions, source))
            {
                Guid batchId = source == StoreOperationSource.Local ? Guid.Empty : Guid.NewGuid();

                return new LocalStoreChangeTracker(targetStore, new StoreTrackingContext(source, batchId.ToString(), trackingOptions), eventManager, settings);
            }
            else
            {
                return new LocalStoreProxy(targetStore);               
            }
        }

        private static bool IsTrackingEnabled(StoreTrackingOptions trackingOptions, StoreOperationSource source)
        {
            bool result = false;

            switch (source)
            {
                case StoreOperationSource.Local:
                case StoreOperationSource.LocalPurge:
                    result = trackingOptions.HasFlag(StoreTrackingOptions.NotifyLocalRecordOperations);
                    break;
                case StoreOperationSource.ServerPull:
                    result = (trackingOptions & (StoreTrackingOptions.NotifyServerPullBatch | StoreTrackingOptions.NotifyServerPullRecordOperations)) != StoreTrackingOptions.None;
                    break;
                case StoreOperationSource.ServerPush:
                    result = (trackingOptions & (StoreTrackingOptions.NotifyServerPushBatch | StoreTrackingOptions.NotifyServerPushRecordOperations)) != StoreTrackingOptions.None;
                    break;
                default:
                    throw new InvalidOperationException("Unknown store operation source.");
            }

            return result;
        }
    }
}
