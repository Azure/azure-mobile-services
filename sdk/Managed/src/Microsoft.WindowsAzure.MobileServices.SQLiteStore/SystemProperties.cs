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
    internal class SystemProperties
    {
        /// <summary>
        /// The name of the reserved Mobile Services id member.
        /// </summary>
        public const string Id = "id";
        
        /// <summary>
        /// The name of the reserved Mobile Services version member.
        /// </summary>
        public const string Version = "__version";
    }
}
