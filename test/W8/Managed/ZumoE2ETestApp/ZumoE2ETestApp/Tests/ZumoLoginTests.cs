using Microsoft.Live;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.UI.Xaml.Controls.Primitives;
using ZumoE2ETestApp.Framework;
using ZumoE2ETestApp.UIElements;

namespace ZumoE2ETestApp.Tests
{
    internal static class ZumoLoginTests
    {
        private static JsonObject lastUserIdentityObject = null;

        public static ZumoTestGroup CreateTests()
        {
            ZumoTestGroup result = new ZumoTestGroup("Login tests");
            result.AddTest(CreateLogoutTest());
            result.AddTest(CreateCRUDTest("w8Public", null, TablePermission.Public, false));
            result.AddTest(CreateCRUDTest("w8Application", null, TablePermission.Application, false));
            result.AddTest(CreateCRUDTest("w8Authenticated", null, TablePermission.User, false));
            result.AddTest(CreateCRUDTest("w8Admin", null, TablePermission.Admin, false));

            Dictionary<MobileServiceAuthenticationProvider, bool> providersWithRecycledTokenSupport;
            providersWithRecycledTokenSupport = new Dictionary<MobileServiceAuthenticationProvider, bool>
            {
                { MobileServiceAuthenticationProvider.Facebook, true },
                { MobileServiceAuthenticationProvider.Google, true },
                { MobileServiceAuthenticationProvider.MicrosoftAccount, false },
                { MobileServiceAuthenticationProvider.Twitter, false },
            };

            result.AddTest(ZumoTestCommon.CreateTestWithSingleAlert("In the next few tests you will be prompted for username / password four times."));

            foreach (MobileServiceAuthenticationProvider provider in Enum.GetValues(typeof(MobileServiceAuthenticationProvider)))
            {
                result.AddTest(CreateLogoutTest());
                result.AddTest(CreateLoginTest(provider, false));
                result.AddTest(CreateCRUDTest("w8Application", provider.ToString(), TablePermission.Application, true));
                result.AddTest(CreateCRUDTest("w8Authenticated", provider.ToString(), TablePermission.User, true));
                result.AddTest(CreateCRUDTest("w8Admin", provider.ToString(), TablePermission.Admin, true));

                bool supportsTokenRecycling;
                if (providersWithRecycledTokenSupport.TryGetValue(provider, out supportsTokenRecycling) && supportsTokenRecycling)
                {
                    result.AddTest(CreateLogoutTest());
                    result.AddTest(CreateClientSideLoginTest(provider));
                    result.AddTest(CreateCRUDTest("w8Authenticated", provider.ToString(), TablePermission.User, true));
                }
            }

            result.AddTest(ZumoTestCommon.CreateYesNoTest("Were you prompted for username / password four times?", true));

            //result.AddTest(CreateLogoutTest());
            //result.AddTest(CreateLiveSDKLoginTest());
            //result.AddTest(CreateCRUDTest("w8Authenticated", "Microsoft via Live SDK", TablePermission.User, true));

            result.AddTest(ZumoTestCommon.CreateTestWithSingleAlert("We'll log in again; you may or may not be asked for password in the next few moments."));
            foreach (MobileServiceAuthenticationProvider provider in Enum.GetValues(typeof(MobileServiceAuthenticationProvider)))
            {
                result.AddTest(CreateLogoutTest());
                result.AddTest(CreateLoginTest(provider, true));
                result.AddTest(CreateCRUDTest("w8Authenticated", provider.ToString(), TablePermission.User, true));
            }

            result.AddTest(ZumoTestCommon.CreateTestWithSingleAlert("Now we'll continue running the tests, but you *should not be prompted for the username anymore, and in some even the password should also not be required."));
            foreach (MobileServiceAuthenticationProvider provider in Enum.GetValues(typeof(MobileServiceAuthenticationProvider)))
            {
                result.AddTest(CreateLogoutTest());
                result.AddTest(CreateLoginTest(provider, true));
                result.AddTest(CreateCRUDTest("w8Authenticated", provider.ToString(), TablePermission.User, true));
            }

            result.AddTest(ZumoTestCommon.CreateYesNoTest("Were you prompted for the username in any of the providers?", false));

            return result;
        }

