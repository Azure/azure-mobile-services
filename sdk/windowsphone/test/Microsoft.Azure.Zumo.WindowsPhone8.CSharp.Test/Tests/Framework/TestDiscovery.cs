// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Azure.Zumo.WindowsPhone8.Test;

namespace Microsoft.Azure.Zumo.WindowsPhone8.CSharp.Test
{
    public static class TestDiscovery
    {
        public static void Populate(TestHarness harness)
        {
            Assembly assembly = typeof(TestDiscovery).GetTypeInfo().Assembly;
            Dictionary<TypeInfo, TestGroup> groups = new Dictionary<TypeInfo, TestGroup>();
            Dictionary<TestGroup, object> instances = new Dictionary<TestGroup, object>();
            foreach (TypeInfo type in assembly.DefinedTypes)
            {
                foreach (MethodInfo method in type.DeclaredMethods)
                {
                    if (method.GetCustomAttribute<TestMethodAttribute>(true) != null ||
                        method.GetCustomAttribute<AsyncTestMethodAttribute>(true) != null)
                    {
                        TestGroup group = null;
                        object instance = null;
                        if (!groups.TryGetValue(type, out group))
                        {
                            group = CreateGroup(type);
                            harness.Groups.Add(group);
                            groups[type] = group;

                            instance = Activator.CreateInstance(type.AsType());
                            instances[group] = instance;
                        }
                        else
                        {
                            instances.TryGetValue(group, out instance);
                        }

                        TestMethod test = CreateMethod(type, instance, method);
                        group.Methods.Add(test);
                    }
                }
            }
        }

        private static TestGroup CreateGroup(TypeInfo type)
        {
            TestGroup group = new TestGroup();
            group.Name = type.Name;
            group.Tags.Add(type.Name);
            group.Tags.Add(type.FullName);
            if (type.GetCustomAttribute<FunctionalTestAttribute>(true) != null)
            {
                group.Tags.Add("Functional");
            }
            foreach (TagAttribute attr in type.GetCustomAttributes<TagAttribute>(true))
            {
                group.Tags.Add(attr.Tag);
            }
            return group;
        }

        private static TestMethod CreateMethod(TypeInfo type, object instance, MethodInfo method)
        {
            TestMethod test = new TestMethod();
            test.Name = method.Name;

            if (method.GetCustomAttribute<AsyncTestMethodAttribute>(true) != null)
            {
                test.Test = new AsyncTestMethodAsyncAction(instance, method);                
            }
            else
            {
                test.Test = new TestMethodAsyncAction(instance, method);
            }

            ExcludeTestAttribute excluded = method.GetCustomAttribute<ExcludeTestAttribute>(true);
            if (excluded != null)
            {
                test.Exclude(excluded.Reason);
            }

            if (method.GetCustomAttribute<FunctionalTestAttribute>(true) != null)
            {
                test.Tags.Add("Functional");
            }

            test.Tags.Add(type.FullName + "." + method.Name);
            test.Tags.Add(type.Name + "." + method.Name);
            foreach (TagAttribute attr in method.GetCustomAttributes<TagAttribute>(true))
            {
                test.Tags.Add(attr.Tag);
            }

            return test;
        }
    }
}
