// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Test.Unit.Table.Sync.Queue.Operations
{
    [TestClass]
    public class MobileServiceTableOperationTests
    {
        private Mock<MobileServiceTableOperation> operation;
        private Mock<MobileServiceTable> table;

        [TestInitialize]
        public void Initialize()
        {
            this.operation = new Mock<MobileServiceTableOperation>("test", MobileServiceTableKind.Table, "abc") { CallBase = true };
            var client = new Mock<MobileServiceClient>(MockBehavior.Strict);
            client.Object.Serializer = new MobileServiceSerializer();
            this.table = new Mock<MobileServiceTable>("test", client.Object);
            operation.Object.Table = this.table.Object;
        }

        [TestMethod]
        public async Task ExecuteAsync_Throws_WhenItemIsNull()
        {
            var ex = await AssertEx.Throws<MobileServiceInvalidOperationException>(() => this.operation.Object.ExecuteAsync());
            Assert.AreEqual("Operation must have an item associated with it.", ex.Message);
        }

        [TestMethod]
        public async Task ExecuteAsync_Returns_WhenItIsCancelled()
        {
            this.operation.Object.Cancel();
            await this.operation.Object.ExecuteAsync();
        }

        [TestMethod]
        public async Task ExecuteAsync_Throws_WhenResultIsNotJObject()
        {
            this.operation.Protected()
                          .Setup<Task<JToken>>("OnExecuteAsync")
                          .Returns(Task.FromResult<JToken>(new JArray()));

            this.operation.Object.Item = new JObject();

            var ex = await AssertEx.Throws<MobileServiceInvalidOperationException>(() => this.operation.Object.ExecuteAsync());
            Assert.AreEqual("Mobile Service table operation returned an unexpected response.", ex.Message);
        }

        [TestMethod]
        public async Task ExecuteAsync_DoesNotThrow_WhenResultIsNull()
        {
            this.operation.Protected()
                          .Setup<Task<JToken>>("OnExecuteAsync")
                          .Returns(Task.FromResult<JToken>(null));

            this.operation.Object.Item = new JObject();

            JObject result = await this.operation.Object.ExecuteAsync();
            Assert.IsNull(result);
        }

        [TestMethod]
        public void Serialize_Succeeds()
        {
            this.operation.Object.Item = JObject.Parse("{\"id\":\"abc\",\"text\":\"example\"}");

            var serializedOperation = this.operation.Object.Serialize();

            Assert.IsNotNull(serializedOperation["id"]);
            Assert.AreEqual(serializedOperation["itemId"], "abc");
            Assert.AreEqual(serializedOperation["tableName"], "test");
            Assert.AreEqual(serializedOperation["kind"], 0);
            Assert.AreEqual(serializedOperation["item"], JValue.CreateString(null));
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
            Assert.AreEqual(serializedOperation["sequence"], operation.Sequence);
        }

        [TestMethod]
        public void Deserialize_Succeeds_WithItem()
        {
            var serializedOperation = JObject.Parse(@"
            {""id"":""70cf6cc2-5981-4a32-ae6c-249572917a46"",
            ""kind"": 2,
            ""tableName"":""test"",
            ""itemId"":""abc"",
            ""version"":123,
            ""sequence"":null,
            ""state"":null,
            ""item"":""{\""id\"":\""abc\"",\""text\"":\""example\""}"",
            ""__createdAt"":""2014-03-11T20:37:10.3366689Z"",
            ""sequence"":0
            }");
            var operation = MobileServiceTableOperation.Deserialize(serializedOperation);

            Assert.AreEqual(serializedOperation["id"], operation.Id);
            Assert.AreEqual(serializedOperation["itemId"], operation.ItemId);
            Assert.AreEqual(serializedOperation["version"], operation.Version);
            Assert.AreEqual(serializedOperation["tableName"], operation.TableName);
            Assert.AreEqual(MobileServiceTableOperationKind.Delete, operation.Kind);
            Assert.AreEqual(serializedOperation["sequence"], operation.Sequence);
            Assert.AreEqual("abc", operation.Item["id"]);
            Assert.AreEqual("example", operation.Item["text"]);
        }

        [TestMethod]
        public void Deserialize_Succeeds_WhenVersionSequenceOrStateIsNull()
        {
            var serializedOperation = JObject.Parse(@"
            {""id"":""70cf6cc2-5981-4a32-ae6c-249572917a46"",
            ""kind"": 2,
            ""tableName"":""test"",
            ""itemId"":""abc"",
            ""version"":null,
            ""sequence"":null,
            ""state"":null,
            ""item"":""{\""id\"":\""abc\"",\""text\"":\""example\""}"",
            ""__createdAt"":""2014-03-11T20:37:10.3366689Z"",
            ""sequence"":0
            }");
            var operation = MobileServiceTableOperation.Deserialize(serializedOperation);

            Assert.AreEqual(serializedOperation["id"], operation.Id);
            Assert.AreEqual(serializedOperation["itemId"], operation.ItemId);
            Assert.AreEqual(serializedOperation["tableName"], operation.TableName);
            Assert.AreEqual(MobileServiceTableOperationKind.Delete, operation.Kind);
            Assert.AreEqual(serializedOperation["sequence"], operation.Sequence);
            Assert.AreEqual(0, operation.Version);
            Assert.AreEqual("abc", operation.Item["id"]);
            Assert.AreEqual("example", operation.Item["text"]);
        }
    }
}
