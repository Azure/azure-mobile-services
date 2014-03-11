// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Query;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Newtonsoft.Json.Linq;
using SQLitePCL;

namespace Microsoft.WindowsAzure.MobileServices.SQLiteStore
{
    public class MobileServiceSQLiteStore: IMobileServiceLocalStore
    {
        private Dictionary<string, TableDefinition> tables = new Dictionary<string, TableDefinition>();

        private SQLiteConnection connection;

        protected MobileServiceSQLiteStore() { }

        public MobileServiceSQLiteStore(string fileName)
        {
            this.connection = new SQLiteConnection(fileName);

            this.DefineTable(LocalSystemTables.OperationQueue, new JObject()
            {
                { SystemProperties.Id, String.Empty },
                { "kind", 0 },
                { "tableName", String.Empty },
                { "item", String.Empty },
                { "itemId", String.Empty },
                { "__createdAt", DateTime.Now },
                { "sequence", 0 }
            });
            this.DefineTable(LocalSystemTables.SyncErrors, new JObject()
            {
                { SystemProperties.Id, String.Empty },
                { "status", 0 },
                { "operationKind", 0 },
                { "tableName", String.Empty },
                { "item", String.Empty },
                { "rawResult", String.Empty }
            });
        }

        public void DefineTable(string tableName, JObject item)
        {
            // add id if it is not defined
            if (item[SystemProperties.Id] == null)
            {
                item[SystemProperties.Id] = String.Empty;
            }

            // add version if it is not defined
            if (item[SystemProperties.Version] == null)
            {
                item[SystemProperties.Version] = String.Empty;
            }

            var tableDefinition = (from property in item.Properties()
                                   let columnType = SqlHelpers.GetColumnType(property.Value.Type, allowNull: false)
                                   select new ColumnDefinition(columnType, property))
                                  .ToDictionary(p => p.Definition.Name, StringComparer.OrdinalIgnoreCase);

            this.tables.Add(tableName, new TableDefinition(tableDefinition));
        }

        public Task InitializeAsync()
        {
            foreach (KeyValuePair<string, TableDefinition> table in this.tables)
            {
                this.CreateTableFromObject(table.Key, table.Value.Values.Select(v => v));
            }

            return Task.FromResult(0);
        }

        public Task<JToken> ReadAsync(MobileServiceTableQueryDescription query)
        {
            var formatter = new SqlFormatter(query);
            string sql = formatter.FormatSelect();


            IList<JObject> rows = this.ExecuteQuery(query.TableName, sql, formatter.Parameters);
            JToken result = new JArray(rows.ToArray());

            if (query.IncludeTotalCount)
            {
                sql = formatter.FormatSelectCount();
                IList<JObject> countRows = this.ExecuteQuery(query.TableName, sql, formatter.Parameters);
                long count = countRows[0].Value<long>("count");
                result = new JObject() 
                { 
                    { "results", result },
                    { "count", count}
                };
            }

            return Task.FromResult(result);
        }

        public Task UpsertAsync(string tableName, JObject item)
        {
            TableDefinition table = GetTable(tableName);

            var sql = new StringBuilder();
            sql.AppendFormat("INSERT OR REPLACE INTO {0} (", SqlHelpers.FormatTableName(tableName));
            var columns = item.Properties().ToList();

            string columnNames = String.Join(", ", columns.Select(c => SqlHelpers.FormatMember(c.Name)));
            sql.Append(columnNames);
            
            sql.AppendFormat(") VALUES (");
            
            string separator = String.Empty;
            ColumnDefinition columnDefinition;

            var parameters = new Dictionary<string, object>();

            foreach (JProperty column in columns)
            {
                if (!table.TryGetValue(column.Name, out columnDefinition))
                {
                    throw new InvalidOperationException(string.Format(Properties.Resources.SQLiteStore_ColumnNotDefined, column.Name, tableName));
                }

                object value = SqlHelpers.SerializeValue(column.Value.Value<JValue>(), columnDefinition.SqlType);
                string paramName = "@p" + (parameters.Count + 1);
                parameters.Add(paramName, value);
                sql.AppendFormat("{0}{1}", separator, paramName);
                separator = ", ";
            }
            sql.Append(")");
            this.ExecuteNonQuery(sql.ToString(), parameters);
            return Task.FromResult(0);
        }        

        public Task DeleteAsync(MobileServiceTableQueryDescription query)
        {
            var formatter = new SqlFormatter(query);
            string sql = formatter.FormatDelete();

            this.ExecuteNonQuery(sql, formatter.Parameters);

            return Task.FromResult(0);
        }

        public Task DeleteAsync(string tableName, string id)
        {
            string sql = string.Format("DELETE FROM {0} WHERE {1} = @id", SqlHelpers.FormatTableName(tableName), SystemProperties.Id);

            var parameters = new Dictionary<string, object>
            {
                {"@id", id}
            };

            this.ExecuteNonQuery(sql, parameters);
            return Task.FromResult(0);
        }

