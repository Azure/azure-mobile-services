//// ----------------------------------------------------------------------------
//// Copyright (c) Microsoft Corporation. All rights reserved.
//// ----------------------------------------------------------------------------

//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.WindowsAzure.MobileServices.TestFramework;

//namespace Microsoft.WindowsAzure.MobileServices.Test
//{
//    [DataTable("types")]
//    public class DataTypes
//    {
//        // Use a nullable int ID
//        public int? Id { get; set; }

//        public bool Bool { get; set; }
//        public byte Byte { get; set; }
//        public sbyte SByte { get; set; }
//        public ushort UShort { get; set; }
//        public short Short { get; set; }
//        public uint UInt { get; set; }
//        public int Int { get; set; }
//        public ulong ULong { get; set; }
//        public long Long { get; set; }
//        public double Double { get; set; }
//        public float Float { get; set; }
//        public Decimal Decimal { get; set; }
//        public string String { get; set; }
//        public char Char { get; set; }
//        public DateTime Date { get; set; }
//        public Uri Uri { get; set; }

//        public DataTypes()
//        {
//            Id = null;
//            Bool = true;
//            Byte = 0;
//            SByte = 0;
//            UShort = 0;
//            Short = 0;
//            UInt = 0;
//            Int = 0;
//            ULong = 0;
//            Long = 0;
//            Double = 0.0;
//            Float = 0.0f;
//            Decimal = 0.0M;
//            String = "test";
//            Char = '0';
//            Date = DateTime.Now;
//            Uri = new Uri("http://www.microsoft.com");
//        }
//    }

//    [Tag("e2e")]
//    public class DataTypesTest : FunctionalTestBase
//    {
//        [AsyncTestMethod]
//        public async Task BooleanTest()
//        {
//            using (DataTypesContext context = await DataTypesContext.Create(this))
//            {
//                await context.CheckValues((o, v) => o.Bool = v,
//                    true, false);
//            }
//        }

//        [AsyncTestMethod]
//        public async Task ByteTest()
//        {
//            using (DataTypesContext context = await DataTypesContext.Create(this))
//            {
//                await context.CheckValues<byte>((o, v) => o.Byte = v,
//                    0, byte.MinValue, byte.MaxValue, 1, 2, 3);
//            }
//        }

//        [AsyncTestMethod]
//        public async Task SByteTest()
//        {
//            using (DataTypesContext context = await DataTypesContext.Create(this))
//            {
//                await context.CheckValues<sbyte>((o, v) => o.SByte = v,
//                    0, sbyte.MinValue, sbyte.MaxValue, 1, -1, 2, -3);
//            }
//        }

//        [AsyncTestMethod]
//        public async Task UShortTest()
//        {
//            using (DataTypesContext context = await DataTypesContext.Create(this))
//            {
//                await context.CheckValues<ushort>((o, v) => o.UShort = v,
//                    0, ushort.MinValue, ushort.MaxValue, 1, 2, 3, 10);
//            }
//        }

//        [AsyncTestMethod]
//        public async Task ShortTest()
//        {
//            using (DataTypesContext context = await DataTypesContext.Create(this))
//            {
//                await context.CheckValues<short>((o, v) => o.Short = v,
//                    0, short.MinValue, short.MaxValue, 1, -1, 2, 10, -10);
//            }
//        }

//        [AsyncTestMethod]
//        public async Task UintTest()
//        {
//            using (DataTypesContext context = await DataTypesContext.Create(this))
//            {
//                await context.CheckValues<uint>((o, v) => o.UInt = v,
//                    0, uint.MinValue, uint.MaxValue, 1, 2, 3, 10, 120000);
//            }
//        }

//        [AsyncTestMethod]
//        public async Task IntTest()
//        {
//            using (DataTypesContext context = await DataTypesContext.Create(this))
//            {
//                await context.CheckValues<int>((o, v) => o.Int = v,
//                    0, int.MinValue, int.MaxValue, 1, -1, 2, 120000, -120000);
//            }
//        }

