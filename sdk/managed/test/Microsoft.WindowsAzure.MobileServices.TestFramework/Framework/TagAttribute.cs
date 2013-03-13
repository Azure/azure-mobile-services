// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;

namespace Microsoft.WindowsAzure.MobileServices.TestFramework
{
    /// <summary>
    /// Tag a test method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class TagAttribute : Attribute
    {
        public string Tag { get; private set; }

        public TagAttribute(string tag)
        {
            Tag = tag;
        }
    }
}
