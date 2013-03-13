// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// The ITotalCountProvider interface provides the total count for all the
    /// records that would have been returned ignoring any take paging/limit
    /// clause specified by client or server.  If you call
    /// query.RequestTotalCount(), you can cast the result (whether its a
    /// sequence or a list) to ITotalCountProvider.
    /// </summary>
    public interface ITotalCountProvider
    {
        /// <summary>
        /// Gets the total count for all the records that would have been
        /// returned ignoring any take paging/limit clause specified by client
        /// or server.
        /// </summary>
        long TotalCount { get; }
    }
}
