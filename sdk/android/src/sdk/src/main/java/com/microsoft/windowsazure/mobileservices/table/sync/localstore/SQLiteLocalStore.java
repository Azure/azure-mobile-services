/*
Copyright (c) Microsoft Open Technologies, Inc.
All Rights Reserved
Apache 2.0 License
 
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at
 
     http://www.apache.org/licenses/LICENSE-2.0
 
   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
 
See the Apache Version 2.0 License for specific language governing permissions and limitations under the License.
 */

/**
 * SQLiteLocalStore.java
 */
package com.microsoft.windowsazure.mobileservices.table.sync.localstore;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Locale;
import java.util.Map;
import java.util.Map.Entry;

import android.annotation.TargetApi;
import android.content.Context;
import android.database.Cursor;
import android.database.DatabaseErrorHandler;
import android.database.sqlite.SQLiteDatabase;
import android.database.sqlite.SQLiteDatabase.CursorFactory;
import android.database.sqlite.SQLiteOpenHelper;
import android.os.Build;

import com.google.gson.JsonArray;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import com.microsoft.windowsazure.mobileservices.MobileServiceException;
import com.microsoft.windowsazure.mobileservices.table.query.Query;
import com.microsoft.windowsazure.mobileservices.table.query.QuerySQLWriter;

/**
 * Implements MobileServiceLocalStore backed by an SQLite DB
 */
public class SQLiteLocalStore extends SQLiteOpenHelper implements MobileServiceLocalStore {
    private static class Statement {
        private String sql;
        private List<Object> parameters;
    }

    private Map<String, Map<String, ColumnDataType>> mTables;

    /**
     * Constructor for SQLiteLocalStore
     *
     * @param context context to use to open or create the database
     * @param name    name of the database file, or null for an in-memory database
     * @param factory factory to use for creating cursor objects, or null for the
     *                default
     * @param version version number of the database (starting at 1); if the
     *                database is older, onUpgrade will be used to upgrade the
     *                database; if the database is newer, onDowngrade will be used
     *                to downgrade the database
     */
    public SQLiteLocalStore(Context context, String name, CursorFactory factory, int version) {
        super(context, name, factory, version);
        this.mTables = new HashMap<String, Map<String, ColumnDataType>>();
    }

    /**
     * Constructor for SQLiteLocalStore
     *
     * @param context      context to use to open or create the database
     * @param name         name of the database file, or null for an in-memory database
     * @param factory      factory to use for creating cursor objects, or null for the
     *                     default
     * @param version      version number of the database (starting at 1); if the
     *                     database is older, onUpgrade will be used to upgrade the
     *                     database; if the database is newer, onDowngrade will be used
     *                     to downgrade the database
     * @param errorHandler the DatabaseErrorHandler to be used when sqlite reports
     *                     database corruption, or null to use the default error handler.
     */
    @TargetApi(Build.VERSION_CODES.HONEYCOMB)
    public SQLiteLocalStore(Context context, String name, CursorFactory factory, int version, DatabaseErrorHandler errorHandler) {
        super(context, name, factory, version, errorHandler);
        this.mTables = new HashMap<String, Map<String, ColumnDataType>>();
    }

    @Override
    public void initialize() throws MobileServiceLocalStoreException {
        try {
            SQLiteDatabase db = this.getWritableDatabase();
            db.close();

            for (Entry<String, Map<String, ColumnDataType>> entry : this.mTables.entrySet()) {
                createTableFromObject(entry.getKey(), entry.getValue());
            }
        } catch (Throwable t) {
            throw new MobileServiceLocalStoreException(t);
        }
    }

    @Override
    public void defineTable(String tableName, Map<String, ColumnDataType> columns) throws MobileServiceLocalStoreException {
        try {
            String invTableName = normalizeTableName(tableName);

            Map<String, ColumnDataType> table = this.mTables.containsKey(invTableName) ? this.mTables.get(invTableName) : new HashMap<String, ColumnDataType>();
            table.put("id", ColumnDataType.String);

            for (String colName : columns.keySet()) {
                ColumnDataType colDataType = columns.get(colName);
                String invColumnName = normalizeColumnName(colName);

                validateReservedProperties(colDataType, invColumnName);

                if (!invColumnName.equals("id")) {
                    table.put(invColumnName, colDataType);
                }
            }

            this.mTables.put(invTableName, table);
        } catch (Throwable t) {
            throw new MobileServiceLocalStoreException(t);
        }
    }

