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
        // This class is not meant to have a long lifetime as there are 
        // situations in a test run where TestMethod and TestGroup 
        // references in fact are renewed.
        // 
        // Three primary sets of relationships are stored:
        // _groupTags: List of tags for the Type associated with this instance
        // _methodTags: Relates TestMethod instances to a list of tags
        // _tagsToMethods: Relates a tag to a set of method metadata

        /// <summary>
        /// The test group being filtered.
        /// </summary>
        private TestGroup _group;

        /// <summary>
        /// The test tags associated with the class.
        /// </summary>
        private Tags _groupTags;

        /// <summary>
        /// The test tags associated with methods.
        /// </summary>
        private Dictionary<TestMethod, Tags> _methodTags;

        /// <summary>
        /// The ability to grab the set of methods, given a test class type, 
        /// and the tag of interest.
        /// </summary>
        private Dictionary<string, List<TestMethod>> _tagsToMethods;

        /// <summary>
        /// Gets or sets the universe of all test methods for expression 
        /// evaluation.
        /// </summary>
        private List<TestMethod> Universe { get; set; }

        /// <summary>
        /// Initializes a new instance of the TagManager class.
        /// </summary>
        /// <param name="group">The test group eing filter.</param>
        /// <param name="methods">The set of methods to run.</param>
        public TagManager(TestGroup group)
        {
            _group = group;
            _groupTags = new Tags();
            _methodTags = new Dictionary<TestMethod, Tags>();
            _tagsToMethods = new Dictionary<string, List<TestMethod>>();
            Universe = new List<TestMethod>(group.Methods);

            CreateClassTags(_group);
            foreach (TestMethod method in group.Methods)
            {
                CreateMethodTags(method);
            }
        }

        /// <summary>
        /// Reflect, read and prepare the tags for the group metadata. Performs 
        /// the work if this is the first time the metadata has been seen.
        /// </summary>
        /// <param name="group">The test group.</param>
        private void CreateClassTags(TestGroup group)
        {
            // 1. Full class name
            //_groupTags.AddTag(group.FullName);

            // 2. Class name
            _groupTags.AddTag(group.Name);

            // 3. All [Tag] attributes on the type
            foreach (string tag in group.Tags)
            {
                _groupTags.AddTag(tag);
            }
        }

        /// <summary>
        /// Reflect, read and prepare the tags for the method metadata.
        /// Performs the work if this is the first time the metadata has been
        /// seen.
        /// </summary>
        /// <param name="method">The method metadata.</param>
        private void CreateMethodTags(TestMethod method)
        {
            // 1. All the tags from the group
            Tags tags = new Tags(_groupTags);

            // 2. Method.Name
            tags.AddTag(method.Name);

            // 3. Type.FullName + Method.Name
            //tags.AddTag(m.ReflectedType.FullName + "." + method.Name);

            // 4. Type.Name + Method.Name
            //tags.AddTag(method.ReflectedType.Name + "." + method.Name);
            
            // 5. Implicit Inherited tag
            //if (method.ReflectedType != method.DeclaringType)
            //{
            //    tags.AddTag("Inherited");
            //}

            // 6. All [Tag] attributes on the method
            foreach (string tag in method.Tags)
            {
                tags.AddTag(tag);
            }

            // 7. Add priority as a tag
            //if (method.Priority != null)
            //{
            //    tags.AddTag(PriorityTagPrefix + method.Priority.ToString());
            //}

            _methodTags.Add(method, tags);

            // Populate the inverted index
            foreach (string tag in tags)
            {
                List<TestMethod> methods;
                if (!_tagsToMethods.TryGetValue(tag, out methods))
                {
                    methods = new List<TestMethod>();
                    _tagsToMethods[tag] = methods;
                }
                methods.Add(method);
            }
        }

        /// <summary>
        /// Get the test methods that correspond to a tag expression.
        /// </summary>
        /// <param name="tagExpression">Tag expression.</param>
        /// <returns>Test methods for the tag expression.</returns>
        public IEnumerable<TestMethod> EvaluateExpression(string tagExpression)
        {
            if (tagExpression == null)
            {
                throw new ArgumentNullException("tagExpression");
            }
            else if (tagExpression.Length == 0)
            {
                throw new ArgumentOutOfRangeException("tagExpression");
            }
            return ExpressionEvaluator.Evaluate(this, tagExpression);
        }
    }
}
