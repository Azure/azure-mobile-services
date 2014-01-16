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
    /// Names of tables in local store that are reserved by sync framework
    /// </summary>
    public static class LocalSystemTables
    {
        /// <summary>
        /// Table that stores operation queue items
        /// </summary>
        public static readonly string OperationQueue = "__operations";

        /// <summary>
        /// Table that stores sync errors
        /// </summary>
        public static readonly string SyncErrors = "__errors";
    }
}
