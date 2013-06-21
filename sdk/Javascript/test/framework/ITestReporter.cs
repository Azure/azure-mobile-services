// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;

namespace Microsoft.Azure.Zumo.Win8.Test
{
    /// <summary>
    /// Defines the interface used to report test results.
    /// </summary>
    /// <remarks>
    /// This has been pulled into an interface since we'll need to have
    /// separate implementations of the UI for WWA and XAML tests.
    /// </remarks>
    public interface ITestReporter
    {
        /// <summary>
        /// The test run started.
        /// </summary>
        /// <param name="harness">The test harness.</param>
        void StartRun(TestHarness harness);
        
        /// <summary>
        /// Update the test run progress.
        /// </summary>
        /// <param name="harness">
        /// The test harness (which contains Progress, Error, and Count
        /// properties).
        /// </param>
        void Progress(TestHarness harness);

        /// <summary>
        /// The test run finished.
        /// </summary>
        /// <param name="harness">
        /// The test harness (which contains Error and Count properties).
        /// </param>
        void EndRun(TestHarness harness);
        
        /// <summary>
        /// The test group started is about to start executing its tests.
        /// </summary>
        /// <param name="group">The test group.</param>
        void StartGroup(TestGroup group);

        /// <summary>
        /// The test group has finished executing its tests.
        /// </summary>
        /// <param name="group">The test group.</param>
        void EndGroup(TestGroup group);

        /// <summary>
        /// The test method is about to be executed.
        /// </summary>
        /// <param name="test">The test method.</param>
        void StartTest(TestMethod test);

        /// <summary>
        /// The test method has finished executing.
        /// </summary>
        /// <param name="test">The test method.</param>
        void EndTest(TestMethod test);

        /// <summary>
        /// A message has been logged.
        /// </summary>
        /// <param name="message">The message.</param>
        void Log(string message);

        /// <summary>
        /// An error has been raised.
        /// </summary>
        /// <param name="errorDetails">Information about the error.</param>
        void Error(string errorDetails);

        /// <summary>
        /// Update the test run status.
        /// </summary>
        /// <param name="status">The status message.</param>
        void Status(string status);
    }
}
