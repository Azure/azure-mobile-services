// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.MobileServices.TestFramework;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    [Tag("serialize75")]
    [Tag("unit")]
    public class MobileServiceSerializerTestsWP75 : TestBase
    {
        MobileServiceSerializer defaultSerializer;

        MobileServiceSerializer DefaultSerializer
        {
            get
            {
                if (this.defaultSerializer == null)
                {
                    this.defaultSerializer = new MobileServiceSerializer();
                }

                return this.defaultSerializer;
            }
        }

        [TestMethod]
        public void DateTimeDeserializationNegative()
        {
            List<Tuple<string, string>> testCases = new List<Tuple<string, string>>() {
                new Tuple<string, string>("{\"DateTime\":\"I'm not a Date\"}", "String was not recognized as a valid DateTime."),
                new Tuple<string, string>("{\"DateTime\":true}", "Unexpected token parsing date. Expected String, got Boolean. Path 'DateTime', line 1, position 16."),
                new Tuple<string, string>("{\"DateTime\":5}", "Unexpected token parsing date. Expected String, got Integer. Path 'DateTime', line 1, position 13."),
                new Tuple<string, string>("{\"DateTime\":\"\t\"}", "String was not recognized as a valid DateTime."),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(DateTimeType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expectedError = testCase.Item2;

                DateTimeType actual = new DateTimeType();
                Exception actualError = null;
                try
                {
                    DefaultSerializer.Deserialize(input, actual);
                }
                catch (Exception e)
                {
                    actualError = e;
                }

                Assert.AreEqual(actualError.Message, expectedError);
            }
        }

        [TestMethod]
        public void DateTimeOffsetDeserializationNegative()
        {
            List<Tuple<string, string>> testCases = new List<Tuple<string, string>>() {
                new Tuple<string, string>("{\"DateTimeOffset\":\"I'm not a Date\"}", "String was not recognized as a valid DateTime."),
                new Tuple<string, string>("{\"DateTimeOffset\":true}", "Unexpected token parsing date. Expected String, got Boolean. Path 'DateTimeOffset', line 1, position 22."),
                new Tuple<string, string>("{\"DateTimeOffset\":5}", "Unexpected token parsing date. Expected String, got Integer. Path 'DateTimeOffset', line 1, position 19."),
                new Tuple<string, string>("{\"DateTimeOffset\":\"\t\"}", "String was not recognized as a valid DateTime."),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(DateTimeOffsetType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expectedError = testCase.Item2;

                DateTimeOffsetType actual = new DateTimeOffsetType();
                Exception actualError = null;
                try
                {
                    DefaultSerializer.Deserialize(input, actual);
                }
                catch (Exception e)
                {
                    actualError = e;
                }

                Assert.AreEqual(actualError.Message, expectedError);
            }
        }

        [TestMethod]
        public void FloatDeserializationNegative()
        {
            List<string> testCases = new List<string>() {
                "{\"Float\":1.7976931348623157E+309}",      // Larger double.MaxValue
                "{\"Float\":-1.7976931348623157E+309}",     // Larger double.MinValue
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(FloatType));

            foreach (var testCase in testCases)
            {
                var input = testCase;

                FloatType actual = new FloatType();
                Exception actualError = null;
                try
                {
                    DefaultSerializer.Deserialize(input, actual);
                }
                catch (Exception e)
                {
                    actualError = e;
                }

                Assert.AreEqual(actualError.Message, "Value was either too large or too small for a Double.");
            }
        }

        [TestMethod]
        public void NullableDeserializationNegative()
        {
            List<string> testCases = new List<string>() {
                "{\"Nullable\":1.7976931348623157E+309}",
                "{\"Nullable\":-1.7976931348623157E+309}",
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(NullableType));

            foreach (var testCase in testCases)
            {
                var input = testCase;

                NullableType actual = new NullableType();
                Exception actualError = null;
                try
                {
                    DefaultSerializer.Deserialize(input, actual);
                }
                catch (Exception e)
                {
                    actualError = e;
                }

                Assert.AreEqual(actualError.Message, "Value was either too large or too small for a Double.");
            }
        }

        [TestMethod]
        public void LongDeserializationNegative()
        {
            List<Tuple<string, string>> testCases = new List<Tuple<string, string>>() {
                new Tuple<string, string>("{\"Long\":9.2233720368547758E+18}","Error converting value 9.22337203685478E+18 to type 'System.Int64'. Path 'Long', line 1, position 30."),  // Fails because this will be read as a double, which then doesn't convert into a long      
                new Tuple<string, string>("{\"Long\":9223372036854775808}","JSON integer 9223372036854775808 is too large or small for an Int64. Path 'Long', line 1, position 27."), // long.MaxValue + 1
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(LongType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expectedError = testCase.Item2;

                LongType actual = new LongType();
                Exception actualError = null;
                try
                {
                    DefaultSerializer.Deserialize(input, actual);
                }
                catch (Exception e)
                {
                    actualError = e;
                }

                Assert.AreEqual(actualError.Message, expectedError);
            }
        }

        [TestMethod]
        public void ULongDeserializationNegative()
        {
            List<Tuple<string, string>> testCases = new List<Tuple<string, string>>() {
                new Tuple<string, string>("{\"ULong\":18446744073709551616}","JSON integer 18446744073709551616 is too large or small for an Int64. Path 'ULong', line 1, position 29."), // ulong.MaxValue + 1
                new Tuple<string, string>("{\"ULong\":-1}","Error converting value -1 to type 'System.UInt64'. Path 'ULong', line 1, position 11."),
            };

            // Need to ensure that the type is registered as a table to force the id property check
            DefaultSerializer.SerializerSettings.ContractResolver.ResolveTableName(typeof(ULongType));

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                var expected = testCase.Item2;

                ULongType actual = new ULongType();
                Exception actualError = null;
                try
                {
                    DefaultSerializer.Deserialize(input, actual);
                }
                catch (Exception e)
                {
                    actualError = e;
                }

                Assert.AreEqual(actualError.Message, expected);
            }
        }
    }
}
