// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// An implementation of the <see cref="IApplicationStorage"/> interface
    /// for the .NET Platform that uses .NET
    /// <see cref="System.Configuration.ApplicationSettingsBase"/> APIs.
    /// </summary>
    internal class ApplicationStorage : IApplicationStorage
    {
        /// <summary>
        /// A singleton instance of the <see cref="ApplicationStorage"/>.
        /// </summary>
        private static IApplicationStorage instance = new ApplicationStorage();

        /// <summary>
        /// A singleton instance of the <see cref="ApplicationStorage"/>.
        /// </summary>
        internal static IApplicationStorage Instance
        {
            get
            {
                return instance;
            }
        }

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

            try
            {
                using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.Assembly | IsolatedStorageScope.User, null, null))
                {
                    using (IsolatedStorageFileStream fileStream = isoStore.OpenFile(name, FileMode.OpenOrCreate, FileAccess.Read))
                    {
                        using (var reader = new StreamReader(fileStream))
                        {
                            value = reader.ReadToEnd();
                            return value != null;
                        }
                    }
                }
            }
            catch
            {
                value = Guid.Empty;
                return true;
            }
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

            try
            {
                using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.Assembly | IsolatedStorageScope.User, null, null))
                {
                    using (IsolatedStorageFileStream fileStream = isoStore.OpenFile(name, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        using (var writer = new StreamWriter(fileStream))
                        {
                            writer.WriteLine(value.ToString());
                        }
                    }
                }
            }
            catch { }
        }
    }
}
