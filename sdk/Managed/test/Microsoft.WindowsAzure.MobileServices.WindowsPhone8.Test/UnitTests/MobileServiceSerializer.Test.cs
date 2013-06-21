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
    [Tag("unit")]
    public class MobileServiceSerializerTestsWP80 : TestBase
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
