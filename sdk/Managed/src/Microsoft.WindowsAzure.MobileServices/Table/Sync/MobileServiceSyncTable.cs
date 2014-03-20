// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    internal class MobileServiceSyncTable: IMobileServiceSyncTable
    {
        private MobileServiceSyncContext syncContext;
        private MobileServiceTable remoteTable;

        public MobileServiceClient MobileServiceClient { get; private set; }
        
        public string TableName
        {
            get { return this.remoteTable.TableName; }
        }

        public MobileServiceSyncTable(string tableName, MobileServiceClient client)
        {
            Debug.Assert(tableName != null);
            Debug.Assert(client != null);

            this.MobileServiceClient = client;
            this.syncContext = (MobileServiceSyncContext)client.SyncContext;
            this.remoteTable = client.GetTable(tableName) as MobileServiceTable;
            this.remoteTable.SystemProperties = MobileServiceSystemProperties.Version;
        }

        public Task<JToken> ReadAsync(string query)
        {
            return this.syncContext.ReadAsync(this.remoteTable, query);
        }

        public Task PullAsync(string query, CancellationToken cancellationToken)
        {
            return this.syncContext.PullAsync(this.remoteTable, query, cancellationToken);
        }

        public Task PurgeAsync(string query, CancellationToken cancellationToken)
        {
            return this.syncContext.PurgeAsync(this.remoteTable, query, cancellationToken);
        }

        public async Task<JObject> InsertAsync(JObject instance)
        {
            object id = MobileServiceSerializer.GetId(instance, ignoreCase: false, allowDefault: true);
            if (id == null)
            {
                id = Guid.NewGuid().ToString();
                instance = (JObject)instance.DeepClone();
                instance[MobileServiceSerializer.IdPropertyName] = (string)id;
            }
            else
            {
                EnsureIdIsString(id);
            }

            await this.syncContext.InsertAsync(this.remoteTable, (string)id, instance);

            return instance;
        }        

        public async Task UpdateAsync(JObject instance)
        {
            string id = EnsureIdIsString(instance);
            instance = RemoveSystemPropertiesKeepVersion(instance);

            await this.syncContext.UpdateAsync(this.remoteTable, id, instance);
        }        

        public async Task DeleteAsync(JObject instance)
        {
            string id = EnsureIdIsString(instance);
            instance = RemoveSystemPropertiesKeepVersion(instance);

            await this.syncContext.DeleteAsync(this.remoteTable, id, instance);
        }

        public Task<JObject> LookupAsync(string id)
        {
            return this.syncContext.LookupAsync(this.remoteTable, id);
        }

        // we want to keep version as it rides on the object until the sync operation happens using classic table.
        internal static JObject RemoveSystemPropertiesKeepVersion(JObject instance)
        {
            string version;
            instance = MobileServiceSerializer.RemoveSystemProperties(instance, out version);
            if (version != null)
            {
                instance[MobileServiceSerializer.VersionSystemPropertyString] = version;
            }
            return instance;
        }

        // we don't support int id tables for offline. therefore id must be of type string
        private static string EnsureIdIsString(JObject instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            object id = MobileServiceSerializer.GetId(instance, ignoreCase: false, allowDefault: false);
            return EnsureIdIsString(id);
        }

        protected static string EnsureIdIsString(object id)
        {
            string strId = id as string;
            if (strId == null)
            {
                throw new ArgumentException(
                     string.Format(
                        CultureInfo.InvariantCulture,
                        Resources.MobileServiceSyncTable_IdMustBeString,
                        MobileServiceSerializer.IdPropertyName),
                     "instance");
            }
            return strId;
        }
    }
}
