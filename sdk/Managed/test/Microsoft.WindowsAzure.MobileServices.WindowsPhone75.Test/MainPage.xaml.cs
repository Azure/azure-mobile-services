// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.WindowsAzure.MobileServices.TestFramework;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Test
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
            this.Loaded += MainPage_Loaded;
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Setup the groups data source
            _groups = new ObservableCollection<GroupDescription>();
            unitTests.ItemsSource = _groups;

            txtRuntimeUri.Text = ""; // Set the default URI here
            txtRuntimeKey.Text = ""; // Set the default Key here
            txtTags.Text = ""; // Set the default tags here
        }

        private void ExecuteLoginTests(object sender, RoutedEventArgs e)
        {
            // Get the test settings from the UI
            App.Harness.Settings.Custom["MobileServiceRuntimeUrl"] = txtRuntimeUri.Text;
            App.Harness.Settings.Custom["MobileServiceRuntimeKey"] = txtRuntimeKey.Text;

            // Hide Test Settings UI
            testSettings.Visibility = Visibility.Collapsed;

            // Make the Login Test UI visible
            loginTests.Visibility = Visibility.Visible;
        }

        private async void LoginButtonClicked(object sender, RoutedEventArgs e)
        {
            Button buttonClicked = sender as Button;
            if (buttonClicked != null)
            {
                String testName = null;
                MobileServiceAuthenticationProvider provider =
                    MobileServiceAuthenticationProvider.MicrosoftAccount;

                if (buttonClicked.Name.Contains("MicrosoftAccount"))
                {
                    provider = MobileServiceAuthenticationProvider.MicrosoftAccount;
                    testName = "Microsoft Account Login";
                }
                else if (buttonClicked.Name.Contains("Facebook"))
                {
                    provider = MobileServiceAuthenticationProvider.Facebook;
                    testName = "Facebook Login";
                }
                else if (buttonClicked.Name.Contains("Twitter"))
                {
                    provider = MobileServiceAuthenticationProvider.Twitter;
                    testName = "Twitter Login";
                }
                else if (buttonClicked.Name.Contains("Google"))
                {
                    provider = MobileServiceAuthenticationProvider.Google;
                    testName = "Google Login";
                }

                bool useProviderStringOverload = UseProviderStringOverloadCheckBox.IsChecked.Value;

                TestResultsTextBlock.Text = await LoginTests.ExecuteTest(testName, () => LoginTests.TestLoginAsync(provider, useProviderStringOverload));
            }
        }

        private void ExecuteUnitTests(object sender, RoutedEventArgs e)
        {
            // Get the test settings from the UI
            App.Harness.Settings.Custom["MobileServiceRuntimeUrl"] = txtRuntimeUri.Text;
            App.Harness.Settings.Custom["MobileServiceRuntimeKey"] = txtRuntimeKey.Text;
            App.Harness.Settings.TagExpression = txtTags.Text;

            //ignore tests for WP75
            if (!string.IsNullOrEmpty(App.Harness.Settings.TagExpression))
            {
                App.Harness.Settings.TagExpression += " - notWP75";
            }
            else                
            {
                App.Harness.Settings.TagExpression = "!notWP75";
            }

            // Hide Test Settings UI
            testSettings.Visibility = Visibility.Collapsed;

            // Display Status UI
            lblStatus.Visibility = Visibility.Visible;
            unitTests.Visibility = Visibility.Visible;
            unitTestResults.Visibility = Visibility.Visible;

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
                progress.Value = 1;
            });
        }

        public void EndRun(TestHarness harness)
        {
            Dispatcher.BeginInvoke(() =>
            {
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
                progress.Value = value;
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
                TestDescription testDescription = new TestDescription { Name = test.Name };
                _currentTest = testDescription;
                _currentGroup.Add(_currentTest);

                Dispatcher.BeginInvoke(() =>
                {
                    unitTests.ScrollTo(testDescription);
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