        private static ZumoTest CreateLoginTest(MobileServiceAuthenticationProvider provider, bool useSingleSignOn)
        {
            string testName = string.Format("Login with {0}{1}", provider, useSingleSignOn ? " (using single sign-on)" : "");
            return new ZumoTest(testName, async delegate(ZumoTest test)
            {
                var client = ZumoTestGlobals.Instance.Client;
                var user = await client.LoginAsync(provider, useSingleSignOn);
                test.AddLog("Logged in as {0}", user.UserId);
                return true;
            });
        }

        private static ZumoTest CreateLogoutTest()
        {
            return new ZumoTest("Logout", delegate(ZumoTest test)
            {
                var client = ZumoTestGlobals.Instance.Client;
                client.Logout();
                test.AddLog("Logged out. Current logged-in client: {0}", client.CurrentUser == null ? "<<NULL>>" : client.CurrentUser.UserId);
                TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
                tcs.SetResult(true);
                return tcs.Task;
            });
        }

        enum TablePermission { Public, Application, User, Admin }

        private static ZumoTest CreateLiveSDKLoginTest()
        {
            return new ZumoTest("Login via token with Live SDK", async delegate(ZumoTest test)
            {
                var client = ZumoTestGlobals.Instance.Client;
                var uri = client.ApplicationUri.ToString();
                var liveIdClient = new LiveAuthClient(uri);
                var liveLoginResult = await liveIdClient.LoginAsync(new string[] { "wl.basic" });
                if (liveLoginResult.Status == LiveConnectSessionStatus.Connected)
                {
                    var liveConnectClient = new LiveConnectClient(liveLoginResult.Session);
                    var me = await liveConnectClient.GetAsync("me");
                    test.AddLog("Logged in as {0}", me.RawResult);
                    if (liveLoginResult.Session.AuthenticationToken == null)
                    {
                        test.AddLog("Error, authentication token in the live login result is null");
                        return false;
                    }
                    else
                    {
                        JsonObject token = new JsonObject();
                        token.Add("authenticationToken", JsonValue.CreateStringValue(liveLoginResult.Session.AuthenticationToken));
                        var user = await client.LoginAsync(MobileServiceAuthenticationProvider.MicrosoftAccount, token);
                        test.AddLog("Logged in as {0}", user.UserId);
                        return true;
                    }
                }
                else
                {
                    test.AddLog("Login failed.");
                    return false;
                }
            });
        }

        private static ZumoTest CreateClientSideLoginTest(MobileServiceAuthenticationProvider provider)
        {
            return new ZumoTest("Login via token for " + provider, async delegate(ZumoTest test)
            {
                var client = ZumoTestGlobals.Instance.Client;
                var lastIdentity = lastUserIdentityObject;
                if (lastIdentity == null)
                {
                    test.AddLog("Last identity object is null. Cannot run this test.");
                    return false;
                }

                test.AddLog("Last user identity object: {0}", lastIdentity.Stringify());
                JsonObject token = new JsonObject();
                switch (provider)
                {
                    case MobileServiceAuthenticationProvider.Facebook:
                        token.Add("access_token", lastIdentity["facebook"].GetObject()["accessToken"]);
                        break;
                    case MobileServiceAuthenticationProvider.Google:
                        token.Add("access_token", lastIdentity["google"].GetObject()["accessToken"]);
                        break;
                    default:
                        test.AddLog("Client-side login test for {0} is not implemented or not supported.", provider);
                        return false;
                }

                var user = await client.LoginAsync(provider, token);
                test.AddLog("Logged in as {0}", user.UserId);
                return true;
            });
        }

