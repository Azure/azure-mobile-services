using System;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;

namespace Microsoft.WindowsAzure.MobileServices
{
    public static class MobileServiceClientExtensions
    {
        public static Task<MobileServiceUser> LoginAsync (this IMobileServiceClient client, Context context, MobileServiceAuthenticationProvider provider)
        {
            var auth = new MobileServiceUIAuthentication (context, client, provider);
            return auth.LoginAsync();
        }
    }
}