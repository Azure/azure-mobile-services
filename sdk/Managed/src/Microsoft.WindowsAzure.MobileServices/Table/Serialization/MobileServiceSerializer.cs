﻿// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Provides serialization and deserialization for a 
    /// <see cref="MobileServiceClient"/>.
    /// </summary>
    internal class MobileServiceSerializer
    {
        /// <summary>
        /// The version system property as a string
        /// </summary>
        internal static readonly string VersionSystemPropertyString = MobileServiceSystemProperties.Version.ToString().ToLowerInvariant();

        /// <summary>
        /// The version system property as a string
        /// </summary>
        internal static readonly string UpdatedAtSystemPropertyString = MobileServiceSystemProperties.UpdatedAt.ToString().ToLowerInvariant();

        /// <summary>
        /// The version system property as a string
        /// </summary>
        internal static readonly string CreatedAtSystemPropertyString = MobileServiceSystemProperties.CreatedAt.ToString().ToLowerInvariant();

        /// <summary>
        /// The deleted system property as a string
        /// </summary>
        internal static readonly string DeletedSystemPropertyString = MobileServiceSystemProperties.Deleted.ToString().ToLowerInvariant();

        /// <summary>
        /// A regex for validating string ids
        /// </summary>
        private static Regex stringIdValidationRegex = new Regex(@"([\u0000-\u001F]|[\u007F-\u009F]|[""\+\?\\\/\`]|^\.{1,2}$)");

        /// <summary>
        /// The long type.
        /// </summary>
        private static Type longType = typeof(long);

        /// <summary>
        /// The int type.
        /// </summary>
        private static Type intType = typeof(int);

        /// <summary>
        /// The max length of valid string ids.
        /// </summary>
        internal const int MaxStringIdLength = 255;

        /// <summary>
        /// The JSON serializer settings to use with the 
        /// <see cref="MobileServiceSerializer"/>.
        /// </summary>
        public MobileServiceJsonSerializerSettings SerializerSettings { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="MobileServiceSerializer"/>
        /// class.
        /// </summary>
        public MobileServiceSerializer()
        {
            this.SerializerSettings = new MobileServiceJsonSerializerSettings();
        }

        /// <summary>
        /// Returns the system properties for a given type.
        /// </summary>
        /// <param name="type">The type for which to get the system properties.</param>
        /// <returns>
        /// The system properties for a given type.
        /// </returns>
        public MobileServiceSystemProperties GetSystemProperties(Type type)
        {
            return this.SerializerSettings.ContractResolver.ResolveSystemProperties(type);
        }

        /// <summary>
        /// Tries to get the value of the id property from an instance.
        /// </summary>
        /// <param name="instance">
        /// The instance to get the id property value from.
        /// </param>
        /// <param name="ignoreCase">
        /// Set to true to find any variant spelling of the id is in the object
        /// </param> 
        /// <param name="id">
        /// The value of the id property from the instance if the instance has an 
        /// id property.
        /// </param>
        /// <returns>
        /// Returns 'true' if the instance had an id property; 'false' otherwise.
        /// </returns>
        public static bool TryGetId(JObject instance, bool ignoreCase, out object id)
        {
            bool gotId = false;
            JToken idToken = null;

            id = null;

            if (ignoreCase)
            {
                gotId = instance.TryGetValue(MobileServiceSystemColumns.Id, StringComparison.OrdinalIgnoreCase, out idToken);
            }
            else
            {
                gotId = instance.TryGetValue(MobileServiceSystemColumns.Id, out idToken);
            }

            if (gotId)
            {
                JValue idValue = idToken as JValue;
                if (idValue == null)
                {
                    gotId = false;
                }
                else
                {
                    id = idValue.Value;
                }
            }

            return gotId;
        }

        /// <summary>
        /// Gets the value of the id property from an instance. Also ensures that the
        /// id is valid.
        /// </summary>
        /// <param name="instance">
        /// The instance to get the id property value from.
        /// </param>
        /// <param name="ignoreCase">
        /// Set to true to find any variant spelling of the id is in the object
        /// </param> 
        /// <param name="allowDefault">
        /// Indicates if a defualt value for the id is considered valid.
        /// </param> 
        /// <returns>
        /// The id property value.
        /// </returns>
        public static object GetId(JObject instance, bool ignoreCase = false, bool allowDefault = false)
        {
            Debug.Assert(instance != null);

            object id = null;
            bool gotID = TryGetId(instance, ignoreCase, out id);

            // Check that the id is present but incorrectly cased
            if (!gotID && !ignoreCase && TryGetId(instance, true, out id))
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "The casing of the '{0}' property is invalid.",
                        MobileServiceSystemColumns.Id),
                     "instance");
            }

            if (gotID)
            {
                EnsureValidId(id, allowDefault);
            }
            else if (!allowDefault)
            {
                throw new ArgumentException(
                       string.Format(
                           CultureInfo.InvariantCulture,
                           "Expected {0} member not found.",
                           MobileServiceSystemColumns.Id),
                       "instance");
            }

            return id;
        }

        /// <summary>
        /// Removes all system properties (name start with '__') from the instance
        /// if the instance is determined to have a string id and therefore be for table that
        /// supports system properties.
        /// </summary>
        /// <param name="instance">The instance to remove the system properties from.</param>
        /// <param name="version">Set to the value of the version system property before it is removed.</param>
        /// <param name="propertiesToKeep">The system properties to keep</param>        /// <returns>
        /// The instance with the system properties removed.
        /// </returns>
        public static JObject RemoveSystemProperties(JObject instance, out string version, MobileServiceSystemProperties propertiesToKeep = MobileServiceSystemProperties.None)
        {
            version = null;
            var systemProperties = new String[]{MobileServiceSerializer.CreatedAtSystemPropertyString, MobileServiceSerializer.DeletedSystemPropertyString, MobileServiceSerializer.VersionSystemPropertyString, MobileServiceSerializer.UpdatedAtSystemPropertyString}.AsEnumerable<String>();

            bool haveCloned = false;
            foreach (JProperty property in instance.Properties())
            {
                if (systemProperties.Contains(property.Name))
                {
                    // We don't want to alter the original jtoken passed in by the caller
                    // so if we find a system property to remove, we have to clone first
                    if (!haveCloned)
                    {
                        instance = instance.DeepClone() as JObject;
                        haveCloned = true;
                    }

                    if (String.Equals(property.Name, MobileServiceSystemColumns.Version, StringComparison.OrdinalIgnoreCase))
                    {
                        version = (string)instance[property.Name];
                        if ((propertiesToKeep & MobileServiceSystemProperties.Version) == MobileServiceSystemProperties.Version)
                        {
                            continue;
                        }
                    }
                    else if (propertiesToKeep == MobileServiceSystemProperties.All ||
                        IsKeptSystemProperty(property.Name, propertiesToKeep, MobileServiceSystemProperties.CreatedAt, MobileServiceSerializer.CreatedAtSystemPropertyString) ||
                        IsKeptSystemProperty(property.Name, propertiesToKeep, MobileServiceSystemProperties.UpdatedAt, MobileServiceSerializer.UpdatedAtSystemPropertyString))
                    {
                        continue;
                    }

                    instance.Remove(property.Name);
                }
            }

            return instance;
        }

        /// <summary>
        /// Checks if the given system property should be kept or removed from the returned item
        /// </summary>
        /// <param name="propertyName">
        /// Name of the system property to see if it should be removed
        /// </param> 
        /// <param name="propertiesToKeep">
        /// The list of system properties to be kept
        /// </param> 
        /// <param name="property">
        /// Type of the system property to check
        /// </param> 
        /// <param name="systemPropertyName">
        /// Name of the actual system property to look for
        /// </param> 
        private static bool IsKeptSystemProperty(string propertyName, MobileServiceSystemProperties propertiesToKeep, MobileServiceSystemProperties property, string systemPropertyName)
        {
            if ((propertiesToKeep & property) == property &&
                    String.Equals(propertyName, systemPropertyName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Ensures that the id is valid. Throws an exception if the id is not valid.
        /// </summary>
        /// <param name="allowDefault">
        /// Indicates if a defualt value for the id is considered valid.
        /// </param> 
        /// <param name="id">The id to validate.</param>
        public static void EnsureValidId(object id, bool allowDefault = false)
        {
            if (id == null)
            {
                if (!allowDefault)
                {
                    throw new InvalidOperationException("The id can not be null or an empty string.");
                }
                return;
            }

            if (id is string)
            {
                EnsureValidStringId(id, allowDefault);
            }
            if (IsIntegerId(id))
            {
                EnsureValidIntId(id, allowDefault);
            }
        }                

        public static bool IsIntegerId(JsonProperty idProperty)
        {
            return IsIntegerId(idProperty.PropertyType);
        }        

        public static bool IsIntegerId(object id)
        {
            return id != null && IsIntegerId(id.GetType());
        }

        public static bool IsIntegerId(Type idType)
        {
            bool isIntegerIdType = idType == longType ||
                                   idType == intType;
            return isIntegerIdType;
        }

        private static void EnsureValidIntId(object id, bool allowDefault)
        {
            long longId = Convert.ToInt64(id);
            if (longId < 0 ||
                (!allowDefault && longId == 0))
            {
                throw new InvalidOperationException(
                    string.Format("The integer id '{0}' is not a positive integer value.", longId));
            }
        }

        public static void EnsureValidStringId(object id, bool allowDefault = false)
        {
            string stringId = (string)id;
            if (stringId.Length > MaxStringIdLength)
            {
                throw new InvalidOperationException(
                    string.Format("The string id '{0}' is longer than the max string id length of {1} characters.",
                        stringId,
                        MaxStringIdLength));
            }
            else if (stringIdValidationRegex.IsMatch(stringId))
            {
                throw new InvalidOperationException(
                    string.Format("The string id '{0}' is invalid. An id must not contain any control characters or the characters \",+,?,\\,/,`.", stringId));
            }
            else if (!allowDefault && stringId.Length == 0)
            {
                throw new InvalidOperationException("The id can not be null or an empty string.");
            }
        }

        /// <summary>
        /// Ensures that the id type is valid for the given type parameter. Throws an exception if the id is not valid.
        /// </summary>
        /// <typeparam name="T">The type to validate the id against.</typeparam>
        /// <param name="id">The id to validate.</param>
        public void EnsureValidIdForType<T>(object id)
        {
            Type idPropertyType = this.GetIdPropertyType<T>();

            if (id != null)
            {
                Type idType = id.GetType();
                bool isInvalid = false;

                // Allow an int id to be passed in for item types that have a long id and
                // vice-versa
                if (idPropertyType == longType || idPropertyType == intType)
                {
                    if (idType != longType && idType != intType)
                    {
                        isInvalid = true;
                    }

                }
                else if (!idPropertyType.GetTypeInfo().IsAssignableFrom(idType.GetTypeInfo()))
                {
                    isInvalid = true;
                }

                if (isInvalid)
                {
                    throw new InvalidOperationException(
                    string.Format("The id parameter type '{0}' is invalid for looking up items of type '{1}'.",
                        id.GetType().FullName,
                        typeof(T).FullName));
                }

            }
            else if (idPropertyType.GetTypeInfo().IsValueType)
            {
                throw new InvalidOperationException(
                     string.Format("The id parameter type '{0}' is invalid for looking up items of type '{1}'.",
                         "<null>",
                         typeof(T).FullName));
            }

        }

        /// <summary>
        /// Sets the instance's id property value to the default id value.
        /// </summary>
        /// <param name="instance">
        /// The instance on which to clear the id property.
        /// </param>
        public static void SetIdToDefault(JObject instance)
        {
            if (instance != null)
            {
                instance[MobileServiceSystemColumns.Id] = null;
            }
        }

        /// <summary>
        /// Gets the value of the id property from an instance.
        /// </summary>
        /// <param name="instance">
        /// The instance to get the id property value from.
        /// </param>
        /// <param name="ignoreCase">
        /// Set to true to find any variant spelling of the id is in the object
        /// </param>
        /// <param name="allowDefault">
        /// Indicates if a defualt value for the id is considered valid.
        /// </param> 
        /// <returns>
        /// The id property value.
        /// </returns>
        public object GetId(object instance, bool ignoreCase = false, bool allowDefault = false)
        {
            Debug.Assert(instance != null);

            object id = null;

            JObject jobject = instance as JObject;
            if (jobject != null)
            {
                id = GetId(jobject, ignoreCase, allowDefault);
            }
            else
            {
                JsonProperty idProperty = this.SerializerSettings.ContractResolver.ResolveIdProperty(instance.GetType());
                id = idProperty.ValueProvider.GetValue(instance);
                EnsureValidId(id, allowDefault);
            }

            return id;
        }

        /// <summary>
        /// Returns the type of the Id property for items of the given type.
        /// </summary>
        /// <typeparam name="T">The type of the item that should have an id property.</typeparam>
        /// <returns>
        /// The type of the Id property for items of the given type, or null if the type does not
        /// have an Id property.
        /// </returns>
        public Type GetIdPropertyType<T>(bool throwIfNotFound = true)
        {
            Type idPropertyType = null;
            JsonProperty idProperty = this.SerializerSettings.ContractResolver.ResolveIdProperty(typeof(T), throwIfNotFound);

            if (idProperty != null)
            {
                idPropertyType = idProperty.PropertyType;
            }

            return idPropertyType;
        }

        /// <summary>
        /// Indicates if the id value is the default id value.
        /// </summary>
        /// <param name="id">
        /// The id value to determine if it is the default.
        /// </param>
        /// <returns>
        /// <c>true</c> if the id value is the default value;
        /// <c>false</c> otherwise.
        /// </returns>
        public static bool IsDefaultId(object id)
        {
            return id == null ||
                   object.Equals(id, 0L) ||
                   object.Equals(id, 0) ||
                   (id is string && string.IsNullOrEmpty((string)id)) ||
                   object.Equals(id, 0.0) ||
                   object.Equals(id, 0.0F) ||
                   object.Equals(id, 0.0M);
        }

        /// <summary>
        /// Sets the instance's id property value to the default id value.
        /// </summary>
        /// <param name="instance">
        /// The instance on which to clear the id property.
        /// </param>
        public void SetIdToDefault(object instance)
        {
            Debug.Assert(instance != null);

            JObject jobject = instance as JObject;
            if (jobject != null)
            {
                SetIdToDefault(jobject);
            }
            else
            {
                JsonProperty idProperty = this.SerializerSettings.ContractResolver.ResolveIdProperty(instance.GetType());
                idProperty.ValueProvider.SetValue(instance, null);
            }
        }

        /// <summary>
        /// Deserializes an array of JSON values into a collection of objects.
        /// </summary>
        /// <param name="json">
        /// The JSON array.
        /// </param>
        /// <param name="type">
        /// The type to deserialize objects into.
        /// </param>
        /// <returns>
        /// The collection of deserialized instances.
        /// </returns>
        public IEnumerable<object> Deserialize(JArray json, Type type)
        {
            Debug.Assert(json != null);
            Debug.Assert(type != null);

            JsonSerializer jsonSerializer = this.SerializerSettings.GetSerializerFromSettings();
            return json.Select(jtoken => jtoken.ToObject(type, jsonSerializer));
        }

        /// <summary>
        /// Deserializes an array of JSON values into a collection of objects.
        /// </summary>
        /// <typeparam name="T">
        /// The type of objects to deserialize to.
        /// </typeparam>
        /// <param name="json">
        /// The JSON array.
        /// </param>
        /// <returns>
        /// The collection of deserialized instances.
        /// </returns>
        public IEnumerable<T> Deserialize<T>(JArray json)
        {
            Debug.Assert(json != null);

            JsonSerializer jsonSerializer = this.SerializerSettings.GetSerializerFromSettings();
            return json.Select(jtoken => Deserialize<T>(jtoken, jsonSerializer));
        }

        /// <summary>
        /// Deserializes a JSON string into an instance of type T.
        /// </summary>
        /// <typeparam name="T">
        /// The type of object to deserialize.
        /// </typeparam>
        /// <param name="json">
        /// The JSON string.
        /// </param>
        /// <returns>
        /// The deserialized instance.
        /// </returns>
        public T Deserialize<T>(JToken json)
        {
            Debug.Assert(json != null);

            JsonSerializer jsonSerializer = this.SerializerSettings.GetSerializerFromSettings();
            return Deserialize<T>(json, jsonSerializer);
        }        

        /// <summary>
        /// Deserializes a JSON string into an instance.
        /// </summary>
        /// <remarks>
        /// This method uses JSON.Net's <see cref="JsonConvert.PopulateObject(string, object)"/>
        /// API to set the member values on the instance from the values
        /// in the JSON string.
        /// </remarks>
        /// <typeparam name="T">
        /// The type of object to deserialize into.
        /// </typeparam>
        /// <param name="json">
        /// The JSON.
        /// </param>
        /// <param name="instance">
        /// The instance to deserialize into.
        /// </param>
        public void Deserialize<T>(JToken json, T instance)
        {
            Debug.Assert(json != null);
            Debug.Assert(instance != null);

            TransformSerializationException<T>(() =>
            {
                JsonConvert.PopulateObject(json.ToString(), instance, this.SerializerSettings);
            }, json);
        }

        /// <summary>
        /// Serializes an instance into a JSON string.
        /// </summary>
        /// <param name="instance">
        /// The instance to serialize.
        /// </param>
        /// <returns>
        /// The instance serialized into a JToken.
        /// </returns>
        public JToken Serialize(object instance)
        {
            Debug.Assert(instance != null);

            JsonSerializer jsonSerializer = this.SerializerSettings.GetSerializerFromSettings();
            return JToken.FromObject(instance, jsonSerializer);
        }

        private T Deserialize<T>(JToken json, JsonSerializer jsonSerializer)
        {
            T result = default(T);
            TransformSerializationException<T>(() =>
            {
                result = json.ToObject<T>(jsonSerializer);
            }, json);
            return result;
        }

        private void TransformSerializationException<T>(Action action, JToken token)
        {
            try
            {
                action();
            }
            catch (JsonSerializationException ex)
            {
                var obj = token as JObject;
                if (obj == null)
                {
                    throw;
                }
              
                object id;
                bool idTokenIsString = TryGetId(obj, true, out id) && id.GetType() == typeof(string);

                JsonProperty idProperty = this.SerializerSettings.ContractResolver.ResolveIdProperty(typeof(T), throwIfNotFound: false);
                bool idPropertyIsInteger = idProperty != null && IsIntegerId(idProperty);
                
                if (idTokenIsString && idPropertyIsInteger)
                {
                    throw new JsonSerializationException(ex.Message + Environment.NewLine + "You might be affected by Mobile Services latest changes to support string Ids. For more details: http://go.microsoft.com/fwlink/?LinkId=330396", ex);
                }

                throw;
            }
        }
    }
}
