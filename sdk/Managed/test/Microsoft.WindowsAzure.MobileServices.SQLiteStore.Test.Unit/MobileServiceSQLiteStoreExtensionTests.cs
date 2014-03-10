using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore.Test.Unit.Mocks;
using Microsoft.WindowsAzure.MobileServices.Test;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.SQLiteStore.Test.Unit
{
    [TestClass]
    public class MobileServiceSQLiteStoreExtensionTests
    {
        [TestMethod]
        public async Task DefineTable_Succeeds_WithReadonlyProperty()
        {
            var columns = new[]
            {
                new JProperty("id", String.Empty),
                new JProperty("PublicField", String.Empty),
                new JProperty("PublicProperty", String.Empty),
                new JProperty("__version", String.Empty)
            };
            await TestDefineTable<PocoType>("PocoType", columns);
        }

        [TestMethod]
        public async Task DefineTable_Succeeds_WithSystemProperties()
        {
            var columns = new[]
            {
                new JProperty("id", String.Empty),
                new JProperty("String", String.Empty),
                new JProperty("__updatedAt", default(DateTime)),
                new JProperty("__createdAt", default(DateTime)),
                new JProperty("__version", String.Empty)
            };

            await TestDefineTable<ToDoWithSystemPropertiesType>("stringId_test_table", columns);
        }

        private static async Task TestDefineTable<T>(string testTableName, JProperty[] columns)
        {
            bool defined = false;

            var storeImplMock = new MobileServiceSQLiteStoreImplMock();
            storeImplMock.CreateTableFromObjectFunc = (tableName, properties) =>
            {
                if (tableName == testTableName)
                {
                    var propertyStrings = properties.Select(p => p.Definition.ToString()).ToList();
                    defined = true;
                    CollectionAssert.AreEquivalent(columns.Select(c => c.ToString()).ToList(), propertyStrings);
                }
            };
            var store = new MobileServiceSQLiteStore(storeImplMock);

            store.DefineTable<T>();
            await store.InitializeAsync();

            Assert.IsTrue(defined);
        }
    }
}
