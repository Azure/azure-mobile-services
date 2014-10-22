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
                new ColumnDefinition("id", JTokenType.Integer, SqlColumnType.Integer),
                new ColumnDefinition("PublicField", JTokenType.String, SqlColumnType.Text),
                new ColumnDefinition("PublicProperty", JTokenType.String, SqlColumnType.Text)
            };
            await TestDefineTable<PocoType>("PocoType", columns);
        }

        [TestMethod]
        public async Task DefineTable_Succeeds_WithSystemProperties()
        {
            var columns = new[]
            {
                new ColumnDefinition("id", JTokenType.String, SqlColumnType.Text),
                new ColumnDefinition("String", JTokenType.String, SqlColumnType.Text),
                new ColumnDefinition("__createdAt", JTokenType.Date, SqlColumnType.Real),
                new ColumnDefinition("__updatedAt", JTokenType.Date, SqlColumnType.Real),
                new ColumnDefinition("__version", JTokenType.String, SqlColumnType.Text),
                new ColumnDefinition("__deleted", JTokenType.Boolean, SqlColumnType.Integer)
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
                new ColumnDefinition("id", JTokenType.Integer, SqlColumnType.Integer),
                new ColumnDefinition("Name", JTokenType.String, SqlColumnType.Text),
                new ColumnDefinition("Child", JTokenType.Object, SqlColumnType.Text)
            };

            await TestDefineTable<ComplexType>("ComplexType", columns);
        }

        [TestMethod]
        public async Task DefineTable_Succeeds_WithDerivedType()
        {
            var columns = new[]
            {
                new ColumnDefinition("id", JTokenType.Integer, SqlColumnType.Integer),
                new ColumnDefinition("DerivedPublicField", JTokenType.String, SqlColumnType.Text),
                new ColumnDefinition("PublicField", JTokenType.String, SqlColumnType.Text),
                new ColumnDefinition("DerivedPublicProperty", JTokenType.String, SqlColumnType.Text),
                new ColumnDefinition("PublicProperty", JTokenType.String, SqlColumnType.Text),
            };

            await TestDefineTable<PocoDerivedPocoType>("PocoDerivedPocoType", columns);
        }

        private static async Task TestDefineTable<T>(string testTableName, ColumnDefinition[] columns)
        {
            bool defined = false;

            var storeMock = new Mock<MobileServiceSQLiteStore>() { CallBase = true };

            storeMock.Setup(store => store.CreateTableFromObject(It.IsAny<string>(), It.IsAny<IEnumerable<ColumnDefinition>>()))
                     .Callback<string, IEnumerable<ColumnDefinition>>((tableName, properties) =>
                     {
                         if (tableName == testTableName)
                         {
                             defined = true;

                             CollectionAssert.AreEquivalent(columns, properties.ToList());
                         }
                     });

            storeMock.Setup(store => store.SaveSetting(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(0));

            storeMock.Object.DefineTable<T>();
            await storeMock.Object.InitializeAsync();

            Assert.IsTrue(defined);
        }
    }
}
