// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MobileServices.Sync;

namespace Microsoft.WindowsAzure.MobileServices.Test.Unit.Table.Sync.Queue.Operations
{
    [TestClass]
    public class UpdateOperationTests
    {
        private UpdateOperation operation;

        [TestInitialize]
        public void Initialize()
        {
            this.operation = new UpdateOperation("test", "abc");
        }

        [TestMethod]
        public async Task ExecuteAsync_Throws_WhenItemIsNull()
        {
            var ex = await AssertEx.Throws<MobileServiceInvalidOperationException>(() => this.operation.ExecuteAsync());
            Assert.AreEqual("Operation must have an item associated with it.", ex.Message);
        }
    }
}
