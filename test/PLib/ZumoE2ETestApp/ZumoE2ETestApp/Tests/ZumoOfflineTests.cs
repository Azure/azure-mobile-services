// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Newtonsoft.Json.Linq;
using Windows.Storage;
using ZumoE2ETestApp.Framework;
using ZumoE2ETestApp.Tests.Types;

namespace ZumoE2ETestApp.Tests
{
    internal static class ZumoOfflineTests
    {
        public const string OfflineReadyTableName = "offlineReady";
        public const string OfflineReadyNoVersionAuthenticatedTableName = "offlineReadyNoVersionAuthenticated";

        private const string StoreFileName = "store.bin";

        internal static ZumoTestGroup CreateTests()
        {
            ZumoTestGroup result = new ZumoTestGroup("Offline tests");

            result.AddTest(CreateClearStoreTest());

            result.AddTest(CreateBasicTest());
            result.AddTest(CreateSyncConflictTest(true));
            result.AddTest(CreateSyncConflictTest(false));

            result.AddTest(CreateAbortPushDuringSyncTest(whereToAbort: SyncAbortLocation.Start));
            result.AddTest(CreateAbortPushDuringSyncTest(whereToAbort: SyncAbortLocation.Middle));
            result.AddTest(CreateAbortPushDuringSyncTest(whereToAbort: SyncAbortLocation.End));

            result.AddTest(ZumoLoginTests.CreateLogoutTest());
            result.AddTest(CreateSyncTestForAuthenticatedTable(false));
            result.AddTest(ZumoLoginTests.CreateLoginTest(MobileServiceAuthenticationProvider.Facebook));
            var noOptimisticConcurrencyTest = CreateNoOptimisticConcurrencyTest();
            noOptimisticConcurrencyTest.CanRunUnattended = false;
            result.AddTest(noOptimisticConcurrencyTest);
            result.AddTest(CreateSyncTestForAuthenticatedTable(true));
            result.AddTest(ZumoLoginTests.CreateLogoutTest());

            return result;
        }

        private static MobileServiceClient CreateClient(params HttpMessageHandler[] handlers)
        {
            var globalClient = ZumoTestGlobals.Instance.Client;
            var offlineReadyClient = new MobileServiceClient(
                globalClient.ApplicationUri,
                globalClient.ApplicationKey,
                handlers);

            if (globalClient.CurrentUser != null)
            {
                offlineReadyClient.CurrentUser = new MobileServiceUser(globalClient.CurrentUser.UserId);
                offlineReadyClient.CurrentUser.MobileServiceAuthenticationToken = globalClient.CurrentUser.MobileServiceAuthenticationToken;
            }
            else
            {
                offlineReadyClient.Logout();
            }

            return offlineReadyClient;
        }

