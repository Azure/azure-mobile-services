// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Zumo.WindowsPhone8.Test;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Zumo.WindowsPhone8.CSharp.Test
{
    public class JsonExtensionsTests : TestBase
    {
        [TestMethod]
        public void AsObject()
        {
            JToken value = null;
            Assert.IsNull(value.AsObject());
            Assert.IsNull(JsonExtensions.Null().AsObject());
            Assert.IsNull(new JValue("asdfafd").AsObject());
            Assert.IsNull(new JValue(2.0).AsObject());
            Assert.IsNull(new JValue(true).AsObject());
            Assert.IsNull(new JArray().AsObject());
            JObject obj = new JObject();
            value = obj;
            Assert.AreEqual(obj, obj.AsObject());
            Assert.AreEqual(obj, value);
        }

        [TestMethod]
        public void AsString()
        {
            JValue value = null;
            Assert.IsNull(value.AsString());
            Assert.IsNull(JsonExtensions.Null().AsString());
            Assert.IsNull(new JObject().AsString());
            Assert.IsNull(new JValue(2.0).AsString());
            Assert.IsNull(new JValue(true).AsString());
            Assert.IsNull(new JArray().AsString());
            Assert.AreEqual("123", new JValue("123").AsString());
            value = new JValue("ABC");
            Assert.AreEqual("ABC", value.AsString());
        }

        [TestMethod]
        public void AsNumber()
        {
            JToken value = null;
            Assert.IsNull(value.AsNumber());
            Assert.IsNull(JsonExtensions.Null().AsNumber());
            Assert.IsNull(new JObject().AsNumber());
            Assert.IsNull(new JValue("2.0").AsNumber());
            Assert.IsNull(new JValue(true).AsNumber());
            Assert.IsNull(new JArray().AsNumber());
            Assert.AreEqual(2.0, new JValue(2).AsNumber());
        }

        [TestMethod]
        public void AsInteger()
        {
            JToken value = null;
            Assert.IsNull(value.AsInteger());
            Assert.IsNull(JsonExtensions.Null().AsInteger());
            Assert.IsNull(new JObject().AsInteger());
            Assert.IsNull(new JValue("2.0").AsInteger());
            Assert.IsNull(new JValue(true).AsInteger());
            Assert.IsNull(new JArray().AsInteger());
            Assert.AreEqual(2, new JValue(2).AsInteger());
            Assert.AreEqual(2, new JValue(2.2).AsInteger());
        }

        [TestMethod]
        public void AsBool()
        {
            JToken value = null;
            Assert.IsNull(value.AsBool());
            Assert.IsNull(JsonExtensions.Null().AsBool());
            Assert.IsNull(new JObject().AsBool());
            Assert.IsNull(new JValue(2.0).AsBool());
            Assert.IsNull(new JValue("2.0").AsBool());
            Assert.IsNull(new JArray().AsBool());
            Assert.AreEqual(true, new JValue(true).AsBool());
        }

        [TestMethod]
        public void AsArray()
        {
            JToken value = null;
            Assert.IsNull(value.AsArray());
            Assert.IsNull(JsonExtensions.Null().AsArray());
            Assert.IsNull(new JObject().AsArray());
            Assert.IsNull(new JValue(true).AsArray());
            Assert.IsNull(new JValue(2.0).AsArray());
            Assert.IsNull(new JValue("2.0").AsArray());
            Assert.IsNotNull(new JArray().AsArray());
        }

        [TestMethod]
        public void IsNull()
        {
            JToken value = null;
            Assert.IsTrue(value.IsNull());
            Assert.IsTrue(JsonExtensions.Null().IsNull());
            Assert.IsFalse(new JObject().IsNull());
            Assert.IsFalse(new JValue("").IsNull());
            Assert.IsFalse(new JValue(0).IsNull());
        }
        
        [TestMethod]
        public void GetPropertyValues()
        {
            JObject obj = null;
            Assert.AreEqual(0, obj.GetPropertyValues().Count());

            obj = new JObject()
                .Set("b", true)
                .Set("n", 2.0)
                .Set("s", "text") as JObject;
            
            IDictionary<string, JToken> values = 
                obj.GetPropertyValues().ToDictionary(p => p.Key, p => p.Value);
            Assert.AreEqual(3, values.Count);
            Assert.AreEqual(true, values["b"].AsBool());
            Assert.AreEqual(2.0, values["n"].AsNumber());
            Assert.AreEqual("text", values["s"].AsString());
        }

        [TestMethod]
        public void Get()
        {
            JObject obj = null;
            Assert.IsNull(obj.Get("fail"));
            obj = new JObject()
                .Set("a", "apple")
                .Set("b", "banana")
                .Set("c", "cherry") as JObject;
            Assert.AreEqual("apple", obj.Get("a").AsString());
            Assert.AreEqual("banana", obj.Get("b").AsString());
            Assert.AreEqual("cherry", obj.Get("c").AsString());
            Assert.IsNull(obj.Get("d"));
        }

        [TestMethod]
        public void TryConvert()
        {
            object value = null;

            new JValue(true).TryConvert(out value);
            Assert.AreEqual(true, value);

            new JValue(2.0).TryConvert(out value);
            Assert.AreEqual(2.0, value);

            new JValue("abc").TryConvert(out value);
            Assert.AreEqual("abc", value);

            new JObject().TryConvert(out value);
            Assert.IsNull(value);

            new JArray().TryConvert(out value);
            Assert.IsNull(value);
        }

        [TestMethod]
        public void Set()
        {
            JObject value = null;
            value.Set("silent", "fail");

            value = new JObject();
            
            value.Set("b", true);
            Assert.AreEqual(true, value.Get("b").AsBool());

            value.Set("c", 12);
            Assert.AreEqual(12, value.Get("c").AsInteger());

            value.Set("d", 12.2);
            Assert.AreEqual(12.2, value.Get("d").AsNumber());
            Assert.AreEqual(12, value.Get("d").AsInteger());

            value.Set("e", "abc");
            Assert.AreEqual("abc", value.Get("e").AsString());

            value.Set("f", new JObject());
            Assert.IsNotNull(value.Get("f"));
        }

        [TestMethod]
        public void TrySet()
        {
            JObject obj = null;
            Assert.IsFalse(obj.TrySet("fail", null));

            obj = new JObject();

            Assert.IsTrue(obj.TrySet("x", null));
            Assert.IsTrue(obj.Get("x").IsNull());

            Assert.IsTrue(obj.TrySet("x", true));
            Assert.AreEqual(true, obj.Get("x").AsBool());

            Assert.IsTrue(obj.TrySet("x", 1));
            Assert.AreEqual(1.0, obj.Get("x").AsNumber());

            Assert.IsTrue(obj.TrySet("x", 1.0));
            Assert.AreEqual(1.0, obj.Get("x").AsNumber());

            Assert.IsTrue(obj.TrySet("x", 1.0f));
            Assert.AreEqual(1.0, obj.Get("x").AsNumber());

            Assert.IsTrue(obj.TrySet("x", 'x'));
            Assert.AreEqual("x", obj.Get("x").AsString());

            Assert.IsTrue(obj.TrySet("x", "abc"));
            Assert.AreEqual("abc", obj.Get("x").AsString());

            DateTime now = DateTime.Now;
            Assert.IsTrue(obj.TrySet("x", now));
            Assert.AreEqual(
                now.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffK", CultureInfo.InvariantCulture),
                obj.Get("x").AsString());

            DateTimeOffset offset = DateTimeOffset.Now;
            Assert.IsTrue(obj.TrySet("x", offset));
            Assert.AreEqual(
                offset.DateTime.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffK", CultureInfo.InvariantCulture),
                obj.Get("x").AsString());

            Assert.IsFalse(obj.TrySet("x", new Uri("http://www.microsoft.com")));
        }

        [TestMethod]
        public void Append()
        {
            JArray array = null;
            array.Append(new JValue("test"));

            array = new JArray()
                .Append(new JValue(true))
                .Append(new JValue("test"));
            Assert.AreEqual(2, array.Count);
        }
    }
}
