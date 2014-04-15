// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
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

            string tags = Settings.Default.MobileServiceTags;

            txtTags.Text = tags ?? "";

            Loaded += (s, e) =>
            {
                btnUnitTests.Focus();
                string[] args = Environment.GetCommandLineArgs();
                if (args.Length >= 2 && args[1] == "/auto")
                {
                    if (args.Length > 2)
                    {
                        tags = args[2];
                    }
                    ExecuteUnitTests(tags, auto: true);
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
            // Get the test settings from the UI
            Settings.Default.MobileServiceTags = txtTags.Text;

            ExecuteUnitTests(txtTags.Text, auto: false);
        }

        private void ExecuteUnitTests(string tags, bool auto)
        {
            App.Harness.Settings.TagExpression = tags;
            App.Harness.Settings.Custom["Auto"] = auto ? "True" : "False";

            this.NavigationService.Navigate(new TestPage());
        }
    }
}
