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
        public static Task<MobileServiceUser> LoginAsync (this IMobileServiceClient client, UIViewController viewController, MobileServiceAuthenticationProvider provider)
        {
            return LoginAsync (client, default(RectangleF), viewController, provider);
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
        public static Task<MobileServiceUser> LoginAsync (this IMobileServiceClient client, RectangleF rectangle, UIView view, MobileServiceAuthenticationProvider provider)
        {
            return LoginAsync (client, rectangle, (object)view, provider);
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
        public static Task<MobileServiceUser> LoginAsync (this IMobileServiceClient client, UIBarButtonItem barButtonItem, MobileServiceAuthenticationProvider provider)
        {
            return LoginAsync (client, default(RectangleF), barButtonItem, provider);
        }

        internal static Task<MobileServiceUser> LoginAsync (IMobileServiceClient client, RectangleF rect, object view, MobileServiceAuthenticationProvider provider)
        {
            var auth = new MobileServiceUIAuthentication (rect, view, client, provider);
            return auth.LoginAsync();
        }
    }
}