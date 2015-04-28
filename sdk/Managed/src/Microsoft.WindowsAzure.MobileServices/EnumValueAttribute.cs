// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Reflection;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Attribute used to associate a name to an enum value.
    /// </summary>
    class EnumValueAttribute : Attribute
    {
        /// <summary>
        /// The string associated to the enum value.
        /// </summary>
        public string Value { private set; get; }

        public EnumValueAttribute(string value)
        {
            this.Value = value;
        }

        /// <summary>
        /// Returns the associated string for a specific enum value.
        /// </summary>
        /// <param name="value">The enum value.</param>
        /// <returns>The string associated with the value, or <code>null</code> if not found.</returns>
        internal static string GetValue<TEnum>(TEnum value)
        {
            FieldInfo fi = typeof(MobileServiceFeatures).GetTypeInfo().GetDeclaredField(value.ToString());
            string result = null;
            if (fi != null)
            {
                EnumValueAttribute eva = fi.GetCustomAttribute<EnumValueAttribute>();
                if (eva != null)
                {
                    result = eva.Value;
                }
            }

            return result;
        }
    }
}
