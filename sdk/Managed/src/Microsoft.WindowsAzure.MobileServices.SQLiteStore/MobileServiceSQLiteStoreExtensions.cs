// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Microsoft.WindowsAzure.MobileServices.SQLiteStore
{
    /// <summary>
    ///  Provides extension methods on <see cref="MobileServiceSQLiteStore"/>.
    /// </summary>
    public static class MobileServiceSQLiteStoreExtensions
    {
        public static void DefineTable<T>(this MobileServiceSQLiteStore store)
        {
            var settings = new MobileServiceJsonSerializerSettings();
            DefineTable<T>(store, settings);
        }

        public static void DefineTable<T>(this MobileServiceSQLiteStore store, MobileServiceJsonSerializerSettings settings)
        {
            string tableName = settings.ContractResolver.ResolveTableName(typeof(T));
            JsonContract contract = settings.ContractResolver.ResolveContract(typeof(T));

            // create an empty object
            object theObject = contract.DefaultCreator();
            // set default values so serialized version can be used to infer types
            SetDefaultValues<T>(theObject);

            string json = JsonConvert.SerializeObject(theObject, settings);
            
            // read the serialized object
            JObject item = JObject.Parse(json);
            store.DefineTable(tableName, item);
        }

        private static void SetDefaultValues<T>(object theObject)
        {
            TypeInfo typeInfo = typeof(T).GetTypeInfo();

            foreach (MemberInfo member in typeInfo.DeclaredMembers)
            {
                var property = member as PropertyInfo;
                if (property != null && property.PropertyType == typeof(string) && property.CanWrite)
                {
                    property.SetValue(theObject, String.Empty);
                    continue;
                }
                var field = member as FieldInfo;
                if (field != null && field.FieldType == typeof(string))
                {
                    field.SetValue(theObject, String.Empty);
                }
            }
        }
    }
}