//        [AsyncTestMethod]
//        public async Task ULongTest()
//        {
//            ulong max = Convert.ToUInt64(Math.Pow(2, 53));
//            using (DataTypesContext context = await DataTypesContext.Create(this))
//            {
//                await context.CheckValues<ulong>((o, v) => o.ULong = v,
//                    0, max, ulong.MinValue, 1, 2, 3, 10, 1 >> 40);
//            }
//        }

//        [AsyncTestMethod]
//        public async Task LongTest()
//        {
//            long max = Convert.ToInt64(Math.Pow(2, 53));
//            using (DataTypesContext context = await DataTypesContext.Create(this))
//            {
//                await context.CheckValues<long>((o, v) => o.Long = v,
//                    0, max, -max, 1, -1, 2, 1 >> 40, -(1 >> 40));
//            }
//        }

//        [AsyncTestMethod]
//        public async Task DoubleTest()
//        {
//            using (DataTypesContext context = await DataTypesContext.Create(this))
//            {
//                await context.CheckValues<double>((o, v) => o.Double = v,
//                     0.0, double.MaxValue, double.MinValue, double.Epsilon, 1.0,
//                    1.5, 2.0, -2.0, 3.14e12, -5.2222e-10);
//                await context.ExpectInsertFailure<double>((o, v) => o.Double = v,
//                    2e19, double.PositiveInfinity, double.NegativeInfinity, double.NaN);
//            }
//        }

//        [AsyncTestMethod]
//        public async Task FloatTest()
//        {
//            using (DataTypesContext context = await DataTypesContext.Create(this))
//            {
//                await context.CheckValues<float>((o, v) => o.Float = v,
//                    0.0f, float.MaxValue, float.MinValue, float.Epsilon, 1.0f,
//                    1.5f, 2.0f, -2.0f, 3.14e12f, -5.2222e-10f);
//                await context.ExpectInsertFailure<float>((o, v) => o.Float = v,
//                    float.PositiveInfinity, float.NegativeInfinity, float.NaN);
//            }
//        }

//        [AsyncTestMethod]
//        public async Task DecimalTest()
//        {
//            decimal max = Convert.ToDecimal(Math.Pow(2, 53));
//            using (DataTypesContext context = await DataTypesContext.Create(this))
//            {
//                await context.CheckValues<decimal>((o, v) => o.Decimal = v,
//                   max, max, 0.0M, 1.0M, 1.5M, 2.0M, -2.0M, 3.14e12M, -5.2222e-10M);
//            }
//        }

//        [AsyncTestMethod]
//        public async Task StringTest()
//        {
//            using (DataTypesContext context = await DataTypesContext.Create(this))
//            {
//                await context.CheckValues<string>((o, v) => o.String = v,
//                    null, "", "a", "abc", "a bad ba sdf asdf", "'", "\"",
//                    "'asdfas'", "\"asdfasd\"", "a's'\"asdf'\"as", "a ' \" a",
//                    new string('*', 1025), new string('*', 2049), "ÃÇßÑᾆΏ");

//                // Split every character between 128 and 0xD7FF into 64
//                // strings and try them out.  Disabled only so the test
//                // doesn't take forever to run.
//                //await context.CheckValues<string>((o, v) => o.String = v,
//                //    Enumerable.Zip(
//                //        Enumerable.Range(128, 0xD7FF),
//                //        Enumerable.Range(128, 0xD7FF),
//                //        (index, ch) => Tuple.Create(index, (char)ch))
//                //    .GroupBy(pair => pair.Item1 % 64)
//                //    .Select(group => new string(group.Select(pair => pair.Item2).ToArray()))
//                //    .ToArray());

//                await context.ExpectInsertFailure<string>((o, v) => o.String = v,
//                    "\uDC801\uDC01");
//            }
//        }

