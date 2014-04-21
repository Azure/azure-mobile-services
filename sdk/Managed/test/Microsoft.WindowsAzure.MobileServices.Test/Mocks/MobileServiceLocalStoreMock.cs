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
            if (query.TableName == MobileServiceLocalSystemTables.OperationQueue || query.TableName == MobileServiceLocalSystemTables.SyncErrors)
            {
                MockTable table = GetTable(query.TableName);

                IEnumerable<JObject> items = table.Values;
                if (query.TableName == MobileServiceLocalSystemTables.OperationQueue)
                {
                    string odata = query.ToQueryString();
                    if ( odata.Contains("$orderby=__createdAt,sequence"))
                    {
                        // must be the query to read all operations in order or peek the top item
                        items = items.OrderBy(o => o.Value<DateTime>("__createdAt"))
                                     .ThenBy(o => o.Value<long>("sequence"));
                    }
                    else if (odata.Contains("$filter=(itemId eq '"))
                    {
                        items = items.Where(o => o.Value<string>("itemId") == ((ConstantNode)((BinaryOperatorNode)query.Filter).RightOperand).Value.ToString());
                    }
                    else if (odata.Contains("$filter=(tableName eq '"))
                    {
                        items = items.Where(o => o.Value<string>("tableName") == ((ConstantNode)((BinaryOperatorNode)query.Filter).RightOperand).Value.ToString());
                    }
                }

                if (query.IncludeTotalCount)
                {
                    return Task.FromResult<JToken>(new JObject() { { "count", items.Count() }, { "results", new JArray(items) } });
                }

                return Task.FromResult<JToken>(new JArray(items));
            }

            this.ReadQueries.Add(query);
            JToken response;

            if (ReadAsyncFunc != null)
            {
                response = ReadAsyncFunc(query);
            }
            else
            {
                response = JToken.Parse(ReadResponses.Dequeue());
            }

            return Task.FromResult(response);
        }

        public Task DeleteAsync(MobileServiceTableQueryDescription query)
        {
            this.DeleteQueries.Add(query);
            this.Tables[query.TableName].Clear();
            return Task.FromResult(0);
        }

        public Task UpsertAsync(string tableName, JObject item, bool fromServer)
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
