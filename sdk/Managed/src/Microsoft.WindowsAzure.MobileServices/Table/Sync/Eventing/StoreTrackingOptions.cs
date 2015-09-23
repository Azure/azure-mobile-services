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
    /// A flags enumeration for the available store tracking options available.
    /// </summary>
    [Flags]
    public enum StoreTrackingOptions
    {
        /// <summary>
        /// No tracking options.
        /// </summary>
        None = 0x00,
        /// <summary>
        /// Generates notifications for local record operations.
        /// </summary>
        NotifyLocalOperations = 0x01,
        /// <summary>
        /// Generates notifications for local conflict resolution record operations.
        /// </summary>
        NotifyLocalConflictResolutionOperations = 0x02,
        /// <summary>
        /// Generates notifications for operations triggered by a server pull.
        /// </summary>
        NotifyServerPullOperations = 0x04,
        /// <summary>
        /// Generates notifiactions for operations triggered by a server push.
        /// </summary>
        NotifyServerPushOperations = 0x08,
        /// <summary>
        /// Generates notifications for operations triggerd by a server pull or a server push.
        /// </summary>
        NotifyServerOperations = NotifyServerPullOperations | NotifyServerPushOperations,
        /// <summary>
        /// Generates notifications for local and server (pull or push) record operations.
        /// </summary>
        NotifyLocalAndServerOperations = NotifyLocalOperations | NotifyLocalConflictResolutionOperations | NotifyServerOperations,
        /// <summary>
        /// Generates a notification at the end of an operation batch triggered by a server pull.
        /// </summary>
        NotifyServerPullBatch = 0x10,
        /// <summary>
        /// Generates a notification at the end of an operation batch triggered by a server push.
        /// </summary>
        NotifyServerPushBatch = 0x20,
        /// <summary>
        /// Generates a notification at the end of an operation batch triggered by a server pull or a server push.
        /// </summary>
        NotifyServerBatch = NotifyServerPullBatch | NotifyServerPushBatch,
        /// <summary>
        /// Enable "upsert" analysis to detect if the record is being created or updated.
        /// </summary>
        DetectInsertsAndUpdates = 0x40,
        /// <summary>
        /// On updates, enable change detection on records and only generates notifications if data changes are detected.
        /// This automatically enables inserts and updates analysis.
        /// </summary>
        DetectRecordChanges = 0x80 | DetectInsertsAndUpdates,
        /// <summary>
        /// Generates all record operations and batch notifications.
        /// </summary>
        AllNotifications = NotifyLocalAndServerOperations | NotifyServerBatch,
        /// <summary>
        /// Generates all record operation notifications, batch notifications, insert and updates detection and record change detection.
        /// </summary>
        AllNotificationsAndChangeDetection = AllNotifications | DetectInsertsAndUpdates | DetectRecordChanges
    }
}
