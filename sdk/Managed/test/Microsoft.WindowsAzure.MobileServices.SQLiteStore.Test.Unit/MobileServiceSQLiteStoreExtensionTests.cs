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
        public async Task DefineTable_Succeeds_WithAllTypes_Generic()
        {
            var columns = new[]
            {
                new ColumnDefinition("id", JTokenType.String, SqlColumnType.Text),
                new ColumnDefinition("__createdAt", JTokenType.Date, SqlColumnType.DateTime),
                new ColumnDefinition("__updatedAt", JTokenType.Date, SqlColumnType.DateTime),
                new ColumnDefinition("__version", JTokenType.String, SqlColumnType.Text),
                new ColumnDefinition("Bool", JTokenType.Boolean, SqlColumnType.Boolean),
                new ColumnDefinition("Byte", JTokenType.Integer, SqlColumnType.Integer),
                new ColumnDefinition("SByte", JTokenType.Integer, SqlColumnType.Integer),
                new ColumnDefinition("UShort", JTokenType.Integer, SqlColumnType.Integer),
                new ColumnDefinition("Short", JTokenType.Integer, SqlColumnType.Integer),
                new ColumnDefinition("UInt", JTokenType.Integer, SqlColumnType.Integer),
                new ColumnDefinition("Int", JTokenType.Integer, SqlColumnType.Integer),
                new ColumnDefinition("ULong", JTokenType.Integer, SqlColumnType.Integer),
                new ColumnDefinition("Long", JTokenType.Integer, SqlColumnType.Integer),
                new ColumnDefinition("Float", JTokenType.Float, SqlColumnType.Float),
                new ColumnDefinition("Double", JTokenType.Float, SqlColumnType.Float),
                new ColumnDefinition("Decimal", JTokenType.Float, SqlColumnType.Float),
                new ColumnDefinition("String", JTokenType.String, SqlColumnType.Text),
                new ColumnDefinition("Char", JTokenType.String, SqlColumnType.Text),
                new ColumnDefinition("DateTime", JTokenType.Date, SqlColumnType.DateTime),
                new ColumnDefinition("DateTimeOffset", JTokenType.Date, SqlColumnType.DateTime),
                new ColumnDefinition("Nullable", JTokenType.Float, SqlColumnType.Float),
                new ColumnDefinition("NullableDateTime", JTokenType.Date, SqlColumnType.DateTime),
                new ColumnDefinition("TimeSpan", JTokenType.String, SqlColumnType.Text),
                new ColumnDefinition("Uri", JTokenType.String, SqlColumnType.Text),
                new ColumnDefinition("Enum1", JTokenType.String, SqlColumnType.Text),
                new ColumnDefinition("Enum2", JTokenType.String, SqlColumnType.Text),
                new ColumnDefinition("Enum3", JTokenType.String, SqlColumnType.Text),
                new ColumnDefinition("Enum4", JTokenType.String, SqlColumnType.Text),
                new ColumnDefinition("Enum5", JTokenType.String, SqlColumnType.Text),
                new ColumnDefinition("Enum6", JTokenType.String, SqlColumnType.Text)
            };
            await TestDefineTable<AllBaseTypesWithAllSystemPropertiesType>("AllBaseTypesWithAllSystemPropertiesType", columns);
        }

        [TestMethod]
        public async Task DefineTable_Succeeds_WithAllTypes()
        {
            var item = JObjectTypes.GetObjectWithAllTypes();

            var columns = new[]
            {
                new ColumnDefinition("id", JTokenType.String, SqlColumnType.Text),
                new ColumnDefinition("Object", JTokenType.Object, SqlColumnType.Json),
                new ColumnDefinition("Array", JTokenType.Array, SqlColumnType.Json),
                new ColumnDefinition("Integer", JTokenType.Integer, SqlColumnType.Integer),
                new ColumnDefinition("Float", JTokenType.Float, SqlColumnType.Float),
                new ColumnDefinition("String", JTokenType.String, SqlColumnType.Text),
                new ColumnDefinition("Boolean", JTokenType.Boolean, SqlColumnType.Boolean),
                new ColumnDefinition("Date", JTokenType.Date, SqlColumnType.DateTime),
                new ColumnDefinition("Bytes", JTokenType.Bytes, SqlColumnType.Blob),
                new ColumnDefinition("Guid", JTokenType.Guid, SqlColumnType.Guid),
                new ColumnDefinition("TimeSpan", JTokenType.TimeSpan, SqlColumnType.TimeSpan)
            };

            await TestDefineTable(item, "AllTypes", columns);
        }

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
                new ColumnDefinition("__createdAt", JTokenType.Date, SqlColumnType.DateTime),
                new ColumnDefinition("__updatedAt", JTokenType.Date, SqlColumnType.DateTime),
                new ColumnDefinition("__version", JTokenType.String, SqlColumnType.Text),
                new ColumnDefinition("__deleted", JTokenType.Boolean, SqlColumnType.Boolean)
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
                new ColumnDefinition("Child", JTokenType.Object, SqlColumnType.Json)
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
            await TestDefineTable(testTableName, columns, store => store.DefineTable<T>());
        }

        private static async Task TestDefineTable(JObject item, string testTableName, ColumnDefinition[] columns)
        {
            await TestDefineTable(testTableName, columns, store => store.DefineTable(testTableName, item));
        }

        private static async Task TestDefineTable(string testTableName, ColumnDefinition[] columns, Action<MobileServiceSQLiteStore> defineAction)
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

            defineAction(storeMock.Object);
            await storeMock.Object.InitializeAsync();

            Assert.IsTrue(defined);
        }
    }
}
