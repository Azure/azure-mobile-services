// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        /// Gets the value of the id property from an instance.
        /// </summary>
        /// <param name="instance">
        /// The instance to get the id property value from.
        /// </param>
        /// <param name="ignoreCase">
        /// Set to true to find any variant spelling of the id is in the object
        /// </param>        
        /// <returns>
        /// The id property value.
        /// </returns>
        public object GetId(object instance, bool ignoreCase = false)
        {
            Debug.Assert(instance != null);

            object id = null;

            JObject jobject = instance as JObject;
            if (jobject != null)
            {
                bool gotID = false;
                JToken idToken = null;
                if (ignoreCase)
                {
                    gotID = jobject.TryGetValue(MobileServiceUrlBuilder.IdPropertyName, StringComparison.OrdinalIgnoreCase, out idToken);
                } 
                else 
                {
                    gotID = jobject.TryGetValue(MobileServiceUrlBuilder.IdPropertyName, out idToken);
                }

                if(gotID) {
                    JValue idValue = idToken as JValue;
                    if (idValue != null)
                    {
                        id = idValue.Value;
                    }
                }
            }
            else
            {
                JsonProperty idProperty = this.SerializerSettings.ContractResolver.ResolveIdProperty(instance.GetType());
                id = idProperty.ValueProvider.GetValue(instance);
            }

            return id;
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
        public bool IsDefaultId(object id)
        {
            return id == null ||
                   object.Equals(id, 0) ||
                   object.Equals(id, 0L) ||
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
        public void ClearId(object instance)
        {
            Debug.Assert(instance != null);

            JsonProperty idProperty = this.SerializerSettings.ContractResolver.ResolveIdProperty(instance.GetType());
            idProperty.ValueProvider.SetValue(instance, null);
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
            return json.Select(jtoken => jtoken.ToObject<T>(jsonSerializer));
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
        public T Deserialize<T>(string json)
        {
            Debug.Assert(json != null);

            return JsonConvert.DeserializeObject<T>(json, this.SerializerSettings);
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
        /// The JSON string.
        /// </param>
        /// <param name="instance">
        /// The instance to deserialize into.
        /// </param>
        public void Deserialize<T>(string json, T instance)
        {
            Debug.Assert(json != null);
            Debug.Assert(instance != null);

            JsonConvert.PopulateObject(json, instance, this.SerializerSettings);
        }

        /// <summary>
        /// Serializes an instance into a JSON string.
        /// </summary>
        /// <param name="instance">
        /// The instance to serialize.
        /// </param>
        /// <returns>
        /// The serialized JSON string.
        /// </returns>
        public string Serialize(object instance)
        {
            Debug.Assert(instance != null);

            return JsonConvert.SerializeObject(instance, this.SerializerSettings);
        }
    }
}
