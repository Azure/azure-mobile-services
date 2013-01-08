using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ZumoE2ETestApp.UIElements
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UploadLogsPage : Page
    {
        public event EventHandler CloseRequested;

        private string uploadUrl;
        public string logs;

        public UploadLogsPage(string testGroupName, string logs, string uploadUrl)
        {
            this.InitializeComponent();
            this.lblTitle.Text = "Logs for " + testGroupName;
            var bounds = Window.Current.Bounds;
            this.grdRootPanel.Width = bounds.Width;
            this.grdRootPanel.Height = bounds.Height;

            this.uploadUrl = uploadUrl;
            this.logs = logs;
            this.txtArea.Text = logs;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.uploadUrl))
            {
                using (var client = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Post, this.uploadUrl))
                    {
                        request.Content = new StringContent(this.logs, Encoding.UTF8, "text/plain");
                        using (var response = await client.SendAsync(request))
                        {
                            var body = await response.Content.ReadAsStringAsync();
                            var title = response.IsSuccessStatusCode ? "Upload successful" : "Error uploading logs";
                            var dialog = new MessageDialog(body, title);
                            await dialog.ShowAsync();
                        }
                    }
                }
            }
        }

        private void btnClose_Click_1(object sender, RoutedEventArgs e)
        {
            if (this.CloseRequested != null)
            {
                this.CloseRequested(this, EventArgs.Empty);
            }
        }
    }
}
