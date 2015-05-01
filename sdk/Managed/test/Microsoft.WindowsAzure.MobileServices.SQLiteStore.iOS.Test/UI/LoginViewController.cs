using Microsoft.WindowsAzure.MobileServices;
using MonoTouch.Dialog;
using Foundation;
using UIKit;

namespace Microsoft.WindowsAzure.Mobile.SQLiteStore.iOS.Test
{
    class LoginViewController
        : DialogViewController
    {
        public LoginViewController()
            : base (UITableViewStyle.Grouped, null)
        {
            var defaults = NSUserDefaults.StandardUserDefaults;
            string tags = defaults.StringForKey (TagsKey);

            this.tagsEntry = new EntryElement (null, "Tags", tags);

            Root = new RootElement ("C# Client Library Tests") {
                new Section ("Login") {
                    this.tagsEntry
                },

                new Section {
                    new StringElement ("Run Tests", RunTests)                    
                }                
            };
        }

        private const string TagsKey = "Tags";

        private readonly EntryElement tagsEntry;

        private void RunTests()
        {
            var defaults = NSUserDefaults.StandardUserDefaults;
            defaults.SetString (this.tagsEntry.Value, TagsKey);

            AppDelegate.Harness.Settings.TagExpression = this.tagsEntry.Value;

            if (!string.IsNullOrEmpty(AppDelegate.Harness.Settings.TagExpression))
            {
                AppDelegate.Harness.Settings.TagExpression += " - notXamarin";
            }
            else
            {
                AppDelegate.Harness.Settings.TagExpression = "!notXamarin";
            }

            NavigationController.PushViewController (new HarnessViewController(), true);
        }
    }
}