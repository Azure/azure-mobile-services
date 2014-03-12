// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Test.Unit.Table.Sync.Queue.Operations
{
    [TestClass]
    public class DeleteOperationTests
    {
        private DeleteOperation operation;

        [TestInitialize]
        public void Initialize()
        {
            this.operation = new DeleteOperation("test", "abc");
        }

        [TestMethod]
        public async Task ExecuteAsync_DeletesItemOnTable()
        {
            var client = new Mock<MobileServiceClient>(MockBehavior.Strict);

            var table = new Mock<MobileServiceTable>("test", client.Object);
            this.operation.Table = table.Object;

            var item = JObject.Parse("{\"id\":\"abc\",\"Text\":\"Example\"}");
            this.operation.Item = item;

            table.Setup(t => t.DeleteAsync(item)).Returns(Task.FromResult<JToken>(item));

            await this.operation.ExecuteAsync();
        }

        [TestMethod]
        public async Task ExecuteAsync_Throws_WhenItemIsNull()
        {
            var ex = await AssertEx.Throws<MobileServiceInvalidOperationException>(() => this.operation.ExecuteAsync());
            Assert.AreEqual("Operation must have an item associated with it.", ex.Message);
        }

        [TestMethod]
        public async Task ExecuteLocalAsync_DeletesItemOnStore()
        {
            var store = new Mock<IMobileServiceLocalStore>();
            await this.operation.ExecuteLocalAsync(store.Object, null);
            store.Verify(s => s.DeleteAsync("test", "abc"), Times.Once());
        }

        [TestMethod]
        public async Task ExecuteLocalAsync_Throws_WhenStoreThrows()
        {
            var store = new Mock<IMobileServiceLocalStore>();
            var storeError = new InvalidOperationException();
            store.Setup(s => s.DeleteAsync("test", "abc")).Throws(storeError);

            var ex = await AssertEx.Throws<InvalidOperationException>(() => this.operation.ExecuteLocalAsync(store.Object, null));
            Assert.AreSame(storeError, ex);
        }

        [TestMethod]
        public void Validate_Throws_WithInsertOperation()
        {
            var tableOperation = new InsertOperation("test", "abc");
            TestDeleteValidateThrows(tableOperation);
        }

        [TestMethod]
        public void Validate_Throws_WithUpdateOperation()
        {
            var tableOperation = new UpdateOperation("test", "abc");
            TestDeleteValidateThrows(tableOperation);
        }

        [TestMethod]
        public void Validate_Throws_WithDeleteOperation()
        {
            var tableOperation = new DeleteOperation("test", "abc");
            TestDeleteValidateThrows(tableOperation);
        }

        private void TestDeleteValidateThrows(MobileServiceTableOperation tableOperation)
        {
            var ex = AssertEx.Throws<InvalidOperationException>(() => this.operation.Validate(tableOperation));
            Assert.AreEqual("Delete operation on the item is already in the queue.", ex.Message);
        }

        [TestMethod]
        public void Serialize_Succeeds_DeleteHasItem()
        {
            var serializedItem = "{\"id\":\"abc\",\"text\":\"example\"}";
            this.operation.Item = JObject.Parse(serializedItem);

            var serializedOperation = this.operation.Serialize();

            // Check delete successfully overrides keeping an item
            Assert.AreEqual(serializedOperation["kind"], 2);
            Assert.AreEqual(serializedOperation["item"], serializedItem);
        }
    }
}
