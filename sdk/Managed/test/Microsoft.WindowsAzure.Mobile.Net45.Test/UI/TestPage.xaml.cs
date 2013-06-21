// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.WindowsAzure.MobileServices.TestFramework;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    /// <summary>
    /// Test harness results page.
    /// </summary>
    public sealed partial class TestPage : Page, ITestReporter
    {
        private ObservableCollection<GroupDescription> _groups;
        private ObservableCollection<TestDescription> _tests;
        private GroupDescription _currentGroup = null;
        private TestDescription _currentTest = null;

        /// <summary>
        /// Initializes a new instance of the TestPage.
        /// </summary>
        public TestPage()
        {
            InitializeComponent();

            // Setup the groups data source
            _groups = new ObservableCollection<GroupDescription>();
            _tests = new ObservableCollection<TestDescription>();
            CollectionViewSource src = (CollectionViewSource)this.Resources["cvsTests"];
            src.Source = _tests;

            // Start a test run
            App.Harness.Reporter = this;
            Task.Factory.StartNew(() => App.Harness.RunAsync());
        }

        public async void StartRun(TestHarness harness)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                lblCurrentTestNumber.Text = harness.Progress.ToString();
                lblTotalTestNumber.Text = harness.Count.ToString();
                lblFailureNumber.Tag = harness.Failures.ToString() ?? "0";
                progress.Value = 1;
                pnlFooter.Visibility = Visibility.Visible;
            });
        }

        public async void EndRun(TestHarness harness)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                pnlFooter.Visibility = Visibility.Collapsed;
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

        public async void Progress(TestHarness harness)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                lblCurrentTestNumber.Text = harness.Progress.ToString();
                lblFailureNumber.Text = " " + (harness.Failures.ToString() ?? "0");
                double value = harness.Progress;
                int count = harness.Count;
                if (count > 0)
                {
                    value = value * 100.0 / (double)count;
                }
                progress.Value = value;
            });
        }

        public async void StartGroup(TestGroup group)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                _currentGroup = new GroupDescription { Name = group.Name };
                _groups.Add(_currentGroup);
            });
        }

        public async void EndGroup(TestGroup group)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                _currentGroup = null;
            });
        }

        public async void StartTest(TestMethod test)
        {
            await Dispatcher.InvokeAsync(async () =>
            {
                _currentTest = new TestDescription { Name = test.Name };
                _currentGroup.Add(_currentTest);
                _tests.Add(_currentTest);

                await Dispatcher.InvokeAsync(() =>
                {
                    lstTests.ScrollIntoView(_currentTest);
                });
            });
        }

        public async void EndTest(TestMethod method)
        {
            await Dispatcher.InvokeAsync(() =>
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

        public async void Log(string message)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                _currentTest.Details.Add(message);
            });
        }

        public async void Error(string errorDetails)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                _currentTest.Details.Add(errorDetails);
            });
        }

        public async void Status(string status)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                lblStatus.Text = status;
            });
        }
    }
}
