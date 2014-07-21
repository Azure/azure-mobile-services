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
    /// <summary>
    /// SQLite based implementation of <see cref="IMobileServiceLocalStore"/>
    /// </summary>
    public class MobileServiceSQLiteStore: MobileServiceLocalStore
    {
        private Dictionary<string, TableDefinition> tables = new Dictionary<string, TableDefinition>(StringComparer.OrdinalIgnoreCase);
        private SQLiteConnection connection;

        protected MobileServiceSQLiteStore() { }

        /// <summary>
        /// Initializes a new instance of <see cref="MobileServiceSQLiteStore"/>
        /// </summary>
        /// <param name="fileName">Name of the local SQLite database file.</param>
        public MobileServiceSQLiteStore(string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }

            this.connection = new SQLiteConnection(fileName);            
        }

        /// <summary>
        /// Defines the local table on the store.
        /// </summary>
        /// <param name="tableName">Name of the local table.</param>
        /// <param name="item">An object that represents the structure of the table.</param>
        public override void DefineTable(string tableName, JObject item)
        {
            if (tableName == null)
            {
                throw new ArgumentNullException("tableName");
            }
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (this.Initialized)
            {
                throw new InvalidOperationException(Properties.Resources.SQLiteStore_DefineAfterInitialize);
            }

            // add id if it is not defined
            JToken ignored;
            if (!item.TryGetValue(MobileServiceSystemColumns.Id, StringComparison.OrdinalIgnoreCase, out ignored))
            {
                item[MobileServiceSystemColumns.Id] = String.Empty;
            }

            var tableDefinition = (from property in item.Properties()
                                   let columnType = SqlHelpers.GetColumnType(property.Value.Type, allowNull: false)
                                   select new ColumnDefinition(columnType, property))
                                  .ToDictionary(p => p.Property.Name, StringComparer.OrdinalIgnoreCase);

            var sysProperties = MobileServiceSystemProperties.None;

            if (item[MobileServiceSystemColumns.Version] != null)
            {
                sysProperties = sysProperties | MobileServiceSystemProperties.Version;
            }
            if (item[MobileServiceSystemColumns.CreatedAt] != null)
            {
                sysProperties = sysProperties | MobileServiceSystemProperties.CreatedAt;
            }
            if (item[MobileServiceSystemColumns.UpdatedAt] != null)
            {
                sysProperties = sysProperties | MobileServiceSystemProperties.UpdatedAt;
            }

            this.tables.Add(tableName, new TableDefinition(tableDefinition, sysProperties));
        }

        protected override async Task OnInitialize()
        {
            foreach (KeyValuePair<string, TableDefinition> table in this.tables)
            {
                this.CreateTableFromObject(table.Key, table.Value.Values);

                if (!MobileServiceLocalSystemTables.All.Contains(table.Key))
                {
                    // preserve system properties setting for non-system tables
                    string name = String.Format("{0}_systemProperties", table.Key);
                    string value = ((int)table.Value.SystemProperties).ToString();
                    await this.SaveSetting(name, value);
                }
            }
        }

        /// <summary>
        /// Reads data from local store by executing the query.
        /// </summary>
        /// <param name="query">The query to execute on local store.</param>
        /// <returns>A task that will return with results when the query finishes.</returns>
        public override Task<JToken> ReadAsync(MobileServiceTableQueryDescription query)
        {
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }

            this.EnsureInitialized();

            var formatter = new SqlQueryFormatter(query);
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

        /// <summary>
        /// Updates or inserts data in local table.
        /// </summary>
        /// <param name="tableName">Name of the local table.</param>
        /// <param name="items">A list of items to be inserted.</param>
        /// <param name="fromServer"><code>true</code> if the call is made based on data coming from the server e.g. in a pull operation; <code>false</code> if the call is made by the client, such as insert or update calls on an <see cref="IMobileServiceSyncTable"/>.</param>
        /// <returns>A task that completes when item has been upserted in local table.</returns>
        public override Task UpsertAsync(string tableName, IEnumerable<JObject> items, bool fromServer)
        {
            if (tableName == null)
            {
                throw new ArgumentNullException("tableName");
            }
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            this.EnsureInitialized();

            return UpsertAsyncInternal(tableName, items, fromServer);
        }

        private Task UpsertAsyncInternal(string tableName, IEnumerable<JObject> items, bool fromServer)
        {
            TableDefinition table = GetTable(tableName);

            var parameters = new Dictionary<string, object>();
            var sql = new StringBuilder();

            var first = items.FirstOrDefault();

            if (first == null)
            {
                return Task.FromResult(0);
            }

            IEnumerable<JProperty> properties = first.Properties();
            if (fromServer)
            {
                properties = properties.Where(p => table.ContainsKey(p.Name));
            }

            if (!properties.Any())
            {
                return Task.FromResult(0); // no query to execute if there are no columns in the item
            }

            IList<string> columns = properties.Select(c => c.Name).ToList();
            string columnNames = String.Join(", ", columns.Select(c => SqlHelpers.FormatMember(c)));

            sql.AppendFormat("INSERT OR REPLACE INTO {0} ({1}) VALUES ", SqlHelpers.FormatTableName(tableName), columnNames);

            foreach (JObject item in items)
            {
                AppendInsertValuesSql(tableName, item, table, columns, sql, parameters);
            }

            if (parameters.Any())
            {
                sql.Remove(sql.Length-1, 1); // remove the trailing comma
                this.ExecuteNonQuery(sql.ToString(), parameters);
            }
            
            return Task.FromResult(0);
        }        

        /// <summary>
        /// Deletes items from local table that match the given query.
        /// </summary>
        /// <param name="query">A query to find records to delete.</param>
        /// <returns>A task that completes when delete query has executed.</returns>
        public override Task DeleteAsync(MobileServiceTableQueryDescription query)
        {
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }

            this.EnsureInitialized();

            var formatter = new SqlQueryFormatter(query);
            string sql = formatter.FormatDelete();

            this.ExecuteNonQuery(sql, formatter.Parameters);

            return Task.FromResult(0);
        }

        /// <summary>
        /// Deletes items from local table with the given list of ids
        /// </summary>
        /// <param name="tableName">Name of the local table.</param>
        /// <param name="ids">A list of ids of the items to be deleted</param>
        /// <returns>A task that completes when delete query has executed.</returns>
        public override Task DeleteAsync(string tableName, IEnumerable<string> ids)
        {
            if (tableName == null)
            {
                throw new ArgumentNullException("tableName");
            }
            if (ids == null)
            {
                throw new ArgumentNullException("ids");
            }

            this.EnsureInitialized();

            string idRange = String.Join(",", ids.Select((_, i) => "@id" + i));

            string sql = string.Format("DELETE FROM {0} WHERE {1} IN ({2})", 
                                       SqlHelpers.FormatTableName(tableName), 
                                       MobileServiceSystemColumns.Id,
                                       idRange);

            var parameters = new Dictionary<string, object>();

            int j=0;
            foreach (string id in ids)
            {
                parameters.Add("@id" + (j++), id);
            }

            this.ExecuteNonQuery(sql, parameters);
            return Task.FromResult(0);
        }

        /// <summary>
        /// Executes a lookup against a local table.
        /// </summary>
        /// <param name="tableName">Name of the local table.</param>
        /// <param name="id">The id of the item to lookup.</param>
        /// <returns>A task that will return with a result when the lookup finishes.</returns>
        public override Task<JObject> LookupAsync(string tableName, string id)
        {
            if (tableName == null)
            {
                throw new ArgumentNullException("tableName");
            }
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }

            this.EnsureInitialized();

            string sql = string.Format("SELECT * FROM {0} WHERE {1} = @id", SqlHelpers.FormatTableName(tableName), MobileServiceSystemColumns.Id);
            var parameters = new Dictionary<string, object>
            {
                {"@id", id}
            };

            IList<JObject> results = this.ExecuteQuery(tableName, sql, parameters);

            return Task.FromResult(results.FirstOrDefault());
        }

        private static void AppendInsertValuesSql(string tableName, JObject item, TableDefinition table, IEnumerable<string> columns, StringBuilder sql, Dictionary<string, object> parameters)
        {
            string separator = String.Empty;
            ColumnDefinition columnDefinition;

            sql.Append("(");
            foreach (string columnName in columns)
            {
                if (!table.TryGetValue(columnName, out columnDefinition))
                {
                    throw new InvalidOperationException(string.Format(Properties.Resources.SQLiteStore_ColumnNotDefined, columnName, tableName));
                }

                JToken rawValue = item[columnName];

                object value = SqlHelpers.SerializeValue(rawValue, columnDefinition.SqlType, columnDefinition.Property.Value.Type);
                string paramName = "@p" + (parameters.Count + 1);
                parameters.Add(paramName, value);
                sql.AppendFormat("{0}{1}", separator, paramName);
                separator = ", ";
            }
            sql.Append("),");
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

        internal virtual async Task SaveSetting(string name, string value)
        {
            var setting = new JObject() 
            { 
                { "id", name }, 
                { "value", value } 
            };
            await this.UpsertAsyncInternal(MobileServiceLocalSystemTables.Config, new[]{ setting }, fromServer: false);
        }

        internal virtual void CreateTableFromObject(string tableName, IEnumerable<ColumnDefinition> columns)
        {
            String tblSql = string.Format("CREATE TABLE IF NOT EXISTS {0} ({1} PRIMARY KEY)", SqlHelpers.FormatTableName(tableName), SqlHelpers.FormatMember(MobileServiceSystemColumns.Id));
            this.ExecuteNonQuery(tblSql);

            string infoSql = string.Format("PRAGMA table_info({0});", SqlHelpers.FormatTableName(tableName));
            IDictionary<string, JObject> existingColumns = this.ExecuteQuery((TableDefinition)null, infoSql)
                                                               .ToDictionary(c => c.Value<string>("name"), StringComparer.OrdinalIgnoreCase);

            // new columns that do not exist in existing columns
            var columnsToCreate = columns.Where(c => !existingColumns.ContainsKey(c.Property.Name));

            foreach (ColumnDefinition column in columnsToCreate)
            {
                string createSql = string.Format("ALTER TABLE {0} ADD COLUMN {1} {2}",
                                                 SqlHelpers.FormatTableName(tableName),
                                                 SqlHelpers.FormatMember(column.Property.Name),
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

        private IList<JObject> ExecuteQuery(string tableName, string sql, IDictionary<string, object> parameters = null)
        {
            TableDefinition table = GetTable(tableName);
            return this.ExecuteQuery(table, sql, parameters);
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

        private JToken DeserializeValue(ColumnDefinition column, object value)
        {
            if (value == null)
            {
                return null;
            }

            string sqlType = column.SqlType;
            JTokenType jsonType = column.Property.Value.Type;
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
            
            return null;
        }

        private static void ValidateResult(SQLiteResult result)
        {
            if (result != SQLiteResult.DONE)
            {
                throw new SQLiteException(string.Format(Properties.Resources.SQLiteStore_QueryExecutionFailed, result));
            }
        }

        private JObject ReadRow(TableDefinition table, ISQLiteStatement statement)
        {
            var row = new JObject();
            for (int i = 0; i < statement.ColumnCount; i++)
            {
                string name = statement.ColumnName(i);
                object value = statement[i];

                ColumnDefinition column;
                if (table.TryGetValue(name, out column))
                {
                    JToken jVal = this.DeserializeValue(column, value);
                    row[name] = jVal;
                }
                else
                {
                    row[name] = value == null ? null : JToken.FromObject(value);
                }
            }
            return row;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.connection.Dispose();
            }
        }
    }
}