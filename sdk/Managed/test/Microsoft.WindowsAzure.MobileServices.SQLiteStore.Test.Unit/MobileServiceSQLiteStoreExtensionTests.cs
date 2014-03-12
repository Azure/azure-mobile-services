using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MobileServices.Test;
using Moq;
using Moq.Protected;
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
                new JProperty("PublicProperty", String.Empty)
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
                new JProperty("__createdAt", default(DateTime).ToUniversalTime()),
                new JProperty("__updatedAt", default(DateTime).ToUniversalTime()),
                new JProperty("__version", String.Empty)
            };

            await TestDefineTable<ToDoWithSystemPropertiesType>("stringId_test_table", columns);
        }

        private static async Task TestDefineTable<T>(string testTableName, JProperty[] columns)
        {
            bool defined = false;

            var storeMock = new Mock<MobileServiceSQLiteStore>(MockBehavior.Strict) { CallBase = true };

            storeMock.Setup(store => store.CreateTableFromObject(It.IsAny<string>(), It.IsAny<IEnumerable<ColumnDefinition>>()))
                     .Callback<string, IEnumerable<ColumnDefinition>>((tableName, properties) =>
                    {
                        if (tableName == testTableName)
                        {
                            var expectedProperties = columns.Select(c => c.ToString()).ToList();
                            var actualProperties = properties.Select(p => p.Definition.ToString()).ToList();
                            defined = true;
                            CollectionAssert.AreEquivalent(expectedProperties, actualProperties);
                        }
                    });

            storeMock.Object.DefineTable<T>();
            await storeMock.Object.InitializeAsync();

            Assert.IsTrue(defined);
        }
    }
}
