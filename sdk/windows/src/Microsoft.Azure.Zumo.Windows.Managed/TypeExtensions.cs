// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Extensions to make accessing type info easier (especially given all of
    /// the new reflection changes).
    /// </summary>
    internal static partial class TypeExtensions
    {
        /// <summary>
        /// Get a sequence of types from the given type up through the base
        /// type.  This is helpful given that all of the new reflection APIs
        /// only provide DeclaredFoos, but you can use SelectMany with this
        /// sequence to get all of the inherited Foos.
        /// </summary>
        /// <param name="type">The type to get the hierarchy for.</param>
        /// <returns>Sequence describing the type hierarchy.</returns>
        public static IEnumerable<TypeInfo> GetBaseTypesAndSelf(this Type type)
        {
            Debug.Assert(type != null, "type cannot be null!");

            TypeInfo info = type.GetTypeInfo();
            while (info != null)
            {
                yield return info;
                info = info.BaseType != null ?
                    info.BaseType.GetTypeInfo() :
                    null;
            }
        }

        /// <summary>
        /// Get the properties for a given type.
        /// </summary>
        /// <param name="type">The type to get properties of.</param>
        /// <param name="inherit">
        /// A value indicating whether to get inherited properties.
        /// </param>
        /// <param name="publicOnly">
        /// A value indicating whether to get only public properties.
        /// </param>
        /// <returns>Sequence of properties for the given type.</returns>
        public static IEnumerable<PropertyInfo> GetProperties(this Type type, bool inherit, bool publicOnly)
        {
            Debug.Assert(type != null, "type cannot be null!");
            IEnumerable<PropertyInfo> properties =
                (inherit ?
                    type.GetBaseTypesAndSelf().SelectMany(t => t.DeclaredProperties) :
                    type.GetTypeInfo().DeclaredProperties)
                .Where(
                    p =>
                    {
                        // Ignore indexers
                        ParameterInfo[] parameters = p.GetIndexParameters();
                        return parameters == null || parameters.Length == 0;
                    });
            return publicOnly ?
                properties.Where(p =>
                    p.GetMethod != null && p.GetMethod.IsPublic &&
                    p.SetMethod != null && p.SetMethod.IsPublic) :
                properties;
        }

        /// <summary>
        /// Get the fields for a given type.
        /// </summary>
        /// <param name="type">The type to get fields of.</param>
        /// <param name="inherit">
        /// A value indicating whether to get inherited fields.
        /// </param>
        /// <param name="publicOnly">
        /// A value indicating whether to get only public fields.
        /// </param>
        /// <returns>Sequence of fields for the given type.</returns>
        public static IEnumerable<FieldInfo> GetFields(this Type type, bool inherit, bool publicOnly)
        {
            Debug.Assert(type != null, "type cannot be null!");

            IEnumerable<FieldInfo> fields = inherit ?
                type.GetBaseTypesAndSelf().SelectMany(t => t.DeclaredFields) :
                type.GetTypeInfo().DeclaredFields;
            return publicOnly ?
                fields.Where(f => f.IsPublic) :
                fields;
        }

        /// <summary>
        /// Gets a value indicating whether a member has an attribute applied.
        /// </summary>
        /// <typeparam name="T">The type of attribute.</typeparam>
        /// <param name="member">The member.</param>
        /// <param name="inherit">
        /// A value indicating whether we should check for attributes inherited
        /// from base classes.
        /// </param>
        /// <returns>
        /// A value indicating whether a member has an attribute applied.
        /// </returns>
        public static bool Has<T>(this MemberInfo member, bool inherit = true)
            where T : Attribute
        {
            Debug.Assert(member != null, "member cannot be null!");
            return member.GetCustomAttribute<T>(inherit) != null;
        }
        
        /// <summary>
        /// Get the element type for a sequence type.
        /// </summary>
        /// <param name="type">The sequence type.</param>
        /// <returns>The element type for the sequence or null.</returns>
        public static Type GetUnderlyingType(this Type type)
        {
            Debug.Assert(type != null, "type cannot be null.");

            Type closedType = GetIEnumerableClosedType(type);
            return closedType != null ?
                closedType.GetTypeInfo().GenericTypeArguments[0] :
                type;
        }

        /// <summary>
        /// Find the first closed implementation of IEnumerable of T.
        /// </summary>
        /// <param name="type">The type to search.</param>
        /// <returns>
        /// The first closed implementation of IEnumerable of T.
        /// </returns>
        private static Type GetIEnumerableClosedType(Type type)
        {
            TypeInfo info = type != null ? type.GetTypeInfo() : null;

            if (type == null || type == typeof(string))
            {
                return null;
            }
            else if (type.IsArray)
            {
                return typeof(IEnumerable<>).MakeGenericType(type.GetElementType());
            }
            else if (info.IsGenericType)
            {
                foreach (Type arg in info.GenericTypeArguments)
                {
                    Type enumerableType = typeof(IEnumerable<>).MakeGenericType(arg);
                    if (enumerableType.GetTypeInfo().IsAssignableFrom(info))
                    {
                        return enumerableType;
                    }
                }
            }
            
            foreach (Type interfaceType in info.ImplementedInterfaces)
            {
                Type enumerableType = GetIEnumerableClosedType(interfaceType);
                if (enumerableType != null)
                {
                    return enumerableType;
                }
            }

            if (info.BaseType != null && info.BaseType != typeof(object))
            {
                return GetIEnumerableClosedType(info.BaseType);
            }

            return null;
        }

        /// <summary>
        /// Change the value of an object from one type to another.
        /// </summary>
        /// <param name="value">The value to change.</param>
        /// <param name="desiredType">The desired type.</param>
        /// <returns>The converted value.</returns>
        public static object ChangeType(object value, Type desiredType)
        {
            Debug.Assert(desiredType != null, "desiredType cannot be null.");

            // Ignore null values because there's nothing we need to convert
            if (value == null)
            {
                return null;
            }

            // If the desired type is nullable, convert the value (which we've
            // already verified isn't null) into the underlying type and then
            // convert that result into the nullable type.
            Type underlyingType = Nullable.GetUnderlyingType(desiredType);
            if (underlyingType != null)
            {
                value = ChangeType(value, underlyingType);
                return AsNullable(value, underlyingType);
            }

            // Try the primitive types (note that Convert.ToFoo will allow us
            // to cast and truncate precision in a lot of cases where the
            // Convert.ChangeType will throw an InvalidCastException).
            RuntimeTypeHandle handle = desiredType.TypeHandle;
            if (handle.Equals(typeof(bool).TypeHandle))
            {
                return Convert.ToBoolean(value, CultureInfo.InvariantCulture);
            }
            else if (handle.Equals(typeof(int).TypeHandle))
            {
                return Convert.ToInt32(value, CultureInfo.InvariantCulture);
            }
            else if (handle.Equals(typeof(uint).TypeHandle))
            {
                return Convert.ToUInt32(value, CultureInfo.InvariantCulture);
            }
            else if (handle.Equals(typeof(sbyte).TypeHandle))
            {
                return Convert.ToSByte(value, CultureInfo.InvariantCulture);
            }
            else if (handle.Equals(typeof(byte).TypeHandle))
            {
                return Convert.ToByte(value, CultureInfo.InvariantCulture);
            }
            else if (handle.Equals(typeof(short).TypeHandle))
            {
                return Convert.ToInt16(value, CultureInfo.InvariantCulture);
            }
            else if (handle.Equals(typeof(ushort).TypeHandle))
            {
                return Convert.ToUInt16(value, CultureInfo.InvariantCulture);
            }
            else if (handle.Equals(typeof(long).TypeHandle))
            {
                return Convert.ToInt64(value, CultureInfo.InvariantCulture);
            }
            else if (handle.Equals(typeof(ulong).TypeHandle))
            {
                return Convert.ToUInt64(value, CultureInfo.InvariantCulture);
            }
            else if (handle.Equals(typeof(double).TypeHandle))
            {
                return Convert.ToDouble(value, CultureInfo.InvariantCulture);
            }
            else if (handle.Equals(typeof(float).TypeHandle))
            {
                return Convert.ToSingle(value, CultureInfo.InvariantCulture);
            }
            else if (handle.Equals(typeof(Decimal).TypeHandle))
            {
                return Convert.ToDecimal(value, CultureInfo.InvariantCulture);
            }
            else if (handle.Equals(typeof(char).TypeHandle))
            {
                return Convert.ToChar(value, CultureInfo.InvariantCulture);
            }
            else if (handle.Equals(typeof(string).TypeHandle))
            {
                return Convert.ToString(value, CultureInfo.InvariantCulture);
            }
            else if (handle.Equals(typeof(DateTime).TypeHandle))
            {
                return Convert.ToDateTime(value, CultureInfo.InvariantCulture);
            }
            else if (handle.Equals(typeof(DateTimeOffset).TypeHandle))
            {
                return new DateTimeOffset(Convert.ToDateTime(value, CultureInfo.InvariantCulture));
            }

            // If all else fails, default to ChangeType
            return Convert.ChangeType(value, desiredType, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Convert a value to its nullable equivalent.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="underlyingType">The type to make nullable.</param>
        /// <returns>The value cast to a nullable type.</returns>
        private static object AsNullable(object value, Type underlyingType)
        {
            RuntimeTypeHandle underlyingHandle = underlyingType.TypeHandle;
            if (underlyingHandle.Equals(typeof(bool).TypeHandle))
            {
                return (bool?)value;
            }
            else if (underlyingHandle.Equals(typeof(int).TypeHandle))
            {
                return (int?)value;
            }
            else if (underlyingHandle.Equals(typeof(uint).TypeHandle))
            {
                return (uint?)value;
            }
            else if (underlyingHandle.Equals(typeof(sbyte).TypeHandle))
            {
                return (sbyte?)value;
            }
            else if (underlyingHandle.Equals(typeof(byte).TypeHandle))
            {
                return (byte?)value;
            }
            else if (underlyingHandle.Equals(typeof(short).TypeHandle))
            {
                return (short?)value;
            }
            else if (underlyingHandle.Equals(typeof(ushort).TypeHandle))
            {
                return (ushort?)value;
            }
            else if (underlyingHandle.Equals(typeof(long).TypeHandle))
            {
                return (long?)value;
            }
            else if (underlyingHandle.Equals(typeof(ulong).TypeHandle))
            {
                return (ulong?)value;
            }
            else if (underlyingHandle.Equals(typeof(double).TypeHandle))
            {
                return (double?)value;
            }
            else if (underlyingHandle.Equals(typeof(float).TypeHandle))
            {
                return (float?)value;
            }
            else if (underlyingHandle.Equals(typeof(Decimal).TypeHandle))
            {
                return (Decimal?)value;
            }
            else if (underlyingHandle.Equals(typeof(char).TypeHandle))
            {
                return (char?)value;
            }
            else if (underlyingHandle.Equals(typeof(DateTime).TypeHandle))
            {
                return (DateTime?)value;
            }

            // If all else fails, default to ChangeType
            return Convert.ChangeType(
                value,
                typeof(Nullable<>).MakeGenericType(underlyingType),
                CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Gets a value indicating whether the type is assignable to the
        /// argument type  (i.e., can an instance of type be assigned to a
        /// variable of T?).
        /// </summary>
        /// <typeparam name="T">The type to check for assignability</typeparam>
        /// <param name="type">
        /// The type to see if it can be assigned to T.
        /// </param>
        /// <returns>
        /// A value indicating whether the type is assignable to the argument
        /// type.
        /// </returns>
        public static bool IsAssignableTo<T>(this Type type)
        {
            if (type == null)
            {
                return false;
            }

            return typeof(T).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo());
        }

        /// <summary>
        /// Convert a value into an OData literal.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The corresponding OData literal.</returns>
        public static string ToODataConstant(object value)
        {
            if (value == null)
            {
                return "null";
            }

            // Special case a few primtive types
            RuntimeTypeHandle handle = value.GetType().TypeHandle;
            if (handle.Equals(typeof(bool).TypeHandle))
            {
                // Make sure booleans are lower case
                return ((bool)value).ToString().ToLower();
            }
            else if (handle.Equals(typeof(Byte).TypeHandle))
            {
                // Format bytes as hex pairs
                return ((byte)value).ToString("X2", CultureInfo.InvariantCulture);
            }
            else if (handle.Equals(typeof(long).TypeHandle))
            {
                return value.ToString() + "L";
            }
            else if (handle.Equals(typeof(float).TypeHandle))
            {
                return value.ToString() + "f";
            }
            else if (handle.Equals(typeof(Decimal).TypeHandle))
            {
                return value.ToString() + "M";
            }
            else if (handle.Equals(typeof(string).TypeHandle))
            {
                // Escape the string constant by: (1) replacing single quotes with a 
                // pair of single quotes, and (2) Uri escaping with percent encoding
                string text = value as string ?? string.Empty;
                string textEscaped = Uri.EscapeDataString(text.Replace("'", "''"));
                return string.Format(CultureInfo.InvariantCulture, "'{0}'", textEscaped);
            }
            else if (handle.Equals(typeof(char).TypeHandle))
            {
                // Escape the char constant by: (1) replacing a single quote with a 
                // pair of single quotes, and (2) Uri escaping with percent encoding
                char ch = (char)value;
                string charEscaped = Uri.EscapeDataString(ch == '\'' ? "''" : ch.ToString());
                return string.Format(CultureInfo.InvariantCulture, "'{0}'", charEscaped);
            }
            else if (handle.Equals(typeof(DateTime).TypeHandle))
            {
                // Format dates in the official OData format
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "datetime'{0}'",
                    ((DateTime)value).ToRoundtripDateString());
            }
            else if (handle.Equals(typeof(DateTimeOffset).TypeHandle))
            {
                // Format dates in the official OData format (note: the server
                // doesn't recgonize datetimeoffset'...', so we'll just convert
                // to a UTC date.
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "datetime'{0}'",
                    ((DateTimeOffset)value).DateTime.ToRoundtripDateString());
            }
            else if (handle.Equals(typeof(Guid).TypeHandle))
            {
                // GUIDs are in registry format without the { }s
                Guid guid = (Guid)value;
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "guid'{0}'",
                    guid.ToString().TrimStart('{').TrimEnd('}'));
            }

            // We'll just ToString everything else
            return value.ToString();
        }

        /// <summary>
        /// Find a method by type and name.
        /// </summary>
        /// <param name="type">The type with the method.</param>
        /// <param name="name">The name of the method.</param>
        /// <param name="parameterTypes">The parameters to the method.</param>
        /// <returns>A sequence of MethodInfos.</returns>
        private static IEnumerable<MethodInfo> GetMethods(Type type, string name, Type[] parameterTypes)
        {
            return GetBaseTypesAndSelf(type)
                .SelectMany(t => t.DeclaredMethods.Where(m => m.Name == name))
                .Where(m => 
                {
                    ParameterInfo[] parameters = m.GetParameters();
                    if (parameterTypes.Length != parameters.Length)
                    {
                        return false;
                    }

                    for (int i = 0; i < parameterTypes.Length; i++)
                    {
                        if (parameterTypes[i] != parameters[i].ParameterType)
                        {
                            return false;
                        }
                    }

                    return true;
                });
        }

        /// <summary>
        /// Find an instance method info by type and name.
        /// </summary>
        /// <param name="type">The type declaring the method.</param>
        /// <param name="name">The name of the method.</param>
        /// <param name="parameterTypes">
        /// Types of the method parameters.
        /// </param>
        /// <returns>The desired MethodInfo or null.</returns>
        public static MethodInfo FindInstanceMethod(Type type, string name, params Type[] parameterTypes)
        {
            return
                GetMethods(type, name, parameterTypes)
                .Where(m => !m.IsStatic)
                .SingleOrDefault();
        }

        /// <summary>
        /// Find an instance property info by type and name.
        /// </summary>
        /// <param name="type">The type declaring the property.</param>
        /// <param name="name">The name of the property.</param>
        /// <returns>
        /// The desired property's getter's MethodInfo or null.
        /// </returns>
        public static MemberInfo FindInstanceProperty(Type type, string name)
        {
            return
                GetBaseTypesAndSelf(type)
                .SelectMany(t => t.DeclaredProperties.Where(
                    p => p.Name == name && p.CanRead && !p.GetMethod.IsStatic))
                .Cast<MemberInfo>()
                .SingleOrDefault();            
        }

        /// <summary>
        /// Find a static method info by type and name.
        /// </summary>
        /// <param name="type">The type declaring the method.</param>
        /// <param name="name">The name of the method.</param>
        /// <param name="parameterTypes">
        /// Types of the method parameters.
        /// </param>
        /// <returns>The desired MethodInfo or null.</returns>
        public static MethodInfo FindStaticMethod(Type type, string name, params Type[] parameterTypes)
        {
            return
                GetMethods(type, name, parameterTypes)
                .Where(m => m.IsStatic)
                .SingleOrDefault();
        }
    }
}
