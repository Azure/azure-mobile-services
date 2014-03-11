// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SQLitePCL;

namespace Microsoft.WindowsAzure.MobileServices.SQLiteStore
{
    internal sealed class MobileServiceSQLiteStoreImpl : IMobileServiceSQLiteStoreImpl
    {
        private SQLiteConnection connection;

        public MobileServiceSQLiteStoreImpl(string filePath)
        {
            this.connection = new SQLiteConnection(filePath);
        }                     

        public void Dispose()
        {
            this.connection.Dispose();
        }

        public Task CreateTableFromObject(string tableName, IEnumerable<ColumnDefinition> columns)
        {
            String tblSql = string.Format("CREATE TABLE IF NOT EXISTS {0} ({1} PRIMARY KEY)", SqlHelpers.FormatTableName(tableName), SqlHelpers.FormatMember(SystemProperties.Id));
            ExecuteNonQuery(tblSql);
            
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
                ExecuteNonQuery(createSql);
            }

            // NOTE: In SQLite you cannot drop columns, only add them.

            return Task.FromResult(0);
        }         

        public void ExecuteNonQuery(string sql, IDictionary<string, object> parameters = null)
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

        public IList<JObject> ExecuteQuery(TableDefinition table, string sql, IDictionary<string, object> parameters = null)
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

        public object DeserializeValue(ColumnDefinition column, object value)
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
            int i=0;
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
    }
}
