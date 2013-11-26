// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using ZumoE2ETestApp.Framework;
using ZumoE2ETestApp.Tests.Types;

namespace ZumoE2ETestApp.Tests
{
    public static class ZumoRoundTripTests
    {
        public static ZumoTestGroup CreateTests()
        {
            ZumoTestGroup result = new ZumoTestGroup("Round trip tests");
            result.AddTest(CreateSetupSchemaTest());

            DateTime now = DateTime.UtcNow;
            int seed = now.Year * 10000 + now.Month * 100 + now.Day;
            Random rndGen = new Random(seed);

            result.AddTest(CreateSimpleTypedRoundTripTest("String: Empty", RoundTripTestType.String, ""));
            result.AddTest(CreateSimpleTypedRoundTripTest("String: null", RoundTripTestType.String, null));
            result.AddTest(CreateSimpleTypedRoundTripTest("String: random value",
                RoundTripTestType.String, Util.CreateSimpleRandomString(rndGen, 10)));
            result.AddTest(CreateSimpleTypedRoundTripTest("String: large (1000 characters)", RoundTripTestType.String, new string('*', 1000)));
            result.AddTest(CreateSimpleTypedRoundTripTest("String: large (64k+1 characters)", RoundTripTestType.String, new string('*', 65537)));

            result.AddTest(CreateSimpleTypedRoundTripTest("String: non-ASCII characters - Latin", RoundTripTestType.String, "ãéìôü ÇñÑ"));
            result.AddTest(CreateSimpleTypedRoundTripTest("String: non-ASCII characters - Arabic", RoundTripTestType.String, "الكتاب على الطاولة"));
            result.AddTest(CreateSimpleTypedRoundTripTest("String: non-ASCII characters - Chinese", RoundTripTestType.String, "这本书在桌子上"));
            result.AddTest(CreateSimpleTypedRoundTripTest("String: non-ASCII characters - Chinese 2", RoundTripTestType.String, "⒈①Ⅻㄨㄩ 啊阿鼾齄 丂丄狚狛 狜狝﨨﨩 ˊˋ˙–〇 㐀㐁䶴䶵"));
            result.AddTest(CreateSimpleTypedRoundTripTest("String: non-ASCII characters - Japanese", RoundTripTestType.String, "本は机の上に"));
            result.AddTest(CreateSimpleTypedRoundTripTest("String: non-ASCII characters - Hebrew", RoundTripTestType.String, "הספר הוא על השולחן"));
            result.AddTest(CreateSimpleTypedRoundTripTest("String: non-ASCII characters - Russian", RoundTripTestType.String, "Книга лежит на столе"));

            result.AddTest(CreateSimpleTypedRoundTripTest("Date: now", RoundTripTestType.Date, Util.TrimSubMilliseconds(DateTime.Now)));
            result.AddTest(CreateSimpleTypedRoundTripTest("Date: now (UTC)", RoundTripTestType.Date, Util.TrimSubMilliseconds(DateTime.UtcNow)));
            result.AddTest(CreateSimpleTypedRoundTripTest("Date: null", RoundTripTestType.Date, null));
            result.AddTest(CreateSimpleTypedRoundTripTest("Date: min date", RoundTripTestType.Date, DateTime.MinValue));
            result.AddTest(CreateSimpleTypedRoundTripTest("Date: specific date, before unix 0", RoundTripTestType.Date, new DateTime(1901, 1, 1)));
            result.AddTest(CreateSimpleTypedRoundTripTest("Date: specific date, after unix 0", RoundTripTestType.Date, new DateTime(2000, 12, 31)));

            result.AddTest(CreateSimpleTypedRoundTripTest("Bool: true", RoundTripTestType.Bool, true));
            result.AddTest(CreateSimpleTypedRoundTripTest("Bool: false", RoundTripTestType.Bool, false));
            result.AddTest(CreateSimpleTypedRoundTripTest("Bool: null", RoundTripTestType.Bool, null));

            result.AddTest(CreateSimpleTypedRoundTripTest("Int: zero", RoundTripTestType.Int, 0));
            result.AddTest(CreateSimpleTypedRoundTripTest("Int: MaxValue", RoundTripTestType.Int, int.MaxValue));
            result.AddTest(CreateSimpleTypedRoundTripTest("Int: MinValue", RoundTripTestType.Int, int.MinValue));

            result.AddTest(CreateSimpleTypedRoundTripTest("Long: zero", RoundTripTestType.Long, 0L));
            long maxAllowedValue = 0x0020000000000000;
            long minAllowedValue = 0;
            unchecked
            {
                minAllowedValue = (long)0xFFE0000000000000;
            }

            result.AddTest(CreateSimpleTypedRoundTripTest("Long: max allowed", RoundTripTestType.Long, maxAllowedValue));
            result.AddTest(CreateSimpleTypedRoundTripTest("Long: min allowed", RoundTripTestType.Long, minAllowedValue));
            long largePositiveValue = maxAllowedValue - rndGen.Next(0, int.MaxValue);
            long largeNegativeValue = minAllowedValue + rndGen.Next(0, int.MaxValue);
            result.AddTest(CreateSimpleTypedRoundTripTest("Long: large value, less than max allowed (" + largePositiveValue + ")", RoundTripTestType.Long, largePositiveValue));
            result.AddTest(CreateSimpleTypedRoundTripTest("Long: large negative value, more than min allowed (" + largeNegativeValue + ")", RoundTripTestType.Long, largeNegativeValue));

            result.AddTest(CreateSimpleTypedRoundTripTest<InvalidOperationException>("(Neg) Long: more than max allowed", RoundTripTestType.Long, maxAllowedValue + 1));
            result.AddTest(CreateSimpleTypedRoundTripTest<InvalidOperationException>("(Neg) Long: less than min allowed", RoundTripTestType.Long, minAllowedValue - 1));

            result.AddTest(CreateSimpleTypedRoundTripTest("Enum (with JSON converter): simple value", RoundTripTestType.Enum, EnumType.Second));

            result.AddTest(CreateSimpleTypedRoundTripTest(
                "Complex type (custom table serialization): simple value",
                RoundTripTestType.ComplexWithCustomSerialization,
                new ComplexType2(rndGen)));
            result.AddTest(CreateSimpleTypedRoundTripTest(
                "Complex type (custom table serialization): null",
                RoundTripTestType.ComplexWithCustomSerialization,
                null));

            result.AddTest(CreateSimpleTypedRoundTripTest(
                "Complex type (converter): empty array",
                RoundTripTestType.ComplexWithConverter,
                new ComplexType[0]));
            result.AddTest(CreateSimpleTypedRoundTripTest(
                "Complex type (converter): 1-element array",
                RoundTripTestType.ComplexWithConverter,
                new ComplexType[] { new ComplexType(rndGen) }));
            result.AddTest(CreateSimpleTypedRoundTripTest(
                "Complex type (converter): multi-element array",
                RoundTripTestType.ComplexWithConverter,
                new ComplexType[] { new ComplexType(rndGen), null, new ComplexType(rndGen) }));
            result.AddTest(CreateSimpleTypedRoundTripTest(
                "Complex type (converter): null array",
                RoundTripTestType.ComplexWithConverter,
                null));

            result.AddTest(
                CreateSimpleTypedRoundTripTest<ArgumentException>(
                    "(Neg) Insert item with non-default id", RoundTripTestType.Id, 1));

            var uniqueId = Environment.TickCount.ToString(CultureInfo.InvariantCulture);
            var differentIds = new Dictionary<string, string>
            {
                { "none", null },
                { "ascii", "myid" },
                { "latin", "ãéìôü ÇñÑ" },
                { "arabic", "الكتاب على الطاولة" },
                { "chinese", "这本书在桌子上" },
                { "hebrew", "הספר הוא על השולחן" }
            };

            foreach (var name in differentIds.Keys)
            {
                var id = differentIds[name];
                result.AddTest(new ZumoTest("String id - " + name + " id on insert", async delegate(ZumoTest test) {
                    var item = new StringIdRoundTripTableItem(rndGen);
                    var itemId = id;
                    if (itemId != null)
                    {
                        itemId = itemId + "-" + Guid.NewGuid().ToString();
                    }

                    item.Id = itemId;
                    var client = ZumoTestGlobals.Instance.Client;
                    var table = client.GetTable<StringIdRoundTripTableItem>();
                    await table.InsertAsync(item);
                    test.AddLog("Inserted item with id = {0}", item.Id);
                    if (id != null && itemId != item.Id)
                    {
                        test.AddLog("Error, id passed to insert is not the same ({0}) as the id returned by the server ({1})", id, item.Id);
                        return false;
                    }

                    var retrieved = await table.LookupAsync(item.Id);
                    test.AddLog("Retrieved item: {0}", retrieved);
                    if (!item.Equals(retrieved))
                    {
                        test.AddLog("Error, round-tripped item is different");
                        return false;
                    }

                    test.AddLog("Now trying to insert an item with an existing id (should fail)");
                    try
                    {
                        await table.InsertAsync(new StringIdRoundTripTableItem { Id = retrieved.Id, Name = "should not work" });
                        test.AddLog("Error, insertion succeeded but it should have failed");
                        return false;
                    }
                    catch (MobileServiceInvalidOperationException e)
                    {
                        test.AddLog("Caught expected exception: {0}", e);
                    }

                    test.AddLog("Cleaning up...");
                    await table.DeleteAsync(retrieved);
                    test.AddLog("Removed the inserted item");
                    return true;
                }));
            }

            var invalidIds = new string[] { ".", "..", "control\u0010characters", "large id " + new string('*', 260) };
            foreach (var id in invalidIds)
            {
                result.AddTest(new ZumoTest("(Neg) string id - insert with invalid id: " + (id.Length > 30 ? (id.Substring(0, 30) + "...") : id), async delegate(ZumoTest test)
                {
                    var client = ZumoTestGlobals.Instance.Client;
                    var table = client.GetTable<StringIdRoundTripTableItem>();
                    var item = new StringIdRoundTripTableItem { Id = id, Name = "should not work" };
                    try
                    {
                        await table.InsertAsync(item);
                        test.AddLog("Error, insert operation should have failed. Inserted id = {0}", item.Id);
                        return false;
                    }
                    catch (InvalidOperationException ex)
                    {
                        test.AddLog("Caught expected exception: {0}", ex);
                        return true;
                    }
                }));
            }

            // Untyped overloads
            result.AddTest(CreateSimpleUntypedRoundTripTest("Untyped String: Empty", "string1", ""));
            result.AddTest(CreateSimpleUntypedRoundTripTest("Untyped String: null", "string1", null));
            result.AddTest(CreateSimpleUntypedRoundTripTest("Untyped String: random value",
                "string1", Util.CreateSimpleRandomString(rndGen, 10)));
            result.AddTest(CreateSimpleUntypedRoundTripTest("Untyped String: large (1000 characters)", "string1", new string('*', 1000)));
            result.AddTest(CreateSimpleUntypedRoundTripTest("Untyped String: large (64k+1 characters)", "string1", new string('*', 65537)));

            result.AddTest(CreateSimpleUntypedRoundTripTest("Untyped String: non-ASCII characters - Latin", "string1", "ãéìôü ÇñÑ"));
            result.AddTest(CreateSimpleUntypedRoundTripTest("Untyped String: non-ASCII characters - Arabic", "string1", "الكتاب على الطاولة"));
            result.AddTest(CreateSimpleUntypedRoundTripTest("Untyped String: non-ASCII characters - Chinese", "string1", "这本书在桌子上"));
            result.AddTest(CreateSimpleUntypedRoundTripTest("Untyped String: non-ASCII characters - Chinese 2", "string1", "⒈①Ⅻㄨㄩ 啊阿鼾齄 丂丄狚狛 狜狝﨨﨩 ˊˋ˙–〇 㐀㐁䶴䶵"));
            result.AddTest(CreateSimpleUntypedRoundTripTest("Untyped String: non-ASCII characters - Japanese", "string1", "本は机の上に"));
            result.AddTest(CreateSimpleUntypedRoundTripTest("Untyped String: non-ASCII characters - Hebrew", "string1", "הספר הוא על השולחן"));
            result.AddTest(CreateSimpleUntypedRoundTripTest("Untyped String: non-ASCII characters - Russian", "string1", "Книга лежит на столе"));

            result.AddTest(CreateSimpleUntypedRoundTripTest("Untyped Date: now", "date1", Util.TrimSubMilliseconds(DateTime.Now)));
            result.AddTest(CreateSimpleUntypedRoundTripTest("Untyped Date: now (UTC)", "date1", Util.TrimSubMilliseconds(DateTime.UtcNow)));
            result.AddTest(CreateSimpleUntypedRoundTripTest("Untyped Date: null", "date1", null));
            result.AddTest(CreateSimpleUntypedRoundTripTest("Untyped Date: min date", "date1", DateTime.MinValue));
            result.AddTest(CreateSimpleUntypedRoundTripTest("Untyped Date: specific date, before unix 0", "date1", new DateTime(1901, 1, 1)));
            result.AddTest(CreateSimpleUntypedRoundTripTest("Untyped Date: specific date, after unix 0", "date1", new DateTime(2000, 12, 31)));

            result.AddTest(CreateSimpleUntypedRoundTripTest("Untyped Bool: true", "bool1", true));
            result.AddTest(CreateSimpleUntypedRoundTripTest("Untyped Bool: false", "bool1", false));
            result.AddTest(CreateSimpleUntypedRoundTripTest("Untyped Bool: null", "bool1", null));

            result.AddTest(CreateSimpleUntypedRoundTripTest("Untyped Int: zero", "int1", 0));
            result.AddTest(CreateSimpleUntypedRoundTripTest("Untyped Int: MaxValue", "int1", int.MaxValue));
            result.AddTest(CreateSimpleUntypedRoundTripTest("Untyped Int: MinValue", "int1", int.MinValue));

            result.AddTest(CreateSimpleUntypedRoundTripTest("Untyped Long: zero", "long1", 0L));
            result.AddTest(CreateSimpleUntypedRoundTripTest("Untyped Long: max allowed", "long1", maxAllowedValue));
            result.AddTest(CreateSimpleUntypedRoundTripTest("Untyped Long: min allowed", "long1", minAllowedValue));
            result.AddTest(CreateSimpleUntypedRoundTripTest("Untyped Long: more than max allowed (positive) for typed", "long1", maxAllowedValue + 1));
            result.AddTest(CreateSimpleUntypedRoundTripTest("Untyped Long: more than max allowed (negative) for typed", "long1", minAllowedValue - 1));

            result.AddTest(CreateSimpleUntypedRoundTripTest("Untyped complex (object): simple value",
                JObject.Parse(@"{""complexType2"":{""Name"":""John Doe"",""Age"":33,""Friends"":[""Jane Roe"", ""John Smith""]}}")));
            result.AddTest(CreateSimpleUntypedRoundTripTest(
                "Untyped complex (object): null",
                JObject.Parse(@"{""complexType2"":null}")));

            result.AddTest(CreateSimpleUntypedRoundTripTest(
                "Untyped complex (array): simple value",
                JObject.Parse(@"{""complexType1"":[{""Name"":""Scooby"",""Age"":10}, {""Name"":""Shaggy"",""Age"":19}]}")));
            result.AddTest(CreateSimpleUntypedRoundTripTest(
                "Untyped complex (array): empty array",
                JObject.Parse(@"{""complexType1"":[]}")));
            result.AddTest(CreateSimpleUntypedRoundTripTest(
                "Untyped complex (array): null",
                JObject.Parse(@"{""complexType1"":null}")));
            result.AddTest(CreateSimpleUntypedRoundTripTest(
                "Untyped complex (array): array with null elements",
                JObject.Parse(@"{""complexType1"":[{""Name"":""Scooby"",""Age"":10}, null, {""Name"":""Shaggy"",""Age"":19}]}")));

            result.AddTest(CreateSimpleUntypedRoundTripTest<ArgumentException>("(Neg) Insert item with non-default 'id' property",
                JObject.Parse("{\"id\":1,\"value\":2}"), false));
            result.AddTest(CreateSimpleUntypedRoundTripTest<ArgumentException>("(Neg) Insert item with non-default 'ID' property",
                JObject.Parse("{\"ID\":1,\"value\":2}"), false));
            result.AddTest(CreateSimpleUntypedRoundTripTest<ArgumentException>("(Neg) Insert item with non-default 'Id' property",
                JObject.Parse("{\"Id\":1,\"value\":2}"), false));

            uniqueId = (Environment.TickCount + 1).ToString(CultureInfo.InvariantCulture);

            var obj = new StringIdRoundTripTableItem(rndGen);
            var properties = new Dictionary<string, object>
            {
                { "name", obj.Name },
                { "date1", obj.Date },
                { "bool", obj.Bool },
                { "number", obj.Number },
                { "complex", obj.ComplexType },
            };
            foreach (var name in differentIds.Keys)
            {
                foreach (var property in properties.Keys)
                {
                    var testName = "String id (untyped) - " + name + " id on insert - " + property;
                    var item = JObjectFromValue(property, properties[property]);
                    var id = differentIds[name];
                    if (id != null)
                    {
                        item.Add("id", id + "-" + property + "-" + Guid.NewGuid().ToString());
                    }

                    result.AddTest(CreateSimpleUntypedRoundTripTest(testName, item, true));
                }
            }

            foreach (var id in invalidIds)
            {
                var testName = "(Neg) [string id] Insert item with invalid 'id' property: " + (id.Length > 30 ? (id.Substring(0, 30) + "...") : id);
                JObject jo = new JObject(new JProperty("id", id), new JProperty("name", "should not work"));
                result.AddTest(CreateSimpleUntypedRoundTripTest<InvalidOperationException>(testName, jo, true));
            }

            return result;
        }

