using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Widget;
using Microsoft.WindowsAzure.MobileServices;

namespace Microsoft.WindowsAzure.Mobile.SQLiteStore.Android.Test
{
    [Activity(Label = "Microsoft.WindowsAzure.Mobile.SQLiteStore.Android.Test", MainLauncher = true, Icon = "@drawable/icon")]
    public class LoginActivity : Activity
    {
        private const string TagsKey = "Tags";

        private EditText tagsText;

        protected override void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle);
            SetContentView (Resource.Layout.Login);

            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences (this);
            
            this.tagsText = FindViewById<EditText> (Resource.Id.ServiceTags);
            this.tagsText.Text = prefs.GetString (TagsKey, null);

            FindViewById<Button> (Resource.Id.RunTests).Click += OnClickRunTests;
        }

        private void OnClickRunTests (object sender, EventArgs eventArgs)
        {
            using (ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences (this))
            using (ISharedPreferencesEditor editor = prefs.Edit()) {
                editor.PutString (TagsKey, this.tagsText.Text);

                editor.Commit();
            }

            if (!string.IsNullOrEmpty(App.Harness.Settings.TagExpression))
            {
                App.Harness.Settings.TagExpression += " - notXamarin";
            }
            else
            {
                App.Harness.Settings.TagExpression = "!notXamarin";
            }

            Task.Factory.StartNew (App.Harness.RunAsync);

            Intent intent = new Intent (this, typeof (HarnessActivity));
            StartActivity (intent);
        }
    }
}