// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.MobileServices.Http {
    /// <summary>
    /// The types of routes that the client can provide.
    /// </summary>
    public enum RouteKind
    {
        /// <summary>
        /// Represents a custom API route.
        /// </summary>
        API,

        /// <summary>
        /// Represents a table route.
        /// </summary>
        Table,

        /// <summary>
        /// Represents a login route.
        /// </summary>
        Login,

        /// <summary>
        /// Represents a push registration route.
        /// </summary>
        Push,
    }
}
