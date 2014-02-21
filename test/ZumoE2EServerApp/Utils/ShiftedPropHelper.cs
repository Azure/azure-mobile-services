// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Http.OData;

namespace System.Web.Http.OData
{
    public static class DeltaExtensions
    {
        public static Delta<T> FixShiftedProps<T>(this Delta<T> patch,
            Expression<Func<T, object>> publicProperty,
            Expression<Func<T, object>> dbProperty) where T : class, new()
        {
            var publicName = ((MemberExpression)publicProperty.Body).Member.Name;
            var dbName = ((MemberExpression)dbProperty.Body).Member.Name;

            if (patch.GetChangedPropertyNames().Contains(publicName))
            {
                if (patch.GetChangedPropertyNames().Contains(dbName))
                {
                    throw new InvalidOperationException();
                }

                var tmp = new T();

                // Get the value from the Delta
                object val = null;
                if (!patch.TryGetPropertyValue(publicName, out val))
                {
                    throw new InvalidOperationException();
                }

                // Put it into the tmp object.
                tmp.GetType().GetProperty(publicName).GetSetMethod().Invoke(tmp, new object[] { val });

                // Pull the transformed value out the other property in the pair.
                var otherVal = tmp.GetType().GetProperty(dbName).GetGetMethod().Invoke(tmp, null);

                // And put that value into the Delta.
                if (!patch.TrySetPropertyValue(dbName, otherVal))
                {
                    throw new InvalidOperationException();
                }
            }

            return patch;
        }
    }
}