// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using Windows.ApplicationModel.Activation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Azure.Zumo.Win8.Test;
using Windows.Data.Json;

namespace Microsoft.Azure.Zumo.Win8.CSharp.Test
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

            
            // Load the tests
            TestDiscovery.Populate(Harness);

            if (!string.IsNullOrEmpty(args.Arguments))
            {
                JsonObject settings = JsonValue.Parse(args.Arguments).GetObject();
                if (settings != null)
                {
                    App.Harness.Settings.TestResultsServerUrl = settings.GetNamedString("TestServerUri");
                    App.Harness.Settings.BreakOnStart = settings.GetNamedBoolean("BreakOnStart");
                    App.Harness.Settings.Custom["MobileServiceRuntimeUrl"] = settings.GetNamedString("RuntimeUri");
                    App.Harness.Settings.TagExpression = "";
                }
            }

            Frame rootFrame = new Frame();
            Type firstPageType = App.Harness.Settings.BreakOnStart ?
                typeof(MainPage) :
                typeof(TestPage);
            if (!rootFrame.Navigate(firstPageType))
            {
                throw new Exception("Failed to create initial page");
            }
            Window.Current.Content = rootFrame;
            Window.Current.Activate();

        }
    }
}
