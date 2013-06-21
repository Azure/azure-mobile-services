// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;

namespace Microsoft.Azure.Zumo.Win8.Test
{
    /// <summary>
    /// Represents an async operation that can be given a continuation to 
    /// invoke when the async operation has completed (either successfully or
    /// with an error).
    /// </summary>
    public interface IAsyncExecution
    {
        /// <summary>
        /// Start the async operation.
        /// </summary>
        /// <param name="continuation">
        /// Continuation that should be invoked when the async operation has
        /// completed.
        /// </param>
        void Start(IContinuation continuation);
    }
}
