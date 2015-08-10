using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Sync;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    internal class LocalStoreProxy : IMobileServiceLocalStore
    {
        private readonly IMobileServiceLocalStore store;

        public LocalStoreProxy(IMobileServiceLocalStore store)
        {
            this.store = store;
        }
        public Task InitializeAsync()
        {
            throw new NotSupportedException();
        }

        public Task<Newtonsoft.Json.Linq.JToken> ReadAsync(Query.MobileServiceTableQueryDescription query)
        {
            return this.store.ReadAsync(query);
        }

        public Task UpsertAsync(string tableName, IEnumerable<Newtonsoft.Json.Linq.JObject> items, bool ignoreMissingColumns)
        {
            return this.store.UpsertAsync(tableName, items, ignoreMissingColumns);
        }

        public Task DeleteAsync(Query.MobileServiceTableQueryDescription query)
        {
            return this.store.DeleteAsync(query);
        }

        public Task DeleteAsync(string tableName, IEnumerable<string> ids)
        {
            return this.store.DeleteAsync(tableName, ids);
        }

        public Task<Newtonsoft.Json.Linq.JObject> LookupAsync(string tableName, string id)
        {
            return this.store.LookupAsync(tableName, id);
        }

        public void Dispose()
        {
            // no op
        }
    }
}
