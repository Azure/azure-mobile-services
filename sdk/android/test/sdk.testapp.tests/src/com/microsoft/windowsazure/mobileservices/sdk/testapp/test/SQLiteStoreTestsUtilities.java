package com.microsoft.windowsazure.mobileservices.sdk.testapp.test;

import android.content.Context;
import android.database.sqlite.SQLiteDatabase;
import android.database.sqlite.SQLiteOpenHelper;
import android.database.sqlite.SQLiteStatement;

public class SQLiteStoreTestsUtilities extends SQLiteOpenHelper {

	public SQLiteStoreTestsUtilities(Context context, String name) {
		super(context, name, null, 1);
	}

	public void dropTestTable(String tableName)
    {
        executeNonQuery("DROP TABLE IF EXISTS " + tableName);
    }

    public long countRows(String tableName)
    {
    	SQLiteDatabase db = this.getReadableDatabase();
    	
    	String countQuery = "SELECT COUNT(1) from " + tableName;
    	
    	SQLiteStatement s = db.compileStatement(countQuery);
    	
    	long count = s.simpleQueryForLong();
    	
    	return count;
    }

    public void truncate(String tableName)
    {
    	executeNonQuery("DELETE FROM " + tableName);
    }

    public void executeNonQuery(String sql)
    {
    	SQLiteDatabase db = this.getWritableDatabase();
    	
    	db.execSQL(sql);
    }

	@Override
	public void onCreate(SQLiteDatabase db) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public void onUpgrade(SQLiteDatabase db, int oldVersion, int newVersion) {
		// TODO Auto-generated method stub
		
	}
	
    public static void Truncate(Context context, String dbName, String tableName)
    {
    	executeNonQuery(context, dbName, "DELETE FROM " + tableName);
    }

    public static void executeNonQuery(Context context, String dbName, String sql)
    {
    	SQLiteStoreTestsUtilities u = 
    			new SQLiteStoreTestsUtilities(context, dbName);
    	
    	u.executeNonQuery(sql);
    }
    
    public static void dropTestTable(Context context, String dbName, String tableName)
    {
        executeNonQuery(context, dbName, "DROP TABLE IF EXISTS " + tableName);
    }

    public static long countRows(Context context, String dbName, String tableName)
    {
    	SQLiteStoreTestsUtilities u = 
    			new SQLiteStoreTestsUtilities(context, dbName);
    	
    	return u.countRows(tableName);
    }
}
