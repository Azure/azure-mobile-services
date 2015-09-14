using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Eventing;
using Microsoft.WindowsAzure.MobileServices.TestFramework;

namespace Microsoft.WindowsAzure.MobileServices.Test.UnitTests.Eventing
{
    [Tag("unit")]
    [Tag("eventing")]
    public class MobileServiceEventManagerTests : TestBase
    {
		[TestMethod]
        public void Subscribe_FiltersOnGenericType()
        {
            var eventManager = new MobileServiceEventManager();

            var testEvent = new TestEvent<bool>(false);
            var mobileServiceEvent = new MobileServiceEvent<bool>("msevent", false);

            IDisposable testEventSubscription = eventManager.Subscribe<TestEvent<bool>>(e => e.Payload = true);

            eventManager.PublishAsync(testEvent).Wait(1000);
            eventManager.PublishAsync(mobileServiceEvent).Wait(1000);

            Assert.IsTrue(testEvent.Payload, "Test event was not handled");
            Assert.IsFalse(mobileServiceEvent.Payload, "Mobile service event was not filtered");

            testEventSubscription.Dispose();
        }

        [TestMethod]
        public void Subscribe_OnHandler_Succeeds()
        {
            var eventManager = new MobileServiceEventManager();

            var mobileServiceEvent = new MobileServiceEvent<bool>("msevent", false);
            bool eventHandled = false;
            IDisposable innerSubscription = null;
            IDisposable outerSubscription = eventManager
                .Subscribe<IMobileServiceEvent>(e => innerSubscription = eventManager.Subscribe<IMobileServiceEvent>(b => eventHandled = true));


            bool result = eventManager.PublishAsync(mobileServiceEvent).Wait(1000);
            Assert.IsTrue(result, "Subscribe failed");

            outerSubscription.Dispose();

            eventManager.PublishAsync(mobileServiceEvent).Wait(1000);
            Assert.IsTrue(eventHandled, "Subscribe failed");
        }

        [TestMethod]
        public void Subscribe_FiltersOnObserverGenericType()
        {
            var eventManager = new MobileServiceEventManager();

            var testEvent = new TestEvent<bool>(false);
            var mobileServiceEvent = new MobileServiceEvent<bool>("msevent", false);

            var observer = new MobileServiceEventObserver<TestEvent<bool>>(e=>e.Payload = true);

            IDisposable testEventSubscription = eventManager.Subscribe(observer);

            eventManager.PublishAsync(testEvent).Wait(1000);
            eventManager.PublishAsync(mobileServiceEvent).Wait(1000);

            Assert.IsTrue(testEvent.Payload, "Test event was not handled");
            Assert.IsFalse(mobileServiceEvent.Payload, "Mobile service event was not filtered");

            testEventSubscription.Dispose();
        }

        [TestMethod]
        public void Subscribe_WithObserver_DoesNotFilterDerivedTypes()
        {
            var eventManager = new MobileServiceEventManager();

            var testEventA = new DerivedEventA<bool>(false);
            var testEventB = new DerivedEventA<bool>(false);

            var observer = new MobileServiceEventObserver<TestEvent<bool>>(e => e.Payload = true);

            IDisposable testEventSubscription = eventManager.Subscribe(observer);

            eventManager.PublishAsync(testEventA).Wait(1000);
            eventManager.PublishAsync(testEventB).Wait(1000);

            Assert.IsTrue(testEventA.Payload, "Derived event A was not handled");
            Assert.IsTrue(testEventA.Payload, "Derived event B was not handled");

            testEventSubscription.Dispose();
        }

        [TestMethod]
        public void Subscribe_WithGenericType_DoesNotFilterDerivedTypes()
        {
            var eventManager = new MobileServiceEventManager();

            var testEventA = new DerivedEventA<bool>(false);
            var testEventB = new DerivedEventA<bool>(false);

            IDisposable testEventSubscription = eventManager.Subscribe<TestEvent<bool>>(e => e.Payload = true);

            eventManager.PublishAsync(testEventA).Wait(1000);
            eventManager.PublishAsync(testEventB).Wait(1000);

            Assert.IsTrue(testEventA.Payload, "Derived event A was not handled");
            Assert.IsTrue(testEventA.Payload, "Derived event B was not handled");

            testEventSubscription.Dispose();
        }

        [TestMethod]
        public void Subscribe_Throws_WhenObserverIsNull()
        {
            var eventManager = new MobileServiceEventManager();
			IObserver<IMobileServiceEvent> observer = null;

            ArgumentNullException exception = AssertEx.Throws<ArgumentNullException>(() => eventManager.Subscribe(observer));
            Assert.AreEqual(exception.ParamName, "observer", "Incorrect parameter name");
        }

        [TestMethod]
        public void Subscribe_Throws_WhenActionIsNull()
        {
            var eventManager = new MobileServiceEventManager();
            Action<IMobileServiceEvent> action = null;

            ArgumentNullException exception = AssertEx.Throws<ArgumentNullException>(() => eventManager.Subscribe(action));
            Assert.AreEqual(exception.ParamName, "next", "Incorrect parameter name");
        }

        [AsyncTestMethod]
        public async Task PublishAsync_Throws_WhenEventIsNull()
        {
            var eventManager = new MobileServiceEventManager();
            ArgumentNullException exception = await AssertEx.Throws<ArgumentNullException>(() => eventManager.PublishAsync(null));
            Assert.AreEqual(exception.ParamName, "mobileServiceEvent", "Incorrect parameter name");
        }

        private class DerivedEventA<T> : TestEvent<T>
        {
            public DerivedEventA(T payload)
                : base(payload)
            { }
        }

        private class DerivedEventB<T> : TestEvent<T>
        {
            public DerivedEventB(T payload)
                : base(payload)
            { }
        }
    }
}
