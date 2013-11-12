using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoTouch.UIKit;

namespace Microsoft.WindowsAzure.MobileServices
{
    public static class MobileServiceClientExtensions
    {
        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="viewController" type="MonoTouch.UIKit.UIViewController">
        /// UIViewController used to display modal login UI on iPhone/iPods.
        /// </param>
        /// <param name="provider" type="MobileServiceAuthenticationProvider">
        /// Authentication provider to use.
        /// </param>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public static Task<MobileServiceUser> LoginAsync (this IMobileServiceClient client, UIViewController viewController, string providerName)
        {
            return LoginAsync(client, default(RectangleF), viewController, providerName);
        }

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
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
        public static Task<MobileServiceUser> LoginAsync (this IMobileServiceClient client, RectangleF rectangle, UIView view, string providerName)
        {
            return LoginAsync(client, rectangle, (object)view, providerName);
        }

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="barButtonItem" type="MonoTouch.UIKit.UIBarButtonItem">
        /// UIBarButtonItem used to display a popover from on iPad.
        /// </param>
        /// <param name="provider" type="MobileServiceAuthenticationProvider">
        /// Authentication provider to use.
        /// </param>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public static Task<MobileServiceUser> LoginAsync (this IMobileServiceClient client, UIBarButtonItem barButtonItem, string providerName)
        {
            return LoginAsync(client, default(RectangleF), barButtonItem, providerName);
        }

        internal static Task<MobileServiceUser> LoginAsync (IMobileServiceClient client, RectangleF rect, object view, string providerName)
        {
            var auth = new MobileServiceUIAuthentication(rect, view, client, providerName);
            return auth.LoginAsync();
        }
    }
}