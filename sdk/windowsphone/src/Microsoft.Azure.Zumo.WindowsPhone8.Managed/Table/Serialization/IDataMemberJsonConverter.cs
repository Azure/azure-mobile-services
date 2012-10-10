// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Provides a way to apply custom conversion logic for the serialization
    /// or deserialization of a Mobile Services member.  A converter is
    /// associated with a given field via the DataMemberJsonConverterAttribute.
    /// </summary>
    public interface IDataMemberJsonConverter
    {
        /// <summary>
        /// Convert an object from its JSON representation.
        /// </summary>
        /// <param name="value">The JSON value.</param>
        /// <returns>A deserialized instance.</returns>
        object ConvertFromJson(JToken value);

        /// <summary>
        /// Convert an instance into its serialized JSON representation.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns>Serialized JSON value.</returns>
        JToken ConvertToJson(object instance);
    }
}
