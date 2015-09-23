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
    /// Manages event notifications and subscriptions.
    /// </summary>
    public interface IMobileServiceEventManager : IObservable<IMobileServiceEvent>
    {
        /// <summary>
        /// Publishes a <see cref="IMobileServiceEvent"/>.
        /// </summary>
        /// <param name="mobileServiceEvent">The event to be published.</param>
        /// <returns>A task that completes when the event is published.</returns>
        Task PublishAsync(IMobileServiceEvent mobileServiceEvent);

        /// <summary>
        /// Subscribes to event notifications.
        /// </summary>
        /// <typeparam name="T">The base type of event to filter notifications by.</typeparam>
        /// <param name="next">The delegate to be invoked to handle events.</param>
        /// <returns>An <see cref="IDisposable"/> instance representing the subscription.</returns>
        IDisposable Subscribe<T>(Action<T> next) where T : class, IMobileServiceEvent;
    }
}
