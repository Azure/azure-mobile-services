// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// A flags enum for the available system properties in 
    /// Mobile Services.
    /// </summary>
    [Flags]
    public enum MobileServiceSystemProperties
    {
        /// <summary>
        /// No system properties
        /// </summary>
        None = 0x0,

        /// <summary>
        /// The __createdAt system property
        /// </summary>
        CreatedAt = 0x1,

        /// <summary>
        /// The __updatedAt system property
        /// </summary>
        UpdatedAt = 0x2,

        /// <summary>
        /// The __version system property
        /// </summary>
        Version = 0x4,

        /// <summary>
        /// All of the system properties.
        /// </summary>
        All = 0xFFFF,
    }
}