    @Override
    public JsonElement read(Query query) throws MobileServiceLocalStoreException {
        try {
            JsonElement result;
            JsonArray rows = new JsonArray();

            String invTableName = normalizeTableName(query.getTableName());

            Map<String, ColumnDataType> table = this.mTables.get(invTableName);

            String[] columns = getColumns(query, table);

            String whereClause = getWhereClause(query);

            String orderByClause = QuerySQLWriter.getOrderByClause(query);

            String limitClause = QuerySQLWriter.getLimitClause(query);

            Integer inlineCount = null;

            SQLiteDatabase db = this.getWritableDatabase();

            try {
                Cursor cursor = null;

                try {
                    if (query.hasInlineCount()) {
                        cursor = db.query(invTableName, columns, whereClause, null, null, null, orderByClause, null);
                        inlineCount = cursor.getCount();

                        if (query.getSkip() > 0) {
                            cursor.move(query.getSkip());
                        }
                    } else {
                        cursor = db.query(invTableName, columns, whereClause, null, null, null, orderByClause, limitClause);
                    }

                    int limit = 0;

                    while (!(query.getTop() > 0 && limit == query.getTop()) && cursor.moveToNext()) {
                        JsonObject row = parseRow(cursor, table);
                        rows.add(row);

                        limit++;
                    }
                } finally {
                    if (cursor != null && !cursor.isClosed()) {
                        cursor.close();
                    }
                }
            } finally {
                db.close();
            }

            if (query.hasInlineCount()) {
                JsonObject resObj = new JsonObject();
                resObj.addProperty("count", inlineCount);
                resObj.add("results", rows);
                result = resObj;
            } else {
                result = rows;
            }

            return result;
        } catch (Throwable t) {
            throw new MobileServiceLocalStoreException(t);
        }
    }

    @Override
    public JsonObject lookup(String tableName, String itemId) throws MobileServiceLocalStoreException {
        try {
            JsonObject result = null;
            String invTableName = normalizeTableName(tableName);

            Map<String, ColumnDataType> table = this.mTables.get(invTableName);

            SQLiteDatabase db = this.getWritableDatabase();

            try {
                Cursor cursor = null;

                try {
                    cursor = db.query(invTableName, table.keySet().toArray(new String[0]), "id = '" + itemId + "'", null, null, null, null);

                    if (cursor.moveToNext()) {
                        result = parseRow(cursor, table);
                    }
                } finally {
                    if (cursor != null && !cursor.isClosed()) {
                        cursor.close();
                    }
                }
            } finally {
                db.close();
            }

            return result;
        } catch (Throwable t) {
            throw new MobileServiceLocalStoreException(t);
        }
    }

    @Override
    public void upsert(String tableName, JsonObject item) throws MobileServiceLocalStoreException {
        try {

            JsonObject[] items = new JsonObject[1];
            items[0] = item;

            upsert(tableName, items);

        } catch (Throwable t) {
            throw new MobileServiceLocalStoreException(t);
        }
    }

    @Override
    public void upsert(String tableName, JsonObject[] items) throws MobileServiceLocalStoreException {
        try {
            String invTableName = normalizeTableName(tableName);

            Statement statement = generateUpsertStatement(invTableName, items);

            SQLiteDatabase db = this.getWritableDatabase();

            try {
                db.execSQL(statement.sql, statement.parameters.toArray());
            } finally {
                db.close();
            }
        } catch (Throwable t) {
            throw new MobileServiceLocalStoreException(t);
        }
    }

    @Override
    public void delete(String tableName, String itemId) throws MobileServiceLocalStoreException {
        try {
            String invTableName = normalizeTableName(tableName);

            SQLiteDatabase db = this.getWritableDatabase();

            try {
                db.delete(invTableName, "id = '" + itemId + "'", null);
            } finally {
                db.close();
            }
        } catch (Throwable t) {
            throw new MobileServiceLocalStoreException(t);
        }
    }

    @Override
    public void delete(Query query) throws MobileServiceLocalStoreException {
        try {
            String invTableName = normalizeTableName(query.getTableName());

            String whereClause = getWhereClause(query);

            SQLiteDatabase db = this.getWritableDatabase();

            try {
                db.delete(invTableName, whereClause, null);
            } finally {
                db.close();
            }
        } catch (Throwable t) {
            throw new MobileServiceLocalStoreException(t);
        }
    }

    @Override
    public void onCreate(SQLiteDatabase db) {
    }

    @Override
    public void onUpgrade(SQLiteDatabase db, int oldVersion, int newVersion) {
    }

