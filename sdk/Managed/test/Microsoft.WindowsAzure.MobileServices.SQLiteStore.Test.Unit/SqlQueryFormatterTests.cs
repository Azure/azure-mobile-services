// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MobileServices.Query;
using Microsoft.WindowsAzure.MobileServices.Test;

namespace Microsoft.WindowsAzure.MobileServices.SQLiteStore.Test.Unit
{
    [TestClass]
    public class SqlQueryFormatterTests
    {
        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        [TestMethod]
        public void FormatSelect_Math_Floor()
        {
            string odata = "$filter=floor(weight) gt 5&$orderby=price asc&$select=name";

            string expectedSql = "SELECT [name] FROM [test] WHERE ((CASE WHEN ([weight] >= @p1) THEN CAST([weight] AS INTEGER) WHEN (CAST([weight] AS INTEGER) = [weight]) THEN [weight] ELSE CAST(([weight] - @p2) AS INTEGER) END) > @p3) ORDER BY [price]";

            TestSqlFormatting(f => f.FormatSelect, odata, expectedSql, 0L, 1L, 5L);
        }

        [TestMethod]
        public void FormatSelect_InvalidQuery()
        {
            string odata = "$filter=(2 ! ??)";
            var ex = AssertEx.Throws<MobileServiceODataException>(() => MobileServiceTableQueryDescription.Parse("test", odata));
            Assert.AreEqual("The specified odata query has syntax errors.", ex.Message);
            Assert.AreEqual(3, ex.ErrorPosition);
        }

        [TestMethod]
        public void FormatSelect_Math_Ceiling()
        {
            string odata = "$filter=ceiling(weight) gt 5&$orderby=price asc&$select=name";

            string expectedSql = "SELECT [name] FROM [test] WHERE ((CASE WHEN ([weight] >= @p1) THEN CAST([weight] AS INTEGER) WHEN (CAST([weight] AS INTEGER) = [weight]) THEN [weight] ELSE CAST(([weight] - @p2) AS INTEGER) END) + (CASE WHEN [weight] = (CASE WHEN ([weight] >= @p3) THEN CAST([weight] AS INTEGER) WHEN (CAST([weight] AS INTEGER) = [weight]) THEN [weight] ELSE CAST(([weight] - @p4) AS INTEGER) END) THEN 0 ELSE 1 END) > @p5) ORDER BY [price]";

            TestSqlFormatting(f => f.FormatSelect, odata, expectedSql, 0L, 1L, 0L, 1L, 5L);
        }

        [TestMethod]
        public void FormatSelect_Math_Round()
        {
            string odata = "$filter=round(weight) gt 5.3&$orderby=price asc&$select=name";

            string expectedSql = "SELECT [name] FROM [test] WHERE (ROUND([weight], 0) > @p1) ORDER BY [price]";

            TestSqlFormatting(f => f.FormatSelect, odata, expectedSql, (double)5.3);
        }

        [TestMethod]
        public void FormatSelect_Date_Comparison()
        {
            string odata = "$filter=close_dt gt datetime'2012-05-29T09:13:28'";

            string expectedSql = "SELECT * FROM [test] WHERE ([close_dt] > @p1)";

            TestSqlFormatting(f => f.FormatSelect, odata, expectedSql, (DateTime.Parse("2012-05-29T09:13:28") - epoch).TotalSeconds);
        }

        [TestMethod]
        public void FormatSelect_DateTimeOffset_Comparison()
        {
            string odata = "$filter=close_dt gt datetimeoffset'2012-05-29T09:13:28'";

            string expectedSql = "SELECT * FROM [test] WHERE ([close_dt] > @p1)";

            TestSqlFormatting(f => f.FormatSelect, odata, expectedSql, (DateTime.Parse("2012-05-29T09:13:28") - epoch).TotalSeconds);
        }

        [TestMethod]
        public void FormatSelect_Date_Day()
        {
            TestSelectDateTimeFunction("day", "%d");
        }

        [TestMethod]
        public void FormatSelect_Date_Month()
        {
            TestSelectDateTimeFunction("month", "%m");
        }

        [TestMethod]
        public void FormatSelect_Date_Year()
        {
            TestSelectDateTimeFunction("year", "%Y");
        }

        [TestMethod]
        public void FormatSelect_Date_Hour()
        {
            TestSelectDateTimeFunction("hour", "%H");
        }

        [TestMethod]
        public void FormatSelect_Date_Minute()
        {
            TestSelectDateTimeFunction("minute", "%M");
        }

        [TestMethod]
        public void FormatSelect_Date_Second()
        {
            TestSelectDateTimeFunction("second", "%S");
        }

        private static void TestSelectDateTimeFunction(string fn, string format)
        {
            string odata = "$filter=" + fn + "(close_dt) eq 5";

            string expectedSql = "SELECT * FROM [test] WHERE (CAST(strftime('" + format + "', datetime([close_dt], 'unixepoch')) AS INTEGER) = @p1)";

            TestSqlFormatting(f => f.FormatSelect, odata, expectedSql, 5L);
        }

        [TestMethod]
        public void FormatSelect_String_ToLower()
        {
            TestSelectStringFunction("tolower(name)", "LOWER([name])");
        }

        [TestMethod]
        public void FormatSelect_String_ToUpper()
        {
            TestSelectStringFunction("toupper(name)", "UPPER([name])");
        }

