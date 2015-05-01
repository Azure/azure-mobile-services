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
                throw new ArgumentException("The generic type T is not an object.");
            }
            if (contract.DefaultCreator == null)
            {
                throw new ArgumentException("The generic type T does not have parameterless constructor.");
            }

            // create an empty object
            object theObject = contract.DefaultCreator();
            SetEnumDefault(contract, theObject);

            JObject item = ConvertToJObject(settings, theObject);

            //// set default values so serialized version can be used to infer types
            SetIdDefault<T>(settings, item);
            SetNullDefault(contract, item);

            store.DefineTable(tableName, item);
        }

        private static void SetEnumDefault(JsonObjectContract contract, object theObject)
        {
            foreach (JsonProperty contractProperty in contract.Properties)
            {
                if (contractProperty.PropertyType.GetTypeInfo().IsEnum)
                {
                    object firstValue = Enum.GetValues(contractProperty.PropertyType)
                                            .Cast<object>()
                                            .FirstOrDefault();
                    if (firstValue != null)
                    {
                        contractProperty.ValueProvider.SetValue(theObject, firstValue);
                    }
                }
            }
        }

        private static JObject ConvertToJObject(MobileServiceJsonSerializerSettings settings, object theObject)
        {
            string json = JsonConvert.SerializeObject(theObject, settings);
            JObject item = JsonConvert.DeserializeObject<JObject>(json, settings);
            return item;
        }

        private static void SetIdDefault<T>(MobileServiceJsonSerializerSettings settings, JObject item)
        {
            JsonProperty idProperty = settings.ContractResolver.ResolveIdProperty(typeof(T));
            if (idProperty.PropertyType == typeof(long) || idProperty.PropertyType == typeof(int))
            {
                item[MobileServiceSystemColumns.Id] = 0;
            }
            else
            {
                item[MobileServiceSystemColumns.Id] = String.Empty;
            }
        }

        private static void SetNullDefault(JsonObjectContract contract, JObject item)
        {
            foreach (JProperty itemProperty in item.Properties().Where(i => i.Value.Type == JTokenType.Null))
            {
                JsonProperty contractProperty = contract.Properties[itemProperty.Name];
                if (contractProperty.PropertyType == typeof(string) || contractProperty.PropertyType == typeof(Uri))
                {
                    item[itemProperty.Name] = String.Empty;
                }
                else if (contractProperty.PropertyType == typeof(byte[]))
                {
                    item[itemProperty.Name] = new byte[0];
                }
                else if (contractProperty.PropertyType.GetTypeInfo().IsGenericType && contractProperty.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    item[itemProperty.Name] = new JValue(Activator.CreateInstance(contractProperty.PropertyType.GenericTypeArguments[0]));
                }
                else
                {
                    item[itemProperty.Name] = new JObject();
                }
            }
        }
    }
}