        private static ZumoTest CreateCRUDTest(string tableName, string providerName, TablePermission tableType, bool userIsAuthenticated)
        {
            string testName = string.Format(CultureInfo.InvariantCulture, "CRUD, {0}, table with {1} permissions",
                userIsAuthenticated ? ("auth by " + providerName) : "unauthenticated", tableType);
            return new ZumoTest(testName, async delegate(ZumoTest test)
            {
                var client = ZumoTestGlobals.Instance.Client;
                var currentUser = client.CurrentUser;
                var table = client.GetTable(tableName);
                var crudShouldWork = tableType == TablePermission.Public || 
                    tableType == TablePermission.Application || 
                    (tableType == TablePermission.User && userIsAuthenticated);
                var item = new JsonObject();
                item.Add("Name", JsonValue.CreateStringValue("John Doe"));
                int id = 1;
                MobileServiceInvalidOperationException ex = null;
                try
                {
                    await table.InsertAsync(item);
                    test.AddLog("Inserted item: {0}", item.Stringify());
                    id = (int)item["id"].GetNumber();
                    if (tableType == TablePermission.User)
                    {
                        // script added user id to the document. Validating it
                        var userId = item["userId"].GetString();
                        if (userId == currentUser.UserId)
                        {
                            test.AddLog("User id correctly added by the server script");
                        }
                        else
                        {
                            test.AddLog("Error, user id not set by the server script");
                            return false;
                        }
                    }
                }
                catch (MobileServiceInvalidOperationException e)
                {
                    item["id"] = JsonValue.CreateNumberValue(1); // used in other requests
                    ex = e;
                }

                if (!ValidateExpectedError(test, ex, crudShouldWork))
                {
                    return false;
                }

                if (tableType == TablePermission.Public)
                {
                    // Update, Read and Delete are public; we don't need the app key anymore
                    client = new MobileServiceClient(client.ApplicationUri);
                    table = client.GetTable(tableName);
                }

                ex = null;
                try
                {
                    item["Name"] = JsonValue.CreateStringValue("Jane Roe");
                    await table.UpdateAsync(item);
                    test.AddLog("Updated item: {0}", item.Stringify());
                }
                catch (MobileServiceInvalidOperationException e)
                {
                    ex = e;
                }

                if (!ValidateExpectedError(test, ex, crudShouldWork))
                {
                    return false;
                }

                ex = null;
                try
                {
                    var item2 = await table.LookupAsync(id);
                    test.AddLog("Retrieved item via Lookup: {0}", item2.Stringify());
                    var obj = item2.GetObject();
                    if (obj.ContainsKey("Identities"))
                    {
                        string identities = obj["Identities"].GetString();
                        try
                        {
                            JsonObject identitiesObj = JsonObject.Parse(identities);
                            test.AddLog("Identities object: {0}", identitiesObj.Stringify());
                            lastUserIdentityObject = identitiesObj;
                        }
                        catch (Exception ex2)
                        {
                            test.AddLog("Could not parse the identites object as JSON: {0}", ex2);
                            lastUserIdentityObject = null;
                        }
                    }
                }
                catch (MobileServiceInvalidOperationException e)
                {
                    ex = e;
                }

                if (!ValidateExpectedError(test, ex, crudShouldWork))
                {
                    return false;
                }

                ex = null;
                try
                {
                    var items = await table.ReadAsync("$filter=id eq " + id);
                    test.AddLog("Retrieved items via Read: {0}", items.Stringify());
                    if (items.GetArray().Count != 1)
                    {
                        test.AddLog("Error, query should have returned exactly one item");
                        return false;
                    }
                }
                catch (MobileServiceInvalidOperationException e)
                {
                    ex = e;
                }

                if (!ValidateExpectedError(test, ex, crudShouldWork))
                {
                    return false;
                }

                ex = null;
                try
                {
                    await table.DeleteAsync(item);
                    test.AddLog("Deleted item: {0}", item.Stringify());
                }
                catch (MobileServiceInvalidOperationException e)
                {
                    ex = e;
                }

                if (!ValidateExpectedError(test, ex, crudShouldWork))
                {
                    return false;
                }

                return true;
            });
        }

        private static bool ValidateExpectedError(ZumoTest test, MobileServiceInvalidOperationException exception, bool operationShouldSucceed)
        {
            if (operationShouldSucceed)
            {
                if (exception != null)
                {
                    test.AddLog("Operation should have succeeded, but it failed: {0}", exception);
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                if (exception == null)
                {
                    test.AddLog("Operation should have failed, but it succeeded.");
                    return false;
                }
                else
                {
                    if (exception.Response.StatusCode == 401)
                    {
                        test.AddLog("Expected exception thrown, with expected status code.");
                        return true;
                    }
                    else
                    {
                        test.AddLog("Expected exception was thrown, but with invalid status code: {0}", exception.Response.StatusCode);
                        return false;
                    }
                }
            }
        }
    }
}
