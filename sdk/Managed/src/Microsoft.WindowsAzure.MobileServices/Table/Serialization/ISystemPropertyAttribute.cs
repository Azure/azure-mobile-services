// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Interface for attributes applied to a member of a type to 
    /// specify that the member represents a system property.
    /// </summary>
    internal interface ISystemPropertyAttribute
    {
        /// <summary>
        /// Gets the system property the attribute represents.
        /// </summary>
        MobileServiceSystemProperties SystemProperty { get; }
    }
}
