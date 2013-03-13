// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;

namespace Microsoft.WindowsAzure.MobileServices.TestFramework
{
    /// <summary>
    /// Mark a test as functional (and requiring a runtime server).
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class FunctionalTestAttribute : Attribute
    {
    }
}