//        [AsyncTestMethod]
//        public async Task CharTest()
//        {
//            using (DataTypesContext context = await DataTypesContext.Create(this))
//            {
//                await context.CheckValues<char>((o, v) => o.Char = v,
//                    (char)0, (char)1, (char)2, ' ', '\t', '\n', '\b', '?',
//                    'a', '1', '\u1000', 'Ã', 'Ç', 'ß', 'Ñ', 'ᾆ', 'Ώ');
//                await context.ExpectInsertFailure<char>((o, v) => o.Char = v,
//                    '\uDC01');
//            }
//        }

//        [AsyncTestMethod]
//        public async Task DateTimeTest()
//        {
//            using (DataTypesContext context = await DataTypesContext.Create(this))
//            {
//                await context.CheckValues<DateTime>((o, v) => o.Date = v,
//                    DateTime.Now,
//                    new DateTime(1999, 12, 31, 23, 59, 59),
//                    new DateTime(2005, 3, 14, 12, 34, 16, DateTimeKind.Local),
//                    new DateTime(2005, 4, 14, 12, 34, 16, DateTimeKind.Unspecified),
//                    new DateTime(2005, 5, 14, 12, 34, 16, DateTimeKind.Utc),
//                    new DateTime(2012, 2, 29, 12, 0, 0, DateTimeKind.Utc)); // Leap Day

//                // The following values trigger an assert in the node SQL driver
//                // so they're commented out to unblock the test run
//                //await context.ExpectInsertFailure<DateTime>((o, v) => o.Date = v,
//                //    DateTime.MinValue,
//                //    DateTime.MaxValue,
//                //    DateTime.MaxValue.AddSeconds(-1),
//                //    DateTime.MinValue.AddSeconds(1),
//                //    new DateTime(1900, 1, 1));
//            }
//        }

//        [AsyncTestMethod]
//        public async Task UriTest()
//        {
//            using (DataTypesContext context = await DataTypesContext.Create(this))
//            {
//                await context.CheckValues<Uri>((o, v) => o.Uri = v,
//                    null,
//                    new Uri("http://www.microsoft.com"),
//                    new Uri("ftp://127.0.0.1"));
//            }
//        }
//    }

//    public class DataTypesContext : IDisposable
//    {
//        private static bool _initialized = false;

//        public TestBase Test { get; private set; }
//        public IMobileServiceTable<DataTypes> Table { get; private set; }
//        public List<Exception> Failures { get; private set; }

//        private DataTypesContext()
//        {
//            Failures = new List<Exception>();
//        }

//        public static async Task<DataTypesContext> Create(FunctionalTestBase test, bool logging = false)
//        {
//            MobileServiceClient client = logging ?
//                test.GetClient() :
//                test.GetClientNoLogging();

//            DataTypesContext context = new DataTypesContext();
//            context.Test = test;
//            context.Table = client.GetTable<DataTypes>();

//            if (!_initialized)
//            {
//                // Insert non-null values for everything so we get correct
//                // column types
//                DataTypes basic = new DataTypes();
//                basic.String = "test";
//                basic.Uri = new Uri("http://www.microsoft.com");
//                basic.Date = DateTime.Now;
//                basic.Char = 'a';
//                await context.Table.InsertAsync(basic);

//                _initialized = true;
//            }

//            return context;
//        }

//        public void Dispose()
//        {
//            if (Failures.Count > 0)
//            {
//                string message = string.Format(
//                    CultureInfo.InvariantCulture,
//                    "{0} unexpected failure(s){2}{1}{2}{2}",
//                    Failures.Count,
//                    String.Concat(Failures.Select(ex =>
//                        Environment.NewLine + Environment.NewLine +
//                        Environment.NewLine + ex.Message)),
//                    Environment.NewLine);
//                throw new AggregateException(message, Failures);
//            }
//        }

