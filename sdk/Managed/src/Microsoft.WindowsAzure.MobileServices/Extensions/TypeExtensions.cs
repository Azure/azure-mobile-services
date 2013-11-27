// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Microsoft.WindowsAzure.MobileServices
{
    internal static class TypeExtensions
    {
        private static Type nullableType = typeof(Nullable<>);

        /// <summary>
        /// Returns the underlying type in case of a Nullable.
        /// </summary>
        /// <param name="thisType"></param>
        /// <returns></returns>
        public static Type UnwrapNullable(this Type thisType)
        {
            return thisType.GetTypeInfo().IsGenericType && thisType.GetGenericTypeDefinition() == nullableType
                ?
                Nullable.GetUnderlyingType(thisType)
                :
                thisType;
        }
    }
}
