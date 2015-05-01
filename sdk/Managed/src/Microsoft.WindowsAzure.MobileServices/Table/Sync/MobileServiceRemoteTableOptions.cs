// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// A flags enum for the capabilities of remote table.
    /// </summary>
    [Flags]
    public enum MobileServiceRemoteTableOptions
    {
        /// <summary>
        /// No options are supported
        /// </summary>
        None = 0x0,

        /// <summary>
        /// $skip odata option
        /// </summary>
        Skip = 0x1,

        /// <summary>
        /// $top odata option
        /// </summary>
        Top = 0x2,

        /// <summary>
        /// $orderby odata option
        /// </summary>
        OrderBy = 0x4,

        /// <summary>
        /// All options are supported
        /// </summary>
        All = 0xFFFF
    }
}
