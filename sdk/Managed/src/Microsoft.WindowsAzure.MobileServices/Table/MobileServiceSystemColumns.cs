// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------


namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Name of system columns on a Mobile Service table.
    /// </summary>
    public static class MobileServiceSystemColumns
    {
        /// <summary>
        /// The id column on a Mobile Service table.
        /// </summary>
        public static readonly string Id = "id";

        /// <summary>
        /// The version column on a Mobile Service table.
        /// </summary>
        public static readonly string Version = "__version";

        /// <summary>
        /// The createdAt column on a Mobile Service table.
        /// </summary>
        public static readonly string CreatedAt = "__createdAt";

        /// <summary>
        /// The updatedAt column on a Mobile Service table.
        /// </summary>
        public static readonly string UpdatedAt = "__updatedAt";

        /// <summary>
        /// The deleted colum on a Mobile Service table.
        /// </summary>
        public static readonly string Deleted = "__deleted";
    }
}