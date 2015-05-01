// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MobileServices.Test;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.SQLiteStore.Test.Unit
{
    [TestClass]
    public class SqlHelperTests
    {
        [TestMethod]
        public void GetStoreCastType_Throws_WhenTypeIsNotSupported()
        {
            var types = new[] { typeof(SqlHelperTests), typeof(DateTimeOffset) };

            foreach (Type type in types)
            {
                var ex = AssertEx.Throws<NotSupportedException>(() => SqlHelpers.GetStoreCastType(type));
                Assert.AreEqual("Value of type '" + type.Name + "' is not supported.", ex.Message);
            }
        }

        [TestMethod]
        public void GetStoreCastType_ReturnsCorrectType()
        {
            var data = new Dictionary<Type, string>()
            {
                { typeof(bool), SqlColumnType.Numeric },
                { typeof(DateTime), SqlColumnType.Numeric },
                { typeof(decimal), SqlColumnType.Numeric },
                { typeof(int), SqlColumnType.Integer },
                { typeof(uint), SqlColumnType.Integer },
                { typeof(long), SqlColumnType.Integer },
                { typeof(ulong), SqlColumnType.Integer },
                { typeof(short), SqlColumnType.Integer },
                { typeof(ushort), SqlColumnType.Integer },
                { typeof(byte), SqlColumnType.Integer },
                { typeof(sbyte), SqlColumnType.Integer },
                { typeof(float), SqlColumnType.Real },
                { typeof(double), SqlColumnType.Real },
                { typeof(string), SqlColumnType.Text },
                { typeof(Guid), SqlColumnType.Text },
                { typeof(byte[]), SqlColumnType.Text },
                { typeof(Uri), SqlColumnType.Text },
                { typeof(TimeSpan), SqlColumnType.Text }
            };

            foreach (var item in data)
            {
                Assert.AreEqual(SqlHelpers.GetStoreCastType(item.Key), item.Value);
            }
        }

        [TestMethod]
        public void GetStoreType_ReturnsCorrectType()
        {
            var data = new Dictionary<JTokenType, string>()
            {
                { JTokenType.Boolean, SqlColumnType.Boolean },
                { JTokenType.Integer, SqlColumnType.Integer },
                { JTokenType.Date, SqlColumnType.DateTime },
                { JTokenType.Float, SqlColumnType.Float },
                { JTokenType.String, SqlColumnType.Text },
                { JTokenType.Guid, SqlColumnType.Guid },
                { JTokenType.Array, SqlColumnType.Json },
                { JTokenType.Object, SqlColumnType.Json },
                { JTokenType.Bytes, SqlColumnType.Blob },
                { JTokenType.Uri, SqlColumnType.Uri },
                { JTokenType.TimeSpan, SqlColumnType.TimeSpan },
            };

            foreach (var item in data)
            {
                Assert.AreEqual(SqlHelpers.GetStoreType(item.Key, allowNull: false), item.Value);
            }
        }

        [TestMethod]
        public void GetStoreType_Throws_OnUnsupportedTypes()
        {
            var items = new[] { JTokenType.Comment, JTokenType.Constructor, JTokenType.None, JTokenType.Property, JTokenType.Raw, JTokenType.Undefined, JTokenType.Null };

            foreach (var type in items)
            {
                var ex = AssertEx.Throws<NotSupportedException>(() => SqlHelpers.GetStoreType(type, allowNull: false));
                Assert.AreEqual(ex.Message, String.Format("Property of type '{0}' is not supported.", type));
            }
        }

        [TestMethod]
        public void SerializeValue_LosesPrecision_WhenValueIsDate()
        {
            var original = new DateTime(635338107839470268);
            var serialized = (double)SqlHelpers.SerializeValue(new JValue(original), SqlColumnType.DateTime, JTokenType.Date);
            Assert.AreEqual(1398213983.9470000, serialized);
        }

        [TestMethod]
        public void ParseReal_LosesPrecision_WhenValueIsDate()
        {
            var date = (DateTime)SqlHelpers.DeserializeValue(1398213983.9470267, SqlColumnType.Real, JTokenType.Date);
            Assert.AreEqual(635338107839470000, date.Ticks);
        }
    }
}
