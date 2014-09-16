// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Threading;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    /// <summary>
    /// Reads and writes settings in the __config table of <see cref="IMobileServiceLocalStore"/>
    /// </summary>
    internal class MobileServiceSyncSettingsManager : IDisposable
    {
        private const string DefaultDeltaToken = "1970-01-01T00:00:00.0000000+00:00";
        private AsyncLockDictionary cacheLock = new AsyncLockDictionary();
        private Dictionary<string, string> cache = new Dictionary<string, string>();
        private IMobileServiceLocalStore store;

        protected MobileServiceSyncSettingsManager()
        {
        }

        public MobileServiceSyncSettingsManager(IMobileServiceLocalStore store)
        {
            this.store = store;
        }

        public virtual async Task<MobileServiceSystemProperties> GetSystemPropertiesAsync(string tableName)
        {
            string key = GetSystemPropertiesKey(tableName);
            string value = await GetSettingAsync(key, ((int)MobileServiceSystemProperties.Version).ToString());
            return (MobileServiceSystemProperties)Int32.Parse(value);
        }

        public virtual Task ResetDeltaTokenAsync(string tableName, string queryKey)
        {
            return this.store.DeleteAsync(MobileServiceLocalSystemTables.Config, GetDeltaTokenKey(tableName, queryKey));
        }

        public async virtual Task<DateTimeOffset> GetDeltaTokenAsync(string tableName, string queryKey)
        {
            string value = await this.GetSettingAsync(GetDeltaTokenKey(tableName, queryKey), DefaultDeltaToken);
            return DateTimeOffset.Parse(value);
        }

        public virtual Task SetDeltaTokenAsync(string tableName, string queryKey, DateTimeOffset token)
        {
            return this.SetSettingAsync(GetDeltaTokenKey(tableName, queryKey), token.ToString("o"));
        }

        /// <summary>
        /// Defines the the table for storing config settings
        /// </summary>
        /// <param name="store">An instance of <see cref="IMobileServiceLocalStore"/></param>
        public static void DefineTable(MobileServiceLocalStore store)
        {
            store.DefineTable(MobileServiceLocalSystemTables.Config, new JObject()
            {
                { MobileServiceSystemColumns.Id, String.Empty },
                { "value", String.Empty },
            });
        }

        private async Task SetSettingAsync(string key, string value)
        {
            using (await this.cacheLock.Acquire(key, CancellationToken.None))
            {
                this.cache[key] = value;

                await this.store.UpsertAsync(MobileServiceLocalSystemTables.Config, new JObject()
                {
                    { MobileServiceSystemColumns.Id, key },
                    { "value", value }
                }, fromServer: false);
            }
        }


        private async Task<string> GetSettingAsync(string key, string defaultValue)
        {
            using (await this.cacheLock.Acquire(key, CancellationToken.None))
            {
                string value;
                if (!this.cache.TryGetValue(key, out value))
                {
                    JObject setting = await this.store.LookupAsync(MobileServiceLocalSystemTables.Config, key);
                    if (setting == null)
                    {
                        value = defaultValue;
                    }
                    else
                    {
                        string rawValue = setting.Value<string>("value");
                        value = rawValue ?? defaultValue;
                    }
                    this.cache[key] = value;
                }

                return value;
            }
        }

        private string GetDeltaTokenKey(string tableName, string queryKey)
        {
            return tableName + "_" + queryKey + "_deltaToken";
        }

        private static string GetSystemPropertiesKey(string tableName)
        {
            return tableName + "_systemProperties";
        }

        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            this.store.Dispose();
        }
    }
}
