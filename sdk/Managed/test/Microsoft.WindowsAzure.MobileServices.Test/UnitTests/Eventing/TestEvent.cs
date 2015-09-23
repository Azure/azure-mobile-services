using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Eventing;

namespace Microsoft.WindowsAzure.MobileServices.Test.UnitTests.Eventing
{
    public class TestEvent<T> : IMobileServiceEvent
    {
        public TestEvent(T payload)
        {
            this.Payload = payload;
        }

        public string Name
        {
            get { return "TestEvent"; }
        }

        public T Payload { get; set; }
    }
}
