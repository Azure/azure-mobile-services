// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------


namespace Microsoft.WindowsAzure.MobileServices.SQLiteStore
{
    internal static class SqlColumnType
    {
        // base types
        public const string Integer = "INTEGER";
        public const string Text = "TEXT";
        public const string None = "NONE";
        public const string Real = "REAL";
        public const string Numeric = "NUMERIC";

        // type aliases
        public const string Boolean = "BOOLEAN"; // NUMERIC
        public const string DateTime = "DATETIME"; // NUMERIC
        public const string Float = "FLOAT"; // REAL
        public const string Blob = "BLOB"; // NONE


        // custom types = NONE
        public const string Guid = "GUID";
        public const string Json = "JSON";
        public const string Uri = "URI";
        public const string TimeSpan = "TIMESPAN";
    }
}
