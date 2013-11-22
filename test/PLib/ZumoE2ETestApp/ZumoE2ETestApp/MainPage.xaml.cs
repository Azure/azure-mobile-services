// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
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
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
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

        private async void btnSaveAppInfo_Click_1(object sender, RoutedEventArgs e)
        {
            SavedAppInfo appInfo = await AppInfoRepository.Instance.GetSavedAppInfo();
            string appUrl = this.txtAppUrl.Text;
            string appKey = this.txtAppKey.Text;
            if (string.IsNullOrEmpty(appUrl) || string.IsNullOrEmpty(appKey))
            {
                await Util.MessageBox("Please enter valid application URL / key", "Error");
            }
            else
            {
                if (appInfo.MobileServices.Any(ms => ms.AppKey == appKey && ms.AppUrl == appUrl))
                {
                    await Util.MessageBox("Mobile service info already saved", "Information");
                }
                else
                {
                    appInfo.MobileServices.Add(new MobileServiceInfo { AppUrl = appUrl, AppKey = appKey });
                    await AppInfoRepository.Instance.SaveAppInfo(appInfo);
                    await Util.MessageBox("Mobile service info successfully saved", "Information");
                }
            }
        }

        private async void btnLoadAppInfo_Click_1(object sender, RoutedEventArgs e)
        {
            SavedAppInfo savedAppInfo = await AppInfoRepository.Instance.GetSavedAppInfo();
            if (savedAppInfo.MobileServices.Count == 0)
            {
                await Util.MessageBox("There are no saved applications.", "Error");
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
                popup.IsOpen = true;
            }
        }

        private async void lstTestGroups_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            int selectedIndex = this.lstTestGroups.SelectedIndex;
            if (selectedIndex >= 0)
            {
                ZumoTestGroup testGroup = allTests[selectedIndex];
                List<ListViewForTest> sources = testGroup.GetTests().Select((t, i) => new ListViewForTest(i + 1, t)).ToList();
                this.lstTests.ItemsSource = sources;
                this.lblTestGroupTitle.Text = string.Format("{0}. {1}", selectedIndex + 1, testGroup.Name);
                if (testGroup.Name.StartsWith(TestStore.AllTestsGroupName) && !string.IsNullOrEmpty(this.txtUploadLogsUrl.Text))
                {
                    await this.RunTestGroup(testGroup);
                    int passed = testGroup.AllTests.Count(t => t.Status == TestStatus.Passed);
                    string message = string.Format(CultureInfo.InvariantCulture, "Passed {0} of {1} tests", passed, testGroup.AllTests.Count());
                    if (passed == testGroup.AllTests.Count())
                    {
                        if (testGroup.Name == TestStore.AllTestsGroupName)
                        {
                            btnRunAllTests.Content = "Passed";
                        }
                        else
                        {
                            btnRunAllUnattendedTests.Content = "Passed";
                        }
                    }

                    if (ZumoTestGlobals.ShowAlerts)
                    {
                        await Util.MessageBox(message, "Test group finished");
                    }

                    ZumoTestGlobals.ShowAlerts = true;
                }
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
                int passed = testGroup.AllTests.Count(t => t.Status == TestStatus.Passed);
                string message = string.Format(CultureInfo.InvariantCulture, "Passed {0} of {1} tests", passed, testGroup.AllTests.Count());
                await Util.MessageBox(message, "Test group finished");
            }
            else
            {
                await Util.MessageBox("Please select a test group.", "Error");
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
                await Util.MessageBox(error, "Error initializing client");
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
                    savedAppInfo.LastUploadUrl = this.txtUploadLogsUrl.Text;
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
                await Util.MessageBox(error, "Error");
            }

            if (testGroup.Name.StartsWith(TestStore.AllTestsGroupName) && !string.IsNullOrEmpty(this.txtUploadLogsUrl.Text))
            {
                // Upload logs automatically if running all tests and write the the logs location to done.txt
                var logsUploadedURL = await Util.UploadLogs(this.txtUploadLogsUrl.Text, string.Join("\n", testGroup.GetLogs()), "winstorecs", true);
                StorageFolder storageFolder = KnownFolders.PicturesLibrary;
                StorageFile logsUploadedFile = await storageFolder.CreateFileAsync(ZumoTestGlobals.LogsLocationFile, CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(logsUploadedFile, logsUploadedURL);
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
                    Util.MessageBox("Please select a group to reset the tests from.", "Error");
                }
            }
            catch (Exception ex)
            {
                Util.MessageBox(string.Format(CultureInfo.InvariantCulture, "Unhandled exception: {0}", ex), "Error");
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
                await Util.MessageBox("A test group needs to be selected", "Error");
            }
        }

        private void lstTests_DoubleTapped_1(object sender, DoubleTappedRoutedEventArgs e)
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
                await Util.MessageBox("No tests selected", "Error");
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
            this.btnRunSelected.IsEnabled = selectedItems.Count > 0;
        }

        private void btnRunAllTests_Click(object sender, RoutedEventArgs e)
        {
            ZumoTestGlobals.ShowAlerts = false;
            for (int i = 0; i < this.allTests.Count; i++)
            {
                if (allTests[i].Name == TestStore.AllTestsGroupName)
                {
                    this.lstTestGroups.SelectedIndex = i;
                    break;
                }
            }
        }

        private void btnRunAllUnattendedTests_Click(object sender, RoutedEventArgs e)
        {
            ZumoTestGlobals.ShowAlerts = false;
            for (int i = 0; i < this.allTests.Count; i++)
            {
                if (allTests[i].Name == TestStore.AllTestsUnattendedGroupName)
                {
                    this.lstTestGroups.SelectedIndex = i;
                    break;
                }
            }
        }
    }
}
