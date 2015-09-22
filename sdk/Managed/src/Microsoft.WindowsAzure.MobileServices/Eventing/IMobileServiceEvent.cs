// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Eventing
{
    /// <summary>
    /// Represents a mobile service event.
    /// </summary>
    public interface IMobileServiceEvent
    {
        /// <summary>
        /// Gets the event name.
        /// </summary>
        string Name { get; }
    }
}
