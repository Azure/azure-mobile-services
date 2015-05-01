using System;
using System.Globalization;
using Android.Content;
using System.Threading.Tasks;
using Xamarin.Auth._MobileServices;
using System.Collections.Generic;

namespace Microsoft.WindowsAzure.MobileServices
{
    internal class MobileServiceUIAuthentication : MobileServiceAuthentication
    {
        public MobileServiceUIAuthentication (Context context, IMobileServiceClient client, string providerName, IDictionary<string, string> parameters)
            : base (client, providerName, parameters)
        {
            this.context = context;
        }

        private Context context;

        protected override Task<string> LoginAsyncOverride()
        {
            var tcs = new TaskCompletionSource<string>();

            var auth = new WebRedirectAuthenticator (StartUri, EndUri);
            auth.ShowUIErrors = false;
            auth.ClearCookiesBeforeLogin = false;

            Intent intent = auth.GetUI (this.context);

            auth.Error += (sender, e) =>
            {
                string message = String.Format (CultureInfo.InvariantCulture, "Authentication failed with HTTP response code {0}.", e.Message);
                InvalidOperationException ex = (e.Exception == null)
                    ? new InvalidOperationException (message)
                    : new InvalidOperationException (message, e.Exception);

                tcs.TrySetException (ex);
            };

            auth.Completed += (sender, e) =>
            {
                if (!e.IsAuthenticated)
                    tcs.TrySetException (new InvalidOperationException ("Authentication was cancelled by the user."));
                else
                    tcs.TrySetResult(e.Account.Properties["token"]);
            };

            context.StartActivity (intent);
            
            return tcs.Task;
        }
    }
}