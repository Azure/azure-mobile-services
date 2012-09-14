// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Azure.Zumo.Win8.Test
{
    /// <summary>
    /// An individual test method to be executed.
    /// </summary>
    public sealed class TestMethod
    {
        /// <summary>
        /// Initializes a new instance of the TestMethod class.
        /// </summary>
        public TestMethod()
        {
            // WinJS is unhappy if we attempt to get a null string
            this.Name = string.Empty;
            this.Description = string.Empty;
            this.ExcludeReason = string.Empty;
            this.Tags = new List<string>();
            this.ErrorInfo = string.Empty;
        }

        /// <summary>
        /// Gets or sets the name of the test method.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a more detailed description of the test method.
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the test should be excluded
        /// from execution.
        /// </summary>
        public bool Excluded { get; private set; }
        
        /// <summary>
        /// Gets or sets the reason the test was excluded (usually referencing
        /// a known bug or work item).
        /// </summary>
        public string ExcludeReason { get; private set; }

        /// <summary>
        /// Gets the list of tags associated with this test method.
        /// </summary>
        public IList<string> Tags { get; private set; }
        
        /// <summary>
        /// The test action to execute.  This could be sync or async.  Any
        /// exceptions raised by invoking the test will be considered a test
        /// failure.
        /// </summary>
        public IAsyncExecution Test { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the test execution passed.
        /// </summary>
        public bool Passed { get; set; }
        
        /// <summary>
        /// Gets or sets any error information associated with a failing test.
        /// </summary>
        public string ErrorInfo { get; set; }

        /// <summary>
        /// Exclude the test method.
        /// </summary>
        /// <param name="reason">
        /// The reason for excluding the test method.
        /// </param>
        public void Exclude(string reason)
        {
            Excluded = true;
            ExcludeReason = reason ?? "";
        }
    }
}