//        public async Task CheckValues<T>(Action<DataTypes, T> setter, params T[] values)
//        {
//            foreach (T value in values)
//            {
//                DataTypes instance = new DataTypes();
//                setter(instance, value);
//                try
//                {
//                    // DateTime date = instance.Date;
//                    await Table.InsertAsync(instance);

//                    try
//                    {
//                        //DataTypes newInstance = new DataTypes();
//                        //setter(newInstance, value);
//                        //newInstance.Id = instance.Id;
//                        //newInstance.Date = date;
//                        await CompareWithServer(instance);
//                    }
//                    catch (Exception ex)
//                    {
//                        Failures.Add(new Exception(
//                            string.Format(
//                                "Failed to roundtrip '{0}' value '{1}'.  ({2})",
//                                typeof(T).Name, value,
//                                ex)));
//                    }
//                }
//                catch (Exception ex)
//                {
//                    Failures.Add(new Exception(
//                        string.Format(
//                            "Failed to insert '{0}' value '{1}'.  ({2})",
//                            typeof(T).Name, value,
//                            ex)));
//                }
//            }
//        }

//        public async Task ExpectInsertFailure<T>(Action<DataTypes, T> setter, params T[] values)
//        {
//            foreach (T value in values)
//            {
//                DataTypes instance = new DataTypes();
//                setter(instance, value);
//                try
//                {
//                    await Table.InsertAsync(instance);
//                }
//                catch (Exception)
//                {
//                    continue;
//                }

//                Failures.Add(new Exception(
//                    string.Format(
//                        CultureInfo.InvariantCulture,
//                        "No expected failure for insertion of '{0}' value '{1}'",
//                        typeof(T).Name, value)));
//            }
//        }

//        public async Task ExpectRoundtripFailure<T>(Action<DataTypes, T> setter, params T[] values)
//        {
//            foreach (T value in values)
//            {
//                DataTypes instance = new DataTypes();
//                setter(instance, value);
//                try
//                {
//                    await Table.InsertAsync(instance);
//                }
//                catch (Exception)
//                {
//                    continue;
//                }

//                try
//                {
//                    if (instance.Id != null)
//                    {
//                        await CompareWithServer(instance);
//                    }
//                }
//                catch (Exception)
//                {
//                    continue;
//                }

//                Failures.Add(new Exception(
//                    string.Format(
//                        CultureInfo.InvariantCulture,
//                        "No expected failure for round trip of '{0}' value '{1}'",
//                        typeof(T).Name, value)));
//            }
//        }

//        public async Task CompareWithServer(DataTypes original)
//        {
//            Assert.IsNotNull(original);
//            Assert.IsNotNull(original.Id.Value);
//            DataTypes roundtrip = await Table.LookupAsync(original.Id.Value);

//            Assert.AreEqual(original.Id, roundtrip.Id);
//            Assert.AreEqual(original.Bool, roundtrip.Bool);
//            Assert.AreEqual(original.Byte, roundtrip.Byte);
//            Assert.AreEqual(original.SByte, roundtrip.SByte);
//            Assert.AreEqual(original.UShort, roundtrip.UShort);
//            Assert.AreEqual(original.Short, roundtrip.Short);
//            Assert.AreEqual(original.UInt, roundtrip.UInt);
//            Assert.AreEqual(original.Int, roundtrip.Int);
//            Assert.AreEqual(original.ULong, roundtrip.ULong);
//            Assert.AreEqual(original.Long, roundtrip.Long);
//            Assert.AreEqual(original.Double, roundtrip.Double);
//            Assert.AreEqual(original.Float, roundtrip.Float);
//            Assert.AreEqual(original.Decimal, roundtrip.Decimal);
//            Assert.AreEqual(original.String, roundtrip.String);
//            Assert.AreEqual(original.Char, roundtrip.Char);
//            Assert.AreEqual(original.Date, roundtrip.Date);
//            Assert.AreEqual(original.Uri, roundtrip.Uri);
//        }
//    }
//}
