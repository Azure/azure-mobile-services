// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.Azure.Zumo.Win8.Test;
using Microsoft.WindowsAzure.MobileServices;

namespace Microsoft.Azure.Zumo.Win8.CSharp.Test
{
    /// <summary>
    /// Verify TypeExtensions functionality.
    /// </summary>
    public class TypeExtensionsTests : TestBase
    {
        /// <summary>
        /// An example class to check for properties and fields.
        /// </summary>
        public class TestClassA
        {
            [DataMember]
            public string FieldA;
            private int PrivateFieldA = 0;
            
            public double PropertyA { get; set; }
            private int PrivatePropertyA { get; set; }
            protected bool ProtectedProperty { get; set; }
            internal DateTime InternalProperty { get; set; }

            public int ReadOnlyProperty { get; private set; }

            // Use PrivateFieldA so we don't get compiler warnings
            public void SetA(int value)
            {
                PrivateFieldA = value;
            }
            public int GetA()
            {
                return PrivateFieldA;
            }
        }
            
        /// <summary>
        /// An example derived class to check for properties and fields.
        /// </summary>
        public class TestClassB : TestClassA
        {
            private int PrivateFieldB = 0;
            public int FieldB;
            public string PropertyB { get; set; }
            private int PrivatePropertyB { get; set; }

            // Indexers should be ignored
            public string this[string key]
            {
                get { return key; }
                set { throw new InvalidOperationException(); }
            }

            // Use PrivateFieldB so we don't get compiler warnings
            public void SetB(int value)
            {
                PrivateFieldB = value;
            }
            public int GetB()
            {
                return PrivateFieldB;
            }
        }

        /// <summary>
        /// Verify TestExtensions.GetBaseTypesAndSelf.
        /// </summary>
        [TestMethod]
        public void GetBaseTypesAndSelf()
        {
            Assert.AreEquivalent(
                typeof(object).GetBaseTypesAndSelf().ToList(),
                new List<TypeInfo> { typeof(object).GetTypeInfo() });

            Assert.AreEquivalent(
                typeof(TestClassA).GetBaseTypesAndSelf().ToList(),
                new List<TypeInfo> {
                    typeof(object).GetTypeInfo(),
                    typeof(TestClassA).GetTypeInfo() });

            Assert.AreEquivalent(
                typeof(TestClassB).GetBaseTypesAndSelf().ToList(),
                new List<TypeInfo> {
                    typeof(object).GetTypeInfo(),
                    typeof(TestClassA).GetTypeInfo(),
                    typeof(TestClassB).GetTypeInfo() });
        }

        /// <summary>
        /// Verify GetProperties values for a given type.
        /// </summary>
        /// <typeparam name="T">The type to check.</typeparam>
        /// <param name="inherited">
        /// Whether to include inherited propeties.
        /// </param>
        /// <param name="publicOnly">
        /// Whether to include anything other than public properties.
        /// </param>
        /// <param name="expectedProperties">
        /// List of expected properties.
        /// </param>
        private static void VerifyProperties<T>(bool inherited, bool publicOnly, params string[] expectedProperties)
        {
            List<string> actual =
                typeof(T)
                .GetProperties(inherited, publicOnly)
                .Select(p => p.Name)
                .OrderBy(n => n)
                .ToList();
            List<string> expected = expectedProperties.OrderBy(n => n).ToList();
            Assert.AreEquivalent(
                expected,
                actual,
                string.Format(
                    "Expected->Actual: {0}",
                    string.Join(", ",
                        Enumerable.Zip(expected, actual, (e, a) => e + "->" + a))));
        }

        /// <summary>
        /// Verify TypeExtensions.GetProperties.
        /// </summary>
        [TestMethod]
        public void GetProperties()
        {
            VerifyProperties<TestClassA>(false, true, "PropertyA");
            VerifyProperties<TestClassA>(true, true, "PropertyA");
            VerifyProperties<TestClassA>(true, false,
                "PropertyA", "PrivatePropertyA", "ReadOnlyProperty",
                "ProtectedProperty", "InternalProperty");
            VerifyProperties<TestClassA>(false, false,
                "PropertyA", "PrivatePropertyA", "ReadOnlyProperty",
                "ProtectedProperty", "InternalProperty");

            VerifyProperties<TestClassB>(false, true, "PropertyB");
            VerifyProperties<TestClassB>(true, true,
                "PropertyA", "PropertyB");
            VerifyProperties<TestClassB>(false, false,
                "PropertyB", "PrivatePropertyB");
            VerifyProperties<TestClassB>(true, false,
                "PropertyA", "PrivatePropertyA", "ReadOnlyProperty",
                "ProtectedProperty", "InternalProperty",
                "PropertyB", "PrivatePropertyB");
        }

