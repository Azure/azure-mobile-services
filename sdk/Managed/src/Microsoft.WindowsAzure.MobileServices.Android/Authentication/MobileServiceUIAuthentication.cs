using System;
using System.Globalization;
using Android.Content;
using System.Threading.Tasks;
using Xamarin.Auth;
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
            auth.ClearCookiesBeforeLogin = false;

            Intent intent = auth.GetUI (this.context);

            auth.Error += (sender, e) =>
            {
                string message = String.Format (CultureInfo.InvariantCulture, Resources.IAuthenticationBroker_AuthenticationFailed, e.Message);
                InvalidOperationException ex = (e.Exception == null)
                    ? new InvalidOperationException (message)
                    : new InvalidOperationException (message, e.Exception);

                tcs.TrySetException (ex);
            };

            auth.Completed += (sender, e) =>
            {
                if (!e.IsAuthenticated)
                    tcs.TrySetException (new InvalidOperationException (Resources.IAuthenticationBroker_AuthenticationCanceled));
                else
                    tcs.TrySetResult(e.Account.Properties["token"]);
            };

            context.StartActivity (intent);
            
            return tcs.Task;
        }
    }
}