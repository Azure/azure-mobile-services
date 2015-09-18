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
    /// Represents the source of an operation performed against the local store.
    /// </summary>
    public enum StoreOperationSource
    {
        /// <summary>
        /// The operation was triggered by a local action (e.g. locally inserting, updating or deleting a record)
        /// </summary>
        Local,
        /// <summary>
        /// The operation was triggered by a local conflict resolution action taken against an operation error.
        /// </summary>
        LocalConflictResolution,
        /// <summary>
        /// The operation was triggered by a local purge action.
        /// </summary>
        LocalPurge,
        /// <summary>
        /// The operation was triggered by a Pull action and reflects a newer version of the record from the server.
        /// </summary>
        ServerPull,
        /// <summary>
        /// The operation was triggered by a Push action.
        /// </summary>
        ServerPush,
    }
}
