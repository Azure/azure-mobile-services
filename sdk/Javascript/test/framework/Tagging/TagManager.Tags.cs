// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Azure.Zumo.Win8.Test
{
    /// <summary>
    /// A helper class that manages tags and associated metadata. Tag
    /// expressions are evaluated at the TestGroup level.
    /// </summary>
    internal sealed partial class TagManager
    {
        /// <summary>
        /// A string list for storing tags. Provides an Add method that takes 
        /// an attribute object and, if a TagAttribute, will append its tag 
        /// value to the list.
        /// </summary>
        private class Tags : List<string>
        {
            /// <summary>
            /// Initializes a new Tags instance.
            /// </summary>
            public Tags() :
                base()
            {
            }

            /// <summary>
            /// Initializes a new Tags instance from an existing collection.
            /// </summary>
            /// <param name="collection">The collection to copy.</param>
            public Tags(IEnumerable<string> collection) :
                base(collection)
            {
            }

            /// <summary>
            /// Adds a TagAttribute's tag value.
            /// </summary>
            /// <param name="tag">The tag object.</param>
            public void AddTag(string tag)
            {
                if (!string.IsNullOrEmpty(tag) && !Contains(tag))
                {
                    Add(tag);
                }
            }
        }
    }
}
