// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZumoE2ETestApp.Tests;

namespace ZumoE2ETestApp.Framework
{
    public delegate Task<bool> TestExecution(ZumoTest test);

    public class TestStatusChangedEventArgs : EventArgs
    {
        public TestStatusChangedEventArgs(TestStatus status)
        {
            this.NewStatus = status;
        }

        public TestStatus NewStatus { get; private set; }
    }

    public class ZumoTest
    {
        public string Name { get; private set; }
        public Dictionary<string, object> Data { get; private set; }
        public TestStatus Status { get; private set; }

        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }

        public bool CanRunUnattended { get; set; }

        internal const string TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff";
        private TestExecution execution;
        private List<string> logs;
        private List<string> runtimeFeatures;

        public event EventHandler<TestStatusChangedEventArgs> TestStatusChanged;

        public ZumoTest(string name, TestExecution execution, params string[] requiredRuntimeFeatures)
        {
            this.Name = name;
            this.Data = new Dictionary<string, object>();
            this.CanRunUnattended = true;
            this.logs = new List<string>();
            this.execution = execution;
            this.Status = TestStatus.NotRun;
            this.runtimeFeatures = ZumoTestGlobals.EnvRuntimeFeatures;
            if (requiredRuntimeFeatures.Length > 0)
            {
                foreach (string requiredRuntimeFeature in requiredRuntimeFeatures)
                {
                    this.runtimeFeatures.Add(requiredRuntimeFeature);
                }
            }
        }

        public void AddLog(string text, params object[] args)
        {
            if (args != null && args.Length > 0)
            {
                text = string.Format(CultureInfo.InvariantCulture, text, args);
            }

            text = string.Format(CultureInfo.InvariantCulture, "[{0}] {1}",
                DateTime.UtcNow.ToString(TimestampFormat, CultureInfo.InvariantCulture),
                text);
            this.logs.Add(text);
        }

        public void Reset()
        {
            this.logs.Clear();
            this.Status = TestStatus.NotRun;
            if (this.TestStatusChanged != null)
            {
                this.TestStatusChanged(this, new TestStatusChangedEventArgs(this.Status));
            }
        }

        public IEnumerable<string> GetLogs()
        {
            return this.logs;
        }

        public bool ShouldBeSkipped()
        {
            if (runtimeFeatures.Contains(ZumoTestGlobals.RuntimeFeatureNames.NET_RUNTIME_ENABLED)
                && runtimeFeatures.Count > 1)
            {
                return true;
            }
            
            if (!runtimeFeatures.Contains(ZumoTestGlobals.RuntimeFeatureNames.NET_RUNTIME_ENABLED)
                && runtimeFeatures.Contains(ZumoTestGlobals.RuntimeFeatureNames.NOTIFICATION_HUB_ENABLED))
            {
                return true;
            }

            return false;
        }

        public string WhySkipped()
        {
            var ret = string.Join(",", runtimeFeatures);
            return ret;
        }

        public async Task Run()
        {
            this.Status = TestStatus.Running;
            if (this.TestStatusChanged != null)
            {
                this.TestStatusChanged(this, new TestStatusChangedEventArgs(this.Status));
            }

            try
            {
                this.StartTime = DateTime.UtcNow;
                if (this.ShouldBeSkipped())
                {
                    this.AddLog("Test skipped, missing required runtime features [{0}].", WhySkipped());
                    this.Status = TestStatus.Skipped;
                }
                else
                {
                    bool passed = await this.execution(this);
                    this.Status = passed ? TestStatus.Passed : TestStatus.Failed;
                    this.AddLog("Test {0}", this.Status);
                }
            }
            catch (Exception ex)
            {
                this.AddLog("Test failed with exception: {0}", ex);
                this.Status = TestStatus.Failed;
            }

            this.EndTime = DateTime.UtcNow;

            if (this.TestStatusChanged != null)
            {
                this.TestStatusChanged(this, new TestStatusChangedEventArgs(this.Status));
            }
        }
    }
}
