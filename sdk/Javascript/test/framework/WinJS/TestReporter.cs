// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;

namespace Microsoft.Azure.Zumo.Win8.Test.WinJS
{
    /// <summary>
    /// Defines the interface used to report test results in a WinJS test
    /// runner.
    /// </summary>
    /// <remarks>
    /// This class is a hack.  WinJS types can't implement interfaces, so we're
    /// going to instead define an event for each interface method that we can
    /// invoke.
    /// </remarks>
    public sealed class TestReporter : ITestReporter
    {
        /// <summary>
        /// The test run started.
        /// </summary>
        public event EventHandler<TestHarness> ExecuteStartRun;

        /// <summary>
        /// Update the test run progress.
        /// </summary>
        public event EventHandler<TestHarness> ExecuteProgress;

        /// <summary>
        /// The test run finished.
        /// </summary>
        public event EventHandler<TestHarness> ExecuteEndRun;

        /// <summary>
        /// The test group started is about to start executing its tests.
        /// </summary>
        public event EventHandler<TestGroup> ExecuteStartGroup;

        /// <summary>
        /// The test group has finished executing its tests.
        /// </summary>
        public event EventHandler<TestGroup> ExecuteEndGroup;

        /// <summary>
        /// The test method is about to be executed.
        /// </summary>
        public event EventHandler<TestMethod> ExecuteStartTest;
        
        /// <summary>
        /// The test method has finished executing.
        /// </summary>
        public event EventHandler<TestMethod> ExecuteEndTest;

        /// <summary>
        /// A message has been logged.
        /// </summary>
        public event EventHandler<MessageEventArgs> ExecuteLog;

        /// <summary>
        /// An error has been raised.
        /// </summary>
        public event EventHandler<MessageEventArgs> ExecuteError;

        /// <summary>
        /// The status has been changed.
        /// </summary>
        public event EventHandler<MessageEventArgs> ExecuteStatus;
        
        /// <summary>
        /// Raise an event.
        /// </summary>
        /// <typeparam name="T">The event arguments type.</typeparam>
        /// <param name="handler">The event handler to invoke.</param>
        /// <param name="args">The event arguments.</param>
        private void RaiseEvent<T>(EventHandler<T> handler, T args)
        {
            if (handler != null)
            {
                handler(this, args);
            }
        }

        /// <summary>
        /// The test run started.
        /// </summary>
        /// <param name="harness">The test harness.</param>
        public void StartRun(TestHarness harness)
        {
            this.RaiseEvent(this.ExecuteStartRun, harness);
        }

        /// <summary>
        /// Update the test run progress.
        /// </summary>
        /// <param name="harness">
        /// The test harness (which contains Progress, Error, and Count
        /// properties).
        /// </param>
        public void Progress(TestHarness harness)
        {
            this.RaiseEvent(this.ExecuteProgress, harness);
        }

        /// <summary>
        /// The test run finished.
        /// </summary>
        /// <param name="harness">
        /// The test harness (which contains Error and Count properties).
        /// </param>
        public void EndRun(TestHarness harness)
        {
            this.RaiseEvent(this.ExecuteEndRun, harness);
        }

        /// <summary>
        /// The test group started is about to start executing its tests.
        /// </summary>
        /// <param name="group">The test group.</param>
        public void StartGroup(TestGroup group)
        {
            this.RaiseEvent(this.ExecuteStartGroup, group);
        }

        /// <summary>
        /// The test group has finished executing its tests.
        /// </summary>
        /// <param name="group">The test group.</param>
        public void EndGroup(TestGroup group)
        {
            this.RaiseEvent(this.ExecuteEndGroup, group);
        }

        /// <summary>
        /// The test method is about to be executed.
        /// </summary>
        /// <param name="test">The test method.</param>S
        public void StartTest(TestMethod test)
        {
            this.RaiseEvent(this.ExecuteStartTest, test);
        }

        /// <summary>
        /// The test method has finished executing.
        /// </summary>
        /// <param name="method">The test method.</param>
        public void EndTest(TestMethod test)
        {
            this.RaiseEvent(this.ExecuteEndTest, test);
        }

        /// <summary>
        /// A message has been logged.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Log(string message)
        {
            this.RaiseEvent(this.ExecuteLog, new MessageEventArgs { Message = message });
        }

        /// <summary>
        /// An error has been raised.
        /// </summary>
        /// <param name="errorDetails">Information about the error.</param>
        public void Error(string errorDetails)
        {
            this.RaiseEvent(this.ExecuteError, new MessageEventArgs { Message = errorDetails });
        }

        /// <summary>
        /// The status has been updated.
        /// </summary>
        /// <param name="status">The status message.</param>
        public void Status(string status)
        {
            this.RaiseEvent(this.ExecuteStatus, new MessageEventArgs { Message = status });
        }
    }
}
