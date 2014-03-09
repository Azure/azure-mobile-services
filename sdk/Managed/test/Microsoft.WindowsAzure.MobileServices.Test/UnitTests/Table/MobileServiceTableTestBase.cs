using Microsoft.WindowsAzure.MobileServices.TestFramework;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    public class MobileServiceTableTestBase : TestBase
    {

        #region Test Helpers

        /// <summary>
        /// Gets the object typically used as the "response" object, with a String payload of "Hey".
        /// </summary>
        /// <returns>A JObject to test against.</returns>
        /// <remarks>
        /// RWM: If the "Hey", "what?" payloads are attempting to mimic a conversation, whomever wrote these tests wrote them backwards.
        /// </remarks>
        internal JObject GetHeyObject()
        {
            var response = new JObject();
            response["String"] = "Hey";
            return response;
        }

        /// <summary>
        /// Gets the object typically used as the "response" object, with a String payload of "Hey".
        /// </summary>
        /// <param name="testId">The ID to test, in the form of a <see cref="string" />.</param>
        /// <returns>A JObject to test against.</returns>
        /// <remarks>
        /// RWM: If the "Hey", "what?" payloads are attempting to mimic a conversation, whomever wrote these tests wrote them backwards.
        /// </remarks>
        internal JObject GetHeyObject(string testId)
        {
            var response = new JObject();
            response["id"] = GetEscapedTestId(testId);
            response["String"] = "Hey";
            return response;
        }

        /// <summary>
        /// Gets the object typically used as the "response" object, with a String payload of "Hey".
        /// </summary>
        /// <param name="testId">The ID to test, in the form of a <see cref="long" />.</param>
        /// <returns>A JObject to test against.</returns>
        /// <remarks>
        /// RWM: If the "Hey", "what?" payloads are attempting to mimic a conversation, whomever wrote these tests wrote them backwards.
        /// </remarks>
        internal JObject GetHeyObject(long testId)
        {
            var response = new JObject();
            response["id"] = testId;
            response["String"] = "Hey";
            return response;
        }

        /// <summary>
        /// Gets the object array typically used as the "response" object, with a String payload of "Hey".
        /// </summary>
        /// <returns>A JObject to test against.</returns>
        /// <remarks>
        /// RWM: If the "Hey", "what?" payloads are attempting to mimic a conversation, whomever wrote these tests wrote them backwards.
        /// </remarks>
        internal JArray GetHeyObjectArray()
        {
            return new JArray { GetHeyObject() };
        }

        /// <summary>
        /// Gets the object array typically used as the "response" object, with a String payload of "Hey".
        /// </summary>
        /// <param name="testId">The ID to test, in the form of a <see cref="string" />.</param>
        /// <returns>A JObject to test against.</returns>
        /// <remarks>
        /// RWM: If the "Hey", "what?" payloads are attempting to mimic a conversation, whomever wrote these tests wrote them backwards.
        /// </remarks>
        internal JArray GetHeyObjectArray(string testId)
        {
            return new JArray { GetHeyObject(testId) };
        }

        /// <summary>
        /// Gets the object array typically used as the "response" object, with a String payload of "Hey".
        /// </summary>
        /// <param name="testId">The ID to test, in the form of a <see cref="long" />.</param>
        /// <returns>A JObject to test against.</returns>
        /// <remarks>
        /// RWM: If the "Hey", "what?" payloads are attempting to mimic a conversation, whomever wrote these tests wrote them backwards.
        /// </remarks>
        internal JArray GetHeyObjectArray(long testId)
        {
            return new JArray { GetHeyObject(testId) };
        }

        /// <summary>
        /// Gets the object typically used as the "request" object, with a string payload of "what?".
        /// </summary>
        /// <param name="testId">The ID to test, in the form of a string.</param>
        /// <returns>A JObject to test against.</returns>
        /// <remarks>
        /// RWM: If the "Hey", "what?" payloads are attempting to mimic a conversation, whomever wrote these tests wrote them backwards.
        /// </remarks>
        internal JObject GetWhatObject(string testId)
        {
            var response = new JObject();
            response["id"] = GetEscapedTestId(testId);
            response["String"] = "what?";
            return response;
        }

        /// <summary>
        /// Properly escapes a string ID if it is not null.
        /// </summary>
        /// <param name="testId">The string ID to test.</param>
        /// <returns>Either a null string, or the string with backslashes properly escaped.</returns>
        internal string GetEscapedTestId(string testId)
        {
            return testId == null ? testId : testId.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        #endregion

    }
}
