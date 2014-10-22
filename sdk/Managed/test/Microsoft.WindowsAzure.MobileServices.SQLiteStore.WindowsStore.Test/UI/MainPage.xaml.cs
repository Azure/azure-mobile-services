// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Microsoft.WindowsAzure.MobileServices.SQLiteStore.Test
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
            App.Harness.Settings.Custom["MobileServiceRuntimeUrl"] = String.Empty;
            App.Harness.Settings.Custom["MobileServiceRuntimeKey"] = String.Empty;
            App.Harness.Settings.TagExpression = txtTags.Text;

            ApplicationData.Current.LocalSettings.Values["MobileServiceTags"] = txtTags.Text;

            Frame.Navigate(typeof(TestPage));
        }
    }
}
