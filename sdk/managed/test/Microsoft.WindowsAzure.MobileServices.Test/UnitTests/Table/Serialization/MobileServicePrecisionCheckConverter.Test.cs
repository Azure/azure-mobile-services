// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.MobileServices.TestFramework;
using Newtonsoft.Json;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    public class MobileServicePrecisionCheckConverterTests : TestBase
    {
        MobileServicePrecisionCheckConverter precisionConverter;

        MobileServicePrecisionCheckConverter PrecisionConverter
        {
            get
            {
                if (this.precisionConverter == null)
                {
                    this.precisionConverter = new MobileServicePrecisionCheckConverter();
                }

                return this.precisionConverter;
            }
        }

        [TestMethod]
        public void CanConvertReturnsTrueForDecimalDoubleLong()
        {
            bool canConvert = PrecisionConverter.CanConvert(typeof(decimal));
            Assert.IsTrue(canConvert);

            canConvert = PrecisionConverter.CanConvert(typeof(ulong));
            Assert.IsTrue(canConvert);

            canConvert = PrecisionConverter.CanConvert(typeof(long));
            Assert.IsTrue(canConvert);
        }

        [TestMethod]
        public void CanConvertReturnsFalseForNotDecimalDoubleLong()
        {
            //false
            bool canConvert = PrecisionConverter.CanConvert(typeof(byte));
            Assert.IsFalse(canConvert);

            canConvert = PrecisionConverter.CanConvert(typeof(int));
            Assert.IsFalse(canConvert);

            canConvert = PrecisionConverter.CanConvert(typeof(short));
            Assert.IsFalse(canConvert);

            canConvert = PrecisionConverter.CanConvert(typeof(byte[]));
            Assert.IsFalse(canConvert);

            canConvert = PrecisionConverter.CanConvert(typeof(object));
            Assert.IsFalse(canConvert);

            canConvert = PrecisionConverter.CanConvert(typeof(string));
            Assert.IsFalse(canConvert);

            canConvert = PrecisionConverter.CanConvert(typeof(bool));
            Assert.IsFalse(canConvert);

            canConvert = PrecisionConverter.CanConvert(typeof(DateTime));
            Assert.IsFalse(canConvert);

            canConvert = PrecisionConverter.CanConvert(typeof(DateTimeOffset));
            Assert.IsFalse(canConvert);

            canConvert = PrecisionConverter.CanConvert(typeof(double));
            Assert.IsFalse(canConvert);
        }

        [TestMethod]
        public void CanReadShouldReturnFalse()
        {
            bool canRead = PrecisionConverter.CanRead;

            Assert.IsFalse(canRead);
        }

        [TestMethod]
        public void CanWriteShouldReturnTrue()
        {
            bool canWrite = PrecisionConverter.CanWrite;

            Assert.IsTrue(canWrite);
        }
    }
}
