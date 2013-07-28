// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using ZumoE2ETestApp.Framework;
using ZumoE2ETestApp.Tests;
using ZumoE2ETestApp.UIElements;
using ZumoE2ETestAppWP8.Resources;

namespace ZumoE2ETestAppWP8
{
    public partial class MainPage : PhoneApplicationPage
    {
        List<ZumoTestGroup> allTestGroups;
        ZumoTestGroup currentGroup;

        const string AllTestsGroupName = "All tests";

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            this.allTestGroups = TestStore.CreateTestGroups();
            if (this.appBtnBack == null)
            {
                var appBar = this.ApplicationBar;
                foreach (ApplicationBarIconButton button in appBar.Buttons)
                {
                    switch (button.Text.ToLowerInvariant())
                    {
                        case "test groups":
                            this.appBtnBack = button;
                            break;
                        case "reset":
                            this.appBtnReset = button;
                            break;
                        case "upload logs":
                            this.appBtnUploadLogs = button;
                            break;
                        case "run tests":
                            this.appBtnRunTests = button;
                            break;
                    }
                }
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (this.lstTestGroups.ItemsSource == null)
            {
                List<ListViewForTestGroup> sources = allTestGroups.Select((tg, i) => new ListViewForTestGroup(i + 1, tg)).ToList();
                this.lstTestGroups.ItemsSource = sources;

                SavedAppInfo savedAppInfo = await AppInfoRepository.Instance.GetSavedAppInfo();
                this.txtUploadUrl.Text = savedAppInfo.LastUploadUrl ?? "";
                if (savedAppInfo.LastService != null && !string.IsNullOrEmpty(savedAppInfo.LastService.AppUrl) && !string.IsNullOrEmpty(savedAppInfo.LastService.AppKey))
                {
                    this.txtAppUrl.Text = savedAppInfo.LastService.AppUrl;
                    this.txtAppKey.Text = savedAppInfo.LastService.AppKey;
                }
            }
        }

        private void lstTestGroups_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            int selectedIndex = this.lstTestGroups.SelectedIndex;
            if (selectedIndex >= 0)
            {
                ZumoTestGroup testGroup = allTestGroups[selectedIndex];
                this.currentGroup = testGroup;
                List<ListViewForTest> sources = testGroup.GetTests().Select((t, i) => new ListViewForTest(i + 1, t)).ToList();
                this.lstTests.ItemsSource = sources;
                this.lblTestGroupTitle.Text = string.Format("{0}. {1}", selectedIndex + 1, testGroup.Name);
                SwapPanels(false);
            }
            else
            {
                this.lstTests.ItemsSource = null;
                this.currentGroup = null;
                SwapPanels(true);
            }
        }

        private void SwapPanels(bool showTestGroups)
        {
            this.appBtnBack.IsEnabled = !showTestGroups;
            this.appBtnReset.IsEnabled = !showTestGroups;
            this.pnlGroups.Visibility = showTestGroups ? Visibility.Visible : Visibility.Collapsed;
            this.grdTests.Visibility = showTestGroups ? Visibility.Collapsed : Visibility.Visible;
            if (showTestGroups)
            {
                this.currentGroup = null;
            }
        }

        private async Task SaveAppInfo()
        {
            SavedAppInfo appInfo = await AppInfoRepository.Instance.GetSavedAppInfo();
            string uploadUrl = this.txtUploadUrl.Text;
            string appUrl = this.txtAppUrl.Text;
            string appKey = this.txtAppKey.Text;
            if (appInfo.LastUploadUrl != uploadUrl || appInfo.LastService.AppUrl != appUrl || appInfo.LastService.AppKey != appKey)
            {
                appInfo.LastUploadUrl = uploadUrl;
                appInfo.LastService.AppUrl = appUrl;
                appInfo.LastService.AppKey = appKey;
                await AppInfoRepository.Instance.SaveAppInfo(appInfo);
            }
        }