        public Task<JObject> LookupAsync(string tableName, string id)
        {
            string sql = string.Format("SELECT * FROM {0} WHERE {1} = @id", SqlHelpers.FormatTableName(tableName), SystemProperties.Id);
            var parameters = new Dictionary<string, object>
            {
                {"@id", id}
            };

            IList<JObject> results = this.ExecuteQuery(tableName, sql, parameters);

            return Task.FromResult(results.FirstOrDefault());
        }

        private IList<JObject> ExecuteQuery(string tableName, string sql, IDictionary<string, object> parameters = null)
        {
            TableDefinition table = GetTable(tableName);
            return this.ExecuteQuery(table, sql, parameters);
        }

        private TableDefinition GetTable(string tableName)
        {
            TableDefinition table;
            if (!this.tables.TryGetValue(tableName, out table))
            {
                throw new InvalidOperationException(string.Format(Properties.Resources.SQLiteStore_TableNotDefined, tableName));
            }
            return table;
        }

        internal virtual void CreateTableFromObject(string tableName, IEnumerable<ColumnDefinition> columns)
        {
            String tblSql = string.Format("CREATE TABLE IF NOT EXISTS {0} ({1} PRIMARY KEY)", SqlHelpers.FormatTableName(tableName), SqlHelpers.FormatMember(SystemProperties.Id));
            this.ExecuteNonQuery(tblSql);

            string infoSql = string.Format("PRAGMA table_info({0});", SqlHelpers.FormatTableName(tableName));
            IDictionary<string, JObject> existingColumns = this.ExecuteQuery((TableDefinition)null, infoSql)
                                                               .ToDictionary(c => c.Value<string>("name"), StringComparer.OrdinalIgnoreCase);

            var columnsToCreate = from c in columns // new column name
                                  where !existingColumns.ContainsKey(c.Definition.Name) // that does not exist in existing columns
                                  select c;

            foreach (ColumnDefinition column in columnsToCreate)
            {
                string createSql = string.Format("ALTER TABLE {0} ADD COLUMN {1} {2}",
                                                 SqlHelpers.FormatTableName(tableName),
                                                 SqlHelpers.FormatMember(column.Definition.Name),
                                                 column.SqlType);
                this.ExecuteNonQuery(createSql);
            }

            // NOTE: In SQLite you cannot drop columns, only add them.
        }

        private void ExecuteNonQuery(string sql, IDictionary<string, object> parameters = null)
        {
            parameters = parameters ?? new Dictionary<string, object>();

            using (ISQLiteStatement statement = this.connection.Prepare(sql))
            {
                foreach (KeyValuePair<string, object> parameter in parameters)
                {
                    statement.Bind(parameter.Key, parameter.Value);
                }

                SQLiteResult result = statement.Step();
                ValidateResult(result);
            }
        }

        private IList<JObject> ExecuteQuery(TableDefinition table, string sql, IDictionary<string, object> parameters = null)
        {
            table = table ?? new TableDefinition();
            parameters = parameters ?? new Dictionary<string, object>();

            var rows = new List<JObject>();
            using (ISQLiteStatement statement = this.connection.Prepare(sql))
            {
                foreach (KeyValuePair<string, object> parameter in parameters)
                {
                    statement.Bind(parameter.Key, parameter.Value);
                }

                SQLiteResult result;
                while ((result = statement.Step()) == SQLiteResult.ROW)
                {
                    var row = ReadRow(table, statement);
                    rows.Add(row);
                }

                ValidateResult(result);
            }

            return rows;
        }

        private object DeserializeValue(ColumnDefinition column, object value)
        {
            if (value == null)
            {
                return null;
            }

            string sqlType = column.SqlType;
            JTokenType jsonType = column.Definition.Value.Type;
            if (sqlType == SqlColumnType.Integer)
            {
                return SqlHelpers.ParseInteger(jsonType, value);
            }
            if (sqlType == SqlColumnType.Real)
            {
                return SqlHelpers.ParseReal(jsonType, value);
            }
            if (sqlType == SqlColumnType.Text)
            {
                return SqlHelpers.ParseText(jsonType, value);
            }
            return value;
        }

        private static void ValidateResult(SQLiteResult result)
        {
            if (result != SQLiteResult.DONE)
            {
                throw new SQLiteException(Properties.Resources.SQLiteStore_QueryExecutionFailed);
            }
        }

        private JObject ReadRow(TableDefinition table, ISQLiteStatement statement)
        {
            var row = new JObject();
            int i = 0;
            string name = statement.ColumnName(i);
            while (name != null)
            {
                object value = statement[i];

                ColumnDefinition column;
                if (table.TryGetValue(name, out column))
                {
                    value = this.DeserializeValue(column, value);
                }
                row[name] = new JValue(value);

                name = statement.ColumnName(++i);
            }
            return row;
        }          

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.connection.Dispose();
            }
        }        
    }
}
