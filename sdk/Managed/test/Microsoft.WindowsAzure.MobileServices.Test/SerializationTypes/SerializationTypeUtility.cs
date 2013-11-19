// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Reflection;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    public class SerializationTypeUtility
    {
        public static bool AreEqual(object one, object two)
        {
            if (one == null && two == null)
            {
                return true;
            }
            else if (one == null || two == null)
            {
                return false;
            }

            Type oneType = one.GetType();
            Type twoType = two.GetType();

            if (oneType != twoType)
            {
                return false;
            }

            if (oneType != typeof(long) && 
                oneType != typeof(string))
            {
                foreach (PropertyInfo property in oneType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (!AreEqual(property.GetValue(one, null), property.GetValue(two, null)))
                    {
                        return false;
                    }
                }

                foreach (FieldInfo field in oneType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (!field.Name.StartsWith("<") && !field.IsStatic) // To ensure we don't set backing fields or static fields
                    {
                        if (!AreEqual(field.GetValue(one), field.GetValue(two)))
                        {
                            return false;
                        }
                    }
                }
            }
            else if (one.ToString() != two.ToString())
            {
                return false;
            }

            return true;
        }
    }
}
