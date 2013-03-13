// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.MobileServices.TestFramework;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    [Tag("serialize")]
    [Tag("unit")]
    public class MobileServiceContractResolverTests
    {
        [TestMethod]
        public void ResolveTableName()
        {
            MobileServiceContractResolver contractResolver = new MobileServiceContractResolver();

            List<Tuple<Type, string>> testCases = new List<Tuple<Type, string>>() {
                new Tuple<Type, string>(typeof(PocoType), "PocoType"),
                new Tuple<Type, string>(typeof(DataContractType), "DataContractNameFromAttributeType"),
                new Tuple<Type, string>(typeof(DataTableType), "NamedDataTableType"),
                new Tuple<Type, string>(typeof(JsonContainerType), "NamedJsonContainerType"),
                new Tuple<Type, string>(typeof(UnnamedJsonContainerType), "UnnamedJsonContainerType"),
            };

            foreach (var testCase in testCases)
            {
                var input = testCase.Item1;
                string expected = testCase.Item2;

                var actual = contractResolver.ResolveTableName(input);

                Assert.AreEqual(actual, expected);
            }
        }
    }
}
