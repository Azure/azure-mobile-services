// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;

namespace Microsoft.WindowsAzure.MobileServices.Test.Unit.Table.Sync.Queue.Operations
{
    [TestClass]
    public class DeleteOperationTests
    {
        private DeleteOperation operation;

        [TestInitialize]
        public void Initialize()
        {
            this.operation = new DeleteOperation("test", MobileServiceTableKind.Table, "abc");
        }

        [TestMethod]
        public void WriteResultToStore_IsFalse()
        {
            Assert.IsFalse(this.operation.CanWriteResultToStore);
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
        public async Task ExecuteAsync_IgnoresNotFound()
        {
            var client = new Mock<MobileServiceClient>(MockBehavior.Strict);

            var table = new Mock<MobileServiceTable>("test", client.Object);
            this.operation.Table = table.Object;

            var item = JObject.Parse("{\"id\":\"abc\",\"Text\":\"Example\"}");
            this.operation.Item = item;

            table.Setup(t => t.DeleteAsync(item)).Throws(new MobileServiceInvalidOperationException("not found", new HttpRequestMessage(), new HttpResponseMessage(HttpStatusCode.NotFound)));

            JObject result = await this.operation.ExecuteAsync();
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task ExecuteLocalAsync_DeletesItemOnStore()
        {
            var store = new Mock<IMobileServiceLocalStore>();
            await this.operation.ExecuteLocalAsync(store.Object, null);
            store.Verify(s => s.DeleteAsync("test", It.Is<string[]>(i => i.Contains("abc"))), Times.Once());
        }

        [TestMethod]
        public async Task ExecuteLocalAsync_Throws_WhenStoreThrows()
        {
            var store = new Mock<IMobileServiceLocalStore>();
            var storeError = new InvalidOperationException();
            store.Setup(s => s.DeleteAsync("test", It.Is<string[]>(i => i.Contains("abc")))).Throws(storeError);

            var ex = await AssertEx.Throws<InvalidOperationException>(() => this.operation.ExecuteLocalAsync(store.Object, null));
            Assert.AreSame(storeError, ex);
        }

        [TestMethod]
        public void Validate_Throws_WithInsertOperation()
        {
            var tableOperation = new InsertOperation("test", MobileServiceTableKind.Table, "abc");
            TestDeleteValidateThrows(tableOperation);
        }

        [TestMethod]
        public void Validate_Throws_WithUpdateOperation()
        {
            var tableOperation = new UpdateOperation("test", MobileServiceTableKind.Table, "abc");
            TestDeleteValidateThrows(tableOperation);
        }

        [TestMethod]
        public void Validate_Throws_WithDeleteOperation()
        {
            var tableOperation = new DeleteOperation("test", MobileServiceTableKind.Table, "abc");
            TestDeleteValidateThrows(tableOperation);
        }

        private void TestDeleteValidateThrows(MobileServiceTableOperation tableOperation)
        {
            var ex = AssertEx.Throws<InvalidOperationException>(() => this.operation.Validate(tableOperation));
            Assert.AreEqual("A delete operation on the item is already in the queue.", ex.Message);
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
