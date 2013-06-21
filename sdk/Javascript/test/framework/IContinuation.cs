// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;

namespace Microsoft.Azure.Zumo.Win8.Test
{
    /// <summary>
    /// Represents an action that can be continued.
    /// </summary>
    public interface IContinuation
    {
        /// <summary>
        /// The operation invoking the continuation completed successfully so
        /// the continuation can resume normally.
        /// </summary>
        void Success();

        /// <summary>
        /// The operating invoking the continuation failed, so the continuation
        /// should handle the error.
        /// </summary>
        /// <param name="message">The error message.</param>
        void Error(string message);
    }
}