        private static ZumoTest CreateSyncTestForAuthenticatedTable(bool userIsLoggedIn)
        {
            bool isLoggedIn = userIsLoggedIn;
            var testName = "Sync test for authenticated table, with user " + (isLoggedIn ? "logged in" : "not logged in");
            return new ZumoTest(testName, async delegate(ZumoTest test)
            {
                DateTime now = DateTime.UtcNow;
                int seed = now.Year * 10000 + now.Month * 100 + now.Day;
                test.AddLog("Using random seed: {0}", seed);
                Random rndGen = new Random(seed);

                var offlineReadyClient = CreateClient();

                var localStore = new MobileServiceSQLiteStore(StoreFileName);
                test.AddLog("Defined the table on the local store");
                localStore.DefineTable<OfflineReadyItemNoVersion>();

                await offlineReadyClient.SyncContext.InitializeAsync(localStore);
                test.AddLog("Initialized the store and sync context");

                var localTable = offlineReadyClient.GetSyncTable<OfflineReadyItemNoVersion>();
                var remoteTable = offlineReadyClient.GetTable<OfflineReadyItemNoVersion>();

                var item = new OfflineReadyItemNoVersion(rndGen);
                await localTable.InsertAsync(item);
                test.AddLog("Inserted the item to the local store:", item);

                try
                {
                    await offlineReadyClient.SyncContext.PushAsync();
                    test.AddLog("Pushed the changes to the server");
                    if (isLoggedIn)
                    {
                        test.AddLog("As expected, push succeeded");
                    }
                    else
                    {
                        test.AddLog("Error, table should only work with authenticated access, but user is not logged in");
                        return false;
                    }
                }
                catch (MobileServicePushFailedException ex)
                {
                    if (isLoggedIn)
                    {
                        test.AddLog("Error, user is logged in but push operation failed: {0}", ex);
                        return false;
                    }

                    test.AddLog("Got expected exception: {0}: {1}", ex.GetType().FullName, ex.Message);
                    Exception inner = ex.InnerException;
                    while (inner != null)
                    {
                        test.AddLog("  {0}: {1}", inner.GetType().FullName, inner.Message);
                        inner = inner.InnerException;
                    }
                }

                if (!isLoggedIn)
                {
                    test.AddLog("Push should have failed, so now will try to log in to complete the push operation");
                    await offlineReadyClient.LoginAsync(MobileServiceAuthenticationProvider.Facebook);
                    test.AddLog("Logged in as {0}", offlineReadyClient.CurrentUser.UserId);
                    await offlineReadyClient.SyncContext.PushAsync();
                    test.AddLog("Push succeeded");
                }

                await localTable.PurgeAsync();
                test.AddLog("Purged the local table");
                await localTable.PullAsync(null, localTable.Where(i => i.Id == item.Id));
                test.AddLog("Pulled the data into the local table");
                List<OfflineReadyItemNoVersion> serverItems = await localTable.ToListAsync();
                test.AddLog("Retrieved items from the local table");

                test.AddLog("Removing item from the remote table");
                await remoteTable.DeleteAsync(item);

                if (!isLoggedIn)
                {
                    offlineReadyClient.Logout();
                    test.AddLog("Logged out again");
                }

                var firstServerItem = serverItems.FirstOrDefault();
                bool testResult = true;
                if (item.Equals(firstServerItem))
                {
                    test.AddLog("Data round-tripped successfully");
                }
                else
                {
                    test.AddLog("Error, data did not round-trip successfully. Expected: {0}, actual: {1}", item, firstServerItem);
                    testResult = false;
                }

                test.AddLog("Cleaning up");
                await localTable.PurgeAsync();
                test.AddLog("Done");
                return testResult;
            })
            {
                CanRunUnattended = false
            };
        }

        enum SyncAbortLocation { Start, Middle, End };

        class AbortingSyncHandler : IMobileServiceSyncHandler
        {
            ZumoTest test;

            public AbortingSyncHandler(ZumoTest test, Func<string, bool> shouldAbortForId)
            {
                this.test = test;
                this.AbortCondition = shouldAbortForId;
            }

            public Func<string, bool> AbortCondition { get; set; }

            public Task<JObject> ExecuteTableOperationAsync(IMobileServiceTableOperation operation)
            {
                var itemId = (string)operation.Item[MobileServiceSystemColumns.Id];
                if (this.AbortCondition(itemId))
                {
                    test.AddLog("Found id to abort ({0}), aborting the push operation");
                    operation.AbortPush();
                }
                else
                {
                    test.AddLog("Pushing operation {0} for item {1}", operation.Kind, itemId);
                }

                return operation.ExecuteAsync();
            }

            public Task OnPushCompleteAsync(MobileServicePushCompletionResult result)
            {
                return Task.FromResult(0);
            }
        }

