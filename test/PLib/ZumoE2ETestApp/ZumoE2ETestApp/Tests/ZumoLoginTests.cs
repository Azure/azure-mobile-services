// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Live;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using ZumoE2ETestApp.Framework;

namespace ZumoE2ETestApp.Tests
{
    internal static class ZumoLoginTests
    {
        private const string TablePublicPermission = "public";
        private const string TableApplicationPermission = "application";
        private const string TableUserPermission = "authenticated";
        private const string TableAdminPermission = "admin";

        private static JObject lastUserIdentityObject = null;

        private static Dictionary<string, string> testPropertyBag = new Dictionary<string, string>();
        private const string ClientIdKeyName = "clientId";

        public static ZumoTestGroup CreateTests()
        {
            ZumoTestGroup result = new ZumoTestGroup("Login tests");
            result.AddTest(CreateLogoutTest());
            result.AddTest(CreateCRUDTest(TablePublicPermission, null, TablePermission.Public, false));
            result.AddTest(CreateCRUDTest(TableApplicationPermission, null, TablePermission.Application, false));
            result.AddTest(CreateCRUDTest(TableUserPermission, null, TablePermission.User, false));
            result.AddTest(CreateCRUDTest(TableAdminPermission, null, TablePermission.Admin, false));

            int indexOfTestsWithAuthentication = result.AllTests.Count();

            Dictionary<MobileServiceAuthenticationProvider, bool> providersWithRecycledTokenSupport;
            providersWithRecycledTokenSupport = new Dictionary<MobileServiceAuthenticationProvider, bool>
            {
                { MobileServiceAuthenticationProvider.Facebook, true },
                { MobileServiceAuthenticationProvider.Google, false },   // Known bug - Drop login via Google token until Google client flow is reintroduced
                { MobileServiceAuthenticationProvider.MicrosoftAccount, false },
                { MobileServiceAuthenticationProvider.Twitter, false },
            };

#if !WINDOWS_PHONE
            result.AddTest(ZumoTestCommon.CreateTestWithSingleAlert("In the next few tests you will be prompted for username / password four times."));
#endif

            foreach (MobileServiceAuthenticationProvider provider in Util.EnumGetValues(typeof(MobileServiceAuthenticationProvider)))
            {
                result.AddTest(CreateLogoutTest());
#if !WINDOWS_PHONE
                result.AddTest(CreateLoginTest(provider, false));
#else
                result.AddTest(CreateLoginTest(provider));
#endif
                result.AddTest(CreateCRUDTest(TableApplicationPermission, provider.ToString(), TablePermission.Application, true));
                result.AddTest(CreateCRUDTest(TableUserPermission, provider.ToString(), TablePermission.User, true));
                result.AddTest(CreateCRUDTest(TableAdminPermission, provider.ToString(), TablePermission.Admin, true));

                bool supportsTokenRecycling;
                if (providersWithRecycledTokenSupport.TryGetValue(provider, out supportsTokenRecycling) && supportsTokenRecycling)
                {
                    result.AddTest(CreateLogoutTest());
                    result.AddTest(CreateClientSideLoginTest(provider));
                    result.AddTest(CreateCRUDTest(TableUserPermission, provider.ToString(), TablePermission.User, true));
                }
            }

#if !WINDOWS_PHONE
            result.AddTest(ZumoTestCommon.CreateYesNoTest("Were you prompted for username / password four times?", true));
#endif

            result.AddTest(CreateLogoutTest());

#if WINDOWS_PHONE && !WP75
            result.AddTest(ZumoTestCommon.CreateInputTest("Enter Live App Client ID", testPropertyBag, ClientIdKeyName));
#endif

#if !WP75
            result.AddTest(CreateLiveSDKLoginTest());
            result.AddTest(CreateCRUDTest(TableUserPermission, "Microsoft via Live SDK", TablePermission.User, true));
#endif

#if !WINDOWS_PHONE
            result.AddTest(ZumoTestCommon.CreateTestWithSingleAlert("We'll log in again; you may or may not be asked for password in the next few moments."));
            foreach (MobileServiceAuthenticationProvider provider in Enum.GetValues(typeof(MobileServiceAuthenticationProvider)))
            {
                if (provider == MobileServiceAuthenticationProvider.MicrosoftAccount)
                {
                    // Known issue - SSO with MS account will not work if Live SDK is also used
                    continue;
                }

                result.AddTest(CreateLogoutTest());
                result.AddTest(CreateLoginTest(provider, true));
                result.AddTest(CreateCRUDTest(TableUserPermission, provider.ToString(), TablePermission.User, true));
            }

            result.AddTest(ZumoTestCommon.CreateTestWithSingleAlert("Now we'll continue running the tests, but you *should not be prompted for the username or password anymore*."));
            foreach (MobileServiceAuthenticationProvider provider in Enum.GetValues(typeof(MobileServiceAuthenticationProvider)))
            {
                if (provider == MobileServiceAuthenticationProvider.MicrosoftAccount)
                {
                    // Known issue - SSO with MS account will not work if Live SDK is also used
                    continue;
                }

                result.AddTest(CreateLogoutTest());
                result.AddTest(CreateLoginTest(provider, true));
                result.AddTest(CreateCRUDTest(TableUserPermission, provider.ToString(), TablePermission.User, true));
            }

            result.AddTest(ZumoTestCommon.CreateYesNoTest("Were you prompted for the username in any of the providers?", false));
#endif

            foreach (var test in result.AllTests.Skip(indexOfTestsWithAuthentication))
            {
                test.CanRunUnattended = false;
            }

            // Clean-up any logged in user
            result.AddTest(CreateLogoutTest());

            return result;
        }

#if !WINDOWS_PHONE
        internal static ZumoTest CreateLoginTest(MobileServiceAuthenticationProvider provider, bool useSingleSignOn = false)
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
#else
        internal static ZumoTest CreateLoginTest(MobileServiceAuthenticationProvider provider)
        {
            string testName = string.Format("Login with {0}", provider);
            return new ZumoTest(testName, async delegate(ZumoTest test)
            {
                var client = ZumoTestGlobals.Instance.Client;
                var user = await client.LoginAsync(provider);
                test.AddLog("Logged in as {0}", user.UserId);
                return true;
            });
        }
#endif

