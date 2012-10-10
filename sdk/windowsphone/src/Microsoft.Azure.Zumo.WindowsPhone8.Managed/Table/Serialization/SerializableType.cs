// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Represents the type information needed to serialize instance of a given
    /// type with the MobileServiceTableSerializer.
    /// </summary>
    [DebuggerDisplay("{Type.FullName,nq}")]
    internal class SerializableType
    {
        /// <summary>
        /// The name of the property representing the ID of an element if we're
        /// searching for the ID member of a POCO.  We'll search for this text
        /// as well as upper and lowercase variants.
        /// </summary>
        private const string IdPropertyName = "Id";

        /// <summary>
        /// Type cache mapping types to SerializableType info.
        /// </summary>
        private static Dictionary<Type, SerializableType> typeCache =
            new Dictionary<Type, SerializableType>();

        /// <summary>
        /// Converter cache mapping types to IDataMemberJsonConverter.
        /// </summary>
        private static Dictionary<Type, IDataMemberJsonConverter> converterCache =
            new Dictionary<Type, IDataMemberJsonConverter>();

        /// <summary>
        /// Initializes a new instance of the SerializableType class.
        /// </summary>
        /// <param name="type">The type to serialize.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "We normalize to upper first and then to lower.  These strings are also constant and not subject to mapping issues that uppercase solves.")]
        private SerializableType(Type type)
        {
            Debug.Assert(type != null, "type cannot be null.");

            this.Type = type;
            this.Members = new Dictionary<string, SerializableMember>();

            // The table name mapped by this type is the same as the type
            // by default unless a DataTable attribute is supplied or a DataContract
            // is being used in which the name is being set.
            DataTableAttribute dataTableAttribute =
                type.GetTypeInfo().GetCustomAttribute<DataTableAttribute>(true);

            DataContractAttribute dataContractAttribute =
                type.GetTypeInfo().GetCustomAttribute<DataContractAttribute>(true);

            if (dataTableAttribute != null)
            {
                this.TableName = dataTableAttribute.Name;
            }
            else if (dataContractAttribute != null &&
                     !string.IsNullOrWhiteSpace(dataContractAttribute.Name))
            {
                this.TableName = dataContractAttribute.Name;
            }
            else
            {
                this.TableName = type.Name;
            }

            // Add all of the Mobile Services properties and fields to Members
            // (TODO: We're using Members.Add(n, m) instead of Members[n] = m
            // so that any duplicates will throw an exception.  We can consider
            // whether we should throw a friendlier exception at some point in
            // the future, but this will prevent duplicate names)
            bool hasContract = dataContractAttribute != null;
            foreach (PropertyInfo property in GetSerializableMembers(hasContract, type.GetProperties))
            {
                SerializableMember member = new SerializableMember(property);
                this.Members.Add(member.Name, member);
            }
            foreach (FieldInfo field in GetSerializableMembers(hasContract, type.GetFields))
            {
                SerializableMember member = new SerializableMember(field);
                this.Members.Add(member.Name ?? member.MemberName, member);
            }

            // Ensure we have a valid ID field (and check a couple of variants
            // to enable POCOs).
            SerializableMember id = null;
            if (!this.Members.TryGetValue(MobileServiceTable.IdPropertyName, out id) &&
                !this.Members.TryGetValue(IdPropertyName, out id) &&
                !this.Members.TryGetValue(IdPropertyName.ToUpperInvariant(), out id) &&
                !this.Members.TryGetValue(IdPropertyName.ToLowerInvariant(), out id))
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources.SerializableType_Ctor_MemberNotFound,
                        MobileServiceTable.IdPropertyName,
                        type.FullName));
            }

            // Coerce the name of the ID property to the required format if
            // we've got a POCO with a slightly different name.
            if (id.Name != MobileServiceTable.IdPropertyName)
            {
                this.Members.Remove(id.Name);
                id.Name = MobileServiceTable.IdPropertyName;
                this.Members.Add(id.Name, id);
            }
            this.IdMember = id;
        }

        /// <summary>
        /// Gets the type represented by the SerializeType.
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// Gets or sets the name of the table this type maps to.
        /// </summary>
        public string TableName { get; private set; }

        /// <summary>
        /// Get the members of the type that should be serialized.
        /// </summary>
        public IDictionary<string, SerializableMember> Members { get; private set; }

        /// <summary>
        /// Gets the special Id member of the type.
        /// </summary>
        public SerializableMember IdMember { get; private set; }

        /// <summary>
        /// Get all of the members eligible for serialization.
        /// </summary>
        /// <typeparam name="T">PropertyInfo or FieldInfo.</typeparam>
        /// <param name="hasDataContract">
        /// A value indicating whether the type has a DataContract applied
        /// (which changes the semantics for which members we include).
        /// </param>
        /// <param name="getMembers">
        /// Either the GetProperties or GetFields extension methods mapping
        /// (inherited, publicOnly) => { Members... }.
        /// </param>
        /// <returns>
        /// A sequence of all members eligible for serialization.
        /// </returns>
        private static IEnumerable<T> GetSerializableMembers<T>(bool hasDataContract, Func<bool, bool, IEnumerable<T>> getMembers)
            where T : MemberInfo
        {
            // A member is eligible for serialization if
            if (hasDataContract)
            {
                // 1. The type has a [DataContract] attribute applied and the
                //    member has a [DataMember] attribute applied (regardless
                //    of visibility).
            
                // getMembers(inherited, publicOnly)
                return getMembers(true, false).Where(p =>
                    p.Has<DataMemberAttribute>() &&
                    !p.Has<IgnoreDataMemberAttribute>());
            }
            else
            {
                // 2. The type does not have a [DataContract] attribute applied
                //    and the member is public or has a [DataMember] atribute
                return Enumerable.Concat(
                    // getMembers(inherited, publicOnly)
                    getMembers(true, false).Where(p =>
                        p.Has<DataMemberAttribute>() &&
                        !p.Has<IgnoreDataMemberAttribute>()),
                    getMembers(true, true).Where(p =>
                        !p.Has<DataMemberAttribute>() &&
                        !p.Has<IgnoreDataMemberAttribute>()));
            }
        }

        /// <summary>
        /// Get the member ordering for a given member name.  This is used to
        /// deserialize members in the desired order.
        /// </summary>
        /// <param name="memberName">Name of the member.</param>
        /// <returns>The ordering for the member.</returns>
        public int GetMemberOrder(string memberName)
        {
            Debug.Assert(!string.IsNullOrEmpty(memberName), "memberName cannot be null or empty!");

            // Get the order from a member if we can find one, otherwise we'll
            // default to MaxValue so that any "extra" properties are
            // serialized after the known properties.
            SerializableMember member = null;
            return this.Members.TryGetValue(memberName, out member) && member.Order >= 0 ?
                member.Order :
                int.MaxValue;
        }

        /// <summary>
        /// Get the SerializableType info for a type.
        /// </summary>
        /// <param name="type">The type to serialize.</param>
        /// <returns>The SerializableType info.</returns>
        public static SerializableType Get(Type type)
        {
            Debug.Assert(type != null, "type cannot be null!");

            // Use the cached type info or create on demand if needed
            SerializableType typeInfo = null;
            if (!typeCache.TryGetValue(type, out typeInfo))
            {
                typeInfo = new SerializableType(type);
                typeCache[type] = typeInfo;
            }
            return typeInfo;
        }

        /// <summary>
        /// Get an instance of an IDataMemberJsonConverter.
        /// </summary>
        /// <param name="type">The type of the converter.</param>
        /// <returns>An instance of the converter.</returns>
        public static IDataMemberJsonConverter GetConverter(Type type)
        {
            Debug.Assert(type != null, "type cannot be null!");

            // Use the cached converter or create on demand if needed
            IDataMemberJsonConverter converter = null;
            if (!converterCache.TryGetValue(type, out converter))
            {
                converter = Activator.CreateInstance(type) as IDataMemberJsonConverter;
                
                // Ensure the converter actually implements
                // IDataMemberJsonConverter
                if (converter == null)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            Resources.SerializableType_GetConverter_DoesNotImplementConverter,
                            type.FullName));
                }

                converterCache[type] = converter;
            }
            return converter;
        }

        /// <summary>
        /// Get the SerializableMember for a member.
        /// </summary>
        /// <param name="member">The member to lookup.</param>
        /// <returns>The SerializableMember for the member.</returns>
        public static SerializableMember GetMember(MemberInfo member)
        {
            Debug.Assert(member != null, "member cannot be nul!");

            // Look up the declaring type and then search its members for a
            // matching member name.
            SerializableType type = Get(member.DeclaringType);
            return
                type
                .Members
                .Where(m => m.Value.MemberName == member.Name)
                .Select(m => m.Value)
                .FirstOrDefault();
        }

        /// <summary>
        /// Check if the value for an ID property is the default value.
        /// </summary>
        /// <param name="value">The value of the ID property.</param>
        /// <returns>
        /// A value indicating whether the ID property has its default value.
        /// </returns>
        public static bool IsDefaultIdValue(object value)
        {
            // We'll consider either null or 0 to be default
            return value == null ||
                object.Equals(value, 0) ||
                object.Equals(value, 0L);
        }
    }
}
