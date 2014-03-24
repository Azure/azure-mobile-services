// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Linq;
using System.Net;
using System.Net.Http;
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
            this.operation = new Mock<MobileServiceTableOperation>("test", "abc") { CallBase = true };            
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
        public async Task ExecuteAsync_CopiesVersion_WhenResultHasVersion()
        {
            this.operation.Protected()
                          .Setup<Task<JToken>>("OnExecuteAsync")
                          .Returns(Task.FromResult<JToken>(new JObject()));

            this.operation.Object.Result = new JObject() {{"__version", "abc"}};
            this.operation.Object.ErrorRawResult = "some result";
            this.operation.Object.ErrorStatusCode = HttpStatusCode.Accepted;
            this.operation.Object.Item = new JObject();

            await this.operation.Object.ExecuteAsync();

            Assert.AreEqual(this.operation.Object.Item["__version"], "abc");
        }

        [TestMethod]
        public async Task ExecuteAsync_DoesNotCopyVersion_WhenResultDoesNotHaveVersion()
        {
            this.operation.Protected()
                          .Setup<Task<JToken>>("OnExecuteAsync")
                          .Returns(Task.FromResult<JToken>(new JObject()));

            this.operation.Object.Result = new JObject();
            this.operation.Object.ErrorRawResult = "some result";
            this.operation.Object.ErrorStatusCode = HttpStatusCode.Accepted;
            this.operation.Object.Item = new JObject();

            await this.operation.Object.ExecuteAsync();

            Assert.AreEqual(this.operation.Object.Item.Properties().Count(), 0);
        }

        [TestMethod]
        public async Task ExecuteAsync_ResetsTheResultAndStatus()
        {
            var secondResult = new JObject();
            this.operation.Protected()
                          .Setup<Task<JToken>>("OnExecuteAsync")
                          .Returns(Task.FromResult<JToken>(secondResult));

            this.operation.Object.Result = new JObject();
            this.operation.Object.ErrorRawResult = "some result";
            this.operation.Object.ErrorStatusCode = HttpStatusCode.Accepted;
            this.operation.Object.Item = new JObject();

            await this.operation.Object.ExecuteAsync();

            Assert.AreEqual(this.operation.Object.Result, secondResult);
            Assert.IsNull(this.operation.Object.ErrorRawResult);
            Assert.IsNull(this.operation.Object.ErrorStatusCode);
        }

        [TestMethod]
        public async Task ExecuteAsync_ParsesResult_IfOperationFails()
        {            
            var result = new JObject() 
            {
                {"id", "abc"},
                {"__version", "aaaaa"}
            };
            var ex = new MobileServiceInvalidOperationException("", null, new HttpResponseMessage(HttpStatusCode.Conflict) 
                                                                        { 
                                                                            Content = new StringContent(result.ToString())
                                                                        });
            this.operation.Protected()
                          .Setup<Task<JToken>>("OnExecuteAsync")
                          .Throws(ex);

            this.operation.Object.Item = new JObject();

            var thrown = await AssertEx.Throws<MobileServiceInvalidOperationException>(() => this.operation.Object.ExecuteAsync());
            
            Assert.AreEqual(thrown, ex);
            Assert.AreEqual(operation.Object.ErrorStatusCode, thrown.Response.StatusCode);
            Assert.AreEqual(operation.Object.Result.ToString(), result.ToString());
            Assert.AreEqual(operation.Object.ErrorRawResult, result.ToString());
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