        private static ZumoTest CreateSetupSchemaTest()
        {
            return new ZumoTest("Setup dynamic schema", async delegate(ZumoTest test)
            {
                var client = ZumoTestGlobals.Instance.Client;
                var table = client.GetTable<RoundTripTableItem>();
                Random rndGen = new Random(1);
                try
                {
                    RoundTripTableItem item = new RoundTripTableItem
                    {
                        Bool1 = true,
                        ComplexType1 = new ComplexType[] { new ComplexType(rndGen) },
                        ComplexType2 = new ComplexType2(rndGen),
                        Date1 = DateTime.Now,
                        Double1 = 123.456,
                        EnumType = EnumType.First,
                        Int1 = 1,
                        Long1 = 1,
                        String1 = "hello",
                    };

                    await table.InsertAsync(item);
                    test.AddLog("Inserted item to create schema on the int id table");

                    var table2 = client.GetTable<StringIdRoundTripTableItem>();
                    var item2 = new StringIdRoundTripTableItem { Bool = true, Name = "hello", Number = 1.23, ComplexType = "a b c".Split(), Date = DateTime.UtcNow };
                    await table2.InsertAsync(item2);
                    test.AddLog("Inserted item to create schema on the string id table");

                    return true;
                }
                catch (Exception ex)
                {
                    test.AddLog("Error setting up the dynamic schema: {0}", ex);
                    return false;
                }
            });
        }

