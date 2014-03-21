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
            var contract = settings.ContractResolver.ResolveContract(typeof(T)) as JsonObjectContract;
            if (contract == null)
            {
                throw new ArgumentException(Properties.Resources.SQLiteStore_DefineTableTNotAnObject);
            }

            // create an empty object
            object theObject = contract.DefaultCreator();
            string json = JsonConvert.SerializeObject(theObject, settings);
            JObject item = JsonConvert.DeserializeObject<JObject>(json, settings);

            //// set default values so serialized version can be used to infer types
            SetDefaultId<T>(settings, item);
            SetNullValues(contract, item);

            store.DefineTable(tableName, item);
        }

        private static void SetDefaultId<T>(MobileServiceJsonSerializerSettings settings, JObject item)
        {
            JsonProperty idProperty = settings.ContractResolver.ResolveIdProperty(typeof(T));
            if (idProperty.PropertyType == typeof(long) || idProperty.PropertyType == typeof(int))
            {
                item[SystemProperties.Id] = 0;
            }
            else
            {
                item[SystemProperties.Id] = String.Empty;
            }
        }

        private static void SetNullValues(JsonObjectContract contract, JObject item)
        {
            IEnumerable<JProperty> nullProperties = item.Properties().Where(p => p.Value.Type == JTokenType.Null);
            foreach (JProperty itemProperty in nullProperties)
            {
                JsonProperty contractProperty = contract.Properties[itemProperty.Name];
                if (contractProperty.PropertyType == typeof(string))
                {
                    item[itemProperty.Name] = String.Empty;
                }
                else
                {
                    item[itemProperty.Name] = new JObject();
                }
            }
        }
    }
}
