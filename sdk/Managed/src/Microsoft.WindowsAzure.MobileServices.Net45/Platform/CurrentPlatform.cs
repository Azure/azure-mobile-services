// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;

namespace Microsoft.WindowsAzure.MobileServices
{
    internal class CurrentPlatform : IPlatform
    {
        /// <summary>
        /// Returns a platform-specific implemention of application storage.
        /// </summary>
        public IApplicationStorage ApplicationStorage { get { return Microsoft.WindowsAzure.MobileServices.ApplicationStorage.Instance; } }

        /// <summary>
        /// Returns a platform-specific implemention of platform information.
        /// </summary>
        public IPlatformInformation PlatformInformation { get { return Microsoft.WindowsAzure.MobileServices.PlatformInformation.Instance; } }

        /// <summary>
        /// Returns a platform-specific implementation of a utility class
        /// that provides functionality for manipulating 
        /// <see cref="System.Linq.Expressions.Expression"/> instances.
        /// </summary>
        public IExpressionUtility ExpressionUtility { get { return Microsoft.WindowsAzure.MobileServices.ExpressionUtility.Instance; } }

        /// <summary>
        /// Returns a platform-specific implementation of a utility class
        /// that provides functionality for platform-specifc push capabilities.
        /// </summary>
        public IPushUtility PushUtility { get { return null; } }

        public IApplicationStorage GetNamedApplicationStorage(string name)
        {
            return new ApplicationStorage(name);
        }
    }
}
