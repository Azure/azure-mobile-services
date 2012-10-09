using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Azure.Zumo.WindowsPhone8.CSharp.Test.Resources;

using Microsoft.Azure.Zumo.WindowsPhone8.Test;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Microsoft.Azure.Zumo.WindowsPhone8.CSharp.Test
{
    public partial class MainPage : PhoneApplicationPage, ITestReporter
    {
        private ObservableCollection<GroupDescription> _groups;
        private GroupDescription _currentGroup = null;
        private TestDescription _currentTest = null;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Setup the groups data source
            _groups = new ObservableCollection<GroupDescription>();
            cvsTests.Source = _groups;

            // Start a test run
            App.Harness.Reporter = this;
            Task.Factory.StartNew(() => App.Harness.RunAsync());
        }
        public void StartRun(TestHarness harness)
        {
            Dispatcher.BeginInvoke(() =>
            {
                lblCurrentTestNumber.Text = harness.Progress.ToString();
                lblTotalTestNumber.Text = harness.Count.ToString();
                lblFailureNumber.Tag = harness.Failures.ToString() ?? "0";
                //progress.Value = 1;
                //pnlFooter.Visibility = Visibility.Visible;
            });
        }

        public void EndRun(TestHarness harness)
        {
            Dispatcher.BeginInvoke(() =>
            {
                //pnlFooter.Visibility = Visibility.Collapsed;
                if (harness.Failures > 0)
                {
                    lblResults.Text = string.Format(CultureInfo.InvariantCulture, "{0}/{1} tests failed!", harness.Failures, harness.Count);
                    lblResults.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x00, 0x6E));
                }
                else
                {
                    lblResults.Text = string.Format(CultureInfo.InvariantCulture, "{0} tests passed!", harness.Count);
                }
                lblResults.Visibility = Visibility.Visible;
            });
        }

        public void Progress(TestHarness harness)
        {
            Dispatcher.BeginInvoke(() =>
            {
                lblCurrentTestNumber.Text = harness.Progress.ToString();
                lblFailureNumber.Text = " " + (harness.Failures.ToString() ?? "0");
                double value = harness.Progress;
                int count = harness.Count;
                if (count > 0)
                {
                    value = value * 100.0 / (double)count;
                }
                //progress.Value = value;
            });
        }

        public void StartGroup(TestGroup group)
        {
            Dispatcher.BeginInvoke(() =>
            {
                _currentGroup = new GroupDescription { Name = group.Name };
                _groups.Add(_currentGroup);
            });
        }

        public void EndGroup(TestGroup group)
        {
            Dispatcher.BeginInvoke(() =>
            {
                _currentGroup = null;
            });
        }

        public void StartTest(TestMethod test)
        {
            Dispatcher.BeginInvoke(() =>
            {
                _currentTest = new TestDescription { Name = test.Name };
                _currentGroup.Tests.Add(_currentTest);

                Dispatcher.BeginInvoke(() =>
                {
                    //lstTests.ScrollIntoView(_currentTest);
                    tbOutput.Text = tbOutput.Text + '\n' + test.Name;
                });
            });
        }

        public void EndTest(TestMethod method)
        {
            Dispatcher.BeginInvoke(() =>
            {
                if (method.Excluded)
                {
                    _currentTest.Color = Color.FromArgb(0xFF, 0x66, 0x66, 0x66);
                }
                else if (!method.Passed)
                {
                    _currentTest.Color = Color.FromArgb(0xFF, 0xFF, 0x00, 0x6E);
                }
                else
                {
                    _currentTest.Color = Color.FromArgb(0xFF, 0x2A, 0x9E, 0x39);
                }
                _currentTest = null;
            });
        }

        public void Log(string message)
        {
            Dispatcher.BeginInvoke(() =>
            {
                _currentTest.Details.Add(message);
            });
        }

        public void Error(string errorDetails)
        {
            Dispatcher.BeginInvoke(() =>
            {
                _currentTest.Details.Add(errorDetails);
            });
        }

        public void Status(string status)
        {
            Dispatcher.BeginInvoke(() =>
            {
                lblStatus.Text = status;
            });
        }
    }
}