        private static ZumoTest CreateAbortPushDuringSyncTest(SyncAbortLocation whereToAbort)
        {
            SyncAbortLocation abortLocation = whereToAbort;
            var testName = "Aborting push during sync - aborting at " + whereToAbort.ToString().ToLowerInvariant() + " of queue";
            return new ZumoTest(testName, async delegate(ZumoTest test)
            {
                DateTime now = DateTime.UtcNow;
                int seed = now.Year * 10000 + now.Month * 100 + now.Day;
                test.AddLog("Using random seed: {0}", seed);
                Random rndGen = new Random(seed);

                var offlineReadyClient = CreateClient();

                var items = Enumerable.Range(0, 10).Select(_ => new OfflineReadyItem(rndGen)).ToArray();
                foreach (var item in items)
                {
                    item.Id = Guid.NewGuid().ToString("D");
                }

                int abortIndex = abortLocation == SyncAbortLocation.Start ? 0 :
                    (abortLocation == SyncAbortLocation.End ? items.Length - 1 : rndGen.Next(1, items.Length - 1));
                var idToAbort = items[abortIndex].Id;
                test.AddLog("Will send {0} items, aborting when id = {1}", items.Length, idToAbort);

                var localStore = new MobileServiceSQLiteStore(StoreFileName);
                test.AddLog("Defined the table on the local store");
                localStore.DefineTable<OfflineReadyItem>();

                var syncHandler = new AbortingSyncHandler(test, id => id == idToAbort);
                await offlineReadyClient.SyncContext.InitializeAsync(localStore, syncHandler);
                test.AddLog("Initialized the store and sync context");

                var localTable = offlineReadyClient.GetSyncTable<OfflineReadyItem>();
                var remoteTable = offlineReadyClient.GetTable<OfflineReadyItem>();

                foreach (var item in items)
                {
                    await localTable.InsertAsync(item);
                }

                test.AddLog("Inserted {0} items in the local table. Now pushing those");

                try
                {
                    await offlineReadyClient.SyncContext.PushAsync();
                    test.AddLog("Error, push call should have failed");
                    return false;
                }
                catch (MobileServicePushFailedException ex)
                {
                    test.AddLog("Caught (expected) exception: {0}", ex);
                }

                var expectedOperationQueueSize = items.Length - abortIndex;
                test.AddLog("Current operation queue size: {0}", offlineReadyClient.SyncContext.PendingOperations);
                if (expectedOperationQueueSize != offlineReadyClient.SyncContext.PendingOperations)
                {
                    test.AddLog("Error, expected {0} items in the queue", expectedOperationQueueSize);
                    return false;
                }

                foreach (var allItemsPushed in new bool[] { false, true })
                {
                    HashSet<OfflineReadyItem> itemsInServer, itemsNotInServer;
                    if (allItemsPushed)
                    {
                        itemsInServer = new HashSet<OfflineReadyItem>(items.ToArray());
                        itemsNotInServer = new HashSet<OfflineReadyItem>(Enumerable.Empty<OfflineReadyItem>());
                    }
                    else
                    {
                        itemsInServer = new HashSet<OfflineReadyItem>(items.Where((item, index) => index < abortIndex));
                        itemsNotInServer = new HashSet<OfflineReadyItem>(items.Where((item, index) => index >= abortIndex));
                    }

                    foreach (var item in items)
                    {
                        var itemFromServer = (await remoteTable.Where(i => i.Id == item.Id).Take(1).ToEnumerableAsync()).FirstOrDefault();
                        test.AddLog("Item with id = {0} from server: {1}", item.Id,
                            itemFromServer == null ? "<<null>>" : itemFromServer.ToString());
                        if (itemsInServer.Contains(item) && itemFromServer == null)
                        {
                            test.AddLog("Error, the item {0} should have made to the server", item.Id);
                            return false;
                        }
                        else if (itemsNotInServer.Contains(item) && itemFromServer != null)
                        {
                            test.AddLog("Error, the item {0} should not have made to the server", item.Id);
                            return false;
                        }
                    }

                    if (!allItemsPushed)
                    {
                        test.AddLog("Changing the handler so that it doesn't abort anymore.");
                        syncHandler.AbortCondition = _ => false;
                        test.AddLog("Pushing again");
                        await offlineReadyClient.SyncContext.PushAsync();
                        test.AddLog("Finished pushing all elements");
                    }
                }

                test.AddLog("Changing the handler so that it doesn't abort anymore.");
                syncHandler.AbortCondition = _ => false;

                test.AddLog("Cleaning up");
                foreach (var item in items)
                {
                    await localTable.DeleteAsync(item);
                }

                await offlineReadyClient.SyncContext.PushAsync();
                test.AddLog("Done");

                return true;
            });
        }

        private static ZumoTest CreateClearStoreTest()
        {
            return new ZumoTest("Clear store", async delegate(ZumoTest test)
            {
                var files = await ApplicationData.Current.LocalFolder.GetFilesAsync();
                foreach (var file in files)
                {
                    if (file.Name == StoreFileName)
                    {
                        test.AddLog("Deleting store file");
                        await file.DeleteAsync();
                        break;
                    }
                }

                return true;
            });
        }

        class CountingHandler : DelegatingHandler
        {
            public int RequestCount { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                this.RequestCount++;
                return base.SendAsync(request, cancellationToken);
            }
        }

