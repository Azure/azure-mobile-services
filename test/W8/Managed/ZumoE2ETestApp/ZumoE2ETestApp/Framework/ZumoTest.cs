using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZumoE2ETestApp.Framework
{
    public delegate Task<bool> TestExecution(ZumoTest test);

    public class ZumoTest
    {
        public string Name { get; private set; }
        public Dictionary<string, object> Data { get; private set; }
        public TestStatus Status { get; private set; }

        private TestExecution execution;
        private List<string> logs;

        public event EventHandler TestStarted;
        public event EventHandler<ZumoTestEventArgs> TestFinished;

        public ZumoTest(string name, TestExecution execution)
        {
            this.Name = name;
            this.Data = new Dictionary<string, object>();
            this.logs = new List<string>();
            this.execution = execution;
            this.Status = TestStatus.NotRun;
        }

        public void AddLog(string text, params object[] args)
        {
            if (args != null && args.Length > 0)
            {
                text = string.Format(CultureInfo.InvariantCulture, text, args);
            }

            this.logs.Add(text);
        }

        public void Reset()
        {
            this.logs.Clear();
            this.Status = TestStatus.NotRun;
        }

        public IEnumerable<string> GetLogs()
        {
            return this.logs;
        }

        public async Task<bool> Run()
        {
            this.Status = TestStatus.Running;
            if (this.TestStarted != null)
            {
                this.TestStarted(this, new EventArgs());
            }

            bool passed;
            try
            {
                passed = await this.execution(this);
                this.Status = passed ? TestStatus.Passed : TestStatus.Failed;
                this.AddLog("Test {0}", this.Status);
            }
            catch (Exception ex)
            {
                this.AddLog("Test failed with exception: {0}", ex);
                passed = false;
                this.Status = TestStatus.Failed;
            }

            if (this.TestFinished != null)
            {
                this.TestFinished(this, new ZumoTestEventArgs { TestStatus = this.Status });
            }

            return passed;
        }
    }
}
