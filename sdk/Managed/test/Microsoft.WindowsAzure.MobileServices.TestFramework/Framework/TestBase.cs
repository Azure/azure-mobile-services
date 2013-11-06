// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.TestFramework
{
    /// <summary>
    /// Base class for test classes.
    /// </summary>
    public class TestBase
    {
        private TestHarness TestHarness { get; set; }

        internal void SetTestHarness(TestHarness testHarness)
        {
            this.TestHarness = testHarness;
        }

        public void Log(string message, params object[] args)
        {
            if (args != null && args.Length > 0)
            {
                message = string.Format(message, args);
            }

            TestHarness.Log(message);
        }

        public string GetTestSetting(string name)
        {
            string value = null;
            if (this.TestHarness.Settings.Custom.TryGetValue(name, out value))
            {
                return value;
            }

            return null;
        }

        public T Throws<T>(Action action)
            where T : Exception
        {
            try
            {
                action();
                Assert.Fail(string.Format("{0} not thrown.", typeof(T).Name));
            }
            catch (T ex)
            {
               TestHarness.Log(
                        string.Format(
                            "Caught expected error {0}: {1}",
                            ex.GetType().Name,
                            ex.Message.Replace("\n", "   ").Replace("\r", "   ")));
               return ex;
            }
            catch (Exception ex)
            {
                Assert.Fail(string.Format(
                    "Unexpected exception {0} thrown instead of {1}.",
                    ex.GetType().Name,
                    typeof(T).Name));
            }

            return null;
        }

        public async void ThrowsAsync<T>(Func<Task> action)
            where T : Exception
        {
            try
            {
                await action();
                Assert.Fail(string.Format("{0} not thrown.", typeof(T).Name));
            }
            catch (T ex)
            {
                TestHarness.Log(
                    string.Format(
                        "Caught expected error {0}: {1}",
                        ex.GetType().Name,
                        ex.Message.Replace("\n", "   ").Replace("\r", "   ")));
            }
            catch (Exception ex)
            {
                Assert.Fail(string.Format(
                    "Unexpected exception {0} thrown instead of {1}.",
                    ex.GetType().Name,
                    typeof(T).Name));
            }
        }
    }
}
