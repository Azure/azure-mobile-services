// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Third-party provider authentication control for the Windows Phone platform.
    /// </summary>
    public partial class LoginPage : PhoneApplicationPage
    {
        private string responseData = "";
        private uint responseErrorDetail = 0;
        private PhoneAuthenticationStatus responseStatus = PhoneAuthenticationStatus.UserCancel;

        // We need to keep this state to make sure we do the right thing even during
        // normal phone navigation actions (such as going to start screen and back).
        private bool authenticationStarted = false;
        private bool authenticationFinished = false;

        /// <summary>
        /// The AuthenticationBroker associated with the current Login action.
        /// </summary>
        internal AuthenticationBroker Broker { get; set; }

        /// <summary>
        /// Initiatlizes the page by hooking up some event handlers.
        /// </summary>
        public LoginPage()
        {
            InitializeComponent();

            BackKeyPress += LoginPage_BackKeyPress;
            browserControl.Navigating += BrowserControl_Navigating;
            browserControl.LoadCompleted += BrowserControl_LoadCompleted;
            browserControl.NavigationFailed += BrowserControl_NavigationFailed;
        }

        /// <summary>
        /// Handler for the browser control's load completed event.  We use this to detect when
        /// to hide the progress bar and show the browser control.
        /// </summary>
        void BrowserControl_LoadCompleted(object sender, NavigationEventArgs e)
        {
            HideProgressBar();
#if DEBUG
            // For test automation purposes, we can register some scripts in the app's isolated storage
            // which can "automatically" login in the providers. This way we can have an unattended test run.
            bool testMode;
            string loginScript;
            if (System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings.TryGetValue<bool>("testMode", out testMode) &&
                testMode &&
                System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings.TryGetValue<string>("loginScript", out loginScript) &&
                !string.IsNullOrEmpty(loginScript))
            {
                browserControl.InvokeScript("eval", loginScript);
            }
#endif
        }

        /// <summary>
        /// Initiates the authentication operation by pointing the browser control
        /// to the PhoneWebAuthenticationBroker.StartUri.  If the PhoneWebAuthenticationBroker
        /// isn't currently in the middle of an authentication operation, then we immediately
        /// navigate back.
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Make sure that there is an authentication operation in progress.
            // If not, we'll navigate back to the previous page.
            if (!Broker.AuthenticationInProgress)
            {
                this.NavigationService.GoBack();
            }

            if (!authenticationStarted)
            {
                authenticationStarted = true;
                authenticationFinished = false;

                // Point the browser control to the authentication start page.
                browserControl.Source = Broker.StartUri;
            }
        }

        /// <summary>
        /// Updates the PhoneWebAuthenticationBroker on the state of the authentication
        /// operation.  If we navigated back by pressing the back key, then the operation
        /// will be canceled.  If the browser control successfully completed the operation,
        /// signaled by its navigating to the PhoneWebAuthenticationBroker.EndUri, then we
        /// pass the results on.
        /// </summary>
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            // If there is an active authentication operation in progress and we have
            // finished, then we need to inform the authentication broker of the results.
            // We don't want to stop the operation prematurely, such as when navigating to
            // the start screen.
            if (Broker.AuthenticationInProgress && authenticationFinished)
            {
                authenticationStarted = false;
                authenticationFinished = false;

                Broker.OnAuthenticationFinished(responseData, responseStatus, responseErrorDetail);
            }
        }

        /// <summary>
        /// Handler for the page's back key events.  We use this to determine whether navigations
        /// away from this page are benign (such as going to the start screen) or actually meant
        /// to cancel the operation.
        /// </summary>
        void LoginPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ShowProgressBar();

            responseData = "";
            responseStatus = PhoneAuthenticationStatus.UserCancel;

            authenticationFinished = true;
        }

        /// <summary>
        /// Handler for the browser control's navigating event.  We use this to detect when login
        /// has completed.
        /// </summary>
        private void BrowserControl_Navigating(object sender, NavigatingEventArgs e)
        {
            if (e.Uri == Broker.EndUri)
            {
                responseData = e.Uri.ToString();
                responseStatus = PhoneAuthenticationStatus.Success;

                authenticationFinished = true;

                // Navigate back now.
                ShowProgressBar();
                NavigationService.GoBack();
            }
        }

        /// <summary>
        /// Handler for the browser control's navigation failed event.  We use this to detect errors
        /// </summary>
        private void BrowserControl_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            WebBrowserNavigationException navEx = e.Exception as WebBrowserNavigationException;

            if (navEx != null)
            {
                // Pass along the provided error information.
                responseErrorDetail = (uint)navEx.StatusCode;
            }
            else
            {
                // No error information available.
                responseErrorDetail = 0;
            }
            responseStatus = PhoneAuthenticationStatus.ErrorHttp;

            authenticationFinished = true;
            e.Handled = true;

            // Navigate back now.
            ShowProgressBar();
            NavigationService.GoBack();
        }

        /// <summary>
        /// Shows the progress bar and hides the browser control.
        /// </summary>
        private void ShowProgressBar()
        {
            browserControl.Visibility = System.Windows.Visibility.Collapsed;
            progress.Visibility = System.Windows.Visibility.Visible;
        }

        /// <summary>
        /// Hides the progress bar and shows the browser control.
        /// </summary>
        private void HideProgressBar()
        {
            browserControl.Visibility = System.Windows.Visibility.Visible;
            progress.Visibility = System.Windows.Visibility.Collapsed;
        }
    }
}