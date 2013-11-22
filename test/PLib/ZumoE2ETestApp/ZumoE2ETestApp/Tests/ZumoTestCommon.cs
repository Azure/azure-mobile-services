// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
#if WINDOWS_PHONE
using System.Windows;
#endif
using ZumoE2ETestApp.Framework;
#if !WINDOWS_PHONE
using ZumoE2ETestApp.UIElements;
#else
using ZumoE2ETestAppWP8.UIElements;
#endif

namespace ZumoE2ETestApp.Tests
{
    internal static class ZumoTestCommon
    {
        public static ZumoTest CreateTestWithSingleAlert(string alert)
        {
#if !WINDOWS_PHONE
            return new ZumoTest("Simple alert", async delegate(ZumoTest test)
            {
                InputDialog dialog = new InputDialog("Information", alert, "OK");
                if (ZumoTestGlobals.ShowAlerts)
                {
                    await dialog.Display();
                }
                return true;
            })
            {
                CanRunUnattended = false
            };
#else
            return new ZumoTest("Alert: " + alert, delegate(ZumoTest test)
            {
                MessageBox.Show(alert);
                TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
                tcs.SetResult(true);
                return tcs.Task;
            })
            {
                CanRunUnattended = false
            };
#endif
        }

        public static ZumoTest CreateYesNoTest(string question, bool expectedAnswer, int delayBeforeDialogMilliseconds = 0)
        {
            string testName = string.Format(CultureInfo.InvariantCulture, "Validation: {0} (expected {1})", question, expectedAnswer ? "Yes" : "No");
            return new ZumoTest(testName, async delegate(ZumoTest test)
            {
                if (delayBeforeDialogMilliseconds > 0)
                {
                    await Util.TaskDelay(delayBeforeDialogMilliseconds);
                }

#if !WINDOWS_PHONE
                InputDialog dialog = new InputDialog("Question", question, "No", "Yes");
                bool answerWasYes = expectedAnswer;

                if (ZumoTestGlobals.ShowAlerts)
                {
                    await dialog.Display();
                    answerWasYes = !dialog.Cancelled;
                }

#else
                bool answerWasYes = await InputDialog.DisplayYesNo(question);
#endif
                if (expectedAnswer != answerWasYes)
                {
                    test.AddLog("Test failed. The answer to <<{0}>> was {1}, it should have been {2}",
                        question, answerWasYes ? "Yes" : "No", expectedAnswer ? "Yes" : "No");
                    return false;
                }
                else
                {
                    return true;
                }
            })
            {
                CanRunUnattended = false
            };
        }

#if WINDOWS_PHONE
        public static ZumoTest CreateInputTest(string title, Dictionary<string, string> propertyBag, string key)
        {
            return new ZumoTest("Input: " + title, async delegate(ZumoTest test)
            {
                string initialText;
                if (System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings.TryGetValue<string>(key, out initialText))
                {
                    // From the isolated storage, takes precedence (used in automation)
                    test.AddLog("Retrieved value from isolated storage: {0}", initialText);
                    propertyBag[key] = initialText;
                    return true;
                }

                propertyBag.TryGetValue(key, out initialText);
                var result = await InputDialog.Display(title, initialText);
                if (result != null)
                {
                    propertyBag[key] = result;
                }

                return true;
            })
            {
                CanRunUnattended = false
            };
        }

#endif

        /// <summary>
        /// Creates a test which doesn't do anything, used only to separate groups of tests
        /// </summary>
        /// <param name="name">The test name.</param>
        /// <returns>A test which always passes without doing anything.</returns>
        public static ZumoTest CreateSeparator(string name)
        {
            return new ZumoTest(name, delegate(ZumoTest test)
            {
                TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
                tcs.SetResult(true);
                return tcs.Task;
            });
        }
    }
}
