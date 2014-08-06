// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.TestFramework;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    /// <summary>
    /// Helper assert functions
    /// </summary>
    public static class AssertEx
    {
        public static TException Throws<TException>(Action action) where TException : Exception
        {
            return Throws<TException>(() => Task.Run(action)).Result;
        }

        public static async Task<TException> Throws<TException>(Func<Task> action) where TException : Exception
        {
            TException thrown = null;
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                thrown = ex as TException;
            }

            Assert.IsNotNull(thrown, "Expected exception not thrown");

            return thrown;
        }

        public static void QueryStringContains(string query, string queryKey, string matchQueryValue)
        {
            QueryStringContains(query, queryKey, value => value == matchQueryValue);
        }

        public static void QueryStringContains(string query, string queryKey, Func<string, bool> matchQueryValue)
        {
            var parts = QueryStringHelper.EnumerateQueryParts(query);

            var match = parts.FirstOrDefault(p => p.Key == queryKey);

            if (match.Key != queryKey)
            {
                Assert.Fail("Key not found in query string");
                return;
            }

            if (!matchQueryValue(match.Value))
            {
                Assert.Fail("Query string key/value pair did not match expectations");
            }
        }

        public static void QueryStringContains(string query, IEnumerable<KeyValuePair<string, string>> partsToMatch, bool matchExactly = true)
        {
            var parts = QueryStringHelper.EnumerateQueryParts(query).ToArray();

            int matched = 0;

            foreach (var part in partsToMatch)
            {
                var matchingPart = parts.FirstOrDefault(p => p.Key == part.Key);

                if (matchingPart.Key != part.Key)
                {
                    Assert.Fail("Key not found in query string");
                    return;
                }

                if (matchingPart.Value != part.Value)
                {
                    Assert.Fail("Query string key/value pair did not match expectations");
                    return;
                }

                matched++;
            }

            if (matchExactly)
            {
                Assert.AreEqual(matched, parts.Length, "Query string contained unexpected additional parameters");
            }
        }
    }

    public static class QueryStringHelper
    {
        public static IEnumerable<KeyValuePair<string, string>> EnumerateQueryParts(string query)
        {
            query = (query ?? "").TrimStart('?');

            char[] @ampersand = { '&' }, @equals = { '=' };

            return from pair in query.Split(@ampersand)
                   let kvp = pair.Split(@equals, 2)
                   select new KeyValuePair<string, string>(
                       key: Uri.UnescapeDataString(kvp[0]),
                       value: Uri.UnescapeDataString(kvp.Length > 1 ? kvp[1] : "")
                       );
        }
    }
}
