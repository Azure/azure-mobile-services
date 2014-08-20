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
        private const string Platform = "platform";

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
            string platform = null;
            bool dotNet = false;
            this.Settings.Custom.TryGetValue(Platform, out platform);

            this.Settings.TestRunStatusMessage += " - " + platform + " Runtime";
            platform += "_not_supported";
            Remove(groups, g => g.Tags.Contains(platform));
            foreach (TestGroup group in groups)
            {
                Remove(group.Methods, m => m.Tags.Contains(platform));
            }
            Remove(groups, g => g.Methods.Count == 0);
        }
    }
}
