using System;
using System.Collections.Generic;
using System.Globalization;
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
        List<ZumoTestGroup> allTests;
        ZumoTestGroup currentGroup;

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            this.allTests = TestStore.CreateTests();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            List<ListViewForTestGroup> sources = allTests.Select((tg, i) => new ListViewForTestGroup(i + 1, tg)).ToList();
            this.lstTestGroups.ItemsSource = sources;
        }

        private void lstTestGroups_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            int selectedIndex = this.lstTestGroups.SelectedIndex;
            if (selectedIndex >= 0)
            {
                ZumoTestGroup testGroup = allTests[selectedIndex];
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
                    // Saving app info for future runs
                    // TODO: implement saving
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