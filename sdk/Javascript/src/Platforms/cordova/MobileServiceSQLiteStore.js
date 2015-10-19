// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

var Platform = require('Platforms/Platform');
var Validate = require('../../Utilities/Validate');
var _ = require('../../Utilities/Extensions');

var idPropertyName = "id";
var tables = {};

var MobileServiceSQLiteStore = function (dbName) {
    /// <summary>
    /// Initializes a new instance of the MobileServiceSQLiteStore class.
    /// </summary>

    this._db = window.sqlitePlugin.openDatabase({ name: dbName });

    this.defineTable = Platform.async(function (tableDefinition, callback) {
        /// <summary>Defines the local table in the sqlite store</summary>
        /// <param name="tableDefinition">Table definition object defining the table name and columns
        /// Example of a valid tableDefinition object:
        /// tableDefinition : {
        ///     name: "todoItemTable",
        ///     columnDefinitions : {
        ///         id : "INTEGER",
        ///         description : MobileServiceSQLiteStore.ColumnType.TEXT,
        ///         price : "REAL"
        ///     }
        /// }
        /// </param>
        /// <returns type="Promise">
        /// A promise that is resolved when the operation is completed successfully.
        /// If the operation fails, the promise is rejected
        /// </returns>

        // Platform.async silently appends a callback argument to the original list of arguments.
        // Validate the argument length to ensure the callback argument is indeed the callback 
        // provided by Platform.async.
        Validate.length(arguments, 2, 'arguments');

        Validate.notNull(callback, 'callback');

        Validate.notNull(tableDefinition, 'tableDefinition');
        Validate.isString(tableDefinition.name, 'tableDefinition.name');
        Validate.notNullOrEmpty(tableDefinition.name, 'tableDefinition.name');

        var columnDefinitions = tableDefinition.columnDefinitions;

        // Validate the specified column types
        for (var columnName in columnDefinitions) {
            var columnType = columnDefinitions[columnName];

            Validate.isString(columnType, 'columnType');
            Validate.notNullOrEmpty(columnType, 'columnType');
        }

        this._db.transaction(function(transaction) {

            var pragmaStatement = _.format("PRAGMA table_info({0});", tableDefinition.name);

            transaction.executeSql(pragmaStatement, [], function (transaction, result) {

                // If table already exists, add missing columns, if any.
                // Else, create the table
                if (result.rows.length > 0) {

                    // Columns that are present in the table already
                    var existingColumns = {};

                    // Remove columns that are already present in the table from the columnDefinitions array
                    for (var i = 0; i < result.rows.length; i++) {
                        var column = result.rows.item(i);
                        existingColumns[column.name] = true;
                    }

                    addMissingColumns(transaction, tableDefinition, existingColumns);

                } else {
                    createTable(transaction, tableDefinition);
                }
            });

        }, function (error) {
            callback(error);
        }, function(result) {
            callback();
        });
    });

    //TODO(shrirs): instance needs to be an array instead of an object
    this.upsert = Platform.async(function (tableName, instance, callback) {
        /// <summary>Updates or inserts an object in the local table</summary>
        /// <param name="tableName">Name of the local table in which the object is to be upserted</param>
        /// <param name="instance">Object to be inserted or updated in the table</param>
        /// <returns type="Promise">
        /// A promise that is resolved when the operation is completed successfully.
        /// If the operation fails, the promise is rejected
        /// </returns>

        // Platform.async silently appends a callback argument to the original list of arguments.
        // Validate the argument length to ensure the callback argument is indeed the callback 
        // provided by Platform.async.
        Validate.length(arguments, 3, 'arguments');

        Validate.isString(tableName, 'tableName');
        Validate.notNullOrEmpty(tableName, 'tableName');

        Validate.notNull(instance, 'instance');

        // Note: The default maximum number of parameters allowed by sqlite is 999
        // See: http://www.sqlite.org/limits.html#max_variable_number
        // TODO(shrirs): Add support for tables with more than 999 columns

        var columnNames = [];
        var columnValues = [];

        for (var property in instance) {
            columnNames.push(property);
            columnValues.push(instance[property]);
        }

        // Form string of the following form: ?,?,?,?....,?,? with one '?' for each column
        var valueClause = Array(columnNames.length + 1).join('?').split('').join();

        var insertStatement = _.format("INSERT OR REPLACE INTO {0} ({1}) VALUES ({2})", tableName, columnNames.join(), valueClause);

        this._db.transaction(function(transaction) {
            transaction.executeSql(insertStatement, columnValues);
        }, function(error) {
            callback(error);
        }, function() {
            callback();
        });
    });

    // TODO(shrirs): Implement equivalents of readWithQuery and deleteUsingQuery
    this.lookup = Platform.async(function (tableName, id, callback) {
        /// <summary>Perform a lookup against a local table</summary>
        /// <param name="tableName">Name of the local table in which look up is to be performed</param>
        /// <param name="id">ID of the object to be looked up</param>
        /// <returns type="Promise">
        /// A promise that is resolved with the looked up object when the operation is completed successfully.
        /// If the operation fails, the promise is rejected
        /// </returns>

        // Platform.async silently appends a callback argument to the original list of arguments.
        // Validate the argument length to ensure the callback argument is indeed the callback 
        // provided by Platform.async.
        Validate.length(arguments, 3, 'arguments');

        Validate.isString(tableName, 'tableName');
        Validate.notNullOrEmpty(tableName, 'tableName');

        Validate.notNull(id, 'id');

        var lookupStatement = _.format("SELECT * FROM [{0}] WHERE {1} = ? COLLATE NOCASE", tableName, idPropertyName);

        this._db.executeSql(lookupStatement, [id], function (result) {

            var instance = null;
            if (result.rows.length !== 0) {
                instance = result.rows.item(0); 
            }

            callback(null, instance);
        }, function (err) {
            callback(err);
        });
    });

    //TODO(shrirs): instance needs to be an array instead of an object
    this.del = Platform.async(function (tableName, instance, callback) {
        /// <summary>The items to delete from the local table</summary>
        /// <param name="tableName">Name of the local table in which delete is to be performed</param>
        /// <param name="instance">Object to delete from the table</param>
        /// <returns type="Promise">
        /// A promise that is resolved when the operation is completed successfully.
        /// If the operation fails, the promise is rejected
        /// </returns>

        var deleteStatement = _.format("DELETE FROM {0} WHERE {1} = ? COLLATE NOCASE", tableName, idPropertyName);

        this._db.executeSql(deleteStatement, [instance[idPropertyName]], function (result) {
            callback();
        }, function(error) {
            callback(error);
        });
    });
};

