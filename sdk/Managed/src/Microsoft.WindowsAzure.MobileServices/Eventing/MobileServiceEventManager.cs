// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Eventing
{
    internal sealed class MobileServiceEventManager : IMobileServiceEventManager, IDisposable
    {
        private readonly List<ISubscription> subscriptions;
        private readonly ReaderWriterLockSlim subscriptionLock = new ReaderWriterLockSlim();
        private static Type observerTypeDefinition;

        static MobileServiceEventManager()
        {
            observerTypeDefinition = typeof(IEventObserver<IMobileServiceEvent>).GetTypeInfo().GetGenericTypeDefinition();
        }

        public MobileServiceEventManager()
        {
            this.subscriptions = new List<ISubscription>();
        }

        ~MobileServiceEventManager()
        {
            Dispose(false);
        }

        public Task PublishAsync(IMobileServiceEvent mobileServiceEvent)
        {
            if (mobileServiceEvent == null)
            {
                throw new ArgumentNullException("mobileServiceEvent");
            }

            return Task.Run(() =>
            {
                TypeInfo messageType = mobileServiceEvent.GetType().GetTypeInfo();

                this.subscriptionLock.EnterReadLock();

                IList<ISubscription> subscriptionMatches = null;

                try
                {
                    subscriptionMatches = this.subscriptions.Where(s => s.TargetMessageType.GetTypeInfo()
                        .IsAssignableFrom(messageType)).ToList();
                }
                finally
                {
                    this.subscriptionLock.ExitReadLock();
                }

                foreach (var subscription in subscriptionMatches)
                {
                    subscription.OnNext(mobileServiceEvent);
                }
            });
        }

        public IDisposable Subscribe(IObserver<IMobileServiceEvent> observer)
        {
            if (observer == null)
            {
                throw new ArgumentNullException("observer");
            }

            Type messageType = GetMessageType(observer);
            var subscription = new Subscription<IMobileServiceEvent>(this, observer, messageType);

            return Subscribe(subscription);
        }

        public IDisposable Subscribe<T>(Action<T> next) where T : class, IMobileServiceEvent
        {
            if (next == null)
            {
                throw new ArgumentNullException("next");
            }

            var observer = new MobileServiceEventObserver<T>(next);
            var subscription = new Subscription<T>(this, observer);

            return Subscribe(subscription);
        }

        private IDisposable Subscribe(ISubscription subscription)
        {
            this.subscriptionLock.EnterWriteLock();

            try
            {
                this.subscriptions.Add(subscription);
            }
            finally
            {
                this.subscriptionLock.ExitWriteLock();
            }

            return subscription;
        }

        private void Unsubscribe(ISubscription subscription)
        {
            Task.Run(() =>
            {
                this.subscriptionLock.EnterWriteLock();
                try
                {
                    this.subscriptions.Remove(subscription);
                }
                finally
                {
                    this.subscriptionLock.ExitWriteLock();
                }
            });
        }

        private Type GetMessageType(IObserver<IMobileServiceEvent> observer)
        {
            TypeInfo typeInfo = observer.GetType().GetTypeInfo();
            return typeInfo.ImplementedInterfaces.Select(i => i.GetTypeInfo())
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == observerTypeDefinition)
                .Select(i => i.GenericTypeArguments[0]).First();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.subscriptionLock.Dispose();
            }
        }

        private sealed class Subscription<T> : IDisposable, ISubscription where T : class, IMobileServiceEvent
        {
            private readonly MobileServiceEventManager service;
            private readonly IObserver<T> observer;
            private Type targetMessageType;

            public Subscription(MobileServiceEventManager service, IObserver<T> observer, Type targetMessageType = null)
            {
                if (service == null)
                {
                    throw new ArgumentNullException("service");
                }

                if (observer == null)
                {
                    throw new ArgumentNullException("observer");
                }

                this.service = service;
                this.observer = observer;

                this.targetMessageType = targetMessageType ?? typeof(T);
            }

            public Type TargetMessageType
            {
                get { return this.targetMessageType; }
            }

            public void Dispose()
            {
                this.service.Unsubscribe(this);
            }


            public void OnNext(IMobileServiceEvent mobileServiceEvent)
            {
                this.observer.OnNext((T)mobileServiceEvent);
            }
        }
    }
}
