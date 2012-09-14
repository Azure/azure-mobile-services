// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Attribute applied to a type to specify the Mobile Services table it
    /// represents.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DataTableAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name of the table.
        /// </summary>
        public string Name { get; set; }
    }
}
