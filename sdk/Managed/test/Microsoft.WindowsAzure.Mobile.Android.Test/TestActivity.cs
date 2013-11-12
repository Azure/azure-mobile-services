using Android.App;
using Android.OS;
using Android.Widget;

namespace Microsoft.WindowsAzure.Mobile.Android.Test
{
    [Activity]			
    public class TestActivity : Activity
    {
        protected override void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle);

            SetContentView (Resource.Layout.Test);

            Title = Intent.GetStringExtra ("name");

            FindViewById<TextView> (Resource.Id.Description).Text = Intent.GetStringExtra ("desc");
            FindViewById<TextView> (Resource.Id.Log).Text = Intent.GetStringExtra ("log");
        }
    }
}
