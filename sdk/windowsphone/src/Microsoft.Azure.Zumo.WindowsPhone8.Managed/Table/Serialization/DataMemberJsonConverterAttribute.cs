// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Provides the ability to perform custom JSON serialization and
    /// deserialization for a given member.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class DataMemberJsonConverterAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the type of the converter that will be created to
        /// serialize or deserialize the value of the member to or from JSON.
        /// The type must iplement the IDataMemberJsonConverter interface.
        /// </summary>
        public Type ConverterType { get; set; }
    }
}
