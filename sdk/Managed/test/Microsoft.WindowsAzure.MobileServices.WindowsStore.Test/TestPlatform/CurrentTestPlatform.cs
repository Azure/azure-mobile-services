// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    internal class CurrentTestPlatform : ITestPlatform
    {
        public IPushTestUtility PushTestUtility
        {
            get { return new PushTestUtility(); }
        }
    }
}
