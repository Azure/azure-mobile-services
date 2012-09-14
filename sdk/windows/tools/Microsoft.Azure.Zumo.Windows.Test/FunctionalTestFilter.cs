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
    internal class FunctionalTestFilter : TestFilter
    {
        /// <summary>
        /// The custom key for the runtime url.
        /// </summary>
        private const string RuntimeUrlKey = "MobileServiceRuntimeUrl";

        /// <summary>
        /// The tag used to mark functional tests.
        /// </summary>
        private const string FunctionalTag = "Functional";

        /// <summary>
        /// The reason used to exclude functional tests.
        /// </summary>
        private const string ExcludedReason = "No Mobile Service Runtime server provided for functional tests.";

        /// <summary>
        /// Initializes a new instance of the FunctionalTestFilter class.
        /// </summary>
        /// <param name="settings">The test settings.</param>
        public FunctionalTestFilter(TestSettings settings)
            : base(settings)
        {
        }

        /// <summary>
        /// Filter the tests according to whether functional tests are enabled.
        /// </summary>
        /// <param name="groups">The groups to test.</param>
        public override void Filter(IList<TestGroup> groups)
        {
            string runtimeUrl = null;
            bool enableFunctionalTests = this.Settings.Custom.TryGetValue(RuntimeUrlKey, out runtimeUrl);
            enableFunctionalTests &= !string.IsNullOrEmpty(runtimeUrl);

            if (enableFunctionalTests)
            {
                this.Settings.TestRunStatusMessage += " - Functional Test Server " + runtimeUrl;
            }
            else
            {
                this.Settings.TestRunStatusMessage += " - Excluding Functional Tests";
                Remove(groups, g => g.Tags.Contains(FunctionalTag));
                foreach (TestGroup group in groups)
                {
                    Remove(group.Methods, m => m.Tags.Contains(FunctionalTag));
                }
                Remove(groups, g => g.Methods.Count == 0);
            }
        }
    }
}
