// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// This class mimics the functionality provided by WebAuthenticationStatus available in Win8.
    /// </summary>
    internal enum PhoneAuthenticationStatus
    {
        Success = 0,

        UserCancel = 1,

        ErrorHttp = 2
    }

    /// <summary>
    /// An AuthenticationBroker for the Windows Phone Platform 
    /// that is like the Windows Store WebAuthenticationBroker 
    /// APIs.
    /// </summary>
    internal class AuthenticationBroker : IDisposable
    {
        public Uri LoginPageUri { get; set; }

        /// <summary>
        /// Indicates if authentication is currently in progress or not.
        /// </summary>
        public bool AuthenticationInProgress { get; private set; }

        /// <summary>
        /// The URL that the <see cref="AuthenticationBroker"/> started at
        /// to begin the authentication flow. 
        /// </summary>
        public Uri StartUri { get; private set; }

        /// <summary>
        /// The URL that the <see cref="AuthenticationBroker"/> will use to
        /// determine if the authentication flow has completed or not.
        /// </summary>
        public Uri EndUri { get; private set; }

        private string responseData = "";
        private uint responseErrorDetail = 0;
        private PhoneAuthenticationStatus responseStatus = PhoneAuthenticationStatus.UserCancel;
        private AutoResetEvent authenticateFinishedEvent = new AutoResetEvent(false);

        /// <summary>
        /// Instantiates a new <see cref="AuthenticationBroker"/>.
        /// </summary>
        public AuthenticationBroker()
        {
            this.LoginPageUri = new Uri("/Microsoft.WindowsAzure.Mobile.UI;component/loginpage.xaml", UriKind.Relative);
        }

        /// <summary>
        /// Begins a server-side authentication flow by navigating the WebAuthenticationBroker
        /// to the <paramref name="startUrl"/>.
        /// </summary>
        /// <param name="startUrl">The URL that the browser-based control should 
        /// first navigate to in order to start the authenication flow.
        /// </param>
        /// <param name="endUrl">The URL that indicates the authentication flow has 
        /// completed. Upon being redirected to any URL that starts with the 
        /// <paramref name="endUrl"/>, the browser-based control must stop navigating and
        /// return the response data to the <see cref="AuthenticationBroker"/>.
        /// </param>
        /// <param name="useSingleSignOn">Indicates if single sign-on should be used so 
        /// that users do not have to re-enter his/her credentials every time.
        /// </param>
        /// <returns>
        /// The response data from the authentication flow that contains a string of JSON 
        /// that represents a Mobile Services authentication token.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the user cancels the authentication flow or an error occurs during
        /// the authentication flow.
        /// </exception>
        public Task<string> AuthenticateAsync(Uri startUrl, Uri endUrl, bool useSingleSignOn)
        {
            PhoneApplicationFrame rootFrame = Application.Current.RootVisual as PhoneApplicationFrame;

            if (rootFrame == null)
            {
                throw new InvalidOperationException();
            }

            this.StartUri = startUrl;
            this.EndUri = endUrl;
            this.AuthenticationInProgress = true;

            //hook up the broker to the page on the event.
            rootFrame.Navigated += rootFrame_Navigated;

            // Navigate to the login page.
            rootFrame.Navigate(this.LoginPageUri);

            Task<string> task = Task<string>.Factory.StartNew(() =>
            {
                authenticateFinishedEvent.WaitOne();
                if (this.responseStatus != PhoneAuthenticationStatus.Success)
                {
                    string message;
                    if (this.responseStatus == PhoneAuthenticationStatus.UserCancel)
                    {
                        message = Resources.IAuthenticationBroker_AuthenticationCanceled;
                        throw new InvalidOperationException(message);
                    }
                    else
                    {
                        message = string.Format(CultureInfo.InvariantCulture,
                                                Resources.IAuthenticationBroker_AuthenticationFailed,
                                                this.responseErrorDetail);
                    }

                    throw new InvalidOperationException(message);
                }

                return GetTokenStringFromResponseData(this.responseData);
            });

            return task;
        }

        /// <summary>
        /// Hooks up the broker to the page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rootFrame_Navigated(object sender, NavigationEventArgs e)
        {
            PhoneApplicationFrame rootFrame = Application.Current.RootVisual as PhoneApplicationFrame;
            rootFrame.Navigated -= rootFrame_Navigated;

            LoginPage page = e.Content as LoginPage;
            page.Broker = this;
        }

        internal void OnAuthenticationFinished(string data, PhoneAuthenticationStatus status, uint error)
        {
            this.responseData = data;
            this.responseStatus = status;
            this.responseErrorDetail = error;

            this.AuthenticationInProgress = false;

            // Signal the waiting task that the authentication operation has finished.
            authenticateFinishedEvent.Set();
        }


        /// <summary>
        /// Gets the JSON string that represents the Mobile Service authentication token
        /// from the result of the authentication attempt.
        /// </summary>
        /// <param name="responseData">
        /// The response data returned from the WebAuthenticationBroker.
        /// </param>
        /// <returns>
        /// A JSON string that represents a Mobile Service authentication token.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the authentication flow resulted in an error message or an invalid response.
        /// </exception>
        private string GetTokenStringFromResponseData(string responseData)
        {
            string tokenString = null;
            if (!string.IsNullOrEmpty(responseData))
            {
                tokenString = GetSubStringAfterMatch(responseData, "#token=");
            }

            if (string.IsNullOrEmpty(tokenString))
            {
                string message = null;
                string errorString = GetSubStringAfterMatch(responseData, "#error=");
                if (string.IsNullOrEmpty(errorString))
                {
                    message = Resources.IAuthenticationBroker_InvalidLoginResponse;
                }
                else
                {
                    message = string.Format(CultureInfo.InvariantCulture,
                                            Resources.IAuthenticationBroker_LoginFailed,
                                            errorString);
                }

                throw new InvalidOperationException(message);
            }

            return tokenString;
        }

        /// <summary>
        /// Returns a substring from the <paramref name="stringToSearch"/> starting from
        /// the first character after the <paramref name="matchString"/> if the 
        /// <paramref name="stringToSearch"/> contains the <paramref name="matchString"/>;
        /// otherwise, returns <c>null</c>.
        /// </summary>
        /// <param name="stringToSearch">The string to search for the <paramref name="matchString"/>.
        /// </param>
        /// <param name="matchString">The string to look for in the <paramref name="stringToSearch"/>
        /// </param>
        /// <returns>The substring from <paramref name="stringToSearch"/> that follows the
        /// <paramref name="matchString"/> if the <paramref name="stringToSearch"/> contains 
        /// the <paramref name="matchString"/>; otherwise, returns <c>null</c>.
        /// </returns>
        private string GetSubStringAfterMatch(string stringToSearch, string matchString)
        {
            Debug.Assert(stringToSearch != null);
            Debug.Assert(matchString != null);

            string value = null;

            int index = stringToSearch.IndexOf(matchString);
            if (index > 0)
            {
                value = Uri.UnescapeDataString(stringToSearch.Substring(index + matchString.Length));
            }

            return value;
        }

        /// <summary>
        /// Implemenation of <see cref="IDisposable"/>
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Implemenation of <see cref="IDisposable"/> for
        /// derived classes to use.
        /// </summary>
        /// <param name="disposing">
        /// Indicates if being called from the Dispose() method
        /// or the finalizer.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                if (this.authenticateFinishedEvent != null)
                {
                    this.authenticateFinishedEvent.Dispose();
                    this.authenticateFinishedEvent = null;
                }
            }
        }
    }
}
