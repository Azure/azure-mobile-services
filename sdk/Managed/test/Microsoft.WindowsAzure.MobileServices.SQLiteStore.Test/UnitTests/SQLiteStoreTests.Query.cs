// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Query;
using Microsoft.WindowsAzure.MobileServices.TestFramework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.SQLiteStore.Test.UnitTests
{
    [Tag("query")]
    public class SQLiteStoreQueryTests : TestBase
    {

        private static readonly Guid guid = Guid.Parse("d9c8bcf9-9c85-42e6-967b-c686d92f32cb");
        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static readonly JObject[] testData = new[]
        {
            new JObject(){{"id", "1"}, {"col1", "the"}, {"col2", 5}, {"col3", 234f}, {"col4", epoch.AddMilliseconds(32434)}, {"col5", false }, {"col6", Guid.Empty}},
            new JObject(){{"id", "2"}, {"col1", "quick"}, {"col2", 3}, {"col3", 9867.12}, {"col4", epoch.AddMilliseconds(99797)}, {"col5", true }, {"col6", Guid.Empty}},
            new JObject(){{"id", "3"}, {"col1", "brown"}, {"col2", 1}, {"col3", 11f}, {"col4", epoch.AddMilliseconds(23987239840)}, {"col5", false}, {"col6", Guid.Empty}},
            new JObject(){{"id", "4"}, {"col1", "fox"}, {"col2", 6}, {"col3", 23908.99}, {"col4", epoch.AddMilliseconds(8888888888888)}, {"col5", true}, {"col6", Guid.Empty}},
            new JObject(){{"id", "5"}, {"col1", "jumped"}, {"col2", 9}, {"col3", 678.932}, {"col4", epoch.AddMilliseconds(333333333332)}, {"col5", true}, {"col6", guid}},
            new JObject(){{"id", "6"}, {"col1", "EndsWithBackslash\\"}, {"col2", 8}, {"col3", 521f}, {"col4", epoch.AddMilliseconds(17071985)}, {"col5", true}, {"col6", guid}}
        };
        private const string TestTable = "test";
        private const string MathTestTable = "mathtest";
        private static bool queryTableInitialized;

        public static string TestDbName = SQLiteStoreTests.TestDbName;

        [AsyncTestMethod]
        public async Task Query_OnBool_Implicit()
        {
            await TestQuery("$filter=col5", 4);
            await TestQuery("$filter=not(col5)", 2);
        }

        [AsyncTestMethod]
        public async Task Query_OnBool_Explicit()
        {
            await TestQuery("$filter=col5 eq true", 4);
            await TestQuery("$filter=col5 eq false", 2);
        }

        [AsyncTestMethod]
        public async Task Query_NotOperator_WithBoolComparison()
        {
            await TestQuery("$filter=not(col5 eq true)", 2);
        }

        [AsyncTestMethod]
        public async Task Query_Date_BeforeEpoch()
        {
            await TestQuery("$filter=col4 gt datetime'1969-12-31T11:00:00.000Z'", 6);
        }

        [AsyncTestMethod]
        public async Task Query_Date_Functions()
        {
            await TestQuery("$filter=year(col4) ge 1980", 2);
            await TestQuery("$filter=col4 gt datetime'1970-09-12T12:00:00Z'", 3);
        }

        [AsyncTestMethod]
        public async Task Query_Date_ReturnedAsDate()
        {
            JArray results = await Query<JArray>("$filter=id eq '1'");
            Assert.AreEqual(results.Count, 1);

            JObject result = (JObject)results[0];
            DateTime col4 = result.Value<DateTime>("col4");

            Assert.AreEqual(col4.Kind, DateTimeKind.Utc);
            Assert.AreEqual(col4, testData[0]["col4"].Value<DateTime>());
        }

        [AsyncTestMethod]
        public async Task Query_WithTop()
        {
            await TestQuery("$top=5", 5);
        }

        [AsyncTestMethod]
        public async Task Query_OnString()
        {
            await TestQuery("$filter=col1 eq 'jumped'", 1);
        }

        [AsyncTestMethod]
        public async Task Query_OnGuid()
        {
            await TestQuery(string.Format("$filter=col6 eq guid'{0}'", guid), 2);
        }

        [AsyncTestMethod]
        public async Task Query_WithSelection()
        {
            JArray results = await Query<JArray>("$select=col1,col2");
            var expected = new JArray(testData.Select(x => new JObject() { { "col1", x["col1"] }, { "col2", x["col2"] } }));
            AssertJArraysAreEqual(results, expected);
        }

        [AsyncTestMethod]
        public async Task Query_WithOrdering_Ascending()
        {
            JArray results = await Query<JArray>("$orderby=col2");
            var expected = new JArray(testData.OrderBy(x => x["col2"].Value<int>()));
            AssertJArraysAreEqual(results, expected);
        }

        [AsyncTestMethod]
        public async Task Query_WithOrdering_Descending()
        {
            JArray results = await Query<JArray>("$orderby=col2 desc");
            var expected = new JArray(testData.OrderByDescending(x => x["col2"].Value<int>()));
            AssertJArraysAreEqual(results, expected);
        }

        [AsyncTestMethod]
        public async Task Query_Complex_Filter()
        {
            JArray results = await Query<JArray>("$filter=((col1 eq 'brown') or (col1 eq 'fox')) and (col2 le 5)");
            Assert.AreEqual(results.Count, 1);

            Assert.AreEqual(results[0].ToString(), testData[2].ToString());
        }

        [AsyncTestMethod, Tag("notXamarin")] // Android/iOS SQLite does not have INSTR function
        public async Task Query_OnString_IndexOf()
        {
            await TestQuery("$filter=indexof(col1, 'ump') eq 1", 1);
            await TestQuery("$filter=indexof(col1, 'ump') eq 2", 0);
        }

        [AsyncTestMethod]
        public async Task Query_OnString_SubstringOf()
        {
            await TestQuery("$filter=substringof('ump', col1)", 1);
            await TestQuery("$filter=substringof('umx', col1)", 0);
        }

        [AsyncTestMethod]
        public async Task Query_OnString_StartsWith()
        {
            await TestQuery("$filter=startswith(col1, 'jum')", 1);
            await TestQuery("$filter=startswith(col1, 'pum')", 0);
        }

        [AsyncTestMethod]
        public async Task Query_OnString_EndsWith()
        {
            await TestQuery("$filter=endswith(col1, 'umped')", 1);
            await TestQuery("$filter=endswith(col1, 'umxed')", 0);
        }

        [AsyncTestMethod]
        public async Task Query_OnString_ConcatAndCompare()
        {
            await TestQuery("$filter=concat(concat(col1, 'ies'), col2) eq 'brownies1'", 1);
            await TestQuery("$filter=concat(concat(col1, 'ies'), col2) eq 'brownies2'", 0);
        }

        [AsyncTestMethod]
        public async Task Query_OnString_ReplaceAndCompare()
        {
            await TestQuery("$filter=replace(col1, 'j', 'p') eq 'pumped'", 1);
            await TestQuery("$filter=replace(col1, 'j', 'x') eq 'pumped'", 0);
        }

        [AsyncTestMethod]
        public async Task Query_OnString_SubstringAndCompare()
        {
            await TestQuery("$filter=substring(col1, 1) eq 'ox'", 1);
            await TestQuery("$filter=substring(col1, 1) eq 'oy'", 0);
        }

        [AsyncTestMethod]
        public async Task Query_OnString_SubstringWithLengthAndCompare()
        {
            await TestQuery("$filter=substring(col1, 1, 3) eq 'uic'", 1);
            await TestQuery("$filter=substring(col1, 1, 3) eq 'uix'", 0);
        }

        [AsyncTestMethod]
        public async Task Query_Math_ModuloOperator()
        {
            JArray results = await Query<JArray>("$filter=(col3 mod 6) eq 0");
            var expected = new JArray(testData.Where(x => (int)(x["col3"].Value<float>() % 6) == 0));
            AssertJArraysAreEqual(results, expected);

            results = await Query<JArray>("$filter=(col2 mod 3) eq 0");
            expected = new JArray(testData.Where(x => x["col2"].Value<int>() % 3 == 0));
            AssertJArraysAreEqual(results, expected);
        }

        [AsyncTestMethod]
        public async Task Query_Math_Round()
        {
            JObject[] mathTestData = new[]{
                new JObject(){{"val", -0.0900}, {"expected", 0}},
                new JObject(){{"val", -1.0900}, {"expected", -1}},
                new JObject(){{"val", 1.0900}, {"expected", 1}},
                new JObject(){{"val", 0.0900}, {"expected", 0}},
                new JObject(){{"val", 1.5}, {"expected", 2}},
                new JObject(){{"val", -1.5}, {"expected", -2}},
            };
            await TestMathQuery(mathTestData, "$filter=round(val) eq expected");
        }

        [AsyncTestMethod]
        public async Task Query_Math_Floor()
        {
            JObject[] mathTestData = new[]{
                new JObject(){{"val", -0.0900}, {"expected", -1}},
                new JObject(){{"val", -1.0900}, {"expected", -2}},
                new JObject(){{"val", 1.0900}, {"expected", 1}},
                new JObject(){{"val", 0.0900}, {"expected", 0}},
            };
            await TestMathQuery(mathTestData, "$filter=floor(val) eq expected");
        }

        [AsyncTestMethod]
        public async Task Query_Math_Ceiling()
        {
            JObject[] mathTestData = new[]{
                new JObject(){{"val", -0.0900}, {"expected", 0}},
                new JObject(){{"val", -1.0900}, {"expected", -1}},
                new JObject(){{"val", 1.0900}, {"expected", 2}},
                new JObject(){{"val", 0.0900}, {"expected", 1}},
            };
            await TestMathQuery(mathTestData, "$filter=ceiling(val) eq expected");
        }

        [AsyncTestMethod]
        public async Task Query_OnString_Length()
        {
            await TestQuery("$filter=length(col1) eq 18", 1);
            await TestQuery("$filter=length(col1) eq 19", 0);
        }

        [AsyncTestMethod]
        public async Task Query_WithPaging()
        {
            for (int skip = 0; skip < 4; skip++)
            {
                for (int take = 0; take < 4; take++)
                {
                    var expected = new JArray(testData.Skip(skip).Take(take));
                    JArray results = await Query<JArray>("$skip=" + skip + "&$top=" + take);

                    AssertJArraysAreEqual(results, expected);
                }
            }
        }

        [AsyncTestMethod]
        public async Task Query_WithTotalCount()
        {
            JObject result = await Query<JObject>("$top=5&$inlinecount=allpages");
            Assert.AreEqual(result.Value<JArray>("results").Count, 5);
            Assert.AreEqual(result.Value<int>("count"), 6);
        }

        private static async Task<MobileServiceSQLiteStore> SetupMathTestTable(JObject[] mathTestData)
        {
            TestUtilities.DropTestTable(TestDbName, MathTestTable);

            // first create a table called todo
            var store = new MobileServiceSQLiteStore(TestDbName);
            store.DefineTable(MathTestTable, new JObject()
            {
                { "id", String.Empty },
                { "val", 0f },
                { "expected", 0f }
            });

            await store.InitializeAsync();

            foreach (JObject item in mathTestData)
            {
                item[MobileServiceSystemColumns.Id] = Guid.NewGuid().ToString();
            }

            await store.UpsertAsync(MathTestTable, mathTestData, ignoreMissingColumns: false);

            return store;
        }

        private static async Task<MobileServiceSQLiteStore> SetupTestTable()
        {
            if (!queryTableInitialized)
            {
                TestUtilities.DropTestTable(TestDbName, TestTable);
            }

            // first create a table called todo
            var store = new MobileServiceSQLiteStore(TestDbName);
            store.DefineTable(TestTable, new JObject()
            {
                { "col1", String.Empty },
                { "col2", 0L },
                { "col3", 0f },
                { "col4", DateTime.UtcNow },
                { "col5", false },
                { "col6", Guid.Empty }
            });

            await store.InitializeAsync();

            if (!queryTableInitialized)
            {
                await store.UpsertAsync(TestTable, testData, ignoreMissingColumns: false);
            }

            queryTableInitialized = true;
            return store;
        }

        private static void AssertJArraysAreEqual(JArray results, JArray expected)
        {
            string actualResult = results.ToString(Formatting.None);
            string expectedResult = expected.ToString(Formatting.None);
            Assert.AreEqual(actualResult, expectedResult);
        }

        private static async Task TestMathQuery(JObject[] mathTestData, string query)
        {
            using (MobileServiceSQLiteStore store = await SetupMathTestTable(mathTestData))
            {
                var results = await Query<JArray>(store, MathTestTable, query);
                Assert.AreEqual(results.Count, mathTestData.Length);
            }
        }

        private static async Task TestQuery(string query, int expectedResults)
        {
            JArray results = await Query<JArray>(query);
            Assert.AreEqual(results.Count, expectedResults);
        }

        private static async Task<T> Query<T>(string query) where T : JToken
        {
            using (MobileServiceSQLiteStore store = await SetupTestTable())
            {
                return (T)await store.ReadAsync(MobileServiceTableQueryDescription.Parse(TestTable, query));
            }
        }

        private static async Task<T> Query<T>(MobileServiceSQLiteStore store, string tableName, string query) where T : JToken
        {
            return (T)await store.ReadAsync(MobileServiceTableQueryDescription.Parse(tableName, query));
        }
    }
}
