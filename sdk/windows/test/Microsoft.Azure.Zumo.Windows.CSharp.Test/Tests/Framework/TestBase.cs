// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.Azure.Zumo.Win8.Test;

namespace Microsoft.Azure.Zumo.Win8.CSharp.Test
{
    /// <summary>
    /// Base class for test classes.
    /// </summary>
    public class TestBase
    {
        public void Log(string message, params object[] args)
        {
            if (args != null && args.Length > 0)
            {
                message = string.Format(message, args);
            }
            App.Harness.Log(message);
        }

        public static void Throws<T>(Action action)
            where T : Exception
        {
            try
            {
                action();
                Assert.Fail(string.Format("{0} not thrown.", typeof(T).Name));
            }
            catch (T ex)
            {
                App.Harness.Log(
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

        public static async void ThrowsAsync<T>(Func<Task> action)
            where T : Exception
        {
            try
            {
                await action();
                Assert.Fail(string.Format("{0} not thrown.", typeof(T).Name));
            }
            catch (T ex)
            {
                App.Harness.Log(
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
