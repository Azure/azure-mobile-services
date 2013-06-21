// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;

namespace Microsoft.Azure.Zumo.Win8.Test.WinJS
{
    /// <summary>
    /// Event arguments that wrap a continuation to be invoked when an async
    /// operation completes.
    /// </summary>
    public sealed class PromiseAsyncExecutionEventArgs
    {
        /// <summary>
        /// The continuation to invoke when the operation completes.
        /// </summary>
        private IContinuation continuation;

        /// <summary>
        /// Always throw an InvalidOperationException.
        /// </summary>
        /// <remarks>
        /// We're required provide a parameterless constructor for types
        /// exposed via WinMD even if that type will never be instantiated on
        /// the other side of the boundary.
        /// </remarks>
        public PromiseAsyncExecutionEventArgs()
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Initializes a new instance of the PromiseAsyncExecutionEventArgs
        /// class.
        /// </summary>
        /// <param name="continuation">
        /// The continuation to be invoked when the async operation completes.
        /// </param>
        public PromiseAsyncExecutionEventArgs(IContinuation continuation)
        {
            if (continuation == null)
            {
                throw new ArgumentNullException("continuation");
            }
            this.continuation = continuation;
        }

        /// <summary>
        /// Invoke the continuation from a successful async operation.
        /// </summary>
        public void Complete()
        {
            this.continuation.Success();
        }

        /// <summary>
        /// Invoke the continuation from a failing async operation.
        /// </summary>
        /// <param name="message">The error message.</param>
        public void Error(string message)
        {
            this.continuation.Error(message);
        }
    }
}
