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

namespace Microsoft.WindowsAzure.MobileServices.SQLiteStore
{
    public sealed class MobileServiceSQLiteStore: IMobileServiceLocalStore
    {
        private Dictionary<string, TableDefinition> tables = new Dictionary<string, TableDefinition>();

        private IMobileServiceSQLiteStoreImpl innerStore;

        internal MobileServiceSQLiteStore(IMobileServiceSQLiteStoreImpl innerStore)
        {
            this.innerStore = innerStore;

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

        public MobileServiceSQLiteStore(string fileName): this(Platform.Instance.GetSyncStoreImpl(fileName))
        {            
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

        public async Task InitializeAsync()
        {
            foreach (KeyValuePair<string, TableDefinition> table in this.tables)
            {
                await this.innerStore.CreateTableFromObject(table.Key, table.Value.Values.Select(v => v));
            }
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
            TableDefinition table;
            if (!this.tables.TryGetValue(tableName, out table))
            {
                throw new InvalidOperationException(string.Format(Properties.Resources.SQLiteStore_TableNotDefined, tableName));
            }

            var sql = new StringBuilder();
            sql.AppendFormat("INSERT OR REPLACE INTO {0} (", SqlHelpers.FormatTableName(tableName));
            string separator = String.Empty;
            var columns = item.Properties().ToList();

            foreach (JProperty column in columns)
            {
                sql.AppendFormat("{0}{1}", separator, SqlHelpers.FormatMember(column.Name));
                separator = ", ";
            }
            sql.AppendFormat(") VALUES (");
            
            separator = String.Empty;
            ColumnDefinition columnDefinition;

            foreach (JProperty column in columns)
            {
                if (!table.TryGetValue(column.Name, out columnDefinition))
                {
                    throw new InvalidOperationException(string.Format(Properties.Resources.SQLiteStore_ColumnNotDefined, column.Name, tableName));
                }

                //TODO: Use parameterized queries
                object value = SqlHelpers.SerializeValue(column.Value.Value<JValue>(), columnDefinition.SqlType);
                if (value is string)
                {
                    value = "'" + value + "'";
                }
                else if (value == null)
                {
                    value = "NULL";
                }
                sql.AppendFormat("{0}{1}", separator, value);
                separator = ", ";
            }
            sql.Append(")");
            this.innerStore.ExecuteNonQuery(sql.ToString());
            return Task.FromResult(0);
        }

        public Task DeleteAsync(MobileServiceTableQueryDescription query)
        {
            var formatter = new SqlFormatter(query);
            string sql = formatter.FormatDelete();

            this.innerStore.ExecuteNonQuery(sql, formatter.Parameters);

            return Task.FromResult(0);
        }

        public Task DeleteAsync(string tableName, string id)
        {
            //string sql = string.Format("DELETE FROM {0} WHERE id = @id", tableName);
            string sql = string.Format("DELETE FROM {0} WHERE {1} = '{2}'", SqlHelpers.FormatTableName(tableName), SystemProperties.Id, id);

            var parameters = new Dictionary<string, object>
            {
                //TODO: {"@id", id} string binding is broken as of now. https://sqlitepcl.codeplex.com/workitem/2
            };

            this.innerStore.ExecuteNonQuery(sql, parameters);
            return Task.FromResult(0);
        }

        public Task<JObject> LookupAsync(string tableName, string id)
        {
            //string sql = string.Format("SELECT * FROM {0} WHERE id = @id'", tableName);
            string sql = string.Format("SELECT * FROM {0} WHERE {1} = '{2}'", SqlHelpers.FormatTableName(tableName), SystemProperties.Id, id);
            var parameters = new Dictionary<string, object>
            {
                //TODO: {"@id", id} string binding is broken as of now. https://sqlitepcl.codeplex.com/workitem/2
            };

            IList<JObject> results = this.ExecuteQuery(tableName, sql, parameters);

            return Task.FromResult(results.FirstOrDefault());
        }

        private void Vacuum()
        {
            this.innerStore.ExecuteNonQuery("VACUUM;"); //TODO: This fails
        }
        
        private IList<JObject> ExecuteQuery(string tableName, string sql, IDictionary<string, object> parameters = null)
        {
            TableDefinition table;
            this.tables.TryGetValue(tableName, out table);
            return this.innerStore.ExecuteQuery(table, sql, parameters);
        }

        public void Dispose()
        {
            this.innerStore.Dispose();
        }        
    }
}
