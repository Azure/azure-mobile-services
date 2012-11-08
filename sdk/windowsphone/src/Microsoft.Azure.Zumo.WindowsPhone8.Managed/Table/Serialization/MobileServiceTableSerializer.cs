// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Serializes objects to and from JSON.
    /// </summary>
    public static class MobileServiceTableSerializer
    {
        /// <summary>
        /// Deserialize a JSON value into an instance of type T.
        /// </summary>
        /// <typeparam name="T">
        /// The type of object to deserialize.
        /// </typeparam>
        /// <param name="value">The JSON value.</param>
        /// <returns>The deserialized instance.</returns>
        public static T Deserialize<T>(JToken value)
        {
            T instance = Activator.CreateInstance<T>();
            Deserialize(value, instance);
            return instance;
        }

        /// <summary>
        /// Deserialize a JSON value into an instance.
        /// </summary>
        /// <param name="value">The JSON value.</param>
        /// <param name="instance">The instance to deserialize into.</param>
        public static void Deserialize(JToken value, object instance)
        {
            Deserialize(value, instance, false);
        }

        /// <summary>
        /// Deserialize a JSON value into an instance.
        /// </summary>
        /// <param name="value">The JSON value.</param>
        /// <param name="instance">The instance to deserialize into.</param>
        /// <param name="ignoreCustomSerialization">
        /// A value to indicate whether or not custom serialization should be
        /// ignored if the instance implements
        /// ICustomMobileServiceTableSerialization.  This flag is used by
        /// implementations of ICustomMobileServiceTableSerialization that want
        /// to invoke the default serialization behavior.
        /// </param>
        public static void Deserialize(JToken value, object instance, bool ignoreCustomSerialization)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            else if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            // If the instance implements
            // ICustomMobileServiceTableSerialization, allow it to handle its
            // own deserialization.
            if (!ignoreCustomSerialization)
            {
                ICustomMobileServiceTableSerialization custom = instance as ICustomMobileServiceTableSerialization;
                if (custom != null)
                {
                    custom.Deserialize(value);
                    return;
                }
            }

            // Get the Mobile Services specific type info
            SerializableType type = SerializableType.Get(instance.GetType());

            // Get the object to deserialize
            JObject obj = value.AsObject();
            if (obj == null)
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources.MobileServiceTableSerializer_Deserialize_NeedObject,
                        value.Type),
                    "value");
            }

            // Create a set of required members that we can remove from as we
            // process their values from the object.  If there's anything left
            // in the set after processing all of the values, then we weren't
            // given all of our required properties.
            HashSet<SerializableMember> requiredMembers =
                new HashSet<SerializableMember>(type.Members.Values.Where(m => m.IsRequired));

            // Walk through all of the members defined in the object and
            // deserialize them into the instance one at a time
            foreach (KeyValuePair<string, JToken> assignment in obj.GetPropertyValues().OrderBy(a => type.GetMemberOrder(a.Key)))
            {
                // Look up the instance member corresponding to the JSON member
                SerializableMember member = null;
                if (type.Members.TryGetValue(assignment.Key, out member))
                {
                    // Remove the member from the required list (does nothing
                    // if it wasn't present)
                    requiredMembers.Remove(member);

                    // Convert the property value into a CLR value using either
                    // the converter or a standard simple JSON mapping.  This
                    // will throw for JSON arrays or objects.  Also note that
                    // we'll still run the value returned from the converter
                    // through the ChangeType call below to make writing
                    // converters a little easier (but it should be a no-op
                    // for most folks anyway).
                    object propertyValue = null;
                    if (member.Converter != null)
                    {
                        propertyValue = member.Converter.ConvertFromJson(assignment.Value);
                    }
                    else if (!assignment.Value.TryConvert(out propertyValue))
                    {
                        throw new ArgumentException(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                Resources.MobileServiceTableSerializer_Deserialize_CannotDeserializeValue,
                                assignment.Value.ToString(),
                                type.Type.FullName,
                                member.MemberName),
                            "value");
                    }
                    
                    // Change the type of the value to the desired property
                    // type (mostly to handle things like casting issues) and
                    // set the value on the instance.
                    object convertedValue = TypeExtensions.ChangeType(propertyValue, member.Type);
                    member.SetValue(instance, convertedValue);
                }
            }

            // Ensure we were provided all of the required properties.
            if (requiredMembers.Count > 0)
            {
                throw new SerializationException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources.MobileServiceTableSerializer_Deserialize_MissingRequired,
                        type.Type.FullName,
                        string.Join(", ", requiredMembers.Select(m => m.Name))));
            }
        }

        /// <summary>
        /// Serialize an instance to a JSON value.
        /// </summary>
        /// <param name="instance">The instance to serialize.</param>
        /// <returns>The serialized JSON value.</returns>
        public static JToken Serialize(object instance)
        {
            return Serialize(instance, false);
        }

        /// <summary>
        /// Serialize an instance to a JSON value.
        /// </summary>
        /// <param name="instance">The instance to serialize.</param>
        /// <returns>The serialized JSON value.</returns>
        /// <param name="ignoreCustomSerialization">
        /// A value to indicate whether or not custom serialization should be
        /// ignored if the instance implements
        /// ICustomMobileServiceTableSerialization.  This flag is used by
        /// implementations of ICustomMobileServiceTableSerialization that
        /// want to invoke the default serialization behavior.
        /// </param>
        public static JToken Serialize(object instance, bool ignoreCustomSerialization)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            // If the instance implements
            // ICustomMobileServiceTableSerialization, allow it to handle its
            // own serialization.
            if (!ignoreCustomSerialization)
            {
                ICustomMobileServiceTableSerialization custom = instance as ICustomMobileServiceTableSerialization;
                if (custom != null)
                {
                    return custom.Serialize();
                }
            }

            // Get the Mobile Services specific type info
            SerializableType type = SerializableType.Get(instance.GetType());

            // Create a new JSON object to represent the instance
            JObject obj = new JObject();

            foreach (SerializableMember member in type.Members.Values.OrderBy(m => m.Order))
            {
                // Get the value to serialize
                object value = member.GetValue(instance);
                if (member.Converter != null)
                {
                    // If there's a user defined converter, we can apply that
                    // and set the value directly.
                    obj.Set(member.Name, member.Converter.ConvertToJson(value));
                }
                else if (member == type.IdMember &&
                    SerializableType.IsDefaultIdValue(value))
                {
                    // Special case the ID member so we don't write out any
                    // value if wasn't set (i.e., has a 0 or null value).  This
                    // allows us flexibility for the type of the ID column
                    // which is currently a long in SQL but could someday be a
                    // GUID in something like  a document database.  At some
                    // point we might also change the server to quietly ignore
                    // default values for the ID column on insert.
                }
                else if (!obj.TrySet(member.Name, value))
                {
                    throw new ArgumentException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            Resources.MobileServiceTableSerializer_Serialize_UnknownType,
                            member.Name,
                            member.Type.FullName,
                            type.Type.Name),
                        "instance");
                }
            }

            return obj;
        }
    }
}
