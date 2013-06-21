// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;

namespace Microsoft.Azure.Zumo.Win8.Test
{
    /// <summary>
    /// A continuation with actions that will be invoked when the operation
    /// completes or errors.
    /// </summary>
    internal class ActionContinuation : IContinuation
    {
        /// <summary>
        /// Gets the action to take if the continuation is invoked from a
        /// successful operation.
        /// </summary>
        public Action OnSuccess { get; set; }

        /// <summary>
        /// Gets the action to take if the continuation is invoked from a
        /// failing operation.
        /// </summary>
        public Action<string> OnError { get; set; }

        /// <summary>
        /// Invoke the continuation from a successful operation.
        /// </summary>
        void IContinuation.Success()
        {
            this.OnSuccess();
        }

        /// <summary>
        /// Invoke the continuation from a failing operation.
        /// </summary>
        /// <param name="message">The error message.</param>
        void IContinuation.Error(string message)
        {
            this.OnError(message);
        }
    }
}
