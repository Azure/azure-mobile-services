using Microsoft.WindowsAzure.MobileServices.Test;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            string mobileServiceRuntimeURL = ApplicationData.Current.LocalSettings.Values["MobileServiceRuntimeUrl"] as string;
            string mobileServiceRuntimeKey = ApplicationData.Current.LocalSettings.Values["MobileServiceRuntimeKey"] as string;
            string tags = ApplicationData.Current.LocalSettings.Values["MobileServiceTags"] as string;

            txtRuntimeUri.Text = mobileServiceRuntimeURL ?? "";
            txtRuntimeKey.Text = mobileServiceRuntimeKey ?? "";
            txtTags.Text = tags ?? "";

            Loaded += (s, e) => btnUnitTests.Focus(FocusState.Keyboard);
        }

        /// <summary>
        /// Start the unit test run.
        /// </summary>
        /// <param name="sender">Start button.</param>
        /// <param name="e">Event arguments.</param>
        private void ExecuteUnitTests(object sender, RoutedEventArgs e)
        {
            // Get the test settings from the UI
            App.Harness.Settings.Custom["MobileServiceRuntimeUrl"] = txtRuntimeUri.Text;
            App.Harness.Settings.Custom["MobileServiceRuntimeKey"] = txtRuntimeKey.Text;
            App.Harness.Settings.TagExpression = txtTags.Text;

            ApplicationData.Current.LocalSettings.Values["MobileServiceRuntimeUrl"] = txtRuntimeUri.Text;
            ApplicationData.Current.LocalSettings.Values["MobileServiceRuntimeKey"] = txtRuntimeKey.Text;
            ApplicationData.Current.LocalSettings.Values["MobileServiceTags"] = txtTags.Text;

            if (!string.IsNullOrEmpty(App.Harness.Settings.TagExpression))
            {
                App.Harness.Settings.TagExpression += " - notWP81";
            }
            else
            {
                App.Harness.Settings.TagExpression = "!notWP81";
            }

            Frame.Navigate(typeof(TestPage));
        }

        /// <summary>
        /// Loads the login test page.
        /// </summary>
        /// <param name="sender">Login button.</param>
        /// <param name="e">Event arguments.</param>
        private void ExecuteLoginTests(object sender, RoutedEventArgs e)
        {
            App.Harness.Settings.Custom["MobileServiceRuntimeUrl"] = txtRuntimeUri.Text;
            ApplicationData.Current.LocalSettings.Values["MobileServiceRuntimeUrl"] = txtRuntimeUri.Text;

            Frame.Navigate(typeof(LoginPage));
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }
    }
}
