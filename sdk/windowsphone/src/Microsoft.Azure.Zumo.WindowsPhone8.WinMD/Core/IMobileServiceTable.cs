// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Provides operations on a table for a Mobile Service.
    /// </summary>
    public partial interface IMobileServiceTable
    {
        /// <summary>
        /// Gets a reference to the MobileServiceClient associated with this
        /// table.
        /// </summary>
        MobileServiceClient MobileServiceClient { get; }

        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        string TableName { get; }
    }
}
