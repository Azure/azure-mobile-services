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

        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }

        public event EventHandler TestGroupStarted;
        public event EventHandler<ZumoTestGroupEventArgs> TestGroupFinished;
        public event EventHandler TestStarted;
        public event EventHandler<ZumoTestEventArgs> TestFinished;

        private bool runningTests;
        private int testsPassed;
        private int testsFailed;
        private int testsSkipped;
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
            this.testsSkipped = 0;
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
            this.testsSkipped = 0;
            this.tests.Clear();
            this.runningTests = false;
        }

        public List<string> GetLogs()
        {
            List<string> result = new List<string>();
            result.Add(string.Format("[{0}] Tests for group '{1}'",
                this.StartTime.ToString(ZumoTest.TimestampFormat, CultureInfo.InvariantCulture),
                this.Name));
            if (testsSkipped == 0)
            {
                result.Add(string.Format(CultureInfo.InvariantCulture, "Passed: {0}; Failed: {1}", this.testsPassed, this.testsFailed));
            }
            else
            {
                result.Add(string.Format(CultureInfo.InvariantCulture, "Passed: {0}; Failed: {1}; Skipped: {2}", this.testsPassed, this.testsFailed, this.testsSkipped));
            }

            result.Add("----------------------------");
            foreach (var test in this.tests)
            {
                result.Add(string.Format(CultureInfo.InvariantCulture, "[{0}] Logs for test {1} ({2})",
                    test.StartTime.ToString(ZumoTest.TimestampFormat, CultureInfo.InvariantCulture),
                    test.Name,
                    test.Status));
                result.AddRange(test.GetLogs());
                result.Add(string.Format("[{0}] -*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-",
                    test.EndTime.ToString(ZumoTest.TimestampFormat, CultureInfo.InvariantCulture)));
            }

            result.Add(string.Format("[{0}] ----------------------------",
                this.EndTime.ToString(ZumoTest.TimestampFormat, CultureInfo.InvariantCulture)));

            return result;
        }

        private bool shouldStop = false;

        public async Task Run(bool unattendedOnly = false)
        {
            this.shouldStop = false;

            if (this.runningTests)
            {
                throw new InvalidOperationException("This group is already running");
            }

            this.runningTests = true;

            if (this.TestGroupStarted != null)
            {
                this.TestGroupStarted(this, new EventArgs());
            }

            this.StartTime = DateTime.UtcNow;

            foreach (ZumoTest test in this.tests)
            {
                if (shouldStop)
                {
                    break;
                }

                if (!test.CanRunUnattended && unattendedOnly)
                {
                    continue;
                }

                if (this.TestStarted != null)
                {
                    this.TestStarted(test, new EventArgs());
                }

                await test.Run();

                if (this.TestFinished != null)
                {
                    this.TestFinished(test, new ZumoTestEventArgs { TestStatus = test.Status });
                }

                switch (test.Status)
                {
                    case TestStatus.Failed:
                        testsFailed++;
                        break;
                    case TestStatus.Passed:
                        testsPassed++;
                        break;
                    case TestStatus.Skipped:
                        testsSkipped++;
                        break;
                    default:
                        throw new InvalidOperationException("Status out of range: " + test.Status.ToString());
                }
            }

            this.EndTime = DateTime.UtcNow;

            if (this.TestGroupFinished != null)
            {
                this.TestGroupFinished(this, new ZumoTestGroupEventArgs
                {
                    TestsFailed = this.testsFailed,
                    TestsPassed = this.testsPassed,
                    TestsSkipped = this.testsSkipped,
                });
            }

            this.runningTests = false;
        }

        internal void Stop()
        {
            this.shouldStop = true;
        }
    }
}
