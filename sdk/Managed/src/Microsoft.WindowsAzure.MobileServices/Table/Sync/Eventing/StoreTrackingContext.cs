// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    internal sealed class StoreTrackingContext
    {
        /// <summary>
        /// Initializes a new <see cref="StoreTrackingContext"/> with the specified <paramref name="source"/> and
        /// <paramref name="batchId"/>, enabling the NotifyRecordOperation and NotifyBatch <see cref="TrackingOptions"/>.
        /// </summary>
        /// <param name="source">The store operation source used by this tracking context.</param>
        /// <param name="batchId">The batch ID used by this tracking context.</param>
        public StoreTrackingContext(StoreOperationSource source, string batchId)
            : this(source, batchId, StoreTrackingOptions.AllNotificationsAndChangeDetection)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="StoreTrackingContext"/> with the specified <paramref name="source"/> and
        /// <paramref name="batchId"/> and <paramref name="trackingOptions"/>.
        /// </summary>
        /// <param name="source">The store operation source used by this tracking context.</param>
        /// <param name="batchId">The batch ID used by this tracking context.</param>
        /// <param name="trackingOptions">The tracking options used by this tracking context.</param>
        public StoreTrackingContext(StoreOperationSource source, string batchId, StoreTrackingOptions trackingOptions)
        {
            this.BatchId = batchId;
            this.Source = source;
            this.TrackingOptions = trackingOptions;
        }

        public string BatchId { get; private set; }

        public StoreOperationSource Source { get; private set; }

        public StoreTrackingOptions TrackingOptions { get; private set; }
    }
}
