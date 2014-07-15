using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Microsoft.WindowsAzure.MobileServices.TestFramework;

namespace Microsoft.WindowsAzure.MobileServices.SQLiteStore.Test.UnitTests
{
    public class SyncSettingsManagerTests : TestBase
    {
        public static string TestDbName = SQLiteStoreTests.TestDbName;
        private const string TestTable = "todoItem";
        private const string TestQueryKey = "abc";


        [AsyncTestMethod]
        public async Task GetDeltaTokenAsync_ReturnsMinValue_WhenTokenDoesNotExist()
        {
            MobileServiceSyncSettingsManager settings = await GetSettingManager();

            DateTimeOffset token = await settings.GetDeltaTokenAsync(TestTable, TestQueryKey);

            Assert.AreEqual(token, DateTimeOffset.MinValue.ToUniversalTime());
        }

        [AsyncTestMethod]
        public async Task SetDeltaTokenAsync_SavesTheSetting_AsUTCDate()
        {
            MobileServiceSyncSettingsManager settings = await GetSettingManager();

            DateTimeOffset saved = new DateTime(2014, 7, 24, 3, 4, 5, DateTimeKind.Local);
            await settings.SetDeltaTokenAsync(TestTable, TestQueryKey, saved);

            // with cache
            DateTimeOffset read = await settings.GetDeltaTokenAsync(TestTable, TestQueryKey);
            Assert.AreEqual(read, saved.ToUniversalTime());

            // without cache
            settings = await GetSettingManager(resetDb: false);
            read = await settings.GetDeltaTokenAsync(TestTable, TestQueryKey);
            Assert.AreEqual(read, saved.ToUniversalTime());
        }

        [AsyncTestMethod]
        public async Task SetDeltaTokenAsync_SavesTheSetting()
        {
            MobileServiceSyncSettingsManager settings = await GetSettingManager();

            var saved = new DateTimeOffset(2014, 7, 24, 3, 4, 5, TimeSpan.Zero);
            await settings.SetDeltaTokenAsync(TestTable, TestQueryKey, saved);

            // with cache
            DateTimeOffset read = await settings.GetDeltaTokenAsync(TestTable, TestQueryKey);
            Assert.AreEqual(read, saved);

            // without cache
            settings = await GetSettingManager(resetDb: false);
            read = await settings.GetDeltaTokenAsync(TestTable, TestQueryKey);
            Assert.AreEqual(read, saved);
        }

        [AsyncTestMethod]
        public async Task SetDeltaTokenAsync_UpdatesCacheAndDatabase()
        {
            MobileServiceSyncSettingsManager settings = await GetSettingManager();

            // first save
            var saved = new DateTimeOffset(2014, 7, 24, 3, 4, 5, TimeSpan.Zero);
            await settings.SetDeltaTokenAsync(TestTable, TestQueryKey, saved);

            // then read and update
            DateTimeOffset read = await settings.GetDeltaTokenAsync(TestTable, TestQueryKey);
            await settings.SetDeltaTokenAsync(TestTable, TestQueryKey, read.AddDays(1));

            // then read again
            read = await settings.GetDeltaTokenAsync(TestTable, TestQueryKey);
            Assert.AreEqual(read, saved.AddDays(1));

            // then read again in fresh instance
            settings = await GetSettingManager(resetDb: false);
            read = await settings.GetDeltaTokenAsync(TestTable, TestQueryKey);
            Assert.AreEqual(read, saved.AddDays(1));
        }

        private static async Task<MobileServiceSyncSettingsManager> GetSettingManager(bool resetDb = true)
        {
            if (resetDb)
            {
                TestUtilities.ResetDatabase(TestDbName);
            }

            var store = new MobileServiceSQLiteStore(TestDbName);
            await store.InitializeAsync();

            var settings = new MobileServiceSyncSettingsManager(store);
            return settings;
        }
    }
}
