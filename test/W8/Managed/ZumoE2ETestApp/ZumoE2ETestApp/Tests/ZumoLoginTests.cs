using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using ZumoE2ETestApp.Framework;

namespace ZumoE2ETestApp.Tests
{
    internal static class ZumoLoginTests
    {
        public static ZumoTestGroup CreateTests()
        {
            ZumoTestGroup result = new ZumoTestGroup("Login tests");
            result.AddTest(CreateLogoutTest());
            result.AddTest(CreateCRUDTest("w8Application", null, TablePermission.Application, false));
            result.AddTest(CreateCRUDTest("w8Authenticated", null, TablePermission.User, false));
            result.AddTest(CreateCRUDTest("w8Admin", null, TablePermission.Admin, false));

            foreach (MobileServiceAuthenticationProvider provider in Enum.GetValues(typeof(MobileServiceAuthenticationProvider)))
            {
                result.AddTest(CreateLogoutTest());
                result.AddTest(CreateLoginTest(provider, false));
                result.AddTest(CreateCRUDTest("w8Application", provider.ToString(), TablePermission.Application, true));
                result.AddTest(CreateCRUDTest("w8Authenticated", provider.ToString(), TablePermission.User, true));
                result.AddTest(CreateCRUDTest("w8Admin", provider.ToString(), TablePermission.Admin, true));
            }

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

        private static ZumoTest CreateCRUDTest(string tableName, string providerName, TablePermission tableType, bool userIsAuthenticated)
        {
            string testName = string.Format(CultureInfo.InvariantCulture, "CRUD, {0}, table with {1} permissions",
                userIsAuthenticated ? "unauthenticated" : ("auth by " + providerName), tableType);
            return new ZumoTest(testName, async delegate(ZumoTest test)
            {
                var client = ZumoTestGlobals.Instance.Client;
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
                    test.AddLog("Retrieved item: {0}", item2.Stringify());
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
