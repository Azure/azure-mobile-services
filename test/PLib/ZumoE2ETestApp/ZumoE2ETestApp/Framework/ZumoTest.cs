// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

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
            this.runtimeFeatures = new List<string>();
            if (requiredRuntimeFeatures.Length > 0)
            {
                foreach (string requiredRuntimeFeature in requiredRuntimeFeatures)
                {
                    if (requiredRuntimeFeature != null)
                    {
                        this.runtimeFeatures.Add(requiredRuntimeFeature);
                    }
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

        public async Task<string> GetUnsupportedFeaturesAsync()
        {
            List<string> unsupportedFeatures = new List<string>();
            if (runtimeFeatures.Count > 0)
            {
                foreach (var requiredFeature in this.runtimeFeatures)
                {
                    bool isEnabled;
                    if (!(await ZumoTestGlobals.Instance.GetRuntimeFeatures(this)).TryGetValue(requiredFeature, out isEnabled))
                    {
                        // Not in the list.
                        AddLog("Warning: Status of feature '{0}' is not provided by the runtime", requiredFeature);
                    }
                    else if (!isEnabled)
                    {
                        unsupportedFeatures.Add(requiredFeature);
                    }
                }
            }

            return string.Join(",", unsupportedFeatures);
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
                string unsupportedFeatures = await GetUnsupportedFeaturesAsync();
                if (!string.IsNullOrEmpty(unsupportedFeatures))
                {
                    this.AddLog("Test skipped, missing required runtime features [{0}].", unsupportedFeatures);
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
