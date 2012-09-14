// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Azure.Zumo.Win8.Test
{
    /// <summary>
    /// A group of tests for a related functional area.
    /// </summary>
    public sealed class TestGroup
    {
        /// <summary>
        /// Initializes a new instance of the TestGroup class.
        /// </summary>
        public TestGroup()
        {
            this.Name = string.Empty;
            this.Methods = new List<TestMethod>();
            this.Tags = new List<string>();
        }

        /// <summary>
        /// Gets or sets the name of the test group.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the list of tags associated with this test group.
        /// </summary>
        public IList<string> Tags { get; private set; }

        /// <summary>
        /// Gets the list of methods available in the test group.
        /// </summary>
        public IList<TestMethod> Methods { get; private set; }

        /// <summary>
        /// Exclude all of the methods in a test group.
        /// </summary>
        /// <param name="reason">The reason to exclude the group.</param>
        public void Exclude(string reason)
        {
            foreach (TestMethod method in this.Methods)
            {
                // Only exclude if it isn't already to prevent stomping on the
                // exclude reason
                if (!method.Excluded)
                {
                    method.Exclude(reason);
                }
            }
        }
    }
}
