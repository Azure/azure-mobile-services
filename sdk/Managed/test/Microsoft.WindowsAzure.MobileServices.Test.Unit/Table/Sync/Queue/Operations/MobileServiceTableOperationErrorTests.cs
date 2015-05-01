using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Test.Unit.Table.Sync.Queue.Operations
{
    [TestClass]
    public class MobileServiceTableOperationErrorTests
    {
        private MobileServiceSerializer serializer;

        [TestInitialize]
        public void Initialize()
        {
            this.serializer = new MobileServiceSerializer();
        }

        [TestMethod]
        public void Deserialize_Succeeds()
        {
            var serializedError = JObject.Parse(@"
            {""id"":""70cf6cc2-5981-4a32-ae6c-249572917a46"",
            ""httpStatus"": 200,
            ""operationVersion"":123,
            ""operationKind"":0,
            ""tableName"":""test"",
            ""tableKind"":1,
            ""item"":""{\""id\"":\""abc\"",\""text\"":\""example\""}"",
            ""rawResult"":""{\""id\"":\""abc\"",\""text\"":\""example\""}""
            }");
            var operation = MobileServiceTableOperationError.Deserialize(serializedError, this.serializer.SerializerSettings);

            Assert.AreEqual(serializedError["id"], operation.Id);
            Assert.AreEqual(serializedError["operationVersion"], operation.OperationVersion);
            Assert.AreEqual(serializedError["operationKind"], (int)operation.OperationKind);
            Assert.AreEqual(serializedError["httpStatus"], (int)operation.Status);
            Assert.AreEqual(serializedError["tableName"], operation.TableName);
            Assert.AreEqual(serializedError["tableKind"], (int)operation.TableKind);
            Assert.AreEqual(serializedError["item"], operation.Item.ToString(Formatting.None));
            Assert.AreEqual(serializedError["rawResult"], operation.RawResult);
        }

        [TestMethod]
        public void Deserialize_Succeeds_WhenOperationVersionIsNull()
        {
            var serializedError = JObject.Parse(@"
            {""id"":""70cf6cc2-5981-4a32-ae6c-249572917a46"",
            ""httpStatus"": 200,
            ""operationVersion"":null,
            ""operationKind"":0,
            ""tableName"":""test"",
            ""tableKind"":1,
            ""item"":""{\""id\"":\""abc\"",\""text\"":\""example\""}"",
            ""rawResult"":""{\""id\"":\""abc\"",\""text\"":\""example\""}""
            }");
            var operation = MobileServiceTableOperationError.Deserialize(serializedError, this.serializer.SerializerSettings);

            Assert.AreEqual(serializedError["id"], operation.Id);
            Assert.AreEqual(0, operation.OperationVersion);
            Assert.AreEqual(serializedError["operationKind"], (int)operation.OperationKind);
            Assert.AreEqual(serializedError["httpStatus"], (int)operation.Status);
            Assert.AreEqual(serializedError["tableName"], operation.TableName);
            Assert.AreEqual(serializedError["tableKind"], (int)operation.TableKind);
            Assert.AreEqual(serializedError["item"], operation.Item.ToString(Formatting.None));
            Assert.AreEqual(serializedError["rawResult"], operation.RawResult);
        }
    }
}
