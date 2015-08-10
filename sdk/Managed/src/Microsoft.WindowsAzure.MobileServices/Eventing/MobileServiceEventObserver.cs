// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Eventing
{
    internal interface IEventObserver<out T> : IObserver<IMobileServiceEvent> { }

    internal sealed class MobileServiceEventObserver<T> : IEventObserver<T> where T : class, IMobileServiceEvent
    {
        private Action<T> next;
        public MobileServiceEventObserver(Action<T> nextHandler)
        {
            if (nextHandler == null)
            {
                throw new ArgumentNullException("nextHandler");
            }

            this.next = nextHandler;
        }

        public void OnNext(IMobileServiceEvent value)
        {
            this.next((T)value);
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public IObserver<T> Observer { get { return this; } }

        public void OnNext(T value)
        {
            throw new NotImplementedException();
        }
    }
}
