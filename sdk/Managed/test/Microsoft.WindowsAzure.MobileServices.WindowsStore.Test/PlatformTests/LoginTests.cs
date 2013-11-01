// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    /// <summary>
    /// Platform-specific Login Tests for the Windows Store platform.
    /// </summary>
    class LoginTests
    {
        private static MobileServiceClient client;

        /// <summary>
        /// Tests the <see cref="MobileServiceClient.LoginAsync"/> functionality on the platform.
        /// </summary>
        /// <param name="provider">The provider with which to login.
        /// </param>
        /// <param name="useSingleSignOn">Indicates if single sign-on should be used when logging in.
        /// </param>
        /// <param name="useProviderStringOverload">Indicates if the call to <see cref="MobileServiceClient.LoginAsync"/>
        /// should be made with the overload where the provider is passed as a string.
        /// </param>
        /// <returns>The UserId and MobileServiceAuthentication token obtained from logging in.</returns>
        public static async Task<string> TestLoginAsync(MobileServiceAuthenticationProvider provider, bool useSingleSignOn, bool useProviderStringOverload)
        {
            MobileServiceUser user;
            if (useProviderStringOverload)
            {
                user = await client.LoginAsync(provider.ToString(), useSingleSignOn);
            }
            else
            {
                user = await client.LoginAsync(provider, useSingleSignOn);
            }
            
            return string.Format("UserId: {0} Token: {1}", user.UserId, user.MobileServiceAuthenticationToken);
        }

        /// <summary>
        /// Utility method that can be used to execute a test.  It will capture any exceptions throw
        /// during the execution of the test and return a message with details of the exception thrown.
        /// </summary>
        /// <param name="testName">The name of the test being executed.
        /// </param>
        /// <param name="test">A test to execute.
        /// </param>
        /// <returns>
        /// Either the result of the test if the test passed, or a message with the exception
        /// that was thrown.
        /// </returns>
        public static async Task<string> ExecuteTest(string testName, Func<Task<string>> test)
        {
            string resultText = null;
            bool didPass = false;

            if (client == null)
            {
                string appUrl = null;
                string appKey = null;
                App.Harness.Settings.Custom.TryGetValue("MobileServiceRuntimeUrl", out appUrl);
                App.Harness.Settings.Custom.TryGetValue("MobileServiceRuntimeKey", out appKey);

                client = new MobileServiceClient(appUrl, appKey);
            }

            try
            {
                resultText = await test();
                didPass = true;
            }
            catch (Exception exception)
            {
                resultText = string.Format("ExceptionType: {0} Message: {1} StackTrace: {2}",
                                               exception.GetType().ToString(),
                                               exception.Message,
                                               exception.StackTrace);
            }

            return string.Format("Test '{0}' {1}.\n{2}",
                                 testName,
                                 didPass ? "PASSED" : "FAILED",
                                 resultText);
        }
    }
}
