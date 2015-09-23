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
    public interface IMobileServiceEventManager : IObservable<IMobileServiceEvent>
    {
        Task PublishAsync(IMobileServiceEvent mobileServiceEvent);

        IDisposable Subscribe<T>(Action<T> next) where T : class, IMobileServiceEvent;
    }
}
