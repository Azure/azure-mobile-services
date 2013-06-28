// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------    

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.WindowsAzure.MobileServices.TestFramework;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    [Tag("MemberInfoKey")]
    public class MemberInfoKeyTests : TestBase
    {
        private static IEnumerable<Type> GetBaseTypesAndSelf(Type type)
        {
            Debug.Assert(type != null, "type cannot be null!");

            while (type != null)
            {
                yield return type;
                type = type.BaseType;
            }
        }

        private static IEnumerable<MethodInfo> GetMethods(Type type, string name, Type[] parameterTypes)
        {
            return GetBaseTypesAndSelf(type)
                .SelectMany(t => t.GetMethods().Where(m => m.Name == name))
                .Where(m => m.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameterTypes));
        }

        private static MethodInfo FindInstanceMethod(Type type, string name, params Type[] parameterTypes)
        {
            return
                GetMethods(type, name, parameterTypes)
                .Where(m => !m.IsStatic)
                .SingleOrDefault();
        }

        private static MemberInfo FindInstanceProperty(Type type, string name)
        {
            return
                GetBaseTypesAndSelf(type)
                .SelectMany(t => t.GetProperties().Where(
                    p => p.Name == name && p.CanRead && !p.GetGetMethod().IsStatic))
                .Cast<MemberInfo>()
                .SingleOrDefault();
        }

        private static MethodInfo FindStaticMethod(Type type, string name, params Type[] parameterTypes)
        {
            return
                GetMethods(type, name, parameterTypes)
                .Where(m => m.IsStatic)
                .SingleOrDefault();
        }

        [TestMethod]
        public void CorrectlyMatchesInstanceMemberInfos()
        {
             Dictionary<MethodInfo, MemberInfoKey> instanceMethods = 
                 new Dictionary<MethodInfo, MemberInfoKey>() {
                { FindInstanceMethod(typeof(string), "ToLower"), 
                    new MemberInfoKey(typeof(string), "ToLower", true, true) },
                { FindInstanceMethod(typeof(string), "ToLowerInvariant"),
                    new MemberInfoKey(typeof(string), "ToLowerInvariant", true, true) },
                { FindInstanceMethod(typeof(string), "ToUpper"),
                    new MemberInfoKey(typeof(string), "ToUpper", true, true) },
                { FindInstanceMethod(typeof(string), "ToUpperInvariant"),
                    new MemberInfoKey(typeof(string), "ToUpperInvariant", true, true) },
                { FindInstanceMethod(typeof(string), "Trim"),
                    new MemberInfoKey(typeof(string), "Trim", true, true) },
                { FindInstanceMethod(typeof(string), "StartsWith", typeof(string)),
                    new MemberInfoKey(typeof(string), "StartsWith", true, true, typeof(string)) },
                { FindInstanceMethod(typeof(string), "EndsWith", typeof(string)),
                    new MemberInfoKey(typeof(string), "EndsWith", true, true, typeof(string)) },
                { FindInstanceMethod(typeof(string), "IndexOf", typeof(string)),
                    new MemberInfoKey(typeof(string), "IndexOf", true, true, typeof(string)) },
                { FindInstanceMethod(typeof(string), "IndexOf", typeof(char)),
                    new MemberInfoKey(typeof(string), "IndexOf", true, true, typeof(char)) },
                { FindInstanceMethod(typeof(string), "Contains", typeof(string)),
                    new MemberInfoKey(typeof(string), "Contains", true, true, typeof(string)) },
                { FindInstanceMethod(typeof(string), "Replace", typeof(string), typeof(string)),
                    new MemberInfoKey(typeof(string), "Replace", true, true, typeof(string), typeof(string)) },
                { FindInstanceMethod(typeof(string), "Replace", typeof(char), typeof(char)),
                    new MemberInfoKey(typeof(string), "Replace", true, true, typeof(char), typeof(char)) },
                { FindInstanceMethod(typeof(string), "Substring", typeof(int)),
                    new MemberInfoKey(typeof(string), "Substring", true, true, typeof(int)) },
                { FindInstanceMethod(typeof(string), "Substring", typeof(int), typeof(int)),
                    new MemberInfoKey(typeof(string), "Substring", true, true, typeof(int), typeof(int)) },
            };

             foreach (MethodInfo key in instanceMethods.Keys)
             {
                 foreach (var pair in instanceMethods)
                 {
                     MemberInfoKey other = new MemberInfoKey(key);
                     if (key == pair.Key)
                     {
                         Assert.IsTrue(pair.Value.Equals(other));
                     }
                     else
                     {
                         Assert.IsFalse(pair.Value.Equals(other));
                     }
                 }
             }
        }

        [TestMethod]
        public void CorrectlyMatchesStaticMemberInfos()
        {
            Dictionary<MethodInfo, MemberInfoKey> staticMethods = 
                new Dictionary<MethodInfo, MemberInfoKey>
            {
                { FindStaticMethod(typeof(Math), "Floor", typeof(double)), 
                    new MemberInfoKey(typeof(Math), "Floor", true, false, typeof(double)) },                 
                { FindStaticMethod(typeof(Math), "Ceiling", typeof(double)), 
                    new MemberInfoKey(typeof(Math), "Ceiling", true, false, typeof(double)) },  
                { FindStaticMethod(typeof(Math), "Round", typeof(double)), 
                    new MemberInfoKey(typeof(Math), "Round", true, false, typeof(double)) },
                { FindStaticMethod(typeof(string), "Concat", typeof(string), typeof(string)), 
                    new MemberInfoKey(typeof(string), "Concat", true, false, typeof(string), typeof(string)) }
            };

            // Windows 8 supports decimal.Floor(), decimal.Ceiling() and decimal.Round(), but Windows Phone does not, 
            // so we have to ensure these method infos exist before we add them.
            MethodInfo possibleFloorMethod = FindStaticMethod(typeof(Decimal), "Floor", typeof(decimal));
            if (possibleFloorMethod != null)
            {
                staticMethods.Add(possibleFloorMethod, 
                    new MemberInfoKey(typeof(Decimal), "Floor", true, false, typeof(decimal)));
            }

            MethodInfo possibleCeilingMethod = FindStaticMethod(typeof(Decimal), "Ceiling", typeof(decimal));
            if (possibleCeilingMethod != null)
            {
                staticMethods.Add(possibleCeilingMethod, 
                    new MemberInfoKey(typeof(Decimal), "Ceiling", true, false, typeof(decimal)));
            }

            MethodInfo possibleRoundMethod = FindStaticMethod(typeof(Decimal), "Round", typeof(decimal));
            if (possibleRoundMethod != null)
            {
                staticMethods.Add(possibleRoundMethod, 
                    new MemberInfoKey(typeof(Decimal), "Round", true, false, typeof(decimal)));
            }

            // Windows Phone 7.5 does not support Math.Floor(), Math.Round() and Math.Ceiling() for decimals 
            MethodInfo possibleCeilingMethodMath = FindStaticMethod(typeof(Math), "Ceiling", typeof(decimal));
            if (possibleCeilingMethodMath != null)
            {
                staticMethods.Add(possibleCeilingMethodMath, 
                    new MemberInfoKey(typeof(Math), "Ceiling", true, false, typeof(decimal)));
            }

            MethodInfo possibleFloorMethodMath = FindStaticMethod(typeof(Math), "Floor", typeof(decimal));
            if (possibleFloorMethodMath != null)
            {
                staticMethods.Add(possibleFloorMethodMath, 
                    new MemberInfoKey(typeof(Math), "Floor", true, false, typeof(decimal)));
            }

            MethodInfo possibleRoundMethodMath = FindStaticMethod(typeof(Math), "Round", typeof(decimal));
            if (possibleRoundMethodMath != null)
            {
                staticMethods.Add(possibleRoundMethodMath, 
                    new MemberInfoKey(typeof(Math), "Round", true, false, typeof(decimal)));
            }

            foreach (MethodInfo key in staticMethods.Keys)
            {
                foreach (var pair in staticMethods)
                {
                    MemberInfoKey other = new MemberInfoKey(key);
                    if (key == pair.Key)
                    {
                        Assert.IsTrue(pair.Value.Equals(other));
                    }
                    else
                    {
                        Assert.IsFalse(pair.Value.Equals(other));
                    }
                }
            }
        }

        [TestMethod]
        public void CorrectlyMatchesPropertyInfos()
        {
            Dictionary<MemberInfo, MemberInfoKey> instanceProperties = 
                new Dictionary<MemberInfo, MemberInfoKey>() {
                 { FindInstanceProperty(typeof(string), "Length"), 
                     new MemberInfoKey(typeof(string), "Length", false, true) },
                 { FindInstanceProperty(typeof(DateTime), "Day"), 
                     new MemberInfoKey(typeof(DateTime), "Day", false, true) },
                 { FindInstanceProperty(typeof(DateTime), "Month"), 
                     new MemberInfoKey(typeof(DateTime), "Month", false, true) },
                 { FindInstanceProperty(typeof(DateTime), "Year"),
                     new MemberInfoKey(typeof(DateTime), "Year", false, true) },
                 { FindInstanceProperty(typeof(DateTime), "Hour"), 
                     new MemberInfoKey(typeof(DateTime), "Hour", false, true) },
                 { FindInstanceProperty(typeof(DateTime), "Minute"), 
                     new MemberInfoKey(typeof(DateTime), "Minute", false, true) },
                 { FindInstanceProperty(typeof(DateTime), "Second"),
                     new MemberInfoKey(typeof(DateTime), "Second", false, true) },
            };

            foreach (MemberInfo key in instanceProperties.Keys)
            {
                foreach (var pair in instanceProperties)
                {
                    MemberInfoKey other = new MemberInfoKey(key);
                    if (key == pair.Key)
                    {
                        Assert.IsTrue(pair.Value.Equals(other));
                    }
                    else
                    {
                        Assert.IsFalse(pair.Value.Equals(other));
                    }
                }
            }
        }
    }
}
