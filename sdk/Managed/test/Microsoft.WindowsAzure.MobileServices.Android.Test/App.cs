using System;
using System.Linq;
using Microsoft.WindowsAzure.MobileServices.TestFramework;
using Microsoft.WindowsAzure.MobileServices.Test;
using Microsoft.WindowsAzure.MobileServices;

namespace Microsoft.WindowsAzure.Mobile.Android.Test
{
    static class App
    {
        public static readonly TestHarness Harness = new TestHarness();

        static App()
        {
            CurrentPlatform.Init ();

            Harness.Reporter = Listener;
            Harness.LoadTestAssembly (typeof (MobileServiceSerializerTests).Assembly);
            Harness.LoadTestAssembly(typeof(PushFunctional).Assembly);
        }

        public static readonly TestListener Listener = new TestListener();
    }
} 