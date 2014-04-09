// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Test.Unit.Extensions
{
    [TestClass]
    public class JTokenExtensionTests
    {
        [TestMethod]
        public void IsValidItem_ReturnsFalse_IfObjectIsNull()
        {
            Assert.IsFalse(JTokenExtensions.IsValidItem(null));
        }

        [TestMethod]
        public void IsValidItem_ReturnsFalse_IfObjectIsNotJObject()
        {
            Assert.IsFalse(JTokenExtensions.IsValidItem(new JValue(true)));
            Assert.IsFalse(JTokenExtensions.IsValidItem(new JArray()));
        }

        [TestMethod]
        public void IsValidItem_ReturnsFalse_IfObjectIsJObjectWithoutId()
        {
            Assert.IsFalse(JTokenExtensions.IsValidItem(new JObject()));
        }

        [TestMethod]
        public void IsValidItem_ReturnsTrue_IfObjectIsJObjectWithId()
        {
            Assert.IsTrue(JTokenExtensions.IsValidItem(new JObject(){{"id", "abc"}}));
        }

        [TestMethod]
        public void ValidItemOrNull_ReturnsItem_IfItIsValid()
        {
            var item = new JObject() { { "id", "abc" } };
            Assert.AreSame(item, JTokenExtensions.ValidItemOrNull(item));
        }

        [TestMethod]
        public void ValidItemOrNull_ReturnsNull_IfItIsInValid()
        {
            var items = new JToken[] { null, new JArray(), new JValue(true), new JObject() };
            foreach (JToken item in items)
            {
                Assert.IsNull(JTokenExtensions.ValidItemOrNull(item));
            }
        }
    }
}