    private String normalizeTableName(String tableName) {
        String invTableName = tableName != null ? tableName.trim().toLowerCase(Locale.getDefault()) : null;

        if (invTableName == null || tableName.length() == 0) {
            throw new IllegalArgumentException("Table name cannot be null or empty.");
        }

        if (invTableName.length() > 60) {
            throw new IllegalArgumentException("Table name cannot be longer than 60 characters.");
        }

        if (invTableName.matches("[a-zA-Z]/w*")) {
            throw new IllegalArgumentException("Table name must start with a letter, and can contain only alpha-numeric characters and underscores.");
        }

        if (invTableName.matches("sqlite_/w*")) {
            throw new IllegalArgumentException("Table names prefixed with \"sqlite_\" are system reserved.");
        }

        return invTableName;
    }

    private String normalizeColumnName(String columnName) {
        String invColumnName = columnName != null ? columnName.trim().toLowerCase(Locale.getDefault()) : null;

        if (invColumnName == null || columnName.length() == 0) {
            throw new IllegalArgumentException("Column name cannot be null or empty.");
        }

        if (invColumnName.length() > 128) {
            throw new IllegalArgumentException("Column name cannot be longer than 128 characters.");
        }

        if (invColumnName.matches("[a-zA-Z_]/w*")) {
            throw new IllegalArgumentException(
                    "Column name must start with a letter or underscore, and can contain only alpha-numeric characters and underscores.");
        }

        if (invColumnName.matches("__/w*") && !isSystemProperty(invColumnName)) {
            throw new IllegalArgumentException("Column names prefixed with \"__\" are system reserved.");
        }

        return invColumnName;
    }

    private List<String> normalizeColumnNames(List<String> columnNames) {
        List<String> invColumnNames = new ArrayList<String>(columnNames.size());

        for (String columnName : columnNames) {
            invColumnNames.add(normalizeColumnName(columnName));
        }

        return invColumnNames;
    }

    private boolean isSystemProperty(String invColumnName) {

        invColumnName = invColumnName.trim().toLowerCase(Locale.getDefault());

        return invColumnName.equals("__version") || invColumnName.equals("__createdat") || invColumnName.equals("__updatedat")
                || invColumnName.equals("__queueloadedat");
    }

    private void validateReservedProperties(ColumnDataType colDataType, String invColumnName) throws IllegalArgumentException {

        invColumnName = invColumnName.trim().toLowerCase(Locale.getDefault());

        if (invColumnName.equals("id") && colDataType != ColumnDataType.String) {
            throw new IllegalArgumentException("System column \"id\" must be ColumnDataType.String.");
        } else if (invColumnName.equals("__version") && colDataType != ColumnDataType.String) {
            throw new IllegalArgumentException("System column \"__version\" must be ColumnDataType.String.");
        } else if (invColumnName.equals("__createdat") && colDataType != ColumnDataType.Date) {
            throw new IllegalArgumentException("System column \"__createdat\" must be ColumnDataType.Date.");
        } else if (invColumnName.equals("__updatedat") && colDataType != ColumnDataType.Date) {
            throw new IllegalArgumentException("System column \"__updatedat\" must be ColumnDataType.Date.");
        } else if (invColumnName.equals("__queueloadedat") && colDataType != ColumnDataType.Date) {
            throw new IllegalArgumentException("System column \"__queueloadedat\" must be ColumnDataType.Date.");
        }
    }

    private JsonObject parseRow(Cursor cursor, Map<String, ColumnDataType> table) {
        JsonObject result = new JsonObject();

        for (Entry<String, ColumnDataType> column : table.entrySet()) {
            String columnName = column.getKey();
            ColumnDataType columnDataType = column.getValue();
            int columnIndex = cursor.getColumnIndex(columnName);

            if (columnIndex != -1) {

                switch (columnDataType) {
                    case Boolean:
                        boolean booleanValue = cursor.getInt(columnIndex) > 0 ? true : false;
                        result.addProperty(columnName, booleanValue);
                        break;
                    case Number:
                        double doubleValue = cursor.getDouble(columnIndex);
                        result.addProperty(columnName, doubleValue);
                        break;
                    case String:
                        String stringValue = cursor.getString(columnIndex);
                        result.addProperty(columnName, stringValue);
                        break;
                    case Date:
                        String dateValue = cursor.getString(columnIndex);
                        result.addProperty(columnName, dateValue);
                        break;
                    case Other:
                        JsonElement otherValue = new JsonParser().parse(cursor.getString(columnIndex));
                        result.add(columnName, otherValue);
                        break;
                }
            }
        }

        return result;
    }