        private static ZumoTest CreateBasicTest()
        {
            return new ZumoTest("Basic offline scenario", async delegate(ZumoTest test)
            {
                DateTime now = DateTime.UtcNow;
                int seed = now.Year * 10000 + now.Month * 100 + now.Day;
                test.AddLog("Using random seed: {0}", seed);
                Random rndGen = new Random(seed);

                CountingHandler handler = new CountingHandler();
                var requestsSentToServer = 0;
                var offlineReadyClient = CreateClient(handler);

                var localStore = new MobileServiceSQLiteStore(StoreFileName);
                test.AddLog("Defined the table on the local store");
                localStore.DefineTable<OfflineReadyItem>();

                await offlineReadyClient.SyncContext.InitializeAsync(localStore);
                test.AddLog("Initialized the store and sync context");

                var localTable = offlineReadyClient.GetSyncTable<OfflineReadyItem>();
                var remoteTable = offlineReadyClient.GetTable<OfflineReadyItem>();

                var item = new OfflineReadyItem(rndGen);
                await localTable.InsertAsync(item);
                test.AddLog("Inserted the item to the local store:", item);

                test.AddLog("Validating that the item is not in the server table");
                try
                {
                    requestsSentToServer++;
                    await remoteTable.LookupAsync(item.Id);
                    test.AddLog("Error, item is present in the server");
                    return false;
                }
                catch (MobileServiceInvalidOperationException ex)
                {
                    test.AddLog("Ok, item is not in the server: {0}", ex.Message);
                }

                Func<int, bool> validateRequestCount = expectedCount =>
                {
                    test.AddLog("So far {0} requests sent to the server", handler.RequestCount);
                    if (handler.RequestCount != expectedCount)
                    {
                        test.AddLog("Error, expected {0} requests to have been sent to the server", expectedCount);
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                };

                if (!validateRequestCount(requestsSentToServer)) return false;

                test.AddLog("Pushing changes to the server");
                await offlineReadyClient.SyncContext.PushAsync();
                requestsSentToServer++;

                if (!validateRequestCount(requestsSentToServer)) return false;

                test.AddLog("Push done; now verifying that item is in the server");

                var serverItem = await remoteTable.LookupAsync(item.Id);
                requestsSentToServer++;
                test.AddLog("Retrieved item from server: {0}", serverItem);
                if (serverItem.Equals(item))
                {
                    test.AddLog("Items are the same");
                }
                else
                {
                    test.AddLog("Items are different. Local: {0}; remote: {1}", item, serverItem);
                    return false;
                }

                test.AddLog("Now updating the item locally");
                item.Flag = !item.Flag;
                item.Age++;
                item.Date = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Millisecond, DateTimeKind.Utc);
                await localTable.UpdateAsync(item);
                test.AddLog("Item has been updated");

                var newItem = new OfflineReadyItem(rndGen);
                test.AddLog("Adding a new item to the local table: {0}", newItem);
                await localTable.InsertAsync(newItem);

                if (!validateRequestCount(requestsSentToServer)) return false;

                test.AddLog("Pushing the new changes to the server");
                await offlineReadyClient.SyncContext.PushAsync();
                requestsSentToServer += 2;

                if (!validateRequestCount(requestsSentToServer)) return false;

                test.AddLog("Push done. Verifying changes on the server");
                serverItem = await remoteTable.LookupAsync(item.Id);
                requestsSentToServer++;
                if (serverItem.Equals(item))
                {
                    test.AddLog("Updated items are the same");
                }
                else
                {
                    test.AddLog("Items are different. Local: {0}; remote: {1}", item, serverItem);
                    return false;
                }

                serverItem = await remoteTable.LookupAsync(newItem.Id);
                requestsSentToServer++;
                if (serverItem.Equals(newItem))
                {
                    test.AddLog("New inserted item is the same");
                }
                else
                {
                    test.AddLog("Items are different. Local: {0}; remote: {1}", item, serverItem);
                    return false;
                }

                test.AddLog("Cleaning up");
                await localTable.DeleteAsync(item);
                await localTable.DeleteAsync(newItem);
                test.AddLog("Local table cleaned up. Now sync'ing once more");
                await offlineReadyClient.SyncContext.PushAsync();
                requestsSentToServer += 2;
                if (!validateRequestCount(requestsSentToServer)) return false;
                test.AddLog("Done");
                return true;
            }, ZumoTestGlobals.RuntimeFeatureNames.STRING_ID_TABLES);
        }

