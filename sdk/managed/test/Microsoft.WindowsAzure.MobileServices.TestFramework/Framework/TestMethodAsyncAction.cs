// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Reflection;

namespace Microsoft.WindowsAzure.MobileServices.TestFramework
{
    /// <summary>
    /// Execute a sync test method.
    /// </summary>
    internal class TestMethodAsyncAction : IAsyncExecution
    {
        public object Instance { get; private set; }
        public MethodInfo Method { get; private set; }

        public TestMethodAsyncAction(object instance, MethodInfo method)
        {
            Instance = instance;
            Method = method;
        }

        public void Start(IContinuation continuation)
        {
            try
            {
                Method.Invoke(Instance, new object[0]);
                continuation.Success();
            }
            catch (TargetInvocationException ex)
            {
                continuation.Error(ex.InnerException.ToString());
            }
            catch (Exception ex)
            {
                continuation.Error(ex.ToString());
            }
        }
    }
}
