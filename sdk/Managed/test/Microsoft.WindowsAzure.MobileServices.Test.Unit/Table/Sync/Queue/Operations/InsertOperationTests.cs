// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Moq;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Test.Unit.Table.Sync.Queue.Operations
{
    [TestClass]
    public class InsertOperationTests
    {
        private InsertOperation operation;

        [TestInitialize]
        public void Initialize()
        {
            this.operation = new InsertOperation("test", MobileServiceTableKind.Table, "abc");
        }

        [TestMethod]
        public async Task ExecuteAsync_InsertsItemOnTable()
        {
            var client = new Mock<MobileServiceClient>(MockBehavior.Strict);

            var table = new Mock<MobileServiceTable>("test", client.Object);
            this.operation.Table = table.Object;

            var item = JObject.Parse("{\"id\":\"abc\",\"Text\":\"Example\"}");
            var itemWithProperties = JObject.Parse("{\"id\":\"abc\",\"Text\":\"Example\",\"__version\":\"1\",\"__system\":12}");
            this.operation.Item = itemWithProperties;

            table.Setup(t => t.InsertAsync(item)).Returns(Task.FromResult<JToken>(item));

            await this.operation.ExecuteAsync();
        }

        [TestMethod]
        public async Task ExecuteLocalAsync_UpsertsItemOnStore()
        {
            var store = new Mock<IMobileServiceLocalStore>();
            var item = JObject.Parse("{\"id\":\"abc\",\"Text\":\"Example\"}");

            await this.operation.ExecuteLocalAsync(store.Object, item);
            store.Verify(s => s.UpsertAsync("test", It.Is<JObject[]>(list => list.Contains(item)), false), Times.Once());
        }

        [TestMethod]
        public async Task ExecuteLocalAsync_Throws_WhenStoreThrows()
        {
            var store = new Mock<IMobileServiceLocalStore>();
            var storeError = new InvalidOperationException();
            var item = JObject.Parse("{\"id\":\"abc\",\"Text\":\"Example\"}");

            store.Setup(s => s.UpsertAsync("test", It.Is<JObject[]>(list => list.Contains(item)), false)).Throws(storeError);
            var ex = await AssertEx.Throws<InvalidOperationException>(() => this.operation.ExecuteLocalAsync(store.Object, item));
            Assert.AreSame(storeError, ex);
        }

        [TestMethod]
        public void Validate_Throws_WithInsertOperation()
        {
            var newOperation = new InsertOperation("test", MobileServiceTableKind.Table, "abc");
            var ex = AssertEx.Throws<InvalidOperationException>(() => this.operation.Validate(newOperation));
            Assert.AreEqual("An insert operation on the item is already in the queue.", ex.Message);
        }

        [TestMethod]
        public void Validate_Succeeds_WithUpdateOperation()
        {
            var newOperation = new UpdateOperation("test", MobileServiceTableKind.Table, "abc");
            this.operation.Validate(newOperation);
        }

        [TestMethod]
        public void Validate_Throws_WithDeleteOperation_WhenInsertIsAttempted()
        {
            var newOperation = new DeleteOperation("test", MobileServiceTableKind.Table, "abc");
            this.operation.State = MobileServiceTableOperationState.Attempted;
            var ex = AssertEx.Throws<InvalidOperationException>(() => this.operation.Validate(newOperation));
            Assert.AreEqual("The item is in inconsistent state in the local store. Please complete the pending sync by calling PushAsync() before deleting the item.", ex.Message);
        }

        [TestMethod]
        public void Validate_Succeeds_WithDeleteOperation()
        {
            var newOperation = new DeleteOperation("test", MobileServiceTableKind.Table, "abc");
            this.operation.Validate(newOperation);
        }

        [TestMethod]
        public void Collapse_CancelsExistingOperation_WithUpdateOperation()
        {
            var newOperation = new UpdateOperation("test", MobileServiceTableKind.Table, "abc");
            this.operation.Collapse(newOperation);

            // new operation should be cancelled
            Assert.IsTrue(newOperation.IsCancelled);

            // existing operation should be updated and not cancelled
            Assert.IsFalse(this.operation.IsCancelled);
            Assert.IsTrue(this.operation.IsUpdated);
            Assert.AreEqual(this.operation.Version, 2);
        }

        [TestMethod]
        public void Collapse_CancelsBothOperations_WithDeleteOperation()
        {
            var newOperation = new DeleteOperation("test", MobileServiceTableKind.Table, "abc");
            this.operation.Collapse(newOperation);

            // new operation should be cancelled
            Assert.IsTrue(newOperation.IsCancelled);

            // existing operation should also be cancelled
            Assert.IsTrue(this.operation.IsCancelled);
        }
    }
}
