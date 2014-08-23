// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// The IQueryResultProvider interface provides extra details returned with response
    /// e.g. total count and next link
    /// </summary>
    public interface IQueryResultProvider
    {
        /// <summary>
        /// Gets the link to next page of result that is returned in response headers.
        /// </summary>
        string NextLink { get; }

        /// <summary>
        /// Gets the total count for all the records that would have been
        /// returned ignoring any take paging/limit clause specified by client
        /// or server.
        /// </summary>
        long TotalCount { get; }
    }
}
