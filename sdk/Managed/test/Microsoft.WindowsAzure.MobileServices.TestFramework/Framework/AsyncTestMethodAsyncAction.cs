// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.TestFramework
{
    /// <summary>
    /// Execute an async test.
    /// </summary>
    internal class AsyncTestMethodAsyncAction : IAsyncExecution
    {
        public object Instance { get; private set; }
        public MethodInfo Method { get; private set; }

        public AsyncTestMethodAsyncAction(object instance, MethodInfo method)
        {
            Instance = instance;
            Method = method;
        }

        public void Start(IContinuation continuation)
        {
            Execute(continuation);
        }

        private async void Execute(IContinuation continuation)
        {
            try
            {
                Task result = Method.Invoke(Instance, new object[0]) as Task;
                if (result != null)
                {
                    await result;
                    continuation.Success();
                }                
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
