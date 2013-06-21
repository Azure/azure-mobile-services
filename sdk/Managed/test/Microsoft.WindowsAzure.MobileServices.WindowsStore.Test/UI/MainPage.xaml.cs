// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    /// <summary>
    /// Test setup UI page.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        /// <summary>
        /// Initializes a new instance of the MainPage class.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();

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
    }
}
