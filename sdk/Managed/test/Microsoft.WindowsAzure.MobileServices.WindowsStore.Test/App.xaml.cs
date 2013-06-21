// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Reflection;
using Microsoft.WindowsAzure.MobileServices.TestFramework;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default
    /// Application class.
    /// </summary>
    public sealed partial class App : Application
    {
        /// <summary>
        /// Gets the test harness used by the application.
        /// </summary>
        public static TestHarness Harness { get; private set; }

        /// <summary>
        /// Initialize the test harness.
        /// </summary>
        static App()
        {
            Harness = new TestHarness();
            Harness.LoadTestAssembly(typeof(MobileServiceSerializerTests).GetTypeInfo().Assembly);
            Harness.LoadTestAssembly(typeof(LoginTests).GetTypeInfo().Assembly);
        }

        /// <summary>
        /// Initializes a new instance of the App class.
        /// </summary>
        public App()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Setup the application and initialize the tests.
        /// </summary>
        /// <param name="args">
        /// Details about the launch request and process.
        /// </param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            // Do not repeat app initialization when already running, just
            // ensure that the window is active
            if (args.PreviousExecutionState == ApplicationExecutionState.Running)
            {
                Window.Current.Activate();
                return;
            }

            Frame rootFrame = new Frame();
            rootFrame.Navigate(typeof(MainPage));
            Window.Current.Content = rootFrame;
            Window.Current.Activate();

        }
    }
}
