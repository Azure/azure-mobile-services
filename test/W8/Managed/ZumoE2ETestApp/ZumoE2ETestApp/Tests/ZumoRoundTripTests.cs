using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
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
            result.AddTest(CreateSimpleTypedRoundTripTest("String: non-ASCII characters - Japanese", RoundTripTestType.String, "本は机の上に"));
            result.AddTest(CreateSimpleTypedRoundTripTest("String: non-ASCII characters - Hebrew", RoundTripTestType.String, "הספר הוא על השולחן"));

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

            result.AddTest(CreateSimpleTypedRoundTripTest<ArgumentOutOfRangeException>("(Neg) Long: more than max allowed", RoundTripTestType.Long, maxAllowedValue + 1));
            result.AddTest(CreateSimpleTypedRoundTripTest<ArgumentOutOfRangeException>("(Neg) Long: less than min allowed", RoundTripTestType.Long, minAllowedValue - 1));

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

            // Untyped table
            result.AddTest(CreateSimpleUntypedRoundTripTest("Untyped String: Empty", "string1", ""));
            result.AddTest(CreateSimpleUntypedRoundTripTest("Untyped String: null", "string1", null));
            result.AddTest(CreateSimpleUntypedRoundTripTest("Untyped String: random value",
                "string1", Util.CreateSimpleRandomString(rndGen, 10)));
            result.AddTest(CreateSimpleUntypedRoundTripTest("Untyped String: large (1000 characters)", "string1", new string('*', 1000)));
            result.AddTest(CreateSimpleUntypedRoundTripTest("Untyped String: large (64k+1 characters)", "string1", new string('*', 65537)));

            result.AddTest(CreateSimpleUntypedRoundTripTest("Untyped String: non-ASCII characters - Latin", "string1", "ãéìôü ÇñÑ"));
            result.AddTest(CreateSimpleUntypedRoundTripTest("Untyped String: non-ASCII characters - Arabic", "string1", "الكتاب على الطاولة"));
            result.AddTest(CreateSimpleUntypedRoundTripTest("Untyped String: non-ASCII characters - Chinese", "string1", "这本书在桌子上"));
            result.AddTest(CreateSimpleUntypedRoundTripTest("Untyped String: non-ASCII characters - Japanese", "string1", "本は机の上に"));
            result.AddTest(CreateSimpleUntypedRoundTripTest("Untyped String: non-ASCII characters - Hebrew", "string1", "הספר הוא על השולחן"));

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
                JsonObject.Parse(@"{""complexType2"":{""Name"":""John Doe"",""Age"":33,""Friends"":[""Jane Roe"", ""John Smith""]}}")));
            result.AddTest(CreateSimpleUntypedRoundTripTest(
                "Untyped complex (object): null",
                JsonObject.Parse(@"{""complexType2"":null}")));

            result.AddTest(CreateSimpleUntypedRoundTripTest(
                "Untyped complex (array): simple value",
                JsonObject.Parse(@"{""complexType1"":[{""Name"":""Scooby"",""Age"":10}, {""Name"":""Shaggy"",""Age"":19}]}")));
            result.AddTest(CreateSimpleUntypedRoundTripTest(
                "Untyped complex (array): empty array",
                JsonObject.Parse(@"{""complexType1"":[]}")));
            result.AddTest(CreateSimpleUntypedRoundTripTest(
                "Untyped complex (array): null",
                JsonObject.Parse(@"{""complexType1"":null}")));
            result.AddTest(CreateSimpleUntypedRoundTripTest(
                "Untyped complex (array): array with null elements",
                JsonObject.Parse(@"{""complexType1"":[{""Name"":""Scooby"",""Age"":10}, null, {""Name"":""Shaggy"",""Age"":19}]}")));

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
                    test.AddLog("Inserted item to create schema");
                    return true;
                }
                catch (Exception ex)
                {
                    test.AddLog("Error setting up the dynamic schema: {0}", ex);
                    return false;
                }
            });
        }

        private static ZumoTest CreateSimpleUntypedRoundTripTest(string testName, string propertyName, object propertyValue)
        {
            JsonObject item = new JsonObject();
            if (propertyValue == null)
            {
                item.Add(propertyName, JsonValue.Parse("null"));
            }
            else
            {
                Type propType = propertyValue.GetType();
                if (propType == typeof(string))
                {
                    item.Add(propertyName, JsonValue.CreateStringValue((string)propertyValue));
                }
                else if (propType == typeof(DateTime))
                {
                    item.Add(
                        propertyName, 
                        JsonValue.CreateStringValue(
                            ((DateTime)propertyValue).ToUniversalTime().ToString(
                                "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture)));
                }
                else if (propType == typeof(bool))
                {
                    item.Add(propertyName, JsonValue.CreateBooleanValue((bool)propertyValue));
                }
                else if (propType == typeof(int))
                {
                    item.Add(propertyName, JsonValue.CreateNumberValue((int)propertyValue));
                }
                else if (propType == typeof(long))
                {
                    item.Add(propertyName, JsonValue.CreateNumberValue((long)propertyValue));
                }
                else if (propType == typeof(double))
                {
                    item.Add(propertyName, JsonValue.CreateNumberValue((double)propertyValue));
                }
                else
                {
                    throw new ArgumentException("Don't know how to create test for type " + propType.FullName);
                }
            }

            return CreateSimpleUntypedRoundTripTest(testName, item);
        }

        private static ZumoTest CreateSimpleUntypedRoundTripTest(string testName, JsonObject item)
        {
            return new ZumoTest(testName, async delegate(ZumoTest test)
            {
                var client = ZumoTestGlobals.Instance.Client;
                var table = client.GetTable(ZumoTestGlobals.RoundTripTableName);
                string originalItem = item == null ? "null" : item.Stringify();

                await table.InsertAsync(item);
                int id = (int)item["id"].GetNumber();
                test.AddLog("Inserted item, id = {0}", id);
                if (id <= 0)
                {
                    test.AddLog("Error, insert didn't succeed (id == 0)");
                    return false;
                }

                IJsonValue roundTripped = await table.LookupAsync(id);
                if (roundTripped.ValueType != JsonValueType.Object)
                {
                    test.AddLog("Result of Lookup is not an object: {0}", roundTripped);
                    return false;
                }

                test.AddLog("Retrieved the item from the service");

                List<string> errors = new List<string>();
                if (!Util.CompareJson(item, roundTripped, errors))
                {
                    foreach (var error in errors)
                    {
                        test.AddLog(error);
                    }

                    test.AddLog("Round-tripped item is different! Expected: {0}; actual: {1}", originalItem, roundTripped);
                    return false;
                }
                else
                {
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
                    if (!originalItem.Equals(roundTripped))
                    {
                        test.AddLog("Round-tripped item is different! Expected: {0}; actual: {1}", originalItem, roundTripped);
                        return false;
                    }
                    else
                    {
                        if (typeof(TExpectedException) == typeof(ExceptionTypeWhichWillNeverBeThrown))
                        {
                            return true;
                        }
                        else
                        {
                            test.AddLog("Error, test should have failed with {0}, but succeeded.", typeof(TExpectedException).FullName);
                            return false;
                        }
                    }
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
