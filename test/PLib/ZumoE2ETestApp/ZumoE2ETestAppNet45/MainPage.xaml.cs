// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Navigation;
using ZumoE2ETestApp.Framework;
using ZumoE2ETestApp.Tests;
using ZumoE2ETestApp.UIElements;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ZumoE2ETestApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        List<ZumoTestGroup> allTests;

        public MainPage()
        {
            this.InitializeComponent();
            this.allTests = TestStore.CreateTestGroups();

            OnNavigatedTo(null);
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        private async void OnNavigatedTo(NavigationEventArgs e)
        {
            List<ListViewForTestGroup> sources = allTests.Select((tg, i) => new ListViewForTestGroup(i + 1, tg)).ToList();
            this.lstTestGroups.ItemsSource = sources;
            SavedAppInfo savedAppInfo = await AppInfoRepository.Instance.GetSavedAppInfo();
            this.txtUploadLogsUrl.Text = savedAppInfo.LastUploadUrl ?? "";
            if (savedAppInfo.LastService != null && !string.IsNullOrEmpty(savedAppInfo.LastService.AppUrl) && !string.IsNullOrEmpty(savedAppInfo.LastService.AppKey))
            {
                this.txtAppUrl.Text = savedAppInfo.LastService.AppUrl;
                this.txtAppKey.Text = savedAppInfo.LastService.AppKey;
            }
        }

        private void btnMainHelp_Click_1(object sender, RoutedEventArgs e)
        {
            Alert("Error", "Not implemented yet");
        }

        private async void btnSaveAppInfo_Click_1(object sender, RoutedEventArgs e)
        {
            SavedAppInfo appInfo = await AppInfoRepository.Instance.GetSavedAppInfo();
            string appUrl = this.txtAppUrl.Text;
            string appKey = this.txtAppKey.Text;
            if (string.IsNullOrEmpty(appUrl) || string.IsNullOrEmpty(appKey))
            {
                await Alert("Error", "Please enter valid application URL / key");
            }
            else
            {
                if (appInfo.MobileServices.Any(ms => ms.AppKey == appKey && ms.AppUrl == appUrl))
                {
                    await Alert("Information", "Mobile service info already saved");
                }
                else
                {
                    appInfo.MobileServices.Add(new MobileServiceInfo { AppUrl = appUrl, AppKey = appKey });
                    await AppInfoRepository.Instance.SaveAppInfo(appInfo);
                    await Alert("Information", "Mobile service info successfully saved");
                }
            }
        }

        private async void btnLoadAppInfo_Click_1(object sender, RoutedEventArgs e)
        {
            SavedAppInfo savedAppInfo = await AppInfoRepository.Instance.GetSavedAppInfo();
            if (savedAppInfo.MobileServices.Count == 0)
            {
                await Alert("Error", "There are no saved applications.");
            }
            else
            {
                Popup popup = new Popup();
                SaveAppsControl page = new SaveAppsControl(savedAppInfo.MobileServices);
                page.CloseRequested += (snd, ea) =>
                {
                    if (page.ApplicationUrl != null && page.ApplicationKey != null)
                    {
                        this.txtAppUrl.Text = page.ApplicationUrl;
                        this.txtAppKey.Text = page.ApplicationKey;
                    }

                    popup.IsOpen = false;
                };

                popup.Child = page;
                popup.PlacementTarget = Application.Current.MainWindow;
                popup.Placement = PlacementMode.Center;
                popup.IsOpen = true;
            }
        }

        private void lstTestGroups_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            int selectedIndex = this.lstTestGroups.SelectedIndex;
            if (selectedIndex >= 0)
            {
                ZumoTestGroup testGroup = allTests[selectedIndex];
                List<ListViewForTest> sources = testGroup.GetTests().Select((t, i) => new ListViewForTest(i + 1, t)).ToList();
                this.lstTests.ItemsSource = sources;
                this.lblTestGroupTitle.Text = string.Format("{0}. {1}", selectedIndex + 1, testGroup.Name);
            }
            else
            {
                this.lstTests.ItemsSource = null;
            }
        }

        private async void btnRunTests_Click_1(object sender, RoutedEventArgs e)
        {
            int selectedIndex = this.lstTestGroups.SelectedIndex;
            if (selectedIndex >= 0)
            {
                ZumoTestGroup testGroup = allTests[selectedIndex];
                await this.RunTestGroup(testGroup);
                if (!testGroup.Name.StartsWith(TestStore.AllTestsGroupName) || string.IsNullOrEmpty(this.txtUploadLogsUrl.Text))
                {
                    int passed = testGroup.AllTests.Count(t => t.Status == TestStatus.Passed);
                    string message = string.Format(CultureInfo.InvariantCulture, "Passed {0} of {1} tests", passed, testGroup.AllTests.Count());
                    await Util.MessageBox(message, "Test group finished");
                }
                else
                {
                    // Upload logs automatically if running all tests
                    await Util.UploadLogs(this.txtUploadLogsUrl.Text, string.Join("\n", testGroup.GetLogs()), "net45", true);
                }
            }
            else
            {
                await Alert("Error", "Please select a test group.");
            }
        }

        private async Task<bool> InitializeClient()
        {
            var appUrl = this.txtAppUrl.Text;
            var appKey = this.txtAppKey.Text;

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
                await Alert("Error initializing client", error);
                return false;
            }
            else
            {
                // Saving app info for future runs
                var savedAppInfo = await AppInfoRepository.Instance.GetSavedAppInfo();
                if (savedAppInfo.LastService == null || savedAppInfo.LastService.AppUrl != appUrl || savedAppInfo.LastService.AppKey != appKey)
                {
                    if (savedAppInfo.LastService == null)
                    {
                        savedAppInfo.LastService = new MobileServiceInfo();
                    }

                    savedAppInfo.LastService.AppKey = appKey;
                    savedAppInfo.LastService.AppUrl = appUrl;
                    await AppInfoRepository.Instance.SaveAppInfo(savedAppInfo);
                }

                return true;
            }
        }

        private async Task RunTestGroup(ZumoTestGroup testGroup)
        {
            var clientInitialized = await InitializeClient();
            if (!clientInitialized)
            {
                return;
            }

            string error = null;
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
                await Alert("Error", error);
            }
        }

        private void btnResetTests_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                int selectedIndex = this.lstTestGroups.SelectedIndex;
                if (selectedIndex >= 0)
                {
                    foreach (var test in this.allTests[selectedIndex].AllTests)
                    {
                        test.Reset();
                    }
                }
                else
                {
                    Alert("Error", "Please select a group to reset the tests from.");
                }
            }
            catch (Exception ex)
            {
                Alert("Error", string.Format(CultureInfo.InvariantCulture, "Unhandled exception: {0}", ex));
            }
        }

        private async void btnSendLogs_Click_1(object sender, RoutedEventArgs e)
        {
            int selectedIndex = this.lstTestGroups.SelectedIndex;
            if (selectedIndex >= 0)
            {
                // Saves URL in local storage
                SavedAppInfo appInfo = await AppInfoRepository.Instance.GetSavedAppInfo();
                string uploadUrl = this.txtUploadLogsUrl.Text;
                if (appInfo.LastUploadUrl != uploadUrl)
                {
                    appInfo.LastUploadUrl = uploadUrl;
                    await AppInfoRepository.Instance.SaveAppInfo(appInfo);
                }

                ZumoTestGroup testGroup = allTests[selectedIndex];
                List<string> lines = new List<string>();
                foreach (var test in testGroup.AllTests)
                {
                    lines.Add(string.Format("Logs for test {0} (status = {1})", test.Name, test.Status));
                    lines.AddRange(test.GetLogs());
                    lines.Add("-----------------------");
                }

                UploadLogsControl uploadLogsPage = new UploadLogsControl(testGroup.Name, string.Join("\n", lines), uploadUrl);
                await uploadLogsPage.Display();
            }
            else
            {
                await Alert("Error", "A test group needs to be selected");
            }
        }

        private Task Alert(string title, string text)
        {
            MessageBox.Show(text, title);
            return Task.FromResult(true);
        }

        private void lstTests_DoubleTapped_1(object sender, MouseEventArgs e)
        {
            int selectedGroup = this.lstTestGroups.SelectedIndex;
            if (selectedGroup >= 0)
            {
                ZumoTestGroup testGroup = allTests[selectedGroup];
                int selectedTest = this.lstTests.SelectedIndex;
                List<ZumoTest> tests = testGroup.AllTests.ToList();
                if (selectedTest >= 0 && selectedTest < tests.Count)
                {
                    ZumoTest test = tests[selectedTest];
                    List<string> lines = new List<string>();
                    lines.Add(string.Format("Logs for test {0} (status = {1})", test.Name, test.Status));
                    lines.AddRange(test.GetLogs());
                    lines.Add("-----------------------");

                    UploadLogsControl uploadLogsPage = new UploadLogsControl(testGroup.Name, string.Join("\n", lines), null);
                    uploadLogsPage.Display();
                }
            }
        }

        private async void btnRunSelected_Click_1(object sender, RoutedEventArgs e)
        {
            var items = this.lstTests.SelectedItems;
            if (items.Count == 0)
            {
                await Alert("Error", "No tests selected");
            }
            else
            {
                ZumoTestGroup partialGroup = new ZumoTestGroup("Partial test group");
                foreach (ListViewForTest test in items)
                {
                    partialGroup.AddTest(test.Test);
                }

                await this.RunTestGroup(partialGroup);
            }
        }

        private void lstTests_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            var selectedItems = this.lstTests.SelectedItems;
            if (selectedItems.Count > 0)
            {
                this.btnRunSelected.Visibility = Visibility.Visible;
            }
            else
            {
                this.btnRunSelected.Visibility = Visibility.Collapsed;
            }
        }
    }
}
