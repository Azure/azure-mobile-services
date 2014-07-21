// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Microsoft.WindowsAzure.MobileServices.Threading;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    /// <summary>
    /// Reads and writes settings in the __config table of <see cref="IMobileServiceLocalStore"/>
    /// </summary>
    internal class MobileServiceSyncSettingsManager: IDisposable
    {
        private AsyncLockDictionary cacheLock = new AsyncLockDictionary();
        private Dictionary<string, object> cache = new Dictionary<string, object>();
        private IMobileServiceLocalStore store;

        protected MobileServiceSyncSettingsManager()
        {
        }

        public MobileServiceSyncSettingsManager(IMobileServiceLocalStore store)
        {
            this.store = store;
        }

        public virtual async Task<MobileServiceSystemProperties> GetSystemProperties(string tableName)
        {
            string key = GetSystemPropertiesKey(tableName);
            int value = await GetSetting(key, (int)MobileServiceSystemProperties.Version);
            return (MobileServiceSystemProperties)value;
        }

        public virtual Task ResetDeltaToken(string tableName, string queryKey)
        {
            return this.store.DeleteAsync(MobileServiceLocalSystemTables.Config, GetDeltaTokenKey(tableName, queryKey));
        }        

        public virtual Task<DateTime> GetDeltaToken(string tableName, string queryKey)
        {
            return this.GetSetting(GetDeltaTokenKey(tableName, queryKey), DateTime.MinValue);
        }

        public virtual Task SetDeltaToken(string tableName, string queryKey, DateTime token)
        {
            return this.SetSetting(GetDeltaTokenKey(tableName, queryKey), token);
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

        private Task SetSetting(string key, object value)
        {
            return this.store.UpsertAsync(MobileServiceLocalSystemTables.Config, new JObject()
            {
                { MobileServiceSystemColumns.Id, key },
                { "value", value == null ? null : (string)Convert.ChangeType(value, typeof(string)) }
            }, fromServer: false);
        }

        private async Task<T> GetSetting<T>(string key, T defaultValue)
        {
            using (await this.cacheLock.Acquire(key, CancellationToken.None))
            {
                object value;
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
                        value = rawValue == null ? defaultValue : Convert.ChangeType(rawValue, typeof(T));
                    }
                    this.cache[key] = value;
                }
                return (T)value;
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
