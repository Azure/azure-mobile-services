// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.MobileServices.SQLiteStore
{
    /// <summary>
    /// Provides an interface that platform-specific Mobile Services SQLite Sync Store assemblies 
    /// can implement to provide functionality required by the Mobile Services SDK 
    /// that is platform specific.
    /// </summary>
    internal interface IPlatform
    {        
        /// <summary>
        /// Get platform specific implementation of <see cref="IMobileServiceSyncStore"/>
        /// </summary>
        IMobileServiceSQLiteStoreImpl GetSyncStoreImpl(string filePath);
    }
}
