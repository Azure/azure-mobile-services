using System;
using System.Linq;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace ZumoE2ETestApp
{
    class LoginViewController
        : DialogViewController
    {
        public LoginViewController()
            : base (UITableViewStyle.Grouped, null)
        {
            var defaults = NSUserDefaults.StandardUserDefaults;
            string mobileServiceUri = defaults.StringForKey (MobileServiceUriKey);
            string mobileServiceKey = defaults.StringForKey (MobileServiceKeyKey);            

            this.uriEntry = new EntryElement (null, "Mobile Service URI", mobileServiceUri);
            this.keyEntry = new EntryElement (null, "Mobile Service Key", mobileServiceKey);            

            Root = new RootElement ("Xamarin.iOS E2E Tests") {
                new Section ("Login") {
                    this.uriEntry,
                    this.keyEntry                    
                },

                new Section {
                    new StringElement ("Run Tests", RunTests)
                }
            };
        }

        private const string MobileServiceUriKey = "MobileServiceUri";
        private const string MobileServiceKeyKey = "MobileServiceKey";       

        private readonly EntryElement uriEntry;
        private readonly EntryElement keyEntry;        

        private void RunTests()
        {
            var defaults = NSUserDefaults.StandardUserDefaults;
            defaults.SetString (this.uriEntry.Value, MobileServiceUriKey);
            defaults.SetString (this.keyEntry.Value, MobileServiceKeyKey);            

            //AppDelegate.Harness.Settings.Custom["MobileServiceRuntimeUrl"] = this.uriEntry.Value;
            //AppDelegate.Harness.Settings.Custom["MobileServiceRuntimeKey"] = this.keyEntry.Value;
            //AppDelegate.Harness.Settings.TagExpression = this.tagsEntry.Value;

            //if (!string.IsNullOrEmpty(AppDelegate.Harness.Settings.TagExpression))
            //{
            //    AppDelegate.Harness.Settings.TagExpression += " - notXamarin";
            //}
            //else
            //{
            //    AppDelegate.Harness.Settings.TagExpression = "!notXamarin";
            //}

            //NavigationController.PushViewController (new HarnessViewController(), true);
        }
    }
}