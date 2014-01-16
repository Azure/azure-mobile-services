using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Query;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Newtonsoft.Json.Linq;
using MockTable = System.Collections.Generic.Dictionary<string, Newtonsoft.Json.Linq.JObject>;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    class MobileServiceLocalStoreMock: IMobileServiceLocalStore
    {
        public readonly Dictionary<string, MockTable> Tables = new Dictionary<string, MockTable>();

        public List<MobileServiceTableQueryDescription> ReadQueries { get; private set; }
        public List<MobileServiceTableQueryDescription> DeleteQueries { get; private set; }

        public Queue<string> ReadResponses { get; private set; }

        public MobileServiceLocalStoreMock()
        {
            this.ReadQueries = new List<MobileServiceTableQueryDescription>();
            this.DeleteQueries = new List<MobileServiceTableQueryDescription>();
            this.ReadResponses = new Queue<string>();
        }

        public Func<MobileServiceTableQueryDescription, JToken> ReadAsyncFunc { get; set; }

        public Task InitializeAsync()
        {
            return Task.FromResult(0);
        }

        public Task<JToken> ReadAsync(MobileServiceTableQueryDescription query)
        {
            if (query.TableName == LocalSystemTables.OperationQueue || query.TableName == LocalSystemTables.SyncErrors)
            {
                // we don't query the queue specially, we just need all records
                return Task.FromResult<JToken>(new JArray(GetTable(query.TableName).Values.ToArray()));
            }

            this.ReadQueries.Add(query);
            JToken result;

            if (ReadAsyncFunc != null)
            {
                result = ReadAsyncFunc(query);
            }
            else
            {
                result = JToken.Parse(ReadResponses.Dequeue());
            }

            return Task.FromResult(result);
        }

        public Task DeleteAsync(MobileServiceTableQueryDescription query)
        {
            this.DeleteQueries.Add(query);
            this.Tables[query.TableName].Clear();
            return Task.FromResult(0);
        }

        public Task UpsertAsync(string tableName, JObject item)
        {
            MockTable table = GetTable(tableName);
            table[item.Value<string>("id")] = item;
            return Task.FromResult(0);
        }       

        public Task DeleteAsync(string tableName, string id)
        {
            MockTable table = GetTable(tableName);
            table.Remove(id);
            return Task.FromResult(0);
        }

        public Task<JObject> LookupAsync(string tableName, string id)
        {
            MockTable table = GetTable(tableName);
            JObject item;
            table.TryGetValue(id, out item);
            return Task.FromResult(item);
        }

        private Dictionary<string, JObject> GetTable(string tableName)
        {
            MockTable table;
            if (!this.Tables.TryGetValue(tableName, out table))
            {
                this.Tables[tableName] = table = new MockTable();
            }
            return table;
        } 

        public void Dispose()
        {            
        }
    }
}
