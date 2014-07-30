// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    /// <summary>
    /// Names of tables in local store that are reserved by sync framework
    /// </summary>
    public static class MobileServiceLocalSystemTables
    {
        /// <summary>
        /// Table that stores operation queue items
        /// </summary>
        public static readonly string OperationQueue = "__operations";

        /// <summary>
        /// Table that stores sync errors
        /// </summary>
        public static readonly string SyncErrors = "__errors";

        /// <summary>
        /// Configuration settings related to sync framework
        /// </summary>
        public static readonly string Config = "__config";

        /// <summary>
        /// Returns the names of all system tables
        /// </summary>
        public static IEnumerable<string> All { get; private set; }

        /// <summary>
        ///  Defines all the system tables on the store
        /// </summary>
        /// <param name="store">An instance of <see cref="IMobileServiceLocalStore"/></param>
        public static void DefineAll(MobileServiceLocalStore store)
        {
            MobileServiceTableOperation.DefineTable(store);
            MobileServiceTableOperationError.DefineTable(store);
            MobileServiceSyncSettingsManager.DefineTable(store);            
        }

        static MobileServiceLocalSystemTables()
        {
            All = new [] { OperationQueue, SyncErrors, Config };
        }
    }
}
