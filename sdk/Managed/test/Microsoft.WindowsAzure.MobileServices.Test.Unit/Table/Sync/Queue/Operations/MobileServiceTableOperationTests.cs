// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Moq;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Test.Unit.Table.Sync.Queue.Operations
{
    [TestClass]
    public class MobileServiceTableOperationTests
    {
        private MobileServiceTableOperation operation;

        [TestInitialize]
        public void Initialize()
        {
            this.operation = new Mock<MobileServiceTableOperation>("test", "abc").Object;
        }

        [TestMethod]
        public void Serialize_Succeeds()
        {
            this.operation.Item = JObject.Parse("{\"id\":\"abc\",\"text\":\"example\"}");

            var serializedOperation = this.operation.Serialize();

            Assert.IsNotNull(serializedOperation["id"]);
            Assert.AreEqual(serializedOperation["itemId"], "abc");
            Assert.AreEqual(serializedOperation["tableName"], "test");
            Assert.AreEqual(serializedOperation["kind"], 0);
            Assert.AreEqual(serializedOperation["item"], JValue.CreateString(null));
            Assert.IsNotNull(serializedOperation["__createdAt"]);
            Assert.IsNotNull(serializedOperation["sequence"]);
        }

        [TestMethod]
        public void Deserialize_Succeeds()
        {
            var serializedOperation = JObject.Parse("{\"id\":\"70cf6cc2-5981-4a32-ae6c-249572917a46\",\"kind\": 0,\"tableName\":\"test\",\"itemId\":\"abc\",\"item\":null,\"__createdAt\":\"2014-03-11T20:37:10.3366689Z\",\"sequence\":0}");

            var operation = MobileServiceTableOperation.Deserialize(serializedOperation);

            Assert.AreEqual(serializedOperation["id"], operation.Id);
            Assert.AreEqual(serializedOperation["itemId"], operation.ItemId);
            Assert.AreEqual(serializedOperation["tableName"], operation.TableName);
            Assert.AreEqual(MobileServiceTableOperationKind.Insert, operation.Kind);
            Assert.IsNull(operation.Item);
            Assert.AreEqual(serializedOperation["__createdAt"], operation.CreatedAt);
            Assert.AreEqual(serializedOperation["sequence"], operation.Sequence);
        }

        [TestMethod]
        public void Deserialize_Succeeds_WithItem()
        {
            var serializedOperation = JObject.Parse("{\"id\":\"70cf6cc2-5981-4a32-ae6c-249572917a46\",\"kind\": 2,\"tableName\":\"test\",\"itemId\":\"abc\",\"item\":\"{\\\"id\\\":\\\"abc\\\",\\\"text\\\":\\\"example\\\"}\",\"__createdAt\":\"2014-03-11T20:37:10.3366689Z\",\"sequence\":0}");
            var operation = MobileServiceTableOperation.Deserialize(serializedOperation);

            Assert.AreEqual(serializedOperation["id"], operation.Id);
            Assert.AreEqual(serializedOperation["itemId"], operation.ItemId);
            Assert.AreEqual(serializedOperation["tableName"], operation.TableName);
            Assert.AreEqual(MobileServiceTableOperationKind.Delete, operation.Kind);
            Assert.AreEqual(serializedOperation["__createdAt"], operation.CreatedAt);
            Assert.AreEqual(serializedOperation["sequence"], operation.Sequence);
            Assert.AreEqual("abc", operation.Item["id"]);
            Assert.AreEqual("example", operation.Item["text"]);
        }
    }
}
