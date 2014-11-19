using System;
using System.Linq;
using Microsoft.WindowsAzure.MobileServices.TestFramework;
using Microsoft.WindowsAzure.MobileServices.Test;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore.Test.UnitTests;
using System.IO;

namespace Microsoft.WindowsAzure.Mobile.SQLiteStore.Android.Test
{
    static class App
    {
        public static readonly TestHarness Harness = new TestHarness();

        static App()
        {
            CurrentPlatform.Init();

            SQLiteStoreTests.TestDbName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "test.db");
            Harness.Reporter = Listener;
            Harness.LoadTestAssembly(typeof(SQLiteStoreTests).Assembly);
        }

        public static readonly TestListener Listener = new TestListener();
    }
} 