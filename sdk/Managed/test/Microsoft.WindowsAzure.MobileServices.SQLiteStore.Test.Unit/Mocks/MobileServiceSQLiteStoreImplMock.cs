using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.SQLiteStore.Test.Unit.Mocks
{
    class MobileServiceSQLiteStoreImplMock: IMobileServiceSQLiteStoreImpl
    {
        public Action<string, IEnumerable<JProperty>> CreateTableFromObjectFunc { get; set; }
        public Action<string, IDictionary<string, object>> ExecuteNonQueryFunc { get; set; }
        public Func<TableDefinition, string, IDictionary<string, object>, IList<JObject>> ExecuteQueryFunc { get; set; }
        public Func<ColumnDefinition, object, object> DeserializeValueFunc { get; set; }

        public Task CreateTableFromObject(string tableName, IEnumerable<JProperty> columns)
        {
            this.CreateTableFromObjectFunc(tableName, columns);
            return Task.FromResult(0);
        }

        public void ExecuteNonQuery(string sql, IDictionary<string, object> parameters = null)
        {
            this.ExecuteNonQueryFunc(sql, parameters);
        }

        public IList<JObject> ExecuteQuery(TableDefinition table, string sql, IDictionary<string, object> parameters = null)
        {
            return this.ExecuteQueryFunc(table, sql, parameters);
        }

        public object DeserializeValue(ColumnDefinition column, object value)
        {
            return this.DeserializeValueFunc(column, value);
        }

        public void Dispose()
        {
        }
    }
}
