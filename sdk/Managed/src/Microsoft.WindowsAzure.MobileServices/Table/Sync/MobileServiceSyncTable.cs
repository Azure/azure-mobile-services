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

        public MobileServiceClient MobileServiceClient { get; private set; }

        public string TableName { get; private set; }

        public MobileServiceSyncTable(string tableName, MobileServiceClient client)
        {
            Debug.Assert(tableName != null);
            Debug.Assert(client != null);

            this.MobileServiceClient = client;
            this.TableName = tableName;
            this.syncContext = (MobileServiceSyncContext)client.SyncContext;
        }

        public Task<JToken> ReadAsync(string query)
        {
            return this.syncContext.ReadAsync(this.TableName, query);
        }

        public Task PullAsync(string query, IDictionary<string, string> parameters, CancellationToken cancellationToken)
        {
            return this.syncContext.PullAsync(this.TableName, query, parameters, cancellationToken);
        }

        public Task PurgeAsync(string query, CancellationToken cancellationToken)
        {
            return this.syncContext.PurgeAsync(this.TableName, query, cancellationToken);
        }

        public async Task<JObject> InsertAsync(JObject instance)
        {
            object id = MobileServiceSerializer.GetId(instance, ignoreCase: false, allowDefault: true);
            if (id == null)
            {
                id = Guid.NewGuid().ToString();
                instance = (JObject)instance.DeepClone();
                instance[MobileServiceSystemColumns.Id] = (string)id;
            }
            else
            {
                EnsureIdIsString(id);
            }

            await this.syncContext.InsertAsync(this.TableName, (string)id, instance);

            return instance;
        }        

        public async Task UpdateAsync(JObject instance)
        {
            string id = EnsureIdIsString(instance);
            instance = RemoveSystemPropertiesKeepVersion(instance);

            await this.syncContext.UpdateAsync(this.TableName, id, instance);
        }        

        public async Task DeleteAsync(JObject instance)
        {
            string id = EnsureIdIsString(instance);
            instance = RemoveSystemPropertiesKeepVersion(instance);

            await this.syncContext.DeleteAsync(this.TableName, id, instance);
        }

        public Task<JObject> LookupAsync(string id)
        {
            return this.syncContext.LookupAsync(this.TableName, id);
        }

        // we want to keep version as it rides on the object until the sync operation happens using classic table.
        internal static JObject RemoveSystemPropertiesKeepVersion(JObject instance)
        {
            string version;
            instance = MobileServiceSerializer.RemoveSystemProperties(instance, out version);
            if (version != null)
            {
                instance[MobileServiceSystemColumns.Version] = version;
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
                        MobileServiceSystemColumns.Id),
                     "instance");
            }
            return strId;
        }
    }
}
