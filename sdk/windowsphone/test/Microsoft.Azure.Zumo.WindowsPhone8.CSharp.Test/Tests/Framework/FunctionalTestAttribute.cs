// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;

namespace Microsoft.Azure.Zumo.WindowsPhone8.CSharp
{
    /// <summary>
    /// Mark a test as functional (and requiring a runtime server).
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class FunctionalTestAttribute : Attribute
    {
    }
}
