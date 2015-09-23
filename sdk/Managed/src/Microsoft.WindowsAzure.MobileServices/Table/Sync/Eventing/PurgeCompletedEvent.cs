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
    /// A mobile service event that is published when a purge operation against the local store is completed.
    /// </summary>
    public sealed class PurgeCompletedEvent : StoreChangeEvent
    {
        /// <summary>
        /// The purge completed event name.
        /// </summary>
        public const string EventName = "MobileServices.PurgeCompleted";

        public PurgeCompletedEvent(string tableName)
        {
            if (tableName == null)
            {
                throw new ArgumentNullException("tableName");
            }

            TableName = tableName;
        }
        
        /// <summary>
        /// Gets the event name.
        /// </summary>
        public override string Name
        {
            get { return EventName; }
        }

        /// <summary>
        /// Gets the name of the table that was the target of the purge operation.
        /// </summary>
        public string TableName { get; private set; }
    }
}
