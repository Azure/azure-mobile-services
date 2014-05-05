// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.ComponentModel;
namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Provides an interface that platform-specific Mobile Services assemblies 
    /// can implement to provide functionality required by the Mobile Services SDK 
    /// that is platform specific.
    /// </summary>
    internal interface IPlatform
    {
        /// <summary>
        /// Returns a platform-specific implemention of application storage.
        /// </summary>
        IApplicationStorage ApplicationStorage { get; }

        /// <summary>
        /// Returns a platform-specific implemention of platform information.
        /// </summary>
        IPlatformInformation PlatformInformation { get; }

        /// <summary>
        /// Returns a platform-specific implementation of a utility class
        /// that provides functionality for manipulating 
        /// <see cref="System.Linq.Expressions.Expression"/> instances.
        /// </summary>
        IExpressionUtility ExpressionUtility { get; }

        /// <summary>
        /// Returns a platform-specific implementation of a utility class
        /// that provides functionality for platform-specifc push capabilities.
        /// </summary>
        IPushUtility PushUtility { get; }

        /// <summary>
        /// Retrieves an ApplicationStorage where all items stored are segmented from other stored items
        /// </summary>
        /// <param name="name">The name of the segemented area in application storage</param>
        /// <returns>The specific instance of that segment</returns>
        IApplicationStorage GetNamedApplicationStorage(string name);
    }
}
