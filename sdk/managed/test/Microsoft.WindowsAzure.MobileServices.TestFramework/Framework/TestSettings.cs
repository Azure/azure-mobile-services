// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.WindowsAzure.MobileServices.TestFramework
{
    /// <summary>
    /// Defines settings for the current test pass that can be provided via
    /// a test application, passed to test filters, etc.
    /// </summary>
    public sealed class TestSettings
    {
        /// <summary>
        /// Initializes a new instance of the TestSettings class.
        /// </summary>
        public TestSettings()
        {
            this.TestRunStatusMessage = "Test Run";
            this.TestResultsServerUrl = "";
            this.BreakOnStart = true;
            this.TagExpression = "";
            this.Custom = new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets or sets a string containing the test run status message.
        /// </summary>
        public string TestRunStatusMessage { get; set; }

        /// <summary>
        /// Gets or sets a URL where test results are posted.
        /// </summary>
        /// <remarks>
        /// This is passed to the test application as an initialization
        /// parameter and is used to enable test runs from the build.  The test
        /// launcher used by the build will create a simple web server (at this
        /// URL) to listen for the posted test results.
        /// </remarks>
        public string TestResultsServerUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the harness should break
        /// before executing (so a debugger can be attached).
        /// </summary>
        public bool BreakOnStart { get; set; }

        /// <summary>
        /// Gets or sets the tag expression used to filter the tests that we
        /// will run.
        /// </summary>
        public string TagExpression { get; set; }

        /// <summary>
        /// Gets a dictionary of custom test settings that are specific to a
        /// given application.
        /// </summary>
        public IDictionary<string, string> Custom { get; private set; }
    }
}