        /// <summary>
        /// Verify GetFields values for a given type.
        /// </summary>
        /// <typeparam name="T">The type to check.</typeparam>
        /// <param name="inherited">
        /// Whether to include inherited fields.
        /// </param>
        /// <param name="publicOnly">
        /// Whether to include anything other than public fields.
        /// </param>
        /// <param name="expectedProperties">
        /// List of expected fields.
        /// </param>
        private static void VerifyFields<T>(bool inherited, bool publicOnly, params string[] expectedFields)
        {
            List<string> actual =
                typeof(T)
                .GetFields(inherited, publicOnly)
                .Select(p => p.Name)
                .OrderBy(n => n)
                .ToList();
            List<string> expected = expectedFields.OrderBy(n => n).ToList();
            Assert.AreEquivalent(
                expected,
                actual,
                string.Format(
                    "Expected->Actual: {0}",
                    string.Join(", ",
                        Enumerable.Zip(expected, actual, (e, a) => e + "->" + a))));
        }

        /// <summary>
        /// Verify TypeExtensions.GetFields.
        /// </summary>
        [TestMethod]
        public void GetFields()
        {
            VerifyFields<TestClassA>(false, true, "FieldA");
            VerifyFields<TestClassA>(true, true, "FieldA");
            VerifyFields<TestClassA>(true, false,
                "FieldA", "PrivateFieldA",
                "<PrivatePropertyA>k__BackingField",
                "<InternalProperty>k__BackingField",
                "<PropertyA>k__BackingField",
                "<ProtectedProperty>k__BackingField",
                "<ReadOnlyProperty>k__BackingField");
            VerifyFields<TestClassA>(false, false,
                "FieldA", "PrivateFieldA",
                "<PrivatePropertyA>k__BackingField",
                "<InternalProperty>k__BackingField",
                "<PropertyA>k__BackingField",
                "<ProtectedProperty>k__BackingField",
                "<ReadOnlyProperty>k__BackingField");

            VerifyFields<TestClassB>(false, true, "FieldB");
            VerifyFields<TestClassB>(true, true,
                "FieldA", "FieldB");
            VerifyFields<TestClassB>(false, false,
                "FieldB", "PrivateFieldB",
                "<PropertyB>k__BackingField",
                "<PrivatePropertyB>k__BackingField");
            VerifyFields<TestClassB>(true, false,
                "FieldB", "PrivateFieldB",
                "<PropertyB>k__BackingField",
                "<PrivatePropertyB>k__BackingField",
                "FieldA", "PrivateFieldA",
                "<PrivatePropertyA>k__BackingField",
                "<InternalProperty>k__BackingField",
                "<PropertyA>k__BackingField",
                "<ProtectedProperty>k__BackingField",
                "<ReadOnlyProperty>k__BackingField");
        }

        /// <summary>
        /// Verify TypeExtensions.Has.
        /// </summary>
        [TestMethod]
        public void Has()
        {
            Assert.IsTrue(typeof(TestClassA).GetFields(true, true).First(f => f.Name == "FieldA").Has<DataMemberAttribute>(false));
            Assert.IsTrue(typeof(TestClassA).GetFields(true, true).First(f => f.Name == "FieldA").Has<DataMemberAttribute>(true));
            Assert.IsTrue(typeof(TestClassB).GetFields(true, true).First(f => f.Name == "FieldA").Has<DataMemberAttribute>(true));
            Assert.IsTrue(typeof(TestClassB).GetFields(true, true).First(f => f.Name == "FieldA").Has<DataMemberAttribute>(false));
            Assert.IsFalse(typeof(TestClassB).GetFields(true, true).First(f => f.Name == "FieldB").Has<DataMemberAttribute>(false));
            Assert.IsFalse(typeof(TestClassB).GetFields(true, true).First(f => f.Name == "FieldB").Has<DataMemberAttribute>(true));
            Assert.IsFalse(typeof(TestClassB).GetFields(true, true).First(f => f.Name == "FieldB").Has<DataMemberAttribute>(true));
        }
        
