// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;

namespace Microsoft.WindowsAzure.MobileServices.Eventing
{
    /// <summary>
    /// Represents a <see cref="IMobileServiceEventManager"/> subscription.
    /// </summary>
    internal interface ISubscription : IDisposable
    {
        Type TargetMessageType { get; }

        void OnNext(IMobileServiceEvent mobileServiceEvent);
    }

    internal interface ISubscription<in T> : ISubscription
    {
    }
}
