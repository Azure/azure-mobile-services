// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZumoE2ETestApp.Framework
{
    public class ZumoTestGroup
    {
        public string Name { get; private set; }
        public string HelpText { get; set; }

        public event EventHandler TestGroupStarted;
        public event EventHandler<ZumoTestGroupEventArgs> TestGroupFinished;
        public event EventHandler TestStarted;
        public event EventHandler<ZumoTestEventArgs> TestFinished;

        private bool runningTests;
        private int testsPassed;
        private int testsFailed;
        private List<ZumoTest> tests;

        public IEnumerable<ZumoTest> AllTests
        {
            get { return tests; }
        }

        public ZumoTestGroup(string name)
        {
            this.Name = name;
            this.testsFailed = 0;
            this.testsPassed = 0;
            this.tests = new List<ZumoTest>();
            this.runningTests = false;
        }

        public void AddTest(ZumoTest test)
        {
            this.tests.Add(test);
        }

        public IEnumerable<ZumoTest> GetTests()
        {
            return this.tests.AsEnumerable();
        }

        public void Reset()
        {
            this.testsFailed = 0;
            this.testsPassed = 0;
            this.tests.Clear();
            this.runningTests = false;
        }

        public List<string> GetLogs()
        {
            List<string> result = new List<string>();
            result.Add("Tests for group '" + this.Name + "'");
            result.Add(string.Format(CultureInfo.InvariantCulture, "Passed: {0}; Failed: {1}", this.testsPassed, this.testsFailed));
            result.Add("----------------------------");
            foreach (var test in this.tests)
            {
                result.Add(string.Format(CultureInfo.InvariantCulture, "Logs for test {0} ({1})", test.Name, test.Status));
                result.AddRange(test.GetLogs());
                result.Add("-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-");
            }

            return result;
        }

        public async Task Run()
        {
            if (this.runningTests)
            {
                throw new InvalidOperationException("This group is already running");
            }

            this.runningTests = true;

            if (this.TestGroupStarted != null)
            {
                this.TestGroupStarted(this, new EventArgs());
            }

            foreach (ZumoTest test in this.tests)
            {
                if (this.TestStarted != null)
                {
                    this.TestStarted(test, new EventArgs());
                }

                bool passed = await test.Run();

                if (this.TestFinished != null)
                {
                    this.TestFinished(test, new ZumoTestEventArgs { TestStatus = test.Status });
                }

                if (passed)
                {
                    testsPassed++;
                }
                else
                {
                    testsFailed++;
                }
            }

            if (this.TestGroupFinished != null)
            {
                this.TestGroupFinished(this, new ZumoTestGroupEventArgs
                {
                    TestsFailed = this.testsFailed,
                    TestsPassed = this.testsPassed
                });
            }

            this.runningTests = false;
        }
    }
}