        [TestMethod]
        public void FormatSelect_String_Length()
        {
            TestSelectStringFunction("length(name)", "LENGTH([name])");
        }

        [TestMethod]
        public void FormatSelect_String_Trim()
        {
            TestSelectStringFunction("trim(name)", "TRIM([name])");
        }

        [TestMethod]
        public void FormatSelect_String_SubstringOf()
        {
            TestSelectStringFunction("substringof('khan', name)", "LIKE('%' || @p1 || '%', [name])", "khan");
        }

        [TestMethod]
        public void FormatSelect_String_StartsWith()
        {
            TestSelectStringFunction("startswith(name, 'khan')", "LIKE(@p1 || '%', [name])", "khan");
        }

        [TestMethod]
        public void FormatSelect_String_EndsWith()
        {
            TestSelectStringFunction("endswith(name, 'khan')", "LIKE('%' || @p1, [name])", "khan");
        }

        [TestMethod]
        public void FormatSelect_String_Concat()
        {
            TestSelectStringFunction("concat(firstName, lastName)", "[firstName] || [lastName]");
        }

        [TestMethod]
        public void FormatSelect_String_IndexOf()
        {
            TestSelectStringFunction("indexof(message, 'hello')", "INSTR([message], @p1) - 1", "hello");
        }

        [TestMethod]
        public void FormatSelect_String_Replace()
        {
            TestSelectStringFunction("replace(message, '$author$', name)", "REPLACE([message], @p1, [name])", "$author$");
        }

        [TestMethod]
        public void FormatSelect_String_Substring()
        {
            TestSelectStringFunction("substring(title, 10)", "SUBSTR([title], @p1 + 1)", 10L);
        }

        private static void TestSelectStringFunction(string fn, string sqlFn, object extraArg = null)
        {
            var args = new List<object>();
            string arg = "@p1";
            if (extraArg != null)
            {
                arg = "@p2";
                args.Add(extraArg);
            }
            args.Add("abc");

            string odata = "$filter=" + fn + " eq 'abc'";
            string expectedSql = "SELECT * FROM [test] WHERE (" + sqlFn + " = " + arg + ")";

            TestSqlFormatting(f => f.FormatSelect, odata, expectedSql, args.ToArray());
        }

        [TestMethod]
        public void FormatSelect_GeneratesSQL_WithoutTotalCount()
        {
            string odata = "$filter=(name eq 'john' and age gt 7)&$orderby=String desc,id&$skip=5&$top=3&$select=name,age";

            string expectedSql = "SELECT [name], [age] FROM [test] WHERE (([name] = @p1) AND ([age] > @p2)) ORDER BY [String] DESC, [id] LIMIT 3 OFFSET 5";

            TestSqlFormatting(f => f.FormatSelect, odata, expectedSql, "john", 7L);
        }

        [TestMethod]
        public void FormatSelect_GeneratesSQL_WithoutColumns()
        {
            string odata = "$filter=name eq 'john' or age le 7&$orderby=String desc,id&$skip=5&$top=3";

            string expectedSql = "SELECT * FROM [test] WHERE (([name] = @p1) OR ([age] <= @p2)) ORDER BY [String] DESC, [id] LIMIT 3 OFFSET 5";

            TestSqlFormatting(f => f.FormatSelect, odata, expectedSql, "john", 7L);
        }

        [TestMethod]
        public void FormatSelect_GeneratesSQL_WithTotalCount()
        {
            string odata = "$filter=(name eq 'john' and age gt 7)&$orderby=String desc,id&$skip=5&$top=3&$select=name,age&$inlinecount=allpages";

            string expectedSql = "SELECT [name], [age] FROM [test] WHERE (([name] = @p1) AND ([age] > @p2)) ORDER BY [String] DESC, [id] LIMIT 3 OFFSET 5";
            string expectedCountSql = "SELECT COUNT(1) AS [count] FROM [test] WHERE (([name] = @p1) AND ([age] > @p2))";

            TestSqlFormatting(f => f.FormatSelect, odata, expectedSql, "john", 7L);
            TestSqlFormatting(f => f.FormatSelectCount, odata, expectedCountSql, "john", 7L);
        }

        [TestMethod]
        public void FormatDeletes_GeneratesSQL()
        {
            string odata = "$filter=(name eq 'john' and age gt 7)&$orderby=String desc,id&$skip=5&$top=3&$select=name,age&$inlinecount=allpages";

            string expectedSql = "DELETE FROM [test] WHERE [id] IN (SELECT [id] FROM [test] WHERE (([name] = @p1) AND ([age] > @p2)) ORDER BY [String] DESC, [id] LIMIT 3 OFFSET 5)";

            TestSqlFormatting(f => f.FormatDelete, odata, expectedSql, "john", 7L);
        }

        private static void TestSqlFormatting(Func<SqlQueryFormatter, Func<string>> action, string odata, string expectedSql, params object[] parameters)
        {
            var query = MobileServiceTableQueryDescription.Parse("test", odata);
            var formatter = new SqlQueryFormatter(query);
            string sql = action(formatter)();

            Assert.AreEqual(expectedSql, sql);
            Assert.AreEqual(formatter.Parameters.Count, parameters.Length);

            for (int i = 0; i < parameters.Length; i++)
            {
                string name = "@p" + (i + 1);
                Assert.IsTrue(formatter.Parameters.ContainsKey(name));
                Assert.AreEqual(formatter.Parameters[name], parameters[i]);
            }
        }
    }
}
