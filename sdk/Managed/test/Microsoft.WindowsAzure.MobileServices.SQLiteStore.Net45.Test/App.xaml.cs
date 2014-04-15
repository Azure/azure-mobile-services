// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore.Test;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore.Test.UnitTests;
using Microsoft.WindowsAzure.MobileServices.Test;
using Microsoft.WindowsAzure.MobileServices.TestFramework;

namespace Microsoft.WindowsAzure.MobileServices.SQLiteStore.Test
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
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
            ConsoleHelper.Attach();
            Harness = new TestHarness();
            Harness.LoadTestAssembly(typeof(SQLiteStoreTests).GetTypeInfo().Assembly);
        }
    }
}
