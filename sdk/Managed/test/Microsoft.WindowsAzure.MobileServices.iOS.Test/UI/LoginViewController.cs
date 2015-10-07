using Microsoft.WindowsAzure.MobileServices;
using MonoTouch.Dialog;
using Foundation;
using UIKit;

namespace MicrosoftWindowsAzureMobileiOSTest
{
    class LoginViewController
        : DialogViewController
    {
        public LoginViewController()
            : base (UITableViewStyle.Grouped, null)
        {
            var defaults = NSUserDefaults.StandardUserDefaults;
            string mobileServiceUri = defaults.StringForKey (MobileServiceUriKey);
            string tags = defaults.StringForKey (TagsKey);

            this.uriEntry = new EntryElement (null, "Mobile Service URI", mobileServiceUri);
            this.tagsEntry = new EntryElement (null, "Tags", tags);

            Root = new RootElement ("C# Client Library Tests") {
                new Section ("Login") {
                    this.uriEntry,
                    this.tagsEntry
                },

                new Section {
                    new StringElement ("Run Tests", RunTests)                    
                },

                new Section{
                    new StringElement("Login with Microsoft", () => Login(MobileServiceAuthenticationProvider.MicrosoftAccount)),
                    new StringElement("Login with Facebook", () => Login(MobileServiceAuthenticationProvider.Facebook)),
                    new StringElement("Login with Twitter", () => Login(MobileServiceAuthenticationProvider.Twitter)),
                    new StringElement("Login with Google", () => Login(MobileServiceAuthenticationProvider.Google))
                }
            };
        }

        private const string MobileServiceUriKey = "MobileServiceUri";
        private const string TagsKey = "Tags";

        private readonly EntryElement uriEntry;
        private readonly EntryElement tagsEntry;

        private void RunTests()
        {
            var defaults = NSUserDefaults.StandardUserDefaults;
            defaults.SetString (this.uriEntry.Value, MobileServiceUriKey);
            defaults.SetString (this.tagsEntry.Value, TagsKey);

            AppDelegate.Harness.Settings.Custom["MobileServiceRuntimeUrl"] = this.uriEntry.Value;
            AppDelegate.Harness.Settings.TagExpression = this.tagsEntry.Value;

            if (!string.IsNullOrEmpty(AppDelegate.Harness.Settings.TagExpression))
            {
                AppDelegate.Harness.Settings.TagExpression += " - notXamarin - notXamarin_iOS";
            }
            else
            {
                AppDelegate.Harness.Settings.TagExpression = "!notXamarin - notXamarin_iOS";
            }

            NavigationController.PushViewController (new HarnessViewController(), true);
        }

        private async void Login(MobileServiceAuthenticationProvider provider)
        {
            var client = new MobileServiceClient(this.uriEntry.Value);
            var user = await client.LoginAsync(this, provider);
            var alert = new UIAlertView("Welcome", "Your userId is: " + user.UserId, null, "OK");
            alert.Show();
        }
    }
}