﻿// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.IO.IsolatedStorage;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// An implementation of the <see cref="IApplicationStorage"/> interface
    /// for the Windows Phone Platform that uses the Windows Phone
    /// <see cref="System.IO.IsolatedStorage.IsolatedStorageSettings"/> APIs.
    /// </summary>
    public class ApplicationStorage : IApplicationStorage
    {
        /// <summary>
        /// A singleton instance of the <see cref="ApplicationStorage"/>.
        /// </summary>
        private static readonly IApplicationStorage instance = new ApplicationStorage();        

        private ApplicationStorage() : this(string.Empty)
        {
        }

        internal ApplicationStorage(string name)
        {
            this.StoragePrefix = name;
        }

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

        private string StoragePrefix { get; set; }

        /// <summary>
        /// Tries to read a setting's value from 
        /// <see cref="IsolatedStorageSettings.ApplicationSettings"/>. 
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
                string message = "An application setting name must be provided. Null, empty or whitespace only names are not allowed.";
                throw new ArgumentException(message);
            }

            return IsolatedStorageSettings.ApplicationSettings.TryGetValue(string.Concat(this.StoragePrefix, name), out value);
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
        void IApplicationStorage.WriteSetting(string name, object value)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                string message = "An application setting name must be provided. Null, empty or whitespace only names are not allowed.";
                throw new ArgumentException(message);
            }

            IsolatedStorageSettings.ApplicationSettings[string.Concat(this.StoragePrefix, name)] = value;
        }

        void IApplicationStorage.Save()
        {
            IsolatedStorageSettings.ApplicationSettings.Save();
        }
    }
}