        private static JObject JObjectFromValue(string propertyName, object propertyValue)
        {
            var item = new JObject();
            if (propertyValue == null)
            {
                item.Add(propertyName, JToken.Parse("null"));
            }
            else
            {
                Type propType = propertyValue.GetType();
                if (propType == typeof(string))
                {
                    item.Add(propertyName, (string)propertyValue);
                }
                else if (propType == typeof(DateTime))
                {
                    item.Add(
                        propertyName,
                        ((DateTime)propertyValue).ToUniversalTime().ToString(
                            "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture));
                }
                else if (propType == typeof(bool))
                {
                    item.Add(propertyName, (bool)propertyValue);
                }
                else if (propType == typeof(int))
                {
                    item.Add(propertyName, (int)propertyValue);
                }
                else if (propType == typeof(long))
                {
                    item.Add(propertyName, (long)propertyValue);
                }
                else if (propType == typeof(double))
                {
                    item.Add(propertyName, (double)propertyValue);
                }
                else if (propType == typeof(string[]))
                {
                    var ja = new JArray();
                    foreach (var value in (string[])propertyValue)
                    {
                        ja.Add(value);
                    }

                    item.Add(propertyName, ja);
                }
                else
                {
                    throw new ArgumentException("Don't know how to create test for type " + propType.FullName);
                }
            }

            return item;
        }

        private static ZumoTest CreateSimpleUntypedRoundTripTest(string testName, string propertyName, object propertyValue, bool useStringIdTable = false)
        {
            var item = JObjectFromValue(propertyName, propertyValue);
            return CreateSimpleUntypedRoundTripTest<ExceptionTypeWhichWillNeverBeThrown>(testName, item, useStringIdTable);
        }

        private static ZumoTest CreateSimpleUntypedRoundTripTest(string testName, JObject item, bool useStringIdTable = false)
        {
            return CreateSimpleUntypedRoundTripTest<ExceptionTypeWhichWillNeverBeThrown>(testName, item, useStringIdTable);
        }

        private static ZumoTest CreateSimpleUntypedRoundTripTest<TExpectedException>(string testName, JObject templateItem, bool useStringIdTable)
            where TExpectedException : Exception
        {
            return new ZumoTest(testName, async delegate(ZumoTest test)
            {
                var item = JObject.Parse(templateItem.ToString()); // prevent outer object from being captured and reused
                var client = ZumoTestGlobals.Instance.Client;
                var table = client.GetTable(useStringIdTable ? ZumoTestGlobals.StringIdRoundTripTableName : ZumoTestGlobals.RoundTripTableName);

                try
                {
                    string originalItem = item == null ? "null" : item.ToString();

                    var inserted = await table.InsertAsync(item);
                    object id;
                    if (!useStringIdTable)
                    {
                        int intId = inserted["id"].Value<int>();
                        test.AddLog("Inserted item, id = {0}", intId);
                        if (intId <= 0)
                        {
                            test.AddLog("Error, insert didn't succeed (id == 0)");
                            return false;
                        }

                        id = intId;
                    }
                    else
                    {
                        var originalId = item["id"];
                        bool hadId = originalId != null && originalId.Type != JTokenType.Null;
                        var originalStringId = (string)originalId;
                        string newId = (string)inserted["id"];
                        if (hadId && newId != originalStringId)
                        {
                            test.AddLog("Error, insert changed the item id!");
                            return false;
                        }

                        id = newId;
                    }

                    JToken roundTripped = await table.LookupAsync(id);
                    if (roundTripped.Type != JTokenType.Object)
                    {
                        test.AddLog("Result of Lookup is not an object: {0}", roundTripped);
                        return false;
                    }

                    test.AddLog("Retrieved the item from the service");

                    List<string> errors = new List<string>();
                    bool testResult;
                    if (!Util.CompareJson(item, roundTripped, errors))
                    {
                        foreach (var error in errors)
                        {
                            test.AddLog(error);
                        }

                        test.AddLog("Round-tripped item is different! Expected: {0}; actual: {1}", originalItem, roundTripped);
                        testResult = false;
                    }
                    else
                    {
                        if (typeof(TExpectedException) == typeof(ExceptionTypeWhichWillNeverBeThrown))
                        {
                            testResult = true;
                        }
                        else
                        {
                            test.AddLog("Error, test should have failed with {0}, but succeeded.", typeof(TExpectedException).FullName);
                            testResult = false;
                        }
                    }

                    test.AddLog("Cleaning up...");
                    await table.DeleteAsync(new JObject(new JProperty("id", id)));
                    test.AddLog("Item deleted");
                    return testResult;
                }
                catch (TExpectedException ex)
                {
                    test.AddLog("Caught expected exception - {0}: {1}", ex.GetType().FullName, ex.Message);
                    return true;
                }
            });
        }

        enum RoundTripTestType
        {
            String, 
            Double, 
            Bool, 
            Int, 
            Long, 
            Date, 
            Enum, 
            ComplexWithConverter,
            ComplexWithCustomSerialization,
            Id
        }

        private static ZumoTest CreateSimpleTypedRoundTripTest(string testName, RoundTripTestType type, object value)
        {
            return CreateSimpleTypedRoundTripTest<ExceptionTypeWhichWillNeverBeThrown>(testName, type, value);
        }

        private static ZumoTest CreateSimpleTypedRoundTripTest<TExpectedException>(
            string testName, RoundTripTestType type, object value)
            where TExpectedException : Exception
        {
            return new ZumoTest(testName, async delegate(ZumoTest test)
            {
                var client = ZumoTestGlobals.Instance.Client;
                var table = client.GetTable<RoundTripTableItem>();
                var item = new RoundTripTableItem();
                switch (type)
                {
                    case RoundTripTestType.Bool:
                        item.Bool1 = (bool?)value;
                        break;
                    case RoundTripTestType.ComplexWithConverter:
                        item.ComplexType1 = (ComplexType[])value;
                        break;
                    case RoundTripTestType.ComplexWithCustomSerialization:
                        item.ComplexType2 = (ComplexType2)value;
                        break;
                    case RoundTripTestType.Date:
                        item.Date1 = (DateTime?)value;
                        break;
                    case RoundTripTestType.Double:
                        item.Double1 = (double)value;
                        break;
                    case RoundTripTestType.Enum:
                        item.EnumType = (EnumType)value;
                        break;
                    case RoundTripTestType.Int:
                        item.Int1 = (int)value;
                        break;
                    case RoundTripTestType.Long:
                        item.Long1 = (long)value;
                        break;
                    case RoundTripTestType.String:
                        item.String1 = (string)value;
                        break;
                    case RoundTripTestType.Id:
                        item.Id = (int)value;
                        break;
                    default:
                        throw new ArgumentException("Invalid type");
                }

                RoundTripTableItem originalItem = item.Clone();
                try
                {
                    await table.InsertAsync(item);
                    test.AddLog("Inserted item, id = {0}", item.Id);
                    if (item.Id <= 0)
                    {
                        test.AddLog("Error, insert didn't succeed (id == 0)");
                        return false;
                    }

                    RoundTripTableItem roundTripped = await table.LookupAsync(item.Id);
                    test.AddLog("Retrieved the item from the service");
                    bool testResult;
                    if (!originalItem.Equals(roundTripped))
                    {
                        test.AddLog("Round-tripped item is different! Expected: {0}; actual: {1}", originalItem, roundTripped);
                        testResult = false;
                    }

                    if (type == RoundTripTestType.String && item.String1 != null && item.String1.Length < 50)
                    {
                        test.AddLog("Now querying the table for the item (validating characters on query)");
                        var queried = await table.Where(i => i.Id > (item.Id - 40) && i.String1 == item.String1).ToListAsync();
                        var lastItem = queried.Where(i => i.Id == item.Id).First();
                        if (originalItem.Equals(lastItem))
                        {
                            test.AddLog("Query for item succeeded");
                        }
                        else
                        {
                            test.AddLog("Round-tripped (queried) item is different! Expected: {0}; actual: {1}", originalItem, lastItem);
                            testResult = false;
                        }
                    }

                    if (typeof(TExpectedException) == typeof(ExceptionTypeWhichWillNeverBeThrown))
                    {
                        testResult = true;
                    }
                    else
                    {
                        test.AddLog("Error, test should have failed with {0}, but succeeded.", typeof(TExpectedException).FullName);
                        testResult = false;
                    }

                    test.AddLog("Cleaning up...");
                    await table.DeleteAsync(roundTripped);
                    test.AddLog("Item deleted");

                    return testResult;
                }
                catch (TExpectedException ex)
                {
                    test.AddLog("Caught expected exception - {0}: {1}", ex.GetType().FullName, ex.Message);
                    return true;
                }
            });
        }
    }
}
