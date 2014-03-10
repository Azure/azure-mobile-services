// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices.SQLiteStore
{
    internal interface IMobileServiceSQLiteStoreImpl: IDisposable
    {
        Task CreateTableFromObject(string tableName, IEnumerable<ColumnDefinition> columns);
        void ExecuteNonQuery(string sql, IDictionary<string, object> parameters = null);
        IList<JObject> ExecuteQuery(TableDefinition table, string sql, IDictionary<string, object> parameters = null);
        object DeserializeValue(ColumnDefinition column, object value);
    }
}