function createTable(transaction, tableDefinition) {
    var columnDefinitions = tableDefinition.columnDefinitions;
    var columnDefinitionClauses = [];

    for (var columnName in columnDefinitions) {
        var columnType = columnDefinitions[columnName];

        var columnDefinitionClause = _.format("[{0}] {1}", columnName, columnType);

        // TODO(shrirs): Handle cases where id property may be missing
        if (columnName === idPropertyName) {
            columnDefinitionClause += " PRIMARY KEY";
        }

        columnDefinitionClauses.push(columnDefinitionClause);
    }

    var createTableStatement = _.format("CREATE TABLE [{0}] ({1})", tableDefinition.name, columnDefinitionClauses.join());

    transaction.executeSql(createTableStatement);
}

// Add missing columns to the table
function addMissingColumns(transaction, tableDefinition, existingColumns) {

    // SQLite does not support adding multiple columns using a single statement; Add one column at a time
    var columnDefinitions = tableDefinition.columnDefinitions;
    for (var columnName in columnDefinitions) {

        // If this column does not already exist, we need to create it
        if (!existingColumns[columnName]) {
            var alterStatement = _.format("ALTER TABLE {0} ADD COLUMN {1} {2}", tableDefinition.name, columnName, columnDefinitions[columnName]);
            transaction.executeSql(alterStatement);
        }
    }
}

// Valid SQL types
MobileServiceSQLiteStore.ColumnType = {
    NULL: "NULL",
    INTEGER: "INTEGER",
    REAL: "REAL",
    TEXT: "TEXT",
    BLOB: "BLOB"
};

// Export
Platform.addToMobileServicesClientNamespace({ MobileServiceSQLiteStore: MobileServiceSQLiteStore });

exports.MobileServiceSQLiteStore = MobileServiceSQLiteStore;
