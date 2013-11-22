// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.MobileServices.TestFramework;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    [Tag("url")]
    [Tag("unit")]
    public class MobileServiceUrlBuilderTests : TestBase
    {
        [TestMethod]
        public void GetQueryStringTest()
        {
            var parameters = new Dictionary<string, string>() { { "x", "$y" }, { "&hello", "?good bye" }, { "a$", "b" } };
            Assert.AreEqual("x=%24y&%26hello=%3Fgood%20bye&a%24=b", MobileServiceUrlBuilder.GetQueryString(parameters));
            Assert.AreEqual(null, MobileServiceUrlBuilder.GetQueryString(null));
            Assert.AreEqual(null, MobileServiceUrlBuilder.GetQueryString(new Dictionary<string, string>()));
        }

        [TestMethod]
        public void GetQueryStringThrowsTest()
        {
            var parameters = new Dictionary<string, string>() { { "$x", "someValue" } };
            Throws<ArgumentException>(() => MobileServiceUrlBuilder.GetQueryString(parameters));
        }

        [TestMethod]
        public void CombinePathAndQueryTest()
        {
            Assert.AreEqual("somePath?x=y&a=b", MobileServiceUrlBuilder.CombinePathAndQuery("somePath", "x=y&a=b"));
            Assert.AreEqual("somePath?x=y&a=b", MobileServiceUrlBuilder.CombinePathAndQuery("somePath", "?x=y&a=b"));
            Assert.AreEqual("somePath", MobileServiceUrlBuilder.CombinePathAndQuery("somePath", null));
            Assert.AreEqual("somePath", MobileServiceUrlBuilder.CombinePathAndQuery("somePath", ""));
        }
    }
}