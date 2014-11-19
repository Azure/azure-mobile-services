// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using Microsoft.WindowsAzure.MobileServices.Sync;
using SQLitePCL;

namespace Microsoft.WindowsAzure.MobileServices.SQLiteStore.Test
{
    internal class TestUtilities
    {
        public static void ResetDatabase(string dbName)
        {            
            TestUtilities.DropTestTable(dbName, MobileServiceLocalSystemTables.OperationQueue);
            TestUtilities.DropTestTable(dbName, MobileServiceLocalSystemTables.SyncErrors);
            TestUtilities.DropTestTable(dbName, MobileServiceLocalSystemTables.Config);
        }

        public static void DropTestTable(string dbName, string tableName)
        {
            ExecuteNonQuery(dbName, "DROP TABLE IF EXISTS " + tableName);
        }

        public static long CountRows(string dbName, string tableName)
        {
            long count;
            using (var connection = new SQLiteConnection(dbName))
            {
                using (var statement = connection.Prepare("SELECT COUNT(1) from " + tableName))
                {
                    statement.Step();

                    count = (long)statement[0];
                }
            }
            return count;
        }

        public static void Truncate(string dbName, string tableName)
        {
            ExecuteNonQuery(dbName, "DELETE FROM " + tableName);
        }

        public static void ExecuteNonQuery(string dbName, string sql)
        {
            using (var connection = new SQLiteConnection(dbName))
            {
                using (var statement = connection.Prepare(sql))
                {
                    if (statement.Step() != SQLiteResult.DONE)
                    {
                        throw new InvalidOperationException();
                    }
                }
            }
        }
    }
}
