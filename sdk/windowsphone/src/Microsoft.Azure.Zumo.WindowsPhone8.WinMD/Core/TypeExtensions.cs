// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Extensions to make accessing type info easier (especially given all of
    /// the new reflection changes).
    /// </summary>
    internal static partial class TypeExtensions
    {
        /// <summary>
        /// Convert the value into a URI id literal for URIs like
        /// {app-uri}/tables/{table}/{id}.
        /// </summary>
        /// <param name="id">The id to convert.</param>
        /// <returns>The corresponding URI literal.</returns>
        public static string ToUriConstant(object id)
        {
            Debug.Assert(id != null, "id cannot be null!");

            // At some point we may want to get more elaborate here, but for
            // now we'll just use ToString().
            return id.ToString();
        }

        /// <summary>
        /// Convert a date to the ISO 8601 roundtrip format supported by the
        /// server.
        /// </summary>
        /// <param name="date">The date to convert.</param>
        /// <returns></returns>
        public static string ToRoundtripDateString(this DateTime date)
        {
            return date.ToUniversalTime().ToString(
                "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffK",
                CultureInfo.InvariantCulture);
        }
    }
}
