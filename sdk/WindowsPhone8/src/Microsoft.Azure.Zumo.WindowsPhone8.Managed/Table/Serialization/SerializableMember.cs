// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Represents a member that can be serialized via
    /// MobileServiceTableSerializer.
    /// </summary>
    [DebuggerDisplay("{Type.Name,nq} {Name,nq}")]
    internal class SerializableMember
    {
        /// <summary>
        /// Initializes a new instance of the SerializableMember class.
        /// </summary>
        /// <param name="member">The member to serialize.</param>
        private SerializableMember(MemberInfo member)
        {
            Debug.Assert(member != null, "member cannot be null!");
            Debug.Assert(!member.Has<IgnoreDataMemberAttribute>(), "member should be ignored!");

            this.Name = member.Name; // May be overwritten by DataMember below
            this.MemberName = member.Name;
            
            DataMemberAttribute dataMemberAttribute = member.GetCustomAttribute<DataMemberAttribute>(true);
            if (dataMemberAttribute != null)
            {
                if (!string.IsNullOrWhiteSpace(dataMemberAttribute.Name))
                {
                    this.Name = dataMemberAttribute.Name;
                }

                this.IsRequired = dataMemberAttribute.IsRequired;
                this.Order = dataMemberAttribute.Order;
            }

            DataMemberJsonConverterAttribute converter = member.GetCustomAttribute<DataMemberJsonConverterAttribute>(true);
            if (converter != null && converter.ConverterType != null)
            {
                this.Converter = SerializableType.GetConverter(converter.ConverterType);
            }
        }

        /// <summary>
        /// Initializes a new instance of the SerializableMember class.
        /// </summary>
        /// <param name="property">The property to serialize.</param>
        public SerializableMember(PropertyInfo property)
            : this(property as MemberInfo)
        {
            Debug.Assert(property != null, "property cannot be null!");

            this.Type = property.PropertyType;
            this.GetValue = property.GetValue;
            this.SetValue = property.SetValue;
        }

        /// <summary>
        /// Initializes a new instance of the SerializableMember class.
        /// </summary>
        /// <param name="field">The field to serialize.</param>
        public SerializableMember(FieldInfo field)
            : this(field as MemberInfo)
        {
            Debug.Assert(field != null, "field cannot be null!");

            this.Type = field.FieldType;
            this.GetValue = field.GetValue;
            this.SetValue = field.SetValue;
        }

        /// <summary>
        /// Gets the serialized name of the member.
        /// </summary>
        /// <remarks>
        /// This will only be set if we're changing the name of an Id property
        /// to enable POCO scenarios.
        /// </remarks>
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the name of the member.
        /// </summary>
        public string MemberName { get; private set; }

        /// <summary>
        /// Gets the type of the member.
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the member is required for
        /// deserialization.  An exception will be thrown if this member is not
        /// present.
        /// </summary>
        public bool IsRequired { get; private set; }

        /// <summary>
        /// Gets or sets the order of serialization and deserialization of a
        /// member.
        /// </summary>
        public int Order { get; private set; }

        /// <summary>
        /// Gets a converter used to convert this member to/from JSON.
        /// </summary>
        public IDataMemberJsonConverter Converter { get; private set; }

        /// <summary>
        /// Gets a function that can be passed an instance to get the value
        /// of this member.
        /// </summary>
        public Func<object, object> GetValue { get; private set; }

        /// <summary>
        /// Gets a function that can be passed an instance and a value to set
        /// the value of this member.
        /// </summary>
        public Action<object, object> SetValue { get; private set; }
    }
}
