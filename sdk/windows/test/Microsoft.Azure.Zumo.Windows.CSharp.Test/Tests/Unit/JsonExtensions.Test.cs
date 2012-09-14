// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Zumo.Win8.Test;
using Microsoft.WindowsAzure.MobileServices;
using Windows.Data.Json;

namespace Microsoft.Azure.Zumo.Win8.CSharp.Test
{
    public class JsonExtensionsTests : TestBase
    {
        [TestMethod]
        public void AsObject()
        {
            IJsonValue value = null;
            Assert.IsNull(value.AsObject());
            Assert.IsNull(JsonExtensions.Null().AsObject());
            Assert.IsNull(JsonValue.CreateStringValue("asdfafd").AsObject());
            Assert.IsNull(JsonValue.CreateNumberValue(2.0).AsObject());
            Assert.IsNull(JsonValue.CreateBooleanValue(true).AsObject());
            Assert.IsNull(new JsonArray().AsObject());
            JsonObject obj = new JsonObject();
            value = obj;
            Assert.AreEqual(obj, obj.AsObject());
            Assert.AreEqual(obj, value);
        }

        [TestMethod]
        public void AsString()
        {
            IJsonValue value = null;
            Assert.IsNull(value.AsString());
            Assert.IsNull(JsonExtensions.Null().AsString());
            Assert.IsNull(new JsonObject().AsString());
            Assert.IsNull(JsonValue.CreateNumberValue(2.0).AsString());
            Assert.IsNull(JsonValue.CreateBooleanValue(true).AsString());
            Assert.IsNull(new JsonArray().AsString());
            Assert.AreEqual("123", JsonValue.CreateStringValue("123").AsString());
            value = JsonValue.CreateStringValue("ABC");
            Assert.AreEqual("ABC", value.AsString());
        }

        [TestMethod]
        public void AsNumber()
        {
            IJsonValue value = null;
            Assert.IsNull(value.AsNumber());
            Assert.IsNull(JsonExtensions.Null().AsNumber());
            Assert.IsNull(new JsonObject().AsNumber());
            Assert.IsNull(JsonValue.CreateStringValue("2.0").AsNumber());
            Assert.IsNull(JsonValue.CreateBooleanValue(true).AsNumber());
            Assert.IsNull(new JsonArray().AsNumber());
            Assert.AreEqual(2.0, JsonValue.CreateNumberValue(2).AsNumber());
        }

        [TestMethod]
        public void AsInteger()
        {
            IJsonValue value = null;
            Assert.IsNull(value.AsInteger());
            Assert.IsNull(JsonExtensions.Null().AsInteger());
            Assert.IsNull(new JsonObject().AsInteger());
            Assert.IsNull(JsonValue.CreateStringValue("2.0").AsInteger());
            Assert.IsNull(JsonValue.CreateBooleanValue(true).AsInteger());
            Assert.IsNull(new JsonArray().AsInteger());
            Assert.AreEqual(2, JsonValue.CreateNumberValue(2).AsInteger());
            Assert.AreEqual(2, JsonValue.CreateNumberValue(2.2).AsInteger());
        }

        [TestMethod]
        public void AsBool()
        {
            IJsonValue value = null;
            Assert.IsNull(value.AsBool());
            Assert.IsNull(JsonExtensions.Null().AsBool());
            Assert.IsNull(new JsonObject().AsBool());
            Assert.IsNull(JsonValue.CreateNumberValue(2.0).AsBool());
            Assert.IsNull(JsonValue.CreateStringValue("2.0").AsBool());
            Assert.IsNull(new JsonArray().AsBool());
            Assert.AreEqual(true, JsonValue.CreateBooleanValue(true).AsBool());
        }

        [TestMethod]
        public void AsArray()
        {
            IJsonValue value = null;
            Assert.IsNull(value.AsArray());
            Assert.IsNull(JsonExtensions.Null().AsArray());
            Assert.IsNull(new JsonObject().AsArray());
            Assert.IsNull(JsonValue.CreateBooleanValue(true).AsArray());
            Assert.IsNull(JsonValue.CreateNumberValue(2.0).AsArray());
            Assert.IsNull(JsonValue.CreateStringValue("2.0").AsArray());
            Assert.IsNotNull(new JsonArray().AsArray());
        }

        [TestMethod]
        public void IsNull()
        {
            IJsonValue value = null;
            Assert.IsTrue(value.IsNull());
            Assert.IsTrue(JsonExtensions.Null().IsNull());
            Assert.IsFalse(new JsonObject().IsNull());
            Assert.IsFalse(JsonValue.CreateStringValue("").IsNull());
            Assert.IsFalse(JsonValue.CreateNumberValue(0).IsNull());
        }
        
        [TestMethod]
        public void GetPropertyValues()
        {
            JsonObject obj = null;
            Assert.AreEqual(0, obj.GetPropertyValues().Count());

            obj = new JsonObject()
                .Set("b", true)
                .Set("n", 2.0)
                .Set("s", "text");
            
            IDictionary<string, JsonValue> values = 
                obj.GetPropertyValues().ToDictionary(p => p.Key, p => p.Value);
            Assert.AreEqual(3, values.Count);
            Assert.AreEqual(true, values["b"].AsBool());
            Assert.AreEqual(2.0, values["n"].AsNumber());
            Assert.AreEqual("text", values["s"].AsString());
        }

        [TestMethod]
        public void Get()
        {
            IJsonValue obj = null;
            Assert.IsNull(obj.Get("fail"));
            obj = new JsonObject()
                .Set("a", "apple")
                .Set("b", "banana")
                .Set("c", "cherry");
            Assert.AreEqual("apple", obj.Get("a").AsString());
            Assert.AreEqual("banana", obj.Get("b").AsString());
            Assert.AreEqual("cherry", obj.Get("c").AsString());
            Assert.IsNull(obj.Get("d"));
        }

        [TestMethod]
        public void TryConvert()
        {
            object value = null;

            JsonValue.CreateBooleanValue(true).TryConvert(out value);
            Assert.AreEqual(true, value);

            JsonValue.CreateNumberValue(2.0).TryConvert(out value);
            Assert.AreEqual(2.0, value);

            JsonValue.CreateStringValue("abc").TryConvert(out value);
            Assert.AreEqual("abc", value);

            new JsonObject().TryConvert(out value);
            Assert.IsNull(value);

            new JsonArray().TryConvert(out value);
            Assert.IsNull(value);
        }

        [TestMethod]
        public void Set()
        {
            JsonObject value = null;
            value.Set("silent", "fail");

            value = new JsonObject();
            
            value.Set("b", true);
            Assert.AreEqual(true, value.Get("b").AsBool());

            value.Set("c", 12);
            Assert.AreEqual(12, value.Get("c").AsInteger());

            value.Set("d", 12.2);
            Assert.AreEqual(12.2, value.Get("d").AsNumber());
            Assert.AreEqual(12, value.Get("d").AsInteger());

            value.Set("e", "abc");
            Assert.AreEqual("abc", value.Get("e").AsString());

            value.Set("f", new JsonObject());
            Assert.IsNotNull(value.Get("f"));
        }

        [TestMethod]
        public void TrySet()
        {
            JsonObject obj = null;
            Assert.IsFalse(obj.TrySet("fail", null));

            obj = new JsonObject();

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
            JsonArray array = null;
            array.Append(JsonValue.CreateStringValue("test"));

            array = new JsonArray()
                .Append(JsonValue.CreateBooleanValue(true))
                .Append(JsonValue.CreateStringValue("test"));
            Assert.AreEqual(2, array.Count);
        }
    }
}
