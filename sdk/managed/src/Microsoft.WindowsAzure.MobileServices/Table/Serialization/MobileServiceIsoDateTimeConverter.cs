// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Converts DateTime and DateTimeOffset object into UTC DateTime and creates a ISO string representation
    /// by calling ToUniversalTime on serialization and ToLocalTime on deserialization.
    /// </summary>
    public class MobileServiceIsoDateTimeConverter : IsoDateTimeConverter
    {
        /// <summary>
        /// Creates a new instance of <see cref="MobileServiceIsoDateTimeConverter"/>.
        /// </summary>
        public MobileServiceIsoDateTimeConverter()
        {
            this.Culture = CultureInfo.InvariantCulture;
            this.DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffK";
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            object datetimeObject = base.ReadJson(reader, objectType, existingValue, serializer);

            if(datetimeObject != null)
            {
                if(datetimeObject is DateTime)
                {
                    return ((DateTime)datetimeObject).ToLocalTime();
                }
                else if(datetimeObject is DateTimeOffset)
                {
                    return new DateTimeOffset((DateTime)reader.Value).ToLocalTime();
                }
            }

            return datetimeObject;
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            DateTime dateTime;
            if (value is DateTime)
            {
                dateTime = ((DateTime)value).ToUniversalTime();
            }
            else
            {
                dateTime = ((DateTimeOffset)value).UtcDateTime;
            }

            base.WriteJson(writer, dateTime, serializer);
        }
    }
}
