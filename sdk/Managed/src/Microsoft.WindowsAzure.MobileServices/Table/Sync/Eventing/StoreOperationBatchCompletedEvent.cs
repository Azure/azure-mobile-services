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
    /// <summary>
    /// A mobile service event that is published when an operations batch against the local store is completed.
    /// </summary>
    public sealed class StoreOperationsBatchCompletedEvent : StoreChangeEvent
    {
        /// <summary>
        /// The store operation batch completed event name.
        /// </summary>
        public const string EventName = "MobileServices.StoreOperationBatchCompleted";

        public StoreOperationsBatchCompletedEvent(StoreOperationsBatch batch)
        {
            if (batch == null)
            {
                throw new ArgumentNullException("batch");
            }
            
            Batch = batch;
        }

        /// <summary>
        /// Gets the event name.
        /// </summary>
        public override string Name
        {
            get { return EventName; }
        }

        /// <summary>
        /// The operations batch instance.
        /// </summary>
        public StoreOperationsBatch Batch { get; private set; }
    }
}
