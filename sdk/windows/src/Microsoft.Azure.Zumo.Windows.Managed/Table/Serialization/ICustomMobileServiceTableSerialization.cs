// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using Windows.Data.Json;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// The ICustomMobileServiceTableSerialization interface allows types to
    /// define custom serialization and deserialization behavior.  It allows
    /// for scenarios like versioning data models or accepting arbitrary sets
    /// of properties.
    /// </summary>
    /// <remarks>
    /// Note that MobileServiceTableSerializer supports an
    /// ignoreCustomSerialization flag to enable the base behavior from the
    /// Serialize and Deserialize methods if you only want to perform small
    /// tweaks before or after.
    /// </remarks>
    public interface ICustomMobileServiceTableSerialization
    {
        /// <summary>
        /// Serialize this instance to a JSON value.
        /// </summary>
        /// <returns>The serialized JSON value.</returns>
        IJsonValue Serialize();

        /// <summary>
        /// Deserialize a JSON value into this instance.
        /// </summary>
        /// <param name="value">The JSON value to deserialize.</param>
        void Deserialize(IJsonValue value);
    }
}
