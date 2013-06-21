// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// An <see cref="IContractResolver"/> implementation that is used with the
    /// <see cref="MobileServiceClient"/>.
    /// </summary>
    public class MobileServiceContractResolver : DefaultContractResolver
    {
        /// <summary>
        /// The property names that are all aliased to be the id of an instance.
        /// </summary>
        private static readonly string[] idPropertyNames = {"id", "Id", "ID"};

        /// <summary>
        /// A cache of the id <see cref="JsonProperty"/> for a given type. Used to
        /// get and set the id value of instances.
        /// </summary>
        private readonly Dictionary<Type, JsonProperty> idPropertyCache =  new Dictionary<Type, JsonProperty>();

        /// <summary>
        /// A cache of <see cref="JsonProperty"/> instances for a given 
        /// <see cref="MemberInfo"/> instance. Used to determine the property name
        /// to serialize from the <see cref="MemberInfo"/> used within an 
        /// <see cref="System.Linq.Expressions.Expression"/> instance.
        /// </summary>
        private readonly Dictionary<MemberInfo, JsonProperty> jsonPropertyCache = new Dictionary<MemberInfo, JsonProperty>();

        /// <summary>
        /// A cache of table names for a given Type that accounts for table renaming 
        /// via the DataContractAttribute, DataTableAttribute and/or the JsonObjectAttribute.
        /// </summary>
        private static readonly Dictionary<Type, string> tableNameCache = new Dictionary<Type, string>();

        /// <summary>
        /// Indicates if the property names should be camel-cased when serialized
        /// out into JSON.
        /// </summary>
        internal bool CamelCasePropertyNames { get; set; }

        /// <summary>
        /// Returns a table name for a type and accounts for table renaming 
        /// via the DataContractAttribute, DataTableAttribute and/or the JsonObjectAttribute.
        /// </summary>
        /// <param name="type">
        /// The type for which to return the table name.
        /// </param>
        /// <returns>
        /// The table name.
        /// </returns>
        public virtual string ResolveTableName(Type type)
        {
            // Lookup the Mobile Services name of the Type and use that
            string name = null;
            if (!tableNameCache.TryGetValue(type, out name))
            {
                // By default, use the type name itself
                name = type.Name;

                DataContractAttribute dataContractAttribute = type.GetCustomAttributes(typeof(DataContractAttribute), true)
                                                                  .FirstOrDefault() as DataContractAttribute;
                if (dataContractAttribute != null)
                {
                    if (!string.IsNullOrWhiteSpace(dataContractAttribute.Name))
                    {
                        name = dataContractAttribute.Name;
                    }
                }

                JsonContainerAttribute jsonContainerAttribute = type.GetCustomAttributes(typeof(JsonContainerAttribute), true)
                                                                    .FirstOrDefault() as JsonContainerAttribute; 
                if (jsonContainerAttribute != null)
                {
                    if (!string.IsNullOrWhiteSpace(jsonContainerAttribute.Title))
                    {
                        name = jsonContainerAttribute.Title;
                    }
                }

                DataTableAttribute dataTableAttribute = type.GetCustomAttributes(typeof(DataTableAttribute), true)
                                                            .FirstOrDefault() as DataTableAttribute;
                if (dataTableAttribute != null)
                {
                    if (!string.IsNullOrWhiteSpace(dataTableAttribute.Name))
                    {
                        name = dataTableAttribute.Name;
                    }
                }

                tableNameCache[type] = name;
            }

            return name;
        }

        /// <summary>
        /// Returns the id <see cref="JsonProperty"/> for the given type. The <see cref="JsonProperty"/>
        /// can be used to get/set the id value of an instance of the given type.
        /// </summary>
        /// <param name="type">
        /// The type for which to get the id <see cref="JsonProperty"/>.
        /// </param>
        /// <returns>
        /// The id <see cref="JsonProperty"/>.
        /// </returns>
        public virtual JsonProperty ResolveIdProperty(Type type)
        {
            JsonProperty property = null;
            if (!this.idPropertyCache.TryGetValue(type, out property))
            {
                JsonContract contract = ResolveContract(type);
                JsonObjectContract objectContract = contract as JsonObjectContract;
                if (objectContract != null)
                {
                    property = objectContract.Properties.Where(p => string.Equals(p.PropertyName, MobileServiceUrlBuilder.IdPropertyName)).FirstOrDefault();
                    if (property != null)
                    {
                        this.idPropertyCache[type] = property;
                    }
                }
            }

            if (property == null)
            {
                throw new InvalidOperationException(
                    string.Format(CultureInfo.InvariantCulture,
                        Resources.MobileServiceContractResolver_MemberNotFound,
                        MobileServiceUrlBuilder.IdPropertyName,
                        type.FullName));
            }

           return property;
        }

        /// <summary>
        /// Returns the <see cref="JsonProperty"/> for the given <see cref="MemberInfo"/> instance.
        /// The <see cref="JsonProperty"/> can be used to get information about how the 
        /// <see cref="MemberInfo"/> should be serialized.
        /// </summary>
        /// <param name="member">
        /// The <see cref="MemberInfo"/> for which to get the <see cref="JsonProperty"/>.
        /// </param>
        /// <returns>
        /// The <see cref="JsonProperty"/> for the given <see cref="MemberInfo"/> instance.
        /// </returns>
        public virtual JsonProperty ResolveProperty(MemberInfo member)
        {
            JsonProperty property = null;
            if (!this.jsonPropertyCache.TryGetValue(member, out property))
            {
                JsonContract contract = ResolveContract(member.DeclaringType);
                JsonObjectContract objectContract = contract as JsonObjectContract;
                if (objectContract != null)
                {
                    property = objectContract.Properties.Where(p => string.Equals(p.UnderlyingName, member.Name)).FirstOrDefault();
                    if (property != null)
                    {
                        this.jsonPropertyCache[member] = property;
                    }
                }
            }

            return property;
        }

        /// <summary>
        /// Returns the name that should be serialized into JSON for a given property name.
        /// </summary>
        /// <remarks>
        /// This method is overridden to support camel-casing the property names.
        /// </remarks>
        /// <param name="propertyName">
        /// The property name to be resolved.
        /// </param>
        /// <returns>
        /// The resolved property name.
        /// </returns>
        protected override string ResolvePropertyName(string propertyName)
        {
            if (this.CamelCasePropertyNames)
            {
                if (!string.IsNullOrEmpty(propertyName) && char.IsUpper(propertyName[0]))
                {
                    string original = propertyName;
                    propertyName = char.ToLower(propertyName[0], CultureInfo.InvariantCulture).ToString();
                    if (original.Length > 1)
                    {
                        propertyName += original.Substring(1);
                    }
                }
            }

            return propertyName;
        }

        /// <summary>
        /// Creates a <see cref="JsonObjectContract"/> that provides information about how
        /// the given type should be serialized to JSON.
        /// </summary>
        /// <remarks>
        /// This method is overridden in order to catch types that have 
        /// <see cref="DataMemberAttribute"/> on one or more members without having a 
        /// <see cref="DataContractAttribute"/> on the type itself. This used to be supported
        /// but no longer is and therefore an exception must be thrown for such types. The exception
        /// informs the developer about how to correctly attribute the type with the
        /// <see cref="JsonPropertyAttribute"/> instead of the <see cref="DataMemberAttribute"/>.
        /// </remarks>
        /// <param name="type">
        /// The type for which to return a <see cref="JsonObjectContract"/>.
        /// </param>
        /// <returns>
        /// The <see cref="JsonObjectContract"/> for the type.
        /// </returns>
        protected override JsonObjectContract CreateObjectContract(Type type)
        {
            JsonObjectContract contract = base.CreateObjectContract(type);

            DataContractAttribute dataContractAttribute = type.GetCustomAttributes(typeof(DataContractAttribute), true)
                                                              .FirstOrDefault() as DataContractAttribute;
            if (dataContractAttribute == null)
            {
                // Make sure the type does not have a base class with a [DataContract]
                Type baseTypeWithDataContract = type.BaseType;
                while (baseTypeWithDataContract != null)
                {
                    if (baseTypeWithDataContract.GetCustomAttributes(typeof(DataContractAttribute), true).Any())
                    {
                        break;
                    }
                    else
                    {
                        baseTypeWithDataContract = baseTypeWithDataContract.BaseType;
                    }
                } 

                if (baseTypeWithDataContract != null)
                {
                    throw new NotSupportedException(
                            string.Format(CultureInfo.InvariantCulture,
                            Resources.MobileServiceContractResolver_TypeNoDataContractButBaseWithDataContract,
                            type.FullName, 
                            baseTypeWithDataContract.FullName));
                }
                
                // [DataMember] attributes on members without a [DataContract]
                // attribute on the type itself used to be honored.  Now with JSON.NET, [DataMember]
                // attributes are ignored if there is no [DataContract] attribute on the type.
                // To ensure types are not serialized differently, an exception must be thrown if this 
                // type is using [DataMember] attributes without a [DataContract] on the type itself.
                if (type.GetMembers(BindingFlags.Public |
                                    BindingFlags.NonPublic |
                                    BindingFlags.FlattenHierarchy |
                                    BindingFlags.Instance) 
                         .Where( m => m.GetCustomAttributes(typeof(DataMemberAttribute), true)
                                       .FirstOrDefault() != null)
                         .Any())      
                {
                    throw new NotSupportedException(
                        string.Format(CultureInfo.InvariantCulture,
                        Resources.MobileServiceContractResolver_TypeWithDataMemberButNoDataContract,
                        type.FullName));
                }
            }

            return contract;
        }

        /// <summary>
        /// Creates a <see cref="JsonProperty"/> for a given <see cref="MemberInfo"/> instance.
        /// </summary>
        /// <remarks>
        /// This method is overridden in order set specialized <see cref="IValueProvider"/>
        /// implementations for certain property types. The date types (<see cref="DateTime"/>, 
        /// <see cref="DateTimeOffset"/>) require conversion to UTC dates on serialization and to
        /// local dates on deserialization. The numerical types (<see cref="long"/>, <see cref="ulong"/>,
        /// <see cref="decimal"/>) require checks to ensure that precision will not be lost on
        /// the server.
        /// </remarks>
        /// <param name="member">
        /// The <see cref="MemberInfo"/> for which to creat the <see cref="JsonProperty"/>.
        /// </param>
        /// <param name="memberSerialization">
        /// Specifies the member serialization options for the member.
        /// </param>
        /// <returns>
        /// A <see cref="JsonProperty"/> for a given <see cref="MemberInfo"/> instance.
        /// </returns>
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (property.PropertyType.IsValueType)
            {
                // The NullHandlingConverter will ensure that nulls get treated as the default value 
                // for value types.
                if (property.MemberConverter == null)
                {
                    property.MemberConverter = NullHandlingConverter.Instance;
                }
                else
                {
                    property.MemberConverter = new NullHandlingConverter(property.MemberConverter);
                }
            }

            return property;
        }

        /// <summary>
        /// Creates a collection of <see cref="JsonProperty"/> instances for the members of a given 
        /// type.
        /// </summary>
        /// <remarks>
        /// This method is overridden in order to handle the id property of the type. Because multiple
        /// property names ("id" with different casings) are all treated as the id property, we must
        /// ensure that there is one and only one id property for the type. Also, the id property
        /// should be ignored when it is the default or null value and it should always serialize to JSON
        /// with a lowercase 'id' name.
        /// </remarks>
        /// <param name="type">
        /// The type for which to create the collection of <see cref="JsonProperty"/> instances.
        /// </param>
        /// <param name="memberSerialization">
        /// Specifies the member serialization options for the type.
        /// </param>
        /// <returns>
        /// A collection of <see cref="JsonProperty"/> instances for the members of a given 
        /// type.
        /// </returns>
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);

            // If this type is for a known table, ensure that it has an id.
            if (tableNameCache.ContainsKey(type))
            {
                // Filter out properties that are not read/write
                properties = properties.Where(p => p.Writable).ToList();

                // Get the Id properties
                JsonProperty[] idProperties = properties.Where(p => idPropertyNames.Contains(p.PropertyName) && !p.Ignored).ToArray();

                // Ensure there is one and only one id property
                if (idProperties.Length > 1)
                {
                    throw new InvalidOperationException(
                        string.Format(CultureInfo.InvariantCulture,
                        Resources.MobileServiceContractResolver_SamePropertyName,
                        MobileServiceUrlBuilder.IdPropertyName,
                        type.FullName));
                }

                if (idProperties.Length < 1)
                {
                    throw new InvalidOperationException(
                        string.Format(CultureInfo.InvariantCulture,
                        Resources.MobileServiceContractResolver_MemberNotFound,
                        MobileServiceUrlBuilder.IdPropertyName,
                        type.FullName));
                }

                // The id property is special. It should always be lowercased and should be ignored when null 
                // or the default value
                JsonProperty idProperty = idProperties[0];
                idProperty.PropertyName = MobileServiceUrlBuilder.IdPropertyName;
                idProperty.NullValueHandling = NullValueHandling.Ignore;
                idProperty.DefaultValueHandling = DefaultValueHandling.Ignore;
            }

            return properties;
        }

        /// <summary>
        /// Creates the <see cref="IValueProvider"/> used by the serializer to get and set values from a member.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <returns>The <see cref="IValueProvider"/> used by the serializer to get and set values from a member.</returns>
        protected override IValueProvider CreateMemberValueProvider(MemberInfo member)
        {
            // always use the ReflectionValueProvider to make sure our behavior is consistent accross platforms.
            return new ReflectionValueProvider(member);
        }

        /// <summary>
        /// An implementation of <see cref="JsonConverter"/> to be used with value type
        /// properties to ensure that null values in a JSON payload are deserialized as
        /// the default value for the value type.
        /// </summary>
        private class NullHandlingConverter : JsonConverter
        {
            /// <summary>
            /// A singleton instance of the <see cref="NullHandlingConverter"/> because
            /// the <see cref="NullHandlingConverter"/> has no state and can be shared
            /// between many properties.
            /// </summary>
            public static NullHandlingConverter Instance = new NullHandlingConverter();

            /// <summary>
            /// Inner converter.
            /// </summary>
            private readonly JsonConverter inner;

            /// <summary>
            /// Handles nulls as default values.
            /// </summary>
            /// <param name="inner"></param>
            public NullHandlingConverter(JsonConverter inner = null)
            {
                this.inner = inner;
            }

            /// <summary>
            /// Indicates if the <see cref="NullHandlingConverter"/> can be used with
            /// a given type.
            /// </summary>
            /// <param name="objectType">
            /// The type under consideration.
            /// </param>
            /// <returns>
            /// <c>true</c> for value types and <c>false</c> otherwise.
            /// </returns>
            public override bool CanConvert(Type objectType)
            {
                return objectType.IsValueType || (inner != null && inner.CanConvert(objectType));
            }

            /// <summary>
            /// Reads the JSON representation of the object.
            /// The reason why this works is 
            /// </summary>
            /// <param name="reader">
            /// The <see cref="JsonReader"/> to read from.
            /// </param>
            /// <param name="objectType">
            /// The type of the object.
            /// </param>
            /// <param name="existingValue">
            /// The exisiting value of the object being read.
            /// </param>
            /// <param name="serializer">
            /// The calling serializer.
            /// </param>
            /// <returns>
            /// The object value.
            /// </returns>
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if(inner != null && inner.CanConvert(objectType) && inner.CanRead)
                {
                    return inner.ReadJson(reader, objectType, existingValue, serializer);
                }
                else if (reader.TokenType == JsonToken.Null)
                {
                    //create default values if null is not a valid value
                    if (objectType.IsValueType)
                    {
                        //create default value
                        return Activator.CreateInstance(objectType);
                    }
                    return null;

                }
                else
                {
                    return serializer.Deserialize(reader, objectType);
                }
            }

            /// <summary>
            /// Writes the JSON representation of the object.
            /// </summary>
            /// <param name="writer">
            /// The <see cref="JsonWriter"/> to write to.
            /// </param>
            /// <param name="value">
            /// The value to write.
            /// </param>
            /// <param name="serializer">
            /// The calling serializer.
            /// </param>
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                if (inner != null && inner.CanWrite)
                {
                    inner.WriteJson(writer, value, serializer);
                }
                else
                {
                    serializer.Serialize(writer, value);
                }
            }
        }
    }
}