        private static ZumoTest CreateNoOptimisticConcurrencyTest()
        {
            // If a table does not have a __version column, then offline will still
            // work, but there will be no conflicts
            return new ZumoTest("Offline without version column", async delegate(ZumoTest test)
            {
                if (ZumoTestGlobals.Instance.IsNetRuntime)
                {
                    return true;
                }

                DateTime now = DateTime.UtcNow;
                int seed = now.Year * 10000 + now.Month * 100 + now.Day;
                test.AddLog("Using random seed: {0}", seed);
                Random rndGen = new Random(seed);

                var offlineReadyClient = CreateClient();

                var localStore = new MobileServiceSQLiteStore(StoreFileName);
                test.AddLog("Defined the table on the local store");
                localStore.DefineTable<OfflineReadyItemNoVersion>();

                await offlineReadyClient.SyncContext.InitializeAsync(localStore);
                test.AddLog("Initialized the store and sync context");

                var localTable = offlineReadyClient.GetSyncTable<OfflineReadyItemNoVersion>();
                var remoteTable = offlineReadyClient.GetTable<OfflineReadyItemNoVersion>();

                var item = new OfflineReadyItemNoVersion(rndGen);
                await localTable.InsertAsync(item);
                test.AddLog("Inserted the item to the local store:", item);

                await offlineReadyClient.SyncContext.PushAsync();
                test.AddLog("Pushed the changes to the server");

                var serverItem = await remoteTable.LookupAsync(item.Id);
                serverItem.Name = "changed name";
                serverItem.Age = 0;
                await remoteTable.UpdateAsync(serverItem);
                test.AddLog("Server item updated (changes will be overwritten later");

                item.Age = item.Age + 1;
                item.Name = item.Name + " - modified";
                await localTable.UpdateAsync(item);
                test.AddLog("Updated item locally, will now push changes to the server: {0}", item);
                await offlineReadyClient.SyncContext.PushAsync();

                serverItem = await remoteTable.LookupAsync(item.Id);
                test.AddLog("Retrieved the item from the server: {0}", serverItem);

                if (serverItem.Equals(item))
                {
                    test.AddLog("Items are the same");
                }
                else
                {
                    test.AddLog("Items are different. Local: {0}; remote: {1}", item, serverItem);
                    return false;
                }

                test.AddLog("Cleaning up");
                await localTable.DeleteAsync(item);
                test.AddLog("Local table cleaned up. Now sync'ing once more");
                await offlineReadyClient.SyncContext.PushAsync();
                test.AddLog("Done");
                return true;
            });
        }

        class ConflictResolvingSyncHandler<T> : IMobileServiceSyncHandler
        {
            public delegate T ConflictResolution(T clientItem, T serverItem);
            IMobileServiceClient client;
            ConflictResolution conflictResolution;
            ZumoTest test;

            public ConflictResolvingSyncHandler(ZumoTest test, IMobileServiceClient client, ConflictResolution resolutionPolicy)
            {
                this.test = test;
                this.client = client;
                this.conflictResolution = resolutionPolicy;
            }

            public async Task<JObject> ExecuteTableOperationAsync(IMobileServiceTableOperation operation)
            {
                MobileServicePreconditionFailedException ex = null;
                JObject result = null;
                do
                {
                    ex = null;
                    try
                    {
                        test.AddLog("Attempting to execute the operation");
                        result = await operation.ExecuteAsync();
                    }
                    catch (MobileServicePreconditionFailedException e)
                    {
                        ex = e;
                    }

                    if (ex != null)
                    {
                        test.AddLog("A MobileServicePreconditionFailedException was thrown, ex.Value = {0}", ex.Value);
                        var serverItem = ex.Value;
                        if (serverItem == null)
                        {
                            test.AddLog("Item not returned in the exception, trying to retrieve it from the server");
                            serverItem = (JObject)(await client.GetTable(operation.Table.TableName).LookupAsync((string)operation.Item["id"]));
                        }

                        var typedClientItem = operation.Item.ToObject<T>();
                        var typedServerItem = serverItem.ToObject<T>();
                        var typedMergedItem = conflictResolution(typedClientItem, typedServerItem);
                        var mergedItem = JObject.FromObject(typedMergedItem);
                        mergedItem[MobileServiceSystemColumns.Version] = serverItem[MobileServiceSystemColumns.Version];
                        test.AddLog("Merged the items, will try to resubmit the operation");
                        operation.Item = mergedItem;
                    }
                } while (ex != null);

                return result;
            }

