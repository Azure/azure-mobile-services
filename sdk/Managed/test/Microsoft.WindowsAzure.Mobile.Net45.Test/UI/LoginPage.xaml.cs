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

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();
        }

        private async void LoginButtonClicked(object sender, RoutedEventArgs e)
        {
            Button buttonClicked = sender as Button;
            if (buttonClicked != null)
            {
                String testName = null;
                MobileServiceAuthenticationProvider provider =
                    MobileServiceAuthenticationProvider.MicrosoftAccount;

                if (buttonClicked.Name.Contains("MicrosoftAccount"))
                {
                    provider = MobileServiceAuthenticationProvider.MicrosoftAccount;
                    testName = "Microsoft Account Login";
                }
                else if (buttonClicked.Name.Contains("Facebook"))
                {
                    provider = MobileServiceAuthenticationProvider.Facebook;
                    testName = "Facebook Login";
                }
                else if (buttonClicked.Name.Contains("Twitter"))
                {
                    provider = MobileServiceAuthenticationProvider.Twitter;
                    testName = "Twitter Login";
                }
                else if (buttonClicked.Name.Contains("Google"))
                {
                    provider = MobileServiceAuthenticationProvider.Google;
                    testName = "Google Login";
                }

                bool useSingleSignOn = UseSingleSignOnCheckBox.IsChecked.Value;

                TestResultsTextBlock.Text = await LoginTests.ExecuteTest(testName, () => LoginTests.TestLoginAsync(provider, useSingleSignOn));
            }
        }
    }
}
