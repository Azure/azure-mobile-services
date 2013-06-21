// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Azure.Zumo.Win8.Test
{
    /// <summary>
    /// A method and group filter that uses expressions and declared tags.
    /// </summary>
    internal class TagTestFilter : TestFilter
    {
        /// <summary>
        /// The reason used to exclude untagged tests.
        /// </summary>
        private const string ExcludedReason = "Tag expression does not match this test.";

        /// <summary>
        /// Initializes a instance of the TagTestFilter class.
        /// </summary>
        /// <param name="settings">Unit test settings.</para
        public TagTestFilter(TestSettings settings)
            : base(settings)
        {
        }

        /// <summary>
        /// Filter the list of active test methods to those matching the tag
        /// expression.
        /// </summary>
        /// <param name="groups">The test groups.</param>
        public override void Filter(IList<TestGroup> groups)
        {
            string tagExpression = this.Settings.TagExpression;
            if (!string.IsNullOrEmpty(tagExpression))
            {
                this.Settings.TestRunStatusMessage += " - Tag Expression '" + tagExpression + "'";
                foreach (TestGroup group in groups)
                {
                    List<TestMethod> activeMethods =
                        new TagManager(group).EvaluateExpression(tagExpression).ToList();
                    Remove(group.Methods, m => !activeMethods.Contains(m));                    
                }
                Remove(groups, g => g.Methods.Count == 0);
            }
        }
    }
}
