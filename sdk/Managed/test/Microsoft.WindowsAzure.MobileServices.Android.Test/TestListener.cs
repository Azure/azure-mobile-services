using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.WindowsAzure.MobileServices.TestFramework;

namespace Microsoft.WindowsAzure.Mobile.Android.Test
{
    sealed class TestListener
        : ITestReporter, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Status
        {
            get { return this.status; }
            private set
            {
                if (this.status == value)
                    return;

                this.status = value;
                OnPropertyChanged();
            }
        }

        public int Progress
        {
            get { return this.progress; }
            private set
            {
                if (this.progress == value)
                    return;

                this.progress = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<GroupDescription> Groups
        {
            get { return this.groups; }
        }

        void ITestReporter.StartRun (TestHarness harness)
        {
            Progress = 0;
            this.groups.Clear();
        }

        void ITestReporter.Progress (TestHarness harness)
        {
            float value = harness.Progress;
            int count = harness.Count;
            if (count > 0)
                value = value / count;

            this.Progress = (int)(value * 10000);
        }

        void ITestReporter.EndRun (TestHarness harness)
        {
        }

        void ITestReporter.StartGroup (TestGroup group)
        {
            this.currentGroup = new GroupDescription (group);
            this.groups.Add (this.currentGroup);
        }

        void ITestReporter.EndGroup (TestGroup group)
        {
        }

        void ITestReporter.StartTest (TestMethod test)
        {
            this.logBuilder = new StringBuilder();
        }

        void ITestReporter.EndTest (TestMethod test)
        {
            var description = new TestDescription (test, this.logBuilder.ToString());
            this.currentGroup.Tests.Add (description);
        }

        void ITestReporter.Log (string message)
        {
            this.logBuilder.AppendLine (message);
        }

        void ITestReporter.Error (string errorDetails)
        {
            this.logBuilder.AppendLine (errorDetails);
        }

        void ITestReporter.Status (string newStatus)
        {
            Status = newStatus;
        }

        private GroupDescription currentGroup;
        private readonly ObservableCollection<GroupDescription> groups = new ObservableCollection<GroupDescription>();

        private StringBuilder logBuilder;
        private string status;
        private int progress;

        private void OnPropertyChanged ([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
                handler (this, new PropertyChangedEventArgs (propertyName));
        }
    }
}