// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;

namespace Microsoft.Azure.Zumo.Win8.Test.WinJS
{
    /// <summary>
    /// Represents a WinJS async operation that can be given a continuation to 
    /// invoke when the async operation has completed (either successfully or
    /// with an error).
    /// </summary>
    /// <remarks>
    /// Given that we can't directly invoke WinJS code, we also use this class
    /// for synchronous operations and make sure everything works fine if we
    /// return immediately.
    /// </remarks>
    public sealed class PromiseAsyncExecution : IAsyncExecution
    {
        /// <summary>
        /// Event used to execute the WinJS test action.
        /// </summary>
        /// <remarks>
        /// We can't pass a WinJS function as a delegate via WinRT, so we're
        /// hacking around that by creating a stub which will wrap Execute and
        /// invoke the desired function.
        /// </remarks>
        public event EventHandler<PromiseAsyncExecutionEventArgs> Execute;
        
        /// <summary>
        /// Start the async WinJS operation.
        /// </summary>
        /// <param name="continuation">
        /// Continuation that should be invoked when the async operation has
        /// completed.
        /// </param>
        public void Start(IContinuation continuation)
        {
            EventHandler<PromiseAsyncExecutionEventArgs> handler = this.Execute;
            if (handler != null)
            {
                try
                {
                    // Invoke the WinJS operation by raising the Execute event
                    handler(this, new PromiseAsyncExecutionEventArgs(continuation));
                }
                catch (Exception ex)
                {
                    // In the event the operation is actually sync, we'll catch
                    // any errors now and pass them to the error continuation.
                    continuation.Error(ex.ToString());
                }
            }
        }
    }
}
