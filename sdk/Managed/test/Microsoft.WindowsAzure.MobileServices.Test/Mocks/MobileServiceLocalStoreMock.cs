using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Query;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Newtonsoft.Json.Linq;
using MockTable = System.Collections.Generic.Dictionary<string, Newtonsoft.Json.Linq.JObject>;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    class MobileServiceLocalStoreMock : IMobileServiceLocalStore
    {
        public readonly Dictionary<string, MockTable> TableMap = new Dictionary<string, MockTable>();

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
            string odata = query.ToODataString();

            MockTable table = GetTable(query.TableName);
            IEnumerable<JObject> items = table.Values;

            if (IsLookup(odata))
            {
                JObject result;
                if (table.TryGetValue(GetLookupId(query), out result))
                {
                    items = new[] { result };
                }
                else
                {
                    items = new JObject[] { };
                }
                return GetMockResult(query, items);
            }
            else if (query.TableName == MobileServiceLocalSystemTables.OperationQueue ||
                query.TableName == MobileServiceLocalSystemTables.SyncErrors)
            {

                if (query.TableName == MobileServiceLocalSystemTables.OperationQueue)
                {
                    if (odata.Contains("$orderby=sequence desc")) // the query to take total count and max sequence
                    {
                        items = items.OrderBy(o => o.Value<long>("sequence"));
                    }
                    else if (odata.StartsWith("$filter=((tableKind eq ") && odata.Contains("(sequence gt "))
                    {
                        var sequenceCompareNode = ((BinaryOperatorNode)query.Filter).RightOperand as BinaryOperatorNode;

                        items = items.Where(o => o.Value<long>("sequence") > (long)((ConstantNode)sequenceCompareNode.RightOperand).Value);
                        items = items.OrderBy(o => o.Value<long>("sequence"));
                    }
                    else if (odata.Contains("(sequence gt ")) // the query to get next operation
                    {
                        items = items.Where(o => o.Value<long>("sequence") > (long)((ConstantNode)((BinaryOperatorNode)query.Filter).RightOperand).Value);
                        items = items.OrderBy(o => o.Value<long>("sequence"));
                    }
                    else if (odata.Contains(") and (itemId eq '")) // the query to retrive operation by item id
                    {
                        string targetTable = ((ConstantNode)((BinaryOperatorNode)((BinaryOperatorNode)query.Filter).LeftOperand).RightOperand).Value.ToString();
                        string targetId = ((ConstantNode)((BinaryOperatorNode)((BinaryOperatorNode)query.Filter).RightOperand).RightOperand).Value.ToString();
                        items = items.Where(o => o.Value<string>("itemId") == targetId && o.Value<string>("tableName") == targetTable);
                    }
                    else if (odata.Contains("$filter=(tableName eq '"))
                    {
                        items = items.Where(o => o.Value<string>("tableName") == ((ConstantNode)((BinaryOperatorNode)query.Filter).RightOperand).Value.ToString());
                    }
                }

                return GetMockResult(query, items);
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

        private static string GetLookupId(MobileServiceTableQueryDescription query)
        {
            return (string)((query.Filter as BinaryOperatorNode).RightOperand as ConstantNode).Value;
        }

        private static bool IsLookup(string odata)
        {
            return odata.StartsWith("$filter=(id eq '") && odata.EndsWith("')");
        }

        private static Task<JToken> GetMockResult(MobileServiceTableQueryDescription query, IEnumerable<JObject> items)
        {
            if (query.IncludeTotalCount)
            {
                return Task.FromResult<JToken>(new JObject() { { "count", items.Count() }, { "results", new JArray(items) } });
            }

            return Task.FromResult<JToken>(new JArray(items));
        }

        public Task DeleteAsync(MobileServiceTableQueryDescription query)
        {
            this.DeleteQueries.Add(query);
            if (query.Filter == null)
            {
                this.TableMap[query.TableName].Clear();
            }
            else
            {
                string odata = query.ToODataString();

                MockTable table = GetTable(query.TableName);
                IEnumerable<JObject> items = table.Values.ToList();
                if (IsLookup(odata))
                {
                    table.Remove(GetLookupId(query));
                }
                else
                {
                    foreach (JObject item in items)
                    {
                        table.Remove((string)item[MobileServiceSystemColumns.Id]);
                    }
                }
            }
            return Task.FromResult(0);
        }

        public Task UpsertAsync(string tableName, IEnumerable<JObject> items, bool fromServer)
        {
            foreach (JObject item in items)
            {
                MockTable table = GetTable(tableName);
                table[item.Value<string>("id")] = item;
            }
            return Task.FromResult(0);
        }

        private Dictionary<string, JObject> GetTable(string tableName)
        {
            MockTable table;
            if (!this.TableMap.TryGetValue(tableName, out table))
            {
                this.TableMap[tableName] = table = new MockTable();
            }
            return table;
        }

        public void Dispose()
        {
        }
    }
}
