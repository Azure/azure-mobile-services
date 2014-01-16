// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.SQLiteStore
{
    internal class CurrentPlatform: IPlatform
    {
        public IMobileServiceSQLiteStoreImpl GetSyncStoreImpl(string filePath)
        {
            return new MobileServiceSQLiteStoreImpl(filePath);
        }
    }
}
