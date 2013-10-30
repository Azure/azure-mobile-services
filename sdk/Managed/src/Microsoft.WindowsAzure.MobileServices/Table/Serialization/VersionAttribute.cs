// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Attribute applied to a member of a type to specify that it represents
    /// the __version system property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class VersionAttribute :  Attribute, ISystemPropertyAttribute
    {
        /// <summary>
        /// Initializes a new instance of the VersionAttribute class.
        /// </summary>
        public VersionAttribute()
        {
        }

        /// <summary>
        /// Gets the system property the attribute represents.
        /// </summary>
        MobileServiceSystemProperties ISystemPropertyAttribute.SystemProperty
        {
            get 
            {
                return MobileServiceSystemProperties.Version;
            }
        }
    }
}