            public Task OnPushCompleteAsync(MobileServicePushCompletionResult result)
            {
                return Task.FromResult(0);
            }
        }

        private static ZumoTest CreateSyncConflictTest(bool autoResolve)
        {
            var testName = "Offline - dealing with conflicts - " +
                (autoResolve ? "client resolves conflicts" : "push fails after conflicts");
            bool resolveConflictsOnClient = autoResolve;
            return new ZumoTest(testName, async delegate(ZumoTest test)
            {
                DateTime now = DateTime.UtcNow;
                int seed = now.Year * 10000 + now.Month * 100 + now.Day;
                test.AddLog("Using random seed: {0}", seed);
                Random rndGen = new Random(seed);

                var offlineReadyClient = CreateClient();

                var localStore = new MobileServiceSQLiteStore(StoreFileName);
                test.AddLog("Defined the table on the local store");
                localStore.DefineTable<OfflineReadyItem>();

                ConflictResolvingSyncHandler<OfflineReadyItem>.ConflictResolution conflictHandlingPolicy;
                conflictHandlingPolicy = (client, server) =>
                        new OfflineReadyItem
                        {
                            Id = client.Id,
                            Age = Math.Max(client.Age, server.Age),
                            Date = client.Date > server.Date ? client.Date : server.Date,
                            Flag = client.Flag || server.Flag,
                            FloatingNumber = Math.Max(client.FloatingNumber, server.FloatingNumber),
                            Name = client.Name
                        };
                if (resolveConflictsOnClient)
                {
                    var handler = new ConflictResolvingSyncHandler<OfflineReadyItem>(test, offlineReadyClient, conflictHandlingPolicy);
                    await offlineReadyClient.SyncContext.InitializeAsync(localStore, handler);
                }
                else
                {
                    await offlineReadyClient.SyncContext.InitializeAsync(localStore);
                }

                test.AddLog("Initialized the store and sync context");

                var localTable = offlineReadyClient.GetSyncTable<OfflineReadyItem>();
                var remoteTable = offlineReadyClient.GetTable<OfflineReadyItem>();

                await localTable.PurgeAsync();
                test.AddLog("Removed all items from the local table");

                var item = new OfflineReadyItem(rndGen);
                await remoteTable.InsertAsync(item);
                test.AddLog("Inserted the item to the remote store:", item);

                var pullQuery = "$filter=id eq '" + item.Id + "'";
                await localTable.PullAsync(null, pullQuery);

                test.AddLog("Changing the item on the server");
                item.Age++;
                await remoteTable.UpdateAsync(item);
                test.AddLog("Updated the item: {0}", item);

                var localItem = await localTable.LookupAsync(item.Id);
                test.AddLog("Retrieved the item from the local table, now updating it");
                localItem.Date = localItem.Date.AddDays(1);
                await localTable.UpdateAsync(localItem);
                test.AddLog("Updated the item on the local table");

                test.AddLog("Now trying to pull changes from the server (will trigger a push)");
                bool testResult = true;
                try
                {
                    await localTable.PullAsync(null, pullQuery);
                    if (!autoResolve)
                    {
                        test.AddLog("Error, pull (push) should have caused a conflict, but none happened.");
                        testResult = false;
                    }
                    else
                    {
                        var expectedMergedItem = conflictHandlingPolicy(localItem, item);
                        var localMergedItem = await localTable.LookupAsync(item.Id);
                        if (localMergedItem.Equals(expectedMergedItem))
                        {
                            test.AddLog("Item was merged correctly.");
                        }
                        else
                        {
                            test.AddLog("Error, item not merged correctly. Expected: {0}, Actual: {1}", expectedMergedItem, localMergedItem);
                            testResult = false;
                        }
                    }
                }
                catch (MobileServicePushFailedException ex)
                {
                    test.AddLog("Push exception: {0}", ex);
                    if (autoResolve)
                    {
                        test.AddLog("Error, push should have succeeded.");
                        testResult = false;
                    }
                    else
                    {
                        test.AddLog("Expected exception was thrown.");
                    }
                }

                test.AddLog("Cleaning up");
                await localTable.DeleteAsync(item);
                test.AddLog("Local table cleaned up. Now sync'ing once more");
                await offlineReadyClient.SyncContext.PushAsync();
                test.AddLog("Done");

                return testResult;
            }, ZumoTestGlobals.RuntimeFeatureNames.STRING_ID_TABLES);
        }
    }
}