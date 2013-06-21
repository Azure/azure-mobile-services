// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Azure.Zumo.Win8.Test;
using Microsoft.WindowsAzure.MobileServices;
using Windows.Data.Json;
using System.Threading.Tasks;

namespace Microsoft.Azure.Zumo.Win8.CSharp.Test
{
    public class MobileServiceLoginTests : TestBase
    {
        [AsyncTestMethod]
        public async Task SendLoginAsync()
        {
            TestServiceFilter hijack = new TestServiceFilter();
            MobileServiceClient client = new MobileServiceClient("http://www.test.com", "secret...").WithFilter(hijack);
            MobileServiceLogin login = new MobileServiceLogin(client, ignoreFilters:false);

            // Send back a successful login response
            hijack.Response.Content =
                new JsonObject()
                    .Set("authenticationToken", "rhubarb")
                    .Set("user",
                        new JsonObject()
                            .Set("userId", "123456")).Stringify();
            MobileServiceUser current = await login.SendLoginAsync("donkey");

            Assert.IsNotNull(current);
            Assert.AreEqual("123456", current.UserId);
            Assert.AreEqual("rhubarb", current.MobileServiceAuthenticationToken);
            Assert.EndsWith(hijack.Request.Uri.ToString(), "login");
            string input = JsonValue.Parse(hijack.Request.Content).Get("authenticationToken").AsString();
            Assert.AreEqual("donkey", input);
            Assert.AreEqual("POST", hijack.Request.Method);
            Assert.AreSame(current, client.CurrentUser);
        }

        [TestMethod]
        public void SendLoginAsyncThrows()
        {
            TestServiceFilter hijack = new TestServiceFilter();
            MobileServiceClient client = new MobileServiceClient("http://www.test.com", "secret...").WithFilter(hijack);
            MobileServiceLogin login = new MobileServiceLogin(client, ignoreFilters: false);

            // Verify error cases
            ThrowsAsync<ArgumentNullException>(async () => await login.SendLoginAsync(null));
            ThrowsAsync<ArgumentException>(async () => await login.SendLoginAsync(""));

            // Send back a failure and ensure it throws
            hijack.Response.Content =
                new JsonObject().Set("error", "login failed").Stringify();
            hijack.Response.StatusCode = 401;
            ThrowsAsync<InvalidOperationException>(async () =>
            {
                await login.SendLoginAsync("donkey");
            });
        }
    }
}