using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
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
            this.allTests = TestStore.CreateTests();
            foreach (var testGroup in allTests)
            {
                testGroup.TestFinished += testGroup_TestFinished;
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            List<ListViewForTestGroup> sources = allTests.Select((tg, i) => new ListViewForTestGroup(i + 1, tg)).ToList();
            this.lstTestGroups.ItemsSource = sources;
        }

        private void btnMainHelp_Click_1(object sender, RoutedEventArgs e)
        {
            Alert("Error", "Not implemented yet");
        }

        private void btnSaveAppInfo_Click_1(object sender, RoutedEventArgs e)
        {
            Alert("Error", "Not implemented yet");
        }

        private void btnLoadAppInfo_Click_1(object sender, RoutedEventArgs e)
        {
            Alert("Error", "Not implemented yet");
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
                bool clientInitialized = false;
                string error = null;
                try
                {
                    ZumoTestGlobals.Instance.InitializeClient(this.txtAppUrl.Text, this.txtAppKey.Text);
                    clientInitialized = true;
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                }

                if (error != null)
                {
                    await Alert("Error", error);
                }
                else if (clientInitialized)
                {
                    ZumoTestGroup testGroup = allTests[selectedIndex];
                    await testGroup.Run();
                }
            }
            else
            {
                await Alert("Error", "Please select a test group.");
            }
        }

        void testGroup_TestFinished(object sender, ZumoTestEventArgs e)
        {
            // Refresh list view
        }

        private void btnResetTests_Click_1(object sender, RoutedEventArgs e)
        {
            Alert("Error", "Not implemented yet");
        }

        private async void btnSendLogs_Click_1(object sender, RoutedEventArgs e)
        {
            int selectedIndex = this.lstTestGroups.SelectedIndex;
            if (selectedIndex >= 0)
            {
                ZumoTestGroup testGroup = allTests[selectedIndex];
                Popup popup = new Popup();
                List<string> lines = new List<string>();
                foreach (var test in testGroup.AllTests)
                {
                    lines.Add(string.Format("Logs for test {0} (status = {1})", test.Name, test.Status));
                    lines.AddRange(test.GetLogs());
                    lines.Add("-----------------------");
                }

                UploadLogsPage uploadLogsPage = new UploadLogsPage(testGroup.Name, string.Join("\n", lines));
                popup.Child = uploadLogsPage;
                uploadLogsPage.CloseRequested += (snd, ea) =>
                {
                    popup.IsOpen = false;
                };

                popup.IsOpen = true;
            }
            else
            {
                await Alert("Error", "A test group needs to be selected");
            }
        }

        void uploadLogsPage_CloseRequested(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private Task Alert(string title, string text)
        {
            return new Windows.UI.Popups.MessageDialog(text, title).ShowAsync().AsTask();
        }
    }
}