        /// <summary>
        /// Non-generic class that derives from an IEnumerable.
        /// </summary>
        public class TestClassC : List<string>
        {
        }

        /// <summary>
        /// Non-generic class that implements an IEnumerable.
        /// </summary>
        public class TestClassD : IList<string>
        {
            public int IndexOf(string item)
            {
                throw new NotImplementedException();
            }

            public void Insert(int index, string item)
            {
                throw new NotImplementedException();
            }

            public void RemoveAt(int index)
            {
                throw new NotImplementedException();
            }

            [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations", Justification = "Test")]
            public string this[int index]
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            public void Add(string item)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(string item)
            {
                throw new NotImplementedException();
            }

            public void CopyTo(string[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations", Justification = "Test")]
            public int Count
            {
                get { throw new NotImplementedException(); }
            }

            [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations", Justification = "Test")]
            public bool IsReadOnly
            {
                get { throw new NotImplementedException(); }
            }

            public bool Remove(string item)
            {
                throw new NotImplementedException();
            }

            public IEnumerator<string> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Verify TestExtensions.GetUnderlyingType.
        /// </summary>
        [TestMethod]
        public void GetUnderlyingType()
        {
            Assert.AreEqual(typeof(string), typeof(string).GetUnderlyingType());
            Assert.AreEqual(typeof(int), typeof(int).GetUnderlyingType());

            Assert.AreEqual(typeof(string), typeof(string[]).GetUnderlyingType());
            Assert.AreEqual(typeof(string), typeof(IEnumerable<string>).GetUnderlyingType());
            Assert.AreEqual(typeof(IEnumerable<string>), typeof(IEnumerable<IEnumerable<string>>).GetUnderlyingType());
            Assert.AreEqual(typeof(string), typeof(TestClassC).GetUnderlyingType());
            Assert.AreEqual(typeof(string), typeof(TestClassD).GetUnderlyingType());
        }

        /// <summary>
        /// Change the type of a value.
        /// </summary>
        /// <typeparam name="T">The type of the value to change to.</typeparam>
        /// <param name="value">The value to convert.</param>
        /// <param name="expected">The converted value.</param>
        public void VerifyChangeType<T>(object value, T expected)
        {
            Assert.AreEqual(
                expected,
                TypeExtensions.ChangeType(value, typeof(T)));
        }

        /// <summary>
        /// Verify TypeExtensions.ChangeType.
        /// </summary>
        [TestMethod]
        public void ChangeType()
        {
            VerifyChangeType<bool>(1, true);
            VerifyChangeType<bool>(0, false);
            VerifyChangeType<bool>("true", true);
            VerifyChangeType<bool?>(null, null);
            VerifyChangeType<bool?>(1, true);
            
            VerifyChangeType<int>(true, 1);
            VerifyChangeType<int>(false, 0);
            VerifyChangeType<int>("10", 10);
            VerifyChangeType<int>(1.0, 1);
            VerifyChangeType<int?>(1, 1);
            VerifyChangeType<int?>(null, null);
            VerifyChangeType<int?>(1.0, 1);
            VerifyChangeType<int?>("12", 12);

            VerifyChangeType<uint>(1.0, (uint)1);
            VerifyChangeType<uint?>(1.0, (uint?)1);
            VerifyChangeType<sbyte>(1.0, (sbyte)1);
            VerifyChangeType<sbyte>(-2.0, (sbyte)-2);
            VerifyChangeType<byte>(1.0, (byte)1);
            VerifyChangeType<byte?>(1.0, (byte?)1);
            VerifyChangeType<short>(1.0, (short)1);
            VerifyChangeType<short?>(1.0, (short?)1);
            VerifyChangeType<ushort>(1.0, (ushort)1);
            VerifyChangeType<ushort?>(1.0, (ushort?)1);
            VerifyChangeType<long>(1.0, 1L);
            VerifyChangeType<long?>(1.0, (long?)1);
            VerifyChangeType<ulong>(1.0, (ulong)1);
            VerifyChangeType<ulong?>(1.0, (ulong?)1);
            VerifyChangeType<double>(1, 1.0);
            VerifyChangeType<double?>(1, (double?)1.0);
            VerifyChangeType<float>(1, 1.0f);
            VerifyChangeType<float?>(1, (float?)1.0);
            VerifyChangeType<Decimal>(1, 1.0M);
            VerifyChangeType<Decimal?>(1, (Decimal?)1.0M);
            VerifyChangeType<char>("a", 'a');
            VerifyChangeType<char?>("a", (char?)'a');
            VerifyChangeType<string>(123, "123");
            VerifyChangeType<string>(null, null);
            VerifyChangeType<DateTime>("1/1/2000", new DateTime(2000, 1, 1));
            VerifyChangeType<DateTime?>("1/1/2000", new DateTime(2000, 1, 1));
        }

        /// <summary>
        /// Verify TypeExtensions.IsAssignableTo.
        /// </summary>
        [TestMethod]
        public void IsAssignableTo()
        {
            Assert.IsFalse(typeof(TestClassA).IsAssignableTo<TestClassB>());
            Assert.IsTrue(typeof(TestClassB).IsAssignableTo<TestClassA>());
            Assert.IsTrue(typeof(TestClassA).IsAssignableTo<TestClassA>());
        }

        /// <summary>
        /// Verify TestExtensions.ToODataConstant.
        /// </summary>
        [TestMethod]
        public void ToODataConstant()
        {
            Assert.AreEqual("null", TypeExtensions.ToODataConstant(null));
            Assert.AreEqual("true", TypeExtensions.ToODataConstant(true));
            Assert.AreEqual("01", TypeExtensions.ToODataConstant((byte)1));
            Assert.AreEqual("07", TypeExtensions.ToODataConstant((byte)7));
            Assert.AreEqual("10", TypeExtensions.ToODataConstant((byte)16));
            Assert.AreEqual("1L", TypeExtensions.ToODataConstant(1L));
            Assert.AreEqual("100L", TypeExtensions.ToODataConstant(100L));
            Assert.AreEqual("10f", TypeExtensions.ToODataConstant(10f));
            Assert.AreEqual("1M", TypeExtensions.ToODataConstant(1M));
            Assert.AreEqual("'test'", TypeExtensions.ToODataConstant("test"));
            Assert.AreEqual("'It''s Bob''s Test'", TypeExtensions.ToODataConstant("It's Bob's Test"));
            Assert.AreEqual("'a'", TypeExtensions.ToODataConstant('a'));
            Assert.AreEqual("''''", TypeExtensions.ToODataConstant('\''));
            Assert.AreEqual(
                "datetime'2000-01-02T03:04:05.000Z'",
                TypeExtensions.ToODataConstant(new DateTime(2000, 1, 2, 3, 4, 5, DateTimeKind.Utc).ToLocalTime()));
            Assert.AreEqual(
                "datetime'2000-01-02T03:04:05.000Z'",
                TypeExtensions.ToODataConstant(
                    new DateTimeOffset(
                        new DateTime(2000, 1, 2, 3, 4, 5, DateTimeKind.Utc).ToLocalTime())));
            Assert.AreEqual(
                "guid'00000064-000c-003a-630d-082dc83c1d72'",
                TypeExtensions.ToODataConstant(new Guid(100, 12, 58, 99, 13, 8, 45, 200, 60, 29, 114)));
            Assert.AreEqual("1", TypeExtensions.ToODataConstant(1));
            Assert.AreEqual("1.5", TypeExtensions.ToODataConstant(1.5));
        }

        /// <summary>
        /// Verify the formatting of the DateRoundtripFormat method.
        /// </summary>
        [TestMethod]
        public void DateRoundtripFormat()
        {
            DateTime utc = new DateTime(2000, 1, 2, 3, 4, 5, DateTimeKind.Utc);
            DateTime local = utc.ToLocalTime();
            Assert.AreEqual("2000-01-02T03:04:05.000Z", utc.ToRoundtripDateString());
            Assert.AreEqual("2000-01-02T03:04:05.000Z", local.ToRoundtripDateString());
        }
    }
}
