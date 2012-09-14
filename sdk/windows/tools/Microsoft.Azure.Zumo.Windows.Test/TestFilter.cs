// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.Azure.Zumo.Win8.Test
{
    /// <summary>
    /// A filter that can be used to exclude a set of tests.
    /// </summary>
    internal abstract class TestFilter
    {
        /// <summary>
        /// Initializes a new instance of the TestFilter class.
        /// </summary>
        /// <param name="settings">The test settings.</param>
        public TestFilter(TestSettings settings)
        {
            Debug.Assert(settings != null, "settings should not be null!");
            this.Settings = settings;
        }
        
        /// <summary>
        /// Gets the TestSettings to be used by the filter.
        /// </summary>
        public TestSettings Settings { get; private set; }

        /// <summary>
        /// Filter the test groups.
        /// </summary>
        /// <param name="groups">The groups to test.</param>
        public virtual void Filter(IList<TestGroup> groups)
        {
        }

        /// <summary>
        /// Remove the elements matching a given predicate.
        /// </summary>
        /// <typeparam name="T">Type of the element.</typeparam>
        /// <param name="elements">The elements to remove.</param>
        /// <param name="predicate">The predicate to compare.</param>
        protected static void Remove<T>(IList<T> elements, Func<T, bool> predicate)
        {
            Debug.Assert(elements != null, "elements cannot be null!");
            Debug.Assert(predicate != null, "predicate cannot be null!");

            foreach (T element in elements.Where(predicate).ToList())
            {
                elements.Remove(element);
            }
        }
    }
}