        internal static ZumoTest CreateLogoutTest()
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

#if !WP75
        private static ZumoTest CreateLiveSDKLoginTest()
        {
            return new ZumoTest("Login via token with Live SDK", async delegate(ZumoTest test)
            {
                var client = ZumoTestGlobals.Instance.Client;
#if !WINDOWS_PHONE
                var uri = client.ApplicationUri.ToString();
                var liveIdClient = new LiveAuthClient(uri);
#else
                string clientId;
                testPropertyBag.TryGetValue(ClientIdKeyName, out clientId);
                if (clientId == null)
                {
                    test.AddLog("ClientId of Microsoft application not entered correctly.");
                    return false;
                }

                var liveIdClient = new LiveAuthClient(clientId);
#endif
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
                        var token = new JObject(new JProperty("authenticationToken", liveLoginResult.Session.AuthenticationToken));
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
#endif

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

                lastUserIdentityObject = null;

                test.AddLog("Last user identity object: {0}", lastIdentity);
                var token = new JObject();
                switch (provider)
                {
                    case MobileServiceAuthenticationProvider.Facebook:
                        token.Add("access_token", lastIdentity["facebook"]["accessToken"]);
                        break;
                    case MobileServiceAuthenticationProvider.Google:
                        token.Add("access_token", lastIdentity["google"]["accessToken"]);
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
                var item = new JObject();
                item.Add("name", "John Doe");
                int id = 1;
                Dictionary<string, string> queryParameters = new Dictionary<string, string>
                {
                    { "userIsAuthenticated", userIsAuthenticated.ToString().ToLowerInvariant() },
                };
                MobileServiceInvalidOperationException ex = null;
                try
                {
                    var inserted = await table.InsertAsync(item, queryParameters);
                    item = (JObject)inserted;
                    test.AddLog("Inserted item: {0}", item);
                    id = item["id"].Value<int>();
                    if (tableType == TablePermission.User)
                    {
                        // script added user id to the document. Validating it
                        var userId = item["userId"].Value<string>();
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
                    item["id"] = 1; // used in other requests
                    ex = e;
                }

                if (!ValidateExpectedError(test, ex, crudShouldWork))
                {
                    return false;
                }

                if (tableType == TablePermission.Public)
                {
                    // Update, Read and Delete are public; we don't need the app key anymore
                    var oldClient = client;
                    client = new MobileServiceClient(oldClient.ApplicationUri);
                    if (oldClient.CurrentUser != null)
                    {
                        client.CurrentUser = new MobileServiceUser(oldClient.CurrentUser.UserId);
                        client.CurrentUser.MobileServiceAuthenticationToken = oldClient.CurrentUser.MobileServiceAuthenticationToken;
                    }

                    table = client.GetTable(tableName);
                }

                ex = null;
                try
                {
                    item["name"] = "Jane Roe";
                    var updated = await table.UpdateAsync(item, queryParameters);
                    test.AddLog("Updated item: {0}", updated);
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
                    var item2 = await table.LookupAsync(id, queryParameters);
                    test.AddLog("Retrieved item via Lookup: {0}", item2);
                    var obj = item2 as JObject;
                    if (obj["Identities"] != null)
                    {
                        string identities = obj["Identities"].Value<string>();
                        try
                        {
                            var identitiesObj = JObject.Parse(identities);
                            test.AddLog("Identities object: {0}", identitiesObj);
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
                    var items = await table.ReadAsync("$filter=id eq " + id, queryParameters);
                    test.AddLog("Retrieved items via Read: {0}", items);
                    if (((JArray)items).Count != 1)
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
                    await table.DeleteAsync(item, queryParameters);
                    test.AddLog("Deleted item: {0}", item);
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
                    if (exception.Response.StatusCode == HttpStatusCode.Unauthorized)
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
