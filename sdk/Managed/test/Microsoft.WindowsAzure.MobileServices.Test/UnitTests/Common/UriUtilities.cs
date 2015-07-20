// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;

namespace Microsoft.WindowsAzure.MobileServices.Test.UnitTests.Common
{
    /// <summary>
    /// Utilities for URL manipulations
    /// </summary>
    public static class UriUtilities
    {
        public const char Slash = '/';

        /// <summary>
        /// Remove trailing slash from <paramref name="uri"/>, if any.
        /// </summary>
        public static string RemoveTrailingSlash(string uri)
        {
            return uri.TrimEnd(Slash);
        }
    }
}
