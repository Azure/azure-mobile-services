// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Microsoft.Azure.Zumo.Win8.CSharp.Test
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

            // Copy the test settings into the UI
            string url = null;
            App.Harness.Settings.Custom.TryGetValue("MobileServiceRuntimeUrl", out url);
            txtRuntimeUri.Text = url ?? "";
            txtTags.Text = App.Harness.Settings.TagExpression ?? "";

            Loaded += (s, e) => btnStart.Focus(FocusState.Keyboard);
        }

        /// <summary>
        /// Start the test run.
        /// </summary>
        /// <param name="sender">Start button.</param>
        /// <param name="e">Event arguments.</param>
        private void OnClick(object sender, RoutedEventArgs e)
        {
            // Get the test settings from the UI
            App.Harness.Settings.Custom["MobileServiceRuntimeUrl"] = txtRuntimeUri.Text;
            App.Harness.Settings.TagExpression = txtTags.Text;

            Frame.Navigate(typeof(TestPage));
        }
    }
}
