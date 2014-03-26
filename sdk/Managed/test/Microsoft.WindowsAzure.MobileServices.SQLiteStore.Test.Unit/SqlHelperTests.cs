// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MobileServices.Test;

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
    }
}
