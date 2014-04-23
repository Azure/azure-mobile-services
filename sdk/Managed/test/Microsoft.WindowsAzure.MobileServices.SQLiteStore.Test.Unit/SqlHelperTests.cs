// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MobileServices.Test;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.SQLiteStore.Test.Unit
{
    [TestClass]
    public class SqlHelperTests
    {
        [TestMethod]
        public void GetColumnType_Throws_WhenTypeIsNotSupported()
        {
            var types = new []{typeof(uint), typeof(long), typeof(short), typeof(ushort), typeof(DateTimeOffset)};

            foreach (Type type in types)
            {
                var ex = AssertEx.Throws<NotSupportedException>(() => SqlHelpers.GetColumnType(type));
                Assert.AreEqual("Value of type '" + type.Name + "' is not supported.", ex.Message);
            }
        }

        [TestMethod]
        public void SerializeValue_LosesPrecision_WhenValueIsDate()
        {
            var original = new DateTime(635338107839470268);
            var serialized = (double)SqlHelpers.SerializeValue(new JValue(original), SqlColumnType.Real, JTokenType.Date);
            Assert.AreEqual(1398213983.9470000, serialized);
        }

        [TestMethod] 
        public void ParseReal_LosesPrecision_WhenValueIsDate()
        {
            var date = (DateTime)SqlHelpers.ParseReal(JTokenType.Date, 1398213983.9470267);
            Assert.AreEqual(635338107839470000, date.Ticks);
        }
    }
}
