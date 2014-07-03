using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Widget;
using Microsoft.WindowsAzure.MobileServices;

namespace Microsoft.WindowsAzure.Mobile.Android.Test
{
    [Activity(Label = "Microsoft.WindowsAzure.Mobile.Android.Test", MainLauncher = true, Icon = "@drawable/icon")]
    public class LoginActivity : Activity
    {
        private const string MobileServiceUriKey = "MobileServiceUri";
        private const string MobileServiceKeyKey = "MobileServiceKey";
        private const string TagsKey = "Tags";

        private EditText uriText, keyText, tagsText;

        protected override void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle);
            SetContentView (Resource.Layout.Login);

            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences (this);
            
            this.uriText = FindViewById<EditText> (Resource.Id.ServiceUri);
            this.uriText.Text = prefs.GetString (MobileServiceUriKey, null);

            this.keyText = FindViewById<EditText> (Resource.Id.ServiceKey);
            this.keyText.Text = prefs.GetString (MobileServiceKeyKey, null);

            this.tagsText = FindViewById<EditText> (Resource.Id.ServiceTags);
            this.tagsText.Text = prefs.GetString (TagsKey, null);

            FindViewById<Button> (Resource.Id.RunTests).Click += OnClickRunTests;
            FindViewById<Button>(Resource.Id.Login).Click += OnClickLogin;
        }

        private void OnClickRunTests (object sender, EventArgs eventArgs)
        {
            using (ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences (this))
            using (ISharedPreferencesEditor editor = prefs.Edit()) {
                editor.PutString (MobileServiceUriKey, this.uriText.Text);
                editor.PutString (MobileServiceKeyKey, this.keyText.Text);
                editor.PutString (TagsKey, this.tagsText.Text);

                editor.Commit();
            }

            App.Harness.Settings.Custom["MobileServiceRuntimeUrl"] = this.uriText.Text;
            App.Harness.Settings.Custom["MobileServiceRuntimeKey"] = this.keyText.Text;
            App.Harness.Settings.TagExpression = this.tagsText.Text;

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

        private async void OnClickLogin(object sender, EventArgs eventArgs)
        {
            var client = new MobileServiceClient(this.uriText.Text, this.keyText.Text);
            var user = await client.LoginAsync(this, MobileServiceAuthenticationProvider.MicrosoftAccount);
            System.Diagnostics.Debug.WriteLine(user.UserId);
        }
    }
}