// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MobileServices.Test;
using Moq;
using Newtonsoft.Json;
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
                new JProperty("id", 0),
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

        [TestMethod]
        public void DefineTable_Throws_WithTypeWithConstructor()
        {
            var storeMock = new Mock<MobileServiceSQLiteStore>() { CallBase = true };
            var ex = AssertEx.Throws<ArgumentException>(() => storeMock.Object.DefineTable<TypeWithConstructor>());
            Assert.AreEqual("The generic type T does not have parameterless constructor.", ex.Message);
        }

        [TestMethod]
        public async Task DefineTable_Succeeds_WithComplexType()
        {
            var columns = new[]
            {
                new JProperty("id", 0),
                new JProperty("Name", String.Empty),
                new JProperty("Child", new JObject())
            };

            await TestDefineTable<ComplexType>("ComplexType", columns);
        }

        [TestMethod]
        public async Task DefineTable_Succeeds_WithDerivedType()
        {
            var columns = new[]
            {
                new JProperty("id", 0),
                new JProperty("DerivedPublicField", String.Empty),
                new JProperty("PublicField", String.Empty),
                new JProperty("DerivedPublicProperty", String.Empty),
                new JProperty("PublicProperty", String.Empty),
            };

            await TestDefineTable<PocoDerivedPocoType>("PocoDerivedPocoType", columns);
        }

        private static async Task TestDefineTable<T>(string testTableName, JProperty[] columns)
        {
            bool defined = false;

            var storeMock = new Mock<MobileServiceSQLiteStore>() { CallBase = true };

            storeMock.Setup(store => store.CreateTableFromObject(It.IsAny<string>(), It.IsAny<IEnumerable<ColumnDefinition>>()))
                     .Callback<string, IEnumerable<ColumnDefinition>>((tableName, properties) =>
                    {
                        if (tableName == testTableName)
                        {
                            var expectedProperties = columns.Select(c => c.ToString(Formatting.None)).ToList();
                            var actualProperties = properties.Select(p => p.Property.ToString(Formatting.None)).ToList();
                            defined = true;
                            CollectionAssert.AreEquivalent(expectedProperties, actualProperties);
                        }
                    });

            storeMock.Setup(store => store.SaveSetting(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(0));

            storeMock.Object.DefineTable<T>();
            await storeMock.Object.InitializeAsync();

            Assert.IsTrue(defined);
        }
    }
}
