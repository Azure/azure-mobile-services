// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;

using Windows.Foundation.Collections;
using Windows.Storage;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// An implementation of the <see cref="IApplicationStorage"/> interface
    /// for the Windows Store Platform that uses the Windows Store
    /// <see cref="ApplicationData"/> APIs.
    /// </summary>
    internal class ApplicationStorage : IApplicationStorage
    {
        /// <summary>
        /// A singleton instance of the <see cref="IApplicationStorage"/>.
        /// </summary>
        private static readonly IApplicationStorage instance = new ApplicationStorage();

        private ApplicationStorage()
        {
            this.Values = ApplicationData.Current.LocalSettings.Values;
        }

        internal ApplicationStorage(string name)
        {
            this.Values = ApplicationData.Current.LocalSettings.CreateContainer(name, ApplicationDataCreateDisposition.Always).Values;
        }

        /// <summary>
        /// A singleton instance of the <see cref="IApplicationStorage"/>.
        /// </summary>
        internal static IApplicationStorage Instance
        {
            get
            {
                return instance;
            }
        }

        public IPropertySet Values { get; set; }

        /// <summary>
        /// Tries to read a setting's value from application storage. 
        /// </summary>
        /// <param name="name">The name of the setting to try to read.
        /// </param>
        /// <param name="value">Upon returning, if the return value was <c>true</c>, 
        /// will be the value of the given setting; will be <c>null</c> otherwise.
        /// </param>
        /// <returns>
        /// <c>true</c> if the setting existed and was successfully read; <c>false</c> otherwise.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if the setting name is <c>null</c> or whitespace.
        /// </exception>
        bool IApplicationStorage.TryReadSetting(string name, out object value)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                string message = Resources.IApplicationStorage_NullOrWhitespaceSettingName;
                throw new ArgumentException(message);
            }

            return this.Values.TryGetValue(name, out value);
        }

        /// <summary>
        /// Writes the setting's value to application storage.
        /// </summary>
        /// <param name="name">The name of the setting to write.
        /// </param>
        /// <param name="value">The value to write.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown if the setting name is <c>null</c> or empty.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if an error occurs while writing the setting.
        /// </exception>
        void IApplicationStorage.WriteSetting(string name, object value)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                string message = Resources.IApplicationStorage_NullOrWhitespaceSettingName;
                throw new ArgumentException(message);
            }

            this.Values[name] = value;
        }

        public void Save()
        {
            // This operation is a no-op on Windows 8
        }        
    }
}