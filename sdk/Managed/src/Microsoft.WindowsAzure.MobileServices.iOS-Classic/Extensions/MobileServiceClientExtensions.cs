using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using MonoTouch.UIKit;

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
        /// <param name="viewController" type="MonoTouch.UIKit.UIViewController">
        /// UIViewController used to display modal login UI on iPhone/iPods.
        /// </param>
        /// <param name="provider" type="MobileServiceAuthenticationProvider">
        /// Authentication provider to use.
        /// </param>        
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public static Task<MobileServiceUser> LoginAsync(this IMobileServiceClient client, UIViewController viewController, MobileServiceAuthenticationProvider provider)
        {
            return LoginAsync(client, viewController, provider, parameters: null);
        }

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="client" type="Microsoft.WindowsAzure.MobileServices.IMobileServiceClient">
        /// The MobileServiceClient instance to login with
        /// </param>
        /// <param name="viewController" type="MonoTouch.UIKit.UIViewController">
        /// UIViewController used to display modal login UI on iPhone/iPods.
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
        public static Task<MobileServiceUser> LoginAsync(this IMobileServiceClient client, UIViewController viewController, MobileServiceAuthenticationProvider provider, IDictionary<string, string> parameters)
        {
            return LoginAsync(client, default(RectangleF), viewController, provider.ToString(), parameters);
        }

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="client" type="Microsoft.WindowsAzure.MobileServices.IMobileServiceClient">
        /// The MobileServiceClient instance to login with
        /// </param>
        /// <param name="viewController" type="MonoTouch.UIKit.UIViewController">
        /// UIViewController used to display modal login UI on iPhone/iPods.
        /// </param>
        /// <param name="provider" type="string">
        /// The name of the authentication provider to use.
        /// </param>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public static Task<MobileServiceUser> LoginAsync(this IMobileServiceClient client, UIViewController viewController, string provider)
        {
            return LoginAsync(client, viewController, provider, parameters: null);
        }

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="client" type="Microsoft.WindowsAzure.MobileServices.IMobileServiceClient">
        /// The MobileServiceClient instance to login with
        /// </param>
        /// <param name="viewController" type="MonoTouch.UIKit.UIViewController">
        /// UIViewController used to display modal login UI on iPhone/iPods.
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
        public static Task<MobileServiceUser> LoginAsync(this IMobileServiceClient client, UIViewController viewController, string provider, IDictionary<string, string> parameters)
        {
            return LoginAsync(client, default(RectangleF), viewController, provider, parameters);
        }

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="client" type="Microsoft.WindowsAzure.MobileServices.IMobileServiceClient">
        /// The MobileServiceClient instance to login with
        /// </param>
        /// <param name="rectangle" type="System.Drawing.RectangleF">
        /// The area in <paramref name="view"/> to anchor to.
        /// </param>
        /// <param name="view" type="MonoTouch.UIKit.UIView">
        /// UIView used to display a popover from on iPad.
        /// </param>
        /// <param name="provider" type="MobileServiceAuthenticationProvider">
        /// Authentication provider to use.
        /// </param>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public static Task<MobileServiceUser> LoginAsync(this IMobileServiceClient client, RectangleF rectangle, UIView view, MobileServiceAuthenticationProvider provider)
        {
            return LoginAsync(client, rectangle, view, provider, parameters: null);
        }

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="client" type="Microsoft.WindowsAzure.MobileServices.IMobileServiceClient">
        /// The MobileServiceClient instance to login with
        /// </param>
        /// <param name="rectangle" type="System.Drawing.RectangleF">
        /// The area in <paramref name="view"/> to anchor to.
        /// </param>
        /// <param name="view" type="MonoTouch.UIKit.UIView">
        /// UIView used to display a popover from on iPad.
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
        public static Task<MobileServiceUser> LoginAsync(this IMobileServiceClient client, RectangleF rectangle, UIView view, MobileServiceAuthenticationProvider provider, IDictionary<string, string> parameters)
        {
            return LoginAsync(client, rectangle, (object)view, provider.ToString(), parameters);
        }

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="client" type="Microsoft.WindowsAzure.MobileServices.IMobileServiceClient">
        /// The MobileServiceClient instance to login with
        /// </param>
        /// <param name="rectangle" type="System.Drawing.RectangleF">
        /// The area in <paramref name="view"/> to anchor to.
        /// </param>
        /// <param name="view" type="MonoTouch.UIKit.UIView">
        /// UIView used to display a popover from on iPad.
        /// </param>
        /// <param name="provider" type="string">
        /// The name of the authentication provider to use.
        /// </param>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public static Task<MobileServiceUser> LoginAsync(this IMobileServiceClient client, RectangleF rectangle, UIView view, string provider)
        {
            return LoginAsync(client, rectangle, view, provider, parameters: null);
        }

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="client" type="Microsoft.WindowsAzure.MobileServices.IMobileServiceClient">
        /// The MobileServiceClient instance to login with
        /// </param>
        /// <param name="rectangle" type="System.Drawing.RectangleF">
        /// The area in <paramref name="view"/> to anchor to.
        /// </param>
        /// <param name="view" type="MonoTouch.UIKit.UIView">
        /// UIView used to display a popover from on iPad.
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
        public static Task<MobileServiceUser> LoginAsync(this IMobileServiceClient client, RectangleF rectangle, UIView view, string provider, IDictionary<string, string> parameters)
        {
            return LoginAsync(client, rectangle, (object)view, provider, parameters);
        }

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="client" type="Microsoft.WindowsAzure.MobileServices.IMobileServiceClient">
        /// The MobileServiceClient instance to login with
        /// </param>
        /// <param name="barButtonItem" type="MonoTouch.UIKit.UIBarButtonItem">
        /// UIBarButtonItem used to display a popover from on iPad.
        /// </param>
        /// <param name="provider" type="MobileServiceAuthenticationProvider">
        /// Authentication provider to use.
        /// </param>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public static Task<MobileServiceUser> LoginAsync(this IMobileServiceClient client, UIBarButtonItem barButtonItem, MobileServiceAuthenticationProvider provider)
        {
            return LoginAsync(client, barButtonItem, provider, parameters: null);
        }

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="client" type="Microsoft.WindowsAzure.MobileServices.IMobileServiceClient">
        /// The MobileServiceClient instance to login with
        /// </param>
        /// <param name="barButtonItem" type="MonoTouch.UIKit.UIBarButtonItem">
        /// UIBarButtonItem used to display a popover from on iPad.
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
        public static Task<MobileServiceUser> LoginAsync(this IMobileServiceClient client, UIBarButtonItem barButtonItem, MobileServiceAuthenticationProvider provider, IDictionary<string, string> parameters)
        {
            return LoginAsync(client, default(RectangleF), barButtonItem, provider.ToString(), parameters);
        }

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="client" type="Microsoft.WindowsAzure.MobileServices.IMobileServiceClient">
        /// The MobileServiceClient instance to login with
        /// </param>
        /// <param name="barButtonItem" type="MonoTouch.UIKit.UIBarButtonItem">
        /// UIBarButtonItem used to display a popover from on iPad.
        /// </param>
        /// <param name="provider" type="string">
        /// The name of the authentication provider to use.
        /// </param>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public static Task<MobileServiceUser> LoginAsync(this IMobileServiceClient client, UIBarButtonItem barButtonItem, string provider)
        {
            return LoginAsync(client, barButtonItem, provider, parameters: null);
        }

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="client" type="Microsoft.WindowsAzure.MobileServices.IMobileServiceClient">
        /// The MobileServiceClient instance to login with
        /// </param>
        /// <param name="barButtonItem" type="MonoTouch.UIKit.UIBarButtonItem">
        /// UIBarButtonItem used to display a popover from on iPad.
        /// </param>
        /// <param name="provider" type="string">
        /// The name of the authentication provider to use.
        /// </param>
        /// <param name="parameters">
        /// Provider specific extra parameters that are sent as query string parameters to login endpoint.
        /// </param>
        /// <param name="parameters">
        /// Provider specific extra parameters that are sent as query string parameters to login endpoint.
        /// </param>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public static Task<MobileServiceUser> LoginAsync(this IMobileServiceClient client, UIBarButtonItem barButtonItem, string provider, IDictionary<string, string> parameters)
        {
            return LoginAsync(client, default(RectangleF), barButtonItem, provider, parameters);
        }

        internal static Task<MobileServiceUser> LoginAsync(IMobileServiceClient client, RectangleF rect, object view, string provider, IDictionary<string, string> parameters)
        {
            var auth = new MobileServiceUIAuthentication(rect, view, client, provider, parameters);
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