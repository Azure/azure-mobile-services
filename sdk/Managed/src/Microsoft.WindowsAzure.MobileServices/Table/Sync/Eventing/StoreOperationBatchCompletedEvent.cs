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
    public sealed class StoreOperationBatchCompletedEvent : StoreChangeEvent
    {
        public const string EventName = "MobileServices.StoreOperationBatchCompleted";

        public StoreOperationBatchCompletedEvent(StoreOperationsBatch batch)
        {
            if (batch == null)
            {
                throw new ArgumentNullException("batch");
            }
            
            Batch = batch;
        }
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