        private async void appBtnUploadLogs_Click_1(object sender, EventArgs e)
        {
            var uploadUrl = this.txtUploadUrl.Text;
            int selectedIndex = this.lstTestGroups.SelectedIndex;
            if (this.currentGroup == null)
            {
                MessageBox.Show("Please select a test group.", "Error", MessageBoxButton.OK);
                return;
            }

            if (string.IsNullOrEmpty(uploadUrl))
            {
                MessageBox.Show("Please enter an URL for the logs to be uploaded");
                return;
            }

            var logs = string.Join(Environment.NewLine, this.currentGroup.GetLogs());
            uploadUrl = uploadUrl + "?platform=wp8";
            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, uploadUrl))
                {
                    request.Content = new StringContent(logs, Encoding.UTF8, "text/plain");
                    using (var response = await client.SendAsync(request))
                    {
                        var body = await response.Content.ReadAsStringAsync();
                        var title = response.IsSuccessStatusCode ? "Upload successful" : "Error uploading logs";
                        MessageBox.Show(body, title, MessageBoxButton.OK);
                    }
                }
            }
        }

        private async void appBtnRunTests_Click_1(object sender, EventArgs e)
        {
            if (this.currentGroup != null)
            {
                await this.RunTestGroup(this.currentGroup);
            }
            else
            {
                MessageBox.Show("Please select a test group.", "Error", MessageBoxButton.OK);
            }
        }

        private async Task RunTestGroup(ZumoTestGroup testGroup)
        {
            var appUrl = this.txtAppUrl.Text;
            var appKey = this.txtAppKey.Text;

            await SaveAppInfo();

            string error = null;
            try
            {
                ZumoTestGlobals.Instance.InitializeClient(appUrl, appKey);
            }
            catch (Exception ex)
            {
                error = string.Format(CultureInfo.InvariantCulture, "{0}", ex);
            }

            if (error != null)
            {
                MessageBox.Show(error, "Error initializing client", MessageBoxButton.OK);
            }
            else
            {
                try
                {
                    await testGroup.Run();
                }
                catch (Exception ex)
                {
                    error = string.Format(CultureInfo.InvariantCulture, "Unhandled exception: {0}", ex);
                }

                if (error != null)
                {
                    MessageBox.Show(error, "Error", MessageBoxButton.OK);
                }
                else
                {
                    if (testGroup.Name.StartsWith(TestStore.AllTestsGroupName) && !string.IsNullOrEmpty(this.txtUploadUrl.Text))
                    {
                        // Upload logs automatically if running all tests
                        using (var client = new HttpClient())
                        {
                            using (var request = new HttpRequestMessage(HttpMethod.Post, this.txtUploadUrl.Text + "?platform=wp8&allTests=true"))
                            {
                                request.Content = new StringContent(string.Join("\n", testGroup.GetLogs()), Encoding.UTF8, "text/plain");
                                using (var response = await client.SendAsync(request))
                                {
                                    var body = await response.Content.ReadAsStringAsync();
                                    var title = response.IsSuccessStatusCode ? "Upload successful" : "Error uploading logs";
                                    MessageBox.Show(body, title, MessageBoxButton.OK);
                                }
                            }
                        }
                    }
                    else
                    {
                        int passed = this.currentGroup.AllTests.Count(t => t.Status == TestStatus.Passed);
                        string message = string.Format(CultureInfo.InvariantCulture, "Passed {0} of {1} tests", passed, this.currentGroup.AllTests.Count());
                        MessageBox.Show(message, "Test group finished", MessageBoxButton.OK);
                    }
                }
            }
        }

        private void appBtnBack_Click_1(object sender, EventArgs e)
        {
            SwapPanels(true);
        }

        private void appBtnReset_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (this.currentGroup != null)
                {
                    foreach (var test in this.currentGroup.AllTests)
                    {
                        test.Reset();
                    }
                }
                else
                {
                    MessageBox.Show("Please select a group to reset the tests from.", "Error", MessageBoxButton.OK);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(CultureInfo.InvariantCulture, "Unhandled exception: {0}", ex), "Error", MessageBoxButton.OK);
            }
        }
    }
}