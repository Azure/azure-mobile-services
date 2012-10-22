using Microsoft.Phone.Controls;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// This class mimics the functionality provided by WebAuthenticationStatus  available in Win8.
    /// </summary>
    internal enum PhoneAuthenticationStatus
    {
        Success = 0,

        UserCancel = 1,

        ErrorHttp = 2
    }

    /// <summary>
    /// This class mimics the functionality provided by WebAuthenticationResult available in Win8.
    /// </summary>
    internal sealed class PhoneAuthenticationResponse
    {
        public string ResponseData { get; private set; }

        public PhoneAuthenticationStatus ResponseStatus { get; private set; }

        public uint ResponseErrorDetail { get; private set; }

        public PhoneAuthenticationResponse(string data, PhoneAuthenticationStatus status, uint error)
        {
            ResponseData = data;
            ResponseStatus = status;
            ResponseErrorDetail = error;
        }
    }

    /// <summary>
    /// This class mimics the functionality provided by WebAuthenticationBroker available in Win8.
    /// </summary>
    internal sealed class PhoneWebAuthenticationBroker
    {
        private const string LoginAsyncUriFragment = "login";
        private const string LoginAsyncDoneUriFragment = "login/done";
        private const string LoginTokenMarker = "#token=";

        private static Uri endUri = null;
        private static Popup popup = null;
        private static WebBrowser browserControl = null;
        private static AutoResetEvent popupClosedEvent = new AutoResetEvent(false);

        private static string responseData = "";
        private static uint responseErrorDetail = 0;
        private static PhoneAuthenticationStatus responseStatus = PhoneAuthenticationStatus.UserCancel;

        /// <summary>
        /// Mimics the WebAuthenticationBroker's AuthenticateAsync method.
        /// </summary>
        public static Task<PhoneAuthenticationResponse> AuthenticateAsync(Uri startUri, Uri endUri)
        {
            PhoneApplicationFrame rootVisual = Application.Current.RootVisual as PhoneApplicationFrame;

            if (rootVisual == null)
            {
                throw new InvalidOperationException();
            }

            PhoneWebAuthenticationBroker.endUri = endUri;

            // Intercept back-key presses to use them to cancel the login request and dismiss the popup.
            rootVisual.BackKeyPress += PhoneWebAuthentication_BackKeyPress;

            browserControl = new WebBrowser() { Width = rootVisual.RenderSize.Width, Height = rootVisual.RenderSize.Height };
            browserControl.Navigating += BrowserControl_Navigating;
            browserControl.IsScriptEnabled = true;
            browserControl.Source = startUri;

            popup = new Popup();
            popup.Closed += Popup_Closed;
            popup.Child = browserControl;
            popup.IsOpen = true;

            Task<PhoneAuthenticationResponse> task = Task<PhoneAuthenticationResponse>.Factory.StartNew(() =>
            {
                popupClosedEvent.WaitOne();
                return new PhoneAuthenticationResponse(responseData, responseStatus, responseErrorDetail);
            });

            return task;
        }

        /// <summary>
        /// Handler for the browser control's navigating event.  We use this to detect when login
        /// has completed.
        /// </summary>
        private static void BrowserControl_Navigating(object sender, NavigatingEventArgs e)
        {
            if (e.Uri == endUri)
            {
                responseData = e.Uri.ToString();

                if (e.Uri.Fragment.StartsWith(LoginTokenMarker))
                {
                    responseStatus = PhoneAuthenticationStatus.Success;
                }
                else
                {
                    // TODO: Parse the Uri for the error code and set it in responseErrorDetail.
                    responseStatus = PhoneAuthenticationStatus.ErrorHttp;
                }

                // Close the popup now.
                popup.IsOpen = false;
                browserControl.Source = new Uri("about:blank");
            }
        }

        /// <summary>
        /// Handler for the popup's closed event.  We use this to signal that the authentication async
        /// task should finish.
        /// </summary>
        private static void Popup_Closed(object sender, EventArgs e)
        {
            (Application.Current.RootVisual as PhoneApplicationFrame).BackKeyPress -= PhoneWebAuthentication_BackKeyPress;

            browserControl.Navigating -= BrowserControl_Navigating;
            browserControl = null;

            popup.Closed -= Popup_Closed;
            popup = null;

            popupClosedEvent.Set();
        }

        /// <summary>
        /// Handler for the back-key pressed event.  We use this to cancel the login attempt and close the popup.
        /// </summary>
        private static void PhoneWebAuthentication_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // We use this event to close the login popup as if the user is canceling it.
            popup.IsOpen = false;

            responseData = "";
            responseStatus = PhoneAuthenticationStatus.UserCancel;

            // Since the popup is closed, we don't need to intercept this event anymore.
            (Application.Current.RootVisual as PhoneApplicationFrame).BackKeyPress -= PhoneWebAuthentication_BackKeyPress;

            e.Cancel = true;
        }
    }
}
