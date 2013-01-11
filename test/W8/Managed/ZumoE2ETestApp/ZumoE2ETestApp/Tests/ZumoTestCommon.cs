using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZumoE2ETestApp.Framework;
using ZumoE2ETestApp.UIElements;

namespace ZumoE2ETestApp.Tests
{
    internal static class ZumoTestCommon
    {
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
    }
}
