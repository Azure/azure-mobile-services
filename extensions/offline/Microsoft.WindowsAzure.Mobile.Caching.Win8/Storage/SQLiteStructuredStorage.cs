using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SQLite;
using Windows.Storage;

namespace Microsoft.WindowsAzure.MobileServices.Caching
{
    public class SQLiteCacheStorage : IStructuredStorage
    {
        private string dbPath;
        private SQLiteConnection db;
        private SQLiteAsyncConnection dbAsync;

        private List<TableMapping.Column> defaultColumns = new List<TableMapping.Column>()
        {
            new TableMapping.Column("guid", typeof(Guid), true, true, int.MaxValue), // globally unique
            new TableMapping.Column("id", typeof(long), false, false, int.MaxValue), // server id
            new TableMapping.Column("timestamp", typeof(string), false, false, 16), // timestamp of local item
            new TableMapping.Column("status", typeof(ItemStatus), false, false, int.MaxValue), // status: unchanged:0 inserted:1 changed:2 deleted:3
        };

        public SQLiteCacheStorage()
        {
            dbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "cache.sqlite");
            dbAsync = new SQLiteAsyncConnection(dbPath);
            db = new SQLiteConnection(dbPath);
        }

        public async Task<IEnumerable<IDictionary<string, JToken>>> GetStoredData(string tableName, IQueryOptions query)
        {
            Debug.WriteLine("Retrieving data for table {0}, query: {1}", tableName, query.ToString());

            TableMapping mapping;
            try
            {
                IEnumerable<SQLiteConnection.ColumnInfo> columns = this.db.GetTableInfo(tableName);
                if (columns.Any())
                {
                    mapping = new TableMapping(tableName, columns.Select(ci => new TableMapping.Column(ci.Name, StringToClrType(ci.ColumnType), ci.pk != 0, false, int.MaxValue)));
                }
                else
                {
                    mapping = new TableMapping(tableName, this.defaultColumns);
                }
            }
            catch
            {
                mapping = new TableMapping(tableName, this.defaultColumns);
            }
            Debug.WriteLine("Table {0}, using mapping: {1}", tableName, string.Join<TableMapping.Column>(",",mapping.Columns));

            await Task.Run(() => db.CreateTable(mapping));

            SQLiteExpressionVisitor visitor = new SQLiteExpressionVisitor();
            visitor.VisitQueryOptions(query);

            Debug.WriteLine("Executing sql: {0}", visitor.SqlStatement);

            SQLiteCommand command = this.db.CreateCommand(string.Format(visitor.SqlStatement, tableName));
            return await Task.Run(() => command.ExecuteQuery(mapping));
        }

        public async Task StoreData(string tableName, IEnumerable<IDictionary<string, JToken>> data)
        {
            Debug.WriteLine("Storing data for table {0}", tableName);

            if (!data.Any())
            {
                return;
            }

            // there will always be one item, since otherwise we would already have returned
            TableMapping mapping = this.DeriveSchema(tableName, data.FirstOrDefault());

            await Task.Run(() => db.CreateTable(mapping));

            Debug.WriteLine("Table {0}, using mapping: {1}", tableName, string.Join<TableMapping.Column>(",", mapping.Columns));

            StringBuilder sql = new StringBuilder();
            sql.Append("INSERT OR REPLACE INTO ");
            sql.Append(tableName);
            sql.Append(string.Format(" ('{0}')", string.Join("','", mapping.Columns.Select(c => c.Name))));
            sql.Append(" VALUES ");
            foreach (var item in data)
            {
                sql.Append("('");
                foreach (var col in mapping.Columns)
                {
                    JValue val = item[col.Name] as JValue;
                    if (val == null)
                    {
                        sql.Append("NULL");
                    }
                    else
                    {
                        sql.Append(col.IsPK ? val.Value.ToString().ToLowerInvariant() : val.Value);
                    }
                    sql.Append("','");
                }
                sql.Remove(sql.Length - 3, 3);
                sql.Append("'),");
            }
            sql.Remove(sql.Length - 1, 1);

            Debug.WriteLine("Executing sql: {0}", sql.ToString());

            SQLiteCommand command = db.CreateCommand(sql.ToString(), tableName);
            await Task.Run(() => command.ExecuteNonQuery());
        }

        public async Task RemoveStoredData(string tableName, IEnumerable<string> guids)
        {
            Debug.WriteLine(string.Format("Removing data for table {0}, guids: {1}", tableName, string.Join(",", guids)));

            if (!guids.Any())
            {
                return;
            }

            StringBuilder sql = new StringBuilder();
            sql.Append("DELETE FROM ");
            sql.Append(tableName);
            sql.Append(" WHERE guid IN ('");
            foreach (string guid in guids)
            {
                sql.Append(guid.ToLowerInvariant());
                sql.Append("','");
            }
            sql.Remove(sql.Length - 3, 3);
            sql.Append("')");

            Debug.WriteLine("Executing sql: {0}", sql.ToString());

            SQLiteCommand command = db.CreateCommand(sql.ToString());
            await Task.Run(() => command.ExecuteNonQuery());
        }

        private TableMapping DeriveSchema(string tableName, IDictionary<string, JToken> item)
        {
            List<TableMapping.Column> cols = new List<TableMapping.Column>();
            foreach (var kvp in item.Where(prop => !(prop.Key.Equals("status") || prop.Key.Equals("id") || prop.Key.Equals("guid") || prop.Key.Equals("timestamp"))))
            {
                cols.Add(new TableMapping.Column(kvp.Key, JTokenToClrType(kvp.Value), false, false, int.MaxValue));
            }
            cols.AddRange(this.defaultColumns);

            return new TableMapping(tableName, cols);
        }

        private Type JTokenToClrType(JToken token)
        {
            JValue val = token as JValue;
            switch (val.Type)
            {
                case JTokenType.String:
                    return typeof(string);
                case JTokenType.Date:
                    return typeof(DateTime);
                case JTokenType.Float:
                case JTokenType.Integer:
                    return typeof(double);
                case JTokenType.Boolean:
                    return typeof(bool);
                default:
                    Debug.WriteLine("Encountered token type {0}.", val.Type);
                    return typeof(string);
            }
        }

        private Type StringToClrType(string typeString)
        {
            switch (typeString)
            {
                case "datetime":
                    return typeof(DateTime);
                case "float":
                    return typeof(double);
                case "integer":
                    return typeof(int);
                case "bigint":
                    return typeof(long);
                case "blob":
                    return typeof(byte[]);
                default:
                    Debug.WriteLine("Encountered type string {0}.", typeString);
                    return typeof(string);
            }
        }
    }
}
