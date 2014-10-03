// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

            string mobileServiceRuntimeURL = Settings.Default.MobileServiceRuntimeUrl;
            string mobileServiceRuntimeKey = Settings.Default.MobileServiceRuntimeKey;
            string tags = Settings.Default.MobileServiceTags;

            txtRuntimeUri.Text = mobileServiceRuntimeURL ?? "";
            txtRuntimeKey.Text = mobileServiceRuntimeKey ?? "";
            txtTags.Text = tags ?? "";

            Loaded += (s, e) =>
            {
                btnUnitTests.Focus();
                string[] args = Environment.GetCommandLineArgs();
                if (args.Length >= 2 && args[1] == "/auto")
                {
                    if (args.Length > 2)
                    {
                        mobileServiceRuntimeURL = args[2];
                    }
                    if (args.Length > 3)
                    {
                        mobileServiceRuntimeKey = args[3];
                    }
                    if (args.Length > 4)
                    {
                        tags = args[4];
                    }
                    ExecuteUnitTests(mobileServiceRuntimeURL, mobileServiceRuntimeKey, tags, auto: true);
                }

            };
        }

        /// <summary>
        /// Start the unit test run.
        /// </summary>
        /// <param name="sender">Start button.</param>
        /// <param name="e">Event arguments.</param>
        private void ExecuteUnitTests(object sender, RoutedEventArgs e)
        {
            Settings.Default.MobileServiceRuntimeUrl = txtRuntimeUri.Text;
            Settings.Default.MobileServiceRuntimeKey = txtRuntimeKey.Text;
            Settings.Default.MobileServiceTags = txtTags.Text;

            ExecuteUnitTests(txtRuntimeUri.Text, txtRuntimeKey.Text, txtTags.Text, auto: false);
        }

        private void ExecuteUnitTests(string runtimeUri, string runtimeKey, string tags, bool auto)
        {
            // Get the test settings from the UI
            App.Harness.Settings.Custom["MobileServiceRuntimeUrl"] = runtimeUri;
            App.Harness.Settings.Custom["MobileServiceRuntimeKey"] = runtimeKey;
            App.Harness.Settings.TagExpression = tags;
            App.Harness.Settings.Custom["Auto"] = auto ? "True": "False";

            if (!string.IsNullOrEmpty(App.Harness.Settings.TagExpression))
            {
                App.Harness.Settings.TagExpression += " - notNetFramework";
            }
            else
            {
                App.Harness.Settings.TagExpression = "!notNetFramework";
            }

            this.NavigationService.Navigate(new TestPage());
        }
    }
}