    private Statement generateUpsertStatement(String tableName, JsonObject[] items) {
        Statement result = new Statement();

        String invTableName = normalizeTableName(tableName);

        StringBuilder sql = new StringBuilder();

        sql.append("INSERT OR REPLACE INTO \"");
        sql.append(invTableName);
        sql.append("\" (");

        String delimiter = "";

        JsonObject firstItem = items[0];

        Map<String, ColumnDataType> tableDefinition = mTables.get(invTableName);

        List<Object> parameters = new ArrayList<Object>(firstItem.entrySet().size());

        for (Entry<String, JsonElement> property : firstItem.entrySet()) {

            if (isSystemProperty(property.getKey()) && !tableDefinition.containsKey(property.getKey())) {
                continue;
            }

            String invColumnName = normalizeColumnName(property.getKey());
            sql.append(delimiter);
            sql.append("\"");
            sql.append(invColumnName);
            sql.append("\"");
            delimiter = ",";
        }

        sql.append(") VALUES ");

        String prefix = "";

        for (JsonObject item : items) {
            sql.append(prefix);
            appendInsertValuesSql(sql, parameters, tableDefinition, item);
            prefix = ",";
        }

        result.sql = sql.toString();
        result.parameters = parameters;

        return result;
    }

    private void appendInsertValuesSql(StringBuilder sql, List<Object> parameters,
                                       Map<String, ColumnDataType> tableDefinition, JsonObject item) {
        sql.append("(");
        int colCount = 0;

        for (Entry<String, JsonElement> property : item.entrySet()) {

            if (isSystemProperty(property.getKey()) && !tableDefinition.containsKey(property.getKey())) {
                continue;
            }

            if (colCount > 0)
                sql.append(",");

            String paramName = "@p" + parameters.size();

            JsonElement value = property.getValue();

            if (value.isJsonNull()) {
                parameters.add(null);
            } else if (value.isJsonPrimitive()) {
                if (value.getAsJsonPrimitive().isBoolean()) {
                    long longVal = value.getAsJsonPrimitive().getAsBoolean() ? 1L : 0L;
                    parameters.add(longVal);
                } else if (value.getAsJsonPrimitive().isNumber()) {
                    parameters.add(value.getAsJsonPrimitive().getAsDouble());
                } else {
                    parameters.add(value.getAsJsonPrimitive().getAsString());
                }
            } else {
                parameters.add(value.toString());
            }

            sql.append(paramName);
            colCount++;
        }

        sql.append(")");
    }


    private String[] getColumns(Query query, Map<String, ColumnDataType> table) {
        String[] columns = table.keySet().toArray(new String[0]);

        List<String> projection = query.getProjection();

        if (projection != null && projection.size() > 0) {
            columns = normalizeColumnNames(projection).toArray(new String[0]);
        }
        return columns;
    }

    private String getWhereClause(Query query) throws MobileServiceLocalStoreException {
        String whereClause;

        try {
            whereClause = QuerySQLWriter.getWhereClause(query);
        } catch (MobileServiceException e) {
            throw new MobileServiceLocalStoreException("Unable to build filter expression.", e);
        }

        if (whereClause != null && whereClause.length() == 0) {
            whereClause = null;
        }

        return whereClause;
    }

    private void createTableFromObject(String invTableName, Map<String, ColumnDataType> table) {
        SQLiteDatabase db = this.getWritableDatabase();

        String tblSql = String.format("CREATE TABLE IF NOT EXISTS \"%s\" (\"id\" TEXT PRIMARY KEY);", invTableName);
        db.execSQL(tblSql);

        List<String> invColumnNames = new ArrayList<String>();

        String infoSql = String.format("PRAGMA table_info(\"%s\");", invTableName);

        Cursor cursor = null;

        try {
            cursor = db.rawQuery(infoSql, null);

            while (cursor.moveToNext()) {
                int columnIndex = cursor.getColumnIndex("name");
                String columnName = cursor.getString(columnIndex);
                String invColumnName = normalizeColumnName(columnName);
                invColumnNames.add(invColumnName);
            }
        } finally {
            if (cursor != null && !cursor.isClosed()) {
                cursor.close();
            }
        }

        Map<String, ColumnDataType> newColumns = new HashMap<String, ColumnDataType>();

        for (Entry<String, ColumnDataType> column : table.entrySet()) {
            if (!invColumnNames.contains(column.getKey())) {
                newColumns.put(column.getKey(), column.getValue());
            }
        }

        for (Entry<String, ColumnDataType> newColumn : newColumns.entrySet()) {
            String invColumnName = newColumn.getKey();

            String type = "";

            switch (newColumn.getValue()) {
                case Boolean:
                    type = "INTEGER";
                    break;
                case Number:
                    type = "REAL";
                    break;
                case String:
                    type = "TEXT";
                    break;
                case Date:
                    type = "TEXT";
                    break;
                case Other:
                    type = "TEXT";
                    break;
            }

            String createSql = String.format("ALTER TABLE \"%s\" ADD COLUMN \"%s\" %s", invTableName, invColumnName, type);

            db.execSQL(createSql);
        }
    }
}