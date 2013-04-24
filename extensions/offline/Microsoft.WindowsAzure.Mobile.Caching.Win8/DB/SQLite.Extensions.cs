using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace SQLite
{
    public partial class SQLiteConnection
    {
        /// <summary>
        /// Executes a "create table if not exists" on the database. It also
        /// creates any specified indexes on the columns of the table. It uses
        /// a schema automatically generated from the specified type. You can
        /// later access this schema by calling GetMapping.
        /// </summary>
        /// <param name="ty">Type to reflect to a database table.</param>
        /// <returns>
        /// The number of entries added to the database schema.
        /// </returns>
        public int CreateTable(TableMapping map)
        {
            var query = "create table if not exists \"" + map.TableName + "\"(\n";

            var decls = map.Columns.Select(p => Orm.SqlDecl(p, StoreDateTimeAsTicks));
            var decl = string.Join(",\n", decls.ToArray());
            query += decl;
            query += ")";

            var count = Execute(query);

            //if (count == 1)
            //{ //Possible bug: This always seems to return 0?
                // Table already exists, migrate it
                MigrateTable(map);
            //}

            var indexes = new Dictionary<string, IndexInfo>();
            foreach (var c in map.Columns)
            {
                foreach (var i in c.Indices)
                {
                    var iname = i.Name ?? map.TableName + "_" + c.Name;
                    IndexInfo iinfo;
                    if (!indexes.TryGetValue(iname, out iinfo))
                    {
                        iinfo = new IndexInfo
                        {
                            IndexName = iname,
                            TableName = map.TableName,
                            Unique = i.Unique,
                            Columns = new List<IndexedColumn>()
                        };
                        indexes.Add(iname, iinfo);
                    }

                    if (i.Unique != iinfo.Unique)
                        throw new Exception("All the columns in an index must have the same value for their Unique property");

                    iinfo.Columns.Add(new IndexedColumn
                    {
                        Order = i.Order,
                        ColumnName = c.Name
                    });
                }
            }

            foreach (var indexName in indexes.Keys)
            {
                var index = indexes[indexName];
                const string sqlFormat = "create {3} index if not exists \"{0}\" on \"{1}\"(\"{2}\")";
                var columns = String.Join("\",\"", index.Columns.OrderBy(i => i.Order).Select(i => i.ColumnName).ToArray());
                var sql = String.Format(sqlFormat, indexName, index.TableName, columns, index.Unique ? "unique" : "");
                count += Execute(sql);
            }

            return count;
        }

        public partial class ColumnInfo
        {
            //			public int cid { get; set; }

            [Column("type")]
            public string ColumnType { get; set; }

            //			public int notnull { get; set; }

            //			public string dflt_value { get; set; }

            public int pk { get; set; }
        }
    }

    public partial class TableMapping
    {
        public TableMapping(string tableName, IEnumerable<Column> columns)
        {
            MappedType = typeof(object);

            TableName = tableName;

            Columns = columns.ToArray();
            foreach (var c in Columns)
            {
                if (c.IsAutoInc && c.IsPK)
                {
                    _autoPk = c;
                }
                if (c.IsPK)
                {
                    PK = c;
                }
            }

            HasAutoIncPK = _autoPk != null;

            if (PK != null)
            {
                GetByPrimaryKeySql = string.Format("select * from \"{0}\" where \"{1}\" = ?", TableName, PK.Name);
            }
            else
            {
                // People should not be calling Get/Find without a PK
                GetByPrimaryKeySql = string.Format("select * from \"{0}\" limit 1", TableName);
            }
        }

        public partial class Column
        {
            public Column(string name, Type propertyType, bool isPrimary, bool hasIndex, int maxStringLength)
            {
                //var colAttr = (ColumnAttribute)prop.GetCustomAttributes(typeof(ColumnAttribute), true).FirstOrDefault();

                _prop = null;
                Name = name; // colAttr == null ? prop.Name : colAttr.Name;
                //If this type is Nullable<T> then Nullable.GetUnderlyingType returns the T, otherwise it returns null, so get the the actual type instead
                ColumnType = Nullable.GetUnderlyingType(propertyType) ?? propertyType; //Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                Collation = string.Empty;
                IsAutoInc = false; //Orm.IsAutoInc(prop);
                IsPK = isPrimary; // Orm.IsPK(prop);
                Indices = hasIndex ? new[] { new IndexedAttribute(name + "_index", 0) } : new IndexedAttribute[0];
                IsNullable = !IsPK;
                MaxStringLength = maxStringLength;
            }
        }
    }

    public partial class SQLiteCommand
    {
        public IEnumerable<Dictionary<string, JToken>> ExecuteQuery(TableMapping map)
        {
            return ExecuteDeferredQuery(map);
        }

        public IEnumerable<Dictionary<string, JToken>> ExecuteDeferredQuery(TableMapping map)
        {
            if (_conn.Trace)
            {
                Debug.WriteLine("Executing Query: " + this);
            }

            var stmt = Prepare();
            try
            {
                var cols = new TableMapping.Column[SQLite3.ColumnCount(stmt)];

                for (int i = 0; i < cols.Length; i++)
                {
                    var name = SQLite3.ColumnName16(stmt, i);
                    cols[i] = map.FindColumn(name);
                }

                while (SQLite3.Step(stmt) == SQLite3.Result.Row)
                {
                    //var obj = Activator.CreateInstance(map.MappedType);
                    Dictionary<string, JToken> obj = new Dictionary<string, JToken>();
                    for (int i = 0; i < cols.Length; i++)
                    {
                        if (cols[i] == null)
                            continue;
                        var colType = SQLite3.ColumnType(stmt, i);
                        var val = ReadCol(stmt, i, colType, cols[i].ColumnType);
                        obj[cols[i].Name] = new JValue(val);
                    }
                    OnInstanceCreated(obj);
                    yield return obj;
                }
            }
            finally
            {
                SQLite3.Finalize(stmt);
            }
        }
    }
}
