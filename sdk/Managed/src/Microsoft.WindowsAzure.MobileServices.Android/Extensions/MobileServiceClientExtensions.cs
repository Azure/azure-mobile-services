using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Content;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Extension methods for UI-based login.
    /// </summary>
    public static class MobileServiceClientExtensions
    {
        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="client" type="Microsoft.WindowsAzure.MobileServices.IMobileServiceClient">
        /// The MobileServiceClient instance to login with
        /// </param>
        /// <param name="context" type="Android.Content.Context">
        /// The Context to display the Login UI in.
        /// </param>
        /// <param name="provider" type="MobileServiceAuthenticationProvider">
        /// Authentication provider to use.
        /// </param>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public static Task<MobileServiceUser> LoginAsync(this IMobileServiceClient client, Context context, MobileServiceAuthenticationProvider provider)
        {
            return LoginAsync(client, context, provider, parameters: null);
        }

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="client" type="Microsoft.WindowsAzure.MobileServices.IMobileServiceClient">
        /// The MobileServiceClient instance to login with
        /// </param>
        /// <param name="context" type="Android.Content.Context">
        /// The Context to display the Login UI in.
        /// </param>
        /// <param name="provider" type="MobileServiceAuthenticationProvider">
        /// Authentication provider to use.
        /// </param>
        /// <param name="parameters">
        /// Provider specific extra parameters that are sent as query string parameters to login endpoint.
        /// </param>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public static Task<MobileServiceUser> LoginAsync(this IMobileServiceClient client, Context context, MobileServiceAuthenticationProvider provider, IDictionary<string, string> parameters)
        {
            return LoginAsync(client, context, provider.ToString(), parameters);
        }

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="client" type="Microsoft.WindowsAzure.MobileServices.IMobileServiceClient">
        /// The MobileServiceClient instance to login with
        /// </param>
        /// <param name="context" type="Android.Content.Context">
        /// The Context to display the Login UI in.
        /// </param>
        /// <param name="provider" type="string">
        /// The name of the authentication provider to use.
        /// </param>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public static Task<MobileServiceUser> LoginAsync(this IMobileServiceClient client, Context context, string provider)
        {
            return LoginAsync(client, context, provider, parameters: null);
        }

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="client" type="Microsoft.WindowsAzure.MobileServices.IMobileServiceClient">
        /// The MobileServiceClient instance to login with
        /// </param>
        /// <param name="context" type="Android.Content.Context">
        /// The Context to display the Login UI in.
        /// </param>
        /// <param name="provider" type="string">
        /// The name of the authentication provider to use.
        /// </param>
        /// <param name="parameters">
        /// Provider specific extra parameters that are sent as query string parameters to login endpoint.
        /// </param>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public static Task<MobileServiceUser> LoginAsync(this IMobileServiceClient client, Context context, string provider, IDictionary<string, string> parameters)
        {
            var auth = new MobileServiceUIAuthentication(context, client, provider, parameters);
            return auth.LoginAsync();
        }

        /// <summary>
        /// Extension method to get a <see cref="Push"/> object made from an existing <see cref="MobileServiceClient"/>.
        /// </summary>
        /// <param name="client">
        /// The <see cref="MobileServiceClient"/> to create with.
        /// </param>
        /// <returns>
        /// The <see cref="Push"/> object used for registering for notifications.
        /// </returns>
        public static Push GetPush(this MobileServiceClient client)
        {
            return new Push(client);
        }
    }
}