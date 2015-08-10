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
    public enum StoreOperationKind
    {
        /// <summary>
        /// Insert operation.
        /// </summary>
        Insert,
        /// <summary>
        /// Update operation.
        /// </summary>
        Update,
        /// <summary>
        /// Update or insert operation.
        /// </summary>
        Upsert,
        /// <summary>
        /// Delete operation.
        /// </summary>
        Delete,
    }
}
