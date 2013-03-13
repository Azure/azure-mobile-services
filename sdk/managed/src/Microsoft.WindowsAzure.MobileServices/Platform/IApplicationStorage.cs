// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// An interface for platform-specific assemblies to implement to support 
    /// accessing settings in application storage.
    /// </summary>
    /// <remarks>
    /// Only base data types are supported for setting values: int, string, bool, 
    /// etc.
    /// </remarks>
    interface IApplicationStorage
    {
        /// <summary>
        /// Tries to read a setting's value from application storage. 
        /// </summary>
        /// <param name="name">
        /// The name of the setting to try to read.
        /// </param>
        /// <param name="value">
        /// Upon returning, if the return value was <c>true</c>, will be the value
        /// of the given setting; will be <c>null</c> otherwise.
        /// </param>
        /// <returns>
        /// <c>true</c> if the setting existed and was successfully read; 
        /// <c>false</c> otherwise.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if the setting name is <c>null</c> or an empty string.
        /// </exception>
        bool TryReadSetting(string name, out object value);

        /// <summary>
        /// Writes the setting's value to application storage.
        /// </summary>
        /// <param name="name">
        /// The name of the setting to write.
        /// </param>
        /// <param name="value">
        /// The value to write.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown if the setting name is <c>null</c> or empty.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if an error occurs while writing the setting.
        /// </exception>
        void WriteSetting(string name, object value);
    }
}
