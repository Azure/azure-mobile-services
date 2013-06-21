// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.MobileServices.TestFramework;

namespace Microsoft.WindowsAzure.MobileServices.Test.UnitTests.Table.Serialization
{
    public class MobileServiceIsoDateTimeConverterTests : TestBase
    {
        MobileServiceIsoDateTimeConverter isoDateTimeConverter;

        MobileServiceIsoDateTimeConverter IsoDateTimeConverter
        {
            get
            {
                if (this.isoDateTimeConverter == null)
                {
                    this.isoDateTimeConverter = new MobileServiceIsoDateTimeConverter();
                }

                return this.isoDateTimeConverter;
            }
        }

        [TestMethod]
        public void CanConvertReturnsTrueForDateTimeDateTimeOffset()
        {
            bool canConvert = IsoDateTimeConverter.CanConvert(typeof(DateTime));
            Assert.IsTrue(canConvert);

            canConvert = IsoDateTimeConverter.CanConvert(typeof(DateTimeOffset));
            Assert.IsTrue(canConvert);
        }

        [TestMethod]
        public void CanConvertReturnsFalseForNotDateTimeDateTimeOffset()
        {
            //false
            bool canConvert = IsoDateTimeConverter.CanConvert(typeof(byte));
            Assert.IsFalse(canConvert);

            canConvert = IsoDateTimeConverter.CanConvert(typeof(ulong));
            Assert.IsFalse(canConvert);

            canConvert = IsoDateTimeConverter.CanConvert(typeof(int));
            Assert.IsFalse(canConvert);

            canConvert = IsoDateTimeConverter.CanConvert(typeof(short));
            Assert.IsFalse(canConvert);

            canConvert = IsoDateTimeConverter.CanConvert(typeof(byte[]));
            Assert.IsFalse(canConvert);

            canConvert = IsoDateTimeConverter.CanConvert(typeof(object));
            Assert.IsFalse(canConvert);

            canConvert = IsoDateTimeConverter.CanConvert(typeof(string));
            Assert.IsFalse(canConvert);

            canConvert = IsoDateTimeConverter.CanConvert(typeof(bool));
            Assert.IsFalse(canConvert);

            canConvert = IsoDateTimeConverter.CanConvert(typeof(decimal));
            Assert.IsFalse(canConvert);

            canConvert = IsoDateTimeConverter.CanConvert(typeof(double));
            Assert.IsFalse(canConvert);

            canConvert = IsoDateTimeConverter.CanConvert(typeof(long));
            Assert.IsFalse(canConvert);
        }

        [TestMethod]
        public void CanReadShouldReturnTrue()
        {
            bool canRead = IsoDateTimeConverter.CanRead;

            Assert.IsTrue(canRead);
        }

        [TestMethod]
        public void CanWriteShouldReturnTrue()
        {
            bool canWrite = IsoDateTimeConverter.CanWrite;

            Assert.IsTrue(canWrite);
        }
    }
}
