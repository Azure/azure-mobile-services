// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Azure.Zumo.Win8.Test
{
    /// <summary>
    /// An individual test method to be executed.
    /// </summary>
    internal class RuntimeTestFilter : TestFilter
    {

        /// <summary>
        /// The custom key for the runtime url.
        /// </summary>
        private const string DotNet = "dotNet";

        /// <summary>
        /// The tag used to mark node.js tests.
        /// </summary>
        private const string NodeTag = "node";

        /// <summary>
        /// Initializes a new instance of the FunctionalTestFilter class.
        /// </summary>
        /// <param name="settings">The test settings.</param>
        public RuntimeTestFilter(TestSettings settings)
            : base(settings)
        {
        }

        /// <summary>
        /// Filter the tests according to whether functional tests are enabled.
        /// </summary>
        /// <param name="groups">The groups to test.</param>
        public override void Filter(IList<TestGroup> groups)
        {
            string runtimeUrl = null, runtimeToRemove;
            bool dotNet = false;
            this.Settings.Custom.TryGetValue(DotNet, out runtimeUrl);
            bool.TryParse(runtimeUrl, out dotNet);
            runtimeToRemove = dotNet ? NodeTag : DotNet;

            this.Settings.TestRunStatusMessage += " - " + runtimeToRemove + " Runtime";
            Remove(groups, g => g.Tags.Contains(runtimeToRemove));
            foreach (TestGroup group in groups)
            {
                Remove(group.Methods, m => m.Tags.Contains(runtimeToRemove));
            }
            Remove(groups, g => g.Methods.Count == 0);
        }
    }
}
