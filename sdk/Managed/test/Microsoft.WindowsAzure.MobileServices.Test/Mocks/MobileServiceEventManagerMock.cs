using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Eventing;

namespace Microsoft.WindowsAzure.MobileServices.Test.Mocks
{
    public class MobileServiceEventManagerMock<TSubscribe> : IMobileServiceEventManager where TSubscribe : class, IMobileServiceEvent
    {
        private Func<IMobileServiceEvent, Task> publishAsyncFunc;
        private Func<Action<TSubscribe>, IDisposable> subscribeActionFunc;
        private Func<IObserver<IMobileServiceEvent>, IDisposable> subscribeObserverFunc;

        public MobileServiceEventManagerMock()
        {
            this.publishAsyncFunc = _ => Task.FromResult(0);
            this.subscribeActionFunc = _ => null;
            this.subscribeObserverFunc = _ => null;
        }


        public Func<IObserver<IMobileServiceEvent>, IDisposable> SubscribeObserverFunc
        {
            get { return this.subscribeObserverFunc; }
            set
            {
                if (value == null)
                {
                    throw new InvalidOperationException("SubscribeObserverFunc may not be null.");
                }

                this.subscribeObserverFunc = value;
            }
        }
        public Func<Action<TSubscribe>, IDisposable> SubscribeActionFunc
        {
            get { return this.subscribeActionFunc; }
            set
            {
                if (value == null)
                {
                    throw new InvalidOperationException("SubscribeActionFunc may not be null.");
                }

                this.subscribeActionFunc = value;
            }
        }

        public Func<IMobileServiceEvent, Task> PublishAsyncFunc
        {
            get { return this.publishAsyncFunc; }
            set
            {
                if (value == null)
                {
                    throw new InvalidOperationException("PublishAsyncFunc may not be null.");
                }

                this.publishAsyncFunc = value;
            }
        }

        public async Task PublishAsync(IMobileServiceEvent mobileServiceEvent)
        {
            await PublishAsyncFunc(mobileServiceEvent);
        }

        public IDisposable Subscribe<T>(Action<T> next) where T : class, IMobileServiceEvent
        {
            var action = next as Action<TSubscribe>;
            if (action == null)
            {
                throw new InvalidOperationException(string.Format("You cannot use a type that is more derived than {0}", typeof(TSubscribe).FullName));
            }

            return subscribeActionFunc(action);
        }

        public IDisposable Subscribe(IObserver<IMobileServiceEvent> observer)
        {
            return this.subscribeObserverFunc(observer);
        }
    }
}
