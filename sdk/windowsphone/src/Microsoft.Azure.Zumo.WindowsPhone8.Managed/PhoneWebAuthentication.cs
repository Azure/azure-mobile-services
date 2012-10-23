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
        private static string responseData = "";
        private static uint responseErrorDetail = 0;
        private static PhoneAuthenticationStatus responseStatus = PhoneAuthenticationStatus.UserCancel;
        private static AutoResetEvent authenticateFinishedEvent = new AutoResetEvent(false);

        static public bool AuthenticationInProgress { get; private set; }
        static public Uri StartUri { get; private set; }
        static public Uri EndUri { get; private set; }

        /// <summary>
        /// Mimics the WebAuthenticationBroker's AuthenticateAsync method.
        /// </summary>
        public static Task<PhoneAuthenticationResponse> AuthenticateAsync(Uri startUri, Uri endUri)
        {
            PhoneApplicationFrame rootFrame = Application.Current.RootVisual as PhoneApplicationFrame;

            if (rootFrame == null)
            {
                throw new InvalidOperationException();
            }

            PhoneWebAuthenticationBroker.StartUri = startUri;
            PhoneWebAuthenticationBroker.EndUri = endUri;
            PhoneWebAuthenticationBroker.AuthenticationInProgress = true;

            // Navigate to the login page.
            rootFrame.Navigate(new Uri("/Microsoft.Azure.Zumo.WindowsPhone8.Managed;component/loginpage.xaml", UriKind.Relative));

            Task<PhoneAuthenticationResponse> task = Task<PhoneAuthenticationResponse>.Factory.StartNew(() =>
            {
                authenticateFinishedEvent.WaitOne();
                return new PhoneAuthenticationResponse(responseData, responseStatus, responseErrorDetail);
            });

            return task;
        }

        public static void OnAuthenticationFinished(string data, PhoneAuthenticationStatus status, uint error)
        {
            PhoneWebAuthenticationBroker.responseData = data;
            PhoneWebAuthenticationBroker.responseStatus = status;
            PhoneWebAuthenticationBroker.responseErrorDetail = error;

            PhoneWebAuthenticationBroker.AuthenticationInProgress = false;

            // Signal the waiting task that the authentication operation has finished.
            authenticateFinishedEvent.Set();
        }
    }
}
