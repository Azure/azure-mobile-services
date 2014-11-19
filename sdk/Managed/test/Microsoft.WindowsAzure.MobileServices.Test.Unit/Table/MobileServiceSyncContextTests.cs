using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Moq;

namespace Microsoft.WindowsAzure.MobileServices.Test.Unit.Table
{
    [TestClass]
    public class MobileServiceSyncContextTests
    {
        private MobileServiceSyncContext context;
        private Mock<MobileServiceClient> client;

        [TestInitialize]
        public void Initialize()
        {
            this.client = new Mock<MobileServiceClient>();
            this.context = new MobileServiceSyncContext(this.client.Object);
        }

        [TestMethod]
        public void PendingOperations_DoesNotThrow_IfItIsNotInitialized()
        {
            Assert.AreEqual(0, this.context.PendingOperations);
        }
    }
}
