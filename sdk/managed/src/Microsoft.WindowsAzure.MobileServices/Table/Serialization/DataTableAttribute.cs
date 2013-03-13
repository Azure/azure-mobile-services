// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Attribute applied to a type to specify the Mobile Service table it
    /// represents.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DataTableAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the DataTableAttribute class.
        /// </summary>
        /// <param name="name">The name of the table the class represents.</param>
        public DataTableAttribute(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Gets the name of the table the class represents.
        /// </summary>
        public string Name { get; private set; }
    }
}
