// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Microsoft.WindowsAzure.MobileServices.TestFramework
{
    /// <summary>
    /// Harness for a test framework that allows asynchronous unit testing.
    /// </summary>
    public sealed class TestHarness
    {
        /// <summary>
        /// Initializes a new instance of the TestHarness class.
        /// </summary>
        public TestHarness()
        {
            this.Groups = new List<TestGroup>();
            this.Settings = new TestSettings();            
        }

        /// <summary>
        /// Gets the list of test groups which contain TestMethods to execute.
        /// </summary>
        public IList<TestGroup> Groups { get; private set; }

        /// <summary>
        /// Gets the test settings used for this run.
        /// </summary>
        public TestSettings Settings { get; private set; }

        /// <summary>
        /// Gets or sets the TestReporter used to update the test interface as
        /// the test run progresses.
        /// </summary>
        public ITestReporter Reporter { get; set; }

        /// <summary>
        /// Gets the total number of test methods.
        /// </summary>
        /// <remarks>
        /// This is a simple helper to provide access to the total number of
        /// test methods which the Reporter can display.
        /// </remarks>
        public int Count
        {
            get { return this.Groups.SelectMany(g => g.Methods).Count(); }
        }

        /// <summary>
        /// Gets the number of test failures.
        /// </summary>
        public int Failures { get; set; }

        /// <summary>
        /// Get the number of tests already executed.
        /// </summary>
        public int Progress { get; private set; }

        /// <summary>
        /// Logs a message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Log(string message)
        {
            this.Reporter.Log(message);
        }

        /// <summary>
        /// Load the tests from the given assembly.
        /// </summary>
        /// <param name="testAssembly">The assembly from which to load tests.</param>
        public void LoadTestAssembly(Assembly testAssembly)
        {
            LoadTestAssembly(this, testAssembly);
        }

        /// <summary>
        /// Filter out any test methods basd on the current settings.
        /// </summary>
        private void FilterTests()
        {
            // Create the list of filters (at some point we could make this
            // publicly accessible)
            List<TestFilter> filters = new List<TestFilter>();
            filters.Add(new FunctionalTestFilter(this.Settings));
            filters.Add(new TagTestFilter(this.Settings));

            // Apply any test filters to the set of tests before we begin
            // executing (test filters will change the Excluded property of
            // individual test methods)
            int originalCount = Count;
            foreach (TestFilter filter in filters)
            {
                filter.Filter(this.Groups);
            }
            int filteredCount = Count;
            if (filteredCount != originalCount)
            {
                this.Settings.TestRunStatusMessage += " - Only running " + filteredCount + "/" + originalCount + " tests";
            }
        }

        /// <summary>
        /// Run the unit tests.
        /// </summary>
        public void RunAsync()
        {
            // Ensure there's an interface to display the test results.
            if (this.Reporter == null)
            {
                throw new ArgumentNullException("Reporter");
            }

            // Setup the progress/failure counters
            this.Progress = 0;
            this.Failures = 0;

            // Filter out any test methods based on the current settings
            FilterTests();

            // Write out the test status message which may be modified by the
            // filters
            Reporter.Status(this.Settings.TestRunStatusMessage);

            // Enumerators that track the current group and method to execute
            // (which allows reentrancy into the test loop below to resume at
            // the correct location).
            IEnumerator<TestGroup> groups = this.Groups.OrderBy(g => g.Name).GetEnumerator();
            IEnumerator<TestMethod> methods = null;

            // Keep a reference to the current group so we can pass it to the
            // Reporter's EndGroup (we don't need one for the test method,
            // however, because we close over in the continuation).
            TestGroup currentGroup = null;

            // Setup the UI
            this.Reporter.StartRun(this);
            
            // The primary test loop is a "recursive" closure that will pass
            // itself as the continuation to async tests.
            // 
            // Note: It's really important for performance to note that any
            // calls to testLoop only occur in the tail position.
            Action testLoop = null;
            testLoop =
                () =>
                {
                    if (methods != null && methods.MoveNext())
                    {
                        // If we were in the middle of a test group and there
                        // are more methods to execute, let's move to the next
                        // test method.

                        // Update the progress
                        this.Progress++;
                        Reporter.Progress(this);

                        // Start the test method
                        Reporter.StartTest(methods.Current);
                        if (methods.Current.Excluded)
                        {
                            // Ignore excluded tests and immediately recurse.
                            Reporter.EndTest(methods.Current);
                            testLoop();
                        }
                        else
                        {
                            // Execute the test method
                            methods.Current.Test.Start(
                                new ActionContinuation
                                {
                                    OnSuccess = () =>
                                    {
                                        // Mark the test as passing, update the
                                        // UI, and continue with the next test.
                                        methods.Current.Passed = true;
                                        methods.Current.Test = null;
                                        Reporter.EndTest(methods.Current);
                                        testLoop();
                                    },
                                    OnError = (message) =>
                                    {
                                        // Mark the test as failing, update the
                                        // UI, and continue with the next test.
                                        methods.Current.Passed = false;
                                        methods.Current.Test = null;
                                        methods.Current.ErrorInfo = message;
                                        this.Failures++;
                                        Reporter.Error(message);
                                        Reporter.EndTest(methods.Current);
                                        testLoop();
                                    }
                                });
                        }
                    }
                    else if (groups.MoveNext())
                    {
                        // If we've finished a test group and there are more,
                        // then move to the next one.

                        // Finish the UI for the last group.
                        if (currentGroup != null)
                        {
                            Reporter.EndGroup(currentGroup);
                            currentGroup = null;
                        }

                        // Setup the UI for this next group
                        currentGroup = groups.Current;
                        Reporter.StartGroup(currentGroup);

                        // Get the methods and immediately recurse which will
                        // start executing them.
                        methods = groups.Current.Methods.OrderBy(m => m.Name).GetEnumerator();
                        testLoop();
                    }
                    else
                    {
                        // Otherwise if we've finished the entire test run
                        
                        // Finish the UI for the last group and update the
                        // progress after the very last test method.
                        Reporter.EndGroup(currentGroup);
                        Reporter.Progress(this);

                        // Finish the UI for the test run.
                        Reporter.EndRun(this);
                    }
                };

            // Start running the tests
            testLoop();
        }

        private static void LoadTestAssembly(TestHarness harness, Assembly testAssembly)
        {
            Dictionary<Type, TestGroup> groups = new Dictionary<Type, TestGroup>();
            Dictionary<TestGroup, object> instances = new Dictionary<TestGroup, object>();
            foreach (Type type in testAssembly.GetTypes())
            {
                foreach (MethodInfo method in type.GetMethods())
                {
                    if (method.GetCustomAttributes(true).Where(a => a.GetType() == typeof(TestMethodAttribute) ||
                                                                     a.GetType() == typeof(AsyncTestMethodAttribute))
                                                        .Any())
                    {
                        TestGroup group = null;
                        object instance = null;
                        if (!groups.TryGetValue(type, out group))
                        {
                            group = CreateGroup(type);
                            harness.Groups.Add(group);
                            groups[type] = group;

                            instance = Activator.CreateInstance(type);
                            TestBase testBase = instance as TestBase;
                            if (testBase != null)
                            {
                                testBase.SetTestHarness(harness);
                            }

                            instances[group] = instance;
                        }
                        else
                        {
                            instances.TryGetValue(group, out instance);
                        }

                        TestMethod test = CreateMethod(type, instance, method);
                        group.Methods.Add(test);
                    }
                }
            }
        }

        private static TestGroup CreateGroup(Type type)
        {
            TestGroup group = new TestGroup();
            group.Name = type.Name;
            group.Tags.Add(type.Name);
            group.Tags.Add(type.FullName);
            if (type.GetCustomAttributes(true).Where(a => a.GetType() == typeof(FunctionalTestAttribute)).Any())
            {
                group.Tags.Add("Functional");
            }
            foreach (TagAttribute attr in type.GetCustomAttributes(true).Where(a => a.GetType() == typeof(TagAttribute)))
            {
                group.Tags.Add(attr.Tag);
            }
            return group;
        }

        private static TestMethod CreateMethod(Type type, object instance, MethodInfo method)
        {
            TestMethod test = new TestMethod();
            test.Name = method.Name;

            if (method.GetCustomAttributes(true).Where(a => a.GetType() == typeof(AsyncTestMethodAttribute)).Any())
            {
                test.Test = new AsyncTestMethodAsyncAction(instance, method);
            }
            else
            {
                test.Test = new TestMethodAsyncAction(instance, method);
            }

            ExcludeTestAttribute excluded = (ExcludeTestAttribute)method.GetCustomAttributes(true).Where(a => a.GetType() == typeof(ExcludeTestAttribute)).FirstOrDefault();
            if (excluded != null)
            {
                test.Exclude(excluded.Reason);
            }

            if (method.GetCustomAttributes(true).Where(a => a.GetType() == typeof(FunctionalTestAttribute)).Any())
            {
                test.Tags.Add("Functional");
            }

            test.Tags.Add(type.FullName + "." + method.Name);
            test.Tags.Add(type.Name + "." + method.Name);
            foreach (TagAttribute attr in method.GetCustomAttributes(true).Where(a => a.GetType() == typeof(TagAttribute)))
            {
                test.Tags.Add(attr.Tag);
            }

            return test;
        }
    }
}
