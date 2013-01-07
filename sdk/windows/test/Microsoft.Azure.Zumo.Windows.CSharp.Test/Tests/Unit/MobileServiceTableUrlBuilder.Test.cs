// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Azure.Zumo.Win8.Test;
using Microsoft.WindowsAzure.MobileServices;
using Windows.Data.Json;

namespace Microsoft.Azure.Zumo.Win8.CSharp.Test
{
    public class MobileServiceTableUrlBuilderTests : TestBase
    {
        [TestMethod]
        public void GetUriFragmentWithTableNameTest()
        {
            Assert.AreEqual("tables\\someTable", MobileServiceTableUrlBuilder.GetUriFragment("someTable"));
        }

        [TestMethod]
        public void GetUriFragmentWithTableNameAndInstanceTest()
        {
            JsonObject item = new JsonObject();
            
            item.Set("id", 5);
            Assert.AreEqual("tables\\someTable\\5", MobileServiceTableUrlBuilder.GetUriFragment("someTable", item));

            item.Set("id", 12.2);
            Assert.AreEqual("tables\\someTable\\12.2", MobileServiceTableUrlBuilder.GetUriFragment("someTable", item));

            item.Set("id", "hi");
            Assert.AreEqual("tables\\someTable\\hi", MobileServiceTableUrlBuilder.GetUriFragment("someTable", item));
        }

        [TestMethod]
        public void GetUriFragmentWithTableNameAndInstanceThrowsTest()
        {
            JsonObject item = new JsonObject();
            
            Throws<ArgumentException>(() => MobileServiceTableUrlBuilder.GetUriFragment("someTable", item));

            item.Set("not_id", 5);
            Throws<ArgumentException>(() => MobileServiceTableUrlBuilder.GetUriFragment("someTable", item));
        }

        [TestMethod]
        public void GetUriFragmentWithTableNameAndIdTest()
        {
            Assert.AreEqual("tables\\someTable\\5", MobileServiceTableUrlBuilder.GetUriFragment("someTable", 5));
            Assert.AreEqual("tables\\someTable\\12.2", MobileServiceTableUrlBuilder.GetUriFragment("someTable", 12.2));
            Assert.AreEqual("tables\\someTable\\hi", MobileServiceTableUrlBuilder.GetUriFragment("someTable", "hi"));
        }

        [TestMethod]
        public void GetQueryStringTest()
        {
            var parameters = new Dictionary<string, string>() { { "x", "$y" }, { "&hello", "?good bye!" }, { "a$", "b" } };
            Assert.AreEqual("x=%24y&%26hello=%3Fgood%20bye%21&a%24=b", MobileServiceTableUrlBuilder.GetQueryString(parameters));
            Assert.AreEqual(null, MobileServiceTableUrlBuilder.GetQueryString(null));
            Assert.AreEqual(null, MobileServiceTableUrlBuilder.GetQueryString(new Dictionary<string, string>()));
        }

        [TestMethod]
        public void GetQueryStringThrowsTest()
        {
            var parameters = new Dictionary<string, string>() { { "$x", "someValue" } };
            Throws<ArgumentException>(() => MobileServiceTableUrlBuilder.GetQueryString(parameters));
        }

        [TestMethod]
        public void CombinePathAndQueryTest()
        {
            Assert.AreEqual("somePath?x=y&a=b", MobileServiceTableUrlBuilder.CombinePathAndQuery("somePath", "x=y&a=b"));
            Assert.AreEqual("somePath?x=y&a=b", MobileServiceTableUrlBuilder.CombinePathAndQuery("somePath", "?x=y&a=b"));
            Assert.AreEqual("somePath", MobileServiceTableUrlBuilder.CombinePathAndQuery("somePath", null));
            Assert.AreEqual("somePath", MobileServiceTableUrlBuilder.CombinePathAndQuery("somePath", ""));
        }
    }
}