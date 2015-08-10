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
    [Flags]
    public enum StoreTrackingOptions
    {
        None = 0x00,
        /// <summary>
        /// Generates notifications for local record operations.
        /// </summary>
        NotifyLocalRecordOperations = 0x01,
        /// <summary>
        /// Generates notifications for operations triggered by a server pull.
        /// </summary>
        NotifyServerPullRecordOperations = 0x02,
        /// <summary>
        /// Generates notifiactions for operations triggered by a server push.
        /// </summary>
        NotifyServerPushRecordOperations = 0x04,
        /// <summary>
        /// Generates notifications for operations triggerd by a server pull or a server push.
        /// </summary>
        NotifyServerRecordOperations = NotifyServerPullRecordOperations | NotifyServerPushRecordOperations,
        /// <summary>
        /// Generates notifications for local and server (pull or push) record operations.
        /// </summary>
        NotifyLocalAndServerRecordOperations = NotifyLocalRecordOperations | NotifyServerRecordOperations,
        /// <summary>
        /// Generates a notification at the end of an operation batch triggered by a server pull.
        /// </summary>
        NotifyServerPullBatch = 0x08,
        /// <summary>
        /// Generates a notification at the end of an operation batch triggered by a server push.
        /// </summary>
        NotifyServerPushBatch = 0x10,
        /// <summary>
        /// Generates a notification at the end of an operation batch triggered by a server pull or a server push.
        /// </summary>
        NotifyServerBatch = NotifyServerPullBatch | NotifyServerPushBatch,
        /// <summary>
        /// Enable "upsert" analysis to detect if the record is being created or updated.
        /// </summary>
        DetectInsertsAndUpdates = 0x20,
        /// <summary>
        /// On updates, enable change detection on records and only generates notifications if data changes are detected.
        /// This automatically enables inserts and updates analysis.
        /// </summary>
        DetectRecordChanges = 0x40 | DetectInsertsAndUpdates,
        /// <summary>
        /// Generates all record operations and batch notifications.
        /// </summary>
        AllNotifications = NotifyLocalAndServerRecordOperations | NotifyServerBatch,
        /// <summary>
        /// Generates all record operation notifications, batch notifications, insert and updates detection and record change detection.
        /// </summary>
        AllNotificationsAndChangeDetection = AllNotifications | DetectInsertsAndUpdates | DetectRecordChanges
    }
}
