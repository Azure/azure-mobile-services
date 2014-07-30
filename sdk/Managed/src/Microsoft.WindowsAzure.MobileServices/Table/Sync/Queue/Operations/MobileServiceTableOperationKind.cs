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
    /// Enumeration for kinds of table operations.
    /// </summary>
    public enum MobileServiceTableOperationKind
    {        
        /// <summary>
        /// Insert operation
        /// </summary>
        Insert = 0,
        /// <summary>
        /// Delete operation
        /// </summary>
        Update = 1,
        /// <summary>
        /// Update operation
        /// </summary>
        Delete = 2
    }
}
