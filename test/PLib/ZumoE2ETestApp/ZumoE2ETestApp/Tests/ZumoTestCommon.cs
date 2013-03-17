using System.Collections.Generic;
using System.Globalization;
using ZumoE2ETestApp.Framework;
using ZumoE2ETestApp.UIElements;

namespace ZumoE2ETestApp.Tests
{
    internal static class ZumoTestCommon
    {
#if !WINDOWS_PHONE
        public static ZumoTest CreateTestWithSingleAlert(string alert)
        {
            return new ZumoTest("Simple alert", async delegate(ZumoTest test)
            {
                InputDialog dialog = new InputDialog("Information", alert, "OK");
                await dialog.Display();
                return true;
            });
        }

        public static ZumoTest CreateYesNoTest(string question, bool expectedAnswer)
        {
            string testName = string.Format(CultureInfo.InvariantCulture, "Validation: {0} (expected {1})", question, expectedAnswer ? "Yes" : "No");
            return new ZumoTest(testName, async delegate(ZumoTest test)
            {
                InputDialog dialog = new InputDialog("Question", question, "No", "Yes");
                await dialog.Display();
                bool answerWasYes = !dialog.Cancelled;
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
            });
        }
#else
        public static ZumoTest CreateInputTest(string title, Dictionary<string, string> propertyBag, string key)
        {
            return new ZumoTest("Input: " + title, async delegate(ZumoTest test)
            {
                string initialText;
                propertyBag.TryGetValue(key, out initialText);
                var result = await ZumoE2ETestAppWP8.UIElements.InputDialog.Display(title, initialText);
                if (result != null)
                {
                    propertyBag[key] = result;
                }

                return true;
            });
        }

#endif
    }
}
