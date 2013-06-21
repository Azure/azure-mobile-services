// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// An implementation of <see cref="JsonConverter"/> to be used with
    /// <see cref="long"/>, <see cref="ulong"/> and <see cref="decimal"/> property
    /// types that only writes the values if we can ensure that precision will not 
    /// be lost if the value is serialized and sent to the server.
    /// </summary>
    public class MobileServicePrecisionCheckConverter : JsonConverter
    {
        /// <summary>
        /// The magnitude of the upper limit of long values such that all values 
        /// equal to or less than this value can all be converted to a double without
        /// a loss of precision.
        /// </summary>
        private static readonly long maxLongMagnitude = 0x20000000000000; // 2^53

        /// <summary>
        /// The magnitude of the upper limit of ulong values such that all values 
        /// equal to or less than this value can all be converted to a double without
        /// a loss of precision.
        /// </summary>
        private static readonly ulong maxUnsignedLongMagnitude = 0x20000000000000; // 2^53

        /// <summary>
        /// The magnitude of the upper limit of decimal values which are not whole numbers
        /// such that all values equal to or less than this value can all be converted to a 
        /// double without a loss of precision. We can guarantee this for decimal with at most
        /// 15 significant digits.
        /// </summary>
        private static readonly decimal maxDecimalNotWholeNumberMagnitude = 999999999999999; // max number with 15 digits

        /// <summary>
        /// Indicates if the specified type can be converted by this converter.
        /// </summary>
        /// <param name="objectType">The type to check.</param>
        /// <returns>A bool indicating if this converter can convert the type.</returns>
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(decimal) ||
                     objectType == typeof(long) ||
                     objectType == typeof(ulong));
        }

        /// <summary>
        /// Indicates this <see cref="JsonConverter"/> should not be used during deserialization.
        /// </summary>
        public override bool CanRead
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Reading is not supported for this converter.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotSupportedException(
                string.Format(CultureInfo.InvariantCulture,
                            Resources.MobileServicePrecisionCheckConverter_ReadNotSupported,
                            typeof(MobileServicePrecisionCheckConverter).Name));
        }

        /// <summary>
        /// Writes the <paramref name="value"/> to Json only if we can ensure that the value 
        /// will not lose precision when sent to the server.
        /// Otherwise it throws an exception.
        /// </summary>
        /// <param name="writer">
        /// The JsonWriter instance to use.
        /// </param>
        /// <param name="value">
        /// The value to check on write.
        /// </param>
        /// <param name="serializer">
        /// The current Serializer.
        /// </param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            bool isOutOfRange = false;

            if (value is long)
            {
                long asLong = (long)value;
                isOutOfRange = asLong > maxLongMagnitude || asLong < -maxLongMagnitude;
            }
            else if (value is ulong)
            {
                ulong asUlong = (ulong)value;
                isOutOfRange = asUlong > maxUnsignedLongMagnitude;
            }
            else if (value is decimal)
            {
                decimal asDecimal = (decimal)value;
                // Retrieve a binary representation of the Decimal. The return value is an 
                // integer array with four elements. Elements 0, 1, and 2 contain the low,
                // middle, and high 32 bits of the 96-bit integer part of the Decimal.
                // Element 3 contains the scale factor and sign of the Decimal: bits 0-15
                // (the lower word) are unused; bits 16-23 contain a value between 0 and 
                // 28, indicating the power of 10 to divide the 96-bit integer part by to
                // produce the Decimal value; bits 24-30 are unused; and finally bit 31 
                // indicates the sign of the Decimal value, 0 meaning positive and 1 
                // meaning negative.
                int[] bits = Decimal.GetBits(asDecimal);
                //create number out of the first 64 bits
                ulong number = (ulong)((uint)bits[1]) << 32 | (uint)bits[0];
                // check if decimal actually presents a whole number
                bool isWholeNumber = asDecimal % 1 == 0;
                // whole number and greater than whole number limit
                bool isWholeNumberOutOfRange = isWholeNumber && (bits[2] != 0 || number > maxUnsignedLongMagnitude);
                // not a whole number and greater than limit
                // if bits[2] is not null the number is too big no matter what
                bool isDecimalNumberOutOfRange = !isWholeNumber && (bits[2] != 0 || number > maxDecimalNotWholeNumberMagnitude);
                // combine cases
                isOutOfRange = isWholeNumberOutOfRange || isDecimalNumberOutOfRange;
            }

            if (isOutOfRange)
            {
                throw new InvalidOperationException(
                    string.Format(CultureInfo.InvariantCulture,
                                  Resources.MobileServiceContractResolver_NumberOutOfRange,
                                  value,
                                  writer.Path));
            }

            //write the value to the stream.
            writer.WriteValue(value);
        }
    }
}
